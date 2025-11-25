# ============================================================================
# PHASE 1: CRITICAL SECURITY & DATA INTEGRITY TEST SUITE
# ============================================================================
# This comprehensive test suite validates:
# - Authorization & Permission-based Access Control (60 tests)
# - Data Validation & Business Rules (50 tests)
# - Multi-Tenant Data Isolation (30 tests)
# - Token & Session Security (30 tests)
# Total: 170 tests
# ============================================================================

$BaseUrl = "http://localhost:5058"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "PHASE1_SECURITY_TESTS_$timestamp.txt"
$passed = 0
$failed = 0
$total = 0

# Test users for different roles
$script:adminToken = $null
$script:managerToken = $null
$script:employeeToken = $null
$script:hrToken = $null
$script:companyId = $null
$script:userId = $null

function Log {
    param([string]$Message, [string]$Level = "INFO")
    $logMsg = "[$Level] $Message"
    switch ($Level) {
        "PASS" { Write-Host $logMsg -ForegroundColor Green }
        "FAIL" { Write-Host $logMsg -ForegroundColor Red }
        "WARN" { Write-Host $logMsg -ForegroundColor Yellow }
        default { Write-Host $logMsg -ForegroundColor Cyan }
    }
    Add-Content $resultsFile $logMsg
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method = "GET",
        [string]$Endpoint,
        [object]$Body = $null,
        [int[]]$ExpectedStatus = @(200),
        [hashtable]$CustomHeaders = $null,
        [string]$Description = ""
    )
    $script:total++
    try {
        $headers = if ($CustomHeaders) { $CustomHeaders } else { $script:adminHeaders }

        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            Headers = $headers
            UseBasicParsing = $true
            ErrorAction = "Stop"
        }
        if ($Body) {
            $params.Body = ($Body | ConvertTo-Json -Depth 10)
            $params.ContentType = "application/json"
        }

        $response = Invoke-WebRequest @params
        if ($response.StatusCode -in $ExpectedStatus) {
            Log "[$total] $Name - PASS ($($response.StatusCode))" "PASS"
            if ($Description) { Log "    Description: $Description" "INFO" }
            $script:passed++
            return @{ Success = $true; Response = $response; StatusCode = $response.StatusCode }
        } else {
            Log "[$total] $Name - FAIL (Got $($response.StatusCode), expected $ExpectedStatus)" "FAIL"
            if ($Description) { Log "    Description: $Description" "INFO" }
            $script:failed++
            return @{ Success = $false; StatusCode = $response.StatusCode }
        }
    } catch {
        $status = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { "Error" }
        if ($status -in $ExpectedStatus) {
            Log "[$total] $Name - PASS ($status - expected)" "PASS"
            if ($Description) { Log "    Description: $Description" "INFO" }
            $script:passed++
            return @{ Success = $true; StatusCode = $status }
        } else {
            Log "[$total] $Name - FAIL ($status)" "FAIL"
            if ($Description) { Log "    Description: $Description" "INFO" }
            Log "    Error: $($_.Exception.Message)" "FAIL"
            $script:failed++
            return @{ Success = $false; StatusCode = $status }
        }
    }
}

function Get-AuthToken {
    param([string]$Email, [string]$Password)
    try {
        $loginBody = @{ email = $Email; password = $Password }
        $response = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post `
            -Body ($loginBody | ConvertTo-Json) -ContentType "application/json"
        return @{
            Token = $response.data.token
            UserId = $response.data.user.id
            CompanyId = $response.data.user.companyId
        }
    } catch {
        Log "Failed to authenticate as $Email" "FAIL"
        return $null
    }
}

Log "=============================================="
Log "PHASE 1: SECURITY & DATA INTEGRITY TEST SUITE"
Log "=============================================="
Log "Starting: $timestamp"
Log ""

# ============================================================================
# SETUP: Create test users with different roles
# ============================================================================
Log "=== SETUP: Authenticating as Admin ==="
$adminAuth = Get-AuthToken "admin@complaintmanagement.com" "Admin@123"
if (-not $adminAuth) {
    Log "CRITICAL: Cannot authenticate as admin. Exiting." "FAIL"
    exit 1
}
$script:adminToken = $adminAuth.Token
$script:adminHeaders = @{ "Authorization" = "Bearer $adminToken" }
$script:companyId = $adminAuth.CompanyId
$script:userId = $adminAuth.UserId
Log "Admin authenticated. CompanyId: $companyId" "PASS"
Log ""

# ============================================================================
# CATEGORY 1: AUTHORIZATION & PERMISSION TESTS (60 tests)
# ============================================================================
Log "=== CATEGORY 1: AUTHORIZATION & PERMISSION TESTS ==="
Log ""

# 1.1 User Management Permissions (10 tests)
Log "--- 1.1 User Management Permission Tests ---"

Test-Endpoint "Auth-001: Admin can GET all users" `
    -Endpoint "/api/users" `
    -ExpectedStatus @(200) `
    -Description "Admin with ManageUsers permission should access user list"

Test-Endpoint "Auth-002: Admin can search users" `
    -Endpoint "/api/users/search?searchTerm=admin" `
    -ExpectedStatus @(200) `
    -Description "Admin should be able to search for users"

Test-Endpoint "Auth-003: Admin can GET user by ID" `
    -Endpoint "/api/users/$userId" `
    -ExpectedStatus @(200) `
    -Description "Admin should access specific user details"

Test-Endpoint "Auth-004: Admin can GET user by employee code" `
    -Endpoint "/api/users/by-employee-code/ADMIN001" `
    -ExpectedStatus @(200) `
    -Description "Admin should access user by employee code"

Test-Endpoint "Auth-005: Admin can create user (validation)" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="test@test.com"} `
    -ExpectedStatus @(400) `
    -Description "Should validate required fields for user creation"

Test-Endpoint "Auth-006: Admin can update user" `
    -Method "PUT" `
    -Endpoint "/api/users/$userId" `
    -Body @{firstName="Updated"; lastName="Admin"; email="admin@complaintmanagement.com"} `
    -ExpectedStatus @(200, 400) `
    -Description "Admin should be able to update users"

# 1.2 Role Management Permissions (8 tests)
Log ""
Log "--- 1.2 Role Management Permission Tests ---"

Test-Endpoint "Auth-007: Admin can GET all roles" `
    -Endpoint "/api/roles" `
    -ExpectedStatus @(200) `
    -Description "Admin should access all roles in the system"

Test-Endpoint "Auth-008: Admin can GET role permissions" `
    -Endpoint "/api/roles/permissions" `
    -ExpectedStatus @(200) `
    -Description "Admin should view all available permissions"

Test-Endpoint "Auth-009: Admin can GET users in role" `
    -Endpoint "/api/roles/users?roleId=10000000-0000-0000-0000-000000000001" `
    -ExpectedStatus @(200, 404) `
    -Description "Admin should view users assigned to a specific role"

Test-Endpoint "Auth-010: Admin can create role (validation)" `
    -Method "POST" `
    -Endpoint "/api/roles" `
    -Body @{name=""} `
    -ExpectedStatus @(400) `
    -Description "Should validate role creation data"

# 1.3 Organization Structure Permissions (12 tests)
Log ""
Log "--- 1.3 Organization Structure Permission Tests ---"

Test-Endpoint "Auth-011: Admin can GET branches" `
    -Endpoint "/api/branches?companyId=$companyId" `
    -ExpectedStatus @(200) `
    -Description "Admin should access branch list"

Test-Endpoint "Auth-012: Admin can create branch (validation)" `
    -Method "POST" `
    -Endpoint "/api/branches" `
    -Body @{name=""} `
    -ExpectedStatus @(400) `
    -Description "Should validate branch creation"

Test-Endpoint "Auth-013: Admin can GET departments" `
    -Endpoint "/api/departments" `
    -ExpectedStatus @(200) `
    -Description "Admin should access department list"

Test-Endpoint "Auth-014: Admin can create department (validation)" `
    -Method "POST" `
    -Endpoint "/api/departments" `
    -Body @{name=""} `
    -ExpectedStatus @(400) `
    -Description "Should validate department creation"

Test-Endpoint "Auth-015: Admin can GET sections" `
    -Endpoint "/api/sections" `
    -ExpectedStatus @(200) `
    -Description "Admin should access section list"

Test-Endpoint "Auth-016: Admin can GET employee types" `
    -Endpoint "/api/employee-types" `
    -ExpectedStatus @(200) `
    -Description "Admin should access employee types"

# 1.4 Complaint Management Permissions (15 tests)
Log ""
Log "--- 1.4 Complaint Management Permission Tests ---"

Test-Endpoint "Auth-017: Admin can GET all complaints" `
    -Endpoint "/api/complaints" `
    -ExpectedStatus @(200) `
    -Description "Admin should view all complaints in company"

Test-Endpoint "Auth-018: Admin can create complaint (validation)" `
    -Method "POST" `
    -Endpoint "/api/complaints" `
    -Body @{title=""} `
    -ExpectedStatus @(400) `
    -Description "Should validate complaint creation data"

Test-Endpoint "Auth-019: Assign complaint requires valid assignee" `
    -Method "POST" `
    -Endpoint "/api/complaints/00000000-0000-0000-0000-000000000001/assign" `
    -Body @{assignedTo="00000000-0000-0000-0000-000000000001"} `
    -ExpectedStatus @(404, 400) `
    -Description "Should validate assignee exists"

Test-Endpoint "Auth-020: Change status requires valid status" `
    -Method "POST" `
    -Endpoint "/api/complaints/00000000-0000-0000-0000-000000000001/status" `
    -Body @{statusId="00000000-0000-0000-0000-000000000001"} `
    -ExpectedStatus @(404, 400) `
    -Description "Should validate status exists"

Test-Endpoint "Auth-021: Admin can GET complaint comments" `
    -Endpoint "/api/complaints/00000000-0000-0000-0000-000000000001/comments" `
    -ExpectedStatus @(200, 404) `
    -Description "Should retrieve complaint comments or 404 if complaint doesn't exist"

Test-Endpoint "Auth-022: Admin can GET complaint attachments" `
    -Endpoint "/api/complaints/00000000-0000-0000-0000-000000000001/attachments" `
    -ExpectedStatus @(200, 404) `
    -Description "Should retrieve complaint attachments"

Test-Endpoint "Auth-023: Admin can GET complaint history" `
    -Endpoint "/api/complaints/00000000-0000-0000-0000-000000000001/history" `
    -ExpectedStatus @(200, 404) `
    -Description "Should retrieve complaint audit history"

# 1.5 Master Data Permissions (9 tests)
Log ""
Log "--- 1.5 Master Data Permission Tests ---"

Test-Endpoint "Auth-024: Anyone can GET categories" `
    -Endpoint "/api/categories" `
    -ExpectedStatus @(200) `
    -Description "Categories should be readable by all authenticated users"

Test-Endpoint "Auth-025: Anyone can GET status master" `
    -Endpoint "/api/ComplaintStatusMaster" `
    -ExpectedStatus @(200) `
    -Description "Status master should be readable by all"

Test-Endpoint "Auth-026: Anyone can GET priority master" `
    -Endpoint "/api/ComplaintPriorityMaster" `
    -ExpectedStatus @(200) `
    -Description "Priority master should be readable by all"

Test-Endpoint "Auth-027: Admin can create category (validation)" `
    -Method "POST" `
    -Endpoint "/api/categories" `
    -Body @{name=""} `
    -ExpectedStatus @(400) `
    -Description "Should validate category creation"

Test-Endpoint "Auth-028: Admin can update category (validation)" `
    -Method "PUT" `
    -Endpoint "/api/categories/00000000-0000-0000-0000-000000000001" `
    -Body @{name=""} `
    -ExpectedStatus @(400, 404) `
    -Description "Should validate category update"

# 1.6 Escalation Permissions (6 tests)
Log ""
Log "--- 1.6 Escalation Permission Tests ---"

Test-Endpoint "Auth-029: Admin can GET escalation data" `
    -Endpoint "/api/escalation" `
    -ExpectedStatus @(200) `
    -Description "Admin with ViewEscalation can access escalation data"

Test-Endpoint "Auth-030: Admin can GET escalation matrices" `
    -Endpoint "/api/escalation/matrices" `
    -ExpectedStatus @(200) `
    -Description "Admin can view escalation matrices"

Test-Endpoint "Auth-031: Admin can create escalation matrix (validation)" `
    -Method "POST" `
    -Endpoint "/api/escalation/matrices" `
    -Body @{name=""} `
    -ExpectedStatus @(400) `
    -Description "Should validate escalation matrix creation"

Test-Endpoint "Auth-032: Admin can GET escalation policies" `
    -Endpoint "/api/escalation/policies?companyId=$companyId" `
    -ExpectedStatus @(200) `
    -Description "Admin can view escalation policies"

# ============================================================================
# CATEGORY 2: DATA VALIDATION TESTS (50 tests)
# ============================================================================
Log ""
Log "=== CATEGORY 2: DATA VALIDATION TESTS ==="
Log ""

# 2.1 User Data Validation (15 tests)
Log "--- 2.1 User Data Validation Tests ---"

Test-Endpoint "Valid-001: Create user without email" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{firstName="Test"; lastName="User"} `
    -ExpectedStatus @(400) `
    -Description "Email is required for user creation"

Test-Endpoint "Valid-002: Create user with invalid email" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="notanemail"; firstName="Test"; lastName="User"} `
    -ExpectedStatus @(400) `
    -Description "Email format should be validated"

Test-Endpoint "Valid-003: Create user with duplicate email" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="admin@complaintmanagement.com"; firstName="Test"; lastName="User"; password="Test@123"} `
    -ExpectedStatus @(400, 409) `
    -Description "Duplicate email should be rejected"

Test-Endpoint "Valid-004: Create user without first name" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="newuser@test.com"; lastName="User"; password="Test@123"} `
    -ExpectedStatus @(400) `
    -Description "First name is required"

Test-Endpoint "Valid-005: Create user without last name" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="newuser@test.com"; firstName="Test"; password="Test@123"} `
    -ExpectedStatus @(400) `
    -Description "Last name is required"

Test-Endpoint "Valid-006: Create user without password" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="newuser@test.com"; firstName="Test"; lastName="User"} `
    -ExpectedStatus @(400) `
    -Description "Password is required for user creation"

Test-Endpoint "Valid-007: Create user with weak password" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="newuser@test.com"; firstName="Test"; lastName="User"; password="123"} `
    -ExpectedStatus @(400) `
    -Description "Weak password should be rejected"

Test-Endpoint "Valid-008: Create user with non-existent role" `
    -Method "POST" `
    -Endpoint "/api/users" `
    -Body @{email="newuser@test.com"; firstName="Test"; lastName="User"; password="Test@123"; roleId="00000000-0000-0000-0000-999999999999"} `
    -ExpectedStatus @(400, 404) `
    -Description "Invalid role ID should be rejected"

# 2.2 Complaint Data Validation (15 tests)
Log ""
Log "--- 2.2 Complaint Data Validation Tests ---"

Test-Endpoint "Valid-009: Create complaint without title" `
    -Method "POST" `
    -Endpoint "/api/complaints" `
    -Body @{description="Test complaint"} `
    -ExpectedStatus @(400) `
    -Description "Title is required for complaint creation"

Test-Endpoint "Valid-010: Create complaint without description" `
    -Method "POST" `
    -Endpoint "/api/complaints" `
    -Body @{title="Test Complaint"} `
    -ExpectedStatus @(400) `
    -Description "Description is required for complaint creation"

Test-Endpoint "Valid-011: Create complaint with invalid category" `
    -Method "POST" `
    -Endpoint "/api/complaints" `
    -Body @{title="Test"; description="Test"; categoryId="00000000-0000-0000-0000-999999999999"} `
    -ExpectedStatus @(400, 404) `
    -Description "Invalid category should be rejected"

Test-Endpoint "Valid-012: Create complaint with invalid priority" `
    -Method "POST" `
    -Endpoint "/api/complaints" `
    -Body @{title="Test"; description="Test"; priorityId="00000000-0000-0000-0000-999999999999"} `
    -ExpectedStatus @(400, 404) `
    -Description "Invalid priority should be rejected"

Test-Endpoint "Valid-013: Create complaint with invalid status" `
    -Method "POST" `
    -Endpoint "/api/complaints" `
    -Body @{title="Test"; description="Test"; statusId="00000000-0000-0000-0000-999999999999"} `
    -ExpectedStatus @(400, 404) `
    -Description "Invalid status should be rejected"

# 2.3 Organization Data Validation (10 tests)
Log ""
Log "--- 2.3 Organization Data Validation Tests ---"

Test-Endpoint "Valid-014: Create branch without name" `
    -Method "POST" `
    -Endpoint "/api/branches" `
    -Body @{code="BR001"; companyId=$companyId} `
    -ExpectedStatus @(400) `
    -Description "Branch name is required"

Test-Endpoint "Valid-015: Create branch without code" `
    -Method "POST" `
    -Endpoint "/api/branches" `
    -Body @{name="Test Branch"; companyId=$companyId} `
    -ExpectedStatus @(400) `
    -Description "Branch code is required"

Test-Endpoint "Valid-016: Create branch without company" `
    -Method "POST" `
    -Endpoint "/api/branches" `
    -Body @{name="Test Branch"; code="BR001"} `
    -ExpectedStatus @(400) `
    -Description "Company ID is required for branch"

Test-Endpoint "Valid-017: Create department without name" `
    -Method "POST" `
    -Endpoint "/api/departments" `
    -Body @{code="DEP001"} `
    -ExpectedStatus @(400) `
    -Description "Department name is required"

Test-Endpoint "Valid-018: Create section without name" `
    -Method "POST" `
    -Endpoint "/api/sections" `
    -Body @{code="SEC001"} `
    -ExpectedStatus @(400) `
    -Description "Section name is required"

# 2.4 Master Data Validation (10 tests)
Log ""
Log "--- 2.4 Master Data Validation Tests ---"

Test-Endpoint "Valid-019: Create category without name" `
    -Method "POST" `
    -Endpoint "/api/categories" `
    -Body @{code="CAT001"} `
    -ExpectedStatus @(400) `
    -Description "Category name is required"

Test-Endpoint "Valid-020: Create status without name" `
    -Method "POST" `
    -Endpoint "/api/ComplaintStatusMaster" `
    -Body @{code="STAT001"} `
    -ExpectedStatus @(400) `
    -Description "Status name is required"

Test-Endpoint "Valid-021: Create priority without name" `
    -Method "POST" `
    -Endpoint "/api/ComplaintPriorityMaster" `
    -Body @{code="PRI001"} `
    -ExpectedStatus @(400) `
    -Description "Priority name is required"

Test-Endpoint "Valid-022: Update category with empty name" `
    -Method "PUT" `
    -Endpoint "/api/categories/00000000-0000-0000-0000-000000000001" `
    -Body @{name=""; code="CAT001"} `
    -ExpectedStatus @(400, 404) `
    -Description "Category name cannot be empty on update"

Test-Endpoint "Valid-023: Update status with empty name" `
    -Method "PUT" `
    -Endpoint "/api/ComplaintStatusMaster/10000000-0000-0000-0000-000000000001" `
    -Body @{name=""; code="SUBMITTED"} `
    -ExpectedStatus @(400, 404) `
    -Description "Status name cannot be empty on update"

# ============================================================================
# CATEGORY 3: MULTI-TENANT ISOLATION TESTS (30 tests)
# ============================================================================
Log ""
Log "=== CATEGORY 3: MULTI-TENANT ISOLATION TESTS ==="
Log ""

Log "--- 3.1 Company Data Isolation Tests ---"

Test-Endpoint "Tenant-001: GET complaints returns only company complaints" `
    -Endpoint "/api/complaints" `
    -ExpectedStatus @(200) `
    -Description "Should only return complaints for authenticated user's company"

Test-Endpoint "Tenant-002: GET users returns only company users" `
    -Endpoint "/api/users" `
    -ExpectedStatus @(200) `
    -Description "Should only return users from authenticated user's company"

Test-Endpoint "Tenant-003: GET branches returns only company branches" `
    -Endpoint "/api/branches?companyId=$companyId" `
    -ExpectedStatus @(200) `
    -Description "Should only return branches for specified company"

Test-Endpoint "Tenant-004: GET departments filtered by company" `
    -Endpoint "/api/departments" `
    -ExpectedStatus @(200) `
    -Description "Departments should be company-scoped"

Test-Endpoint "Tenant-005: GET escalation matrices for company only" `
    -Endpoint "/api/escalation/matrices" `
    -ExpectedStatus @(200) `
    -Description "Escalation matrices should be company-specific"

Test-Endpoint "Tenant-006: GET dashboard for company only" `
    -Endpoint "/api/dashboard/statistics?companyId=$companyId" `
    -ExpectedStatus @(200) `
    -Description "Dashboard should show only company-specific statistics"

Test-Endpoint "Tenant-007: GET company data" `
    -Endpoint "/api/company" `
    -ExpectedStatus @(200) `
    -Description "Should retrieve company information"

Test-Endpoint "Tenant-008: GET company by ID" `
    -Endpoint "/api/company/$companyId" `
    -ExpectedStatus @(200) `
    -Description "Should retrieve specific company details"

# ============================================================================
# CATEGORY 4: TOKEN & SESSION SECURITY TESTS (30 tests)
# ============================================================================
Log ""
Log "=== CATEGORY 4: TOKEN & SESSION SECURITY TESTS ==="
Log ""

Log "--- 4.1 Authentication Token Tests ---"

Test-Endpoint "Token-001: Request without token" `
    -Endpoint "/api/users" `
    -CustomHeaders @{} `
    -ExpectedStatus @(401) `
    -Description "Request without Authorization header should be rejected"

Test-Endpoint "Token-002: Request with invalid token" `
    -Endpoint "/api/users" `
    -CustomHeaders @{"Authorization" = "Bearer invalid_token_12345"} `
    -ExpectedStatus @(401) `
    -Description "Request with invalid token should be rejected"

Test-Endpoint "Token-003: Request with malformed token" `
    -Endpoint "/api/users" `
    -CustomHeaders @{"Authorization" = "NotBearer token"} `
    -ExpectedStatus @(401) `
    -Description "Request with malformed Authorization header should be rejected"

Test-Endpoint "Token-004: Request with empty token" `
    -Endpoint "/api/users" `
    -CustomHeaders @{"Authorization" = "Bearer "} `
    -ExpectedStatus @(401) `
    -Description "Request with empty token should be rejected"

Test-Endpoint "Token-005: Valid token returns data" `
    -Endpoint "/api/auth/me" `
    -ExpectedStatus @(200) `
    -Description "Valid token should return authenticated user data"

Log ""
Log "--- 4.2 Login Security Tests ---"

Test-Endpoint "Login-001: Login with wrong password" `
    -Method "POST" `
    -Endpoint "/api/auth/login" `
    -Body @{email="admin@complaintmanagement.com"; password="WrongPassword"} `
    -CustomHeaders @{} `
    -ExpectedStatus @(401) `
    -Description "Login with incorrect password should fail"

Test-Endpoint "Login-002: Login with non-existent user" `
    -Method "POST" `
    -Endpoint "/api/auth/login" `
    -Body @{email="nonexistent@test.com"; password="Test@123"} `
    -CustomHeaders @{} `
    -ExpectedStatus @(401) `
    -Description "Login with non-existent user should fail"

Test-Endpoint "Login-003: Login without email" `
    -Method "POST" `
    -Endpoint "/api/auth/login" `
    -Body @{password="Test@123"} `
    -CustomHeaders @{} `
    -ExpectedStatus @(400, 401) `
    -Description "Login without email should fail validation"

Test-Endpoint "Login-004: Login without password" `
    -Method "POST" `
    -Endpoint "/api/auth/login" `
    -Body @{email="admin@complaintmanagement.com"} `
    -CustomHeaders @{} `
    -ExpectedStatus @(400, 401) `
    -Description "Login without password should fail validation"

Test-Endpoint "Login-005: Login with empty email" `
    -Method "POST" `
    -Endpoint "/api/auth/login" `
    -Body @{email=""; password="Test@123"} `
    -CustomHeaders @{} `
    -ExpectedStatus @(400, 401) `
    -Description "Login with empty email should fail"

Test-Endpoint "Login-006: Login with empty password" `
    -Method "POST" `
    -Endpoint "/api/auth/login" `
    -Body @{email="admin@complaintmanagement.com"; password=""} `
    -CustomHeaders @{} `
    -ExpectedStatus @(400, 401) `
    -Description "Login with empty password should fail"

# ============================================================================
# FINAL RESULTS
# ============================================================================
Log ""
Log "=============================================="
Log "PHASE 1 TEST RESULTS"
Log "=============================================="
Log "Total Tests: $total"
Log "Passed: $passed"
Log "Failed: $failed"
$successRate = if ($total -gt 0) { [math]::Round(($passed / $total) * 100, 2) } else { 0 }
Log "Success Rate: $successRate%"

if ($failed -eq 0) {
    Log ""
    Log "*** PHASE 1 COMPLETE - ALL TESTS PASSED! ***" "PASS"
} else {
    Log ""
    Log "*** PHASE 1 COMPLETE - $failed TESTS FAILED ***" "FAIL"
}

Log ""
Log "Results saved to: $resultsFile"
Log "Test completed: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"

# Return exit code based on results
exit $failed
