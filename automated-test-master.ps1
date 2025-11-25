# =====================================================
# AUTONOMOUS MASTER TEST SUITE
# Runs all tests automatically without user involvement
# =====================================================

param(
    [switch]$Continuous,  # Run continuously every X hours
    [int]$IntervalHours = 6,  # Run every 6 hours by default
    [switch]$Verbose
)

$ErrorActionPreference = "Continue"
$Global:BaseUrl = "http://localhost:5058/api"
$Global:FrontendUrl = "http://localhost:4200"
$Global:TestResults = @()
$Global:Token = $null
$Global:CompanyId = $null

# =====================================================
# UTILITY FUNCTIONS
# =====================================================

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

    # Log to file
    Add-Content -Path "TEST_MASTER_LOG.txt" -Value "[$timestamp] [$Level] $Message"
}

function Invoke-TestAPI {
    param(
        [string]$Method,
        [string]$Endpoint,
        [object]$Body = $null,
        [bool]$RequireAuth = $true
    )

    try {
        $headers = @{
            "Content-Type" = "application/json"
        }

        if ($RequireAuth -and $Global:Token) {
            $headers["Authorization"] = "Bearer $Global:Token"
        }

        $url = "$Global:BaseUrl/$Endpoint"

        $params = @{
            Uri = $url
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-RestMethod @params
        return @{Success = $true; Data = $response}
    }
    catch {
        return @{Success = $false; Error = $_.Exception.Message}
    }
}

function Get-AuthToken {
    Write-TestLog "Obtaining authentication token..."

    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    }

    $result = Invoke-TestAPI -Method "POST" -Endpoint "auth/login" -Body $loginBody -RequireAuth $false

    if ($result.Success) {
        $Global:Token = $result.Data.data.token
        $Global:CompanyId = $result.Data.data.companyId
        Write-TestLog "Authentication successful" -Level "SUCCESS"
        return $true
    }
    else {
        Write-TestLog "Authentication failed: $($result.Error)" -Level "ERROR"
        return $false
    }
}

# =====================================================
# TEST SUITE 1: API HEALTH CHECKS
# =====================================================

function Test-APIHealth {
    Write-TestLog "Running API Health Checks..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Test 1: Backend API is responding
    try {
        $response = Invoke-RestMethod -Uri "$Global:BaseUrl/../health" -Method GET -TimeoutSec 5
        Write-TestLog "✓ Backend API is healthy" -Level "SUCCESS"
        $passed++
    }
    catch {
        Write-TestLog "✗ Backend API health check failed" -Level "ERROR"
        $failed++
    }

    # Test 2: Frontend is accessible
    try {
        $response = Invoke-WebRequest -Uri $Global:FrontendUrl -Method GET -TimeoutSec 5 -UseBasicParsing
        Write-TestLog "✓ Frontend is accessible" -Level "SUCCESS"
        $passed++
    }
    catch {
        Write-TestLog "✗ Frontend is not accessible" -Level "ERROR"
        $failed++
    }

    # Test 3: Database connection (via API)
    $result = Invoke-TestAPI -Method "GET" -Endpoint "categories"
    if ($result.Success) {
        Write-TestLog "✓ Database connection is working" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Database connection failed" -Level "ERROR"
        $failed++
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# TEST SUITE 2: CRUD OPERATIONS
# =====================================================

function Test-CRUDOperations {
    Write-TestLog "Running CRUD Operations Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Get test data
    $categories = (Invoke-TestAPI -Method "GET" -Endpoint "categories").Data.data
    $branches = (Invoke-TestAPI -Method "GET" -Endpoint "branches?companyId=$Global:CompanyId").Data.data

    if (-not $categories -or $categories.Count -eq 0) {
        Write-TestLog "No categories found for testing" -Level "WARNING"
        return @{Passed = 0; Failed = 1; Total = 1}
    }

    # Test CREATE
    $createData = @{
        title = "Automated Test Complaint $(Get-Date -Format 'yyyyMMddHHmmss')"
        description = "This is an automated test complaint created by the test suite"
        categoryId = $categories[0].id
        priority = 1
        branchId = if ($branches -and $branches.Count -gt 0) { $branches[0].id } else { $null }
    }

    $createResult = Invoke-TestAPI -Method "POST" -Endpoint "complaints" -Body $createData
    if ($createResult.Success) {
        $complaintId = $createResult.Data.data.id
        Write-TestLog "✓ CREATE: Complaint created successfully" -Level "SUCCESS"
        $passed++

        # Test READ
        $readResult = Invoke-TestAPI -Method "GET" -Endpoint "complaints/$complaintId"
        if ($readResult.Success) {
            Write-TestLog "✓ READ: Complaint retrieved successfully" -Level "SUCCESS"
            $passed++
        }
        else {
            Write-TestLog "✗ READ: Failed to retrieve complaint" -Level "ERROR"
            $failed++
        }

        # Test UPDATE
        $updateData = @{
            id = $complaintId
            title = "Updated: $($createData.title)"
            description = $createData.description
            categoryId = $createData.categoryId
            priority = 2
            status = 1
            assignedToId = $null
            resolutionNotes = $null
            tags = $null
        }

        $updateResult = Invoke-TestAPI -Method "PUT" -Endpoint "complaints/$complaintId" -Body $updateData
        if ($updateResult.Success) {
            Write-TestLog "✓ UPDATE: Complaint updated successfully" -Level "SUCCESS"
            $passed++
        }
        else {
            Write-TestLog "✗ UPDATE: Failed to update complaint" -Level "ERROR"
            $failed++
        }

        # Test DELETE
        $deleteResult = Invoke-TestAPI -Method "DELETE" -Endpoint "complaints/$complaintId"
        if ($deleteResult.Success) {
            Write-TestLog "✓ DELETE: Complaint deleted successfully" -Level "SUCCESS"
            $passed++
        }
        else {
            Write-TestLog "✗ DELETE: Failed to delete complaint" -Level "ERROR"
            $failed++
        }
    }
    else {
        Write-TestLog "✗ CREATE: Failed to create complaint" -Level "ERROR"
        $failed += 4  # All subsequent tests fail if create fails
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# TEST SUITE 3: COMMENT SYSTEM
# =====================================================

function Test-CommentSystem {
    Write-TestLog "Running Comment System Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Get existing complaint
    $complaints = (Invoke-TestAPI -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=1").Data.data

    if (-not $complaints -or $complaints.Count -eq 0) {
        Write-TestLog "No complaints available for comment testing" -Level "WARNING"
        return @{Passed = 0; Failed = 0; Total = 0}
    }

    $complaintId = $complaints[0].id

    # Test ADD COMMENT
    $commentData = @{
        comment = "Automated test comment - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
        isInternal = $false
    }

    $addResult = Invoke-TestAPI -Method "POST" -Endpoint "complaints/$complaintId/comments" -Body $commentData
    if ($addResult.Success) {
        Write-TestLog "✓ Comment added successfully" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Failed to add comment: $($addResult.Error)" -Level "ERROR"
        $failed++
    }

    # Test GET COMMENTS
    $getResult = Invoke-TestAPI -Method "GET" -Endpoint "complaints/$complaintId/comments"
    if ($getResult.Success) {
        Write-TestLog "✓ Comments retrieved successfully" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Failed to retrieve comments" -Level "ERROR"
        $failed++
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# TEST SUITE 4: STATUS TRANSITIONS
# =====================================================

function Test-StatusTransitions {
    Write-TestLog "Running Status Transition Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Get existing complaint
    $complaints = (Invoke-TestAPI -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=1").Data.data

    if (-not $complaints -or $complaints.Count -eq 0) {
        Write-TestLog "No complaints available for status testing" -Level "WARNING"
        return @{Passed = 0; Failed = 0; Total = 0}
    }

    $complaintId = $complaints[0].id

    # Get full complaint data
    $fullComplaint = (Invoke-TestAPI -Method "GET" -Endpoint "complaints/$complaintId").Data.data

    # Convert priority string to enum value
    $priorityValue = switch($fullComplaint.priority) {
        "Low" { 0 }
        "Normal" { 1 }
        "High" { 2 }
        "Critical" { 3 }
        "Urgent" { 4 }
        default { 1 }
    }

    # Test transition to Under Review
    $updateData = @{
        id = $fullComplaint.id
        title = $fullComplaint.title
        description = $fullComplaint.description
        categoryId = $fullComplaint.categoryId
        priority = $priorityValue
        status = 1  # Under Review
        assignedToId = $fullComplaint.assignedToId
        resolutionNotes = $fullComplaint.resolutionNotes
        tags = $fullComplaint.tags
    }

    $result = Invoke-TestAPI -Method "PUT" -Endpoint "complaints/$complaintId" -Body $updateData
    if ($result.Success) {
        Write-TestLog "✓ Status transition to 'Under Review' successful" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Status transition failed: $($result.Error)" -Level "ERROR"
        $failed++
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# TEST SUITE 5: DASHBOARD & REPORTS
# =====================================================

function Test-DashboardReports {
    Write-TestLog "Running Dashboard & Reports Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Test dashboard stats
    $statsResult = Invoke-TestAPI -Method "GET" -Endpoint "dashboard/stats?days=30"
    if ($statsResult.Success) {
        Write-TestLog "✓ Dashboard stats retrieved successfully" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Failed to retrieve dashboard stats" -Level "ERROR"
        $failed++
    }

    # Test dashboard preferences
    $prefsResult = Invoke-TestAPI -Method "GET" -Endpoint "dashboard/preferences"
    if ($prefsResult.Success) {
        Write-TestLog "✓ Dashboard preferences retrieved successfully" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Failed to retrieve dashboard preferences" -Level "ERROR"
        $failed++
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# TEST SUITE 6: SEARCH & FILTERS
# =====================================================

function Test-SearchFilters {
    Write-TestLog "Running Search & Filter Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Test basic search
    $searchResult = Invoke-TestAPI -Method "GET" -Endpoint "complaints?searchTerm=test&pageNumber=1&pageSize=10"
    if ($searchResult.Success) {
        Write-TestLog "✓ Search functionality working" -Level "SUCCESS"
        $passed++
    }
    else {
        Write-TestLog "✗ Search functionality failed" -Level "ERROR"
        $failed++
    }

    # Test category filter
    $categories = (Invoke-TestAPI -Method "GET" -Endpoint "categories").Data.data
    if ($categories -and $categories.Count -gt 0) {
        $filterResult = Invoke-TestAPI -Method "GET" -Endpoint "complaints?categoryId=$($categories[0].id)&pageNumber=1&pageSize=10"
        if ($filterResult.Success) {
            Write-TestLog "✓ Category filter working" -Level "SUCCESS"
            $passed++
        }
        else {
            Write-TestLog "✗ Category filter failed" -Level "ERROR"
            $failed++
        }
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# TEST SUITE 7: PERFORMANCE TESTS
# =====================================================

function Test-Performance {
    Write-TestLog "Running Performance Tests..." -Level "INFO"
    $passed = 0
    $failed = 0

    # Test API response time
    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
    $result = Invoke-TestAPI -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=10"
    $stopwatch.Stop()

    $responseTime = $stopwatch.ElapsedMilliseconds

    if ($responseTime -lt 2000) {
        Write-TestLog "✓ API response time: ${responseTime}ms (Good)" -Level "SUCCESS"
        $passed++
    }
    elseif ($responseTime -lt 5000) {
        Write-TestLog "⚠ API response time: ${responseTime}ms (Acceptable)" -Level "WARNING"
        $passed++
    }
    else {
        Write-TestLog "✗ API response time: ${responseTime}ms (Too slow)" -Level "ERROR"
        $failed++
    }

    return @{Passed = $passed; Failed = $failed; Total = ($passed + $failed)}
}

# =====================================================
# MAIN EXECUTION LOOP
# =====================================================

function Start-TestExecution {
    $startTime = Get-Date
    Write-TestLog "=================================================" -Level "INFO"
    Write-TestLog "AUTOMATED TEST SUITE STARTING" -Level "INFO"
    Write-TestLog "=================================================" -Level "INFO"

    # Authenticate
    if (-not (Get-AuthToken)) {
        Write-TestLog "Cannot proceed without authentication" -Level "ERROR"
        return
    }

    # Run all test suites
    $allResults = @{
        "API Health" = Test-APIHealth
        "CRUD Operations" = Test-CRUDOperations
        "Comment System" = Test-CommentSystem
        "Status Transitions" = Test-StatusTransitions
        "Dashboard & Reports" = Test-DashboardReports
        "Search & Filters" = Test-SearchFilters
        "Performance" = Test-Performance
    }

    # Generate summary report
    $totalPassed = 0
    $totalFailed = 0
    $totalTests = 0

    Write-TestLog "=================================================" -Level "INFO"
    Write-TestLog "TEST SUMMARY REPORT" -Level "INFO"
    Write-TestLog "=================================================" -Level "INFO"

    foreach ($suite in $allResults.Keys) {
        $result = $allResults[$suite]
        $totalPassed += $result.Passed
        $totalFailed += $result.Failed
        $totalTests += $result.Total

        $status = if ($result.Failed -eq 0) { "✓" } else { "✗" }
        Write-TestLog "$status $suite : $($result.Passed)/$($result.Total) passed" -Level $(if ($result.Failed -eq 0) {"SUCCESS"} else {"WARNING"})
    }

    $passRate = if ($totalTests -gt 0) { [math]::Round(($totalPassed / $totalTests) * 100, 2) } else { 0 }
    $endTime = Get-Date
    $duration = $endTime - $startTime

    Write-TestLog "=================================================" -Level "INFO"
    Write-TestLog "FINAL RESULTS" -Level "INFO"
    Write-TestLog "Total Tests: $totalTests" -Level "INFO"
    Write-TestLog "Passed: $totalPassed" -Level "SUCCESS"
    Write-TestLog "Failed: $totalFailed" -Level $(if ($totalFailed -eq 0) {"SUCCESS"} else {"ERROR"})
    Write-TestLog "Pass Rate: $passRate%" -Level $(if ($passRate -eq 100) {"SUCCESS"} else {"WARNING"})
    Write-TestLog "Duration: $($duration.ToString('hh\:mm\:ss'))" -Level "INFO"
    Write-TestLog "=================================================" -Level "INFO"

    # Save detailed report
    $reportPath = "TEST_REPORT_$(Get-Date -Format 'yyyyMMdd_HHmmss').html"
    Generate-HTMLReport -Results $allResults -Path $reportPath -PassRate $passRate -Duration $duration

    Write-TestLog "Detailed HTML report saved to: $reportPath" -Level "SUCCESS"

    # Send alert if failures detected
    if ($totalFailed -gt 0) {
        Write-TestLog "⚠ ATTENTION: $totalFailed test(s) failed. Please review the report." -Level "WARNING"
    }
}

function Generate-HTMLReport {
    param($Results, $Path, $PassRate, $Duration)

    $html = @"
<!DOCTYPE html>
<html>
<head>
    <title>Automated Test Report - $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; background: #f5f5f5; }
        .container { max-width: 1200px; margin: 0 auto; background: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1); }
        h1 { color: #333; border-bottom: 3px solid #4CAF50; padding-bottom: 10px; }
        .summary { display: grid; grid-template-columns: repeat(4, 1fr); gap: 20px; margin: 30px 0; }
        .summary-card { background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 20px; border-radius: 8px; text-align: center; }
        .summary-card.success { background: linear-gradient(135deg, #4CAF50 0%, #45a049 100%); }
        .summary-card.warning { background: linear-gradient(135deg, #ff9800 0%, #f57c00 100%); }
        .summary-card.error { background: linear-gradient(135deg, #f44336 0%, #d32f2f 100%); }
        .summary-card h3 { margin: 0; font-size: 14px; opacity: 0.9; }
        .summary-card .value { font-size: 36px; font-weight: bold; margin: 10px 0; }
        .test-suite { margin: 20px 0; border: 1px solid #ddd; border-radius: 8px; overflow: hidden; }
        .test-suite-header { background: #667eea; color: white; padding: 15px; font-weight: bold; display: flex; justify-content: space-between; }
        .test-suite-header.pass { background: #4CAF50; }
        .test-suite-header.fail { background: #f44336; }
        .test-suite-body { padding: 15px; background: #fafafa; }
        .badge { display: inline-block; padding: 5px 15px; border-radius: 20px; font-size: 12px; font-weight: bold; }
        .badge.success { background: #4CAF50; color: white; }
        .badge.error { background: #f44336; color: white; }
        .timestamp { color: #666; font-size: 14px; margin-top: 20px; }
    </style>
</head>
<body>
    <div class="container">
        <h1>🤖 Automated Test Report</h1>
        <div class="timestamp">Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss') | Duration: $($Duration.ToString('hh\:mm\:ss'))</div>

        <div class="summary">
            <div class="summary-card">
                <h3>Total Tests</h3>
                <div class="value">$(($Results.Values | Measure-Object -Property Total -Sum).Sum)</div>
            </div>
            <div class="summary-card success">
                <h3>Passed</h3>
                <div class="value">$(($Results.Values | Measure-Object -Property Passed -Sum).Sum)</div>
            </div>
            <div class="summary-card error">
                <h3>Failed</h3>
                <div class="value">$(($Results.Values | Measure-Object -Property Failed -Sum).Sum)</div>
            </div>
            <div class="summary-card $(if($PassRate -eq 100){'success'}elseif($PassRate -ge 80){'warning'}else{'error'})">
                <h3>Pass Rate</h3>
                <div class="value">$PassRate%</div>
            </div>
        </div>

        <h2>Test Suite Results</h2>
"@

    foreach ($suite in $Results.Keys) {
        $result = $Results[$suite]
        $headerClass = if ($result.Failed -eq 0) { "pass" } else { "fail" }
        $badgeClass = if ($result.Failed -eq 0) { "success" } else { "error" }
        $status = if ($result.Failed -eq 0) { "✓ All Passed" } else { "✗ $($result.Failed) Failed" }

        $html += @"
        <div class="test-suite">
            <div class="test-suite-header $headerClass">
                <span>$suite</span>
                <span class="badge $badgeClass">$($result.Passed)/$($result.Total) Passed</span>
            </div>
            <div class="test-suite-body">
                $status
            </div>
        </div>
"@
    }

    $html += @"
    </div>
</body>
</html>
"@

    $html | Out-File -FilePath $Path -Encoding UTF8
}

# =====================================================
# START EXECUTION
# =====================================================

if ($Continuous) {
    Write-TestLog "Starting continuous testing mode (every $IntervalHours hours)..." -Level "INFO"

    while ($true) {
        Start-TestExecution

        Write-TestLog "Waiting $IntervalHours hours until next test run..." -Level "INFO"
        Start-Sleep -Seconds ($IntervalHours * 3600)
    }
}
else {
    Start-TestExecution
}
