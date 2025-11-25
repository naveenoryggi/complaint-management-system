-- Quick Fix for OAuth Email Configuration
-- Run this in SQL Server Management Studio

USE ComplaintManagementDb;
GO

-- 1. Fix AuthenticationType (2 is invalid, should be 1 for OAuth or 0 for Basic)
UPDATE EmailConfigurations
SET AuthenticationType = 1  -- OAuth 2.0
WHERE Id = '4A1B41EF-CBC5-4858-A6A5-02B1C147A80A';

-- 2. Verify the fix
SELECT
    Id,
    FromEmail,
    AuthenticationType,
    CASE AuthenticationType
        WHEN 0 THEN 'Basic Auth'
        WHEN 1 THEN 'OAuth 2.0'
        ELSE 'INVALID (' + CAST(AuthenticationType AS VARCHAR) + ')'
    END as AuthType,
    CASE
        WHEN OAuthClientId IS NOT NULL AND OAuthClientId != '' THEN 'Set'
        ELSE 'Not Set'
    END as ClientID,
    CASE
        WHEN OAuthTenantId IS NOT NULL AND OAuthTenantId != '' THEN 'Set'
        ELSE 'Not Set'
    END as TenantID,
    CASE
        WHEN OAuthAccessToken IS NOT NULL AND OAuthAccessToken != '' THEN 'Present'
        ELSE 'Not Authorized'
    END as AccessToken,
    OAuthTokenExpiresAt as TokenExpiry,
    IsEnabled
FROM EmailConfigurations
WHERE FromEmail = 'marketing@oryggitech.com';

PRINT '';
PRINT '================================';
PRINT 'Fix Applied Successfully!';
PRINT '================================';
PRINT '';
PRINT 'Next Steps:';
PRINT '1. Refresh your browser (Ctrl+F5)';
PRINT '2. Navigate to: Admin Panel > Communication Settings > Email Ticketing';
PRINT '3. Badge should now show OAuth status correctly';
PRINT '4. Follow OAUTH_QUICK_START.md for Azure AD setup';
PRINT '';
