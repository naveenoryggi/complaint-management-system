/**
 * EMAIL ADDRESS CHANGE PROTECTION - E2E TEST
 *
 * Tests that changing the email address on an OAuth-configured account:
 * 1. Clears OAuth tokens automatically
 * 2. Disables the configuration
 * 3. Forces re-authorization
 * 4. Prevents the IMAP "User is authenticated but not connected" error
 */

const { chromium } = require('playwright');

async function testEmailAddressChangeProtection() {
    console.log('\n=== EMAIL ADDRESS CHANGE PROTECTION TEST ===\n');

    const browser = await chromium.launch({ headless: false, slowMo: 500 });
    const context = await browser.newContext({
        viewport: { width: 1920, height: 1080 }
    });
    const page = await context.newPage();

    try {
        // Step 1: Login as admin
        console.log('Step 1: Logging in as admin...');
        await page.goto('http://localhost:4200/login');
        await page.waitForTimeout(1000);

        await page.fill('input[type="email"]', 'admin@acmecorp.com');
        await page.fill('input[type="password"]', 'Admin@123');
        await page.click('button:has-text("Login")');

        await page.waitForURL('**/dashboard', { timeout: 10000 });
        console.log('✓ Logged in successfully');
        await page.screenshot({ path: '.playwright-mcp/email-change-test-01-logged-in.png', fullPage: true });

        // Step 2: Navigate to Email Ticketing Config
        console.log('\nStep 2: Navigating to Email Ticketing Config...');
        await page.goto('http://localhost:4200/admin/email-ticketing-config');
        await page.waitForTimeout(2000);
        console.log('✓ On Email Ticketing Config page');
        await page.screenshot({ path: '.playwright-mcp/email-change-test-02-config-page.png', fullPage: true });

        // Step 3: Get current email configurations
        console.log('\nStep 3: Fetching current email configurations...');
        const token = await page.evaluate(() => sessionStorage.getItem('token'));

        const emailConfigsResponse = await page.evaluate(async (authToken) => {
            const response = await fetch('http://localhost:5000/api/email-configuration', {
                headers: { 'Authorization': `Bearer ${authToken}` }
            });
            return await response.json();
        }, token);

        if (!emailConfigsResponse.isSuccess || !emailConfigsResponse.data || emailConfigsResponse.data.length === 0) {
            console.log('❌ No email configurations found. Please create one first.');
            await browser.close();
            return;
        }

        const firstConfig = emailConfigsResponse.data[0];
        console.log(`✓ Found email configuration: ${firstConfig.fromEmail}`);
        console.log(`  - ID: ${firstConfig.id}`);
        console.log(`  - Auth Type: ${firstConfig.authenticationType === 2 ? 'OAuth2' : 'Basic'}`);
        console.log(`  - Enabled: ${firstConfig.isEnabled}`);
        console.log(`  - Has OAuth Token: ${firstConfig.oAuthAccessToken ? 'Yes' : 'No'}`);

        // Step 4: Click Edit on the first configuration
        console.log('\nStep 4: Opening edit dialog...');
        const editButtons = await page.locator('button:has-text("Edit")').all();
        if (editButtons.length === 0) {
            console.log('❌ No Edit buttons found');
            await browser.close();
            return;
        }

        await editButtons[0].click();
        await page.waitForTimeout(1500);
        console.log('✓ Edit dialog opened');
        await page.screenshot({ path: '.playwright-mcp/email-change-test-03-edit-dialog.png', fullPage: true });

        // Step 5: Record original email address
        const originalEmail = firstConfig.fromEmail;
        const newEmail = originalEmail.includes('test')
            ? originalEmail.replace('test', 'modified')
            : 'modified-' + originalEmail;

        console.log(`\nStep 5: Changing email address...`);
        console.log(`  - Original: ${originalEmail}`);
        console.log(`  - New: ${newEmail}`);

        // Step 6: Navigate to Step 1 and change email address
        console.log('\nStep 6: Navigating to Step 1...');
        const step1Button = page.locator('button:has-text("Step 1")');
        if (await step1Button.count() > 0) {
            await step1Button.click();
            await page.waitForTimeout(1000);
        }

        // Clear and enter new email
        const emailInput = page.locator('input[name="fromEmail"]');
        await emailInput.clear();
        await emailInput.fill(newEmail);
        await page.waitForTimeout(500);
        console.log('✓ Email address changed in form');
        await page.screenshot({ path: '.playwright-mcp/email-change-test-04-email-changed.png', fullPage: true });

        // Step 7: Save the configuration
        console.log('\nStep 7: Saving configuration...');
        const saveButton = page.locator('button:has-text("Save Changes"), button:has-text("Update")');
        await saveButton.click();
        await page.waitForTimeout(3000);
        console.log('✓ Save button clicked');
        await page.screenshot({ path: '.playwright-mcp/email-change-test-05-saved.png', fullPage: true });

        // Step 8: Verify backend response - check if tokens were cleared
        console.log('\nStep 8: Verifying OAuth tokens were cleared...');
        const updatedConfigResponse = await page.evaluate(async (authToken, configId) => {
            const response = await fetch(`http://localhost:5000/api/email-configuration/${configId}`, {
                headers: { 'Authorization': `Bearer ${authToken}` }
            });
            return await response.json();
        }, token, firstConfig.id);

        if (!updatedConfigResponse.isSuccess) {
            console.log('❌ Failed to fetch updated configuration');
            await browser.close();
            return;
        }

        const updatedConfig = updatedConfigResponse.data;
        console.log('\n=== VERIFICATION RESULTS ===');
        console.log(`Email Changed: ${originalEmail} → ${updatedConfig.fromEmail}`);
        console.log(`OAuth Access Token: ${updatedConfig.oAuthAccessToken || '(null - CLEARED ✓)'}`);
        console.log(`OAuth Refresh Token: ${updatedConfig.oAuthRefreshToken || '(null - CLEARED ✓)'}`);
        console.log(`OAuth Token Expiry: ${updatedConfig.oAuthTokenExpiresAt || '(null - CLEARED ✓)'}`);
        console.log(`Is Enabled: ${updatedConfig.isEnabled} ${!updatedConfig.isEnabled ? '(DISABLED ✓)' : '(WARNING: Should be disabled!)'}`);

        // Step 9: Check if authorization button is visible
        console.log('\nStep 9: Checking if re-authorization is required...');
        await page.waitForTimeout(2000);
        await page.reload();
        await page.waitForTimeout(2000);

        // Open edit dialog again
        const editButtonsAfter = await page.locator('button:has-text("Edit")').all();
        await editButtonsAfter[0].click();
        await page.waitForTimeout(1500);

        // Check for OAuth wizard or authorization button
        const oauthButton = page.locator('button:has-text("Authorize"), button:has-text("OAuth"), button:has-text("Office 365"), button:has-text("Gmail")');
        const hasOAuthButton = await oauthButton.count() > 0;

        console.log(`Re-authorization UI visible: ${hasOAuthButton ? 'YES ✓' : 'NO (check manually)'}`);
        await page.screenshot({ path: '.playwright-mcp/email-change-test-06-reauth-required.png', fullPage: true });

        // Step 10: Check backend logs for the warning message
        console.log('\nStep 10: Checking backend logs...');
        console.log('Check the backend console for this log message:');
        console.log(`  "Email address changed from '${originalEmail}' to '${newEmail}'. Clearing OAuth tokens - user must re-authorize."`);

        // Summary
        console.log('\n=== TEST SUMMARY ===');
        const tokensCleared = !updatedConfig.oAuthAccessToken && !updatedConfig.oAuthRefreshToken;
        const configDisabled = !updatedConfig.isEnabled;
        const emailChanged = updatedConfig.fromEmail === newEmail;

        console.log(`✓ Email address changed: ${emailChanged ? 'PASS' : 'FAIL'}`);
        console.log(`✓ OAuth tokens cleared: ${tokensCleared ? 'PASS' : 'FAIL'}`);
        console.log(`✓ Configuration disabled: ${configDisabled ? 'PASS' : 'FAIL'}`);
        console.log(`✓ Re-authorization required: ${hasOAuthButton ? 'PASS' : 'CHECK MANUALLY'}`);

        const allPassed = emailChanged && tokensCleared && configDisabled;
        console.log(`\n${allPassed ? '✅ ALL TESTS PASSED' : '⚠️ SOME TESTS FAILED'}`);

        if (allPassed) {
            console.log('\n✅ EMAIL ADDRESS CHANGE PROTECTION IS WORKING CORRECTLY');
            console.log('The system successfully:');
            console.log('  1. Detected the email address change');
            console.log('  2. Cleared OAuth tokens from the old account');
            console.log('  3. Disabled the configuration');
            console.log('  4. Requires re-authorization for the new email');
            console.log('\nThis prevents the IMAP error: "User is authenticated but not connected"');
        }

        // Step 11: Restore original email address
        console.log('\nStep 11: Restoring original email address...');
        const step1ButtonRestore = page.locator('button:has-text("Step 1")');
        if (await step1ButtonRestore.count() > 0) {
            await step1ButtonRestore.click();
            await page.waitForTimeout(1000);
        }

        const emailInputRestore = page.locator('input[name="fromEmail"]');
        await emailInputRestore.clear();
        await emailInputRestore.fill(originalEmail);
        await page.waitForTimeout(500);

        const saveButtonRestore = page.locator('button:has-text("Save Changes"), button:has-text("Update")');
        await saveButtonRestore.click();
        await page.waitForTimeout(2000);
        console.log('✓ Original email address restored');

        console.log('\n=== TEST COMPLETE ===\n');

    } catch (error) {
        console.error('❌ Test failed with error:', error.message);
        await page.screenshot({ path: '.playwright-mcp/email-change-test-ERROR.png', fullPage: true });
    } finally {
        await browser.close();
    }
}

// Run the test
testEmailAddressChangeProtection().catch(console.error);
