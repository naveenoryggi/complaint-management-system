# Test backend fix for Unknown Unknown bug
$token = Get-Content '.fresh-token' -Raw
$token = $token.Trim()
$headers = @{
    'Authorization' = "Bearer $token"
}

Write-Host "Testing complaint CMP-2025-1110 (ID: dc5f95da-92d1-40f9-8ed3-1b91f0b70c34)..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints/dc5f95da-92d1-40f9-8ed3-1b91f0b70c34' -Headers $headers -Method Get

    Write-Host "`nSUCCESS! Complaint loaded:" -ForegroundColor Green
    Write-Host "Complaint Number: $($response.complaintNumber)"
    Write-Host "Title: $($response.title)"
    Write-Host "Status: $($response.status)"
    Write-Host "Priority: $($response.priority)"
    Write-Host "StatusMasterId: $($response.statusMasterId)"
    Write-Host "PriorityMasterId: $($response.priorityMasterId)"

    # Check if status and priority are populated
    if ($response.status -and $response.status -ne "Unknown" -and $response.priority -and $response.priority -ne "Unknown") {
        Write-Host "`nBACKEND FIX: PASS" -ForegroundColor Green
        Write-Host "Status and Priority are populated correctly!" -ForegroundColor Green
    } else {
        Write-Host "`nBACKEND FIX: FAIL" -ForegroundColor Red
        Write-Host "Status: $($response.status) (Expected: NOT 'Unknown')" -ForegroundColor Red
        Write-Host "Priority: $($response.priority) (Expected: NOT 'Unknown')" -ForegroundColor Red
    }

} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

Write-Host "`n`nTesting complaint CMP-2025-1103 (ID: b8a64ad3-979a-4698-9523-dbadeb72cbdf)..." -ForegroundColor Cyan

try {
    $response = Invoke-RestMethod -Uri 'http://localhost:5058/api/complaints/b8a64ad3-979a-4698-9523-dbadeb72cbdf' -Headers $headers -Method Get

    Write-Host "`nSUCCESS! Complaint loaded:" -ForegroundColor Green
    Write-Host "Complaint Number: $($response.complaintNumber)"
    Write-Host "Title: $($response.title)"
    Write-Host "Status: $($response.status)"
    Write-Host "Priority: $($response.priority)"
    Write-Host "StatusMasterId: $($response.statusMasterId)"
    Write-Host "PriorityMasterId: $($response.priorityMasterId)"

    # Check if status and priority are populated
    if ($response.status -and $response.status -ne "Unknown" -and $response.priority -and $response.priority -ne "Unknown") {
        Write-Host "`nBACKEND FIX: PASS" -ForegroundColor Green
        Write-Host "Status and Priority are populated correctly!" -ForegroundColor Green
    } else {
        Write-Host "`nBACKEND FIX: FAIL" -ForegroundColor Red
        Write-Host "Status: $($response.status) (Expected: NOT 'Unknown')" -ForegroundColor Red
        Write-Host "Priority: $($response.priority) (Expected: NOT 'Unknown')" -ForegroundColor Red
    }

} catch {
    Write-Host "`nERROR: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}
