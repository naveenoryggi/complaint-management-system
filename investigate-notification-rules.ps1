# Investigation Script for Notification Rules Issue
# Date: November 10, 2025

$ErrorActionPreference = "Continue"
$baseUrl = "http://localhost:5000"

# Read token
$token = Get-Content ".test-token" -Raw
$token = $token.Trim()

# Remove BOM if present
if ($token.StartsWith([char]0xFEFF)) {
    $token = $token.Substring(1)
}

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "=" * 80
Write-Host "NOTIFICATION RULES INVESTIGATION REPORT"
Write-Host "=" * 80
Write-Host ""

# Step 1: Get Event Types
Write-Host "STEP 1: Fetching Event Types..."
Write-Host "-" * 80
try {
    $eventTypes = Invoke-RestMethod -Uri "$baseUrl/api/event-types" -Method Get -Headers $headers
    Write-Host "SUCCESS: Found $($eventTypes.Count) event types"
    Write-Host ""

    $eventTypes | ForEach-Object {
        Write-Host "  ID: $($_.id)"
        Write-Host "  Code: $($_.code)"
        Write-Host "  Name: $($_.name)"
        Write-Host "  Description: $($_.description)"
        Write-Host ""
    }

    # Save to JSON
    $eventTypes | ConvertTo-Json -Depth 10 | Out-File "event-types-result.json"
} catch {
    Write-Host "ERROR: Failed to fetch event types"
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Message: $($_.Exception.Message)"
}

Write-Host ""
Write-Host ""

# Step 2: Get Communication Templates
Write-Host "STEP 2: Fetching Communication Templates..."
Write-Host "-" * 80
try {
    $templates = Invoke-RestMethod -Uri "$baseUrl/api/communication-templates" -Method Get -Headers $headers
    Write-Host "SUCCESS: Found $($templates.Count) templates"
    Write-Host ""

    $templates | ForEach-Object {
        Write-Host "  ID: $($_.id)"
        Write-Host "  Name: $($_.name)"
        Write-Host "  Code: $($_.code)"
        Write-Host "  Channel: $($_.channel) (0=Email, 1=SMS, 2=WhatsApp, 3=InApp)"
        Write-Host "  IsActive: $($_.isActive)"
        Write-Host "  IsSystem: $($_.isSystem)"
        Write-Host ""
    }

    # Save to JSON
    $templates | ConvertTo-Json -Depth 10 | Out-File "templates-result.json"
} catch {
    Write-Host "ERROR: Failed to fetch templates"
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Message: $($_.Exception.Message)"
}

Write-Host ""
Write-Host ""

# Step 3: Get Notification Rules
Write-Host "STEP 3: Fetching Notification Rules..."
Write-Host "-" * 80
try {
    $rules = Invoke-RestMethod -Uri "$baseUrl/api/event-communication-rules" -Method Get -Headers $headers
    Write-Host "SUCCESS: Found $($rules.Count) notification rules"
    Write-Host ""

    $rules | ForEach-Object {
        Write-Host "  Rule ID: $($_.id)"
        Write-Host "  EventTypeId: $($_.eventTypeId)"
        Write-Host "  TemplateId: $($_.templateId)"
        Write-Host "  RecipientType: $($_.recipientType)"
        Write-Host "  IsActive: $($_.isActive)"
        Write-Host "  Priority: $($_.priority)"

        # Check if IDs are valid
        $eventTypeExists = $eventTypes | Where-Object { $_.id -eq $_.eventTypeId }
        $templateExists = $templates | Where-Object { $_.id -eq $_.templateId }

        if (-not $eventTypeExists) {
            Write-Host "  WARNING: Event Type ID does not match any existing event type!" -ForegroundColor Red
        }
        if (-not $templateExists) {
            Write-Host "  WARNING: Template ID does not match any existing template!" -ForegroundColor Red
        }

        Write-Host ""
    }

    # Save to JSON
    $rules | ConvertTo-Json -Depth 10 | Out-File "notification-rules-result.json"
} catch {
    Write-Host "ERROR: Failed to fetch notification rules"
    Write-Host "Status: $($_.Exception.Response.StatusCode.value__)"
    Write-Host "Message: $($_.Exception.Message)"
}

Write-Host ""
Write-Host ""

# Step 4: Analysis Summary
Write-Host "STEP 4: Analysis Summary..."
Write-Host "-" * 80

$missingEventTypes = @()
$missingTemplates = @()

if ($rules -and $eventTypes) {
    $rules | ForEach-Object {
        $rule = $_
        $eventType = $eventTypes | Where-Object { $_.id -eq $rule.eventTypeId }
        if (-not $eventType) {
            $missingEventTypes += $rule.eventTypeId
        }

        $template = $templates | Where-Object { $_.id -eq $rule.templateId }
        if (-not $template) {
            $missingTemplates += $rule.templateId
        }
    }
}

Write-Host "Total Event Types: $($eventTypes.Count)"
Write-Host "Total Templates: $($templates.Count)"
Write-Host "Total Notification Rules: $($rules.Count)"
Write-Host ""
Write-Host "Invalid Event Type IDs in Rules: $($missingEventTypes.Count)"
Write-Host "Invalid Template IDs in Rules: $($missingTemplates.Count)"
Write-Host ""

if ($missingEventTypes.Count -gt 0) {
    Write-Host "Missing Event Type IDs:" -ForegroundColor Red
    $missingEventTypes | ForEach-Object { Write-Host "  - $_" }
    Write-Host ""
}

if ($missingTemplates.Count -gt 0) {
    Write-Host "Missing Template IDs:" -ForegroundColor Red
    $missingTemplates | ForEach-Object { Write-Host "  - $_" }
    Write-Host ""
}

Write-Host ""
Write-Host "=" * 80
Write-Host "INVESTIGATION COMPLETE"
Write-Host "=" * 80
