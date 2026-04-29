import platform

from .base import *  # noqa: F401, F403

DEBUG = True
ALLOWED_HOSTS = ["*"]
CSRF_TRUSTED_ORIGINS = [
    "http://localhost:8000",
    "http://127.0.0.1:8000",
    "https://oryx.nssfug.org",
]

# Trust the X-Forwarded-Proto header set by nginx so Django knows the request
# arrived over HTTPS — required for SAML, secure cookies, and CSRF to work correctly.
SECURE_PROXY_SSL_HEADER = ("HTTP_X_FORWARDED_PROTO", "https")
SESSION_COOKIE_SECURE = True
CSRF_COOKIE_SECURE = True

# Windows: prefork uses Unix semaphores that fail on Windows (WinError 5/6).
# Inside Docker (Linux) the default prefork pool works correctly.
if platform.system() == "Windows":
    CELERY_WORKER_POOL = "solo"
    CELERY_WORKER_CONCURRENCY = 1
