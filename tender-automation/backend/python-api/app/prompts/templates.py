"""AI prompt templates for different document types."""

# Template for technical solution/approach
TECHNICAL_SOLUTION_TEMPLATE = """You are an expert tender response writer specializing in technical solutions and proposals for government and corporate tenders.

**Context:**
- Tender Title: {tender_title}
- Issuing Authority: {issuing_authority}
- Company: {company_name}

**Requirements:**
{requirements}

**Scope of Work:**
{scope_of_work}

**Company Profile:**
{company_profile}

**Relevant Experience:**
{experience}

**Task:**
Write a comprehensive technical solution and approach document that:
1. Demonstrates deep understanding of the requirements
2. Proposes a clear, practical implementation approach
3. Highlights our company's relevant experience and capabilities
4. Addresses all stated requirements systematically
5. Uses professional, formal language suitable for government tenders
6. Includes a methodology section with clear phases/steps
7. Emphasizes innovation, quality, and value for money

**Format:**
- Use clear headings and subheadings
- Include numbered sections (1, 1.1, 1.2, etc.)
- Write in third person (e.g., "Our company proposes...")
- Aim for 1500-2000 words
- Include a proposed timeline/project plan section

Begin the document with "Technical Solution & Approach" as the main heading."""

# Template for compliance declarations
COMPLIANCE_DECLARATION_TEMPLATE = """You are a legal and compliance expert for government tender submissions.

**Context:**
- Tender Title: {tender_title}
- Tender Reference: {tender_reference}
- Issuing Authority: {issuing_authority}
- Company: {company_name}

**Task:**
Generate a formal compliance declaration that includes:
1. Declaration of eligibility and legal standing
2. Confirmation of no conflicts of interest
3. Acknowledgment of tender terms and conditions
4. Affirmation of information accuracy
5. Commitment to fulfill contractual obligations
6. Standard declarations for Indian government tenders (if applicable):
   - No blacklisting
   - GST/PAN compliance
   - MSME status (if applicable)
   - Make in India compliance
   - No land border sharing country involvement (China clause)

**Format:**
- Use formal, legal language
- Include standard declaration phrases
- Add placeholders for signature, date, and company stamp
- Structure with numbered points
- Keep concise (300-500 words)

Begin with "DECLARATION" as the heading."""

# Template for covering letter
COVERING_LETTER_TEMPLATE = """You are a professional business communication expert writing covering letters for tender submissions.

**Context:**
- Tender Title: {tender_title}
- Tender Reference: {tender_reference}
- Issuing Authority: {issuing_authority}
- Company: {company_name}
- Company Profile: {company_profile}

**Task:**
Write a professional covering letter that:
1. Introduces our company and expresses interest in the tender
2. Briefly highlights our relevant qualifications and experience
3. Confirms understanding of requirements
4. States our commitment to quality and timely delivery
5. Provides key contact information
6. Thanks the authority for the opportunity

**Format:**
- Use standard business letter format
- Professional, respectful tone
- Concise (300-400 words)
- Include placeholders for:
  * Date
  * Recipient address
  * Reference number
  * Authorized signatory name and designation
  * Contact details

Begin with proper letter salutation."""

# Template for executive summary
EXECUTIVE_SUMMARY_TEMPLATE = """You are a strategic business writer creating executive summaries for tender proposals.

**Context:**
- Tender Title: {tender_title}
- Issuing Authority: {issuing_authority}
- Company: {company_name}
- Company Profile: {company_profile}
- Key Requirements: {requirements}

**Task:**
Write a compelling executive summary that:
1. Captures the essence of our proposal in 1-2 pages
2. Highlights our unique value proposition
3. Summarizes our technical approach
4. Emphasizes our competitive advantages
5. States expected outcomes and benefits
6. Demonstrates alignment with authority's objectives

**Format:**
- Clear, persuasive language
- Bullet points for key highlights
- 400-600 words
- Focus on value and outcomes, not just features

Begin with "Executive Summary" as the heading."""

# Template for methodology
METHODOLOGY_TEMPLATE = """You are a project methodology expert for government and enterprise projects.

**Context:**
- Tender Title: {tender_title}
- Scope of Work: {scope_of_work}
- Requirements: {requirements}
- Company: {company_name}

**Task:**
Develop a detailed project methodology that includes:
1. Overall approach and framework
2. Phase-wise breakdown of activities
3. Deliverables for each phase
4. Quality assurance procedures
5. Risk management approach
6. Stakeholder engagement plan
7. Project governance structure
8. Proposed timeline with milestones

**Format:**
- Use standard project management terminology
- Include phases: Initiation, Planning, Execution, Monitoring, Closure
- Add diagrams/workflow descriptions (in text format)
- 800-1200 words
- Professional, structured presentation

Begin with "Project Methodology" as the heading."""

# Template registry
TEMPLATES = {
    "technical_solution": TECHNICAL_SOLUTION_TEMPLATE,
    "compliance_declaration": COMPLIANCE_DECLARATION_TEMPLATE,
    "covering_letter": COVERING_LETTER_TEMPLATE,
    "executive_summary": EXECUTIVE_SUMMARY_TEMPLATE,
    "methodology": METHODOLOGY_TEMPLATE,
}


def get_template(generation_type: str) -> str:
    """
    Get prompt template by generation type.

    Args:
        generation_type: Type of document to generate

    Returns:
        Prompt template string
    """
    return TEMPLATES.get(generation_type, TECHNICAL_SOLUTION_TEMPLATE)


def format_template(generation_type: str, context: dict) -> str:
    """
    Format prompt template with context data.

    Args:
        generation_type: Type of document
        context: Dictionary with context values

    Returns:
        Formatted prompt string
    """
    template = get_template(generation_type)

    # Provide defaults for missing context
    safe_context = {
        "tender_title": context.get("tender_title", "[Tender Title]"),
        "tender_reference": context.get("tender_reference", "[Reference Number]"),
        "issuing_authority": context.get("issuing_authority", "[Issuing Authority]"),
        "company_name": context.get("company_name", "[Company Name]"),
        "company_profile": context.get("company_profile", "[Brief company profile]"),
        "experience": context.get("experience", "[Relevant experience]"),
        "requirements": context.get("requirements", "[Key requirements]"),
        "scope_of_work": context.get("scope_of_work", "[Scope of work description]"),
    }

    # Format requirements as bullet list if it's a list
    if isinstance(safe_context["requirements"], list):
        safe_context["requirements"] = "\n".join(f"- {req}" for req in safe_context["requirements"])

    return template.format(**safe_context)
