# Comprehensive Gap Analysis Script
# Tests all critical API endpoints and identifies configuration gaps

$baseUrl = "http://localhost:5000"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$reportFile = "GAP_ANALYSIS_REPORT_$timestamp.json"
$reportMdFile = "GAP_ANALYSIS_REPORT_$timestamp.md"

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
Write-Host "COMPREHENSIVE GAP ANALYSIS" -ForegroundColor Cyan
Write-Host "Complaint Management System" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Authentication
Write-Host "[1/4] AUTHENTICATION TEST" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

try {
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token

    $report.authentication = @{
        status = "SUCCESS"
        message = "Successfully authenticated as admin"
        token_length = $token.Length
    }

    Write-Host "  Success: Authenticated as admin" -ForegroundColor Green
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
Write-Host "[2/4] CORE FEATURES ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$coreEndpoints = @(
    @{ method = "GET"; url = "/api/complaints"; name = "List Complaints"; category = "Core" },
    @{ method = "GET"; url = "/api/complaints/1"; name = "Get Single Complaint"; category = "Core" },
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
        } elseif ($response.PSObject.Properties["items"]) {
            $itemCount = $response.items.Count
        } elseif ($response.PSObject.Properties["data"]) {
            if ($response.data -is [Array]) {
                $itemCount = $response.data.Count
            }
        } elseif ($response.PSObject.Properties.Count -gt 0) {
            $itemCount = 1
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

# Step 3: Configuration Endpoints Testing
Write-Host "[3/4] CONFIGURATION ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$configEndpoints = @(
    @{ method = "GET"; url = "/api/sla"; name = "SLA Levels"; category = "Configuration"; configKey = "slaLevels" },
    @{ method = "GET"; url = "/api/notificationrules"; name = "Notification Rules"; category = "Configuration"; configKey = "notificationRules" },
    @{ method = "GET"; url = "/api/workflows"; name = "Workflows"; category = "Configuration"; configKey = "workflows" },
    @{ method = "GET"; url = "/api/escalation/matrices"; name = "Escalation Configuration"; category = "Configuration"; configKey = "escalationMatrices" },
    @{ method = "GET"; url = "/api/settings/email"; name = "Email Settings"; category = "Configuration"; configKey = "emailSettings" },
    @{ method = "GET"; url = "/api/categories"; name = "Complaint Categories"; category = "Configuration"; configKey = "categories" },
    @{ method = "GET"; url = "/api/roles"; name = "Role Definitions"; category = "Configuration"; configKey = "roles" }
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
        } elseif ($response.PSObject.Properties["items"]) {
            $itemCount = $response.items.Count
        } elseif ($response.PSObject.Properties["data"]) {
            if ($response.data -is [Array]) {
                $itemCount = $response.data.Count
            } else {
                $itemCount = 1
            }
        } elseif ($response.PSObject.Properties.Count -gt 0) {
            $itemCount = 1
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

# Step 4: Master Data Testing
Write-Host "[4/4] MASTER DATA ENDPOINTS" -ForegroundColor Yellow
Write-Host "----------------------------------------" -ForegroundColor Gray

$masterDataEndpoints = @(
    @{ method = "GET"; url = "/api/priorities"; name = "Priority Levels"; category = "MasterData"; configKey = "priorities" },
    @{ method = "GET"; url = "/api/statuses"; name = "Status Types"; category = "MasterData"; configKey = "statuses" }
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
        } elseif ($response.PSObject.Properties["items"]) {
            $itemCount = $response.items.Count
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
Write-Host "Failed: $failedTests" -ForegroundColor Red
Write-Host "Success Rate: $($report.summary.successRate)%" -ForegroundColor Cyan
Write-Host ""
Write-Host "Total Gaps Found: $($report.gaps.Count)" -ForegroundColor White
Write-Host "Critical: $criticalGaps" -ForegroundColor Red
Write-Host "High: $highGaps" -ForegroundColor Red
Write-Host "Medium: $mediumGaps" -ForegroundColor Yellow

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
        Write-Host "  CRITICAL: $($rec.action)" -ForegroundColor Red
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
        Write-Host "  $($failure.severity): $($rec.action)" -ForegroundColor Red
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
        Write-Host "  MEDIUM: $($rec.action)" -ForegroundColor Yellow
    }
}

$report.recommendations = $recommendations

# Save reports
$report | ConvertTo-Json -Depth 10 | Out-File -FilePath $reportFile -Encoding UTF8

# Generate Markdown Report
$mdContent = @"
# COMPREHENSIVE GAP ANALYSIS REPORT
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
**Message:** $($report.authentication.message)

---

## ENDPOINT TEST RESULTS

### Core Features

"@

foreach ($test in ($report.endpointTests | Where-Object { $_.category -eq "Core" })) {
    $status = if ($test.status -eq "PASS") { "PASS" } else { "FAIL" }
    $mdContent += "`n**[$status] $($test.name)**`n"
    $mdContent += "- Method: $($test.method)`n"
    $mdContent += "- URL: $($test.url)`n"
    $mdContent += "- Status: $($test.status) ($($test.statusCode))`n"
    if ($test.itemCount) {
        $mdContent += "- Items: $($test.itemCount)`n"
    }
    if ($test.error) {
        $mdContent += "- Error: $($test.error)`n"
    }
    $mdContent += "`n"
}

$mdContent += @"

### Configuration Endpoints

"@

foreach ($test in ($report.endpointTests | Where-Object { $_.category -eq "Configuration" })) {
    $status = if ($test.status -eq "PASS") { "PASS" } else { "FAIL" }
    $configured = if ($test.isConfigured) { "CONFIGURED" } else { "NOT CONFIGURED" }
    $mdContent += "`n**[$status] $($test.name)** - $configured`n"
    $mdContent += "- Method: $($test.method)`n"
    $mdContent += "- URL: $($test.url)`n"
    $mdContent += "- Status: $($test.status) ($($test.statusCode))`n"
    if ($test.itemCount) {
        $mdContent += "- Items: $($test.itemCount)`n"
    }
    if ($test.error) {
        $mdContent += "- Error: $($test.error)`n"
    }
    $mdContent += "`n"
}

$mdContent += @"

### Master Data Endpoints

"@

foreach ($test in ($report.endpointTests | Where-Object { $_.category -eq "MasterData" })) {
    $status = if ($test.status -eq "PASS") { "PASS" } else { "FAIL" }
    $mdContent += "`n**[$status] $($test.name)**`n"
    $mdContent += "- Method: $($test.method)`n"
    $mdContent += "- URL: $($test.url)`n"
    $mdContent += "- Status: $($test.status) ($($test.statusCode))`n"
    if ($test.itemCount) {
        $mdContent += "- Items: $($test.itemCount)`n"
    }
    if ($test.error) {
        $mdContent += "- Error: $($test.error)`n"
    }
    $mdContent += "`n"
}

$mdContent += @"

---

## GAPS IDENTIFIED

"@

if ($report.gaps.Count -eq 0) {
    $mdContent += "`nNo gaps identified. System is fully configured and operational.`n"
} else {
    # Group gaps by severity
    $criticalGapsList = $report.gaps | Where-Object { $_.severity -eq "CRITICAL" }
    $highGapsList = $report.gaps | Where-Object { $_.severity -eq "HIGH" }
    $mediumGapsList = $report.gaps | Where-Object { $_.severity -eq "MEDIUM" }

    if ($criticalGapsList.Count -gt 0) {
        $mdContent += "`n### CRITICAL GAPS ($($criticalGapsList.Count))`n`n"
        foreach ($gap in $criticalGapsList) {
            $mdContent += "**$($gap.type)** - $($gap.area)`n"
            $mdContent += "- Endpoint: $($gap.endpoint)`n"
            $mdContent += "- Issue: $($gap.message)`n`n"
        }
    }

    if ($highGapsList.Count -gt 0) {
        $mdContent += "`n### HIGH PRIORITY GAPS ($($highGapsList.Count))`n`n"
        foreach ($gap in $highGapsList) {
            $mdContent += "**$($gap.type)** - $($gap.area)`n"
            $mdContent += "- Endpoint: $($gap.endpoint)`n"
            $mdContent += "- Issue: $($gap.message)`n`n"
        }
    }

    if ($mediumGapsList.Count -gt 0) {
        $mdContent += "`n### MEDIUM PRIORITY GAPS ($($mediumGapsList.Count))`n`n"
        foreach ($gap in $mediumGapsList) {
            $mdContent += "**$($gap.type)** - $($gap.area)`n"
            $mdContent += "- Endpoint: $($gap.endpoint)`n"
            $mdContent += "- Issue: $($gap.message)`n`n"
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

## CONFIGURATION STATUS MATRIX

| Area | Status | Item Count |
|------|--------|------------|
"@

foreach ($key in $report.configurationStatus.Keys) {
    $config = $report.configurationStatus[$key]
    $status = if ($config.configured) { "Configured" } else { "Not Configured" }
    $mdContent += "| $key | $status | $($config.itemCount) |`n"
}

$mdContent += @"

---

## NEXT STEPS

1. **Address Critical Gaps First**: Focus on master data and endpoint failures
2. **Configure Missing Areas**: Set up notification rules, workflows, and SLA levels
3. **Verify Data Consistency**: Ensure all related endpoints return consistent data
4. **Re-run Gap Analysis**: After fixes, re-run this analysis to verify improvements

---

**Report Generated:** $($report.timestamp)
**Analysis Tool:** Comprehensive Gap Analysis Script v1.0
"@

$mdContent | Out-File -FilePath $reportMdFile -Encoding UTF8

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "REPORTS SAVED" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "JSON Report: $reportFile" -ForegroundColor Green
Write-Host "Markdown Report: $reportMdFile" -ForegroundColor Green
Write-Host ""
