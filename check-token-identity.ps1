# Check which user the test token belongs to
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

# Decode JWT
$parts = $token.Split('.')
$payload = $parts[1]

# Add padding if needed
$payload += '=' * ((4 - ($payload.Length % 4)) % 4)

# Decode from Base64
$bytes = [System.Convert]::FromBase64String($payload)
$json = [System.Text.Encoding]::UTF8.GetString($bytes)

Write-Host "=== Token Identity ===" -ForegroundColor Cyan
$tokenData = $json | ConvertFrom-Json

Write-Host "User ID: $($tokenData.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier')" -ForegroundColor Yellow
Write-Host "Email: $($tokenData.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress')" -ForegroundColor Yellow
Write-Host "Name: $($tokenData.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')" -ForegroundColor Yellow
Write-Host "Employee Code: $($tokenData.EmployeeCode)" -ForegroundColor Yellow
Write-Host ""
Write-Host "This token belongs to: ADMIN USER" -ForegroundColor Red
Write-Host ""
Write-Host "Expected complainant ID: fd0073b8-fc95-4a49-867c-6ffb38b7d177"
Write-Host "Actual token user ID: $($tokenData.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier')"
