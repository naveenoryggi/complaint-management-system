/**
 * Manual Resource Pool CRUD Testing - Interactive Mode
 * This script will help manually test CRUD operations step by step
 */

const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');
const readline = require('readline');

const BASE_URL = 'http://localhost:4200';
const LOGIN_EMAIL = 'admin@complaintmanagement.com';
const LOGIN_PASSWORD = 'Admin@123';
const SCREENSHOT_DIR = path.join(__dirname, '..', 'test-screenshots', 'resource-pool-manual', Date.now().toString());

if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout
});

function question(prompt) {
  return new Promise((resolve) => {
    rl.question(prompt, resolve);
  });
}

async function takeScreenshot(page, name, description = '') {
  const filename = `${Date.now()}-${name}.png`;
  const filepath = path.join(SCREENSHOT_DIR, filename);
  await page.screenshot({ path: filepath, fullPage: true });
  console.log(`\n📸 Screenshot: ${filename} - ${description}`);
  return filepath;
}

async function runManualTest() {
  console.log('='.repeat(60));
  console.log('RESOURCE POOL MANAGEMENT - MANUAL TESTING');
  console.log('='.repeat(60));
  console.log(`Screenshots will be saved to: ${SCREENSHOT_DIR}\n`);

  const browser = await chromium.launch({
    headless: false,
    slowMo: 500
  });

  const context = await browser.newContext({
    viewport: { width: 1920, height: 1080 }
  });

  const page = await context.newPage();
  const testResults = [];

  try {
    // Login
    console.log('\n🔐 Step 1: Logging in...');
    await page.goto(`${BASE_URL}/login`);
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '01-login-page', 'Login page');

    await page.fill('#email', LOGIN_EMAIL);
    await page.fill('#password', LOGIN_PASSWORD);
    await page.click('button[type="submit"]');
    await page.waitForURL(/dashboard|admin/, { timeout: 10000 });
    await takeScreenshot(page, '02-after-login', 'Logged in successfully');
    console.log('✅ Login successful');
    testResults.push({ test: 'Login', status: 'PASS' });

    // Navigate to Resource Pools
    console.log('\n🧭 Step 2: Navigating to Resource Pools...');
    await page.goto(`${BASE_URL}/admin/resource-pools`);
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '03-resource-pools-page', 'Resource Pools page loaded');

    const poolCount = await page.locator('.pool-card').count();
    console.log(`✅ Page loaded with ${poolCount} pool(s)`);
    testResults.push({ test: 'Page Load', status: 'PASS', details: `${poolCount} pools found` });

    await question('\nPress Enter to test EDIT functionality...');

    // Test EDIT on first existing pool
    console.log('\n✏️ Step 3: Testing EDIT operation...');
    const firstPool = page.locator('.pool-card').first();
    const poolName = await firstPool.locator('.pool-name').textContent();
    console.log(`Testing with pool: "${poolName}"`);

    await firstPool.locator('button:has-text("Edit")').click();
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '04-edit-modal-opened', 'Edit modal opened');

    const modalTitle = await page.textContent('.modal-header h2');
    if (modalTitle.toLowerCase().includes('edit')) {
      console.log('✅ Edit modal opened correctly');
      testResults.push({ test: 'Edit Modal Open', status: 'PASS' });
    } else {
      console.log('❌ Edit modal title incorrect');
      testResults.push({ test: 'Edit Modal Open', status: 'FAIL' });
    }

    // Check if form is pre-filled
    const currentName = await page.inputValue('#poolName');
    console.log(`Form pre-filled with name: "${currentName}"`);

    await question('\nPress Enter to update the pool description...');

    // Update description
    const newDescription = `Updated by QA Test - ${new Date().toISOString()}`;
    await page.fill('#poolDescription, textarea[name="description"]', newDescription);
    await takeScreenshot(page, '05-edit-form-updated', 'Edit form updated');

    await question('\nPress Enter to submit the update...');

    await page.click('button:has-text("Update")');
    await page.waitForTimeout(3000);
    await takeScreenshot(page, '06-after-update', 'After update submission');

    // Check for success message or error
    const hasSuccess = await page.isVisible('.alert-success');
    const hasError = await page.isVisible('.alert-error');

    if (hasSuccess) {
      const successMsg = await page.textContent('.alert-success');
      console.log(`✅ Update successful: ${successMsg}`);
      testResults.push({ test: 'Update Pool', status: 'PASS' });
    } else if (hasError) {
      const errorMsg = await page.textContent('.alert-error');
      console.log(`❌ Update failed: ${errorMsg}`);
      testResults.push({ test: 'Update Pool', status: 'FAIL', details: errorMsg });
    }

    await question('\nPress Enter to test VIEW MEMBERS functionality...');

    // Test VIEW MEMBERS
    console.log('\n👥 Step 4: Testing VIEW MEMBERS...');
    const poolWithMembers = page.locator('.pool-card').first();

    // Try clicking the "View All Members" button or clickable member count
    const viewButton = poolWithMembers.locator('button:has-text("View All Members")');
    const memberCountRow = poolWithMembers.locator('.detail-row.clickable:has-text("Members")');

    const hasViewButton = await viewButton.count() > 0;
    if (hasViewButton) {
      await viewButton.click();
    } else {
      await memberCountRow.click();
    }

    await page.waitForTimeout(1000);
    await takeScreenshot(page, '07-view-members-modal', 'View Members modal');

    const membersModalVisible = await page.isVisible('.modal-overlay');
    if (membersModalVisible) {
      console.log('✅ View Members modal opened');
      testResults.push({ test: 'View Members Modal', status: 'PASS' });

      const hasMembersTable = await page.isVisible('.members-full-list');
      const hasEmptyState = await page.isVisible('.empty-members');

      if (hasMembersTable) {
        const memberCount = await page.locator('.member-row').count();
        console.log(`   Pool has ${memberCount} member(s)`);
        await takeScreenshot(page, '08-members-list', `Members list (${memberCount} members)`);
      } else if (hasEmptyState) {
        console.log('   Pool has no members (empty state)');
        await takeScreenshot(page, '08-empty-members', 'Empty members state');
      }

      // Close modal
      await page.click('.modal-container button:has-text("Close")');
      await page.waitForTimeout(500);
    } else {
      console.log('❌ View Members modal did not open');
      testResults.push({ test: 'View Members Modal', status: 'FAIL' });
    }

    await question('\nPress Enter to test CREATE functionality...');

    // Test CREATE
    console.log('\n➕ Step 5: Testing CREATE operation...');
    await page.click('button:has-text("Add Resource Pool")');
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '09-create-modal', 'Create modal opened');

    const createModalTitle = await page.textContent('.modal-header h2');
    if (createModalTitle.toLowerCase().includes('create')) {
      console.log('✅ Create modal opened');
      testResults.push({ test: 'Create Modal Open', status: 'PASS' });
    }

    // Fill form
    const testPoolName = `QA Manual Test Pool ${Date.now()}`;
    await page.fill('#poolName', testPoolName);
    await page.fill('#poolDescription', 'Manual test pool created for QA validation');
    await page.selectOption('#poolType', { label: 'Custom' });
    await takeScreenshot(page, '10-create-form-filled', 'Create form filled');

    await question('\nPress Enter to submit the create form...');

    await page.click('button:has-text("Create")');
    await page.waitForTimeout(3000);
    await takeScreenshot(page, '11-after-create', 'After create submission');

    const createSuccess = await page.isVisible('.alert-success');
    const createError = await page.isVisible('.alert-error');

    if (createSuccess) {
      console.log('✅ Pool created successfully');
      testResults.push({ test: 'Create Pool', status: 'PASS' });
    } else if (createError) {
      const errorMsg = await page.textContent('.alert-error');
      console.log(`❌ Create failed: ${errorMsg}`);
      testResults.push({ test: 'Create Pool', status: 'FAIL', details: errorMsg });

      // Close the modal if it's still open
      const modalStillOpen = await page.isVisible('.modal-overlay');
      if (modalStillOpen) {
        await page.click('.modal-container button:has-text("Cancel")');
        await page.waitForTimeout(500);
      }
    }

    await question('\nPress Enter to test DELETE functionality...');

    // Test DELETE on a "Test Pool"
    console.log('\n🗑️ Step 6: Testing DELETE operation...');
    const testPoolCard = page.locator('.pool-card:has-text("Test Pool")').first();
    const testPoolExists = await testPoolCard.count() > 0;

    if (testPoolExists) {
      const poolToDelete = await testPoolCard.locator('.pool-name').textContent();
      console.log(`Deleting pool: "${poolToDelete}"`);

      await testPoolCard.locator('button:has(.material-icons-round:has-text("delete"))').click();
      await page.waitForTimeout(1000);
      await takeScreenshot(page, '12-delete-confirmation', 'Delete confirmation modal');

      const deleteModalVisible = await page.isVisible('.modal-overlay');
      if (deleteModalVisible) {
        console.log('✅ Delete confirmation modal opened');
        testResults.push({ test: 'Delete Confirmation Modal', status: 'PASS' });

        await question('\nPress Enter to confirm deletion...');

        await page.click('button.btn-danger:has-text("Delete")');
        await page.waitForTimeout(3000);
        await takeScreenshot(page, '13-after-delete', 'After delete confirmation');

        const deleteSuccess = await page.isVisible('.alert-success');
        const deleteError = await page.isVisible('.alert-error');

        if (deleteSuccess) {
          console.log('✅ Pool deleted successfully');
          testResults.push({ test: 'Delete Pool', status: 'PASS' });
        } else if (deleteError) {
          const errorMsg = await page.textContent('.alert-error');
          console.log(`❌ Delete failed: ${errorMsg}`);
          testResults.push({ test: 'Delete Pool', status: 'FAIL', details: errorMsg });
        }
      }
    } else {
      console.log('⚠️ No "Test Pool" found to delete');
      testResults.push({ test: 'Delete Pool', status: 'SKIP', details: 'No test pool available' });
    }

    // Final summary
    console.log('\n' + '='.repeat(60));
    console.log('TEST SUMMARY');
    console.log('='.repeat(60));
    testResults.forEach((result, index) => {
      const icon = result.status === 'PASS' ? '✅' : result.status === 'FAIL' ? '❌' : '⚠️';
      console.log(`${index + 1}. ${icon} ${result.test}: ${result.status}`);
      if (result.details) {
        console.log(`   Details: ${result.details}`);
      }
    });
    console.log('='.repeat(60));
    console.log(`\nAll screenshots saved to: ${SCREENSHOT_DIR}`);

  } catch (error) {
    console.error('\n❌ Error during testing:', error.message);
    await takeScreenshot(page, 'error-final', 'Error occurred');
  } finally {
    await question('\nPress Enter to close the browser and exit...');
    await browser.close();
    rl.close();
  }
}

runManualTest().catch(console.error);
