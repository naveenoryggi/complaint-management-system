# Automated OAuth Fix Script
# This script fixes the database and tests the OAuth system

$baseUrl = "http://localhost:5000/api"
$emailConfigId = "4A1B41EF-CBC5-4858-A6A5-02B1C147A80A"

Write-Host "================================" -ForegroundColor Cyan
Write-Host "OAuth Automated Fix Script" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Get Admin Token
Write-Host "[1/4] Getting admin authentication token..." -ForegroundColor Yellow
$loginBody = @{
    Email = "admin@complaintmanagement.com"
    Password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = if ($loginResponse.data.token) { $loginResponse.data.token } elseif ($loginResponse.token) { $loginResponse.token } else { $null }

    if (!$token) {
        Write-Host "  X Failed to get token" -ForegroundColor Red
        exit 1
    }

    Write-Host "  V Token obtained successfully" -ForegroundColor Green
    $token | Out-File -FilePath ".oauth-fix-token" -NoNewline -Encoding UTF8
} catch {
    Write-Host "  X Login failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Get current email configuration
Write-Host "[2/4] Getting current email configuration..." -ForegroundColor Yellow
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $configs = Invoke-RestMethod -Uri "$baseUrl/EmailConfiguration" -Headers $headers -Method Get
    $config = $configs | Where-Object { $_.id -eq $emailConfigId }

    if (!$config) {
        Write-Host "  X Configuration not found (ID: $emailConfigId)" -ForegroundColor Red
        Write-Host "  Available configs:" -ForegroundColor Yellow
        $configs | ForEach-Object { Write-Host "    - $($_.fromEmail) (ID: $($_.id))" -ForegroundColor Gray }
        exit 1
    }

    Write-Host "  V Configuration found: $($config.fromEmail)" -ForegroundColor Green
    Write-Host "    Current AuthenticationType: $($config.authenticationType)" -ForegroundColor Gray
} catch {
    Write-Host "  X Failed to get configuration: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Fix AuthenticationType if needed
Write-Host "[3/4] Fixing AuthenticationType..." -ForegroundColor Yellow

if ($config.authenticationType -eq 1) {
    Write-Host "  V AuthenticationType is already correct (OAuth = 1)" -ForegroundColor Green
} else {
    Write-Host "  ! Current value: $($config.authenticationType) (should be 1)" -ForegroundColor Yellow

    # Update the configuration
    $config.authenticationType = 1

    try {
        $updateBody = $config | ConvertTo-Json -Depth 10
        $updated = Invoke-RestMethod -Uri "$baseUrl/EmailConfiguration/$emailConfigId" -Headers $headers -Method Put -Body $updateBody
        Write-Host "  V AuthenticationType updated to 1 (OAuth 2.0)" -ForegroundColor Green
    } catch {
        Write-Host "  X Update failed: $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "  Trying direct SQL fix..." -ForegroundColor Yellow

        # Fallback to SQL
        $sqlQuery = "UPDATE EmailConfigurations SET AuthenticationType = 1 WHERE Id = '$emailConfigId'"
        try {
            sqlcmd -S "PRANA-ASUS\SQLEXPRESS" -d "ComplaintManagementDb" -Q $sqlQuery
            Write-Host "  V Fixed via SQL" -ForegroundColor Green
        } catch {
            Write-Host "  X SQL fix also failed" -ForegroundColor Red
        }
    }
}

# Step 4: Verify the fix
Write-Host "[4/4] Verifying the fix..." -ForegroundColor Yellow

try {
    $verifyConfig = Invoke-RestMethod -Uri "$baseUrl/EmailConfiguration/$emailConfigId" -Headers $headers -Method Get

    Write-Host ""
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host "Configuration Status" -ForegroundColor Cyan
    Write-Host "================================" -ForegroundColor Cyan
    Write-Host "Email: $($verifyConfig.fromEmail)" -ForegroundColor White
    Write-Host "Authentication Type: $($verifyConfig.authenticationType) $(if ($verifyConfig.authenticationType -eq 1) { '(OAuth 2.0) V' } else { '(Basic Auth) X' })" -ForegroundColor $(if ($verifyConfig.authenticationType -eq 1) { 'Green' } else { 'Red' })
    Write-Host "OAuth Client ID: $(if ($verifyConfig.oAuthClientId) { 'Configured V' } else { 'Not Set X' })" -ForegroundColor $(if ($verifyConfig.oAuthClientId) { 'Green' } else { 'Yellow' })
    Write-Host "OAuth Tenant ID: $(if ($verifyConfig.oAuthTenantId) { 'Configured V' } else { 'Not Set X' })" -ForegroundColor $(if ($verifyConfig.oAuthTenantId) { 'Green' } else { 'Yellow' })
    Write-Host "OAuth Access Token: $(if ($verifyConfig.oAuthAccessToken) { 'Present V' } else { 'Not Authorized X' })" -ForegroundColor $(if ($verifyConfig.oAuthAccessToken) { 'Green' } else { 'Yellow' })
    Write-Host "Token Expiry: $(if ($verifyConfig.oAuthTokenExpiresAt) { $verifyConfig.oAuthTokenExpiresAt } else { 'N/A' })" -ForegroundColor Gray
    Write-Host "Enabled: $(if ($verifyConfig.isEnabled) { 'Yes V' } else { 'No X' })" -ForegroundColor $(if ($verifyConfig.isEnabled) { 'Green' } else { 'Red' })
    Write-Host ""

    # Determine next steps
    if ($verifyConfig.authenticationType -eq 1) {
        Write-Host "V Database fix successful!" -ForegroundColor Green
        Write-Host ""

        if (!$verifyConfig.oAuthClientId -or !$verifyConfig.oAuthTenantId) {
            Write-Host "Next Steps:" -ForegroundColor Cyan
            Write-Host "1. Open http://localhost:4200 in browser" -ForegroundColor White
            Write-Host "2. Login as admin@complaintmanagement.com / Admin@123" -ForegroundColor White
            Write-Host "3. Navigate to: Admin Panel > Communication Settings > Email Ticketing" -ForegroundColor White
            Write-Host "4. Badge should now show 'OAuth 2.0 - Not Configured' (orange)" -ForegroundColor White
            Write-Host "5. Follow the Azure AD setup guide: OAUTH_QUICK_START.md" -ForegroundColor White
        } elseif (!$verifyConfig.oAuthAccessToken) {
            Write-Host "Next Steps:" -ForegroundColor Cyan
            Write-Host "1. Open http://localhost:4200 in browser" -ForegroundColor White
            Write-Host "2. Navigate to: Admin Panel > Communication Settings > Email Ticketing" -ForegroundColor White
            Write-Host "3. Badge should show 'OAuth 2.0 - Pending' (orange, pulsing)" -ForegroundColor White
            Write-Host "4. Click 'Authorize Now' button" -ForegroundColor White
            Write-Host "5. Login with marketing@oryggitech.com" -ForegroundColor White
        } else {
            Write-Host "V OAuth is fully configured and authorized!" -ForegroundColor Green
            Write-Host "Test the system by clicking 'Poll Now' or sending a test email." -ForegroundColor White
        }
    } else {
        Write-Host "X Fix failed - AuthenticationType is still $($verifyConfig.authenticationType)" -ForegroundColor Red
    }

} catch {
    Write-Host "  X Verification failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "================================" -ForegroundColor Cyan
Write-Host "Script Complete" -ForegroundColor Cyan
Write-Host "================================" -ForegroundColor Cyan
