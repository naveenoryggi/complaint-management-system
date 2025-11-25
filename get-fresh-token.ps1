# Get Fresh Token Script
$baseUrl = "http://localhost:5058/api"

Write-Host "Getting fresh token..." -ForegroundColor Cyan

$loginBody = @{
    Email = "admin@complaintmanagement.com"
    Password = "Admin@123"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "$baseUrl/auth/login" -Method Post -Body $loginBody -ContentType "application/json"

    $token = if ($response.data.token) { $response.data.token } elseif ($response.token) { $response.token } else { $null }

    if ($token) {
        $token | Out-File -FilePath ".test-token" -NoNewline -Encoding UTF8
        Write-Host "✅ Token saved to .test-token" -ForegroundColor Green
        Write-Host "Token: $($token.Substring(0, 50))..." -ForegroundColor Yellow
    } else {
        Write-Host "❌ No token in response" -ForegroundColor Red
        Write-Host "Response: $($response | ConvertTo-Json)"
    }
} catch {
    Write-Host "❌ Login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.ErrorDetails) {
        Write-Host "Details: $($_.ErrorDetails.Message)" -ForegroundColor Red
    }
}
