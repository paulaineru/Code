from django.contrib import admin
from django.http import JsonResponse
from django.urls import include, path
from django.shortcuts import redirect
from django.conf import settings
from django.conf.urls.static import static
from accounts.views import OryxLoginView, OryxLogoutView, session_activity_view, session_status_view


def healthcheck(request):
    return JsonResponse({"status": "ok"})


urlpatterns = [
    path("django-admin/", admin.site.urls),
    path("health/", healthcheck, name="health"),
    path("", lambda request: redirect("document_list"), name="root"),

    # Auth
    path("auth/login/", OryxLoginView.as_view(), name="login"),
    path("auth/logout/", OryxLogoutView.as_view(), name="logout"),
    path("auth/session/activity/", session_activity_view, name="session_activity"),
    path("auth/session/status/", session_status_view, name="session_status"),

    # App
    path("", include("documents.urls")),
]

# Only register SAML URLs when SAML is configured — avoids importing pysaml2/OpenSSL
# on environments where SAML_METADATA_URL is not set (e.g. Windows dev, CI)
if getattr(settings, "SAML_CONFIG", None):
    urlpatterns += [path("sso/", include("djangosaml2.urls"))]

if settings.DEBUG:
    urlpatterns += static(settings.MEDIA_URL, document_root=settings.MEDIA_ROOT)
