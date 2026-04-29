from django.apps import AppConfig


class DocumentsConfig(AppConfig):
    default_auto_field = "django.db.models.BigAutoField"
    name = "documents"

    def ready(self):
        from django.contrib.auth.signals import user_logged_in
        from django.dispatch import receiver

        @receiver(user_logged_in)
        def on_login(sender, request, user, **kwargs):
            try:
                from .models import AuditLog
                AuditLog.objects.create(
                    user=user,
                    action="login",
                    details={"username": user.username},
                    ip_address=request.META.get("REMOTE_ADDR") if request else None,
                )
            except Exception:
                pass  # table may not exist before migrations are applied
