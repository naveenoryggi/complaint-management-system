Write-Host "Attempting login to get fresh token..." -ForegroundColor Cyan

$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    Write-Host "POST http://localhost:5000/api/auth/login" -ForegroundColor Yellow
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    if ($response.isSuccess -and $response.data.token) {
        $token = $response.data.token
        Write-Host "SUCCESS: Login successful!" -ForegroundColor Green

        # Save to .oauth-fix-token
        $token | Out-File -FilePath ".oauth-fix-token" -NoNewline -Encoding utf8
        Write-Host "Token saved to .oauth-fix-token" -ForegroundColor Green

        # Display token info
        Write-Host "`nToken (first 50 chars): $($token.Substring(0, [Math]::Min(50, $token.Length)))..." -ForegroundColor Cyan

        return $token
    } else {
        Write-Host "ERROR: Login failed - no token in response" -ForegroundColor Red
        $response | ConvertTo-Json -Depth 5
    }
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Yellow
    }
}
