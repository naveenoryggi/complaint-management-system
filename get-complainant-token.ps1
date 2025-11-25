# Get fresh token for complainant user
Write-Host "Getting token for complainant user..." -ForegroundColor Cyan

$loginBody = @{
    email = "nav_nainital@yahoo.com"
    password = "Nav@12345"
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    if ($response.isSuccess) {
        Write-Host "Login successful!" -ForegroundColor Green
        Write-Host "User: $($response.data.user.fullName)" -ForegroundColor Yellow
        Write-Host "Email: $($response.data.user.email)" -ForegroundColor Yellow
        Write-Host "User ID: $($response.data.user.id)" -ForegroundColor Yellow
        Write-Host ""

        # Save token
        $response.data.token | Out-File ".complainant-token" -Encoding utf8 -NoNewline
        Write-Host "Token saved to: .complainant-token" -ForegroundColor Green
    } else {
        Write-Host "Login failed: $($response.message)" -ForegroundColor Red
    }
} catch {
    Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
}
