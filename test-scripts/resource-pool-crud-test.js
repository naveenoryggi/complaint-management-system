/**
 * Resource Pool Management CRUD Testing Script
 * Tests all CRUD operations on the Resource Pool Management page
 *
 * Prerequisites:
 * - Angular app running on http://localhost:4200
 * - Backend API running on port 5000
 * - Test credentials: admin@company.com / Admin123!
 */

const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

// Configuration
const BASE_URL = 'http://localhost:4200';
const API_URL = 'http://localhost:5000';
const LOGIN_EMAIL = 'admin@complaintmanagement.com';
const LOGIN_PASSWORD = 'Admin@123';
const SCREENSHOT_DIR = path.join(__dirname, '..', 'test-screenshots', 'resource-pool-crud', new Date().getTime().toString());

// Test data
const TEST_POOL_DATA = {
  name: 'QA Test Pool',
  description: 'Automated test pool created by QA script',
  poolType: 'Custom'
};

const UPDATED_POOL_DATA = {
  name: 'QA Test Pool - Updated',
  description: 'Updated description for automated test pool'
};

// Ensure screenshot directory exists
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

let testResults = {
  passed: [],
  failed: [],
  warnings: [],
  screenshots: []
};

function log(message, type = 'info') {
  const timestamp = new Date().toISOString();
  const prefix = {
    info: '[INFO]',
    success: '[PASS]',
    error: '[FAIL]',
    warning: '[WARN]'
  }[type] || '[INFO]';

  console.log(`${timestamp} ${prefix} ${message}`);
}

async function takeScreenshot(page, name, description = '') {
  const timestamp = Date.now();
  const filename = `${timestamp}-${name}.png`;
  const filepath = path.join(SCREENSHOT_DIR, filename);

  await page.screenshot({ path: filepath, fullPage: true });
  log(`Screenshot saved: ${filename}`, 'info');

  testResults.screenshots.push({
    filename,
    filepath,
    description,
    timestamp: new Date(timestamp).toISOString()
  });

  return filepath;
}

async function waitForNetworkIdle(page, timeout = 3000) {
  await page.waitForLoadState('networkidle', { timeout });
}

async function testLogin(page) {
  log('Starting login test...', 'info');

  try {
    // Navigate to login page
    await page.goto(`${BASE_URL}/login`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '01-login-page', 'Login page loaded');

    // Wait for the form to be visible
    await page.waitForSelector('#email', { state: 'visible', timeout: 10000 });
    await page.waitForSelector('#password', { state: 'visible', timeout: 10000 });

    // Fill in credentials using specific IDs
    await page.fill('#email', LOGIN_EMAIL);
    await page.fill('#password', LOGIN_PASSWORD);
    await page.waitForTimeout(500);
    await takeScreenshot(page, '02-login-credentials-filled', 'Login credentials entered');

    // Click login button
    await page.click('button[type="submit"]');
    log('Login button clicked', 'info');

    // Wait for navigation to dashboard or any page after login
    await page.waitForURL(/dashboard|admin|complaint|resource-pool/i, { timeout: 10000 });
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '03-after-login', 'Successfully logged in');

    testResults.passed.push('Login successful');
    log('Login test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`Login failed: ${error.message}`);
    log(`Login test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-login', 'Login failed');
    throw error;
  }
}

async function navigateToResourcePools(page) {
  log('Navigating to Resource Pools page...', 'info');

  try {
    // Try direct navigation first
    await page.goto(`${BASE_URL}/admin/resource-pools`, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '04-resource-pool-page-initial', 'Resource Pool page loaded');

    // Verify we're on the right page
    const pageTitle = await page.textContent('h1, .page-header h1');
    if (!pageTitle.toLowerCase().includes('resource pool')) {
      throw new Error('Not on Resource Pool Management page');
    }

    testResults.passed.push('Navigation to Resource Pools page successful');
    log('Navigation test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`Navigation failed: ${error.message}`);
    log(`Navigation test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-navigation', 'Navigation failed');
    throw error;
  }
}

async function testPageLoad(page) {
  log('Testing page load and initial state...', 'info');

  try {
    // Check for key elements
    const headerExists = await page.isVisible('h1:has-text("Resource Pool")');
    if (!headerExists) {
      throw new Error('Page header not found');
    }

    // Check for Add Resource Pool button
    const addButtonExists = await page.isVisible('button:has-text("Add Resource Pool")');
    if (!addButtonExists) {
      throw new Error('Add Resource Pool button not found');
    }

    // Check if pools are displayed (or empty state)
    const hasPoolCards = await page.isVisible('.pool-card');
    const hasEmptyState = await page.isVisible('.no-results');

    if (hasPoolCards) {
      const poolCount = await page.locator('.pool-card').count();
      log(`Found ${poolCount} resource pool(s) on page`, 'info');
      testResults.passed.push(`Page loaded with ${poolCount} pool(s)`);
    } else if (hasEmptyState) {
      log('No resource pools found - empty state displayed', 'info');
      testResults.passed.push('Page loaded with empty state');
    } else {
      testResults.warnings.push('Could not determine page state - neither pools nor empty state found');
    }

    await takeScreenshot(page, '05-page-load-verification', 'Page load verified');

    testResults.passed.push('Page load verification successful');
    log('Page load test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`Page load verification failed: ${error.message}`);
    log(`Page load test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-page-load', 'Page load verification failed');
    throw error;
  }
}

async function testCreatePool(page) {
  log('Testing CREATE operation - Adding new resource pool...', 'info');

  try {
    // Click Add Resource Pool button
    await page.click('button:has-text("Add Resource Pool")');
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '06-create-modal-opened', 'Create modal opened');

    // Verify modal is visible
    const modalVisible = await page.isVisible('.modal-overlay');
    if (!modalVisible) {
      throw new Error('Create modal did not open');
    }

    const modalTitle = await page.textContent('.modal-header h2');
    if (!modalTitle.toLowerCase().includes('create')) {
      throw new Error('Modal title does not indicate creation mode');
    }

    testResults.passed.push('Create modal opened successfully');
    log('Modal opened - filling form...', 'info');

    // Fill in the form
    await page.fill('#poolName, input[name="name"]', TEST_POOL_DATA.name);
    await page.fill('#poolDescription, textarea[name="description"]', TEST_POOL_DATA.description);

    // Select pool type
    await page.selectOption('#poolType, select[name="poolType"]', { label: TEST_POOL_DATA.poolType });

    await page.waitForTimeout(500);
    await takeScreenshot(page, '07-create-form-filled', 'Create form filled with test data');

    testResults.passed.push('Create form filled successfully');
    log('Form filled - submitting...', 'info');

    // Submit the form
    await page.click('button:has-text("Create")');
    await page.waitForTimeout(2000);

    // Wait for modal to close and success message
    await page.waitForSelector('.modal-overlay', { state: 'hidden', timeout: 5000 });
    await takeScreenshot(page, '08-after-create-submit', 'After create submission');

    // Check for success message
    const successVisible = await page.isVisible('.alert-success');
    if (successVisible) {
      const successText = await page.textContent('.alert-success');
      log(`Success message: ${successText}`, 'success');
      testResults.passed.push('Success message displayed after creation');
    } else {
      testResults.warnings.push('No success message displayed after creation');
    }

    // Verify the new pool appears in the list
    await page.waitForTimeout(1000);
    const poolCardWithName = await page.locator(`.pool-card:has-text("${TEST_POOL_DATA.name}")`);
    const newPoolExists = await poolCardWithName.count() > 0;

    if (newPoolExists) {
      testResults.passed.push(`New pool "${TEST_POOL_DATA.name}" appears in the list`);
      await takeScreenshot(page, '09-new-pool-in-list', 'New pool visible in list');
    } else {
      throw new Error('Newly created pool not found in the list');
    }

    testResults.passed.push('CREATE operation successful');
    log('CREATE test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`CREATE operation failed: ${error.message}`);
    log(`CREATE test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-create', 'Create operation failed');
    throw error;
  }
}

async function testEditPool(page) {
  log('Testing UPDATE operation - Editing resource pool...', 'info');

  try {
    // Find the test pool we created
    const poolCard = page.locator(`.pool-card:has-text("${TEST_POOL_DATA.name}")`).first();

    // Click Edit button
    await poolCard.locator('button:has-text("Edit")').click();
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '10-edit-modal-opened', 'Edit modal opened');

    // Verify modal is visible
    const modalVisible = await page.isVisible('.modal-overlay');
    if (!modalVisible) {
      throw new Error('Edit modal did not open');
    }

    const modalTitle = await page.textContent('.modal-header h2');
    if (!modalTitle.toLowerCase().includes('edit')) {
      throw new Error('Modal title does not indicate edit mode');
    }

    // Verify form is pre-filled
    const currentName = await page.inputValue('#poolName, input[name="name"]');
    if (currentName !== TEST_POOL_DATA.name) {
      throw new Error('Form not pre-filled with current pool data');
    }

    testResults.passed.push('Edit modal opened with pre-filled data');
    log('Edit modal opened - updating data...', 'info');

    // Update the form
    await page.fill('#poolName, input[name="name"]', UPDATED_POOL_DATA.name);
    await page.fill('#poolDescription, textarea[name="description"]', UPDATED_POOL_DATA.description);

    await page.waitForTimeout(500);
    await takeScreenshot(page, '11-edit-form-updated', 'Edit form updated');

    // Submit the form
    await page.click('button:has-text("Update")');
    await page.waitForTimeout(2000);

    // Wait for modal to close
    await page.waitForSelector('.modal-overlay', { state: 'hidden', timeout: 5000 });
    await takeScreenshot(page, '12-after-edit-submit', 'After edit submission');

    // Check for success message
    const successVisible = await page.isVisible('.alert-success');
    if (successVisible) {
      const successText = await page.textContent('.alert-success');
      log(`Success message: ${successText}`, 'success');
      testResults.passed.push('Success message displayed after update');
    }

    // Verify the updated pool appears with new name
    await page.waitForTimeout(1000);
    const updatedPoolExists = await page.locator(`.pool-card:has-text("${UPDATED_POOL_DATA.name}")`).count() > 0;

    if (updatedPoolExists) {
      testResults.passed.push(`Pool updated to "${UPDATED_POOL_DATA.name}"`);
      await takeScreenshot(page, '13-updated-pool-in-list', 'Updated pool visible in list');
    } else {
      throw new Error('Updated pool not found in the list');
    }

    testResults.passed.push('UPDATE operation successful');
    log('UPDATE test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`UPDATE operation failed: ${error.message}`);
    log(`UPDATE test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-edit', 'Edit operation failed');
    throw error;
  }
}

async function testViewMembers(page) {
  log('Testing View Members functionality...', 'info');

  try {
    // Find the test pool
    const poolCard = page.locator(`.pool-card:has-text("${UPDATED_POOL_DATA.name}")`).first();

    // Click View Members (either the "View All Members" button or the clickable member count)
    const viewMembersButton = poolCard.locator('button:has-text("View All Members")');
    const memberCountClickable = poolCard.locator('.detail-row.clickable:has-text("Members")');

    const hasViewButton = await viewMembersButton.count() > 0;

    if (hasViewButton) {
      await viewMembersButton.click();
    } else {
      await memberCountClickable.click();
    }

    await page.waitForTimeout(1000);
    await takeScreenshot(page, '14-view-members-modal-opened', 'View Members modal opened');

    // Verify modal is visible
    const modalVisible = await page.isVisible('.modal-overlay');
    if (!modalVisible) {
      throw new Error('View Members modal did not open');
    }

    // Check modal content
    const modalTitleElement = await page.locator('.modal-header h2').first();
    const modalTitleVisible = await modalTitleElement.isVisible();

    if (modalTitleVisible) {
      const modalTitle = await modalTitleElement.textContent();
      log(`View Members modal title: ${modalTitle}`, 'info');
      testResults.passed.push('View Members modal opened successfully');
    }

    // Check if members are displayed or empty state
    const hasMembersTable = await page.isVisible('.members-full-list');
    const hasEmptyState = await page.isVisible('.empty-members');

    if (hasMembersTable) {
      const memberCount = await page.locator('.member-row').count();
      log(`Pool has ${memberCount} member(s)`, 'info');
      testResults.passed.push(`View Members modal shows ${memberCount} member(s)`);
      await takeScreenshot(page, '15-view-members-with-data', 'View Members modal with member data');
    } else if (hasEmptyState) {
      log('Pool has no members - empty state displayed', 'info');
      testResults.passed.push('View Members modal shows empty state correctly');
      await takeScreenshot(page, '15-view-members-empty', 'View Members modal empty state');
    }

    // Close the modal
    await page.click('.modal-container button:has-text("Close")');
    await page.waitForTimeout(500);

    testResults.passed.push('View Members functionality verified');
    log('View Members test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`View Members test failed: ${error.message}`);
    log(`View Members test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-view-members', 'View Members test failed');
    // Don't throw - this is not a critical failure
    return false;
  }
}

async function testDeletePool(page) {
  log('Testing DELETE operation - Deleting resource pool...', 'info');

  try {
    // Find the test pool
    const poolCard = page.locator(`.pool-card:has-text("${UPDATED_POOL_DATA.name}")`).first();

    // Click Delete button
    await poolCard.locator('button:has(.material-icons-round:has-text("delete"))').click();
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '16-delete-confirmation-modal', 'Delete confirmation modal opened');

    // Verify confirmation modal is visible
    const modalVisible = await page.isVisible('.modal-overlay');
    if (!modalVisible) {
      throw new Error('Delete confirmation modal did not open');
    }

    const modalTitle = await page.textContent('.modal-header h2');
    if (!modalTitle.toLowerCase().includes('delete')) {
      throw new Error('Modal title does not indicate deletion');
    }

    // Verify pool name is shown in confirmation
    const confirmationText = await page.textContent('.delete-confirm-content, .modal-body');
    if (!confirmationText.includes(UPDATED_POOL_DATA.name)) {
      testResults.warnings.push('Pool name not visible in delete confirmation');
    }

    testResults.passed.push('Delete confirmation modal opened');
    log('Confirmation modal opened - confirming deletion...', 'info');

    // Confirm deletion
    await page.click('button.btn-danger:has-text("Delete")');
    await page.waitForTimeout(2000);

    // Wait for modal to close
    await page.waitForSelector('.modal-overlay', { state: 'hidden', timeout: 5000 });
    await takeScreenshot(page, '17-after-delete-confirm', 'After delete confirmation');

    // Check for success message
    const successVisible = await page.isVisible('.alert-success');
    if (successVisible) {
      const successText = await page.textContent('.alert-success');
      log(`Success message: ${successText}`, 'success');
      testResults.passed.push('Success message displayed after deletion');
    }

    // Verify the pool is removed from the list
    await page.waitForTimeout(1000);
    const deletedPoolExists = await page.locator(`.pool-card:has-text("${UPDATED_POOL_DATA.name}")`).count() > 0;

    if (!deletedPoolExists) {
      testResults.passed.push('Pool successfully removed from list');
      await takeScreenshot(page, '18-pool-deleted-from-list', 'Pool removed from list');
    } else {
      throw new Error('Pool still appears in the list after deletion');
    }

    testResults.passed.push('DELETE operation successful');
    log('DELETE test PASSED', 'success');
    return true;
  } catch (error) {
    testResults.failed.push(`DELETE operation failed: ${error.message}`);
    log(`DELETE test FAILED: ${error.message}`, 'error');
    await takeScreenshot(page, 'error-delete', 'Delete operation failed');
    throw error;
  }
}

async function checkConsoleLogs(page) {
  // Collect console messages
  const consoleLogs = [];

  page.on('console', msg => {
    const type = msg.type();
    const text = msg.text();
    consoleLogs.push({ type, text, timestamp: new Date().toISOString() });

    if (type === 'error') {
      log(`Browser Console Error: ${text}`, 'error');
      testResults.warnings.push(`Console error: ${text}`);
    } else if (type === 'warning') {
      log(`Browser Console Warning: ${text}`, 'warning');
    }
  });

  return consoleLogs;
}

async function generateTestReport() {
  const reportPath = path.join(SCREENSHOT_DIR, 'test-report.json');
  const reportData = {
    testSuite: 'Resource Pool Management CRUD Tests',
    timestamp: new Date().toISOString(),
    summary: {
      totalPassed: testResults.passed.length,
      totalFailed: testResults.failed.length,
      totalWarnings: testResults.warnings.length,
      totalScreenshots: testResults.screenshots.length
    },
    results: testResults,
    screenshotDirectory: SCREENSHOT_DIR
  };

  fs.writeFileSync(reportPath, JSON.stringify(reportData, null, 2));
  log(`Test report saved: ${reportPath}`, 'info');

  // Generate human-readable report
  const readableReportPath = path.join(SCREENSHOT_DIR, 'test-report.txt');
  let reportText = `
========================================
RESOURCE POOL MANAGEMENT - CRUD TEST REPORT
========================================

Test Date: ${new Date().toISOString()}
Screenshot Directory: ${SCREENSHOT_DIR}

========================================
SUMMARY
========================================
Total Tests Passed: ${testResults.passed.length}
Total Tests Failed: ${testResults.failed.length}
Total Warnings: ${testResults.warnings.length}
Total Screenshots: ${testResults.screenshots.length}

========================================
PASSED TESTS (${testResults.passed.length})
========================================
${testResults.passed.map((t, i) => `${i + 1}. ${t}`).join('\n')}

========================================
FAILED TESTS (${testResults.failed.length})
========================================
${testResults.failed.length > 0 ? testResults.failed.map((t, i) => `${i + 1}. ${t}`).join('\n') : 'None'}

========================================
WARNINGS (${testResults.warnings.length})
========================================
${testResults.warnings.length > 0 ? testResults.warnings.map((t, i) => `${i + 1}. ${t}`).join('\n') : 'None'}

========================================
SCREENSHOTS
========================================
${testResults.screenshots.map((s, i) => `${i + 1}. ${s.filename} - ${s.description}`).join('\n')}

========================================
END OF REPORT
========================================
`;

  fs.writeFileSync(readableReportPath, reportText);
  log(`Human-readable report saved: ${readableReportPath}`, 'info');

  return { reportPath, readableReportPath, reportData };
}

async function runTests() {
  log('========================================', 'info');
  log('RESOURCE POOL MANAGEMENT CRUD TESTS', 'info');
  log('========================================', 'info');

  const browser = await chromium.launch({
    headless: false, // Set to true for headless mode
    slowMo: 100 // Slow down operations for visibility
  });

  const context = await browser.newContext({
    viewport: { width: 1920, height: 1080 },
    recordVideo: {
      dir: path.join(SCREENSHOT_DIR, 'videos'),
      size: { width: 1920, height: 1080 }
    }
  });

  const page = await context.newPage();

  // Set up console log monitoring
  await checkConsoleLogs(page);

  try {
    // Run test sequence
    await testLogin(page);
    await navigateToResourcePools(page);
    await testPageLoad(page);
    await testCreatePool(page);
    await testEditPool(page);
    await testViewMembers(page);
    await testDeletePool(page);

    log('========================================', 'info');
    log('ALL TESTS COMPLETED SUCCESSFULLY!', 'success');
    log('========================================', 'info');
  } catch (error) {
    log('========================================', 'error');
    log('TEST EXECUTION FAILED', 'error');
    log(`Error: ${error.message}`, 'error');
    log('========================================', 'error');
  } finally {
    // Generate test report
    const { reportPath, readableReportPath, reportData } = await generateTestReport();

    // Print summary
    console.log('\n========================================');
    console.log('TEST SUMMARY');
    console.log('========================================');
    console.log(`Passed: ${reportData.summary.totalPassed}`);
    console.log(`Failed: ${reportData.summary.totalFailed}`);
    console.log(`Warnings: ${reportData.summary.totalWarnings}`);
    console.log(`Screenshots: ${reportData.summary.totalScreenshots}`);
    console.log(`Report: ${readableReportPath}`);
    console.log('========================================\n');

    // Close browser
    await context.close();
    await browser.close();

    // Exit with appropriate code
    process.exit(testResults.failed.length > 0 ? 1 : 0);
  }
}

// Run the tests
runTests().catch(error => {
  console.error('Fatal error:', error);
  process.exit(1);
});
