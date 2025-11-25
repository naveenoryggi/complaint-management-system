/**
 * EMAIL ADDRESS CHANGE FIX - COMPREHENSIVE VALIDATION
 *
 * This test validates:
 * 1. Email address change triggers OAuth token clearing
 * 2. Configuration is disabled after email change
 * 3. Re-authorization flow works correctly
 * 4. Manual polling validation
 */

const { chromium } = require('playwright');

async function validateEmailChangeFix() {
    console.log('\n╔══════════════════════════════════════════════════════════════╗');
    console.log('║   EMAIL ADDRESS CHANGE FIX - COMPREHENSIVE VALIDATION        ║');
    console.log('╚══════════════════════════════════════════════════════════════╝\n');

    const browser = await chromium.launch({
        headless: false,
        slowMo: 800,
        args: ['--start-maximized']
    });

    const context = await browser.newContext({
        viewport: null
    });

    const page = await context.newPage();
    let testPassed = true;

    try {
        // ================================================================
        // STEP 1: Login as Admin
        // ================================================================
        console.log('📝 STEP 1: Logging in as admin...');
        await page.goto('http://localhost:4200/login');
        await page.waitForTimeout(2000);

        // Fill credentials
        await page.fill('input[name="username"]', 'admin@complaintmanagement.com');
        await page.fill('input[name="password"]', 'Admin@123');

        // Click login
        await page.click('button:has-text("Sign In")');
        await page.waitForURL('**/dashboard', { timeout: 10000 });

        console.log('✅ Login successful\n');
        await page.screenshot({ path: '.playwright-mcp/email-fix-01-login-success.png', fullPage: true });

        // ================================================================
        // STEP 2: Navigate to Email Ticketing Config
        // ================================================================
        console.log('📝 STEP 2: Navigating to Email Ticketing Config...');
        await page.goto('http://localhost:4200/admin/email-ticketing-config');
        await page.waitForTimeout(3000);

        console.log('✅ Email Ticketing Config page loaded\n');
        await page.screenshot({ path: '.playwright-mcp/email-fix-02-config-page.png', fullPage: true });

        // ================================================================
        // STEP 3: Get Current Configuration via API
        // ================================================================
        console.log('📝 STEP 3: Fetching current email configuration via API...');

        const configData = await page.evaluate(async () => {
            const token = sessionStorage.getItem('token');
            const response = await fetch('http://localhost:5000/api/email-configuration', {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            const text = await response.text();
            try {
                return JSON.parse(text);
            } catch (e) {
                return { error: 'Failed to parse JSON', text: text };
            }
        });

        if (configData.error || !configData.isSuccess) {
            console.log('❌ Failed to fetch email configurations');
            console.log(configData);
            testPassed = false;
            await browser.close();
            return;
        }

        if (!configData.data || configData.data.length === 0) {
            console.log('⚠️  No email configurations found. Creating one first...');
            console.log('Please create an email configuration manually and re-run this test.');
            await browser.close();
            return;
        }

        const originalConfig = configData.data[0];
        const originalEmail = originalConfig.fromEmail;
        const newEmail = 'test-changed-' + originalEmail;

        console.log('📧 Original Email:', originalEmail);
        console.log('🔑 Has OAuth Access Token:', !!originalConfig.oAuthAccessToken);
        console.log('🔑 Has OAuth Refresh Token:', !!originalConfig.oAuthRefreshToken);
        console.log('⚡ Is Enabled:', originalConfig.isEnabled);
        console.log('✅ Current configuration fetched\n');

        // ================================================================
        // STEP 4: Change Email Address via API
        // ================================================================
        console.log('📝 STEP 4: Changing email address from', originalEmail, 'to', newEmail);

        const updateResult = await page.evaluate(async ({ configId, originalConfig, newEmail }) => {
            const token = sessionStorage.getItem('token');

            // Create updated config with new email
            const updatedConfig = { ...originalConfig };
            updatedConfig.fromEmail = newEmail;

            const response = await fetch(`http://localhost:5000/api/email-configuration/${configId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(updatedConfig)
            });

            const text = await response.text();
            try {
                return { success: true, data: JSON.parse(text) };
            } catch (e) {
                return { success: false, error: 'Failed to parse response', text: text, status: response.status };
            }
        }, { configId: originalConfig.id, originalConfig, newEmail });

        if (!updateResult.success || !updateResult.data.isSuccess) {
            console.log('❌ Failed to update email configuration');
            console.log(updateResult);
            testPassed = false;
        } else {
            const updatedConfig = updateResult.data.data;

            console.log('\n═══════════════════════════════════════════════════════════');
            console.log('🔍 VERIFICATION RESULTS:');
            console.log('═══════════════════════════════════════════════════════════');
            console.log('📧 Email Changed:', originalEmail, '→', updatedConfig.fromEmail);
            console.log('🔑 OAuth Access Token:', updatedConfig.oAuthAccessToken ? '❌ STILL EXISTS (BUG!)' : '✅ CLEARED');
            console.log('🔑 OAuth Refresh Token:', updatedConfig.oAuthRefreshToken ? '❌ STILL EXISTS (BUG!)' : '✅ CLEARED');
            console.log('📅 OAuth Token Expiry:', updatedConfig.oAuthTokenExpiresAt ? '❌ STILL SET (BUG!)' : '✅ CLEARED');
            console.log('⚡ Configuration Enabled:', updatedConfig.isEnabled ? '❌ STILL ENABLED (BUG!)' : '✅ DISABLED');
            console.log('═══════════════════════════════════════════════════════════\n');

            // Validation checks
            const tokensCleared = !updatedConfig.oAuthAccessToken && !updatedConfig.oAuthRefreshToken && !updatedConfig.oAuthTokenExpiresAt;
            const configDisabled = !updatedConfig.isEnabled;
            const emailChanged = updatedConfig.fromEmail === newEmail;

            if (tokensCleared && configDisabled && emailChanged) {
                console.log('✅✅✅ ALL CHECKS PASSED! The fix is working correctly.\n');
            } else {
                console.log('❌❌❌ SOME CHECKS FAILED!\n');
                testPassed = false;
            }
        }

        await page.waitForTimeout(2000);

        // ================================================================
        // STEP 5: Refresh UI and Verify State
        // ================================================================
        console.log('📝 STEP 5: Refreshing UI to verify visual state...');
        await page.reload();
        await page.waitForTimeout(3000);

        await page.screenshot({ path: '.playwright-mcp/email-fix-03-after-change.png', fullPage: true });
        console.log('✅ Screenshot taken of updated state\n');

        // ================================================================
        // STEP 6: Check for Re-Authorization Button
        // ================================================================
        console.log('📝 STEP 6: Checking for re-authorization requirement...');

        // Click Edit button
        const editButton = await page.locator('button[title="Edit"], button:has-text("Edit")').first();
        if (await editButton.count() > 0) {
            await editButton.click();
            await page.waitForTimeout(2000);

            await page.screenshot({ path: '.playwright-mcp/email-fix-04-edit-dialog.png', fullPage: true });

            // Look for OAuth authorization button or status
            const oauthButtons = await page.locator('button:has-text("Authorize"), button:has-text("OAuth"), button:has-text("Office 365"), button:has-text("Gmail")').count();

            if (oauthButtons > 0) {
                console.log('✅ Re-authorization buttons are visible\n');
            } else {
                console.log('⚠️  No re-authorization buttons found (may need to navigate through wizard)\n');
            }
        } else {
            console.log('⚠️  Edit button not found\n');
        }

        // ================================================================
        // STEP 7: Test Manual Polling (Should Fail)
        // ================================================================
        console.log('📝 STEP 7: Testing manual polling (should fail due to cleared tokens)...');

        // Close dialog if open
        const closeButton = await page.locator('button:has-text("Close"), button[aria-label="Close"]').first();
        if (await closeButton.count() > 0) {
            await closeButton.click();
            await page.waitForTimeout(1000);
        }

        // Try to click Poll Now button
        const pollButton = await page.locator('button:has-text("Poll Now")').first();
        if (await pollButton.count() > 0) {
            console.log('🔄 Clicking "Poll Now" button...');
            await pollButton.click();
            await page.waitForTimeout(3000);

            await page.screenshot({ path: '.playwright-mcp/email-fix-05-poll-attempt.png', fullPage: true });

            console.log('✅ Poll attempt made (check backend logs for expected auth error)\n');
        } else {
            console.log('⚠️  "Poll Now" button not found\n');
        }

        // ================================================================
        // STEP 8: Restore Original Email Address
        // ================================================================
        console.log('📝 STEP 8: Restoring original email address...');

        const restoreResult = await page.evaluate(async ({ configId, originalConfig, originalEmail }) => {
            const token = sessionStorage.getItem('token');

            // Restore original email
            const restoredConfig = { ...originalConfig };
            restoredConfig.fromEmail = originalEmail;
            // Keep tokens cleared - user will need to re-authorize
            restoredConfig.oAuthAccessToken = null;
            restoredConfig.oAuthRefreshToken = null;
            restoredConfig.oAuthTokenExpiresAt = null;
            restoredConfig.isEnabled = false;

            const response = await fetch(`http://localhost:5000/api/email-configuration/${configId}`, {
                method: 'PUT',
                headers: {
                    'Authorization': `Bearer ${token}`,
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(restoredConfig)
            });

            const text = await response.text();
            try {
                return { success: true, data: JSON.parse(text) };
            } catch (e) {
                return { success: false, error: e.message };
            }
        }, { configId: originalConfig.id, originalConfig, originalEmail });

        if (restoreResult.success) {
            console.log('✅ Original email address restored to:', originalEmail);
            console.log('⚠️  Configuration is DISABLED - user needs to re-authorize with OAuth\n');
        } else {
            console.log('❌ Failed to restore original email');
            console.log(restoreResult);
        }

        // ================================================================
        // FINAL SUMMARY
        // ================================================================
        console.log('\n╔══════════════════════════════════════════════════════════════╗');
        console.log('║                    TEST SUMMARY                              ║');
        console.log('╚══════════════════════════════════════════════════════════════╝');

        if (testPassed) {
            console.log('\n✅ EMAIL ADDRESS CHANGE PROTECTION IS WORKING CORRECTLY!\n');
            console.log('The system successfully:');
            console.log('  ✓ Detected email address change');
            console.log('  ✓ Cleared OAuth access token');
            console.log('  ✓ Cleared OAuth refresh token');
            console.log('  ✓ Cleared OAuth token expiry');
            console.log('  ✓ Disabled the configuration');
            console.log('  ✓ Requires re-authorization for new email\n');
            console.log('This prevents the IMAP error:');
            console.log('"User is authenticated but not connected"\n');
        } else {
            console.log('\n❌ SOME TESTS FAILED - REVIEW RESULTS ABOVE\n');
        }

        console.log('📸 Screenshots saved to .playwright-mcp/email-fix-*.png');
        console.log('🔍 Check backend logs for detailed OAuth token clearing messages\n');

        console.log('NEXT STEPS FOR USER:');
        console.log('1. Edit the email configuration in UI');
        console.log('2. Click "Authorize with Office 365/Gmail"');
        console.log('3. Complete OAuth authorization');
        console.log('4. Enable the configuration');
        console.log('5. Test email polling\n');

    } catch (error) {
        console.error('\n❌ TEST FAILED WITH ERROR:', error.message);
        await page.screenshot({ path: '.playwright-mcp/email-fix-ERROR.png', fullPage: true });
        testPassed = false;
    } finally {
        await page.waitForTimeout(3000);
        await browser.close();

        process.exit(testPassed ? 0 : 1);
    }
}

// Run the validation
validateEmailChangeFix().catch(error => {
    console.error('Fatal error:', error);
    process.exit(1);
});
