import logging
import os

from celery import current_app
from django.conf import settings

from documents.models import Job, PageResult

logger = logging.getLogger(__name__)


def queue_processing_job(job):
    from tasks.ocr_task import process_document

    result = process_document.delay(str(job.id))
    job.processing_task_id = result.id or ""
    job.save(update_fields=["processing_task_id"])
    return result


def clear_job_processing_artifacts(job):
    page_results = list(PageResult.objects.filter(job=job))
    for page_result in page_results:
        if not page_result.page_image:
            continue
        try:
            page_result.page_image.delete(save=False)
        except Exception:
            logger.warning("Failed to delete page image for job %s page %s", job.id, page_result.page_number)

    PageResult.objects.filter(job=job).delete()

    export_roots = [job.export_path, getattr(settings, "EXPORT_DIR", "/data/exports")]
    for export_root in {root for root in export_roots if root}:
        for ext in ("xlsx", "docx"):
            path = os.path.join(export_root, f"{job.id}.{ext}")
            if not os.path.exists(path):
                continue
            try:
                os.remove(path)
            except OSError:
                logger.warning("Failed to delete %s export for job %s", ext, job.id)


def revoke_processing_task(job):
    if not job.processing_task_id:
        return False

    try:
        current_app.control.revoke(job.processing_task_id, terminate=False)
        return True
    except Exception:
        logger.exception("Failed to revoke processing task %s for job %s", job.processing_task_id, job.id)
        return False


def mark_job_stopped(job, clear_artifacts=True):
    if clear_artifacts:
        clear_job_processing_artifacts(job)

    job.status = Job.Status.STOPPED
    job.total_pages = None
    job.pages_done = 0
    job.error_message = ""
    job.processing_task_id = ""
    job.export_path = ""
    job.completed_at = None
    job.save(
        update_fields=[
            "status",
            "total_pages",
            "pages_done",
            "error_message",
            "processing_task_id",
            "export_path",
            "completed_at",
        ]
    )


def request_job_stop(job):
    if job.status == Job.Status.PENDING:
        processing_task_id = job.processing_task_id
        mark_job_stopped(job)
        if processing_task_id:
            try:
                current_app.control.revoke(processing_task_id, terminate=False)
            except Exception:
                logger.exception("Failed to revoke processing task %s for job %s", processing_task_id, job.id)
        return job.status

    if job.status == Job.Status.PROCESSING:
        job.status = Job.Status.STOPPING
        job.save(update_fields=["status"])
        revoke_processing_task(job)
        return job.status

    return job.status
