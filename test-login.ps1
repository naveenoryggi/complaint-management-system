$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

Write-Host "Attempting login..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"

Write-Host "Response structure:" -ForegroundColor Cyan
$response | ConvertTo-Json -Depth 5

Write-Host "`nToken extraction:" -ForegroundColor Cyan
if ($response.token) {
    Write-Host "Direct token: $($response.token)" -ForegroundColor Green
} elseif ($response.data.token) {
    Write-Host "Data.token: $($response.data.token)" -ForegroundColor Green
} else {
    Write-Host "Token not found in expected locations" -ForegroundColor Red
}
