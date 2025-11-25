$BaseUrl = "http://localhost:5000"

Write-Host "Checking login response structure..." -ForegroundColor Cyan

$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Sending login request..." -ForegroundColor Yellow
try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

    Write-Host "`nLogin Response:" -ForegroundColor Green
    $loginResponse | ConvertTo-Json -Depth 5

    Write-Host "`nResponse Properties:" -ForegroundColor Yellow
    $loginResponse | Get-Member | Where-Object { $_.MemberType -eq 'NoteProperty' } | ForEach-Object {
        Write-Host "  - $($_.Name): $($loginResponse.($_.Name))"
    }

    if ($loginResponse.token) {
        Write-Host "`nToken exists: YES (Length: $($loginResponse.token.Length))" -ForegroundColor Green
    } elseif ($loginResponse.data.token) {
        Write-Host "`nToken exists in data: YES (Length: $($loginResponse.data.token.Length))" -ForegroundColor Green
    } else {
        Write-Host "`nToken exists: NO" -ForegroundColor Red
    }

} catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
}
