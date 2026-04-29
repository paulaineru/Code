from pathlib import Path

import environ

BASE_DIR = Path(__file__).resolve().parent.parent.parent  # backend/

env = environ.Env(
    DEBUG=(bool, False),
    MAX_UPLOAD_MB=(int, 50),
    LOW_CONFIDENCE_THRESHOLD=(float, 0.70),
    PDF_DPI=(int, 200),
)

environ.Env.read_env(BASE_DIR.parent / ".env")

SECRET_KEY = env("SECRET_KEY", default="dev-secret-change-me-in-production")

INSTALLED_APPS = [
    "django.contrib.admin",
    "django.contrib.auth",
    "django.contrib.contenttypes",
    "django.contrib.sessions",
    "django.contrib.messages",
    "django.contrib.staticfiles",
    "django.contrib.humanize",
    "accounts",
    "documents",
]

# djangosaml2 imports pysaml2/OpenSSL at module level; only activate when SAML is configured
_SAML_METADATA_FILE = env("SAML_METADATA_FILE", default="").strip()
_SAML_METADATA_URL = env("SAML_METADATA_URL", default="").strip()
_SAML_ENABLED = bool(_SAML_METADATA_FILE or _SAML_METADATA_URL)
if _SAML_ENABLED:
    INSTALLED_APPS = ["djangosaml2"] + INSTALLED_APPS

MIDDLEWARE = [
    "django.middleware.security.SecurityMiddleware",
    "django.contrib.sessions.middleware.SessionMiddleware",
]
if _SAML_ENABLED:
    MIDDLEWARE += ["djangosaml2.middleware.SamlSessionMiddleware"]
MIDDLEWARE += [
    "django.middleware.common.CommonMiddleware",
    "django.middleware.csrf.CsrfViewMiddleware",
    "django.contrib.auth.middleware.AuthenticationMiddleware",
    "accounts.middleware.SessionIdleTimeoutMiddleware",
    "django.contrib.messages.middleware.MessageMiddleware",
    "django.middleware.clickjacking.XFrameOptionsMiddleware",
]

ROOT_URLCONF = "config.urls"

TEMPLATES = [
    {
        "BACKEND": "django.template.backends.django.DjangoTemplates",
        "DIRS": [BASE_DIR / "templates"],
        "APP_DIRS": True,
        "OPTIONS": {
            "context_processors": [
                "django.template.context_processors.debug",
                "django.template.context_processors.request",
                "django.contrib.auth.context_processors.auth",
                "django.contrib.messages.context_processors.messages",
            ],
        },
    },
]

WSGI_APPLICATION = "config.wsgi.application"

DATABASES = {
    "default": {
        "ENGINE": "django.db.backends.postgresql",
        "NAME": env("POSTGRES_DB"),
        "USER": env("POSTGRES_USER"),
        "PASSWORD": env("POSTGRES_PASSWORD"),
        "HOST": env("POSTGRES_HOST"),
        "PORT": env("POSTGRES_PORT"),
    }
}

AUTH_USER_MODEL = "accounts.User"
LOGIN_URL = "/auth/login/"
LOGIN_REDIRECT_URL = "/workspace/"
LOGOUT_REDIRECT_URL = "/auth/login/"

AUTH_PASSWORD_VALIDATORS = [
    {"NAME": "django.contrib.auth.password_validation.UserAttributeSimilarityValidator"},
    {"NAME": "django.contrib.auth.password_validation.MinimumLengthValidator"},
    {"NAME": "django.contrib.auth.password_validation.CommonPasswordValidator"},
    {"NAME": "django.contrib.auth.password_validation.NumericPasswordValidator"},
]

LANGUAGE_CODE = "en-us"
TIME_ZONE = "UTC"
USE_I18N = True
USE_TZ = True

STATIC_URL = "/static/"
STATIC_ROOT = BASE_DIR / "staticfiles"
STATICFILES_DIRS = [BASE_DIR / "static"]

MEDIA_URL = "/media/"
MEDIA_ROOT = env("UPLOAD_DIR", default=str(BASE_DIR / "data" / "uploads"))

DEFAULT_AUTO_FIELD = "django.db.models.BigAutoField"

# Celery
CELERY_BROKER_URL = env("CELERY_BROKER_URL", default="redis://redis:6379/1")
CELERY_RESULT_BACKEND = env("CELERY_RESULT_BACKEND", default="redis://redis:6379/2")
CELERY_ACCEPT_CONTENT = ["json"]
CELERY_TASK_SERIALIZER = "json"
# Fail fast if broker is unreachable; prevents the upload view hanging for minutes
CELERY_BROKER_TRANSPORT_OPTIONS = {
    "socket_timeout": 2,
    "socket_connect_timeout": 2,
    "retry_on_timeout": False,
}
CELERY_BROKER_CONNECTION_RETRY_ON_STARTUP = True
CELERY_BROKER_CONNECTION_MAX_RETRIES = 1

# App-specific
MAX_UPLOAD_MB = env("MAX_UPLOAD_MB")
LOW_CONFIDENCE_THRESHOLD = env("LOW_CONFIDENCE_THRESHOLD")
PDF_DPI = env("PDF_DPI")
UPLOAD_DIR = env("UPLOAD_DIR", default="/data/uploads")
EXPORT_DIR = env("EXPORT_DIR", default="/data/exports")
AZURE_ENDPOINT = env("AZURE_ENDPOINT", default="")
AZURE_KEY = env("AZURE_KEY", default="")

SESSION_IDLE_TIMEOUT = env.int("SESSION_IDLE_TIMEOUT", default=300)
_SAML_BASE_URL = env("SAML_BASE_URL", default="https://oryx.nssfug.org")
_SAML_ENTITY_ID = env("SAML_ENTITY_ID", default=f"{_SAML_BASE_URL}/sso/metadata")
ALLOW_LOCAL_LOGIN = env.bool("ALLOW_LOCAL_LOGIN", default=not _SAML_ENABLED)

AUTHENTICATION_BACKENDS = ["django.contrib.auth.backends.ModelBackend"]
if _SAML_ENABLED:
    AUTHENTICATION_BACKENDS.append("accounts.auth_backends.OryxSaml2Backend")

# djangosaml2 behaviour
SAML_SESSION_COOKIE_NAME = "saml_session"
SAML_SESSION_COOKIE_SAMESITE = "None"
SAML_CREATE_UNKNOWN_USER = env.bool("SAML_CREATE_USER", default=False)
SAML_USE_NAME_ID_AS_USERNAME = False
SAML_DJANGO_USER_MAIN_ATTRIBUTE = "email"
SAML_DJANGO_USER_MAIN_ATTRIBUTE_LOOKUP = "__iexact"
SAML_CSP_HANDLER = "accounts.saml.saml_csp_handler"
SAML_IGNORE_LOGOUT_ERRORS = True
SAML_ASSERTION_REPLAY_CACHE_SECONDS = env.int("SAML_ASSERTION_REPLAY_CACHE_SECONDS", default=300)
SAML_DEPARTMENT_CLAIM = env("SAML_DEPARTMENT_CLAIM", default="").strip()
SAML_CREATE_UNKNOWN_DEPARTMENTS = env.bool("SAML_CREATE_UNKNOWN_DEPARTMENTS", default=False)
SAML_ATTRIBUTE_MAPPING = {
    # Azure AD sends short friendly names (not full URI claim names).
    # Department is intentionally absent — it is a FK and is synced in
    # OryxSaml2Backend._sync_department() via the SAML_DEPARTMENT_CLAIM setting.
    # Role is never synced from SAML; it is managed by local admins only.
    "emailAddress": ("email", "username"),
    "givenName":    ("first_name",),
    "surname":      ("last_name",),
}

if _SAML_ENABLED:
    try:
        import saml2
        import saml2.saml

        _metadata_config = (
            {"local": [_SAML_METADATA_FILE]}
            if _SAML_METADATA_FILE
            else {"remote": [{"url": _SAML_METADATA_URL}]}
        )

        SAML_CONFIG = {
            "xmlsec_binary": "/usr/bin/xmlsec1",
            "entityid": _SAML_ENTITY_ID,
            "service": {
                "sp": {
                    "name": "Oryx - OCR Document Intelligence",
                    "name_id_format": saml2.saml.NAMEID_FORMAT_EMAILADDRESS,
                    "endpoints": {
                        "assertion_consumer_service": [
                            (f"{_SAML_BASE_URL}/sso/acs/", saml2.BINDING_HTTP_POST),
                        ],
                        "single_logout_service": [
                            (f"{_SAML_BASE_URL}/sso/ls/", saml2.BINDING_HTTP_REDIRECT),
                            (f"{_SAML_BASE_URL}/sso/ls/post/", saml2.BINDING_HTTP_POST),
                        ],
                    },
                    "authn_requests_signed": False,
                    "want_response_signed": False,
                    "logout_requests_signed": False,
                    "want_assertions_signed": True,
                    "allow_unsolicited": False,
                    "only_use_keys_in_metadata": True,
                },
            },
            "metadata": _metadata_config,
            "debug": env.bool("DEBUG", default=False),
        }
        SAML_LOGOUT_REQUEST_PREFERRED_BINDING = saml2.BINDING_HTTP_REDIRECT
    except ImportError:
        pass

FILE_ENCRYPTION_KEY = env("FILE_ENCRYPTION_KEY", default="")

# ── MongoDB / GridFS ────────────────────────────────────────────────────────
# When MONGODB_URI is set, uploaded files and page images are stored in
# GridFS (MongoDB) instead of the local filesystem.  This eliminates the need
# for shared volume mounts and keeps all persistent data in a single, easily
# replicated store.
GRIDFS_URI = env("MONGODB_URI", default="").strip()
GRIDFS_DB  = env("MONGODB_DB",  default="oryx").strip()

# ── Django 5 STORAGES dict (replaces deprecated DEFAULT_FILE_STORAGE) ───────
# Priority: GridFS (if MongoDB configured) > AES-encrypted filesystem > plain filesystem
if GRIDFS_URI:
    _default_storage = "documents.gridfs_storage.GridFSStorage"
elif FILE_ENCRYPTION_KEY:
    _default_storage = "documents.storage.EncryptedFileSystemStorage"
else:
    _default_storage = "django.core.files.storage.FileSystemStorage"

STORAGES = {
    "default": {
        "BACKEND": _default_storage,
    },
    "staticfiles": {
        "BACKEND": "django.contrib.staticfiles.storage.StaticFilesStorage",
    },
}
