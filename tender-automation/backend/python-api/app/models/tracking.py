"""Portal registration, EMD, and tender fee models."""
import uuid
from datetime import datetime, date

from sqlalchemy import Column, String, Text, DateTime, Date, Float, Boolean, ForeignKey, Uuid

from app.core.db import Base


class PortalRegistration(Base):
    __tablename__ = "portal_registrations"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tenant_id = Column(Uuid, nullable=False, index=True)

    portal_name = Column(String(200), nullable=False)
    portal_url = Column(String(500), nullable=True)
    portal_type = Column(String(50), nullable=True)
    registration_status = Column(String(30), nullable=False, default="active")
    registration_number = Column(String(100), nullable=True)
    registered_email = Column(String(255), nullable=True)
    dsc_class = Column(String(20), nullable=True)
    dsc_holder_name = Column(String(200), nullable=True)
    dsc_expiry_date = Column(Date, nullable=True)
    vendor_category = Column(String(100), nullable=True)
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class EMDRecord(Base):
    __tablename__ = "emd_records"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    amount = Column(Float, nullable=False)
    mode = Column(String(50), nullable=False)  # bg/dd/online/fixed_deposit/insurance_surety
    instrument_number = Column(String(100), nullable=True)
    instrument_date = Column(Date, nullable=True)
    issuing_bank = Column(String(200), nullable=True)
    validity_start_date = Column(Date, nullable=True)
    validity_end_date = Column(Date, nullable=True)
    submitted_date = Column(Date, nullable=True)
    released_date = Column(Date, nullable=True)
    release_amount = Column(Float, nullable=True)
    status = Column(String(30), nullable=False, default="pending")
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)


class TenderFee(Base):
    __tablename__ = "tender_fees"

    id = Column(Uuid, primary_key=True, default=uuid.uuid4)
    tender_id = Column(Uuid, ForeignKey("tenders.id", ondelete="CASCADE"), nullable=False, index=True)

    fee_type = Column(String(50), nullable=False)  # tender_fee/processing_fee/document_fee
    amount = Column(Float, nullable=False)
    payment_mode = Column(String(50), nullable=True)
    payment_reference = Column(String(100), nullable=True)
    payment_date = Column(Date, nullable=True)
    status = Column(String(30), nullable=False, default="pending")
    receipt_path = Column(String(500), nullable=True)
    notes = Column(Text, nullable=True)

    created_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow)
    updated_at = Column(DateTime(timezone=True), nullable=False, default=datetime.utcnow, onupdate=datetime.utcnow)
