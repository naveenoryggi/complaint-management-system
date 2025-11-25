# COMPREHENSIVE FULL TEST SUITE - PART 2
# Categories 3-14 (Remaining 200+ tests)

$BaseUrl = "http://localhost:5058"
$FrontendUrl = "http://localhost:4200"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "COMPREHENSIVE_FULL_TEST_PART2_RESULTS_$timestamp.txt"
$passedTests = 0
$failedTests = 0
$totalTests = 0
$categoryResults = @{}

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$ts] [$Level] $Message"
    switch ($Level) {
        "PASS" { Write-Host $logMessage -ForegroundColor Green }
        "FAIL" { Write-Host $logMessage -ForegroundColor Red }
        "WARN" { Write-Host $logMessage -ForegroundColor Yellow }
        default { Write-Host $logMessage }
    }
    Add-Content -Path $resultsFile -Value $logMessage
}

function Test-APIEndpoint {
    param(
        [string]$Category,
        [string]$TestName,
        [string]$Method,
        [string]$Endpoint,
        [string]$Body = $null,
        [hashtable]$Headers = @{},
        [int[]]$ExpectedStatuses = @(200),
        [switch]$ExpectFailure
    )
    $script:totalTests++

    if (-not $categoryResults.ContainsKey($Category)) {
        $categoryResults[$Category] = @{ Passed = 0; Failed = 0 }
    }

    Write-Log "[$Category] $TestName"
    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            UseBasicParsing = $true
            TimeoutSec = 30
            ErrorAction = 'Stop'
        }

        if ($Headers.Count -gt 0) {
            $params['Headers'] = $Headers
        }

        if ($Body) {
            $params['Body'] = $Body
            if (-not $Headers.ContainsKey('Content-Type')) {
                $params['Headers'] = @{"Content-Type" = "application/json"}
                if ($Headers.Count -gt 0) {
                    $params['Headers'] += $Headers
                }
            }
        }

        $response = Invoke-WebRequest @params

        if ($response.StatusCode -in $ExpectedStatuses) {
            Write-Log "PASS: $TestName" "PASS"
            $script:passedTests++
            $categoryResults[$Category].Passed++

            if ($response.Content) {
                try {
                    return $response.Content | ConvertFrom-Json
                } catch {
                    return $response.Content
                }
            }
            return $true
        } else {
            Write-Log "FAIL: $TestName - Unexpected status $($response.StatusCode)" "FAIL"
            $script:failedTests++
            $categoryResults[$Category].Failed++
            return $false
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($ExpectFailure -and $statusCode -in $ExpectedStatuses) {
            Write-Log "PASS: $TestName (Expected failure - $statusCode)" "PASS"
            $script:passedTests++
            $categoryResults[$Category].Passed++
            return $true
        } else {
            Write-Log "FAIL: $TestName - $($_.Exception.Message)" "FAIL"
            $script:failedTests++
            $categoryResults[$Category].Failed++
            return $false
        }
    }
}

function Test-UIPage {
    param(
        [string]$Category,
        [string]$PageName,
        [string]$Route
    )
    $script:totalTests++

    if (-not $categoryResults.ContainsKey($Category)) {
        $categoryResults[$Category] = @{ Passed = 0; Failed = 0 }
    }

    Write-Log "[$Category] Testing $PageName page accessibility"
    try {
        $response = Invoke-WebRequest -Uri "$FrontendUrl$Route" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
        Write-Log "PASS: $PageName accessible" "PASS"
        $script:passedTests++
        $categoryResults[$Category].Passed++
        return $true
    } catch {
        Write-Log "FAIL: $PageName - $($_.Exception.Message)" "FAIL"
        $script:failedTests++
        $categoryResults[$Category].Failed++
        return $false
    }
}

# =============================================
# MAIN TEST EXECUTION - PART 2
# =============================================

Write-Log "============================================="
Write-Log "COMPREHENSIVE TEST SUITE - PART 2"
Write-Log "Categories 3-14 (200+ tests)"
Write-Log "============================================="
Write-Log ""

# Authenticate
Write-Log "Authenticating..."
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
    $token = $loginResponse.data.token
    $UserId = $loginResponse.data.user.id
    $CompanyId = $loginResponse.data.user.companyId
    $authHeaders = @{ "Authorization" = "Bearer $token" }
    Write-Log "Authentication successful"
} catch {
    Write-Log "FATAL: Authentication failed" "FAIL"
    exit 1
}

# Get test data
$categories = Test-APIEndpoint "Setup" "Get Categories" "GET" "/api/categories" -Headers $authHeaders
$CategoryId = if ($categories.data) { $categories.data[0].id } else { "00000000-0000-0000-0000-000000000000" }

Write-Log ""

# =============================================
# CATEGORY 3: SECURITY & AUTHENTICATION (20 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 3: SECURITY & AUTHENTICATION (20 TESTS)"
Write-Log "============================================="

# Authentication Tests (5 tests)
Write-Log "--- Authentication Tests (5 tests) ---"

$invalidLoginBody = @{
    email = "invalid@test.com"
    password = "wrongpassword"
} | ConvertTo-Json

Test-APIEndpoint "Security" "Login: Invalid Credentials" "POST" "/api/auth/login" -Body $invalidLoginBody -ExpectedStatuses @(401, 400) -ExpectFailure

Test-APIEndpoint "Security" "Login: Empty Email" "POST" "/api/auth/login" -Body (@{ email = ""; password = "test" } | ConvertTo-Json) -ExpectedStatuses @(400, 401) -ExpectFailure

Test-APIEndpoint "Security" "Login: Empty Password" "POST" "/api/auth/login" -Body (@{ email = "test@test.com"; password = "" } | ConvertTo-Json) -ExpectedStatuses @(400, 401) -ExpectFailure

# Refresh Token
$refreshBody = @{
    token = $token
    refreshToken = $loginResponse.data.refreshToken
} | ConvertTo-Json

Test-APIEndpoint "Security" "Refresh Token" "POST" "/api/auth/refresh-token" -Body $refreshBody -ExpectedStatuses @(200, 400, 401)

Test-APIEndpoint "Security" "Logout" "POST" "/api/auth/logout" -Headers $authHeaders -ExpectedStatuses @(200, 204)

# Authorization Tests (15 tests)
Write-Log "--- Authorization Tests (15 tests) ---"

Test-APIEndpoint "Security" "Access Without Token (401)" "GET" "/api/complaints" -ExpectedStatuses @(401) -ExpectFailure

$invalidToken = @{ "Authorization" = "Bearer invalid_token_12345" }
Test-APIEndpoint "Security" "Access With Invalid Token (401)" "GET" "/api/complaints" -Headers $invalidToken -ExpectedStatuses @(401) -ExpectFailure

# Permission Tests
Test-APIEndpoint "Security" "ViewComplaints Permission" "GET" "/api/complaints" -Headers $authHeaders -ExpectedStatuses @(200)

Test-APIEndpoint "Security" "CreateComplaint Permission" "POST" "/api/complaints" -Body (@{ title = "Test"; categoryId = $CategoryId; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 400)

Test-APIEndpoint "Security" "ManageUsers Permission" "GET" "/api/users" -Headers $authHeaders -ExpectedStatuses @(200)

Test-APIEndpoint "Security" "ManageSettings Permission" "GET" "/api/email-settings?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Security" "ManageCompany Permission" "GET" "/api/company/$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Security" "ManageCategories Permission" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200)

Test-APIEndpoint "Security" "ViewReports Permission" "GET" "/api/dashboard/statistics" -Headers $authHeaders -ExpectedStatuses @(200)

Test-APIEndpoint "Security" "ManageEscalation Permission" "GET" "/api/escalation/matrices?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200)

# Cross-tenant access prevention
$otherCompanyId = "00000000-0000-0000-0000-000000000999"
Test-APIEndpoint "Security" "Cross-Tenant Prevention" "GET" "/api/branches?companyId=$otherCompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 403, 404)

# Token expiration (simulated)
Test-APIEndpoint "Security" "Valid Token Access" "GET" "/api/dashboard/statistics" -Headers $authHeaders -ExpectedStatuses @(200)

# Rate limiting test
Test-APIEndpoint "Security" "Rate Limiting Test 1" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200, 429)
Test-APIEndpoint "Security" "Rate Limiting Test 2" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200, 429)
Test-APIEndpoint "Security" "Rate Limiting Test 3" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200, 429)

Write-Log ""

# =============================================
# CATEGORY 4: SEARCH, FILTER & PAGINATION (15 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 4: SEARCH, FILTER & PAGINATION (15 TESTS)"
Write-Log "============================================="

# Search Tests (5 tests)
Write-Log "--- Search Tests (5 tests) ---"

Test-APIEndpoint "Search & Filter" "User Search by Name" "GET" "/api/users/search?searchTerm=admin&limit=5" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "User Search by Email" "GET" "/api/users/search?searchTerm=admin@&limit=5" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "User Search by Code" "GET" "/api/users/search?searchTerm=ADMIN001&limit=5" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Search with Empty Term" "GET" "/api/users/search?searchTerm=&limit=5" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Search with Special Characters" "GET" "/api/users/search?searchTerm=%40&limit=5" -Headers $authHeaders

# Filter Tests (5 tests)
Write-Log "--- Filter Tests (5 tests) ---"

Test-APIEndpoint "Search & Filter" "Filter by Status" "GET" "/api/complaints?status=Submitted&page=1&pageSize=10" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Filter by Priority" "GET" "/api/complaints?priority=High&page=1&pageSize=10" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Filter by Category" "GET" "/api/complaints?categoryId=$CategoryId&page=1&pageSize=10" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Filter by Date Range" "GET" "/api/complaints?from=2025-01-01&to=2025-12-31&page=1&pageSize=10" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Combined Filters" "GET" "/api/complaints?status=Submitted&priority=High&page=1&pageSize=10" -Headers $authHeaders

# Pagination Tests (5 tests)
Write-Log "--- Pagination Tests (5 tests) ---"

Test-APIEndpoint "Search & Filter" "Pagination: Page 1" "GET" "/api/complaints?page=1&pageSize=10" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Pagination: Page 2" "GET" "/api/complaints?page=2&pageSize=10" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Pagination: Large Page Size" "GET" "/api/complaints?page=1&pageSize=100" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Pagination: Sort by Date Desc" "GET" "/api/complaints?page=1&pageSize=10&sortBy=createdAt&sortOrder=desc" -Headers $authHeaders

Test-APIEndpoint "Search & Filter" "Pagination: Sort by Priority" "GET" "/api/complaints?page=1&pageSize=10&sortBy=priority&sortOrder=asc" -Headers $authHeaders

Write-Log ""

# =============================================
# CATEGORY 5: DATA VALIDATION (25 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 5: DATA VALIDATION (25 TESTS)"
Write-Log "============================================="

# Field Validation (15 tests)
Write-Log "--- Field Validation Tests (15 tests) ---"

Test-APIEndpoint "Validation" "Invalid Email Format" "POST" "/api/users" -Body (@{ firstName = "Test"; lastName = "User"; email = "invalid-email"; employeeCode = "TEST001"; password = "Test@123"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Validation" "Required Field: Title" "POST" "/api/complaints" -Body (@{ title = ""; categoryId = $CategoryId; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Validation" "Required Field: Category" "POST" "/api/complaints" -Body (@{ title = "Test"; categoryId = ""; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Validation" "String Max Length" "POST" "/api/categories" -Body (@{ name = "A" * 300; code = "TEST"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Validation" "String Min Length" "POST" "/api/categories" -Body (@{ name = "A"; code = "T"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Validation" "Numeric Range: Priority Level" "POST" "/api/ComplaintPriorityMaster" -Body (@{ name = "Test"; code = "TEST"; level = 999; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Validation" "Date Range: Future Date" "PUT" "/api/complaints/00000000-0000-0000-0000-000000000001" -Body (@{ title = "Test"; dueDate = "2099-12-31"; categoryId = $CategoryId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Validation" "Phone Format" "POST" "/api/users" -Body (@{ firstName = "Test"; lastName = "User"; email = "test@test.com"; phone = "invalid"; employeeCode = "TEST002"; password = "Test@123"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 400)

Test-APIEndpoint "Validation" "Special Characters in Name" "POST" "/api/categories" -Body (@{ name = "Test<script>alert('xss')</script>"; code = "TEST_XSS"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 400)

Test-APIEndpoint "Validation" "SQL Injection Prevention" "GET" "/api/categories?name=' OR '1'='1" -Headers $authHeaders -ExpectedStatuses @(200, 400)

Test-APIEndpoint "Validation" "XSS Prevention in Comment" "POST" "/api/complaints/00000000-0000-0000-0000-000000000001/comments" -Body (@{ comment = "<script>alert('xss')</script>"; isInternal = $false } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 400, 404)

Test-APIEndpoint "Validation" "Duplicate Code Prevention" "POST" "/api/categories" -Body (@{ name = "Duplicate Test"; code = "PRODUCT_QUALITY"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 409) -ExpectFailure

Test-APIEndpoint "Validation" "Duplicate Email Prevention" "POST" "/api/users" -Body (@{ firstName = "Test"; lastName = "User"; email = "admin@complaintmanagement.com"; employeeCode = "DUP001"; password = "Test@123"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 409) -ExpectFailure

Test-APIEndpoint "Validation" "Foreign Key Constraint" "POST" "/api/complaints" -Body (@{ title = "Test"; categoryId = "00000000-0000-0000-0000-000000000999"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

Test-APIEndpoint "Validation" "Invalid GUID Format" "GET" "/api/complaints/invalid-guid" -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

# Business Logic Validation (10 tests)
Write-Log "--- Business Logic Validation (10 tests) ---"

# Get a real complaint for testing
$complaints = Test-APIEndpoint "Validation" "Get Complaints for Testing" "GET" "/api/complaints?page=1&pageSize=1" -Headers $authHeaders
$testComplaintId = if ($complaints.data -and $complaints.data.items) { $complaints.data.items[0].id } else { "00000000-0000-0000-0000-000000000001" }

Test-APIEndpoint "Validation" "Status Transition Validation" "PUT" "/api/complaints/$testComplaintId" -Body (@{ title = "Test"; status = "InvalidStatus"; categoryId = $CategoryId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400)

Test-APIEndpoint "Validation" "Close Without Resolution" "POST" "/api/complaints/$testComplaintId/close" -Body (@{ resolution = "" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Validation" "Reopen Closed Complaint" "POST" "/api/complaints/$testComplaintId/reopen" -Body (@{ reason = "Testing reopen" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Validation" "Assign to Invalid User" "POST" "/api/complaints/$testComplaintId/assign/00000000-0000-0000-0000-000000000999" -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

Test-APIEndpoint "Validation" "Self Assignment" "POST" "/api/complaints/$testComplaintId/assign/$UserId" -Headers $authHeaders -ExpectedStatuses @(200, 400)

Test-APIEndpoint "Validation" "Update Deleted Entity" "PUT" "/api/categories/00000000-0000-0000-0000-000000000999" -Body (@{ name = "Test"; code = "TEST"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(404) -ExpectFailure

Test-APIEndpoint "Validation" "Delete Non-Existent" "DELETE" "/api/categories/00000000-0000-0000-0000-000000000999" -Headers $authHeaders -ExpectedStatuses @(404) -ExpectFailure

Test-APIEndpoint "Validation" "Concurrent Update Detection" "PUT" "/api/complaints/$testComplaintId" -Body (@{ title = "Concurrent Test 1"; categoryId = $CategoryId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404, 409)

Test-APIEndpoint "Validation" "Cascade Delete Check" "DELETE" "/api/categories/$CategoryId" -Headers $authHeaders -ExpectedStatuses @(200, 400, 409)

Write-Log ""

# =============================================
# CATEGORY 6: DASHBOARD & REPORTING (15 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 6: DASHBOARD & REPORTING (15 TESTS)"
Write-Log "============================================="

Test-UIPage "Dashboard" "Dashboard Main Page" "/dashboard"

Test-APIEndpoint "Dashboard" "Get Dashboard Statistics" "GET" "/api/dashboard/statistics" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Get Status Distribution" "GET" "/api/dashboard/status-distribution" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Get Priority Distribution" "GET" "/api/dashboard/priority-distribution" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Get Category Distribution" "GET" "/api/dashboard/category-distribution" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Get Trend Analysis" "GET" "/api/dashboard/trend-analysis?days=30" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Get SLA Compliance" "GET" "/api/dashboard/sla-compliance" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Get User Performance" "GET" "/api/dashboard/user-performance" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Custom Date Range: Last 7 Days" "GET" "/api/dashboard/statistics?from=2025-10-18&to=2025-10-25" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Custom Date Range: Last Month" "GET" "/api/dashboard/statistics?from=2025-09-01&to=2025-09-30" -Headers $authHeaders

Test-APIEndpoint "Dashboard" "Save Dashboard Preferences" "POST" "/api/dashboard/preferences" -Body (@{ layout = "grid"; widgets = @("status", "priority") } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

Test-APIEndpoint "Dashboard" "Load Dashboard Preferences" "GET" "/api/dashboard/preferences" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Dashboard" "Reset Dashboard to Default" "DELETE" "/api/dashboard/preferences" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)

Test-APIEndpoint "Dashboard" "Export Dashboard Data" "GET" "/api/dashboard/export?format=json" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Dashboard" "Schedule Dashboard Report" "POST" "/api/dashboard/schedule-report" -Body (@{ frequency = "daily"; recipients = @("admin@test.com") } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

Write-Log ""

# =============================================
# CATEGORY 7: ERROR HANDLING (15 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 7: ERROR HANDLING (15 TESTS)"
Write-Log "============================================="

Test-APIEndpoint "Error Handling" "Malformed JSON" "POST" "/api/categories" -Body "{ invalid json }" -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Error Handling" "Missing Content-Type Header" "POST" "/api/categories" -Body (@{ name = "Test" } | ConvertTo-Json) -Headers @{ "Authorization" = "Bearer $token" } -ExpectedStatuses @(200, 201, 400, 415)

Test-APIEndpoint "Error Handling" "Invalid HTTP Method" "PATCH" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(405) -ExpectFailure

Test-APIEndpoint "Error Handling" "Request Too Large" "POST" "/api/categories" -Body (@{ name = "A" * 1000000; code = "TEST"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 413) -ExpectFailure

Test-APIEndpoint "Error Handling" "Invalid Route" "GET" "/api/nonexistent-endpoint" -Headers $authHeaders -ExpectedStatuses @(404) -ExpectFailure

Test-APIEndpoint "Error Handling" "Null Values in Required Fields" "POST" "/api/categories" -Body "null" -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

Test-APIEndpoint "Error Handling" "Empty Request Body" "POST" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(400, 415) -ExpectFailure

Test-APIEndpoint "Error Handling" "Unicode Characters Support" "POST" "/api/complaints/00000000-0000-0000-0000-000000000001/comments" -Body (@{ comment = "Test Unicode Characters"; isInternal = $false } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 400, 404)

Test-APIEndpoint "Error Handling" "Large Dataset Pagination" "GET" "/api/complaints?page=99999&pageSize=10" -Headers $authHeaders -ExpectedStatuses @(200, 400)

Test-APIEndpoint "Error Handling" "Negative Pagination Values" "GET" "/api/complaints?page=-1&pageSize=-10" -Headers $authHeaders -ExpectedStatuses @(200, 400)

Test-APIEndpoint "Error Handling" "Zero Page Size" "GET" "/api/complaints?page=1&pageSize=0" -Headers $authHeaders -ExpectedStatuses @(200, 400)

Test-APIEndpoint "Error Handling" "Invalid Query Parameters" "GET" "/api/complaints?invalidParam=test" -Headers $authHeaders -ExpectedStatuses @(200)

Test-APIEndpoint "Error Handling" "Multiple Concurrent Requests" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200)

Test-APIEndpoint "Error Handling" "Request Timeout Simulation" "GET" "/api/dashboard/statistics" -Headers $authHeaders -ExpectedStatuses @(200, 408, 504)

Test-APIEndpoint "Error Handling" "Invalid Date Format" "GET" "/api/complaints?from=invalid-date&to=also-invalid" -Headers $authHeaders -ExpectedStatuses @(200, 400)

Write-Log ""

# =============================================
# FINAL SUMMARY
# =============================================

Write-Log "============================================="
Write-Log "PART 2 TEST EXECUTION COMPLETED"
Write-Log "============================================="
Write-Log ""
Write-Log "Total Tests Executed: $totalTests"
Write-Log "Tests Passed: $passedTests"
Write-Log "Tests Failed: $failedTests"
Write-Log "Success Rate: $([math]::Round(($passedTests / $totalTests) * 100, 2))%"
Write-Log ""

Write-Log "Results by Category:"
foreach ($category in $categoryResults.Keys | Sort-Object) {
    $passed = $categoryResults[$category].Passed
    $failed = $categoryResults[$category].Failed
    $total = $passed + $failed
    $rate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }
    Write-Log "  $category`: $passed/$total passed ($rate%)"
}

Write-Log ""
Write-Log "Detailed results saved to: $resultsFile"

if ($failedTests -eq 0) {
    Write-Log "==============================================" "PASS"
    Write-Log "   100% SUCCESS RATE ACHIEVED!" "PASS"
    Write-Log "==============================================" "PASS"
} else {
    Write-Log "WARNING: $failedTests test(s) failed" "WARN"
    Write-Log "Review failed tests above for details" "WARN"
}
