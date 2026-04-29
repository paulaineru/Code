import math
import time
from urllib.parse import urlencode

from django.conf import settings
from django.contrib import messages
from django.contrib.auth import logout
from django.contrib.auth.views import LoginView
from django.http import JsonResponse
from django.shortcuts import redirect, resolve_url
from django.views import View
from django.views.decorators.http import require_GET, require_POST

from accounts.saml import is_saml_authenticated


def _idle_timeout_seconds() -> int:
    return int(getattr(settings, "SESSION_IDLE_TIMEOUT", 300))


def _timeout_redirect_url() -> str:
    return f"{settings.LOGIN_URL}?timeout=1"


def _remaining_seconds(request, now: float | None = None) -> int:
    last_activity = request.session.get("_last_activity")
    if last_activity is None:
        return _idle_timeout_seconds()
    current = now or time.time()
    remaining = int(_idle_timeout_seconds() - (current - float(last_activity)))
    return max(remaining, 0)


def _timeout_response(request) -> JsonResponse:
    logout(request)
    return JsonResponse(
        {
            "expired": True,
            "login_url": _timeout_redirect_url(),
            "message": "Your session expired due to inactivity.",
        },
        status=401,
    )


class OryxLoginView(LoginView):
    template_name = "registration/login.html"

    def dispatch(self, request, *args, **kwargs):
        if request.method == "POST" and not getattr(settings, "ALLOW_LOCAL_LOGIN", True):
            messages.error(request, "Username/password sign-in is disabled. Use NSSF SSO.")
            return redirect("login")
        return super().dispatch(request, *args, **kwargs)

    def get_context_data(self, **kwargs):
        context = super().get_context_data(**kwargs)
        context["saml_enabled"] = bool(getattr(settings, "SAML_CONFIG", None))
        context["show_local_login"] = bool(getattr(settings, "ALLOW_LOCAL_LOGIN", True))
        return context

    def get(self, request, *args, **kwargs):
        if request.GET.get("timeout"):
            minutes = max(1, math.ceil(_idle_timeout_seconds() / 60))
            messages.warning(request, f"You were signed out after {minutes} minute{'s' if minutes != 1 else ''} of inactivity.")
        return super().get(request, *args, **kwargs)

    def form_invalid(self, form):
        messages.error(self.request, "Invalid username or password. Please try again.")
        return super().form_invalid(form)


class OryxLogoutView(View):
    http_method_names = ["get", "post"]

    def get(self, request, *args, **kwargs):
        if not (getattr(settings, "SAML_CONFIG", None) and is_saml_authenticated(request)):
            return redirect(resolve_url(getattr(settings, "LOGOUT_REDIRECT_URL", settings.LOGIN_URL)))
        return self._logout(request)

    def post(self, request, *args, **kwargs):
        return self._logout(request)

    def _logout(self, request):
        if getattr(settings, "SAML_CONFIG", None) and is_saml_authenticated(request):
            next_url = resolve_url(getattr(settings, "LOGOUT_REDIRECT_URL", settings.LOGIN_URL))
            query = urlencode({"next": next_url})
            return redirect(f"{resolve_url('saml2_logout')}?{query}")

        logout(request)
        return redirect(resolve_url(getattr(settings, "LOGOUT_REDIRECT_URL", settings.LOGIN_URL)))


@require_POST
def session_activity_view(request):
    if not request.user.is_authenticated:
        return JsonResponse({"expired": True, "login_url": _timeout_redirect_url()}, status=401)

    now = time.time()
    if _remaining_seconds(request, now=now) <= 0:
        return _timeout_response(request)

    request.session["_last_activity"] = now
    return JsonResponse(
        {
            "expired": False,
            "remaining_seconds": _remaining_seconds(request, now=now),
        }
    )


@require_GET
def session_status_view(request):
    if not request.user.is_authenticated:
        return JsonResponse({"expired": True, "login_url": _timeout_redirect_url()}, status=401)

    now = time.time()
    remaining = _remaining_seconds(request, now=now)
    if remaining <= 0:
        return _timeout_response(request)

    return JsonResponse(
        {
            "expired": False,
            "remaining_seconds": remaining,
            "warning_threshold_seconds": 60,
        }
    )
