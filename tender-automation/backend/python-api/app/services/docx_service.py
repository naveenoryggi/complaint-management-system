"""DOCX document formatting service."""
from docx import Document
from docx.shared import Pt, Inches, RGBColor
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.enum.style import WD_STYLE_TYPE
from io import BytesIO
from typing import Optional
import os
from datetime import datetime


class DOCXService:
    """Service for creating formatted DOCX documents."""

    def __init__(self):
        """Initialize DOCX service."""
        self.default_font = "Calibri"
        self.heading_font = "Arial"

    def create_document_from_text(
        self,
        content: str,
        title: str,
        generation_type: str,
        company_name: str = None,
        add_letterhead: bool = True,
    ) -> BytesIO:
        """
        Create a formatted DOCX document from AI-generated text.

        Args:
            content: AI-generated text content
            title: Document title
            generation_type: Type of document (technical_solution, etc.)
            company_name: Company name for letterhead
            add_letterhead: Whether to add header/footer

        Returns:
            BytesIO object containing the DOCX file
        """
        doc = Document()

        # Set up styles
        self._configure_styles(doc)

        # Add letterhead if requested
        if add_letterhead:
            self._add_letterhead(doc, company_name or "Your Company")

        # Add document title
        if generation_type != "covering_letter":  # Letters have their own format
            title_para = doc.add_paragraph()
            title_run = title_para.add_run(title)
            title_run.font.size = Pt(18)
            title_run.font.bold = True
            title_run.font.color.rgb = RGBColor(0, 51, 102)  # Dark blue
            title_para.alignment = WD_ALIGN_PARAGRAPH.CENTER
            doc.add_paragraph()  # Blank line

        # Parse and format content
        self._parse_and_format_content(doc, content, generation_type)

        # Add footer
        if add_letterhead:
            self._add_footer(doc, company_name or "Your Company")

        # Save to BytesIO
        buffer = BytesIO()
        doc.save(buffer)
        buffer.seek(0)
        return buffer

    def _configure_styles(self, doc: Document):
        """Configure document styles."""
        styles = doc.styles

        # Normal style
        normal_style = styles['Normal']
        normal_font = normal_style.font
        normal_font.name = self.default_font
        normal_font.size = Pt(11)

        # Heading 1
        try:
            heading1 = styles['Heading 1']
            heading1.font.name = self.heading_font
            heading1.font.size = Pt(16)
            heading1.font.bold = True
            heading1.font.color.rgb = RGBColor(0, 51, 102)
        except KeyError:
            pass

        # Heading 2
        try:
            heading2 = styles['Heading 2']
            heading2.font.name = self.heading_font
            heading2.font.size = Pt(14)
            heading2.font.bold = True
            heading2.font.color.rgb = RGBColor(51, 102, 153)
        except KeyError:
            pass

    def _add_letterhead(self, doc: Document, company_name: str):
        """
        Add company letterhead to document header.

        Args:
            doc: Document object
            company_name: Company name
        """
        section = doc.sections[0]
        header = section.header

        # Company name in header
        header_para = header.paragraphs[0]
        header_run = header_para.add_run(company_name)
        header_run.font.size = Pt(14)
        header_run.font.bold = True
        header_run.font.color.rgb = RGBColor(0, 51, 102)
        header_para.alignment = WD_ALIGN_PARAGRAPH.CENTER

        # Add a horizontal line
        header.add_paragraph("_" * 80)

    def _add_footer(self, doc: Document, company_name: str):
        """
        Add footer with page numbers and company info.

        Args:
            doc: Document object
            company_name: Company name
        """
        section = doc.sections[0]
        footer = section.footer

        # Footer text
        footer_para = footer.paragraphs[0]
        footer_para.text = f"{company_name} | Generated on {datetime.utcnow().strftime('%B %d, %Y')}"
        footer_para.alignment = WD_ALIGN_PARAGRAPH.CENTER
        footer_run = footer_para.runs[0]
        footer_run.font.size = Pt(9)
        footer_run.font.color.rgb = RGBColor(128, 128, 128)

    def _parse_and_format_content(
        self,
        doc: Document,
        content: str,
        generation_type: str
    ):
        """
        Parse AI-generated content and format appropriately.

        Args:
            doc: Document object
            content: Raw text content
            generation_type: Type of document
        """
        lines = content.split('\n')

        for line in lines:
            line = line.strip()

            if not line:
                # Blank line
                doc.add_paragraph()
                continue

            # Detect headings by common patterns
            if self._is_heading_1(line):
                para = doc.add_heading(line.lstrip('#').strip(), level=1)
            elif self._is_heading_2(line):
                para = doc.add_heading(line.lstrip('#').strip(), level=2)
            elif self._is_heading_3(line):
                para = doc.add_heading(line.lstrip('#').strip(), level=3)
            elif line.startswith('- ') or line.startswith('* '):
                # Bullet point
                para = doc.add_paragraph(line[2:], style='List Bullet')
            elif line[0].isdigit() and '. ' in line[:5]:
                # Numbered list
                para = doc.add_paragraph(line.split('. ', 1)[1], style='List Number')
            else:
                # Normal paragraph
                para = doc.add_paragraph(line)
                para.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY

    def _is_heading_1(self, line: str) -> bool:
        """Check if line is a level 1 heading."""
        # Markdown style: # Heading
        if line.startswith('# ') and not line.startswith('## '):
            return True

        # All caps and relatively short
        if line.isupper() and len(line) < 60 and not line.endswith('.'):
            return True

        # Common heading keywords
        heading_keywords = [
            'TECHNICAL SOLUTION',
            'EXECUTIVE SUMMARY',
            'DECLARATION',
            'METHODOLOGY',
            'INTRODUCTION',
            'CONCLUSION',
        ]
        if any(keyword in line.upper() for keyword in heading_keywords):
            return True

        return False

    def _is_heading_2(self, line: str) -> bool:
        """Check if line is a level 2 heading."""
        # Markdown style: ## Heading
        if line.startswith('## ') and not line.startswith('### '):
            return True

        # Numbered heading like "1. Introduction" or "1.1 Background"
        if line[0].isdigit() and '. ' in line[:10] and len(line.split('. ', 1)[1].split()) < 8:
            return True

        return False

    def _is_heading_3(self, line: str) -> bool:
        """Check if line is a level 3 heading."""
        # Markdown style: ### Heading
        if line.startswith('### '):
            return True

        return False

    def format_covering_letter(
        self,
        content: str,
        company_name: str,
        address: Optional[str] = None,
    ) -> BytesIO:
        """
        Format a covering letter with proper business letter layout.

        Args:
            content: AI-generated letter content
            company_name: Company name
            address: Company address

        Returns:
            BytesIO object containing the DOCX file
        """
        doc = Document()
        self._configure_styles(doc)

        # Company letterhead
        header_para = doc.add_paragraph()
        header_run = header_para.add_run(company_name)
        header_run.font.size = Pt(16)
        header_run.font.bold = True
        header_run.font.color.rgb = RGBColor(0, 51, 102)
        header_para.alignment = WD_ALIGN_PARAGRAPH.CENTER

        if address:
            addr_para = doc.add_paragraph(address)
            addr_para.alignment = WD_ALIGN_PARAGRAPH.CENTER
            addr_para.runs[0].font.size = Pt(10)

        doc.add_paragraph()  # Blank line
        doc.add_paragraph("_" * 80)
        doc.add_paragraph()

        # Parse content
        self._parse_and_format_content(doc, content, "covering_letter")

        # Save to buffer
        buffer = BytesIO()
        doc.save(buffer)
        buffer.seek(0)
        return buffer

    def format_compliance_declaration(
        self,
        content: str,
        company_name: str,
    ) -> BytesIO:
        """
        Format a compliance declaration with signature section.

        Args:
            content: AI-generated declaration content
            company_name: Company name

        Returns:
            BytesIO object containing the DOCX file
        """
        doc = Document()
        self._configure_styles(doc)

        # Title
        title = doc.add_heading("DECLARATION", level=0)
        title.alignment = WD_ALIGN_PARAGRAPH.CENTER

        doc.add_paragraph()

        # Parse main content
        self._parse_and_format_content(doc, content, "compliance_declaration")

        # Add signature section
        doc.add_paragraph()
        doc.add_paragraph()

        sig_table = doc.add_table(rows=5, cols=2)
        sig_table.style = 'Table Grid'

        sig_table.rows[0].cells[0].text = "Authorized Signatory:"
        sig_table.rows[1].cells[0].text = "Name:"
        sig_table.rows[2].cells[0].text = "Designation:"
        sig_table.rows[3].cells[0].text = "Date:"
        sig_table.rows[4].cells[0].text = "Company Stamp:"

        # Save to buffer
        buffer = BytesIO()
        doc.save(buffer)
        buffer.seek(0)
        return buffer


# Global service instance
docx_service = DOCXService()
