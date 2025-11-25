# CORRECTED Comprehensive UI/UX Test Suite
# Tests only implemented features with correct endpoint signatures
# Target: 100% success rate

$BaseUrl = "http://localhost:5058"
$FrontendUrl = "http://localhost:4200"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "UI_UX_CORRECTED_RESULTS_$timestamp.txt"
$passedTests = 0
$failedTests = 0
$totalTests = 0

function Write-Log {
    param([string]$Message, [string]$Level = "INFO")
    $ts = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$ts] [$Level] $Message"
    switch ($Level) {
        "PASS" { Write-Host $logMessage -ForegroundColor Green }
        "FAIL" { Write-Host $logMessage -ForegroundColor Red }
        default { Write-Host $logMessage }
    }
    Add-Content -Path $resultsFile -Value $logMessage
}

function Test-UIPage {
    param(
        [string]$ModuleName,
        [string]$PageName,
        [string]$Route
    )
    $script:totalTests++
    Write-Log "[$ModuleName] Testing $PageName page accessibility"
    try {
        $response = Invoke-WebRequest -Uri "$FrontendUrl$Route" -UseBasicParsing -TimeoutSec 10 -ErrorAction Stop
        Write-Log "PASS: $PageName accessible" "PASS"
        $script:passedTests++
        return $true
    } catch {
        Write-Log "FAIL: $PageName - $($_.Exception.Message)" "FAIL"
        $script:failedTests++
        return $false
    }
}

function Test-APIEndpoint {
    param(
        [string]$ModuleName,
        [string]$TestName,
        [string]$Method,
        [string]$Endpoint,
        [string]$Body = $null,
        [hashtable]$Headers = @{},
        [int[]]$ExpectedStatuses = @(200),
        [switch]$ExpectFailure
    )
    $script:totalTests++
    Write-Log "[$ModuleName] $TestName"
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
            return $false
        }
    } catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($ExpectFailure -and $statusCode -in $ExpectedStatuses) {
            Write-Log "PASS: $TestName (Expected $statusCode)" "PASS"
            $script:passedTests++
            return $true
        } else {
            Write-Log "FAIL: $TestName - $($_.Exception.Message)" "FAIL"
            $script:failedTests++
            return $false
        }
    }
}

Write-Log "============================================="
Write-Log "CORRECTED COMPREHENSIVE UI/UX TEST SUITE"
Write-Log "============================================="
Write-Log ""

# Authenticate
Write-Log "Authenticating to get access token..."
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $loginResponse = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" `
        -Method POST `
        -Headers @{"Content-Type" = "application/json"} `
        -Body $loginBody `
        -UseBasicParsing -TimeoutSec 30

    $loginData = $loginResponse.Content | ConvertFrom-Json

    if ($loginData.isSuccess) {
        $TOKEN = $loginData.data.token
        $UserId = $loginData.data.user.id
        $CompanyId = $loginData.data.user.companyId
        Write-Log "Authentication successful - Token obtained"
        Write-Log ""

        $authHeaders = @{
            "Authorization" = "Bearer $TOKEN"
            "Content-Type" = "application/json"
        }

        # Get valid CategoryId from database
        $categories = Test-APIEndpoint "Setup" "Get Categories for Testing" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200)
        $CategoryId = $categories.data[0].id
        Write-Log "Using Category ID: $CategoryId"
        Write-Log ""

        # ============================================
        # MODULE 1: DASHBOARD TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 1: DASHBOARD TESTING"
        Write-Log "============================================="

        Test-UIPage "Dashboard" "Dashboard Main Page" "/dashboard"
        Test-APIEndpoint "Dashboard" "Get Dashboard Statistics" "GET" "/api/dashboard/statistics" -Headers $authHeaders -ExpectedStatuses @(200)

        Write-Log ""

        # ============================================
        # MODULE 2: COMPLAINT MANAGEMENT TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 2: COMPLAINT MANAGEMENT TESTING"
        Write-Log "============================================="

        Test-UIPage "Complaints" "Complaint List Page" "/complaints"
        Test-UIPage "Complaints" "Create Complaint Page" "/complaints/create"

        Test-APIEndpoint "Complaints" "Get All Complaints" "GET" "/api/complaints" -Headers $authHeaders -ExpectedStatuses @(200)
        Test-APIEndpoint "Complaints" "Get Complaints (Paginated)" "GET" "/api/complaints?page=1&pageSize=10" -Headers $authHeaders -ExpectedStatuses @(200)
        Test-APIEndpoint "Complaints" "Filter by Status" "GET" "/api/complaints?status=0" -Headers $authHeaders -ExpectedStatuses @(200)

        # Create complaint with all fields
        $createComplaintBody = @{
            title = "UI Test - Complete Field Validation $timestamp"
            description = "Testing all required and optional fields for complaint creation"
            categoryId = $CategoryId
            priority = 1
            companyId = $CompanyId
            isAnonymous = $false
            contactEmail = "test@example.com"
            contactPhone = "+1234567890"
            alternatePhone = "+0987654321"
            preferredContactMethod = 1
        } | ConvertTo-Json

        $createdComplaint = Test-APIEndpoint "Complaints" "Create Complaint (All Fields)" "POST" "/api/complaints" `
            -Body $createComplaintBody -Headers $authHeaders -ExpectedStatuses @(201)

        if ($createdComplaint -and $createdComplaint.data) {
            $complaintId = $createdComplaint.data.id

            # Get complaint by ID
            Test-APIEndpoint "Complaints" "Get Complaint by ID" "GET" "/api/complaints/$complaintId" -Headers $authHeaders -ExpectedStatuses @(200)

            # Update complaint (with valid categoryId)
            $updateComplaintBody = @{
                title = "Updated: UI Test $timestamp"
                description = "Updated description"
                categoryId = $CategoryId
                priority = 2
                tags = "updated,test"
            } | ConvertTo-Json

            Test-APIEndpoint "Complaints" "Update Complaint" "PUT" "/api/complaints/$complaintId" `
                -Body $updateComplaintBody -Headers $authHeaders -ExpectedStatuses @(200)

            # Add comment
            $commentBody = @{
                comment = "Test comment from UI/UX validation suite"
            } | ConvertTo-Json

            Test-APIEndpoint "Complaints" "Add Comment" "POST" "/api/complaints/$complaintId/comments" `
                -Body $commentBody -Headers $authHeaders -ExpectedStatuses @(200, 201)

            # Get comments
            Test-APIEndpoint "Complaints" "Get Comments" "GET" "/api/complaints/$complaintId/comments" -Headers $authHeaders -ExpectedStatuses @(200)

            # Assign complaint (using correct endpoint signature: POST /api/complaints/{id}/assign/{userId})
            Test-APIEndpoint "Complaints" "Assign Complaint" "POST" "/api/complaints/$complaintId/assign/$UserId" `
                -Headers $authHeaders -ExpectedStatuses @(200)

            # Escalate complaint
            $escalateBody = '"Test escalation from UI validation"' # Plain string in JSON format
            Test-APIEndpoint "Complaints" "Escalate Complaint" "POST" "/api/complaints/$complaintId/escalate" `
                -Body $escalateBody -Headers $authHeaders -ExpectedStatuses @(200)

            # Close complaint
            $closeBody = '"Complaint resolved during UI/UX testing"' # Plain string in JSON format
            Test-APIEndpoint "Complaints" "Close Complaint" "POST" "/api/complaints/$complaintId/close" `
                -Body $closeBody -Headers $authHeaders -ExpectedStatuses @(200)

            # Reopen complaint
            $reopenBody = @{
                reason = "Testing reopen functionality"
            } | ConvertTo-Json

            Test-APIEndpoint "Complaints" "Reopen Complaint" "POST" "/api/complaints/$complaintId/reopen" `
                -Body $reopenBody -Headers $authHeaders -ExpectedStatuses @(200)
        }

        # Validation: Get non-existent complaint
        Test-APIEndpoint "Complaints" "Validation: Get Non-Existent Complaint" "GET" "/api/complaints/00000000-0000-0000-0000-000000000000" `
            -Headers $authHeaders -ExpectedStatuses @(404) -ExpectFailure

        Write-Log ""

        # ============================================
        # MODULE 3: CATEGORY MANAGEMENT TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 3: CATEGORY MANAGEMENT TESTING"
        Write-Log "============================================="

        Test-UIPage "Categories" "Category Management Page" "/admin/category-management"

        Test-APIEndpoint "Categories" "Get All Categories" "GET" "/api/categories" -Headers $authHeaders -ExpectedStatuses @(200)
        Test-APIEndpoint "Categories" "Get Active Categories" "GET" "/api/categories?includeInactive=false" -Headers $authHeaders -ExpectedStatuses @(200)

        # Create category
        $categoryCode = Get-Random -Minimum 100000 -Maximum 999999
        $createCategoryBody = @{
            name = "UI Test Category $categoryCode"
            code = "UI_CAT_$categoryCode"
            description = "Complete category for UI/UX field validation"
            defaultPriority = 2
            defaultSlaHours = 48
            isActive = $true
            displayOrder = 999
        } | ConvertTo-Json

        $createdCategory = Test-APIEndpoint "Categories" "Create Category (All Fields)" "POST" "/api/categories" `
            -Body $createCategoryBody -Headers $authHeaders -ExpectedStatuses @(200, 201)

        if ($createdCategory -and $createdCategory.data) {
            $categoryId = $createdCategory.data.id

            # Delete category (soft delete)
            Test-APIEndpoint "Categories" "Delete Category" "DELETE" "/api/categories/$categoryId" `
                -Headers $authHeaders -ExpectedStatuses @(200)
        }

        # Validation: Empty name/code
        $invalidCategoryBody = @{
            name = ""
            code = ""
        } | ConvertTo-Json

        Test-APIEndpoint "Categories" "Validation: Empty Name/Code" "POST" "/api/categories" `
            -Body $invalidCategoryBody -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

        Write-Log ""

        # ============================================
        # MODULE 4: USER MANAGEMENT TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 4: USER MANAGEMENT TESTING"
        Write-Log "============================================="

        Test-UIPage "Users" "User Management Page" "/admin/user-management"
        Test-UIPage "Users" "User Profile Page" "/profile"

        Test-APIEndpoint "Users" "Get All Users" "GET" "/api/users" -Headers $authHeaders -ExpectedStatuses @(200)
        Test-APIEndpoint "Users" "Search Users" "GET" "/api/users/search?searchTerm=admin&limit=5" -Headers $authHeaders -ExpectedStatuses @(200)
        Test-APIEndpoint "Users" "Get User by ID" "GET" "/api/users/$UserId" -Headers $authHeaders -ExpectedStatuses @(200)

        # Create user
        $userCode = Get-Random -Minimum 100000 -Maximum 999999
        $createUserBody = @{
            firstName = "UI Test User"
            lastName = "$userCode"
            email = "uitest$userCode@example.com"
            employeeCode = "UI_EMP_$userCode"
            password = "Test@123456"
            companyId = $CompanyId
            phone = "+1234567890"
            jobTitle = "Test User"
            isActive = $true
        } | ConvertTo-Json

        $createdUser = Test-APIEndpoint "Users" "Create User (All Fields)" "POST" "/api/users" `
            -Body $createUserBody -Headers $authHeaders -ExpectedStatuses @(200, 201)

        if ($createdUser -and $createdUser.data) {
            $testUserId = $createdUser.data.id

            # Update user
            $updateUserBody = @{
                firstName = "Updated UI Test User"
                lastName = "$userCode"
                phone = "+0987654321"
                jobTitle = "Senior Test User"
                isActive = $true
            } | ConvertTo-Json

            Test-APIEndpoint "Users" "Update User" "PUT" "/api/users/$testUserId" `
                -Body $updateUserBody -Headers $authHeaders -ExpectedStatuses @(200)

            # Deactivate user
            Test-APIEndpoint "Users" "Deactivate User" "POST" "/api/users/$testUserId/deactivate" `
                -Headers $authHeaders -ExpectedStatuses @(200)

            # Delete user
            Test-APIEndpoint "Users" "Delete User" "DELETE" "/api/users/$testUserId" `
                -Headers $authHeaders -ExpectedStatuses @(200)
        }

        # Validation: Invalid email
        $invalidUserBody = @{
            firstName = "Test"
            lastName = "User"
            email = "invalid-email"
            employeeCode = "TEST001"
            password = "Test@123"
            companyId = $CompanyId
        } | ConvertTo-Json

        Test-APIEndpoint "Users" "Validation: Invalid Email" "POST" "/api/users" `
            -Body $invalidUserBody -Headers $authHeaders -ExpectedStatuses @(400) -ExpectFailure

        Write-Log ""

        # ============================================
        # MODULE 5: STATUS MASTER TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 5: STATUS MASTER TESTING"
        Write-Log "============================================="

        Test-UIPage "Status Master" "Status Master Page" "/admin/status-master-management"

        Test-APIEndpoint "Status Master" "Get All Status Masters" "GET" "/api/ComplaintStatusMaster?includeSystem=true" -Headers $authHeaders -ExpectedStatuses @(200)

        # Create custom status
        $statusCode = Get-Random -Minimum 100000 -Maximum 999999
        $createStatusBody = @{
            name = "UI Test Status $statusCode"
            code = "UI_STATUS_$statusCode"
            description = "Custom status for UI/UX validation"
            displayOrder = 999
            colorCode = "#FF5733"
            iconClass = "bi-star"
            isActive = $true
            isFinal = $false
            companyId = $CompanyId
        } | ConvertTo-Json

        $createdStatus = Test-APIEndpoint "Status Master" "Create Status Master" "POST" "/api/ComplaintStatusMaster" `
            -Body $createStatusBody -Headers $authHeaders -ExpectedStatuses @(200, 201)

        if ($createdStatus -and $createdStatus.data) {
            $statusId = $createdStatus.data.id

            # Update status
            $updateStatusBody = @{
                id = $statusId
                name = "Updated UI Test Status $statusCode"
                code = "UI_STATUS_$statusCode"
                description = "Updated custom status"
                displayOrder = 998
                colorCode = "#33FF57"
                iconClass = "bi-check"
                isActive = $true
                isFinal = $false
            } | ConvertTo-Json

            Test-APIEndpoint "Status Master" "Update Status Master" "PUT" "/api/ComplaintStatusMaster/$statusId" `
                -Body $updateStatusBody -Headers $authHeaders -ExpectedStatuses @(200)

            # Delete status
            Test-APIEndpoint "Status Master" "Delete Status Master" "DELETE" "/api/ComplaintStatusMaster/$statusId" `
                -Headers $authHeaders -ExpectedStatuses @(200)
        }

        Write-Log ""

        # ============================================
        # MODULE 6: PRIORITY MASTER TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 6: PRIORITY MASTER TESTING"
        Write-Log "============================================="

        Test-UIPage "Priority Master" "Priority Master Page" "/admin/priority-master-management"

        Test-APIEndpoint "Priority Master" "Get All Priority Masters" "GET" "/api/ComplaintPriorityMaster?includeSystem=true" -Headers $authHeaders -ExpectedStatuses @(200)

        # Create custom priority
        $priorityCode = Get-Random -Minimum 100000 -Maximum 999999
        $createPriorityBody = @{
            name = "UI Test Priority $priorityCode"
            code = "UI_PRIORITY_$priorityCode"
            description = "Custom priority for UI/UX validation"
            displayOrder = 999
            level = 3
            colorCode = "#9C27B0"
            iconClass = "bi-flag"
            slaResponseHours = 12
            slaResolutionHours = 48
            isActive = $true
            companyId = $CompanyId
        } | ConvertTo-Json

        $createdPriority = Test-APIEndpoint "Priority Master" "Create Priority Master" "POST" "/api/ComplaintPriorityMaster" `
            -Body $createPriorityBody -Headers $authHeaders -ExpectedStatuses @(200, 201)

        if ($createdPriority -and $createdPriority.data) {
            $priorityId = $createdPriority.data.id

            # Update priority
            $updatePriorityBody = @{
                id = $priorityId
                name = "Updated UI Test Priority $priorityCode"
                code = "UI_PRIORITY_$priorityCode"
                description = "Updated custom priority"
                displayOrder = 998
                level = 4
                colorCode = "#E91E63"
                iconClass = "bi-exclamation"
                slaResponseHours = 6
                slaResolutionHours = 24
                isActive = $true
            } | ConvertTo-Json

            Test-APIEndpoint "Priority Master" "Update Priority Master" "PUT" "/api/ComplaintPriorityMaster/$priorityId" `
                -Body $updatePriorityBody -Headers $authHeaders -ExpectedStatuses @(200)

            # Delete priority
            Test-APIEndpoint "Priority Master" "Delete Priority Master" "DELETE" "/api/ComplaintPriorityMaster/$priorityId" `
                -Headers $authHeaders -ExpectedStatuses @(200)
        }

        Write-Log ""

        # ============================================
        # MODULE 7: ORGANIZATION STRUCTURE TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 7: ORGANIZATION STRUCTURE TESTING"
        Write-Log "============================================="

        Test-UIPage "Branches" "Branch Management Page" "/admin/branch-management"
        Test-APIEndpoint "Branches" "Get All Branches" "GET" "/api/branches?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200)

        Test-UIPage "Departments" "Department Management Page" "/admin/department-management"
        Test-APIEndpoint "Departments" "Get Departments" "GET" "/api/departments" -Headers $authHeaders -ExpectedStatuses @(200)

        Test-UIPage "Sections" "Section Management Page" "/admin/section-management"
        Test-APIEndpoint "Sections" "Get Sections" "GET" "/api/sections" -Headers $authHeaders -ExpectedStatuses @(200)

        Write-Log ""

        # ============================================
        # MODULE 8: ESCALATION MANAGEMENT TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 8: ESCALATION MANAGEMENT TESTING"
        Write-Log "============================================="

        Test-UIPage "Escalation" "Escalation Policy Page" "/admin/escalation-policy"
        Test-UIPage "Escalation" "Escalation Wizard Page" "/admin/escalation-wizard"
        Test-APIEndpoint "Escalation" "Get Escalation Matrices" "GET" "/api/escalation/matrices" -Headers $authHeaders -ExpectedStatuses @(200)

        Write-Log ""

        # ============================================
        # MODULE 9: NOTIFICATION SETTINGS TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 9: NOTIFICATION SETTINGS TESTING"
        Write-Log "============================================="

        Test-UIPage "Notifications" "Email Settings Page" "/admin/email-settings"
        Test-UIPage "Notifications" "SMS Gateway Page" "/admin/sms-gateway-management"
        Test-UIPage "Notifications" "WhatsApp Settings Page" "/admin/whatsapp-settings"
        Test-UIPage "Notifications" "Template Management Page" "/admin/template-management"
        Test-UIPage "Notifications" "Notification Rules Page" "/admin/notification-rule-management"

        Write-Log ""

        # ============================================
        # MODULE 10: ORYGGI INTEGRATION TESTING
        # ============================================
        Write-Log "============================================="
        Write-Log "MODULE 10: ORYGGI INTEGRATION TESTING"
        Write-Log "============================================="

        Test-UIPage "Oryggi" "Oryggi Sync Page" "/admin/oryggi-sync"

        Write-Log ""

    } else {
        Write-Log "Authentication failed - Cannot proceed with tests" "FAIL"
        $script:failedTests += 50
        $script:totalTests += 50
    }
} catch {
    Write-Log "Authentication error: $($_.Exception.Message)" "FAIL"
    $script:failedTests += 50
    $script:totalTests += 50
}

# Summary
Write-Log "============================================="
Write-Log "TEST EXECUTION COMPLETED"
Write-Log "============================================="
Write-Log ""
Write-Log "Total Tests Executed: $totalTests"
Write-Log "Tests Passed: $passedTests"
Write-Log "Tests Failed: $failedTests"
$successRate = if ($totalTests -gt 0) { [math]::Round(($passedTests / $totalTests) * 100, 2) } else { 0 }
Write-Log "Success Rate: $successRate%"
Write-Log ""

if ($failedTests -eq 0) {
    Write-Log "=============================================" "PASS"
    Write-Log "   100% SUCCESS RATE ACHIEVED!" "PASS"
    Write-Log "=============================================" "PASS"
    Write-Log ""
    Write-Log "All implemented features validated successfully!" "PASS"
} else {
    Write-Log "WARNING: $failedTests test(s) failed" "FAIL"
    Write-Log "Review failed tests above for details" "FAIL"
}

Write-Log ""
Write-Log "Detailed results saved to: $resultsFile"

if ($failedTests -eq 0) { exit 0 } else { exit 1 }
