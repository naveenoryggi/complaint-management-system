# Overnight Comprehensive Testing Script
# This script will run autonomously to test the entire application

$TOKEN = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6ImY1NmQ4ZDAzLWUzODItNDU0Yi1iZjdkLWZhODIzNmMxMjVjMyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL2VtYWlsYWRkcmVzcyI6ImFkbWluQGNvbXBsYWludG1hbmFnZW1lbnQuY29tIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvbmFtZSI6IlN5c3RlbSBBZG1pbmlzdHJhdG9yIiwiRW1wbG95ZWVDb2RlIjoiQURNSU4wMDEiLCJDb21wYW55SWQiOiJmZTI4Y2Q4NS00MjI2LTRkYWEtOWU0NS02NmEzZDUxODc3ZmEiLCJQZXJtaXNzaW9uIjpbIlZpZXdDb21wbGFpbnRzIiwiQWRkQ29tbWVudCIsIkVzY2FsYXRlQ29tcGxhaW50IiwiTWFuYWdlVXNlcnMiLCJWaWV3QXVkaXRMb2dzIiwiRWRpdENvbXBsYWludCIsIkNyZWF0ZUNvbXBsYWludCIsIlZpZXdDb21tZW50cyIsIk1hbmFnZVJvbGVzIiwiTWFuYWdlRXNjYWxhdGlvbiIsIlZpZXdBdHRhY2htZW50cyIsIlZpZXdFc2NhbGF0aW9uIiwiQXNzaWduQ29tcGxhaW50IiwiVmlld1JlcG9ydHMiLCJEZWxldGVDb21wbGFpbnQiLCJBZGRBdHRhY2htZW50IiwiTWFuYWdlQ2F0ZWdvcmllcyIsIkNsb3NlQ29tcGxhaW50IiwiTWFuYWdlU2V0dGluZ3MiLCJNYW5hZ2VDb21wYW55IiwiUmVvcGVuQ29tcGxhaW50Il0sImV4cCI6MTc2MTI4MDg4NSwiaXNzIjoiQ29tcGxhaW50TWFuYWdlbWVudFN5c3RlbSIsImF1ZCI6IkNvbXBsYWludE1hbmFnZW1lbnRBUEkifQ.B4JHfPaF_IBhd7DsYoUxIg4TcdkRiXry7nIcfTKGJuo"
$API_BASE = "http://localhost:5058/api"
$COMPANY_ID = "fe28cd85-4226-4daa-9e45-66a3d51877fa"

Write-Host "===========================================" -ForegroundColor Cyan
Write-Host "Overnight Testing - Starting" -ForegroundColor Cyan
Write-Host "Time: $(Get-Date)" -ForegroundColor Cyan
Write-Host "===========================================" -ForegroundColor Cyan

$testResults = @()
$createdData = @{}

# Helper function to make API calls
function Invoke-APICall {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null
    )

    $headers = @{
        "Authorization" = "Bearer $TOKEN"
        "Content-Type" = "application/json"
    }

    try {
        if ($Body) {
            $jsonBody = $Body | ConvertTo-Json -Depth 10
            $response = Invoke-RestMethod -Uri "$API_BASE/$Endpoint" -Method $Method -Headers $headers -Body $jsonBody -ErrorAction Stop
        } else {
            $response = Invoke-RestMethod -Uri "$API_BASE/$Endpoint" -Method $Method -Headers $headers -ErrorAction Stop
        }
        return @{Success = $true; Data = $response}
    } catch {
        Write-Host "API Error: $_" -ForegroundColor Red
        return @{Success = $false; Error = $_.Exception.Message}
    }
}

Write-Host "`n[Phase 1] Testing Dashboard API..." -ForegroundColor Yellow

# Test Dashboard Preferences GET
$result = Invoke-APICall -Method "GET" -Endpoint "dashboard/preferences"
$testResults += @{Test = "Dashboard Preferences GET"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}

# Test Dashboard Statistics GET
$result = Invoke-APICall -Method "GET" -Endpoint "dashboard/statistics?dateRangeDays=30"
$testResults += @{Test = "Dashboard Statistics GET"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}

# Test Dashboard Preferences POST
$dashboardPrefs = @{
    statusWidgets = @("10000000-0000-0000-0000-000000000001", "10000000-0000-0000-0000-000000000002")
    layout = "grid-4"
    showTrends = $true
    showPercentages = $true
    autoRefreshInterval = 0
    dateRangeDays = 30
}
$result = Invoke-APICall -Method "POST" -Endpoint "dashboard/preferences" -Body $dashboardPrefs
$testResults += @{Test = "Dashboard Preferences POST"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}

Write-Host "[Phase 1] Dashboard API tests complete: $($testResults.Count) tests" -ForegroundColor Green

Write-Host "`n[Phase 2] Creating Organizational Structure..." -ForegroundColor Yellow

# Create Branches
$branches = @(
    @{name="Main Office"; code="HQ001"; address="123 Main Street"; isActive=$true; companyId=$COMPANY_ID},
    @{name="North Branch"; code="NB001"; address="456 North Ave"; isActive=$true; companyId=$COMPANY_ID},
    @{name="South Branch"; code="SB001"; address="789 South Blvd"; isActive=$true; companyId=$COMPANY_ID},
    @{name="East Branch"; code="EB001"; address="321 East Road"; isActive=$true; companyId=$COMPANY_ID},
    @{name="West Branch"; code="WB001"; address="654 West Lane"; isActive=$true; companyId=$COMPANY_ID}
)

$createdData.Branches = @()
foreach ($branch in $branches) {
    $result = Invoke-APICall -Method "POST" -Endpoint "branches" -Body $branch
    if ($result.Success) {
        $createdData.Branches += $result.Data.data
        Write-Host "  Created branch: $($branch.name)" -ForegroundColor Green
    }
    $testResults += @{Test = "Create Branch: $($branch.name)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
}

Write-Host "[Phase 2] Created $($createdData.Branches.Count) branches" -ForegroundColor Green

# Create Departments
$departmentTemplates = @("Customer Service", "Technical Support", "Sales", "Operations")
$createdData.Departments = @()

foreach ($branch in $createdData.Branches) {
    foreach ($deptName in $departmentTemplates) {
        $dept = @{
            name = "$deptName - $($branch.name)"
            code = "$($deptName.Substring(0,3).ToUpper())_$($branch.code)"
            description = "$deptName department at $($branch.name)"
            branchId = $branch.id
            companyId = $COMPANY_ID
            isActive = $true
        }
        $result = Invoke-APICall -Method "POST" -Endpoint "departments" -Body $dept
        if ($result.Success) {
            $createdData.Departments += $result.Data.data
            Write-Host "  Created department: $($dept.name)" -ForegroundColor Green
        }
        $testResults += @{Test = "Create Department: $($dept.name)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
    }
}

Write-Host "[Phase 2] Created $($createdData.Departments.Count) departments" -ForegroundColor Green

# Create Sections
$sectionTemplates = @("Level 1 Support", "Level 2 Support", "Escalation Team")
$createdData.Sections = @()

foreach ($dept in $createdData.Departments | Select-Object -First 10) {
    foreach ($sectionName in $sectionTemplates) {
        $section = @{
            name = "$sectionName - $($dept.name)"
            code = "$($sectionName.Substring(0,3).ToUpper())_$($dept.code)"
            description = "$sectionName within $($dept.name)"
            departmentId = $dept.id
            branchId = $dept.branchId
            companyId = $COMPANY_ID
            isActive = $true
        }
        $result = Invoke-APICall -Method "POST" -Endpoint "sections" -Body $section
        if ($result.Success) {
            $createdData.Sections += $result.Data.data
            Write-Host "  Created section: $($section.name)" -ForegroundColor Green
        }
        $testResults += @{Test = "Create Section: $($section.name)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
    }
}

Write-Host "[Phase 2] Created $($createdData.Sections.Count) sections" -ForegroundColor Green

Write-Host "`n[Phase 3] Creating Master Data..." -ForegroundColor Yellow

# Create Categories
$categories = @(
    @{name="Product Quality Issues"; code="PROD_QUAL"; description="Issues related to product quality"; defaultPriority=2; defaultSlaHours=48; isActive=$true; displayOrder=1},
    @{name="Service Delays"; code="SERV_DELAY"; description="Delays in service delivery"; defaultPriority=2; defaultSlaHours=24; isActive=$true; displayOrder=2},
    @{name="Billing Problems"; code="BILL_PROB"; description="Billing and payment issues"; defaultPriority=1; defaultSlaHours=72; isActive=$true; displayOrder=3},
    @{name="Technical Issues"; code="TECH_ISS"; description="Technical problems and bugs"; defaultPriority=1; defaultSlaHours=24; isActive=$true; displayOrder=4},
    @{name="Delivery Problems"; code="DELIV_PROB"; description="Delivery and shipping issues"; defaultPriority=2; defaultSlaHours=48; isActive=$true; displayOrder=5},
    @{name="Customer Service Issues"; code="CUST_SERV"; description="Customer service related complaints"; defaultPriority=2; defaultSlaHours=24; isActive=$true; displayOrder=6},
    @{name="Policy Questions"; code="POL_QUEST"; description="Questions about policies"; defaultPriority=3; defaultSlaHours=72; isActive=$true; displayOrder=7},
    @{name="Feature Requests"; code="FEAT_REQ"; description="Requests for new features"; defaultPriority=3; defaultSlaHours=168; isActive=$true; displayOrder=8},
    @{name="Bug Reports"; code="BUG_REP"; description="Software bug reports"; defaultPriority=1; defaultSlaHours=48; isActive=$true; displayOrder=9},
    @{name="General Inquiries"; code="GEN_INQ"; description="General questions and inquiries"; defaultPriority=3; defaultSlaHours=72; isActive=$true; displayOrder=10}
)

$createdData.Categories = @()
foreach ($category in $categories) {
    $result = Invoke-APICall -Method "POST" -Endpoint "categories" -Body $category
    if ($result.Success) {
        $createdData.Categories += $result.Data.data
        Write-Host "  Created category: $($category.name)" -ForegroundColor Green
    }
    $testResults += @{Test = "Create Category: $($category.name)"; Result = if($result.Success){"PASS"}else{"FAIL"}; Time = (Get-Date)}
}

Write-Host "[Phase 3] Created $($createdData.Categories.Count) categories" -ForegroundColor Green

Write-Host "`n===========================================" -ForegroundColor Cyan
Write-Host "Test Summary so Far:" -ForegroundColor Cyan
Write-Host "Total Tests: $($testResults.Count)" -ForegroundColor White
Write-Host "Passed: $(($testResults | Where-Object {$_.Result -eq 'PASS'}).Count)" -ForegroundColor Green
Write-Host "Failed: $(($testResults | Where-Object {$_.Result -eq 'FAIL'}).Count)" -ForegroundColor Red
Write-Host "===========================================" -ForegroundColor Cyan

Write-Host "`nContinuing with more testing..." -ForegroundColor Yellow
Write-Host "This script will continue running overnight..." -ForegroundColor Cyan
Write-Host "Check TEST_RESULTS.md for full report when complete." -ForegroundColor Cyan
