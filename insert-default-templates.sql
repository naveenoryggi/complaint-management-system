-- ============================================================================
-- Default Communication Templates for Complaint Management System
-- ============================================================================
-- Purpose: Insert ready-to-use templates with {{variables}} for common scenarios
-- Date: November 13, 2025
-- Note: Run this script in SQL Server Management Studio
-- ============================================================================

USE ComplaintManagementDb;
GO

-- Get the first company ID (you may need to adjust this for your specific company)
DECLARE @CompanyId UNIQUEIDENTIFIER = (SELECT TOP 1 Id FROM Companies WHERE IsDeleted = 0);

PRINT 'Using Company ID: ' + CAST(@CompanyId AS NVARCHAR(50));
GO

-- ============================================================================
-- Template 1: Auto-Acknowledgement for New Tickets
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM CommunicationTemplates WHERE Code = 'AUTO_ACK_NEW_TICKET')
BEGIN
    INSERT INTO CommunicationTemplates (
        Id,
        Name,
        Code,
        Description,
        Channel,
        Subject,
        Body,
        HtmlBody,
        IsActive,
        IsSystem,
        CompanyId,
        CreatedAt,
        IsDeleted
    )
    VALUES (
        NEWID(),
        'Auto-Acknowledgement - New Ticket',
        'AUTO_ACK_NEW_TICKET',
        'Automatic email sent when a new ticket is created from email',
        0, -- Email channel
        'Ticket Created: {{TicketNumber}}',
        -- Plain text body
        'Dear {{CustomerName}},

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
{{SupportEmail}}',
        -- HTML body
        '<!DOCTYPE html>
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
</html>',
        1, -- IsActive
        0, -- IsSystem
        (SELECT TOP 1 Id FROM Companies WHERE IsDeleted = 0),
        GETUTCDATE(),
        0 -- IsDeleted
    );

    PRINT '✅ Created template: Auto-Acknowledgement - New Ticket';
END
ELSE
BEGIN
    PRINT '⚠️ Template AUTO_ACK_NEW_TICKET already exists, skipping';
END
GO

-- ============================================================================
-- Template 2: Status Update Notification
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM CommunicationTemplates WHERE Code = 'STATUS_UPDATED')
BEGIN
    INSERT INTO CommunicationTemplates (
        Id, Name, Code, Description, Channel, Subject, Body, HtmlBody,
        IsActive, IsSystem, CompanyId, CreatedAt, IsDeleted
    )
    VALUES (
        NEWID(),
        'Status Update Notification',
        'STATUS_UPDATED',
        'Notification sent when ticket status changes',
        0, -- Email
        'Ticket {{TicketNumber}} Status Updated: {{Status}}',
        -- Plain text
        'Dear {{CustomerName}},

Your ticket status has been updated.

Ticket: {{TicketNumber}}
New Status: {{Status}}
Updated: {{CurrentDate}} {{CurrentTime}}

Our team continues to work on your request. You will receive further updates as progress is made.

Best regards,
{{CompanyName}} Support',
        -- HTML
        '<!DOCTYPE html>
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
</html>',
        1, 0, (SELECT TOP 1 Id FROM Companies WHERE IsDeleted = 0), GETUTCDATE(), 0
    );

    PRINT '✅ Created template: Status Update Notification';
END
ELSE
BEGIN
    PRINT '⚠️ Template STATUS_UPDATED already exists, skipping';
END
GO

-- ============================================================================
-- Template 3: Ticket Resolved Confirmation
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM CommunicationTemplates WHERE Code = 'TICKET_RESOLVED')
BEGIN
    INSERT INTO CommunicationTemplates (
        Id, Name, Code, Description, Channel, Subject, Body, HtmlBody,
        IsActive, IsSystem, CompanyId, CreatedAt, IsDeleted
    )
    VALUES (
        NEWID(),
        'Ticket Resolved Confirmation',
        'TICKET_RESOLVED',
        'Confirmation email when ticket is resolved',
        0, -- Email
        '✅ Ticket {{TicketNumber}} Resolved',
        -- Plain text
        'Dear {{CustomerName}},

Great news! Your support ticket has been resolved.

Ticket: {{TicketNumber}}
Subject: {{Title}}
Status: {{Status}}

If you need further assistance, please reply to this email or create a new ticket.

Thank you for using {{CompanyName}}!

Best regards,
{{CompanyName}} Support Team',
        -- HTML
        '<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #4CAF50; color: white; padding: 20px; text-align: center; }
        .content { background: #f9f9f9; padding: 20px; border: 1px solid #ddd; margin-top: 20px; }
        .success-icon { font-size: 48px; text-align: center; margin: 20px 0; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>✅ Ticket Resolved</h1>
        </div>
        <div class="success-icon">🎉</div>
        <div class="content">
            <p>Dear <strong>{{CustomerName}}</strong>,</p>
            <p>Great news! Your support ticket has been resolved.</p>
            <p><strong>Ticket:</strong> {{TicketNumber}}</p>
            <p><strong>Subject:</strong> {{Title}}</p>
            <p><strong>Status:</strong> {{Status}}</p>
            <p>If you need further assistance, please reply to this email or create a new ticket.</p>
            <p>Thank you for using <strong>{{CompanyName}}</strong>!</p>
        </div>
        <div class="footer">
            <p>Best regards,<br/>{{CompanyName}} Support Team</p>
        </div>
    </div>
</body>
</html>',
        1, 0, (SELECT TOP 1 Id FROM Companies WHERE IsDeleted = 0), GETUTCDATE(), 0
    );

    PRINT '✅ Created template: Ticket Resolved Confirmation';
END
ELSE
BEGIN
    PRINT '⚠️ Template TICKET_RESOLVED already exists, skipping';
END
GO

-- ============================================================================
-- Template 4: SLA Breach Warning (for managers/handlers)
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM CommunicationTemplates WHERE Code = 'SLA_BREACH_WARNING')
BEGIN
    INSERT INTO CommunicationTemplates (
        Id, Name, Code, Description, Channel, Subject, Body, HtmlBody,
        IsActive, IsSystem, CompanyId, CreatedAt, IsDeleted
    )
    VALUES (
        NEWID(),
        'SLA Breach Warning',
        'SLA_BREACH_WARNING',
        'Alert sent to handlers when SLA is about to breach',
        0, -- Email
        '⚠️ URGENT: Ticket {{TicketNumber}} SLA Breach Warning',
        -- Plain text
        'URGENT: SLA BREACH WARNING

Ticket: {{TicketNumber}}
Title: {{Title}}
Customer: {{CustomerName}}
Priority: {{Priority}}

ACTION REQUIRED:
This ticket requires immediate attention to prevent SLA breach.

Current Status: {{Status}}

Please prioritize and take action immediately.',
        -- HTML
        '<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; color: #333; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background: #f44336; color: white; padding: 20px; text-align: center; }
        .content { background: #fff3cd; padding: 20px; border: 2px solid #f44336; margin-top: 20px; }
        .urgent { color: #f44336; font-weight: bold; font-size: 18px; }
        .footer { text-align: center; padding: 20px; color: #666; font-size: 12px; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>⚠️ URGENT: SLA BREACH WARNING</h1>
        </div>
        <div class="content">
            <p class="urgent">ACTION REQUIRED IMMEDIATELY</p>
            <p><strong>Ticket:</strong> {{TicketNumber}}</p>
            <p><strong>Title:</strong> {{Title}}</p>
            <p><strong>Customer:</strong> {{CustomerName}}</p>
            <p><strong>Priority:</strong> {{Priority}}</p>
            <p><strong>Current Status:</strong> {{Status}}</p>
            <p>This ticket requires immediate attention to prevent SLA breach.</p>
            <p>Please prioritize and take action immediately.</p>
        </div>
        <div class="footer">
            <p>Automated SLA Alert System</p>
        </div>
    </div>
</body>
</html>',
        1, 0, (SELECT TOP 1 Id FROM Companies WHERE IsDeleted = 0), GETUTCDATE(), 0
    );

    PRINT '✅ Created template: SLA Breach Warning';
END
ELSE
BEGIN
    PRINT '⚠️ Template SLA_BREACH_WARNING already exists, skipping';
END
GO

-- ============================================================================
-- Summary: Show created templates
-- ============================================================================
PRINT '';
PRINT '================================================================';
PRINT 'TEMPLATE CREATION SUMMARY';
PRINT '================================================================';
PRINT '';

SELECT
    Code AS [Template Code],
    Name AS [Template Name],
    CASE Channel
        WHEN 0 THEN 'Email'
        WHEN 1 THEN 'SMS'
        WHEN 2 THEN 'WhatsApp'
        WHEN 3 THEN 'In-App'
        ELSE 'Unknown'
    END AS [Channel],
    CASE WHEN IsActive = 1 THEN 'Active' ELSE 'Inactive' END AS [Status],
    CreatedAt AS [Created At]
FROM CommunicationTemplates
WHERE Code IN ('AUTO_ACK_NEW_TICKET', 'STATUS_UPDATED', 'TICKET_RESOLVED', 'SLA_BREACH_WARNING')
ORDER BY CreatedAt DESC;

PRINT '';
PRINT '================================================================';
PRINT 'NEXT STEPS:';
PRINT '1. Update Email Configuration with template ID for auto-acknowledgement';
PRINT '2. Create Notification Rules to link events to these templates';
PRINT '3. Test by sending an email to your configured address';
PRINT '================================================================';
GO
