"""Collaboration service — team assignments and threaded comments."""
from uuid import UUID
from datetime import datetime

from sqlalchemy import select, and_, desc, delete
from sqlalchemy.ext.asyncio import AsyncSession
from fastapi import HTTPException

from app.models.tender_collaboration import TenderAssignment, TenderComment


# ---------------------------------------------------------------------------
# Assignments
# ---------------------------------------------------------------------------

async def list_assignments(
    db: AsyncSession, tender_id: str, tenant_id: str
) -> list[dict]:
    result = await db.execute(
        select(TenderAssignment)
        .where(
            and_(
                TenderAssignment.tender_id == UUID(tender_id),
                TenderAssignment.tenant_id == UUID(tenant_id),
            )
        )
        .order_by(TenderAssignment.assigned_at)
    )
    return [
        {
            "id": str(a.id),
            "tender_id": str(a.tender_id),
            "user_id": str(a.user_id),
            "user_name": a.user_name,
            "role": a.role,
            "assigned_by": str(a.assigned_by),
            "assigned_at": str(a.assigned_at),
        }
        for a in result.scalars().all()
    ]


async def assign_user(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
    user_id: str,
    user_name: str,
    role: str,
    assigned_by: str,
) -> dict:
    assignment = TenderAssignment(
        tender_id=UUID(tender_id),
        tenant_id=UUID(tenant_id),
        user_id=UUID(user_id),
        user_name=user_name,
        role=role,
        assigned_by=UUID(assigned_by),
    )
    db.add(assignment)
    await db.flush()
    return {
        "id": str(assignment.id),
        "tender_id": tender_id,
        "user_id": user_id,
        "user_name": user_name,
        "role": role,
        "assigned_at": str(assignment.assigned_at),
    }


async def unassign_user(
    db: AsyncSession, tender_id: str, tenant_id: str, user_id: str
) -> dict:
    await db.execute(
        delete(TenderAssignment).where(
            and_(
                TenderAssignment.tender_id == UUID(tender_id),
                TenderAssignment.tenant_id == UUID(tenant_id),
                TenderAssignment.user_id == UUID(user_id),
            )
        )
    )
    await db.flush()
    return {"removed": True}


# ---------------------------------------------------------------------------
# Comments
# ---------------------------------------------------------------------------

async def list_comments(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
    page: int = 1,
    page_size: int = 50,
) -> dict:
    query = (
        select(TenderComment)
        .where(
            and_(
                TenderComment.tender_id == UUID(tender_id),
                TenderComment.tenant_id == UUID(tenant_id),
            )
        )
        .order_by(desc(TenderComment.created_at))
    )

    from sqlalchemy import func
    count_q = select(func.count()).select_from(query.subquery())
    total_result = await db.execute(count_q)
    total = total_result.scalar() or 0

    result = await db.execute(
        query.offset((page - 1) * page_size).limit(page_size)
    )
    comments = result.scalars().all()

    return {
        "items": [
            {
                "id": str(c.id),
                "tender_id": str(c.tender_id),
                "user_id": str(c.user_id),
                "user_name": c.user_name,
                "comment_text": c.comment_text,
                "parent_id": str(c.parent_id) if c.parent_id else None,
                "created_at": str(c.created_at),
                "updated_at": str(c.updated_at),
            }
            for c in comments
        ],
        "total": total,
        "page": page,
        "page_size": page_size,
    }


async def create_comment(
    db: AsyncSession,
    tender_id: str,
    tenant_id: str,
    user_id: str,
    user_name: str,
    comment_text: str,
    parent_id: str | None = None,
) -> dict:
    comment = TenderComment(
        tender_id=UUID(tender_id),
        tenant_id=UUID(tenant_id),
        user_id=UUID(user_id),
        user_name=user_name,
        comment_text=comment_text,
        parent_id=UUID(parent_id) if parent_id else None,
    )
    db.add(comment)
    await db.flush()
    return {
        "id": str(comment.id),
        "tender_id": tender_id,
        "user_id": user_id,
        "user_name": user_name,
        "comment_text": comment_text,
        "parent_id": parent_id,
        "created_at": str(comment.created_at),
    }


async def update_comment(
    db: AsyncSession,
    comment_id: str,
    tenant_id: str,
    user_id: str,
    comment_text: str,
) -> dict:
    result = await db.execute(
        select(TenderComment).where(
            and_(
                TenderComment.id == UUID(comment_id),
                TenderComment.tenant_id == UUID(tenant_id),
                TenderComment.user_id == UUID(user_id),
            )
        )
    )
    comment = result.scalar_one_or_none()
    if not comment:
        raise HTTPException(status_code=404, detail="Comment not found or not authorized")

    comment.comment_text = comment_text
    comment.updated_at = datetime.utcnow()
    await db.flush()
    return {
        "id": str(comment.id),
        "comment_text": comment_text,
        "updated_at": str(comment.updated_at),
    }


async def delete_comment(
    db: AsyncSession, comment_id: str, tenant_id: str, user_id: str
) -> dict:
    result = await db.execute(
        select(TenderComment).where(
            and_(
                TenderComment.id == UUID(comment_id),
                TenderComment.tenant_id == UUID(tenant_id),
                TenderComment.user_id == UUID(user_id),
            )
        )
    )
    comment = result.scalar_one_or_none()
    if not comment:
        raise HTTPException(status_code=404, detail="Comment not found or not authorized")

    await db.delete(comment)
    await db.flush()
    return {"deleted": True}


async def get_collaboration_summary(
    db: AsyncSession, tender_id: str, tenant_id: str
) -> dict:
    from sqlalchemy import func

    assignment_count = await db.execute(
        select(func.count()).where(
            and_(
                TenderAssignment.tender_id == UUID(tender_id),
                TenderAssignment.tenant_id == UUID(tenant_id),
            )
        )
    )

    comment_count = await db.execute(
        select(func.count()).where(
            and_(
                TenderComment.tender_id == UUID(tender_id),
                TenderComment.tenant_id == UUID(tenant_id),
            )
        )
    )

    return {
        "assignment_count": assignment_count.scalar() or 0,
        "comment_count": comment_count.scalar() or 0,
    }
