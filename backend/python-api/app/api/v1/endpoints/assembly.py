"""
Document assembly endpoints
"""
from fastapi import APIRouter, Depends, HTTPException
from sqlalchemy.orm import Session
from sqlalchemy import select
import uuid

from app.core.database import get_db
from app.core.security import get_current_user, TokenData
from app.models.tender import Document
from app.schemas.tender import AssemblyRequest, AssemblyResponse

router = APIRouter()


@router.post("/merge", response_model=AssemblyResponse)
def merge_documents(
    request: AssemblyRequest,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Merge multiple documents into a single PDF or ZIP"""

    # Validate documents exist and belong to tenant
    query = select(Document).where(
        Document.id.in_(request.document_ids),
        Document.tenant_id == uuid.UUID(current_user.tenant_id)
    )

    result = db.execute(query)
    documents = result.scalars().all()

    if len(documents) != len(request.document_ids):
        raise HTTPException(
            status_code=404,
            detail="Some documents not found or access denied"
        )

    # For MVP, return placeholder response
    # In production, this would:
    # 1. Merge PDFs using PyPDF2
    # 2. Add cover page with reportlab
    # 3. Package as ZIP if requested
    # 4. Save to disk and return file path

    return AssemblyResponse(
        file_path="placeholder_merged.pdf",
        file_size=sum(doc.file_size for doc in documents),
        document_count=len(documents)
    )


@router.post("/export/{tender_id}", response_model=AssemblyResponse)
def export_tender_package(
    tender_id: uuid.UUID,
    current_user: TokenData = Depends(get_current_user),
    db: Session = Depends(get_db)
):
    """Export all documents for a tender as a ZIP package"""

    # This would:
    # 1. Get all documents associated with tender
    # 2. Create ZIP file
    # 3. Include cover page with tender details
    # 4. Return download link

    return AssemblyResponse(
        file_path=f"tender_{tender_id}_package.zip",
        file_size=0,
        document_count=0
    )
