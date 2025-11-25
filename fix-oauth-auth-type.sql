-- Fix OAuth Authentication Type
-- Issue: Configuration has OAuth credentials but AuthenticationType is set to 1 (Basic) instead of 2 (OAuth2)
-- This causes the backend to reject OAuth operations

USE ComplaintManagementDb;
GO

-- Check current state
SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 1 THEN 'Basic (WRONG for OAuth)'
        WHEN 2 THEN 'OAuth2 (CORRECT)'
        ELSE 'Unknown'
    END AS AuthTypeDescription,
    OAuthClientId,
    OAuthTenantId,
    OAuthTokenExpiresAt,
    IsEnabled
FROM EmailConfigurations
WHERE Id = '4a1b41ef-cbc5-4858-a6a5-02b1c147a80a';

-- Update to OAuth2 (value = 2)
UPDATE EmailConfigurations
SET
    AuthenticationType = 2, -- OAuth2
    UpdatedAt = GETUTCDATE()
WHERE Id = '4a1b41ef-cbc5-4858-a6a5-02b1c147a80a';

-- Verify the fix
SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 1 THEN 'Basic'
        WHEN 2 THEN 'OAuth2 (CORRECT!)'
        ELSE 'Unknown'
    END AS AuthTypeDescription,
    OAuthClientId,
    OAuthTenantId,
    OAuthTokenExpiresAt,
    IsEnabled,
    UpdatedAt
FROM EmailConfigurations
WHERE Id = '4a1b41ef-cbc5-4858-a6a5-02b1c147a80a';

PRINT 'Authentication type updated to OAuth2 (value = 2)';
PRINT 'You can now use the Re-authorize button in the UI';
