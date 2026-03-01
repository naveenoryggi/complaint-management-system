"""Document service - business logic for document management."""
from typing import List, Optional, Tuple
from uuid import UUID
from sqlalchemy import select, func, or_, and_
from sqlalchemy.ext.asyncio import AsyncSession
from fastapi import UploadFile, HTTPException, status

from app.models.document import Document
from app.schemas.document import (
    DocumentCreate,
    DocumentUpdate,
    DocumentResponse,
    DocumentSearchRequest,
    DocumentListResponse,
)
from app.core.storage import file_storage
from app.core.security import TokenData


class DocumentService:
    """Service for document operations."""

    async def upload_document(
        self,
        db: AsyncSession,
        file: UploadFile,
        document_data: DocumentCreate,
        current_user: TokenData,
    ) -> Document:
        """
        Upload a new document.

        Args:
            db: Database session
            file: Uploaded file
            document_data: Document metadata
            current_user: Current authenticated user

        Returns:
            Created document
        """
        # Save file to storage
        file_path, file_size = await file_storage.save_file(file, subfolder="documents")

        # Get MIME type
        mime_type = file_storage.get_mime_type(file.filename)

        # Create document record
        document = Document(
            tenant_id=UUID(current_user.tenant_id),
            created_by=UUID(current_user.user_id),
            name=document_data.name,
            description=document_data.description,
            file_path=file_path,
            file_size=file_size,
            mime_type=mime_type,
            document_type=document_data.document_type,
            tags=document_data.tags,
            is_template=document_data.is_template,
        )

        db.add(document)
        await db.commit()
        await db.refresh(document)

        return document

    async def get_document(
        self,
        db: AsyncSession,
        document_id: UUID,
        current_user: TokenData,
    ) -> Optional[Document]:
        """
        Get document by ID (enforces tenant isolation).

        Args:
            db: Database session
            document_id: Document ID
            current_user: Current authenticated user

        Returns:
            Document if found and user has access, None otherwise
        """
        result = await db.execute(
            select(Document).where(
                and_(
                    Document.id == document_id,
                    Document.tenant_id == UUID(current_user.tenant_id)
                )
            )
        )
        return result.scalar_one_or_none()

    async def list_documents(
        self,
        db: AsyncSession,
        current_user: TokenData,
        page: int = 1,
        page_size: int = 20,
    ) -> DocumentListResponse:
        """
        List all documents for current tenant (paginated).

        Args:
            db: Database session
            current_user: Current authenticated user
            page: Page number
            page_size: Items per page

        Returns:
            Paginated list of documents
        """
        # Get total count
        count_result = await db.execute(
            select(func.count(Document.id)).where(
                Document.tenant_id == UUID(current_user.tenant_id)
            )
        )
        total = count_result.scalar_one()

        # Get paginated documents
        offset = (page - 1) * page_size
        result = await db.execute(
            select(Document)
            .where(Document.tenant_id == UUID(current_user.tenant_id))
            .order_by(Document.created_at.desc())
            .limit(page_size)
            .offset(offset)
        )
        documents = result.scalars().all()

        # Calculate total pages
        total_pages = (total + page_size - 1) // page_size

        return DocumentListResponse(
            items=[DocumentResponse.model_validate(doc) for doc in documents],
            total=total,
            page=page,
            page_size=page_size,
            total_pages=total_pages,
        )

    async def search_documents(
        self,
        db: AsyncSession,
        search_request: DocumentSearchRequest,
        current_user: TokenData,
    ) -> DocumentListResponse:
        """
        Search documents with filters.

        Args:
            db: Database session
            search_request: Search parameters
            current_user: Current authenticated user

        Returns:
            Paginated search results
        """
        # Base query - tenant isolation
        query = select(Document).where(
            Document.tenant_id == UUID(current_user.tenant_id)
        )

        # Apply filters
        filters = []

        if search_request.query:
            # Search in name and description
            search_term = f"%{search_request.query}%"
            filters.append(
                or_(
                    Document.name.ilike(search_term),
                    Document.description.ilike(search_term),
                )
            )

        if search_request.document_type:
            filters.append(Document.document_type == search_request.document_type)

        if search_request.tags:
            # Match any of the provided tags
            filters.append(Document.tags.overlap(search_request.tags))

        if search_request.is_template is not None:
            filters.append(Document.is_template == search_request.is_template)

        # Apply all filters
        if filters:
            query = query.where(and_(*filters))

        # Get total count
        count_query = select(func.count()).select_from(query.subquery())
        count_result = await db.execute(count_query)
        total = count_result.scalar_one()

        # Apply pagination
        offset = (search_request.page - 1) * search_request.page_size
        query = query.order_by(Document.created_at.desc()).limit(search_request.page_size).offset(offset)

        # Execute query
        result = await db.execute(query)
        documents = result.scalars().all()

        # Calculate total pages
        total_pages = (total + search_request.page_size - 1) // search_request.page_size

        return DocumentListResponse(
            items=[DocumentResponse.model_validate(doc) for doc in documents],
            total=total,
            page=search_request.page,
            page_size=search_request.page_size,
            total_pages=total_pages,
        )

    async def update_document(
        self,
        db: AsyncSession,
        document_id: UUID,
        update_data: DocumentUpdate,
        current_user: TokenData,
    ) -> Optional[Document]:
        """
        Update document metadata.

        Args:
            db: Database session
            document_id: Document ID
            update_data: Fields to update
            current_user: Current authenticated user

        Returns:
            Updated document if found, None otherwise
        """
        # Get document (with tenant check)
        document = await self.get_document(db, document_id, current_user)
        if not document:
            return None

        # Update fields
        update_dict = update_data.model_dump(exclude_unset=True)
        for field, value in update_dict.items():
            setattr(document, field, value)

        await db.commit()
        await db.refresh(document)

        return document

    async def delete_document(
        self,
        db: AsyncSession,
        document_id: UUID,
        current_user: TokenData,
    ) -> bool:
        """
        Delete document (both record and file).

        Args:
            db: Database session
            document_id: Document ID
            current_user: Current authenticated user

        Returns:
            True if deleted, False if not found
        """
        # Get document (with tenant check)
        document = await self.get_document(db, document_id, current_user)
        if not document:
            return False

        # Delete file from storage
        file_storage.delete_file(document.file_path)

        # Delete database record
        await db.delete(document)
        await db.commit()

        return True


# Global service instance
document_service = DocumentService()
