from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ("documents", "0004_rename_auditlog_user_ts_idx_documents_a_user_id_481fbb_idx"),
    ]

    operations = [
        migrations.AddField(
            model_name="job",
            name="pages_done",
            field=models.PositiveIntegerField(default=0),
        ),
    ]
