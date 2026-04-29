import time
from django.conf import settings
from django.contrib.auth import logout
from django.http import JsonResponse
from django.shortcuts import redirect

_EXEMPT_PREFIXES = ("/auth/login/", "/auth/logout/", "/auth/session/", "/sso/", "/health/")
_BACKGROUND_HEADER = "HTTP_X_ORYX_BACKGROUND"


class SessionIdleTimeoutMiddleware:
    """
    - Logs out an authenticated user after SESSION_IDLE_TIMEOUT seconds of
    inactivity (default 300 s / 5 minutes).  
    - Each successful request resets the idle clock.  
    - The timeout redirect appends ?timeout=1 so the login
    page can show a "You were logged out due to inactivity" message.
    """

    def __init__(self, get_response):
        self.get_response = get_response

    def __call__(self, request):
        if request.user.is_authenticated and not self._is_exempt(request.path):
            last_activity = request.session.get("_last_activity")
            now = time.time()
            idle_timeout = self._idle_timeout()

            if last_activity is not None and (now - float(last_activity)) > idle_timeout:
                logout(request)
                if self._is_background(request):
                    return JsonResponse(
                        {
                            "expired": True,
                            "login_url": f"{settings.LOGIN_URL}?timeout=1",
                            "message": "Your session expired due to inactivity.",
                        },
                        status=401,
                    )
                return redirect(f"{settings.LOGIN_URL}?timeout=1")

            if not self._is_background(request):
                request.session["_last_activity"] = now

        return self.get_response(request)

    @staticmethod
    def _is_exempt(path: str) -> bool:
        return any(path.startswith(prefix) for prefix in _EXEMPT_PREFIXES)

    @staticmethod
    def _is_background(request) -> bool:
        return request.META.get(_BACKGROUND_HEADER) == "1"

    @staticmethod
    def _idle_timeout() -> int:
        return int(getattr(settings, "SESSION_IDLE_TIMEOUT", 300))
