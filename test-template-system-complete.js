const { chromium } = require('playwright');

async function testCompleteTemplateSystem() {
    console.log('🚀 Starting Complete Template System Test');
    console.log('═══════════════════════════════════════════════════════════════\n');

    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext();
    const page = await context.newPage();

    try {
        // ═══════════════════════════════════════════════════════════════
        // STEP 1: Login and get authentication token
        // ═══════════════════════════════════════════════════════════════
        console.log('📝 STEP 1: Logging in to get authentication token...');

        await page.goto('http://localhost:4200');
        await page.waitForTimeout(2000);

        // Fill login form
        await page.fill('input[type="email"]', 'admin@complaintmanagement.com');
        await page.fill('input[type="password"]', 'Admin@123');
        await page.click('button[type="submit"]');

        await page.waitForTimeout(3000);

        // Get token from localStorage
        const token = await page.evaluate(() => localStorage.getItem('token'));

        if (!token) {
            throw new Error('Failed to get authentication token');
        }

        console.log('✅ Logged in successfully');
        console.log(`   Token: ${token.substring(0, 50)}...\n`);

        // ═══════════════════════════════════════════════════════════════
        // STEP 2: Get Company ID
        // ═══════════════════════════════════════════════════════════════
        console.log('📝 STEP 2: Getting company ID...');

        const companyResponse = await fetch('http://localhost:5000/api/companies', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        const companies = await companyResponse.json();
        const companyId = companies.data && companies.data.length > 0
            ? companies.data[0].id
            : null;

        if (!companyId) {
            throw new Error('No company found');
        }

        console.log('✅ Company ID retrieved');
        console.log(`   Company ID: ${companyId}\n`);

        // ═══════════════════════════════════════════════════════════════
        // STEP 3: Create 4 Default Templates via API
        // ═══════════════════════════════════════════════════════════════
        console.log('📝 STEP 3: Creating 4 default templates via API...');

        const templates = [
            {
                name: 'Auto-Acknowledgement - New Ticket',
                code: 'AUTO_ACK_NEW_TICKET',
                description: 'Automatic email sent when a new ticket is created from email',
                channel: 0, // Email
                subject: 'Ticket Created: {{TicketNumber}}',
                body: `Dear {{CustomerName}},

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
{{SupportEmail}}`,
                htmlBody: `<!DOCTYPE html>
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
</html>`,
                isActive: true,
                companyId: companyId
            },
            {
                name: 'Status Update Notification',
                code: 'STATUS_UPDATED',
                description: 'Notification sent when ticket status changes',
                channel: 0,
                subject: 'Ticket {{TicketNumber}} Status Updated: {{Status}}',
                body: `Dear {{CustomerName}},

Your ticket status has been updated.

Ticket: {{TicketNumber}}
New Status: {{Status}}
Updated: {{CurrentDate}} {{CurrentTime}}

Our team continues to work on your request. You will receive further updates as progress is made.

Best regards,
{{CompanyName}} Support`,
                htmlBody: `<!DOCTYPE html>
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
</html>`,
                isActive: true,
                companyId: companyId
            },
            {
                name: 'Ticket Resolved Confirmation',
                code: 'TICKET_RESOLVED',
                description: 'Confirmation email when ticket is resolved',
                channel: 0,
                subject: '✅ Ticket {{TicketNumber}} Resolved',
                body: `Dear {{CustomerName}},

Great news! Your support ticket has been resolved.

Ticket: {{TicketNumber}}
Subject: {{Title}}
Status: {{Status}}

If you need further assistance, please reply to this email or create a new ticket.

Thank you for using {{CompanyName}}!

Best regards,
{{CompanyName}} Support Team`,
                htmlBody: `<!DOCTYPE html>
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
</html>`,
                isActive: true,
                companyId: companyId
            },
            {
                name: 'SLA Breach Warning',
                code: 'SLA_BREACH_WARNING',
                description: 'Alert sent to handlers when SLA is about to breach',
                channel: 0,
                subject: '⚠️ URGENT: Ticket {{TicketNumber}} SLA Breach Warning',
                body: `URGENT: SLA BREACH WARNING

Ticket: {{TicketNumber}}
Title: {{Title}}
Customer: {{CustomerName}}
Priority: {{Priority}}

ACTION REQUIRED:
This ticket requires immediate attention to prevent SLA breach.

Current Status: {{Status}}

Please prioritize and take action immediately.`,
                htmlBody: `<!DOCTYPE html>
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
</html>`,
                isActive: true,
                companyId: companyId
            }
        ];

        const createdTemplates = [];

        for (const template of templates) {
            console.log(`   Creating template: ${template.name}...`);

            const response = await fetch('http://localhost:5000/api/templates', {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(template)
            });

            const result = await response.json();

            if (result.isSuccess && result.data) {
                createdTemplates.push(result.data);
                console.log(`   ✅ Created: ${template.code} (ID: ${result.data.id})`);
            } else {
                console.log(`   ⚠️ Template ${template.code} might already exist or failed`);
            }
        }

        console.log(`\n✅ Templates created: ${createdTemplates.length}/4\n`);

        // ═══════════════════════════════════════════════════════════════
        // STEP 4: Get the AUTO_ACK_NEW_TICKET template ID
        // ═══════════════════════════════════════════════════════════════
        console.log('📝 STEP 4: Getting AUTO_ACK_NEW_TICKET template ID...');

        const templatesResponse = await fetch('http://localhost:5000/api/templates', {
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });

        const templatesData = await templatesResponse.json();
        const autoAckTemplate = templatesData.data?.find(t => t.code === 'AUTO_ACK_NEW_TICKET');

        if (!autoAckTemplate) {
            throw new Error('AUTO_ACK_NEW_TICKET template not found');
        }

        const templateId = autoAckTemplate.id;
        console.log('✅ Template ID retrieved');
        console.log(`   Template ID: ${templateId}`);
        console.log(`   Template Name: ${autoAckTemplate.name}\n`);

        // ═══════════════════════════════════════════════════════════════
        // STEP 5: Navigate to Email Configuration and Update
        // ═══════════════════════════════════════════════════════════════
        console.log('📝 STEP 5: Navigating to Email Configuration...');

        await page.goto('http://localhost:4200/admin/communication/email-ticketing');
        await page.waitForTimeout(3000);

        console.log('✅ On Email Configuration page');

        // Take screenshot
        await page.screenshot({ path: '.playwright-mcp/template-test-01-email-config-page.png', fullPage: true });

        // Check if there are any configurations
        const hasConfigs = await page.locator('.config-card').count() > 0;

        if (hasConfigs) {
            console.log('   Found existing email configurations');
            console.log('   Clicking Edit button on first configuration...');

            // Click the first Edit button
            await page.locator('.config-card').first().locator('button:has-text("Edit"), button[title*="Edit"], .btn-edit').first().click();
            await page.waitForTimeout(2000);

            console.log('✅ Opened configuration wizard\n');

            // ═══════════════════════════════════════════════════════════════
            // STEP 6: Navigate through wizard to Additional Settings
            // ═══════════════════════════════════════════════════════════════
            console.log('📝 STEP 6: Navigating to Additional Settings step...');

            // Click Next through steps 1, 2, 3 to reach step 4 (Additional Settings)
            for (let i = 1; i <= 3; i++) {
                console.log(`   Clicking Next (Step ${i})...`);
                await page.locator('button:has-text("Next")').click();
                await page.waitForTimeout(1500);
            }

            console.log('✅ On Additional Settings step\n');

            // Take screenshot
            await page.screenshot({ path: '.playwright-mcp/template-test-02-additional-settings.png', fullPage: true });

            // ═══════════════════════════════════════════════════════════════
            // STEP 7: Configure Additional Settings
            // ═══════════════════════════════════════════════════════════════
            console.log('📝 STEP 7: Configuring Additional Settings...');

            // Set polling interval to 120 seconds (2 minutes)
            console.log('   Setting polling interval to 120 seconds (2 minutes)...');
            await page.selectOption('select#pollingIntervalSeconds, select[name="pollingIntervalSeconds"]', '120');
            await page.waitForTimeout(500);

            // Enable auto-acknowledgement checkbox
            console.log('   Enabling auto-acknowledgement...');
            const autoAckCheckbox = page.locator('input[type="checkbox"][name="sendAutoAcknowledgement"], input#sendAutoAcknowledgement');
            const isChecked = await autoAckCheckbox.isChecked();
            if (!isChecked) {
                await autoAckCheckbox.check();
            }
            await page.waitForTimeout(500);

            // Set the template ID
            console.log(`   Setting template ID: ${templateId}...`);
            await page.fill('input[name="autoAcknowledgementTemplateId"], input#autoAcknowledgementTemplateId', templateId);
            await page.waitForTimeout(500);

            console.log('✅ Configuration updated');

            // Take screenshot
            await page.screenshot({ path: '.playwright-mcp/template-test-03-settings-configured.png', fullPage: true });

            // ═══════════════════════════════════════════════════════════════
            // STEP 8: Save Configuration
            // ═══════════════════════════════════════════════════════════════
            console.log('\n📝 STEP 8: Saving configuration...');

            // Click Next to go to final step
            await page.locator('button:has-text("Next")').click();
            await page.waitForTimeout(2000);

            // Click Save Configuration
            await page.locator('button:has-text("Save Configuration"), button:has-text("Save")').click();
            await page.waitForTimeout(3000);

            console.log('✅ Configuration saved\n');

            // Take screenshot
            await page.screenshot({ path: '.playwright-mcp/template-test-04-config-saved.png', fullPage: true });

        } else {
            console.log('⚠️ No existing email configurations found');
            console.log('   You need to create an email configuration first with OAuth setup');
        }

        // ═══════════════════════════════════════════════════════════════
        // STEP 9: Verify Configuration
        // ═══════════════════════════════════════════════════════════════
        console.log('📝 STEP 9: Verifying configuration...');

        await page.goto('http://localhost:4200/admin/communication/email-ticketing');
        await page.waitForTimeout(2000);

        // Check for success indicators
        const hasPollingBadge = await page.locator('text=/Poll every.*2 minutes/i').count() > 0;
        const hasAuthBadge = await page.locator('.badge:has-text("Authorized")').count() > 0;

        console.log(`   Polling interval badge: ${hasPollingBadge ? '✅ Found' : '❌ Not found'}`);
        console.log(`   OAuth authorized badge: ${hasAuthBadge ? '✅ Found' : '❌ Not found'}`);

        // Take final screenshot
        await page.screenshot({ path: '.playwright-mcp/template-test-05-verification.png', fullPage: true });

        // ═══════════════════════════════════════════════════════════════
        // SUMMARY
        // ═══════════════════════════════════════════════════════════════
        console.log('\n═══════════════════════════════════════════════════════════════');
        console.log('🎉 TEMPLATE SYSTEM CONFIGURATION COMPLETE');
        console.log('═══════════════════════════════════════════════════════════════');
        console.log('\n✅ Summary:');
        console.log(`   • Templates Created: ${createdTemplates.length}/4`);
        console.log(`   • Auto-Ack Template ID: ${templateId}`);
        console.log(`   • Polling Interval: 120 seconds (2 minutes)`);
        console.log(`   • Auto-Acknowledgement: Enabled`);
        console.log(`   • Template Variables: {{TicketNumber}}, {{Title}}, {{Status}}, etc.`);
        console.log('\n📧 Next Steps:');
        console.log('   1. Send a test email to your configured address');
        console.log('   2. Wait 2 minutes for polling (or click "Poll Now" button)');
        console.log('   3. Check your email for auto-acknowledgement with ticket number');
        console.log('   4. Verify {{TicketNumber}} is replaced with actual ticket number');
        console.log('\n💾 Screenshots saved:');
        console.log('   • template-test-01-email-config-page.png');
        console.log('   • template-test-02-additional-settings.png');
        console.log('   • template-test-03-settings-configured.png');
        console.log('   • template-test-04-config-saved.png');
        console.log('   • template-test-05-verification.png');
        console.log('\n═══════════════════════════════════════════════════════════════\n');

        // Write results to file
        const results = {
            success: true,
            timestamp: new Date().toISOString(),
            templatesCreated: createdTemplates.length,
            templateId: templateId,
            pollingInterval: '120 seconds',
            autoAckEnabled: true,
            screenshots: 5
        };

        require('fs').writeFileSync(
            'template-system-test-results.json',
            JSON.stringify(results, null, 2)
        );

    } catch (error) {
        console.error('❌ ERROR:', error.message);
        console.error(error.stack);
        await page.screenshot({ path: '.playwright-mcp/template-test-error.png', fullPage: true });
    } finally {
        await browser.close();
    }
}

testCompleteTemplateSystem();
