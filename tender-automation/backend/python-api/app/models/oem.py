"""OEM master and tender requirement models."""
import uuid
from datetime import datetime, date

from sqlalchemy import Column, String, Text, DateTime, Date, Boolean, ForeignKey, Uuid, JSON

from app.core.db import Base


class OEMMaster(Base):
    __tablename__ = "oem_master"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(Uuid, nullable=False, index=True)

    name = Column(String(300), nullable=False)
    country = Column(String(100), nullable=True)
    is_indian = Column(Boolean, nullable=False, default=True)
    india_distributor = Column(String(300), nullable=True)
    partner_tier = Column(String(50), nullable=True)
    partner_certificate_path = Column(String(500), nullable=True)
    am_contact_name = Column(String(200), nullable=True)
    am_contact_email = Column(String(255), nullable=True)
    am_contact_phone = Column(String(20), nullable=True)
    legal_contact_name = Column(String(200), nullable=True)
    legal_contact_email = Column(String(255), nullable=True)
    product_categories = Column(JSON, nullable=True)  # stored as JSON array

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class OEMTenderRequirement(Base):
    __tablename__ = "oem_tender_requirements"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)
    oem_id = Column(Uuid, ForeignKey("oem_master.id", ondelete="CASCADE"), nullable=False, index=True)

    maf_status = Column(String(30), nullable=False, default="pending")
    maf_document_path = Column(String(500), nullable=True)
    maf_received_date = Column(Date, nullable=True)
    ms_status = Column(String(30), nullable=False, default="pending")
    ms_document_path = Column(String(500), nullable=True)
    partner_cert_status = Column(String(30), nullable=False, default="pending")
    partner_cert_path = Column(String(500), nullable=True)
    datasheet_status = Column(String(30), nullable=False, default="pending")
    datasheet_path = Column(String(500), nullable=True)
    compliance_cert_status = Column(String(30), nullable=False, default="pending")
    compliance_cert_path = Column(String(500), nullable=True)
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
