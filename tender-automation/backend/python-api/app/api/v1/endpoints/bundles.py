"""Reference bundles, criteria, and scoring API endpoints."""
from typing import List, Optional
from uuid import UUID
from datetime import datetime
from fastapi import APIRouter, Depends, HTTPException, status, Query
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select, func, or_

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.reference_bundle import (
    ReferenceBundle,
    BundleDocument,
    TenderCriteria,
    BundleCriteriaAssignment,
)
from app.models.tender import Tender
from app.schemas.reference_bundle import (
    ReferenceBundleCreate,
    ReferenceBundleUpdate,
    ReferenceBundleResponse,
    BundleListResponse,
    BundleDocumentCreate,
    BundleDocumentResponse,
    TenderCriteriaCreate,
    TenderCriteriaUpdate,
    TenderCriteriaResponse,
    BundleCriteriaAssignmentCreate,
    BundleCriteriaAssignmentResponse,
    CriteriaScore,
    ScoringSimulation,
)

router = APIRouter()


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------


def compute_value_bands(value):
    """Compute value band tags based on contract value."""
    if not value:
        return []
    bands = []
    if value >= 10000000:
        bands.append(">=1cr")
    if value >= 50000000:
        bands.append(">=5cr")
    if value >= 100000000:
        bands.append(">=10cr")
    if value >= 500000000:
        bands.append(">=50cr")
    if value >= 1000000000:
        bands.append(">=100cr")
    return bands


def compute_completeness(docs_list) -> int:
    """
    Compute completeness score based on document tiers present.

    Scoring: work_order=30, cc=30, invoice=20, performance_cert=10, reference_letter=10
    """
    if not docs_list:
        return 0

    subtypes = {d.doc_subtype for d in docs_list}
    score = 0
    if "work_order" in subtypes:
        score += 30
    if "cc" in subtypes:
        score += 30
    if "invoice" in subtypes:
        score += 20
    if "performance_cert" in subtypes:
        score += 10
    if "reference_letter" in subtypes:
        score += 10
    return score


def get_missing_documents(docs_list) -> List[str]:
    """Return list of missing document subtypes."""
    required = {"work_order", "cc", "invoice", "performance_cert", "reference_letter"}
    if not docs_list:
        return list(required)
    present = {d.doc_subtype for d in docs_list}
    return list(required - present)


# ---------------------------------------------------------------------------
# Reference Bundles CRUD
# ---------------------------------------------------------------------------


@router.get("/", response_model=BundleListResponse)
async def list_bundles(
    page: int = Query(1, ge=1),
    page_size: int = Query(50, ge=1, le=100),
    client_type: Optional[str] = None,
    status_filter: Optional[str] = Query(None, alias="status"),
    search: Optional[str] = None,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    List reference bundles with pagination and filtering.

    Filters: client_type, status, search (in bundle_name, client_name).
    """
    query = select(ReferenceBundle).where(
        ReferenceBundle.tenant_id == UUID(current_user.tenant_id)
    )

    if client_type:
        query = query.where(ReferenceBundle.client_type == client_type)

    if status_filter:
        query = query.where(ReferenceBundle.status == status_filter)

    if search:
        search_pattern = f"%{search}%"
        query = query.where(
            or_(
                ReferenceBundle.bundle_name.ilike(search_pattern),
                ReferenceBundle.client_name.ilike(search_pattern),
            )
        )

    # Count
    count_query = select(func.count()).select_from(query.subquery())
    total_result = await db.execute(count_query)
    total = total_result.scalar()

    # Paginate
    query = query.order_by(ReferenceBundle.created_at.desc())
    query = query.offset((page - 1) * page_size).limit(page_size)

    result = await db.execute(query)
    bundles = result.scalars().all()

    # Eagerly load documents for each bundle
    bundle_responses = []
    for bundle in bundles:
        docs_result = await db.execute(
            select(BundleDocument).where(BundleDocument.bundle_id == bundle.id)
        )
        docs = docs_result.scalars().all()

        resp = ReferenceBundleResponse.model_validate(bundle)
        resp.bundle_documents = [BundleDocumentResponse.model_validate(d) for d in docs]
        bundle_responses.append(resp)

    return BundleListResponse(
        items=bundle_responses,
        total=total,
        page=page,
        page_size=page_size,
    )


@router.post("/", response_model=ReferenceBundleResponse, status_code=status.HTTP_201_CREATED)
async def create_bundle(
    data: ReferenceBundleCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new reference bundle. Auto-computes value_bands."""
    bundle = ReferenceBundle(
        tenant_id=UUID(current_user.tenant_id),
        **data.model_dump(),
    )
    # Auto-compute value bands from contract_value
    bundle.value_bands = compute_value_bands(bundle.contract_value)
    bundle.completeness_score = 0
    bundle.missing_documents = list({"work_order", "cc", "invoice", "performance_cert", "reference_letter"})

    db.add(bundle)
    await db.commit()
    await db.refresh(bundle)
    return bundle


@router.get("/{bundle_id}", response_model=ReferenceBundleResponse)
async def get_bundle(
    bundle_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get a reference bundle by ID with its documents."""
    result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    bundle = result.scalar_one_or_none()

    if not bundle:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Reference bundle not found",
        )

    # Load documents
    docs_result = await db.execute(
        select(BundleDocument).where(BundleDocument.bundle_id == bundle.id)
    )
    docs = docs_result.scalars().all()

    resp = ReferenceBundleResponse.model_validate(bundle)
    resp.bundle_documents = [BundleDocumentResponse.model_validate(d) for d in docs]
    return resp


@router.put("/{bundle_id}", response_model=ReferenceBundleResponse)
async def update_bundle(
    bundle_id: UUID,
    data: ReferenceBundleUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a reference bundle by ID."""
    result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    bundle = result.scalar_one_or_none()

    if not bundle:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Reference bundle not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(bundle, field, value)

    # Re-compute value bands if contract_value changed
    if "contract_value" in update_data:
        bundle.value_bands = compute_value_bands(bundle.contract_value)

    bundle.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(bundle)
    return bundle


@router.delete("/{bundle_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_bundle(
    bundle_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a reference bundle by ID."""
    result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    bundle = result.scalar_one_or_none()

    if not bundle:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Reference bundle not found",
        )

    await db.delete(bundle)
    await db.commit()


# ---------------------------------------------------------------------------
# Bundle Documents
# ---------------------------------------------------------------------------


@router.post("/{bundle_id}/documents", response_model=BundleDocumentResponse, status_code=status.HTTP_201_CREATED)
async def add_bundle_document(
    bundle_id: UUID,
    data: BundleDocumentCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Add a document record to a reference bundle."""
    # Verify bundle exists and belongs to tenant
    bundle_result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    bundle = bundle_result.scalar_one_or_none()

    if not bundle:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Reference bundle not found",
        )

    doc = BundleDocument(
        bundle_id=bundle_id,
        **data.model_dump(),
    )
    db.add(doc)
    await db.flush()

    # Recompute completeness
    docs_result = await db.execute(
        select(BundleDocument).where(BundleDocument.bundle_id == bundle_id)
    )
    all_docs = docs_result.scalars().all()
    bundle.completeness_score = compute_completeness(all_docs)
    bundle.missing_documents = get_missing_documents(all_docs)
    bundle.updated_at = datetime.utcnow()

    await db.commit()
    await db.refresh(doc)
    return doc


@router.delete("/{bundle_id}/documents/{doc_id}", status_code=status.HTTP_204_NO_CONTENT)
async def remove_bundle_document(
    bundle_id: UUID,
    doc_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Remove a document from a reference bundle."""
    # Verify bundle belongs to tenant
    bundle_result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    bundle = bundle_result.scalar_one_or_none()

    if not bundle:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Reference bundle not found",
        )

    result = await db.execute(
        select(BundleDocument).where(
            BundleDocument.id == doc_id,
            BundleDocument.bundle_id == bundle_id,
        )
    )
    doc = result.scalar_one_or_none()

    if not doc:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Bundle document not found",
        )

    await db.delete(doc)
    await db.flush()

    # Recompute completeness
    docs_result = await db.execute(
        select(BundleDocument).where(BundleDocument.bundle_id == bundle_id)
    )
    remaining_docs = docs_result.scalars().all()
    bundle.completeness_score = compute_completeness(remaining_docs)
    bundle.missing_documents = get_missing_documents(remaining_docs)
    bundle.updated_at = datetime.utcnow()

    await db.commit()


# ---------------------------------------------------------------------------
# Tender Criteria
# ---------------------------------------------------------------------------


@router.get("/criteria/tender/{tender_id}", response_model=List[TenderCriteriaResponse])
async def list_criteria(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all criteria for a tender."""
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
        select(TenderCriteria)
        .where(TenderCriteria.tender_id == tender_id)
        .order_by(TenderCriteria.criteria_code)
    )
    return result.scalars().all()


@router.post("/criteria/tender/{tender_id}", response_model=TenderCriteriaResponse, status_code=status.HTTP_201_CREATED)
async def create_criteria(
    tender_id: UUID,
    data: TenderCriteriaCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a new criteria for a tender."""
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

    criteria = TenderCriteria(
        tender_id=tender_id,
        **data.model_dump(),
    )
    db.add(criteria)
    await db.commit()
    await db.refresh(criteria)
    return criteria


@router.put("/criteria/{criteria_id}", response_model=TenderCriteriaResponse)
async def update_criteria(
    criteria_id: UUID,
    data: TenderCriteriaUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a criteria by ID."""
    result = await db.execute(
        select(TenderCriteria).where(TenderCriteria.id == criteria_id)
    )
    criteria = result.scalar_one_or_none()

    if not criteria:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Criteria not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == criteria.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Criteria not found",
        )

    update_data = data.model_dump(exclude_unset=True)
    for field, value in update_data.items():
        setattr(criteria, field, value)

    criteria.updated_at = datetime.utcnow()
    await db.commit()
    await db.refresh(criteria)
    return criteria


@router.delete("/criteria/{criteria_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_criteria(
    criteria_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a criteria by ID."""
    result = await db.execute(
        select(TenderCriteria).where(TenderCriteria.id == criteria_id)
    )
    criteria = result.scalar_one_or_none()

    if not criteria:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Criteria not found",
        )

    # Verify tender belongs to tenant
    tender_result = await db.execute(
        select(Tender).where(
            Tender.id == criteria.tender_id,
            Tender.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not tender_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Criteria not found",
        )

    await db.delete(criteria)
    await db.commit()


# ---------------------------------------------------------------------------
# Bundle-Criteria Assignments
# ---------------------------------------------------------------------------


@router.post("/assignments", response_model=BundleCriteriaAssignmentResponse, status_code=status.HTTP_201_CREATED)
async def create_assignment(
    data: BundleCriteriaAssignmentCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a bundle-criteria assignment."""
    # Verify bundle belongs to tenant
    bundle_result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == data.bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not bundle_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Reference bundle not found",
        )

    # Verify criteria exists
    criteria_result = await db.execute(
        select(TenderCriteria).where(TenderCriteria.id == data.criteria_id)
    )
    if not criteria_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Criteria not found",
        )

    # Check for duplicate assignment
    existing_result = await db.execute(
        select(BundleCriteriaAssignment).where(
            BundleCriteriaAssignment.bundle_id == data.bundle_id,
            BundleCriteriaAssignment.criteria_id == data.criteria_id,
        )
    )
    if existing_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="This bundle is already assigned to this criteria",
        )

    assignment = BundleCriteriaAssignment(**data.model_dump())
    db.add(assignment)
    await db.commit()
    await db.refresh(assignment)
    return assignment


@router.get("/assignments/tender/{tender_id}", response_model=List[BundleCriteriaAssignmentResponse])
async def list_assignments(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all bundle-criteria assignments for a tender."""
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

    # Get all criteria IDs for this tender
    criteria_result = await db.execute(
        select(TenderCriteria.id).where(TenderCriteria.tender_id == tender_id)
    )
    criteria_ids = [row[0] for row in criteria_result.all()]

    if not criteria_ids:
        return []

    result = await db.execute(
        select(BundleCriteriaAssignment)
        .where(BundleCriteriaAssignment.criteria_id.in_(criteria_ids))
        .order_by(BundleCriteriaAssignment.created_at.desc())
    )
    return result.scalars().all()


@router.delete("/assignments/{assignment_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_assignment(
    assignment_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a bundle-criteria assignment."""
    result = await db.execute(
        select(BundleCriteriaAssignment).where(BundleCriteriaAssignment.id == assignment_id)
    )
    assignment = result.scalar_one_or_none()

    if not assignment:
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Assignment not found",
        )

    # Verify ownership via bundle tenant
    bundle_result = await db.execute(
        select(ReferenceBundle).where(
            ReferenceBundle.id == assignment.bundle_id,
            ReferenceBundle.tenant_id == UUID(current_user.tenant_id),
        )
    )
    if not bundle_result.scalar_one_or_none():
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="Assignment not found",
        )

    await db.delete(assignment)
    await db.commit()


# ---------------------------------------------------------------------------
# Scoring Simulation
# ---------------------------------------------------------------------------


@router.get("/scoring/tender/{tender_id}", response_model=ScoringSimulation)
async def scoring_simulation(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Get scoring simulation for a tender.

    Aggregates criteria, assigned bundles, and predicted marks to produce
    a gap analysis per criteria.
    """
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

    # Get all criteria for the tender
    criteria_result = await db.execute(
        select(TenderCriteria)
        .where(TenderCriteria.tender_id == tender_id)
        .order_by(TenderCriteria.criteria_code)
    )
    criteria_list = criteria_result.scalars().all()

    total_max = 0.0
    total_predicted = 0.0
    scores = []

    for c in criteria_list:
        # Get best predicted marks for this criteria
        assignment_result = await db.execute(
            select(func.max(BundleCriteriaAssignment.predicted_marks)).where(
                BundleCriteriaAssignment.criteria_id == c.id,
            )
        )
        best_predicted = assignment_result.scalar() or 0.0

        gap = ""
        if best_predicted < (c.qualifying_marks or 0):
            gap = f"Below qualifying marks ({c.qualifying_marks}). Need {(c.qualifying_marks or 0) - best_predicted} more marks."
        elif best_predicted < c.max_marks:
            gap = f"Partial score. Potential to gain {c.max_marks - best_predicted} more marks."
        else:
            gap = "Full marks achieved."

        total_max += c.max_marks
        total_predicted += best_predicted

        scores.append(
            CriteriaScore(
                criteria_code=c.criteria_code,
                max_marks=c.max_marks,
                predicted_marks=best_predicted,
                gap_analysis=gap,
            )
        )

    return ScoringSimulation(
        tender_id=tender_id,
        total_max_marks=total_max,
        total_predicted_marks=total_predicted,
        criteria_scores=scores,
    )
