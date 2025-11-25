# Investigation 2: Find the 32 "missing" complaints
Write-Host "=== INVESTIGATION 2: Finding the 32 Complaints ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
Write-Host "Step 1: Logging in as ADMIN..." -ForegroundColor Yellow
$adminLogin = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$adminResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $adminLogin -ContentType "application/json"
$adminToken = $adminResponse.data.token
Write-Host "  Logged in as Admin" -ForegroundColor Green
Write-Host ""

# Get ALL complaints
Write-Host "Step 2: Fetching ALL complaints..." -ForegroundColor Yellow
$complaintsUrl = 'http://localhost:5000/api/complaints'
$params = @{
    page = 1
    pageSize = 200
}
$response = Invoke-RestMethod -Uri $complaintsUrl -Method GET -Body $params -Headers @{ "Authorization" = "Bearer $adminToken" } -ContentType "application/json"

Write-Host "  Total Complaints: $($response.data.totalCount)" -ForegroundColor White
Write-Host ""

# Group by complainant
Write-Host "Step 3: Grouping by Complainant..." -ForegroundColor Yellow
$byComplainant = $response.data.items | Group-Object -Property complainantName | Select-Object Name, Count | Sort-Object Count -Descending

Write-Host "  Breakdown by Complainant:" -ForegroundColor Cyan
$byComplainant | ForEach-Object {
    $color = if ($_.Name -eq "Nav Nainital") { "Green" } else { "Yellow" }
    Write-Host "    $($_.Name): $($_.Count) complaints" -ForegroundColor $color
}
Write-Host ""

# Group by status
Write-Host "Step 4: Grouping by Status..." -ForegroundColor Yellow
$byStatus = $response.data.items | Group-Object -Property status | Select-Object Name, Count | Sort-Object Count -Descending

Write-Host "  Breakdown by Status:" -ForegroundColor Cyan
$byStatus | ForEach-Object {
    Write-Host "    $($_.Name): $($_.Count) complaints" -ForegroundColor White
}
Write-Host ""

# Show the 5 test complaints
Write-Host "Step 5: Locating the 5 Test Complaints..." -ForegroundColor Yellow
$testComplaints = $response.data.items | Where-Object { $_.complaintNumber -match "CMP-2025-114[3-7]" } | Sort-Object complaintNumber

if ($testComplaints.Count -gt 0) {
    Write-Host "  Found $($testComplaints.Count) test complaints:" -ForegroundColor Green
    $testComplaints | ForEach-Object {
        Write-Host "    - $($_.complaintNumber): $($_.title)" -ForegroundColor White
    }
} else {
    Write-Host "  ERROR: Test complaints NOT found!" -ForegroundColor Red
}
Write-Host ""

# Show sample of "other" complaints
Write-Host "Step 6: Sample of OTHER Complaints..." -ForegroundColor Yellow
$otherComplaints = $response.data.items | Where-Object { $_.complaintNumber -notmatch "CMP-2025-114[3-7]" } | Select-Object -First 10

if ($otherComplaints.Count -gt 0) {
    Write-Host "  Sample of other $($response.data.totalCount - 5) complaints:" -ForegroundColor Cyan
    $otherComplaints | ForEach-Object {
        Write-Host "    - $($_.complaintNumber): $($_.title) (Owner: $($_.complainantName), Created: $($_.createdAt))" -ForegroundColor Yellow
    }
} else {
    Write-Host "  No other complaints found" -ForegroundColor Green
}
Write-Host ""

# Summary
Write-Host "Step 7: SUMMARY..." -ForegroundColor Yellow
Write-Host "  Database State:" -ForegroundColor Cyan
Write-Host "    Total Complaints: $($response.data.totalCount)" -ForegroundColor White
Write-Host "    Nav Nainital's Complaints: $(($byComplainant | Where-Object { $_.Name -eq 'Nav Nainital' }).Count)" -ForegroundColor Green
Write-Host "    Other Users' Complaints: $($response.data.totalCount - 5)" -ForegroundColor Yellow
Write-Host ""

if ($response.data.totalCount -eq 5) {
    Write-Host "  STATUS: Database is clean - only 5 test complaints" -ForegroundColor Green
} else {
    Write-Host "  STATUS: Database has $($response.data.totalCount - 5) OLD complaints that need investigation" -ForegroundColor Red
    Write-Host "  ACTION NEEDED: Review if these should be deleted" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== INVESTIGATION 2 COMPLETE ===" -ForegroundColor Cyan
