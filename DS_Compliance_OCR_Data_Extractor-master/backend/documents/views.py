import csv
import io
import json
import os
import re
import logging
from datetime import timedelta

from django.contrib import messages
from django.contrib.auth.decorators import login_required
from django.core.paginator import Paginator
from django.db import transaction
from django.db.models import Count, Q
from django.http import FileResponse, Http404, HttpResponse, JsonResponse
from django.shortcuts import get_object_or_404, redirect, render
from django.utils import timezone
from django.views.decorators.http import require_POST
from django.conf import settings as django_settings

from accounts.access import (
    can_export_audit_trail,
    can_manage_department_settings,
    can_manage_global_settings,
    can_manage_job,
    can_upload_documents,
    can_view_activity_dashboard,
    show_owner_details,
    user_is_admin,
    user_is_supervisor,
    visible_audit_logs_queryset,
    visible_batches_queryset,
    visible_jobs_queryset,
    visible_users_queryset,
)
from .exporting import build_job_exports
from .forms import DepartmentSettingsForm, SystemConfigForm, UploadForm
from .job_control import queue_processing_job, request_job_stop
from .models import AuditLog, BatchUpload, Job, ReviewFlag, SystemConfig
from .reviewing import approve_review_flags
from processing.review_annotations import annotate_tables_with_review_state
from tasks.export_task import rebuild_exports_async
from tasks.review_task import approve_all_flags_async

logger = logging.getLogger(__name__)

ASYNC_APPROVE_ALL_THRESHOLD = 25

_MONEY_HEADER_RE = re.compile(
    r"\b(amount|pay|salary|total|gross|net|balance|value|price|cost|tax|fee|deduction|contribution|nssf|nhif|paye)\b",
    re.IGNORECASE,
)
_NAME_HEADER_RE = re.compile(r"\b(name|employee|payee|employer|member)\b", re.IGNORECASE)
_CURRENCY_MARKER_RE = re.compile(r"[$\u20ac\u00a3\u00a5]|(?:USD|UGX|KES|TZS|RWF|EUR|GBP)\b", re.IGNORECASE)
_LEADING_CURRENCY_CODE_RE = re.compile(r"^(?:USD|UGX|KES|TZS|RWF|EUR|GBP)\s*", re.IGNORECASE)
_TRAILING_CURRENCY_CODE_RE = re.compile(r"\s*(?:USD|UGX|KES|TZS|RWF|EUR|GBP)$", re.IGNORECASE)
_EDGE_SYMBOL_RE = re.compile(
    r"^[\s=#$%&/+\u00a3\u20ac\u00a5]+"          # leading noise (not -, preserves negatives)
    r"|[\s=#$%&/+\-\u00a3\u20ac\u00a5]+"         # trailing noise (includes - and +)
    r"$"
)
# Allow comma or period as thousands separator, and both as decimal separator
_NUMERIC_RE = re.compile(r"^-?[\d]{1,3}(?:[,.][\d]{3})*(?:[,.]\d+)?$|^-?\d+$")


def _table_cell_text(cell):
    if isinstance(cell, dict):
        return str(cell.get("content", ""))
    return str(cell or "")


def _is_money_header(header: str) -> bool:
    return bool(_MONEY_HEADER_RE.search((header or "").strip()))


def _is_name_header(header: str) -> bool:
    return bool(_NAME_HEADER_RE.search((header or "").strip()))


def _clean_money_value(value: str) -> str:
    raw = (value or "").strip()
    if not raw:
        return ""
    cleaned = _LEADING_CURRENCY_CODE_RE.sub("", raw)
    cleaned = _TRAILING_CURRENCY_CODE_RE.sub("", cleaned)
    cleaned = _EDGE_SYMBOL_RE.sub("", cleaned).strip()
    return cleaned if _NUMERIC_RE.match(cleaned) else raw


def _looks_money_column(values: list[str]) -> bool:
    for value in values:
        raw = (value or "").strip()
        if not raw:
            continue
        if _CURRENCY_MARKER_RE.search(raw):
            return True
        cleaned = _clean_money_value(raw)
        if cleaned != raw and _NUMERIC_RE.match(cleaned):
            return True
    return False


def _normalize_table(table: list) -> list:
    if not isinstance(table, list) or not table:
        return table

    headers = [_table_cell_text(cell) for cell in (table[0] if table else [])]
    money_cols = {idx for idx, header in enumerate(headers) if _is_money_header(header)}
    name_cols = {idx for idx, header in enumerate(headers) if _is_name_header(header)}

    for idx in range(len(headers)):
        if idx in money_cols:
            continue
        col_values = [_table_cell_text(row[idx]) for row in table if isinstance(row, list) and len(row) > idx]
        if _looks_money_column(col_values):
            money_cols.add(idx)

    normalized = []
    for row_idx, row in enumerate(table):
        if not isinstance(row, list):
            normalized.append(row)
            continue
        normalized_row = []
        for idx, cell in enumerate(row):
            content = _table_cell_text(cell)
            if idx in money_cols:
                content = _clean_money_value(content)
            if idx in name_cols and row_idx > 0 and content:
                content = content.upper()
            if isinstance(cell, dict):
                merged = dict(cell)
                merged["content"] = content
                normalized_row.append(merged)
            else:
                normalized_row.append({"content": content})
        normalized.append(normalized_row)
    return normalized


def _log(request, action, job=None, details=None, duration_ms=None):
    """Write an audit entry — never raises, so a logging failure can't break a request."""
    try:
        department = None
        if job and getattr(job, "department_id", None):
            department = job.department
        elif request.user.is_authenticated and getattr(request.user, "department_id", None):
            department = request.user.department

        AuditLog.objects.create(
            user=request.user if request.user.is_authenticated else None,
            action=action,
            job=job,
            department=department,
            details=details or {},
            duration_ms=duration_ms,
            ip_address=request.META.get("REMOTE_ADDR"),
        )
    except Exception:
        pass  # table may not exist yet (pre-migration) or DB is unavailable


def _queue_job_or_mark_failed(job):
    try:
        queue_processing_job(job)
        return True, ""
    except Exception as exc:
        job.status = Job.Status.FAILED
        job.error_message = f"Could not queue task (broker unavailable): {exc}"
        job.processing_task_id = ""
        job.save(update_fields=["status", "error_message", "processing_task_id"])
        return False, str(exc)


def _refresh_job_exports(job) -> bool:
    try:
        build_job_exports(job)
        return True
    except Exception:
        logger.exception(
            "Failed to regenerate exports inline for job %s; falling back to async rebuild",
            job.id,
        )

    try:
        rebuild_exports_async.delay(str(job.id))
        return True
    except Exception:
        logger.exception("Failed to queue async export rebuild for job %s", job.id)
        return False


def _document_list_copy(user):
    if user_is_admin(user):
        return {
            "page_title": "Documents",
            "page_subtitle": "Browse processed documents and extraction results across all departments.",
        }
    if user_is_supervisor(user) and user.department_id:
        return {
            "page_title": "Department Documents",
            "page_subtitle": f"Browse documents and extraction results for {user.department.name}.",
        }
    return {
        "page_title": "Documents",
        "page_subtitle": "Browse processed documents and extraction results.",
    }


def _review_copy(user):
    if user_is_admin(user):
        return "Verify low-confidence extractions across all departments before finalizing results."
    if user_is_supervisor(user) and user.department_id:
        return f"Verify low-confidence extractions for {user.department.name} before finalizing department results."
    return "Verify low-confidence extractions before finalizing document results."


def _audit_log_copy(user):
    if user_is_admin(user):
        return {
            "page_title": "Audit Log",
            "page_subtitle": "Track user actions across all departments, including uploads, reviews, and configuration changes.",
        }
    if user_is_supervisor(user) and user.department_id:
        return {
            "page_title": "Department Audit Log",
            "page_subtitle": f"Track user activity and review actions for {user.department.name}.",
        }
    return {
        "page_title": "Audit Log",
        "page_subtitle": "Track your actions, including uploads, reviews, and processing activity.",
    }


def _activity_dashboard_copy(user, thirty_days_ago):
    if user_is_admin(user):
        return {
            "page_title": "User Statistics",
            "breadcrumbs": "Admin / User Statistics",
            "page_subtitle": f"Activity overview across all users. Last 30 days from {thirty_days_ago:%d %b %Y}.",
        }

    department_name = user.department.name if user.department_id else "your department"
    return {
        "page_title": "Department Overview",
        "breadcrumbs": "Department / Overview",
        "page_subtitle": f"Activity overview for {department_name}. Last 30 days from {thirty_days_ago:%d %b %Y}.",
    }


@login_required
def _legacy_upload_view(request):
    form = UploadForm(request.POST or None, request.FILES or None)
    if request.method == "POST" and form.is_valid():
        # Ensure the upload directory exists before Django tries to write to it
        os.makedirs(django_settings.MEDIA_ROOT, exist_ok=True)
        files = form.cleaned_data["files"]
        document_type = form.cleaned_data["document_type"]
        batch = None
        if len(files) > 1:
            batch = BatchUpload.objects.create(
                owner=request.user,
                requested_document_type=document_type,
            )

        created_jobs = []
        queued_jobs = 0
        failed_jobs = 0
        # Dispatch async task — wrapped so a missing/slow broker never blocks the response
        for uploaded_file in files:
            job = Job.objects.create(
                owner=request.user,
                batch=batch,
                filename=uploaded_file.name,
                document_type=document_type,
                upload=uploaded_file,
                status=Job.Status.PENDING,
            )
            created_jobs.append(job)
            task_queued, queue_error = _queue_job_or_mark_failed(job)
            if task_queued:
                queued_jobs += 1
            else:
                failed_jobs += 1

            _log(
                request,
                "upload",
                job=job,
                details={
                    "filename": uploaded_file.name,
                    "document_type": job.document_type,
                    "batch_id": str(batch.id) if batch else "",
                    "batch_size": len(files),
                    "queued": task_queued,
                    "queue_error": queue_error,
                },
            )
        try:
            queue_processing_job(job)
            task_queued = True
        except Exception as exc:
            # Redis not running or unreachable — mark job failed so the user knows
            job.status = Job.Status.FAILED
            job.error_message = f"Could not queue task (broker unavailable): {exc}"
            job.processing_task_id = ""
            job.save(update_fields=["status", "error_message", "processing_task_id"])

        _log(request, "upload", job=job, details={"filename": f.name, "document_type": job.document_type})

        if task_queued:
            messages.success(request, f"'{f.name}' uploaded — extraction queued.")
        else:
            messages.warning(
                request,
                f"'{f.name}' saved but could not be queued (is Redis running?). "
                f"Saved to: {job.upload.path}"
            )
        return redirect("document_detail", job_id=job.id)
    return render(request, "documents/upload.html", {"form": form})


@login_required
def document_list_view(request):
    status_filter = request.GET.get("status", "")
    query = request.GET.get("q", "").strip()
    show_owner = show_owner_details(request.user)

    select_related_fields = ["batch", "department"]
    if show_owner:
        select_related_fields.append("owner")
    base_qs = visible_jobs_queryset(request.user).select_related(*select_related_fields)

    jobs = base_qs
    if status_filter:
        jobs = jobs.filter(status=status_filter)
    if query:
        jobs = jobs.filter(filename__icontains=query)
    jobs = jobs.order_by("-created_at")

    stats = base_qs.aggregate(
        total=Count("id"),
        pending=Count("id", filter=Q(status=Job.Status.PENDING)),
        processing=Count("id", filter=Q(status__in=[Job.Status.PROCESSING, Job.Status.REVIEWING, Job.Status.STOPPING])),
        completed=Count("id", filter=Q(status=Job.Status.COMPLETED)),
        needs_review=Count("id", filter=Q(status=Job.Status.NEEDS_REVIEW)),
        failed=Count("id", filter=Q(status=Job.Status.FAILED)),
    )

    paginator = Paginator(jobs, 20)
    page_obj = paginator.get_page(request.GET.get("page"))
    page_range = paginator.get_elided_page_range(page_obj.number, on_each_side=2, on_ends=1)

    return render(request, "documents/list.html", {
        "page_obj": page_obj,
        "page_range": page_range,
        "ellipsis": paginator.ELLIPSIS,
        "stats": stats,
        "active_status": status_filter,
        "query": query,
        "status_choices": Job.Status.choices,
        "show_owner": show_owner,
        "can_upload": can_upload_documents(request.user),
        **_document_list_copy(request.user),
    })


@login_required
def document_detail_view(request, job_id):
    qs = visible_jobs_queryset(request.user).select_related("owner", "department").prefetch_related("pages__flags")
    job = get_object_or_404(qs, id=job_id)
    for page in job.pages.all():
        page.unreviewed_flags_list = [flag for flag in page.flags.all() if not flag.reviewed]
        if isinstance(page.extracted_fields, dict):
            page.extracted_fields = {
                key: (str(value).upper() if _is_name_header(key) and value is not None else value)
                for key, value in page.extracted_fields.items()
            }
        if isinstance(page.tables_json, list):
            normalized_tables = [_normalize_table(table) for table in page.tables_json]
            page.tables_json = annotate_tables_with_review_state(
                page.page_number,
                normalized_tables,
                page.unreviewed_flags_list,
                unresolved_only=True,
            )
    return render(request, "documents/detail.html", {
        "job": job,
        "show_owner": show_owner_details(request.user),
        "can_manage_job": can_manage_job(request.user, job),
    })


@login_required
def document_status_view(request, job_id):
    qs = visible_jobs_queryset(request.user)
    job = get_object_or_404(qs, id=job_id)
    return JsonResponse({
        "status": job.status,
        "status_display": job.get_status_display(),
        "pages_done": job.pages_done or 0,
        "total_pages": job.total_pages or 0,
    })


@login_required
@require_POST
def stop_job_view(request, job_id):
    qs = visible_jobs_queryset(request.user)
    job = get_object_or_404(qs, id=job_id)
    if not can_manage_job(request.user, job):
        messages.error(request, "You do not have permission to stop this document.")
        return redirect("document_detail", job_id=job_id)

    if job.status == Job.Status.STOPPING:
        messages.info(request, "This document is already being stopped.")
        return redirect("document_detail", job_id=job_id)

    if not job.can_stop:
        messages.info(request, "Only queued or actively processing documents can be stopped.")
        return redirect("document_detail", job_id=job_id)

    previous_status = job.status
    new_status = request_job_stop(job)
    _log(request, "processing_stop_requested", job=job, details={"from_status": previous_status, "to_status": new_status})

    if new_status == Job.Status.STOPPED:
        messages.info(request, "Document processing was stopped.")
    else:
        messages.info(request, "Stop requested. The current page will finish before processing is halted.")
    return redirect("document_detail", job_id=job_id)


@login_required
@require_POST
def restart_job_view(request, job_id):
    qs = visible_jobs_queryset(request.user)
    job = get_object_or_404(qs, id=job_id)
    if not can_manage_job(request.user, job):
        messages.error(request, "You do not have permission to restart this document.")
        return redirect("document_detail", job_id=job_id)

    if not job.can_restart:
        messages.info(request, "Only failed or stopped jobs can be restarted right now.")
        return redirect("document_detail", job_id=job_id)

    previous_state = {
        "status": job.status,
        "total_pages": job.total_pages,
        "pages_done": job.pages_done,
        "error_message": job.error_message,
        "processing_task_id": job.processing_task_id,
        "export_path": job.export_path,
        "completed_at": job.completed_at,
    }

    job.status = Job.Status.PENDING
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

    try:
        queue_processing_job(job)
    except Exception as exc:
        for field, value in previous_state.items():
            setattr(job, field, value)
        job.save(update_fields=list(previous_state.keys()))
        messages.error(request, f"Could not restart processing right now: {exc}")
        return redirect("document_detail", job_id=job_id)

    _log(request, "processing_restart_queued", job=job, details={"from_status": previous_state["status"]})
    messages.success(request, "Document processing restarted.")
    return redirect("document_detail", job_id=job_id)


@login_required
def page_image_view(request, job_id, page_num):
    qs = visible_jobs_queryset(request.user)
    job = get_object_or_404(qs, id=job_id)
    from .models import PageResult
    page = get_object_or_404(PageResult, job=job, page_number=page_num)
    if not page.page_image:
        raise Http404
    # Catch FileNotFoundError (filesystem) and gridfs.errors.NoFile (GridFS)
    # plus any other storage retrieval failure — all map cleanly to 404.
    try:
        f = page.page_image.open("rb")
    except Exception:
        raise Http404
    return FileResponse(f, content_type="image/png")


@login_required
def export_download_view(request, job_id, fmt):
    qs = visible_jobs_queryset(request.user)
    job = get_object_or_404(qs, id=job_id)
    if fmt not in ("xlsx", "docx"):
        raise Http404

    from django.conf import settings as django_settings
    export_dir = getattr(django_settings, "EXPORT_DIR", "/data/exports")
    path = os.path.join(export_dir, f"{job_id}.{fmt}")
    if not os.path.exists(path):
        messages.error(request, "The requested download is not available yet. Re-process the document to generate fresh files.")
        return redirect("document_detail", job_id=job_id)

    content_type = (
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        if fmt == "xlsx"
        else "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    )
    name = f"{job.filename.rsplit('.', 1)[0]}.{fmt}"
    return FileResponse(open(path, "rb"), content_type=content_type, as_attachment=True, filename=name)


@login_required
def review_view(request):
    base_qs = visible_jobs_queryset(request.user).select_related("owner", "department")
    jobs = (
        base_qs.filter(status=Job.Status.NEEDS_REVIEW)
        .prefetch_related("pages__flags")
        .annotate(unreviewed_flags=Count("pages__flags", filter=Q(pages__flags__reviewed=False)))
    )
    config = SystemConfig.get()
    return render(request, "documents/review.html", {
        "jobs": jobs,
        "config": config,
        "page_subtitle": _review_copy(request.user),
    })


@login_required
@require_POST
def approve_flags_view(request, job_id):
    qs = visible_jobs_queryset(request.user)
    job = get_object_or_404(qs, id=job_id)
    # Admins overriding any job, supervisors overriding a dept member's job.
    is_override = (
        (user_is_admin(request.user) or user_is_supervisor(request.user))
        and job.owner_id != request.user.pk
    )

    if job.status == Job.Status.REVIEWING:
        messages.info(request, "A bulk review update is already running for this document.")
        return redirect("document_detail", job_id=job_id)

    if request.POST.get("approve_all") == "1":
        # Approve all unreviewed flags for this job
        flags_qs = ReviewFlag.objects.filter(page_result__job=job, reviewed=False)
        if flags_qs.count() >= ASYNC_APPROVE_ALL_THRESHOLD:
            job.status = Job.Status.REVIEWING
            job.save(update_fields=["status"])
            try:
                approve_all_flags_async.delay(str(job.id), request.user.id, is_override)
                _log(
                    request,
                    "review_bulk_queued",
                    job=job,
                    details={"flags_queued": flags_qs.count(), "async_threshold": ASYNC_APPROVE_ALL_THRESHOLD},
                )
                messages.info(
                    request,
                    "Large review approval queued. This page will refresh once the updated downloads are ready.",
                )
                return redirect("document_detail", job_id=job_id)
            except Exception:
                logger.exception("Failed to queue async bulk review for job %s; falling back to inline approval", job.id)
                job.status = Job.Status.NEEDS_REVIEW
                job.save(update_fields=["status"])
                messages.warning(
                    request,
                    "Background queue was unavailable, so the review is being completed inline.",
                )
    else:
        # detail.html packs checked IDs into one comma-separated string;
        # review.html submits individual checkbox values (multiple flag_ids).
        # Flatten both into a clean list of UUID strings.
        raw = request.POST.getlist("flag_ids")
        flag_ids = [fid.strip() for item in raw for fid in item.split(",") if fid.strip()]
        if not flag_ids:
            # Nothing selected — no-op
            return redirect("document_detail", job_id=job_id)
        flags_qs = ReviewFlag.objects.filter(id__in=flag_ids, page_result__job=job)

    is_approve_all = request.POST.get("approve_all") == "1"
    approved_count = approve_review_flags(
        job,
        user=request.user,
        is_override=is_override,
        flag_ids=None if is_approve_all else flag_ids,
        approve_all=is_approve_all,
    )

    # For partial approvals, refresh downloads inline so the redirected detail
    # page reflects the review immediately. If regeneration fails, fall back to
    # the async task and warn only if both paths fail.
    if not is_approve_all and not _refresh_job_exports(job):
        messages.warning(
            request,
            "Review changes were saved, but the refreshed download files are not ready yet.",
        )

    if job.status == Job.Status.COMPLETED:
        _log(request, "review_complete", job=job, details={"flags_approved": approved_count})
    else:
        _log(request, "flags_approved", job=job, details={"flags_approved": approved_count})

    # next_url = request.META.get("HTTP_REFERER", "")
    # if next_url and url_has_allowed_host_and_scheme(
    #     next_url, 
    #     allowed_hosts={
    #         request.get_host()
    #         }, require_https=request.is_secure()
    #     ):
    #     return redirect(next_url)
    return redirect("document_detail", job_id=job_id)


@login_required
def settings_view(request):
    can_manage_global = can_manage_global_settings(request.user)
    can_manage_department = can_manage_department_settings(request.user)
    if not can_manage_global and not can_manage_department:
        messages.error(request, "You do not have permission to view the settings page.")
        return redirect("document_list")

    config = SystemConfig.get()
    form = None
    if can_manage_global:
        initial = {
            "confidence_threshold": int(config.confidence_threshold * 100),
            "default_document_type": config.default_document_type,
            "pdf_dpi": config.pdf_dpi,
            "azure_endpoint": config.azure_endpoint,
            "azure_key": config.azure_key,
        }
        form = SystemConfigForm(initial=initial)

    department = request.user.department if can_manage_department else None
    department_form = None
    if department is not None:
        department_form = DepartmentSettingsForm(
            initial={"confidence_threshold": int(department.confidence_threshold * 100)},
            department=department,
        )

    return render(request, "settings_page/index.html", {
        "user": request.user,
        "form": form,
        "config": config,
        "dpi_options": [150, 200, 300],
        "can_manage_global_settings": can_manage_global,
        "can_manage_department_settings": can_manage_department,
        "department": department,
        "department_form": department_form,
        "page_title": "Configuration" if can_manage_global else "Department Settings",
        "page_subtitle": (
            "Adjust OCR thresholds, document defaults, and Azure credentials."
            if can_manage_global
            else f"Manage OCR review sensitivity for {department.name}."
        ),
        "department_member_count": department.users.count() if department is not None else 0,
    })


@login_required
@require_POST
def settings_save_view(request):
    if can_manage_global_settings(request.user):
        form = SystemConfigForm(request.POST)
        if form.is_valid():
            form.save()
            _log(request, "settings_update", details={"fields": list(form.cleaned_data.keys())})
            messages.success(request, "Global settings saved.")
        else:
            for field, errs in form.errors.items():
                for err in errs:
                    messages.error(request, f"{field}: {err}")
        return redirect("settings")

    if can_manage_department_settings(request.user):
        form = DepartmentSettingsForm(request.POST, department=request.user.department)
        if form.is_valid():
            department = form.save()
            _log(
                request,
                "department_settings_update",
                details={"department": department.name, "fields": list(form.cleaned_data.keys())},
            )
            messages.success(request, "Department settings saved.")
        else:
            for field, errs in form.errors.items():
                for err in errs:
                    messages.error(request, f"{field}: {err}")
        return redirect("settings")

    messages.error(request, "You do not have permission to update settings.")
    return redirect("settings")


@login_required
def audit_log_view(request):
    logs = visible_audit_logs_queryset(request.user).select_related("user", "job", "department")
    show_user_column = show_owner_details(request.user)

    user_filter = request.GET.get("user", "").strip()
    action_filter = request.GET.get("action", "").strip()

    if show_user_column and user_filter:
        logs = logs.filter(user__username__icontains=user_filter)
    if action_filter:
        logs = logs.filter(action=action_filter)

    paginator = Paginator(logs, 25)
    page_obj = paginator.get_page(request.GET.get("page"))
    page_range = paginator.get_elided_page_range(page_obj.number, on_each_side=2, on_ends=1)
    distinct_actions = logs.values_list("action", flat=True).distinct().order_by("action")
    return render(request, "documents/audit_log.html", {
        "page_obj": page_obj,
        "page_range": page_range,
        "ellipsis": paginator.ELLIPSIS,
        "user_filter": user_filter,
        "action_filter": action_filter,
        "distinct_actions": distinct_actions,
        "show_user_column": show_user_column,
        **_audit_log_copy(request.user),
    })


@login_required
def export_audit_trail_view(request):
    """
    Stream the visible audit log as a UTF-8 CSV file.
    Access is restricted to admins and supervisors (supervisors only see their
    department's entries, matching the same queryset used by audit_log_view).
    """
    if not can_export_audit_trail(request.user):
        messages.error(request, "You do not have permission to export audit trails.")
        return redirect("audit_log")

    logs = (
        visible_audit_logs_queryset(request.user)
        .select_related("user", "job", "department")
        .order_by("-timestamp")
    )

    # Optional filters — mirror the GET params from audit_log_view so that
    # "export current view" works correctly.
    user_filter = request.GET.get("user", "").strip()
    action_filter = request.GET.get("action", "").strip()
    if user_filter:
        logs = logs.filter(user__username__icontains=user_filter)
    if action_filter:
        logs = logs.filter(action=action_filter)

    # Build filename that includes scope so files are self-documenting.
    if user_is_admin(request.user):
        scope = "all-departments"
    else:
        scope = (request.user.department.name if request.user.department_id else "unknown").replace(" ", "-")
    filename = f"audit-trail_{scope}.csv"

    # Stream directly into the response — avoids loading the entire result set
    # into memory for large tenants.
    response = HttpResponse(content_type="text/csv; charset=utf-8")
    response["Content-Disposition"] = f'attachment; filename="{filename}"'

    # UTF-8 BOM so Excel opens the file correctly on Windows.
    response.write("\ufeff")

    writer = csv.writer(response)
    writer.writerow([
        "Timestamp",
        "User",
        "Full Name",
        "Department",
        "Action",
        "Document",
        "IP Address",
        "Duration (ms)",
        "Details",
    ])

    for entry in logs.iterator(chunk_size=500):
        writer.writerow([
            entry.timestamp.strftime("%Y-%m-%d %H:%M:%S UTC") if entry.timestamp else "",
            entry.user.username if entry.user else "(system)",
            entry.user.full_name if entry.user else "",
            entry.department.name if entry.department else "",
            entry.action,
            entry.job.filename if entry.job else "",
            entry.ip_address or "",
            entry.duration_ms if entry.duration_ms is not None else "",
            json.dumps(entry.details, ensure_ascii=False) if entry.details else "",
        ])

    _log(request, "audit_export", details={"scope": scope, "filters": {"user": user_filter, "action": action_filter}})
    return response


@login_required
def admin_stats_view(request):
    if not can_view_activity_dashboard(request.user):
        messages.error(request, "You do not have permission to view this page.")
        return redirect("document_list")

    thirty_days_ago = timezone.now() - timedelta(days=30)
    users_qs = visible_users_queryset(request.user).select_related("department", "supervisor")
    jobs_qs = visible_jobs_queryset(request.user)

    use_scoped_job_counts = user_is_supervisor(request.user) and bool(request.user.department_id)
    user_job_scope = Q()
    if use_scoped_job_counts:
        department_id = request.user.department_id
        user_job_scope = Q(jobs__department_id=department_id) | Q(
            jobs__department__isnull=True,
            jobs__owner__department_id=department_id,
        )

    users = (
        users_qs.annotate(
            total_docs=Count("jobs", filter=user_job_scope, distinct=True) if use_scoped_job_counts else Count("jobs", distinct=True),
            completed=Count("jobs", filter=user_job_scope & Q(jobs__status="completed"), distinct=True)
            if use_scoped_job_counts
            else Count("jobs", filter=Q(jobs__status="completed"), distinct=True),
            needs_review=Count("jobs", filter=user_job_scope & Q(jobs__status="needs_review"), distinct=True)
            if use_scoped_job_counts
            else Count("jobs", filter=Q(jobs__status="needs_review"), distinct=True),
            failed=Count("jobs", filter=user_job_scope & Q(jobs__status="failed"), distinct=True)
            if use_scoped_job_counts
            else Count("jobs", filter=Q(jobs__status="failed"), distinct=True),
            in_progress=Count(
                "jobs",
                filter=user_job_scope & Q(jobs__status__in=["pending", "processing", "stopping"]),
                distinct=True,
            )
            if use_scoped_job_counts
            else Count("jobs", filter=Q(jobs__status__in=["pending", "processing", "stopping"]), distinct=True),
            recent_uploads=Count("jobs", filter=user_job_scope & Q(jobs__created_at__gte=thirty_days_ago), distinct=True)
            if use_scoped_job_counts
            else Count("jobs", filter=Q(jobs__created_at__gte=thirty_days_ago), distinct=True),
        )
        .order_by("-total_docs")
    )

    type_breakdown = (
        jobs_qs.values("document_type")
        .annotate(count=Count("id"))
        .order_by("-count")
    )
    type_total = jobs_qs.count() or 1  # avoid division by zero in template

    active_users = users_qs.filter(last_login__gte=thirty_days_ago).count()
    recent_doc_count = jobs_qs.filter(created_at__gte=thirty_days_ago).count()

    return render(request, "documents/admin_stats.html", {
        "users": users,
        "type_breakdown": type_breakdown,
        "type_total": type_total,
        "active_users": active_users,
        "total_users": users_qs.count(),
        "recent_doc_count": recent_doc_count,
        "thirty_days_ago": thirty_days_ago,
        "show_department_column": user_is_admin(request.user),
        **_activity_dashboard_copy(request.user, thirty_days_ago),
    })


@login_required
def batch_detail_view(request, batch_id):
    show_owner = show_owner_details(request.user)
    batches = visible_batches_queryset(request.user).select_related("owner", "department")

    batch = get_object_or_404(batches, id=batch_id)
    jobs = batch.jobs.select_related("batch", "owner", "department").order_by("-created_at")
    return render(request, "documents/batch_detail.html", {
        "batch": batch,
        "jobs": jobs,
        "show_owner": show_owner,
        "should_auto_refresh": batch.active_jobs > 0,
        "can_upload": can_upload_documents(request.user),
    })


@login_required
def upload_view(request):
    if not can_upload_documents(request.user):
        messages.error(request, "You do not have permission to upload documents.")
        return redirect("document_list")

    form = UploadForm(request.POST or None, request.FILES or None)
    if request.method == "POST" and form.is_valid():
        os.makedirs(django_settings.MEDIA_ROOT, exist_ok=True)
        files = list(form.cleaned_data["files"])
        document_type = form.cleaned_data["document_type"]
        department = request.user.department if getattr(request.user, "department_id", None) else None

        created_jobs = []
        batch = None
        with transaction.atomic():
            if len(files) > 1:
                batch = BatchUpload.objects.create(
                    owner=request.user,
                    department=department,
                    requested_document_type=document_type,
                )

            for uploaded_file in files:
                job = Job.objects.create(
                    owner=request.user,
                    department=department,
                    batch=batch,
                    filename=uploaded_file.name,
                    document_type=document_type,
                    upload=uploaded_file,
                    status=Job.Status.PENDING,
                )
                created_jobs.append(job)

        queued_jobs = 0
        failed_jobs = 0
        for uploaded_file, job in zip(files, created_jobs):
            task_queued, queue_error = _queue_job_or_mark_failed(job)
            if task_queued:
                queued_jobs += 1
            else:
                failed_jobs += 1

            _log(
                request,
                "upload",
                job=job,
                details={
                    "filename": uploaded_file.name,
                    "document_type": job.document_type,
                    "batch_id": str(batch.id) if batch else "",
                    "batch_size": len(files),
                    "queued": task_queued,
                    "queue_error": queue_error,
                },
            )

        if batch:
            _log(
                request,
                "batch_upload",
                details={
                    "batch_id": str(batch.id),
                    "batch_size": len(created_jobs),
                    "document_type": document_type,
                    "queued_jobs": queued_jobs,
                    "failed_jobs": failed_jobs,
                },
            )
            if failed_jobs == 0:
                messages.success(
                    request,
                    f"{queued_jobs} document{'s' if queued_jobs != 1 else ''} uploaded and queued for extraction.",
                )
            elif queued_jobs == 0:
                messages.warning(
                    request,
                    f"All {failed_jobs} uploaded documents were saved, but none could be queued for extraction.",
                )
            else:
                messages.warning(
                    request,
                    f"{queued_jobs} documents were queued and {failed_jobs} could not be queued. Review the batch for details.",
                )
            return redirect("batch_detail", batch_id=batch.id)

        job = created_jobs[0]
        if queued_jobs == 1:
            messages.success(request, f"'{job.filename}' uploaded and queued for extraction.")
        else:
            messages.warning(
                request,
                f"'{job.filename}' was uploaded, but extraction could not be queued right now. "
                f"Saved to: {job.upload.path}"
            )
        return redirect("document_detail", job_id=job.id)

    return render(request, "documents/upload.html", {"form": form})
