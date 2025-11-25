# Simple OAuth System Configuration API Test

$baseUrl = "http://localhost:5000"
$adminEmail = "admin@complaintmanagement.com"
$adminPassword = "Admin@123"

Write-Host "Testing OAuth System Configuration API..." -ForegroundColor Cyan

# Login
$loginPayload = @{
    employeeIdOrEmail = $adminEmail
    password = $adminPassword
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginPayload -ContentType "application/json"
$token = $loginResponse.data.token

Write-Host "Login successful. Token: $($token.Substring(0,20))..." -ForegroundColor Green

# Get system configuration
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$sysConfig = Invoke-RestMethod -Uri "$baseUrl/api/systemconfiguration" -Method GET -Headers $headers

Write-Host "Current OAuth Token Refresh Interval: $($sysConfig.oAuthTokenRefreshIntervalMinutes) minutes" -ForegroundColor Yellow

# Update to 30 minutes
$sysConfig.oAuthTokenRefreshIntervalMinutes = 30

$updateResponse = Invoke-RestMethod -Uri "$baseUrl/api/systemconfiguration" -Method PUT -Headers $headers -Body ($sysConfig | ConvertTo-Json -Depth 10)

Write-Host "Updated OAuth Token Refresh Interval: $($updateResponse.oAuthTokenRefreshIntervalMinutes) minutes" -ForegroundColor Green

# Verify
$verifyConfig = Invoke-RestMethod -Uri "$baseUrl/api/systemconfiguration" -Method GET -Headers $headers

if ($verifyConfig.oAuthTokenRefreshIntervalMinutes -eq 30) {
    Write-Host "PASS: Settings persisted correctly (30 minutes)" -ForegroundColor Green
} else {
    Write-Host "FAIL: Settings did not persist ($($verifyConfig.oAuthTokenRefreshIntervalMinutes) minutes)" -ForegroundColor Red
}

Write-Host "`nBackend API Test Complete!" -ForegroundColor Cyan
