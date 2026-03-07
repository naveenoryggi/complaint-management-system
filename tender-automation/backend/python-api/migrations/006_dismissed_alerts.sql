-- Migration 006: Dismissed Alerts table
-- Tracks which alerts a user has dismissed

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'dismissed_alerts')
BEGIN
    CREATE TABLE dismissed_alerts (
        id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        tenant_id UNIQUEIDENTIFIER NOT NULL,
        user_id UNIQUEIDENTIFIER NOT NULL,
        alert_key NVARCHAR(200) NOT NULL,
        dismissed_at DATETIMEOFFSET NOT NULL DEFAULT SYSDATETIMEOFFSET()
    );

    CREATE INDEX IX_dismissed_alerts_tenant_user ON dismissed_alerts (tenant_id, user_id);
    CREATE UNIQUE INDEX UQ_dismissed_alert ON dismissed_alerts (tenant_id, user_id, alert_key);
END
GO
