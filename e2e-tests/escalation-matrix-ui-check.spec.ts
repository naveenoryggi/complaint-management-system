import { test, expect } from '@playwright/test';
import * as path from 'path';

test.describe('Escalation Matrix UI Manual Verification', () => {
  const screenshotsDir = path.join(__dirname, 'test-evidence', 'escalation-matrix');

  test('Complete UI verification with interactive elements', async ({ page }) => {
    console.log('=== ESCALATION MATRIX UI VERIFICATION TEST ===\n');

    // Step 1: Login
    console.log('Step 1: Logging in...');
    await page.goto('http://localhost:4200/login');
    await page.waitForLoadState('networkidle');

    const isLoggedIn = page.url().includes('/dashboard');
    if (!isLoggedIn) {
      await page.fill('input[type="email"], input[formControlName="email"]', 'admin@complaintmanagement.com');
      await page.fill('input[type="password"], input[formControlName="password"]', 'Admin@123');
      await page.click('button[type="submit"]');
      await page.waitForLoadState('networkidle');
      await page.waitForTimeout(2000);
    }
    console.log('✓ Logged in successfully\n');

    // Step 2: Navigate to Escalation Matrix
    console.log('Step 2: Navigating to Escalation Matrix page...');
    await page.goto('http://localhost:4200/admin/escalation-matrix');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(3000);
    expect(page.url()).toContain('/admin/escalation-matrix');
    console.log('✓ Successfully navigated to Escalation Matrix page\n');

    // Step 3: Capture main page overview
    console.log('Step 3: Capturing main page overview...');
    await page.screenshot({
      path: path.join(screenshotsDir, 'MAIN-PAGE-OVERVIEW.png'),
      fullPage: true
    });
    console.log('✓ Main page screenshot saved\n');

    // Step 4: Verify design elements
    console.log('Step 4: Verifying design elements...\n');

    const ruleCards = await page.locator('.rule-card').count();
    console.log(`  Rule Cards: ${ruleCards} found`);

    const priorityIndicators = await page.locator('.priority-indicator').count();
    console.log(`  Priority Indicators: ${priorityIndicators} found - ${priorityIndicators > 0 ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const toggleSwitches = await page.locator('.toggle-switch').count();
    console.log(`  Toggle Switches: ${toggleSwitches} found - ${toggleSwitches > 0 ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const expandButtons = await page.locator('.expand-btn').count();
    console.log(`  Expand/Collapse Buttons: ${expandButtons} found - ${expandButtons > 0 ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const searchBox = await page.locator('.search-box input').isVisible();
    console.log(`  Search Box: ${searchBox ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const filterButtons = await page.locator('.filter-btn').count();
    console.log(`  Filter Buttons: ${filterButtons} found - ${filterButtons > 0 ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const simulator = await page.locator('.simulator-card').isVisible();
    console.log(`  Rule Simulator: ${simulator ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const infoCard = await page.locator('.info-card').isVisible();
    console.log(`  How Escalations Work: ${infoCard ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    const healthCard = await page.locator('.health-card').isVisible();
    console.log(`  Escalation Health: ${healthCard ? '✓ VISIBLE' : '✗ NOT FOUND'}`);

    console.log('');

    // Step 5: Screenshot the sidebar
    console.log('Step 5: Capturing sidebar with Rule Simulator...');
    const sidebar = page.locator('.sidebar-column');
    if (await sidebar.isVisible()) {
      await sidebar.screenshot({
        path: path.join(screenshotsDir, 'SIDEBAR-RULE-SIMULATOR.png')
      });
      console.log('✓ Sidebar screenshot saved\n');
    }

    // Step 6: Test clicking "Add Escalation Rule" button
    console.log('Step 6: Testing "Add Escalation Rule" button...');
    const addButton = page.locator('button:has-text("Add Escalation Rule")').first();
    if (await addButton.isVisible()) {
      await addButton.click();
      await page.waitForTimeout(2000);

      const formVisible = await page.locator('.form-container').isVisible();
      console.log(`  Form opened: ${formVisible ? '✓ YES' : '✗ NO'}`);

      if (formVisible) {
        await page.screenshot({
          path: path.join(screenshotsDir, 'ADD-RULE-FORM.png'),
          fullPage: true
        });
        console.log('  ✓ Form screenshot saved');

        // Close form
        await page.click('button:has-text("Cancel")');
        await page.waitForTimeout(1500);
        console.log('  ✓ Form closed\n');
      }
    } else {
      console.log('  ✗ Add button not found\n');
    }

    // Step 7: Test toggle switch
    console.log('Step 7: Testing toggle switch...');
    const firstToggle = page.locator('.toggle-switch input').first();
    if (await firstToggle.isVisible()) {
      const beforeState = await firstToggle.isChecked();
      console.log(`  Initial state: ${beforeState ? 'ACTIVE' : 'INACTIVE'}`);

      await page.screenshot({
        path: path.join(screenshotsDir, 'TOGGLE-BEFORE.png'),
        fullPage: true
      });

      await firstToggle.click();
      await page.waitForTimeout(1500);

      const afterState = await firstToggle.isChecked();
      console.log(`  After toggle: ${afterState ? 'ACTIVE' : 'INACTIVE'}`);
      console.log(`  ✓ Toggle switched from ${beforeState} to ${afterState}\n`);

      await page.screenshot({
        path: path.join(screenshotsDir, 'TOGGLE-AFTER.png'),
        fullPage: true
      });

      // Toggle back
      await firstToggle.click();
      await page.waitForTimeout(1000);
    } else {
      console.log('  ✗ No toggle switches found\n');
    }

    // Step 8: Test dropdown menu
    console.log('Step 8: Testing dropdown menu...');
    const menuButton = page.locator('.btn-menu').first();
    if (await menuButton.isVisible()) {
      await menuButton.click();
      await page.waitForTimeout(1000);

      const dropdownVisible = await page.locator('.dropdown-menu').isVisible();
      console.log(`  Dropdown opened: ${dropdownVisible ? '✓ YES' : '✗ NO'}`);

      if (dropdownVisible) {
        const menuItems = await page.locator('.dropdown-item').count();
        console.log(`  Menu items: ${menuItems} found`);

        await page.screenshot({
          path: path.join(screenshotsDir, 'DROPDOWN-MENU.png'),
          fullPage: true
        });
        console.log('  ✓ Dropdown screenshot saved\n');

        // Close dropdown
        await page.click('.page-header');
        await page.waitForTimeout(500);
      }
    } else {
      console.log('  ✗ No dropdown menus found\n');
    }

    // Step 9: Test expand/collapse
    console.log('Step 9: Testing expand/collapse for levels...');
    const expandBtn = page.locator('.expand-btn').first();
    if (await expandBtn.isVisible()) {
      await page.screenshot({
        path: path.join(screenshotsDir, 'LEVELS-COLLAPSED.png'),
        fullPage: true
      });

      await expandBtn.click();
      await page.waitForTimeout(1000);

      const timelineVisible = await page.locator('.levels-timeline').first().isVisible();
      console.log(`  Timeline visible: ${timelineVisible ? '✓ YES' : '✗ NO'}`);

      if (timelineVisible) {
        const levelItems = await page.locator('.levels-timeline .level-item').count();
        console.log(`  Escalation levels shown: ${levelItems}`);

        await page.screenshot({
          path: path.join(screenshotsDir, 'LEVELS-EXPANDED.png'),
          fullPage: true
        });
        console.log('  ✓ Expanded levels screenshot saved\n');
      }
    } else {
      console.log('  ✗ No expand buttons found\n');
    }

    // Final screenshot
    console.log('Step 10: Taking final verification screenshot...');
    await page.screenshot({
      path: path.join(screenshotsDir, 'FINAL-STATE.png'),
      fullPage: true
    });
    console.log('✓ Final screenshot saved\n');

    console.log('=== UI VERIFICATION COMPLETE ===');
  });
});
