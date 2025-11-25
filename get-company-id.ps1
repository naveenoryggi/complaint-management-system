$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

# Decode JWT to get company ID
$tokenParts = $token.Split(".")
$payload = $tokenParts[1]

# Add padding if needed
$padding = 4 - ($payload.Length % 4)
if ($padding -ne 4) {
    $payload += "=" * $padding
}

$payloadJson = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
$payloadObj = $payloadJson | ConvertFrom-Json

Write-Host "Company ID from token: $($payloadObj.CompanyId)"
Write-Host "User ID from token: $($payloadObj.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier')"
Write-Host "Email from token: $($payloadObj.'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress')"
