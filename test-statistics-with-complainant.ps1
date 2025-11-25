# Test statistics API with complainant token
Write-Host "=== Testing Statistics API with Different Users ===" -ForegroundColor Cyan
Write-Host ""

# Login as complainant (Nav Nainital)
Write-Host "1. Logging in as COMPLAINANT (nav_nainital@yahoo.com)..." -ForegroundColor Yellow
$complainantLogin = @{
    email = "nav_nainital@yahoo.com"
    password = "Nav@12345"
} | ConvertTo-Json

$complainantResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $complainantLogin -ContentType "application/json"
$complainantToken = $complainantResponse.data.token

Write-Host "  Complainant logged in successfully" -ForegroundColor Green
Write-Host "  User: $($complainantResponse.data.fullName)" -ForegroundColor White
Write-Host ""

# Get statistics for complainant
Write-Host "2. Getting COMPLAINANT statistics..." -ForegroundColor Yellow
$complainantStats = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers @{ "Authorization" = "Bearer $complainantToken" } -ContentType "application/json"

Write-Host "  Total Complaints: $($complainantStats.data.totalComplaints)" -ForegroundColor $(if ($complainantStats.data.totalComplaints -eq 5) { "Green" } else { "Red" })
Write-Host "  Active: $($complainantStats.data.activeComplaints)" -ForegroundColor White
Write-Host "  Completed: $($complainantStats.data.completedComplaints)" -ForegroundColor White
Write-Host "  Status Breakdown:" -ForegroundColor Cyan
$complainantStats.data.statusWidgets | Where-Object { $_.currentCount -gt 0 } | ForEach-Object {
    Write-Host "    - $($_.name): $($_.currentCount)" -ForegroundColor White
}
Write-Host ""

# Login as admin
Write-Host "3. Logging in as ADMIN (admin@complaintmanagement.com)..." -ForegroundColor Yellow
$adminLogin = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$adminResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $adminLogin -ContentType "application/json"
$adminToken = $adminResponse.data.token

Write-Host "  Admin logged in successfully" -ForegroundColor Green
Write-Host "  User: $($adminResponse.data.fullName)" -ForegroundColor White
Write-Host ""

# Get statistics for admin
Write-Host "4. Getting ADMIN statistics..." -ForegroundColor Yellow
$adminStats = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers @{ "Authorization" = "Bearer $adminToken" } -ContentType "application/json"

Write-Host "  Total Complaints: $($adminStats.data.totalComplaints)" -ForegroundColor White
Write-Host "  Active: $($adminStats.data.activeComplaints)" -ForegroundColor White
Write-Host "  Completed: $($adminStats.data.completedComplaints)" -ForegroundColor White
Write-Host "  Status Breakdown:" -ForegroundColor Cyan
$adminStats.data.statusWidgets | Where-Object { $_.currentCount -gt 0 } | ForEach-Object {
    Write-Host "    - $($_.name): $($_.currentCount)" -ForegroundColor White
}
Write-Host ""

# Comparison
Write-Host "=== RESULTS ===" -ForegroundColor Cyan
Write-Host "Complainant sees: $($complainantStats.data.totalComplaints) complaints" -ForegroundColor $(if ($complainantStats.data.totalComplaints -eq 5) { "Green" } else { "Red" })
Write-Host "Admin sees: $($adminStats.data.totalComplaints) complaints" -ForegroundColor White
Write-Host ""

if ($complainantStats.data.totalComplaints -eq 5) {
    Write-Host "SUCCESS: Complainant statistics are correctly filtered!" -ForegroundColor Green
} else {
    Write-Host "FAIL: Complainant statistics are showing $($complainantStats.data.totalComplaints) instead of 5!" -ForegroundColor Red
}
