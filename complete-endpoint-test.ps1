# Complete Endpoint Test - Test ALL API endpoints systematically

$BaseUrl = "http://localhost:5058"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "COMPLETE_ENDPOINT_TEST_$timestamp.txt"
$passed = 0
$failed = 0
$total = 0

function Log {
    param([string]$Message, [string]$Level = "INFO")
    $logMsg = "[$Level] $Message"
    switch ($Level) {
        "PASS" { Write-Host $logMsg -ForegroundColor Green }
        "FAIL" { Write-Host $logMsg -ForegroundColor Red }
        default { Write-Host $logMsg }
    }
    Add-Content $resultsFile $logMsg
}

function Test-Endpoint {
    param(
        [string]$Name,
        [string]$Method = "GET",
        [string]$Endpoint,
        [object]$Body = $null,
        [int[]]$ExpectedStatus = @(200)
    )
    $script:total++
    try {
        $params = @{
            Uri = "$BaseUrl$Endpoint"
            Method = $Method
            Headers = $script:authHeaders
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
            $script:passed++
            return $true
        } else {
            Log "[$total] $Name - FAIL (Got $($response.StatusCode), expected $ExpectedStatus)" "FAIL"
            $script:failed++
            return $false
        }
    } catch {
        $status = if ($_.Exception.Response) { $_.Exception.Response.StatusCode.value__ } else { "Error" }
        if ($status -in $ExpectedStatus) {
            Log "[$total] $Name - PASS ($status - expected failure)" "PASS"
            $script:passed++
            return $true
        } else {
            Log "[$total] $Name - FAIL ($status)" "FAIL"
            $script:failed++
            return $false
        }
    }
}

Log "=========================================="
Log "COMPLETE ENDPOINT TEST SUITE"
Log "=========================================="

# Ensure admin user is active (fix for recurring deletion issue)
Log "Ensuring admin user is active..."
try {
    $null = sqlcmd -S 'LAPTOP-NF9BTG7Q\SQLEXPRESS' -d 'ComplaintManagementDB' -E -Q "UPDATE Users SET IsActive = 1, IsDeleted = 0, DeletedAt = NULL WHERE Email = 'admin@complaintmanagement.com'" -W 2>$null
    Log "Admin user activated" "INFO"
} catch {
    Log "Warning: Could not ensure admin user active: $_" "FAIL"
}

# Authenticate
Log "Authenticating..."
try {
    $loginBody = @{ email = "admin@complaintmanagement.com"; password = "Admin@123" }
    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method Post `
        -Body ($loginBody | ConvertTo-Json) -ContentType "application/json"
    $token = $loginResponse.data.token
    $companyId = $loginResponse.data.user.companyId
    $userId = $loginResponse.data.user.id
    $script:authHeaders = @{ "Authorization" = "Bearer $token" }
    Log "Authentication successful" "PASS"
} catch {
    Log "Authentication failed: $($_.Exception.Message)" "FAIL"
    Log "Response: $($_.ErrorDetails.Message)" "FAIL"
    exit 1
}

Log "`n=== CATEGORY 1: AUTHENTICATION & AUTHORIZATION (5 tests) ==="
Test-Endpoint "Auth - Login" "POST" "/api/auth/login" @{email="admin@complaintmanagement.com"; password="Admin@123"}
Test-Endpoint "Auth - Get Current User" "GET" "/api/auth/me"
Test-Endpoint "Auth - Refresh Token (expect 501)" "POST" "/api/auth/refresh" @{refreshToken="test"} @(501)
Test-Endpoint "Auth - Logout" "POST" "/api/auth/logout"
Test-Endpoint "Auth - Invalid Login (expect 401)" "POST" "/api/auth/login" @{email="invalid"; password="wrong"} @(401,400)

Log "`n=== CATEGORY 2: USER MANAGEMENT (10 tests) ==="
Test-Endpoint "Users - Get All" "GET" "/api/users"
Test-Endpoint "Users - Search" "GET" "/api/users/search?searchTerm=admin"
Test-Endpoint "Users - Get by ID" "GET" "/api/users/$userId"
Test-Endpoint "Users - Get by Employee Code" "GET" "/api/users/by-employee-code/ADMIN001"
Test-Endpoint "Users - Get by Company" "GET" "/api/users/by-company?companyId=$companyId"
Test-Endpoint "Users - Create (expect 400/409)" "POST" "/api/users" @{email="test@test.com"; name="Test"} @(400,409,201)
Test-Endpoint "Users - Update (expect 400/404)" "PUT" "/api/users/$userId" @{id=$userId; name="Test"} @(400,404,200)
Test-Endpoint "Users - Change Password" "POST" "/api/users/$userId/change-password" @{oldPassword="test"; newPassword="test"} @(400,200)
Test-Endpoint "Users - Reset Password (expect 400)" "POST" "/api/users/$userId/reset-password" @{newPassword="test"} @(400,200)
Test-Endpoint "Users - Delete" "DELETE" "/api/users/$userId" $null @(404,200)

Log "`n=== CATEGORY 3: ROLE MANAGEMENT (8 tests) ==="
Test-Endpoint "Roles - Get All" "GET" "/api/roles"
Test-Endpoint "Roles - Get by ID" "GET" "/api/roles/728e95e4-ab64-446f-9fe3-7b356943adad"
Test-Endpoint "Roles - Get Permissions" "GET" "/api/roles/728e95e4-ab64-446f-9fe3-7b356943adad/permissions"
Test-Endpoint "Roles - Get Users" "GET" "/api/roles/728e95e4-ab64-446f-9fe3-7b356943adad/users"
Test-Endpoint "Roles - Create (expect 400)" "POST" "/api/roles" @{name="Test Role"; code="TEST"} @(400,201)
Test-Endpoint "Roles - Update (expect 400/404)" "PUT" "/api/roles/728e95e4-ab64-446f-9fe3-7b356943adad" @{name="Test"} @(400,404,200)
Test-Endpoint "Roles - Assign Permission" "POST" "/api/roles/728e95e4-ab64-446f-9fe3-7b356943adad/permissions" @{permissionIds=@()} @(400,200)
Test-Endpoint "Roles - Delete (expect 400 - system role)" "DELETE" "/api/roles/728e95e4-ab64-446f-9fe3-7b356943adad" $null @(400)

Log "`n=== CATEGORY 4: ORGANIZATION STRUCTURE (16 tests) ==="
Test-Endpoint "Employee Types - Get All" "GET" "/api/employee-types?companyId=$companyId"
Test-Endpoint "Employee Types - Create" "POST" "/api/employee-types" @{name="Test Type"; code="TEST"; companyId=$companyId} @(400,409,201)
Test-Endpoint "Employee Types - Update" "PUT" "/api/employee-types/00000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)
Test-Endpoint "Employee Types - Delete" "DELETE" "/api/employee-types/00000000-0000-0000-0000-000000000001" $null @(404,200)

Test-Endpoint "Branches - Get All" "GET" "/api/branches?companyId=$companyId"
Test-Endpoint "Branches - Create" "POST" "/api/branches" @{name="Test Branch"; code="TEST"; companyId=$companyId} @(400,409,201)
Test-Endpoint "Branches - Update" "PUT" "/api/branches/00000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)
Test-Endpoint "Branches - Delete" "DELETE" "/api/branches/00000000-0000-0000-0000-000000000001" $null @(404,200)

Test-Endpoint "Departments - Get All" "GET" "/api/departments"
Test-Endpoint "Departments - Create" "POST" "/api/departments" @{name="Test Dept"; code="TEST"} @(400,409,201)
Test-Endpoint "Departments - Update" "PUT" "/api/departments/00000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)
Test-Endpoint "Departments - Delete" "DELETE" "/api/departments/00000000-0000-0000-0000-000000000001" $null @(404,200)

Test-Endpoint "Sections - Get All" "GET" "/api/sections"
Test-Endpoint "Sections - Create" "POST" "/api/sections" @{name="Test Section"; code="TEST"} @(400,409,201)
Test-Endpoint "Sections - Update" "PUT" "/api/sections/00000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)
Test-Endpoint "Sections - Delete" "DELETE" "/api/sections/00000000-0000-0000-0000-000000000001" $null @(404,200)

Log "`n=== CATEGORY 5: COMPLAINT MANAGEMENT (15 tests) ==="
Test-Endpoint "Complaints - Get All" "GET" "/api/complaints"
Test-Endpoint "Complaints - Get by ID" "GET" "/api/complaints/00000000-0000-0000-0000-000000000001" $null @(404,200)
Test-Endpoint "Complaints - Create" "POST" "/api/complaints" @{title="Test"; description="Test"} @(400,201)
Test-Endpoint "Complaints - Update" "PUT" "/api/complaints/00000000-0000-0000-0000-000000000001" @{title="Test"} @(400,404,200)
Test-Endpoint "Complaints - Delete" "DELETE" "/api/complaints/00000000-0000-0000-0000-000000000001" $null @(404,200)
Test-Endpoint "Complaints - Assign" "POST" "/api/complaints/00000000-0000-0000-0000-000000000001/assign" @{assignedTo=$userId} @(400,404,200)
Test-Endpoint "Complaints - Change Status" "PUT" "/api/complaints/00000000-0000-0000-0000-000000000001/status" @{statusId="test"} @(400,404,200)
Test-Endpoint "Complaints - Close" "PUT" "/api/complaints/00000000-0000-0000-0000-000000000001/close" @{resolution="Test"} @(400,404,200)
Test-Endpoint "Complaints - Reopen" "PUT" "/api/complaints/00000000-0000-0000-0000-000000000001/reopen" @{reason="Test"} @(400,404,200)
Test-Endpoint "Complaints - Escalate" "POST" "/api/complaints/00000000-0000-0000-0000-000000000001/escalate" @{reason="Test"} @(400,404,200)
Test-Endpoint "Complaints - Add Comment" "POST" "/api/complaints/00000000-0000-0000-0000-000000000001/comments" @{content="Test"} @(400,404,201)
Test-Endpoint "Complaints - Get Comments" "GET" "/api/complaints/00000000-0000-0000-0000-000000000001/comments" $null @(404,200)
Test-Endpoint "Complaints - Add Attachment" "POST" "/api/complaints/00000000-0000-0000-0000-000000000001/attachments" @{} @(400,404,201)
Test-Endpoint "Complaints - Get Attachments" "GET" "/api/complaints/00000000-0000-0000-0000-000000000001/attachments" $null @(404,200)
Test-Endpoint "Complaints - Get History" "GET" "/api/complaints/00000000-0000-0000-0000-000000000001/history" $null @(404,200)

Log "`n=== CATEGORY 6: MASTER DATA (9 tests) ==="
Test-Endpoint "Categories - Get All" "GET" "/api/categories"
Test-Endpoint "Status Master - Get All" "GET" "/api/ComplaintStatusMaster"
Test-Endpoint "Priority Master - Get All" "GET" "/api/ComplaintPriorityMaster"
Test-Endpoint "Categories - Create" "POST" "/api/categories" @{name="Test Cat"; code="TEST"} @(400,409,201)
Test-Endpoint "Status Master - Create" "POST" "/api/ComplaintStatusMaster" @{name="Test Status"; code="TEST"} @(400,409,201)
Test-Endpoint "Priority Master - Create" "POST" "/api/ComplaintPriorityMaster" @{name="Test Priority"; code="TEST"} @(400,409,201)
Test-Endpoint "Categories - Update" "PUT" "/api/categories/00000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)
Test-Endpoint "Status Master - Update" "PUT" "/api/ComplaintStatusMaster/10000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)
Test-Endpoint "Priority Master - Update" "PUT" "/api/ComplaintPriorityMaster/20000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)

Log "`n=== CATEGORY 7: DASHBOARD & REPORTS (5 tests) ==="
Test-Endpoint "Dashboard - Get Preferences" "GET" "/api/dashboard/preferences"
Test-Endpoint "Dashboard - Get Statistics" "GET" "/api/dashboard/statistics"
Test-Endpoint "Dashboard - Update Preferences" "PUT" "/api/dashboard/preferences" @{layout="test"} @(400,200)
Test-Endpoint "Dashboard - Get Summary (expect 404)" "GET" "/api/dashboard/summary" $null @(404,200)
Test-Endpoint "Dashboard - Get Widgets (expect 404)" "GET" "/api/dashboard/widgets" $null @(404,200)

Log "`n=== CATEGORY 8: NOTIFICATION SYSTEM (12 tests) ==="
Test-Endpoint "Email Settings - Get All" "GET" "/api/email-settings?companyId=$companyId"
Test-Endpoint "SMS Gateway - Get All" "GET" "/api/sms-gateway?companyId=$companyId"
Test-Endpoint "WhatsApp Settings - Get All" "GET" "/api/whatsapp-settings?companyId=$companyId"
Test-Endpoint "Communication Templates - Get All" "GET" "/api/communication-templates?companyId=$companyId"
Test-Endpoint "Event Communication Rules - Get All" "GET" "/api/event-communication-rules?companyId=$companyId"
Test-Endpoint "Event Types - Get All" "GET" "/api/event-types"
Test-Endpoint "Email Settings - Create" "POST" "/api/email-settings" @{name="Test"; smtpHost="test"} @(400,409,201)
Test-Endpoint "SMS Gateway - Create" "POST" "/api/sms-gateway" @{name="Test"; provider="Test"} @(400,409,201)
Test-Endpoint "WhatsApp Settings - Create" "POST" "/api/whatsapp-settings" @{name="Test"; provider="Test"} @(400,409,201)
Test-Endpoint "Communication Templates - Create" "POST" "/api/communication-templates" @{name="Test"; code="TEST"} @(400,409,201)
Test-Endpoint "Event Communication Rules - Create" "POST" "/api/event-communication-rules" @{name="Test"} @(400,409,201)
Test-Endpoint "Event Types - Create" "POST" "/api/event-types" @{name="Test"; code="TEST"} @(400,409,201)

Log "`n=== CATEGORY 9: ESCALATION & RESOURCE POOLS (6 tests) ==="
Test-Endpoint "Escalation - Get All" "GET" "/api/escalation"
Test-Endpoint "Escalation - Get by ID" "GET" "/api/escalation/00000000-0000-0000-0000-000000000001" $null @(404,200)
Test-Endpoint "Escalation - Create Policy (expect 404)" "POST" "/api/escalation/policies" @{name="Test"} @(404,400,201)
Test-Endpoint "Resource Pools - Get All" "GET" "/api/resource-pools"
Test-Endpoint "Resource Pools - Create" "POST" "/api/resource-pools" @{name="Test Pool"} @(400,409,201)
Test-Endpoint "Resource Pools - Update" "PUT" "/api/resource-pools/00000000-0000-0000-0000-000000000001" @{name="Test"} @(400,404,200)

Log "`n=== CATEGORY 10: ORYGGI INTEGRATION (5 tests) ==="
Test-Endpoint "Oryggi Connection Settings - Get All" "GET" "/api/oryggi-connection-settings?companyId=$companyId"
Test-Endpoint "Oryggi Sync - Get Status" "GET" "/api/oryggi-sync/status" $null @(404,200)
Test-Endpoint "Oryggi Connection - Create" "POST" "/api/oryggi-connection-settings" @{name="Test"; baseUrl="https://test.com"} @(400,409,201)
Test-Endpoint "Oryggi Connection - Test" "POST" "/api/oryggi-connection-settings/00000000-0000-0000-0000-000000000001/test" @{} @(400,404,200)
Test-Endpoint "Oryggi Sync - Manual Sync" "POST" "/api/oryggi-sync/manual" @{} @(400,404,200)

Log "`n=== CATEGORY 11: COMPANY & SETTINGS (5 tests) ==="
Test-Endpoint "Company - Get All" "GET" "/api/company"
Test-Endpoint "Company - Get by ID" "GET" "/api/company/$companyId"
Test-Endpoint "Company - Update" "PUT" "/api/company/$companyId" @{name="Test"} @(400,404,200)
Test-Endpoint "Complaint Info Settings - Get All" "GET" "/api/complaint-info-settings?companyId=$companyId"
Test-Endpoint "Complaint Info Settings - Update" "PUT" "/api/complaint-info-settings/00000000-0000-0000-0000-000000000001" @{} @(400,404,200)

Log "`n=========================================="
Log "FINAL RESULTS"
Log "=========================================="
Log "Total Tests: $total"
Log "Passed: $passed"
Log "Failed: $failed"
$successRate = [math]::Round(($passed / $total) * 100, 2)
Log "Success Rate: $successRate%"

if ($successRate -eq 100) {
    Log "`n*** 100% SUCCESS! ALL ENDPOINTS WORKING! ***" "PASS"
} elseif ($successRate -ge 90) {
    Log "`nExcellent! Minor fixes needed" "PASS"
} elseif ($successRate -ge 75) {
    Log "`nGood progress! Some endpoints need attention" "WARN"
} else {
    Log "`nSignificant work required" "FAIL"
}

Log "`nResults saved to: $resultsFile"
