# Comprehensive Auto-Response Email System Test Script
# Tests all auto-response features: acknowledgment, status change, assignment, resolution

param(
    [string]$BaseUrl = "http://localhost:5000/api",
    [string]$AdminEmail = "admin@oryggi.is",
    [string]$AdminPassword = "Admin@123"
)

$ErrorActionPreference = "Continue"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "AUTO-RESPONSE SYSTEM COMPREHENSIVE TEST" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# Test results tracking
$testResults = @{
    Passed = 0
    Failed = 0
    Tests = @()
}

function Test-Endpoint {
    param($Name, $Result)

    if ($Result) {
        Write-Host "[PASS] $Name" -ForegroundColor Green
        $testResults.Passed++
    } else {
        Write-Host "[FAIL] $Name" -ForegroundColor Red
        $testResults.Failed++
    }

    $testResults.Tests += @{
        Name = $Name
        Passed = $Result
        Timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    }
}

# ========================================
# 1. AUTHENTICATION
# ========================================
Write-Host "`n1. AUTHENTICATING..." -ForegroundColor Yellow

try {
    $loginPayload = @{
        email = $AdminEmail
        password = $AdminPassword
    } | ConvertTo-Json

    $loginResponse = Invoke-RestMethod -Uri "$BaseUrl/auth/login" `
        -Method Post `
        -Body $loginPayload `
        -ContentType "application/json"

    $token = $loginResponse.data.token
    $companyId = $loginResponse.data.user.companyId
    $userId = $loginResponse.data.user.id

    Write-Host "   Authenticated as: $($loginResponse.data.user.fullName)" -ForegroundColor Green
    Write-Host "   Company ID: $companyId" -ForegroundColor Gray
    Write-Host "   User ID: $userId" -ForegroundColor Gray

    Test-Endpoint "Authentication" $true
} catch {
    Write-Host "   Authentication failed: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Authentication" $false
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

# ========================================
# 2. CHECK EMAIL CONFIGURATION
# ========================================
Write-Host "`n2. CHECKING EMAIL CONFIGURATION..." -ForegroundColor Yellow

try {
    $emailConfigs = Invoke-RestMethod -Uri "$BaseUrl/email-configuration" `
        -Method Get `
        -Headers $headers

    $emailConfig = $emailConfigs.data | Where-Object { $_.isEnabled -eq $true } | Select-Object -First 1

    if ($emailConfig) {
        Write-Host "   Email Config Found:" -ForegroundColor Green
        Write-Host "     - ID: $($emailConfig.id)" -ForegroundColor Gray
        Write-Host "     - From: $($emailConfig.fromEmail)" -ForegroundColor Gray
        Write-Host "     - Auto-Acknowledgment: $($emailConfig.sendAutoAcknowledgement)" -ForegroundColor Gray
        Write-Host "     - Template: $($emailConfig.autoAcknowledgementTemplateId)" -ForegroundColor Gray

        Test-Endpoint "Email Configuration Check" $true
    } else {
        Write-Host "   No active email configuration found" -ForegroundColor Yellow
        Write-Host "   Auto-responses will use NotificationDispatcher instead" -ForegroundColor Yellow
        Test-Endpoint "Email Configuration Check" $false
    }
} catch {
    Write-Host "   Error checking email config: $($_.Exception.Message)" -ForegroundColor Yellow
    Test-Endpoint "Email Configuration Check" $false
}

# ========================================
# 3. GET MASTER DATA FOR TESTS
# ========================================
Write-Host "`n3. FETCHING MASTER DATA..." -ForegroundColor Yellow

try {
    # Get Categories
    $categories = Invoke-RestMethod -Uri "$BaseUrl/master/complaint-categories" `
        -Method Get -Headers $headers
    $categoryId = $categories.data[0].id
    Write-Host "   Category ID: $categoryId" -ForegroundColor Gray

    # Get Priorities
    $priorities = Invoke-RestMethod -Uri "$BaseUrl/master/complaint-priorities" `
        -Method Get -Headers $headers
    $priorityId = $priorities.data[0].id
    Write-Host "   Priority ID: $priorityId" -ForegroundColor Gray

    # Get Statuses
    $statuses = Invoke-RestMethod -Uri "$BaseUrl/master/complaint-statuses" `
        -Method Get -Headers $headers
    $submittedStatusId = ($statuses.data | Where-Object { $_.name -eq "Submitted" }).id
    $inProgressStatusId = ($statuses.data | Where-Object { $_.name -eq "In Progress" }).id
    $resolvedStatusId = ($statuses.data | Where-Object { $_.name -eq "Resolved" }).id
    Write-Host "   Status IDs: Submitted=$submittedStatusId, InProgress=$inProgressStatusId, Resolved=$resolvedStatusId" -ForegroundColor Gray

    # Get Handler User
    $users = Invoke-RestMethod -Uri "$BaseUrl/users" -Method Get -Headers $headers
    $handlerUser = $users.data | Where-Object { $_.id -ne $userId } | Select-Object -First 1
    $handlerId = $handlerUser.id
    Write-Host "   Handler ID: $handlerId ($($handlerUser.fullName))" -ForegroundColor Gray

    Test-Endpoint "Master Data Fetch" $true
} catch {
    Write-Host "   Error fetching master data: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Master Data Fetch" $false
    exit 1
}

# ========================================
# 4. TEST COMPLAINT CREATION AUTO-RESPONSE
# ========================================
Write-Host "`n4. TESTING COMPLAINT CREATION AUTO-RESPONSE..." -ForegroundColor Yellow

try {
    $complaintPayload = @{
        title = "Auto-Response Test - Complaint Created $(Get-Date -Format 'HH:mm:ss')"
        description = "Testing auto-acknowledgment email when complaint is created via web interface"
        categoryId = $categoryId
        priorityMasterId = $priorityId
        complainantId = $userId
        companyId = $companyId
        isAnonymous = $false
    } | ConvertTo-Json

    $createResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints" `
        -Method Post `
        -Headers $headers `
        -Body $complaintPayload

    $complaintId = $createResponse.data.id
    $complaintNumber = $createResponse.data.complaintNumber

    Write-Host "   Complaint Created:" -ForegroundColor Green
    Write-Host "     - ID: $complaintId" -ForegroundColor Gray
    Write-Host "     - Number: $complaintNumber" -ForegroundColor Gray
    Write-Host "     - Auto-acknowledgment should be sent to complainant" -ForegroundColor Cyan

    Test-Endpoint "Complaint Creation Auto-Response Trigger" $true
} catch {
    Write-Host "   Error creating complaint: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Complaint Creation Auto-Response Trigger" $false
}

Start-Sleep -Seconds 2

# ========================================
# 5. TEST ASSIGNMENT AUTO-RESPONSE
# ========================================
Write-Host "`n5. TESTING ASSIGNMENT AUTO-RESPONSE..." -ForegroundColor Yellow

try {
    $assignPayload = @{
        complaintId = $complaintId
        assignedToId = $handlerId
    } | ConvertTo-Json

    $assignResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints/$complaintId/assign" `
        -Method Post `
        -Headers $headers `
        -Body $assignPayload

    Write-Host "   Complaint Assigned:" -ForegroundColor Green
    Write-Host "     - Assigned To: $($assignResponse.data.assignedToName)" -ForegroundColor Gray
    Write-Host "     - Assignment notification should be sent to handler" -ForegroundColor Cyan

    Test-Endpoint "Assignment Auto-Response Trigger" $true
} catch {
    Write-Host "   Error assigning complaint: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Assignment Auto-Response Trigger" $false
}

Start-Sleep -Seconds 2

# ========================================
# 6. TEST STATUS CHANGE AUTO-RESPONSE
# ========================================
Write-Host "`n6. TESTING STATUS CHANGE AUTO-RESPONSE..." -ForegroundColor Yellow

try {
    # Get current complaint state
    $complaint = Invoke-RestMethod -Uri "$BaseUrl/complaints/$complaintId" `
        -Method Get -Headers $headers

    $updatePayload = @{
        id = $complaintId
        title = $complaint.data.title
        description = $complaint.data.description
        categoryId = $complaint.data.categoryId
        priorityMasterId = $complaint.data.priorityMasterId
        statusMasterId = $inProgressStatusId
        tags = $complaint.data.tags
    } | ConvertTo-Json

    $updateResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints/$complaintId" `
        -Method Put `
        -Headers $headers `
        -Body $updatePayload

    Write-Host "   Status Changed:" -ForegroundColor Green
    Write-Host "     - Old Status: $($complaint.data.status)" -ForegroundColor Gray
    Write-Host "     - New Status: $($updateResponse.data.status)" -ForegroundColor Gray
    Write-Host "     - Status change notification should be sent to complainant" -ForegroundColor Cyan

    Test-Endpoint "Status Change Auto-Response Trigger" $true
} catch {
    Write-Host "   Error changing status: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Status Change Auto-Response Trigger" $false
}

Start-Sleep -Seconds 2

# ========================================
# 7. TEST RESOLUTION AUTO-RESPONSE
# ========================================
Write-Host "`n7. TESTING RESOLUTION AUTO-RESPONSE..." -ForegroundColor Yellow

try {
    # Get current complaint state
    $complaint = Invoke-RestMethod -Uri "$BaseUrl/complaints/$complaintId" `
        -Method Get -Headers $headers

    $resolvePayload = @{
        id = $complaintId
        title = $complaint.data.title
        description = $complaint.data.description
        categoryId = $complaint.data.categoryId
        priorityMasterId = $complaint.data.priorityMasterId
        statusMasterId = $resolvedStatusId
        resolutionNotes = "Issue resolved successfully via auto-response testing. All systems working as expected."
        tags = $complaint.data.tags
    } | ConvertTo-Json

    $resolveResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints/$complaintId" `
        -Method Put `
        -Headers $headers `
        -Body $resolvePayload

    Write-Host "   Complaint Resolved:" -ForegroundColor Green
    Write-Host "     - Status: $($resolveResponse.data.status)" -ForegroundColor Gray
    Write-Host "     - Resolution Notes: $($resolveResponse.data.resolutionNotes)" -ForegroundColor Gray
    Write-Host "     - Resolution notification should be sent to complainant" -ForegroundColor Cyan

    Test-Endpoint "Resolution Auto-Response Trigger" $true
} catch {
    Write-Host "   Error resolving complaint: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Resolution Auto-Response Trigger" $false
}

Start-Sleep -Seconds 2

# ========================================
# 8. CHECK EVENT COMMUNICATION RULES
# ========================================
Write-Host "`n8. CHECKING EVENT COMMUNICATION RULES..." -ForegroundColor Yellow

try {
    $rules = Invoke-RestMethod -Uri "$BaseUrl/event-communication-rules" `
        -Method Get -Headers $headers

    $relevantEvents = @(
        "COMPLAINT_CREATED",
        "COMPLAINT_ASSIGNED",
        "COMPLAINT_STATUS_CHANGED",
        "COMPLAINT_RESOLVED"
    )

    foreach ($eventCode in $relevantEvents) {
        $eventRules = $rules.data | Where-Object { $_.eventType.code -eq $eventCode -and $_.isActive }

        if ($eventRules) {
            Write-Host "   $eventCode`: $($eventRules.Count) active rule(s)" -ForegroundColor Green
            foreach ($rule in $eventRules) {
                Write-Host "     - $($rule.name) -> $($rule.recipientType) via $($rule.channel)" -ForegroundColor Gray
            }
        } else {
            Write-Host "   $eventCode`: No active rules configured" -ForegroundColor Yellow
        }
    }

    Test-Endpoint "Event Communication Rules Check" $true
} catch {
    Write-Host "   Error checking rules: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Event Communication Rules Check" $false
}

# ========================================
# 9. CHECK COMMUNICATION LOGS
# ========================================
Write-Host "`n9. CHECKING COMMUNICATION LOGS..." -ForegroundColor Yellow

try {
    $logs = Invoke-RestMethod -Uri "$BaseUrl/communication-logs?limit=20" `
        -Method Get -Headers $headers

    $recentLogs = $logs.data | Where-Object {
        $_.entityId -eq $complaintId
    }

    Write-Host "   Communication Logs for Complaint $complaintNumber`:" -ForegroundColor Green
    Write-Host "   Found $($recentLogs.Count) log entries" -ForegroundColor Gray

    foreach ($log in $recentLogs) {
        $statusColor = if ($log.status -eq "Sent") { "Green" } else { "Red" }
        Write-Host "     - [$($log.status)] $($log.channel) to $($log.recipientEmail)" -ForegroundColor $statusColor
        Write-Host "       Subject: $($log.subject)" -ForegroundColor Gray
        if ($log.errorMessage) {
            Write-Host "       Error: $($log.errorMessage)" -ForegroundColor Red
        }
    }

    Test-Endpoint "Communication Logs Check" $true
} catch {
    Write-Host "   Error checking logs: $($_.Exception.Message)" -ForegroundColor Yellow
    Test-Endpoint "Communication Logs Check" $false
}

# ========================================
# 10. REASSIGNMENT TEST
# ========================================
Write-Host "`n10. TESTING REASSIGNMENT AUTO-RESPONSE..." -ForegroundColor Yellow

try {
    # Reassign back to admin
    $reassignPayload = @{
        complaintId = $complaintId
        assignedToId = $userId
    } | ConvertTo-Json

    $reassignResponse = Invoke-RestMethod -Uri "$BaseUrl/complaints/$complaintId/assign" `
        -Method Post `
        -Headers $headers `
        -Body $reassignPayload

    Write-Host "   Complaint Reassigned:" -ForegroundColor Green
    Write-Host "     - From: $($handlerUser.fullName)" -ForegroundColor Gray
    Write-Host "     - To: $($reassignResponse.data.assignedToName)" -ForegroundColor Gray
    Write-Host "     - Reassignment notifications should be sent to both users" -ForegroundColor Cyan

    Test-Endpoint "Reassignment Auto-Response Trigger" $true
} catch {
    Write-Host "   Error reassigning complaint: $($_.Exception.Message)" -ForegroundColor Red
    Test-Endpoint "Reassignment Auto-Response Trigger" $false
}

# ========================================
# SUMMARY
# ========================================
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST SUMMARY" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$totalTests = $testResults.Passed + $testResults.Failed
$passRate = if ($totalTests -gt 0) { [math]::Round(($testResults.Passed / $totalTests) * 100, 2) } else { 0 }

Write-Host "`nTotal Tests: $totalTests" -ForegroundColor White
Write-Host "Passed: $($testResults.Passed)" -ForegroundColor Green
Write-Host "Failed: $($testResults.Failed)" -ForegroundColor Red
Write-Host "Pass Rate: $passRate%" -ForegroundColor $(if ($passRate -ge 80) { "Green" } else { "Yellow" })

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "AUTO-RESPONSE VERIFICATION CHECKLIST" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

Write-Host "`nPlease verify the following manually:" -ForegroundColor Yellow
Write-Host "1. Check complainant's email inbox for acknowledgment email" -ForegroundColor White
Write-Host "2. Check handler's email inbox for assignment notification" -ForegroundColor White
Write-Host "3. Check complainant's email inbox for status change notification" -ForegroundColor White
Write-Host "4. Check complainant's email inbox for resolution notification" -ForegroundColor White
Write-Host "5. Check both users' email for reassignment notifications" -ForegroundColor White
Write-Host "6. Verify template variable substitution is working correctly" -ForegroundColor White
Write-Host "7. Check that emails are properly formatted (HTML/plain text)" -ForegroundColor White
Write-Host "8. Verify email threading (In-Reply-To headers) if configured" -ForegroundColor White

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "TEST COMPLAINT DETAILS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Complaint Number: $complaintNumber" -ForegroundColor White
Write-Host "Complaint ID: $complaintId" -ForegroundColor White
Write-Host "View URL: http://localhost:4200/complaints/$complaintId" -ForegroundColor Cyan

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "CONFIGURATION FILES" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend Config: complaint-system-dotnet/src/ComplaintManagement.API/appsettings.json" -ForegroundColor White
Write-Host "Auto-Response Settings:" -ForegroundColor White
Write-Host "  - AutoResponse:Enabled" -ForegroundColor Gray
Write-Host "  - AutoResponse:SendAcknowledgmentOnWebCreation" -ForegroundColor Gray
Write-Host "  - AutoResponse:StatusChangeNotifications" -ForegroundColor Gray
Write-Host "  - AutoResponse:AssignmentNotifications" -ForegroundColor Gray
Write-Host "  - AutoResponse:ResolutionNotifications" -ForegroundColor Gray

Write-Host "`nTest completed at: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')" -ForegroundColor White
Write-Host "`n" -ForegroundColor White

# Save test results to file
$resultsFile = "auto-response-test-results-$(Get-Date -Format 'yyyyMMdd-HHmmss').json"
$testResults | ConvertTo-Json -Depth 10 | Out-File $resultsFile
Write-Host "Test results saved to: $resultsFile" -ForegroundColor Cyan
