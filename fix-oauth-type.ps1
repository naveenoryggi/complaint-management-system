Write-Host "Fixing OAuth Authentication Type..." -ForegroundColor Cyan

$query = @"
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
"@

try {
    Write-Host "Executing SQL update..." -ForegroundColor Yellow
    $result = Invoke-Sqlcmd -ServerInstance "PRANA-ASUS\SQLEXPRESS" -Database "ComplaintManagementDb" -Query $query

    Write-Host "`nSUCCESS: Authentication type updated!" -ForegroundColor Green
    Write-Host "`nUpdated Configuration:" -ForegroundColor Cyan
    $result | Format-Table -AutoSize

    Write-Host "`n================================================" -ForegroundColor Yellow
    Write-Host "Authentication type is now set to OAuth2 (value = 2)" -ForegroundColor Green
    Write-Host "You can now use the Re-authorize button in the UI" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Yellow
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
