# Quick verification of the statistics bug
Write-Host "=== Quick Statistics Bug Verification ===" -ForegroundColor Cyan
Write-Host ""

# Read admin token
$token = (Get-Content ".working-token" -Raw).Trim()

# Test 1: Complaints API
Write-Host "Test 1: Complaints API" -ForegroundColor Yellow
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
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Test 2: Statistics API
Write-Host "Test 2: Statistics API" -ForegroundColor Yellow
try {
    $statsResp = Invoke-RestMethod -Uri "http://localhost:5000/api/dashboard/statistics" -Method GET -Headers $headers
    $statsCount = $statsResp.data.totalComplaints
    Write-Host "  Total Complaints: $statsCount" -ForegroundColor White
    Write-Host "  Active: $($statsResp.data.activeComplaints)" -ForegroundColor White
    Write-Host "  Completed: $($statsResp.data.completedComplaints)" -ForegroundColor White
} catch {
    Write-Host "  ERROR: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Comparison
Write-Host "RESULT:" -ForegroundColor Yellow
$discrepancy = $statsCount - $complaintsCount

if ($discrepancy -eq 0) {
    Write-Host "  PASS - Counts match ($complaintsCount)" -ForegroundColor Green
} else {
    Write-Host "  FAIL - Mismatch detected!" -ForegroundColor Red
    Write-Host "    Complaints API: $complaintsCount" -ForegroundColor Red
    Write-Host "    Statistics API: $statsCount" -ForegroundColor Red
    Write-Host "    Discrepancy: $discrepancy complaints" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Verification Complete ===" -ForegroundColor Cyan
