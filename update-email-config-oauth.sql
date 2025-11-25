-- Update Email Configuration to OAuth Authentication Type
-- This will change the authentication type to OAuth and add test OAuth credentials

USE ComplaintManagementDB;
GO

-- First, check current configuration
SELECT
    Id,
    FromEmail,
    FromName,
    AuthenticationType,
    OAuthClientId,
    OAuthTenantId,
    OAuthAccessToken,
    IsEnabled
FROM EmailConfigurations
WHERE IsDeleted = 0;
GO

-- Update to OAuth authentication
UPDATE EmailConfigurations
SET
    AuthenticationType = 1,  -- 0 = Basic, 1 = OAuth
    OAuthClientId = '12345678-1234-1234-1234-123456789abc',
    OAuthTenantId = '87654321-4321-4321-4321-cba987654321',
    OAuthClientSecret = 'test-client-secret-value-for-oauth-testing',
    OAuthAccessToken = NULL,  -- No access token yet (pending authorization)
    OAuthTokenExpiresAt = NULL,  -- No expiry yet
    UpdatedAt = GETUTCDATE()
WHERE IsDeleted = 0;
GO

-- Verify the update
SELECT
    Id,
    FromEmail,
    FromName,
    AuthenticationType,
    OAuthClientId,
    OAuthTenantId,
    CASE
        WHEN OAuthClientSecret IS NOT NULL THEN 'SET'
        ELSE 'NULL'
    END AS OAuthClientSecret,
    CASE
        WHEN OAuthAccessToken IS NOT NULL THEN 'SET'
        ELSE 'NULL'
    END AS OAuthAccessToken,
    OAuthTokenExpiresAt,
    IsEnabled,
    UpdatedAt
FROM EmailConfigurations
WHERE IsDeleted = 0;
GO

PRINT 'Email configuration updated to OAuth authentication type';
PRINT 'Status: OAuth 2.0 - Pending Authorization (no access token yet)';
GO
