# Analyze Complaint Management System Data
$baseUrl = "http://localhost:5000"

# Login to get token
Write-Host "Logging in..." -ForegroundColor Cyan
$loginBody = @{
    email = "admin@complaintmanagement.com"
    password = "Admin@123"
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.data.token

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "COMPLAINT MANAGEMENT SYSTEM DATA ANALYSIS" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green

# 1. Check Users
Write-Host "1. USERS ANALYSIS" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow
try {
    $users = Invoke-RestMethod -Uri "$baseUrl/api/users" -Headers $headers -Method Get
    Write-Host "Total Users: $($users.data.Count)" -ForegroundColor Cyan

    # Check for specific emails
    $navNainital = $users.data | Where-Object { $_.email -like "*nav_nainital*" -or $_.email -like "*nainital*" }
    $naveenCandra = $users.data | Where-Object { $_.email -like "*naveen.candra*" -or $_.email -like "*Naveen.candra*" }

    if ($navNainital) {
        Write-Host "  Found user: $($navNainital.email) - $($navNainital.fullName)" -ForegroundColor Green
    } else {
        Write-Host "  NOT FOUND: nav_nainital@yahoo.com" -ForegroundColor Red
    }

    if ($naveenCandra) {
        Write-Host "  Found user: $($naveenCandra.email) - $($naveenCandra.fullName)" -ForegroundColor Green
    } else {
        Write-Host "  NOT FOUND: Naveen.candra@oryggitech.com" -ForegroundColor Red
    }

    # Show sample of users
    Write-Host "`n  Sample Users (first 5):" -ForegroundColor White
    $users.data | Select-Object -First 5 | ForEach-Object {
        Write-Host "    - $($_.email) ($($_.fullName))" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error fetching users: $_" -ForegroundColor Red
}

# 2. Check SLA Policies
Write-Host "`n2. SLA POLICIES" -ForegroundColor Yellow
Write-Host "===============" -ForegroundColor Yellow
try {
    $sla = Invoke-RestMethod -Uri "$baseUrl/api/sla" -Headers $headers -Method Get
    if ($sla -and $sla.Count -gt 0) {
        Write-Host "Total SLA Policies: $($sla.Count)" -ForegroundColor Cyan
        $sla | ForEach-Object {
            Write-Host "  - $($_.name) (Priority: $($_.priorityName), Response: $($_.responseTime)h, Resolution: $($_.resolutionTime)h)" -ForegroundColor Gray
        }
    } else {
        Write-Host "NO SLA Policies configured" -ForegroundColor Red
    }
} catch {
    Write-Host "Error fetching SLA policies: $_" -ForegroundColor Red
}

# 3. Check Workflows
Write-Host "`n3. WORKFLOWS" -ForegroundColor Yellow
Write-Host "============" -ForegroundColor Yellow
try {
    $workflows = Invoke-RestMethod -Uri "$baseUrl/api/workflows" -Headers $headers -Method Get -ErrorAction SilentlyContinue
    if ($workflows -and $workflows.Count -gt 0) {
        Write-Host "Total Workflows: $($workflows.Count)" -ForegroundColor Cyan
        $workflows | ForEach-Object {
            Write-Host "  - $($_.name) (Category: $($_.categoryName))" -ForegroundColor Gray
        }
    } else {
        Write-Host "NO Workflows configured" -ForegroundColor Red
    }
} catch {
    Write-Host "NO Workflows configured (endpoint may not exist)" -ForegroundColor Red
}

# 4. Check Escalation Policies
Write-Host "`n4. ESCALATION POLICIES" -ForegroundColor Yellow
Write-Host "======================" -ForegroundColor Yellow
try {
    $escalationPolicies = Invoke-RestMethod -Uri "$baseUrl/api/escalation-policies" -Headers $headers -Method Get
    if ($escalationPolicies -and $escalationPolicies.Count -gt 0) {
        Write-Host "Total Escalation Policies: $($escalationPolicies.Count)" -ForegroundColor Cyan
        $escalationPolicies | ForEach-Object {
            Write-Host "  - $($_.name) (Category: $($_.categoryName), Levels: $($_.levels.Count))" -ForegroundColor Gray
        }
    } else {
        Write-Host "NO Escalation Policies configured" -ForegroundColor Red
    }
} catch {
    Write-Host "Error fetching escalation policies: $_" -ForegroundColor Red
}

# 5. Check Email Templates
Write-Host "`n5. EMAIL TEMPLATES" -ForegroundColor Yellow
Write-Host "==================" -ForegroundColor Yellow
try {
    $templates = Invoke-RestMethod -Uri "$baseUrl/api/communication-templates" -Headers $headers -Method Get
    Write-Host "Total Email Templates: $($templates.Count)" -ForegroundColor Cyan

    # Group by system vs custom
    $systemTemplates = $templates | Where-Object { $_.isSystem -eq $true }
    $customTemplates = $templates | Where-Object { $_.isSystem -eq $false }

    Write-Host "  System Templates: $($systemTemplates.Count)" -ForegroundColor Gray
    Write-Host "  Custom Templates: $($customTemplates.Count)" -ForegroundColor Gray

    # Show system templates
    Write-Host "`n  System Templates:" -ForegroundColor White
    $systemTemplates | ForEach-Object {
        $channelName = switch($_.channel) {
            0 { "Email" }
            1 { "SMS" }
            2 { "WhatsApp" }
            default { "Unknown" }
        }
        Write-Host "    - $($_.name) ($channelName)" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error fetching templates: $_" -ForegroundColor Red
}

# 6. Check Event Types
Write-Host "`n6. EVENT TYPES" -ForegroundColor Yellow
Write-Host "===============" -ForegroundColor Yellow
try {
    $eventTypes = Invoke-RestMethod -Uri "$baseUrl/api/event-types" -Headers $headers -Method Get
    Write-Host "Total Event Types: $($eventTypes.Count)" -ForegroundColor Cyan

    # Group by system vs custom
    $systemEvents = $eventTypes | Where-Object { $_.isSystem -eq $true }
    $customEvents = $eventTypes | Where-Object { $_.isSystem -eq $false }

    Write-Host "  System Event Types: $($systemEvents.Count)" -ForegroundColor Gray
    Write-Host "  Custom Event Types: $($customEvents.Count)" -ForegroundColor Gray

    Write-Host "`n  System Event Types:" -ForegroundColor White
    $systemEvents | ForEach-Object {
        Write-Host "    - $($_.name) [$($_.code)]" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error fetching event types: $_" -ForegroundColor Red
}

# 7. Check Email Server Settings
Write-Host "`n7. EMAIL SERVER SETTINGS" -ForegroundColor Yellow
Write-Host "========================" -ForegroundColor Yellow
try {
    $emailSettings = Invoke-RestMethod -Uri "$baseUrl/api/email-server-settings" -Headers $headers -Method Get
    if ($emailSettings) {
        Write-Host "Email Server Configured: YES" -ForegroundColor Green
        Write-Host "  Host: $($emailSettings.host)" -ForegroundColor Gray
        Write-Host "  Port: $($emailSettings.port)" -ForegroundColor Gray
        Write-Host "  From Email: $($emailSettings.fromEmail)" -ForegroundColor Gray
        Write-Host "  Use SSL: $($emailSettings.useSsl)" -ForegroundColor Gray
        Write-Host "  Is Active: $($emailSettings.isActive)" -ForegroundColor Gray
    } else {
        Write-Host "NO Email Server configured" -ForegroundColor Red
    }
} catch {
    Write-Host "NO Email Server configured" -ForegroundColor Red
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "SUMMARY" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green

Write-Host "`nWhat exists in the system:" -ForegroundColor Cyan
Write-Host "  Users: $($users.data.Count) (many Oryggi employees)" -ForegroundColor White
Write-Host "  SLA Policies: $(if($sla) { $sla.Count } else { 0 })" -ForegroundColor White
Write-Host "  Workflows: $(if($workflows) { $workflows.Count } else { 0 })" -ForegroundColor White
Write-Host "  Escalation Policies: $(if($escalationPolicies) { $escalationPolicies.Count } else { 0 })" -ForegroundColor White
Write-Host "  Email Templates: $($templates.Count) ($($systemTemplates.Count) system, $($customTemplates.Count) custom)" -ForegroundColor White
Write-Host "  Event Types: $($eventTypes.Count) ($($systemEvents.Count) system, $($customEvents.Count) custom)" -ForegroundColor White
Write-Host "  Email Server: $(if($emailSettings) { 'Configured' } else { 'NOT configured' })" -ForegroundColor White

Write-Host "`nWhat needs to be created:" -ForegroundColor Yellow
if (-not $navNainital) {
    Write-Host "  - User: nav_nainital@yahoo.com" -ForegroundColor Red
}
if (-not $naveenCandra) {
    Write-Host "  - User: Naveen.candra@oryggitech.com" -ForegroundColor Red
}
if (-not $sla -or $sla.Count -eq 0) {
    Write-Host "  - SLA Policies (currently none)" -ForegroundColor Red
}
if (-not $workflows -or $workflows.Count -eq 0) {
    Write-Host "  - Workflows (currently none)" -ForegroundColor Red
}
if (-not $escalationPolicies -or $escalationPolicies.Count -eq 0) {
    Write-Host "  - Escalation Policies (currently none)" -ForegroundColor Red
}
if (-not $emailSettings) {
    Write-Host "  - Email Server Configuration" -ForegroundColor Red
}

Write-Host "`nAnalysis complete!" -ForegroundColor Green
