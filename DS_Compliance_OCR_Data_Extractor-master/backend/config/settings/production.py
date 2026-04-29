from .base import *
import environ
from urllib.parse import urlparse

env = environ.Env()

DEBUG = False
_default_host = urlparse(_SAML_BASE_URL).hostname if _SAML_BASE_URL else None
ALLOWED_HOSTS = [h.strip() for h in env("DJANGO_ALLOWED_HOSTS", default="").split(",") if h.strip()]
CSRF_TRUSTED_ORIGINS = [h.strip() for h in env("CSRF_TRUSTED_ORIGINS", default="").split(",") if h.strip()]
if not ALLOWED_HOSTS and _default_host:
    ALLOWED_HOSTS = [_default_host]
if not CSRF_TRUSTED_ORIGINS and _SAML_BASE_URL:
    CSRF_TRUSTED_ORIGINS = [_SAML_BASE_URL]

# Security hardening
SECURE_BROWSER_XSS_FILTER = True
SECURE_CONTENT_TYPE_NOSNIFF = True
SECURE_PROXY_SSL_HEADER = ("HTTP_X_FORWARDED_PROTO", "https")
USE_X_FORWARDED_HOST = True
SESSION_COOKIE_SECURE = True
CSRF_COOKIE_SECURE = True
X_FRAME_OPTIONS = "DENY"
SECURE_HSTS_SECONDS = 31536000
SECURE_HSTS_INCLUDE_SUBDOMAINS = True
