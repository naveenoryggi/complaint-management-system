"""Symmetric encryption utility for storing API keys.

Uses Fernet (AES-128-CBC) with key derived from the existing jwt_secret_key
so no additional secrets are needed.
"""
import base64
import hashlib

from cryptography.fernet import Fernet

from app.core.config import settings


def _derive_key() -> bytes:
    """Derive a Fernet-compatible 32-byte key from jwt_secret_key."""
    digest = hashlib.sha256(settings.jwt_secret_key.encode()).digest()
    return base64.urlsafe_b64encode(digest)


_fernet = Fernet(_derive_key())


def encrypt_value(plaintext: str) -> str:
    """Encrypt a plaintext string and return a URL-safe base64 token."""
    return _fernet.encrypt(plaintext.encode()).decode()


def decrypt_value(ciphertext: str) -> str:
    """Decrypt a Fernet token back to the original plaintext string."""
    return _fernet.decrypt(ciphertext.encode()).decode()
