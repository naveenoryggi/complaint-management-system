# Automated Testing Script for Complaint Management System
# This script will create all test data and execute comprehensive tests

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5058/api"
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTI1MzE0MywiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.yxRgHOSsIynW1ozrkWx0IqutXQyerQ-hNJlbbj_K9mw"
$companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Storage for created IDs
$branchIds = @()
$departmentIds = @()
$sectionIds = @()
$testResults = @()

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "AUTOMATED TESTING STARTED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Start Time: $(Get-Date)" -ForegroundColor Yellow

# Function to make API calls with error handling
function Invoke-APICall {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [string]$Description
    )

    $startTime = Get-Date
    try {
        $params = @{
            Method = $Method
            Uri = "$baseUrl/$Endpoint"
            Headers = $headers
            TimeoutSec = 30
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-RestMethod @params
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalMilliseconds

        $result = @{
            Test = $Description
            Method = $Method
            Endpoint = $Endpoint
            Status = if ($response.isSuccess) { "PASS" } else { "FAIL" }
            Duration = "$([math]::Round($duration, 2))ms"
            Message = $response.message
            Data = $response.data
        }

        $script:testResults += $result
        Write-Host "  ✓ $Description - $($result.Status) ($($result.Duration))" -ForegroundColor Green
        return $response
    }
    catch {
        $endTime = Get-Date
        $duration = ($endTime - $startTime).TotalMilliseconds

        $result = @{
            Test = $Description
            Method = $Method
            Endpoint = $Endpoint
            Status = "ERROR"
            Duration = "$([math]::Round($duration, 2))ms"
            Message = $_.Exception.Message
            Data = $null
        }

        $script:testResults += $result
        Write-Host "  ✗ $Description - ERROR: $($_.Exception.Message)" -ForegroundColor Red
        return $null
    }
}

# Phase 1: Get existing branches
Write-Host "`n[PHASE 1] Getting Existing Branches..." -ForegroundColor Cyan
$branchesResponse = Invoke-APICall -Method "GET" -Endpoint "branches" -Description "List all branches"
if ($branchesResponse -and $branchesResponse.data) {
    $branchIds = $branchesResponse.data | ForEach-Object { $_.id }
    Write-Host "  Found $($branchIds.Count) branches" -ForegroundColor Yellow
}

# Phase 2: Create Departments (3-4 per branch)
Write-Host "`n[PHASE 2] Creating Departments..." -ForegroundColor Cyan
$departmentNames = @("Customer Service", "Technical Support", "Sales", "Operations")
$deptCount = 0

foreach ($branchId in $branchIds) {
    foreach ($deptName in $departmentNames) {
        $deptCount++
        $deptCode = ($deptName -replace " ", "_").ToUpper() + "_" + $deptCount
        $dept = @{
            companyId = $companyId
            branchId = $branchId
            name = $deptName
            code = $deptCode
            description = "$deptName department operations"
            isActive = $true
        }

        $response = Invoke-APICall -Method "POST" -Endpoint "departments" -Body $dept -Description "Create $deptName for branch"
        if ($response -and $response.data) {
            $departmentIds += $response.data.id
        }
        Start-Sleep -Milliseconds 100
    }
}

Write-Host "  Created $($departmentIds.Count) departments" -ForegroundColor Yellow

# Phase 3: Create Sections (2-3 per department)
Write-Host "`n[PHASE 3] Creating Sections..." -ForegroundColor Cyan
$sectionNames = @("Level 1 Support", "Level 2 Support", "Escalation Team")
$sectionCount = 0

foreach ($deptId in $departmentIds) {
    foreach ($sectionName in $sectionNames) {
        $sectionCount++
        $sectionCode = ($sectionName -replace " ", "_").ToUpper() + "_" + $sectionCount
        $section = @{
            departmentId = $deptId
            name = $sectionName
            code = $sectionCode
            description = "$sectionName operations"
            isActive = $true
        }

        $response = Invoke-APICall -Method "POST" -Endpoint "sections" -Body $section -Description "Create $sectionName for department"
        if ($response -and $response.data) {
            $sectionIds += $response.data.id
        }
        Start-Sleep -Milliseconds 100
    }
}

Write-Host "  Created $($sectionIds.Count) sections" -ForegroundColor Yellow

# Export test results
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST RESULTS SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$passCount = ($testResults | Where-Object { $_.Status -eq "PASS" }).Count
$failCount = ($testResults | Where-Object { $_.Status -eq "FAIL" }).Count
$errorCount = ($testResults | Where-Object { $_.Status -eq "ERROR" }).Count

Write-Host "Total Tests: $($testResults.Count)" -ForegroundColor White
Write-Host "Passed: $passCount" -ForegroundColor Green
Write-Host "Failed: $failCount" -ForegroundColor Red
Write-Host "Errors: $errorCount" -ForegroundColor Red

# Calculate average response time
$avgResponseTime = ($testResults | Measure-Object -Property { [double]($_.Duration -replace 'ms', '') } -Average).Average
Write-Host "Average Response Time: $([math]::Round($avgResponseTime, 2))ms" -ForegroundColor Yellow

Write-Host "`nEnd Time: $(Get-Date)" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

# Export detailed results to JSON
$resultsJson = $testResults | ConvertTo-Json -Depth 10
$resultsJson | Out-File "test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"

Write-Host "`nTest results exported to test-results-*.json" -ForegroundColor Green
