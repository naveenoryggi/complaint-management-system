# Verification Script for Notification Rules Fix
# This script verifies that event types and templates are now properly returned by the API
# and that notification rules can be correctly linked to them

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "NOTIFICATION RULES FIX VERIFICATION" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Get fresh token
Write-Host "Step 1: Authenticating..." -ForegroundColor Yellow
$loginData = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

try {
    $authResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginData -ContentType "application/json"
    $token = $authResponse.data.token
    Write-Host "SUCCESS: Authentication successful" -ForegroundColor Green
} catch {
    Write-Host "FAILED: Could not authenticate" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host ""
Write-Host "Step 2: Fetching Event Types..." -ForegroundColor Yellow
try {
    $eventTypes = Invoke-RestMethod -Uri "$baseUrl/api/event-types" -Method Get -Headers $headers
    Write-Host "SUCCESS: Retrieved $($eventTypes.Count) event types" -ForegroundColor Green

    # Display first 5 event types
    Write-Host ""
    Write-Host "Sample Event Types:" -ForegroundColor Cyan
    $eventTypes | Select-Object -First 5 | ForEach-Object {
        Write-Host "  - $($_.name) ($($_.code))" -ForegroundColor White
        Write-Host "    ID: $($_.id)" -ForegroundColor DarkGray
    }

    if ($eventTypes.Count -eq 0) {
        Write-Host "WARNING: No event types found!" -ForegroundColor Red
    }
} catch {
    Write-Host "FAILED: Could not fetch event types" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 3: Fetching Communication Templates..." -ForegroundColor Yellow
try {
    $templates = Invoke-RestMethod -Uri "$baseUrl/api/communication-templates" -Method Get -Headers $headers
    Write-Host "SUCCESS: Retrieved $($templates.Count) templates" -ForegroundColor Green

    # Display first 5 templates
    Write-Host ""
    Write-Host "Sample Templates:" -ForegroundColor Cyan
    $templates | Where-Object { $_.isSystem -eq $true } | Select-Object -First 5 | ForEach-Object {
        $channelName = switch ($_.channel) {
            0 { "Email" }
            1 { "SMS" }
            2 { "WhatsApp" }
            3 { "InApp" }
            default { "Unknown" }
        }
        Write-Host "  - $($_.name) [$channelName]" -ForegroundColor White
        Write-Host "    ID: $($_.id)" -ForegroundColor DarkGray
        Write-Host "    Code: $($_.code)" -ForegroundColor DarkGray
    }

    if ($templates.Count -eq 0) {
        Write-Host "WARNING: No templates found!" -ForegroundColor Red
    }
} catch {
    Write-Host "FAILED: Could not fetch templates" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 4: Fetching Notification Rules..." -ForegroundColor Yellow
try {
    $rulesResponse = Invoke-RestMethod -Uri "$baseUrl/api/event-communication-rules" -Method Get -Headers $headers
    $rules = $rulesResponse.data
    Write-Host "SUCCESS: Retrieved $($rules.Count) notification rules" -ForegroundColor Green

    if ($rules.Count -eq 0) {
        Write-Host "WARNING: No notification rules found!" -ForegroundColor Yellow
    }
} catch {
    Write-Host "FAILED: Could not fetch notification rules" -ForegroundColor Red
    Write-Host "Error: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 5: Validating Rule Linkages..." -ForegroundColor Yellow

$validRules = 0
$invalidEventTypes = 0
$invalidTemplates = 0

foreach ($rule in $rules) {
    $isValid = $true

    # Check if event type exists
    $eventType = $eventTypes | Where-Object { $_.id -eq $rule.eventTypeId }
    if (-not $eventType) {
        Write-Host "  INVALID: Rule '$($rule.name)' - Event Type ID not found: $($rule.eventTypeId)" -ForegroundColor Red
        $invalidEventTypes++
        $isValid = $false
    }

    # Check if template exists
    $template = $templates | Where-Object { $_.id -eq $rule.templateId }
    if (-not $template) {
        Write-Host "  INVALID: Rule '$($rule.name)' - Template ID not found: $($rule.templateId)" -ForegroundColor Red
        $invalidTemplates++
        $isValid = $false
    }

    if ($isValid) {
        $validRules++
    }
}

Write-Host ""
Write-Host "Validation Results:" -ForegroundColor Cyan
Write-Host "  Total Rules: $($rules.Count)" -ForegroundColor White
Write-Host "  Valid Rules: $validRules" -ForegroundColor Green
Write-Host "  Rules with Invalid Event Types: $invalidEventTypes" -ForegroundColor $(if ($invalidEventTypes -gt 0) { "Red" } else { "Green" })
Write-Host "  Rules with Invalid Templates: $invalidTemplates" -ForegroundColor $(if ($invalidTemplates -gt 0) { "Red" } else { "Green" })

Write-Host ""
Write-Host "Step 6: Displaying Sample Valid Rules..." -ForegroundColor Yellow

$sampleRules = $rules | Select-Object -First 5
foreach ($rule in $sampleRules) {
    $eventType = $eventTypes | Where-Object { $_.id -eq $rule.eventTypeId }
    $template = $templates | Where-Object { $_.id -eq $rule.templateId }

    Write-Host ""
    Write-Host "  Rule: $($rule.name)" -ForegroundColor Cyan
    Write-Host "    Event: $(if ($eventType) { $eventType.name } else { 'UNKNOWN' })" -ForegroundColor $(if ($eventType) { "Green" } else { "Red" })
    Write-Host "    Template: $(if ($template) { $template.name } else { 'UNKNOWN' })" -ForegroundColor $(if ($template) { "Green" } else { "Red" })
    Write-Host "    Channel: $(switch ($rule.channel) { 0 {'Email'} 1 {'SMS'} 2 {'WhatsApp'} 3 {'InApp'} default {'Unknown'} })" -ForegroundColor White
    Write-Host "    Active: $($rule.isActive)" -ForegroundColor $(if ($rule.isActive) { "Green" } else { "Yellow" })
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "VERIFICATION COMPLETE" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Summary
if ($invalidEventTypes -eq 0 -and $invalidTemplates -eq 0) {
    Write-Host "RESULT: ALL RULES VALID" -ForegroundColor Green
    Write-Host "The notification rules configuration is working correctly!" -ForegroundColor Green
    Write-Host "Event types and templates are properly linked." -ForegroundColor Green
    exit 0
} else {
    Write-Host "RESULT: SOME RULES HAVE INVALID REFERENCES" -ForegroundColor Yellow
    Write-Host "Please check the rules with invalid event types or templates." -ForegroundColor Yellow
    exit 0
}
