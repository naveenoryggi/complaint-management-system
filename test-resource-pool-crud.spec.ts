import { test, expect, Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

// Test configuration
const BASE_URL = 'http://localhost:4200';
const ADMIN_EMAIL = 'admin@complaintmanagement.com';
const ADMIN_PASSWORD = 'Admin@123';
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots', 'resource-pool-crud');

// Test data
const TEST_POOL = {
  name: 'Test Support Pool',
  description: 'E2E Testing Pool',
  poolType: 'Custom',
  updatedDescription: 'Updated E2E Testing Pool'
};

// Helper function to take screenshots
async function takeScreenshot(page: Page, name: string) {
  const screenshotPath = path.join(SCREENSHOT_DIR, `${Date.now()}-${name}.png`);
  if (!fs.existsSync(SCREENSHOT_DIR)) {
    fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
  }
  await page.screenshot({ path: screenshotPath, fullPage: true });
  console.log(`Screenshot saved: ${screenshotPath}`);
  return screenshotPath;
}

// Helper function to wait for network idle
async function waitForNetworkIdle(page: Page) {
  await page.waitForLoadState('networkidle');
  await page.waitForTimeout(500); // Extra buffer
}

test.describe('Resource Pool Management - CRUD Operations E2E Test', () => {
  let page: Page;

  test.beforeAll(async ({ browser }) => {
    page = await browser.newPage();
    // Enable console logging
    page.on('console', msg => {
      console.log(`BROWSER CONSOLE [${msg.type()}]:`, msg.text());
    });
    // Enable error logging
    page.on('pageerror', error => {
      console.error('BROWSER ERROR:', error);
    });
  });

  test.afterAll(async () => {
    await page.close();
  });

  test('1. Navigate to application and capture login page', async () => {
    await page.goto(BASE_URL);
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '01-login-page');

    // Verify login page elements
    await expect(page.locator('input[type="email"]')).toBeVisible();
    await expect(page.locator('input[type="password"]')).toBeVisible();
  });

  test('2. Authenticate with admin credentials', async () => {
    await page.fill('input[type="email"]', ADMIN_EMAIL);
    await page.fill('input[type="password"]', ADMIN_PASSWORD);
    await takeScreenshot(page, '02-login-filled');

    await page.click('button[type="submit"]');
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '03-dashboard-after-login');
  });

  test('3. Navigate to Resource Pool Management', async () => {
    // Click on Admin menu or navigate directly
    await page.goto(`${BASE_URL}/admin/resource-pools`);
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '04-resource-pool-page-initial');

    // Verify page header
    await expect(page.locator('h1:has-text("Resource Pool Management")')).toBeVisible();
  });

  test('4. READ - Verify page layout and elements', async () => {
    // Verify header
    await expect(page.locator('.page-header h1')).toContainText('Resource Pool Management');
    await expect(page.locator('.page-description')).toBeVisible();

    // Verify info banner
    await expect(page.locator('.info-banner h3')).toContainText('About Resource Pools');
    await expect(page.locator('.info-banner')).toBeVisible();

    // Verify search bar
    await expect(page.locator('.search-input')).toBeVisible();
    await expect(page.locator('.search-input')).toHaveAttribute('placeholder', /Search resource pools/i);

    // Verify "Add Resource Pool" button
    await expect(page.locator('button:has-text("Add Resource Pool")')).toBeVisible();

    // Verify "Show Active Only" toggle
    await expect(page.locator('.toggle-text:has-text("Show Active Only")')).toBeVisible();

    await takeScreenshot(page, '05-page-layout-verified');
  });

  test('5. READ - Test search functionality', async () => {
    // Get initial pool count
    const initialPools = await page.locator('.pool-card').count();
    console.log(`Initial pool count: ${initialPools}`);

    // Test search with valid term
    if (initialPools > 0) {
      const firstPoolName = await page.locator('.pool-name').first().textContent();
      console.log(`First pool name: ${firstPoolName}`);

      await page.fill('.search-input', firstPoolName || '');
      await page.waitForTimeout(500);
      await takeScreenshot(page, '06-search-valid');

      const filteredCount = await page.locator('.pool-card').count();
      console.log(`Filtered count: ${filteredCount}`);
      expect(filteredCount).toBeGreaterThan(0);
    }

    // Test search with non-existent term
    await page.fill('.search-input', 'NonExistentPoolXYZ123');
    await page.waitForTimeout(500);
    await takeScreenshot(page, '07-search-no-results');

    const noResults = await page.locator('.no-results').isVisible();
    expect(noResults).toBeTruthy();

    // Clear search
    await page.fill('.search-input', '');
    await page.waitForTimeout(500);
  });

  test('6. READ - Test "Show Active Only" toggle', async () => {
    // Check current state
    const toggleInput = page.locator('input[type="checkbox"]').first();
    const isChecked = await toggleInput.isChecked();
    console.log(`Toggle is initially checked: ${isChecked}`);

    // Toggle off
    if (isChecked) {
      await toggleInput.click();
      await page.waitForTimeout(500);
      await takeScreenshot(page, '08-toggle-show-all');
    }

    // Toggle on
    await toggleInput.click();
    await page.waitForTimeout(500);
    await takeScreenshot(page, '09-toggle-active-only');
  });

  test('7. CREATE - Open form and verify fields', async () => {
    await page.click('button:has-text("Add Resource Pool")');
    await page.waitForTimeout(500);
    await takeScreenshot(page, '10-create-modal-opened');

    // Verify modal is visible
    await expect(page.locator('.modal-overlay')).toBeVisible();
    await expect(page.locator('.modal-header h2')).toContainText('Create New Resource Pool');

    // Verify form fields
    await expect(page.locator('#poolName')).toBeVisible();
    await expect(page.locator('#poolDescription')).toBeVisible();
    await expect(page.locator('#poolType')).toBeVisible();

    // Verify required labels
    await expect(page.locator('label[for="poolName"]')).toContainText('*');
    await expect(page.locator('label[for="poolType"]')).toContainText('*');
  });

  test('8. CREATE - Test form validation', async () => {
    // Try to submit empty form
    await page.click('button:has-text("Create")');
    await page.waitForTimeout(500);
    await takeScreenshot(page, '11-validation-empty-form');

    // Should show error message
    const errorVisible = await page.locator('.alert-error').isVisible();
    expect(errorVisible).toBeTruthy();

    // Fill only name, missing pool type validation
    await page.fill('#poolName', 'Test');
    await page.selectOption('#poolType', ''); // Try empty pool type
    await page.click('button:has-text("Create")');
    await page.waitForTimeout(500);
    await takeScreenshot(page, '12-validation-partial-form');
  });

  test('9. CREATE - Create new pool with valid data', async () => {
    // Fill the form
    await page.fill('#poolName', TEST_POOL.name);
    await page.fill('#poolDescription', TEST_POOL.description);
    await page.selectOption('#poolType', 'Custom');
    await takeScreenshot(page, '13-create-form-filled');

    // Submit the form
    await page.click('button:has-text("Create")');
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '14-create-pool-submitted');

    // Verify success message
    await expect(page.locator('.alert-success')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.alert-success')).toContainText('created successfully');
  });

  test('10. CREATE - Verify new pool appears in list', async () => {
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '15-pool-list-after-create');

    // Search for the newly created pool
    await page.fill('.search-input', TEST_POOL.name);
    await page.waitForTimeout(500);
    await takeScreenshot(page, '16-search-new-pool');

    // Verify it exists
    const poolCard = page.locator('.pool-card', { hasText: TEST_POOL.name });
    await expect(poolCard).toBeVisible();

    // Verify pool details
    await expect(poolCard.locator('.pool-name')).toContainText(TEST_POOL.name);
    await expect(poolCard.locator('.pool-description')).toContainText(TEST_POOL.description);
    await expect(poolCard.locator('.pool-type-badge')).toContainText('Custom');
    await expect(poolCard.locator('.status-badge')).toContainText('Active');

    // Clear search
    await page.fill('.search-input', '');
    await page.waitForTimeout(500);
  });

  test('11. UPDATE - Open edit form and verify pre-populated data', async () => {
    // Find the test pool
    await page.fill('.search-input', TEST_POOL.name);
    await page.waitForTimeout(500);

    const poolCard = page.locator('.pool-card', { hasText: TEST_POOL.name });
    await expect(poolCard).toBeVisible();

    // Click edit button
    await poolCard.locator('button:has-text("Edit")').click();
    await page.waitForTimeout(500);
    await takeScreenshot(page, '17-edit-modal-opened');

    // Verify modal title
    await expect(page.locator('.modal-header h2')).toContainText('Edit Resource Pool');

    // Verify pre-populated data
    const nameValue = await page.locator('#poolName').inputValue();
    const descValue = await page.locator('#poolDescription').inputValue();
    const typeValue = await page.locator('#poolType').inputValue();

    expect(nameValue).toBe(TEST_POOL.name);
    expect(descValue).toBe(TEST_POOL.description);
    expect(typeValue).toBe('Custom');

    await takeScreenshot(page, '18-edit-form-prepopulated');
  });

  test('12. UPDATE - Modify description and toggle active status', async () => {
    // Update description
    await page.fill('#poolDescription', TEST_POOL.updatedDescription);

    // Toggle active status
    const activeCheckbox = page.locator('input[name="isActive"]');
    const isChecked = await activeCheckbox.isChecked();
    await activeCheckbox.click();

    await takeScreenshot(page, '19-edit-form-modified');

    // Submit update
    await page.click('button:has-text("Update")');
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '20-update-submitted');

    // Verify success message
    await expect(page.locator('.alert-success')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.alert-success')).toContainText('updated successfully');
  });

  test('13. UPDATE - Verify changes are saved', async () => {
    await page.waitForTimeout(1000);

    // Toggle to show all pools (including inactive)
    const toggleInput = page.locator('input[type="checkbox"]').first();
    if (await toggleInput.isChecked()) {
      await toggleInput.click();
      await page.waitForTimeout(500);
    }

    // Search for the pool
    await page.fill('.search-input', TEST_POOL.name);
    await page.waitForTimeout(500);
    await takeScreenshot(page, '21-verify-updates');

    const poolCard = page.locator('.pool-card', { hasText: TEST_POOL.name });
    await expect(poolCard).toBeVisible();

    // Verify updated description
    await expect(poolCard.locator('.pool-description')).toContainText(TEST_POOL.updatedDescription);

    // Verify inactive status
    await expect(poolCard.locator('.status-badge')).toContainText('Inactive');
    await expect(poolCard).toHaveClass(/inactive/);
  });

  test('14. MEMBERS - Test add member functionality', async () => {
    // First, toggle the pool back to active so we can add members
    const poolCard = page.locator('.pool-card', { hasText: TEST_POOL.name });
    await poolCard.locator('button:has-text("Edit")').click();
    await page.waitForTimeout(500);

    const activeCheckbox = page.locator('input[name="isActive"]');
    if (!await activeCheckbox.isChecked()) {
      await activeCheckbox.click();
    }
    await page.click('button:has-text("Update")');
    await waitForNetworkIdle(page);
    await page.waitForTimeout(1000);

    // Now try to add a member
    await page.fill('.search-input', TEST_POOL.name);
    await page.waitForTimeout(500);

    const activePoolCard = page.locator('.pool-card', { hasText: TEST_POOL.name });
    await activePoolCard.locator('button:has-text("Add Member")').click();
    await page.waitForTimeout(500);
    await takeScreenshot(page, '22-add-member-modal-opened');

    // Verify member modal
    await expect(page.locator('.modal-header h2')).toContainText('Add Members to');

    // Check if user autocomplete is present
    const autocompletePresent = await page.locator('app-user-autocomplete').isVisible();
    console.log(`User autocomplete component present: ${autocompletePresent}`);

    await takeScreenshot(page, '23-add-member-form');
  });

  test('15. MEMBERS - Attempt to add member (autocomplete test)', async () => {
    // This test will document the member addition interface
    // Note: Actual member addition requires searching for users

    // Check for empty state
    const emptyState = await page.locator('.empty-selection').isVisible();
    console.log(`Empty selection state visible: ${emptyState}`);

    if (emptyState) {
      await expect(page.locator('.empty-selection')).toContainText('No users selected');
    }

    await takeScreenshot(page, '24-member-selection-empty-state');

    // Close the modal
    await page.click('button:has-text("Cancel")');
    await page.waitForTimeout(500);
  });

  test('16. DELETE - Open delete confirmation', async () => {
    // Search for the test pool
    await page.fill('.search-input', TEST_POOL.name);
    await page.waitForTimeout(500);

    const poolCard = page.locator('.pool-card', { hasText: TEST_POOL.name });
    await expect(poolCard).toBeVisible();

    // Click delete button
    await poolCard.locator('button .material-icons-round:has-text("delete")').click();
    await page.waitForTimeout(500);
    await takeScreenshot(page, '25-delete-confirmation-modal');

    // Verify delete confirmation modal
    await expect(page.locator('.modal-header-danger h2')).toContainText('Delete Resource Pool');
    await expect(page.locator('.delete-message')).toBeVisible();
    await expect(page.locator('.pool-name-highlight')).toContainText(TEST_POOL.name);
    await expect(page.locator('.warning-text')).toContainText('This action cannot be undone');
  });

  test('17. DELETE - Confirm deletion', async () => {
    // Click delete button
    await page.click('button.btn-danger:has-text("Delete")');
    await waitForNetworkIdle(page);
    await takeScreenshot(page, '26-after-delete');

    // Verify success message
    await expect(page.locator('.alert-success')).toBeVisible({ timeout: 10000 });
    await expect(page.locator('.alert-success')).toContainText('deleted successfully');
  });

  test('18. DELETE - Verify pool is removed', async () => {
    await page.waitForTimeout(1000);

    // Search for the deleted pool
    await page.fill('.search-input', TEST_POOL.name);
    await page.waitForTimeout(500);
    await takeScreenshot(page, '27-verify-deletion');

    // Should show no results
    const noResults = await page.locator('.no-results').isVisible();
    expect(noResults).toBeTruthy();
    await expect(page.locator('.no-results h3')).toContainText('No Resource Pools Found');

    // Clear search
    await page.fill('.search-input', '');
    await page.waitForTimeout(500);
    await takeScreenshot(page, '28-final-state');
  });

  test('19. Collect console logs and network activity', async () => {
    console.log('=== TEST EXECUTION COMPLETE ===');
    console.log(`Screenshots saved to: ${SCREENSHOT_DIR}`);

    // Take final screenshot
    await takeScreenshot(page, '29-test-complete');
  });
});
