const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage({ viewport: { width: 1920, height: 1080 } });

  try {
    // Navigate to login
    console.log('Navigating to login...');
    await page.goto('http://localhost:4200/login');
    await page.waitForTimeout(2000);

    // Login with correct credentials
    console.log('Logging in...');
    await page.fill('input[placeholder*="Employee ID"]', 'admin@complaintmanagement.com');
    await page.fill('input[placeholder*="password"]', 'Admin@123');
    await page.click('button:has-text("Sign In")');

    // Wait for navigation
    await page.waitForTimeout(4000);
    console.log('Logged in');

    // Take screenshot to verify login
    await page.screenshot({ path: 'test-screenshots/after-login.png', fullPage: true });

    // Navigate to sections
    console.log('Navigating to sections...');
    await page.goto('http://localhost:4200/admin/sections');
    await page.waitForTimeout(3000);

    // Take screenshot before clicking
    await page.screenshot({ path: 'test-screenshots/section-page.png', fullPage: true });
    console.log('Screenshot saved: section-page.png');

    // Find and click Add Section button
    console.log('Looking for Add Section button...');
    const addBtn = await page.$('button.btn-add, button:has-text("Add Section")');
    if (addBtn) {
      await addBtn.click();
      await page.waitForTimeout(1500);

      // Take screenshot after clicking
      await page.screenshot({ path: 'test-screenshots/section-modal.png', fullPage: true });
      console.log('Screenshot saved: section-modal.png');

      // Check modal position
      const modal = await page.$('.modal-content');
      if (modal) {
        const box = await modal.boundingBox();
        console.log('Modal bounding box:', JSON.stringify(box));
      }
    } else {
      console.log('Add Section button not found');
    }

    // Keep browser open
    console.log('Done. Keeping browser open for 20 seconds...');
    await page.waitForTimeout(20000);

  } catch (error) {
    console.error('Error:', error.message);
    await page.screenshot({ path: 'test-screenshots/error.png', fullPage: true });
  } finally {
    await browser.close();
  }
})();
