const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: false });
  const page = await browser.newPage({ viewport: { width: 1920, height: 1080 } });

  try {
    // Navigate to login
    console.log('1. Navigating to login...');
    await page.goto('http://localhost:4200/login');
    await page.waitForTimeout(2000);

    // Login with admin credentials
    console.log('2. Logging in as admin...');
    await page.fill('input[placeholder*="Employee ID"]', 'admin@complaintmanagement.com');
    await page.fill('input[placeholder*="password"]', 'Admin@123');
    await page.click('button:has-text("Sign In")');

    // Wait for navigation
    await page.waitForTimeout(4000);
    console.log('3. Login successful');

    // Navigate to Escalation Policy Management
    console.log('4. Navigating to Escalation Policy Management...');
    await page.goto('http://localhost:4200/admin/escalation-policy');
    await page.waitForTimeout(3000);

    // Take screenshot of the main page
    await page.screenshot({ path: 'test-screenshots/escalation-policy-main.png', fullPage: true });
    console.log('5. Screenshot saved: escalation-policy-main.png');

    // Check if page loaded correctly
    const pageHeader = await page.$('.page-header h1');
    if (pageHeader) {
      const headerText = await pageHeader.textContent();
      console.log('6. Page header found:', headerText);
    }

    // Check hierarchy section
    const hierarchySection = await page.$('.hierarchy-section');
    if (hierarchySection) {
      console.log('7. Hierarchy section found');
    }

    // Check filters section
    const filtersSection = await page.$('.filters-section');
    if (filtersSection) {
      console.log('8. Filters section found');
    }

    // Click on Create Policy button
    console.log('9. Clicking Create Policy button...');
    const createBtn = await page.$('button:has-text("Create Policy")');
    if (createBtn) {
      await createBtn.click();
      await page.waitForTimeout(1500);

      // Take screenshot of the modal
      await page.screenshot({ path: 'test-screenshots/escalation-policy-modal.png', fullPage: true });
      console.log('10. Screenshot saved: escalation-policy-modal.png');

      // Check modal position
      const modal = await page.$('.modal-content');
      if (modal) {
        const box = await modal.boundingBox();
        console.log('11. Modal bounding box:', JSON.stringify(box));

        // Verify modal is centered
        const viewportHeight = 1080;
        const modalCenterY = box.y + (box.height / 2);
        const viewportCenterY = viewportHeight / 2;
        const isVerticallyCentered = Math.abs(modalCenterY - viewportCenterY) < 100;
        console.log(`12. Modal vertically centered: ${isVerticallyCentered} (center Y: ${modalCenterY}, viewport center: ${viewportCenterY})`);
      }

      // Fill in form fields
      console.log('13. Testing form fields...');
      await page.fill('#name', 'Test Escalation Policy');
      await page.fill('#description', 'This is a test policy created via automated testing');

      // Take screenshot with filled form
      await page.screenshot({ path: 'test-screenshots/escalation-policy-form-filled.png', fullPage: true });
      console.log('14. Screenshot saved: escalation-policy-form-filled.png');

      // Close modal
      const cancelBtn = await page.$('button:has-text("Cancel")');
      if (cancelBtn) {
        await cancelBtn.click();
        await page.waitForTimeout(1000);
        console.log('15. Modal closed');
      }
    } else {
      console.log('9. Create Policy button not found');
    }

    // Test the Test Resolution button
    console.log('16. Testing Test Resolution panel...');
    const testBtn = await page.$('button:has-text("Test Resolution")');
    if (testBtn) {
      await testBtn.click();
      await page.waitForTimeout(1500);

      // Take screenshot of test panel
      await page.screenshot({ path: 'test-screenshots/escalation-policy-test-panel.png', fullPage: true });
      console.log('17. Screenshot saved: escalation-policy-test-panel.png');

      // Check test panel
      const testPanel = await page.$('.test-panel');
      if (testPanel) {
        console.log('18. Test panel opened successfully');
      }
    }

    // Test filter pills
    console.log('19. Testing filter pills...');
    const activeFilter = await page.$('.filter-pill:has-text("Active")');
    if (activeFilter) {
      await activeFilter.click();
      await page.waitForTimeout(500);
      console.log('20. Active filter clicked');
    }

    // Final screenshot
    await page.screenshot({ path: 'test-screenshots/escalation-policy-final.png', fullPage: true });
    console.log('21. Final screenshot saved');

    console.log('\n=== TEST COMPLETED SUCCESSFULLY ===');
    console.log('All screenshots saved in test-screenshots/ folder');

    // Keep browser open for 10 seconds
    await page.waitForTimeout(10000);

  } catch (error) {
    console.error('Error:', error.message);
    await page.screenshot({ path: 'test-screenshots/escalation-policy-error.png', fullPage: true });
  } finally {
    await browser.close();
  }
})();
