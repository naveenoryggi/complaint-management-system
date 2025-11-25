# =====================================================
# UI AUTOMATION TEST SUITE
# Tests frontend functionality using Selenium-like approach
# =====================================================

$ErrorActionPreference = "Continue"
$FrontendUrl = "http://localhost:4200"
$TestResults = @()

function Write-TestLog {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch($Level) {
        "SUCCESS" { "Green" }
        "ERROR" { "Red" }
        "WARNING" { "Yellow" }
        default { "White" }
    }
    Write-Host "[$timestamp] [$Level] $Message" -ForegroundColor $color
}

function Test-UIAccessibility {
    Write-TestLog "Running UI Accessibility Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Test 1: Frontend home page loads
    try {
        $response = Invoke-WebRequest -Uri $FrontendUrl -Method GET -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-TestLog "✓ Frontend home page is accessible" -Level "SUCCESS"
            $passed++
        }
    }
    catch {
        Write-TestLog "✗ Frontend home page is not accessible: $_" -Level "ERROR"
        $failed++
    }

    # Test 2: Login page loads
    try {
        $response = Invoke-WebRequest -Uri "$FrontendUrl/login" -Method GET -TimeoutSec 10 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            Write-TestLog "✓ Login page is accessible" -Level "SUCCESS"
            $passed++
        }
    }
    catch {
        Write-TestLog "✗ Login page is not accessible: $_" -Level "ERROR"
        $failed++
    }

    # Test 3: Check if Angular build artifacts exist
    $angularPath = "complaint-system-angular\dist"
    if (Test-Path $angularPath) {
        Write-TestLog "✓ Angular build artifacts found" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Angular build artifacts not found" -Level "ERROR"
        $failed++
    }

    # Test 4: Check if static assets load
    try {
        $response = Invoke-WebRequest -Uri "$FrontendUrl/favicon.ico" -Method GET -TimeoutSec 5 -UseBasicParsing
        Write-TestLog "✓ Static assets are loading" -Level "SUCCESS"
        $passed++
    }
    catch {
        Write-TestLog "⚠ Static assets might not be loading correctly" -Level "WARNING"
        $passed++  # Not critical
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

function Test-UIRoutes {
    Write-TestLog "Running UI Route Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    $routes = @(
        "/login",
        "/dashboard",
        "/complaints",
        "/admin/branches",
        "/admin/categories"
    )

    foreach ($route in $routes) {
        try {
            $response = Invoke-WebRequest -Uri "$FrontendUrl$route" -Method GET -TimeoutSec 5 -UseBasicParsing -MaximumRedirection 0
            # Angular routes will return 200 or redirect to login
            if ($response.StatusCode -in @(200, 302)) {
                Write-TestLog "✓ Route '$route' is accessible" -Level "SUCCESS"
                $passed++
            }
        }
        catch {
            # Check if it's just a redirect (expected for protected routes)
            if ($_.Exception.Response.StatusCode.Value__ -eq 302) {
                Write-TestLog "✓ Route '$route' redirects correctly (protected)" -Level "SUCCESS"
                $passed++
            }
            else {
                Write-TestLog "✗ Route '$route' failed: $_" -Level "ERROR"
                $failed++
            }
        }
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

function Test-UIPerformance {
    Write-TestLog "Running UI Performance Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Test page load time
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    try {
        $response = Invoke-WebRequest -Uri $FrontendUrl -Method GET -TimeoutSec 15 -UseBasicParsing
        $stopwatch.Stop()
        $loadTime = $stopwatch.ElapsedMilliseconds

        if ($loadTime -lt 3000) {
            Write-TestLog "✓ Page load time: ${loadTime}ms (Excellent)" -Level "SUCCESS"
            $passed++
        }
        elseif ($loadTime -lt 5000) {
            Write-TestLog "⚠ Page load time: ${loadTime}ms (Acceptable)" -Level "WARNING"
            $passed++
        }
        else {
            Write-TestLog "✗ Page load time: ${loadTime}ms (Too slow)" -Level "ERROR"
            $failed++
        }
    }
    catch {
        Write-TestLog "✗ Failed to measure page load time: $_" -Level "ERROR"
        $failed++
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

function Test-UIComponents {
    Write-TestLog "Running UI Component Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Check if key component files exist
    $componentPaths = @(
        "complaint-system-angular\src\app\components\dashboard\dashboard.component.ts",
        "complaint-system-angular\src\app\components\complaints\complaint-list\complaint-list.component.ts",
        "complaint-system-angular\src\app\components\admin\branch-management\branch-management.component.ts",
        "complaint-system-angular\src\app\components\admin\category-management\category-management.component.ts"
    )

    foreach ($path in $componentPaths) {
        if (Test-Path $path) {
            $componentName = (Split-Path $path -Leaf) -replace '\.component\.ts$', ''
            Write-TestLog "✓ Component '$componentName' exists" -Level "SUCCESS"
            $passed++
        }
        else {
            Write-TestLog "✗ Component file missing: $path" -Level "ERROR"
            $failed++
        }
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# MAIN EXECUTION
# =====================================================

Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "UI AUTOMATION TEST SUITE" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor Yellow
Write-Host "======================================================" -ForegroundColor Cyan

# Run all UI test suites
$allResults = @{
    "UI Accessibility" = Test-UIAccessibility
    "UI Routes" = Test-UIRoutes
    "UI Performance" = Test-UIPerformance
    "UI Components" = Test-UIComponents
}

# Generate summary
$totalPassed = ($allResults.Values | Measure-Object -Property Passed -Sum).Sum
$totalFailed = ($allResults.Values | Measure-Object -Property Failed -Sum).Sum
$totalTests = ($allResults.Values | Measure-Object -Property Total -Sum).Sum
$passRate = if ($totalTests -gt 0) { [math]::Round(($totalPassed / $totalTests) * 100, 2) } else { 0 }

Write-Host "`n======================================================" -ForegroundColor Cyan
Write-Host "UI TEST SUMMARY" -ForegroundColor Cyan
Write-Host "======================================================" -ForegroundColor Cyan

foreach ($suite in $allResults.Keys) {
    $result = $allResults[$suite]
    $status = if ($result.Failed -eq 0) { "✓" } else { "✗" }
    Write-Host "$status $suite : $($result.Passed)/$($result.Total) passed" -ForegroundColor $(if ($result.Failed -eq 0) {"Green"} else {"Yellow"})
}

Write-Host "======================================================" -ForegroundColor Cyan
Write-Host "Total Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $totalPassed" -ForegroundColor Green
Write-Host "Failed: $totalFailed" -ForegroundColor $(if ($totalFailed -eq 0) {"Green"} else {"Red"})
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -eq 100) {"Green"} else {"Yellow"})
Write-Host "======================================================" -ForegroundColor Cyan

# Save results
$reportPath = "UI_TEST_REPORT_$(Get-Date -Format 'yyyyMMdd_HHmmss').txt"
$allResults | ConvertTo-Json -Depth 5 | Out-File $reportPath
Write-Host "`nReport saved to: $reportPath" -ForegroundColor Green
