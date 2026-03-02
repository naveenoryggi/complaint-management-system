"""AI-powered tender PDF extraction service.

Uses PyPDF2 to extract text from uploaded PDFs, then sends the text
to Claude for structured data extraction. Extracted data can be
previewed and then applied to update tender records.
"""
import json
import logging
from typing import Optional
from uuid import UUID
from datetime import datetime

from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.core.config import settings
from app.models.tender import Tender
from app.models.tracking import EMDRecord, TenderFee
from app.models.reference_bundle import TenderCriteria
from app.models.oem import OEMMaster, OEMTenderRequirement

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
    "validity_end_date": "YYYY-MM-DD"
  },
  "tender_fees": [
    {"fee_type": "tender_fee or processing_fee or document_fee", "amount": 0, "payment_mode": "online or dd"}
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
    {"oem_name": "Name of OEM mentioned", "product_category": "Product type", "maf_required": true}
  ]
}

IMPORTANT:
- Extract ALL OEMs mentioned in the document, not just one
- For amounts, extract numeric values only (no currency symbols)
- For dates, convert to ISO format
- For evaluation criteria, extract the full marking scheme if available
- Include page/section references in descriptions where helpful

TENDER DOCUMENT TEXT:
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


async def extract_tender_data(
    db: AsyncSession,
    tender_id: UUID,
    file_path: str,
    current_user_id: str,
    model: str = "claude-sonnet-4-5-20250514"
) -> dict:
    """Extract structured tender data from PDF using Claude AI.

    Args:
        db: Database session
        tender_id: ID of the tender to associate extraction with
        file_path: Path to the uploaded PDF
        current_user_id: ID of the requesting user
        model: Claude model to use for extraction

    Returns:
        Dictionary with extracted_data, model_used, and tokens_used
    """
    # Verify tender exists
    result = await db.execute(select(Tender).where(Tender.id == tender_id))
    tender = result.scalar_one_or_none()
    if not tender:
        raise ValueError(f"Tender {tender_id} not found")

    # Extract text from PDF
    pdf_text = extract_pdf_text(file_path)
    if not pdf_text.strip():
        raise ValueError("No text could be extracted from the PDF")

    # Truncate if too long (Claude context limit)
    max_chars = 150000
    if len(pdf_text) > max_chars:
        pdf_text = pdf_text[:max_chars] + "\n\n[Document truncated due to length]"

    # Call Claude API
    try:
        import anthropic

        client = anthropic.Anthropic(api_key=settings.anthropic_api_key)

        message = client.messages.create(
            model=model,
            max_tokens=8192,
            messages=[
                {
                    "role": "user",
                    "content": EXTRACTION_PROMPT + pdf_text
                }
            ]
        )

        # Parse response
        response_text = message.content[0].text.strip()

        # Try to extract JSON from response (handle markdown code blocks)
        if response_text.startswith("```"):
            # Strip markdown code block
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
        tokens_used = message.usage.input_tokens + message.usage.output_tokens

        logger.info(f"Extraction complete: {tokens_used} tokens used")

        return {
            "tender_id": str(tender_id),
            "extracted_data": extracted_data,
            "model_used": model,
            "tokens_used": tokens_used,
        }

    except json.JSONDecodeError as e:
        logger.error(f"Failed to parse Claude response as JSON: {e}")
        raise ValueError(f"AI returned invalid JSON: {str(e)}")
    except anthropic.APIError as e:
        logger.error(f"Claude API error: {e}")
        raise ValueError(f"AI extraction failed: {str(e)}")


async def apply_extraction(
    db: AsyncSession,
    tender_id: UUID,
    extracted_data: dict,
    current_user_id: str
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

    # Store full extraction in requirements JSON
    tender.requirements = extracted_data
    fields_updated.append("requirements")

    # Create EMD record if present
    emd_created = False
    emd_data = extracted_data.get("emd")
    if emd_data and emd_data.get("amount"):
        emd = EMDRecord(
            tender_id=tender_id,
            amount=float(emd_data["amount"]),
            mode=emd_data.get("mode", "online"),
            status="pending",
            notes="Auto-extracted from tender document",
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
            fee = TenderFee(
                tender_id=tender_id,
                fee_type=fee_data.get("fee_type", "tender_fee"),
                amount=float(fee_data["amount"]),
                payment_mode=fee_data.get("payment_mode"),
                status="pending",
                notes="Auto-extracted from tender document",
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

        # Create OEMTenderRequirement linking OEM to this tender
        oem_req = OEMTenderRequirement(
            tender_id=tender_id,
            oem_id=oem_master.id,
            maf_status="pending" if oem_data.get("maf_required", True) else "not_required",
            ms_status="pending",
            partner_cert_status="pending",
            datasheet_status="pending",
            compliance_cert_status="pending",
            notes=f"Auto-extracted: {oem_data.get('product_category', '')}".strip(),
        )
        db.add(oem_req)
        oem_requirements_created += 1

    await db.flush()

    return {
        "tender_id": str(tender_id),
        "fields_updated": fields_updated,
        "criteria_created": criteria_created,
        "emd_created": emd_created,
        "fees_created": fees_created,
        "oem_requirements_created": oem_requirements_created,
    }
