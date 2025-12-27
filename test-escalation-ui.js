const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

// Test configuration
const BASE_URL = 'http://localhost:4200';
const LOGIN_URL = `${BASE_URL}/login`;
const ESCALATION_URL = `${BASE_URL}/admin/escalation-policy`;
const SCREENSHOT_DIR = path.join(__dirname, 'escalation-test-screenshots');
const TEST_CREDENTIALS = {
  email: 'admin@complaintmanagement.com',
  password: 'Admin@123'
};

// Create screenshot directory
if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

// Test results
const testResults = {
  timestamp: new Date().toISOString(),
  testName: 'Escalation Management UI Test',
  passed: [],
  failed: [],
  warnings: [],
  screenshots: []
};

// Helper function to save screenshot
async function takeScreenshot(page, name, description) {
  const filename = `${Date.now()}_${name}.png`;
  const filepath = path.join(SCREENSHOT_DIR, filename);
  await page.screenshot({ path: filepath, fullPage: true });
  testResults.screenshots.push({
    name,
    description,
    path: filepath,
    timestamp: new Date().toISOString()
  });
  console.log(`✓ Screenshot saved: ${name}`);
  return filepath;
}

// Helper function to log test result
function logTest(name, status, details = '') {
  const result = { name, details, timestamp: new Date().toISOString() };
  if (status === 'pass') {
    testResults.passed.push(result);
    console.log(`✓ PASS: ${name}${details ? ' - ' + details : ''}`);
  } else if (status === 'fail') {
    testResults.failed.push(result);
    console.log(`✗ FAIL: ${name}${details ? ' - ' + details : ''}`);
  } else if (status === 'warn') {
    testResults.warnings.push(result);
    console.log(`⚠ WARN: ${name}${details ? ' - ' + details : ''}`);
  }
}

// Main test execution
async function runTest() {
  let browser;
  let context;
  let page;

  try {
    console.log('\n========================================');
    console.log('ESCALATION MANAGEMENT UI TEST');
    console.log('========================================\n');

    // Launch browser
    console.log('Launching browser...');
    browser = await chromium.launch({
      headless: false,
      slowMo: 500 // Slow down for visibility
    });
    context = await browser.newContext({
      viewport: { width: 1920, height: 1080 },
      recordVideo: {
        dir: SCREENSHOT_DIR,
        size: { width: 1920, height: 1080 }
      }
    });
    page = await context.newPage();

    // Enable console logging
    page.on('console', msg => {
      if (msg.type() === 'error') {
        console.log(`Browser Console Error: ${msg.text()}`);
        testResults.warnings.push({
          name: 'Console Error',
          details: msg.text(),
          timestamp: new Date().toISOString()
        });
      }
    });

    // Capture network errors
    page.on('pageerror', err => {
      console.log(`Page Error: ${err.message}`);
      testResults.warnings.push({
        name: 'Page Error',
        details: err.message,
        timestamp: new Date().toISOString()
      });
    });

    // ==========================================
    // TEST 1: Navigate to Login Page
    // ==========================================
    console.log('\n--- TEST 1: Navigate to Login Page ---');
    try {
      await page.goto(LOGIN_URL, { waitUntil: 'networkidle', timeout: 30000 });
      logTest('Navigate to login page', 'pass', `URL: ${LOGIN_URL}`);
      await takeScreenshot(page, '01_login_page', 'Initial login page load');
    } catch (error) {
      logTest('Navigate to login page', 'fail', error.message);
      throw error;
    }

    // ==========================================
    // TEST 2: Perform Login
    // ==========================================
    console.log('\n--- TEST 2: Perform Login ---');
    try {
      // Wait for login form
      await page.waitForSelector('input[type="email"], input[formcontrolname="email"]', { timeout: 10000 });

      // Fill in credentials
      const emailInput = await page.locator('input[type="email"], input[formcontrolname="email"]').first();
      await emailInput.fill(TEST_CREDENTIALS.email);
      logTest('Fill email field', 'pass', TEST_CREDENTIALS.email);

      const passwordInput = await page.locator('input[type="password"], input[formcontrolname="password"]').first();
      await passwordInput.fill(TEST_CREDENTIALS.password);
      logTest('Fill password field', 'pass', '********');

      await takeScreenshot(page, '02_login_filled', 'Login form filled with credentials');

      // Click login button
      const loginButton = await page.locator('button[type="submit"]').first();
      await loginButton.click();
      logTest('Click login button', 'pass');

      // Wait for navigation after login
      await page.waitForURL(/dashboard|admin/, { timeout: 15000 });
      logTest('Login successful', 'pass', `Redirected to: ${page.url()}`);

      await page.waitForTimeout(2000);
      await takeScreenshot(page, '03_after_login', 'After successful login');
    } catch (error) {
      logTest('Login process', 'fail', error.message);
      await takeScreenshot(page, '03_login_error', 'Login error state');
      throw error;
    }

    // ==========================================
    // TEST 3: Navigate to Escalation Rules Page
    // ==========================================
    console.log('\n--- TEST 3: Navigate to Escalation Rules Page ---');
    try {
      await page.goto(ESCALATION_URL, { waitUntil: 'networkidle', timeout: 30000 });
      logTest('Navigate to escalation rules page', 'pass', ESCALATION_URL);

      // Wait for page to load
      await page.waitForTimeout(2000);

      await takeScreenshot(page, '04_escalation_page_initial', 'Initial escalation rules page');
    } catch (error) {
      logTest('Navigate to escalation rules page', 'fail', error.message);
      await takeScreenshot(page, '04_escalation_navigation_error', 'Navigation error');
      throw error;
    }

    // ==========================================
    // TEST 4: Verify UI Design Elements
    // ==========================================
    console.log('\n--- TEST 4: Verify UI Design Elements ---');

    // Check for page title
    try {
      const pageTitle = await page.locator('h1, h2, .page-title').first();
      if (await pageTitle.isVisible({ timeout: 5000 })) {
        const titleText = await pageTitle.textContent();
        logTest('Page title visible', 'pass', titleText);
      } else {
        logTest('Page title visible', 'fail', 'Title not found');
      }
    } catch (error) {
      logTest('Page title visible', 'fail', error.message);
    }

    // Check for search/filter bar
    try {
      const searchBar = await page.locator('input[placeholder*="Search"], .search-bar, input[type="search"]').first();
      if (await searchBar.isVisible({ timeout: 5000 })) {
        logTest('Search/filter bar present', 'pass');
      } else {
        logTest('Search/filter bar present', 'warn', 'Search bar not visible');
      }
    } catch (error) {
      logTest('Search/filter bar present', 'warn', error.message);
    }

    // Check for rule cards with priority indicators
    try {
      const ruleCards = await page.locator('.rule-card, .card, mat-card, .escalation-rule').all();
      if (ruleCards.length > 0) {
        logTest('Rule cards present', 'pass', `Found ${ruleCards.length} cards`);

        // Check for priority indicators
        const priorityIndicators = await page.locator('.priority-indicator, .priority-badge, .badge-priority').count();
        if (priorityIndicators > 0) {
          logTest('Priority indicators present', 'pass', `Found ${priorityIndicators} indicators`);
        } else {
          logTest('Priority indicators present', 'warn', 'No priority indicators found');
        }
      } else {
        logTest('Rule cards present', 'warn', 'No rule cards found (may be empty state)');
      }
    } catch (error) {
      logTest('Rule cards present', 'warn', error.message);
    }

    // Check for toggle switches
    try {
      const toggleSwitches = await page.locator('mat-slide-toggle, .toggle-switch, input[type="checkbox"]').count();
      if (toggleSwitches > 0) {
        logTest('Toggle switches present', 'pass', `Found ${toggleSwitches} toggles`);
      } else {
        logTest('Toggle switches present', 'warn', 'No toggle switches found');
      }
    } catch (error) {
      logTest('Toggle switches present', 'warn', error.message);
    }

    // Check for right sidebar (Rule Simulator)
    try {
      const sidebar = await page.locator('.sidebar, .rule-simulator, aside, [class*="sidebar"]').first();
      if (await sidebar.isVisible({ timeout: 5000 })) {
        logTest('Right sidebar (Rule Simulator) present', 'pass');
      } else {
        logTest('Right sidebar (Rule Simulator) present', 'warn', 'Sidebar not visible');
      }
    } catch (error) {
      logTest('Right sidebar (Rule Simulator) present', 'warn', error.message);
    }

    // Check for "Add Escalation Rule" button
    try {
      const addButton = await page.locator('button:has-text("Add"), button:has-text("Escalation"), button[class*="add"]').first();
      if (await addButton.isVisible({ timeout: 5000 })) {
        logTest('"Add Escalation Rule" button present', 'pass');
      } else {
        logTest('"Add Escalation Rule" button present', 'fail', 'Button not found');
      }
    } catch (error) {
      logTest('"Add Escalation Rule" button present', 'fail', error.message);
    }

    await takeScreenshot(page, '05_ui_elements_verified', 'After verifying UI design elements');

    // ==========================================
    // TEST 5: Test Add Escalation Rule Functionality
    // ==========================================
    console.log('\n--- TEST 5: Test Add Escalation Rule Functionality ---');
    try {
      // Find and click Add button
      const addButton = await page.locator('button:has-text("Add"), button:has-text("Escalation"), button[class*="add"]').first();
      await addButton.click();
      logTest('Click Add Escalation Rule button', 'pass');

      // Wait for form to appear
      await page.waitForTimeout(1500);
      await takeScreenshot(page, '06_add_form_opened', 'Add Escalation Rule form opened');

      // Check if form/dialog appeared
      const formVisible = await page.locator('form, mat-dialog-container, .dialog, .modal').isVisible({ timeout: 5000 });
      if (formVisible) {
        logTest('Add form appears', 'pass');
      } else {
        logTest('Add form appears', 'fail', 'Form not visible');
      }
    } catch (error) {
      logTest('Add Escalation Rule button functionality', 'fail', error.message);
      await takeScreenshot(page, '06_add_button_error', 'Error clicking Add button');
    }

    // ==========================================
    // TEST 6: Fill in Test Rule Data
    // ==========================================
    console.log('\n--- TEST 6: Fill in Test Rule Data ---');
    try {
      // Fill rule name
      const nameInput = await page.locator('input[formcontrolname="name"], input[placeholder*="Name"], input[id*="name"]').first();
      if (await nameInput.isVisible({ timeout: 5000 })) {
        await nameInput.fill('Test Auto Escalation');
        logTest('Fill rule name', 'pass', 'Test Auto Escalation');
      } else {
        logTest('Fill rule name', 'warn', 'Name input not found');
      }

      await page.waitForTimeout(500);

      // Fill description
      const descInput = await page.locator('textarea[formcontrolname="description"], textarea[placeholder*="Description"], input[formcontrolname="description"]').first();
      if (await descInput.isVisible({ timeout: 5000 })) {
        await descInput.fill('This is a test escalation rule created during UI testing to verify form functionality.');
        logTest('Fill description', 'pass');
      } else {
        logTest('Fill description', 'warn', 'Description input not found');
      }

      await page.waitForTimeout(500);
      await takeScreenshot(page, '07_form_filled', 'Form filled with test data');

      // Check for escalation level form
      const escalationLevelSection = await page.locator('[formgroupname="escalationLevel"], .escalation-level, [class*="level"]').first();
      if (await escalationLevelSection.isVisible({ timeout: 5000 })) {
        logTest('Escalation level form present', 'pass');
      } else {
        logTest('Escalation level form present', 'warn', 'Escalation level section not clearly visible');
      }

      await takeScreenshot(page, '08_form_complete', 'Complete form view with all fields');
    } catch (error) {
      logTest('Fill test rule data', 'warn', error.message);
      await takeScreenshot(page, '08_form_fill_error', 'Error filling form');
    }

    // ==========================================
    // TEST 7: Test Cancel Functionality
    // ==========================================
    console.log('\n--- TEST 7: Test Cancel Functionality ---');
    try {
      // Find and click Cancel button
      const cancelButton = await page.locator('button:has-text("Cancel"), button[mat-dialog-close]').first();
      if (await cancelButton.isVisible({ timeout: 5000 })) {
        await cancelButton.click();
        logTest('Click Cancel button', 'pass');

        await page.waitForTimeout(1000);

        // Verify form closed
        const formStillVisible = await page.locator('mat-dialog-container, .dialog, .modal').isVisible().catch(() => false);
        if (!formStillVisible) {
          logTest('Form closed after cancel', 'pass');
        } else {
          logTest('Form closed after cancel', 'warn', 'Form may still be visible');
        }
      } else {
        logTest('Cancel button functionality', 'warn', 'Cancel button not found');
      }
    } catch (error) {
      logTest('Cancel functionality', 'warn', error.message);
    }

    // ==========================================
    // TEST 8: Final Screenshot and DOM Analysis
    // ==========================================
    console.log('\n--- TEST 8: Final Screenshot and Analysis ---');
    await page.waitForTimeout(1000);
    await takeScreenshot(page, '09_final_state', 'Final state of Escalation Rules page');

    // Capture page HTML for analysis
    const htmlContent = await page.content();
    const htmlPath = path.join(SCREENSHOT_DIR, 'escalation_page_source.html');
    fs.writeFileSync(htmlPath, htmlContent);
    console.log(`✓ Page HTML saved: ${htmlPath}`);

    // Capture all visible text
    const allText = await page.locator('body').textContent();
    const textPath = path.join(SCREENSHOT_DIR, 'escalation_page_text.txt');
    fs.writeFileSync(textPath, allText);
    console.log(`✓ Page text saved: ${textPath}`);

    // List all buttons on the page
    const buttons = await page.locator('button').allTextContents();
    console.log('\nButtons found on page:', buttons);

    // List all form inputs
    const inputs = await page.locator('input').count();
    console.log(`Form inputs found: ${inputs}`);

    // Check for Material Design components
    const matComponents = await page.locator('[class*="mat-"]').count();
    console.log(`Material Design components found: ${matComponents}`);

    logTest('Final page analysis', 'pass', `Captured HTML, text, found ${buttons.length} buttons, ${inputs} inputs`);

    // ==========================================
    // Generate Test Report
    // ==========================================
    console.log('\n========================================');
    console.log('TEST EXECUTION COMPLETE');
    console.log('========================================\n');

    const reportPath = path.join(SCREENSHOT_DIR, 'test_report.json');
    fs.writeFileSync(reportPath, JSON.stringify(testResults, null, 2));
    console.log(`✓ Test report saved: ${reportPath}`);

    // Print summary
    console.log('\n--- TEST SUMMARY ---');
    console.log(`Total Passed: ${testResults.passed.length}`);
    console.log(`Total Failed: ${testResults.failed.length}`);
    console.log(`Total Warnings: ${testResults.warnings.length}`);
    console.log(`Screenshots: ${testResults.screenshots.length}`);
    console.log(`\nScreenshots saved to: ${SCREENSHOT_DIR}`);

    // Wait before closing
    await page.waitForTimeout(3000);

  } catch (error) {
    console.error('\n❌ FATAL ERROR:', error.message);
    console.error(error.stack);

    if (page) {
      await takeScreenshot(page, '99_fatal_error', 'Fatal error state');
    }
  } finally {
    // Cleanup
    if (context) {
      await context.close();
    }
    if (browser) {
      await browser.close();
    }
    console.log('\nBrowser closed.');
  }

  return testResults;
}

// Run the test
runTest().then(results => {
  console.log('\n========================================');
  console.log('FINAL RESULTS');
  console.log('========================================');
  console.log(JSON.stringify(results, null, 2));

  process.exit(results.failed.length > 0 ? 1 : 0);
}).catch(error => {
  console.error('Test execution failed:', error);
  process.exit(1);
});
