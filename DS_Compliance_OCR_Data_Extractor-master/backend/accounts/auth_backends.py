from __future__ import annotations

import logging
from datetime import datetime, timezone
from typing import Any

from django.conf import settings
from django.core.cache import cache
from django.core.exceptions import MultipleObjectsReturned
from django.utils.dateparse import parse_datetime
from djangosaml2.backends import Saml2Backend

from accounts.models import Department, User
from accounts.saml import EMAIL_CLAIM, OID_CLAIM, first_attribute_value, resolve_role_from_title

logger = logging.getLogger("djangosaml2")


class OryxSaml2Backend(Saml2Backend):
    def clean_user_main_attribute(self, main_attribute: Any) -> Any:
        if isinstance(main_attribute, str):
            return main_attribute.strip().lower()
        return main_attribute

    def is_authorized(
        self,
        attributes: dict,
        attribute_mapping: dict,
        idp_entityid: str,
        assertion_info: dict | None,
        **kwargs,
    ) -> bool:
        if not super().is_authorized(
            attributes,
            attribute_mapping,
            idp_entityid,
            assertion_info,
            **kwargs,
        ):
            return False

        assertion_id = (assertion_info or {}).get("assertion_id")
        if not assertion_id:
            return True

        cache_key = f"saml-assertion:{idp_entityid}:{assertion_id}"
        timeout = self._assertion_cache_timeout((assertion_info or {}).get("not_on_or_after"))
        if not cache.add(cache_key, True, timeout=timeout):
            logger.warning("Rejected replayed SAML assertion %s from %s", assertion_id, idp_entityid)
            return False
        return True

    def get_or_create_user(
        self,
        user_lookup_key: str,
        user_lookup_value: Any,
        create_unknown_user: bool,
        idp_entityid: str,
        attributes: dict,
        attribute_mapping: dict,
        request,
    ) -> tuple[User | None, bool]:
        UserModel = self._user_model

        oid = first_attribute_value(attributes, OID_CLAIM)
        if oid:
            try:
                return UserModel.objects.get(azure_oid=oid), False
            except UserModel.DoesNotExist:
                pass

        email = self.clean_user_main_attribute(
            first_attribute_value(attributes, EMAIL_CLAIM) or user_lookup_value
        )
        if not email:
            return None, False

        try:
            return UserModel.objects.get(email__iexact=email), False
        except MultipleObjectsReturned:
            logger.exception("Multiple users matched SAML email lookup for %s", email)
            return None, False
        except UserModel.DoesNotExist:
            if not create_unknown_user:
                logger.warning("No local user matched SAML login for %s", email)
                return None, False

        user = UserModel(
            email=email,
            username=self._build_unique_username(UserModel, email, oid),
            azure_oid=oid or None,
        )
        user.set_unusable_password()
        return user, True

    def _update_user(
        self,
        user: User,
        attributes: dict,
        attribute_mapping: dict,
        force_save: bool = False,
    ) -> User:
        # For a brand-new (unsaved) user, pre-populate department before the
        # first DB insert so the non-admin-requires-department constraint is
        # satisfied on the very first save.
        is_new = user.pk is None
        if is_new:
            self._sync_department(user, attributes)
            # If the department claim wasn't available, the constraint
            # (role='admin' OR is_superuser OR department IS NOT NULL) would
            # be violated on the first INSERT.  Fall back to 'admin' so the
            # user can log in; a local admin can assign the correct role later.
            if user.department_id is None and not user.is_superuser and user.role != "admin":
                user.role = "operator"

        user = super()._update_user(
            user,
            attributes,
            attribute_mapping,
            force_save=force_save,
        )

        updated_fields: list[str] = []
        if self._sync_full_name(user):
            updated_fields.append("full_name")
        if not is_new and self._sync_department(user, attributes):
            updated_fields.append("department")
        if self._sync_role(user, attributes):
            updated_fields.append("role")

        if updated_fields:
            user.save(update_fields=updated_fields)

        return user

    @staticmethod
    def _assertion_cache_timeout(not_on_or_after: Any) -> int:
        default_timeout = int(getattr(settings, "SAML_ASSERTION_REPLAY_CACHE_SECONDS", 300))
        # pysaml2 returns not_on_or_after as a Unix int in live sessions
        if isinstance(not_on_or_after, (int, float)):
            not_on_or_after = datetime.fromtimestamp(not_on_or_after, tz=timezone.utc)
        elif isinstance(not_on_or_after, str):
            not_on_or_after = parse_datetime(not_on_or_after)
        if not isinstance(not_on_or_after, datetime):
            return default_timeout
        if not_on_or_after.tzinfo is None:
            not_on_or_after = not_on_or_after.replace(tzinfo=timezone.utc)
        remaining = int((not_on_or_after.astimezone(timezone.utc) - datetime.now(timezone.utc)).total_seconds())
        return max(remaining, 60)

    @staticmethod
    def _build_unique_username(UserModel, email: str, oid: str | None) -> str:
        base = email.lower()
        candidate = base[:150]
        if not UserModel.objects.filter(username__iexact=candidate).exists():
            return candidate

        suffixes = []
        if oid:
            suffixes.append(oid.split("-")[0].lower())
        suffixes.extend(str(index) for index in range(1, 1000))

        local, _, domain = base.partition("@")
        stem = local or base
        for suffix in suffixes:
            candidate = f"{stem}-{suffix}"
            if domain:
                candidate = f"{candidate}@{domain}"
            candidate = candidate[:150]
            if not UserModel.objects.filter(username__iexact=candidate).exists():
                return candidate

        return f"user-{datetime.now(timezone.utc).strftime('%Y%m%d%H%M%S')}"[:150]

    @staticmethod
    def _sync_full_name(user: User) -> bool:
        full_name = " ".join(part for part in [user.first_name, user.last_name] if part).strip()
        if full_name and user.full_name != full_name:
            user.full_name = full_name
            return True
        return False

    @staticmethod
    def _sync_department(user: User, attributes: dict) -> bool:
        claim_name = getattr(settings, "SAML_DEPARTMENT_CLAIM", "").strip()
        if not claim_name:
            return False

        department_name = first_attribute_value(attributes, claim_name)
        if not department_name:
            return False

        department_name = department_name.strip()
        department = Department.objects.filter(name__iexact=department_name).first()
        if department is None and getattr(settings, "SAML_CREATE_UNKNOWN_DEPARTMENTS", False):
            department = Department.objects.create(name=department_name)
        if department is None or user.department_id == department.id:
            return False

        user.department = department
        return True

    @staticmethod
    def _sync_role(user: User, attributes: dict) -> bool:
        """
        Translate the Azure AD "Role" SAML claim (→ user.jobtitle) into the
        app's role field.  Falls back to the built-in "jobTitle" attribute name
        in case the tenant is not using the custom claim.

        Returns True if the role was updated so the caller can include "role"
        in save(update_fields=...).

        Admins who were manually promoted via Django admin are never
        down-graded by this sync — their role can only change if a
        higher-privilege mapping is sent.
        """
        title_values = (
            attributes.get("Role", [])
            or attributes.get("jobTitle", [])
        )
        raw_title = (title_values[0] if title_values else "").strip()
        if not raw_title:
            return False

        mapped_role = resolve_role_from_title(raw_title)
        if mapped_role is None:
            logger.info(
                "SAML login for %s: unrecognised job title %r — role unchanged (%s)",
                user.email,
                raw_title,
                user.role,
            )
            return False

        # Never silently demote a manually-assigned admin.
        if user.role == User.Role.ADMIN and mapped_role != User.Role.ADMIN:
            logger.info(
                "SAML login for %s: role claim %r would demote admin — keeping admin",
                user.email,
                raw_title,
            )
            return False

        if user.role == mapped_role:
            return False

        logger.info(
            "SAML login for %s: role %r → %r (from title %r)",
            user.email,
            user.role,
            mapped_role,
            raw_title,
        )
        user.role = mapped_role
        return True

