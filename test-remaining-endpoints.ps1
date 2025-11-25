$body = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$resp = Invoke-RestMethod -Uri 'http://localhost:5000/api/auth/login' -Method POST -Body $body -ContentType 'application/json'
$token = $resp.data.token
$headers = @{'Authorization'="Bearer $token"}

Write-Host "Testing corrected routes..." -ForegroundColor Cyan
Write-Host ""

Write-Host "Templates:" -ForegroundColor Yellow
try {
    $r = Invoke-RestMethod -Uri 'http://localhost:5000/api/communication-templates' -Headers $headers
    Write-Host "  Items: $($r.data.Count)" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Event Types:" -ForegroundColor Yellow
try {
    $r = Invoke-RestMethod -Uri 'http://localhost:5000/api/event-types' -Headers $headers
    Write-Host "  Items: $($r.data.Count)" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Resource Pools:" -ForegroundColor Yellow
try {
    $r = Invoke-RestMethod -Uri 'http://localhost:5000/api/resourcepool' -Headers $headers
    Write-Host "  Items: $($r.data.Count)" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "Escalation Policies:" -ForegroundColor Yellow
try {
    $r = Invoke-RestMethod -Uri 'http://localhost:5000/api/escalationpolicy' -Headers $headers
    Write-Host "  Items: $($r.data.Count)" -ForegroundColor Green
} catch {
    Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
}
