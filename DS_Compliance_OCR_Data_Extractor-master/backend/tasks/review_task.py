import logging

from celery import shared_task
from django.contrib.auth import get_user_model

from documents.exporting import build_job_exports
from documents.models import AuditLog, Job
from documents.reviewing import approve_review_flags

logger = logging.getLogger(__name__)


@shared_task(bind=True, max_retries=2, default_retry_delay=15)
def approve_all_flags_async(self, job_id: str, user_id: int | None = None, is_override: bool = False):
    job = Job.objects.get(id=job_id)
    user = None
    if user_id:
        user = get_user_model().objects.filter(id=user_id).first()

    try:
        approved_count = approve_review_flags(
            job,
            user=user,
            is_override=is_override,
            approve_all=True,
        )
        # Already in a worker — call directly rather than queuing another task
        try:
            build_job_exports(job)
        except Exception:
            logger.exception("Failed to regenerate exports after bulk review for job %s", job_id)
        AuditLog.objects.create(
            user=user,
            action="review_complete_async" if job.status == Job.Status.COMPLETED else "flags_approved_async",
            job=job,
            details={"flags_approved": approved_count, "async": True},
        )
    except Exception as exc:
        logger.exception("approve_all_flags_async failed for job %s", job_id)
        job.status = Job.Status.NEEDS_REVIEW
        job.error_message = str(exc)[:2000]
        job.save(update_fields=["status", "error_message"])
        raise
