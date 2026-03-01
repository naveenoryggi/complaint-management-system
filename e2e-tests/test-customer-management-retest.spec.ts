import { test, expect, Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

const BASE_URL = 'http://localhost:4200';
const EVIDENCE_DIR = 'test-evidence/admin-panel-validation';

const ADMIN_CREDENTIALS = {
  email: 'admin@complaintmanagement.com',
  password: 'Admin@123'
};

async function saveScreenshot(page: Page, filename: string) {
  const screenshotPath = path.join(EVIDENCE_DIR, filename);
  await page.screenshot({ path: screenshotPath, fullPage: true });
  console.log(`Screenshot saved: ${screenshotPath}`);
}

async function waitForPageLoad(page: Page, timeout = 5000) {
  try {
    await page.waitForLoadState('networkidle', { timeout });
  } catch (e) {
    await page.waitForLoadState('domcontentloaded');
  }
}

test.describe('Customer Management Re-Test with Correct URL', () => {
  let page: Page;

  test.beforeAll(async () => {
    if (!fs.existsSync(EVIDENCE_DIR)) {
      fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
    }
  });

  test.beforeEach(async ({ browser }) => {
    page = await browser.newPage();
    await page.setViewportSize({ width: 1920, height: 1080 });
  });

  test.afterEach(async () => {
    await page.close();
  });

  test('Navigate to Customer Management via /admin/crm', async () => {
    console.log('\n=== CUSTOMER MANAGEMENT RE-TEST - CORRECT URL ===\n');

    // Login
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

    // Navigate to correct URL: /admin/crm
    console.log('Navigating to /admin/crm...');
    await page.goto(`${BASE_URL}/admin/crm`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    await saveScreenshot(page, '25-customer-management-correct-url.png');

    // Verify we're on the customer management page
    const customerManagementLoaded = currentUrl.includes('/crm');
    console.log(`Customer Management loaded: ${customerManagementLoaded}`);

    if (customerManagementLoaded) {
      console.log('✅ SUCCESS: Customer Management page accessible via /admin/crm');

      // Check page content
      const pageText = await page.textContent('body');
      const hasCustomerContent = pageText?.toLowerCase().includes('customer') ||
                                pageText?.toLowerCase().includes('crm');

      console.log(`✓ Page contains customer/CRM content: ${hasCustomerContent}`);

      // Look for table
      const tableVisible = await page.locator('table, mat-table, .table').first().isVisible({ timeout: 3000 }).catch(() => false);
      if (tableVisible) {
        console.log('✓ Customer table/list found');

        // Count rows
        const rowCount = await page.locator('table tbody tr, mat-row').count();
        console.log(`✓ Customer records visible: ${rowCount}`);
      }

      // Look for add button
      const addButtonVisible = await page.locator('button:has-text("Add"), button:has-text("New Customer"), button:has-text("Create")').first().isVisible({ timeout: 3000 }).catch(() => false);
      if (addButtonVisible) {
        console.log('✓ Add customer button found');
      }

      // Look for search
      const searchVisible = await page.locator('input[type="search"], input[placeholder*="Search"]').first().isVisible({ timeout: 3000 }).catch(() => false);
      if (searchVisible) {
        console.log('✓ Search functionality found');
      }

      await saveScreenshot(page, '26-customer-management-final-verification.png');

      // Check browser console for errors
      const consoleMessages: string[] = [];
      page.on('console', msg => {
        if (msg.type() === 'error') {
          consoleMessages.push(`Console Error: ${msg.text()}`);
        }
      });

      await page.waitForTimeout(2000);

      if (consoleMessages.length > 0) {
        console.log('\n⚠️ Console Errors Detected:');
        consoleMessages.forEach(msg => console.log(`  - ${msg}`));
      } else {
        console.log('\n✓ No console errors detected');
      }

      expect(customerManagementLoaded).toBeTruthy();
    } else {
      console.log('❌ FAILED: Still cannot access Customer Management');
      console.log(`Redirected to: ${currentUrl}`);

      await saveScreenshot(page, '26-customer-management-failed-again.png');

      // Check if there's a permission issue
      const pageText = await page.textContent('body');
      if (pageText?.toLowerCase().includes('permission') || pageText?.toLowerCase().includes('unauthorized')) {
        console.log('⚠️ Permission/Authorization issue detected');
      }

      if (pageText?.toLowerCase().includes('license') || pageText?.toLowerCase().includes('module')) {
        console.log('⚠️ License/Module activation issue detected');
      }

      expect(customerManagementLoaded).toBeTruthy();
    }
  });

  test('Test alternative route: /crm/customers', async () => {
    console.log('\n=== TESTING ALTERNATIVE ROUTE: /crm/customers ===\n');

    // Login
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

    // Try alternative route
    console.log('Navigating to /crm/customers...');
    await page.goto(`${BASE_URL}/crm/customers`);
    await waitForPageLoad(page);
    await page.waitForTimeout(2000);

    const currentUrl = page.url();
    console.log(`Current URL: ${currentUrl}`);

    await saveScreenshot(page, '27-customer-management-alternative-route.png');

    const alternativeRouteWorks = currentUrl.includes('/crm/customers');
    console.log(`Alternative route accessible: ${alternativeRouteWorks}`);

    if (alternativeRouteWorks) {
      console.log('✅ Alternative route /crm/customers works!');
    } else {
      console.log('⚠️ Alternative route redirected to: ' + currentUrl);
    }
  });
});
