"""
Test script for AI generation endpoints.

This script tests the AI generation functionality without requiring
a full database setup or authentication.

Usage:
    python test_ai_generation.py
"""
import asyncio
import os
from app.services.docx_service import docx_service


def test_docx_service():
    """Test DOCX service generation."""
    print("=" * 60)
    print("Testing DOCX Service")
    print("=" * 60)

    # Sample AI-generated content
    sample_content = """# Technical Solution & Approach

## 1. Introduction

Our company proposes a comprehensive solution to address all requirements outlined in the tender document. This technical approach demonstrates our deep understanding of the project scope and our capability to deliver excellence.

## 2. Understanding of Requirements

We have carefully analyzed the following key requirements:
- Implementation of robust security measures
- Scalable architecture design
- 24/7 support and maintenance
- Compliance with industry standards

## 3. Proposed Methodology

### 3.1 Phase 1: Planning and Design
We will conduct detailed requirement analysis and create comprehensive design documents.

### 3.2 Phase 2: Development
Our experienced development team will implement the solution using industry best practices.

### 3.3 Phase 3: Testing and Quality Assurance
Rigorous testing will be performed to ensure the highest quality standards.

## 4. Timeline

The project will be completed in 12 weeks with weekly progress reports.

## 5. Conclusion

Our proven track record and expertise make us the ideal partner for this project."""

    try:
        # Test 1: Technical solution
        print("\n1. Generating technical solution DOCX...")
        buffer = docx_service.create_document_from_text(
            content=sample_content,
            title="Technical Solution & Approach",
            generation_type="technical_solution",
            company_name="Test Company Pvt Ltd",
            add_letterhead=True
        )

        # Save to file
        output_file = "test_technical_solution.docx"
        with open(output_file, "wb") as f:
            f.write(buffer.getvalue())

        print(f"   ✓ Successfully generated: {output_file}")
        print(f"   File size: {len(buffer.getvalue())} bytes")

        # Test 2: Covering letter
        print("\n2. Generating covering letter DOCX...")
        letter_content = """Date: [Date]

To,
The Procurement Officer
[Organization Name]
[Address]

Subject: Submission of Tender for [Tender Title]

Dear Sir/Madam,

We are pleased to submit our tender proposal for the above-referenced project. Our company, Test Company Pvt Ltd, has extensive experience in delivering similar projects with excellence.

We confirm our understanding of all tender requirements and our commitment to deliver the project within the specified timeline and budget. Our team of experts is ready to commence work immediately upon contract award.

We look forward to the opportunity to work with your esteemed organization.

Thank you for your consideration.

Yours faithfully,

[Authorized Signatory]
Managing Director
Test Company Pvt Ltd
Contact: +91-XXXXXXXXXX
Email: info@testcompany.com"""

        buffer = docx_service.format_covering_letter(
            content=letter_content,
            company_name="Test Company Pvt Ltd",
            address="123 Business Park, Tech City - 560001"
        )

        output_file = "test_covering_letter.docx"
        with open(output_file, "wb") as f:
            f.write(buffer.getvalue())

        print(f"   ✓ Successfully generated: {output_file}")
        print(f"   File size: {len(buffer.getvalue())} bytes")

        # Test 3: Compliance declaration
        print("\n3. Generating compliance declaration DOCX...")
        declaration_content = """We, Test Company Pvt Ltd, hereby declare:

1. ELIGIBILITY: We are a legally registered company in India and eligible to participate in this tender.

2. NO CONFLICTS: We have no conflicts of interest with the issuing authority or any related parties.

3. ACCURACY: All information provided in this tender submission is accurate and complete to the best of our knowledge.

4. COMPLIANCE: We agree to comply with all terms and conditions specified in the tender document.

5. BLACKLISTING: We have not been blacklisted or debarred by any government authority or organization.

6. GST COMPLIANCE: We are registered under GST (GSTIN: XXXXXXXXXXXX) and compliant with all tax regulations.

7. NO CHINA CLAUSE: We confirm that no equipment or component from countries sharing land border with India will be used.

8. COMMITMENT: We commit to fulfill all contractual obligations if awarded the contract.

We understand that any false declaration may result in disqualification and legal action."""

        buffer = docx_service.format_compliance_declaration(
            content=declaration_content,
            company_name="Test Company Pvt Ltd"
        )

        output_file = "test_compliance_declaration.docx"
        with open(output_file, "wb") as f:
            f.write(buffer.getvalue())

        print(f"   ✓ Successfully generated: {output_file}")
        print(f"   File size: {len(buffer.getvalue())} bytes")

        print("\n" + "=" * 60)
        print("✓ All DOCX generation tests passed!")
        print("=" * 60)
        print("\nGenerated files:")
        print("  - test_technical_solution.docx")
        print("  - test_covering_letter.docx")
        print("  - test_compliance_declaration.docx")
        print("\nPlease open these files in Microsoft Word to verify formatting.")

    except Exception as e:
        print(f"\n✗ Error during DOCX generation: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

    return True


def test_prompt_templates():
    """Test prompt template formatting."""
    print("\n" + "=" * 60)
    print("Testing Prompt Templates")
    print("=" * 60)

    from app.prompts.templates import format_template, TEMPLATES

    context = {
        "tender_title": "Implementation of E-Governance Platform",
        "tender_reference": "TENDER/2024/001",
        "issuing_authority": "Department of Information Technology",
        "company_name": "Test Company Pvt Ltd",
        "company_profile": "Leading IT solutions provider with 10+ years experience",
        "experience": "Successfully delivered 50+ government projects",
        "requirements": [
            "Cloud-based architecture",
            "Mobile responsive design",
            "99.9% uptime SLA",
            "24/7 support"
        ],
        "scope_of_work": "Design, develop, and deploy a comprehensive e-governance platform"
    }

    try:
        for template_name in TEMPLATES.keys():
            print(f"\n{template_name}:")
            prompt = format_template(template_name, context)
            print(f"  ✓ Generated {len(prompt)} characters")
            print(f"  First 100 chars: {prompt[:100]}...")

        print("\n" + "=" * 60)
        print("✓ All template formatting tests passed!")
        print("=" * 60)

    except Exception as e:
        print(f"\n✗ Error during template formatting: {str(e)}")
        import traceback
        traceback.print_exc()
        return False

    return True


def main():
    """Run all tests."""
    print("\n" + "=" * 60)
    print("TENDER AUTOMATION - AI GENERATION TESTS")
    print("=" * 60)

    all_passed = True

    # Test 1: Prompt templates
    if not test_prompt_templates():
        all_passed = False

    # Test 2: DOCX generation
    if not test_docx_service():
        all_passed = False

    # Summary
    print("\n" + "=" * 60)
    if all_passed:
        print("✓ ALL TESTS PASSED")
        print("=" * 60)
        print("\nNext steps:")
        print("1. Set ANTHROPIC_API_KEY environment variable")
        print("2. Run the FastAPI server: uvicorn app.main:app --reload")
        print("3. Test the /api/v1/ai/generate endpoint with real Claude API")
        print("4. Check generated DOCX files for formatting quality")
    else:
        print("✗ SOME TESTS FAILED")
        print("=" * 60)
        print("\nPlease fix the errors above before proceeding.")

    return 0 if all_passed else 1


if __name__ == "__main__":
    exit(main())
