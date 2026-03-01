"""Unit tests for document service."""
import pytest
from uuid import uuid4
from fastapi import UploadFile
from io import BytesIO

from app.services.document_service import document_service
from app.schemas.document import DocumentCreate, DocumentUpdate, DocumentSearchRequest
from app.core.security import TokenData


# Mock current user
def get_mock_user():
    """Create a mock authenticated user."""
    return TokenData({
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": str(uuid4()),
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "test@example.com",
        "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "Test User",
        "CompanyId": str(uuid4()),
        "TenantId": str(uuid4()),
        "EmployeeCode": "EMP001",
        "Permission": ["ManageDocuments"],
        "RoleCode": ["USER"],
        "PortalUser": "false",
        "exp": 9999999999,
        "iat": 1234567890,
        "iss": "ComplaintManagement",
        "aud": "ComplaintManagement",
    })


def create_mock_file(filename: str = "test.pdf", content: bytes = b"test content"):
    """Create a mock UploadFile."""
    return UploadFile(
        filename=filename,
        file=BytesIO(content),
    )


@pytest.mark.asyncio
async def test_document_create():
    """Test document creation."""
    # This is a basic structure - you'll need to set up a test database
    # For now, this serves as a template

    mock_user = get_mock_user()
    mock_file = create_mock_file()

    document_data = DocumentCreate(
        name="Test Document",
        description="A test document",
        document_type="certificate",
        tags=["test", "example"],
        is_template=False,
    )

    # In a real test, you would:
    # 1. Set up a test database session
    # 2. Call document_service.upload_document
    # 3. Assert the document was created correctly
    # 4. Clean up test data

    assert document_data.name == "Test Document"
    assert "test" in document_data.tags


@pytest.mark.asyncio
async def test_document_search():
    """Test document search functionality."""
    search_request = DocumentSearchRequest(
        query="test",
        document_type="certificate",
        page=1,
        page_size=20,
    )

    assert search_request.query == "test"
    assert search_request.page == 1


# Add more tests as needed:
# - test_document_update
# - test_document_delete
# - test_tenant_isolation
# - test_file_upload_validation
# - test_search_with_tags
