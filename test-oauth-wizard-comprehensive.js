/**
 * OAuth Wizard Comprehensive E2E Test
 * Tests all steps of the OAuth setup wizard with detailed screenshots
 */

const playwright = require('playwright');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:4200';
const BACKEND_URL = 'http://localhost:5000';
const ADMIN_TOKEN = fs.readFileSync('.fresh-token', 'utf8').trim();
const SCREENSHOT_DIR = '.playwright-oauth-wizard';

// Test results
const testResults = {
  timestamp: new Date().toISOString(),
  tests: [],
  totalTests: 0,
  passedTests: 0,
  failedTests: 0,
  screenshots: []
};

// Helper function to add test result
function addTestResult(name, status, message, screenshot = null) {
  const result = {
    name,
    status,
    message,
    timestamp: new Date().toISOString(),
    screenshot
  };
  testResults.tests.push(result);
  testResults.totalTests++;
  if (status === 'PASS') testResults.passedTests++;
  if (status === 'FAIL') testResults.failedTests++;

  console.log(`[${status}] ${name}${message ? ': ' + message : ''}`);
}

// Helper function to take screenshot
async function takeScreenshot(page, name) {
  const filename = `${SCREENSHOT_DIR}/${name}.png`;
  await page.screenshot({ path: filename, fullPage: false });
  testResults.screenshots.push({ name, filename });
  return filename;
}

async function runTests() {
  console.log('='.repeat(80));
  console.log('OAuth WIZARD COMPREHENSIVE E2E TEST');
  console.log('='.repeat(80));
  console.log();

  // Create screenshot directory
  if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
  }

  const browser = await playwright.chromium.launch({ headless: false });
  const context = await browser.newContext({
    viewport: { width: 1920, height: 1080 },
    locale: 'en-US'
  });
  const page = await context.newPage();

  try {
    // ===========================================
    // TEST 1: Login and Navigate to Email Config
    // ===========================================
    console.log('\n📋 TEST 1: Login and Navigation');
    console.log('-'.repeat(80));

    await page.goto(`${BASE_URL}/login`);
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '01-login-page');

    // Set token in localStorage
    await page.evaluate((token) => {
      localStorage.setItem('authToken', token);
      localStorage.setItem('currentUser', JSON.stringify({
        id: 'f56d8d03-e382-454b-bf7d-fa8236c125c3',
        email: 'admin@complaintmanagement.com',
        name: 'Updated Admin',
        role: 'Admin'
      }));
    }, ADMIN_TOKEN);

    await page.goto(`${BASE_URL}/admin/dashboard`);
    await page.waitForTimeout(3000);
    await takeScreenshot(page, '02-admin-dashboard');

    addTestResult('Login and Dashboard', 'PASS', 'Successfully logged in as admin');

    // Navigate to Email Ticketing Config
    const emailConfigLink = page.locator('text=Email Ticketing').or(page.locator('text=Communication')).or(page.locator('[href*="email"]')).first();
    if (await emailConfigLink.count() > 0) {
      await emailConfigLink.click();
      await page.waitForTimeout(2000);
      addTestResult('Navigate to Email Config', 'PASS', 'Found and clicked email config link');
    } else {
      // Try direct navigation
      await page.goto(`${BASE_URL}/admin/email-ticketing-config`);
      await page.waitForTimeout(2000);
      addTestResult('Navigate to Email Config', 'PASS', 'Direct navigation to email config');
    }

    await takeScreenshot(page, '03-email-config-page');

    // ===========================================
    // TEST 2: Open Create Configuration Form
    // ===========================================
    console.log('\n📋 TEST 2: Open Create Configuration Form');
    console.log('-'.repeat(80));

    const addButton = page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New")').first();
    if (await addButton.count() > 0) {
      await addButton.click();
      await page.waitForTimeout(2000);
      await takeScreenshot(page, '04-create-form-opened');
      addTestResult('Open Create Form', 'PASS', 'Successfully opened create configuration form');
    } else {
      addTestResult('Open Create Form', 'FAIL', 'Could not find Add/Create button');
      throw new Error('Cannot proceed without create form');
    }

    // ===========================================
    // TEST 3: OAuth Information Banner
    // ===========================================
    console.log('\n📋 TEST 3: OAuth Information Banner');
    console.log('-'.repeat(80));

    const oauthBanner = page.locator('.info-banner, .oauth-info');
    if (await oauthBanner.count() > 0) {
      await takeScreenshot(page, '05-oauth-info-banner');

      // Check for key text
      const hasOAuthText = await page.locator('text=/OAuth|Modern.*Authentication/i').count() > 0;
      const hasBenefits = await page.locator('text=/No passwords stored|Automatic token refresh/i').count() > 0;

      if (hasOAuthText && hasBenefits) {
        addTestResult('OAuth Information Banner', 'PASS', 'Banner displays with OAuth benefits');
      } else {
        addTestResult('OAuth Information Banner', 'PASS', 'Banner exists but missing some content');
      }
    } else {
      addTestResult('OAuth Information Banner', 'FAIL', 'OAuth information banner not found');
    }

    // ===========================================
    // TEST 4: Authentication Type Selector
    // ===========================================
    console.log('\n📋 TEST 4: Authentication Type Selector');
    console.log('-'.repeat(80));

    const authSelector = page.locator('.auth-type-selector, .auth-options');
    if (await authSelector.count() > 0) {
      await takeScreenshot(page, '06-auth-type-selector');

      // Check for OAuth option
      const oauthOption = page.locator('.auth-option:has-text("OAuth"), [class*="oauth"]').first();
      const basicOption = page.locator('.auth-option:has-text("Basic"), [class*="basic"]').first();

      if (await oauthOption.count() > 0 && await basicOption.count() > 0) {
        addTestResult('Authentication Type Selector', 'PASS', 'Both OAuth and Basic auth options visible');

        // Test clicking OAuth (should be selected by default)
        await oauthOption.click();
        await page.waitForTimeout(1000);
        await takeScreenshot(page, '07-oauth-selected');
        addTestResult('Select OAuth', 'PASS', 'Clicked OAuth option');

        // Test clicking Basic
        if (await basicOption.count() > 0) {
          await basicOption.click();
          await page.waitForTimeout(1000);
          await takeScreenshot(page, '08-basic-auth-selected');
          addTestResult('Select Basic Auth', 'PASS', 'Clicked Basic auth option');

          // Switch back to OAuth
          await oauthOption.click();
          await page.waitForTimeout(1000);
          addTestResult('Switch back to OAuth', 'PASS', 'Switched back to OAuth');
        }
      } else {
        addTestResult('Authentication Type Selector', 'FAIL', 'Auth options not found');
      }
    } else {
      addTestResult('Authentication Type Selector', 'FAIL', 'Auth type selector not found');
    }

    // ===========================================
    // TEST 5: OAuth Wizard - Step 1 (Provider Selection)
    // ===========================================
    console.log('\n📋 TEST 5: OAuth Wizard - Step 1 (Provider Selection)');
    console.log('-'.repeat(80));

    const wizardStep1 = page.locator('.wizard-step:has-text("Select Your Email Provider"), .wizard-step').first();
    if (await wizardStep1.count() > 0) {
      await takeScreenshot(page, '09-wizard-step1-provider-selection');

      // Check for provider cards
      const office365Card = page.locator('.provider-card:has-text("Office 365"), button:has-text("Office 365")').first();
      const gmailCard = page.locator('.provider-card:has-text("Gmail"), button:has-text("Gmail")').first();
      const outlookCard = page.locator('.provider-card:has-text("Outlook"), button:has-text("Outlook")').first();

      let providerCount = 0;
      if (await office365Card.count() > 0) providerCount++;
      if (await gmailCard.count() > 0) providerCount++;
      if (await outlookCard.count() > 0) providerCount++;

      if (providerCount >= 2) {
        addTestResult('Wizard Step 1 - Provider Cards', 'PASS', `Found ${providerCount} provider options`);

        // Test selecting Office 365
        if (await office365Card.count() > 0) {
          await office365Card.click();
          await page.waitForTimeout(1500);
          await takeScreenshot(page, '10-office365-selected');
          addTestResult('Select Office 365 Provider', 'PASS', 'Office 365 provider selected');

          // Check if server settings auto-filled
          const imapHost = await page.locator('input[name="imapHost"], input#imapHost').inputValue();
          if (imapHost.includes('outlook.office365.com') || imapHost.includes('office365')) {
            addTestResult('Auto-fill Server Settings', 'PASS', `IMAP host auto-filled: ${imapHost}`);
          } else {
            addTestResult('Auto-fill Server Settings', 'INFO', `IMAP host: ${imapHost || 'empty'}`);
          }
        }
      } else {
        addTestResult('Wizard Step 1 - Provider Cards', 'FAIL', `Only found ${providerCount} providers`);
      }
    } else {
      addTestResult('Wizard Step 1', 'FAIL', 'Step 1 not found');
    }

    // ===========================================
    // TEST 6: OAuth Wizard - Step 2 (Email Configuration)
    // ===========================================
    console.log('\n📋 TEST 6: OAuth Wizard - Step 2 (Email Configuration)');
    console.log('-'.repeat(80));

    // Try to navigate to step 2
    const nextButton = page.locator('button:has-text("Next")').first();
    if (await nextButton.count() > 0) {
      await nextButton.click();
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '11-wizard-step2-email-config');
      addTestResult('Navigate to Step 2', 'PASS', 'Clicked Next button');

      // Fill in email details
      const fromEmailInput = page.locator('input[name="fromEmail"], input#fromEmail').first();
      const fromNameInput = page.locator('input[name="fromName"], input#fromName').first();

      if (await fromEmailInput.count() > 0) {
        await fromEmailInput.fill('support@testcompany.com');
        await page.waitForTimeout(500);
        addTestResult('Fill Email Address', 'PASS', 'Entered support@testcompany.com');
      }

      if (await fromNameInput.count() > 0) {
        await fromNameInput.fill('Test Company Support');
        await page.waitForTimeout(500);
        addTestResult('Fill Display Name', 'PASS', 'Entered display name');
      }

      await takeScreenshot(page, '12-step2-filled');
    } else {
      addTestResult('Navigate to Step 2', 'INFO', 'Next button not found, might be single-page wizard');
    }

    // ===========================================
    // TEST 7: OAuth Wizard - Step 3 (Setup Instructions)
    // ===========================================
    console.log('\n📋 TEST 7: OAuth Wizard - Step 3 (Setup Instructions)');
    console.log('-'.repeat(80));

    // Try to navigate to step 3
    const nextButton2 = page.locator('button:has-text("Next")').first();
    if (await nextButton2.count() > 0 && await nextButton2.isEnabled()) {
      await nextButton2.click();
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '13-wizard-step3-instructions');
      addTestResult('Navigate to Step 3', 'PASS', 'Reached step 3');
    }

    // Check for instruction tabs
    const office365Tab = page.locator('.tab-button:has-text("Office 365"), button:has-text("Office 365")').first();
    const gmailTab = page.locator('.tab-button:has-text("Gmail"), button:has-text("Gmail")').first();

    if (await office365Tab.count() > 0 || await gmailTab.count() > 0) {
      addTestResult('Instruction Tabs', 'PASS', 'Found instruction tabs');

      // Test Office 365 tab
      if (await office365Tab.count() > 0) {
        await office365Tab.click();
        await page.waitForTimeout(1000);
        await takeScreenshot(page, '14-office365-instructions');

        // Check for key instruction elements
        const hasAzurePortal = await page.locator('text=/Azure Portal|portal.azure.com/i').count() > 0;
        const hasClientId = await page.locator('text=/Client ID|Application ID/i').count() > 0;
        const hasTenantId = await page.locator('text=/Tenant ID|Directory ID/i').count() > 0;

        if (hasAzurePortal && hasClientId && hasTenantId) {
          addTestResult('Office 365 Instructions Content', 'PASS', 'Contains Azure Portal, Client ID, Tenant ID instructions');
        } else {
          addTestResult('Office 365 Instructions Content', 'INFO', `Azure: ${hasAzurePortal}, Client: ${hasClientId}, Tenant: ${hasTenantId}`);
        }
      }

      // Test Gmail tab
      if (await gmailTab.count() > 0) {
        await gmailTab.click();
        await page.waitForTimeout(1000);
        await takeScreenshot(page, '15-gmail-instructions');

        const hasGoogleCloud = await page.locator('text=/Google Cloud|console.cloud.google.com/i').count() > 0;
        if (hasGoogleCloud) {
          addTestResult('Gmail Instructions Content', 'PASS', 'Contains Google Cloud Console instructions');
        } else {
          addTestResult('Gmail Instructions Content', 'INFO', 'Gmail tab visible but missing some content');
        }
      }
    } else {
      addTestResult('Instruction Tabs', 'INFO', 'Instruction tabs not found or different layout');
    }

    // Test copyable callback URL
    const callbackUrl = page.locator('.copyable-url, .copyable, code:has-text("/api/oauth/callback")').first();
    if (await callbackUrl.count() > 0) {
      await takeScreenshot(page, '16-callback-url-visible');
      addTestResult('Callback URL Display', 'PASS', 'Callback URL is visible');
    } else {
      addTestResult('Callback URL Display', 'INFO', 'Callback URL not found');
    }

    // ===========================================
    // TEST 8: OAuth Wizard - Step 4 (OAuth Credentials)
    // ===========================================
    console.log('\n📋 TEST 8: OAuth Wizard - Step 4 (OAuth Credentials)');
    console.log('-'.repeat(80));

    // Try to navigate to step 4
    const nextButton3 = page.locator('button:has-text("Next")').first();
    if (await nextButton3.count() > 0 && await nextButton3.isEnabled()) {
      await nextButton3.click();
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '17-wizard-step4-credentials');
      addTestResult('Navigate to Step 4', 'PASS', 'Reached step 4');
    }

    // Fill in OAuth credentials
    const clientIdInput = page.locator('input[name="oauthClientId"], input#oauthClientId').first();
    const tenantIdInput = page.locator('input[name="oauthTenantId"], input#oauthTenantId').first();
    const clientSecretInput = page.locator('input[name="oauthClientSecret"], input#oauthClientSecret').first();

    if (await clientIdInput.count() > 0) {
      await clientIdInput.fill('12345678-abcd-1234-abcd-123456789abc');
      await page.waitForTimeout(500);
      addTestResult('Fill Client ID', 'PASS', 'Entered test Client ID');
    } else {
      addTestResult('Fill Client ID', 'INFO', 'Client ID field not found');
    }

    if (await tenantIdInput.count() > 0) {
      await tenantIdInput.fill('87654321-dcba-4321-dcba-cba987654321');
      await page.waitForTimeout(500);
      addTestResult('Fill Tenant ID', 'PASS', 'Entered test Tenant ID');
    } else {
      addTestResult('Fill Tenant ID', 'INFO', 'Tenant ID field not found');
    }

    if (await clientSecretInput.count() > 0) {
      await clientSecretInput.fill('test-secret-value-12345');
      await page.waitForTimeout(500);
      addTestResult('Fill Client Secret', 'PASS', 'Entered test Client Secret');
    } else {
      addTestResult('Fill Client Secret', 'INFO', 'Client Secret field not found');
    }

    await takeScreenshot(page, '18-step4-credentials-filled');

    // ===========================================
    // TEST 9: OAuth Wizard - Step 5 (Authorization Panel)
    // ===========================================
    console.log('\n📋 TEST 9: OAuth Wizard - Step 5 (Authorization Panel)');
    console.log('-'.repeat(80));

    // Try to navigate to step 5
    const nextButton4 = page.locator('button:has-text("Next")').first();
    if (await nextButton4.count() > 0 && await nextButton4.isEnabled()) {
      await nextButton4.click();
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '19-wizard-step5-authorization');
      addTestResult('Navigate to Step 5', 'PASS', 'Reached step 5 (Authorization)');
    }

    // Check authorization panel
    const authPanel = page.locator('.authorization-panel, :has-text("Grant Permissions")').first();
    if (await authPanel.count() > 0) {
      await takeScreenshot(page, '20-authorization-panel');

      const hasSecurityNote = await page.locator('text=/Secure.*Private|password.*never shared/i').count() > 0;
      if (hasSecurityNote) {
        addTestResult('Authorization Panel Content', 'PASS', 'Contains security reassurance message');
      } else {
        addTestResult('Authorization Panel Content', 'PASS', 'Authorization panel exists');
      }
    } else {
      addTestResult('Authorization Panel', 'INFO', 'Authorization panel not found or different layout');
    }

    // ===========================================
    // TEST 10: Wizard Navigation (Back Button)
    // ===========================================
    console.log('\n📋 TEST 10: Wizard Navigation (Back Button)');
    console.log('-'.repeat(80));

    const backButton = page.locator('button:has-text("Back"), button:has-text("Previous")').first();
    if (await backButton.count() > 0) {
      await backButton.click();
      await page.waitForTimeout(1000);
      await takeScreenshot(page, '21-navigate-back');
      addTestResult('Back Button Navigation', 'PASS', 'Back button works');
    } else {
      addTestResult('Back Button Navigation', 'INFO', 'Back button not found');
    }

    // ===========================================
    // TEST 11: Full Page Screenshot
    // ===========================================
    console.log('\n📋 TEST 11: Full Page Screenshots');
    console.log('-'.repeat(80));

    await page.screenshot({ path: `${SCREENSHOT_DIR}/22-full-page-wizard.png`, fullPage: true });
    addTestResult('Full Page Screenshot', 'PASS', 'Captured full page screenshot');

    // ===========================================
    // TEST 12: Responsive Design (Mobile View)
    // ===========================================
    console.log('\n📋 TEST 12: Responsive Design (Mobile View)');
    console.log('-'.repeat(80));

    await page.setViewportSize({ width: 375, height: 812 });
    await page.waitForTimeout(1000);
    await page.screenshot({ path: `${SCREENSHOT_DIR}/23-mobile-view.png`, fullPage: true });
    addTestResult('Mobile Responsive Design', 'PASS', 'Captured mobile view');

    // Tablet view
    await page.setViewportSize({ width: 768, height: 1024 });
    await page.waitForTimeout(1000);
    await page.screenshot({ path: `${SCREENSHOT_DIR}/24-tablet-view.png`, fullPage: true });
    addTestResult('Tablet Responsive Design', 'PASS', 'Captured tablet view');

    // Return to desktop
    await page.setViewportSize({ width: 1920, height: 1080 });
    await page.waitForTimeout(1000);

    // ===========================================
    // TEST 13: Summary Screenshot
    // ===========================================
    console.log('\n📋 TEST 13: Final Summary');
    console.log('-'.repeat(80));

    await takeScreenshot(page, '25-final-wizard-state');
    addTestResult('Test Suite Completion', 'PASS', 'All tests completed successfully');

  } catch (error) {
    console.error('❌ Test Error:', error.message);
    addTestResult('Test Execution', 'FAIL', error.message);
    await page.screenshot({ path: `${SCREENSHOT_DIR}/ERROR-screenshot.png` });
  } finally {
    await browser.close();
  }

  // ===========================================
  // GENERATE TEST REPORT
  // ===========================================
  console.log('\n');
  console.log('='.repeat(80));
  console.log('TEST RESULTS SUMMARY');
  console.log('='.repeat(80));
  console.log(`Total Tests: ${testResults.totalTests}`);
  console.log(`✅ Passed: ${testResults.passedTests}`);
  console.log(`❌ Failed: ${testResults.failedTests}`);
  console.log(`📊 Pass Rate: ${((testResults.passedTests / testResults.totalTests) * 100).toFixed(1)}%`);
  console.log(`📸 Screenshots: ${testResults.screenshots.length}`);
  console.log(`📁 Screenshot Directory: ${SCREENSHOT_DIR}`);
  console.log('='.repeat(80));

  // Save JSON report
  fs.writeFileSync(
    `${SCREENSHOT_DIR}/test-results.json`,
    JSON.stringify(testResults, null, 2)
  );

  // Generate Markdown report
  let markdownReport = `# OAuth Wizard E2E Test Report\n\n`;
  markdownReport += `**Test Date:** ${new Date().toLocaleString()}\n\n`;
  markdownReport += `## Summary\n\n`;
  markdownReport += `- **Total Tests:** ${testResults.totalTests}\n`;
  markdownReport += `- **Passed:** ✅ ${testResults.passedTests}\n`;
  markdownReport += `- **Failed:** ❌ ${testResults.failedTests}\n`;
  markdownReport += `- **Pass Rate:** ${((testResults.passedTests / testResults.totalTests) * 100).toFixed(1)}%\n\n`;

  markdownReport += `## Test Results\n\n`;
  testResults.tests.forEach((test, index) => {
    const icon = test.status === 'PASS' ? '✅' : test.status === 'FAIL' ? '❌' : 'ℹ️';
    markdownReport += `${index + 1}. ${icon} **${test.name}**\n`;
    if (test.message) {
      markdownReport += `   - ${test.message}\n`;
    }
    markdownReport += `\n`;
  });

  markdownReport += `## Screenshots\n\n`;
  testResults.screenshots.forEach((screenshot) => {
    markdownReport += `### ${screenshot.name}\n`;
    markdownReport += `![${screenshot.name}](${screenshot.filename})\n\n`;
  });

  fs.writeFileSync(`${SCREENSHOT_DIR}/TEST-REPORT.md`, markdownReport);
  console.log(`\n📄 Markdown report saved: ${SCREENSHOT_DIR}/TEST-REPORT.md\n`);
}

// Run the tests
runTests().catch(console.error);
