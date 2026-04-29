from django.db import migrations, models
from django.db.models import Q


def backfill_department_or_promote(apps, schema_editor):
    """
    Before the constraint is added, ensure every existing user satisfies it.
    Any user who is not a superuser and has no department gets promoted to
    the 'admin' role so the constraint can be applied cleanly.
    """
    User = apps.get_model("accounts", "User")
    User.objects.filter(
        is_superuser=False,
        department__isnull=True,
    ).exclude(role="admin").update(role="admin")


class Migration(migrations.Migration):

    dependencies = [
        ("accounts", "0002_user_department"),
    ]

    operations = [
        migrations.RunPython(backfill_department_or_promote, migrations.RunPython.noop),
        migrations.AddConstraint(
            model_name="user",
            constraint=models.CheckConstraint(
                check=Q(role="admin") | Q(is_superuser=True) | Q(department__isnull=False),
                name="non_admin_user_requires_department",
            ),
        ),
    ]
