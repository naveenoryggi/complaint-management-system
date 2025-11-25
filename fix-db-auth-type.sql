-- Fix OAuth authenticationType in EmailConfigurations table
-- Value 2 is invalid. Valid values: 0 = Basic Auth, 1 = OAuth 2.0

UPDATE EmailConfigurations
SET AuthenticationType = 1
WHERE FromEmail = 'marketing@oryggitech.com';

-- Verify the update
SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE
        WHEN AuthenticationType = 0 THEN 'Basic Auth'
        WHEN AuthenticationType = 1 THEN 'OAuth 2.0'
        ELSE 'INVALID (' + CAST(AuthenticationType AS VARCHAR) + ')'
    END as AuthTypeDescription,
    CASE
        WHEN OAuthAccessToken IS NULL THEN 'No Token'
        WHEN OAuthTokenExpiresAt < GETDATE() THEN 'Token Expired'
        ELSE 'Token Valid'
    END as TokenStatus,
    OAuthClientId,
    OAuthTenantId,
    OAuthTokenExpiresAt,
    IsEnabled
FROM EmailConfigurations
WHERE FromEmail = 'marketing@oryggitech.com';

-- Expected UI behavior after fix:
-- - If token is expired: Badge = "OAuth 2.0 - Expired" (red), Button = "Refresh OAuth"
-- - If no token: Badge = "OAuth 2.0 - Pending" (orange pulsing), Button = "Authorize Now"
-- - If token valid: Badge = "OAuth 2.0 - Authorized" (green), Button = "Refresh OAuth"
