# Check whose token is in .working-token
Write-Host "=== Checking .working-token File ===" -ForegroundColor Cyan
Write-Host ""

$token = (Get-Content ".working-token" -Raw).Trim()

# Decode JWT
$parts = $token.Split('.')
if ($parts.Count -eq 3) {
    $payload = $parts[1]
    $padding = 4 - ($payload.Length % 4)
    if ($padding -ne 4) {
        $payload += "=" * $padding
    }
    $decodedBytes = [System.Convert]::FromBase64String($payload)
    $decodedJson = [System.Text.Encoding]::UTF8.GetString($decodedBytes)
    $claims = $decodedJson | ConvertFrom-Json

    Write-Host "Token belongs to:" -ForegroundColor Yellow
    Write-Host "  User ID: $($claims.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier')" -ForegroundColor White
    Write-Host "  Email: $($claims.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress')" -ForegroundColor White
    Write-Host "  Name: $($claims.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name')" -ForegroundColor White
    Write-Host ""
    Write-Host "  Permissions:" -ForegroundColor Cyan
    if ($claims.Permission -is [array]) {
        Write-Host "  Total: $($claims.Permission.Count) permissions" -ForegroundColor Gray
        Write-Host "  Has ManageUsers: $(if ($claims.Permission -contains 'ManageUsers') { 'YES' } else { 'NO' })" -ForegroundColor $(if ($claims.Permission -contains 'ManageUsers') { "Green" } else { "Red" })
        Write-Host "  Has ManageSettings: $(if ($claims.Permission -contains 'ManageSettings') { 'YES' } else { 'NO' })" -ForegroundColor $(if ($claims.Permission -contains 'ManageSettings') { "Green" } else { "Red" })
        Write-Host "  Has ManageCompany: $(if ($claims.Permission -contains 'ManageCompany') { 'YES' } else { 'NO' })" -ForegroundColor $(if ($claims.Permission -contains 'ManageCompany') { "Green" } else { "Red" })
    } else {
        Write-Host "    - $($claims.Permission)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=== Check Complete ===" -ForegroundColor Cyan
