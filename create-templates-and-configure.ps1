# ═══════════════════════════════════════════════════════════════
# Complete Template System Configuration Script
# ═══════════════════════════════════════════════════════════════

Write-Host "🚀 Starting Complete Template System Configuration" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# ═══════════════════════════════════════════════════════════════
# STEP 1: Get Authentication Token
# ═══════════════════════════════════════════════════════════════
Write-Host "📝 STEP 1: Getting authentication token..." -ForegroundColor Yellow

# Try to get token from file first
$token = $null
if (Test-Path ".working-token") {
    $token = Get-Content ".working-token" -Raw
    $token = $token.Trim()
    Write-Host "✅ Using existing token from .working-token" -ForegroundColor Green
} else {
    # Login to get token
    $loginBody = @{
        email = "admin@complaintmanagement.com"
        password = "Admin@123"
    } | ConvertTo-Json

    try {
        $loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/auth/login" `
            -Method POST `
            -Body $loginBody `
            -ContentType "application/json"

        $token = $loginResponse.data.token
        $token | Out-File -FilePath ".working-token" -NoNewline
        Write-Host "✅ Logged in successfully" -ForegroundColor Green
    } catch {
        Write-Host "❌ Failed to login: $_" -ForegroundColor Red
        exit 1
    }
}

Write-Host "   Token: $($token.Substring(0, 50))...`n" -ForegroundColor Gray

# ═══════════════════════════════════════════════════════════════
# STEP 2: Get Company ID
# ═══════════════════════════════════════════════════════════════
Write-Host "📝 STEP 2: Getting company ID..." -ForegroundColor Yellow

$headers = @{
    "Authorization" = "Bearer $token"
    "Content-Type" = "application/json"
}

$companies = Invoke-RestMethod -Uri "http://localhost:5000/api/companies" -Headers $headers
$companyId = $companies.data[0].id

Write-Host "✅ Company ID: $companyId`n" -ForegroundColor Green

# ═══════════════════════════════════════════════════════════════
# STEP 3: Create 4 Default Templates
# ═══════════════════════════════════════════════════════════════
Write-Host "📝 STEP 3: Creating 4 default templates..." -ForegroundColor Yellow

$templates = @(
    @{
        name = "Auto-Acknowledgement - New Ticket"
        code = "AUTO_ACK_NEW_TICKET"
        description = "Automatic email sent when a new ticket is created from email"
        channel = 0
        subject = "Ticket Created: {{TicketNumber}}"
        body = @"
Dear {{CustomerName}},

Thank you for contacting {{CompanyName}}. We have received your request and created a support ticket.

Ticket Details:
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
  Ticket Number: {{TicketNumber}}
  Subject: {{Title}}
  Priority: {{Priority}}
  Status: {{Status}}
  Submitted: {{SubmittedAt}}
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

What Happens Next:
Our support team will review your request and respond as soon as possible.

Need to add more details? Simply reply to this email.

Best regards,
{{CompanyName}} Support Team
{{SupportEmail}}
"@
        htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #4CAF50; color: white; padding: 20px; text-align: center; }
        .content { background: #f9f9f9; padding: 20px; border: 1px solid #ddd; }
        .ticket-info { background: white; padding: 15px; border-left: 4px solid #4CAF50; margin: 20px 0; }
        .ticket-info p { margin: 5px 0; }
        .ticket-info strong { color: #4CAF50; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>✅ Ticket Created Successfully</h1>
        </div>
        <div class="content">
            <p>Dear <strong>{{CustomerName}}</strong>,</p>
            <p>Thank you for contacting <strong>{{CompanyName}}</strong>. We have received your request and created a support ticket.</p>
            <div class="ticket-info">
                <p><strong>Ticket Number:</strong> {{TicketNumber}}</p>
                <p><strong>Subject:</strong> {{Title}}</p>
                <p><strong>Priority:</strong> {{Priority}}</p>
                <p><strong>Status:</strong> {{Status}}</p>
                <p><strong>Submitted:</strong> {{SubmittedAt}}</p>
            </div>
            <h3>What Happens Next:</h3>
            <p>Our support team will review your request and respond as soon as possible.</p>
            <p><em>Need to add more details? Simply reply to this email.</em></p>
        </div>
        <div class="footer">
            <p>Best regards,<br/>
            <strong>{{CompanyName}} Support Team</strong><br/>
            {{SupportEmail}}</p>
        </div>
    </div>
</body>
</html>
"@
        isActive = $true
        companyId = $companyId
    },
    @{
        name = "Status Update Notification"
        code = "STATUS_UPDATED"
        description = "Notification sent when ticket status changes"
        channel = 0
        subject = "Ticket {{TicketNumber}} Status Updated: {{Status}}"
        body = @"
Dear {{CustomerName}},

Your ticket status has been updated.

Ticket: {{TicketNumber}}
New Status: {{Status}}
Updated: {{CurrentDate}} {{CurrentTime}}

Our team continues to work on your request. You will receive further updates as progress is made.

Best regards,
{{CompanyName}} Support
"@
        htmlBody = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #2196F3; color: white; padding: 20px; text-align: center; }
        .content { background: #f9f9f9; padding: 20px; border: 1px solid #ddd; margin-top: 20px; }
        .status-badge { display: inline-block; padding: 8px 16px; background: #2196F3; color: white; border-radius: 4px; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>📝 Ticket Status Updated</h1>
        </div>
        <div class="content">
            <p>Dear <strong>{{CustomerName}}</strong>,</p>
            <p>Your ticket status has been updated.</p>
            <p><strong>Ticket:</strong> {{TicketNumber}}</p>
            <p><strong>New Status:</strong> <span class="status-badge">{{Status}}</span></p>
            <p><strong>Updated:</strong> {{CurrentDate}} {{CurrentTime}}</p>
            <p>Our team continues to work on your request. You will receive further updates as progress is made.</p>
        </div>
        <div class="footer">
            <p>Best regards,<br/>{{CompanyName}} Support</p>
        </div>
    </div>
</body>
</html>
"@
        isActive = $true
        companyId = $companyId
    }
)

$createdTemplates = @()

foreach ($template in $templates) {
    Write-Host "   Creating: $($template.name)..." -ForegroundColor Gray

    $templateJson = $template | ConvertTo-Json -Depth 10

    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" `
            -Method POST `
            -Headers $headers `
            -Body $templateJson `
            -ContentType "application/json"

        if ($response.isSuccess) {
            $createdTemplates += $response.data
            Write-Host "   ✅ Created: $($template.code) (ID: $($response.data.id))" -ForegroundColor Green
        }
    } catch {
        Write-Host "   ⚠️  Template $($template.code) might already exist" -ForegroundColor Yellow
    }
}

Write-Host "`n✅ Templates creation complete: $($createdTemplates.Count) templates`n" -ForegroundColor Green

# ═══════════════════════════════════════════════════════════════
# STEP 4: Get AUTO_ACK_NEW_TICKET Template ID
# ═══════════════════════════════════════════════════════════════
Write-Host "📝 STEP 4: Getting AUTO_ACK_NEW_TICKET template ID..." -ForegroundColor Yellow

$templatesResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/templates" -Headers $headers
$autoAckTemplate = $templatesResponse.data | Where-Object { $_.code -eq "AUTO_ACK_NEW_TICKET" } | Select-Object -First 1

if ($null -eq $autoAckTemplate) {
    Write-Host "❌ AUTO_ACK_NEW_TICKET template not found!" -ForegroundColor Red
    exit 1
}

$templateId = $autoAckTemplate.id

Write-Host "✅ Template ID retrieved" -ForegroundColor Green
Write-Host "   Template ID: $templateId" -ForegroundColor Gray
Write-Host "   Template Name: $($autoAckTemplate.name)`n" -ForegroundColor Gray

# Save template ID for Playwright
$templateId | Out-File -FilePath ".template-id.txt" -NoNewline

# ═══════════════════════════════════════════════════════════════
# SUMMARY
# ═══════════════════════════════════════════════════════════════
Write-Host "═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "✅ TEMPLATES CREATED SUCCESSFULLY" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

Write-Host "📊 Summary:" -ForegroundColor Yellow
Write-Host "   • Templates Created: $($createdTemplates.Count)" -ForegroundColor White
Write-Host "   • Auto-Ack Template ID: $templateId" -ForegroundColor White
Write-Host "   • Template saved to: .template-id.txt" -ForegroundColor White
Write-Host "`n📝 Next Step:" -ForegroundColor Yellow
Write-Host "   Run Playwright MCP to configure email with this template ID" -ForegroundColor White
Write-Host "`n═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan
