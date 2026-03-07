-- Migration 012: Proposal generation tables
-- Stores parsed tender sections, generated proposal TOC, AI-generated content, and generation run tracking.

CREATE TABLE tender_sections (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
    section_name NVARCHAR(500) NOT NULL,
    section_description NVARCHAR(MAX) NULL,
    section_text NVARCHAR(MAX) NULL,
    page_reference NVARCHAR(100) NULL,
    sort_order INT NOT NULL DEFAULT 0,
    created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE INDEX ix_tender_sections_tender_id ON tender_sections(tender_id);

CREATE TABLE proposal_sections (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
    section_number NVARCHAR(20) NOT NULL,
    section_title NVARCHAR(500) NOT NULL,
    parent_section_id UNIQUEIDENTIFIER NULL,
    mapped_requirement_ids NVARCHAR(MAX) NULL,  -- JSON array of requirement UUIDs
    status NVARCHAR(30) NOT NULL DEFAULT 'pending',
    sort_order INT NOT NULL DEFAULT 0,
    created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    updated_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE INDEX ix_proposal_sections_tender_id ON proposal_sections(tender_id);

-- Self-referential FK added after table creation to avoid ordering issues
ALTER TABLE proposal_sections
    ADD CONSTRAINT fk_proposal_sections_parent
    FOREIGN KEY (parent_section_id) REFERENCES proposal_sections(id);

CREATE TABLE generated_content (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    proposal_section_id UNIQUEIDENTIFIER NOT NULL REFERENCES proposal_sections(id) ON DELETE CASCADE,
    tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
    content NVARCHAR(MAX) NOT NULL,
    content_format NVARCHAR(20) NOT NULL DEFAULT 'markdown',
    version INT NOT NULL DEFAULT 1,
    is_current BIT NOT NULL DEFAULT 1,
    model_used NVARCHAR(100) NULL,
    tokens_used INT NULL,
    generation_prompt NVARCHAR(MAX) NULL,
    diagram_suggestions NVARCHAR(MAX) NULL,  -- JSON
    quality_score FLOAT NULL,
    created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
);

CREATE INDEX ix_generated_content_section_id ON generated_content(proposal_section_id);
CREATE INDEX ix_generated_content_tender_id ON generated_content(tender_id);

CREATE TABLE proposal_generations (
    id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
    tenant_id UNIQUEIDENTIFIER NOT NULL,
    status NVARCHAR(30) NOT NULL DEFAULT 'started',
    current_step NVARCHAR(100) NULL,
    total_steps INT NOT NULL DEFAULT 0,
    completed_steps INT NOT NULL DEFAULT 0,
    total_sections INT NULL,
    total_requirements INT NULL,
    coverage_percentage FLOAT NULL,
    estimated_score FLOAT NULL,
    gap_analysis NVARCHAR(MAX) NULL,       -- JSON
    evaluation_results NVARCHAR(MAX) NULL, -- JSON
    diagram_suggestions NVARCHAR(MAX) NULL,-- JSON
    started_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
    completed_at DATETIMEOFFSET NULL,
    error_message NVARCHAR(MAX) NULL
);

CREATE INDEX ix_proposal_generations_tender_id ON proposal_generations(tender_id);
CREATE INDEX ix_proposal_generations_tenant_id ON proposal_generations(tenant_id);
