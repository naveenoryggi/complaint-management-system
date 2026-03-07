-- Migration 010: Company Document Library + Tender AI Chat
-- Date: 2026-03-04

-- =============================================
-- Feature 1: Company Document Library
-- Extend documents table with company doc fields
-- =============================================

ALTER TABLE documents ADD company_doc_category NVARCHAR(50) NULL;
ALTER TABLE documents ADD expiry_date DATE NULL;
ALTER TABLE documents ADD is_company_document BIT NOT NULL DEFAULT 0;

CREATE INDEX IX_documents_company ON documents(is_company_document, company_doc_category);

-- =============================================
-- Feature 2: Tender AI Chat Messages
-- =============================================

CREATE TABLE tender_chat_messages (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    role NVARCHAR(20) NOT NULL,
    content NVARCHAR(MAX) NOT NULL,
    tokens_used INT NULL,
    model_used NVARCHAR(100) NULL,
    user_id UNIQUEIDENTIFIER NULL,
    user_name NVARCHAR(200) NULL,
    created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE INDEX IX_chat_tender ON tender_chat_messages(tender_id, created_at);
