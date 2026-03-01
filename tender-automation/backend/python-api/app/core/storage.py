"""File storage utilities."""
import os
import uuid
import shutil
from pathlib import Path
from typing import Tuple, Optional
from fastapi import UploadFile, HTTPException, status

from app.core.config import settings


class FileStorageService:
    """Handle file storage operations."""

    def __init__(self):
        self.upload_dir = Path(settings.upload_dir)
        self.max_file_size = settings.max_file_size
        self.allowed_extensions = settings.allowed_extensions

        # Create upload directory if it doesn't exist
        self.upload_dir.mkdir(parents=True, exist_ok=True)

    def validate_file(self, file: UploadFile) -> None:
        """
        Validate uploaded file.

        Args:
            file: UploadFile from FastAPI

        Raises:
            HTTPException: If validation fails
        """
        # Check if file exists
        if not file:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail="No file provided"
            )

        # Check file extension
        file_ext = Path(file.filename).suffix.lower()
        if file_ext not in self.allowed_extensions:
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"File type {file_ext} not allowed. Allowed types: {', '.join(self.allowed_extensions)}"
            )

        # Check file size (if content_length is available)
        if hasattr(file, 'size') and file.size:
            if file.size > self.max_file_size:
                max_mb = self.max_file_size / (1024 * 1024)
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=f"File size exceeds maximum allowed size of {max_mb}MB"
                )

    async def save_file(
        self,
        file: UploadFile,
        subfolder: str = "documents"
    ) -> Tuple[str, int]:
        """
        Save uploaded file to storage.

        Args:
            file: UploadFile from FastAPI
            subfolder: Subfolder within upload directory

        Returns:
            Tuple of (file_path, file_size)

        Raises:
            HTTPException: If file operations fail
        """
        # Validate file
        self.validate_file(file)

        # Create subfolder
        folder_path = self.upload_dir / subfolder
        folder_path.mkdir(parents=True, exist_ok=True)

        # Generate unique filename
        file_ext = Path(file.filename).suffix.lower()
        unique_filename = f"{uuid.uuid4()}{file_ext}"
        file_path = folder_path / unique_filename

        # Save file
        try:
            with file_path.open("wb") as buffer:
                shutil.copyfileobj(file.file, buffer)

            # Get actual file size
            file_size = file_path.stat().st_size

            # Check if file size exceeds limit
            if file_size > self.max_file_size:
                file_path.unlink()  # Delete file
                max_mb = self.max_file_size / (1024 * 1024)
                raise HTTPException(
                    status_code=status.HTTP_400_BAD_REQUEST,
                    detail=f"File size ({file_size / (1024*1024):.2f}MB) exceeds maximum allowed size of {max_mb}MB"
                )

            # Return relative path from upload_dir
            relative_path = str(file_path.relative_to(self.upload_dir))
            return relative_path, file_size

        except HTTPException:
            raise
        except Exception as e:
            # Clean up file if it was created
            if file_path.exists():
                file_path.unlink()
            raise HTTPException(
                status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
                detail=f"Failed to save file: {str(e)}"
            )

    def delete_file(self, file_path: str) -> bool:
        """
        Delete file from storage.

        Args:
            file_path: Relative path to file

        Returns:
            True if deleted successfully, False if file doesn't exist
        """
        try:
            full_path = self.upload_dir / file_path
            if full_path.exists():
                full_path.unlink()
                return True
            return False
        except Exception:
            # Log error but don't raise exception
            return False

    def get_file_path(self, relative_path: str) -> Optional[Path]:
        """
        Get absolute file path.

        Args:
            relative_path: Relative path from upload directory

        Returns:
            Absolute Path if file exists, None otherwise
        """
        full_path = self.upload_dir / relative_path
        return full_path if full_path.exists() else None

    def get_mime_type(self, filename: str) -> str:
        """
        Get MIME type from filename.

        Args:
            filename: File name

        Returns:
            MIME type string
        """
        import mimetypes

        mime_type, _ = mimetypes.guess_type(filename)
        return mime_type or "application/octet-stream"


# Global file storage instance
file_storage = FileStorageService()
