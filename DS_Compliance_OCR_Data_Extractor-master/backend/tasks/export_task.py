import logging

from celery import shared_task

from documents.exporting import build_job_exports
from documents.models import Job

logger = logging.getLogger(__name__)


@shared_task(bind=True, max_retries=2, default_retry_delay=15)
def rebuild_exports_async(self, job_id: str):
    try:
        job = Job.objects.get(id=job_id)
        build_job_exports(job)
        logger.info("Rebuilt exports for job %s", job_id)
    except Exception as exc:
        logger.exception("rebuild_exports_async failed for job %s", job_id)
        raise self.retry(exc=exc)
