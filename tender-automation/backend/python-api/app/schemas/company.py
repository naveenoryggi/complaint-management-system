"""Pydantic schemas for company profile."""
from typing import Optional, List, Dict, Any
from datetime import datetime
from uuid import UUID
from pydantic import BaseModel, ConfigDict, Field


class CompanyProfileBase(BaseModel):
    company_name: str = Field(..., max_length=500)
    cin_number: Optional[str] = None
    pan_number: Optional[str] = None
    gstin: Optional[str] = None
    msme_registration: Optional[str] = None
    registered_address: Optional[str] = None
    corporate_address: Optional[str] = None
    website: Optional[str] = None
    phone: Optional[str] = None
    email: Optional[str] = None
    annual_turnover: Optional[Dict[str, Any]] = None
    year_established: Optional[int] = None
    employee_count: Optional[int] = None
    bank_name: Optional[str] = None
    account_number: Optional[str] = None
    ifsc_code: Optional[str] = None
    branch_name: Optional[str] = None


class CompanyProfileCreate(CompanyProfileBase):
    pass


class CompanyProfileUpdate(BaseModel):
    company_name: Optional[str] = None
    cin_number: Optional[str] = None
    pan_number: Optional[str] = None
    gstin: Optional[str] = None
    msme_registration: Optional[str] = None
    registered_address: Optional[str] = None
    corporate_address: Optional[str] = None
    website: Optional[str] = None
    phone: Optional[str] = None
    email: Optional[str] = None
    annual_turnover: Optional[Dict[str, Any]] = None
    year_established: Optional[int] = None
    employee_count: Optional[int] = None
    bank_name: Optional[str] = None
    account_number: Optional[str] = None
    ifsc_code: Optional[str] = None
    branch_name: Optional[str] = None


class CompanyProfileResponse(CompanyProfileBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tenant_id: UUID
    logo_path: Optional[str] = None
    letterhead_path: Optional[str] = None
    signature_path: Optional[str] = None
    stamp_path: Optional[str] = None
    created_at: datetime
    updated_at: datetime


class CertificationBase(BaseModel):
    name: str = Field(..., max_length=300)
    cert_type: Optional[str] = None
    issuing_body: Optional[str] = None
    certificate_number: Optional[str] = None
    issue_date: Optional[datetime] = None
    expiry_date: Optional[datetime] = None
    is_valid: bool = True


class CertificationCreate(CertificationBase):
    pass


class CertificationUpdate(BaseModel):
    name: Optional[str] = None
    cert_type: Optional[str] = None
    issuing_body: Optional[str] = None
    certificate_number: Optional[str] = None
    issue_date: Optional[datetime] = None
    expiry_date: Optional[datetime] = None
    is_valid: Optional[bool] = None


class CertificationResponse(CertificationBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    company_id: UUID
    document_path: Optional[str] = None
    created_at: datetime
    updated_at: datetime


class PersonnelBase(BaseModel):
    name: str = Field(..., max_length=300)
    designation: Optional[str] = None
    role_in_tender: Optional[str] = None
    qualification: Optional[str] = None
    experience_years: Optional[int] = None
    specialization: Optional[List[str]] = None


class PersonnelCreate(PersonnelBase):
    pass


class PersonnelUpdate(BaseModel):
    name: Optional[str] = None
    designation: Optional[str] = None
    role_in_tender: Optional[str] = None
    qualification: Optional[str] = None
    experience_years: Optional[int] = None
    specialization: Optional[List[str]] = None


class PersonnelResponse(PersonnelBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    company_id: UUID
    cv_path: Optional[str] = None
    experience_cert_path: Optional[str] = None
    qualification_cert_path: Optional[str] = None
    created_at: datetime
    updated_at: datetime
