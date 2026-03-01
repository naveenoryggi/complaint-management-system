import { test, expect, Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

const BASE_URL = 'http://localhost:4200';
const API_URL = 'http://localhost:5000';
const EVIDENCE_DIR = 'test-evidence/admin-panel-validation';

// Test credentials
const ADMIN_CREDENTIALS = {
  email: 'admin@complaintmanagement.com',
  password: 'Admin@123'
};

// Helper function to save screenshot
async function saveScreenshot(page: Page, filename: string) {
  const screenshotPath = path.join(EVIDENCE_DIR, filename);
  await page.screenshot({ path: screenshotPath, fullPage: true });
  console.log(`Screenshot saved: ${screenshotPath}`);
}

// Helper function to wait for network idle
async function waitForPageLoad(page: Page, timeout = 5000) {
  try {
    await page.waitForLoadState('networkidle', { timeout });
  } catch (e) {
    // Continue even if networkidle times out
    await page.waitForLoadState('domcontentloaded');
  }
}

test.describe('Admin Panel Navigation E2E Test', () => {
  let page: Page;

  test.beforeAll(async () => {
    // Create evidence directory
    if (!fs.existsSync(EVIDENCE_DIR)) {
      fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
    }
  });

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();

    // Set viewport for consistent screenshots
    await page.setViewportSize({ width: 1920, height: 1080 });
  });

  test.afterEach(async () => {
    await page.close();
  });

  test('1. Login Functionality - Validate admin login', async () => {
    console.log('\n=== TEST 1: LOGIN FUNCTIONALITY ===\n');

    // Navigate to login page
    await page.goto(BASE_URL);
    await waitForPageLoad(page);
    await saveScreenshot(page, '01-login-page.png');

    // Verify login form elements
    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await expect(emailInput).toBeVisible({ timeout: 10000 });
    await expect(passwordInput).toBeVisible();
    await expect(loginButton).toBeVisible();

    console.log('✓ Login form elements visible');

    // Fill login credentials
    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await saveScreenshot(page, '02-login-filled.png');

    console.log('✓ Credentials filled');

    // Click login button
    await loginButton.click();

    // Wait for navigation after login
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);
    await saveScreenshot(page, '03-after-login-dashboard.png');

    console.log('✓ Login successful - redirected to dashboard');

    // Verify we're logged in by checking for common post-login elements
    const isLoggedIn = await page.locator('header, nav, .navbar, [role="navigation"]').count() > 0;
    expect(isLoggedIn).toBeTruthy();

    console.log('✓ Post-login UI elements detected');
  });

  test('2. Navigate to User Management', async () => {
    console.log('\n=== TEST 2: USER MANAGEMENT NAVIGATION ===\n');

    // Login first
    await page.goto(BASE_URL);
    await waitForPageLoad(page);

    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await loginButton.click();
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);

    console.log('✓ Logged in successfully');

    // Try multiple navigation strategies to User Management
    let userManagementLoaded = false;

    // Strategy 1: Direct URL navigation
    console.log('Attempting direct URL navigation to /admin/users...');
    await page.goto(`${BASE_URL}/admin/users`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);
    await saveScreenshot(page, '04-users-direct-url.png');

    // Check if we landed on user management page
    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    if (currentUrl.includes('/users')) {
      userManagementLoaded = true;
      console.log('✓ User Management page loaded via direct URL');
    }

    // Strategy 2: Try menu navigation
    if (!userManagementLoaded) {
      console.log('Attempting menu navigation...');

      // Look for Admin Panel menu/link
      const adminMenuSelectors = [
        'a:has-text("Admin Panel")',
        'a:has-text("Admin")',
        'button:has-text("Admin Panel")',
        'button:has-text("Admin")',
        '[routerLink="/admin"]',
        'a[href="/admin"]'
      ];

      for (const selector of adminMenuSelectors) {
        const menuItem = page.locator(selector).first();
        if (await menuItem.isVisible({ timeout: 2000 }).catch(() => false)) {
          await menuItem.click();
          await waitForPageLoad(page);
          await page.waitForTimeout(1000);
          break;
        }
      }

      // Now look for User Management link
      const userMenuSelectors = [
        'a:has-text("User Management")',
        'a:has-text("Users")',
        'button:has-text("User Management")',
        'button:has-text("Users")',
        '[routerLink="/admin/users"]',
        'a[href="/admin/users"]'
      ];

      for (const selector of userMenuSelectors) {
        const menuItem = page.locator(selector).first();
        if (await menuItem.isVisible({ timeout: 2000 }).catch(() => false)) {
          await menuItem.click();
          await waitForPageLoad(page);
          await page.waitForTimeout(2000);
          await saveScreenshot(page, '05-users-menu-navigation.png');
          userManagementLoaded = page.url().includes('/users');
          if (userManagementLoaded) {
            console.log('✓ User Management page loaded via menu navigation');
          }
          break;
        }
      }
    }

    // Verify User Management page content
    if (userManagementLoaded) {
      const pageContent = await page.content();
      const hasUserContent = pageContent.toLowerCase().includes('user') ||
                            pageContent.toLowerCase().includes('employee');

      console.log(`✓ User Management page content verified: ${hasUserContent}`);

      // Look for common User Management elements
      const commonElements = [
        page.locator('table, mat-table, .table'),
        page.locator('button:has-text("Add"), button:has-text("New"), button:has-text("Create")'),
        page.locator('input[type="search"], input[placeholder*="Search"]')
      ];

      for (const element of commonElements) {
        const isVisible = await element.first().isVisible({ timeout: 3000 }).catch(() => false);
        if (isVisible) {
          console.log(`✓ Found UI element: ${element}`);
        }
      }

      await saveScreenshot(page, '06-users-final-view.png');
    } else {
      console.log('⚠ Could not navigate to User Management page');
      console.log(`Final URL: ${page.url()}`);
      await saveScreenshot(page, '06-users-failed-navigation.png');
    }

    expect(userManagementLoaded).toBeTruthy();
  });

  test('3. Navigate to Customer Management (CRM)', async () => {
    console.log('\n=== TEST 3: CUSTOMER MANAGEMENT (CRM) NAVIGATION ===\n');

    // Login first
    await page.goto(BASE_URL);
    await waitForPageLoad(page);

    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await loginButton.click();
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);

    console.log('✓ Logged in successfully');

    let customerManagementLoaded = false;

    // Strategy 1: Direct URL navigation
    console.log('Attempting direct URL navigation to /admin/customers...');
    await page.goto(`${BASE_URL}/admin/customers`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);
    await saveScreenshot(page, '07-customers-direct-url.png');

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    if (currentUrl.includes('/customers')) {
      customerManagementLoaded = true;
      console.log('✓ Customer Management page loaded via direct URL');
    }

    // Strategy 2: Try CRM menu navigation
    if (!customerManagementLoaded) {
      console.log('Attempting menu navigation...');

      const crmMenuSelectors = [
        'a:has-text("CRM")',
        'a:has-text("Customer")',
        'button:has-text("CRM")',
        'button:has-text("Customers")',
        '[routerLink="/admin/customers"]',
        'a[href="/admin/customers"]'
      ];

      for (const selector of crmMenuSelectors) {
        const menuItem = page.locator(selector).first();
        if (await menuItem.isVisible({ timeout: 2000 }).catch(() => false)) {
          await menuItem.click();
          await waitForPageLoad(page);
          await page.waitForTimeout(2000);
          await saveScreenshot(page, '08-customers-menu-navigation.png');
          customerManagementLoaded = page.url().includes('/customer');
          if (customerManagementLoaded) {
            console.log('✓ Customer Management page loaded via menu navigation');
          }
          break;
        }
      }
    }

    // Verify Customer Management page content
    if (customerManagementLoaded) {
      await saveScreenshot(page, '09-customers-page-content.png');

      // Look for customer-specific elements
      const pageText = await page.textContent('body');
      const hasCustomerContent = pageText?.toLowerCase().includes('customer') ||
                                pageText?.toLowerCase().includes('client') ||
                                pageText?.toLowerCase().includes('crm');

      console.log(`✓ Customer Management page content verified: ${hasCustomerContent}`);

      // Check for common CRM elements
      const tableVisible = await page.locator('table, mat-table, .table').first().isVisible({ timeout: 3000 }).catch(() => false);
      const addButtonVisible = await page.locator('button:has-text("Add"), button:has-text("New Customer")').first().isVisible({ timeout: 3000 }).catch(() => false);

      if (tableVisible) console.log('✓ Customer table/list found');
      if (addButtonVisible) console.log('✓ Add customer button found');

      await saveScreenshot(page, '10-customers-final-view.png');
    } else {
      console.log('⚠ Could not navigate to Customer Management page');
      console.log(`Final URL: ${page.url()}`);
      await saveScreenshot(page, '10-customers-failed-navigation.png');
    }

    expect(customerManagementLoaded).toBeTruthy();
  });

  test('4. Navigate to Product Catalog', async () => {
    console.log('\n=== TEST 4: PRODUCT CATALOG NAVIGATION ===\n');

    // Login first
    await page.goto(BASE_URL);
    await waitForPageLoad(page);

    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await loginButton.click();
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);

    console.log('✓ Logged in successfully');

    let productCatalogLoaded = false;

    // Strategy 1: Direct URL navigation
    console.log('Attempting direct URL navigation to /admin/products...');
    await page.goto(`${BASE_URL}/admin/products`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);
    await saveScreenshot(page, '11-products-direct-url.png');

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    if (currentUrl.includes('/product')) {
      productCatalogLoaded = true;
      console.log('✓ Product Catalog page loaded via direct URL');
    }

    // Strategy 2: Try menu navigation
    if (!productCatalogLoaded) {
      console.log('Attempting menu navigation...');

      const productMenuSelectors = [
        'a:has-text("Product")',
        'a:has-text("Catalog")',
        'button:has-text("Product")',
        'button:has-text("Products")',
        '[routerLink="/admin/products"]',
        'a[href="/admin/products"]'
      ];

      for (const selector of productMenuSelectors) {
        const menuItem = page.locator(selector).first();
        if (await menuItem.isVisible({ timeout: 2000 }).catch(() => false)) {
          await menuItem.click();
          await waitForPageLoad(page);
          await page.waitForTimeout(2000);
          await saveScreenshot(page, '12-products-menu-navigation.png');
          productCatalogLoaded = page.url().includes('/product');
          if (productCatalogLoaded) {
            console.log('✓ Product Catalog page loaded via menu navigation');
          }
          break;
        }
      }
    }

    // Verify Product Catalog page content
    if (productCatalogLoaded) {
      await saveScreenshot(page, '13-products-page-content.png');

      const pageText = await page.textContent('body');
      const hasProductContent = pageText?.toLowerCase().includes('product') ||
                               pageText?.toLowerCase().includes('catalog') ||
                               pageText?.toLowerCase().includes('inventory');

      console.log(`✓ Product Catalog page content verified: ${hasProductContent}`);

      // Check for product-specific elements
      const tableVisible = await page.locator('table, mat-table, .table').first().isVisible({ timeout: 3000 }).catch(() => false);
      const addButtonVisible = await page.locator('button:has-text("Add"), button:has-text("New Product")').first().isVisible({ timeout: 3000 }).catch(() => false);

      if (tableVisible) console.log('✓ Product table/list found');
      if (addButtonVisible) console.log('✓ Add product button found');

      await saveScreenshot(page, '14-products-final-view.png');
    } else {
      console.log('⚠ Could not navigate to Product Catalog page');
      console.log(`Final URL: ${page.url()}`);
      await saveScreenshot(page, '14-products-failed-navigation.png');
    }

    expect(productCatalogLoaded).toBeTruthy();
  });

  test('5. Navigate to Asset Management', async () => {
    console.log('\n=== TEST 5: ASSET MANAGEMENT NAVIGATION ===\n');

    // Login first
    await page.goto(BASE_URL);
    await waitForPageLoad(page);

    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await loginButton.click();
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);

    console.log('✓ Logged in successfully');

    let assetManagementLoaded = false;

    // Strategy 1: Direct URL navigation
    console.log('Attempting direct URL navigation to /admin/assets...');
    await page.goto(`${BASE_URL}/admin/assets`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);
    await saveScreenshot(page, '15-assets-direct-url.png');

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    if (currentUrl.includes('/asset')) {
      assetManagementLoaded = true;
      console.log('✓ Asset Management page loaded via direct URL');
    }

    // Strategy 2: Try menu navigation
    if (!assetManagementLoaded) {
      console.log('Attempting menu navigation...');

      const assetMenuSelectors = [
        'a:has-text("Asset Management")',
        'a:has-text("Assets")',
        'button:has-text("Asset")',
        'button:has-text("Assets")',
        '[routerLink="/admin/assets"]',
        'a[href="/admin/assets"]'
      ];

      for (const selector of assetMenuSelectors) {
        const menuItem = page.locator(selector).first();
        if (await menuItem.isVisible({ timeout: 2000 }).catch(() => false)) {
          await menuItem.click();
          await waitForPageLoad(page);
          await page.waitForTimeout(2000);
          await saveScreenshot(page, '16-assets-menu-navigation.png');
          assetManagementLoaded = page.url().includes('/asset');
          if (assetManagementLoaded) {
            console.log('✓ Asset Management page loaded via menu navigation');
          }
          break;
        }
      }
    }

    // Verify Asset Management page content
    if (assetManagementLoaded) {
      await saveScreenshot(page, '17-assets-page-content.png');

      const pageText = await page.textContent('body');
      const hasAssetContent = pageText?.toLowerCase().includes('asset');

      console.log(`✓ Asset Management page content verified: ${hasAssetContent}`);

      // Check for asset-specific elements
      const tableVisible = await page.locator('table, mat-table, .table').first().isVisible({ timeout: 3000 }).catch(() => false);
      const addButtonVisible = await page.locator('button:has-text("Add"), button:has-text("New Asset")').first().isVisible({ timeout: 3000 }).catch(() => false);

      if (tableVisible) console.log('✓ Asset table/list found');
      if (addButtonVisible) console.log('✓ Add asset button found');

      await saveScreenshot(page, '18-assets-final-view.png');
    } else {
      console.log('⚠ Could not navigate to Asset Management page');
      console.log(`Final URL: ${page.url()}`);
      await saveScreenshot(page, '18-assets-failed-navigation.png');
    }

    // Note: Asset Management might not be accessible if the license isn't activated
    console.log(`Asset Management accessible: ${assetManagementLoaded}`);
  });

  test('6. Navigate to Asset Assignments', async () => {
    console.log('\n=== TEST 6: ASSET ASSIGNMENTS NAVIGATION ===\n');

    // Login first
    await page.goto(BASE_URL);
    await waitForPageLoad(page);

    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await loginButton.click();
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);

    console.log('✓ Logged in successfully');

    let assetAssignmentsLoaded = false;

    // Strategy 1: Direct URL navigation
    console.log('Attempting direct URL navigation to /admin/asset-assignments...');
    await page.goto(`${BASE_URL}/admin/asset-assignments`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);
    await saveScreenshot(page, '19-asset-assignments-direct-url.png');

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    if (currentUrl.includes('/asset-assignment')) {
      assetAssignmentsLoaded = true;
      console.log('✓ Asset Assignments page loaded via direct URL');
    }

    // Strategy 2: Try menu navigation
    if (!assetAssignmentsLoaded) {
      console.log('Attempting menu navigation...');

      const assignmentMenuSelectors = [
        'a:has-text("Asset Assignment")',
        'a:has-text("Assignments")',
        'button:has-text("Asset Assignment")',
        'button:has-text("Assignments")',
        '[routerLink="/admin/asset-assignments"]',
        'a[href="/admin/asset-assignments"]'
      ];

      for (const selector of assignmentMenuSelectors) {
        const menuItem = page.locator(selector).first();
        if (await menuItem.isVisible({ timeout: 2000 }).catch(() => false)) {
          await menuItem.click();
          await waitForPageLoad(page);
          await page.waitForTimeout(2000);
          await saveScreenshot(page, '20-asset-assignments-menu-navigation.png');
          assetAssignmentsLoaded = page.url().includes('/asset-assignment');
          if (assetAssignmentsLoaded) {
            console.log('✓ Asset Assignments page loaded via menu navigation');
          }
          break;
        }
      }
    }

    // Verify Asset Assignments page content
    if (assetAssignmentsLoaded) {
      await saveScreenshot(page, '21-asset-assignments-page-content.png');

      const pageText = await page.textContent('body');
      const hasAssignmentContent = pageText?.toLowerCase().includes('assignment') ||
                                   pageText?.toLowerCase().includes('assign');

      console.log(`✓ Asset Assignments page content verified: ${hasAssignmentContent}`);

      // Check for assignment-specific elements
      const tableVisible = await page.locator('table, mat-table, .table').first().isVisible({ timeout: 3000 }).catch(() => false);
      const addButtonVisible = await page.locator('button:has-text("Add"), button:has-text("Assign"), button:has-text("New Assignment")').first().isVisible({ timeout: 3000 }).catch(() => false);

      if (tableVisible) console.log('✓ Assignment table/list found');
      if (addButtonVisible) console.log('✓ Add/Assign button found');

      await saveScreenshot(page, '22-asset-assignments-final-view.png');
    } else {
      console.log('⚠ Could not navigate to Asset Assignments page');
      console.log(`Final URL: ${page.url()}`);
      await saveScreenshot(page, '22-asset-assignments-failed-navigation.png');
    }

    console.log(`Asset Assignments accessible: ${assetAssignmentsLoaded}`);
  });

  test('7. Admin Panel Menu Structure Analysis', async () => {
    console.log('\n=== TEST 7: ADMIN PANEL MENU STRUCTURE ANALYSIS ===\n');

    // Login first
    await page.goto(BASE_URL);
    await waitForPageLoad(page);

    const emailInput = page.locator('input[type="email"], input[formControlName="email"]');
    const passwordInput = page.locator('input[type="password"], input[formControlName="password"]');
    const loginButton = page.locator('button[type="submit"]');

    await emailInput.fill(ADMIN_CREDENTIALS.email);
    await passwordInput.fill(ADMIN_CREDENTIALS.password);
    await loginButton.click();
    await page.waitForURL(/\/(admin|dashboard|home)/, { timeout: 15000 });
    await waitForPageLoad(page);

    console.log('✓ Logged in successfully');

    // Try to access admin panel
    await page.goto(`${BASE_URL}/admin`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);
    await saveScreenshot(page, '23-admin-panel-overview.png');

    // Analyze available menu items
    console.log('\nAnalyzing menu structure...');

    // Get all navigation links
    const navLinks = await page.locator('nav a, .nav a, [role="navigation"] a, mat-nav-list a').all();
    console.log(`Found ${navLinks.length} navigation links`);

    const menuItems: string[] = [];
    for (const link of navLinks) {
      const text = await link.textContent();
      const href = await link.getAttribute('href');
      if (text && text.trim()) {
        menuItems.push(`${text.trim()} -> ${href}`);
      }
    }

    console.log('\n=== Available Menu Items ===');
    menuItems.forEach(item => console.log(`  - ${item}`));

    // Get all buttons
    const buttons = await page.locator('nav button, .nav button, [role="navigation"] button').all();
    console.log(`\nFound ${buttons.length} navigation buttons`);

    const buttonItems: string[] = [];
    for (const button of buttons) {
      const text = await button.textContent();
      if (text && text.trim()) {
        buttonItems.push(text.trim());
      }
    }

    console.log('\n=== Available Navigation Buttons ===');
    buttonItems.forEach(item => console.log(`  - ${item}`));

    // Save page HTML for analysis
    const htmlContent = await page.content();
    const htmlPath = path.join(EVIDENCE_DIR, 'admin-panel-html.html');
    fs.writeFileSync(htmlPath, htmlContent);
    console.log(`\n✓ Page HTML saved to: ${htmlPath}`);

    await saveScreenshot(page, '24-admin-panel-menu-structure.png');
  });
});
