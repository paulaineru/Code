# This migration was superseded by 0002_department_user_supervisor_alter_user_role_and_more
# which was merged from the bul-file-upload branch. It is kept as a no-op so that
# environments that recorded it as applied do not error on migrate.
from django.db import migrations


class Migration(migrations.Migration):

    dependencies = [
        ("accounts", "0002_department_user_supervisor_alter_user_role_and_more"),
    ]

    operations = []
