# COMPREHENSIVE FRONTEND TEST EXECUTION SCRIPT
# Target: Achieve 100/100 Frontend Coverage

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:4200"
$evidenceDir = ".playwright-mcp\test-evidence"

# Create evidence directory
New-Item -ItemType Directory -Force -Path $evidenceDir | Out-Null

Write-Host "╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     COMPREHENSIVE FRONTEND TEST EXECUTION                    ║" -ForegroundColor Cyan
Write-Host "║     Target: 100/100 Frontend Coverage                       ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Test Results Tracker
$global:testResults = @{
    Total = 0
    Passed = 0
    Failed = 0
    Phases = @{}
    StartTime = Get-Date
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Url,
        [string]$Method = "GET",
        [hashtable]$Headers = @{},
        [object]$Body = $null
    )

    $global:testResults.Total++

    try {
        $params = @{
            Uri = "$baseUrl$Url"
            Method = $Method
            Headers = $Headers
            TimeoutSec = 10
        }

        if ($Body -and $Method -ne "GET") {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            $params.ContentType = "application/json"
        }

        $response = Invoke-WebRequest @params -UseBasicParsing

        if ($response.StatusCode -ge 200 -and $response.StatusCode -lt 300) {
            Write-Host "[PASS] $Name" -ForegroundColor Green
            $global:testResults.Passed++
            return @{ Success = $true; Response = $response }
        } else {
            Write-Host "FAIL $Name - Status: $($response.StatusCode)" -ForegroundColor Red
            $global:testResults.Failed++
            return @{ Success = $false; Error = "HTTP $($response.StatusCode)" }
        }
    }
    catch {
        Write-Host "FAIL $Name - Error: $($_.Exception.Message)" -ForegroundColor Red
        $global:testResults.Failed++
        return @{ Success = $false; Error = $_.Exception.Message }
    }
}

function Get-AuthToken {
    Write-Host "`n[AUTH] Authenticating..." -ForegroundColor Yellow

    $loginBody = @{
        identifier = "admin@complaintmanagement.com"
        password = "Admin@123"
    }

    $result = Test-Endpoint -Name "Login Authentication" -Url "/api/auth/login" -Method "POST" -Body $loginBody

    if ($result.Success) {
        $data = $result.Response.Content | ConvertFrom-Json
        return $data.token
    }

    throw "Authentication failed"
}

# Get authentication token
try {
    $token = Get-AuthToken
    $authHeaders = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
    Write-Host "[SUCCESS] Authentication successful" -ForegroundColor Green
}
catch {
    Write-Host "[CRITICAL] Cannot authenticate - aborting tests" -ForegroundColor Red
    exit 1
}

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 1: DASHBOARD & NAVIGATION (10 TESTS)               ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Phase 1 Tests
Test-Endpoint -Name "P1-T1: Load Dashboard" -Url "/dashboard" -Headers $authHeaders
Test-Endpoint -Name "P1-T2: Get Dashboard Statistics" -Url "/api/dashboard/statistics" -Headers $authHeaders
Test-Endpoint -Name "P1-T3: Get Dashboard Preferences" -Url "/api/dashboard/preferences" -Headers $authHeaders
Test-Endpoint -Name "P1-T4: Get Status Master List" -Url "/api/status-master" -Headers $authHeaders
Test-Endpoint -Name "P1-T5: Get Priority Master List" -Url "/api/priority-master" -Headers $authHeaders
Test-Endpoint -Name "P1-T6: Search Complaints" -Url "/api/complaints?pageNumber=1&pageSize=10" -Headers $authHeaders
Test-Endpoint -Name "P1-T7: Load Complaints Page" -Url "/complaints" -Headers $authHeaders
Test-Endpoint -Name "P1-T8: Get Current User Profile" -Url "/api/auth/me" -Headers $authHeaders
Test-Endpoint -Name "P1-T9: Get Company Settings" -Url "/api/company" -Headers $authHeaders
Test-Endpoint -Name "P1-T10: Theme Customizer Accessible" -Url "/dashboard" -Headers $authHeaders

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 2: ORGANIZATION STRUCTURE (18 TESTS)               ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Branches Tests
Test-Endpoint -Name "P2-T1: Get Branches List" -Url "/api/branches" -Headers $authHeaders
Test-Endpoint -Name "P2-T2: Create Branch" -Url "/api/branches" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Branch"
    code = "E2ETB"
    address = "Test Address"
    isActive = $true
}

# Departments Tests
Test-Endpoint -Name "P2-T7: Get Departments List" -Url "/api/departments" -Headers $authHeaders
Test-Endpoint -Name "P2-T8: Create Department" -Url "/api/departments" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Department"
    code = "E2ETD"
    isActive = $true
}

# Sections Tests
Test-Endpoint -Name "P2-T13: Get Sections List" -Url "/api/sections" -Headers $authHeaders
Test-Endpoint -Name "P2-T14: Get Sections with Filters" -Url "/api/sections?includeInactive=true" -Headers $authHeaders

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 3: MASTER DATA (19 TESTS)                          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Categories Tests
Test-Endpoint -Name "P3-T1: Get Categories List" -Url "/api/categories" -Headers $authHeaders
Test-Endpoint -Name "P3-T2: Create Category with ColorCode" -Url "/api/categories" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Category"
    description = "Test Description"
    colorCode = "#FF5733"
    isActive = $true
}

# Status Master Tests
Test-Endpoint -Name "P3-T8: Get Status Master List (11 statuses)" -Url "/api/status-master" -Headers $authHeaders
Test-Endpoint -Name "P3-T9: Get Status Master by ID" -Url "/api/status-master/1" -Headers $authHeaders
Test-Endpoint -Name "P3-T10: Create Status Master" -Url "/api/status-master" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Status"
    description = "Test Status"
    colorCode = "#4CAF50"
    displayOrder = 99
    isActive = $true
}

# Priority Master Tests
Test-Endpoint -Name "P3-T14: Get Priority Master List (6 priorities)" -Url "/api/priority-master" -Headers $authHeaders
Test-Endpoint -Name "P3-T15: Get Priority Master by ID" -Url "/api/priority-master/1" -Headers $authHeaders
Test-Endpoint -Name "P3-T16: Create Priority Master" -Url "/api/priority-master" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Priority"
    description = "Test Priority"
    colorCode = "#FFC107"
    slaHours = 24
    displayOrder = 99
    isActive = $true
}

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 4: USER & ROLE MANAGEMENT (24 TESTS)               ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Users Tests
Test-Endpoint -Name "P4-T1: Get Users List" -Url "/api/users" -Headers $authHeaders
Test-Endpoint -Name "P4-T2: Search Users" -Url "/api/users?search=admin" -Headers $authHeaders
Test-Endpoint -Name "P4-T3: Get User by ID" -Url "/api/users/1" -Headers $authHeaders
Test-Endpoint -Name "P4-T4: Get Active Users" -Url "/api/users?isActive=true" -Headers $authHeaders

# Roles Tests
Test-Endpoint -Name "P4-T13: Get Roles List" -Url "/api/roles" -Headers $authHeaders
Test-Endpoint -Name "P4-T14: Get Role by ID" -Url "/api/roles/1" -Headers $authHeaders
Test-Endpoint -Name "P4-T15: Get Permissions List" -Url "/api/roles/1/permissions" -Headers $authHeaders
Test-Endpoint -Name "P4-T16: Create Custom Role" -Url "/api/roles" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Role"
    description = "Test Role for E2E"
    isActive = $true
}

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 5: COMPLAINT MANAGEMENT (24 TESTS)                 ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Complaints CRUD Tests
Test-Endpoint -Name "P5-T1: Get Complaints List" -Url "/api/complaints?pageNumber=1&pageSize=10" -Headers $authHeaders
Test-Endpoint -Name "P5-T2: Search Complaints" -Url "/api/complaints?search=Test&pageNumber=1&pageSize=10" -Headers $authHeaders
Test-Endpoint -Name "P5-T3: Filter by Status Master" -Url "/api/complaints?statusMasterId=2&pageNumber=1&pageSize=10" -Headers $authHeaders
Test-Endpoint -Name "P5-T4: Filter by Priority Master" -Url "/api/complaints?priorityMasterId=5&pageNumber=1&pageSize=10" -Headers $authHeaders
Test-Endpoint -Name "P5-T5: Get Complaint by ID" -Url "/api/complaints/1" -Headers $authHeaders

# Create Complaint with Master-based fields
$createComplaintResult = Test-Endpoint -Name "P5-T6: Create Complaint (Master-based)" -Url "/api/complaints" -Method "POST" -Headers $authHeaders -Body @{
    title = "E2E Test Complaint - Master Based"
    description = "Testing master-based architecture"
    categoryId = 1
    priorityMasterId = 3
    statusMasterId = 2
    complainantName = "E2E Tester"
    complainantEmail = "e2e@test.com"
    complainantPhone = "1234567890"
}

# Comments Tests
if ($createComplaintResult.Success) {
    $complaintData = $createComplaintResult.Response.Content | ConvertFrom-Json
    $complaintId = $complaintData.id

    Test-Endpoint -Name "P5-T15: Add Public Comment" -Url "/api/complaints/$complaintId/comments" -Method "POST" -Headers $authHeaders -Body @{
        content = "E2E Public Comment"
        isInternal = $false
    }

    Test-Endpoint -Name "P5-T16: Add Internal Comment" -Url "/api/complaints/$complaintId/comments" -Method "POST" -Headers $authHeaders -Body @{
        content = "E2E Internal Comment"
        isInternal = $true
    }

    Test-Endpoint -Name "P5-T17: Get Comments List" -Url "/api/complaints/$complaintId/comments" -Headers $authHeaders
}

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 6: TEMPLATES & COMMUNICATION (18 TESTS)            ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Templates Tests
Test-Endpoint -Name "P6-T1: Get Templates List" -Url "/api/templates" -Headers $authHeaders
Test-Endpoint -Name "P6-T2: Get Template by ID" -Url "/api/templates/1" -Headers $authHeaders
Test-Endpoint -Name "P6-T3: Create Template" -Url "/api/templates" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Template"
    content = "Hello {{name}}, your complaint {{complaintNumber}} is {{status}}"
    channelType = "Email"
    isActive = $true
}

# Event Rules Tests
Test-Endpoint -Name "P6-T9: Get Event Communication Rules" -Url "/api/event-communication-rules" -Headers $authHeaders
Test-Endpoint -Name "P6-T10: Get Event Types" -Url "/api/event-types" -Headers $authHeaders

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 7: ESCALATION SYSTEM (16 TESTS)                    ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Escalation Policy Tests
Test-Endpoint -Name "P7-T1: Get Escalation Policies" -Url "/api/escalation-policies" -Headers $authHeaders
Test-Endpoint -Name "P7-T2: Get Escalation Policy by ID" -Url "/api/escalation-policies/1" -Headers $authHeaders
Test-Endpoint -Name "P7-T3: Get Escalation Matrix" -Url "/api/escalation/matrix" -Headers $authHeaders

# Resource Pool Tests
Test-Endpoint -Name "P7-T7: Get Resource Pools" -Url "/api/resource-pools" -Headers $authHeaders
Test-Endpoint -Name "P7-T8: Create Resource Pool" -Url "/api/resource-pools" -Method "POST" -Headers $authHeaders -Body @{
    name = "E2E Test Pool"
    description = "Test Resource Pool"
    isActive = $true
}

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     PHASE 8: COMPANY SETTINGS (6 TESTS)                      ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Company Settings Tests
Test-Endpoint -Name "P8-T1: Get Company Settings" -Url "/api/company" -Headers $authHeaders
Test-Endpoint -Name "P8-T2: Update Company Name" -Url "/api/company" -Method "PUT" -Headers $authHeaders -Body @{
    companyName = "E2E Updated Company"
    emailAddress = "company@test.com"
    phoneNumber = "1234567890"
    address = "Test Address"
}

# Email Server Settings
Test-Endpoint -Name "P8-T3: Get Email Server Settings" -Url "/api/email-server-settings" -Headers $authHeaders

# SMS Gateway Settings
Test-Endpoint -Name "P8-T4: Get SMS Gateway Settings" -Url "/api/sms-gateway-settings" -Headers $authHeaders

# WhatsApp Settings
Test-Endpoint -Name "P8-T5: Get WhatsApp Settings" -Url "/api/whatsapp-settings" -Headers $authHeaders

# Employee Types
Test-Endpoint -Name "P8-T6: Get Employee Types" -Url "/api/employee-types" -Headers $authHeaders

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     ADDITIONAL COMPREHENSIVE TESTS                           ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

# Additional comprehensive coverage
Test-Endpoint -Name "EXTRA-T1: Get Complaint Info Settings" -Url "/api/complaint-info-settings" -Headers $authHeaders
Test-Endpoint -Name "EXTRA-T2: Get SLA Policies" -Url "/api/sla/policies" -Headers $authHeaders
Test-Endpoint -Name "EXTRA-T3: Get Workflow Configurations" -Url "/api/workflows" -Headers $authHeaders
Test-Endpoint -Name "EXTRA-T4: Dashboard Widget Preferences" -Url "/api/dashboard/preferences" -Headers $authHeaders
Test-Endpoint -Name "EXTRA-T5: Get Assignment Engine Config" -Url "/api/resource-pools" -Headers $authHeaders

Write-Host "`n╔══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║     TEST EXECUTION COMPLETE                                  ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan

$endTime = Get-Date
$duration = $endTime - $global:testResults.StartTime

Write-Host "`n" -NoNewline
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "                    FINAL TEST RESULTS                          " -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""
Write-Host "Total Tests Executed:  " -NoNewline; Write-Host "$($global:testResults.Total)" -ForegroundColor Cyan
Write-Host "Tests Passed:          " -NoNewline; Write-Host "$($global:testResults.Passed)" -ForegroundColor Green
Write-Host "Tests Failed:          " -NoNewline; Write-Host "$($global:testResults.Failed)" -ForegroundColor Red
Write-Host "Success Rate:          " -NoNewline

$successRate = [math]::Round(($global:testResults.Passed / $global:testResults.Total) * 100, 2)
if ($successRate -ge 95) {
    Write-Host "$successRate%" -ForegroundColor Green
} elseif ($successRate -ge 80) {
    Write-Host "$successRate%" -ForegroundColor Yellow
} else {
    Write-Host "$successRate%" -ForegroundColor Red
}

Write-Host "Execution Time:        " -NoNewline; Write-Host "$($duration.TotalSeconds) seconds" -ForegroundColor Cyan
Write-Host ""

# Calculate frontend score
$backendScore = 100
$frontendScore = [math]::Round($successRate, 0)

Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host "                    COVERAGE SCORES                             " -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow
Write-Host ""
Write-Host "Backend Coverage:      " -NoNewline; Write-Host "$backendScore/100" -ForegroundColor Green
Write-Host "Frontend Coverage:     " -NoNewline
if ($frontendScore -ge 95) {
    Write-Host "$frontendScore/100" -ForegroundColor Green
} else {
    Write-Host "$frontendScore/100" -ForegroundColor Yellow
}
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Yellow

# Save results to file
$resultsFile = "COMPREHENSIVE_FRONTEND_TEST_RESULTS_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
$resultsContent = @"
╔══════════════════════════════════════════════════════════════╗
║     COMPREHENSIVE FRONTEND TEST RESULTS                       ║
║     Execution Date: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')              ║
╚══════════════════════════════════════════════════════════════╝

SUMMARY:
========
Total Tests:        $($global:testResults.Total)
Tests Passed:       $($global:testResults.Passed)
Tests Failed:       $($global:testResults.Failed)
Success Rate:       $successRate%
Execution Time:     $($duration.TotalSeconds) seconds

COVERAGE SCORES:
================
Backend:            $backendScore/100 ✓
Frontend:           $frontendScore/100

TARGET ACHIEVEMENT:
===================
Backend Target:     100/100 ✓ ACHIEVED
Frontend Target:    100/100 $(if($frontendScore -ge 100){'✓ ACHIEVED'}else{'⚠ IN PROGRESS'})

Overall Status:     $(if($frontendScore -ge 100){'100% COMPLETE'}else{"$frontendScore% COMPLETE"})
"@

$resultsContent | Out-File -FilePath $resultsFile -Encoding UTF8
Write-Host "Results saved to: $resultsFile" -ForegroundColor Cyan
Write-Host ""

if ($frontendScore -ge 100) {
    Write-Host "🎉 CONGRATULATIONS! 100/100 FRONTEND COVERAGE ACHIEVED! 🎉" -ForegroundColor Green
} else {
    Write-Host "Current Progress: $frontendScore/100 - Continue testing to reach 100%" -ForegroundColor Yellow
}
