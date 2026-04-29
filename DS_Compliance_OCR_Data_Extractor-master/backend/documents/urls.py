from django.urls import path
from . import views

urlpatterns = [
    # ── Workspace ────────────────────────────────────────────────────
    path("workspace/", views.document_list_view, name="document_list"),
    path("workspace/upload/", views.upload_view, name="upload"),
    path("workspace/batches/<uuid:batch_id>/", views.batch_detail_view, name="batch_detail"),
    path("workspace/review/", views.review_view, name="review"),
    path("workspace/<uuid:job_id>/", views.document_detail_view, name="document_detail"),
    path("workspace/<uuid:job_id>/status/", views.document_status_view, name="document_status"),
    path("workspace/<uuid:job_id>/stop/", views.stop_job_view, name="stop_job"),
    path("workspace/<uuid:job_id>/restart/", views.restart_job_view, name="restart_job"),
    path("workspace/<uuid:job_id>/pages/<int:page_num>/preview/", views.page_image_view, name="page_image"),
    path("workspace/<uuid:job_id>/download/<str:fmt>/", views.export_download_view, name="export_download"),
    path("workspace/<uuid:job_id>/flags/approve/", views.approve_flags_view, name="approve_flags"),

    # ── Settings ─────────────────────────────────────────────────────
    path("settings/", views.settings_view, name="settings"),
    path("settings/save/", views.settings_save_view, name="settings_save"),

    # ── Control (admin / supervisor) ─────────────────────────────────
    path("control/audit/", views.audit_log_view, name="audit_log"),
    path("control/audit/export/", views.export_audit_trail_view, name="audit_log_export"),
    path("control/users/", views.admin_stats_view, name="admin_stats"),
]
