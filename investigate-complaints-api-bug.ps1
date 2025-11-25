# Investigate why Complaints API returns only 5 out of 37
Write-Host "=== Investigating Complaints API Bug ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResp.data.token

# Get complaints with different page sizes
Write-Host "Test 1: Get complaints with pageSize=10..." -ForegroundColor Yellow
$headers = @{ "Authorization" = "Bearer $token" }
$params1 = @{ page = 1; pageSize = 10 }
$resp1 = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method GET -Body $params1 -Headers $headers -ContentType "application/json"
Write-Host "  Page 1 (size 10): Returned $($resp1.data.items.Count) items, Total: $($resp1.data.totalCount)" -ForegroundColor White
Write-Host ""

Write-Host "Test 2: Get complaints with pageSize=50..." -ForegroundColor Yellow
$params2 = @{ page = 1; pageSize = 50 }
$resp2 = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method GET -Body $params2 -Headers $headers -ContentType "application/json"
Write-Host "  Page 1 (size 50): Returned $($resp2.data.items.Count) items, Total: $($resp2.data.totalCount)" -ForegroundColor White
Write-Host ""

Write-Host "Test 3: Get complaints with pageSize=200..." -ForegroundColor Yellow
$params3 = @{ page = 1; pageSize = 200 }
$resp3 = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method GET -Body $params3 -Headers $headers -ContentType "application/json"
Write-Host "  Page 1 (size 200): Returned $($resp3.data.items.Count) items, Total: $($resp3.data.totalCount)" -ForegroundColor White
Write-Host ""

# Show the complaint numbers returned
Write-Host "Complaint Numbers Returned:" -ForegroundColor Cyan
$resp3.data.items | ForEach-Object {
    Write-Host "  - $($_.complaintNumber): $($_.title)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== Investigation Complete ===" -ForegroundColor Cyan
