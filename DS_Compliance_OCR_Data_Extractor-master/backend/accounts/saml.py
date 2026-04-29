from __future__ import annotations

from django.http import HttpRequest, HttpResponse

# Azure AD SAML claim names.
# emailAddress matches the short friendly name Azure AD uses in its SAML
# assertions and must align with SAML_ATTRIBUTE_MAPPING in settings.
EMAIL_CLAIM = "emailAddress"

# Azure AD sends the user's Object ID under this URI claim name.
OID_CLAIM = "http://schemas.microsoft.com/identity/claims/objectidentifier"


def first_attribute_value(attributes: dict, claim: str) -> str | None:
    """Return the first string value for *claim* from a pysaml2 attribute dict,
    or None if the claim is absent or empty."""
    values = attributes.get(claim, [])
    return values[0] if values else None


def saml_csp_handler(response: HttpResponse) -> HttpResponse:
    """
    Called by djangosaml2 (SAML_CSP_HANDLER) after each SSO view response.
    SAML POST-binding pages submit a self-posting form, so they need
    'unsafe-inline' for scripts and forms.  We append that to whatever
    CSP the main middleware already set, rather than replacing it.
    """
    existing = response.get("Content-Security-Policy", "")
    if existing:
        # Only patch if a CSP is already present.
        if "script-src" not in existing:
            existing += "; script-src 'self' 'unsafe-inline'"
        elif "'unsafe-inline'" not in existing:
            existing = existing.replace("script-src", "script-src 'unsafe-inline'", 1)
        response["Content-Security-Policy"] = existing
    return response


def is_saml_authenticated(request: HttpRequest) -> bool:
    """Return True if the current session was established via SAML SSO."""
    saml_session = getattr(request, "saml_session", None)
    return bool(saml_session)

# Maps Azure AD jobTitle / Role claim values (case-insensitive) to app Role choices.
# Azure AD is configured to send the "Role" claim (→ user.jobtitle) and the
# "Department" claim (→ user.department).  Add entries here as new titles appear.
#
# Supervisor tier  – job titles that imply line management over a team.
# Operator tier    – individual contributors (normal users).
# Admin tier       – full application oversight.
_TITLE_TO_ROLE: dict[str, str] = {
    # ── Admin ─────────────────────────────────────────────────────────
    "admin":            "admin",
    "administrator":    "admin",
    # ── Supervisor ────────────────────────────────────────────────────
    "supervisor":       "supervisor",
    "manager":          "supervisor",
    "superintendent":   "supervisor",
    "team lead":        "supervisor",
    "team leader":      "supervisor",
    "head":             "supervisor",
    "senior manager":   "supervisor",
    "deputy manager":   "supervisor",
    # ── Operator (normal user) ─────────────────────────────────────────
    "operator":         "operator",
    "data entry":       "operator",
    "officer":          "operator",
    "specialist":       "operator",
    "engineer":         "operator",
    "analyst":          "operator",
    "auditor":          "operator",
    "clerk":            "operator",
    "assistant":        "operator",
    "associate":        "operator",
}


def resolve_role_from_title(title: str) -> str | None:
    """
    Return the app role string for a raw job-title string, or *None* if
    the title is unrecognised.  Matching is case-insensitive and trims
    whitespace.  Partial / substring matching is intentionally avoided so
    that unexpected titles fall through to the current user role rather
    than being silently mis-mapped.
    """
    return _TITLE_TO_ROLE.get(title.strip().lower())


def populate_azure_oid(user, saml_attributes: dict) -> None:
    """
    After a successful SAML login:
      - Sync Azure AD Object ID onto the user record.
      - Translate jobTitle / Role claim → app role (operator / supervisor / admin).
      - Sync department from the "Department" or "department" SAML claim.

    This function is called from OryxSaml2Backend._update_user so that role
    and department stay in sync on every login, not just the first.
    """
    updates: dict = {}

    # ── Azure AD Object ID ─────────────────────────────────────────────
    oid_values = saml_attributes.get(OID_CLAIM, [])
    oid = oid_values[0] if oid_values else None
    if oid and user.azure_oid != oid:
        updates["azure_oid"] = oid

    # ── Role from the "Role" claim (Azure custom claim → user.jobtitle)
    #    with fallback to the built-in "jobTitle" attribute name.
    title_values = (
        saml_attributes.get("Role", [])
        or saml_attributes.get("jobTitle", [])
    )
    title = (title_values[0] if title_values else "").strip()
    if title:
        mapped_role = resolve_role_from_title(title)
        if mapped_role and user.role != mapped_role:
            updates["role"] = mapped_role

    # ── Department — check both capitalisation variants Azure may send ──
    dept_values = (
        saml_attributes.get("Department", [])
        or saml_attributes.get("department", [])
    )
    dept_name = (dept_values[0] if dept_values else "").strip()
    if dept_name:
        from accounts.models import Department  # local import avoids circular
        dept_obj = Department.objects.filter(name__iexact=dept_name).first()
        if dept_obj and user.department_id != dept_obj.id:
            updates["department_id"] = dept_obj.id

    if updates:
        type(user).objects.filter(pk=user.pk).update(**updates)
        for field, value in updates.items():
            setattr(user, field, value)
