# Verify Statistics Discrepancy
Write-Host "=== Verifying Statistics vs Complaints Discrepancy ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
$adminLogin = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$adminResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $adminLogin -ContentType "application/json"
$adminToken = $adminResponse.data.token

# Test 1: Get complaints count
Write-Host "Test 1: Complaints API..." -ForegroundColor Yellow
$complaintsUrl = 'http://localhost:5000/api/complaints'
$params = @{ page = 1; pageSize = 200 }
$complaintsResponse = Invoke-RestMethod -Uri $complaintsUrl -Method GET -Body $params -Headers @{ "Authorization" = "Bearer $adminToken" } -ContentType "application/json"
Write-Host "  Total via Complaints API: $($complaintsResponse.data.totalCount)" -ForegroundColor White
Write-Host ""

# Test 2: Get statistics count
Write-Host "Test 2: Statistics API..." -ForegroundColor Yellow
$statsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers @{ "Authorization" = "Bearer $adminToken" } -ContentType "application/json"
Write-Host "  Total via Statistics API: $($statsResponse.data.totalComplaints)" -ForegroundColor White
Write-Host "  Active: $($statsResponse.data.activeComplaints)" -ForegroundColor White
Write-Host "  Completed: $($statsResponse.data.completedComplaints)" -ForegroundColor White
Write-Host ""

Write-Host "  Status Breakdown from Statistics:" -ForegroundColor Cyan
$statsResponse.data.statusWidgets | Where-Object { $_.currentCount -gt 0 } | ForEach-Object {
    Write-Host "    - $($_.name): $($_.currentCount)" -ForegroundColor White
}
Write-Host ""

# Comparison
Write-Host "COMPARISON:" -ForegroundColor Yellow
$complaintsCount = $complaintsResponse.data.totalCount
$statsCount = $statsResponse.data.totalComplaints

if ($complaintsCount -eq $statsCount) {
    Write-Host "  PASS: Both APIs show same count ($complaintsCount)" -ForegroundColor Green
} else {
    Write-Host "  FAIL: MISMATCH DETECTED!" -ForegroundColor Red
    Write-Host "    Complaints API: $complaintsCount" -ForegroundColor Red
    Write-Host "    Statistics API: $statsCount" -ForegroundColor Red
    Write-Host "    Difference: $($statsCount - $complaintsCount)" -ForegroundColor Red
    Write-Host ""
    Write-Host "  Possible Causes:" -ForegroundColor Yellow
    Write-Host "    1. Statistics are reading from a different table/cache" -ForegroundColor White
    Write-Host "    2. Soft-deleted complaints being counted in statistics" -ForegroundColor White
    Write-Host "    3. Statistics calculation has a bug" -ForegroundColor White
    Write-Host "    4. Database synchronization issue" -ForegroundColor White
}

Write-Host ""
Write-Host "=== Verification Complete ===" -ForegroundColor Cyan
