-- Create Tender Automation tables in SQL Server

USE ComplaintManagementDB;
GO

-- Create tenders table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tenders')
BEGIN
    CREATE TABLE tenders (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        created_by UNIQUEIDENTIFIER NOT NULL,
        title NVARCHAR(500) NOT NULL,
        reference_number NVARCHAR(100),
        issuing_authority NVARCHAR(300),
        portal_name NVARCHAR(100),
        portal_url NVARCHAR(MAX),
        deadline DATETIME,
        estimated_value DECIMAL(15, 2),
        requirements NVARCHAR(MAX) DEFAULT '{}',
        notes NVARCHAR(MAX),
        status NVARCHAR(50) DEFAULT 'draft',
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );

    CREATE INDEX idx_tenders_tenant ON tenders(tenant_id);
    CREATE INDEX idx_tenders_deadline ON tenders(deadline);
    CREATE INDEX idx_tenders_status ON tenders(status);

    PRINT 'Created tenders table';
END
ELSE
    PRINT 'tenders table already exists';
GO

-- Create documents table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'documents')
BEGIN
    CREATE TABLE documents (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        created_by UNIQUEIDENTIFIER NOT NULL,
        name NVARCHAR(255) NOT NULL,
        description NVARCHAR(MAX),
        file_path NVARCHAR(500) NOT NULL,
        file_size INT NOT NULL,
        mime_type NVARCHAR(100) NOT NULL,
        document_type NVARCHAR(50),
        tags NVARCHAR(MAX) DEFAULT '',
        metadata NVARCHAR(MAX) DEFAULT '{}',
        is_template BIT DEFAULT 0,
        version INT DEFAULT 1,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );

    CREATE INDEX idx_documents_tenant ON documents(tenant_id);
    CREATE INDEX idx_documents_type ON documents(document_type);

    PRINT 'Created documents table';
END
ELSE
    PRINT 'documents table already exists';
GO

-- Create tender_documents table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tender_documents')
BEGIN
    CREATE TABLE tender_documents (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tender_id UNIQUEIDENTIFIER NOT NULL,
        document_id UNIQUEIDENTIFIER NOT NULL,
        document_order INT DEFAULT 0,
        is_generated BIT DEFAULT 0,
        generation_prompt NVARCHAR(MAX),
        created_at DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (tender_id) REFERENCES tenders(id) ON DELETE CASCADE,
        FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE CASCADE
    );

    CREATE INDEX idx_tender_docs_tender ON tender_documents(tender_id);

    PRINT 'Created tender_documents table';
END
ELSE
    PRINT 'tender_documents table already exists';
GO

-- Create ai_generations table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ai_generations')
BEGIN
    CREATE TABLE ai_generations (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        created_by UNIQUEIDENTIFIER NOT NULL,
        document_id UNIQUEIDENTIFIER,
        prompt NVARCHAR(MAX) NOT NULL,
        model_used NVARCHAR(50) DEFAULT 'claude-sonnet-4-5',
        tokens_used INT,
        generation_type NVARCHAR(50),
        input_context NVARCHAR(MAX),
        output_content NVARCHAR(MAX),
        created_at DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (document_id) REFERENCES documents(id) ON DELETE SET NULL
    );

    CREATE INDEX idx_ai_generations_tenant ON ai_generations(tenant_id);

    PRINT 'Created ai_generations table';
END
ELSE
    PRINT 'ai_generations table already exists';
GO

PRINT 'Tender Automation tables created successfully!';
