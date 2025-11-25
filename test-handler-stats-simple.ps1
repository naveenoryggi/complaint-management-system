# Test Handler Role Statistics - Simple Version
Write-Host "=== INVESTIGATION 1: Handler Statistics ===" -ForegroundColor Cyan
Write-Host ""

# Login as handler
Write-Host "Step 1: Logging in as HANDLER..." -ForegroundColor Yellow
$loginBody = @{
    email = "naveen.chandra@oryggitech.com"
    password = "Naveen@12345"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
Write-Host "  Logged in: $($loginResponse.data.fullName)" -ForegroundColor Green
Write-Host ""

# Get complaints
Write-Host "Step 2: Checking assigned complaints..." -ForegroundColor Yellow
$complaintsUrl = 'http://localhost:5000/api/complaints'
$params = @{
    page = 1
    pageSize = 100
}
$complaintsResponse = Invoke-RestMethod -Uri $complaintsUrl -Method GET -Body $params -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
$complaintsCount = $complaintsResponse.data.totalCount
Write-Host "  Complaints via API: $complaintsCount" -ForegroundColor White
Write-Host ""

# Get statistics
Write-Host "Step 3: Getting statistics..." -ForegroundColor Yellow
$statsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers @{ "Authorization" = "Bearer $token" } -ContentType "application/json"
$statsCount = $statsResponse.data.totalComplaints
Write-Host "  Complaints via Stats: $statsCount" -ForegroundColor White
Write-Host "  Active: $($statsResponse.data.activeComplaints)" -ForegroundColor White
Write-Host "  Completed: $($statsResponse.data.completedComplaints)" -ForegroundColor White
Write-Host ""

# Validation
Write-Host "Step 4: VALIDATION..." -ForegroundColor Yellow
if ($complaintsCount -eq $statsCount) {
    Write-Host "  PASS: Counts match ($complaintsCount = $statsCount)" -ForegroundColor Green
} else {
    Write-Host "  FAIL: Mismatch! Complaints=$complaintsCount, Stats=$statsCount" -ForegroundColor Red
}

if ($complaintsCount -eq 0) {
    Write-Host "  WARNING: Handler has NO complaints assigned" -ForegroundColor Yellow
} else {
    Write-Host "  Handler has $complaintsCount complaints" -ForegroundColor Green
}

Write-Host ""
Write-Host "=== INVESTIGATION 1 COMPLETE ===" -ForegroundColor Cyan
