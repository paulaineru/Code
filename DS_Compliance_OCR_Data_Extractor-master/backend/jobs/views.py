from django.contrib.auth.decorators import login_required
from django.shortcuts import render


def _render_page(request, page_title, page_description, *, job_id=None):
    return render(
        request,
        "jobs/page.html",
        {
            "page_title": page_title,
            "page_description": page_description,
            "job_id": job_id,
        },
    )


@login_required
def upload_page(request):
    return _render_page(
        request,
        "Upload",
        "Upload payroll source files to start an OCR extraction job.",
    )


@login_required
def documents_page(request):
    return _render_page(
        request,
        "Documents",
        "Browse processed documents and open detailed extraction results.",
    )


@login_required
def document_detail_page(request, job_id):
    return _render_page(
        request,
        "Document Detail",
        "Inspect OCR output and confidence highlights for this extraction job.",
        job_id=job_id,
    )


@login_required
def review_page(request):
    return _render_page(
        request,
        "Review",
        "Review low-confidence fields before exporting the final results.",
    )


@login_required
def export_page(request):
    return _render_page(
        request,
        "Export",
        "Export validated extraction results to downstream workflows.",
    )


@login_required
def settings_page(request):
    return _render_page(
        request,
        "Configuration",
        "Adjust ingestion, OCR, and review defaults for your workspace.",
    )
