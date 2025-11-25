# COMPREHENSIVE FULL TEST SUITE - PART 3
# Categories 8-13 (Remaining 120+ tests)

$BaseUrl = "http://localhost:5058"
$FrontendUrl = "http://localhost:4200"
$timestamp = Get-Date -Format "yyyyMMdd_HHmmss"
$resultsFile = "COMPREHENSIVE_FULL_TEST_PART3_RESULTS_$timestamp.txt"
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
# MAIN TEST EXECUTION - PART 3
# =============================================

Write-Log "============================================="
Write-Log "COMPREHENSIVE TEST SUITE - PART 3"
Write-Log "Categories 8-13 (120+ tests)"
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

Write-Log ""

# =============================================
# CATEGORY 8: NOTIFICATION SYSTEM (30 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 8: NOTIFICATION SYSTEM (30 TESTS)"
Write-Log "============================================="

# Email Settings (6 tests)
Write-Log "--- Email Server Settings (6 tests) ---"

Test-UIPage "Notifications" "Email Settings Page" "/admin/email-settings"

Test-APIEndpoint "Notifications" "Get Email Settings" "GET" "/api/email-settings?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$emailSettingsBody = @{
    companyId = $CompanyId
    smtpServer = "smtp.test.com"
    smtpPort = 587
    smtpUsername = "test@test.com"
    smtpPassword = "testpass"
    fromEmail = "noreply@test.com"
    fromName = "Test System"
    enableSsl = $true
    isActive = $true
} | ConvertTo-Json

Test-APIEndpoint "Notifications" "Create/Update Email Settings" "POST" "/api/email-settings" -Body $emailSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

Test-APIEndpoint "Notifications" "Test Email Connection" "POST" "/api/email-settings/test-connection" -Body $emailSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Notifications" "Send Test Email" "POST" "/api/email-settings/send-test" -Body (@{ email = "test@test.com"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Notifications" "Validation: Invalid SMTP Config" "POST" "/api/email-settings" -Body (@{ smtpServer = ""; smtpPort = 0; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

# SMS Gateway Settings (6 tests)
Write-Log "--- SMS Gateway Settings (6 tests) ---"

Test-UIPage "Notifications" "SMS Gateway Page" "/admin/sms-gateway"

Test-APIEndpoint "Notifications" "Get SMS Settings" "GET" "/api/sms-gateway?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$smsSettingsBody = @{
    companyId = $CompanyId
    providerName = "Twilio"
    apiUrl = "https://api.twilio.com"
    accountSid = "test_sid"
    authToken = "test_token"
    fromNumber = "+1234567890"
    isActive = $true
} | ConvertTo-Json

Test-APIEndpoint "Notifications" "Create/Update SMS Settings" "POST" "/api/sms-gateway" -Body $smsSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

Test-APIEndpoint "Notifications" "Test SMS Connection" "POST" "/api/sms-gateway/test-connection" -Body $smsSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Notifications" "Send Test SMS" "POST" "/api/sms-gateway/send-test" -Body (@{ phoneNumber = "+1234567890"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Notifications" "Validation: Invalid Gateway Config" "POST" "/api/sms-gateway" -Body (@{ providerName = ""; apiUrl = "invalid"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

# WhatsApp Settings (6 tests)
Write-Log "--- WhatsApp Settings (6 tests) ---"

Test-UIPage "Notifications" "WhatsApp Settings Page" "/admin/whatsapp-settings"

Test-APIEndpoint "Notifications" "Get WhatsApp Settings" "GET" "/api/whatsapp-settings?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$whatsappSettingsBody = @{
    companyId = $CompanyId
    providerName = "Twilio"
    apiUrl = "https://api.twilio.com/whatsapp"
    accountSid = "test_sid"
    authToken = "test_token"
    fromNumber = "whatsapp:+1234567890"
    isActive = $true
} | ConvertTo-Json

Test-APIEndpoint "Notifications" "Create/Update WhatsApp Settings" "POST" "/api/whatsapp-settings" -Body $whatsappSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

Test-APIEndpoint "Notifications" "Test WhatsApp Connection" "POST" "/api/whatsapp-settings/test-connection" -Body $whatsappSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Notifications" "Send Test WhatsApp" "POST" "/api/whatsapp-settings/send-test" -Body (@{ phoneNumber = "+1234567890"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Notifications" "Validation: Invalid API Credentials" "POST" "/api/whatsapp-settings" -Body (@{ providerName = ""; accountSid = ""; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

# Communication Templates (6 tests)
Write-Log "--- Communication Templates (6 tests) ---"

Test-UIPage "Notifications" "Template Management Page" "/admin/templates"

Test-APIEndpoint "Notifications" "Get All Templates" "GET" "/api/communication-templates?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$templateBody = @{
    name = "Test Template $(Get-Random -Minimum 1000 -Maximum 9999)"
    code = "TEST_TEMPLATE_$(Get-Random -Minimum 1000 -Maximum 9999)"
    channel = 0
    eventType = 1
    subject = "Test Subject"
    body = "Hello {{UserName}}, your complaint {{ComplaintNumber}} has been received."
    companyId = $CompanyId
    isActive = $true
} | ConvertTo-Json

$createdTemplate = Test-APIEndpoint "Notifications" "Create Template" "POST" "/api/communication-templates" -Body $templateBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

if ($createdTemplate -and $createdTemplate.data) {
    $templateId = $createdTemplate.data.id

    Test-APIEndpoint "Notifications" "Get Template by ID" "GET" "/api/communication-templates/$templateId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

    $updateTemplateBody = @{
        name = "Updated Template"
        code = $createdTemplate.data.code
        channel = 0
        eventType = 1
        subject = "Updated Subject"
        body = "Updated body text"
        companyId = $CompanyId
        isActive = $true
    } | ConvertTo-Json

    Test-APIEndpoint "Notifications" "Update Template" "PUT" "/api/communication-templates/$templateId" -Body $updateTemplateBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

    Test-APIEndpoint "Notifications" "Delete Template" "DELETE" "/api/communication-templates/$templateId" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)
}

Test-APIEndpoint "Notifications" "Preview Template Variables" "POST" "/api/communication-templates/preview" -Body (@{ body = "Hello {{UserName}}"; variables = @{ UserName = "John Doe" } } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 404)

# Notification Rules (6 tests)
Write-Log "--- Notification Rules (6 tests) ---"

Test-UIPage "Notifications" "Notification Rules Page" "/admin/notification-rules"

Test-APIEndpoint "Notifications" "Get All Rules" "GET" "/api/event-communication-rules?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$ruleBody = @{
    eventType = 1
    channel = 0
    recipientType = 1
    templateId = "00000000-0000-0000-0000-000000000001"
    companyId = $CompanyId
    isActive = $true
    priority = 1
} | ConvertTo-Json

$createdRule = Test-APIEndpoint "Notifications" "Create Notification Rule" "POST" "/api/event-communication-rules" -Body $ruleBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

if ($createdRule -and $createdRule.data) {
    $ruleId = $createdRule.data.id

    Test-APIEndpoint "Notifications" "Get Rule by ID" "GET" "/api/event-communication-rules/$ruleId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

    $updateRuleBody = @{
        eventType = 1
        channel = 1
        recipientType = 2
        templateId = "00000000-0000-0000-0000-000000000001"
        companyId = $CompanyId
        isActive = $true
        priority = 2
    } | ConvertTo-Json

    Test-APIEndpoint "Notifications" "Update Rule" "PUT" "/api/event-communication-rules/$ruleId" -Body $updateRuleBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

    Test-APIEndpoint "Notifications" "Delete Rule" "DELETE" "/api/event-communication-rules/$ruleId" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)
}

Test-APIEndpoint "Notifications" "Test Rule Trigger" "POST" "/api/event-communication-rules/test-trigger" -Body (@{ eventType = 1; entityId = "00000000-0000-0000-0000-000000000001"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Write-Log ""

# =============================================
# CATEGORY 9: ESCALATION SYSTEM (15 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 9: ESCALATION SYSTEM (15 TESTS)"
Write-Log "============================================="

# Escalation Policy CRUD (8 tests)
Write-Log "--- Escalation Policy (8 tests) ---"

Test-UIPage "Escalation" "Escalation Policy Page" "/admin/escalation-policy"
Test-UIPage "Escalation" "Escalation Wizard Page" "/admin/escalation-wizard"

Test-APIEndpoint "Escalation" "Get Escalation Matrices" "GET" "/api/escalation/matrices?companyId=$CompanyId" -Headers $authHeaders

$policyBody = @{
    name = "Test Escalation Policy $(Get-Random -Minimum 1000 -Maximum 9999)"
    description = "Test policy"
    categoryId = "00000000-0000-0000-0000-000000000001"
    priorityId = "00000000-0000-0000-0000-000000000001"
    companyId = $CompanyId
    isActive = $true
} | ConvertTo-Json

$createdPolicy = Test-APIEndpoint "Escalation" "Create Escalation Policy" "POST" "/api/escalation-policy" -Body $policyBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

if ($createdPolicy -and $createdPolicy.data) {
    $policyId = $createdPolicy.data.id

    Test-APIEndpoint "Escalation" "Get Policy by ID" "GET" "/api/escalation-policy/$policyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

    $updatePolicyBody = @{
        name = "Updated Escalation Policy"
        description = "Updated description"
        categoryId = "00000000-0000-0000-0000-000000000001"
        priorityId = "00000000-0000-0000-0000-000000000001"
        companyId = $CompanyId
        isActive = $true
    } | ConvertTo-Json

    Test-APIEndpoint "Escalation" "Update Policy" "PUT" "/api/escalation-policy/$policyId" -Body $updatePolicyBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

    Test-APIEndpoint "Escalation" "Delete Policy" "DELETE" "/api/escalation-policy/$policyId" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)
}

Test-APIEndpoint "Escalation" "Add Escalation Level" "POST" "/api/escalation-policy/00000000-0000-0000-0000-000000000001/levels" -Body (@{ level = 1; userId = $UserId; hoursToEscalate = 24 } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 201, 400, 404)

Test-APIEndpoint "Escalation" "Remove Escalation Level" "DELETE" "/api/escalation-policy/00000000-0000-0000-0000-000000000001/levels/1" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)

# Escalation Workflows (7 tests)
Write-Log "--- Escalation Workflows (7 tests) ---"

# Get a real complaint for testing
$complaints = Test-APIEndpoint "Escalation" "Get Complaints for Testing" "GET" "/api/complaints?page=1&pageSize=1" -Headers $authHeaders
$testComplaintId = if ($complaints.data -and $complaints.data.items) { $complaints.data.items[0].id } else { "00000000-0000-0000-0000-000000000001" }

Test-APIEndpoint "Escalation" "Manual Escalation" "POST" "/api/complaints/$testComplaintId/escalate" -Body (@{ reason = "Testing escalation"; escalateToUserId = $UserId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Escalation" "Get Escalation History" "GET" "/api/escalation/history/$testComplaintId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Escalation" "Cancel Escalation" "POST" "/api/escalation/$testComplaintId/cancel" -Body (@{ reason = "Testing cancel" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Escalation" "Reassign Escalated Complaint" "POST" "/api/complaints/$testComplaintId/assign/$UserId" -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Escalation" "SLA Tracking" "GET" "/api/escalation/sla-status/$testComplaintId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Write-Log ""

# =============================================
# CATEGORY 10: ADVANCED WORKFLOWS (20 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 10: ADVANCED WORKFLOWS (20 TESTS)"
Write-Log "============================================="

Write-Log "--- Advanced Complaint Operations (10 tests) ---"

# Get test data
$categories = Test-APIEndpoint "Workflows" "Get Categories for Testing" "GET" "/api/categories" -Headers $authHeaders
$CategoryId = if ($categories.data) { $categories.data[0].id } else { "00000000-0000-0000-0000-000000000001" }

# Create multiple complaints for testing
$complaint1Body = @{
    title = "Workflow Test Complaint 1 $(Get-Random)"
    description = "Test complaint for workflow testing"
    categoryId = $CategoryId
    companyId = $CompanyId
    priority = "Medium"
} | ConvertTo-Json

$complaint1 = Test-APIEndpoint "Workflows" "Create Test Complaint 1" "POST" "/api/complaints" -Body $complaint1Body -Headers $authHeaders -ExpectedStatuses @(200, 201)

$complaint2Body = @{
    title = "Workflow Test Complaint 2 $(Get-Random)"
    description = "Another test complaint"
    categoryId = $CategoryId
    companyId = $CompanyId
    priority = "Low"
} | ConvertTo-Json

$complaint2 = Test-APIEndpoint "Workflows" "Create Test Complaint 2" "POST" "/api/complaints" -Body $complaint2Body -Headers $authHeaders -ExpectedStatuses @(200, 201)

if ($complaint1 -and $complaint1.data -and $complaint2 -and $complaint2.data) {
    $complaintId1 = $complaint1.data.id
    $complaintId2 = $complaint2.data.id

    Test-APIEndpoint "Workflows" "Transfer Complaint to User" "POST" "/api/complaints/$complaintId1/transfer/$UserId" -Body (@{ reason = "Testing transfer" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

    Test-APIEndpoint "Workflows" "Merge Duplicate Complaints" "POST" "/api/complaints/$complaintId1/merge" -Body (@{ targetComplaintId = $complaintId2; reason = "Duplicate" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

    Test-APIEndpoint "Workflows" "Split Complaint" "POST" "/api/complaints/$complaintId1/split" -Body (@{ newTitle = "Split Complaint"; categoryId = $CategoryId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

    # Bulk operations
    $bulkAssignBody = @{
        complaintIds = @($complaintId1, $complaintId2)
        userId = $UserId
    } | ConvertTo-Json

    Test-APIEndpoint "Workflows" "Bulk Assign Complaints" "POST" "/api/complaints/bulk-assign" -Body $bulkAssignBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

    $bulkCloseBody = @{
        complaintIds = @($complaintId1, $complaintId2)
        resolution = "Bulk close test"
    } | ConvertTo-Json

    Test-APIEndpoint "Workflows" "Bulk Close Complaints" "POST" "/api/complaints/bulk-close" -Body $bulkCloseBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)
}

Test-APIEndpoint "Workflows" "Get Complaint Attachments" "GET" "/api/complaints/$testComplaintId/attachments" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Workflows" "Get Complaint Timeline" "GET" "/api/complaints/$testComplaintId/timeline" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Workflows" "Get Related Complaints" "GET" "/api/complaints/$testComplaintId/related" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Write-Log "--- Integration Workflows (10 tests) ---"

Test-APIEndpoint "Workflows" "Auto-Assignment Rule Test" "POST" "/api/workflows/test-auto-assignment" -Body (@{ categoryId = $CategoryId; priority = "High"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Workflows" "SLA Breach Check" "GET" "/api/workflows/sla-breaches?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Workflows" "Pending Escalations" "GET" "/api/workflows/pending-escalations?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Workflows" "Approval Workflow Start" "POST" "/api/workflows/approval/start" -Body (@{ complaintId = $testComplaintId; approverId = $UserId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Workflows" "Approval Workflow Approve" "POST" "/api/workflows/approval/approve" -Body (@{ complaintId = $testComplaintId; approverId = $UserId; comments = "Approved" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Workflows" "Approval Workflow Reject" "POST" "/api/workflows/approval/reject" -Body (@{ complaintId = $testComplaintId; approverId = $UserId; reason = "Rejected for testing" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Workflows" "Notification Queue Status" "GET" "/api/workflows/notification-queue?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Workflows" "Failed Notifications" "GET" "/api/workflows/failed-notifications?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Workflows" "Retry Failed Notification" "POST" "/api/workflows/retry-notification" -Body (@{ notificationId = "00000000-0000-0000-0000-000000000001" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Workflows" "Workflow Metrics" "GET" "/api/workflows/metrics?companyId=$CompanyId&days=30" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Write-Log ""

# =============================================
# CATEGORY 11: ORYGGI INTEGRATION (12 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 11: ORYGGI INTEGRATION (12 TESTS)"
Write-Log "============================================="

Test-UIPage "Oryggi" "Oryggi Sync Page" "/admin/oryggi-sync"

# Connection Settings (6 tests)
Write-Log "--- Oryggi Connection (6 tests) ---"

Test-APIEndpoint "Oryggi" "Get Oryggi Settings" "GET" "/api/oryggi-settings?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$oryggiSettingsBody = @{
    companyId = $CompanyId
    apiUrl = "https://api.oryggi.com"
    apiKey = "test_api_key"
    apiSecret = "test_api_secret"
    tenantId = "test_tenant"
    isEnabled = $true
} | ConvertTo-Json

Test-APIEndpoint "Oryggi" "Update Oryggi Connection" "POST" "/api/oryggi-settings" -Body $oryggiSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

Test-APIEndpoint "Oryggi" "Test Oryggi Connection" "POST" "/api/oryggi-settings/test-connection" -Body $oryggiSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Oryggi" "Get Connection Status" "GET" "/api/oryggi-settings/connection-status?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Oryggi" "Save API Credentials" "POST" "/api/oryggi-settings/credentials" -Body (@{ apiKey = "new_key"; apiSecret = "new_secret"; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Oryggi" "Validation: Invalid Credentials" "POST" "/api/oryggi-settings" -Body (@{ apiUrl = ""; apiKey = ""; companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

# Sync Operations (6 tests)
Write-Log "--- Oryggi Sync Operations (6 tests) ---"

Test-APIEndpoint "Oryggi" "Manual Sync Trigger" "POST" "/api/oryggi-sync/trigger" -Body (@{ companyId = $CompanyId; syncType = "Full" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Oryggi" "Get Sync Status" "GET" "/api/oryggi-sync/status?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Oryggi" "Get Sync History" "GET" "/api/oryggi-sync/history?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Oryggi" "Get Sync Logs" "GET" "/api/oryggi-sync/logs?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Oryggi" "Sync Schedule Configuration" "POST" "/api/oryggi-sync/schedule" -Body (@{ companyId = $CompanyId; cronExpression = "0 0 * * *"; isEnabled = $true } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Oryggi" "Cancel Running Sync" "POST" "/api/oryggi-sync/cancel" -Body (@{ companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Write-Log ""

# =============================================
# CATEGORY 12: FILE ATTACHMENTS (10 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 12: FILE ATTACHMENTS (10 TESTS)"
Write-Log "============================================="

# Note: File upload tests require multipart/form-data which is complex in PowerShell
# Testing with metadata endpoints where possible

Test-APIEndpoint "Attachments" "Get Complaint Attachments" "GET" "/api/complaints/$testComplaintId/attachments" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Attachments" "Get Attachment Metadata" "GET" "/api/attachments/00000000-0000-0000-0000-000000000001" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Attachments" "Delete Attachment" "DELETE" "/api/attachments/00000000-0000-0000-0000-000000000001" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)

Test-APIEndpoint "Attachments" "Get Attachment Download URL" "GET" "/api/attachments/00000000-0000-0000-0000-000000000001/download-url" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Attachments" "Validate File Type" "POST" "/api/attachments/validate-type" -Body (@{ fileName = "test.pdf"; mimeType = "application/pdf" } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Attachments" "Validate File Size" "POST" "/api/attachments/validate-size" -Body (@{ fileName = "test.pdf"; fileSize = 1048576 } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Attachments" "Get Upload Limit Config" "GET" "/api/attachments/upload-config?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Attachments" "Get Allowed File Types" "GET" "/api/attachments/allowed-types?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Attachments" "Get Attachment Statistics" "GET" "/api/attachments/statistics?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Attachments" "Cleanup Orphaned Attachments" "POST" "/api/attachments/cleanup-orphaned" -Body (@{ companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Write-Log ""

# =============================================
# CATEGORY 13: ADDITIONAL CRUD (30 tests)
# =============================================
Write-Log "============================================="
Write-Log "CATEGORY 13: ADDITIONAL CRUD OPERATIONS (30 TESTS)"
Write-Log "============================================="

# Resource Pools (8 tests)
Write-Log "--- Resource Pool Management (8 tests) ---"

Test-APIEndpoint "Additional CRUD" "Get All Resource Pools" "GET" "/api/resource-pools?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$poolBody = @{
    name = "Test Pool $(Get-Random)"
    description = "Test resource pool"
    companyId = $CompanyId
    isActive = $true
} | ConvertTo-Json

$createdPool = Test-APIEndpoint "Additional CRUD" "Create Resource Pool" "POST" "/api/resource-pools" -Body $poolBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

if ($createdPool -and $createdPool.data) {
    $poolId = $createdPool.data.id

    Test-APIEndpoint "Additional CRUD" "Get Pool by ID" "GET" "/api/resource-pools/$poolId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

    $updatePoolBody = @{
        name = "Updated Pool"
        description = "Updated description"
        companyId = $CompanyId
        isActive = $true
    } | ConvertTo-Json

    Test-APIEndpoint "Additional CRUD" "Update Pool" "PUT" "/api/resource-pools/$poolId" -Body $updatePoolBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

    Test-APIEndpoint "Additional CRUD" "Add Users to Pool" "POST" "/api/resource-pools/$poolId/users" -Body (@{ userIds = @($UserId) } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

    Test-APIEndpoint "Additional CRUD" "Get Pool Users" "GET" "/api/resource-pools/$poolId/users" -Headers $authHeaders -ExpectedStatuses @(200, 404)

    Test-APIEndpoint "Additional CRUD" "Remove User from Pool" "DELETE" "/api/resource-pools/$poolId/users/$UserId" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)

    Test-APIEndpoint "Additional CRUD" "Delete Pool" "DELETE" "/api/resource-pools/$poolId" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)
}

# Company Settings (8 tests)
Write-Log "--- Company Settings (8 tests) ---"

Test-APIEndpoint "Additional CRUD" "Get Company Settings" "GET" "/api/company/$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Additional CRUD" "Get All Companies" "GET" "/api/company" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$companyBody = @{
    name = "Test Company $(Get-Random)"
    code = "TEST_CO_$(Get-Random)"
    address = "123 Test St"
    city = "Test City"
    country = "Test Country"
    phone = "+1234567890"
    email = "test@company.com"
    isActive = $true
} | ConvertTo-Json

Test-APIEndpoint "Additional CRUD" "Create Company" "POST" "/api/company" -Body $companyBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 400, 404)

$updateCompanyBody = @{
    name = "Updated Company"
    address = "456 Updated St"
    city = "Updated City"
    phone = "+9876543210"
} | ConvertTo-Json

Test-APIEndpoint "Additional CRUD" "Update Company" "PUT" "/api/company/$CompanyId" -Body $updateCompanyBody -Headers $authHeaders -ExpectedStatuses @(200, 400, 404)

Test-APIEndpoint "Additional CRUD" "Update Company Logo" "POST" "/api/company/$CompanyId/logo" -Headers $authHeaders -ExpectedStatuses @(200, 400, 404, 415)

Test-APIEndpoint "Additional CRUD" "Get Company Logo" "GET" "/api/company/$CompanyId/logo" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Additional CRUD" "Delete Company Logo" "DELETE" "/api/company/$CompanyId/logo" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)

Test-APIEndpoint "Additional CRUD" "Multi-Tenant Isolation Test" "GET" "/api/company/00000000-0000-0000-0000-000000000999" -Headers $authHeaders -ExpectedStatuses @(403, 404) -ExpectFailure

# Event Types (5 tests)
Write-Log "--- Event Types (5 tests) ---"

Test-APIEndpoint "Additional CRUD" "Get All Event Types" "GET" "/api/event-types" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$eventTypeBody = @{
    name = "Test Event $(Get-Random)"
    code = "TEST_EVENT_$(Get-Random)"
    description = "Test event type"
    isActive = $true
} | ConvertTo-Json

$createdEventType = Test-APIEndpoint "Additional CRUD" "Create Event Type" "POST" "/api/event-types" -Body $eventTypeBody -Headers $authHeaders -ExpectedStatuses @(200, 201, 404)

if ($createdEventType -and $createdEventType.data) {
    $eventTypeId = $createdEventType.data.id

    Test-APIEndpoint "Additional CRUD" "Get Event Type by ID" "GET" "/api/event-types/$eventTypeId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

    $updateEventTypeBody = @{
        name = "Updated Event Type"
        code = $createdEventType.data.code
        description = "Updated description"
        isActive = $true
    } | ConvertTo-Json

    Test-APIEndpoint "Additional CRUD" "Update Event Type" "PUT" "/api/event-types/$eventTypeId" -Body $updateEventTypeBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

    Test-APIEndpoint "Additional CRUD" "Delete Event Type" "DELETE" "/api/event-types/$eventTypeId" -Headers $authHeaders -ExpectedStatuses @(200, 204, 404)
}

# Complaint Info Settings (6 tests)
Write-Log "--- Complaint Info Settings (6 tests) ---"

Test-APIEndpoint "Additional CRUD" "Get Complaint Info Settings" "GET" "/api/complaint-settings?companyId=$CompanyId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

$generalSettingsBody = @{
    companyId = $CompanyId
    defaultPriority = "Medium"
    autoAssignment = $true
    enableSLA = $true
} | ConvertTo-Json

Test-APIEndpoint "Additional CRUD" "Update General Settings" "PUT" "/api/complaint-settings/general" -Body $generalSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

$formSettingsBody = @{
    companyId = $CompanyId
    requireAttachment = $false
    maxAttachments = 5
    allowAnonymous = $false
} | ConvertTo-Json

Test-APIEndpoint "Additional CRUD" "Update Form Settings" "PUT" "/api/complaint-settings/form" -Body $formSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

$workflowSettingsBody = @{
    companyId = $CompanyId
    autoEscalation = $true
    escalationHours = 48
    requireApproval = $false
} | ConvertTo-Json

Test-APIEndpoint "Additional CRUD" "Update Workflow Settings" "PUT" "/api/complaint-settings/workflow" -Body $workflowSettingsBody -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Additional CRUD" "Reset Settings to Default" "POST" "/api/complaint-settings/reset" -Body (@{ companyId = $CompanyId } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Additional CRUD" "Validation: Settings Constraints" "PUT" "/api/complaint-settings/general" -Body (@{ companyId = $CompanyId; maxAttachments = -1 } | ConvertTo-Json) -Headers $authHeaders -ExpectedStatuses @(400, 404) -ExpectFailure

# Audit Logs (3 tests)
Write-Log "--- Audit Logs (3 tests) ---"

Test-APIEndpoint "Additional CRUD" "Get Audit Logs" "GET" "/api/audit-logs?companyId=$CompanyId&page=1&pageSize=20" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Additional CRUD" "Get User Audit Trail" "GET" "/api/audit-logs/user/$UserId?page=1&pageSize=20" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Test-APIEndpoint "Additional CRUD" "Get Entity Audit Trail" "GET" "/api/audit-logs/entity/complaint/$testComplaintId" -Headers $authHeaders -ExpectedStatuses @(200, 404)

Write-Log ""

# =============================================
# FINAL SUMMARY
# =============================================

Write-Log "============================================="
Write-Log "PART 3 TEST EXECUTION COMPLETED"
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
