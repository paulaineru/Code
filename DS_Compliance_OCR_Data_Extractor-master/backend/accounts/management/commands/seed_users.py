from django.core.management.base import BaseCommand
from accounts.models import Department, User


class Command(BaseCommand):
    help = "Seed default admin and operator users for development"

    def handle(self, *args, **options):
        department, _ = Department.objects.get_or_create(
            name="Operations",
            defaults={"confidence_threshold": 0.70},
        )
        users = [
            {
                "username": "admin",
                "email": "admin@nssfug.org",
                "full_name": "Admin User",
                "role": User.Role.ADMIN,
                "is_staff": True,
                "is_superuser": True,
            },
            {
                "username": "supervisor",
                "email": "supervisor@nssfug.org",
                "full_name": "Department Supervisor",
                "role": User.Role.SUPERVISOR,
                "department": department,
                "is_staff": False,
                "is_superuser": False,
            },
            {
                "username": "operator",
                "email": "operator@nssfug.org",
                "full_name": "Operator User",
                "role": User.Role.OPERATOR,
                "department": department,
                "is_staff": False,
                "is_superuser": False,
            },
        ]

        created_users = {}
        for data in users:
            password = "changeme123"
            user, created = User.objects.get_or_create(
                username=data["username"],
                defaults={k: v for k, v in data.items() if k != "username"},
            )
            created_users[data["username"]] = user
            if created:
                user.set_password(password)
                user.save()
                self.stdout.write(self.style.SUCCESS(f"Created user: {data['email']} / {password}"))
            else:
                changed = False
                for field, value in data.items():
                    if field == "username":
                        continue
                    if getattr(user, field) != value:
                        setattr(user, field, value)
                        changed = True
                if changed:
                    user.save()
                self.stdout.write(f"User already exists: {data['email']}")

        operator = created_users.get("operator")
        supervisor = created_users.get("supervisor")
        if operator and supervisor and operator.supervisor_id != supervisor.id:
            operator.supervisor = supervisor
            operator.save(update_fields=["supervisor"])
