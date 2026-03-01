"""Pydantic schemas for OEM, portal, EMD, and fee tracking."""
from typing import Optional, List
from datetime import datetime, date
from uuid import UUID
from pydantic import BaseModel, ConfigDict, Field


class OEMMasterBase(BaseModel):
    name: str = Field(..., max_length=300)
    country: Optional[str] = None
    is_indian: bool = True
    india_distributor: Optional[str] = None
    partner_tier: Optional[str] = None
    am_contact_name: Optional[str] = None
    am_contact_email: Optional[str] = None
    am_contact_phone: Optional[str] = None
    legal_contact_name: Optional[str] = None
    legal_contact_email: Optional[str] = None
    product_categories: Optional[List[str]] = None


class OEMMasterCreate(OEMMasterBase):
    pass

class OEMMasterUpdate(BaseModel):
    name: Optional[str] = None
    country: Optional[str] = None
    is_indian: Optional[bool] = None
    india_distributor: Optional[str] = None
    partner_tier: Optional[str] = None
    am_contact_name: Optional[str] = None
    am_contact_email: Optional[str] = None
    product_categories: Optional[List[str]] = None


class OEMMasterResponse(OEMMasterBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tenant_id: UUID
    partner_certificate_path: Optional[str] = None
    created_at: datetime
    updated_at: datetime


class OEMTenderRequirementBase(BaseModel):
    oem_id: UUID
    maf_status: str = "pending"
    ms_status: str = "pending"
    partner_cert_status: str = "pending"
    datasheet_status: str = "pending"
    compliance_cert_status: str = "pending"
    notes: Optional[str] = None


class OEMTenderRequirementCreate(OEMTenderRequirementBase):
    pass

class OEMTenderRequirementUpdate(BaseModel):
    maf_status: Optional[str] = None
    ms_status: Optional[str] = None
    partner_cert_status: Optional[str] = None
    datasheet_status: Optional[str] = None
    compliance_cert_status: Optional[str] = None
    notes: Optional[str] = None


class OEMTenderRequirementResponse(OEMTenderRequirementBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tender_id: UUID
    maf_document_path: Optional[str] = None
    maf_received_date: Optional[date] = None
    ms_document_path: Optional[str] = None
    partner_cert_path: Optional[str] = None
    datasheet_path: Optional[str] = None
    compliance_cert_path: Optional[str] = None
    created_at: datetime
    updated_at: datetime


class PortalRegistrationBase(BaseModel):
    portal_name: str = Field(..., max_length=200)
    portal_url: Optional[str] = None
    portal_type: Optional[str] = None
    registration_status: str = "active"
    registration_number: Optional[str] = None
    registered_email: Optional[str] = None
    dsc_class: Optional[str] = None
    dsc_holder_name: Optional[str] = None
    dsc_expiry_date: Optional[date] = None
    vendor_category: Optional[str] = None
    notes: Optional[str] = None


class PortalRegistrationCreate(PortalRegistrationBase):
    pass

class PortalRegistrationUpdate(BaseModel):
    portal_name: Optional[str] = None
    portal_url: Optional[str] = None
    registration_status: Optional[str] = None
    registration_number: Optional[str] = None
    dsc_class: Optional[str] = None
    dsc_holder_name: Optional[str] = None
    dsc_expiry_date: Optional[date] = None
    notes: Optional[str] = None


class PortalRegistrationResponse(PortalRegistrationBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tenant_id: UUID
    created_at: datetime
    updated_at: datetime


class EMDRecordBase(BaseModel):
    amount: float
    mode: str  # bg/dd/online/fixed_deposit/insurance_surety
    instrument_number: Optional[str] = None
    instrument_date: Optional[date] = None
    issuing_bank: Optional[str] = None
    validity_start_date: Optional[date] = None
    validity_end_date: Optional[date] = None
    submitted_date: Optional[date] = None
    status: str = "pending"
    notes: Optional[str] = None


class EMDRecordCreate(EMDRecordBase):
    pass

class EMDRecordUpdate(BaseModel):
    amount: Optional[float] = None
    mode: Optional[str] = None
    instrument_number: Optional[str] = None
    status: Optional[str] = None
    released_date: Optional[date] = None
    release_amount: Optional[float] = None
    notes: Optional[str] = None


class EMDRecordResponse(EMDRecordBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tender_id: UUID
    released_date: Optional[date] = None
    release_amount: Optional[float] = None
    created_at: datetime
    updated_at: datetime


class TenderFeeBase(BaseModel):
    fee_type: str
    amount: float
    payment_mode: Optional[str] = None
    payment_reference: Optional[str] = None
    payment_date: Optional[date] = None
    status: str = "pending"
    notes: Optional[str] = None


class TenderFeeCreate(TenderFeeBase):
    pass

class TenderFeeUpdate(BaseModel):
    amount: Optional[float] = None
    payment_mode: Optional[str] = None
    payment_reference: Optional[str] = None
    payment_date: Optional[date] = None
    status: Optional[str] = None
    notes: Optional[str] = None


class TenderFeeResponse(TenderFeeBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tender_id: UUID
    receipt_path: Optional[str] = None
    created_at: datetime
    updated_at: datetime


class DashboardSummary(BaseModel):
    active_tenders: int = 0
    total_bundles: int = 0
    total_certifications: int = 0
    active_portals: int = 0
    total_oems: int = 0
    total_emd_amount: float = 0
