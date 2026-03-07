"""Database models for tender automation."""
from app.models.tender import Tender
from app.models.document import Document, TenderDocument
from app.models.ai_generation import AIGeneration
from app.models.company import CompanyProfile, Certification, Personnel
from app.models.reference_bundle import ReferenceBundle, BundleDocument, TenderCriteria, BundleCriteriaAssignment
from app.models.oem import OEMMaster, OEMTenderRequirement
from app.models.tracking import PortalRegistration, EMDRecord, TenderFee
from app.models.submission_checklist import SubmissionChecklistItem
from app.models.knowledge_base import KnowledgeBaseEntry, KBEntryVersion, KBTenderLink
from app.models.ai_provider import AIProviderConfig
from app.models.dismissed_alert import DismissedAlert
from app.models.tender_status_transition import TenderStatusTransition
from app.models.tender_collaboration import TenderAssignment, TenderComment
from app.models.tender_result import TenderResult
from app.models.tender_chat import TenderChatMessage
from app.models.proposal import TenderSection, ProposalSection, GeneratedContent, ProposalGeneration

__all__ = [
    "Tender",
    "Document",
    "TenderDocument",
    "AIGeneration",
    "CompanyProfile",
    "Certification",
    "Personnel",
    "ReferenceBundle",
    "BundleDocument",
    "TenderCriteria",
    "BundleCriteriaAssignment",
    "OEMMaster",
    "OEMTenderRequirement",
    "PortalRegistration",
    "EMDRecord",
    "TenderFee",
    "SubmissionChecklistItem",
    "KnowledgeBaseEntry",
    "KBEntryVersion",
    "KBTenderLink",
    "AIProviderConfig",
    "DismissedAlert",
    "TenderStatusTransition",
    "TenderAssignment",
    "TenderComment",
    "TenderResult",
    "TenderChatMessage",
    "TenderSection",
    "ProposalSection",
    "GeneratedContent",
    "ProposalGeneration",
]
