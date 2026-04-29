from django.db import migrations, models


class Migration(migrations.Migration):
    """Remove the redundant 'uploads/' subdirectory from FileField.upload_to.
    MEDIA_ROOT already points to the uploads directory; the extra subfolder
    caused files to land at .../uploads/uploads/<filename>.
    """

    dependencies = [
        ("documents", "0002_systemconfig_auditlog"),
    ]

    operations = [
        migrations.AlterField(
            model_name="job",
            name="upload",
            field=models.FileField(upload_to=""),
        ),
    ]
