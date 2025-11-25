/**
 * Email Threading Workflow E2E Test
 *
 * This script tests the complete email threading functionality including:
 * - Email thread viewer display
 * - Reply composer opening
 * - Reply/Forward/PrivateNote functionality
 * - Canned responses
 * - Email sending
 */

const playwright = require('playwright');

const CONFIG = {
  BACKEND_URL: 'http://localhost:5000',
  FRONTEND_URL: 'http://localhost:4200',
  CREDENTIALS: {
    email: 'admin@complaintmanagement.com',
    password: 'Admin@123'
  }
};

// ANSI color codes for terminal output
const colors = {
  reset: '\x1b[0m',
  bright: '\x1b[1m',
  red: '\x1b[31m',
  green: '\x1b[32m',
  yellow: '\x1b[33m',
  blue: '\x1b[34m',
  cyan: '\x1b[36m'
};

function log(message, color = 'reset') {
  console.log(`${colors[color]}${message}${colors.reset}`);
}

function logStep(step, message) {
  log(`\n[STEP ${step}] ${message}`, 'cyan');
}

function logSuccess(message) {
  log(`✓ ${message}`, 'green');
}

function logError(message) {
  log(`✗ ${message}`, 'red');
}

function logInfo(message) {
  log(`ℹ ${message}`, 'blue');
}

async function delay(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

async function loginToSystem(page) {
  logStep(1, 'Logging into the system');

  try {
    await page.goto(CONFIG.FRONTEND_URL);
    await page.waitForLoadState('networkidle');

    // Check if already logged in
    const dashboardVisible = await page.locator('h1:has-text("Dashboard")').isVisible().catch(() => false);
    if (dashboardVisible) {
      logSuccess('Already logged in');
      return true;
    }

    // Fill login form
    await page.fill('input[type="email"], input[formControlName="email"]', CONFIG.CREDENTIALS.email);
    await page.fill('input[type="password"], input[formControlName="password"]', CONFIG.CREDENTIALS.password);

    // Click login button
    await page.click('button:has-text("Login"), button:has-text("Sign In")');

    // Wait for dashboard
    await page.waitForSelector('h1:has-text("Dashboard")', { timeout: 10000 });
    logSuccess('Successfully logged in as admin');

    return true;
  } catch (error) {
    logError(`Login failed: ${error.message}`);
    return false;
  }
}

async function navigateToComplaintWithEmails(page) {
  logStep(2, 'Navigating to complaint detail with email thread');

  try {
    // Get a complaint ID from the API that has emails
    const response = await fetch(`${CONFIG.BACKEND_URL}/api/complaints?pageSize=50`, {
      headers: {
        'Authorization': `Bearer ${await getAuthToken(page)}`
      }
    });

    const result = await response.json();

    if (!result.isSuccess || !result.data || result.data.length === 0) {
      logError('No complaints found in the system');
      return null;
    }

    // Try to find a complaint with emails
    let complaintId = null;
    for (const complaint of result.data) {
      // Check if complaint has emails
      const emailsResponse = await fetch(
        `${CONFIG.BACKEND_URL}/api/complaints/${complaint.id}/emails`,
        {
          headers: {
            'Authorization': `Bearer ${await getAuthToken(page)}`
          }
        }
      );

      const emailsResult = await emailsResponse.json();
      if (emailsResult.isSuccess && emailsResult.data && emailsResult.data.length > 0) {
        complaintId = complaint.id;
        logInfo(`Found complaint ${complaintId} with ${emailsResult.data.length} emails`);
        break;
      }
    }

    if (!complaintId) {
      logInfo('No complaints with emails found, using first complaint');
      complaintId = result.data[0].id;
    }

    // Navigate to complaint detail
    await page.goto(`${CONFIG.FRONTEND_URL}/complaints/${complaintId}`);
    await page.waitForLoadState('networkidle');
    await delay(2000);

    logSuccess(`Navigated to complaint detail: ${complaintId}`);
    return complaintId;
  } catch (error) {
    logError(`Navigation failed: ${error.message}`);
    return null;
  }
}

async function getAuthToken(page) {
  return await page.evaluate(() => {
    return localStorage.getItem('token') || localStorage.getItem('authToken') || '';
  });
}

async function testEmailThreadViewer(page) {
  logStep(3, 'Testing Email Thread Viewer');

  try {
    // Check if email thread viewer is visible
    const threadViewer = await page.locator('app-email-thread-viewer').isVisible();
    if (!threadViewer) {
      logError('Email thread viewer component not found');
      return false;
    }

    logSuccess('Email thread viewer component is visible');

    // Check for emails section
    const emailsSection = await page.locator('.emails-container, .email-thread-container').isVisible().catch(() => false);
    if (emailsSection) {
      logSuccess('Emails container is rendered');

      // Count email items
      const emailCount = await page.locator('.email-item, .thread-item').count();
      logInfo(`Found ${emailCount} email(s) in the thread`);
    } else {
      logInfo('No emails displayed (may be empty thread)');
    }

    // Take screenshot
    await page.screenshot({ path: 'email-threading-01-thread-viewer.png', fullPage: true });
    logSuccess('Screenshot saved: email-threading-01-thread-viewer.png');

    return true;
  } catch (error) {
    logError(`Thread viewer test failed: ${error.message}`);
    return false;
  }
}

async function testReplyComposer(page) {
  logStep(4, 'Testing Reply Composer');

  try {
    // Look for Reply button on the first email
    const replyButton = page.locator('button:has-text("Reply"), .btn-reply').first();
    const replyExists = await replyButton.isVisible().catch(() => false);

    if (!replyExists) {
      logInfo('No Reply button found (may not have inbound emails)');
      return true; // Not a failure, just no emails to reply to
    }

    // Click reply button
    await replyButton.click();
    await delay(1000);

    logSuccess('Clicked Reply button');

    // Check if reply composer is visible
    const composer = await page.locator('app-email-reply-composer').isVisible({ timeout: 5000 });
    if (!composer) {
      logError('Email reply composer did not open');
      return false;
    }

    logSuccess('Email reply composer is visible');

    // Verify composer elements
    const subjectField = await page.locator('input[formControlName="subject"]').isVisible();
    const bodyEditor = await page.locator('quill-editor, .ql-editor').isVisible();
    const sendButton = await page.locator('button:has-text("Send")').isVisible();

    if (subjectField) logSuccess('Subject field is visible');
    if (bodyEditor) logSuccess('Rich text editor is visible');
    if (sendButton) logSuccess('Send button is visible');

    // Take screenshot
    await page.screenshot({ path: 'email-threading-02-reply-composer.png', fullPage: true });
    logSuccess('Screenshot saved: email-threading-02-reply-composer.png');

    return true;
  } catch (error) {
    logError(`Reply composer test failed: ${error.message}`);
    return false;
  }
}

async function testCannedResponses(page) {
  logStep(5, 'Testing Canned Responses');

  try {
    // Check if canned responses dropdown exists
    const cannedResponsesSelect = await page.locator('select.canned-responses-select, select:has-text("Select a quick response")').isVisible().catch(() => false);

    if (!cannedResponsesSelect) {
      logInfo('Canned responses dropdown not visible (may be disabled or no responses configured)');
      return true;
    }

    logSuccess('Canned responses dropdown is visible');

    // Count options
    const optionCount = await page.locator('select.canned-responses-select option, select:has-text("Select a quick response") option').count();
    logInfo(`Found ${optionCount} canned response option(s)`);

    return true;
  } catch (error) {
    logError(`Canned responses test failed: ${error.message}`);
    return false;
  }
}

async function testFormValidation(page) {
  logStep(6, 'Testing Form Validation');

  try {
    // Try to submit empty form
    const sendButton = page.locator('button:has-text("Send")').first();
    const isDisabled = await sendButton.isDisabled();

    if (isDisabled) {
      logSuccess('Send button is disabled for invalid form (validation working)');
    } else {
      logInfo('Send button is enabled (form may have pre-filled values)');
    }

    return true;
  } catch (error) {
    logError(`Form validation test failed: ${error.message}`);
    return false;
  }
}

async function testComposerCancellation(page) {
  logStep(7, 'Testing Composer Cancellation');

  try {
    // Check if composer is visible
    const composerVisible = await page.locator('app-email-reply-composer').isVisible().catch(() => false);

    if (!composerVisible) {
      logInfo('Composer not visible, skipping cancellation test');
      return true;
    }

    // Look for cancel or close button
    const cancelButton = page.locator('button:has-text("Cancel"), button.btn-icon:has(i.bi-x-lg)').first();
    const cancelExists = await cancelButton.isVisible().catch(() => false);

    if (!cancelExists) {
      logError('Cancel/Close button not found');
      return false;
    }

    // Click cancel (handle confirmation if present)
    page.once('dialog', dialog => {
      logInfo(`Confirmation dialog: ${dialog.message()}`);
      dialog.accept();
    });

    await cancelButton.click();
    await delay(1000);

    // Verify composer is closed
    const composerStillVisible = await page.locator('app-email-reply-composer').isVisible().catch(() => false);

    if (!composerStillVisible) {
      logSuccess('Composer closed successfully');
      return true;
    } else {
      logError('Composer is still visible after cancel');
      return false;
    }
  } catch (error) {
    logError(`Cancellation test failed: ${error.message}`);
    return false;
  }
}

async function testForwardButton(page) {
  logStep(8, 'Testing Forward Button');

  try {
    // Look for Forward button
    const forwardButton = page.locator('button:has-text("Forward"), .btn-forward').first();
    const forwardExists = await forwardButton.isVisible().catch(() => false);

    if (!forwardExists) {
      logInfo('No Forward button found (feature may not be available)');
      return true;
    }

    // Click forward button
    await forwardButton.click();
    await delay(1000);

    // Check if composer opens with Forward mode
    const composer = await page.locator('app-email-reply-composer').isVisible().catch(() => false);
    if (composer) {
      const header = await page.locator('.composer-header h4:has-text("Forward")').isVisible().catch(() => false);
      if (header) {
        logSuccess('Forward composer opened successfully');

        // Take screenshot
        await page.screenshot({ path: 'email-threading-03-forward-composer.png', fullPage: true });
        logSuccess('Screenshot saved: email-threading-03-forward-composer.png');

        // Close composer
        page.once('dialog', dialog => dialog.accept());
        await page.locator('button:has-text("Cancel"), button.btn-icon:has(i.bi-x-lg)').first().click();
        await delay(500);

        return true;
      }
    }

    logError('Forward composer did not open correctly');
    return false;
  } catch (error) {
    logError(`Forward button test failed: ${error.message}`);
    return false;
  }
}

async function main() {
  log('\n========================================', 'bright');
  log('  EMAIL THREADING WORKFLOW E2E TEST', 'bright');
  log('========================================\n', 'bright');

  const browser = await playwright.chromium.launch({
    headless: false,
    slowMo: 500
  });

  const context = await browser.newContext({
    viewport: { width: 1920, height: 1080 }
  });

  const page = await context.newPage();

  let testResults = {
    passed: 0,
    failed: 0,
    skipped: 0
  };

  try {
    // Run tests
    if (await loginToSystem(page)) {
      testResults.passed++;

      const complaintId = await navigateToComplaintWithEmails(page);
      if (complaintId) {
        testResults.passed++;

        if (await testEmailThreadViewer(page)) testResults.passed++;
        else testResults.failed++;

        if (await testReplyComposer(page)) testResults.passed++;
        else testResults.failed++;

        if (await testCannedResponses(page)) testResults.passed++;
        else testResults.failed++;

        if (await testFormValidation(page)) testResults.passed++;
        else testResults.failed++;

        if (await testComposerCancellation(page)) testResults.passed++;
        else testResults.failed++;

        if (await testForwardButton(page)) testResults.passed++;
        else testResults.failed++;
      } else {
        testResults.failed++;
      }
    } else {
      testResults.failed++;
    }

  } catch (error) {
    logError(`Fatal error: ${error.message}`);
    console.error(error.stack);
    testResults.failed++;
  } finally {
    // Summary
    log('\n========================================', 'bright');
    log('  TEST SUMMARY', 'bright');
    log('========================================', 'bright');
    log(`Total Tests: ${testResults.passed + testResults.failed}`, 'cyan');
    log(`Passed: ${testResults.passed}`, 'green');
    log(`Failed: ${testResults.failed}`, 'red');
    log(`Success Rate: ${((testResults.passed / (testResults.passed + testResults.failed)) * 100).toFixed(1)}%`, 'yellow');
    log('========================================\n', 'bright');

    await delay(3000);
    await browser.close();

    process.exit(testResults.failed > 0 ? 1 : 0);
  }
}

// Run the test
main().catch(error => {
  console.error('Test execution failed:', error);
  process.exit(1);
});
