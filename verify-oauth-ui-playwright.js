// Playwright OAuth UI Verification
// Automated test to verify OAuth configuration in the UI

const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const BASE_URL = 'http://localhost:4200';
const API_URL = 'http://localhost:5000';
const ADMIN_EMAIL = 'admin@complaintmanagement.com';
const ADMIN_PASSWORD = 'Admin@123';
const SCREENSHOT_DIR = '.playwright-oauth-verification';

// Create screenshot directory
if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

async function saveScreenshot(page, name) {
    const screenshotPath = path.join(SCREENSHOT_DIR, `${name}.png`);
    await page.screenshot({ path: screenshotPath, fullPage: true });
    console.log(`   📸 Screenshot saved: ${name}.png`);
    return screenshotPath;
}

async function runOAuthVerification() {
    console.log('================================');
    console.log('OAuth UI Verification (Playwright)');
    console.log('================================\n');

    const browser = await chromium.launch({
        headless: false,
        slowMo: 500 // Slow down for visibility
    });

    const context = await browser.newContext({
        viewport: { width: 1920, height: 1080 }
    });

    const page = await context.newPage();

    const results = {
        timestamp: new Date().toISOString(),
        tests: [],
        screenshots: [],
        overallStatus: 'PASS'
    };

    try {
        // Test 1: Check if frontend is accessible
        console.log('[1/6] Checking frontend accessibility...');
        try {
            await page.goto(BASE_URL, { waitUntil: 'networkidle', timeout: 10000 });
            console.log('   ✓ Frontend is accessible');
            results.tests.push({ name: 'Frontend Accessibility', status: 'PASS' });
        } catch (error) {
            console.log('   ✗ Frontend not accessible:', error.message);
            results.tests.push({ name: 'Frontend Accessibility', status: 'FAIL', error: error.message });
            results.overallStatus = 'FAIL';
            throw new Error('Frontend not accessible - servers may not be running');
        }

        // Test 2: Login as admin
        console.log('[2/6] Logging in as admin...');
        try {
            await page.waitForSelector('input[type="email"], input[formControlName="email"]', { timeout: 5000 });
            await page.fill('input[type="email"], input[formControlName="email"]', ADMIN_EMAIL);
            await page.fill('input[type="password"], input[formControlName="password"]', ADMIN_PASSWORD);

            await saveScreenshot(page, '01-login-page');

            await page.click('button[type="submit"]');
            await page.waitForURL('**/dashboard', { timeout: 10000 });

            console.log('   ✓ Login successful');
            results.tests.push({ name: 'Admin Login', status: 'PASS' });

            await saveScreenshot(page, '02-dashboard');
        } catch (error) {
            console.log('   ✗ Login failed:', error.message);
            await saveScreenshot(page, '02-login-failed');
            results.tests.push({ name: 'Admin Login', status: 'FAIL', error: error.message });
            results.overallStatus = 'FAIL';
            throw error;
        }

        // Test 3: Navigate to Email Ticketing Config
        console.log('[3/6] Navigating to Email Ticketing Configuration...');
        try {
            await page.goto(`${BASE_URL}/admin/email-ticketing-config`, { waitUntil: 'networkidle' });
            await page.waitForSelector('table, .email-config-table, mat-table', { timeout: 5000 });

            console.log('   ✓ Email configuration page loaded');
            results.tests.push({ name: 'Navigate to Email Config', status: 'PASS' });

            await saveScreenshot(page, '03-email-config-page');
        } catch (error) {
            console.log('   ✗ Navigation failed:', error.message);
            await saveScreenshot(page, '03-navigation-failed');
            results.tests.push({ name: 'Navigate to Email Config', status: 'FAIL', error: error.message });
            results.overallStatus = 'FAIL';
            throw error;
        }

        // Test 4: Find OAuth badge
        console.log('[4/6] Checking OAuth badge status...');
        try {
            // Wait for the page to stabilize
            await page.waitForTimeout(2000);

            // Look for badge element with various selectors
            const badgeSelectors = [
                '.auth-badge',
                '.badge',
                'span.badge',
                '[class*="badge"]',
                'td:has-text("OAuth")',
                'td:has-text("Basic Auth")'
            ];

            let badgeFound = false;
            let badgeText = '';

            for (const selector of badgeSelectors) {
                try {
                    const badge = await page.$(selector);
                    if (badge) {
                        badgeText = await badge.innerText();
                        if (badgeText.includes('OAuth') || badgeText.includes('Basic')) {
                            badgeFound = true;
                            break;
                        }
                    }
                } catch (e) {
                    // Try next selector
                }
            }

            if (!badgeFound) {
                // Try to find any text with OAuth or Basic Auth
                const bodyText = await page.textContent('body');
                if (bodyText.includes('OAuth') || bodyText.includes('Basic Auth')) {
                    badgeText = bodyText.match(/(OAuth 2\.0|Basic Auth)[^<]*/)?.[0] || 'Found OAuth/Auth text';
                    badgeFound = true;
                }
            }

            if (badgeFound) {
                console.log(`   ✓ Badge found: "${badgeText}"`);

                // Determine badge status
                let badgeStatus = 'UNKNOWN';
                let badgeColor = 'gray';

                if (badgeText.includes('Basic Auth')) {
                    badgeStatus = 'FAIL - Still showing Basic Auth (database fix not applied)';
                    badgeColor = 'blue';
                    results.overallStatus = 'FAIL';
                } else if (badgeText.includes('Authorized')) {
                    badgeStatus = 'PASS - OAuth Authorized (green)';
                    badgeColor = 'green';
                } else if (badgeText.includes('Pending')) {
                    badgeStatus = 'PASS - OAuth Pending (orange, needs authorization)';
                    badgeColor = 'orange';
                } else if (badgeText.includes('Expired')) {
                    badgeStatus = 'PASS - OAuth Expired (red, needs refresh)';
                    badgeColor = 'red';
                } else if (badgeText.includes('Not Configured')) {
                    badgeStatus = 'PASS - OAuth Not Configured (needs credentials)';
                    badgeColor = 'orange';
                } else if (badgeText.includes('OAuth')) {
                    badgeStatus = 'PASS - OAuth detected';
                    badgeColor = 'unknown';
                }

                console.log(`   Badge Status: ${badgeStatus}`);
                console.log(`   Badge Color: ${badgeColor}`);

                results.tests.push({
                    name: 'OAuth Badge Detection',
                    status: badgeStatus.startsWith('PASS') ? 'PASS' : 'FAIL',
                    badgeText: badgeText,
                    badgeStatus: badgeStatus,
                    badgeColor: badgeColor
                });
            } else {
                console.log('   ✗ OAuth badge not found');
                results.tests.push({ name: 'OAuth Badge Detection', status: 'FAIL', error: 'Badge not found' });
                results.overallStatus = 'FAIL';
            }

            await saveScreenshot(page, '04-oauth-badge-status');

        } catch (error) {
            console.log('   ✗ Badge check failed:', error.message);
            await saveScreenshot(page, '04-badge-check-failed');
            results.tests.push({ name: 'OAuth Badge Detection', status: 'FAIL', error: error.message });
            results.overallStatus = 'FAIL';
        }

        // Test 5: Check for Authorize/Refresh buttons
        console.log('[5/6] Checking for OAuth action buttons...');
        try {
            const buttonSelectors = [
                'button:has-text("Authorize Now")',
                'button:has-text("Refresh OAuth")',
                '[class*="authorize"]',
                '[class*="oauth-button"]'
            ];

            let buttonFound = false;
            let buttonText = '';

            for (const selector of buttonSelectors) {
                try {
                    const button = await page.$(selector);
                    if (button) {
                        buttonText = await button.innerText();
                        buttonFound = true;
                        break;
                    }
                } catch (e) {
                    // Try next selector
                }
            }

            if (buttonFound) {
                console.log(`   ✓ OAuth button found: "${buttonText}"`);
                results.tests.push({
                    name: 'OAuth Action Button',
                    status: 'PASS',
                    buttonText: buttonText
                });
            } else {
                console.log('   ! OAuth button not found (may be fully authorized)');
                results.tests.push({
                    name: 'OAuth Action Button',
                    status: 'INFO',
                    note: 'No button found - may already be authorized'
                });
            }

            await saveScreenshot(page, '05-oauth-buttons');

        } catch (error) {
            console.log('   ✗ Button check failed:', error.message);
            results.tests.push({ name: 'OAuth Action Button', status: 'FAIL', error: error.message });
        }

        // Test 6: Verify database via API
        console.log('[6/6] Verifying database state via API...');
        try {
            // Get admin token first
            const loginResponse = await page.evaluate(async (apiUrl, email, password) => {
                const response = await fetch(`${apiUrl}/api/auth/login`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ email, password })
                });
                return response.json();
            }, API_URL, ADMIN_EMAIL, ADMIN_PASSWORD);

            if (loginResponse.data && loginResponse.data.token) {
                const token = loginResponse.data.token;

                // Get email configurations
                const configResponse = await page.evaluate(async (apiUrl, token) => {
                    const response = await fetch(`${apiUrl}/api/email-configuration`, {
                        headers: { 'Authorization': `Bearer ${token}` }
                    });
                    return response.json();
                }, API_URL, token);

                if (configResponse.data && configResponse.data.length > 0) {
                    const config = configResponse.data.find(c => c.fromEmail === 'marketing@oryggitech.com');

                    if (config) {
                        console.log(`   ✓ Email configuration found in database`);
                        console.log(`   Email: ${config.fromEmail}`);
                        console.log(`   AuthenticationType: ${config.authenticationType} (${config.authenticationType === 1 ? 'OAuth' : config.authenticationType === 0 ? 'Basic' : 'Unknown'})`);
                        console.log(`   Has OAuth Client ID: ${config.oAuthClientId ? 'Yes' : 'No'}`);
                        console.log(`   Has OAuth Access Token: ${config.oAuthAccessToken ? 'Yes' : 'No'}`);

                        results.tests.push({
                            name: 'Database Verification',
                            status: 'PASS',
                            authenticationType: config.authenticationType,
                            hasClientId: !!config.oAuthClientId,
                            hasAccessToken: !!config.oAuthAccessToken
                        });
                    } else {
                        console.log('   ! Configuration not found for marketing@oryggitech.com');
                        results.tests.push({ name: 'Database Verification', status: 'WARN', note: 'Config not found' });
                    }
                } else {
                    console.log('   ! No email configurations found');
                    results.tests.push({ name: 'Database Verification', status: 'WARN', note: 'No configs' });
                }
            } else {
                console.log('   ✗ Failed to get API token');
                results.tests.push({ name: 'Database Verification', status: 'FAIL', error: 'No token' });
            }

        } catch (error) {
            console.log('   ✗ API verification failed:', error.message);
            results.tests.push({ name: 'Database Verification', status: 'FAIL', error: error.message });
        }

    } catch (error) {
        console.error('\n❌ Test suite failed:', error.message);
        results.overallStatus = 'FAIL';
        results.error = error.message;
    } finally {
        await browser.close();
    }

    // Generate report
    console.log('\n================================');
    console.log('Test Results Summary');
    console.log('================================');

    results.tests.forEach((test, index) => {
        const icon = test.status === 'PASS' ? '✓' : test.status === 'FAIL' ? '✗' : 'ℹ';
        console.log(`${icon} ${test.name}: ${test.status}`);
        if (test.badgeText) console.log(`  Badge: "${test.badgeText}"`);
        if (test.badgeStatus) console.log(`  Status: ${test.badgeStatus}`);
        if (test.buttonText) console.log(`  Button: "${test.buttonText}"`);
        if (test.error) console.log(`  Error: ${test.error}`);
        if (test.note) console.log(`  Note: ${test.note}`);
    });

    console.log('\n================================');
    console.log(`Overall Status: ${results.overallStatus}`);
    console.log('================================\n');

    // Save results to file
    const resultsPath = path.join(SCREENSHOT_DIR, 'verification-results.json');
    fs.writeFileSync(resultsPath, JSON.stringify(results, null, 2));
    console.log(`📄 Results saved to: ${resultsPath}`);
    console.log(`📁 Screenshots saved to: ${SCREENSHOT_DIR}/`);

    return results.overallStatus === 'PASS' ? 0 : 1;
}

// Run the verification
runOAuthVerification()
    .then(exitCode => {
        process.exit(exitCode);
    })
    .catch(error => {
        console.error('Fatal error:', error);
        process.exit(1);
    });
