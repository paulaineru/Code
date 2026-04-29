import django.db.models.deletion
import uuid
from django.conf import settings
from django.db import migrations, models


class Migration(migrations.Migration):
    dependencies = [
        ("documents", "0008_job_processing_task_id"),
        migrations.swappable_dependency(settings.AUTH_USER_MODEL),
    ]

    operations = [
        migrations.CreateModel(
            name="BatchUpload",
            fields=[
                (
                    "id",
                    models.UUIDField(
                        default=uuid.uuid4,
                        editable=False,
                        primary_key=True,
                        serialize=False,
                    ),
                ),
                (
                    "requested_document_type",
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
                ("created_at", models.DateTimeField(auto_now_add=True)),
                (
                    "owner",
                    models.ForeignKey(
                        on_delete=django.db.models.deletion.CASCADE,
                        related_name="upload_batches",
                        to=settings.AUTH_USER_MODEL,
                    ),
                ),
            ],
            options={
                "ordering": ["-created_at"],
                "indexes": [
                    models.Index(
                        fields=["owner", "created_at"],
                        name="documents_b_owner_i_43d39f_idx",
                    ),
                ],
            },
        ),
        migrations.AddField(
            model_name="job",
            name="batch",
            field=models.ForeignKey(
                blank=True,
                null=True,
                on_delete=django.db.models.deletion.SET_NULL,
                related_name="jobs",
                to="documents.batchupload",
            ),
        ),
    ]
