"""AI Tender Extraction API endpoints.

Provides endpoints to upload tender PDFs, extract structured data using AI,
preview the extraction results, and apply them to update tender records.
"""
import os
import uuid
import logging
from typing import Optional

from fastapi import APIRouter, Depends, HTTPException, UploadFile, File, Form, status
from sqlalchemy.ext.asyncio import AsyncSession
from pydantic import BaseModel

from app.core.db import get_db
from app.core.config import settings
from app.core.security import get_current_user, TokenData
from app.services.extraction_service import extract_tender_data, apply_extraction

logger = logging.getLogger(__name__)

router = APIRouter()


class ApplyExtractionRequest(BaseModel):
    """Request body for applying extracted data to a tender."""
    title: Optional[str] = None
    reference_number: Optional[str] = None
    issuing_authority: Optional[str] = None
    deadline: Optional[str] = None
    estimated_value: Optional[float] = None
    eligibility_criteria: Optional[list] = None
    technical_requirements: Optional[list] = None
    emd: Optional[dict] = None
    tender_fees: Optional[list] = None
    evaluation_criteria: Optional[list] = None
    document_checklist: Optional[list] = None
    important_dates: Optional[dict] = None
    contact_info: Optional[dict] = None
    special_conditions: Optional[list] = None
    oem_requirements: Optional[list] = None


@router.post("/{tender_id}/extract")
async def extract_from_pdf(
    tender_id: str,
    file: UploadFile = File(...),
    model: Optional[str] = Form(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Upload a tender PDF and extract structured data using AI.

    This endpoint accepts a PDF file, extracts text from it, and uses
    Claude AI to identify and structure all tender information including
    eligibility criteria, technical requirements, EMD details, fees,
    evaluation criteria, OEM requirements, and important dates.

    The extracted data is returned as a preview - it is NOT automatically
    applied to the tender. Use the apply-extraction endpoint to confirm.
    """
    # Validate file type
    if not file.filename or not file.filename.lower().endswith(".pdf"):
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Only PDF files are supported for extraction",
        )

    # Save uploaded file temporarily
    upload_dir = os.path.join(settings.upload_dir, "extraction_temp")
    os.makedirs(upload_dir, exist_ok=True)

    temp_filename = f"{uuid.uuid4()}_{file.filename}"
    file_path = os.path.join(upload_dir, temp_filename)

    try:
        # Save file to disk
        contents = await file.read()
        with open(file_path, "wb") as f:
            f.write(contents)

        # Extract data using AI
        extraction_model = model or "claude-sonnet-4-5-20250514"
        result = await extract_tender_data(
            db=db,
            tender_id=uuid.UUID(tender_id),
            file_path=file_path,
            current_user_id=current_user.user_id,
            model=extraction_model,
        )

        return result

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e),
        )
    except Exception as e:
        logger.error(f"Extraction failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Extraction failed: {str(e)}",
        )
    finally:
        # Clean up temp file
        if os.path.exists(file_path):
            try:
                os.remove(file_path)
            except OSError:
                pass


@router.post("/{tender_id}/apply-extraction")
async def apply_extraction_endpoint(
    tender_id: str,
    data: ApplyExtractionRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Apply previously extracted data to update a tender.

    This endpoint takes the extracted data (potentially modified by the user
    after preview) and applies it to:
    - Update tender fields (title, reference_number, deadline, etc.)
    - Store full extraction in the requirements JSON column
    - Create EMD records
    - Create tender fee records

    The user can modify the extracted data before applying, for example
    to select only specific OEM requirements or adjust values.
    """
    try:
        # Convert request to dict, excluding None values
        extracted_data = data.model_dump(exclude_none=True)

        result = await apply_extraction(
            db=db,
            tender_id=uuid.UUID(tender_id),
            extracted_data=extracted_data,
            current_user_id=current_user.user_id,
        )

        return result

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e),
        )
    except Exception as e:
        logger.error(f"Apply extraction failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to apply extraction: {str(e)}",
        )
