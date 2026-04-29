from django.contrib import admin
from django.contrib.auth.admin import UserAdmin
from .models import Department, User


@admin.register(Department)
class DepartmentAdmin(admin.ModelAdmin):
    list_display = ("name", "confidence_threshold", "updated_at")
    search_fields = ("name",)


@admin.register(User)
class CustomUserAdmin(UserAdmin):
    list_display = ("username", "email", "full_name", "role", "department", "supervisor", "is_active", "date_joined")
    list_filter = ("role", "department", "is_active", "is_staff")
    search_fields = ("username", "email", "full_name")
    fieldsets = UserAdmin.fieldsets + (
        ("Profile", {"fields": ("full_name", "role", "department", "supervisor", "azure_oid")}),
    )
    add_fieldsets = UserAdmin.add_fieldsets + (
        ("Profile", {"fields": ("email", "full_name", "role", "department", "supervisor")}),
    )
