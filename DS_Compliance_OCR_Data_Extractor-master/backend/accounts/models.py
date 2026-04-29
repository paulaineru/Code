from django.contrib.auth.models import AbstractUser
from django.core.exceptions import ValidationError
from django.db import models
from django.db.models import Q


class Department(models.Model):
    name = models.CharField(max_length=120, unique=True)
    confidence_threshold = models.FloatField(default=0.70)
    created_at = models.DateTimeField(auto_now_add=True)
    updated_at = models.DateTimeField(auto_now=True)

    class Meta:
        ordering = ["name"]

    def __str__(self):
        return self.name

    def clean(self):
        if not 0.50 <= float(self.confidence_threshold) <= 0.95:
            raise ValidationError({"confidence_threshold": "Confidence threshold must be between 0.50 and 0.95."})


class User(AbstractUser):
    class Role(models.TextChoices):
        OPERATOR = "operator", "Operator"
        SUPERVISOR = "supervisor", "Supervisor"
        ADMIN = "admin", "Admin"

    email = models.EmailField(unique=True)
    full_name = models.CharField(max_length=255, blank=True)
    role = models.CharField(max_length=20, choices=Role.choices, default=Role.OPERATOR)
    department = models.ForeignKey(
        Department,
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name="users",
    )
    supervisor = models.ForeignKey(
        "self",
        on_delete=models.SET_NULL,
        null=True,
        blank=True,
        related_name="team_members",
        limit_choices_to={"role": "supervisor"},
    )
    azure_oid = models.CharField(max_length=255, unique=True, null=True, blank=True)

    class Meta(AbstractUser.Meta):
        indexes = [models.Index(fields=["role", "department"])]
        constraints = [
            # Operators and supervisors must belong to a department.
            # Superusers are exempt so `createsuperuser` and SAML-provisioned
            # admins (who haven't been assigned a department yet) are not blocked.
            # Role assignment is local-admin-only; SAML never writes this field.
            models.CheckConstraint(
                check=Q(role="admin") | Q(is_superuser=True) | Q(department__isnull=False),
                name="non_admin_user_requires_department",
            ),
        ]

    def get_initials(self):
        parts = (self.full_name or self.username or "?").split()
        return "".join(p[0].upper() for p in parts[:2])

    @property
    def is_admin(self) -> bool:
        return self.is_superuser or self.role == self.Role.ADMIN

    @property
    def is_supervisor(self) -> bool:
        return self.role == self.Role.SUPERVISOR

    @property
    def is_operator(self) -> bool:
        return self.role == self.Role.OPERATOR

    def clean(self):
        super().clean()

        errors = {}
        if self.role in {self.Role.OPERATOR, self.Role.SUPERVISOR} and not self.department_id:
            errors["department"] = "Operators and supervisors must belong to a department."

        if self.role == self.Role.OPERATOR and not self.supervisor_id:
            errors["supervisor"] = "Operators must be assigned to a supervisor."

        if self.role in {self.Role.ADMIN, self.Role.SUPERVISOR} and self.supervisor_id:
            errors["supervisor"] = "Only operators can have a supervisor assignment."

        if self.supervisor_id:
            if self.supervisor_id == self.pk:
                errors["supervisor"] = "A user cannot supervise themselves."
            elif self.supervisor and self.supervisor.role != self.Role.SUPERVISOR:
                errors["supervisor"] = "Assigned supervisor must have the supervisor role."
            elif (
                self.department_id
                and self.supervisor
                and self.supervisor.department_id
                and self.supervisor.department_id != self.department_id
            ):
                errors["supervisor"] = "Operators and supervisors must belong to the same department."

        if errors:
            raise ValidationError(errors)
