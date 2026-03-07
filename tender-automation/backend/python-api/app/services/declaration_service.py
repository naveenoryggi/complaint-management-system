"""
Declaration generation service.

Generates formatted DOCX declarations from deterministic templates.
Supports standard (template) and AI-customized (multi-provider) modes.
Supports single and bulk (ZIP) generation with company letterhead.
Supports save-and-link: persist generated DOCX to document library and auto-link to checklist.
"""
import json
import uuid
import zipfile
from io import BytesIO
from pathlib import Path
from datetime import datetime
from typing import Optional

from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.services.declaration_templates import (
    get_all_templates,
    get_template_by_key,
    get_templates_by_keys,
    DECLARATION_TEMPLATES,
)
from app.services.docx_service import docx_service


# ---------------------------------------------------------------------------
# Declaration key → checklist document_name keyword mapping
# Used by save_and_link() to auto-match generated declarations to checklist items
# ---------------------------------------------------------------------------
DECLARATION_TO_CHECKLIST_KEYWORDS = {
    "non_blacklist": ["non-blacklist", "non blacklist", "non-debarment", "debarment", "blacklisting"],
    "make_in_india": ["make in india", "local content", "domestic manufacturing"],
    "land_border": ["land border", "china clause", "sharing land border"],
    "no_relation": ["no relation", "no relative", "conflict of interest with officials"],
    "no_conviction": ["no conviction", "criminal conviction", "not convicted"],
    "authenticity": ["authenticity", "genuineness", "authentic documents"],
    "acceptance_terms": ["acceptance of terms", "terms and conditions", "unconditional acceptance"],
    "msme_declaration": ["msme", "udyam", "micro small medium", "msme declaration"],
    "gst_compliance": ["gst compliance", "gst registration", "goods and services tax"],
    "annual_turnover": ["annual turnover", "turnover certificate", "financial turnover"],
    "oem_authorization": ["oem authorization", "manufacturer authorization", "maf"],
    "no_deviation": ["no deviation", "deviation", "unconditional compliance"],
    "bid_validity": ["bid validity", "offer validity", "validity period"],
    "conflict_of_interest": ["conflict of interest", "coi declaration"],
    "integrity_pact": ["integrity pact", "integrity declaration", "transparency"],
    "pf_compliance": ["provident fund", "pf compliance", "epf", "pf registration", "pf challan"],
    "esi_compliance": ["esi compliance", "esi registration", "employees state insurance", "esic"],
    "power_of_attorney": ["power of attorney", "poa", "authorization letter"],
    "no_litigation": ["no litigation", "pending litigation", "no pending cases"],
    "past_performance": ["past performance", "experience certificate", "work order", "completion certificate"],
    "manpower_declaration": ["manpower", "personnel deployment", "key personnel"],
    "subcontracting": ["subcontracting", "no subcontract", "sub-contracting"],
    "warranty_commitment": ["warranty", "amc", "annual maintenance", "warranty commitment"],
    "environment_compliance": ["environmental", "environment compliance", "pollution control"],
    "data_security": ["data security", "confidentiality", "non-disclosure", "nda"],
    "site_visit": ["site visit", "site inspection", "site certificate"],
    "iso_quality": ["iso certification", "iso 9001", "quality certification", "cmmi"],
    "labour_law": ["labour law", "labor law", "minimum wages", "contract labour"],
    "startup_declaration": ["startup india", "dpiit", "startup recognition"],
    "joint_venture": ["joint venture", "consortium", "jv agreement"],
}


# Claude prompt for analyzing tender requirements against declaration types
ANALYSIS_PROMPT = """You are an expert Indian government tender analyst. Analyze the following tender requirements and identify which standard declarations need CUSTOMIZATION based on tender-specific clauses.

TENDER DETAILS:
Title: {tender_title}
Reference: {tender_reference}
Issuing Authority: {issuing_authority}

EXTRACTED REQUIREMENTS:
{requirements_text}

AVAILABLE DECLARATION TYPES:
{declaration_types_text}

INSTRUCTIONS:
For each declaration type, determine if the tender specifies ANY custom requirements, specific clauses, particular wording, annexure references, or additional points that must appear in that declaration.

Only flag a declaration as "ai_recommended" if the tender EXPLICITLY mentions specific requirements for it. Do NOT flag declarations that can be satisfied with standard generic text.

Examples of when to flag:
- Tender says "Non-blacklisting declaration must specifically mention GeM portal"
- Tender requires "Make in India declaration as per Annexure-VIII format"
- Tender specifies "Integrity pact must name IEM: Mr. XYZ"
- Tender mentions specific turnover thresholds for the turnover certificate
- Tender requires declaration referencing specific government orders or notifications beyond the standard ones

Return ONLY valid JSON (no markdown, no code blocks) in this exact format:
{{
  "analysis": [
    {{
      "key": "declaration_key",
      "ai_recommended": true/false,
      "reason": "Brief reason why customization is needed (or null if standard is fine)",
      "tender_specific_points": ["point 1 from tender", "point 2"]
    }}
  ]
}}

Analyze ALL {num_types} declaration types. Include every type in the response, even if ai_recommended is false."""


# Claude prompt for generating a customized declaration
CUSTOMIZATION_PROMPT = """You are an expert Indian government tender compliance writer. Generate a CUSTOMIZED declaration for the following tender.

DECLARATION TYPE: {declaration_name}
CATEGORY: {declaration_category}

BIDDER ROLE: {bidder_role_context}

TENDER DETAILS:
- Title: {tender_title}
- Reference Number: {tender_reference}
- Issuing Authority: {issuing_authority}

TENDER-SPECIFIC REQUIREMENTS FOR THIS DECLARATION:
{tender_specific_points}

COMPANY DETAILS:
- Company Name: {company_name}
- PAN: {pan_number}
- GSTIN: {gstin}
- CIN: {cin_number}
- MSME Registration: {msme_registration}
- Registered Address: {registered_address}
- Phone: {phone}
- Email: {email}
- Website: {website}

STANDARD TEMPLATE (for reference structure — customize this, don't copy verbatim):
{standard_template}

INSTRUCTIONS:
1. Generate a complete, professional declaration that incorporates ALL the tender-specific requirements listed above
2. Maintain the formal legal tone appropriate for Indian government tenders
3. Include all relevant company details where appropriate
4. Reference the specific tender number and issuing authority
5. Include specific clause numbers, government order references, or annexure formats as required by the tender
6. Keep the declaration self-contained — it should be ready to print and sign
7. Use today's date: {date}
8. Do NOT include signature block — that will be added automatically
9. Do NOT use markdown headings (# or ##) — use plain text with UPPERCASE for section titles

Generate the complete declaration text:"""


class DeclarationService:
    """Service for generating tender declarations (standard + AI-customized)."""

    def __init__(self):
        pass

    def get_available_types(self) -> list[dict]:
        """Return all available declaration types with metadata."""
        return get_all_templates()

    def _build_placeholders(
        self,
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        bidder_role: str = "bidder",
    ) -> dict[str, str]:
        """Build placeholder dict from company + tender data."""
        role_label = "Original Equipment Manufacturer (OEM)" if bidder_role == "oem" else "Authorized Bidder"
        placeholders = {
            "company_name": company_profile.get("company_name", "_______________"),
            "pan_number": company_profile.get("pan_number", "_______________"),
            "gstin": company_profile.get("gstin", "_______________"),
            "cin_number": company_profile.get("cin_number", "_______________"),
            "msme_registration": company_profile.get("msme_registration", "_______________"),
            "registered_address": company_profile.get("registered_address", "_______________"),
            "website": company_profile.get("website", "_______________"),
            "phone": company_profile.get("phone", "_______________"),
            "email": company_profile.get("email", "_______________"),
            "pf_registration_number": company_profile.get("pf_registration_number", "_______________"),
            "esi_registration_number": company_profile.get("esi_registration_number", "_______________"),
            "date": datetime.utcnow().strftime("%d/%m/%Y"),
            "authorized_signatory": signatory_name or "_______________",
            "designation": designation or "_______________",
            "bidder_role": role_label,
            "role_abbrev": bidder_role.upper(),
        }

        if tender:
            placeholders["tender_title"] = tender.get("title", "_______________")
            placeholders["tender_reference"] = tender.get("reference_number", "_______________")
            placeholders["issuing_authority"] = tender.get("issuing_authority", "The Procuring Authority")
        else:
            placeholders["tender_title"] = "_______________"
            placeholders["tender_reference"] = "_______________"
            placeholders["issuing_authority"] = "The Procuring Authority"

        return placeholders

    # -----------------------------------------------------------------------
    # Tender Analysis
    # -----------------------------------------------------------------------

    async def analyze_tender_requirements(
        self,
        db: AsyncSession,
        tenant_id: str,
        tender: dict,
        tender_requirements: dict | None = None,
    ) -> list[dict]:
        """
        Analyze tender requirements and identify which declarations need
        AI customization.

        Args:
            tender: Tender dict with title, reference_number, issuing_authority
            tender_requirements: The tender's extracted requirements JSON
                (eligibility_criteria, special_conditions, document_checklist, etc.)

        Returns:
            List of analysis results per declaration type
        """
        # Build requirements text from extracted data
        req_parts = []

        if tender_requirements:
            if tender_requirements.get("eligibility_criteria"):
                req_parts.append("ELIGIBILITY CRITERIA:")
                for item in tender_requirements["eligibility_criteria"]:
                    if isinstance(item, str):
                        req_parts.append(f"  - {item}")
                    elif isinstance(item, dict):
                        req_parts.append(f"  - {item.get('description', item)}")

            if tender_requirements.get("technical_requirements"):
                req_parts.append("\nTECHNICAL REQUIREMENTS:")
                for item in tender_requirements["technical_requirements"]:
                    if isinstance(item, str):
                        req_parts.append(f"  - {item}")
                    elif isinstance(item, dict):
                        req_parts.append(f"  - {item.get('description', item)}")

            if tender_requirements.get("special_conditions"):
                req_parts.append("\nSPECIAL CONDITIONS:")
                if isinstance(tender_requirements["special_conditions"], list):
                    for item in tender_requirements["special_conditions"]:
                        req_parts.append(f"  - {item}")
                elif isinstance(tender_requirements["special_conditions"], str):
                    req_parts.append(f"  {tender_requirements['special_conditions']}")

            if tender_requirements.get("document_checklist"):
                req_parts.append("\nDOCUMENT CHECKLIST:")
                for item in tender_requirements["document_checklist"]:
                    if isinstance(item, str):
                        req_parts.append(f"  - {item}")
                    elif isinstance(item, dict):
                        req_parts.append(f"  - {item.get('name', item)}")

            if tender_requirements.get("evaluation_criteria"):
                req_parts.append("\nEVALUATION CRITERIA:")
                for item in tender_requirements["evaluation_criteria"]:
                    if isinstance(item, dict):
                        desc = item.get("description", "")
                        marks = item.get("max_marks", "")
                        req_parts.append(f"  - {desc} (Max marks: {marks})")

        requirements_text = "\n".join(req_parts) if req_parts else "No specific requirements extracted from tender document."

        # Build declaration types text
        types_text = "\n".join(
            f"  - {t['key']}: {t['name']} ({t['category']}) — {t['description']}"
            for t in DECLARATION_TEMPLATES
        )

        prompt = ANALYSIS_PROMPT.format(
            tender_title=tender.get("title", "N/A"),
            tender_reference=tender.get("reference_number", "N/A"),
            issuing_authority=tender.get("issuing_authority", "N/A"),
            requirements_text=requirements_text,
            declaration_types_text=types_text,
            num_types=len(DECLARATION_TEMPLATES),
        )

        try:
            from app.services.ai_provider_service import send_message

            response_text, tokens_used, model_used = await send_message(
                db=db,
                tenant_id=tenant_id,
                prompt=prompt,
                feature="declarations",
                max_tokens=4096,
                temperature=0,
            )

            response_text = response_text.strip()

            # Parse JSON response (handle potential markdown code blocks)
            if response_text.startswith("```"):
                lines = response_text.split("\n")
                lines = [l for l in lines if not l.startswith("```")]
                response_text = "\n".join(lines)

            result = json.loads(response_text)
            return result.get("analysis", [])

        except Exception:
            # If analysis fails, return all as standard (no AI recommended)
            return [
                {
                    "key": t["key"],
                    "ai_recommended": False,
                    "reason": None,
                    "tender_specific_points": [],
                }
                for t in DECLARATION_TEMPLATES
            ]

    # -----------------------------------------------------------------------
    # Standard Generation (template-based)
    # -----------------------------------------------------------------------

    def generate_single(
        self,
        declaration_type: str,
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        bidder_role: str = "bidder",
    ) -> tuple[BytesIO, str]:
        """Generate a single standard declaration DOCX."""
        template = get_template_by_key(declaration_type)
        if not template:
            raise ValueError(f"Unknown declaration type: {declaration_type}")

        placeholders = self._build_placeholders(company_profile, tender, signatory_name, designation, bidder_role)

        content = template["content_template"]
        for key, value in placeholders.items():
            content = content.replace(f"{{{key}}}", str(value))

        buffer = docx_service.create_document_from_text(
            content=content,
            title=template["name"],
            generation_type="declaration",
            company_name=company_profile.get("company_name", "Your Company"),
            add_letterhead=True,
            company_profile=company_profile,
            add_signature=True,
            signatory_name=signatory_name,
            designation=designation,
        )

        safe_name = template["key"].replace("_", "-")
        filename = f"{safe_name}-declaration.docx"
        return buffer, filename

    # -----------------------------------------------------------------------
    # AI-Customized Generation (Claude-based)
    # -----------------------------------------------------------------------

    async def generate_ai_customized(
        self,
        db: AsyncSession,
        tenant_id: str,
        declaration_type: str,
        company_profile: dict,
        tender: dict,
        tender_requirements: dict | None = None,
        tender_specific_points: list[str] | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        bidder_role: str = "bidder",
    ) -> tuple[BytesIO, str]:
        """
        Generate a single AI-customized declaration using Claude.

        Args:
            declaration_type: Declaration key
            company_profile: Company profile dict
            tender: Tender dict
            tender_requirements: Full extracted requirements JSON
            tender_specific_points: Specific points that need to be in this declaration
            signatory_name: Signatory name
            designation: Signatory designation

        Returns:
            Tuple of (BytesIO with DOCX, filename)
        """
        template = get_template_by_key(declaration_type)
        if not template:
            raise ValueError(f"Unknown declaration type: {declaration_type}")

        # Build specific points text
        points_text = ""
        if tender_specific_points:
            points_text = "\n".join(f"- {p}" for p in tender_specific_points)
        else:
            points_text = "No specific points identified — generate based on overall tender requirements."

        # Also include broader requirements context
        if tender_requirements:
            extra_context = []
            if tender_requirements.get("special_conditions"):
                sc = tender_requirements["special_conditions"]
                if isinstance(sc, list):
                    extra_context.extend(sc)
                elif isinstance(sc, str):
                    extra_context.append(sc)
            if extra_context:
                points_text += "\n\nADDITIONAL TENDER CONTEXT:\n" + "\n".join(f"- {c}" for c in extra_context[:10])

        placeholders = self._build_placeholders(company_profile, tender, signatory_name, designation, bidder_role)

        # Fill the standard template for reference
        standard_content = template["content_template"]
        for key, value in placeholders.items():
            standard_content = standard_content.replace(f"{{{key}}}", str(value))

        role_context = "The company is participating as an OEM (Original Equipment Manufacturer) in this tender." if bidder_role == "oem" else "The company is participating as a Bidder/Reseller in this tender."

        prompt = CUSTOMIZATION_PROMPT.format(
            declaration_name=template["name"],
            declaration_category=template["category"],
            bidder_role_context=role_context,
            tender_title=tender.get("title", "N/A"),
            tender_reference=tender.get("reference_number", "N/A"),
            issuing_authority=tender.get("issuing_authority", "N/A"),
            tender_specific_points=points_text,
            company_name=company_profile.get("company_name", "_______________"),
            pan_number=company_profile.get("pan_number", "_______________"),
            gstin=company_profile.get("gstin", "_______________"),
            cin_number=company_profile.get("cin_number", "_______________"),
            msme_registration=company_profile.get("msme_registration", "_______________"),
            registered_address=company_profile.get("registered_address", "_______________"),
            phone=company_profile.get("phone", "_______________"),
            email=company_profile.get("email", "_______________"),
            website=company_profile.get("website", "_______________"),
            standard_template=standard_content,
            date=datetime.utcnow().strftime("%d/%m/%Y"),
        )

        from app.services.ai_provider_service import send_message

        ai_content, tokens_used, model_used = await send_message(
            db=db,
            tenant_id=tenant_id,
            prompt=prompt,
            feature="declarations",
            max_tokens=4096,
            temperature=0.2,
        )

        # Generate DOCX with letterhead + signature
        buffer = docx_service.create_document_from_text(
            content=ai_content,
            title=template["name"],
            generation_type="declaration",
            company_name=company_profile.get("company_name", "Your Company"),
            add_letterhead=True,
            company_profile=company_profile,
            add_signature=True,
            signatory_name=signatory_name,
            designation=designation,
        )

        safe_name = template["key"].replace("_", "-")
        filename = f"{safe_name}-declaration-custom.docx"
        return buffer, filename

    # -----------------------------------------------------------------------
    # Bulk Generation (mixed standard + AI)
    # -----------------------------------------------------------------------

    async def generate_bulk(
        self,
        db: AsyncSession,
        tenant_id: str,
        declaration_types: list[str],
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        ai_types: list[str] | None = None,
        tender_requirements: dict | None = None,
        analysis_results: list[dict] | None = None,
        bidder_role: str = "bidder",
    ) -> tuple[BytesIO, str, list[dict]]:
        """
        Generate multiple declarations packaged as a ZIP file.
        Supports mixed standard + AI-customized generation.

        Args:
            declaration_types: List of declaration keys to generate
            company_profile: Company profile dict
            tender: Tender dict (optional for standard, required for AI)
            signatory_name: Signatory name
            designation: Designation
            ai_types: List of declaration keys that should use AI mode
            tender_requirements: Extracted requirements JSON (for AI mode)
            analysis_results: Analysis results with tender_specific_points per type

        Returns:
            Tuple of (BytesIO ZIP, zip_filename, generated files info)
        """
        ai_types_set = set(ai_types or [])

        # Build a lookup of analysis results for tender-specific points
        analysis_lookup: dict[str, list[str]] = {}
        if analysis_results:
            for item in analysis_results:
                if item.get("tender_specific_points"):
                    analysis_lookup[item["key"]] = item["tender_specific_points"]

        zip_buffer = BytesIO()
        generated_files = []

        with zipfile.ZipFile(zip_buffer, "w", zipfile.ZIP_DEFLATED) as zf:
            for dtype in declaration_types:
                try:
                    use_ai = dtype in ai_types_set and tender is not None

                    if use_ai:
                        docx_buffer, filename = await self.generate_ai_customized(
                            db=db,
                            tenant_id=tenant_id,
                            declaration_type=dtype,
                            company_profile=company_profile,
                            tender=tender,
                            tender_requirements=tender_requirements,
                            tender_specific_points=analysis_lookup.get(dtype),
                            signatory_name=signatory_name,
                            designation=designation,
                            bidder_role=bidder_role,
                        )
                    else:
                        docx_buffer, filename = self.generate_single(
                            declaration_type=dtype,
                            company_profile=company_profile,
                            tender=tender,
                            signatory_name=signatory_name,
                            designation=designation,
                            bidder_role=bidder_role,
                        )

                    zf.writestr(filename, docx_buffer.read())
                    template = get_template_by_key(dtype)
                    generated_files.append({
                        "key": dtype,
                        "name": template["name"] if template else dtype,
                        "filename": filename,
                        "mode": "ai" if use_ai else "standard",
                        "status": "success",
                    })
                except Exception as e:
                    generated_files.append({
                        "key": dtype,
                        "name": dtype,
                        "filename": None,
                        "mode": "ai" if dtype in ai_types_set else "standard",
                        "status": "error",
                        "error": str(e),
                    })

        zip_buffer.seek(0)

        company_short = company_profile.get("company_name", "declarations")[:30].replace(" ", "_")
        zip_filename = f"{company_short}_declarations.zip"

        return zip_buffer, zip_filename, generated_files

    # -----------------------------------------------------------------------
    # Preview (text-only, no DOCX wrapping)
    # -----------------------------------------------------------------------

    def preview_single_standard(
        self,
        declaration_type: str,
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        bidder_role: str = "bidder",
    ) -> dict:
        """Preview a standard declaration — returns text content without DOCX."""
        template = get_template_by_key(declaration_type)
        if not template:
            raise ValueError(f"Unknown declaration type: {declaration_type}")

        placeholders = self._build_placeholders(company_profile, tender, signatory_name, designation, bidder_role)

        content = template["content_template"]
        for key, value in placeholders.items():
            content = content.replace(f"{{{key}}}", str(value))

        return {
            "key": template["key"],
            "name": template["name"],
            "content": content,
            "mode": "standard",
            "category": template["category"],
        }

    async def preview_single_ai(
        self,
        db: AsyncSession,
        tenant_id: str,
        declaration_type: str,
        company_profile: dict,
        tender: dict,
        tender_requirements: dict | None = None,
        tender_specific_points: list[str] | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        bidder_role: str = "bidder",
    ) -> dict:
        """Preview an AI-customized declaration — returns text content without DOCX."""
        template = get_template_by_key(declaration_type)
        if not template:
            raise ValueError(f"Unknown declaration type: {declaration_type}")

        # Build specific points text
        points_text = ""
        if tender_specific_points:
            points_text = "\n".join(f"- {p}" for p in tender_specific_points)
        else:
            points_text = "No specific points identified — generate based on overall tender requirements."

        if tender_requirements:
            extra_context = []
            if tender_requirements.get("special_conditions"):
                sc = tender_requirements["special_conditions"]
                if isinstance(sc, list):
                    extra_context.extend(sc)
                elif isinstance(sc, str):
                    extra_context.append(sc)
            if extra_context:
                points_text += "\n\nADDITIONAL TENDER CONTEXT:\n" + "\n".join(f"- {c}" for c in extra_context[:10])

        placeholders = self._build_placeholders(company_profile, tender, signatory_name, designation, bidder_role)

        standard_content = template["content_template"]
        for key, value in placeholders.items():
            standard_content = standard_content.replace(f"{{{key}}}", str(value))

        role_context = "The company is participating as an OEM (Original Equipment Manufacturer) in this tender." if bidder_role == "oem" else "The company is participating as a Bidder/Reseller in this tender."

        prompt = CUSTOMIZATION_PROMPT.format(
            declaration_name=template["name"],
            declaration_category=template["category"],
            bidder_role_context=role_context,
            tender_title=tender.get("title", "N/A"),
            tender_reference=tender.get("reference_number", "N/A"),
            issuing_authority=tender.get("issuing_authority", "N/A"),
            tender_specific_points=points_text,
            company_name=company_profile.get("company_name", "_______________"),
            pan_number=company_profile.get("pan_number", "_______________"),
            gstin=company_profile.get("gstin", "_______________"),
            cin_number=company_profile.get("cin_number", "_______________"),
            msme_registration=company_profile.get("msme_registration", "_______________"),
            registered_address=company_profile.get("registered_address", "_______________"),
            phone=company_profile.get("phone", "_______________"),
            email=company_profile.get("email", "_______________"),
            website=company_profile.get("website", "_______________"),
            standard_template=standard_content,
            date=datetime.utcnow().strftime("%d/%m/%Y"),
        )

        from app.services.ai_provider_service import send_message

        ai_content, tokens_used, model_used = await send_message(
            db=db,
            tenant_id=tenant_id,
            prompt=prompt,
            feature="declarations",
            max_tokens=4096,
            temperature=0.2,
        )

        result = {
            "key": template["key"],
            "name": template["name"],
            "content": ai_content,
            "mode": "ai",
            "category": template["category"],
        }

        # Auto-validate if we have tender_specific_points
        if tender_specific_points:
            result["validation"] = self.validate_preview(ai_content, tender_specific_points)

        return result

    async def preview_single(
        self,
        db: AsyncSession,
        tenant_id: str,
        declaration_type: str,
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        mode: str = "standard",
        tender_requirements: dict | None = None,
        tender_specific_points: list[str] | None = None,
        bidder_role: str = "bidder",
    ) -> dict:
        """Preview a single declaration (standard or AI). Returns text, no DOCX."""
        if mode == "ai" and tender:
            return await self.preview_single_ai(
                db=db,
                tenant_id=tenant_id,
                declaration_type=declaration_type,
                company_profile=company_profile,
                tender=tender,
                tender_requirements=tender_requirements,
                tender_specific_points=tender_specific_points,
                signatory_name=signatory_name,
                designation=designation,
                bidder_role=bidder_role,
            )
        else:
            return self.preview_single_standard(
                declaration_type=declaration_type,
                company_profile=company_profile,
                tender=tender,
                signatory_name=signatory_name,
                designation=designation,
                bidder_role=bidder_role,
            )

    async def preview_bulk(
        self,
        db: AsyncSession,
        tenant_id: str,
        declaration_types: list[str],
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        ai_types: list[str] | None = None,
        tender_requirements: dict | None = None,
        analysis_results: list[dict] | None = None,
        bidder_role: str = "bidder",
    ) -> list[dict]:
        """Preview multiple declarations. Returns list of text previews."""
        ai_types_set = set(ai_types or [])

        # Build analysis lookup for tender-specific points
        analysis_lookup: dict[str, list[str]] = {}
        if analysis_results:
            for item in analysis_results:
                if item.get("tender_specific_points"):
                    analysis_lookup[item["key"]] = item["tender_specific_points"]

        previews = []
        for dtype in declaration_types:
            try:
                use_ai = dtype in ai_types_set and tender is not None
                preview = await self.preview_single(
                    db=db,
                    tenant_id=tenant_id,
                    declaration_type=dtype,
                    company_profile=company_profile,
                    tender=tender,
                    signatory_name=signatory_name,
                    designation=designation,
                    mode="ai" if use_ai else "standard",
                    tender_requirements=tender_requirements,
                    tender_specific_points=analysis_lookup.get(dtype),
                    bidder_role=bidder_role,
                )
                previews.append(preview)
            except Exception as e:
                template = get_template_by_key(dtype)
                previews.append({
                    "key": dtype,
                    "name": template["name"] if template else dtype,
                    "content": f"Error generating preview: {str(e)}",
                    "mode": "ai" if dtype in ai_types_set else "standard",
                    "category": template["category"] if template else "unknown",
                    "error": str(e),
                })

        return previews

    # -----------------------------------------------------------------------
    # Validation
    # -----------------------------------------------------------------------

    def validate_preview(self, content: str, tender_specific_points: list[str]) -> dict:
        """
        Check if tender-specific points appear in the generated text.
        Uses simple lowercase substring matching on key phrases.
        Returns {total_points, found, missing[], status}.
        """
        if not tender_specific_points:
            return {"total_points": 0, "found": 0, "missing": [], "status": "none"}

        content_lower = content.lower()
        missing = []
        found_count = 0

        for point in tender_specific_points:
            # Extract key phrases from the point (words 3+ chars, skip common words)
            stop_words = {"the", "and", "for", "with", "that", "this", "from", "must", "should", "shall", "have", "been", "are", "was", "were", "has"}
            words = point.lower().split()
            key_phrases = [w.strip(".,;:()\"'") for w in words if len(w) > 2 and w.lower() not in stop_words]

            # A point is "found" if at least 60% of its key phrases appear in content
            if not key_phrases:
                found_count += 1
                continue

            matches = sum(1 for phrase in key_phrases if phrase in content_lower)
            threshold = max(1, int(len(key_phrases) * 0.6))

            if matches >= threshold:
                found_count += 1
            else:
                missing.append(point)

        return {
            "total_points": len(tender_specific_points),
            "found": found_count,
            "missing": missing,
            "status": "pass" if not missing else "warning",
        }

    # -----------------------------------------------------------------------
    # Save & Link to Checklist
    # -----------------------------------------------------------------------

    async def save_and_link(
        self,
        db: AsyncSession,
        tenant_id: str,
        user_id: str,
        tender_id: str,
        declaration_types: list[str],
        company_profile: dict,
        tender: dict,
        signatory_name: str | None = None,
        designation: str | None = None,
        ai_types: list[str] | None = None,
        tender_requirements: dict | None = None,
        analysis_results: list[dict] | None = None,
        bidder_role: str = "bidder",
    ) -> dict:
        """
        Generate declarations, persist to document library, and auto-link to
        matching submission checklist items.

        Returns:
            dict with results[], saved count, linked count
        """
        from app.models.document import Document, TenderDocument
        from app.models.submission_checklist import SubmissionChecklistItem
        from app.core.config import settings

        ai_types_set = set(ai_types or [])

        # Build analysis lookup for AI points
        analysis_lookup: dict[str, list[str]] = {}
        if analysis_results:
            for item in analysis_results:
                if item.get("tender_specific_points"):
                    analysis_lookup[item["key"]] = item["tender_specific_points"]

        # Prepare upload directory
        upload_dir = Path(settings.upload_dir)
        declarations_dir = upload_dir / "declarations" / str(tender_id)
        declarations_dir.mkdir(parents=True, exist_ok=True)

        # Load existing checklist items for this tender
        checklist_result = await db.execute(
            select(SubmissionChecklistItem).where(
                SubmissionChecklistItem.tender_id == uuid.UUID(tender_id),
            )
        )
        checklist_items = list(checklist_result.scalars().all())

        results = []
        saved_count = 0
        linked_count = 0

        for dtype in declaration_types:
            entry = {"key": dtype, "document_id": None, "linked_checklist_item_id": None, "filename": None, "status": "error"}
            try:
                use_ai = dtype in ai_types_set and tender is not None

                if use_ai:
                    docx_buffer, filename = await self.generate_ai_customized(
                        db=db,
                        tenant_id=tenant_id,
                        declaration_type=dtype,
                        company_profile=company_profile,
                        tender=tender,
                        tender_requirements=tender_requirements,
                        tender_specific_points=analysis_lookup.get(dtype),
                        signatory_name=signatory_name,
                        designation=designation,
                        bidder_role=bidder_role,
                    )
                else:
                    docx_buffer, filename = self.generate_single(
                        declaration_type=dtype,
                        company_profile=company_profile,
                        tender=tender,
                        signatory_name=signatory_name,
                        designation=designation,
                        bidder_role=bidder_role,
                    )

                # Write DOCX to disk
                file_path = declarations_dir / filename
                docx_buffer.seek(0)
                file_bytes = docx_buffer.read()
                file_path.write_bytes(file_bytes)
                file_size = len(file_bytes)

                # Relative path from upload_dir
                relative_path = str(file_path.relative_to(upload_dir))

                # Get template name for the document record
                template = get_template_by_key(dtype)
                doc_name = template["name"] if template else dtype

                # Create Document record
                doc_id = uuid.uuid4()
                doc = Document(
                    id=doc_id,
                    tenant_id=uuid.UUID(tenant_id),
                    created_by=uuid.UUID(user_id),
                    name=f"{doc_name} - {tender.get('reference_number', 'Draft')}",
                    description=f"Auto-generated declaration for tender {tender.get('title', '')}",
                    file_path=relative_path,
                    file_size=file_size,
                    mime_type="application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    document_type="declaration",
                    tags=json.dumps([dtype, "declaration", "auto-generated"]),
                    is_template=False,
                    version=1,
                )
                db.add(doc)

                # Create TenderDocument association
                tender_doc = TenderDocument(
                    id=uuid.uuid4(),
                    tender_id=uuid.UUID(tender_id),
                    document_id=doc_id,
                    is_generated=True,
                    generation_prompt=f"Declaration type: {dtype}, mode: {'ai' if use_ai else 'standard'}",
                )
                db.add(tender_doc)

                saved_count += 1
                entry["document_id"] = str(doc_id)
                entry["filename"] = filename
                entry["status"] = "saved"

                # Auto-link to matching checklist item
                keywords = DECLARATION_TO_CHECKLIST_KEYWORDS.get(dtype, [])
                matched_item = None

                for ci in checklist_items:
                    if ci.linked_document_id is not None:
                        continue  # already linked
                    ci_name_lower = (ci.document_name or "").lower()
                    ci_category = (ci.document_category or "").lower()

                    # Match by keywords in document_name or by declaration category
                    if ci_category == "declaration" and any(kw in ci_name_lower for kw in keywords):
                        matched_item = ci
                        break

                # Fallback 1: match by keywords in document_name (any category)
                if not matched_item:
                    for ci in checklist_items:
                        if ci.linked_document_id is not None:
                            continue
                        ci_name_lower = (ci.document_name or "").lower()
                        if any(kw in ci_name_lower for kw in keywords):
                            matched_item = ci
                            break

                # Fallback 2: match by declaration name substring in checklist item name
                if not matched_item:
                    template = get_template_by_key(dtype)
                    decl_name = (template["name"] if template else dtype).lower()
                    for ci in checklist_items:
                        if ci.linked_document_id is not None:
                            continue
                        ci_name_lower = (ci.document_name or "").lower()
                        if decl_name in ci_name_lower or ci_name_lower in decl_name:
                            matched_item = ci
                            break

                if not matched_item:
                    entry["link_reason"] = "No matching checklist item found"

                if matched_item:
                    matched_item.linked_document_id = doc_id
                    matched_item.online_status = "uploaded"
                    linked_count += 1
                    entry["linked_checklist_item_id"] = str(matched_item.id)
                    entry["status"] = "saved_and_linked"

            except Exception as e:
                entry["error"] = str(e)

            results.append(entry)

        await db.commit()

        return {
            "results": results,
            "saved": saved_count,
            "linked": linked_count,
        }


# Global service instance
declaration_service = DeclarationService()
