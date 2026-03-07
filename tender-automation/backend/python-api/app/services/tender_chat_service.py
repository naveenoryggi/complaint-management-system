"""Tender AI Chat service — context-aware per-tender conversations."""
import logging
from uuid import UUID
from datetime import datetime

from sqlalchemy import select, func, delete
from sqlalchemy.ext.asyncio import AsyncSession

from app.models.tender import Tender
from app.models.tender_chat import TenderChatMessage
from app.services.checklist_service import compute_checklist_summary
from app.services.ai_provider_service import send_message
from app.core.security import TokenData

logger = logging.getLogger(__name__)

MAX_HISTORY_MESSAGES = 15


async def _assemble_context(db: AsyncSession, tender_id: UUID) -> str:
    """Build a system-level context string from the tender record and checklist."""
    result = await db.execute(select(Tender).where(Tender.id == tender_id))
    tender = result.scalar_one_or_none()

    if not tender:
        return "No tender data available."

    parts = [
        f"TENDER CONTEXT:",
        f"- Title: {tender.title}",
        f"- Reference: {tender.reference_number or 'N/A'}",
        f"- Issuing Authority: {tender.issuing_authority or 'N/A'}",
        f"- Portal: {tender.portal_name or 'N/A'}",
        f"- Deadline: {tender.deadline.isoformat() if tender.deadline else 'Not set'}",
        f"- Status: {tender.status}",
        f"- Estimated Value: {tender.estimated_value or 'N/A'}",
    ]

    # Add requirements (truncated)
    if tender.requirements:
        req_text = tender.requirements[:2000]
        parts.append(f"- Key Requirements: {req_text}")

    # Add checklist summary if available
    try:
        summary = await compute_checklist_summary(db, tender_id)
        if summary["total"] > 0:
            online_pct = summary["online"]["percentage"]
            offline_pct = summary["offline"]["percentage"]
            parts.append(f"- Submission Readiness: Online {online_pct}%, Offline {offline_pct}%")
            parts.append(f"- Critical Items Pending: {summary['critical_pending']}")
            parts.append(f"- Notarization Pending: {summary['notarization_pending']}")
    except Exception:
        pass

    return "\n".join(parts)


async def send_chat_message(
    db: AsyncSession,
    tender_id: UUID,
    user_message: str,
    current_user: TokenData,
) -> dict:
    """Process a user chat message and get AI response.

    Returns dict with user_message and assistant_message.
    """
    tenant_id = UUID(current_user.tenant_id)
    user_id = UUID(current_user.user_id)
    user_name = getattr(current_user, "user_name", None) or "User"

    # 1. Save user message
    user_msg = TenderChatMessage(
        tender_id=tender_id,
        tenant_id=tenant_id,
        role="user",
        content=user_message,
        user_id=user_id,
        user_name=user_name,
    )
    db.add(user_msg)
    await db.flush()

    # 2. Load recent chat history
    history_result = await db.execute(
        select(TenderChatMessage)
        .where(TenderChatMessage.tender_id == tender_id, TenderChatMessage.tenant_id == tenant_id)
        .order_by(TenderChatMessage.created_at.desc())
        .limit(MAX_HISTORY_MESSAGES)
    )
    recent_messages = list(reversed(history_result.scalars().all()))

    # 3. Build prompt with context
    context = await _assemble_context(db, tender_id)

    system_prompt = (
        "You are an AI Tender Assistant. You help users prepare government and corporate tenders. "
        "Answer questions about this specific tender using the context below. "
        "Be concise, practical, and specific to Indian tendering practices (GFR 2017, GeM, CPPP). "
        "If you don't know something from the context, say so.\n\n"
        f"{context}"
    )

    # Build conversation history string
    conversation_parts = []
    for msg in recent_messages:
        role_label = "User" if msg.role == "user" else "Assistant"
        conversation_parts.append(f"{role_label}: {msg.content}")

    full_prompt = (
        f"{system_prompt}\n\nCONVERSATION:\n"
        + "\n".join(conversation_parts)
        + "\nAssistant:"
    )

    # 4. Call AI
    try:
        content, total_tokens, model_used = await send_message(
            db=db,
            tenant_id=current_user.tenant_id,
            prompt=full_prompt,
            feature="tender_chat",
            max_tokens=1024,
            temperature=0.3,
        )
    except Exception as e:
        logger.error(f"Chat AI call failed: {e}")
        content = "I'm sorry, I couldn't process your request right now. Please try again later."
        total_tokens = 0
        model_used = "fallback"

    # 5. Save assistant message
    assistant_msg = TenderChatMessage(
        tender_id=tender_id,
        tenant_id=tenant_id,
        role="assistant",
        content=content,
        tokens_used=total_tokens,
        model_used=model_used,
        user_id=user_id,
        user_name="AI Assistant",
    )
    db.add(assistant_msg)
    await db.flush()

    return {
        "user_message": _msg_to_dict(user_msg),
        "assistant_message": _msg_to_dict(assistant_msg),
    }


async def get_chat_history(
    db: AsyncSession,
    tender_id: UUID,
    tenant_id: UUID,
    page: int = 1,
    page_size: int = 50,
) -> dict:
    """Get paginated chat history for a tender."""
    offset = (page - 1) * page_size

    # Count total
    count_result = await db.execute(
        select(func.count(TenderChatMessage.id))
        .where(TenderChatMessage.tender_id == tender_id, TenderChatMessage.tenant_id == tenant_id)
    )
    total = count_result.scalar_one()

    # Fetch messages
    result = await db.execute(
        select(TenderChatMessage)
        .where(TenderChatMessage.tender_id == tender_id, TenderChatMessage.tenant_id == tenant_id)
        .order_by(TenderChatMessage.created_at.asc())
        .offset(offset)
        .limit(page_size)
    )
    messages = result.scalars().all()

    return {
        "messages": [_msg_to_dict(m) for m in messages],
        "total": total,
        "page": page,
        "page_size": page_size,
    }


async def clear_chat(db: AsyncSession, tender_id: UUID, tenant_id: UUID) -> dict:
    """Delete all chat messages for a tender."""
    result = await db.execute(
        delete(TenderChatMessage).where(
            TenderChatMessage.tender_id == tender_id,
            TenderChatMessage.tenant_id == tenant_id,
        )
    )
    deleted = result.rowcount
    return {"deleted": deleted}


def _msg_to_dict(msg: TenderChatMessage) -> dict:
    return {
        "id": str(msg.id),
        "tender_id": str(msg.tender_id),
        "role": msg.role,
        "content": msg.content,
        "tokens_used": msg.tokens_used,
        "model_used": msg.model_used,
        "user_name": msg.user_name,
        "created_at": msg.created_at.isoformat() if msg.created_at else None,
    }
