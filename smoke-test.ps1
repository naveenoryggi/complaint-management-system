# Complaint Management System - Automated Smoke Test
# This script tests all 6 refactored master management components

param(
    [string]$ApiUrl = "http://localhost:5058",
    [string]$Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MDk5MTc3MSwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.LkPFNyCDL3eJe39wwUOq8lwgQL72AO4YXmrWXJgQVD8"
)

$ErrorActionPreference = "Continue"
$ProgressPreference = "SilentlyContinue"

# Test results
$script:TotalTests = 0
$script:PassedTests = 0
$script:FailedTests = 0
$script:TestResults = @()

# Colors
$successColor = "Green"
$errorColor = "Red"
$warningColor = "Yellow"
$infoColor = "Cyan"

function Write-TestHeader {
    param([string]$Title)
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $infoColor
    Write-Host " $Title" -ForegroundColor $infoColor
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $infoColor
}

function Write-TestResult {
    param(
        [string]$TestName,
        [bool]$Passed,
        [string]$Message = "",
        [object]$Data = $null
    )

    $script:TotalTests++

    if ($Passed) {
        $script:PassedTests++
        Write-Host "  ✓ " -ForegroundColor $successColor -NoNewline
        Write-Host $TestName -ForegroundColor White
        if ($Message) {
            Write-Host "    └─ $Message" -ForegroundColor Gray
        }
    } else {
        $script:FailedTests++
        Write-Host "  ✗ " -ForegroundColor $errorColor -NoNewline
        Write-Host $TestName -ForegroundColor White
        if ($Message) {
            Write-Host "    └─ ERROR: $Message" -ForegroundColor $errorColor
        }
    }

    $script:TestResults += @{
        Test = $TestName
        Passed = $Passed
        Message = $Message
        Data = $Data
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
}

function Test-ApiEndpoint {
    param(
        [string]$Name,
        [string]$Endpoint,
        [int]$ExpectedRecordCount = -1
    )

    try {
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Content-Type" = "application/json"
        }

        $response = Invoke-RestMethod -Uri "$ApiUrl$Endpoint" -Method Get -Headers $headers -ErrorAction Stop

        $recordCount = 0
        if ($response.data) {
            if ($response.data -is [Array]) {
                $recordCount = $response.data.Count
            } else {
                $recordCount = 1
            }
        }

        if ($response.isSuccess -eq $true) {
            $message = "Retrieved $recordCount record(s)"
            if ($ExpectedRecordCount -ge 0 -and $recordCount -ne $ExpectedRecordCount) {
                Write-TestResult -TestName $Name -Passed $false -Message "Expected $ExpectedRecordCount records but got $recordCount"
            } else {
                Write-TestResult -TestName $Name -Passed $true -Message $message -Data $response
            }
        } else {
            Write-TestResult -TestName $Name -Passed $false -Message "API returned isSuccess=false: $($response.message)"
        }
    }
    catch {
        Write-TestResult -TestName $Name -Passed $false -Message $_.Exception.Message
    }
}

function Test-ApplicationAccessibility {
    Write-TestHeader "Application Accessibility Tests"

    try {
        # Test Angular app
        $angularResponse = Invoke-WebRequest -Uri "http://localhost:4200" -Method Get -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
        Write-TestResult -TestName "Angular App (http://localhost:4200)" -Passed ($angularResponse.StatusCode -eq 200) -Message "HTTP $($angularResponse.StatusCode)"
    }
    catch {
        Write-TestResult -TestName "Angular App (http://localhost:4200)" -Passed $false -Message $_.Exception.Message
    }

    try {
        # Test .NET API
        $apiResponse = Invoke-WebRequest -Uri "$ApiUrl/health" -Method Get -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
        if ($apiResponse.StatusCode -eq 200) {
            Write-TestResult -TestName ".NET API ($ApiUrl)" -Passed $true -Message "HTTP $($apiResponse.StatusCode)"
        } else {
            # If /health doesn't exist, try root
            $apiResponse = Invoke-WebRequest -Uri $ApiUrl -Method Get -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
            Write-TestResult -TestName ".NET API ($ApiUrl)" -Passed $true -Message "HTTP $($apiResponse.StatusCode)"
        }
    }
    catch {
        Write-TestResult -TestName ".NET API ($ApiUrl)" -Passed $false -Message $_.Exception.Message
    }
}

function Test-StatusMasterEndpoints {
    Write-TestHeader "Status Master Management API Tests"

    Test-ApiEndpoint -Name "GET Status Masters (with system)" -Endpoint "/api/ComplaintStatusMaster?includeSystem=true" -ExpectedRecordCount 9
    Test-ApiEndpoint -Name "GET Status Masters (without system)" -Endpoint "/api/ComplaintStatusMaster?includeSystem=false"
}

function Test-PriorityMasterEndpoints {
    Write-TestHeader "Priority Master Management API Tests"

    Test-ApiEndpoint -Name "GET Priority Masters (with system)" -Endpoint "/api/ComplaintPriorityMaster?includeSystem=true" -ExpectedRecordCount 5
    Test-ApiEndpoint -Name "GET Priority Masters (without system)" -Endpoint "/api/ComplaintPriorityMaster?includeSystem=false"
}

function Test-CategoryEndpoints {
    Write-TestHeader "Category Management API Tests"

    Test-ApiEndpoint -Name "GET Categories" -Endpoint "/api/categories" -ExpectedRecordCount 11
}

function Test-BranchEndpoints {
    Write-TestHeader "Branch Management API Tests"

    $companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"
    Test-ApiEndpoint -Name "GET Branches" -Endpoint "/api/branches?companyId=$companyId"
}

function Test-DepartmentEndpoints {
    Write-TestHeader "Department Management API Tests"

    # First get branches to get a branchId
    try {
        $companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Content-Type" = "application/json"
        }

        $branchResponse = Invoke-RestMethod -Uri "$ApiUrl/api/branches?companyId=$companyId" -Method Get -Headers $headers -ErrorAction Stop

        if ($branchResponse.isSuccess -and $branchResponse.data -and $branchResponse.data.Count -gt 0) {
            $branchId = $branchResponse.data[0].id
            Test-ApiEndpoint -Name "GET Departments for branch" -Endpoint "/api/departments?branchId=$branchId"
        } else {
            Write-TestResult -TestName "GET Departments" -Passed $false -Message "No branches available for testing departments"
        }
    }
    catch {
        Write-TestResult -TestName "GET Departments" -Passed $false -Message $_.Exception.Message
    }
}

function Test-SectionEndpoints {
    Write-TestHeader "Section Management API Tests"

    # First get departments to get a departmentId
    try {
        $companyId = "fe28cd85-4226-4daa-9e45-66a3d51877fa"
        $headers = @{
            "Authorization" = "Bearer $Token"
            "Content-Type" = "application/json"
        }

        # Get branches first
        $branchResponse = Invoke-RestMethod -Uri "$ApiUrl/api/branches?companyId=$companyId" -Method Get -Headers $headers -ErrorAction Stop

        if ($branchResponse.isSuccess -and $branchResponse.data -and $branchResponse.data.Count -gt 0) {
            $branchId = $branchResponse.data[0].id

            # Get departments for the branch
            $deptResponse = Invoke-RestMethod -Uri "$ApiUrl/api/departments?branchId=$branchId" -Method Get -Headers $headers -ErrorAction Stop

            if ($deptResponse.isSuccess -and $deptResponse.data -and $deptResponse.data.Count -gt 0) {
                $departmentId = $deptResponse.data[0].id
                Test-ApiEndpoint -Name "GET Sections for department" -Endpoint "/api/sections?departmentId=$departmentId"
            } else {
                Write-TestResult -TestName "GET Sections" -Passed $false -Message "No departments available for testing sections"
            }
        } else {
            Write-TestResult -TestName "GET Sections" -Passed $false -Message "No branches available for testing sections"
        }
    }
    catch {
        Write-TestResult -TestName "GET Sections" -Passed $false -Message $_.Exception.Message
    }
}

function Show-TestSummary {
    Write-Host ""
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $infoColor
    Write-Host " TEST SUMMARY" -ForegroundColor $infoColor
    Write-Host "═══════════════════════════════════════════════════════" -ForegroundColor $infoColor
    Write-Host ""
    Write-Host "  Total Tests:   $script:TotalTests"
    Write-Host "  Passed:        " -NoNewline
    Write-Host "$script:PassedTests" -ForegroundColor $successColor
    Write-Host "  Failed:        " -NoNewline
    Write-Host "$script:FailedTests" -ForegroundColor $errorColor

    if ($script:TotalTests -gt 0) {
        $passRate = [math]::Round(($script:PassedTests / $script:TotalTests) * 100, 2)
        Write-Host "  Pass Rate:     " -NoNewline
        if ($passRate -eq 100) {
            Write-Host "$passRate%" -ForegroundColor $successColor
        } elseif ($passRate -ge 80) {
            Write-Host "$passRate%" -ForegroundColor $warningColor
        } else {
            Write-Host "$passRate%" -ForegroundColor $errorColor
        }
    }

    Write-Host ""

    if ($script:FailedTests -gt 0) {
        Write-Host "Failed Tests:" -ForegroundColor $errorColor
        foreach ($result in $script:TestResults) {
            if (-not $result.Passed) {
                Write-Host "  • $($result.Test)" -ForegroundColor $errorColor
                if ($result.Message) {
                    Write-Host "    $($result.Message)" -ForegroundColor Gray
                }
            }
        }
        Write-Host ""
    }

    # Exit code
    if ($script:FailedTests -eq 0) {
        Write-Host "✓ All tests passed!" -ForegroundColor $successColor
        exit 0
    } else {
        Write-Host "✗ Some tests failed" -ForegroundColor $errorColor
        exit 1
    }
}

# Main execution
Write-Host ""
Write-Host "╔═══════════════════════════════════════════════════════╗" -ForegroundColor $infoColor
Write-Host "║   COMPLAINT MANAGEMENT SYSTEM - SMOKE TEST            ║" -ForegroundColor $infoColor
Write-Host "║   Base Class Refactored Components Verification      ║" -ForegroundColor $infoColor
Write-Host "╚═══════════════════════════════════════════════════════╝" -ForegroundColor $infoColor
Write-Host ""
Write-Host "Starting smoke tests..." -ForegroundColor $infoColor
Write-Host "API URL: $ApiUrl" -ForegroundColor Gray
$timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
Write-Host "Timestamp: $timestamp" -ForegroundColor Gray

# Run all test suites
Test-ApplicationAccessibility
Test-StatusMasterEndpoints
Test-PriorityMasterEndpoints
Test-CategoryEndpoints
Test-BranchEndpoints
Test-DepartmentEndpoints
Test-SectionEndpoints

# Show summary
Show-TestSummary
