-- Migration 005: AI Provider Configs table
-- Stores encrypted API keys for multiple AI providers per tenant

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'ai_provider_configs')
BEGIN
    CREATE TABLE ai_provider_configs (
        id                  UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID() PRIMARY KEY,
        tenant_id           UNIQUEIDENTIFIER NOT NULL,
        provider_name       NVARCHAR(50)     NOT NULL,  -- anthropic, openai, google, azure_openai
        display_name        NVARCHAR(200)    NOT NULL,
        api_key_encrypted   NVARCHAR(MAX)    NOT NULL,
        endpoint_url        NVARCHAR(500)    NULL,       -- Azure endpoint
        deployment_name     NVARCHAR(200)    NULL,       -- Azure deployment
        api_version         NVARCHAR(50)     NULL,       -- Azure API version
        default_model       NVARCHAR(100)    NOT NULL,
        feature_mapping     NVARCHAR(MAX)    NULL,       -- JSON: {"extraction":true,...}
        is_active           BIT              NOT NULL DEFAULT 1,
        is_default          BIT              NOT NULL DEFAULT 0,
        total_tokens_used   INT              NOT NULL DEFAULT 0,
        total_requests      INT              NOT NULL DEFAULT 0,
        last_used_at        DATETIMEOFFSET   NULL,
        last_test_at        DATETIMEOFFSET   NULL,
        last_test_success   BIT              NULL,
        created_by          UNIQUEIDENTIFIER NULL,
        created_at          DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET(),
        updated_at          DATETIMEOFFSET   NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );

    CREATE INDEX ix_ai_provider_configs_tenant ON ai_provider_configs (tenant_id);

    PRINT 'Created table ai_provider_configs';
END
ELSE
BEGIN
    PRINT 'Table ai_provider_configs already exists — skipping';
END
GO
