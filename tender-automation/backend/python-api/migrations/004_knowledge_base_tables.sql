-- Migration 004: Knowledge Base, Compliance Matrix, and Submission Checklist tables
-- Date: 2026-03-03
-- Dependencies: 001 (tenders, documents), 002 (oem_master)

-- =========================================================================
-- 1. technical_compliance_items
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'technical_compliance_items')
BEGIN
    CREATE TABLE technical_compliance_items (
        id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tender_id               UNIQUEIDENTIFIER NOT NULL,
        clause_number           NVARCHAR(50)     NOT NULL,
        clause_title            NVARCHAR(500)    NOT NULL,
        clause_description      NVARCHAR(MAX)    NULL,
        section                 NVARCHAR(200)    NULL,
        compliance_status       NVARCHAR(30)     NOT NULL DEFAULT 'pending',
        deviation_remarks       NVARCHAR(MAX)    NULL,
        page_reference          NVARCHAR(100)    NULL,
        supporting_document_id  UNIQUEIDENTIFIER NULL,
        is_mandatory            BIT              NOT NULL DEFAULT 1,
        is_critical             BIT              NOT NULL DEFAULT 0,
        ai_extracted            BIT              NOT NULL DEFAULT 0,
        sort_order              INT              NOT NULL DEFAULT 0,
        created_at              DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at              DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_compliance_items_tender
            FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE
    );

    CREATE INDEX IX_compliance_items_tender ON technical_compliance_items(tender_id);
END;
GO

-- =========================================================================
-- 2. boq_line_items
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'boq_line_items')
BEGIN
    CREATE TABLE boq_line_items (
        id                UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tender_id         UNIQUEIDENTIFIER NOT NULL,
        item_number       NVARCHAR(50)     NOT NULL,
        description       NVARCHAR(MAX)    NOT NULL,
        unit              NVARCHAR(50)     NULL,
        quantity          NUMERIC(15,2)    NOT NULL DEFAULT 0,
        unit_rate         NUMERIC(15,2)    NULL DEFAULT 0,
        total_before_tax  NUMERIC(15,2)    NULL,
        gst_percentage    NUMERIC(5,2)     NULL,
        gst_amount        NUMERIC(15,2)    NULL,
        total_with_tax    NUMERIC(15,2)    NULL,
        category          NVARCHAR(100)    NULL,
        is_optional       BIT              NOT NULL DEFAULT 0,
        make_model        NVARCHAR(500)    NULL,
        oem_id            UNIQUEIDENTIFIER NULL,
        sort_order        INT              NOT NULL DEFAULT 0,
        notes             NVARCHAR(MAX)    NULL,
        created_at        DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at        DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_boq_items_tender
            FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE
    );

    CREATE INDEX IX_boq_items_tender ON boq_line_items(tender_id);
END;
GO

-- =========================================================================
-- 3. tender_milestones
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_milestones')
BEGIN
    CREATE TABLE tender_milestones (
        id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tender_id        UNIQUEIDENTIFIER NOT NULL,
        milestone_type   NVARCHAR(50)     NOT NULL,
        label            NVARCHAR(500)    NOT NULL,
        event_date       NVARCHAR(50)     NULL,
        event_time       NVARCHAR(50)     NULL,
        location         NVARCHAR(500)    NULL,
        is_completed     BIT              NOT NULL DEFAULT 0,
        notes            NVARCHAR(MAX)    NULL,
        alert_days_before INT             NULL,
        sort_order       INT              NOT NULL DEFAULT 0,
        created_at       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_milestones_tender
            FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE
    );

    CREATE INDEX IX_milestones_tender ON tender_milestones(tender_id);
END;
GO

-- =========================================================================
-- 4. tender_corrigendums
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_corrigendums')
BEGIN
    CREATE TABLE tender_corrigendums (
        id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tender_id           UNIQUEIDENTIFIER NOT NULL,
        corrigendum_number  INT              NOT NULL,
        issue_date          NVARCHAR(50)     NULL,
        title               NVARCHAR(500)    NOT NULL,
        summary             NVARCHAR(MAX)    NULL,
        changes             NVARCHAR(MAX)    NULL,
        document_path       NVARCHAR(1000)   NULL,
        is_acknowledged     BIT              NOT NULL DEFAULT 0,
        created_at          DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at          DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_corrigendums_tender
            FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE
    );

    CREATE INDEX IX_corrigendums_tender ON tender_corrigendums(tender_id);
END;
GO

-- =========================================================================
-- 5. submission_checklist_items
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'submission_checklist_items')
BEGIN
    CREATE TABLE submission_checklist_items (
        id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tender_id               UNIQUEIDENTIFIER NOT NULL,
        document_name           NVARCHAR(500)    NOT NULL,
        document_category       NVARCHAR(50)     NOT NULL DEFAULT 'other',
        submission_mode         NVARCHAR(20)     NOT NULL DEFAULT 'both',
        is_critical             BIT              NOT NULL DEFAULT 0,
        envelope                NVARCHAR(50)     NULL,
        cover_name              NVARCHAR(200)    NULL,
        online_status           NVARCHAR(30)     NOT NULL DEFAULT 'not_started',
        offline_status          NVARCHAR(30)     NOT NULL DEFAULT 'not_started',
        notarization_required   BIT              NOT NULL DEFAULT 0,
        notarization_type       NVARCHAR(30)     NULL,
        notarization_status     NVARCHAR(30)     NOT NULL DEFAULT 'not_required',
        document_origin         NVARCHAR(30)     NOT NULL DEFAULT 'self_generated',
        format_reference        NVARCHAR(500)    NULL,
        obtaining_source        NVARCHAR(500)    NULL,
        can_auto_generate       BIT              NOT NULL DEFAULT 0,
        linked_document_id      UNIQUEIDENTIFIER NULL,
        due_date                NVARCHAR(50)     NULL,
        notes                   NVARCHAR(MAX)    NULL,
        sort_order              INT              NOT NULL DEFAULT 0,
        source                  NVARCHAR(30)     NOT NULL DEFAULT 'manual',
        created_at              DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at              DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_checklist_tender
            FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE,
        CONSTRAINT FK_checklist_document
            FOREIGN KEY (linked_document_id) REFERENCES documents(id) ON DELETE SET NULL
    );

    CREATE INDEX IX_checklist_tender ON submission_checklist_items(tender_id);
END;
GO

-- =========================================================================
-- 6. knowledge_base_entries
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'knowledge_base_entries')
BEGIN
    CREATE TABLE knowledge_base_entries (
        id                   UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tenant_id            UNIQUEIDENTIFIER NOT NULL,
        title                NVARCHAR(500)    NOT NULL,
        content              NVARCHAR(MAX)    NOT NULL,
        content_format       NVARCHAR(20)     NOT NULL DEFAULT 'text',
        category             NVARCHAR(50)     NOT NULL,
        subcategory          NVARCHAR(100)    NULL,
        tags                 NVARCHAR(MAX)    NULL,
        keywords             NVARCHAR(MAX)    NULL,
        source_type          NVARCHAR(30)     NOT NULL DEFAULT 'manual',
        source_tender_id     UNIQUEIDENTIFIER NULL,
        source_description   NVARCHAR(500)    NULL,
        valid_from           DATE             NULL,
        valid_until          DATE             NULL,
        fiscal_year          NVARCHAR(20)     NULL,
        is_current           BIT              NOT NULL DEFAULT 1,
        usage_count          INT              NOT NULL DEFAULT 0,
        last_used_at         DATETIMEOFFSET   NULL,
        last_used_tender_id  UNIQUEIDENTIFIER NULL,
        confidence_score     FLOAT            NULL,
        is_verified          BIT              NOT NULL DEFAULT 0,
        is_archived          BIT              NOT NULL DEFAULT 0,
        created_by           UNIQUEIDENTIFIER NULL,
        created_at           DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at           DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_kb_entries_source_tender
            FOREIGN KEY (source_tender_id) REFERENCES tenders(id) ON DELETE SET NULL
    );

    CREATE INDEX IX_kb_entries_tenant ON knowledge_base_entries(tenant_id);
    CREATE INDEX IX_kb_entries_category ON knowledge_base_entries(category);
    CREATE INDEX IX_kb_entries_tenant_category ON knowledge_base_entries(tenant_id, category);
END;
GO

-- =========================================================================
-- 7. kb_entry_versions
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'kb_entry_versions')
BEGIN
    CREATE TABLE kb_entry_versions (
        id               UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        entry_id         UNIQUEIDENTIFIER NOT NULL,
        version_number   INT              NOT NULL,
        content          NVARCHAR(MAX)    NOT NULL,
        change_reason    NVARCHAR(500)    NULL,
        changed_by       UNIQUEIDENTIFIER NULL,
        created_at       DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),

        CONSTRAINT FK_kb_versions_entry
            FOREIGN KEY (entry_id) REFERENCES knowledge_base_entries(id) ON DELETE CASCADE,
        CONSTRAINT UQ_kb_entry_version
            UNIQUE (entry_id, version_number)
    );

    CREATE INDEX IX_kb_versions_entry ON kb_entry_versions(entry_id);
END;
GO

-- =========================================================================
-- 8. kb_tender_links
-- =========================================================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'kb_tender_links')
BEGIN
    CREATE TABLE kb_tender_links (
        id              UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        entry_id        UNIQUEIDENTIFIER NOT NULL,
        tender_id       UNIQUEIDENTIFIER NOT NULL,
        usage_context   NVARCHAR(200)    NULL,
        used_at         DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        used_by         UNIQUEIDENTIFIER NULL,

        CONSTRAINT FK_kb_links_entry
            FOREIGN KEY (entry_id) REFERENCES knowledge_base_entries(id) ON DELETE CASCADE,
        CONSTRAINT FK_kb_links_tender
            FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE,
        CONSTRAINT UQ_kb_tender_usage
            UNIQUE (entry_id, tender_id, usage_context)
    );

    CREATE INDEX IX_kb_links_entry ON kb_tender_links(entry_id);
    CREATE INDEX IX_kb_links_tender ON kb_tender_links(tender_id);
END;
GO
