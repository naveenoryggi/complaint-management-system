"""Submission checklist API — CRUD, auto-generate, summary, cover packaging, and assembly."""
from typing import List, Optional
from uuid import UUID
from datetime import datetime

from fastapi import APIRouter, Depends, HTTPException, Query, status
from fastapi.responses import FileResponse
from pydantic import BaseModel
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.models.submission_checklist import SubmissionChecklistItem
from app.models.document import Document
from app.models.tender import Tender
from app.services.checklist_service import (
    auto_generate_checklist,
    compute_checklist_summary,
    deduplicate_checklist,
    find_duplicates,
    analyze_checklist_requirements,
    remove_irrelevant_items,
    DEFAULT_DEDUP_THRESHOLD,
)
from app.services.checklist_templates import get_all_templates, get_cover_summary
from app.services.cover_assembly_service import cover_assembly_service

router = APIRouter()


# ---------------------------------------------------------------------------
# Pydantic Schemas
# ---------------------------------------------------------------------------

class ChecklistItemCreate(BaseModel):
    document_name: str
    document_category: str = "other"
    submission_mode: str = "online"
    is_critical: bool = False
    envelope: str | None = None
    cover_name: str | None = None
    online_status: str = "not_started"
    offline_status: str = "not_started"
    notarization_required: bool = False
    notarization_type: str | None = None
    document_origin: str = "self_generated"  # tender_provided, self_generated, pre_existing
    format_reference: str | None = None
    obtaining_source: str | None = None
    can_auto_generate: bool = False
    linked_document_id: str | None = None
    due_date: str | None = None
    notes: str | None = None
    sort_order: int = 0
    source: str = "manual"


class ChecklistItemUpdate(BaseModel):
    document_name: str | None = None
    document_category: str | None = None
    submission_mode: str | None = None
    is_critical: bool | None = None
    envelope: str | None = None
    cover_name: str | None = None
    online_status: str | None = None
    offline_status: str | None = None
    notarization_required: bool | None = None
    notarization_type: str | None = None
    notarization_status: str | None = None
    document_origin: str | None = None
    format_reference: str | None = None
    obtaining_source: str | None = None
    can_auto_generate: bool | None = None
    linked_document_id: str | None = None
    due_date: str | None = None
    notes: str | None = None
    sort_order: int | None = None


# ---------------------------------------------------------------------------
# Endpoints
# ---------------------------------------------------------------------------

@router.get("/{tender_id}/submission-checklist")
async def list_checklist_items(
    tender_id: UUID,
    mode: str | None = Query(None, description="Filter by submission_mode: online, offline, both"),
    category: str | None = Query(None, description="Filter by document_category"),
    envelope: str | None = Query(None, description="Filter by envelope"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """List all submission checklist items for a tender, with optional filters."""
    query = (
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender_id)
    )
    if mode:
        if mode in ("online", "offline"):
            query = query.where(
                SubmissionChecklistItem.submission_mode.in_([mode, "both"])
            )
        else:
            query = query.where(SubmissionChecklistItem.submission_mode == mode)
    if category:
        query = query.where(SubmissionChecklistItem.document_category == category)
    if envelope:
        query = query.where(SubmissionChecklistItem.envelope == envelope)

    query = query.order_by(SubmissionChecklistItem.sort_order, SubmissionChecklistItem.document_name)
    result = await db.execute(query)
    items = result.scalars().all()
    return [_item_to_dict(i) for i in items]


@router.post("/{tender_id}/submission-checklist", status_code=status.HTTP_201_CREATED)
async def create_checklist_item(
    tender_id: UUID,
    data: ChecklistItemCreate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Create a single submission checklist item."""
    notarization_status = "pending" if data.notarization_required else "not_required"

    item = SubmissionChecklistItem(
        tender_id=tender_id,
        document_name=data.document_name,
        document_category=data.document_category,
        submission_mode=data.submission_mode,
        is_critical=data.is_critical,
        envelope=data.envelope,
        cover_name=data.cover_name,
        online_status=data.online_status,
        offline_status=data.offline_status,
        notarization_required=data.notarization_required,
        notarization_type=data.notarization_type,
        notarization_status=notarization_status,
        document_origin=data.document_origin,
        format_reference=data.format_reference,
        obtaining_source=data.obtaining_source,
        can_auto_generate=data.can_auto_generate,
        linked_document_id=UUID(data.linked_document_id) if data.linked_document_id else None,
        due_date=datetime.strptime(data.due_date, "%Y-%m-%d").date() if data.due_date else None,
        notes=data.notes,
        sort_order=data.sort_order,
        source=data.source,
    )
    db.add(item)
    await db.flush()
    return _item_to_dict(item)


@router.post("/{tender_id}/submission-checklist/generate")
async def generate_checklist(
    tender_id: UUID,
    include_offline: bool = Query(True, description="Include offline-only documents"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Auto-generate checklist from standard Indian tender templates. Idempotent.

    Also deduplicates existing items to remove near-duplicate entries
    (e.g., from AI extraction + templates having overlapping documents).
    """
    # First deduplicate any existing items
    removed = await deduplicate_checklist(db, tender_id)

    # Then generate from templates (will skip items that already exist)
    created = await auto_generate_checklist(db, tender_id, include_offline)

    parts = []
    if removed > 0:
        parts.append(f"{removed} duplicate(s) removed")
    if created > 0:
        parts.append(f"{created} new item(s) added")
    if not parts:
        parts.append("Checklist is up to date")

    return {"created": created, "removed": removed, "message": ". ".join(parts)}


@router.get("/{tender_id}/submission-checklist/duplicates")
async def preview_duplicates(
    tender_id: UUID,
    threshold: float = Query(DEFAULT_DEDUP_THRESHOLD, ge=0.1, le=1.0,
                              description="Similarity threshold (0.1=loose, 1.0=exact only). Default 0.6"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Preview duplicate checklist items at a given similarity threshold.

    Returns pairs of items considered duplicates with match details.
    Does NOT delete anything — use the deduplicate endpoint to apply.
    """
    duplicates = await find_duplicates(db, tender_id, threshold)
    return {
        "threshold": threshold,
        "duplicate_count": len(duplicates),
        "duplicates": duplicates,
    }


@router.post("/{tender_id}/submission-checklist/deduplicate")
async def deduplicate(
    tender_id: UUID,
    threshold: float = Query(DEFAULT_DEDUP_THRESHOLD, ge=0.1, le=1.0,
                              description="Similarity threshold (0.1=loose, 1.0=exact only). Default 0.6"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Remove duplicate checklist items at a given similarity threshold."""
    removed = await deduplicate_checklist(db, tender_id, threshold)
    return {
        "threshold": threshold,
        "removed": removed,
        "message": f"{removed} duplicate(s) removed at threshold {threshold}",
    }


@router.post("/{tender_id}/submission-checklist/analyze")
async def analyze_checklist(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    AI-analyze checklist items against tender requirements.

    Determines which items are relevant/required for this specific tender
    and which can be removed.
    """
    # Load tender
    tender_result = await db.execute(
        select(Tender).where(Tender.id == tender_id)
    )
    tender = tender_result.scalars().first()
    if not tender:
        raise HTTPException(status_code=404, detail="Tender not found")

    tender_dict = {
        "title": tender.title,
        "reference_number": tender.reference_number,
        "issuing_authority": tender.issuing_authority,
    }
    requirements = tender.requirements if tender.requirements else None

    # Load existing checklist items
    items_result = await db.execute(
        select(SubmissionChecklistItem)
        .where(SubmissionChecklistItem.tender_id == tender_id)
        .order_by(SubmissionChecklistItem.sort_order)
    )
    items = items_result.scalars().all()

    if not items:
        raise HTTPException(
            status_code=400,
            detail="No checklist items found. Generate the checklist first.",
        )

    items_dicts = [_item_to_dict(i) for i in items]

    result = await analyze_checklist_requirements(
        db=db,
        tenant_id=current_user.tenant_id,
        tender=tender_dict,
        tender_requirements=requirements,
        checklist_items=items_dicts,
    )

    # Count stats
    analysis = result.get("analysis", [])
    relevant_count = sum(1 for a in analysis if a.get("relevant", True))
    required_count = sum(1 for a in analysis if a.get("is_required", False))
    irrelevant_count = len(analysis) - relevant_count

    return {
        "tender_id": str(tender_id),
        "total_items": len(analysis),
        "relevant_count": relevant_count,
        "required_count": required_count,
        "irrelevant_count": irrelevant_count,
        "updated_count": result.get("updated_count", 0),
        "analysis": analysis,
        "summary": result.get("summary", ""),
    }


class RemoveIrrelevantRequest(BaseModel):
    item_ids: List[str]


@router.post("/{tender_id}/submission-checklist/remove-irrelevant")
async def remove_irrelevant(
    tender_id: UUID,
    data: RemoveIrrelevantRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Remove checklist items marked as irrelevant by AI analysis."""
    if not data.item_ids:
        return {"removed": 0, "message": "No items to remove"}

    removed = await remove_irrelevant_items(db, tender_id, data.item_ids)
    return {
        "removed": removed,
        "message": f"{removed} irrelevant item(s) removed from checklist",
    }


@router.post("/{tender_id}/submission-checklist/bulk", status_code=status.HTTP_201_CREATED)
async def bulk_create_checklist_items(
    tender_id: UUID,
    items: List[ChecklistItemCreate],
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Bulk create checklist items (e.g., from AI extraction)."""
    created = []
    for i, data in enumerate(items):
        notarization_status = "pending" if data.notarization_required else "not_required"
        item = SubmissionChecklistItem(
            tender_id=tender_id,
            document_name=data.document_name,
            document_category=data.document_category,
            submission_mode=data.submission_mode,
            is_critical=data.is_critical,
            envelope=data.envelope,
            cover_name=data.cover_name,
            online_status=data.online_status,
            offline_status=data.offline_status,
            notarization_required=data.notarization_required,
            notarization_type=data.notarization_type,
            notarization_status=notarization_status,
            document_origin=data.document_origin,
            format_reference=data.format_reference,
            obtaining_source=data.obtaining_source,
            can_auto_generate=data.can_auto_generate,
            sort_order=data.sort_order or i,
            source=data.source,
        )
        db.add(item)
        created.append(item)
    await db.flush()
    return {"created": len(created)}


@router.get("/{tender_id}/submission-checklist/summary")
async def get_checklist_summary(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get dual-mode readiness summary with cover packaging breakdown."""
    return await compute_checklist_summary(db, tender_id)


@router.get("/{tender_id}/submission-checklist/templates")
async def get_available_templates(
    tender_id: UUID,
    current_user: TokenData = Depends(get_current_user),
):
    """Get all available standard templates (for preview before generating)."""
    return get_all_templates()


@router.get("/{tender_id}/submission-checklist/covers")
async def get_cover_packaging(
    tender_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Get document packaging by cover/envelope — shows which documents go in which cover."""
    summary = await compute_checklist_summary(db, tender_id)
    return summary.get("by_cover", [])


@router.put("/submission-checklist/{item_id}")
async def update_checklist_item(
    item_id: UUID,
    data: ChecklistItemUpdate,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Update a checklist item (status change, link document, change cover, etc.)."""
    result = await db.execute(
        select(SubmissionChecklistItem).where(SubmissionChecklistItem.id == item_id)
    )
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="Checklist item not found")

    update_data = data.model_dump(exclude_unset=True)
    if "linked_document_id" in update_data and update_data["linked_document_id"]:
        update_data["linked_document_id"] = UUID(update_data["linked_document_id"])
    if "due_date" in update_data and update_data["due_date"]:
        update_data["due_date"] = datetime.strptime(update_data["due_date"], "%Y-%m-%d").date()
    # Auto-set notarization_status when notarization_required changes
    if "notarization_required" in update_data:
        if not update_data["notarization_required"]:
            update_data["notarization_status"] = "not_required"
        elif item.notarization_status == "not_required":
            update_data["notarization_status"] = "pending"

    for key, value in update_data.items():
        setattr(item, key, value)
    await db.flush()
    return _item_to_dict(item)


@router.delete("/submission-checklist/{item_id}", status_code=status.HTTP_204_NO_CONTENT)
async def delete_checklist_item(
    item_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Delete a checklist item."""
    result = await db.execute(
        select(SubmissionChecklistItem).where(SubmissionChecklistItem.id == item_id)
    )
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="Checklist item not found")
    await db.delete(item)


# ---------------------------------------------------------------------------
# Document Staging (Link document to checklist item)
# ---------------------------------------------------------------------------

class LinkDocumentRequest(BaseModel):
    document_id: str


@router.post("/submission-checklist/{item_id}/link-document")
async def link_document_to_item(
    item_id: UUID,
    data: LinkDocumentRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Stage a document by linking it to a checklist item.

    This links an existing document from the document library to a
    checklist item, effectively staging it for cover assembly.
    """
    # Verify checklist item exists
    result = await db.execute(
        select(SubmissionChecklistItem).where(SubmissionChecklistItem.id == item_id)
    )
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="Checklist item not found")

    # Verify document exists and user has access
    doc_result = await db.execute(
        select(Document).where(
            Document.id == UUID(data.document_id),
            Document.tenant_id == UUID(current_user.tenant_id)
        )
    )
    doc = doc_result.scalars().first()
    if not doc:
        raise HTTPException(status_code=404, detail="Document not found or access denied")

    item.linked_document_id = UUID(data.document_id)
    await db.flush()

    return {
        **_item_to_dict(item),
        "linked_document_name": doc.name,
        "linked_document_type": doc.mime_type,
    }


@router.delete("/submission-checklist/{item_id}/link-document")
async def unlink_document_from_item(
    item_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Unstage a document by removing its link from a checklist item."""
    result = await db.execute(
        select(SubmissionChecklistItem).where(SubmissionChecklistItem.id == item_id)
    )
    item = result.scalars().first()
    if not item:
        raise HTTPException(status_code=404, detail="Checklist item not found")

    item.linked_document_id = None
    await db.flush()
    return _item_to_dict(item)


@router.post("/submission-checklist/{item_id}/bulk-link")
async def bulk_link_documents(
    item_id: UUID,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """[Placeholder] Auto-link documents to checklist items by name matching."""
    # Get all unlinked items
    result = await db.execute(
        select(SubmissionChecklistItem)
        .where(
            SubmissionChecklistItem.tender_id == (
                select(SubmissionChecklistItem.tender_id)
                .where(SubmissionChecklistItem.id == item_id)
                .scalar_subquery()
            ),
            SubmissionChecklistItem.linked_document_id.is_(None),
        )
    )
    items = result.scalars().all()

    # Get all available documents
    doc_result = await db.execute(
        select(Document).where(Document.tenant_id == UUID(current_user.tenant_id))
    )
    documents = doc_result.scalars().all()

    linked = 0
    for item in items:
        item_name_lower = item.document_name.lower()
        for doc in documents:
            doc_name_lower = doc.name.lower()
            # Simple heuristic: match if doc name contains key words from checklist item
            words = [w for w in item_name_lower.split() if len(w) > 3]
            match_count = sum(1 for w in words if w in doc_name_lower)
            if match_count >= 2:
                item.linked_document_id = doc.id
                linked += 1
                break

    if linked > 0:
        await db.flush()

    return {"linked": linked, "total_unlinked": len(items)}


# ---------------------------------------------------------------------------
# Cover Assembly — Merge staged documents into cover PDFs
# ---------------------------------------------------------------------------

@router.post("/{tender_id}/submission-checklist/covers/assemble")
async def assemble_single_cover(
    tender_id: UUID,
    cover_name: str = Query(..., description="Name of the cover to assemble"),
    include_title_page: bool = Query(True, description="Include a title page with document index"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Assemble all staged documents in a specific cover into a single PDF.

    Generates a cover title page with document index, then merges
    all linked PDF documents in sort order.
    """
    try:
        result = await cover_assembly_service.assemble_cover(
            db, tender_id, cover_name, current_user, include_title_page
        )
        return {
            "success": True,
            **result,
            "message": f"Cover '{cover_name}' assembled with {result['document_count']} document(s)",
        }
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


@router.post("/{tender_id}/submission-checklist/covers/assemble-all")
async def assemble_all_covers(
    tender_id: UUID,
    include_title_pages: bool = Query(True, description="Include title pages for each cover"),
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """Assemble all covers into a ZIP package with one PDF per cover.

    Each cover PDF includes a title page with document index followed
    by all staged documents. The ZIP contains all cover PDFs.
    """
    try:
        result = await cover_assembly_service.assemble_all_covers(
            db, tender_id, current_user, include_title_pages
        )
        return {
            "success": True,
            **result,
            "message": f"Bid package assembled with {len(result['covers'])} cover(s)",
        }
    except ValueError as e:
        raise HTTPException(status_code=400, detail=str(e))
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

def _item_to_dict(item: SubmissionChecklistItem) -> dict:
    return {
        "id": str(item.id),
        "tender_id": str(item.tender_id),
        "document_name": item.document_name,
        "document_category": item.document_category,
        "submission_mode": item.submission_mode,
        "is_critical": item.is_critical,
        "envelope": item.envelope,
        "cover_name": item.cover_name,
        "online_status": item.online_status,
        "offline_status": item.offline_status,
        "notarization_required": item.notarization_required,
        "notarization_type": item.notarization_type,
        "notarization_status": item.notarization_status,
        "document_origin": item.document_origin,
        "format_reference": item.format_reference,
        "obtaining_source": item.obtaining_source,
        "can_auto_generate": item.can_auto_generate,
        "linked_document_id": str(item.linked_document_id) if item.linked_document_id else None,
        "due_date": item.due_date.isoformat() if item.due_date else None,
        "notes": item.notes,
        "sort_order": item.sort_order,
        "source": item.source,
        "created_at": item.created_at.isoformat() if item.created_at else None,
        "updated_at": item.updated_at.isoformat() if item.updated_at else None,
    }
