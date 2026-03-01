"""Company profile, certifications, and personnel models."""
import uuid
from datetime import datetime

from sqlalchemy import Column, String, Text, DateTime, Integer, Boolean, ForeignKey, Uuid, JSON
from sqlalchemy.orm import relationship

from app.core.db import Base


class CompanyProfile(Base):
    """Company profile entity - master data for the tenant's company."""

    __tablename__ = "company_profiles"

    # Primary Key
    id = Column(Uuid, primary_key=True, default=uuid.uuid4)

    # Multi-tenancy (one profile per tenant)
    tenant_id = Column(Uuid, nullable=False, unique=True, index=True)

    # Company Information
    company_name = Column(String(500), nullable=False)
    cin_number = Column(String(50), nullable=True)
    pan_number = Column(String(20), nullable=True)
    gstin = Column(String(20), nullable=True)
    msme_registration = Column(String(50), nullable=True)

    # Addresses
    registered_address = Column(Text, nullable=True)
    corporate_address = Column(Text, nullable=True)

    # Contact
    website = Column(String(300), nullable=True)
    phone = Column(String(20), nullable=True)
    email = Column(String(255), nullable=True)

    # Financials
    annual_turnover = Column(JSON, nullable=True)  # {"2023-24": 50000000, "2022-23": 42000000}

    # Company Details
    year_established = Column(Integer, nullable=True)
    employee_count = Column(Integer, nullable=True)

    # Brand Assets (file paths)
    logo_path = Column(String(500), nullable=True)
    letterhead_path = Column(String(500), nullable=True)
    signature_path = Column(String(500), nullable=True)
    stamp_path = Column(String(500), nullable=True)

    # Bank Details
    bank_name = Column(String(200), nullable=True)
    account_number = Column(String(50), nullable=True)
    ifsc_code = Column(String(20), nullable=True)
    branch_name = Column(String(200), nullable=True)

    # Audit fields
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    # Relationships
    certifications = relationship("Certification", back_populates="company", cascade="all, delete-orphan", lazy="noload")
    personnel = relationship("Personnel", back_populates="company", cascade="all, delete-orphan", lazy="noload")

    def __repr__(self):
        return f"<CompanyProfile(id={self.id}, company_name='{self.company_name}')>"


class Certification(Base):
    """Certification entity - company certifications (ISO, CMMI, etc.)."""

    __tablename__ = "certifications"

    # Primary Key
    id = Column(Uuid, primary_key=True, default=uuid.uuid4)

    # Foreign Key
    company_id = Column(Uuid, ForeignKey("company_profiles.id", ondelete="CASCADE"), nullable=False, index=True)

    # Certification Info
    name = Column(String(300), nullable=False)
    cert_type = Column(String(100), nullable=True)  # iso, cmmi, quality, safety, etc.
    issuing_body = Column(String(300), nullable=True)
    certificate_number = Column(String(100), nullable=True)
    issue_date = Column(DateTime(timezone=True), nullable=True)
    expiry_date = Column(DateTime(timezone=True), nullable=True)
    document_path = Column(String(500), nullable=True)
    is_valid = Column(Boolean, nullable=False, default=True)

    # Audit fields
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    # Relationships
    company = relationship("CompanyProfile", back_populates="certifications")

    def __repr__(self):
        return f"<Certification(id={self.id}, name='{self.name}', valid={self.is_valid})>"


class Personnel(Base):
    """Personnel entity - key personnel for tender submissions."""

    __tablename__ = "personnel"

    # Primary Key
    id = Column(Uuid, primary_key=True, default=uuid.uuid4)

    # Foreign Key
    company_id = Column(Uuid, ForeignKey("company_profiles.id", ondelete="CASCADE"), nullable=False, index=True)

    # Personnel Info
    name = Column(String(300), nullable=False)
    designation = Column(String(200), nullable=True)
    role_in_tender = Column(String(200), nullable=True)  # project_manager, site_engineer, etc.
    qualification = Column(String(300), nullable=True)
    experience_years = Column(Integer, nullable=True)
    specialization = Column(JSON, nullable=True)  # stored as JSON array

    # Document paths
    cv_path = Column(String(500), nullable=True)
    experience_cert_path = Column(String(500), nullable=True)
    qualification_cert_path = Column(String(500), nullable=True)

    # Audit fields
    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)

    # Relationships
    company = relationship("CompanyProfile", back_populates="personnel")

    def __repr__(self):
        return f"<Personnel(id={self.id}, name='{self.name}', role='{self.role_in_tender}')>"
