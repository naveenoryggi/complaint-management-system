$body = @{
    Email = 'admin@complaintmanagement.com'
    Password = 'Admin@123'
} | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method Post -Body $body -ContentType 'application/json' -TimeoutSec 10
    Write-Host "Login successful!" -ForegroundColor Green
    Write-Host "Username: $($response.username)"
    Write-Host "Role: $($response.role)"
    $response.token | Out-File '.admin-token' -NoNewline
    Write-Host "Token saved to .admin-token"

    # Output credentials for user
    Write-Host "`n==================================" -ForegroundColor Cyan
    Write-Host "Admin Login Credentials:" -ForegroundColor Cyan
    Write-Host "==================================" -ForegroundColor Cyan
    Write-Host "URL: http://localhost:4200/login"
    Write-Host "Email: admin@complaintmanagement.com"
    Write-Host "Password: Admin@123"
    Write-Host "==================================" -ForegroundColor Cyan
} catch {
    Write-Host "Login failed: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
    }
}
