"""Pydantic schemas for reference bundles and criteria."""
from typing import Optional, List, Dict, Any
from datetime import datetime, date
from uuid import UUID
from pydantic import BaseModel, ConfigDict, Field


class ReferenceBundleBase(BaseModel):
    bundle_name: str = Field(..., max_length=500)
    client_name: str = Field(..., max_length=500)
    client_short_name: Optional[str] = None
    client_type: Optional[str] = None
    project_name: Optional[str] = None
    work_order_number: Optional[str] = None
    work_order_date: Optional[date] = None
    contract_value: Optional[float] = None
    gst_value: Optional[float] = None
    total_value: Optional[float] = None
    duration_months: Optional[int] = None
    start_date: Optional[date] = None
    actual_completion_date: Optional[date] = None
    status: str = "completed"
    scope_description: Optional[str] = None


class ReferenceBundleCreate(ReferenceBundleBase):
    pass


class ReferenceBundleUpdate(BaseModel):
    bundle_name: Optional[str] = None
    client_name: Optional[str] = None
    client_short_name: Optional[str] = None
    client_type: Optional[str] = None
    project_name: Optional[str] = None
    work_order_number: Optional[str] = None
    contract_value: Optional[float] = None
    status: Optional[str] = None
    scope_description: Optional[str] = None


class BundleDocumentBase(BaseModel):
    tier: str
    doc_subtype: str
    file_name: Optional[str] = None
    invoice_number: Optional[str] = None
    invoice_date: Optional[date] = None
    invoice_amount: Optional[float] = None
    signatory_name: Optional[str] = None
    signatory_designation: Optional[str] = None
    notes: Optional[str] = None


class BundleDocumentCreate(BundleDocumentBase):
    pass


class BundleDocumentResponse(BundleDocumentBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    bundle_id: UUID
    document_path: Optional[str] = None
    is_verified: bool = False
    created_at: datetime
    updated_at: datetime


class ReferenceBundleResponse(ReferenceBundleBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tenant_id: UUID
    value_bands: Optional[List[str]] = None
    client_type_tags: Optional[List[str]] = None
    work_type_tags: Optional[List[str]] = None
    completeness_score: int = 0
    missing_documents: Optional[List[str]] = None
    bundle_documents: List[BundleDocumentResponse] = []
    created_at: datetime
    updated_at: datetime


class BundleListResponse(BaseModel):
    items: List[ReferenceBundleResponse]
    total: int
    page: int
    page_size: int


class TenderCriteriaBase(BaseModel):
    criteria_code: str
    description: Optional[str] = None
    stage: str = "marking"
    max_marks: float = 0
    qualifying_marks: Optional[float] = None
    scoring_rules: Optional[Dict[str, Any]] = None
    evidence_required: Optional[List[str]] = None
    can_reuse_across_tenders: bool = True
    max_reuse_count: Optional[int] = None


class TenderCriteriaCreate(TenderCriteriaBase):
    pass


class TenderCriteriaUpdate(BaseModel):
    criteria_code: Optional[str] = None
    description: Optional[str] = None
    max_marks: Optional[float] = None
    qualifying_marks: Optional[float] = None
    scoring_rules: Optional[Dict[str, Any]] = None
    evidence_required: Optional[List[str]] = None


class TenderCriteriaResponse(TenderCriteriaBase):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    tender_id: UUID
    ai_extracted: bool = False
    source_document_id: Optional[UUID] = None
    created_at: datetime
    updated_at: datetime


class BundleCriteriaAssignmentCreate(BaseModel):
    bundle_id: UUID
    criteria_id: UUID
    predicted_marks: Optional[float] = None
    submitted_doc_types: Optional[List[str]] = None
    notes: Optional[str] = None


class BundleCriteriaAssignmentResponse(BaseModel):
    model_config = ConfigDict(from_attributes=True)
    id: UUID
    bundle_id: UUID
    criteria_id: UUID
    predicted_marks: Optional[float] = None
    actual_marks: Optional[float] = None
    submitted_doc_types: Optional[List[str]] = None
    notes: Optional[str] = None
    created_at: datetime
    updated_at: datetime


class CriteriaScore(BaseModel):
    criteria_code: str
    max_marks: float
    predicted_marks: float = 0
    gap_analysis: str = ""


class ScoringSimulation(BaseModel):
    tender_id: UUID
    total_max_marks: float = 0
    total_predicted_marks: float = 0
    criteria_scores: List[CriteriaScore] = []
