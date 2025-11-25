# =====================================================
# LOAD & PERFORMANCE TESTING SUITE
# Tests all modules under concurrent load
# Focuses on: User search, Information loading, Assignment
# =====================================================

$ErrorActionPreference = "Continue"
$Global:BaseUrl = "http://localhost:5058/api"
$Global:Token = $null
$Global:CompanyId = $null
$Global:PerformanceResults = @()

function Write-TestLog {
    param([string]$Message, [string]$Level = "INFO")
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $color = switch($Level) {
        "SUCCESS" { "Green" }
        "ERROR" { "Red" }
        "WARNING" { "Yellow" }
        "SLOW" { "Magenta" }
        default { "Cyan" }
    }
    Write-Host "[$timestamp] $Message" -ForegroundColor $color
}

function Measure-APICall {
    param([string]$Method, [string]$Endpoint, [object]$Body = $null, [string]$TestName)

    $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

    try {
        $headers = @{"Content-Type" = "application/json"}
        if ($Global:Token) {
            $headers["Authorization"] = "Bearer $Global:Token"
        }

        $params = @{
            Uri = "$Global:BaseUrl/$Endpoint"
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
            TimeoutSec = 60
        }

        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
        }

        $response = Invoke-RestMethod @params
        $stopwatch.Stop()

        $result = @{
            TestName = $TestName
            Endpoint = $Endpoint
            Method = $Method
            ResponseTime = $stopwatch.ElapsedMilliseconds
            Status = "SUCCESS"
            Timestamp = Get-Date
        }

        # Categorize performance
        if ($stopwatch.ElapsedMilliseconds -lt 500) {
            $performance = "EXCELLENT"
            $color = "Green"
        }
        elseif ($stopwatch.ElapsedMilliseconds -lt 1000) {
            $performance = "GOOD"
            $color = "Cyan"
        }
        elseif ($stopwatch.ElapsedMilliseconds -lt 2000) {
            $performance = "ACCEPTABLE"
            $color = "Yellow"
        }
        else {
            $performance = "SLOW"
            $color = "Red"
        }

        Write-TestLog "$TestName - ${performance} ($($stopwatch.ElapsedMilliseconds)ms)" -Level $performance

        $Global:PerformanceResults += $result
        return @{Success = $true; Time = $stopwatch.ElapsedMilliseconds; Data = $response}
    }
    catch {
        $stopwatch.Stop()
        Write-TestLog "$TestName - FAILED after $($stopwatch.ElapsedMilliseconds)ms: $_" -Level "ERROR"

        $result = @{
            TestName = $TestName
            Endpoint = $Endpoint
            Method = $Method
            ResponseTime = $stopwatch.ElapsedMilliseconds
            Status = "FAILED"
            Error = $_.Exception.Message
            Timestamp = Get-Date
        }
        $Global:PerformanceResults += $result
        return @{Success = $false; Time = $stopwatch.ElapsedMilliseconds}
    }
}

function Get-AuthToken {
    Write-TestLog "Authenticating..." -Level "INFO"

    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    }

    $result = Measure-APICall -Method "POST" -Endpoint "auth/login" -Body $loginBody -TestName "Authentication"

    if ($result.Success) {
        $Global:Token = $result.Data.data.token
        $Global:CompanyId = $result.Data.data.companyId
        Write-TestLog "Authentication successful in $($result.Time)ms" -Level "SUCCESS"
        return $true
    }

    return $false
}

# =====================================================
# LOAD TEST 1: USER SEARCH PERFORMANCE
# =====================================================

function Test-UserSearchPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 1: USER SEARCH PERFORMANCE" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    $searchTerms = @("admin", "user", "test", "a", "system", "manager", "emp", "john", "jane", "support")

    Write-TestLog "Testing user search with various terms..." -Level "INFO"

    foreach ($term in $searchTerms) {
        # Single search
        $result = Measure-APICall -Method "GET" -Endpoint "users/search?searchTerm=$term&limit=10" -TestName "User Search: '$term'"
        Start-Sleep -Milliseconds 100

        # Concurrent searches (simulate multiple users)
        Write-TestLog "  Simulating 5 concurrent searches for '$term'..." -Level "INFO"

        $jobs = @()
        for ($i = 1; $i -le 5; $i++) {
            $jobs += Start-Job -ScriptBlock {
                param($url, $token, $term)
                $headers = @{
                    "Authorization" = "Bearer $token"
                    "Content-Type" = "application/json"
                }
                $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
                try {
                    Invoke-RestMethod -Uri "$url/users/search?searchTerm=$term&limit=10" -Headers $headers -Method GET -UseBasicParsing | Out-Null
                    $stopwatch.Stop()
                    return $stopwatch.ElapsedMilliseconds
                }
                catch {
                    $stopwatch.Stop()
                    return -1
                }
            } -ArgumentList $Global:BaseUrl, $Global:Token, $term
        }

        $results = $jobs | Wait-Job | Receive-Job
        $jobs | Remove-Job

        $avgTime = ($results | Where-Object { $_ -gt 0 } | Measure-Object -Average).Average
        if ($avgTime -gt 0) {
            Write-TestLog "  Concurrent avg: $([math]::Round($avgTime, 0))ms" -Level $(if($avgTime -lt 1000){"SUCCESS"}else{"SLOW"})
        }
    }

    # Stress test: 20 concurrent searches
    Write-TestLog "`nStress test: 20 concurrent user searches..." -Level "INFO"
    $stressJobs = @()
    for ($i = 1; $i -le 20; $i++) {
        $term = $searchTerms[($i % $searchTerms.Count)]
        $stressJobs += Start-Job -ScriptBlock {
            param($url, $token, $term)
            $headers = @{
                "Authorization" = "Bearer $token"
                "Content-Type" = "application/json"
            }
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            try {
                Invoke-RestMethod -Uri "$url/users/search?searchTerm=$term&limit=10" -Headers $headers -Method GET -UseBasicParsing | Out-Null
                $stopwatch.Stop()
                return $stopwatch.ElapsedMilliseconds
            }
            catch {
                $stopwatch.Stop()
                return -1
            }
        } -ArgumentList $Global:BaseUrl, $Global:Token, $term
    }

    $stressResults = $stressJobs | Wait-Job | Receive-Job
    $stressJobs | Remove-Job

    $stressAvg = ($stressResults | Where-Object { $_ -gt 0 } | Measure-Object -Average).Average
    $stressMax = ($stressResults | Where-Object { $_ -gt 0 } | Measure-Object -Maximum).Maximum
    $stressMin = ($stressResults | Where-Object { $_ -gt 0 } | Measure-Object -Minimum).Minimum

    Write-TestLog "Stress test results:" -Level "INFO"
    Write-TestLog "  Min: $([math]::Round($stressMin, 0))ms" -Level "SUCCESS"
    Write-TestLog "  Avg: $([math]::Round($stressAvg, 0))ms" -Level $(if($stressAvg -lt 2000){"SUCCESS"}else{"SLOW"})
    Write-TestLog "  Max: $([math]::Round($stressMax, 0))ms" -Level $(if($stressMax -lt 5000){"WARNING"}else{"SLOW"})
}

# =====================================================
# LOAD TEST 2: COMPLAINT INFORMATION LOADING
# =====================================================

function Test-ComplaintLoadingPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 2: COMPLAINT LOADING" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    # Test list loading with different page sizes
    $pageSizes = @(10, 20, 50, 100)

    foreach ($size in $pageSizes) {
        $result = Measure-APICall -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=$size" -TestName "Load $size complaints"
        Start-Sleep -Milliseconds 100
    }

    # Test individual complaint loading
    Write-TestLog "`nTesting individual complaint loading..." -Level "INFO"
    $complaints = (Measure-APICall -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=10" -TestName "Get complaints for detail test").Data.data

    if ($complaints -and $complaints.Count -gt 0) {
        foreach ($complaint in ($complaints | Select-Object -First 5)) {
            $result = Measure-APICall -Method "GET" -Endpoint "complaints/$($complaint.id)" -TestName "Load complaint details: $($complaint.complaintNumber)"
            Start-Sleep -Milliseconds 50
        }
    }

    # Concurrent complaint loading
    Write-TestLog "`nStress test: 10 concurrent complaint list loads..." -Level "INFO"
    $jobs = @()
    for ($i = 1; $i -le 10; $i++) {
        $jobs += Start-Job -ScriptBlock {
            param($url, $token)
            $headers = @{
                "Authorization" = "Bearer $token"
                "Content-Type" = "application/json"
            }
            $stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
            try {
                Invoke-RestMethod -Uri "$url/complaints?pageNumber=1&pageSize=20" -Headers $headers -Method GET -UseBasicParsing | Out-Null
                $stopwatch.Stop()
                return $stopwatch.ElapsedMilliseconds
            }
            catch {
                $stopwatch.Stop()
                return -1
            }
        } -ArgumentList $Global:BaseUrl, $Global:Token
    }

    $concurrentResults = $jobs | Wait-Job | Receive-Job
    $jobs | Remove-Job

    $concurrentAvg = ($concurrentResults | Where-Object { $_ -gt 0 } | Measure-Object -Average).Average
    Write-TestLog "Concurrent load avg: $([math]::Round($concurrentAvg, 0))ms" -Level $(if($concurrentAvg -lt 2000){"SUCCESS"}else{"SLOW"})
}

# =====================================================
# LOAD TEST 3: TICKET ASSIGNMENT PERFORMANCE
# =====================================================

function Test-AssignmentPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 3: TICKET ASSIGNMENT" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    # Get complaints for assignment
    $complaints = (Measure-APICall -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=20" -TestName "Get complaints for assignment").Data.data

    if ($complaints -and $complaints.Count -gt 0) {
        Write-TestLog "Testing assignment workflow..." -Level "INFO"

        foreach ($complaint in ($complaints | Select-Object -First 5)) {
            # Step 1: Search for user to assign
            $searchResult = Measure-APICall -Method "GET" -Endpoint "users/search?searchTerm=admin&limit=5" -TestName "Search user for assignment"

            if ($searchResult.Success) {
                Start-Sleep -Milliseconds 100

                # Step 2: Load full complaint details
                $detailResult = Measure-APICall -Method "GET" -Endpoint "complaints/$($complaint.id)" -TestName "Load complaint before assignment"

                if ($detailResult.Success) {
                    $fullComplaint = $detailResult.Data.data

                    # Convert priority
                    $priorityValue = switch($fullComplaint.priority) {
                        "Low" { 0 }; "Normal" { 1 }; "High" { 2 }; "Critical" { 3 }; "Urgent" { 4 }
                        default { 1 }
                    }

                    # Step 3: Perform assignment (update complaint)
                    $updateData = @{
                        id = $fullComplaint.id
                        title = $fullComplaint.title
                        description = $fullComplaint.description
                        categoryId = $fullComplaint.categoryId
                        priority = $priorityValue
                        status = 1  # Under Review
                        assignedToId = $fullComplaint.assignedToId
                        resolutionNotes = "Load test assignment"
                        tags = $fullComplaint.tags
                    }

                    $assignResult = Measure-APICall -Method "PUT" -Endpoint "complaints/$($complaint.id)" -Body $updateData -TestName "Assign complaint: $($complaint.complaintNumber)"
                }
            }

            Start-Sleep -Milliseconds 200
        }
    }
}

# =====================================================
# LOAD TEST 4: DASHBOARD PERFORMANCE
# =====================================================

function Test-DashboardPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 4: DASHBOARD LOADING" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    $dateRanges = @(7, 30, 90, 180, 365)

    foreach ($days in $dateRanges) {
        $result = Measure-APICall -Method "GET" -Endpoint "dashboard/stats?days=$days" -TestName "Dashboard stats: $days days"
        Start-Sleep -Milliseconds 100
    }

    # Test preferences loading
    $result = Measure-APICall -Method "GET" -Endpoint "dashboard/preferences" -TestName "Load dashboard preferences"
}

# =====================================================
# LOAD TEST 5: SEARCH & FILTER PERFORMANCE
# =====================================================

function Test-SearchFilterPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 5: SEARCH & FILTER" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    # Test search
    $searchTerms = @("printer", "delivery", "urgent", "issue", "problem")

    foreach ($term in $searchTerms) {
        $result = Measure-APICall -Method "GET" -Endpoint "complaints?searchTerm=$term&pageNumber=1&pageSize=20" -TestName "Search: '$term'"
        Start-Sleep -Milliseconds 100
    }

    # Test category filters
    $categories = (Measure-APICall -Method "GET" -Endpoint "categories" -TestName "Load categories").Data.data

    if ($categories -and $categories.Count -gt 0) {
        foreach ($category in ($categories | Select-Object -First 5)) {
            $result = Measure-APICall -Method "GET" -Endpoint "complaints?categoryId=$($category.id)&pageNumber=1&pageSize=20" -TestName "Filter by: $($category.name)"
            Start-Sleep -Milliseconds 100
        }
    }

    # Combined search + filter
    Write-TestLog "`nTesting combined search + filter..." -Level "INFO"
    $result = Measure-APICall -Method "GET" -Endpoint "complaints?searchTerm=issue&categoryId=$($categories[0].id)&pageNumber=1&pageSize=20" -TestName "Combined search + filter"
}

# =====================================================
# LOAD TEST 6: COMMENT SYSTEM PERFORMANCE
# =====================================================

function Test-CommentPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 6: COMMENT SYSTEM" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    $complaints = (Measure-APICall -Method "GET" -Endpoint "complaints?pageNumber=1&pageSize=10" -TestName "Get complaints for comments").Data.data

    if ($complaints -and $complaints.Count -gt 0) {
        foreach ($complaint in ($complaints | Select-Object -First 5)) {
            # Load existing comments
            $loadResult = Measure-APICall -Method "GET" -Endpoint "complaints/$($complaint.id)/comments" -TestName "Load comments: $($complaint.complaintNumber)"

            Start-Sleep -Milliseconds 100

            # Add new comment
            $commentData = @{
                comment = "Load test comment - $(Get-Date -Format 'HHmmss')"
                isInternal = $false
            }

            $addResult = Measure-APICall -Method "POST" -Endpoint "complaints/$($complaint.id)/comments" -Body $commentData -TestName "Add comment: $($complaint.complaintNumber)"

            Start-Sleep -Milliseconds 100
        }
    }
}

# =====================================================
# LOAD TEST 7: MASTER DATA LOADING
# =====================================================

function Test-MasterDataPerformance {
    Write-TestLog "`n========================================" -Level "INFO"
    Write-TestLog "LOAD TEST 7: MASTER DATA LOADING" -Level "INFO"
    Write-TestLog "========================================" -Level "INFO"

    Measure-APICall -Method "GET" -Endpoint "categories" -TestName "Load all categories"
    Start-Sleep -Milliseconds 100

    Measure-APICall -Method "GET" -Endpoint "ComplaintStatusMaster?includeSystem=true" -TestName "Load all status masters"
    Start-Sleep -Milliseconds 100

    Measure-APICall -Method "GET" -Endpoint "ComplaintPriorityMaster?includeSystem=true" -TestName "Load all priority masters"
    Start-Sleep -Milliseconds 100

    Measure-APICall -Method "GET" -Endpoint "branches?companyId=$Global:CompanyId" -TestName "Load all branches"
    Start-Sleep -Milliseconds 100
}

# =====================================================
# GENERATE PERFORMANCE REPORT
# =====================================================

function Generate-PerformanceReport {
    Write-Host "`n"
    Write-Host "╔═══════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                                                               ║" -ForegroundColor Cyan
    Write-Host "║          LOAD & PERFORMANCE TEST RESULTS                      ║" -ForegroundColor Cyan
    Write-Host "║                                                               ║" -ForegroundColor Cyan
    Write-Host "╚═══════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host "`n"

    # Categorize results
    $excellent = $Global:PerformanceResults | Where-Object { $_.ResponseTime -lt 500 -and $_.Status -eq "SUCCESS" }
    $good = $Global:PerformanceResults | Where-Object { $_.ResponseTime -ge 500 -and $_.ResponseTime -lt 1000 -and $_.Status -eq "SUCCESS" }
    $acceptable = $Global:PerformanceResults | Where-Object { $_.ResponseTime -ge 1000 -and $_.ResponseTime -lt 2000 -and $_.Status -eq "SUCCESS" }
    $slow = $Global:PerformanceResults | Where-Object { $_.ResponseTime -ge 2000 -and $_.Status -eq "SUCCESS" }
    $failed = $Global:PerformanceResults | Where-Object { $_.Status -eq "FAILED" }

    Write-Host "Performance Summary:" -ForegroundColor Cyan
    Write-Host "  Total Tests:       $($Global:PerformanceResults.Count)" -ForegroundColor White
    Write-Host ("  Excellent (<500ms): " + $excellent.Count) -ForegroundColor Green
    Write-Host ("  Good (500ms to 1s):  " + $good.Count) -ForegroundColor Cyan
    Write-Host ("  Acceptable (1s to 2s):  " + $acceptable.Count) -ForegroundColor Yellow
    Write-Host ("  Slow (over 2s):         " + $slow.Count) -ForegroundColor $(if($slow.Count -eq 0){"Green"}else{"Red"})
    Write-Host ("  Failed:             " + $failed.Count) -ForegroundColor $(if($failed.Count -eq 0){"Green"}else{"Red"})
    Write-Host "`n"

    # Calculate averages
    $avgTime = ($Global:PerformanceResults | Where-Object { $_.Status -eq "SUCCESS" } | Measure-Object -Property ResponseTime -Average).Average
    $maxTime = ($Global:PerformanceResults | Where-Object { $_.Status -eq "SUCCESS" } | Measure-Object -Property ResponseTime -Maximum).Maximum
    $minTime = ($Global:PerformanceResults | Where-Object { $_.Status -eq "SUCCESS" } | Measure-Object -Property ResponseTime -Minimum).Minimum

    Write-Host "Response Time Statistics:" -ForegroundColor Cyan
    Write-Host "  Average: $([math]::Round($avgTime, 0))ms" -ForegroundColor $(if($avgTime -lt 1000){"Green"}else{"Yellow"})
    Write-Host "  Minimum: $([math]::Round($minTime, 0))ms" -ForegroundColor Green
    Write-Host "  Maximum: $([math]::Round($maxTime, 0))ms" -ForegroundColor $(if($maxTime -lt 2000){"Yellow"}else{"Red"})
    Write-Host "`n"

    # Identify slow endpoints
    if ($slow.Count -gt 0) {
        Write-Host "⚠ SLOW ENDPOINTS (>2s):" -ForegroundColor Red
        foreach ($test in $slow) {
            Write-Host "  - $($test.TestName): $($test.ResponseTime)ms" -ForegroundColor Red
            Write-Host "    Endpoint: $($test.Method) $($test.Endpoint)" -ForegroundColor Gray
        }
        Write-Host "`n"
    }

    # Performance rating
    $excellentPercent = if($Global:PerformanceResults.Count -gt 0){[math]::Round(($excellent.Count / $Global:PerformanceResults.Count) * 100, 1)}else{0}
    $slowPercent = if($Global:PerformanceResults.Count -gt 0){[math]::Round(($slow.Count / $Global:PerformanceResults.Count) * 100, 1)}else{0}

    Write-Host "Overall Performance Rating:" -ForegroundColor Cyan
    if ($excellentPercent -ge 80) {
        Write-Host ("  FIVE STAR EXCELLENT (" + $excellentPercent + "% under 500ms)") -ForegroundColor Green
    }
    elseif ($excellentPercent -ge 60) {
        Write-Host ("  FOUR STAR GOOD (" + $excellentPercent + "% under 500ms)") -ForegroundColor Cyan
    }
    elseif ($slowPercent -lt 10) {
        Write-Host ("  THREE STAR ACCEPTABLE (Only " + $slowPercent + "% slow)") -ForegroundColor Yellow
    }
    else {
        Write-Host ("  TWO STAR NEEDS OPTIMIZATION (" + $slowPercent + "% slow requests)") -ForegroundColor Red
    }
    Write-Host "`n"

    # Save detailed report
    $reportFile = "LOAD_TEST_RESULTS_$(Get-Date -Format 'yyyyMMdd_HHmmss').json"
    $Global:PerformanceResults | ConvertTo-Json -Depth 5 | Out-File $reportFile
    Write-Host "Detailed results saved to: $reportFile" -ForegroundColor Green
    Write-Host "`n"
}

# =====================================================
# MAIN EXECUTION
# =====================================================

Write-Host "`n"
Write-Host "╔══════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║                                                          ║" -ForegroundColor Cyan
Write-Host "║     LOAD & PERFORMANCE TESTING SUITE                    ║" -ForegroundColor Cyan
Write-Host "║     Testing slowness and bottlenecks                    ║" -ForegroundColor Cyan
Write-Host "║                                                          ║" -ForegroundColor Cyan
Write-Host "╚══════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host "`n"

$startTime = Get-Date

if (-not (Get-AuthToken)) {
    Write-TestLog "Cannot proceed without authentication" -Level "ERROR"
    exit
}

# Execute all load tests
Test-UserSearchPerformance
Test-ComplaintLoadingPerformance
Test-AssignmentPerformance
Test-DashboardPerformance
Test-SearchFilterPerformance
Test-CommentPerformance
Test-MasterDataPerformance

$endTime = Get-Date
$duration = $endTime - $startTime

Write-Host "`n"
Write-Host "Load testing completed in $($duration.ToString('mm\:ss'))" -ForegroundColor Green

# Generate comprehensive report
Generate-PerformanceReport

Write-Host "All load tests completed!" -ForegroundColor Green
Write-Host "`n"
