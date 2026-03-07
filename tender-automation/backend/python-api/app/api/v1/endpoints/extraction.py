"""AI Tender Extraction API endpoints.

Provides endpoints to upload tender PDFs, extract structured data using AI,
preview the extraction results, and apply them to update tender records.
"""
import os
import uuid
import logging
from typing import List, Optional

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


@router.post("/extract-preview")
async def extract_preview(
    files: List[UploadFile] = File(...),
    model: Optional[str] = Form(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Extract tender data from PDFs WITHOUT creating a tender first.

    Used by the tender creation form to auto-fill fields from uploaded documents.
    Returns extracted data that can be reviewed before creating the tender.
    """
    if not files:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="At least one PDF file is required",
        )

    for f in files:
        if not f.filename or not f.filename.lower().endswith(".pdf"):
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Only PDF files are supported. '{f.filename}' is not a PDF.",
            )

    upload_dir = os.path.join(settings.upload_dir, "extraction_temp")
    os.makedirs(upload_dir, exist_ok=True)

    saved_paths: list[str] = []

    try:
        for f in files:
            temp_filename = f"{uuid.uuid4()}_{f.filename}"
            file_path = os.path.join(upload_dir, temp_filename)
            contents = await f.read()
            with open(file_path, "wb") as out:
                out.write(contents)
            saved_paths.append(file_path)

        extraction_model = model or "claude-sonnet-4-6"
        extraction_result = await extract_tender_data(
            db=db,
            tender_id=None,
            file_paths=saved_paths,
            current_user_id=current_user.user_id,
            model=extraction_model,
            tenant_id_override=current_user.tenant_id,
        )

        # Return just the extracted data for form auto-fill (don't apply to any tender)
        return {
            "extracted_data": extraction_result.get("extracted_data", {}),
            "model_used": extraction_result.get("model_used"),
            "tokens_used": extraction_result.get("tokens_used"),
            "extraction_mode": extraction_result.get("extraction_mode"),
            "files_processed": extraction_result.get("files_processed"),
        }

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_422_UNPROCESSABLE_ENTITY,
            detail=str(e),
        )
    except Exception as e:
        logger.error(f"Preview extraction failed: {e}", exc_info=True)
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Extraction failed: {str(e)}",
        )
    finally:
        for path in saved_paths:
            if os.path.exists(path):
                try:
                    os.remove(path)
                except OSError:
                    pass


@router.post("/{tender_id}/extract")
async def extract_from_pdf(
    tender_id: str,
    files: List[UploadFile] = File(...),
    model: Optional[str] = Form(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Upload one or more tender PDFs, extract structured data using AI,
    and automatically apply all extracted information to the tender.

    Accepts multiple PDF files (NIT, technical specs, BOQ, corrigendum, etc.).
    All documents are sent to Claude in a single call so the AI sees the
    complete tender context across all files.

    After extraction, the data is immediately applied — tender fields are
    updated, EMD/fee/criteria/OEM/checklist records are created automatically.
    """
    if not files:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="At least one PDF file is required",
        )

    # Validate all files are PDFs
    for f in files:
        if not f.filename or not f.filename.lower().endswith(".pdf"):
            raise HTTPException(
                status_code=status.HTTP_400_BAD_REQUEST,
                detail=f"Only PDF files are supported. '{f.filename}' is not a PDF.",
            )

    # Save uploaded files temporarily
    upload_dir = os.path.join(settings.upload_dir, "extraction_temp")
    os.makedirs(upload_dir, exist_ok=True)

    saved_paths: list[str] = []

    try:
        for f in files:
            temp_filename = f"{uuid.uuid4()}_{f.filename}"
            file_path = os.path.join(upload_dir, temp_filename)
            contents = await f.read()
            with open(file_path, "wb") as out:
                out.write(contents)
            saved_paths.append(file_path)

        # Step 1: Extract data using AI
        extraction_model = model or "claude-sonnet-4-6"
        extraction_result = await extract_tender_data(
            db=db,
            tender_id=uuid.UUID(tender_id),
            file_paths=saved_paths,
            current_user_id=current_user.user_id,
            model=extraction_model,
        )

        # Step 2: Auto-apply extracted data to tender
        apply_result = await apply_extraction(
            db=db,
            tender_id=uuid.UUID(tender_id),
            extracted_data=extraction_result["extracted_data"],
            current_user_id=current_user.user_id,
            extraction_meta={
                "model_used": extraction_result.get("model_used"),
                "tokens_used": extraction_result.get("tokens_used"),
                "extraction_mode": extraction_result.get("extraction_mode"),
                "files_processed": extraction_result.get("files_processed"),
            },
        )

        # Return combined response
        return {
            **extraction_result,
            "applied": True,
            "applied_summary": apply_result,
        }

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
        # Clean up all temp files
        for path in saved_paths:
            if os.path.exists(path):
                try:
                    os.remove(path)
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
