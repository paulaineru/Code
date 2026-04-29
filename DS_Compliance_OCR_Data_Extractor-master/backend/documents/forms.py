from django import forms
from django.conf import settings
from .models import Job, SystemConfig


ALLOWED_EXTENSIONS = {".pdf", ".png", ".jpg", ".jpeg", ".tiff", ".tif", ".bmp", ".webp"}


class MultipleFileInput(forms.ClearableFileInput):
    allow_multiple_selected = True


class MultipleFileField(forms.FileField):
    widget = MultipleFileInput

    def clean(self, data, initial=None):
        clean_one = super().clean
        if not data:
            return []
        if not isinstance(data, (list, tuple)):
            data = [data]
        return [clean_one(item, initial) for item in data]


class UploadForm(forms.Form):
    document_type = forms.ChoiceField(
        choices=Job.DocumentType.choices,
        initial=Job.DocumentType.GENERIC,
    )
    files = MultipleFileField()

    def clean_files(self):
        files = self.cleaned_data["files"]
        if not files:
            raise forms.ValidationError("Select at least one file to upload.")

        validated = []
        errors = []
        max_bytes = settings.MAX_UPLOAD_MB * 1024 * 1024
        for f in files:
            ext = "." + f.name.rsplit(".", 1)[-1].lower() if "." in f.name else ""
            if ext not in ALLOWED_EXTENSIONS:
                errors.append(
                    forms.ValidationError(
                        f"{f.name}: unsupported file type '{ext}'. Allowed: {', '.join(sorted(ALLOWED_EXTENSIONS))}"
                    )
                )
                continue
            if f.size > max_bytes:
                errors.append(
                    forms.ValidationError(
                        f"{f.name}: file too large ({f.size / 1024 / 1024:.1f} MB). Maximum is {settings.MAX_UPLOAD_MB} MB."
                    )
                )
                continue
            validated.append(f)

        if errors:
            raise forms.ValidationError(errors)
        return validated


class SystemConfigForm(forms.Form):
    """Flat form for SystemConfig — manually maps to model to avoid type mismatch on confidence."""

    confidence_threshold = forms.IntegerField(min_value=50, max_value=95)
    default_document_type = forms.ChoiceField(choices=Job.DocumentType.choices)
    pdf_dpi = forms.IntegerField(min_value=72, max_value=600)
    azure_endpoint = forms.CharField(max_length=500, required=False)
    azure_key = forms.CharField(max_length=500, required=False)

    def save(self):
        config = SystemConfig.get()
        config.confidence_threshold = self.cleaned_data["confidence_threshold"] / 100.0
        config.default_document_type = self.cleaned_data["default_document_type"]
        config.pdf_dpi = self.cleaned_data["pdf_dpi"]
        config.azure_endpoint = self.cleaned_data["azure_endpoint"]
        config.azure_key = self.cleaned_data["azure_key"]
        config.save()
        return config

    @property
    def changed_data(self):
        # All submitted fields are "changed" for audit purposes
        return list(self.cleaned_data.keys()) if self.is_valid() else []


class DepartmentSettingsForm(forms.Form):
    confidence_threshold = forms.IntegerField(min_value=50, max_value=95)

    def __init__(self, *args, department=None, **kwargs):
        super().__init__(*args, **kwargs)
        self.department = department

    def save(self):
        if self.department is None:
            raise ValueError("DepartmentSettingsForm requires a department instance.")

        self.department.confidence_threshold = self.cleaned_data["confidence_threshold"] / 100.0
        self.department.full_clean()
        self.department.save()
        return self.department

    @property
    def changed_data(self):
        return list(self.cleaned_data.keys()) if self.is_valid() else []
