-- Migration 008: Tender Collaboration tables
-- Team assignments and threaded comments

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_assignments')
BEGIN
    CREATE TABLE tender_assignments (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        user_id UNIQUEIDENTIFIER NOT NULL,
        user_name NVARCHAR(200) NOT NULL,
        role NVARCHAR(50) NOT NULL DEFAULT 'member',
        assigned_by UNIQUEIDENTIFIER NOT NULL,
        assigned_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );

    CREATE INDEX IX_tender_assignments_tender ON tender_assignments (tender_id);
    CREATE INDEX IX_tender_assignments_tenant ON tender_assignments (tenant_id);
END
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_comments')
BEGIN
    CREATE TABLE tender_comments (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        user_id UNIQUEIDENTIFIER NOT NULL,
        user_name NVARCHAR(200) NOT NULL,
        comment_text NVARCHAR(MAX) NOT NULL,
        parent_id UNIQUEIDENTIFIER NULL,
        created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );

    CREATE INDEX IX_tender_comments_tender ON tender_comments (tender_id);
    CREATE INDEX IX_tender_comments_tenant ON tender_comments (tenant_id);
END
GO
