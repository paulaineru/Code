import uuid
from django.db import models
from django.db.models import Count, Q
from django.conf import settings
from django.utils import timezone
from django.utils.functional import cached_property


class Job(models.Model):
    class DocumentType(models.TextChoices):
        GENERIC = "generic", "Generic"
        RECEIPT = "receipt", "Receipt"
        INVOICE = "invoice", "Invoice"
        BANK_STATEMENT = "bank_statement", "Bank Statement"
        PAYROLL = "payroll", "Payroll"

    class Status(models.TextChoices):
        PENDING = "pending", "Pending"
        PROCESSING = "processing", "Processing"
        STOPPING = "stopping", "Stopping"
        STOPPED = "stopped", "Stopped"
        REVIEWING = "reviewing", "Applying Reviews"
        COMPLETED = "completed", "Completed"
        NEEDS_REVIEW = "needs_review", "Needs Review"
        FAILED = "failed", "Failed"

    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    owner = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name="jobs")
    department = models.ForeignKey("accounts.Department", on_delete=models.SET_NULL, null=True, blank=True, related_name="jobs")
    batch = models.ForeignKey("BatchUpload", on_delete=models.SET_NULL, null=True, blank=True, related_name="jobs")
    filename = models.CharField(max_length=255)
    document_type = models.CharField(max_length=30, choices=DocumentType.choices, default=DocumentType.GENERIC)
    status = models.CharField(max_length=20, choices=Status.choices, default=Status.PENDING)
    total_pages = models.PositiveIntegerField(null=True, blank=True)
    pages_done = models.PositiveIntegerField(default=0)
    error_message = models.TextField(blank=True)
    processing_task_id = models.CharField(max_length=255, blank=True)
    upload = models.FileField(upload_to="")  # stored directly in MEDIA_ROOT (UPLOAD_DIR)
    export_path = models.CharField(max_length=500, blank=True)
    created_at = models.DateTimeField(auto_now_add=True)
    completed_at = models.DateTimeField(null=True, blank=True)

    class Meta:
        indexes = [
            models.Index(fields=["owner", "status"]),
            models.Index(fields=["department", "status"]),
        ]
        ordering = ["-created_at"]

    def __str__(self):
        return self.filename

    @property
    def flag_count(self):
        return ReviewFlag.objects.filter(page_result__job=self, reviewed=False).count()

    @property
    def file_size_display(self):
        try:
            size = self.upload.size
        except Exception:
            return "—"
        for unit in ("B", "KB", "MB", "GB"):
            if size < 1024:
                return f"{size:.1f} {unit}"
            size /= 1024
        return f"{size:.1f} TB"

    @property
    def can_stop(self):
        return self.status in {self.Status.PENDING, self.Status.PROCESSING}

    @property
    def can_restart(self):
        return self.status in {self.Status.FAILED, self.Status.STOPPED}

    @property
    def effective_confidence_threshold(self):
        if self.department_id and self.department:
            return float(self.department.confidence_threshold)
        return float(SystemConfig.get().confidence_threshold)


class BatchUpload(models.Model):
    class Status(models.TextChoices):
        PROCESSING = "processing", "Processing"
        NEEDS_REVIEW = "needs_review", "Needs Review"
        COMPLETED = "completed", "Completed"
        COMPLETED_WITH_ERRORS = "completed_with_errors", "Completed with Errors"
        FAILED = "failed", "Failed"

    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    owner = models.ForeignKey(settings.AUTH_USER_MODEL, on_delete=models.CASCADE, related_name="upload_batches")
    department = models.ForeignKey(
        "accounts.Department", on_delete=models.SET_NULL, null=True, blank=True, related_name="upload_batches"
    )
    requested_document_type = models.CharField(
        max_length=30, choices=Job.DocumentType.choices, default=Job.DocumentType.GENERIC
    )
    created_at = models.DateTimeField(auto_now_add=True)

    class Meta:
        ordering = ["-created_at"]
        indexes = [
            models.Index(fields=["owner", "created_at"]),
            models.Index(fields=["department", "created_at"]),
        ]

    def __str__(self):
        return f"Batch {str(self.id)[:8]}"

    @cached_property
    def job_counts(self):
        return self.jobs.aggregate(
            total=Count("id"),
            pending=Count("id", filter=Q(status=Job.Status.PENDING)),
            processing=Count(
                "id",
                filter=Q(status__in=[Job.Status.PROCESSING, Job.Status.REVIEWING, Job.Status.STOPPING]),
            ),
            completed=Count("id", filter=Q(status=Job.Status.COMPLETED)),
            needs_review=Count("id", filter=Q(status=Job.Status.NEEDS_REVIEW)),
            failed=Count("id", filter=Q(status=Job.Status.FAILED)),
            stopped=Count("id", filter=Q(status=Job.Status.STOPPED)),
        )

    @property
    def total_jobs(self):
        return int(self.job_counts.get("total") or 0)

    @property
    def pending_jobs(self):
        return int(self.job_counts.get("pending") or 0)

    @property
    def processing_jobs(self):
        return int(self.job_counts.get("processing") or 0)

    @property
    def completed_jobs(self):
        return int(self.job_counts.get("completed") or 0)

    @property
    def review_jobs(self):
        return int(self.job_counts.get("needs_review") or 0)

    @property
    def failed_jobs(self):
        return int(self.job_counts.get("failed") or 0)

    @property
    def stopped_jobs(self):
        return int(self.job_counts.get("stopped") or 0)

    @property
    def error_jobs(self):
        return self.failed_jobs + self.stopped_jobs

    @property
    def active_jobs(self):
        return self.pending_jobs + self.processing_jobs

    @property
    def finished_jobs(self):
        return self.completed_jobs + self.review_jobs + self.error_jobs

    @property
    def progress_percent(self):
        if self.total_jobs == 0:
            return 0
        return round((self.finished_jobs / self.total_jobs) * 100)

    @property
    def status(self):
        if self.total_jobs == 0:
            return self.Status.PROCESSING
        if self.active_jobs > 0:
            return self.Status.PROCESSING
        if self.failed_jobs == self.total_jobs:
            return self.Status.FAILED
        if self.error_jobs > 0:
            return self.Status.COMPLETED_WITH_ERRORS
        if self.review_jobs > 0:
            return self.Status.NEEDS_REVIEW
        return self.Status.COMPLETED

    @property
    def status_label(self):
        return self.Status(self.status).label


class PageResult(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    job = models.ForeignKey(Job, on_delete=models.CASCADE, related_name="pages")
    page_number = models.PositiveIntegerField()
    extracted_text = models.TextField(blank=True)
    tables_json = models.JSONField(default=list)
    extracted_fields = models.JSONField(default=dict)
    avg_confidence = models.FloatField(null=True, blank=True)
    azure_latency_s = models.FloatField(null=True, blank=True)
    raw_response = models.JSONField(null=True, blank=True)
    page_image = models.ImageField(upload_to="page_images/", null=True, blank=True)

    class Meta:
        unique_together = ("job", "page_number")
        ordering = ["page_number"]

    def __str__(self):
        return f"{self.job.filename} — page {self.page_number}"


class ReviewFlag(models.Model):
    id = models.UUIDField(primary_key=True, default=uuid.uuid4, editable=False)
    page_result = models.ForeignKey(PageResult, on_delete=models.CASCADE, related_name="flags")
    word = models.CharField(max_length=500)
    confidence = models.FloatField()
    polygon_json = models.JSONField(null=True, blank=True)
    span_offset = models.PositiveIntegerField(null=True, blank=True)
    span_length = models.PositiveIntegerField(null=True, blank=True)
    table_index = models.PositiveIntegerField(null=True, blank=True)
    row_index = models.PositiveIntegerField(null=True, blank=True)
    column_index = models.PositiveIntegerField(null=True, blank=True)
    reviewed = models.BooleanField(default=False)
    corrected_value = models.CharField(max_length=500, blank=True)
    reviewed_by = models.ForeignKey(
        settings.AUTH_USER_MODEL, on_delete=models.SET_NULL, null=True, blank=True
    )
    reviewed_at = models.DateTimeField(null=True, blank=True)
    admin_override = models.BooleanField(default=False)

    class Meta:
        ordering = ["confidence"]

    def __str__(self):
        return f'"{self.word}" ({self.confidence:.0%})'

    @property
    def confidence_level(self):
        if self.confidence >= 0.85:
            return "high"
        if self.confidence >= 0.50:
            return "medium"
        return "low"


class SystemConfig(models.Model):
    """Singleton workspace-level configuration stored in the database."""

    confidence_threshold = models.FloatField(default=0.70)
    default_document_type = models.CharField(
        max_length=30, choices=Job.DocumentType.choices, default=Job.DocumentType.GENERIC
    )
    pdf_dpi = models.IntegerField(default=200)
    azure_endpoint = models.CharField(max_length=500, blank=True)
    azure_key = models.CharField(max_length=500, blank=True)
    updated_at = models.DateTimeField(auto_now=True)

    class Meta:
        verbose_name = "System Configuration"

    def save(self, *args, **kwargs):
        self.pk = 1
        super().save(*args, **kwargs)

    @classmethod
    def get(cls):
        obj, _ = cls.objects.get_or_create(pk=1)
        return obj


class AuditLog(models.Model):
    """Records user actions for compliance and operational traceability."""

    user = models.ForeignKey(
        settings.AUTH_USER_MODEL,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name="audit_logs",
    )
    action = models.CharField(max_length=100)
    job = models.ForeignKey(
        Job, on_delete=models.SET_NULL, null=True, blank=True, related_name="audit_logs"
    )
    department = models.ForeignKey(
        "accounts.Department",
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name="audit_logs",
    )
    details = models.JSONField(default=dict)
    duration_ms = models.PositiveIntegerField(null=True, blank=True)
    ip_address = models.GenericIPAddressField(null=True, blank=True)
    timestamp = models.DateTimeField(default=timezone.now)

    class Meta:
        ordering = ["-timestamp"]
        indexes = [
            models.Index(fields=["user", "timestamp"]),
            models.Index(fields=["department", "timestamp"]),
        ]

    def __str__(self):
        actor = self.user.username if self.user else "anon"
        return f"{actor} — {self.action} @ {self.timestamp:%Y-%m-%d %H:%M}"
