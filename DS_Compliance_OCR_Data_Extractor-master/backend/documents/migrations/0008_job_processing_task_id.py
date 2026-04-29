from django.db import migrations, models


class Migration(migrations.Migration):
    dependencies = [
        ("documents", "0007_reviewflag_column_index_reviewflag_row_index_and_more"),
    ]

    operations = [
        migrations.AddField(
            model_name="job",
            name="processing_task_id",
            field=models.CharField(blank=True, max_length=255),
        ),
    ]
