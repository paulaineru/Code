FROM python:3.12-slim

ENV PYTHONDONTWRITEBYTECODE=1
ENV PYTHONUNBUFFERED=1
ENV DJANGO_SETTINGS_MODULE=config.settings.development

# Put the venv outside /app so the bind-mount (./backend:/app) doesn't hide it
ENV UV_PROJECT_ENVIRONMENT=/opt/venv

RUN apt-get update && apt-get install -y --no-install-recommends \
    libgl1 libglib2.0-0 libpq-dev curl \
    # Required by pysaml2 (djangosaml2) for XML signature operations
    xmlsec1 libxmlsec1-dev libxml2-dev pkg-config && \
    rm -rf /var/lib/apt/lists/*

# Install uv from the official image
COPY --from=ghcr.io/astral-sh/uv:latest /uv /uvx /bin/

WORKDIR /app

# Copy dependency manifests and install — use --frozen so the lockfile is authoritative
COPY pyproject.toml uv.lock ./
RUN uv sync --no-dev --frozen

# Copy source code
COPY . /app/

RUN mkdir -p /data/uploads /data/exports

EXPOSE 8000

CMD ["uv", "run", "python", "manage.py", "runserver", "0.0.0.0:8343"]
