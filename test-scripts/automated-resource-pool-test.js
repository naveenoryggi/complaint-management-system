/**
 * Automated Resource Pool CRUD Testing
 * Tests all CRUD operations using existing pools
 */

const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const BASE_URL = 'http://localhost:4200';
const LOGIN_EMAIL = 'admin@complaintmanagement.com';
const LOGIN_PASSWORD = 'Admin@123';
const SCREENSHOT_DIR = path.join(__dirname, '..', 'test-screenshots', 'resource-pool-automated', Date.now().toString());

if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

const testResults = {
  passed: [],
  failed: [],
  warnings: [],
  screenshots: []
};

function log(message, type = 'info') {
  const prefix = { info: '[INFO]', success: '[PASS]', error: '[FAIL]', warning: '[WARN]' }[type];
  console.log(`${new Date().toISOString()} ${prefix} ${message}`);
}

async function takeScreenshot(page, name, description) {
  const filename = `${Date.now()}-${name}.png`;
  const filepath = path.join(SCREENSHOT_DIR, filename);
  await page.screenshot({ path: filepath, fullPage: true });
  log(`Screenshot: ${filename}`, 'info');
  testResults.screenshots.push({ filename, filepath, description });
  return filepath;
}

async function runTests() {
  log('='.repeat(60), 'info');
  log('RESOURCE POOL CRUD - AUTOMATED TEST SUITE', 'info');
  log('='.repeat(60), 'info');

  const browser = await chromium.launch({ headless: false, slowMo: 300 });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  // Collect console errors
  page.on('console', msg => {
    if (msg.type() === 'error' && !msg.text().includes('SignalR') && !msg.text().includes('notification')) {
      testResults.warnings.push(`Console error: ${msg.text()}`);
    }
  });

  try {
    // === LOGIN ===
    log('TEST 1: Login', 'info');
    await page.goto(`${BASE_URL}/login`);
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '01-login', 'Login page');

    await page.waitForSelector('#email');
    await page.fill('#email', LOGIN_EMAIL);
    await page.fill('#password', LOGIN_PASSWORD);
    await takeScreenshot(page, '02-login-filled', 'Credentials entered');

    await page.click('button[type="submit"]');
    await page.waitForURL(/dashboard|admin/, { timeout: 15000 });
    await takeScreenshot(page, '03-logged-in', 'Logged in');

    testResults.passed.push('Login successful');
    log('Login test PASSED', 'success');

    // === NAVIGATE TO RESOURCE POOLS ===
    log('TEST 2: Navigate to Resource Pools', 'info');
    await page.goto(`${BASE_URL}/admin/resource-pools`);
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '04-resource-pools-page', 'Resource Pools page');

    const poolCount = await page.locator('.pool-card').count();
    log(`Found ${poolCount} resource pools`, 'info');
    testResults.passed.push(`Page loaded with ${poolCount} pools`);

    if (poolCount === 0) {
      testResults.warnings.push('No pools found - cannot test edit/delete/view members');
      log('No pools found', 'warning');
    } else {
      log('Navigation test PASSED', 'success');

      // === TEST VIEW MEMBERS (READ) ===
      log('TEST 3: View Members (READ operation)', 'info');
      try {
        const firstPool = page.locator('.pool-card').first();
        const poolName = await firstPool.locator('.pool-name').textContent();
        log(`Testing View Members on: ${poolName}`, 'info');

        // Click on member count or View All Members button
        const viewButton = firstPool.locator('button:has-text("View All Members")');
        const memberRow = firstPool.locator('.detail-row.clickable:has-text("Members")');

        if (await viewButton.count() > 0) {
          await viewButton.click();
        } else {
          await memberRow.click();
        }

        await page.waitForTimeout(1500);
        await takeScreenshot(page, '05-view-members-modal', 'View Members modal');

        const modalVisible = await page.isVisible('.modal-overlay');
        if (modalVisible) {
          testResults.passed.push('View Members modal opened');
          log('View Members modal opened', 'success');

          const hasMembersTable = await page.isVisible('.members-full-list');
          const hasEmptyState = await page.isVisible('.empty-members');

          if (hasMembersTable) {
            const memberCount = await page.locator('.member-row').count();
            testResults.passed.push(`View Members shows ${memberCount} member(s)`);
            log(`Pool has ${memberCount} member(s)`, 'info');
          } else if (hasEmptyState) {
            testResults.passed.push('View Members shows empty state');
            log('Pool has no members', 'info');
          }

          // Close modal
          await page.click('.modal-container button:has-text("Close")');
          await page.waitForTimeout(500);
          log('View Members test PASSED', 'success');
        } else {
          throw new Error('View Members modal did not open');
        }
      } catch (error) {
        testResults.failed.push(`View Members test failed: ${error.message}`);
        log(`View Members test FAILED: ${error.message}`, 'error');
      }

      // === TEST EDIT (UPDATE) ===
      log('TEST 4: Edit Pool (UPDATE operation)', 'info');
      try {
        const poolToEdit = page.locator('.pool-card').first();
        const originalName = await poolToEdit.locator('.pool-name').textContent();
        log(`Testing Edit on: ${originalName}`, 'info');

        await poolToEdit.locator('button:has-text("Edit")').click();
        await page.waitForTimeout(1500);
        await takeScreenshot(page, '06-edit-modal-opened', 'Edit modal opened');

        const modalTitle = await page.textContent('.modal-header h2');
        if (!modalTitle.toLowerCase().includes('edit')) {
          throw new Error('Edit modal title incorrect');
        }

        // Verify form is pre-filled
        const currentName = await page.inputValue('#poolName');
        if (!currentName) {
          throw new Error('Form not pre-filled with pool data');
        }

        testResults.passed.push('Edit modal opened with pre-filled data');
        log(`Form pre-filled with: ${currentName}`, 'info');

        // Update description only (safe change)
        const newDescription = `QA Test Update - ${new Date().toISOString()}`;
        await page.fill('textarea[name="description"]', newDescription);
        await takeScreenshot(page, '07-edit-form-updated', 'Description updated');

        await page.click('button:has-text("Update")');
        await page.waitForTimeout(3000);
        await takeScreenshot(page, '08-after-update', 'After update');

        // Check for success or error
        const hasSuccess = await page.isVisible('.alert-success');
        const hasError = await page.isVisible('.alert-error');

        if (hasSuccess) {
          testResults.passed.push('Pool updated successfully');
          log('Update test PASSED', 'success');
        } else if (hasError) {
          const errorMsg = await page.textContent('.alert-error');
          testResults.failed.push(`Update failed: ${errorMsg}`);
          log(`Update test FAILED: ${errorMsg}`, 'error');

          // Close modal if still open
          if (await page.isVisible('.modal-overlay')) {
            await page.click('.modal-container button:has-text("Cancel")');
            await page.waitForTimeout(500);
          }
        } else {
          testResults.warnings.push('Update completed but no success/error message shown');
        }
      } catch (error) {
        testResults.failed.push(`Edit test failed: ${error.message}`);
        log(`Edit test FAILED: ${error.message}`, 'error');
        await takeScreenshot(page, 'error-edit', 'Edit operation error');
      }

      // === TEST CREATE ===
      log('TEST 5: Create Pool (CREATE operation)', 'info');
      try {
        await page.click('button:has-text("Add Resource Pool")');
        await page.waitForTimeout(1500);
        await takeScreenshot(page, '09-create-modal-opened', 'Create modal opened');

        const createModalTitle = await page.textContent('.modal-header h2');
        if (!createModalTitle.toLowerCase().includes('create')) {
          throw new Error('Create modal title incorrect');
        }

        testResults.passed.push('Create modal opened');

        // Fill form
        const testName = `QA Auto Test ${Date.now()}`;
        await page.fill('#poolName', testName);
        await page.fill('textarea[name="description"]', 'Automated test pool for QA validation');
        await page.selectOption('select[name="poolType"]', { label: 'Custom' });
        await takeScreenshot(page, '10-create-form-filled', 'Create form filled');

        testResults.passed.push('Create form filled');

        await page.click('button:has-text("Create")');
        await page.waitForTimeout(4000);
        await takeScreenshot(page, '11-after-create-submit', 'After create submission');

        const createSuccess = await page.isVisible('.alert-success');
        const createError = await page.isVisible('.alert-error');

        if (createSuccess) {
          testResults.passed.push('Pool created successfully');
          log('Create test PASSED', 'success');

          // Verify pool appears in list
          await page.waitForTimeout(1000);
          const newPoolExists = await page.locator(`.pool-card:has-text("${testName}")`).count() > 0;
          if (newPoolExists) {
            testResults.passed.push('New pool appears in the list');
          }
        } else if (createError) {
          const errorMsg = await page.textContent('.alert-error');
          testResults.failed.push(`Create failed: ${errorMsg}`);
          log(`Create test FAILED: ${errorMsg}`, 'error');

          // Close modal if still open
          if (await page.isVisible('.modal-overlay')) {
            await page.click('.modal-container button:has-text("Cancel")');
            await page.waitForTimeout(500);
          }
        } else {
          testResults.warnings.push('Create completed but no success/error message shown');
        }
      } catch (error) {
        testResults.failed.push(`Create test failed: ${error.message}`);
        log(`Create test FAILED: ${error.message}`, 'error');
        await takeScreenshot(page, 'error-create', 'Create operation error');
      }

      // === TEST DELETE ===
      log('TEST 6: Delete Pool (DELETE operation)', 'info');
      try {
        // Find a Test Pool to delete
        const testPool = page.locator('.pool-card:has-text("Test Pool")').first();
        const testPoolExists = await testPool.count() > 0;

        if (!testPoolExists) {
          testResults.warnings.push('No Test Pool found for deletion test');
          log('No Test Pool found to delete - skipping DELETE test', 'warning');
        } else {
          const poolToDelete = await testPool.locator('.pool-name').textContent();
          log(`Testing Delete on: ${poolToDelete}`, 'info');

          await testPool.locator('button:has(.material-icons-round:has-text("delete"))').click();
          await page.waitForTimeout(1500);
          await takeScreenshot(page, '12-delete-confirmation-modal', 'Delete confirmation modal');

          const deleteModalVisible = await page.isVisible('.modal-overlay');
          if (!deleteModalVisible) {
            throw new Error('Delete confirmation modal did not open');
          }

          testResults.passed.push('Delete confirmation modal opened');

          // Verify pool name is shown
          const confirmationText = await page.textContent('.delete-confirm-content');
          if (confirmationText.includes(poolToDelete)) {
            testResults.passed.push('Pool name shown in confirmation');
          }

          await page.click('button.btn-danger:has-text("Delete")');
          await page.waitForTimeout(3000);
          await takeScreenshot(page, '13-after-delete', 'After delete confirmation');

          const deleteSuccess = await page.isVisible('.alert-success');
          const deleteError = await page.isVisible('.alert-error');

          if (deleteSuccess) {
            testResults.passed.push('Pool deleted successfully');
            log('Delete test PASSED', 'success');

            // Verify pool removed from list
            const stillExists = await page.locator(`.pool-card:has-text("${poolToDelete}")`).count() > 0;
            if (!stillExists) {
              testResults.passed.push('Pool removed from list after deletion');
            } else {
              testResults.warnings.push('Pool may still appear in list after deletion');
            }
          } else if (deleteError) {
            const errorMsg = await page.textContent('.alert-error');
            testResults.failed.push(`Delete failed: ${errorMsg}`);
            log(`Delete test FAILED: ${errorMsg}`, 'error');
          }
        }
      } catch (error) {
        testResults.failed.push(`Delete test failed: ${error.message}`);
        log(`Delete test FAILED: ${error.message}`, 'error');
        await takeScreenshot(page, 'error-delete', 'Delete operation error');
      }
    }

    // === GENERATE REPORT ===
    const report = {
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

    const reportPath = path.join(SCREENSHOT_DIR, 'test-report.json');
    fs.writeFileSync(reportPath, JSON.stringify(report, null, 2));

    // Generate text report
    let textReport = `
${'='.repeat(70)}
RESOURCE POOL MANAGEMENT - CRUD TEST REPORT
${'='.repeat(70)}

Test Date: ${new Date().toISOString()}
Screenshot Directory: ${SCREENSHOT_DIR}

${'='.repeat(70)}
SUMMARY
${'='.repeat(70)}
✅ Tests Passed: ${testResults.passed.length}
❌ Tests Failed: ${testResults.failed.length}
⚠️ Warnings: ${testResults.warnings.length}
📸 Screenshots: ${testResults.screenshots.length}

${'='.repeat(70)}
PASSED TESTS (${testResults.passed.length})
${'='.repeat(70)}
${testResults.passed.map((t, i) => `${i + 1}. ✅ ${t}`).join('\n')}

${'='.repeat(70)}
FAILED TESTS (${testResults.failed.length})
${'='.repeat(70)}
${testResults.failed.length > 0 ? testResults.failed.map((t, i) => `${i + 1}. ❌ ${t}`).join('\n') : 'None'}

${'='.repeat(70)}
WARNINGS (${testResults.warnings.length})
${'='.repeat(70)}
${testResults.warnings.length > 0 ? testResults.warnings.map((t, i) => `${i + 1}. ⚠️ ${t}`).join('\n') : 'None'}

${'='.repeat(70)}
SCREENSHOTS
${'='.repeat(70)}
${testResults.screenshots.map((s, i) => `${i + 1}. ${s.filename} - ${s.description}`).join('\n')}

${'='.repeat(70)}
END OF REPORT
${'='.repeat(70)}
`;

    const textReportPath = path.join(SCREENSHOT_DIR, 'test-report.txt');
    fs.writeFileSync(textReportPath, textReport);

    console.log(textReport);
    log(`Report saved: ${textReportPath}`, 'info');

  } catch (error) {
    log(`FATAL ERROR: ${error.message}`, 'error');
    await takeScreenshot(page, 'fatal-error', 'Fatal error occurred');
  } finally {
    await browser.close();
    process.exit(testResults.failed.length > 0 ? 1 : 0);
  }
}

runTests().catch(console.error);
