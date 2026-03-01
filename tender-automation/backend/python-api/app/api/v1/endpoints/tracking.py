"""Tracking API endpoints - OEM, portals, EMD, tender fees, and dashboard."""
from typing import List, Optional
from uuid import UUID
from datetime import datetime
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, func

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.oem import OEMMaster, OEMTenderRequirement
from app.models.tracking import PortalRegistration, EMDRecord, TenderFee
from app.models.tender import Tender
from app.models.reference_bundle import ReferenceBundle
from app.models.company import Certification
from app.schemas.oem import (
    OEMMasterCreate,
    OEMMasterUpdate,
    OEMMasterResponse,
    OEMTenderRequirementCreate,
    OEMTenderRequirementUpdate,
    OEMTenderRequirementResponse,
    PortalRegistrationCreate,
    PortalRegistrationUpdate,
    PortalRegistrationResponse,
    EMDRecordCreate,
    EMDRecordUpdate,
    EMDRecordResponse,
    TenderFeeCreate,
    TenderFeeUpdate,
    TenderFeeResponse,
    DashboardSummary,
)

router = APIRouter()


# ---------------------------------------------------------------------------
# OEM Master CRUD
# ---------------------------------------------------------------------------


@router.get("/oem", response_model=List[OEMMasterResponse])
async def list_oems(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all OEM masters for the tenant."""
    result = await db.execute(
        select(OEMMaster)
        .where(OEMMaster.tenant_id == UUID(current_user.tenant_id))
        .order_by(OEMMaster.name)
    )
    return result.scalars().all()


@router.post("/oem", response_model=OEMMasterResponse, status_code=status.HTTP_201_CREATED)
async def create_oem(
    data: OEMMasterCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new OEM master record."""
    oem = OEMMaster(
        tenant_id=UUID(current_user.tenant_id),
        **data.model_dump(),
    )
    db.add(oem)
    await db.commit()
    await db.refresh(oem)
    return oem


@router.get("/oem/{oem_id}", response_model=OEMMasterResponse)
async def get_oem(
    oem_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get an OEM master by ID."""
    result = await db.execute(
        select(OEMMaster).where(
            OEMMaster.id == oem_id,
            OEMMaster.tenant_id == UUID(current_user.tenant_id),
        )
    )
    oem = result.scalar_one_or_none()

    if not oem:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM not found",
        )
    return oem


@router.put("/oem/{oem_id}", response_model=OEMMasterResponse)
async def update_oem(
    oem_id: UUID,
    data: OEMMasterUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update an OEM master by ID."""
    result = await db.execute(
        select(OEMMaster).where(
            OEMMaster.id == oem_id,
            OEMMaster.tenant_id == UUID(current_user.tenant_id),
        )
    )
    oem = result.scalar_one_or_none()

    if not oem:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(oem, field, value)

    oem.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(oem)
    return oem


@router.delete("/oem/{oem_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_oem(
    oem_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete an OEM master by ID."""
    result = await db.execute(
        select(OEMMaster).where(
            OEMMaster.id == oem_id,
            OEMMaster.tenant_id == UUID(current_user.tenant_id),
        )
    )
    oem = result.scalar_one_or_none()

    if not oem:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM not found",
        )

    await db.delete(oem)
    await db.commit()


# ---------------------------------------------------------------------------
# OEM Tender Requirements
# ---------------------------------------------------------------------------


@router.get("/oem-requirements/tender/{tender_id}", response_model=List[OEMTenderRequirementResponse])
async def list_oem_requirements(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all OEM requirements for a tender."""
    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found",
        )

    result = await db.execute(
        select(OEMTenderRequirement)
        .where(OEMTenderRequirement.tender_id == tender_id)
        .order_by(OEMTenderRequirement.created_at.desc())
    )
    return result.scalars().all()


@router.post("/oem-requirements/tender/{tender_id}", response_model=OEMTenderRequirementResponse, status_code=status.HTTP_201_CREATED)
async def create_oem_requirement(
    tender_id: UUID,
    data: OEMTenderRequirementCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new OEM tender requirement."""
    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found",
        )

    # Verify OEM exists and belongs to tenant
    oem_result = await db.execute(
        select(OEMMaster).where(
            OEMMaster.id == data.oem_id,
            OEMMaster.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not oem_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM not found",
        )

    requirement = OEMTenderRequirement(
        tender_id=tender_id,
        **data.model_dump(),
    )
    db.add(requirement)
    await db.commit()
    await db.refresh(requirement)
    return requirement


@router.put("/oem-requirements/{req_id}", response_model=OEMTenderRequirementResponse)
async def update_oem_requirement(
    req_id: UUID,
    data: OEMTenderRequirementUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update an OEM tender requirement."""
    result = await db.execute(
        select(OEMTenderRequirement).where(OEMTenderRequirement.id == req_id)
    )
    requirement = result.scalar_one_or_none()

    if not requirement:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM requirement not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == requirement.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM requirement not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(requirement, field, value)

    requirement.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(requirement)
    return requirement


@router.delete("/oem-requirements/{req_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_oem_requirement(
    req_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete an OEM tender requirement."""
    result = await db.execute(
        select(OEMTenderRequirement).where(OEMTenderRequirement.id == req_id)
    )
    requirement = result.scalar_one_or_none()

    if not requirement:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM requirement not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == requirement.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="OEM requirement not found",
        )

    await db.delete(requirement)
    await db.commit()


# ---------------------------------------------------------------------------
# Portal Registrations
# ---------------------------------------------------------------------------


@router.get("/portals", response_model=List[PortalRegistrationResponse])
async def list_portals(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all portal registrations for the tenant."""
    result = await db.execute(
        select(PortalRegistration)
        .where(PortalRegistration.tenant_id == UUID(current_user.tenant_id))
        .order_by(PortalRegistration.portal_name)
    )
    return result.scalars().all()


@router.post("/portals", response_model=PortalRegistrationResponse, status_code=status.HTTP_201_CREATED)
async def create_portal(
    data: PortalRegistrationCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new portal registration."""
    portal = PortalRegistration(
        tenant_id=UUID(current_user.tenant_id),
        **data.model_dump(),
    )
    db.add(portal)
    await db.commit()
    await db.refresh(portal)
    return portal


@router.put("/portals/{portal_id}", response_model=PortalRegistrationResponse)
async def update_portal(
    portal_id: UUID,
    data: PortalRegistrationUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a portal registration by ID."""
    result = await db.execute(
        select(PortalRegistration).where(
            PortalRegistration.id == portal_id,
            PortalRegistration.tenant_id == UUID(current_user.tenant_id),
        )
    )
    portal = result.scalar_one_or_none()

    if not portal:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Portal registration not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(portal, field, value)

    portal.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(portal)
    return portal


@router.delete("/portals/{portal_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_portal(
    portal_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a portal registration by ID."""
    result = await db.execute(
        select(PortalRegistration).where(
            PortalRegistration.id == portal_id,
            PortalRegistration.tenant_id == UUID(current_user.tenant_id),
        )
    )
    portal = result.scalar_one_or_none()

    if not portal:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Portal registration not found",
        )

    await db.delete(portal)
    await db.commit()


@router.post("/portals/seed-indian-portals", response_model=List[PortalRegistrationResponse], status_code=status.HTTP_201_CREATED)
async def seed_indian_portals(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Seed standard Indian government e-procurement portals.

    Creates 30 common portal entries (GeM, CPPP, state portals, PSU portals).
    Skips portals that already exist for this tenant (matched by portal_name).
    """
    tenant_id = UUID(current_user.tenant_id)

    indian_portals = [
        # Central Government
        {"portal_name": "GeM (Government e-Marketplace)", "portal_url": "https://gem.gov.in", "portal_type": "central"},
        {"portal_name": "CPPP (Central Public Procurement Portal)", "portal_url": "https://eprocure.gov.in", "portal_type": "central"},
        {"portal_name": "IREPS (Indian Railways)", "portal_url": "https://www.ireps.gov.in", "portal_type": "central"},
        # State eProcurement Portals
        {"portal_name": "Maharashtra eProcurement", "portal_url": "https://mahatenders.gov.in", "portal_type": "state"},
        {"portal_name": "Karnataka eProcurement", "portal_url": "https://eproc.karnataka.gov.in", "portal_type": "state"},
        {"portal_name": "Tamil Nadu eProcurement", "portal_url": "https://tntenders.gov.in", "portal_type": "state"},
        {"portal_name": "Uttar Pradesh eProcurement", "portal_url": "https://etender.up.nic.in", "portal_type": "state"},
        {"portal_name": "Madhya Pradesh eProcurement", "portal_url": "https://mptenders.gov.in", "portal_type": "state"},
        {"portal_name": "Rajasthan eProcurement", "portal_url": "https://eproc.rajasthan.gov.in", "portal_type": "state"},
        {"portal_name": "Gujarat eProcurement", "portal_url": "https://gmdc.guj.nic.in", "portal_type": "state"},
        {"portal_name": "Andhra Pradesh eProcurement", "portal_url": "https://tender.apeprocurement.gov.in", "portal_type": "state"},
        {"portal_name": "Telangana eProcurement", "portal_url": "https://tender.telangana.gov.in", "portal_type": "state"},
        {"portal_name": "Kerala eProcurement", "portal_url": "https://etenders.kerala.gov.in", "portal_type": "state"},
        {"portal_name": "West Bengal eProcurement", "portal_url": "https://wbtenders.gov.in", "portal_type": "state"},
        {"portal_name": "Bihar eProcurement", "portal_url": "https://eproc.bihar.gov.in", "portal_type": "state"},
        {"portal_name": "Odisha eProcurement", "portal_url": "https://tendersodisha.gov.in", "portal_type": "state"},
        {"portal_name": "Punjab eProcurement", "portal_url": "https://eproc.punjab.gov.in", "portal_type": "state"},
        {"portal_name": "Haryana eProcurement", "portal_url": "https://etenders.hry.nic.in", "portal_type": "state"},
        {"portal_name": "Delhi eProcurement", "portal_url": "https://govtprocurement.delhi.gov.in", "portal_type": "state"},
        {"portal_name": "Jharkhand eProcurement", "portal_url": "https://jharkhandtenders.gov.in", "portal_type": "state"},
        {"portal_name": "Chhattisgarh eProcurement", "portal_url": "https://eproc.cgstate.gov.in", "portal_type": "state"},
        {"portal_name": "Assam eProcurement", "portal_url": "https://assamtenders.gov.in", "portal_type": "state"},
        {"portal_name": "Uttarakhand eProcurement", "portal_url": "https://uktenders.gov.in", "portal_type": "state"},
        {"portal_name": "Himachal Pradesh eProcurement", "portal_url": "https://hptenders.gov.in", "portal_type": "state"},
        {"portal_name": "Goa eProcurement", "portal_url": "https://goaenivida.gov.in", "portal_type": "state"},
        # PSU Portals
        {"portal_name": "NTPC eProcurement", "portal_url": "https://etender.ntpc.co.in", "portal_type": "psu"},
        {"portal_name": "BHEL eProcurement", "portal_url": "https://www.bhel.com/tenders", "portal_type": "psu"},
        {"portal_name": "ONGC eProcurement", "portal_url": "https://etender.ongc.co.in", "portal_type": "psu"},
        {"portal_name": "Coal India eProcurement", "portal_url": "https://coalindiatenders.nic.in", "portal_type": "psu"},
        {"portal_name": "BSNL eProcurement", "portal_url": "https://www.bsnltenders.com", "portal_type": "psu"},
    ]

    # Get existing portal names for this tenant
    existing_result = await db.execute(
        select(PortalRegistration.portal_name).where(
            PortalRegistration.tenant_id == tenant_id
        )
    )
    existing_names = {row[0] for row in existing_result.all()}

    created = []
    for portal_data in indian_portals:
        if portal_data["portal_name"] in existing_names:
            continue

        portal = PortalRegistration(
            tenant_id=tenant_id,
            portal_name=portal_data["portal_name"],
            portal_url=portal_data["portal_url"],
            portal_type=portal_data["portal_type"],
            registration_status="not_registered",
        )
        db.add(portal)
        created.append(portal)

    await db.commit()

    # Refresh all created portals to get IDs
    for portal in created:
        await db.refresh(portal)

    return created


# ---------------------------------------------------------------------------
# EMD Records
# ---------------------------------------------------------------------------


@router.get("/emd", response_model=List[EMDRecordResponse])
async def list_emd_records(
    tender_id: Optional[UUID] = Query(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    List EMD records.

    Optionally filter by tender_id. Only returns records for tenders
    belonging to the current tenant.
    """
    if tender_id:
        # Verify tender belongs to tenant
        tender_result = await db.execute(
            select(Tender).where(
                Tender.id == tender_id,
                Tender.tenant_id == UUID(current_user.tenant_id),
            )
        )
        if not tender_result.scalar_one_or_none():
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Tender not found",
            )

        result = await db.execute(
            select(EMDRecord)
            .where(EMDRecord.tender_id == tender_id)
            .order_by(EMDRecord.created_at.desc())
        )
    else:
        # Get all EMDs for tenant's tenders
        result = await db.execute(
            select(EMDRecord)
            .join(Tender, EMDRecord.tender_id == Tender.id)
            .where(Tender.tenant_id == UUID(current_user.tenant_id))
            .order_by(EMDRecord.created_at.desc())
        )

    return result.scalars().all()


@router.post("/emd/tender/{tender_id}", response_model=EMDRecordResponse, status_code=status.HTTP_201_CREATED)
async def create_emd_record(
    tender_id: UUID,
    data: EMDRecordCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new EMD record for a tender."""
    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found",
        )

    emd = EMDRecord(
        tender_id=tender_id,
        **data.model_dump(),
    )
    db.add(emd)
    await db.commit()
    await db.refresh(emd)
    return emd


@router.put("/emd/{emd_id}", response_model=EMDRecordResponse)
async def update_emd_record(
    emd_id: UUID,
    data: EMDRecordUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update an EMD record by ID."""
    result = await db.execute(
        select(EMDRecord).where(EMDRecord.id == emd_id)
    )
    emd = result.scalar_one_or_none()

    if not emd:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="EMD record not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == emd.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="EMD record not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(emd, field, value)

    emd.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(emd)
    return emd


@router.delete("/emd/{emd_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_emd_record(
    emd_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete an EMD record by ID."""
    result = await db.execute(
        select(EMDRecord).where(EMDRecord.id == emd_id)
    )
    emd = result.scalar_one_or_none()

    if not emd:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="EMD record not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == emd.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="EMD record not found",
        )

    await db.delete(emd)
    await db.commit()


# ---------------------------------------------------------------------------
# Tender Fees
# ---------------------------------------------------------------------------


@router.get("/fees", response_model=List[TenderFeeResponse])
async def list_tender_fees(
    tender_id: Optional[UUID] = Query(None),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    List tender fee records.

    Optionally filter by tender_id.
    """
    if tender_id:
        # Verify tender belongs to tenant
        tender_result = await db.execute(
            select(Tender).where(
                Tender.id == tender_id,
                Tender.tenant_id == UUID(current_user.tenant_id),
            )
        )
        if not tender_result.scalar_one_or_none():
            raise HTTPException(
                status_code=status.HTTP_404_NOT_FOUND,
                detail="Tender not found",
            )

        result = await db.execute(
            select(TenderFee)
            .where(TenderFee.tender_id == tender_id)
            .order_by(TenderFee.created_at.desc())
        )
    else:
        # Get all fees for tenant's tenders
        result = await db.execute(
            select(TenderFee)
            .join(Tender, TenderFee.tender_id == Tender.id)
            .where(Tender.tenant_id == UUID(current_user.tenant_id))
            .order_by(TenderFee.created_at.desc())
        )

    return result.scalars().all()


@router.post("/fees/tender/{tender_id}", response_model=TenderFeeResponse, status_code=status.HTTP_201_CREATED)
async def create_tender_fee(
    tender_id: UUID,
    data: TenderFeeCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new tender fee record."""
    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender not found",
        )

    fee = TenderFee(
        tender_id=tender_id,
        **data.model_dump(),
    )
    db.add(fee)
    await db.commit()
    await db.refresh(fee)
    return fee


@router.put("/fees/{fee_id}", response_model=TenderFeeResponse)
async def update_tender_fee(
    fee_id: UUID,
    data: TenderFeeUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a tender fee by ID."""
    result = await db.execute(
        select(TenderFee).where(TenderFee.id == fee_id)
    )
    fee = result.scalar_one_or_none()

    if not fee:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender fee not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == fee.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender fee not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(fee, field, value)

    fee.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(fee)
    return fee


@router.delete("/fees/{fee_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_tender_fee(
    fee_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a tender fee by ID."""
    result = await db.execute(
        select(TenderFee).where(TenderFee.id == fee_id)
    )
    fee = result.scalar_one_or_none()

    if not fee:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender fee not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == fee.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Tender fee not found",
        )

    await db.delete(fee)
    await db.commit()


# ---------------------------------------------------------------------------
# Dashboard
# ---------------------------------------------------------------------------


@router.get("/dashboard", response_model=DashboardSummary)
async def get_dashboard(
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get aggregate dashboard summary for the tenant.

    Returns counts of active tenders, bundles, certifications,
    portals, OEMs, and total EMD amount.
    """
    tenant_id = UUID(current_user.tenant_id)

    # Active tenders
    active_tenders_result = await db.execute(
        select(func.count()).select_from(Tender).where(
            Tender.tenant_id == tenant_id,
            Tender.status.in_(["draft", "in_progress"]),
        )
    )
    active_tenders = active_tenders_result.scalar() or 0

    # Total bundles
    bundles_result = await db.execute(
        select(func.count()).select_from(ReferenceBundle).where(
            ReferenceBundle.tenant_id == tenant_id,
        )
    )
    total_bundles = bundles_result.scalar() or 0

    # Total certifications (via company profile)
    from app.models.company import CompanyProfile
    profile_result = await db.execute(
        select(CompanyProfile.id).where(CompanyProfile.tenant_id == tenant_id)
    )
    profile_row = profile_result.first()
    total_certifications = 0
    if profile_row:
        cert_result = await db.execute(
            select(func.count()).select_from(Certification).where(
                Certification.company_id == profile_row[0],
            )
        )
        total_certifications = cert_result.scalar() or 0

    # Active portals
    portals_result = await db.execute(
        select(func.count()).select_from(PortalRegistration).where(
            PortalRegistration.tenant_id == tenant_id,
            PortalRegistration.registration_status == "active",
        )
    )
    active_portals = portals_result.scalar() or 0

    # Total OEMs
    oems_result = await db.execute(
        select(func.count()).select_from(OEMMaster).where(
            OEMMaster.tenant_id == tenant_id,
        )
    )
    total_oems = oems_result.scalar() or 0

    # Total EMD amount (sum of non-released EMDs for tenant's tenders)
    emd_result = await db.execute(
        select(func.coalesce(func.sum(EMDRecord.amount), 0))
        .join(Tender, EMDRecord.tender_id == Tender.id)
        .where(
            Tender.tenant_id == tenant_id,
            EMDRecord.status.in_(["submitted", "pending"]),
        )
    )
    total_emd_amount = float(emd_result.scalar() or 0)

    return DashboardSummary(
        active_tenders=active_tenders,
        total_bundles=total_bundles,
        total_certifications=total_certifications,
        active_portals=active_portals,
        total_oems=total_oems,
        total_emd_amount=total_emd_amount,
    )
