# Test with fresh admin token
Write-Host "=== Statistics Bug Verification with Fresh Token ===" -ForegroundColor Cyan
Write-Host ""

# Login as admin
Write-Host "Step 1: Logging in as admin..." -ForegroundColor Yellow
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResp = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResp.data.token
    Write-Host "  Logged in successfully" -ForegroundColor Green
} catch {
    Write-Host "  Login FAILED: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 1: Complaints API
Write-Host "Step 2: Testing Complaints API..." -ForegroundColor Yellow
$headers = @{
    "Authorization" = "Bearer $token"
}
$params = @{
    page = 1
    pageSize = 200
}

try {
    $complaintsResp = Invoke-RestMethod -Uri "http://localhost:5000/api/complaints" -Method GET -Body $params -Headers $headers -ContentType "application/json"
    $complaintsCount = $complaintsResp.data.totalCount
    Write-Host "  Total Count: $complaintsCount" -ForegroundColor White

    # Save some sample data
    if ($complaintsResp.data.items.Count -gt 0) {
        $sample = $complaintsResp.data.items[0]
        Write-Host "  Sample Complaint: $($sample.complaintNumber) - $($sample.title)" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Statistics API
Write-Host "Step 3: Testing Statistics API..." -ForegroundColor Yellow
try {
    $statsResp = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers $headers
    $statsCount = $statsResp.data.totalComplaints
    Write-Host "  Total Complaints: $statsCount" -ForegroundColor White
    Write-Host "  Active: $($statsResp.data.activeComplaints)" -ForegroundColor White
    Write-Host "  Completed: $($statsResp.data.completedComplaints)" -ForegroundColor White

    # Show status breakdown
    Write-Host "  Status Breakdown:" -ForegroundColor Gray
    $statsResp.data.statusWidgets | Where-Object { $_.currentCount -gt 0 } | ForEach-Object {
        Write-Host "    - $($_.name): $($_.currentCount)" -ForegroundColor Gray
    }
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Comparison
Write-Host "Step 4: Comparison Analysis..." -ForegroundColor Yellow
$discrepancy = $statsCount - $complaintsCount

Write-Host ""
Write-Host "FINAL RESULT:" -ForegroundColor Cyan
Write-Host "  Complaints API: $complaintsCount" -ForegroundColor White
Write-Host "  Statistics API: $statsCount" -ForegroundColor White
Write-Host "  Discrepancy: $discrepancy" -ForegroundColor $(if ($discrepancy -eq 0) { "Green" } else { "Red" })

Write-Host ""
if ($discrepancy -eq 0) {
    Write-Host "✅ BUG FIXED - Both APIs return same count!" -ForegroundColor Green
} else {
    Write-Host "🔴 BUG STILL EXISTS - $discrepancy complaint mismatch" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
