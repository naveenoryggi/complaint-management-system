# Debug Token Claims
$loginUrl = "http://localhost:5000/api/auth/login"
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri $loginUrl -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.token

Write-Host "Token received:" -ForegroundColor Cyan
Write-Host $token -ForegroundColor Gray

# Decode JWT (simple base64 decode of payload)
$parts = $token.Split('.')
if ($parts.Length -eq 3) {
    $payload = $parts[1]
    # Add padding if needed
    while ($payload.Length % 4 -ne 0) {
        $payload += '='
    }
    $decoded = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($payload))
    Write-Host "`nDecoded Token Payload:" -ForegroundColor Cyan
    $decoded | ConvertFrom-Json | ConvertTo-Json -Depth 5
} else {
    Write-Host "Invalid token format" -ForegroundColor Red
}
