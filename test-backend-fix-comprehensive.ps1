# Comprehensive Backend Fix Verification for "Unknown Unknown" Bug
# Testing .Include(c => c.StatusMaster) and .Include(c => c.PriorityMaster) fix

$token = Get-Content '.fresh-token' -Raw
$token = $token.Trim()
$headers = @{
    'Authorization' = "Bearer $token"
}

# Test data: complaint IDs
$testComplaints = @(
    @{Number = "CMP-2025-1110"; Id = "dc5f95da-92d1-40f9-8ed3-1b91f0b70c34"},
    @{Number = "CMP-2025-1103"; Id = "b8a64ad3-979a-4698-9523-dbadeb72cbdf"},
    @{Number = "CMP-2025-1102"; Id = "4c0c7a9c-fb54-4df7-b957-4b93bf307505"}
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "BACKEND FIX VERIFICATION REPORT" -ForegroundColor Cyan
Write-Host "Fix: Added .Include(c => c.StatusMaster) and .Include(c => c.PriorityMaster)" -ForegroundColor Cyan
Write-Host "Expected: Status and Priority fields should show actual values, not 'Unknown'" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

$passCount = 0
$failCount = 0
$results = @()

foreach ($complaint in $testComplaints) {
    Write-Host "Testing: $($complaint.Number)" -ForegroundColor Yellow
    Write-Host "ID: $($complaint.Id)" -ForegroundColor Gray

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5058/api/complaints/$($complaint.Id)" -Headers $headers -Method Get

        $complaintData = $response.data

        Write-Host "  Complaint Number: $($complaintData.complaintNumber)" -ForegroundColor White
        Write-Host "  Title: $($complaintData.title)" -ForegroundColor White
        Write-Host "  Status: '$($complaintData.status)'" -ForegroundColor $(if ($complaintData.status -and $complaintData.status -ne "Unknown") { "Green" } else { "Red" })
        Write-Host "  Priority: '$($complaintData.priority)'" -ForegroundColor $(if ($complaintData.priority -and $complaintData.priority -ne "Unknown") { "Green" } else { "Red" })
        Write-Host "  StatusMasterId: $($complaintData.statusId)" -ForegroundColor Gray
        Write-Host "  PriorityMasterId: $($complaintData.priorityId)" -ForegroundColor Gray

        # Validate
        $statusValid = $complaintData.status -and $complaintData.status -ne "Unknown" -and $complaintData.status -ne ""
        $priorityValid = $complaintData.priority -and $complaintData.priority -ne "Unknown" -and $complaintData.priority -ne ""

        if ($statusValid -and $priorityValid) {
            Write-Host "  Result: PASS" -ForegroundColor Green
            $passCount++
            $results += @{
                ComplaintNumber = $complaint.Number
                Status = "PASS"
                StatusValue = $complaintData.status
                PriorityValue = $complaintData.priority
            }
        } else {
            Write-Host "  Result: FAIL" -ForegroundColor Red
            if (-not $statusValid) {
                Write-Host "    - Status is invalid: '$($complaintData.status)'" -ForegroundColor Red
            }
            if (-not $priorityValid) {
                Write-Host "    - Priority is invalid: '$($complaintData.priority)'" -ForegroundColor Red
            }
            $failCount++
            $results += @{
                ComplaintNumber = $complaint.Number
                Status = "FAIL"
                StatusValue = $complaintData.status
                PriorityValue = $complaintData.priority
            }
        }

    } catch {
        Write-Host "  Result: ERROR" -ForegroundColor Red
        Write-Host "  Error: $($_.Exception.Message)" -ForegroundColor Red
        $failCount++
        $results += @{
            ComplaintNumber = $complaint.Number
            Status = "ERROR"
            StatusValue = "N/A"
            PriorityValue = "N/A"
        }
    }

    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "FINAL VERIFICATION RESULTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total Tests: $($passCount + $failCount)" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host ""

if ($failCount -eq 0) {
    Write-Host "VERDICT: PASS - Backend fix is working correctly!" -ForegroundColor Green
    Write-Host "The .Include() fix successfully populates Status and Priority fields." -ForegroundColor Green
} else {
    Write-Host "VERDICT: FAIL - Backend fix has issues" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "DETAILED RESULTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$results | ForEach-Object {
    Write-Host "$($_.ComplaintNumber): $($_.Status)" -ForegroundColor $(if ($_.Status -eq "PASS") { "Green" } else { "Red" })
    Write-Host "  Status: $($_.StatusValue)" -ForegroundColor White
    Write-Host "  Priority: $($_.PriorityValue)" -ForegroundColor White
}

Write-Host "`n"
