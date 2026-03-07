"""Declaration generation API endpoints with standard + AI-customized modes."""
from typing import List, Optional
from uuid import UUID
from pydantic import BaseModel
from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.responses import StreamingResponse
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.company import CompanyProfile
from app.models.tender import Tender
from app.services.declaration_service import declaration_service

router = APIRouter()


# ---------------------------------------------------------------------------
# Request Schemas
# ---------------------------------------------------------------------------


class SingleDeclarationRequest(BaseModel):
    declaration_type: str
    tender_id: Optional[UUID] = None
    signatory_name: Optional[str] = None
    designation: Optional[str] = None
    mode: str = "standard"  # "standard" or "ai"
    tender_specific_points: Optional[List[str]] = None
    bidder_role: str = "bidder"  # "bidder" or "oem"


class BulkDeclarationRequest(BaseModel):
    declaration_types: List[str]
    tender_id: Optional[UUID] = None
    signatory_name: Optional[str] = None
    designation: Optional[str] = None
    ai_types: Optional[List[str]] = None  # keys that should use AI mode
    analysis_results: Optional[List[dict]] = None  # pass analysis back for points
    bidder_role: str = "bidder"  # "bidder" or "oem"


class SaveAndLinkRequest(BaseModel):
    tender_id: UUID
    declaration_types: List[str]
    signatory_name: Optional[str] = None
    designation: Optional[str] = None
    ai_types: Optional[List[str]] = None
    analysis_results: Optional[List[dict]] = None
    bidder_role: str = "bidder"  # "bidder" or "oem"


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


async def _get_company_profile_dict(db: AsyncSession, tenant_id: str) -> dict:
    """Load company profile and return as dict."""
    result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(tenant_id)
        )
    )
    profile = result.scalar_one_or_none()
    if not profile:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Company profile must be created first. Go to Company Profile to set up your details.",
        )

    return {
        "company_name": profile.company_name,
        "pan_number": profile.pan_number,
        "gstin": profile.gstin,
        "cin_number": profile.cin_number,
        "msme_registration": profile.msme_registration,
        "registered_address": profile.registered_address,
        "corporate_address": profile.corporate_address,
        "website": profile.website,
        "phone": profile.phone,
        "email": profile.email,
        "logo_path": profile.logo_path,
        "signature_path": profile.signature_path,
        "stamp_path": profile.stamp_path,
        "letterhead_path": profile.letterhead_path,
    }


async def _get_tender_full(db: AsyncSession, tender_id: UUID, tenant_id: str) -> tuple[dict, dict | None]:
    """Load tender and return (tender_dict, requirements_json)."""
    result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(tenant_id),
        )
    )
    tender = result.scalar_one_or_none()
    if not tender:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found",
        )

    tender_dict = {
        "id": str(tender.id),
        "title": tender.title,
        "reference_number": tender.reference_number,
        "issuing_authority": tender.issuing_authority,
    }

    # requirements is a JSON column with extracted data
    requirements = tender.requirements if tender.requirements else None

    return tender_dict, requirements


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------


@router.get("/types")
async def get_declaration_types(
    current_user: TokenData = Depends(get_current_user),
):
    """List all available declaration types with metadata."""
    return declaration_service.get_available_types()


@router.post("/analyze/{tender_id}")
async def analyze_tender_declarations(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Analyze a tender's requirements and identify which declarations need
    AI customization vs. standard templates.

    Returns analysis per declaration type with:
    - ai_recommended: bool
    - reason: why customization is needed
    - tender_specific_points: specific points from the tender
    """
    tender_dict, requirements = await _get_tender_full(db, tender_id, current_user.tenant_id)

    analysis = await declaration_service.analyze_tender_requirements(
        db=db,
        tenant_id=current_user.tenant_id,
        tender=tender_dict,
        tender_requirements=requirements,
    )

    ai_count = sum(1 for a in analysis if a.get("ai_recommended"))

    return {
        "tender_id": str(tender_id),
        "total_types": len(analysis),
        "ai_recommended_count": ai_count,
        "analysis": analysis,
    }


@router.post("/generate")
async def generate_single_declaration(
    request: SingleDeclarationRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Generate a single declaration DOCX (standard or AI-customized)."""
    company_profile = await _get_company_profile_dict(db, current_user.tenant_id)

    tender_dict = None
    requirements = None
    if request.tender_id:
        tender_dict, requirements = await _get_tender_full(db, request.tender_id, current_user.tenant_id)

    try:
        if request.mode == "ai" and tender_dict:
            buffer, filename = await declaration_service.generate_ai_customized(
                db=db,
                tenant_id=current_user.tenant_id,
                declaration_type=request.declaration_type,
                company_profile=company_profile,
                tender=tender_dict,
                tender_requirements=requirements,
                tender_specific_points=request.tender_specific_points,
                signatory_name=request.signatory_name,
                designation=request.designation,
                bidder_role=request.bidder_role,
            )
        else:
            buffer, filename = declaration_service.generate_single(
                declaration_type=request.declaration_type,
                company_profile=company_profile,
                tender=tender_dict,
                signatory_name=request.signatory_name,
                designation=request.designation,
                bidder_role=request.bidder_role,
            )
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e),
        )

    return StreamingResponse(
        buffer,
        media_type="application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        headers={"Content-Disposition": f'attachment; filename="{filename}"'},
    )


@router.post("/generate-bulk")
async def generate_bulk_declarations(
    request: BulkDeclarationRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Generate multiple declarations as a ZIP (mixed standard + AI modes)."""
    if not request.declaration_types:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="At least one declaration type must be specified.",
        )

    company_profile = await _get_company_profile_dict(db, current_user.tenant_id)

    tender_dict = None
    requirements = None
    if request.tender_id:
        tender_dict, requirements = await _get_tender_full(db, request.tender_id, current_user.tenant_id)

    zip_buffer, zip_filename, generated_files = await declaration_service.generate_bulk(
        db=db,
        tenant_id=current_user.tenant_id,
        declaration_types=request.declaration_types,
        company_profile=company_profile,
        tender=tender_dict,
        signatory_name=request.signatory_name,
        designation=request.designation,
        ai_types=request.ai_types,
        tender_requirements=requirements,
        analysis_results=request.analysis_results,
        bidder_role=request.bidder_role,
    )

    return StreamingResponse(
        zip_buffer,
        media_type="application/zip",
        headers={"Content-Disposition": f'attachment; filename="{zip_filename}"'},
    )


@router.post("/tenders/{tender_id}/generate-bulk")
async def generate_bulk_for_tender(
    tender_id: UUID,
    request: BulkDeclarationRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Generate multiple declarations for a specific tender as a ZIP."""
    if not request.declaration_types:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="At least one declaration type must be specified.",
        )

    company_profile = await _get_company_profile_dict(db, current_user.tenant_id)
    tender_dict, requirements = await _get_tender_full(db, tender_id, current_user.tenant_id)

    zip_buffer, zip_filename, generated_files = await declaration_service.generate_bulk(
        db=db,
        tenant_id=current_user.tenant_id,
        declaration_types=request.declaration_types,
        company_profile=company_profile,
        tender=tender_dict,
        signatory_name=request.signatory_name,
        designation=request.designation,
        ai_types=request.ai_types,
        tender_requirements=requirements,
        analysis_results=request.analysis_results,
        bidder_role=request.bidder_role,
    )

    return StreamingResponse(
        zip_buffer,
        media_type="application/zip",
        headers={"Content-Disposition": f'attachment; filename="{zip_filename}"'},
    )


@router.post("/preview")
async def preview_bulk_declarations(
    request: BulkDeclarationRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Generate text previews for multiple declarations without creating DOCX files.
    Returns text content for user review before saving.
    """
    if not request.declaration_types:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="At least one declaration type must be specified.",
        )

    company_profile = await _get_company_profile_dict(db, current_user.tenant_id)

    tender_dict = None
    requirements = None
    if request.tender_id:
        tender_dict, requirements = await _get_tender_full(db, request.tender_id, current_user.tenant_id)

    previews = await declaration_service.preview_bulk(
        db=db,
        tenant_id=current_user.tenant_id,
        declaration_types=request.declaration_types,
        company_profile=company_profile,
        tender=tender_dict,
        signatory_name=request.signatory_name,
        designation=request.designation,
        ai_types=request.ai_types,
        tender_requirements=requirements,
        analysis_results=request.analysis_results,
        bidder_role=request.bidder_role,
    )

    return {"previews": previews}


@router.post("/preview-single")
async def preview_single_declaration(
    request: SingleDeclarationRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Generate a text preview for a single declaration without creating a DOCX file.
    Returns text content for user review.
    """
    company_profile = await _get_company_profile_dict(db, current_user.tenant_id)

    tender_dict = None
    requirements = None
    if request.tender_id:
        tender_dict, requirements = await _get_tender_full(db, request.tender_id, current_user.tenant_id)

    try:
        preview = await declaration_service.preview_single(
            db=db,
            tenant_id=current_user.tenant_id,
            declaration_type=request.declaration_type,
            company_profile=company_profile,
            tender=tender_dict,
            signatory_name=request.signatory_name,
            designation=request.designation,
            mode=request.mode,
            tender_requirements=requirements,
            tender_specific_points=request.tender_specific_points,
            bidder_role=request.bidder_role,
        )
    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e),
        )

    return preview


@router.post("/save-and-link")
async def save_declarations_and_link(
    request: SaveAndLinkRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Generate declarations, persist to document library, and auto-link
    to matching submission checklist items.

    Returns saved/linked counts and per-declaration results.
    """
    if not request.declaration_types:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="At least one declaration type must be specified.",
        )

    company_profile = await _get_company_profile_dict(db, current_user.tenant_id)
    tender_dict, requirements = await _get_tender_full(db, request.tender_id, current_user.tenant_id)

    result = await declaration_service.save_and_link(
        db=db,
        tenant_id=current_user.tenant_id,
        user_id=current_user.user_id,
        tender_id=str(request.tender_id),
        declaration_types=request.declaration_types,
        company_profile=company_profile,
        tender=tender_dict,
        signatory_name=request.signatory_name,
        designation=request.designation,
        ai_types=request.ai_types,
        tender_requirements=requirements,
        analysis_results=request.analysis_results,
        bidder_role=request.bidder_role,
    )

    return result
