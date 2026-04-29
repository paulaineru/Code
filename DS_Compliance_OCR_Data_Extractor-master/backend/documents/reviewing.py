import logging

from django.utils import timezone

from documents.models import Job, ReviewFlag

logger = logging.getLogger(__name__)


def approve_review_flags(job, user=None, is_override=False, flag_ids=None, approve_all=False):
    if approve_all:
        flags_qs = ReviewFlag.objects.filter(page_result__job=job, reviewed=False)
    else:
        flag_ids = [str(flag_id).strip() for flag_id in (flag_ids or []) if str(flag_id).strip()]
        flags_qs = ReviewFlag.objects.filter(id__in=flag_ids, page_result__job=job)

    approved_count = flags_qs.count()
    if approved_count == 0:
        return 0

    flags_qs.update(
        reviewed=True,
        reviewed_by=user,
        reviewed_at=timezone.now(),
        admin_override=is_override,
    )

    if not ReviewFlag.objects.filter(page_result__job=job, reviewed=False).exists():
        job.status = Job.Status.COMPLETED
    else:
        job.status = Job.Status.NEEDS_REVIEW
    job.save(update_fields=["status"])

    return approved_count
