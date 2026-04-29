from django.conf import settings
from django.db import migrations, models
import django.db.models.deletion
import django.utils.timezone


class Migration(migrations.Migration):

    dependencies = [
        ("documents", "0001_initial"),
        migrations.swappable_dependency(settings.AUTH_USER_MODEL),
    ]

    operations = [
        migrations.CreateModel(
            name="SystemConfig",
            fields=[
                ("id", models.BigAutoField(auto_created=True, primary_key=True, serialize=False, verbose_name="ID")),
                ("confidence_threshold", models.FloatField(default=0.7)),
                (
                    "default_document_type",
                    models.CharField(
                        choices=[
                            ("generic", "Generic"),
                            ("receipt", "Receipt"),
                            ("invoice", "Invoice"),
                            ("bank_statement", "Bank Statement"),
                            ("payroll", "Payroll"),
                        ],
                        default="generic",
                        max_length=30,
                    ),
                ),
                ("pdf_dpi", models.IntegerField(default=200)),
                ("azure_endpoint", models.CharField(blank=True, max_length=500)),
                ("azure_key", models.CharField(blank=True, max_length=500)),
                ("updated_at", models.DateTimeField(auto_now=True)),
            ],
            options={
                "verbose_name": "System Configuration",
            },
        ),
        migrations.CreateModel(
            name="AuditLog",
            fields=[
                ("id", models.BigAutoField(auto_created=True, primary_key=True, serialize=False, verbose_name="ID")),
                ("action", models.CharField(max_length=100)),
                ("details", models.JSONField(default=dict)),
                ("duration_ms", models.PositiveIntegerField(blank=True, null=True)),
                ("ip_address", models.GenericIPAddressField(blank=True, null=True)),
                ("timestamp", models.DateTimeField(default=django.utils.timezone.now)),
                (
                    "job",
                    models.ForeignKey(
                        blank=True,
                        null=True,
                        on_delete=django.db.models.deletion.SET_NULL,
                        related_name="audit_logs",
                        to="documents.job",
                    ),
                ),
                (
                    "user",
                    models.ForeignKey(
                        blank=True,
                        null=True,
                        on_delete=django.db.models.deletion.SET_NULL,
                        related_name="audit_logs",
                        to=settings.AUTH_USER_MODEL,
                    ),
                ),
            ],
            options={
                "ordering": ["-timestamp"],
                "indexes": [
                    models.Index(fields=["user", "timestamp"], name="auditlog_user_ts_idx"),
                ],
            },
        ),
    ]
