# Decode admin JWT token to verify role
Write-Host "=== Decoding Admin Token ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResp.data.token

Write-Host "Admin Token Received" -ForegroundColor Green
Write-Host ""

# Decode JWT (parse the payload)
$parts = $token.Split('.')
if ($parts.Count -eq 3) {
    $payload = $parts[1]

    # Add padding if needed
    $padding = 4 - ($payload.Length % 4)
    if ($padding -ne 4) {
        $payload += "=" * $padding
    }

    # Decode from base64
    $decodedBytes = [System.Convert]::FromBase64String($payload)
    $decodedJson = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
    $claims = $decodedJson | ConvertFrom-Json

    Write-Host "Token Claims:" -ForegroundColor Yellow
    Write-Host "  User ID: $($claims.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier')" -ForegroundColor White
    Write-Host "  Email: $($claims.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress')" -ForegroundColor White
    Write-Host "  Name: $($claims.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')" -ForegroundColor White
    Write-Host "  Company ID: $($claims.CompanyId)" -ForegroundColor White
    Write-Host ""
    Write-Host "  Permissions:" -ForegroundColor Cyan
    if ($claims.Permission -is [array]) {
        $claims.Permission | ForEach-Object { Write-Host "    - $_" -ForegroundColor Gray }
    } else {
        Write-Host "    - $($claims.Permission)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ERROR: Invalid JWT format" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Decode Complete ===" -ForegroundColor Cyan
