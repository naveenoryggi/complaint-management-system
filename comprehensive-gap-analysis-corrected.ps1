# Comprehensive Gap Analysis Script - CORRECTED ROUTES
# Tests all critical API endpoints with correct routing

$baseUrl = "http://localhost:5000"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "GAP_ANALYSIS_REPORT_CORRECTED_$timestamp.json"
$reportMdFile = "GAP_ANALYSIS_REPORT_CORRECTED_$timestamp.md"

# Initialize report structure
$report = @{
    timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    baseUrl = $baseUrl
    authentication = @{}
    endpointTests = @()
    configurationStatus = @{}
    gaps = @()
    summary = @{}
    recommendations = @()
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "COMPREHENSIVE GAP ANALYSIS (CORRECTED)" -ForegroundColor Cyan
Write-Host "Complaint Management System" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Authentication
Write-Host "[1/5] AUTHENTICATION TEST" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $companyId = $loginResponse.data.user.companyId

    $report.authentication = @{
        status = "SUCCESS"
        message = "Successfully authenticated as admin"
        token_length = $token.Length
        companyId = $companyId
        userName = $loginResponse.data.user.fullName
    }

    Write-Host "  Success: Authenticated as $($loginResponse.data.user.fullName)" -ForegroundColor Green
    Write-Host "  Company ID: $companyId" -ForegroundColor Gray
    Write-Host "  Token length: $($token.Length) characters" -ForegroundColor Gray

} catch {
    $report.authentication = @{
        status = "FAILED"
        error = $_.Exception.Message
    }
    Write-Host "  Failed: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Create authorization headers
$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# Step 2: Core Features Testing
Write-Host "[2/5] CORE FEATURES ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$coreEndpoints = @(
    @{ method = "GET"; url = "/api/complaints"; name = "List Complaints"; category = "Core" },
    @{ method = "GET"; url = "/api/dashboard/statistics"; name = "Dashboard Statistics"; category = "Core" },
    @{ method = "GET"; url = "/api/users"; name = "User Management"; category = "Core" }
)

foreach ($endpoint in $coreEndpoints) {
    $testResult = @{
        method = $endpoint.method
        url = $endpoint.url
        name = $endpoint.name
        category = $endpoint.category
    }

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl$($endpoint.url)" -Method $endpoint.method -Headers $headers -ErrorAction Stop

        $itemCount = 0
        if ($response -is [Array]) {
            $itemCount = $response.Count
        } elseif ($response.PSObject.Properties["data"]) {
            if ($response.data -is [Array]) {
                $itemCount = $response.data.Count
            } elseif ($response.data.PSObject.Properties["items"]) {
                $itemCount = $response.data.items.Count
            } elseif ($response.data.PSObject.Properties["totalCount"]) {
                $itemCount = $response.data.totalCount
            }
        }

        $testResult.status = "PASS"
        $testResult.statusCode = 200
        $testResult.itemCount = $itemCount
        $testResult.hasData = $itemCount -gt 0

        Write-Host "  $($endpoint.name): PASS (Items: $itemCount)" -ForegroundColor Green

    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        $testResult.status = "FAIL"
        $testResult.statusCode = $statusCode
        $testResult.error = $_.Exception.Message

        Write-Host "  $($endpoint.name): FAIL (Status: $statusCode)" -ForegroundColor Red

        $report.gaps += @{
            type = "ENDPOINT_FAILURE"
            severity = "HIGH"
            endpoint = $endpoint.url
            message = "Core endpoint not accessible or returning errors"
        }
    }

    $report.endpointTests += $testResult
}

Write-Host ""

# Step 3: Configuration Endpoints Testing (CORRECTED ROUTES)
Write-Host "[3/5] CONFIGURATION ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$configEndpoints = @(
    @{ method = "GET"; url = "/api/sla/settings"; name = "SLA Settings"; category = "Configuration"; configKey = "slaSettings" },
    @{ method = "GET"; url = "/api/sla/levels"; name = "SLA Levels"; category = "Configuration"; configKey = "slaLevels" },
    @{ method = "GET"; url = "/api/event-communication-rules"; name = "Notification Rules"; category = "Configuration"; configKey = "notificationRules" },
    @{ method = "GET"; url = "/api/workflows"; name = "Workflows"; category = "Configuration"; configKey = "workflows" },
    @{ method = "GET"; url = "/api/escalation/matrices"; name = "Escalation Configuration"; category = "Configuration"; configKey = "escalationMatrices" },
    @{ method = "GET"; url = "/api/email-settings"; name = "Email Settings"; category = "Configuration"; configKey = "emailSettings" },
    @{ method = "GET"; url = "/api/categories"; name = "Complaint Categories"; category = "Configuration"; configKey = "categories" },
    @{ method = "GET"; url = "/api/roles"; name = "Role Definitions"; category = "Configuration"; configKey = "roles" },
    @{ method = "GET"; url = "/api/communicationtemplates"; name = "Communication Templates"; category = "Configuration"; configKey = "templates" },
    @{ method = "GET"; url = "/api/eventtypes"; name = "Event Types"; category = "Configuration"; configKey = "eventTypes" }
)

foreach ($endpoint in $configEndpoints) {
    $testResult = @{
        method = $endpoint.method
        url = $endpoint.url
        name = $endpoint.name
        category = $endpoint.category
    }

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl$($endpoint.url)" -Method $endpoint.method -Headers $headers -ErrorAction Stop

        $itemCount = 0
        $isConfigured = $false

        if ($response -is [Array]) {
            $itemCount = $response.Count
        } elseif ($response.PSObject.Properties["data"]) {
            if ($response.data -is [Array]) {
                $itemCount = $response.data.Count
            } elseif ($response.data.PSObject.Properties["items"]) {
                $itemCount = $response.data.items.Count
            } elseif ($response.data.PSObject.Properties.Count -gt 0) {
                $itemCount = 1
            }
        }

        $isConfigured = $itemCount -gt 0

        $testResult.status = "PASS"
        $testResult.statusCode = 200
        $testResult.itemCount = $itemCount
        $testResult.isConfigured = $isConfigured

        $report.configurationStatus[$endpoint.configKey] = @{
            configured = $isConfigured
            itemCount = $itemCount
        }

        if ($isConfigured) {
            Write-Host "  $($endpoint.name): CONFIGURED (Items: $itemCount)" -ForegroundColor Green
        } else {
            Write-Host "  $($endpoint.name): NOT CONFIGURED (Items: 0)" -ForegroundColor Yellow

            $report.gaps += @{
                type = "CONFIGURATION_MISSING"
                severity = "MEDIUM"
                area = $endpoint.name
                endpoint = $endpoint.url
                message = "Configuration area has no items defined"
            }
        }

    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        $testResult.status = "FAIL"
        $testResult.statusCode = $statusCode
        $testResult.error = $_.Exception.Message

        Write-Host "  $($endpoint.name): ENDPOINT FAILURE (Status: $statusCode)" -ForegroundColor Red

        $report.gaps += @{
            type = "ENDPOINT_FAILURE"
            severity = "HIGH"
            endpoint = $endpoint.url
            message = "Configuration endpoint not accessible"
        }
    }

    $report.endpointTests += $testResult
}

Write-Host ""

# Step 4: Master Data Testing (CORRECTED ROUTES)
Write-Host "[4/5] MASTER DATA ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$masterDataEndpoints = @(
    @{ method = "GET"; url = "/api/ComplaintPriorityMaster"; name = "Priority Levels"; category = "MasterData"; configKey = "priorities" },
    @{ method = "GET"; url = "/api/ComplaintStatusMaster"; name = "Status Types"; category = "MasterData"; configKey = "statuses" }
)

foreach ($endpoint in $masterDataEndpoints) {
    $testResult = @{
        method = $endpoint.method
        url = $endpoint.url
        name = $endpoint.name
        category = $endpoint.category
    }

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl$($endpoint.url)" -Method $endpoint.method -Headers $headers -ErrorAction Stop

        $itemCount = 0
        if ($response -is [Array]) {
            $itemCount = $response.Count
        } elseif ($response.PSObject.Properties["data"]) {
            if ($response.data -is [Array]) {
                $itemCount = $response.data.Count
            }
        }

        $testResult.status = "PASS"
        $testResult.statusCode = 200
        $testResult.itemCount = $itemCount
        $testResult.hasData = $itemCount -gt 0

        $report.configurationStatus[$endpoint.configKey] = @{
            configured = $itemCount -gt 0
            itemCount = $itemCount
        }

        if ($itemCount -gt 0) {
            Write-Host "  $($endpoint.name): AVAILABLE (Items: $itemCount)" -ForegroundColor Green
        } else {
            Write-Host "  $($endpoint.name): NO DATA (Items: 0)" -ForegroundColor Yellow

            $report.gaps += @{
                type = "MASTER_DATA_MISSING"
                severity = "CRITICAL"
                area = $endpoint.name
                endpoint = $endpoint.url
                message = "Critical master data is missing - system cannot function properly"
            }
        }

    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        $testResult.status = "FAIL"
        $testResult.statusCode = $statusCode
        $testResult.error = $_.Exception.Message

        Write-Host "  $($endpoint.name): ENDPOINT FAILURE (Status: $statusCode)" -ForegroundColor Red

        $report.gaps += @{
            type = "ENDPOINT_FAILURE"
            severity = "CRITICAL"
            endpoint = $endpoint.url
            message = "Master data endpoint not accessible"
        }
    }

    $report.endpointTests += $testResult
}

Write-Host ""

# Step 5: Additional Critical Endpoints
Write-Host "[5/5] ADDITIONAL SYSTEM ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$additionalEndpoints = @(
    @{ method = "GET"; url = "/api/branches"; name = "Branches"; category = "Organization"; configKey = "branches" },
    @{ method = "GET"; url = "/api/departments"; name = "Departments"; category = "Organization"; configKey = "departments" },
    @{ method = "GET"; url = "/api/sections"; name = "Sections"; category = "Organization"; configKey = "sections" },
    @{ method = "GET"; url = "/api/resourcepool"; name = "Resource Pools"; category = "Escalation"; configKey = "resourcePools" },
    @{ method = "GET"; url = "/api/escalationpolicy"; name = "Escalation Policies"; category = "Escalation"; configKey = "escalationPolicies" }
)

foreach ($endpoint in $additionalEndpoints) {
    $testResult = @{
        method = $endpoint.method
        url = $endpoint.url
        name = $endpoint.name
        category = $endpoint.category
    }

    try {
        $response = Invoke-RestMethod -Uri "$baseUrl$($endpoint.url)" -Method $endpoint.method -Headers $headers -ErrorAction Stop

        $itemCount = 0
        if ($response -is [Array]) {
            $itemCount = $response.Count
        } elseif ($response.PSObject.Properties["data"]) {
            if ($response.data -is [Array]) {
                $itemCount = $response.data.Count
            }
        }

        $testResult.status = "PASS"
        $testResult.statusCode = 200
        $testResult.itemCount = $itemCount
        $testResult.hasData = $itemCount -gt 0

        $report.configurationStatus[$endpoint.configKey] = @{
            configured = $itemCount -gt 0
            itemCount = $itemCount
        }

        if ($itemCount -gt 0) {
            Write-Host "  $($endpoint.name): CONFIGURED (Items: $itemCount)" -ForegroundColor Green
        } else {
            Write-Host "  $($endpoint.name): NOT CONFIGURED (Items: 0)" -ForegroundColor Yellow
        }

    } catch {
        $statusCode = $_.Exception.Response.StatusCode.Value__
        $testResult.status = "FAIL"
        $testResult.statusCode = $statusCode
        $testResult.error = $_.Exception.Message

        Write-Host "  $($endpoint.name): ENDPOINT FAILURE (Status: $statusCode)" -ForegroundColor Red
    }

    $report.endpointTests += $testResult
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Calculate summary statistics
$totalTests = $report.endpointTests.Count
$passedTests = ($report.endpointTests | Where-Object { $_.status -eq "PASS" }).Count
$failedTests = ($report.endpointTests | Where-Object { $_.status -eq "FAIL" }).Count

$criticalGaps = ($report.gaps | Where-Object { $_.severity -eq "CRITICAL" }).Count
$highGaps = ($report.gaps | Where-Object { $_.severity -eq "HIGH" }).Count
$mediumGaps = ($report.gaps | Where-Object { $_.severity -eq "MEDIUM" }).Count

$report.summary = @{
    totalEndpointsTested = $totalTests
    passedTests = $passedTests
    failedTests = $failedTests
    successRate = [math]::Round(($passedTests / $totalTests) * 100, 2)
    totalGaps = $report.gaps.Count
    criticalGaps = $criticalGaps
    highGaps = $highGaps
    mediumGaps = $mediumGaps
}

Write-Host ""
Write-Host "Total Endpoints Tested: $totalTests" -ForegroundColor White
Write-Host "Passed: $passedTests" -ForegroundColor Green
Write-Host "Failed: $failedTests" -ForegroundColor $(if ($failedTests -gt 0) { "Red" } else { "Green" })
Write-Host "Success Rate: $($report.summary.successRate)%" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total Gaps Found: $($report.gaps.Count)" -ForegroundColor White
Write-Host "Critical: $criticalGaps" -ForegroundColor $(if ($criticalGaps -gt 0) { "Red" } else { "Green" })
Write-Host "High: $highGaps" -ForegroundColor $(if ($highGaps -gt 0) { "Red" } else { "Green" })
Write-Host "Medium: $mediumGaps" -ForegroundColor $(if ($mediumGaps -gt 0) { "Yellow" } else { "Green" })

# Generate recommendations
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "PRIORITY RECOMMENDATIONS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$recommendations = @()

# Check for critical master data issues
if ($criticalGaps -gt 0) {
    $masterDataGaps = $report.gaps | Where-Object { $_.type -eq "MASTER_DATA_MISSING" }
    foreach ($gap in $masterDataGaps) {
        $rec = @{
            priority = "CRITICAL"
            area = $gap.area
            action = "Populate $($gap.area) immediately - system cannot function without this data"
        }
        $recommendations += $rec
        Write-Host "  [CRITICAL] $($rec.action)" -ForegroundColor Red
    }
}

# Check for endpoint failures
$endpointFailures = $report.gaps | Where-Object { $_.type -eq "ENDPOINT_FAILURE" }
if ($endpointFailures.Count -gt 0) {
    foreach ($failure in $endpointFailures) {
        $rec = @{
            priority = $failure.severity
            area = $failure.endpoint
            action = "Fix endpoint accessibility issue: $($failure.endpoint)"
        }
        $recommendations += $rec
        Write-Host "  [$($failure.severity)] $($rec.action)" -ForegroundColor Red
    }
}

# Check for configuration gaps
$configGaps = $report.gaps | Where-Object { $_.type -eq "CONFIGURATION_MISSING" }
if ($configGaps.Count -gt 0) {
    foreach ($gap in $configGaps) {
        $rec = @{
            priority = "MEDIUM"
            area = $gap.area
            action = "Configure $($gap.area) to enable full system functionality"
        }
        $recommendations += $rec
        Write-Host "  [MEDIUM] $($rec.action)" -ForegroundColor Yellow
    }
}

if ($recommendations.Count -eq 0) {
    Write-Host "  No critical issues found. System is operational." -ForegroundColor Green
}

$report.recommendations = $recommendations

# Save JSON report
$report | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportFile -Encoding UTF8

# Generate Markdown Report (abbreviated for brevity)
$mdContent = @"
# COMPREHENSIVE GAP ANALYSIS REPORT (CORRECTED)
**Complaint Management System**

**Generated:** $($report.timestamp)
**Base URL:** $($report.baseUrl)

---

## EXECUTIVE SUMMARY

| Metric | Value |
|--------|-------|
| Total Endpoints Tested | $($report.summary.totalEndpointsTested) |
| Passed Tests | $($report.summary.passedTests) |
| Failed Tests | $($report.summary.failedTests) |
| Success Rate | $($report.summary.successRate)% |
| **Total Gaps Found** | **$($report.summary.totalGaps)** |
| Critical Gaps | $($report.summary.criticalGaps) |
| High Priority Gaps | $($report.summary.highGaps) |
| Medium Priority Gaps | $($report.summary.mediumGaps) |

---

## AUTHENTICATION STATUS

**Status:** $($report.authentication.status)
**User:** $($report.authentication.userName)
**Company ID:** $($report.authentication.companyId)

---

## CONFIGURATION STATUS MATRIX

| Area | Status | Item Count |
|------|--------|------------|
"@

foreach ($key in ($report.configurationStatus.Keys | Sort-Object)) {
    $config = $report.configurationStatus[$key]
    $status = if ($config.configured) { "Configured" } else { "Not Configured" }
    $mdContent += "| $key | $status | $($config.itemCount) |`n"
}

$mdContent += @"

---

## GAPS IDENTIFIED

"@

if ($report.gaps.Count -eq 0) {
    $mdContent += "`nNo gaps identified. System is fully configured and operational.`n"
} else {
    $criticalGapsList = $report.gaps | Where-Object { $_.severity -eq "CRITICAL" }
    $highGapsList = $report.gaps | Where-Object { $_.severity -eq "HIGH" }
    $mediumGapsList = $report.gaps | Where-Object { $_.severity -eq "MEDIUM" }

    if ($criticalGapsList.Count -gt 0) {
        $mdContent += "`n### CRITICAL GAPS ($($criticalGapsList.Count))`n`n"
        foreach ($gap in $criticalGapsList) {
            $mdContent += "- **$($gap.area)**: $($gap.message) [$($gap.endpoint)]`n"
        }
    }

    if ($highGapsList.Count -gt 0) {
        $mdContent += "`n### HIGH PRIORITY GAPS ($($highGapsList.Count))`n`n"
        foreach ($gap in $highGapsList) {
            $mdContent += "- **$($gap.area)**: $($gap.message) [$($gap.endpoint)]`n"
        }
    }

    if ($mediumGapsList.Count -gt 0) {
        $mdContent += "`n### MEDIUM PRIORITY GAPS ($($mediumGapsList.Count))`n`n"
        foreach ($gap in $mediumGapsList) {
            $mdContent += "- **$($gap.area)**: $($gap.message) [$($gap.endpoint)]`n"
        }
    }
}

$mdContent += @"

---

## PRIORITY RECOMMENDATIONS

"@

if ($recommendations.Count -eq 0) {
    $mdContent += "`nNo recommendations. System is fully operational.`n"
} else {
    $priorityOrder = @("CRITICAL", "HIGH", "MEDIUM", "LOW")
    foreach ($priority in $priorityOrder) {
        $recs = $recommendations | Where-Object { $_.priority -eq $priority }
        if ($recs.Count -gt 0) {
            $mdContent += "`n### $priority PRIORITY ($($recs.Count))`n`n"
            $index = 1
            foreach ($rec in $recs) {
                $mdContent += "$index. **$($rec.area)**: $($rec.action)`n"
                $index++
            }
        }
    }
}

$mdContent += @"

---

**Report Generated:** $($report.timestamp)
**Analysis Tool:** Comprehensive Gap Analysis Script v2.0 (Corrected Routes)
"@

$mdContent | Out-File -FilePath $reportMdFile -Encoding UTF8

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "REPORTS SAVED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "JSON Report: $reportFile" -ForegroundColor Green
Write-Host "Markdown Report: $reportMdFile" -ForegroundColor Green
Write-Host ""
