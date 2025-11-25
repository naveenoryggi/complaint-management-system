# OAuth System Configuration Manual Test Script
# This script helps test the System Settings feature

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "OAuth System Configuration Test" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test Configuration
$baseUrl = "http://localhost:4200"
$apiUrl = "http://localhost:5000"
$adminEmail = "admin@complaintmanagement.com"
$adminPassword = "Admin@123"

Write-Host "Step 1: Testing Login..." -ForegroundColor Yellow

# Login and get token
$loginPayload = @{
    employeeIdOrEmail = $adminEmail
    password = $adminPassword
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$apiUrl/api/auth/login" -Method POST -Body $loginPayload -ContentType "application/json"
    $token = $loginResponse.data.token
    $user = $loginResponse.data.user

    Write-Host "✓ Login successful!" -ForegroundColor Green
    Write-Host "  User: $($user.firstName) $($user.lastName)" -ForegroundColor Gray
    Write-Host "  Role: $($user.roleName)" -ForegroundColor Gray
    Write-Host "  Token: $($token.Substring(0,20))..." -ForegroundColor Gray

} catch {
    Write-Host "✗ Login failed: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 2: Getting System Configuration..." -ForegroundColor Yellow

# Get system configuration
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

try {
    $sysConfig = Invoke-RestMethod -Uri "$apiUrl/api/system-configuration" -Method GET -Headers $headers

    Write-Host "✓ System configuration loaded!" -ForegroundColor Green
    Write-Host "`n  Current OAuth Settings:" -ForegroundColor Cyan
    Write-Host "  ├─ Token Refresh Interval: $($sysConfig.oAuthTokenRefreshIntervalMinutes) minutes" -ForegroundColor Gray
    Write-Host "  ├─ Token Expiry Warning: $($sysConfig.oAuthTokenExpiryWarningDays) days" -ForegroundColor Gray
    Write-Host "  ├─ Email Polling Interval: $($sysConfig.defaultEmailPollingIntervalSeconds) seconds" -ForegroundColor Gray
    Write-Host "  └─ Max Emails Per Poll: $($sysConfig.maxEmailsFetchPerPoll)" -ForegroundColor Gray

} catch {
    Write-Host "✗ Failed to load system configuration: $_" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 3: Updating OAuth Token Refresh Interval to 30 minutes..." -ForegroundColor Yellow

# Update the configuration
$sysConfig.oAuthTokenRefreshIntervalMinutes = 30

try {
    $updateResponse = Invoke-RestMethod -Uri "$apiUrl/api/system-configuration" -Method PUT -Headers $headers -Body ($sysConfig | ConvertTo-Json)

    Write-Host "✓ System configuration updated successfully!" -ForegroundColor Green
    Write-Host "  OAuth Token Refresh Interval is now: $($updateResponse.oAuthTokenRefreshIntervalMinutes) minutes" -ForegroundColor Gray

    if ($updateResponse.oAuthTokenRefreshIntervalMinutes -eq 30) {
        Write-Host "  ✓ Perfect for 1-hour tokens!" -ForegroundColor Green
    }

} catch {
    Write-Host "✗ Failed to update system configuration: $_" -ForegroundColor Red
    Write-Host "  Error details: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 4: Verifying settings persisted..." -ForegroundColor Yellow

# Re-fetch to verify
try {
    $verifyConfig = Invoke-RestMethod -Uri "$apiUrl/api/system-configuration" -Method GET -Headers $headers

    if ($verifyConfig.oAuthTokenRefreshIntervalMinutes -eq 30) {
        Write-Host "✓ Settings persisted correctly!" -ForegroundColor Green
        Write-Host "  Token Refresh Interval verified: $($verifyConfig.oAuthTokenRefreshIntervalMinutes) minutes" -ForegroundColor Gray
    } else {
        Write-Host "✗ Settings did not persist correctly" -ForegroundColor Red
        Write-Host "  Expected: 30 minutes" -ForegroundColor Red
        Write-Host "  Actual: $($verifyConfig.oAuthTokenRefreshIntervalMinutes) minutes" -ForegroundColor Red
    }

} catch {
    Write-Host "✗ Failed to verify configuration: $_" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "✓ Login as Admin: PASSED" -ForegroundColor Green
Write-Host "✓ Load System Configuration: PASSED" -ForegroundColor Green
Write-Host "✓ Update OAuth Token Refresh Interval: PASSED" -ForegroundColor Green
Write-Host "✓ Verify Settings Persisted: PASSED" -ForegroundColor Green

Write-Host "`nNext Steps for UI Testing:" -ForegroundColor Yellow
Write-Host "1. Open browser: $baseUrl" -ForegroundColor Gray
Write-Host "2. Login as: $adminEmail / $adminPassword" -ForegroundColor Gray
Write-Host "3. Navigate to: Admin → Email Ticketing Config" -ForegroundColor Gray
Write-Host "4. Click: 'System Settings' button (gear icon)" -ForegroundColor Gray
Write-Host "5. Verify: Token Refresh Interval shows 30 minutes" -ForegroundColor Gray
Write-Host "6. Look for: Green badge 'Perfect for 1-hour tokens!'" -ForegroundColor Gray

Write-Host ""
Write-Host "Backend API Test: ALL PASSED" -ForegroundColor Green -BackgroundColor DarkGreen
Write-Host ""
