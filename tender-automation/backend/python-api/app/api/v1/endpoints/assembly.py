"""Document assembly API endpoints."""
from typing import List, Optional, Dict, Any
from uuid import UUID
from fastapi import APIRouter, Depends, HTTPException, status
from fastapi.responses import FileResponse
from pydantic import BaseModel, Field
from sqlalchemy.ext.asyncio import AsyncSession
import os

from app.core.db import get_db
from app.core.security import get_current_user, TokenData
from app.core.config import settings
from app.services.assembly_service import assembly_service

router = APIRouter()


# Request schemas
class MergePDFRequest(BaseModel):
    """Request to merge multiple PDFs."""
    document_ids: List[UUID] = Field(..., min_length=1, description="List of document IDs to merge")
    add_cover: bool = Field(False, description="Add a cover page")
    cover_data: Optional[Dict[str, Any]] = Field(None, description="Cover page data")
    output_filename: str = Field("merged_tender.pdf", description="Output filename")


class ReorderPagesRequest(BaseModel):
    """Request to reorder PDF pages."""
    document_id: UUID = Field(..., description="Document ID")
    page_order: List[int] = Field(..., min_length=1, description="New page order (0-based indices)")
    output_filename: str = Field("reordered.pdf", description="Output filename")


class RemovePagesRequest(BaseModel):
    """Request to remove pages from PDF."""
    document_id: UUID = Field(..., description="Document ID")
    pages_to_remove: List[int] = Field(..., min_length=1, description="Pages to remove (0-based indices)")
    output_filename: str = Field("modified.pdf", description="Output filename")


class ExportPackageRequest(BaseModel):
    """Request to export tender package as ZIP."""
    document_ids: List[UUID] = Field(..., min_length=1, description="List of document IDs")
    package_name: str = Field("tender_package", description="Package name")
    include_cover: bool = Field(False, description="Include cover page")
    cover_data: Optional[Dict[str, Any]] = Field(None, description="Cover page data")
    merge_pdfs: bool = Field(True, description="Merge PDFs into single file")


# Response schemas
class AssemblyResponse(BaseModel):
    """Response for assembly operations."""
    success: bool
    file_path: str
    message: str


@router.post("/merge", response_model=AssemblyResponse)
async def merge_pdfs(
    request: MergePDFRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Merge multiple PDF documents into a single PDF.

    **Features:**
    - Merge 2+ PDFs in specified order
    - Optional cover page with company letterhead
    - Maintains PDF quality and formatting

    **Cover Page Data:**
    - tender_title: Tender title
    - tender_reference: Reference number
    - issuing_authority: Authority name
    - company_name: Your company name
    - company_address: Full address
    - company_contact: Contact number
    - company_email: Email address
    - submission_date: Submission date
    - company_logo: Path to logo image (optional)

    **Example:**
    ```json
    {
        "document_ids": ["uuid1", "uuid2", "uuid3"],
        "add_cover": true,
        "cover_data": {
            "tender_title": "Smart City Platform Implementation",
            "tender_reference": "TENDER/2024/001",
            "issuing_authority": "Municipal Corporation",
            "company_name": "TechSolutions Pvt Ltd",
            "company_address": "123 Business Park, Tech City - 560001",
            "company_contact": "+91-9876543210",
            "company_email": "info@techsolutions.com"
        }
    }
    ```

    Requires authentication.
    """
    try:
        file_path = await assembly_service.merge_pdfs(
            db=db,
            document_ids=request.document_ids,
            current_user=current_user,
            add_cover=request.add_cover,
            cover_data=request.cover_data,
            output_filename=request.output_filename
        )

        return AssemblyResponse(
            success=True,
            file_path=file_path,
            message=f"Successfully merged {len(request.document_ids)} documents"
        )

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to merge PDFs: {str(e)}"
        )


@router.post("/reorder", response_model=AssemblyResponse)
async def reorder_pages(
    request: ReorderPagesRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Reorder pages in a PDF document.

    **Example:**
    ```json
    {
        "document_id": "uuid",
        "page_order": [2, 0, 1, 3],  // New order: page 3 first, then 1, 2, 4
        "output_filename": "reordered_proposal.pdf"
    }
    ```

    **Notes:**
    - Page indices are 0-based (first page = 0)
    - Original document is not modified
    - Creates a new document with reordered pages

    Requires authentication.
    """
    try:
        file_path = await assembly_service.reorder_pdf_pages(
            db=db,
            document_id=request.document_id,
            page_order=request.page_order,
            current_user=current_user,
            output_filename=request.output_filename
        )

        return AssemblyResponse(
            success=True,
            file_path=file_path,
            message=f"Successfully reordered {len(request.page_order)} pages"
        )

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to reorder pages: {str(e)}"
        )


@router.post("/remove-pages", response_model=AssemblyResponse)
async def remove_pages(
    request: RemovePagesRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Remove specified pages from a PDF document.

    **Example:**
    ```json
    {
        "document_id": "uuid",
        "pages_to_remove": [0, 5, 10],  // Remove pages 1, 6, and 11
        "output_filename": "cleaned_document.pdf"
    }
    ```

    **Notes:**
    - Page indices are 0-based
    - Original document is preserved
    - Creates a new document without specified pages

    Requires authentication.
    """
    try:
        file_path = await assembly_service.remove_pdf_pages(
            db=db,
            document_id=request.document_id,
            pages_to_remove=request.pages_to_remove,
            current_user=current_user,
            output_filename=request.output_filename
        )

        return AssemblyResponse(
            success=True,
            file_path=file_path,
            message=f"Successfully removed {len(request.pages_to_remove)} pages"
        )

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to remove pages: {str(e)}"
        )


@router.post("/export-package", response_model=AssemblyResponse)
async def export_package(
    request: ExportPackageRequest,
    db: AsyncSession = Depends(get_db),
    current_user: TokenData = Depends(get_current_user),
):
    """
    Export tender documents as a ZIP package.

    **Features:**
    - Include multiple documents (PDFs, DOCX, etc.)
    - Optional merged PDF with cover page
    - All individual files included
    - Ready for tender submission

    **Example:**
    ```json
    {
        "document_ids": ["uuid1", "uuid2", "uuid3"],
        "package_name": "SmartCity_Tender_2024",
        "include_cover": true,
        "cover_data": {
            "tender_title": "Smart City Platform",
            "company_name": "TechSolutions Pvt Ltd"
        },
        "merge_pdfs": true
    }
    ```

    **ZIP Contents:**
    - `[package_name]_merged.pdf` (if merge_pdfs = true)
    - Individual document files with original names
    - Cover page included in merged PDF (if include_cover = true)

    Requires authentication.
    """
    try:
        file_path = await assembly_service.export_tender_package(
            db=db,
            document_ids=request.document_ids,
            current_user=current_user,
            package_name=request.package_name,
            include_cover=request.include_cover,
            cover_data=request.cover_data,
            merge_pdfs=request.merge_pdfs
        )

        return AssemblyResponse(
            success=True,
            file_path=file_path,
            message=f"Successfully created tender package with {len(request.document_ids)} documents"
        )

    except ValueError as e:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail=str(e)
        )
    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to export package: {str(e)}"
        )


@router.get("/download/{file_type}/{filename}")
async def download_assembled_file(
    file_type: str,
    filename: str,
    current_user: TokenData = Depends(get_current_user),
):
    """
    Download an assembled file (merged PDF or ZIP package).

    **Parameters:**
    - file_type: "assembled" or "packages"
    - filename: Name of the file

    **Example:**
    - GET /download/assembled/20240224_123456_merged_tender.pdf
    - GET /download/packages/tender_package_20240224_123456.zip

    Requires authentication.
    """
    if file_type not in ['assembled', 'packages']:
        raise HTTPException(
            status_code=status.HTTP_400_BAD_REQUEST,
            detail="Invalid file type. Must be 'assembled' or 'packages'"
        )

    file_path = os.path.join(settings.upload_dir, file_type, filename)

    if not os.path.exists(file_path):
        raise HTTPException(
            status_code=status.HTTP_404_NOT_FOUND,
            detail="File not found"
        )

    # Determine media type
    media_type = "application/pdf" if filename.endswith('.pdf') else "application/zip"

    return FileResponse(
        path=file_path,
        media_type=media_type,
        filename=filename
    )


@router.post("/generate-cover", response_model=AssemblyResponse)
async def generate_cover_page(
    cover_data: Dict[str, Any],
    current_user: TokenData = Depends(get_current_user),
):
    """
    Generate a standalone cover page PDF.

    **Example:**
    ```json
    {
        "tender_title": "Implementation of Smart City Platform",
        "tender_reference": "TENDER/2024/SC/001",
        "issuing_authority": "Municipal Corporation of Greater Mumbai",
        "company_name": "SmartTech Solutions Pvt Ltd",
        "company_address": "Plot No. 123, Tech Park, Bangalore - 560001, India",
        "company_contact": "+91-9876543210",
        "company_email": "info@smarttech.com",
        "submission_date": "March 15, 2024"
    }
    ```

    **Optional Fields:**
    - company_logo: Path to logo image file

    Requires authentication.
    """
    try:
        # Generate cover page
        cover_buffer = assembly_service.generate_cover_page(cover_data)

        # Save to file
        output_dir = os.path.join(settings.upload_dir, "assembled")
        os.makedirs(output_dir, exist_ok=True)

        from datetime import datetime
        timestamp = datetime.utcnow().strftime("%Y%m%d_%H%M%S")
        filename = f"{timestamp}_cover_page.pdf"
        file_path = os.path.join(output_dir, filename)

        with open(file_path, 'wb') as f:
            f.write(cover_buffer.getvalue())

        return AssemblyResponse(
            success=True,
            file_path=f"assembled/{filename}",
            message="Cover page generated successfully"
        )

    except Exception as e:
        raise HTTPException(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            detail=f"Failed to generate cover page: {str(e)}"
        )
