from django.db.models import Q

from documents.models import AuditLog, BatchUpload, Job


def department_resource_q(department_id):
    """Q filter for any owner-bearing model that carries an optional department FK."""
    return Q(department_id=department_id) | Q(department__isnull=True, owner__department_id=department_id)


def department_audit_q(department_id):
    return (
        Q(department_id=department_id)
        | Q(department__isnull=True, job__department_id=department_id)
        | Q(department__isnull=True, job__department__isnull=True, user__department_id=department_id)
    )


def user_is_admin(user) -> bool:
    return bool(getattr(user, "is_authenticated", False) and getattr(user, "is_admin", False))


def user_is_supervisor(user) -> bool:
    return bool(getattr(user, "is_authenticated", False) and getattr(user, "is_supervisor", False))


def user_is_operator(user) -> bool:
    return bool(getattr(user, "is_authenticated", False) and getattr(user, "is_operator", False))


def can_upload_documents(user) -> bool:
    """Admins, supervisors, and operators can all upload documents."""
    return user_is_admin(user) or user_is_supervisor(user) or user_is_operator(user)


def can_manage_global_settings(user) -> bool:
    return user_is_admin(user)


def can_manage_department_settings(user) -> bool:
    return user_is_supervisor(user) and bool(getattr(user, "department_id", None))


def can_view_activity_dashboard(user) -> bool:
    return user_is_admin(user) or user_is_supervisor(user)


def show_owner_details(user) -> bool:
    return user_is_admin(user) or user_is_supervisor(user)


def can_export_audit_trail(user) -> bool:
    """Only supervisors (scoped to their dept) and admins may export audit logs."""
    return user_is_admin(user) or (user_is_supervisor(user) and bool(getattr(user, "department_id", None)))


def visible_jobs_queryset(user):
    if user_is_admin(user):
        return Job.objects.all()
    if user_is_supervisor(user):
        if not user.department_id:
            return Job.objects.none()
        return Job.objects.filter(department_resource_q(user.department_id))
    return Job.objects.filter(owner=user)


def visible_batches_queryset(user):
    if user_is_admin(user):
        return BatchUpload.objects.all()
    if user_is_supervisor(user):
        if not user.department_id:
            return BatchUpload.objects.none()
        return BatchUpload.objects.filter(department_resource_q(user.department_id))
    return BatchUpload.objects.filter(owner=user)


def visible_audit_logs_queryset(user):
    if user_is_admin(user):
        return AuditLog.objects.all()
    if user_is_supervisor(user):
        if not user.department_id:
            return AuditLog.objects.none()
        return AuditLog.objects.filter(department_audit_q(user.department_id))
    return AuditLog.objects.filter(user=user)


def visible_users_queryset(user):
    from accounts.models import User

    if user_is_admin(user):
        return User.objects.all()
    if user_is_supervisor(user):
        if not user.department_id:
            return User.objects.none()
        return User.objects.filter(department_id=user.department_id)
    return User.objects.filter(pk=user.pk)


def can_manage_job(user, job) -> bool:
    """
    Admins can manage any job.
    Supervisors can manage any job within their department (override capability).
    Operators can only manage their own jobs.
    """
    if user_is_admin(user):
        return True
    if user_is_supervisor(user):
        dept_id = getattr(user, "department_id", None)
        if dept_id:
            job_dept = getattr(job, "department_id", None)
            owner_dept = getattr(getattr(job, "owner", None), "department_id", None)
            return job_dept == dept_id or (job_dept is None and owner_dept == dept_id)
    return getattr(job, "owner_id", None) == getattr(user, "id", None)
