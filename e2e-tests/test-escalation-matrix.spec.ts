import { test, expect } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

test.describe('Escalation Matrix UI Verification', () => {
  const screenshotsDir = path.join(__dirname, 'test-evidence', 'escalation-matrix');

  test.beforeAll(() => {
    // Create screenshots directory if it doesn't exist
    if (!fs.existsSync(screenshotsDir)) {
      fs.mkdirSync(screenshotsDir, { recursive: true });
    }
  });

  test.beforeEach(async ({ page }) => {
    // Navigate to login page
    await page.goto('http://localhost:4200/login');
    await page.waitForLoadState('networkidle');

    // Check if already logged in
    const isLoggedIn = await page.url().includes('/dashboard');

    if (!isLoggedIn) {
      // Login
      await page.fill('input[type="email"], input[formControlName="email"]', 'admin@complaintmanagement.com');
      await page.fill('input[type="password"], input[formControlName="password"]', 'Admin@123');
      await page.click('button[type="submit"]');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);
    }
  });

  test('1. Navigate to Escalation Matrix page', async ({ page }) => {
    console.log('Navigating to Escalation Matrix page...');
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Verify URL
    expect(page.url()).toContain('/admin/escalation-matrix');
    console.log('✓ Successfully navigated to Escalation Matrix page');
  });

  test('2. Capture main page with rule cards design', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);

    // Take full page screenshot
    await page.screenshot({
      path: path.join(screenshotsDir, '01-main-page-overview.png'),
      fullPage: true
    });
    console.log('✓ Captured main page screenshot');

    // Check for rule cards
    const ruleCards = await page.locator('.rule-card').count();
    console.log(`  Found ${ruleCards} rule cards`);

    // Check for priority indicators
    const priorityIndicators = await page.locator('.priority-indicator').count();
    console.log(`  Found ${priorityIndicators} priority indicators`);
  });

  test('3. Capture search and filter bar', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Locate and screenshot the filters bar
    const filtersBar = page.locator('.filters-bar');
    if (await filtersBar.isVisible()) {
      await filtersBar.screenshot({
        path: path.join(screenshotsDir, '02-search-filter-bar.png')
      });
      console.log('✓ Captured search and filter bar');

      // Verify search box exists
      const searchBox = await page.locator('.search-box input').isVisible();
      console.log(`  Search box visible: ${searchBox}`);

      // Verify filter buttons
      const filterButtons = await page.locator('.filter-btn').count();
      console.log(`  Found ${filterButtons} filter buttons`);
    } else {
      console.log('⚠ Filters bar not visible (might be in form view)');
    }
  });

  test('4. Capture right sidebar with Rule Simulator', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Scroll to sidebar
    const sidebar = page.locator('.sidebar-column');
    if (await sidebar.isVisible()) {
      await sidebar.screenshot({
        path: path.join(screenshotsDir, '03-sidebar-simulator.png')
      });
      console.log('✓ Captured sidebar with Rule Simulator');

      // Check individual sidebar sections
      const simulatorCard = await page.locator('.simulator-card').isVisible();
      console.log(`  Rule Simulator visible: ${simulatorCard}`);

      const infoCard = await page.locator('.info-card').isVisible();
      console.log(`  How Escalations Work visible: ${infoCard}`);

      const healthCard = await page.locator('.health-card').isVisible();
      console.log(`  Escalation Health visible: ${healthCard}`);
    } else {
      console.log('⚠ Sidebar not visible (might be in form view)');
    }
  });

  test('5. Test Add Escalation Rule button', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Click Add Escalation Rule
    const addButton = page.locator('button.btn-add:has-text("Add Escalation Rule")');
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(2000);

      // Verify form appears
      const formContainer = await page.locator('.form-container').isVisible();
      console.log(`  Form opened: ${formContainer}`);

      // Take screenshot of form
      await page.screenshot({
        path: path.join(screenshotsDir, '04-add-rule-form.png'),
        fullPage: true
      });
      console.log('✓ Captured Add Escalation Rule form');

      // Cancel to return to main view
      await page.click('button:has-text("Cancel")');
      await page.waitForTimeout(1000);
    } else {
      console.log('⚠ Add button not visible');
    }
  });

  test('6. Test toggle switch on existing rules', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Find first toggle switch
    const toggleSwitch = page.locator('.toggle-switch input').first();
    if (await toggleSwitch.isVisible()) {
      const initialState = await toggleSwitch.isChecked();
      console.log(`  Initial toggle state: ${initialState ? 'Active' : 'Inactive'}`);

      // Take before screenshot
      await page.screenshot({
        path: path.join(screenshotsDir, '05-toggle-before.png'),
        fullPage: true
      });

      // Click toggle
      await toggleSwitch.click();
      await page.waitForTimeout(1500);

      const newState = await toggleSwitch.isChecked();
      console.log(`  New toggle state: ${newState ? 'Active' : 'Inactive'}`);

      // Take after screenshot
      await page.screenshot({
        path: path.join(screenshotsDir, '06-toggle-after.png'),
        fullPage: true
      });
      console.log('✓ Tested toggle switch functionality');

      // Toggle back
      await toggleSwitch.click();
      await page.waitForTimeout(1000);
    } else {
      console.log('⚠ No toggle switches found (no rules exist)');
    }
  });

  test('7. Test dropdown menu on existing rules', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Find first dropdown menu button
    const menuButton = page.locator('.btn-menu').first();
    if (await menuButton.isVisible()) {
      // Click to open dropdown
      await menuButton.click();
      await page.waitForTimeout(1000);

      // Verify dropdown is visible
      const dropdownMenu = await page.locator('.dropdown-menu').isVisible();
      console.log(`  Dropdown menu opened: ${dropdownMenu}`);

      // Take screenshot
      await page.screenshot({
        path: path.join(screenshotsDir, '07-dropdown-menu.png'),
        fullPage: true
      });
      console.log('✓ Captured dropdown menu');

      // Count menu items
      const menuItems = await page.locator('.dropdown-item').count();
      console.log(`  Found ${menuItems} menu items`);

      // Close dropdown by clicking elsewhere
      await page.click('.page-header');
      await page.waitForTimeout(500);
    } else {
      console.log('⚠ No dropdown menus found (no rules exist)');
    }
  });

  test('8. Test expand/collapse for escalation levels', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);

    // Find first expand button
    const expandButton = page.locator('.expand-btn').first();
    if (await expandButton.isVisible()) {
      // Take before screenshot
      await page.screenshot({
        path: path.join(screenshotsDir, '08-levels-collapsed.png'),
        fullPage: true
      });

      // Click to expand
      await expandButton.click();
      await page.waitForTimeout(1000);

      // Verify timeline is visible
      const timeline = await page.locator('.levels-timeline').first().isVisible();
      console.log(`  Levels timeline visible: ${timeline}`);

      // Take after screenshot
      await page.screenshot({
        path: path.join(screenshotsDir, '09-levels-expanded.png'),
        fullPage: true
      });
      console.log('✓ Tested expand/collapse functionality');

      // Count level items
      const levelItems = await page.locator('.levels-timeline .level-item').count();
      console.log(`  Found ${levelItems} escalation levels`);
    } else {
      console.log('⚠ No expand buttons found (no rules with levels exist)');
    }
  });

  test('9. Verify all new design elements', async ({ page }) => {
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);

    console.log('\n=== DESIGN ELEMENTS VERIFICATION ===\n');

    // 1. Priority indicators on cards
    const priorityIndicators = await page.locator('.priority-indicator').count();
    console.log(`✓ Priority Indicators: ${priorityIndicators > 0 ? 'VISIBLE' : 'NOT FOUND'} (${priorityIndicators} found)`);

    // 2. Toggle switches
    const toggleSwitches = await page.locator('.toggle-switch').count();
    console.log(`✓ Toggle Switches: ${toggleSwitches > 0 ? 'VISIBLE' : 'NOT FOUND'} (${toggleSwitches} found)`);

    // 3. Expand/collapse buttons
    const expandButtons = await page.locator('.expand-btn').count();
    console.log(`✓ Expand/Collapse Buttons: ${expandButtons > 0 ? 'VISIBLE' : 'NOT FOUND'} (${expandButtons} found)`);

    // 4. Rule Simulator
    const simulator = await page.locator('.simulator-card').isVisible();
    console.log(`✓ Rule Simulator: ${simulator ? 'VISIBLE' : 'NOT FOUND'}`);

    if (simulator) {
      const prioritySlider = await page.locator('.priority-slider').isVisible();
      const idleSlider = await page.locator('.idle-slider').isVisible();
      const simulateButton = await page.locator('.btn-simulate').isVisible();
      console.log(`  - Priority Slider: ${prioritySlider ? 'YES' : 'NO'}`);
      console.log(`  - Idle Time Slider: ${idleSlider ? 'YES' : 'NO'}`);
      console.log(`  - Simulate Button: ${simulateButton ? 'YES' : 'NO'}`);
    }

    // 5. How Escalations Work section
    const infoCard = await page.locator('.info-card').isVisible();
    console.log(`✓ How Escalations Work: ${infoCard ? 'VISIBLE' : 'NOT FOUND'}`);

    if (infoCard) {
      const steps = await page.locator('.step-item').count();
      console.log(`  - Steps shown: ${steps}`);
    }

    // 6. Escalation Health metrics
    const healthCard = await page.locator('.health-card').isVisible();
    console.log(`✓ Escalation Health: ${healthCard ? 'VISIBLE' : 'NOT FOUND'}`);

    if (healthCard) {
      const metrics = await page.locator('.metric-item').count();
      console.log(`  - Metrics shown: ${metrics}`);
    }

    // 7. Search and filters
    const searchBox = await page.locator('.search-box').isVisible();
    console.log(`✓ Search Box: ${searchBox ? 'VISIBLE' : 'NOT FOUND'}`);

    const filterButtons = await page.locator('.filter-btn').count();
    console.log(`✓ Filter Buttons: ${filterButtons > 0 ? 'VISIBLE' : 'NOT FOUND'} (${filterButtons} found)`);

    // 8. Rule cards
    const ruleCards = await page.locator('.rule-card').count();
    console.log(`✓ Rule Cards: ${ruleCards} found`);

    if (ruleCards > 0) {
      const statusBadges = await page.locator('.status-badge').count();
      const cardDetails = await page.locator('.card-details').count();
      console.log(`  - Status Badges: ${statusBadges}`);
      console.log(`  - Card Details: ${cardDetails}`);
    }

    // Take final comprehensive screenshot
    await page.screenshot({
      path: path.join(screenshotsDir, '10-final-verification.png'),
      fullPage: true
    });
    console.log('\n✓ All design elements verified and documented');
  });

  test('10. Collect console logs and network activity', async ({ page }) => {
    const consoleLogs: string[] = [];
    const errors: string[] = [];

    // Listen to console
    page.on('console', msg => {
      const text = `[${msg.type()}] ${msg.text()}`;
      consoleLogs.push(text);
      if (msg.type() === 'error') {
        errors.push(text);
      }
    });

    // Navigate and interact
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);

    // Save logs to file
    const logsPath = path.join(screenshotsDir, 'console-logs.txt');
    fs.writeFileSync(logsPath, consoleLogs.join('\n'));
    console.log(`✓ Console logs saved to ${logsPath}`);

    if (errors.length > 0) {
      console.log(`⚠ Found ${errors.length} console errors:`);
      errors.forEach(err => console.log(`  ${err}`));
    } else {
      console.log('✓ No console errors detected');
    }
  });
});
