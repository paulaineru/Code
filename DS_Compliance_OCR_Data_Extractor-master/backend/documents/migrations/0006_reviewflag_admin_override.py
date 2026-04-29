from django.db import migrations, models


class Migration(migrations.Migration):

    dependencies = [
        ("documents", "0005_job_pages_done"),
    ]

    operations = [
        migrations.AddField(
            model_name="reviewflag",
            name="admin_override",
            field=models.BooleanField(default=False),
        ),
    ]
