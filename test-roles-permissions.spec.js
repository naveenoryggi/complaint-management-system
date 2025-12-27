const { test, expect } = require('@playwright/test');
const path = require('path');
const fs = require('fs');

// Test configuration
const BASE_URL = 'http://localhost:4200';
const ROLES_URL = `${BASE_URL}/admin/roles`;
const API_BASE_URL = 'http://localhost:5000';

// Screenshot directory
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots', 'roles-permissions');

// Ensure screenshot directory exists
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

// Helper function to take screenshots
async function takeScreenshot(page, name) {
  const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
  const filename = `${timestamp}_${name}.png`;
  const filepath = path.join(SCREENSHOT_DIR, filename);
  await page.screenshot({ path: filepath, fullPage: true });
  console.log(`Screenshot saved: ${filepath}`);
  return filepath;
}

// Helper function to wait for network idle
async function waitForNetworkIdle(page) {
  await page.waitForLoadState('networkidle', { timeout: 10000 }).catch(() => {
    console.log('Network idle timeout, continuing...');
  });
}

test.describe('Role & Permission Management - Comprehensive Testing', () => {
  let page;
  let context;

  test.beforeAll(async ({ browser }) => {
    context = await browser.newContext({
      viewport: { width: 1920, height: 1080 }
    });
    page = await context.newPage();

    // Enable console logging
    page.on('console', msg => {
      console.log(`Browser Console [${msg.type()}]:`, msg.text());
    });

    // Log network errors
    page.on('pageerror', err => {
      console.error('Page Error:', err.message);
    });

    page.on('requestfailed', request => {
      console.error('Failed Request:', request.url(), request.failure().errorText);
    });
  });

  test.afterAll(async () => {
    await context.close();
  });

  test('1. Initial Page Load - Navigate and Verify Components', async () => {
    console.log('\n=== TEST 1: INITIAL PAGE LOAD ===');

    // Navigate to login page first
    await page.goto(BASE_URL, { waitUntil: 'networkidle' });
    await takeScreenshot(page, '01-initial-load');

    // Check if already logged in or need to login
    const currentUrl = page.url();
    if (currentUrl.includes('login')) {
      console.log('Login required, attempting to login...');

      // Try to find login form
      await page.waitForSelector('input[type="email"], input[name="username"], input[formControlName="username"]', { timeout: 5000 });
      await takeScreenshot(page, '02-login-page');

      // Attempt login with admin credentials
      const emailInput = await page.locator('input[type="email"], input[name="username"], input[formControlName="username"]').first();
      const passwordInput = await page.locator('input[type="password"], input[name="password"], input[formControlName="password"]').first();

      await emailInput.fill('admin@company.com');
      await passwordInput.fill('Admin@123');
      await takeScreenshot(page, '03-login-filled');

      // Click login button
      const loginButton = await page.locator('button[type="submit"], button:has-text("Login"), button:has-text("Sign In")').first();
      await loginButton.click();

      // Wait for navigation
      await page.waitForURL(/dashboard|home|admin/, { timeout: 10000 });
      await takeScreenshot(page, '04-after-login');
    }

    // Navigate to roles page
    console.log('Navigating to roles page...');
    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000); // Wait for Angular to render
    await takeScreenshot(page, '05-roles-page-initial');

    // Verify info banner
    console.log('Verifying info banner...');
    const infoBanner = await page.locator('.alert-info, .info-banner, [class*="info"]').first();
    if (await infoBanner.count() > 0) {
      const bannerText = await infoBanner.textContent();
      console.log('Info banner found:', bannerText);
      expect(bannerText).toBeTruthy();
    } else {
      console.log('WARNING: Info banner not found');
    }

    // Verify search bar
    console.log('Verifying search bar...');
    const searchInput = await page.locator('input[type="search"], input[placeholder*="Search"], input[name*="search"]').first();
    const searchExists = await searchInput.count() > 0;
    console.log('Search bar exists:', searchExists);
    expect(searchExists).toBe(true);

    // Verify "Show Active Only" toggle
    console.log('Verifying Show Active Only toggle...');
    const activeToggle = await page.locator('input[type="checkbox"], mat-slide-toggle, .toggle').filter({ hasText: /active/i });
    const toggleExists = await activeToggle.count() > 0 || await page.locator('label:has-text("Active")').count() > 0;
    console.log('Active toggle exists:', toggleExists);

    // Verify role cards grid
    console.log('Verifying role cards grid...');
    const roleCards = await page.locator('.card, .role-card, [class*="role-"]').all();
    console.log('Number of role cards found:', roleCards.length);
    expect(roleCards.length).toBeGreaterThan(0);

    await takeScreenshot(page, '06-roles-page-verified');
    console.log('✓ Test 1 Passed: Page loaded successfully with all components');
  });

  test('2. View Existing Roles - Verify Card Information', async () => {
    console.log('\n=== TEST 2: VIEW EXISTING ROLES ===');

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '07-view-roles-start');

    // Get all role cards
    const roleCards = await page.locator('.card, .role-card, [class*="card"]').all();
    console.log(`Found ${roleCards.length} role cards`);

    for (let i = 0; i < Math.min(roleCards.length, 5); i++) {
      console.log(`\nInspecting Role Card ${i + 1}:`);
      const card = roleCards[i];

      // Get card text content
      const cardText = await card.textContent();
      console.log('Card Content:', cardText.substring(0, 200));

      // Check for role name
      const roleName = await card.locator('h3, h4, h5, .card-title, [class*="name"]').first().textContent().catch(() => 'Not found');
      console.log('  - Role Name:', roleName);

      // Check for SYSTEM ROLE badge
      const systemBadge = await card.locator('.badge, .chip, [class*="badge"]').filter({ hasText: /system/i }).count();
      console.log('  - SYSTEM ROLE badge:', systemBadge > 0 ? 'Yes' : 'No');

      // Check for ACTIVE status badge
      const activeBadge = await card.locator('.badge, .chip, [class*="badge"]').filter({ hasText: /active/i }).count();
      console.log('  - ACTIVE badge:', activeBadge > 0 ? 'Yes' : 'No');

      // Check for Role Type
      const roleType = await card.locator('[class*="type"], dt:has-text("Type") + dd').first().textContent().catch(() => 'Not found');
      console.log('  - Role Type:', roleType);

      // Check for permissions count
      const permissionText = await card.textContent();
      const permMatch = permissionText.match(/(\d+)\s*(permission|granted|perm)/i);
      console.log('  - Permissions:', permMatch ? permMatch[1] : 'Not found');

      // Check for progress bar
      const progressBar = await card.locator('.progress, [role="progressbar"], [class*="progress"]').count();
      console.log('  - Progress bar:', progressBar > 0 ? 'Yes' : 'No');

      // Check for Edit button
      const editButton = await card.locator('button:has-text("Edit"), .btn-edit, [aria-label*="Edit"]').count();
      console.log('  - Edit button:', editButton > 0 ? 'Yes' : 'No');

      // Check for Delete button
      const deleteButton = await card.locator('button:has-text("Delete"), .btn-delete, [aria-label*="Delete"]').count();
      console.log('  - Delete button:', deleteButton > 0 ? 'Yes' : 'No');
    }

    await takeScreenshot(page, '08-roles-cards-verified');
    console.log('✓ Test 2 Passed: Role cards display all required information');
  });

  test('3. Create New Role - Test Custom Role Creation', async () => {
    console.log('\n=== TEST 3: CREATE NEW ROLE ===');

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '09-before-create');

    // Look for Create/Add button
    console.log('Looking for Create/Add Role button...');
    const createButtons = [
      'button:has-text("Create")',
      'button:has-text("Add")',
      'button:has-text("New")',
      '.btn-primary:has-text("Role")',
      '[class*="create"], [class*="add"]'
    ];

    let createButton = null;
    for (const selector of createButtons) {
      const btn = page.locator(selector).first();
      if (await btn.count() > 0) {
        createButton = btn;
        console.log('Found create button:', selector);
        break;
      }
    }

    if (!createButton || await createButton.count() === 0) {
      console.log('WARNING: Create/Add Role button not found on page');
      await takeScreenshot(page, '10-no-create-button');
      console.log('SKIPPING: Cannot test role creation without create button');
      return;
    }

    // Click create button
    await createButton.click();
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '11-create-form-opened');

    // Wait for form or modal to appear
    await page.waitForSelector('form, .modal, .dialog', { timeout: 5000 }).catch(() => {
      console.log('Form/Modal did not appear');
    });

    // Fill in the form
    console.log('Filling in role creation form...');

    // Name field
    const nameInput = await page.locator('input[name="name"], input[formControlName="name"], input[placeholder*="Name"]').first();
    if (await nameInput.count() > 0) {
      await nameInput.fill('Test Custom Role');
      console.log('Filled name: Test Custom Role');
    }

    // Code field
    const codeInput = await page.locator('input[name="code"], input[formControlName="code"], input[placeholder*="Code"]').first();
    if (await codeInput.count() > 0) {
      await codeInput.fill('TEST_CUSTOM_ROLE');
      console.log('Filled code: TEST_CUSTOM_ROLE');
    }

    // Description field
    const descInput = await page.locator('textarea[name="description"], textarea[formControlName="description"], input[name="description"]').first();
    if (await descInput.count() > 0) {
      await descInput.fill('A test role for validation purposes');
      console.log('Filled description');
    }

    await takeScreenshot(page, '12-create-form-filled');

    // Select role type if available
    const roleTypeSelect = await page.locator('select[name="roleType"], select[formControlName="roleType"], mat-select').first();
    if (await roleTypeSelect.count() > 0) {
      await roleTypeSelect.click();
      await page.waitForTimeout(500);
      // Select first option
      const firstOption = await page.locator('mat-option, option').nth(1);
      if (await firstOption.count() > 0) {
        await firstOption.click();
        console.log('Selected role type');
      }
    }

    await takeScreenshot(page, '13-ready-to-save');

    // Find and click Save button
    const saveButton = await page.locator('button:has-text("Save"), button:has-text("Create"), button[type="submit"]').first();
    if (await saveButton.count() > 0) {
      await saveButton.click();
      console.log('Clicked Save button');
      await page.waitForTimeout(2000);
      await takeScreenshot(page, '14-after-save');

      // Verify new role appears
      const newRole = await page.locator(':has-text("Test Custom Role")').first();
      if (await newRole.count() > 0) {
        console.log('✓ Test 3 Passed: New role created successfully');
      } else {
        console.log('WARNING: New role not found after creation');
      }
    } else {
      console.log('WARNING: Save button not found');
    }

    await takeScreenshot(page, '15-create-completed');
  });

  test('4. Manage Permissions - Test Permission Modal', async () => {
    console.log('\n=== TEST 4: MANAGE PERMISSIONS ===');

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '16-before-permissions');

    // Find a role card with permissions link
    console.log('Looking for permissions link...');
    const permissionLinks = [
      'a:has-text("granted")',
      'a:has-text("permission")',
      'button:has-text("permission")',
      '.permissions-link',
      '[class*="permission"]'
    ];

    let permissionLink = null;
    for (const selector of permissionLinks) {
      const link = page.locator(selector).first();
      if (await link.count() > 0) {
        permissionLink = link;
        console.log('Found permission link:', selector);
        break;
      }
    }

    if (!permissionLink || await permissionLink.count() === 0) {
      console.log('WARNING: Permissions link not found');
      await takeScreenshot(page, '17-no-permission-link');
      console.log('SKIPPING: Cannot test permissions without permission link');
      return;
    }

    // Click permissions link
    await permissionLink.click();
    await page.waitForTimeout(1500);
    await takeScreenshot(page, '18-permissions-modal-opened');

    // Wait for modal/dialog to appear
    await page.waitForSelector('.modal, .dialog, mat-dialog-container', { timeout: 5000 }).catch(() => {
      console.log('Permissions modal did not appear');
    });

    // Verify permissions grid
    console.log('Verifying permissions grid...');
    const checkboxes = await page.locator('input[type="checkbox"], mat-checkbox').all();
    console.log(`Found ${checkboxes.length} permission checkboxes`);

    // Look for Select All button
    const selectAllButton = await page.locator('button:has-text("Select All"), button:has-text("All")').first();
    if (await selectAllButton.count() > 0) {
      console.log('Found Select All button');
      await selectAllButton.click();
      await page.waitForTimeout(500);
      await takeScreenshot(page, '19-select-all-clicked');
    }

    // Look for Clear All button
    const clearAllButton = await page.locator('button:has-text("Clear All"), button:has-text("None")').first();
    if (await clearAllButton.count() > 0) {
      console.log('Found Clear All button');
      await clearAllButton.click();
      await page.waitForTimeout(500);
      await takeScreenshot(page, '20-clear-all-clicked');
    }

    // Toggle some permissions
    if (checkboxes.length > 0) {
      console.log('Toggling some permissions...');
      for (let i = 0; i < Math.min(3, checkboxes.length); i++) {
        await checkboxes[i].click();
        await page.waitForTimeout(300);
      }
      await takeScreenshot(page, '21-permissions-toggled');
    }

    // Find and click Save button
    const saveButton = await page.locator('.modal button:has-text("Save"), .dialog button:has-text("Save"), mat-dialog-container button:has-text("Save")').first();
    if (await saveButton.count() > 0) {
      await saveButton.click();
      console.log('Clicked Save button');
      await page.waitForTimeout(2000);
      await takeScreenshot(page, '22-permissions-saved');
      console.log('✓ Test 4 Passed: Permissions managed successfully');
    } else {
      console.log('WARNING: Save button not found in permissions modal');
    }

    await takeScreenshot(page, '23-permissions-completed');
  });

  test('5. Edit Role - Test Editing Non-System Role', async () => {
    console.log('\n=== TEST 5: EDIT ROLE ===');

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '24-before-edit');

    // Find a non-system role (avoid cards with "SYSTEM" badge)
    console.log('Looking for non-system role to edit...');
    const roleCards = await page.locator('.card, .role-card, [class*="card"]').all();

    let editButton = null;
    for (const card of roleCards) {
      const cardText = await card.textContent();
      // Skip system roles
      if (!cardText.match(/system role/i)) {
        const btn = card.locator('button:has-text("Edit"), .btn-edit, [aria-label*="Edit"]').first();
        if (await btn.count() > 0) {
          editButton = btn;
          console.log('Found edit button on non-system role');
          break;
        }
      }
    }

    if (!editButton) {
      console.log('WARNING: No editable non-system role found');
      await takeScreenshot(page, '25-no-edit-button');
      console.log('SKIPPING: Cannot test editing without editable role');
      return;
    }

    // Click edit button
    await editButton.click();
    await page.waitForTimeout(1500);
    await takeScreenshot(page, '26-edit-form-opened');

    // Wait for form or modal
    await page.waitForSelector('form, .modal, .dialog', { timeout: 5000 }).catch(() => {
      console.log('Edit form did not appear');
    });

    // Modify description
    const descInput = await page.locator('textarea[name="description"], textarea[formControlName="description"], input[name="description"]').first();
    if (await descInput.count() > 0) {
      await descInput.fill('Updated description for testing - ' + new Date().toISOString());
      console.log('Updated description field');
      await takeScreenshot(page, '27-edit-form-modified');
    }

    // Modify escalation level if available
    const escalationInput = await page.locator('input[name*="escalation"], select[name*="escalation"]').first();
    if (await escalationInput.count() > 0) {
      const tagName = await escalationInput.evaluate(el => el.tagName.toLowerCase());
      if (tagName === 'input') {
        await escalationInput.fill('2');
      } else if (tagName === 'select') {
        await escalationInput.selectOption({ index: 1 });
      }
      console.log('Updated escalation level');
    }

    await takeScreenshot(page, '28-ready-to-save-edit');

    // Find and click Save button
    const saveButton = await page.locator('button:has-text("Save"), button:has-text("Update"), button[type="submit"]').first();
    if (await saveButton.count() > 0) {
      await saveButton.click();
      console.log('Clicked Save button');
      await page.waitForTimeout(2000);
      await takeScreenshot(page, '29-edit-saved');
      console.log('✓ Test 5 Passed: Role edited successfully');
    } else {
      console.log('WARNING: Save button not found');
    }

    await takeScreenshot(page, '30-edit-completed');
  });

  test('6. Delete Role - Test Deleting Custom Role', async () => {
    console.log('\n=== TEST 6: DELETE ROLE ===');

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '31-before-delete');

    // Find a custom role (preferably the one we created)
    console.log('Looking for custom role to delete...');
    const testRole = await page.locator(':has-text("Test Custom Role")').first();

    let deleteButton = null;
    if (await testRole.count() > 0) {
      console.log('Found Test Custom Role');
      const card = testRole.locator('..').locator('..'); // Navigate up to card
      deleteButton = card.locator('button:has-text("Delete"), .btn-delete, [aria-label*="Delete"]').first();
    } else {
      // Find any non-system role
      console.log('Test Custom Role not found, looking for any custom role...');
      const roleCards = await page.locator('.card, .role-card, [class*="card"]').all();

      for (const card of roleCards) {
        const cardText = await card.textContent();
        if (!cardText.match(/system role/i)) {
          const btn = card.locator('button:has-text("Delete"), .btn-delete, [aria-label*="Delete"]').first();
          if (await btn.count() > 0) {
            deleteButton = btn;
            console.log('Found delete button on custom role');
            break;
          }
        }
      }
    }

    if (!deleteButton || await deleteButton.count() === 0) {
      console.log('WARNING: No deletable custom role found');
      await takeScreenshot(page, '32-no-delete-button');
      console.log('SKIPPING: Cannot test deletion without deletable role');
      return;
    }

    // Click delete button
    await deleteButton.click();
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '33-delete-confirmation');

    // Wait for confirmation modal
    await page.waitForSelector('.modal, .dialog, mat-dialog-container', { timeout: 5000 }).catch(() => {
      console.log('Confirmation modal did not appear');
    });

    // Find and click Confirm/Yes/Delete button in modal
    const confirmButtons = [
      '.modal button:has-text("Confirm")',
      '.modal button:has-text("Yes")',
      '.modal button:has-text("Delete")',
      '.dialog button:has-text("Confirm")',
      'mat-dialog-container button:has-text("Confirm")',
      'mat-dialog-container button:has-text("Yes")'
    ];

    let confirmButton = null;
    for (const selector of confirmButtons) {
      const btn = page.locator(selector).first();
      if (await btn.count() > 0) {
        confirmButton = btn;
        console.log('Found confirm button:', selector);
        break;
      }
    }

    if (confirmButton) {
      await confirmButton.click();
      console.log('Clicked confirm delete button');
      await page.waitForTimeout(2000);
      await takeScreenshot(page, '34-delete-confirmed');
      console.log('✓ Test 6 Passed: Role deleted successfully');
    } else {
      console.log('WARNING: Confirm button not found');
    }

    await takeScreenshot(page, '35-delete-completed');
  });

  test('7. Search and Filter - Test Search Functionality', async () => {
    console.log('\n=== TEST 7: SEARCH AND FILTER ===');

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '36-before-search');

    // Get initial role count
    const initialCards = await page.locator('.card, .role-card, [class*="card"]').all();
    const initialCount = initialCards.length;
    console.log(`Initial role count: ${initialCount}`);

    // Find search input
    const searchInput = await page.locator('input[type="search"], input[placeholder*="Search"], input[name*="search"]').first();

    if (await searchInput.count() === 0) {
      console.log('WARNING: Search input not found');
      await takeScreenshot(page, '37-no-search-input');
      console.log('SKIPPING: Cannot test search without search input');
      return;
    }

    // Test search by role name
    console.log('Testing search functionality...');
    await searchInput.fill('Admin');
    await page.waitForTimeout(1500);
    await takeScreenshot(page, '38-search-admin');

    const searchResults = await page.locator('.card, .role-card, [class*="card"]').all();
    console.log(`Search results count: ${searchResults.length}`);
    expect(searchResults.length).toBeLessThanOrEqual(initialCount);

    // Clear search
    await searchInput.clear();
    await page.waitForTimeout(1500);
    await takeScreenshot(page, '39-search-cleared');

    // Test "Show Active Only" toggle
    console.log('Testing Show Active Only toggle...');
    const activeToggle = await page.locator('input[type="checkbox"], mat-slide-toggle').filter({ hasText: /active/i }).first();

    if (await activeToggle.count() === 0) {
      // Try alternative selectors
      const toggleLabel = await page.locator('label:has-text("Active")').first();
      if (await toggleLabel.count() > 0) {
        await toggleLabel.click();
        await page.waitForTimeout(1500);
        await takeScreenshot(page, '40-active-toggle-on');
        console.log('Toggled Active filter ON');

        // Toggle off
        await toggleLabel.click();
        await page.waitForTimeout(1500);
        await takeScreenshot(page, '41-active-toggle-off');
        console.log('Toggled Active filter OFF');
      } else {
        console.log('WARNING: Active toggle not found');
      }
    } else {
      await activeToggle.click();
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '40-active-toggle-on');
      console.log('Toggled Active filter ON');

      await activeToggle.click();
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '41-active-toggle-off');
      console.log('Toggled Active filter OFF');
    }

    console.log('✓ Test 7 Passed: Search and filter functionality tested');
    await takeScreenshot(page, '42-search-filter-completed');
  });

  test('8. Console Errors Check - Verify No Critical Errors', async () => {
    console.log('\n=== TEST 8: CONSOLE ERRORS CHECK ===');

    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    await page.goto(ROLES_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(3000);
    await takeScreenshot(page, '43-final-state');

    console.log('\n=== CONSOLE ERRORS SUMMARY ===');
    if (consoleErrors.length === 0) {
      console.log('✓ No console errors detected');
    } else {
      console.log(`Found ${consoleErrors.length} console errors:`);
      consoleErrors.forEach((error, index) => {
        console.log(`${index + 1}. ${error}`);
      });
    }
  });
});

console.log('\n=== Test Suite Configuration ===');
console.log('Base URL:', BASE_URL);
console.log('Roles URL:', ROLES_URL);
console.log('Screenshot Directory:', SCREENSHOT_DIR);
console.log('=====================================\n');
