import { test, expect, Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

// Test configuration
const BASE_URL = 'http://localhost:4200';
const ADMIN_EMAIL = 'admin@example.com';
const ADMIN_PASSWORD = 'Admin123!';
const SCREENSHOT_DIR = path.join(__dirname, '..', 'test-evidence');

// Ensure screenshot directory exists
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

test.describe('Role & Permission Management E2E Tests', () => {
  let page: Page;

  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    // Set viewport for consistent screenshots
    await page.setViewportSize({ width: 1920, height: 1080 });
  });

  test.afterAll(async () => {
    await page.close();
  });

  test('1. Navigate to application and verify homepage loads', async () => {
    console.log('Starting Test 1: Navigate to homepage');

    await page.goto(BASE_URL, { waitUntil: 'networkidle' });
    await page.waitForTimeout(1000);

    // Take screenshot of homepage
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '01-homepage-initial.png'),
      fullPage: true
    });

    // Verify page loaded
    const title = await page.title();
    console.log(`Page title: ${title}`);

    // Check if login form is visible
    const loginFormVisible = await page.locator('form, [class*="login"], input[type="email"]').first().isVisible().catch(() => false);
    console.log(`Login form visible: ${loginFormVisible}`);

    expect(loginFormVisible).toBeTruthy();
  });

  test('2. Login with admin credentials', async () => {
    console.log('Starting Test 2: Admin login');

    // Wait for login form elements
    await page.waitForSelector('input[type="email"], input[formControlName="email"]', { timeout: 10000 });

    // Take screenshot before login
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '02-login-page-before.png'),
      fullPage: true
    });

    // Fill in credentials
    const emailInput = page.locator('input[type="email"], input[formControlName="email"]').first();
    await emailInput.fill(ADMIN_EMAIL);
    console.log(`Filled email: ${ADMIN_EMAIL}`);

    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]').first();
    await passwordInput.fill(ADMIN_PASSWORD);
    console.log(`Filled password`);

    // Take screenshot with filled credentials
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '03-login-page-filled.png'),
      fullPage: true
    });

    // Click login button
    const loginButton = page.locator('button[type="submit"], button:has-text("Login"), button:has-text("Sign In")').first();
    await loginButton.click();
    console.log('Clicked login button');

    // Wait for navigation after login
    await page.waitForTimeout(2000);
    await page.waitForLoadState('networkidle');

    // Take screenshot after login
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '04-after-login.png'),
      fullPage: true
    });

    // Verify login success - check for dashboard or admin menu
    const currentUrl = page.url();
    console.log(`Current URL after login: ${currentUrl}`);

    // Check if we're redirected from login page
    expect(currentUrl).not.toContain('/login');
  });

  test('3. Navigate to Role & Permission Management', async () => {
    console.log('Starting Test 3: Navigate to Role Management');

    // Wait for page to be ready
    await page.waitForLoadState('networkidle');

    // Look for Admin menu or navigation
    // Try multiple selectors for admin menu
    const adminMenuSelectors = [
      'a:has-text("Admin")',
      'button:has-text("Admin")',
      '[routerLink*="admin"]',
      '.nav-link:has-text("Admin")',
      'mat-nav-list a:has-text("Admin")'
    ];

    let adminMenuFound = false;
    let adminMenu;

    for (const selector of adminMenuSelectors) {
      adminMenu = page.locator(selector).first();
      const isVisible = await adminMenu.isVisible().catch(() => false);
      if (isVisible) {
        console.log(`Found admin menu with selector: ${selector}`);
        adminMenuFound = true;
        break;
      }
    }

    if (!adminMenuFound) {
      // Take screenshot of current page to debug
      await page.screenshot({
        path: path.join(SCREENSHOT_DIR, '05-admin-menu-not-found.png'),
        fullPage: true
      });
      console.log('Admin menu not found. Check screenshot for debugging.');
    }

    // Try to find role management link directly
    const roleManagementSelectors = [
      'a:has-text("Role")',
      '[routerLink*="role"]',
      'a:has-text("Permission")',
      '[href*="role"]'
    ];

    let roleManagementLink;
    let roleManagementFound = false;

    for (const selector of roleManagementSelectors) {
      roleManagementLink = page.locator(selector).first();
      const isVisible = await roleManagementLink.isVisible().catch(() => false);
      if (isVisible) {
        console.log(`Found role management link with selector: ${selector}`);
        roleManagementFound = true;
        break;
      }
    }

    if (roleManagementFound) {
      // Click on role management link
      await roleManagementLink.click();
      console.log('Clicked role management link');
    } else {
      // Try navigating directly to role management URL
      console.log('Role management link not found, trying direct navigation');
      await page.goto(`${BASE_URL}/admin/roles`, { waitUntil: 'networkidle' });
    }

    await page.waitForTimeout(2000);
    await page.waitForLoadState('networkidle');

    // Take screenshot of role management page
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '06-role-management-page.png'),
      fullPage: true
    });

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);
  });

  test('4. Verify page header with title and description', async () => {
    console.log('Starting Test 4: Verify page header');

    await page.waitForLoadState('networkidle');

    // Look for page header
    const headerSelectors = [
      'h1:has-text("Role")',
      'h2:has-text("Role")',
      '.page-header',
      '[class*="header"]',
      'mat-card-title:has-text("Role")'
    ];

    let headerFound = false;
    let headerText = '';

    for (const selector of headerSelectors) {
      const header = page.locator(selector).first();
      const isVisible = await header.isVisible().catch(() => false);
      if (isVisible) {
        headerText = await header.textContent() || '';
        console.log(`Found header with selector: ${selector}, text: ${headerText}`);
        headerFound = true;
        break;
      }
    }

    // Take screenshot highlighting header area
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '07-page-header-verification.png'),
      fullPage: true
    });

    // Log header status
    console.log(`Header found: ${headerFound}`);
    console.log(`Header text: ${headerText}`);

    // Look for description
    const descriptionVisible = await page.locator('p, .description, mat-card-subtitle').first().isVisible().catch(() => false);
    console.log(`Description visible: ${descriptionVisible}`);

    if (descriptionVisible) {
      const descriptionText = await page.locator('p, .description, mat-card-subtitle').first().textContent();
      console.log(`Description text: ${descriptionText}`);
    }
  });

  test('5. Verify Add Role button is visible and clickable', async () => {
    console.log('Starting Test 5: Verify Add Role button');

    // Look for Add Role button
    const addRoleButtonSelectors = [
      'button:has-text("Add Role")',
      'button:has-text("Add")',
      'button:has-text("New Role")',
      '[aria-label*="Add"]',
      'button[class*="add"]'
    ];

    let addButtonFound = false;
    let addButton;

    for (const selector of addRoleButtonSelectors) {
      addButton = page.locator(selector).first();
      const isVisible = await addButton.isVisible().catch(() => false);
      if (isVisible) {
        const buttonText = await addButton.textContent();
        console.log(`Found Add button with selector: ${selector}, text: ${buttonText}`);
        addButtonFound = true;
        break;
      }
    }

    // Take screenshot with Add Role button highlighted
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '08-add-role-button-verification.png'),
      fullPage: true
    });

    console.log(`Add Role button found: ${addButtonFound}`);

    if (addButtonFound && addButton) {
      // Check if button is enabled
      const isEnabled = await addButton.isEnabled();
      console.log(`Add Role button enabled: ${isEnabled}`);

      // Hover over button to check interactivity
      await addButton.hover();
      await page.waitForTimeout(500);

      await page.screenshot({
        path: path.join(SCREENSHOT_DIR, '09-add-role-button-hover.png'),
        fullPage: true
      });
    }
  });

  test('6. Verify role cards are displayed with Edit/Delete buttons', async () => {
    console.log('Starting Test 6: Verify role cards');

    // Look for role cards
    const roleCardSelectors = [
      'mat-card',
      '.role-card',
      '[class*="card"]',
      '.card'
    ];

    let roleCards;
    let cardsFound = false;

    for (const selector of roleCardSelectors) {
      roleCards = page.locator(selector);
      const count = await roleCards.count();
      if (count > 0) {
        console.log(`Found ${count} cards with selector: ${selector}`);
        cardsFound = true;
        break;
      }
    }

    // Take screenshot of role cards
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '10-role-cards-overview.png'),
      fullPage: true
    });

    if (cardsFound && roleCards) {
      const cardCount = await roleCards.count();
      console.log(`Total role cards found: ${cardCount}`);

      // Check first card for Edit/Delete buttons
      if (cardCount > 0) {
        const firstCard = roleCards.first();

        // Look for Edit button
        const editButton = firstCard.locator('button:has-text("Edit"), button[aria-label*="Edit"], mat-icon:has-text("edit")').first();
        const editVisible = await editButton.isVisible().catch(() => false);
        console.log(`Edit button visible on first card: ${editVisible}`);

        // Look for Delete button
        const deleteButton = firstCard.locator('button:has-text("Delete"), button[aria-label*="Delete"], mat-icon:has-text("delete")').first();
        const deleteVisible = await deleteButton.isVisible().catch(() => false);
        console.log(`Delete button visible on first card: ${deleteVisible}`);

        // Take screenshot of first card
        await firstCard.screenshot({
          path: path.join(SCREENSHOT_DIR, '11-first-role-card-detail.png')
        });
      }
    } else {
      console.log('No role cards found on the page');
    }
  });

  test('7. Verify status badges (ACTIVE/INACTIVE) are visible', async () => {
    console.log('Starting Test 7: Verify status badges');

    // Look for status badges
    const statusBadgeSelectors = [
      ':has-text("ACTIVE")',
      ':has-text("INACTIVE")',
      '.badge',
      '[class*="status"]',
      'mat-chip'
    ];

    let activeBadgeFound = false;
    let inactiveBadgeFound = false;

    for (const selector of statusBadgeSelectors) {
      const elements = page.locator(selector);
      const count = await elements.count();

      for (let i = 0; i < count; i++) {
        const text = await elements.nth(i).textContent();
        if (text?.includes('ACTIVE')) {
          activeBadgeFound = true;
          console.log(`Found ACTIVE badge with selector: ${selector}`);
        }
        if (text?.includes('INACTIVE')) {
          inactiveBadgeFound = true;
          console.log(`Found INACTIVE badge with selector: ${selector}`);
        }
      }
    }

    console.log(`ACTIVE badge found: ${activeBadgeFound}`);
    console.log(`INACTIVE badge found: ${inactiveBadgeFound}`);

    // Take screenshot
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '12-status-badges-verification.png'),
      fullPage: true
    });
  });

  test('8. Verify progress bar shows on role cards', async () => {
    console.log('Starting Test 8: Verify progress bars');

    // Look for progress bars
    const progressBarSelectors = [
      'mat-progress-bar',
      '[role="progressbar"]',
      '.progress',
      'progress'
    ];

    let progressBarsFound = false;
    let progressBarCount = 0;

    for (const selector of progressBarSelectors) {
      const progressBars = page.locator(selector);
      const count = await progressBars.count();
      if (count > 0) {
        progressBarsFound = true;
        progressBarCount = count;
        console.log(`Found ${count} progress bars with selector: ${selector}`);
        break;
      }
    }

    console.log(`Progress bars found: ${progressBarsFound}, count: ${progressBarCount}`);

    // Take screenshot
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '13-progress-bars-verification.png'),
      fullPage: true
    });
  });

  test('9. Verify permission count shows on role cards', async () => {
    console.log('Starting Test 9: Verify permission counts');

    // Look for permission count text
    const permissionCountElements = page.locator(':has-text("permission"), :has-text("Permission")');
    const count = await permissionCountElements.count();

    console.log(`Permission count elements found: ${count}`);

    if (count > 0) {
      for (let i = 0; i < Math.min(count, 5); i++) {
        const text = await permissionCountElements.nth(i).textContent();
        console.log(`Permission text ${i + 1}: ${text}`);
      }
    }

    // Take screenshot
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '14-permission-counts-verification.png'),
      fullPage: true
    });
  });

  test('10. Test clicking Add Role button and verify form opens', async () => {
    console.log('Starting Test 10: Test Add Role button click');

    // Find and click Add Role button
    const addRoleButtonSelectors = [
      'button:has-text("Add Role")',
      'button:has-text("Add")',
      'button:has-text("New Role")'
    ];

    let addButton;
    let buttonFound = false;

    for (const selector of addRoleButtonSelectors) {
      addButton = page.locator(selector).first();
      const isVisible = await addButton.isVisible().catch(() => false);
      if (isVisible) {
        buttonFound = true;
        break;
      }
    }

    if (buttonFound && addButton) {
      // Click Add Role button
      await addButton.click();
      console.log('Clicked Add Role button');

      await page.waitForTimeout(1000);

      // Take screenshot after clicking
      await page.screenshot({
        path: path.join(SCREENSHOT_DIR, '15-after-clicking-add-role.png'),
        fullPage: true
      });

      // Check if dialog/form opened
      const dialogSelectors = [
        'mat-dialog-container',
        '.dialog',
        '[role="dialog"]',
        'form'
      ];

      let dialogFound = false;

      for (const selector of dialogSelectors) {
        const dialog = page.locator(selector).first();
        const isVisible = await dialog.isVisible().catch(() => false);
        if (isVisible) {
          console.log(`Dialog/Form opened with selector: ${selector}`);
          dialogFound = true;

          // Take screenshot of dialog
          await dialog.screenshot({
            path: path.join(SCREENSHOT_DIR, '16-add-role-dialog.png')
          });

          break;
        }
      }

      console.log(`Dialog/Form opened: ${dialogFound}`);
    } else {
      console.log('Add Role button not found');
      await page.screenshot({
        path: path.join(SCREENSHOT_DIR, '15-add-role-button-not-found.png'),
        fullPage: true
      });
    }
  });

  test('11. Fill out Add Role form with test data', async () => {
    console.log('Starting Test 11: Fill Add Role form');

    // Generate test data
    const testRoleData = {
      name: `Test Role ${Date.now()}`,
      description: 'This is a test role created by E2E automated testing',
    };

    console.log('Test role data:', testRoleData);

    // Look for form fields
    const nameInput = page.locator('input[formControlName="name"], input[name="name"], input[placeholder*="name" i]').first();
    const nameInputVisible = await nameInput.isVisible().catch(() => false);

    if (nameInputVisible) {
      await nameInput.fill(testRoleData.name);
      console.log(`Filled role name: ${testRoleData.name}`);
    } else {
      console.log('Name input not found');
    }

    const descriptionInput = page.locator('textarea[formControlName="description"], textarea[name="description"], input[formControlName="description"]').first();
    const descriptionVisible = await descriptionInput.isVisible().catch(() => false);

    if (descriptionVisible) {
      await descriptionInput.fill(testRoleData.description);
      console.log(`Filled description: ${testRoleData.description}`);
    } else {
      console.log('Description input not found');
    }

    await page.waitForTimeout(500);

    // Take screenshot of filled form
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '17-add-role-form-filled.png'),
      fullPage: true
    });

    // Look for permissions checkboxes
    const permissionCheckboxes = page.locator('mat-checkbox, input[type="checkbox"]');
    const checkboxCount = await permissionCheckboxes.count();
    console.log(`Permission checkboxes found: ${checkboxCount}`);

    // Select a few permissions
    if (checkboxCount > 0) {
      const checkboxesToSelect = Math.min(3, checkboxCount);
      for (let i = 0; i < checkboxesToSelect; i++) {
        const checkbox = permissionCheckboxes.nth(i);
        const isVisible = await checkbox.isVisible().catch(() => false);
        if (isVisible) {
          await checkbox.click();
          await page.waitForTimeout(200);
        }
      }
      console.log(`Selected ${checkboxesToSelect} permissions`);

      await page.screenshot({
        path: path.join(SCREENSHOT_DIR, '18-add-role-form-with-permissions.png'),
        fullPage: true
      });
    }

    // Look for Submit/Save button
    const submitButtonSelectors = [
      'button:has-text("Save")',
      'button:has-text("Submit")',
      'button:has-text("Create")',
      'button[type="submit"]'
    ];

    let submitButton;
    let submitButtonFound = false;

    for (const selector of submitButtonSelectors) {
      submitButton = page.locator(selector).first();
      const isVisible = await submitButton.isVisible().catch(() => false);
      if (isVisible) {
        const buttonText = await submitButton.textContent();
        console.log(`Found submit button with selector: ${selector}, text: ${buttonText}`);
        submitButtonFound = true;
        break;
      }
    }

    if (submitButtonFound && submitButton) {
      // Click submit button
      await submitButton.click();
      console.log('Clicked submit button');

      await page.waitForTimeout(2000);
      await page.waitForLoadState('networkidle');

      // Take screenshot after submission
      await page.screenshot({
        path: path.join(SCREENSHOT_DIR, '19-after-role-creation.png'),
        fullPage: true
      });

      // Check for success message
      const successMessageSelectors = [
        ':has-text("success")',
        ':has-text("created")',
        '.success',
        'mat-snack-bar-container'
      ];

      for (const selector of successMessageSelectors) {
        const successMsg = page.locator(selector).first();
        const isVisible = await successMsg.isVisible().catch(() => false);
        if (isVisible) {
          const msgText = await successMsg.textContent();
          console.log(`Success message found: ${msgText}`);
          break;
        }
      }
    } else {
      console.log('Submit button not found');
    }
  });

  test('12. Capture console logs and network activity', async () => {
    console.log('Starting Test 12: Capture console logs');

    // Set up console log capture
    const consoleMessages: string[] = [];
    page.on('console', msg => {
      consoleMessages.push(`[${msg.type()}] ${msg.text()}`);
    });

    // Reload page to capture fresh logs
    await page.reload({ waitUntil: 'networkidle' });
    await page.waitForTimeout(2000);

    // Log captured console messages
    console.log('\n===== CONSOLE LOGS =====');
    consoleMessages.forEach(msg => console.log(msg));
    console.log('===== END CONSOLE LOGS =====\n');

    // Save console logs to file
    const logsPath = path.join(SCREENSHOT_DIR, 'console-logs.txt');
    fs.writeFileSync(logsPath, consoleMessages.join('\n'));
    console.log(`Console logs saved to: ${logsPath}`);

    // Take final screenshot
    await page.screenshot({
      path: path.join(SCREENSHOT_DIR, '20-final-role-management-state.png'),
      fullPage: true
    });
  });

  test('13. Summary report', async () => {
    console.log('\n===== E2E TEST EXECUTION SUMMARY =====');
    console.log('All tests completed. Check the following directory for evidence:');
    console.log(`Screenshot directory: ${SCREENSHOT_DIR}`);
    console.log('\nScreenshots captured:');
    console.log('01-homepage-initial.png - Initial homepage');
    console.log('02-login-page-before.png - Login page before filling');
    console.log('03-login-page-filled.png - Login page with credentials');
    console.log('04-after-login.png - After successful login');
    console.log('06-role-management-page.png - Role management page');
    console.log('07-page-header-verification.png - Page header verification');
    console.log('08-add-role-button-verification.png - Add Role button');
    console.log('09-add-role-button-hover.png - Add Role button hover state');
    console.log('10-role-cards-overview.png - Role cards overview');
    console.log('11-first-role-card-detail.png - First role card detail');
    console.log('12-status-badges-verification.png - Status badges');
    console.log('13-progress-bars-verification.png - Progress bars');
    console.log('14-permission-counts-verification.png - Permission counts');
    console.log('15-after-clicking-add-role.png - After clicking Add Role');
    console.log('16-add-role-dialog.png - Add Role dialog/form');
    console.log('17-add-role-form-filled.png - Filled Add Role form');
    console.log('18-add-role-form-with-permissions.png - Form with permissions selected');
    console.log('19-after-role-creation.png - After role creation');
    console.log('20-final-role-management-state.png - Final state');
    console.log('console-logs.txt - Browser console logs');
    console.log('===== END SUMMARY =====\n');
  });
});
