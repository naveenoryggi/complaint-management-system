"""
Declaration generation service.

Generates formatted DOCX declarations from deterministic templates.
Supports standard (template) and AI-customized (Claude) modes.
Supports single and bulk (ZIP) generation with company letterhead.
"""
import json
import zipfile
from io import BytesIO
from datetime import datetime
from typing import Optional

import anthropic

from app.core.config import settings
from app.services.declaration_templates import (
    get_all_templates,
    get_template_by_key,
    get_templates_by_keys,
    DECLARATION_TEMPLATES,
)
from app.services.docx_service import docx_service


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
        self.client = anthropic.Anthropic(api_key=settings.anthropic_api_key)

    def get_available_types(self) -> list[dict]:
        """Return all available declaration types with metadata."""
        return get_all_templates()

    def _build_placeholders(
        self,
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
    ) -> dict[str, str]:
        """Build placeholder dict from company + tender data."""
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
            "date": datetime.utcnow().strftime("%d/%m/%Y"),
            "authorized_signatory": signatory_name or "_______________",
            "designation": designation or "_______________",
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

    def analyze_tender_requirements(
        self,
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
            message = self.client.messages.create(
                model="claude-sonnet-4-5-20250514",
                max_tokens=4096,
                temperature=0,
                messages=[{"role": "user", "content": prompt}],
            )

            response_text = message.content[0].text.strip()

            # Parse JSON response (handle potential markdown code blocks)
            if response_text.startswith("```"):
                lines = response_text.split("\n")
                lines = [l for l in lines if not l.startswith("```")]
                response_text = "\n".join(lines)

            result = json.loads(response_text)
            return result.get("analysis", [])

        except (json.JSONDecodeError, anthropic.APIError) as e:
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
    ) -> tuple[BytesIO, str]:
        """Generate a single standard declaration DOCX."""
        template = get_template_by_key(declaration_type)
        if not template:
            raise ValueError(f"Unknown declaration type: {declaration_type}")

        placeholders = self._build_placeholders(company_profile, tender, signatory_name, designation)

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

    def generate_ai_customized(
        self,
        declaration_type: str,
        company_profile: dict,
        tender: dict,
        tender_requirements: dict | None = None,
        tender_specific_points: list[str] | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
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

        placeholders = self._build_placeholders(company_profile, tender, signatory_name, designation)

        # Fill the standard template for reference
        standard_content = template["content_template"]
        for key, value in placeholders.items():
            standard_content = standard_content.replace(f"{{{key}}}", str(value))

        prompt = CUSTOMIZATION_PROMPT.format(
            declaration_name=template["name"],
            declaration_category=template["category"],
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

        message = self.client.messages.create(
            model="claude-sonnet-4-5-20250514",
            max_tokens=4096,
            temperature=0.2,
            messages=[{"role": "user", "content": prompt}],
        )

        ai_content = message.content[0].text

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

    def generate_bulk(
        self,
        declaration_types: list[str],
        company_profile: dict,
        tender: dict | None = None,
        signatory_name: str | None = None,
        designation: str | None = None,
        ai_types: list[str] | None = None,
        tender_requirements: dict | None = None,
        analysis_results: list[dict] | None = None,
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
                        docx_buffer, filename = self.generate_ai_customized(
                            declaration_type=dtype,
                            company_profile=company_profile,
                            tender=tender,
                            tender_requirements=tender_requirements,
                            tender_specific_points=analysis_lookup.get(dtype),
                            signatory_name=signatory_name,
                            designation=designation,
                        )
                    else:
                        docx_buffer, filename = self.generate_single(
                            declaration_type=dtype,
                            company_profile=company_profile,
                            tender=tender,
                            signatory_name=signatory_name,
                            designation=designation,
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


# Global service instance
declaration_service = DeclarationService()
