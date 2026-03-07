"""Cover assembly service — stage documents into covers, assemble cover PDFs, export ZIP.

Pipeline:
1. Checklist items get `linked_document_id` when user stages a document
2. Per-cover assembly: generates cover title page + merges all linked PDFs in sort order
3. Full bid package: assembles all covers into a ZIP with cover-level PDFs
"""
import os
import io
import zipfile
from typing import Optional, Dict, Any
from datetime import datetime
from uuid import UUID

from PyPDF2 import PdfMerger
from reportlab.lib.pagesizes import A4
from reportlab.lib.units import inch
from reportlab.lib import colors
from reportlab.platypus import SimpleDocTemplate, Table, TableStyle, Paragraph, Spacer
from reportlab.lib.styles import getSampleStyleSheet, ParagraphStyle
from reportlab.lib.enums import TA_CENTER

from sqlalchemy.ext.asyncio import AsyncSession
from sqlalchemy import select

from app.core.config import settings
from app.models.document import Document
from app.models.submission_checklist import SubmissionChecklistItem
from app.models.tender import Tender
from app.core.security import TokenData


class CoverAssemblyService:
    """Assembles staged documents into cover-level PDFs for tender submission."""

    def __init__(self):
        self.page_size = A4
        self.styles = getSampleStyleSheet()
        self._setup_styles()

    def _setup_styles(self):
        self.styles.add(ParagraphStyle(
            name='CoverPageTitle',
            parent=self.styles['Heading1'],
            fontSize=28,
            textColor=colors.HexColor('#003366'),
            spaceAfter=20,
            alignment=TA_CENTER,
            fontName='Helvetica-Bold'
        ))
        self.styles.add(ParagraphStyle(
            name='CoverPageSubtitle',
            parent=self.styles['Normal'],
            fontSize=16,
            textColor=colors.HexColor('#336699'),
            spaceAfter=12,
            alignment=TA_CENTER,
            fontName='Helvetica'
        ))
        self.styles.add(ParagraphStyle(
            name='CoverPageInfo',
            parent=self.styles['Normal'],
            fontSize=12,
            spaceAfter=8,
            alignment=TA_CENTER,
            fontName='Helvetica'
        ))
        self.styles.add(ParagraphStyle(
            name='CoverDocListItem',
            parent=self.styles['Normal'],
            fontSize=11,
            spaceBefore=4,
            spaceAfter=4,
            fontName='Helvetica',
            leftIndent=20,
        ))

    def generate_cover_title_page(
        self,
        cover_name: str,
        tender_title: str,
        tender_reference: str,
        company_name: str,
        document_list: list[dict],
        envelope: str = "",
    ) -> io.BytesIO:
        """Generate a title page for a specific cover/envelope.

        Includes cover name, tender info, and an index of documents inside.
        """
        buffer = io.BytesIO()
        doc = SimpleDocTemplate(buffer, pagesize=self.page_size)
        elements = []

        # Cover envelope label
        elements.append(Spacer(1, 1 * inch))
        elements.append(Paragraph(cover_name.upper(), self.styles['CoverPageTitle']))
        elements.append(Spacer(1, 0.3 * inch))

        # Envelope code
        if envelope:
            envelope_labels = {
                "envelope_a": "ENVELOPE A - TECHNICAL BID",
                "envelope_b": "ENVELOPE B - FINANCIAL BID",
                "superscription": "SUPERSCRIPTION ENVELOPE",
            }
            label = envelope_labels.get(envelope, envelope.upper())
            elements.append(Paragraph(label, self.styles['CoverPageSubtitle']))
            elements.append(Spacer(1, 0.3 * inch))

        # Tender details
        elements.append(Paragraph(f"Tender: {tender_title}", self.styles['CoverPageInfo']))
        if tender_reference:
            elements.append(Paragraph(f"Reference: {tender_reference}", self.styles['CoverPageInfo']))
        elements.append(Paragraph(f"Submitted by: {company_name}", self.styles['CoverPageInfo']))
        elements.append(Paragraph(
            f"Date: {datetime.utcnow().strftime('%B %d, %Y')}",
            self.styles['CoverPageInfo']
        ))
        elements.append(Spacer(1, 0.5 * inch))

        # Document index table
        if document_list:
            elements.append(Paragraph("DOCUMENT INDEX", self.styles['CoverPageSubtitle']))
            elements.append(Spacer(1, 0.2 * inch))

            table_data = [["#", "Document Name", "Critical", "Status"]]
            for idx, d in enumerate(document_list, 1):
                critical = "YES" if d.get("is_critical") else ""
                status = d.get("status", "")
                table_data.append([str(idx), d["document_name"][:60], critical, status])

            col_widths = [0.4 * inch, 3.5 * inch, 0.7 * inch, 1.2 * inch]
            table = Table(table_data, colWidths=col_widths)
            table.setStyle(TableStyle([
                ('BACKGROUND', (0, 0), (-1, 0), colors.HexColor('#003366')),
                ('TEXTCOLOR', (0, 0), (-1, 0), colors.white),
                ('FONTNAME', (0, 0), (-1, 0), 'Helvetica-Bold'),
                ('FONTSIZE', (0, 0), (-1, -1), 9),
                ('ALIGN', (0, 0), (0, -1), 'CENTER'),
                ('ALIGN', (2, 0), (2, -1), 'CENTER'),
                ('GRID', (0, 0), (-1, -1), 0.5, colors.grey),
                ('ROWBACKGROUNDS', (0, 1), (-1, -1), [colors.white, colors.HexColor('#f5f5f5')]),
                ('TOPPADDING', (0, 0), (-1, -1), 6),
                ('BOTTOMPADDING', (0, 0), (-1, -1), 6),
            ]))
            elements.append(table)

        doc.build(elements)
        buffer.seek(0)
        return buffer

    async def assemble_cover(
        self,
        db: AsyncSession,
        tender_id: UUID,
        cover_name: str,
        current_user: TokenData,
        include_title_page: bool = True,
    ) -> dict:
        """Assemble all staged (linked) documents for a specific cover into a single PDF.

        Returns: {"file_path": "assembled/...", "document_count": N, "cover_name": "..."}
        """
        # Load tender
        tender_result = await db.execute(select(Tender).where(Tender.id == tender_id))
        tender = tender_result.scalars().first()
        if not tender:
            raise ValueError("Tender not found")

        # Get checklist items for this cover that have linked documents
        result = await db.execute(
            select(SubmissionChecklistItem)
            .where(
                SubmissionChecklistItem.tender_id == tender_id,
                SubmissionChecklistItem.cover_name == cover_name,
            )
            .order_by(SubmissionChecklistItem.sort_order)
        )
        items = result.scalars().all()

        if not items:
            raise ValueError(f"No checklist items found for cover '{cover_name}'")

        # Separate linked vs unlinked
        linked_items = [i for i in items if i.linked_document_id]
        if not linked_items:
            raise ValueError(f"No documents staged for cover '{cover_name}'. Link documents to checklist items first.")

        # Fetch actual document records
        doc_ids = [i.linked_document_id for i in linked_items]
        doc_result = await db.execute(
            select(Document)
            .where(
                Document.id.in_(doc_ids),
                Document.tenant_id == UUID(current_user.tenant_id)
            )
        )
        documents = {str(d.id): d for d in doc_result.scalars().all()}

        # Build document list for title page
        document_list = []
        for item in items:
            status = "Staged" if item.linked_document_id else "Not staged"
            if item.linked_document_id and str(item.linked_document_id) in documents:
                doc = documents[str(item.linked_document_id)]
                if doc.mime_type == "application/pdf":
                    status = "Ready (PDF)"
                else:
                    status = f"Ready ({doc.mime_type})"
            document_list.append({
                "document_name": item.document_name,
                "is_critical": item.is_critical,
                "status": status,
            })

        # Get company name
        from app.models.company import CompanyProfile
        profile_result = await db.execute(
            select(CompanyProfile).where(CompanyProfile.tenant_id == UUID(current_user.tenant_id))
        )
        profile = profile_result.scalars().first()
        company_name = profile.company_name if profile and profile.company_name else "Company"

        # Merge PDFs
        merger = PdfMerger()
        try:
            # Add title page
            if include_title_page:
                envelope = items[0].envelope if items else ""
                title_page = self.generate_cover_title_page(
                    cover_name=cover_name,
                    tender_title=tender.title or "",
                    tender_reference=tender.reference_number or "",
                    company_name=company_name,
                    document_list=document_list,
                    envelope=envelope,
                )
                merger.append(title_page)

            # Add linked documents in sort order
            merged_count = 0
            for item in linked_items:
                doc = documents.get(str(item.linked_document_id))
                if not doc:
                    continue
                file_path = os.path.join(settings.upload_dir, doc.file_path)
                if doc.mime_type == "application/pdf" and os.path.exists(file_path):
                    merger.append(file_path)
                    merged_count += 1

            if merged_count == 0 and not include_title_page:
                raise ValueError("No PDF documents found to assemble")

            # Save
            output_dir = os.path.join(settings.upload_dir, "assembled")
            os.makedirs(output_dir, exist_ok=True)

            sanitized = cover_name.replace(" ", "_").replace("/", "_").replace("\\", "_")[:50]
            timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
            filename = f"{timestamp}_{sanitized}.pdf"
            output_path = os.path.join(output_dir, filename)

            merger.write(output_path)
            merger.close()

            return {
                "file_path": f"assembled/{filename}",
                "document_count": merged_count,
                "cover_name": cover_name,
                "total_items": len(items),
                "staged_items": len(linked_items),
            }

        except Exception as e:
            merger.close()
            raise Exception(f"Failed to assemble cover: {str(e)}")

    async def assemble_all_covers(
        self,
        db: AsyncSession,
        tender_id: UUID,
        current_user: TokenData,
        include_title_pages: bool = True,
    ) -> dict:
        """Assemble all covers for a tender into a ZIP with one PDF per cover.

        Returns: {"file_path": "packages/...", "covers": [...]}
        """
        # Load tender
        tender_result = await db.execute(select(Tender).where(Tender.id == tender_id))
        tender = tender_result.scalars().first()
        if not tender:
            raise ValueError("Tender not found")

        # Get distinct covers that have at least one linked document
        result = await db.execute(
            select(SubmissionChecklistItem)
            .where(
                SubmissionChecklistItem.tender_id == tender_id,
                SubmissionChecklistItem.linked_document_id.isnot(None),
            )
            .order_by(SubmissionChecklistItem.sort_order)
        )
        items = result.scalars().all()

        cover_names = list(dict.fromkeys(i.cover_name or i.envelope or "Unassigned" for i in items))

        if not cover_names:
            raise ValueError("No documents staged in any cover")

        # Assemble each cover
        cover_results = []
        cover_file_paths = []

        for cn in cover_names:
            try:
                cover_result = await self.assemble_cover(
                    db, tender_id, cn, current_user, include_title_pages
                )
                cover_results.append(cover_result)
                cover_file_paths.append(cover_result["file_path"])
            except ValueError:
                cover_results.append({
                    "cover_name": cn,
                    "error": "No staged documents or PDFs found",
                })

        # Create ZIP
        output_dir = os.path.join(settings.upload_dir, "packages")
        os.makedirs(output_dir, exist_ok=True)

        sanitized_title = (tender.title or "tender").replace(" ", "_")[:40]
        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
        zip_filename = f"{sanitized_title}_bid_package_{timestamp}.zip"
        zip_path = os.path.join(output_dir, zip_filename)

        with zipfile.ZipFile(zip_path, 'w', zipfile.ZIP_DEFLATED) as zipf:
            for fp in cover_file_paths:
                full_path = os.path.join(settings.upload_dir, fp)
                if os.path.exists(full_path):
                    arcname = os.path.basename(fp)
                    zipf.write(full_path, arcname)

        return {
            "file_path": f"packages/{zip_filename}",
            "covers": cover_results,
        }


# Global service instance
cover_assembly_service = CoverAssemblyService()
