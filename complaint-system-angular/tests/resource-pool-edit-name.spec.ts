import { test, expect } from '@playwright/test';

test.describe('Resource Pool Edit - Name Field Pre-population', () => {
  test('should pre-populate name field when editing existing resource pool', async ({ page }) => {
    // Step 1: Navigate to login page
    await page.goto('http://localhost:4200/login');
    await page.waitForLoadState('networkidle');

    // Step 2: Fill in credentials manually based on test credentials shown
    const usernameInput = page.locator('input').first();
    const passwordInput = page.locator('input').nth(1);

    // Use the admin credentials from the test credentials section shown on login page
    await usernameInput.fill('admin@complaintmanagement.com');
    await passwordInput.fill('Admin@123');

    // Wait a moment for form validation
    await page.waitForTimeout(500);

    // Click sign in
    await page.click('button:has-text("Sign In")');

    // Wait for navigation
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);

    // Verify we're logged in
    const currentUrl = page.url();
    console.log('Current URL after login:', currentUrl);

    if (currentUrl.includes('/login')) {
      // Take a screenshot to see what happened
      await page.screenshot({
        path: 'C:\\Users\\Navin Chandra\\Pictures\\Complaint management system\\test-results\\login-failed.png',
        fullPage: true
      });
      throw new Error('Login failed - still on login page');
    }

    // Step 3: Navigate to Resource Pools page
    await page.goto('http://localhost:4200/admin/resource-pools');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Take screenshot of the resource pools list
    await page.screenshot({
      path: 'C:\\Users\\Navin Chandra\\Pictures\\Complaint management system\\test-results\\resource-pools-list.png',
      fullPage: true
    });

    // Step 4: Find and click Edit button on the first existing pool
    // Look for Edit buttons in the table
    const editButtons = page.locator('button:has-text("Edit"), button[aria-label*="Edit"], mat-icon:has-text("edit")').first();

    // Check if any resource pools exist
    const poolsExist = await editButtons.count() > 0;

    if (!poolsExist) {
      console.log('No existing resource pools found. Creating one first...');

      // Click Add/Create button
      const addButton = page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New")').first();
      await addButton.click();
      await page.waitForTimeout(1000);

      // Fill in new pool details
      await page.fill('input[formControlName="name"], input[name="name"]', 'Test Resource Pool');
      await page.fill('textarea[formControlName="description"], textarea[name="description"]', 'Test pool for edit verification');

      // Save the new pool
      const saveButton = page.locator('button:has-text("Save"), button:has-text("Create"), button[type="submit"]').first();
      await saveButton.click();
      await page.waitForTimeout(2000);

      // Now click edit on this newly created pool
      await page.locator('button:has-text("Edit"), button[aria-label*="Edit"], mat-icon:has-text("edit")').first().click();
    } else {
      // Click edit on existing pool
      await editButtons.click();
    }

    // Wait for modal/form to appear
    await page.waitForTimeout(2000);

    // Step 5: Take screenshot of edit modal
    await page.screenshot({
      path: 'C:\\Users\\Navin Chandra\\Pictures\\Complaint management system\\test-results\\resource-pool-edit-modal.png',
      fullPage: true
    });

    // Step 6: Verify Name field is pre-populated
    const nameInput = page.locator('input[formControlName="name"], input[name="name"]').first();

    // Wait for the input to be visible
    await nameInput.waitFor({ state: 'visible', timeout: 5000 });

    // Get the value of the name field
    const nameValue = await nameInput.inputValue();

    // Log the value for debugging
    console.log('Name field value:', nameValue);

    // Take a focused screenshot of the form
    const formElement = page.locator('form, mat-dialog-content, .modal-body').first();
    await formElement.screenshot({
      path: 'C:\\Users\\Navin Chandra\\Pictures\\Complaint management system\\test-results\\resource-pool-edit-form-focused.png'
    });

    // Step 7: Verify the name is NOT empty
    expect(nameValue).not.toBe('');
    expect(nameValue.length).toBeGreaterThan(0);

    console.log('✓ SUCCESS: Name field is pre-populated with value:', nameValue);

    // Additional verification: Check if other fields are also populated
    const descriptionInput = page.locator('textarea[formControlName="description"], textarea[name="description"]').first();
    const descriptionValue = await descriptionInput.inputValue().catch(() => '');

    console.log('Description field value:', descriptionValue || '(empty)');

    // Log all form field values for comprehensive verification
    const allInputs = await page.locator('input, textarea, select').all();
    console.log('\nAll form fields:');
    for (const input of allInputs) {
      const tagName = await input.evaluate(el => el.tagName);
      const type = await input.getAttribute('type');
      const name = await input.getAttribute('name') || await input.getAttribute('formControlName');
      const value = await input.inputValue().catch(() => '');
      console.log(`- ${tagName}${type ? `[type="${type}"]` : ''}${name ? ` (${name})` : ''}: "${value}"`);
    }
  });
});
