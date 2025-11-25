$BaseUrl = "http://localhost:5000"

# Login
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token
$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

# Get test event and template IDs
$eventTypes = Invoke-RestMethod -Uri "$BaseUrl/api/event-types" -Method GET -Headers $headers
$testEvent = $eventTypes | Where-Object { $_.code -like "TEST_EVENT_*" } | Select-Object -First 1

$templates = Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates" -Method GET -Headers $headers
$testTemplate = $templates | Where-Object { $_.code -like "TEST_TEMPLATE_*" } | Select-Object -First 1

if (!$testEvent -or !$testTemplate) {
    Write-Host "Creating test event and template first..." -ForegroundColor Yellow

    $newEvent = @{
        name = "Debug Event"
        code = "DEBUG_EVENT_999"
        description = "Debug event"
        entityType = "Complaint"
        category = "Notification"
        isActive = $true
        availableFields = '["ComplaintNumber"]'
        iconClass = "fa fa-bell"
    }
    $testEvent = Invoke-RestMethod -Uri "$BaseUrl/api/event-types" -Method POST -Headers $headers -Body ($newEvent | ConvertTo-Json)

    $newTemplate = @{
        name = "Debug Template"
        code = "DEBUG_TEMPLATE_999"
        description = "Debug template"
        channel = 0
        category = "Notification"
        language = "en"
        subject = "Test"
        body = "Test"
        htmlBody = "<p>Test</p>"
        availablePlaceholders = '["ComplaintNumber"]'
        isActive = $true
        isSystem = $false
    }
    $testTemplate = Invoke-RestMethod -Uri "$BaseUrl/api/communication-templates" -Method POST -Headers $headers -Body ($newTemplate | ConvertTo-Json)
}

Write-Host "Using Event ID: $($testEvent.id)" -ForegroundColor Cyan
Write-Host "Using Template ID: $($testTemplate.id)" -ForegroundColor Cyan
Write-Host ""

Write-Host "Creating notification rule..." -ForegroundColor Yellow

$newRule = @{
    name = "Debug Notification Rule"
    description = "Debug rule"
    eventTypeId = $testEvent.id
    templateId = $testTemplate.id
    channel = 0
    recipientType = "AssignedUser"
    specificEmails = @()
    specificUserIds = @()
    specificRoleIds = @()
    conditions = '{}'
    priority = 1
    delayMinutes = 0
    isActive = $true
}

Write-Host "Request body:" -ForegroundColor Cyan
$newRule | ConvertTo-Json -Depth 5

try {
    $result = Invoke-WebRequest -Uri "$BaseUrl/api/event-communication-rules" -Method POST -Headers $headers -Body ($newRule | ConvertTo-Json -Depth 5) -ContentType "application/json"
    Write-Host "Success!" -ForegroundColor Green
    $result.Content | ConvertFrom-Json | ConvertTo-Json -Depth 5
} catch {
    Write-Host "Error:" -ForegroundColor Red
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
    $responseStream = $_.Exception.Response.GetResponseStream()
    $reader = New-Object System.IO.StreamReader($responseStream)
    $responseBody = $reader.ReadToEnd()
    Write-Host "Response: $responseBody" -ForegroundColor Red
}
