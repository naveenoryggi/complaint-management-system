"""Company master API endpoints - profile, certifications, and personnel."""
from typing import List
from uuid import UUID
from datetime import datetime
from fastapi import APIRouter, Depends, HTTPException, status
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.company import CompanyProfile, Certification, Personnel
from app.schemas.company import (
    CompanyProfileCreate,
    CompanyProfileUpdate,
    CompanyProfileResponse,
    CertificationCreate,
    CertificationUpdate,
    CertificationResponse,
    PersonnelCreate,
    PersonnelUpdate,
    PersonnelResponse,
)

router = APIRouter()


# ---------------------------------------------------------------------------
# Company Profile
# ---------------------------------------------------------------------------


@router.get("/profile", response_model=CompanyProfileResponse)
async def get_company_profile(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get the company profile for the current tenant.

    Returns the existing profile or 404 if none exists yet.
    """
    result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Company profile not found. Create one first.",
        )

    return profile


@router.post("/profile", response_model=CompanyProfileResponse, status_code=status.HTTP_201_CREATED)
async def create_company_profile(
    data: CompanyProfileCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Create a company profile for the current tenant.

    Only one profile per tenant is allowed.
    """
    # Check if profile already exists
    existing = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    if existing.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Company profile already exists for this tenant. Use PUT to update.",
        )

    profile = CompanyProfile(
        tenant_id=UUID(current_user.tenant_id),
        **data.model_dump(),
    )
    db.add(profile)
    await db.commit()
    await db.refresh(profile)
    return profile


@router.put("/profile", response_model=CompanyProfileResponse)
async def update_company_profile(
    data: CompanyProfileUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Update the company profile for the current tenant.

    Only updates fields that are provided (partial update).
    """
    result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Company profile not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(profile, field, value)

    profile.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(profile)
    return profile


# ---------------------------------------------------------------------------
# Certifications
# ---------------------------------------------------------------------------


@router.get("/certifications", response_model=List[CertificationResponse])
async def list_certifications(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all certifications for the tenant's company profile."""
    # Get company profile
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        return []

    result = await db.execute(
        select(Certification)
        .where(Certification.company_id == profile.id)
        .order_by(Certification.created_at.desc())
    )
    return result.scalars().all()


@router.post("/certifications", response_model=CertificationResponse, status_code=status.HTTP_201_CREATED)
async def create_certification(
    data: CertificationCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new certification for the tenant's company profile."""
    # Get company profile
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Company profile must be created first.",
        )

    cert = Certification(
        company_id=profile.id,
        **data.model_dump(),
    )
    db.add(cert)
    await db.commit()
    await db.refresh(cert)
    return cert


@router.put("/certifications/{cert_id}", response_model=CertificationResponse)
async def update_certification(
    cert_id: UUID,
    data: CertificationUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a certification by ID."""
    # Get company profile to verify ownership
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Company profile not found",
        )

    result = await db.execute(
        select(Certification).where(
            Certification.id == cert_id,
            Certification.company_id == profile.id,
        )
    )
    cert = result.scalar_one_or_none()

    if not cert:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Certification not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(cert, field, value)

    cert.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(cert)
    return cert


@router.delete("/certifications/{cert_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_certification(
    cert_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a certification by ID."""
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Company profile not found",
        )

    result = await db.execute(
        select(Certification).where(
            Certification.id == cert_id,
            Certification.company_id == profile.id,
        )
    )
    cert = result.scalar_one_or_none()

    if not cert:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Certification not found",
        )

    await db.delete(cert)
    await db.commit()


# ---------------------------------------------------------------------------
# Personnel
# ---------------------------------------------------------------------------


@router.get("/personnel", response_model=List[PersonnelResponse])
async def list_personnel(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all key personnel for the tenant's company profile."""
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        return []

    result = await db.execute(
        select(Personnel)
        .where(Personnel.company_id == profile.id)
        .order_by(Personnel.created_at.desc())
    )
    return result.scalars().all()


@router.post("/personnel", response_model=PersonnelResponse, status_code=status.HTTP_201_CREATED)
async def create_personnel(
    data: PersonnelCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new personnel record for the tenant's company profile."""
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Company profile must be created first.",
        )

    person = Personnel(
        company_id=profile.id,
        **data.model_dump(),
    )
    db.add(person)
    await db.commit()
    await db.refresh(person)
    return person


@router.put("/personnel/{person_id}", response_model=PersonnelResponse)
async def update_personnel(
    person_id: UUID,
    data: PersonnelUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a personnel record by ID."""
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Company profile not found",
        )

    result = await db.execute(
        select(Personnel).where(
            Personnel.id == person_id,
            Personnel.company_id == profile.id,
        )
    )
    person = result.scalar_one_or_none()

    if not person:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Personnel not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(person, field, value)

    person.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(person)
    return person


@router.delete("/personnel/{person_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_personnel(
    person_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a personnel record by ID."""
    profile_result = await db.execute(
        select(CompanyProfile).where(
            CompanyProfile.tenant_id == UUID(current_user.tenant_id)
        )
    )
    profile = profile_result.scalar_one_or_none()

    if not profile:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Company profile not found",
        )

    result = await db.execute(
        select(Personnel).where(
            Personnel.id == person_id,
            Personnel.company_id == profile.id,
        )
    )
    person = result.scalar_one_or_none()

    if not person:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Personnel not found",
        )

    await db.delete(person)
    await db.commit()
