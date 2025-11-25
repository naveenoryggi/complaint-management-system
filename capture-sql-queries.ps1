# Capture SQL queries for both APIs
Write-Host "=== Capturing SQL Queries ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResp.data.token
$headers = @{ "Authorization" = "Bearer $token" }

Write-Host "Step 1: Calling Complaints API..." -ForegroundColor Yellow
Start-Sleep -Seconds 1
$params = @{ page = 1; pageSize = 200 }
$complaintsResp = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method GET -Body $params -Headers $headers -ContentType "application/json"
Write-Host "  Returned: $($complaintsResp.data.totalCount) complaints" -ForegroundColor White
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "Step 2: Calling Statistics API..." -ForegroundColor Yellow
Start-Sleep -Seconds 1
$statsResp = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers $headers
Write-Host "  Returned: $($statsResp.data.totalComplaints) complaints" -ForegroundColor White
Start-Sleep -Seconds 2

Write-Host ""
Write-Host "=== API Calls Complete ===" -ForegroundColor Green
Write-Host "Check backend console for SQL queries" -ForegroundColor Yellow
