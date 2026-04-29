import logging
import os

from django.conf import settings

from documents.models import PageResult, ReviewFlag

logger = logging.getLogger(__name__)


def build_job_exports(job, page_results=None, export_dir=None):
    from processing.exporters import build_docx, build_excel

    export_dir = export_dir or getattr(settings, "EXPORT_DIR", "/data/exports")
    os.makedirs(export_dir, exist_ok=True)

    if page_results is None:
        page_results = (
            PageResult.objects.filter(job=job)
            .prefetch_related("flags")
            .order_by("page_number")
        )
    page_results = list(page_results)

    all_flags = list(
        ReviewFlag.objects.filter(page_result__job=job)
        .select_related("page_result")
        .order_by("page_result__page_number", "table_index", "row_index", "column_index", "confidence")
    )

    pages_data = [
        {
            "page_number": page.page_number,
            "extracted_text": page.extracted_text,
            "tables": page.tables_json or [],
            "extracted_fields": page.extracted_fields or {},
        }
        for page in page_results
    ]

    xlsx_bytes = build_excel(pages_data, all_flags)
    xlsx_path = os.path.join(export_dir, f"{job.id}.xlsx")
    with open(xlsx_path, "wb") as handle:
        handle.write(xlsx_bytes)

    docx_bytes = build_docx(pages_data, {}, all_flags)
    docx_path = os.path.join(export_dir, f"{job.id}.docx")
    with open(docx_path, "wb") as handle:
        handle.write(docx_bytes)

    logger.info("Regenerated exports for job %s", job.id)
    return {"xlsx": xlsx_path, "docx": docx_path}
