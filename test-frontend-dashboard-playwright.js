// Investigation 3: Frontend Dashboard Testing with Playwright
const { chromium } = require('playwright');

(async () => {
  console.log('=== INVESTIGATION 3: Frontend Dashboard Testing ===\n');

  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();

  try {
    // Test 1: Complainant Dashboard
    console.log('Test 1: Testing COMPLAINANT Dashboard...');
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'nav_nainital@yahoo.com');
    await page.fill('input[name="password"]', 'Nav@12345');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');
    console.log('  ✓ Logged in as complainant');

    // Wait for statistics to load
    await page.waitForTimeout(2000);

    // Check statistics on dashboard
    const complainantStats = await page.evaluate(() => {
      // Try to find statistics elements
      const statsElements = document.querySelectorAll('[class*="statistic"], [class*="stat"], [class*="count"]');
      return {
        found: statsElements.length,
        visible: Array.from(statsElements).map(el => el.textContent.trim()).slice(0, 5)
      };
    });

    console.log(`  Statistics found: ${complainantStats.found}`);
    console.log(`  Sample values: ${JSON.stringify(complainantStats.visible)}`);

    await page.screenshot({ path: '.playwright-mcp/investigation-3-complainant-dashboard.png' });
    console.log('  📸 Screenshot saved\n');

    // Test 2: Admin Dashboard
    console.log('Test 2: Testing ADMIN Dashboard...');
    await page.goto('http://localhost:4200/login');
    await page.fill('input[name="email"]', 'admin@complaintmanagement.com');
    await page.fill('input[name="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForURL('**/dashboard');
    console.log('  ✓ Logged in as admin');

    await page.waitForTimeout(2000);

    const adminStats = await page.evaluate(() => {
      const statsElements = document.querySelectorAll('[class*="statistic"], [class*="stat"], [class*="count"]');
      return {
        found: statsElements.length,
        visible: Array.from(statsElements).map(el => el.textContent.trim()).slice(0, 5)
      };
    });

    console.log(`  Statistics found: ${adminStats.found}`);
    console.log(`  Sample values: ${JSON.stringify(adminStats.visible)}`);

    await page.screenshot({ path: '.playwright-mcp/investigation-3-admin-dashboard.png' });
    console.log('  📸 Screenshot saved\n');

    console.log('=== INVESTIGATION 3 COMPLETE ===');
    console.log('\nResults:');
    console.log(`  Complainant stats elements: ${complainantStats.found}`);
    console.log(`  Admin stats elements: ${adminStats.found}`);
    console.log(`  Status: ${complainantStats.found > 0 && adminStats.found > 0 ? '✅ PASS' : '❌ FAIL'}`);

  } catch (error) {
    console.error('  ✗ ERROR:', error.message);
  } finally {
    await browser.close();
  }
})();
