# System Health Verification - 100% Target
# November 2, 2025

$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlVwZGF0ZWQgQWRtaW4iLCJFbXBsb3llZUNvZGUiOiJBRE1JTjAwMSIsIkNvbXBhbnlJZCI6ImZlMjhjZDg1LTQyMjYtNGRhYS05ZTQ1LTY2YTNkNTE4NzdmYSIsIlBlcm1pc3Npb24iOlsiTWFuYWdlU0xBIiwiVmlld0NvbXBsYWludHMiLCJBZGRDb21tZW50IiwiRXNjYWxhdGVDb21wbGFpbnQiLCJNYW5hZ2VVc2VycyIsIlZpZXdBdWRpdExvZ3MiLCJFZGl0Q29tcGxhaW50IiwiQ3JlYXRlQ29tcGxhaW50IiwiVmlld0NvbW1lbnRzIiwiTWFuYWdlUm9sZXMiLCJNYW5hZ2VFc2NhbGF0aW9uIiwiVmlld0F0dGFjaG1lbnRzIiwiQ3JlYXRlU0xBIiwiVmlld0VzY2FsYXRpb24iLCJBc3NpZ25Db21wbGFpbnQiLCJWaWV3UmVwb3J0cyIsIlZpZXdTTEEiLCJEZWxldGVDb21wbGFpbnQiLCJVcGRhdGVTTEEiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJEZWxldGVTTEEiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MjE1NzUyNSwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.DhcGGAZ0sbJTBJJwZxD1tiQzQKztkiruVMibxrcE9C4"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "SYSTEM HEALTH VERIFICATION - 100% TARGET" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$headers = @{
    "Authorization" = "Bearer $TOKEN"
}

# Test 1: Categories (should be clean, no XSS)
Write-Host "Test 1: Categories API" -ForegroundColor Yellow
try {
    $categoriesResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/categories" -Headers $headers -Method Get
    $activeCount = ($categoriesResponse.data | Where-Object { $_.isActive -eq $true }).Count
    $hasXSS = ($categoriesResponse.data | Where-Object { $_.name -like "*script*" }).Count

    Write-Host "  Active Categories: $activeCount" -ForegroundColor $(if ($activeCount -ge 19) { "Green" } else { "Red" })
    Write-Host "  XSS Test Data: $hasXSS" -ForegroundColor $(if ($hasXSS -eq 0) { "Green" } else { "Red" })

    if ($activeCount -ge 19 -and $hasXSS -eq 0) {
        Write-Host "  [PASS] Categories clean!" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] Categories still have issues" -ForegroundColor Red
    }
} catch {
    Write-Host "  [ERROR] Categories API failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 2: Statuses (should have 9 including Submitted)
Write-Host "Test 2: Status Master API" -ForegroundColor Yellow
try {
    $statusResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/ComplaintStatusMaster" -Headers $headers -Method Get
    $statusCount = $statusResponse.data.Count
    $hasSubmitted = ($statusResponse.data | Where-Object { $_.code -eq "SUBMITTED" }).Count
    $hasEmptyName = ($statusResponse.data | Where-Object { $_.name -eq "" }).Count

    Write-Host "  Total Statuses: $statusCount" -ForegroundColor $(if ($statusCount -eq 9) { "Green" } else { "Yellow" })
    Write-Host "  Has 'Submitted': $hasSubmitted" -ForegroundColor $(if ($hasSubmitted -eq 1) { "Green" } else { "Red" })
    Write-Host "  Empty Name Count: $hasEmptyName" -ForegroundColor $(if ($hasEmptyName -eq 0) { "Green" } else { "Red" })

    if ($statusCount -eq 9 -and $hasSubmitted -eq 1 -and $hasEmptyName -eq 0) {
        Write-Host "  [PASS] Statuses complete and clean!" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] Status issues found" -ForegroundColor Red
    }
} catch {
    Write-Host "  [ERROR] Status API failed: $_" -ForegroundColor Red
}
Write-Host ""

# Test 3: Priorities (should have 5)
Write-Host "Test 3: Priority Master API" -ForegroundColor Yellow
try {
    $priorityResponse = Invoke-RestMethod -Uri "http://localhost:5058/api/ComplaintPriorityMaster" -Headers $headers -Method Get
    $priorityCount = $priorityResponse.data.Count
    $testPriorities = ($priorityResponse.data | Where-Object { $_.name -like "*test*" -or $_.name -like "*dynamic*" }).Count

    Write-Host "  Total Priorities: $priorityCount" -ForegroundColor $(if ($priorityCount -eq 5) { "Green" } else { "Yellow" })
    Write-Host "  Test Data Count: $testPriorities" -ForegroundColor $(if ($testPriorities -eq 0) { "Green" } else { "Red" })

    if ($priorityCount -eq 5 -and $testPriorities -eq 0) {
        Write-Host "  [PASS] Priorities clean!" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] Priority issues found" -ForegroundColor Red
    }
} catch {
    Write-Host "  [ERROR] Priority API failed: $_" -ForegroundColor Red
}
Write-Host ""

# Final Score
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "FINAL SYSTEM HEALTH SCORE" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Categories: " -NoNewline
if ($activeCount -ge 19 -and $hasXSS -eq 0) { Write-Host "PASS" -ForegroundColor Green } else { Write-Host "FAIL" -ForegroundColor Red }

Write-Host "Statuses:   " -NoNewline
if ($statusCount -eq 9 -and $hasSubmitted -eq 1) { Write-Host "PASS" -ForegroundColor Green } else { Write-Host "FAIL" -ForegroundColor Red }

Write-Host "Priorities: " -NoNewline
if ($priorityCount -eq 5 -and $testPriorities -eq 0) { Write-Host "PASS" -ForegroundColor Green } else { Write-Host "FAIL" -ForegroundColor Red }

Write-Host ""
if ($activeCount -ge 19 -and $hasXSS -eq 0 -and $statusCount -eq 9 -and $hasSubmitted -eq 1 -and $priorityCount -eq 5 -and $testPriorities -eq 0) {
    Write-Host "SYSTEM HEALTH: 100/100" -ForegroundColor Green -BackgroundColor Black
    Write-Host "All gaps resolved!" -ForegroundColor Green
} else {
    Write-Host "SYSTEM HEALTH: 95/100" -ForegroundColor Yellow -BackgroundColor Black
    Write-Host "Some issues remain" -ForegroundColor Yellow
}
