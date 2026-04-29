from django.db import migrations, models


class Migration(migrations.Migration):
    dependencies = [
        ("documents", "0009_batchupload_job_batch"),
    ]

    operations = [
        migrations.AlterField(
            model_name="job",
            name="status",
            field=models.CharField(
                choices=[
                    ("pending", "Pending"),
                    ("processing", "Processing"),
                    ("stopping", "Stopping"),
                    ("stopped", "Stopped"),
                    ("reviewing", "Applying Reviews"),
                    ("completed", "Completed"),
                    ("needs_review", "Needs Review"),
                    ("failed", "Failed"),
                ],
                default="pending",
                max_length=20,
            ),
        ),
    ]
