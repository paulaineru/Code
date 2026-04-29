"""
GridFS storage backend for Django.

Stores uploaded documents and page images in MongoDB GridFS instead of the
local filesystem.  The entire application continues to use Django's standard
FileField / ImageField API (`.read()`, `.open()`, `.save()`, `.delete()`,
`.size`) — nothing else in the codebase needs to change.

Configuration (set in settings.py):
    STORAGES = {
        "default": {"BACKEND": "documents.gridfs_storage.GridFSStorage"},
        ...
    }
    GRIDFS_URI = "mongodb://mongodb:27017/"   # or from env MONGODB_URI
    GRIDFS_DB  = "oryx"                       # or from env MONGODB_DB

Why GridFS vs plain filesystem?
  - Files live in MongoDB, so they replicate with your data.
  - No separate volume-mount wiring for horizontal scale-out.
  - Single backup target for everything (DB + files).
  - Works transparently in Docker / Kubernetes with no extra volume config.
"""
from __future__ import annotations

import io
import threading
from typing import IO

from django.conf import settings
from django.core.files.storage import Storage


# ── Thread-local MongoClient cache ──────────────────────────────────────────
# MongoClient is NOT fork-safe but IS thread-safe.  Storing one per thread
# (via threading.local) means each gunicorn / Celery thread gets its own
# connection pool, and forked workers start fresh without sharing file
# descriptors.

_local = threading.local()


def _get_gridfs():
    """
    Return the thread-local GridFS instance, creating the MongoClient on
    first access.  Raises ImportError if pymongo is not installed.
    """
    try:
        from pymongo import MongoClient
        import gridfs
    except ImportError as exc:
        raise ImportError(
            "pymongo is required for GridFSStorage.  "
            "Add it to pyproject.toml: pymongo>=4.6"
        ) from exc

    uri = getattr(settings, "GRIDFS_URI", "mongodb://mongodb:27017/")
    db_name = getattr(settings, "GRIDFS_DB", "oryx")

    # 5-second timeout so mis-configured deployments fail fast instead of
    # blocking gunicorn workers for the default 30 s.
    client = MongoClient(uri, serverSelectionTimeoutMS=5_000)
    return gridfs.GridFS(client[db_name])


def _fs():
    """Return the GridFS instance for this thread, lazy-initialising once."""
    if not getattr(_local, "gridfs", None):
        _local.gridfs = _get_gridfs()
    return _local.gridfs


# ── Storage backend ──────────────────────────────────────────────────────────

class GridFSStorage(Storage):
    """
    Persists files in MongoDB GridFS.

    File identity is the *logical name* (e.g. ``page_images/page_<uuid>_1.png``).
    Old revisions are deleted on each save so storage does not grow without
    bound.  Concurrent writes to the same name are safe — the last write wins.
    """

    # ── Internal Django storage hooks ────────────────────────────────────────

    def _save(self, name: str, content) -> str:
        """Write *content* to GridFS under *name*; return the stored name."""
        fs = _fs()
        data: bytes = content.read() if hasattr(content, "read") else bytes(content)

        # Replace any existing file with this logical name so the GridFS
        # collection doesn't accumulate orphaned chunks over time.
        for old in list(fs.find({"filename": name})):
            try:
                fs.delete(old._id)
            except Exception:
                pass  # best-effort cleanup; the put() below is authoritative

        fs.put(data, filename=name)
        return name

    def _open(self, name: str, mode: str = "rb") -> IO[bytes]:
        """
        Return a seekable, in-memory file-like object for *name*.

        All bytes are fetched from GridFS in a single round-trip and wrapped
        in BytesIO so callers can seek freely (e.g. Pillow, pdf readers).
        A ``gridfs.errors.NoFile`` exception propagates as-is; the caller
        (typically a Django view) should catch it and return 404.
        """
        grid_out = _fs().get_last_version(name)
        return io.BytesIO(grid_out.read())

    # ── Public Storage API ────────────────────────────────────────────────────

    def exists(self, name: str) -> bool:
        return _fs().exists(name)

    def delete(self, name: str) -> None:
        """Delete all GridFS chunks for *name* (all revisions)."""
        fs = _fs()
        for f in list(fs.find({"filename": name})):
            try:
                fs.delete(f._id)
            except Exception:
                pass

    def size(self, name: str) -> int:
        """Return the stored byte-length of *name*, or 0 if not found."""
        try:
            return _fs().get_last_version(name).length
        except Exception:
            return 0

    def url(self, name: str) -> str:
        """
        Page images are served through ``page_image_view``; raw uploads are
        never served directly.  If a template or the admin ever calls this,
        raise a clear error rather than silently returning a broken URL.
        """
        raise NotImplementedError(
            "GridFSStorage files are served through Django views, not raw "
            "media URLs.  Use {% url 'page_image' ... %} in templates."
        )

    def path(self, name: str) -> str:
        """GridFS has no local filesystem path."""
        raise NotImplementedError(
            "GridFSStorage has no filesystem path.  "
            "Use storage.open(name) to read file contents."
        )
