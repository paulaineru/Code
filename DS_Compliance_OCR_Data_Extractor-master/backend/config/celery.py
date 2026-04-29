import os
from celery import Celery

os.environ.setdefault("DJANGO_SETTINGS_MODULE", "config.settings.development")

app = Celery("config")
app.config_from_object("django.conf:settings", namespace="CELERY")
app.autodiscover_tasks(["tasks"])  # discovers tasks/tasks.py (if present)
app.conf.include = ["tasks.ocr_task", "tasks.review_task"]  # explicitly register task modules
