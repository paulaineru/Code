"""
OCR Celery task — bridges Django models and the processing pipeline.
Import order matters: Django models live here; processing/ has zero Django imports.
"""
import io
import logging
import os
import time
from pathlib import Path

from celery import shared_task
from django.conf import settings
from django.utils import timezone

from documents.job_control import clear_job_processing_artifacts, mark_job_stopped
from documents.models import Job, PageResult, ReviewFlag

logger = logging.getLogger(__name__)

"""
Helpers
"""
def _bgr_from_pil(pil_img):
    import numpy as np
    import cv2
    arr = np.array(pil_img.convert("RGB"))
    return cv2.cvtColor(arr, cv2.COLOR_RGB2BGR)


def _save_page_image(page_result, bgr_array):
    """Save a numpy BGR array as PNG to the PageResult.page_image field."""
    import cv2
    _, buf = cv2.imencode(".png", bgr_array)
    from django.core.files.base import ContentFile
    page_result.page_image.save(
        f"page_{page_result.job_id}_{page_result.page_number}.png",
        ContentFile(buf.tobytes()),
        save=False,
    )


def _stop_requested(job):
    job.refresh_from_db(fields=["status"])
    return job.status in {Job.Status.STOPPING, Job.Status.STOPPED}


def _stop_if_requested(job):
    if not _stop_requested(job):
        return False
    mark_job_stopped(job)
    return True

"""
Main task
"""
@shared_task(
    bind=True,
    max_retries=3,
    default_retry_delay=30,
    autoretry_for=(ConnectionError, TimeoutError),
)
def process_document(self, job_id: str):
    job = Job.objects.get(id=job_id)
    job.processing_task_id = self.request.id or job.processing_task_id
    if _stop_if_requested(job):
        return
    job.status = Job.Status.PROCESSING
    job.error_message = ""
    job.completed_at = None
    job.save(update_fields=["status", "error_message", "completed_at", "processing_task_id"])

    try:
        clear_job_processing_artifacts(job)
        _run_pipeline(job)
    except Exception as exc:
        if _stop_if_requested(job):
            return
        logger.exception("process_document failed for job %s", job_id)
        job.status = Job.Status.FAILED
        job.error_message = str(exc)[:2000]
        job.processing_task_id = ""
        job.save(update_fields=["status", "error_message", "processing_task_id"])
        raise


def _run_pipeline(job):
    # Read uploaded file
    file_bytes = job.upload.read()
    export_dir = getattr(settings, "EXPORT_DIR", "/data/exports")
    os.makedirs(export_dir, exist_ok=True)

    # Prefer live SystemConfig values (editable in UI) over env-var defaults
    try:
        from documents.models import SystemConfig
        config = SystemConfig.get()
        threshold = job.effective_confidence_threshold
        azure_endpoint = config.azure_endpoint or getattr(settings, "AZURE_ENDPOINT", "")
        azure_key = config.azure_key or getattr(settings, "AZURE_KEY", "")
    except Exception:
        threshold = float(getattr(settings, "LOW_CONFIDENCE_THRESHOLD", 0.70))
        azure_endpoint = getattr(settings, "AZURE_ENDPOINT", "")
        azure_key = getattr(settings, "AZURE_KEY", "")

    # Fail fast if Azure credentials are not configured
    if not (azure_endpoint and azure_key):
        raise RuntimeError(
            "Azure Document Intelligence is not configured. "
            "Add your endpoint and API key in Settings before processing documents."
        )

    # Convert to list of BGR frames
    filename_lower = job.filename.lower()
    if filename_lower.endswith(".pdf"):
        from processing.preprocess import pdf_to_bgr_images
        try:
            from documents.models import SystemConfig
            dpi = SystemConfig.get().pdf_dpi
        except Exception:
            dpi = int(getattr(settings, "PDF_DPI", 200))
        pages_bgr = pdf_to_bgr_images(file_bytes, dpi=dpi)
    else:
        from PIL import Image
        import numpy as np
        import cv2
        img = Image.open(io.BytesIO(file_bytes))
        arr = np.array(img.convert("RGB"))
        pages_bgr = [cv2.cvtColor(arr, cv2.COLOR_RGB2BGR)]

    if not pages_bgr:
        raise RuntimeError(f"Could not extract any pages from '{job.filename}'. The file may be corrupted or empty.")

    # Set total_pages early so the progress bar has a denominator before any page finishes
    job.total_pages = len(pages_bgr)
    job.pages_done = 0
    job.save(update_fields=["total_pages", "pages_done"])
    if _stop_if_requested(job):
        return

    page_results = []
    any_flags = False

    for page_idx, bgr in enumerate(pages_bgr, start=1):
        if _stop_if_requested(job):
            return
        page_result = _process_page(
            job=job,
            page_number=page_idx,
            bgr=bgr,
            threshold=threshold,
            azure_endpoint=azure_endpoint,
            azure_key=azure_key,
        )
        page_results.append(page_result)
        if page_result.flags.filter(reviewed=False).exists():
            any_flags = True
        if _stop_if_requested(job):
            return
        job.pages_done = page_idx
        job.save(update_fields=["pages_done"])

    # Build exports
    if _stop_if_requested(job):
        return
    try:
        _build_exports(job, page_results, export_dir)
    except Exception as e:
        logger.warning("Export generation failed for job %s: %s", job.id, e)
    if _stop_if_requested(job):
        return

    job.status = Job.Status.NEEDS_REVIEW if any_flags else Job.Status.COMPLETED
    job.completed_at = timezone.now()
    job.export_path = export_dir
    job.processing_task_id = ""
    job.error_message = ""
    job.save(update_fields=["status", "completed_at", "export_path", "processing_task_id", "error_message"])


def _process_page(job, page_number, bgr, threshold, azure_endpoint, azure_key):
    from documents.models import PageResult, ReviewFlag

    # Preprocess
    try:
        from processing.preprocess import preprocess, compress_for_azure
        processed = preprocess(bgr)
        image_bytes, content_type = compress_for_azure(processed)
    except Exception:
        import cv2
        processed = bgr
        _, buf = cv2.imencode(".png", bgr)
        image_bytes = buf.tobytes()
        content_type = "image/png"

    # Run OCR — no fallback; surface Azure errors directly
    try:
        from processing.azure_client import analyse
        result = analyse(image_bytes, content_type, job.document_type, threshold,
                         endpoint=azure_endpoint, key=azure_key)
    except Exception as e:
        raise RuntimeError(
            f"Azure Document Intelligence returned an error on page {page_number}: {e}"
        ) from e

    extracted_text = result.full_text
    tables_json = result.tables
    avg_confidence = result.avg_confidence
    low_conf_words = result.low_confidence_words
    latency = result.latency_s
    extracted_fields = result.extracted_fields
    raw_response = result.raw_response

    # Save PageResult
    page_result = PageResult(
        job=job,
        page_number=page_number,
        extracted_text=extracted_text,
        tables_json=tables_json,
        extracted_fields=extracted_fields,
        avg_confidence=avg_confidence,
        azure_latency_s=latency,
        raw_response=raw_response,
    )
    _save_page_image(page_result, processed)
    page_result.save()

    # Save ReviewFlags
    flags = [
        ReviewFlag(
            page_result=page_result,
            word=w["text"],
            confidence=w["confidence"],
            polygon_json=w.get("polygon"),
            span_offset=w.get("span_offset"),
            span_length=w.get("span_length"),
            table_index=w.get("table_index"),
            row_index=w.get("row_index"),
            column_index=w.get("column_index"),
        )
        for w in low_conf_words
    ]
    if flags:
        ReviewFlag.objects.bulk_create(flags)

    return page_result



def _build_exports(job, page_results, export_dir):
    from documents.exporting import build_job_exports

    build_job_exports(job, page_results=page_results, export_dir=export_dir)
