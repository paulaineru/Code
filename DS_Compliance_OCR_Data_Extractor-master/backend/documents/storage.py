"""
Encrypted file storage backend.

Every file is encrypted with AES-256-GCM (via cryptography.hazmat) before
being written to disk. The encryption key is read from the FILE_ENCRYPTION_KEY
environment variable (a URL-safe base64-encoded 32-byte key).

Generate a key once and store it in .env:
    python -c "from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())"

Files on disk are:
    <12-byte nonce> + <ciphertext> + <16-byte GCM tag>

This means even a sysadmin with raw disk access cannot read document contents
without the key.
"""
import os
import io

from django.conf import settings
from django.core.files.base import ContentFile
from django.core.files.storage import FileSystemStorage


def _get_cipher():
    """Return an AES-256-GCM cipher object using the configured key."""
    import base64
    from cryptography.hazmat.primitives.ciphers.aead import AESGCM

    raw_key = getattr(settings, "FILE_ENCRYPTION_KEY", "")
    if not raw_key:
        raise ValueError(
            "FILE_ENCRYPTION_KEY is not set. "
            "Generate one with: python -c \"from cryptography.fernet import Fernet; print(Fernet.generate_key().decode())\""
        )
    # Fernet keys are URL-safe base64-encoded 32-byte keys
    key_bytes = base64.urlsafe_b64decode(raw_key.encode())[:32]
    return AESGCM(key_bytes)


class EncryptedFileSystemStorage(FileSystemStorage):
    """
    Drop-in replacement for FileSystemStorage that encrypts files at rest.
    Transparent to the rest of the application — read/write work as normal.
    """

    def _save(self, name, content):
        """Encrypt content before handing off to the parent _save."""
        from cryptography.hazmat.primitives.ciphers.aead import AESGCM

        plaintext = content.read()
        cipher = _get_cipher()
        nonce = os.urandom(12)              # 96-bit nonce for GCM
        ciphertext = cipher.encrypt(nonce, plaintext, None)
        encrypted = nonce + ciphertext      # nonce prepended for decryption

        return super()._save(name, ContentFile(encrypted))

    def open(self, name, mode="rb"):
        """Decrypt on open so callers receive the original plaintext."""
        encrypted_file = super().open(name, mode)
        encrypted_bytes = encrypted_file.read()
        encrypted_file.close()

        if len(encrypted_bytes) < 28:       # 12-byte nonce + at minimum 16-byte tag
            raise ValueError(f"Encrypted file {name!r} is too short — may be corrupted.")

        nonce = encrypted_bytes[:12]
        ciphertext = encrypted_bytes[12:]
        cipher = _get_cipher()
        plaintext = cipher.decrypt(nonce, ciphertext, None)

        # Return a file-like object the rest of Django can use
        f = ContentFile(plaintext)
        f.name = name
        return f

    def size(self, name):
        """
        Return the apparent (decrypted) size.
        GCM overhead: 12 bytes nonce + 16 bytes tag = 28 bytes.
        """
        raw = super().size(name)
        return max(0, raw - 28) if raw is not None else None
