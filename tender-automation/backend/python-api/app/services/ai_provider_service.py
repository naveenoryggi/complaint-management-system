"""AI Provider factory service — multi-provider abstraction with encrypted key storage.

Resolution order for choosing a provider:
  1. Provider with feature_mapping[feature]=true and is_active=true
  2. Default provider (is_default=true)
  3. Any active provider
  4. Fallback to settings.anthropic_api_key from .env (backward compat)
"""
import json
import logging
import time
from datetime import datetime, timezone
from typing import Optional
from uuid import UUID

from sqlalchemy import select, update
from sqlalchemy.ext.asyncio import AsyncSession

from app.core.config import settings
from app.core.encryption import encrypt_value, decrypt_value
from app.models.ai_provider import AIProviderConfig

logger = logging.getLogger(__name__)

# Valid provider names
VALID_PROVIDERS = {"anthropic", "openai", "google", "azure_openai"}

# Valid feature keys
VALID_FEATURES = {
    "extraction", "declarations", "prebid", "document_generation", "alignment",
    "tender_chat",
}


# ---------------------------------------------------------------------------
# Core send_message — provider-agnostic AI call
# ---------------------------------------------------------------------------

async def send_message(
    db: AsyncSession,
    tenant_id: str,
    prompt: str,
    feature: str = "extraction",
    model: Optional[str] = None,
    max_tokens: int = 4096,
    temperature: float = 0,
) -> tuple[str, int, str]:
    """Send a prompt to the best available AI provider and return the response.

    Args:
        db: Async database session
        tenant_id: Tenant UUID string
        prompt: The full prompt to send
        feature: Feature key for provider routing
        model: Override model (uses provider's default_model if None)
        max_tokens: Max output tokens
        temperature: Sampling temperature

    Returns:
        Tuple of (response_text, tokens_used, model_used)
    """
    provider = await _resolve_provider(db, tenant_id, feature)

    if provider:
        api_key = decrypt_value(provider.api_key_encrypted)
        use_model = model or provider.default_model

        text, tokens = await _call_provider(
            provider_name=provider.provider_name,
            api_key=api_key,
            prompt=prompt,
            model=use_model,
            max_tokens=max_tokens,
            temperature=temperature,
            endpoint_url=provider.endpoint_url,
            deployment_name=provider.deployment_name,
            api_version=provider.api_version,
        )

        # Update usage stats (fire and forget — don't block on it)
        try:
            await db.execute(
                update(AIProviderConfig)
                .where(AIProviderConfig.id == provider.id)
                .values(
                    total_tokens_used=AIProviderConfig.total_tokens_used + tokens,
                    total_requests=AIProviderConfig.total_requests + 1,
                    last_used_at=datetime.now(timezone.utc),
                )
            )
        except Exception:
            pass  # Non-critical

        return text, tokens, use_model

    # Fallback to .env anthropic key
    if settings.anthropic_api_key:
        logger.info("No DB provider found — falling back to .env anthropic_api_key")
        fallback_model = model or "claude-sonnet-4-6"
        text, tokens = await _call_provider(
            provider_name="anthropic",
            api_key=settings.anthropic_api_key,
            prompt=prompt,
            model=fallback_model,
            max_tokens=max_tokens,
            temperature=temperature,
        )
        return text, tokens, fallback_model

    raise ValueError("No AI provider configured. Add a provider in AI Provider Settings or set ANTHROPIC_API_KEY in .env")


# ---------------------------------------------------------------------------
# Native document support (PDF → Claude)
# ---------------------------------------------------------------------------

async def get_provider_name(db: AsyncSession, tenant_id: str, feature: str) -> str:
    """Return the provider_name that would be used for a given feature."""
    provider = await _resolve_provider(db, tenant_id, feature)
    if provider:
        return provider.provider_name
    if settings.anthropic_api_key:
        return "anthropic"
    return "none"


async def send_message_with_documents(
    db: AsyncSession,
    tenant_id: str,
    prompt: str,
    documents: list[dict],
    feature: str = "extraction",
    model: Optional[str] = None,
    max_tokens: int = 8192,
    temperature: float = 0,
) -> tuple[str, int, str]:
    """Send a prompt with document attachments to the AI provider.

    Only Anthropic supports native PDF documents. For other providers this
    falls back to text-based send_message() (caller must handle text extraction).

    Args:
        documents: List of dicts with keys 'data' (base64 string) and 'media_type'.

    Returns:
        Tuple of (response_text, tokens_used, model_used)
    """
    provider = await _resolve_provider(db, tenant_id, feature)

    if provider:
        api_key = decrypt_value(provider.api_key_encrypted)
        use_model = model or provider.default_model

        if provider.provider_name != "anthropic":
            raise ValueError(
                "Native document support requires an Anthropic provider. "
                "Use send_message() with text extraction for other providers."
            )

        text, tokens = await _call_anthropic_with_documents(
            api_key=api_key,
            prompt=prompt,
            documents=documents,
            model=use_model,
            max_tokens=max_tokens,
            temperature=temperature,
        )

        # Update usage stats
        try:
            await db.execute(
                update(AIProviderConfig)
                .where(AIProviderConfig.id == provider.id)
                .values(
                    total_tokens_used=AIProviderConfig.total_tokens_used + tokens,
                    total_requests=AIProviderConfig.total_requests + 1,
                    last_used_at=datetime.now(timezone.utc),
                )
            )
        except Exception:
            pass

        return text, tokens, use_model

    # Fallback to .env anthropic key
    if settings.anthropic_api_key:
        logger.info("No DB provider found — falling back to .env anthropic_api_key for document extraction")
        fallback_model = model or "claude-sonnet-4-6"
        text, tokens = await _call_anthropic_with_documents(
            api_key=settings.anthropic_api_key,
            prompt=prompt,
            documents=documents,
            model=fallback_model,
            max_tokens=max_tokens,
            temperature=temperature,
        )
        return text, tokens, fallback_model

    raise ValueError("No Anthropic provider configured for native document extraction.")


async def _call_anthropic_with_documents(
    api_key: str,
    prompt: str,
    documents: list[dict],
    model: str,
    max_tokens: int,
    temperature: float,
) -> tuple[str, int]:
    """Call Anthropic Messages API with native PDF document blocks."""
    import anthropic

    client = anthropic.Anthropic(api_key=api_key)

    content = []
    for doc in documents:
        content.append({
            "type": "document",
            "source": {
                "type": "base64",
                "media_type": doc["media_type"],
                "data": doc["data"],
            },
        })
    content.append({"type": "text", "text": prompt})

    message = client.messages.create(
        model=model,
        max_tokens=max_tokens,
        temperature=temperature,
        messages=[{"role": "user", "content": content}],
    )
    text = message.content[0].text
    tokens = message.usage.input_tokens + message.usage.output_tokens
    return text, tokens


# ---------------------------------------------------------------------------
# Provider resolution
# ---------------------------------------------------------------------------

async def _resolve_provider(
    db: AsyncSession,
    tenant_id: str,
    feature: str,
) -> Optional[AIProviderConfig]:
    """Find the best provider for a given feature + tenant."""
    tid = UUID(tenant_id)

    # 1. Feature-mapped provider
    result = await db.execute(
        select(AIProviderConfig).where(
            AIProviderConfig.tenant_id == tid,
            AIProviderConfig.is_active == True,
        )
    )
    providers = result.scalars().all()

    for p in providers:
        mapping = _parse_feature_mapping(p.feature_mapping)
        if mapping.get(feature):
            return p

    # 2. Default provider
    for p in providers:
        if p.is_default:
            return p

    # 3. Any active
    if providers:
        return providers[0]

    return None


def _parse_feature_mapping(raw: Optional[str]) -> dict:
    """Parse feature_mapping JSON string to dict."""
    if not raw:
        return {}
    try:
        return json.loads(raw)
    except (json.JSONDecodeError, TypeError):
        return {}


# ---------------------------------------------------------------------------
# Provider-specific call dispatcher
# ---------------------------------------------------------------------------

async def _call_provider(
    provider_name: str,
    api_key: str,
    prompt: str,
    model: str,
    max_tokens: int,
    temperature: float,
    endpoint_url: Optional[str] = None,
    deployment_name: Optional[str] = None,
    api_version: Optional[str] = None,
) -> tuple[str, int]:
    """Dispatch to the correct provider SDK and return (text, tokens_used)."""

    if provider_name == "anthropic":
        return await _call_anthropic(api_key, prompt, model, max_tokens, temperature)
    elif provider_name == "openai":
        return await _call_openai(api_key, prompt, model, max_tokens, temperature)
    elif provider_name == "google":
        return await _call_google(api_key, prompt, model, max_tokens, temperature)
    elif provider_name == "azure_openai":
        return await _call_azure_openai(
            api_key, prompt, model, max_tokens, temperature,
            endpoint_url, deployment_name, api_version,
        )
    else:
        raise ValueError(f"Unknown provider: {provider_name}")


async def _call_anthropic(
    api_key: str, prompt: str, model: str, max_tokens: int, temperature: float,
) -> tuple[str, int]:
    import anthropic
    client = anthropic.Anthropic(api_key=api_key)
    message = client.messages.create(
        model=model,
        max_tokens=max_tokens,
        temperature=temperature,
        messages=[{"role": "user", "content": prompt}],
    )
    text = message.content[0].text
    tokens = message.usage.input_tokens + message.usage.output_tokens
    return text, tokens


async def _call_openai(
    api_key: str, prompt: str, model: str, max_tokens: int, temperature: float,
) -> tuple[str, int]:
    from openai import OpenAI
    client = OpenAI(api_key=api_key)
    response = client.chat.completions.create(
        model=model,
        max_tokens=max_tokens,
        temperature=temperature,
        messages=[{"role": "user", "content": prompt}],
    )
    text = response.choices[0].message.content or ""
    tokens = (response.usage.prompt_tokens + response.usage.completion_tokens) if response.usage else 0
    return text, tokens


async def _call_google(
    api_key: str, prompt: str, model: str, max_tokens: int, temperature: float,
) -> tuple[str, int]:
    import google.generativeai as genai
    genai.configure(api_key=api_key)
    gen_model = genai.GenerativeModel(model)
    response = gen_model.generate_content(
        prompt,
        generation_config=genai.types.GenerationConfig(
            max_output_tokens=max_tokens,
            temperature=temperature,
        ),
    )
    text = response.text or ""
    tokens = 0
    if hasattr(response, "usage_metadata") and response.usage_metadata:
        tokens = (
            getattr(response.usage_metadata, "prompt_token_count", 0)
            + getattr(response.usage_metadata, "candidates_token_count", 0)
        )
    return text, tokens


async def _call_azure_openai(
    api_key: str, prompt: str, model: str, max_tokens: int, temperature: float,
    endpoint_url: Optional[str] = None,
    deployment_name: Optional[str] = None,
    api_version: Optional[str] = None,
) -> tuple[str, int]:
    from openai import AzureOpenAI
    client = AzureOpenAI(
        api_key=api_key,
        azure_endpoint=endpoint_url or "",
        api_version=api_version or "2024-02-01",
    )
    response = client.chat.completions.create(
        model=deployment_name or model,
        max_tokens=max_tokens,
        temperature=temperature,
        messages=[{"role": "user", "content": prompt}],
    )
    text = response.choices[0].message.content or ""
    tokens = (response.usage.prompt_tokens + response.usage.completion_tokens) if response.usage else 0
    return text, tokens


# ---------------------------------------------------------------------------
# CRUD operations
# ---------------------------------------------------------------------------

async def list_providers(db: AsyncSession, tenant_id: str) -> list[dict]:
    """List all providers for a tenant (API keys masked)."""
    result = await db.execute(
        select(AIProviderConfig)
        .where(AIProviderConfig.tenant_id == UUID(tenant_id))
        .order_by(AIProviderConfig.is_default.desc(), AIProviderConfig.created_at)
    )
    providers = result.scalars().all()
    return [_provider_to_dict(p) for p in providers]


async def get_provider(db: AsyncSession, provider_id: UUID, tenant_id: str) -> Optional[dict]:
    """Get a single provider (masked key)."""
    result = await db.execute(
        select(AIProviderConfig).where(
            AIProviderConfig.id == provider_id,
            AIProviderConfig.tenant_id == UUID(tenant_id),
        )
    )
    provider = result.scalar_one_or_none()
    return _provider_to_dict(provider) if provider else None


async def create_provider(db: AsyncSession, tenant_id: str, data: dict) -> dict:
    """Create a new AI provider config with encrypted API key."""
    provider = AIProviderConfig(
        tenant_id=UUID(tenant_id),
        provider_name=data["provider_name"],
        display_name=data["display_name"],
        api_key_encrypted=encrypt_value(data["api_key"]),
        endpoint_url=data.get("endpoint_url"),
        deployment_name=data.get("deployment_name"),
        api_version=data.get("api_version"),
        default_model=data["default_model"],
        feature_mapping=json.dumps(data.get("feature_mapping", {})),
        is_active=data.get("is_active", True),
        is_default=data.get("is_default", False),
        created_by=UUID(data["created_by"]) if data.get("created_by") else None,
    )

    # If setting as default, clear other defaults
    if provider.is_default:
        await _clear_defaults(db, tenant_id)

    db.add(provider)
    await db.flush()
    await db.refresh(provider)
    return _provider_to_dict(provider)


async def update_provider(db: AsyncSession, provider_id: UUID, tenant_id: str, data: dict) -> Optional[dict]:
    """Update an existing provider. Re-encrypts key if changed."""
    result = await db.execute(
        select(AIProviderConfig).where(
            AIProviderConfig.id == provider_id,
            AIProviderConfig.tenant_id == UUID(tenant_id),
        )
    )
    provider = result.scalar_one_or_none()
    if not provider:
        return None

    if "display_name" in data:
        provider.display_name = data["display_name"]
    if "api_key" in data and data["api_key"]:
        provider.api_key_encrypted = encrypt_value(data["api_key"])
    if "endpoint_url" in data:
        provider.endpoint_url = data["endpoint_url"]
    if "deployment_name" in data:
        provider.deployment_name = data["deployment_name"]
    if "api_version" in data:
        provider.api_version = data["api_version"]
    if "default_model" in data:
        provider.default_model = data["default_model"]
    if "feature_mapping" in data:
        provider.feature_mapping = json.dumps(data["feature_mapping"])
    if "is_active" in data:
        provider.is_active = data["is_active"]
    if "is_default" in data:
        if data["is_default"]:
            await _clear_defaults(db, tenant_id)
        provider.is_default = data["is_default"]

    provider.updated_at = datetime.now(timezone.utc)
    await db.flush()
    await db.refresh(provider)
    return _provider_to_dict(provider)


async def delete_provider(db: AsyncSession, provider_id: UUID, tenant_id: str) -> bool:
    """Delete a provider config."""
    result = await db.execute(
        select(AIProviderConfig).where(
            AIProviderConfig.id == provider_id,
            AIProviderConfig.tenant_id == UUID(tenant_id),
        )
    )
    provider = result.scalar_one_or_none()
    if not provider:
        return False
    await db.delete(provider)
    await db.flush()
    return True


async def set_default(db: AsyncSession, provider_id: UUID, tenant_id: str) -> Optional[dict]:
    """Set a provider as the tenant default."""
    await _clear_defaults(db, tenant_id)

    result = await db.execute(
        select(AIProviderConfig).where(
            AIProviderConfig.id == provider_id,
            AIProviderConfig.tenant_id == UUID(tenant_id),
        )
    )
    provider = result.scalar_one_or_none()
    if not provider:
        return None

    provider.is_default = True
    provider.updated_at = datetime.now(timezone.utc)
    await db.flush()
    await db.refresh(provider)
    return _provider_to_dict(provider)


async def test_connection(db: AsyncSession, provider_id: UUID, tenant_id: str) -> dict:
    """Test a provider's API key with a minimal request."""
    result = await db.execute(
        select(AIProviderConfig).where(
            AIProviderConfig.id == provider_id,
            AIProviderConfig.tenant_id == UUID(tenant_id),
        )
    )
    provider = result.scalar_one_or_none()
    if not provider:
        return {"success": False, "error": "Provider not found", "latency_ms": 0}

    api_key = decrypt_value(provider.api_key_encrypted)
    start = time.time()

    try:
        text, tokens = await _call_provider(
            provider_name=provider.provider_name,
            api_key=api_key,
            prompt="Reply with exactly: OK",
            model=provider.default_model,
            max_tokens=10,
            temperature=0,
            endpoint_url=provider.endpoint_url,
            deployment_name=provider.deployment_name,
            api_version=provider.api_version,
        )
        latency_ms = int((time.time() - start) * 1000)

        provider.last_test_at = datetime.now(timezone.utc)
        provider.last_test_success = True
        await db.flush()

        return {"success": True, "latency_ms": latency_ms, "response": text.strip()[:100]}

    except Exception as e:
        latency_ms = int((time.time() - start) * 1000)
        provider.last_test_at = datetime.now(timezone.utc)
        provider.last_test_success = False
        await db.flush()
        return {"success": False, "error": str(e)[:300], "latency_ms": latency_ms}


# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

async def _clear_defaults(db: AsyncSession, tenant_id: str):
    """Clear is_default on all providers for a tenant."""
    await db.execute(
        update(AIProviderConfig)
        .where(
            AIProviderConfig.tenant_id == UUID(tenant_id),
            AIProviderConfig.is_default == True,
        )
        .values(is_default=False)
    )


def _mask_key(encrypted_key: str) -> str:
    """Decrypt then mask API key to show only last 4 chars."""
    try:
        key = decrypt_value(encrypted_key)
        if len(key) <= 4:
            return "****"
        return "****" + key[-4:]
    except Exception:
        return "****"


def _provider_to_dict(provider: AIProviderConfig) -> dict:
    """Convert a provider model to a serializable dict with masked key."""
    return {
        "id": str(provider.id),
        "tenant_id": str(provider.tenant_id),
        "provider_name": provider.provider_name,
        "display_name": provider.display_name,
        "api_key_masked": _mask_key(provider.api_key_encrypted),
        "endpoint_url": provider.endpoint_url,
        "deployment_name": provider.deployment_name,
        "api_version": provider.api_version,
        "default_model": provider.default_model,
        "feature_mapping": json.loads(provider.feature_mapping) if provider.feature_mapping else {},
        "is_active": provider.is_active,
        "is_default": provider.is_default,
        "total_tokens_used": provider.total_tokens_used,
        "total_requests": provider.total_requests,
        "last_used_at": provider.last_used_at.isoformat() if provider.last_used_at else None,
        "last_test_at": provider.last_test_at.isoformat() if provider.last_test_at else None,
        "last_test_success": provider.last_test_success,
        "created_at": provider.created_at.isoformat() if provider.created_at else None,
        "updated_at": provider.updated_at.isoformat() if provider.updated_at else None,
    }
