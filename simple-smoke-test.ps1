# Simple Smoke Test for Complaint Management System APIs
# Tests all 6 refactored master management components

$ApiUrl = "http://localhost:5058"
$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MDk5MTc3MSwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.LkPFNyCDL3eJe39wwUOq8lwgQL72AO4YXmrWXJgQVD8"

$headers = @{
    "Authorization" = "Bearer $Token"
    "Content-Type" = "application/json"
}

$passCount = 0
$failCount = 0

Write-Host "`n===== SMOKE TEST STARTED =====" -ForegroundColor Cyan
Write-Host "Testing API: $ApiUrl`n"

function Test-Endpoint {
    param([string]$Name, [string]$Url)

    try {
        $response = Invoke-RestMethod -Uri $Url -Headers $headers -Method Get -ErrorAction Stop
        if ($response.isSuccess) {
            Write-Host "[PASS] $Name" -ForegroundColor Green
            $script:passCount++
            return $response
        } else {
            Write-Host "[FAIL] $Name - $($response.message)" -ForegroundColor Red
            $script:failCount++
        }
    } catch {
        Write-Host "[FAIL] $Name - $($_.Exception.Message)" -ForegroundColor Red
        $script:failCount++
    }
    return $null
}

# Test 1: Status Master
Test-Endpoint "Status Master API" "$ApiUrl/api/ComplaintStatusMaster?includeSystem=true"

# Test 2: Priority Master
Test-Endpoint "Priority Master API" "$ApiUrl/api/ComplaintPriorityMaster?includeSystem=true"

# Test 3: Category
Test-Endpoint "Category API" "$ApiUrl/api/categories"

# Test 4: Branch
$companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"
$branchResponse = Test-Endpoint "Branch API" "$ApiUrl/api/branches?companyId=$companyId"

# Test 5: Department
if ($branchResponse -and $branchResponse.data.Count -gt 0) {
    $branchId = $branchResponse.data[0].id
    $deptResponse = Test-Endpoint "Department API" "$ApiUrl/api/departments?branchId=$branchId"

    # Test 6: Section
    if ($deptResponse -and $deptResponse.data.Count -gt 0) {
        $deptId = $deptResponse.data[0].id
        Test-Endpoint "Section API" "$ApiUrl/api/sections?departmentId=$deptId"
    }
}

Write-Host "`n===== TEST SUMMARY =====" -ForegroundColor Cyan
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red

if ($failCount -eq 0) {
    Write-Host "`nAll tests PASSED!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "`nSome tests FAILED!" -ForegroundColor Red
    exit 1
}
