from django.contrib import admin
from .models import BatchUpload, Job, PageResult, ReviewFlag


@admin.register(BatchUpload)
class BatchUploadAdmin(admin.ModelAdmin):
    list_display = ("id", "owner", "requested_document_type", "created_at")
    list_filter = ("requested_document_type", "created_at")
    search_fields = ("id", "owner__username", "owner__email")
    readonly_fields = ("id", "created_at")


@admin.register(Job)
class JobAdmin(admin.ModelAdmin):
    list_display = ("filename", "owner", "batch", "document_type", "status", "total_pages", "created_at")
    list_filter = ("status", "document_type")
    search_fields = ("filename", "owner__username", "owner__email", "batch__id")
    readonly_fields = ("id", "created_at", "completed_at")


@admin.register(PageResult)
class PageResultAdmin(admin.ModelAdmin):
    list_display = ("job", "page_number", "avg_confidence", "azure_latency_s")
    list_filter = ("job__status",)
    search_fields = ("job__filename",)
    readonly_fields = ("id",)


@admin.register(ReviewFlag)
class ReviewFlagAdmin(admin.ModelAdmin):
    list_display = ("word", "confidence", "reviewed", "admin_override", "page_result", "reviewed_by")
    list_filter = ("reviewed", "admin_override")
    search_fields = ("word",)
    readonly_fields = ("id",)
