from django.shortcuts import redirect
from django.contrib.auth.decorators import login_required
from django.http import JsonResponse


@login_required
def dashboard_redirect(request):
    return redirect('upload')

def healthcheck(request):
    return JsonResponse({
        "status":"ok"
    })
