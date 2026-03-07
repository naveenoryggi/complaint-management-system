"""AI-powered tender PDF extraction service.

Uses native PDF document support (Anthropic) or PyPDF2 text extraction
(fallback) to send tender PDFs to AI for structured data extraction.
Extracted data can be previewed and then applied to update tender records.

For large multi-PDF uploads that exceed token limits, documents are
automatically split into batches. Each batch is processed independently,
and the extracted JSON results are merged into a single consolidated output.
"""
import base64
import copy
import json
import logging
import os
from typing import Optional
from uuid import UUID
from datetime import datetime, timezone

from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.models.tender import Tender
from app.models.tracking import EMDRecord, TenderFee
from app.models.reference_bundle import TenderCriteria
from app.models.oem import OEMMaster, OEMTenderRequirement
from app.models.submission_checklist import SubmissionChecklistItem

logger = logging.getLogger(__name__)

EXTRACTION_PROMPT = """You are a government tender document analyst. Extract ALL structured information from the following tender document text and return it as a single JSON object.

Be thorough - extract every detail you can find. If a field is not found in the document, omit it from the response (do not include null values).

Return ONLY valid JSON with these fields:

{
  "title": "Full tender title",
  "reference_number": "Tender/NIT/RFP reference number",
  "issuing_authority": "Organization issuing the tender",
  "deadline": "Submission deadline in ISO format (YYYY-MM-DDTHH:MM:SS)",
  "estimated_value": 0,
  "eligibility_criteria": ["List of eligibility requirements"],
  "technical_requirements": ["List of technical specifications and requirements"],
  "emd": {
    "amount": 0,
    "mode": "bg or dd or online or fixed_deposit or insurance_surety",
    "validity_end_date": "YYYY-MM-DD",
    "msme_exempted": true,
    "exemption_details": "Who is exempt from EMD and under what conditions (e.g., MSME, Startups, DPIIT, NSIC registered)",
    "emd_format": "Specific format requirements if mentioned (e.g., BG format, DD drawn on scheduled bank)",
    "bank_details": {
      "bank_name": "Bank name where EMD is to be submitted",
      "account_number": "Account number if provided",
      "ifsc_code": "IFSC code if mentioned",
      "branch": "Branch name/address",
      "account_holder": "Account holder / beneficiary name",
      "dd_in_favour_of": "If DD, drawn in favour of whom"
    }
  },
  "tender_fees": [
    {
      "fee_type": "tender_fee or processing_fee or document_fee or e_procurement_fee or bid_security",
      "amount": 0,
      "payment_mode": "online or dd or neft or rtgs or challan",
      "msme_exempted": true,
      "exemption_details": "Exemption conditions if any",
      "bank_details": {
        "bank_name": "Bank name for fee payment",
        "account_number": "Account number",
        "ifsc_code": "IFSC code",
        "branch": "Branch name",
        "dd_in_favour_of": "DD drawn in favour of"
      },
      "payment_deadline": "YYYY-MM-DD or description of deadline",
      "non_refundable": true
    }
  ],
  "evaluation_criteria": [
    {"criteria_code": "TC1", "stage": "technical or financial or combined", "max_marks": 0, "description": "What is being evaluated"}
  ],
  "document_checklist": ["List of documents required for submission"],
  "important_dates": {
    "published_date": "YYYY-MM-DD",
    "pre_bid_meeting": "YYYY-MM-DD",
    "clarification_deadline": "YYYY-MM-DD",
    "submission_deadline": "YYYY-MM-DDTHH:MM:SS",
    "technical_opening": "YYYY-MM-DD",
    "financial_opening": "YYYY-MM-DD"
  },
  "contact_info": {
    "name": "Contact person name",
    "designation": "Title/role",
    "email": "Email address",
    "phone": "Phone number",
    "address": "Office address"
  },
  "special_conditions": ["Special terms, conditions, or clauses"],
  "oem_requirements": [
    {"oem_name": "Name of OEM/manufacturer/brand", "product_category": "Product type or line item", "maf_required": true, "make_model": "Specific model if mentioned", "is_approved_make": true}
  ]
}

IMPORTANT:
- Extract ALL OEMs mentioned in the document, not just one
- For amounts, extract numeric values only (no currency symbols)
- For dates, convert to ISO format
- For evaluation criteria, extract the full marking scheme if available
- Include page/section references in descriptions where helpful

EMD & TENDER FEE EXTRACTION GUIDANCE:
- Look for EMD/Earnest Money Deposit section — extract amount, mode, validity, and bank details
- Check if MSME/NSIC/DPIIT/Startup India registered firms are exempted from EMD. Many govt tenders exempt MSMEs
- Extract full bank details: bank name, account number, IFSC, branch, DD in favour of whom
- For BG (Bank Guarantee): note the format requirements, validity period, issuing bank requirements
- For DD (Demand Draft): note "drawn in favour of" and "payable at" details
- Extract ALL fees separately: tender fee, document fee, processing fee, e-procurement fee, bid security
- Note which fees are refundable vs non-refundable
- Check if fees have MSME exemption too (often tender fee is also exempt for MSMEs)
- Look for payment deadlines specific to each fee
- GEM portal tenders may have different fee structures — check for Bid Security Declaration in lieu of EMD

OEM/MANUFACTURER EXTRACTION GUIDANCE:
- Look for "approved makes", "make and model", "brand", "manufacturer" mentions
- GEM tenders often list product specifications with approved brands — extract those as OEMs
- Check BOQ/item tables for brand/make columns
- Look for phrases like "equivalent to", "or equivalent", "make: XYZ", "brand: XYZ"
- MAF (Manufacturer Authorization Form) is required if the tender says "authorized dealer/partner" or "OEM authorization"
- If the tender lists specific product line items with brands, create one oem_requirement per brand
- Even if no specific OEM is named, extract product categories that will need OEM authorization (e.g., "Networking Equipment", "Server Hardware", "UPS Systems")

TENDER DOCUMENT TEXT:
"""

EXTRACTION_PROMPT_NATIVE = """You are a government tender document analyst. Extract ALL structured information from the attached tender PDF document(s) and return it as a single consolidated JSON object.

You may receive multiple PDF documents (NIT, technical specifications, BOQ, corrigendum, amendments, etc.) that together form one complete tender. Merge and consolidate information from ALL documents into a single unified response.

Be thorough - extract every detail you can find including tables, charts, and formatted data. If a field is not found in any document, omit it from the response (do not include null values).

Return ONLY valid JSON with these fields:

{
  "title": "Full tender title",
  "reference_number": "Tender/NIT/RFP reference number",
  "issuing_authority": "Organization issuing the tender",
  "deadline": "Submission deadline in ISO format (YYYY-MM-DDTHH:MM:SS)",
  "estimated_value": 0,
  "eligibility_criteria": ["List of eligibility requirements"],
  "technical_requirements": ["List of technical specifications and requirements"],
  "emd": {
    "amount": 0,
    "mode": "bg or dd or online or fixed_deposit or insurance_surety",
    "validity_end_date": "YYYY-MM-DD",
    "msme_exempted": true,
    "exemption_details": "Who is exempt from EMD and under what conditions (e.g., MSME, Startups, DPIIT, NSIC registered)",
    "emd_format": "Specific format requirements if mentioned (e.g., BG format, DD drawn on scheduled bank)",
    "bank_details": {
      "bank_name": "Bank name where EMD is to be submitted",
      "account_number": "Account number if provided",
      "ifsc_code": "IFSC code if mentioned",
      "branch": "Branch name/address",
      "account_holder": "Account holder / beneficiary name",
      "dd_in_favour_of": "If DD, drawn in favour of whom"
    }
  },
  "tender_fees": [
    {
      "fee_type": "tender_fee or processing_fee or document_fee or e_procurement_fee or bid_security",
      "amount": 0,
      "payment_mode": "online or dd or neft or rtgs or challan",
      "msme_exempted": true,
      "exemption_details": "Exemption conditions if any",
      "bank_details": {
        "bank_name": "Bank name for fee payment",
        "account_number": "Account number",
        "ifsc_code": "IFSC code",
        "branch": "Branch name",
        "dd_in_favour_of": "DD drawn in favour of"
      },
      "payment_deadline": "YYYY-MM-DD or description of deadline",
      "non_refundable": true
    }
  ],
  "evaluation_criteria": [
    {"criteria_code": "TC1", "stage": "technical or financial or combined", "max_marks": 0, "description": "What is being evaluated"}
  ],
  "document_checklist": ["List of documents required for submission"],
  "important_dates": {
    "published_date": "YYYY-MM-DD",
    "pre_bid_meeting": "YYYY-MM-DD",
    "clarification_deadline": "YYYY-MM-DD",
    "submission_deadline": "YYYY-MM-DDTHH:MM:SS",
    "technical_opening": "YYYY-MM-DD",
    "financial_opening": "YYYY-MM-DD"
  },
  "contact_info": {
    "name": "Contact person name",
    "designation": "Title/role",
    "email": "Email address",
    "phone": "Phone number",
    "address": "Office address"
  },
  "special_conditions": ["Special terms, conditions, or clauses"],
  "oem_requirements": [
    {"oem_name": "Name of OEM/manufacturer/brand", "product_category": "Product type or line item", "maf_required": true, "make_model": "Specific model if mentioned", "is_approved_make": true}
  ]
}

IMPORTANT:
- Extract ALL OEMs mentioned in the document, not just one
- For amounts, extract numeric values only (no currency symbols)
- For dates, convert to ISO format
- For evaluation criteria, extract the full marking scheme if available
- Pay special attention to tables and structured data in the document
- Include page/section references in descriptions where helpful

EMD & TENDER FEE EXTRACTION GUIDANCE:
- Look for EMD/Earnest Money Deposit section — extract amount, mode, validity, and bank details
- Check if MSME/NSIC/DPIIT/Startup India registered firms are exempted from EMD. Many govt tenders exempt MSMEs
- Extract full bank details: bank name, account number, IFSC, branch, DD in favour of whom
- For BG (Bank Guarantee): note the format requirements, validity period, issuing bank requirements
- For DD (Demand Draft): note "drawn in favour of" and "payable at" details
- Extract ALL fees separately: tender fee, document fee, processing fee, e-procurement fee, bid security
- Note which fees are refundable vs non-refundable
- Check if fees have MSME exemption too (often tender fee is also exempt for MSMEs)
- Look for payment deadlines specific to each fee
- GEM portal tenders may have different fee structures — check for Bid Security Declaration in lieu of EMD

OEM/MANUFACTURER EXTRACTION GUIDANCE:
- Look for "approved makes", "make and model", "brand", "manufacturer" mentions in tables and specifications
- GEM tenders often list product specifications with approved brands — extract those as OEMs
- Check BOQ/item tables for brand/make columns, line items with specific product names
- Look for phrases like "equivalent to", "or equivalent", "make: XYZ", "brand: XYZ"
- MAF (Manufacturer Authorization Form) is required if the tender says "authorized dealer/partner" or "OEM authorization"
- If the tender lists specific product line items with brands, create one oem_requirement per brand
- Even if no specific OEM is named, extract product categories that will need OEM authorization (e.g., "Networking Equipment", "Server Hardware", "UPS Systems")
- For GEM tenders, check the product/service description and technical specifications for brand references
"""


def extract_pdf_text(file_path: str) -> str:
    """Extract text content from a PDF file using PyPDF2.

    Args:
        file_path: Path to the PDF file on disk

    Returns:
        Extracted text content from all pages
    """
    try:
        from PyPDF2 import PdfReader

        reader = PdfReader(file_path)
        text_parts = []

        for i, page in enumerate(reader.pages):
            page_text = page.extract_text()
            if page_text:
                text_parts.append(f"--- Page {i + 1} ---\n{page_text}")

        full_text = "\n\n".join(text_parts)
        logger.info(f"Extracted {len(full_text)} characters from {len(reader.pages)} pages")
        return full_text

    except Exception as e:
        logger.error(f"Error extracting PDF text: {e}")
        raise ValueError(f"Failed to extract text from PDF: {str(e)}")


# ---------------------------------------------------------------------------
# Batch processing helpers for large multi-PDF uploads
# ---------------------------------------------------------------------------

MAX_CHARS_PER_BATCH = 120_000  # ~30K tokens — safe margin under 200K limit


def _build_text_batches(file_paths: list[str]) -> list[list[tuple[str, str]]]:
    """Split extracted PDF texts into batches that fit within token limits.

    Each batch is a list of (label, text) tuples.
    Documents are added greedily until the batch would exceed MAX_CHARS_PER_BATCH.
    A single document that exceeds the limit is placed alone in its own batch
    and truncated.

    Returns:
        List of batches, where each batch is [(label, text), ...]
    """
    doc_texts: list[tuple[str, str]] = []
    for idx, fp in enumerate(file_paths):
        file_label = os.path.basename(fp).split("_", 1)[-1]  # strip uuid prefix
        text = extract_pdf_text(fp)
        if text.strip():
            doc_texts.append((f"DOCUMENT {idx + 1}: {file_label}", text))

    if not doc_texts:
        raise ValueError("No text could be extracted from the PDF(s)")

    batches: list[list[tuple[str, str]]] = []
    current_batch: list[tuple[str, str]] = []
    current_size = 0

    for label, text in doc_texts:
        doc_size = len(text) + len(label) + 20  # overhead for header

        if current_size + doc_size > MAX_CHARS_PER_BATCH and current_batch:
            # Current batch is full — seal it and start a new one
            batches.append(current_batch)
            current_batch = []
            current_size = 0

        # Truncate individual oversized docs
        if doc_size > MAX_CHARS_PER_BATCH:
            text = text[:MAX_CHARS_PER_BATCH - 500] + "\n\n[Document truncated due to length]"

        current_batch.append((label, text))
        current_size += len(text) + len(label) + 20

    if current_batch:
        batches.append(current_batch)

    return batches


def _format_batch_text(batch: list[tuple[str, str]]) -> str:
    """Format a batch of (label, text) tuples into a single prompt string."""
    parts = [f"=== {label} ===\n{text}" for label, text in batch]
    return "\n\n".join(parts)


def _merge_extractions(results: list[dict]) -> dict:
    """Merge multiple extraction JSON results into one consolidated output.

    Strategy:
    - Scalar fields (title, reference_number, etc.): first non-empty wins
    - List fields (eligibility_criteria, technical_requirements, etc.): concatenate & deduplicate
    - Dict fields (emd, important_dates, contact_info): deep merge, first non-empty wins per key
    - oem_requirements, evaluation_criteria, tender_fees: concatenate (unique by key fields)
    """
    if len(results) == 1:
        return results[0]

    merged: dict = {}

    scalar_keys = {"title", "reference_number", "issuing_authority", "deadline", "estimated_value"}
    list_keys = {"eligibility_criteria", "technical_requirements", "document_checklist", "special_conditions"}
    dict_keys = {"emd", "important_dates", "contact_info"}
    struct_list_keys = {"evaluation_criteria", "tender_fees", "oem_requirements"}

    for result in results:
        # Scalars: first non-empty wins
        for key in scalar_keys:
            if key not in merged and key in result and result[key]:
                merged[key] = result[key]

        # Simple lists: concatenate
        for key in list_keys:
            if key in result and isinstance(result[key], list):
                existing = merged.get(key, [])
                for item in result[key]:
                    if item and item not in existing:
                        existing.append(item)
                merged[key] = existing

        # Dicts: deep merge
        for key in dict_keys:
            if key in result and isinstance(result[key], dict):
                existing = merged.get(key, {})
                for k, v in result[key].items():
                    if k not in existing or not existing[k]:
                        existing[k] = v
                merged[key] = existing

        # Structured lists: concatenate, deduplicate by identifying field
        for key in struct_list_keys:
            if key in result and isinstance(result[key], list):
                existing = merged.get(key, [])
                # Build a set of existing identifiers for dedup
                if key == "evaluation_criteria":
                    existing_ids = {e.get("criteria_code") for e in existing if e.get("criteria_code")}
                    for item in result[key]:
                        if item.get("criteria_code") and item["criteria_code"] not in existing_ids:
                            existing.append(item)
                            existing_ids.add(item["criteria_code"])
                elif key == "oem_requirements":
                    existing_ids = {e.get("oem_name", "").lower() for e in existing}
                    for item in result[key]:
                        if item.get("oem_name") and item["oem_name"].lower() not in existing_ids:
                            existing.append(item)
                            existing_ids.add(item["oem_name"].lower())
                elif key == "tender_fees":
                    existing_ids = {(e.get("fee_type"), e.get("amount")) for e in existing}
                    for item in result[key]:
                        ident = (item.get("fee_type"), item.get("amount"))
                        if ident not in existing_ids:
                            existing.append(item)
                            existing_ids.add(ident)
                merged[key] = existing

    return merged


async def extract_tender_data(
    db: AsyncSession,
    tender_id: UUID | None,
    file_paths: list[str] | str,
    current_user_id: str,
    model: str = "claude-sonnet-4-6",
    tenant_id_override: str | None = None,
) -> dict:
    """Extract structured tender data from one or more PDFs using Claude AI.

    When multiple PDFs are provided (NIT, specs, BOQ, corrigendum, etc.)
    they are all sent to Claude in a single call so it sees the full
    tender context across all documents.

    Args:
        db: Database session
        tender_id: ID of the tender (None for preview-only extraction)
        file_paths: Path(s) to the uploaded PDF(s) — single string or list
        current_user_id: ID of the requesting user
        model: Claude model to use for extraction
        tenant_id_override: Tenant ID when no tender exists yet (preview mode)

    Returns:
        Dictionary with extracted_data, model_used, tokens_used, extraction_mode
    """
    # Normalize to list for uniform handling
    if isinstance(file_paths, str):
        file_paths = [file_paths]

    if not file_paths:
        raise ValueError("At least one PDF file is required")

    # Get tenant_id from tender or override
    if tender_id:
        result = await db.execute(select(Tender).where(Tender.id == tender_id))
        tender = result.scalar_one_or_none()
        if not tender:
            raise ValueError(f"Tender {tender_id} not found")
        tenant_id = str(tender.tenant_id)
    elif tenant_id_override:
        tenant_id = tenant_id_override
    else:
        raise ValueError("Either tender_id or tenant_id_override is required")

    from PyPDF2 import PdfReader
    from app.services.ai_provider_service import (
        get_provider_name, send_message, send_message_with_documents,
    )

    # Compute combined size and page count across all files
    total_size = 0
    total_pages = 0
    for fp in file_paths:
        total_size += os.path.getsize(fp)
        total_pages += len(PdfReader(fp).pages)

    # Native PDF works best under ~1.5MB total / 60 pages to stay within token limits
    can_use_native = total_size < 1_500_000 and total_pages <= 60
    provider_name = await get_provider_name(db, tenant_id, "extraction")
    is_anthropic = provider_name == "anthropic"

    extraction_mode = "text_extraction"  # default

    try:
        if can_use_native and is_anthropic:
            # FAST PATH: Send all PDFs directly to Claude as document blocks
            logger.info(
                f"Using native PDF extraction — {len(file_paths)} file(s), "
                f"{total_size / 1024:.0f} KB total, {total_pages} pages"
            )

            documents = []
            for fp in file_paths:
                with open(fp, "rb") as f:
                    pdf_b64 = base64.standard_b64encode(f.read()).decode("utf-8")
                documents.append({"data": pdf_b64, "media_type": "application/pdf"})

            try:
                response_text, tokens_used, model_used = await send_message_with_documents(
                    db=db,
                    tenant_id=tenant_id,
                    prompt=EXTRACTION_PROMPT_NATIVE,
                    documents=documents,
                    feature="extraction",
                    model=model,
                    max_tokens=8192,
                )
            except Exception as native_err:
                if "too long" in str(native_err) or "too many" in str(native_err) or "400" in str(native_err):
                    logger.warning(f"Native PDF failed ({native_err}), falling back to text extraction")
                    can_use_native = False  # Fall through to text extraction below
                else:
                    raise
            else:
                extraction_mode = "native_pdf"

        if extraction_mode != "native_pdf":
            # FALLBACK: PyPDF2 text extraction with batch processing
            if not is_anthropic:
                logger.info(f"Provider is {provider_name} — using text extraction fallback")
            else:
                logger.info(
                    f"PDFs too large for native processing ({total_size / 1024:.0f} KB, "
                    f"{total_pages} pages) — using text extraction fallback"
                )

            batches = _build_text_batches(file_paths)
            logger.info(f"Split {len(file_paths)} PDFs into {len(batches)} batch(es)")

            if len(batches) == 1:
                # Single batch — simple path
                pdf_text = _format_batch_text(batches[0])
                response_text, tokens_used, model_used = await send_message(
                    db=db,
                    tenant_id=tenant_id,
                    prompt=EXTRACTION_PROMPT + pdf_text,
                    feature="extraction",
                    model=model,
                    max_tokens=8192,
                )
            else:
                # Multi-batch — process each batch independently, then merge
                batch_results: list[dict] = []
                tokens_used = 0

                for batch_idx, batch in enumerate(batches):
                    batch_text = _format_batch_text(batch)
                    doc_names = [label for label, _ in batch]
                    logger.info(
                        f"Processing batch {batch_idx + 1}/{len(batches)}: "
                        f"{', '.join(doc_names)} ({len(batch_text)} chars)"
                    )

                    batch_response, batch_tokens, model_used = await send_message(
                        db=db,
                        tenant_id=tenant_id,
                        prompt=EXTRACTION_PROMPT + batch_text,
                        feature="extraction",
                        model=model,
                        max_tokens=8192,
                    )
                    tokens_used += batch_tokens

                    # Parse batch JSON
                    batch_response = batch_response.strip()
                    if batch_response.startswith("```"):
                        lines = batch_response.split("\n")
                        json_lines = []
                        in_block = False
                        for line in lines:
                            if line.strip().startswith("```"):
                                in_block = not in_block
                                continue
                            if in_block:
                                json_lines.append(line)
                        batch_response = "\n".join(json_lines)

                    batch_data = json.loads(batch_response)
                    batch_results.append(batch_data)

                # Merge all batch results into one consolidated extraction
                merged_data = _merge_extractions(batch_results)
                extraction_mode = "text_extraction_batched"

                logger.info(
                    f"Batch extraction complete: {len(batches)} batches, "
                    f"{tokens_used} total tokens via {model_used}"
                )

                return {
                    "tender_id": str(tender_id),
                    "extracted_data": merged_data,
                    "model_used": model_used,
                    "tokens_used": tokens_used,
                    "extraction_mode": extraction_mode,
                    "files_processed": len(file_paths),
                    "batches_used": len(batches),
                }

        # Parse response
        response_text = response_text.strip()

        # Try to extract JSON from response (handle markdown code blocks)
        if response_text.startswith("```"):
            lines = response_text.split("\n")
            json_lines = []
            in_block = False
            for line in lines:
                if line.strip().startswith("```"):
                    in_block = not in_block
                    continue
                if in_block:
                    json_lines.append(line)
            response_text = "\n".join(json_lines)

        extracted_data = json.loads(response_text)

        logger.info(
            f"Extraction complete ({extraction_mode}): "
            f"{len(file_paths)} file(s), {tokens_used} tokens used via {model_used}"
        )

        return {
            "tender_id": str(tender_id),
            "extracted_data": extracted_data,
            "model_used": model_used,
            "tokens_used": tokens_used,
            "extraction_mode": extraction_mode,
            "files_processed": len(file_paths),
        }

    except json.JSONDecodeError as e:
        logger.error(f"Failed to parse AI response as JSON: {e}")
        raise ValueError(f"AI returned invalid JSON: {str(e)}")
    except ValueError:
        raise
    except Exception as e:
        logger.error(f"AI extraction failed: {e}")
        raise ValueError(f"AI extraction failed: {str(e)}")


async def apply_extraction(
    db: AsyncSession,
    tender_id: UUID,
    extracted_data: dict,
    current_user_id: str,
    extraction_meta: dict | None = None,
) -> dict:
    """Apply extracted data to update tender and create related records.

    Args:
        db: Database session
        tender_id: ID of the tender to update
        extracted_data: The extracted data from AI
        current_user_id: ID of the requesting user

    Returns:
        Summary of what was created/updated
    """
    result = await db.execute(select(Tender).where(Tender.id == tender_id))
    tender = result.scalar_one_or_none()
    if not tender:
        raise ValueError(f"Tender {tender_id} not found")

    fields_updated = []

    # Update tender fields
    field_map = {
        "title": "title",
        "reference_number": "reference_number",
        "issuing_authority": "issuing_authority",
        "deadline": "deadline",
        "estimated_value": "estimated_value",
    }

    for json_key, db_field in field_map.items():
        if json_key in extracted_data and extracted_data[json_key]:
            value = extracted_data[json_key]
            if json_key == "deadline" and isinstance(value, str):
                try:
                    value = datetime.fromisoformat(value.replace("Z", "+00:00"))
                except ValueError:
                    continue
            setattr(tender, db_field, value)
            fields_updated.append(db_field)

    # Store full extraction in requirements JSON (include extraction metadata for UI persistence)
    requirements_to_store = dict(extracted_data)
    requirements_to_store["_extraction_meta"] = {
        "model_used": extraction_meta.get("model_used") if extraction_meta else None,
        "tokens_used": extraction_meta.get("tokens_used") if extraction_meta else None,
        "extraction_mode": extraction_meta.get("extraction_mode") if extraction_meta else None,
        "files_processed": extraction_meta.get("files_processed") if extraction_meta else None,
        "extracted_at": datetime.now(timezone.utc).isoformat(),
    }
    tender.requirements = requirements_to_store
    fields_updated.append("requirements")

    # Create EMD record if present
    emd_created = False
    emd_data = extracted_data.get("emd")
    if emd_data and emd_data.get("amount"):
        # Build notes with rich extracted details
        emd_notes_parts = ["Auto-extracted from tender document"]
        if emd_data.get("msme_exempted"):
            emd_notes_parts.append(f"MSME Exempted: {emd_data.get('exemption_details', 'Yes')}")
        if emd_data.get("emd_format"):
            emd_notes_parts.append(f"Format: {emd_data['emd_format']}")
        bank = emd_data.get("bank_details", {})
        if bank:
            bank_parts = []
            if bank.get("bank_name"): bank_parts.append(f"Bank: {bank['bank_name']}")
            if bank.get("branch"): bank_parts.append(f"Branch: {bank['branch']}")
            if bank.get("account_number"): bank_parts.append(f"A/C: {bank['account_number']}")
            if bank.get("ifsc_code"): bank_parts.append(f"IFSC: {bank['ifsc_code']}")
            if bank.get("dd_in_favour_of"): bank_parts.append(f"DD in favour of: {bank['dd_in_favour_of']}")
            if bank.get("account_holder"): bank_parts.append(f"Beneficiary: {bank['account_holder']}")
            if bank_parts:
                emd_notes_parts.append(" | ".join(bank_parts))

        emd = EMDRecord(
            tender_id=tender_id,
            amount=float(emd_data["amount"]),
            mode=emd_data.get("mode", "online"),
            issuing_bank=bank.get("bank_name") if bank else None,
            status="pending",
            notes="\n".join(emd_notes_parts),
        )
        if emd_data.get("validity_end_date"):
            try:
                emd.validity_end_date = datetime.strptime(
                    emd_data["validity_end_date"], "%Y-%m-%d"
                ).date()
            except ValueError:
                pass
        db.add(emd)
        emd_created = True

    # Create tender fee records
    fees_created = 0
    for fee_data in extracted_data.get("tender_fees", []):
        if fee_data.get("amount"):
            # Build notes with rich fee details
            fee_notes_parts = ["Auto-extracted from tender document"]
            if fee_data.get("msme_exempted"):
                fee_notes_parts.append(f"MSME Exempted: {fee_data.get('exemption_details', 'Yes')}")
            if fee_data.get("non_refundable"):
                fee_notes_parts.append("Non-refundable")
            if fee_data.get("payment_deadline"):
                fee_notes_parts.append(f"Deadline: {fee_data['payment_deadline']}")
            fee_bank = fee_data.get("bank_details", {})
            if fee_bank:
                fb_parts = []
                if fee_bank.get("bank_name"): fb_parts.append(f"Bank: {fee_bank['bank_name']}")
                if fee_bank.get("account_number"): fb_parts.append(f"A/C: {fee_bank['account_number']}")
                if fee_bank.get("ifsc_code"): fb_parts.append(f"IFSC: {fee_bank['ifsc_code']}")
                if fee_bank.get("dd_in_favour_of"): fb_parts.append(f"DD in favour of: {fee_bank['dd_in_favour_of']}")
                if fb_parts:
                    fee_notes_parts.append(" | ".join(fb_parts))

            fee = TenderFee(
                tender_id=tender_id,
                fee_type=fee_data.get("fee_type", "tender_fee"),
                amount=float(fee_data["amount"]),
                payment_mode=fee_data.get("payment_mode"),
                status="pending",
                notes="\n".join(fee_notes_parts),
            )
            db.add(fee)
            fees_created += 1

    # Create TenderCriteria records from evaluation_criteria
    criteria_created = 0
    for crit_data in extracted_data.get("evaluation_criteria", []):
        if crit_data.get("criteria_code"):
            criteria = TenderCriteria(
                tender_id=tender_id,
                criteria_code=crit_data["criteria_code"],
                description=crit_data.get("description"),
                stage=crit_data.get("stage", "technical"),
                max_marks=float(crit_data.get("max_marks", 0)),
                ai_extracted=True,
                evidence_required=crit_data.get("evidence_required"),
            )
            db.add(criteria)
            criteria_created += 1

    # Create OEM records from oem_requirements
    oem_requirements_created = 0
    for oem_data in extracted_data.get("oem_requirements", []):
        oem_name = oem_data.get("oem_name")
        if not oem_name:
            continue

        # Find or create OEMMaster by name
        oem_result = await db.execute(
            select(OEMMaster).where(
                OEMMaster.tenant_id == tender.tenant_id,
                OEMMaster.name == oem_name
            )
        )
        oem_master = oem_result.scalar_one_or_none()

        if not oem_master:
            oem_master = OEMMaster(
                tenant_id=tender.tenant_id,
                name=oem_name,
                product_categories=[oem_data["product_category"]] if oem_data.get("product_category") else [],
            )
            db.add(oem_master)
            await db.flush()  # Get the ID

        # Build notes from extracted data
        note_parts = []
        if oem_data.get("product_category"):
            note_parts.append(f"Product: {oem_data['product_category']}")
        if oem_data.get("make_model"):
            note_parts.append(f"Model: {oem_data['make_model']}")
        if oem_data.get("is_approved_make"):
            note_parts.append("Listed as approved make")
        notes_text = " | ".join(note_parts) if note_parts else "Auto-extracted"

        # Check for existing requirement for same OEM + tender to avoid duplicates
        existing_req = await db.execute(
            select(OEMTenderRequirement).where(
                OEMTenderRequirement.tender_id == tender_id,
                OEMTenderRequirement.oem_id == oem_master.id,
            )
        )
        if existing_req.scalar_one_or_none():
            continue  # skip duplicate

        # Create OEMTenderRequirement linking OEM to this tender
        oem_req = OEMTenderRequirement(
            tender_id=tender_id,
            oem_id=oem_master.id,
            maf_status="pending" if oem_data.get("maf_required", True) else "not_required",
            ms_status="pending",
            partner_cert_status="pending",
            datasheet_status="pending",
            compliance_cert_status="pending",
            notes=notes_text,
        )
        db.add(oem_req)
        oem_requirements_created += 1

    # Create SubmissionChecklistItem records from document_checklist
    # First, load existing checklist items for this tender to avoid duplicates
    existing_checklist = await db.execute(
        select(SubmissionChecklistItem.document_name)
        .where(SubmissionChecklistItem.tender_id == tender_id)
    )
    existing_checklist_names = {row[0].lower().strip() for row in existing_checklist.all()}
    seen_names: set[str] = set()  # track names added in this batch

    checklist_created = 0
    for idx, doc_name in enumerate(extracted_data.get("document_checklist", [])):
        if not doc_name or not isinstance(doc_name, str):
            continue

        doc_name_normalized = doc_name.strip()
        doc_name_lower = doc_name_normalized.lower()

        # Skip exact duplicates (case-insensitive)
        if doc_name_lower in existing_checklist_names or doc_name_lower in seen_names:
            continue

        # Skip near-duplicates: if an existing name contains this one or vice versa
        is_near_dup = False
        for existing in existing_checklist_names | seen_names:
            # Compare core words (strip punctuation, ignore short words)
            existing_words = set(w for w in existing.split() if len(w) > 3)
            new_words = set(w for w in doc_name_lower.split() if len(w) > 3)
            if existing_words and new_words:
                overlap = len(existing_words & new_words)
                max_len = max(len(existing_words), len(new_words))
                if max_len > 0 and overlap / max_len >= 0.7:
                    is_near_dup = True
                    break
        if is_near_dup:
            continue

        seen_names.add(doc_name_lower)

        doc_lower = doc_name.lower()

        # Heuristic mode detection
        mode = "online"
        if any(kw in doc_lower for kw in ["original", "dd ", "demand draft", "sealed", "physical"]):
            mode = "both"
        if any(kw in doc_lower for kw in ["notarized", "notarise", "affidavit"]):
            mode = "both"

        # Heuristic category detection
        category = "other"
        if any(kw in doc_lower for kw in ["emd", "bid security", "earnest money", "tender fee"]):
            category = "emd"
        elif any(kw in doc_lower for kw in ["maf", "authorization", "oem"]):
            category = "oem"
        elif any(kw in doc_lower for kw in ["certificate", "registration", "pan ", "gst", "msme", "udyam"]):
            category = "certificate"
        elif any(kw in doc_lower for kw in ["declaration", "undertaking", "affidavit", "self-declaration"]):
            category = "declaration"
        elif any(kw in doc_lower for kw in ["financial", "boq", "price", "bid", "rate"]):
            category = "financial"
        elif any(kw in doc_lower for kw in ["technical", "compliance", "datasheet", "specification"]):
            category = "technical"
        elif any(kw in doc_lower for kw in ["experience", "work order", "completion"]):
            category = "experience"
        elif any(kw in doc_lower for kw in ["company", "profile", "capability"]):
            category = "company_profile"
        elif any(kw in doc_lower for kw in ["legal", "power of attorney", "partnership", "moa", "aoa"]):
            category = "legal"

        # Heuristic critical flag
        is_critical = any(kw in doc_lower for kw in [
            "emd", "bid security", "financial bid", "technical bid",
            "dsc", "digitally signed", "boq", "price schedule",
            "cover letter", "pan ", "gst",
        ])

        # Heuristic envelope
        envelope = "envelope_a"
        if category == "financial":
            envelope = "envelope_b"
        if any(kw in doc_lower for kw in ["original emd", "original dd", "tender fee dd"]):
            envelope = "superscription"

        # Heuristic notarization
        notarization_required = any(kw in doc_lower for kw in ["notarized", "notarise", "affidavit"])
        notarization_type = None
        if notarization_required:
            notarization_type = "judicial" if "affidavit" in doc_lower else "non_judicial"

        # Heuristic document origin
        document_origin = "self_generated"
        format_reference = None
        obtaining_source = None
        can_auto_generate = False

        if any(kw in doc_lower for kw in ["annexure", "format", "proforma", "form no", "section "]):
            document_origin = "tender_provided"
            for kw in ["annexure", "format", "proforma", "form no"]:
                if kw in doc_lower:
                    format_reference = f"Check tender for {kw.title()} reference"
                    break
            can_auto_generate = True
        elif any(kw in doc_lower for kw in [
            "certificate", "registration", "pan ", "gst ", "iso ", "epf", "esi",
            "udyam", "msme", "balance sheet", "p&l", "work order", "datasheet",
            "catalogue", "solvency", "completion certificate",
        ]):
            document_origin = "pre_existing"
            if "pan " in doc_lower:
                obtaining_source = "Income Tax Department"
            elif "gst " in doc_lower:
                obtaining_source = "GST Portal"
            elif "iso " in doc_lower:
                obtaining_source = "ISO Certification Body"
            elif "epf" in doc_lower:
                obtaining_source = "EPFO Portal"
            elif "esi" in doc_lower:
                obtaining_source = "ESIC Portal"
            elif any(kw in doc_lower for kw in ["udyam", "msme"]):
                obtaining_source = "Udyam Portal"
            elif any(kw in doc_lower for kw in ["balance sheet", "p&l", "turnover"]):
                obtaining_source = "Chartered Accountant"
            elif "solvency" in doc_lower:
                obtaining_source = "Bank"
            elif any(kw in doc_lower for kw in ["work order", "completion"]):
                obtaining_source = "Past Clients"
            elif any(kw in doc_lower for kw in ["datasheet", "catalogue"]):
                obtaining_source = "OEM / Manufacturer"
        else:
            if category == "declaration":
                can_auto_generate = True

        item = SubmissionChecklistItem(
            tender_id=tender_id,
            document_name=doc_name[:500],
            document_category=category,
            submission_mode=mode,
            is_critical=is_critical,
            envelope=envelope,
            notarization_required=notarization_required,
            notarization_type=notarization_type,
            notarization_status="pending" if notarization_required else "not_required",
            document_origin=document_origin,
            format_reference=format_reference,
            obtaining_source=obtaining_source,
            can_auto_generate=can_auto_generate,
            sort_order=idx + 1,
            source="ai_extracted",
        )
        db.add(item)
        checklist_created += 1

    await db.flush()

    return {
        "tender_id": str(tender_id),
        "fields_updated": fields_updated,
        "criteria_created": criteria_created,
        "emd_created": emd_created,
        "fees_created": fees_created,
        "oem_requirements_created": oem_requirements_created,
        "checklist_created": checklist_created,
    }
