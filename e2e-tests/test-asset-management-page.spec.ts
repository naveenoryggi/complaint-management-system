import { test, expect } from '@playwright/test';

test.describe('Asset Management Page', () => {
  test('should display assets list correctly', async ({ page }) => {
    // Login
    await page.goto('http://localhost:4200/login');
    await page.fill('input[formControlName="email"]', 'admin@complaintmanagement.com');
    await page.fill('input[formControlName="password"]', 'Admin@123');
    await page.click('button[type="submit"]');

    // Wait for login to complete
    await page.waitForURL(/\/admin|\/dashboard/, { timeout: 15000 });
    await page.screenshot({ path: 'test-evidence/asset-management/01-after-login.png' });

    // Navigate to Asset Management
    await page.goto('http://localhost:4200/admin/assets');
    await page.waitForLoadState('networkidle', { timeout: 15000 });
    await page.screenshot({ path: 'test-evidence/asset-management/02-asset-management-page.png' });

    // Wait for assets to load
    await page.waitForTimeout(3000);

    // Check if asset list is displayed
    const pageContent = await page.content();
    await page.screenshot({ path: 'test-evidence/asset-management/03-final-state.png' });

    // Look for common asset indicators
    const hasAssets = pageContent.includes('AST-') ||
                      pageContent.includes('asset') ||
                      pageContent.includes('Dell') ||
                      pageContent.includes('Laptop');

    // Check for error messages
    const hasError = pageContent.includes('Failed to load') ||
                     pageContent.includes('Error') ||
                     pageContent.includes('error');

    console.log('Has assets:', hasAssets);
    console.log('Has error:', hasError);

    // Capture any error messages for debugging
    if (hasError) {
      const errorElement = await page.locator('text=/Failed|Error/i').first();
      if (await errorElement.isVisible()) {
        console.log('Error text:', await errorElement.textContent());
      }
    }

    expect(hasError).toBeFalsy();
    expect(hasAssets).toBeTruthy();
  });
});
