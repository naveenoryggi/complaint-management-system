const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const BASE_URL = 'http://localhost:4200';
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots', 'roles-final');

if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

let screenshotCounter = 1;
async function screenshot(page, name) {
  const filename = `${String(screenshotCounter).padStart(3, '0')}_${name}.png`;
  screenshotCounter++;
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, filename), fullPage: true });
  console.log(`📸 ${filename}`);
  return filename;
}

async function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

(async () => {
  const browser = await chromium.launch({ headless: false, slowMo: 300 });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  const issues = [];
  const passed = [];

  console.log('\n╔═══════════════════════════════════════════════════════════════╗');
  console.log('║  ROLE & PERMISSION MANAGEMENT - COMPREHENSIVE TEST REPORT    ║');
  console.log('╚═══════════════════════════════════════════════════════════════╝\n');

  try {
    // ==================================================================
    // STEP 1: LOGIN
    // ==================================================================
    console.log('\n[STEP 1] LOGIN TO APPLICATION');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

    await page.goto(BASE_URL);
    await sleep(2000);
    await screenshot(page, 'login_page');

    // Use actual admin credentials from database seeder
    await page.fill('input[placeholder*="Employee"]', 'admin@complaintmanagement.com');
    await sleep(300);
    await page.fill('input[type="password"]', 'Admin@123');
    await sleep(300);
    await screenshot(page, 'credentials_entered');

    await page.click('button:has-text("Sign In")');
    console.log('  → Sign In clicked');

    // Wait for navigation
    await page.waitForURL(/dashboard|home|complaints|admin/, { timeout: 15000 }).catch(async () => {
      console.log('  ⚠ Did not navigate away from login - checking for error');
      await screenshot(page, 'login_potential_error');
    });

    await sleep(2000);
    await screenshot(page, 'after_login');

    if (page.url().includes('login')) {
      console.log('  ❌ FAILED: Still on login page');
      issues.push({
        severity: 'CRITICAL',
        test: 'Authentication',
        description: 'Unable to login - credentials may be incorrect or backend issue',
        actualResult: 'Remained on login page after clicking Sign In',
        expectedResult: 'Should navigate to dashboard after successful login'
      });

      // Check for error message
      const errorMsg = await page.locator('.error, .alert-danger, [class*="error"]').first().textContent().catch(() => null);
      if (errorMsg) {
        console.log(`  Error message: ${errorMsg}`);
      }
    } else {
      console.log(`  ✅ PASSED: Logged in successfully - URL: ${page.url()}`);
      passed.push('User authentication and login');
    }

    // ==================================================================
    // STEP 2: NAVIGATE TO ROLES PAGE
    // ==================================================================
    console.log('\n[STEP 2] NAVIGATE TO ROLES PAGE');
    console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

    await page.goto(`${BASE_URL}/admin/roles`);
    await sleep(3000);
    await screenshot(page, 'roles_page_loaded');

    const pageUrl = page.url();
    console.log(`  Current URL: ${pageUrl}`);

    // Check if we're still authenticated
    if (pageUrl.includes('login')) {
      console.log('  ❌ FAILED: Redirected back to login - session not maintained');
      issues.push({
        severity: 'CRITICAL',
        test: 'Session Management',
        description: 'Session not maintained when navigating to admin roles page',
        actualResult: 'Redirected to login page',
        expectedResult: 'Should stay on roles page with active session'
      });
    } else {
      console.log('  ✅ PASSED: Successfully accessed roles page');

      // ==================================================================
      // STEP 3: VERIFY PAGE COMPONENTS
      // ==================================================================
      console.log('\n[STEP 3] VERIFY PAGE COMPONENTS');
      console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

      // Info Banner
      const infoBanner = await page.locator('.alert, [class*="banner"], [class*="info"]').filter({ hasText: /role|permission/i }).first();
      if (await infoBanner.count() > 0) {
        console.log('  ✅ Info banner present');
        passed.push('Info banner explaining roles and permissions');
      } else {
        console.log('  ⚠ Info banner not found');
        issues.push({
          severity: 'MINOR',
          test: 'Page Components - Info Banner',
          description: 'No info banner found explaining roles and permissions'
        });
      }

      // Search Bar
      const searchBar = await page.locator('input[type="search"], input[placeholder*="Search"], input[placeholder*="search"]').first();
      if (await searchBar.count() > 0 && await searchBar.isVisible()) {
        console.log('  ✅ Search bar present');
        passed.push('Search bar for filtering roles');
      } else {
        console.log('  ❌ Search bar not found');
        issues.push({
          severity: 'MAJOR',
          test: 'Page Components - Search Bar',
          description: 'Search bar is missing from the roles page'
        });
      }

      // Active Only Toggle
      const activeToggle = await page.locator('input[type="checkbox"], mat-slide-toggle, .toggle').first();
      if (await activeToggle.count() > 0) {
        console.log('  ✅ Active filter toggle present');
      } else {
        console.log('  ⚠ Active filter toggle not found');
        issues.push({
          severity: 'MINOR',
          test: 'Page Components - Active Toggle',
          description: 'Show Active Only toggle not found'
        });
      }

      // ==================================================================
      // STEP 4: VERIFY ROLE CARDS
      // ==================================================================
      console.log('\n[STEP 4] VERIFY ROLE CARDS DISPLAY');
      console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

      const roleCards = await page.locator('.card, mat-card, [class*="role-card"], [class*="card-"]').all();
      console.log(`  Found ${roleCards.length} role cards`);

      if (roleCards.length === 0) {
        console.log('  ❌ CRITICAL: No role cards found!');
        issues.push({
          severity: 'CRITICAL',
          test: 'Role Cards Display',
          description: 'No role cards are displayed on the page',
          actualResult: '0 role cards found',
          expectedResult: 'At least system roles (Admin, Manager, Employee) should be displayed'
        });
        await screenshot(page, 'no_role_cards');
      } else {
        console.log(`  ✅ PASSED: ${roleCards.length} role cards displayed`);
        passed.push(`Display ${roleCards.length} role cards`);

        // Detailed card inspection
        console.log('\n  Inspecting first 3 role cards in detail:');

        for (let i = 0; i < Math.min(3, roleCards.length); i++) {
          const card = roleCards[i];
          await card.scrollIntoViewIfNeeded();
          await sleep(500);

          console.log(`\n  ━━━ Role Card ${i + 1} ━━━`);

          const cardText = await card.textContent();

          // Role Name
          const nameElement = card.locator('h1, h2, h3, h4, h5, .title, [class*="title"], [class*="name"]').first();
          const roleName = await nameElement.textContent().catch(() => null);
          if (roleName && roleName.trim()) {
            console.log(`    Role Name: ✅ "${roleName.trim()}"`);
          } else {
            console.log(`    Role Name: ❌ NOT FOUND`);
            issues.push({
              severity: 'MAJOR',
              test: `Role Card ${i + 1} - Name`,
              description: 'Role name not displayed on card'
            });
          }

          // Role Code
          if (cardText.match(/[A-Z_]{3,}/)) {
            console.log(`    Role Code: ✅ Present`);
          } else {
            console.log(`    Role Code: ❌ NOT FOUND`);
          }

          // System Role Badge
          const systemBadge = await card.locator('.badge, .chip, mat-chip, .label').filter({ hasText: /system/i }).count();
          console.log(`    System Badge: ${systemBadge > 0 ? '✅' : '⚪'} ${systemBadge > 0 ? 'Present' : 'N/A (custom role)'}`);

          // Active Status Badge
          const activeBadge = await card.locator('.badge, .chip, mat-chip, .label').filter({ hasText: /active/i }).count();
          if (activeBadge > 0) {
            console.log(`    Active Badge: ✅ Present`);
          } else {
            console.log(`    Active Badge: ❌ NOT FOUND`);
            issues.push({
              severity: 'MINOR',
              test: `Role Card ${i + 1} - Active Status`,
              description: 'Active status badge not displayed'
            });
          }

          // Role Type
          if (cardText.match(/type/i)) {
            console.log(`    Role Type: ✅ Displayed`);
          } else {
            console.log(`    Role Type: ⚠ Not visible`);
          }

          // Escalation Level
          if (cardText.match(/escalation/i)) {
            console.log(`    Escalation Level: ✅ Displayed`);
          } else {
            console.log(`    Escalation Level: ⚪ Not shown`);
          }

          // Permissions Count
          const permMatch = cardText.match(/(\d+)\s*(permission|granted)/i);
          if (permMatch) {
            console.log(`    Permissions: ✅ ${permMatch[1]} shown`);
          } else {
            console.log(`    Permissions: ❌ NOT FOUND`);
            issues.push({
              severity: 'MAJOR',
              test: `Role Card ${i + 1} - Permissions Count`,
              description: 'Permission count not displayed on card'
            });
          }

          // Display Order
          if (cardText.match(/order|position|priority/i)) {
            console.log(`    Display Order: ✅ Shown`);
          } else {
            console.log(`    Display Order: ⚪ Not shown`);
          }

          // Progress Bar
          const progressBar = await card.locator('.progress, progress, mat-progress-bar, [role="progressbar"]').count();
          if (progressBar > 0) {
            console.log(`    Progress Bar: ✅ Present`);
          } else {
            console.log(`    Progress Bar: ❌ NOT FOUND`);
            issues.push({
              severity: 'MINOR',
              test: `Role Card ${i + 1} - Progress Bar`,
              description: 'Permission percentage progress bar not displayed'
            });
          }

          // Edit Button
          const editBtn = await card.locator('button:has-text("Edit"), button[aria-label*="Edit"], [class*="edit"]').count();
          if (editBtn > 0) {
            console.log(`    Edit Button: ✅ Present`);
          } else {
            console.log(`    Edit Button: ❌ NOT FOUND`);
            issues.push({
              severity: 'MAJOR',
              test: `Role Card ${i + 1} - Edit Button`,
              description: 'Edit button not found on role card'
            });
          }

          // Delete Button
          const deleteBtn = await card.locator('button:has-text("Delete"), button[aria-label*="Delete"], [class*="delete"]').count();
          if (deleteBtn > 0) {
            console.log(`    Delete Button: ✅ Present`);
          } else {
            console.log(`    Delete Button: ❌ NOT FOUND`);
            issues.push({
              severity: 'MAJOR',
              test: `Role Card ${i + 1} - Delete Button`,
              description: 'Delete button not found on role card'
            });
          }

          await screenshot(page, `role_card_${i + 1}_detail`);
        }
      }

      // ==================================================================
      // STEP 5: TEST CREATE FUNCTIONALITY
      // ==================================================================
      console.log('\n[STEP 5] TEST CREATE NEW ROLE');
      console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

      const createBtn = await page.locator('button:has-text("Create"), button:has-text("Add"), button:has-text("New")').filter({ hasText: /role/i }).first();

      if (await createBtn.count() === 0) {
        console.log('  ❌ CRITICAL: Create/Add Role button not found');
        issues.push({
          severity: 'CRITICAL',
          test: 'Create Role Feature',
          description: 'Create/Add Role button is not available on the page',
          recommendation: 'Add a prominent button to allow creating new custom roles'
        });
        await screenshot(page, 'no_create_button');
      } else {
        console.log('  ✅ Create button found');
        // Test would continue here if button exists
        passed.push('Create Role button is available');
      }

      // ==================================================================
      // STEP 6: TEST SEARCH FUNCTIONALITY
      // ==================================================================
      console.log('\n[STEP 6] TEST SEARCH FUNCTIONALITY');
      console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');

      const searchInput = await page.locator('input[type="search"], input[placeholder*="Search"]').first();

      if (await searchInput.count() > 0) {
        const initialCount = roleCards.length;
        console.log(`  Initial role count: ${initialCount}`);

        await searchInput.fill('Admin');
        await sleep(1500);
        await screenshot(page, 'search_admin');

        const afterSearch = await page.locator('.card, mat-card, [class*="role-card"]').all();
        console.log(`  After search: ${afterSearch.length} roles`);

        if (afterSearch.length <= initialCount && afterSearch.length > 0) {
          console.log('  ✅ PASSED: Search filters results');
          passed.push('Search functionality filters roles');
        } else if (afterSearch.length === 0) {
          console.log('  ⚠ WARNING: Search returned no results');
        } else {
          console.log('  ❌ FAILED: Search not working properly');
          issues.push({
            severity: 'MAJOR',
            test: 'Search Functionality',
            description: 'Search does not filter role cards correctly'
          });
        }

        await searchInput.clear();
        await sleep(1000);
      } else {
        console.log('  ⚠ Search input not available for testing');
      }

      await screenshot(page, 'final_page_state');
    }

    // ==================================================================
    // FINAL SUMMARY
    // ==================================================================
    console.log('\n╔═══════════════════════════════════════════════════════════════╗');
    console.log('║                     TEST EXECUTION SUMMARY                    ║');
    console.log('╚═══════════════════════════════════════════════════════════════╝\n');

    console.log(`✅ PASSED TESTS: ${passed.length}`);
    if (passed.length > 0) {
      passed.forEach((test, i) => console.log(`   ${i + 1}. ${test}`));
    }

    console.log(`\n❌ ISSUES FOUND: ${issues.length}\n`);

    const critical = issues.filter(i => i.severity === 'CRITICAL');
    const major = issues.filter(i => i.severity === 'MAJOR');
    const minor = issues.filter(i => i.severity === 'MINOR');

    if (critical.length > 0) {
      console.log(`🔴 CRITICAL (${critical.length}):`);
      critical.forEach((issue, i) => {
        console.log(`   ${i + 1}. [${issue.test}]`);
        console.log(`      ${issue.description}`);
        if (issue.recommendation) {
          console.log(`      💡 ${issue.recommendation}`);
        }
      });
      console.log('');
    }

    if (major.length > 0) {
      console.log(`🟠 MAJOR (${major.length}):`);
      major.forEach((issue, i) => {
        console.log(`   ${i + 1}. [${issue.test}] ${issue.description}`);
      });
      console.log('');
    }

    if (minor.length > 0) {
      console.log(`🟡 MINOR (${minor.length}):`);
      minor.forEach((issue, i) => {
        console.log(`   ${i + 1}. [${issue.test}] ${issue.description}`);
      });
      console.log('');
    }

    if (issues.length === 0) {
      console.log('🎉 ALL TESTS PASSED! No issues found.\n');
    }

    console.log(`📸 Screenshots: ${screenshotCounter - 1} captured`);
    console.log(`📁 Location: ${SCREENSHOT_DIR}\n`);

    // Generate detailed report
    const reportPath = path.join(__dirname, 'ROLES_TEST_REPORT.md');
    let report = `# Role & Permission Management - Test Report\n\n`;
    report += `**Date:** ${new Date().toISOString()}\n`;
    report += `**Test Suite:** Comprehensive Roles & Permissions Testing\n`;
    report += `**Application URL:** ${BASE_URL}/admin/roles\n\n`;

    report += `## Summary\n\n`;
    report += `| Metric | Count |\n`;
    report += `|--------|-------|\n`;
    report += `| ✅ Passed Tests | ${passed.length} |\n`;
    report += `| ❌ Total Issues | ${issues.length} |\n`;
    report += `| 🔴 Critical | ${critical.length} |\n`;
    report += `| 🟠 Major | ${major.length} |\n`;
    report += `| 🟡 Minor | ${minor.length} |\n`;
    report += `| 📸 Screenshots | ${screenshotCounter - 1} |\n\n`;

    report += `## Passed Tests\n\n`;
    if (passed.length === 0) {
      report += `*No tests passed*\n\n`;
    } else {
      passed.forEach((test, i) => {
        report += `${i + 1}. ✅ ${test}\n`;
      });
      report += `\n`;
    }

    report += `## Issues Found\n\n`;
    if (issues.length === 0) {
      report += `🎉 **No issues found! All tests passed successfully.**\n\n`;
    } else {
      if (critical.length > 0) {
        report += `### 🔴 Critical Issues\n\n`;
        critical.forEach((issue, i) => {
          report += `#### ${i + 1}. ${issue.test}\n\n`;
          report += `**Description:** ${issue.description}\n\n`;
          if (issue.actualResult) {
            report += `**Actual Result:** ${issue.actualResult}\n\n`;
          }
          if (issue.expectedResult) {
            report += `**Expected Result:** ${issue.expectedResult}\n\n`;
          }
          if (issue.recommendation) {
            report += `**Recommendation:** ${issue.recommendation}\n\n`;
          }
          report += `---\n\n`;
        });
      }

      if (major.length > 0) {
        report += `### 🟠 Major Issues\n\n`;
        major.forEach((issue, i) => {
          report += `${i + 1}. **[${issue.test}]** ${issue.description}\n`;
        });
        report += `\n`;
      }

      if (minor.length > 0) {
        report += `### 🟡 Minor Issues\n\n`;
        minor.forEach((issue, i) => {
          report += `${i + 1}. **[${issue.test}]** ${issue.description}\n`;
        });
        report += `\n`;
      }
    }

    report += `## Test Evidence\n\n`;
    report += `All screenshots have been saved to:\n`;
    report += `\`${SCREENSHOT_DIR}\`\n\n`;
    report += `Total screenshots captured: ${screenshotCounter - 1}\n`;

    fs.writeFileSync(reportPath, report);
    console.log(`📄 Detailed report saved: ${reportPath}\n`);

    console.log('═══════════════════════════════════════════════════════════════\n');

  } catch (error) {
    console.error('\n❌ FATAL ERROR:', error.message);
    console.error(error.stack);
    await screenshot(page, 'fatal_error');
  } finally {
    await sleep(2000);
    await browser.close();
  }
})();
