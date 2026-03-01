"""Document assembly service - PDF merging, cover page generation, and ZIP export."""
import os
import io
import zipfile
from typing import List, Optional, Dict, Any
from datetime import datetime
from uuid import UUID

from PyPDF2 import PdfReader, PdfWriter, PdfMerger
from reportlab.lib.pagesizes import A4, letter
from reportlab.lib.units import inch
from reportlab.lib import colors
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer, Image
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER, TA_LEFT, TA_RIGHT
from reportlab.pdfgen import canvas
from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select
import aiofiles

from app.core.config import settings
from app.models.document import Document
from app.core.security import TokenData


class AssemblyService:
    """Service for assembling tender documents (PDF merge, cover page, ZIP export)."""

    def __init__(self):
        """Initialize assembly service."""
        self.page_size = A4
        self.styles = getSampleStyleSheet()
        self._setup_custom_styles()

    def _setup_custom_styles(self):
        """Set up custom paragraph styles for cover page."""
        # Title style
        self.styles.add(ParagraphStyle(
            name='CoverTitle',
            parent=self.styles['Heading1'],
            fontSize=24,
            textColor=colors.HexColor('#003366'),
            spaceAfter=30,
            alignment=TA_CENTER,
            fontName='Helvetica-Bold'
        ))

        # Subtitle style
        self.styles.add(ParagraphStyle(
            name='CoverSubtitle',
            parent=self.styles['Normal'],
            fontSize=16,
            textColor=colors.HexColor('#336699'),
            spaceAfter=20,
            alignment=TA_CENTER,
            fontName='Helvetica'
        ))

        # Company info style
        self.styles.add(ParagraphStyle(
            name='CompanyInfo',
            parent=self.styles['Normal'],
            fontSize=12,
            spaceAfter=10,
            alignment=TA_CENTER,
            fontName='Helvetica'
        ))

    async def merge_pdfs(
        self,
        db: AsyncSession,
        document_ids: List[UUID],
        current_user: TokenData,
        add_cover: bool = False,
        cover_data: Optional[Dict[str, Any]] = None,
        output_filename: str = "merged_tender.pdf"
    ) -> str:
        """
        Merge multiple PDF documents into a single PDF.

        Args:
            db: Database session
            document_ids: List of document IDs to merge
            current_user: Current authenticated user
            add_cover: Whether to add a cover page
            cover_data: Data for cover page generation
            output_filename: Name for output file

        Returns:
            Path to merged PDF file
        """
        merger = PdfMerger()

        try:
            # Add cover page if requested
            if add_cover and cover_data:
                cover_pdf = self.generate_cover_page(cover_data)
                merger.append(cover_pdf)

            # Fetch and validate documents
            result = await db.execute(
                select(Document)
                .where(
                    Document.id.in_(document_ids),
                    Document.tenant_id == UUID(current_user.tenant_id)
                )
                .order_by(Document.created_at)
            )
            documents = result.scalars().all()

            if len(documents) != len(document_ids):
                raise ValueError("Some documents not found or access denied")

            # Merge PDFs in the order provided
            document_map = {str(doc.id): doc for doc in documents}
            for doc_id in document_ids:
                doc = document_map.get(str(doc_id))
                if not doc:
                    continue

                file_path = os.path.join(settings.upload_dir, doc.file_path)

                # Only merge PDF files
                if doc.mime_type == 'application/pdf':
                    if os.path.exists(file_path):
                        merger.append(file_path)
                    else:
                        print(f"Warning: File not found: {file_path}")

            # Save merged PDF
            output_dir = os.path.join(settings.upload_dir, "assembled")
            os.makedirs(output_dir, exist_ok=True)

            timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
            output_path = os.path.join(output_dir, f"{timestamp}_{output_filename}")

            merger.write(output_path)
            merger.close()

            # Return relative path
            return f"assembled/{timestamp}_{output_filename}"

        except Exception as e:
            merger.close()
            raise Exception(f"Failed to merge PDFs: {str(e)}")

    def generate_cover_page(self, cover_data: Dict[str, Any]) -> io.BytesIO:
        """
        Generate a professional cover page for tender submission.

        Args:
            cover_data: Dictionary with cover page information
                - tender_title: Title of the tender
                - tender_reference: Reference number
                - issuing_authority: Authority name
                - company_name: Submitting company name
                - company_address: Company address
                - submission_date: Date of submission
                - company_logo: Optional path to logo image

        Returns:
            BytesIO buffer containing the cover page PDF
        """
        buffer = io.BytesIO()
        doc = SimpleDocTemplate(buffer, pagesize=self.page_size)

        elements = []

        # Add company logo if provided
        if cover_data.get('company_logo') and os.path.exists(cover_data['company_logo']):
            try:
                logo = Image(cover_data['company_logo'], width=2*inch, height=1*inch)
                logo.hAlign = 'CENTER'
                elements.append(logo)
                elements.append(Spacer(1, 0.5*inch))
            except:
                pass  # Skip logo if there's an error

        # Company name
        company_name = cover_data.get('company_name', 'Company Name')
        elements.append(Paragraph(company_name, self.styles['CoverTitle']))
        elements.append(Spacer(1, 0.3*inch))

        # Horizontal line
        elements.append(Spacer(1, 0.2*inch))

        # Tender title
        elements.append(Spacer(1, 0.5*inch))
        elements.append(Paragraph("TENDER PROPOSAL", self.styles['CoverSubtitle']))
        elements.append(Spacer(1, 0.3*inch))

        tender_title = cover_data.get('tender_title', 'Tender Title')
        elements.append(Paragraph(tender_title, self.styles['CoverTitle']))
        elements.append(Spacer(1, 0.5*inch))

        # Tender details table
        tender_ref = cover_data.get('tender_reference', 'N/A')
        issuing_authority = cover_data.get('issuing_authority', 'N/A')
        submission_date = cover_data.get('submission_date', datetime.utcnow().strftime('%B %d, %Y'))

        details_data = [
            ['Tender Reference:', tender_ref],
            ['Issuing Authority:', issuing_authority],
            ['Submission Date:', submission_date],
        ]

        details_table = Table(details_data, colWidths=[2.5*inch, 3.5*inch])
        details_table.setStyle(TableStyle([
            ('ALIGN', (0, 0), (0, -1), 'RIGHT'),
            ('ALIGN', (1, 0), (1, -1), 'LEFT'),
            ('FONTNAME', (0, 0), (0, -1), 'Helvetica-Bold'),
            ('FONTNAME', (1, 0), (1, -1), 'Helvetica'),
            ('FONTSIZE', (0, 0), (-1, -1), 12),
            ('TEXTCOLOR', (0, 0), (0, -1), colors.HexColor('#003366')),
            ('BOTTOMPADDING', (0, 0), (-1, -1), 12),
            ('TOPPADDING', (0, 0), (-1, -1), 12),
        ]))

        elements.append(details_table)
        elements.append(Spacer(1, 1*inch))

        # Company information
        elements.append(Paragraph("SUBMITTED BY", self.styles['CoverSubtitle']))
        elements.append(Spacer(1, 0.2*inch))

        company_address = cover_data.get('company_address', '')
        if company_address:
            elements.append(Paragraph(company_address, self.styles['CompanyInfo']))

        company_contact = cover_data.get('company_contact', '')
        if company_contact:
            elements.append(Paragraph(company_contact, self.styles['CompanyInfo']))

        company_email = cover_data.get('company_email', '')
        if company_email:
            elements.append(Paragraph(f"Email: {company_email}", self.styles['CompanyInfo']))

        # Build PDF
        doc.build(elements)
        buffer.seek(0)
        return buffer

    async def reorder_pdf_pages(
        self,
        db: AsyncSession,
        document_id: UUID,
        page_order: List[int],
        current_user: TokenData,
        output_filename: str = "reordered.pdf"
    ) -> str:
        """
        Reorder pages in a PDF document.

        Args:
            db: Database session
            document_id: ID of the document to reorder
            page_order: List of page indices in desired order (0-based)
            current_user: Current authenticated user
            output_filename: Name for output file

        Returns:
            Path to reordered PDF file
        """
        # Fetch document
        result = await db.execute(
            select(Document)
            .where(
                Document.id == document_id,
                Document.tenant_id == UUID(current_user.tenant_id)
            )
        )
        document = result.scalar_one_or_none()

        if not document:
            raise ValueError("Document not found or access denied")

        if document.mime_type != 'application/pdf':
            raise ValueError("Document is not a PDF")

        file_path = os.path.join(settings.upload_dir, document.file_path)

        if not os.path.exists(file_path):
            raise ValueError("File not found on disk")

        # Read PDF
        reader = PdfReader(file_path)
        writer = PdfWriter()

        # Validate page order
        total_pages = len(reader.pages)
        if max(page_order) >= total_pages or min(page_order) < 0:
            raise ValueError(f"Invalid page order. Document has {total_pages} pages")

        # Add pages in specified order
        for page_num in page_order:
            writer.add_page(reader.pages[page_num])

        # Save reordered PDF
        output_dir = os.path.join(settings.upload_dir, "assembled")
        os.makedirs(output_dir, exist_ok=True)

        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
        output_path = os.path.join(output_dir, f"{timestamp}_{output_filename}")

        with open(output_path, 'wb') as output_file:
            writer.write(output_file)

        return f"assembled/{timestamp}_{output_filename}"

    async def remove_pdf_pages(
        self,
        db: AsyncSession,
        document_id: UUID,
        pages_to_remove: List[int],
        current_user: TokenData,
        output_filename: str = "modified.pdf"
    ) -> str:
        """
        Remove specified pages from a PDF document.

        Args:
            db: Database session
            document_id: ID of the document
            pages_to_remove: List of page indices to remove (0-based)
            current_user: Current authenticated user
            output_filename: Name for output file

        Returns:
            Path to modified PDF file
        """
        # Fetch document
        result = await db.execute(
            select(Document)
            .where(
                Document.id == document_id,
                Document.tenant_id == UUID(current_user.tenant_id)
            )
        )
        document = result.scalar_one_or_none()

        if not document:
            raise ValueError("Document not found or access denied")

        if document.mime_type != 'application/pdf':
            raise ValueError("Document is not a PDF")

        file_path = os.path.join(settings.upload_dir, document.file_path)

        if not os.path.exists(file_path):
            raise ValueError("File not found on disk")

        # Read PDF
        reader = PdfReader(file_path)
        writer = PdfWriter()

        # Add all pages except those to be removed
        total_pages = len(reader.pages)
        for i in range(total_pages):
            if i not in pages_to_remove:
                writer.add_page(reader.pages[i])

        # Save modified PDF
        output_dir = os.path.join(settings.upload_dir, "assembled")
        os.makedirs(output_dir, exist_ok=True)

        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
        output_path = os.path.join(output_dir, f"{timestamp}_{output_filename}")

        with open(output_path, 'wb') as output_file:
            writer.write(output_file)

        return f"assembled/{timestamp}_{output_filename}"

    async def export_tender_package(
        self,
        db: AsyncSession,
        document_ids: List[UUID],
        current_user: TokenData,
        package_name: str = "tender_package",
        include_cover: bool = False,
        cover_data: Optional[Dict[str, Any]] = None,
        merge_pdfs: bool = True
    ) -> str:
        """
        Export tender documents as a ZIP package.

        Args:
            db: Database session
            document_ids: List of document IDs to include
            current_user: Current authenticated user
            package_name: Name for the ZIP file
            include_cover: Whether to include a cover page
            cover_data: Data for cover page
            merge_pdfs: Whether to merge all PDFs into one file

        Returns:
            Path to ZIP file
        """
        # Fetch documents
        result = await db.execute(
            select(Document)
            .where(
                Document.id.in_(document_ids),
                Document.tenant_id == UUID(current_user.tenant_id)
            )
        )
        documents = result.scalars().all()

        if len(documents) != len(document_ids):
            raise ValueError("Some documents not found or access denied")

        # Create ZIP file
        output_dir = os.path.join(settings.upload_dir, "packages")
        os.makedirs(output_dir, exist_ok=True)

        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
        zip_filename = f"{package_name}_{timestamp}.zip"
        zip_path = os.path.join(output_dir, zip_filename)

        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
            # Add merged PDF if requested
            if merge_pdfs:
                merged_path = await self.merge_pdfs(
                    db=db,
                    document_ids=document_ids,
                    current_user=current_user,
                    add_cover=include_cover,
                    cover_data=cover_data,
                    output_filename=f"{package_name}_merged.pdf"
                )
                merged_full_path = os.path.join(settings.upload_dir, merged_path)
                if os.path.exists(merged_full_path):
                    zipf.write(merged_full_path, f"{package_name}_merged.pdf")

            # Add individual documents
            document_map = {str(doc.id): doc for doc in documents}
            for doc_id in document_ids:
                doc = document_map.get(str(doc_id))
                if not doc:
                    continue

                file_path = os.path.join(settings.upload_dir, doc.file_path)
                if os.path.exists(file_path):
                    # Use document name for the file in ZIP
                    ext = os.path.splitext(doc.file_path)[1]
                    zip_file_name = f"{doc.name}{ext}"
                    zipf.write(file_path, zip_file_name)

        return f"packages/{zip_filename}"


# Global service instance
assembly_service = AssemblyService()
