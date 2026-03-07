-- Migration 009: Tender Results table
-- Post-submission outcome tracking

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_results')
BEGIN
    CREATE TABLE tender_results (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tender_id UNIQUEIDENTIFIER NOT NULL REFERENCES tenders(id) ON DELETE CASCADE,
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        result NVARCHAR(20) NOT NULL,
        awarded_value DECIMAL(15,2) NULL,
        award_date DATETIMEOFFSET NULL,
        winning_bidder NVARCHAR(500) NULL,
        loss_reason NVARCHAR(MAX) NULL,
        lessons_learned NVARCHAR(MAX) NULL,
        contract_number NVARCHAR(200) NULL,
        contract_start DATETIMEOFFSET NULL,
        contract_end DATETIMEOFFSET NULL,
        created_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );

    CREATE UNIQUE INDEX UQ_tender_result ON tender_results (tender_id);
    CREATE INDEX IX_tender_results_tenant ON tender_results (tenant_id);
END
GO
