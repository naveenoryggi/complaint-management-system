-- Migration 007: Tender Status Transitions table
-- Audit trail for status workflow changes

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_status_transitions')
BEGIN
    CREATE TABLE tender_status_transitions (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        from_status NVARCHAR(50) NOT NULL,
        to_status NVARCHAR(50) NOT NULL,
        changed_by UNIQUEIDENTIFIER NOT NULL,
        change_reason NVARCHAR(500) NULL,
        changed_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );

    CREATE INDEX IX_tender_status_transitions_tender ON tender_status_transitions (tender_id);
    CREATE INDEX IX_tender_status_transitions_tenant ON tender_status_transitions (tenant_id);
END
GO
