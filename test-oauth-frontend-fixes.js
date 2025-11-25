const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();

  console.log('=== Testing OAuth Frontend Fixes ===\n');

  try {
    // Step 1: Login
    console.log('1. Logging in as admin...');
    await page.goto('http://localhost:4200/login');
    await page.waitForTimeout(2000);

    await page.fill('input[type="email"]', 'admin@complaintmanagement.com');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    console.log('   ✓ Login successful\n');

    // Step 2: Navigate to Email Ticketing Config
    console.log('2. Navigating to Email Ticketing Config...');
    await page.goto('http://localhost:4200/admin/email-ticketing-config');
    await page.waitForTimeout(3000);

    await page.screenshot({ path: '.playwright-mcp/oauth-fix-test-01-page-loaded.png', fullPage: true });
    console.log('   ✓ Page loaded\n');

    // Step 3: Check authentication badge
    console.log('3. Checking Authentication Badge Fix...');
    const badges = await page.locator('.badge').allTextContents();
    console.log('   Badges found:', badges);

    const oauthBadges = badges.filter(b => b.includes('OAuth'));
    const basicAuthBadges = badges.filter(b => b.includes('Basic'));

    console.log(`   OAuth badges: ${oauthBadges.length}`);
    console.log(`   Basic Auth badges: ${basicAuthBadges.length}`);

    if (oauthBadges.length > 0) {
      console.log('   ✅ SUCCESS: OAuth badge is displaying correctly!');
      console.log(`   Badge text: "${oauthBadges[0]}"\n`);
    } else {
      console.log('   ❌ FAIL: OAuth badge not found\n');
    }

    // Take screenshot of badge
    await page.screenshot({ path: '.playwright-mcp/oauth-fix-test-02-badge-check.png', fullPage: true });

    // Step 4: Get configuration from API
    console.log('4. Getting configuration from API...');
    const apiConfig = await page.evaluate(async () => {
      const token = localStorage.getItem('token');
      const res = await fetch('http://localhost:5000/api/EmailConfiguration', {
        headers: { 'Authorization': `Bearer ${token}` }
      });
      return await res.json();
    });

    if (apiConfig.data && apiConfig.data.length > 0) {
      const config = apiConfig.data[0];
      console.log(`   Config ID: ${config.id}`);
      console.log(`   From Email: ${config.fromEmail}`);
      console.log(`   Auth Type: ${config.authenticationType} (1=Basic, 2=OAuth2)`);
      console.log(`   Is Enabled: ${config.isEnabled}`);
      console.log(`   Last Polled: ${config.lastPolledAt || 'Never'}\n`);
    }

    // Step 5: Test Poll Now button
    console.log('5. Testing Poll Now Button...');

    // Set up network request monitoring
    let pollRequestMade = false;
    let pollRequestUrl = '';
    let pollResponseStatus = 0;
    let pollResponseBody = null;

    page.on('request', request => {
      if (request.url().includes('/poll-now')) {
        pollRequestMade = true;
        pollRequestUrl = request.url();
        console.log(`   📤 Request: ${request.method()} ${pollRequestUrl}`);
      }
    });

    page.on('response', async response => {
      if (response.url().includes('/poll-now')) {
        pollResponseStatus = response.status();
        console.log(`   📥 Response: ${pollResponseStatus}`);
        try {
          pollResponseBody = await response.json();
          console.log(`   Response data:`, JSON.stringify(pollResponseBody, null, 2));
        } catch (e) {
          console.log(`   Response: ${await response.text()}`);
        }
      }
    });

    // Find and click Poll Now button
    const pollButton = page.locator('button:has-text("Poll Now")').first();
    const isVisible = await pollButton.isVisible();
    const isDisabled = await pollButton.isDisabled();

    console.log(`   Poll Now button visible: ${isVisible}`);
    console.log(`   Poll Now button disabled: ${isDisabled}`);

    if (!isVisible) {
      console.log('   ❌ FAIL: Poll Now button not visible\n');
    } else if (isDisabled) {
      console.log('   ⚠️  WARNING: Poll Now button is disabled (config might not be enabled)\n');
    } else {
      console.log('   Clicking Poll Now button...');
      await page.screenshot({ path: '.playwright-mcp/oauth-fix-test-03-before-poll.png', fullPage: true });

      await pollButton.click();
      console.log('   Button clicked, waiting for response...');

      // Wait for request/response
      await page.waitForTimeout(15000);

      await page.screenshot({ path: '.playwright-mcp/oauth-fix-test-04-after-poll.png', fullPage: true });

      if (pollRequestMade) {
        console.log(`   ✅ SUCCESS: API request was made to ${pollRequestUrl}`);
        console.log(`   Response status: ${pollResponseStatus}`);

        if (pollResponseStatus === 200) {
          console.log('   ✅ Poll completed successfully!');
          if (pollResponseBody && pollResponseBody.data) {
            console.log(`   Emails fetched: ${pollResponseBody.data.emailsFetched || 0}`);
            console.log(`   Complaints created: ${pollResponseBody.data.complaintsCreated || 0}`);
          }
        } else {
          console.log(`   ⚠️  Poll request returned status ${pollResponseStatus}`);
        }
      } else {
        console.log('   ❌ FAIL: No API request was made\n');
      }

      // Check for any toast messages
      const toasts = await page.locator('.toast, .alert, [role="alert"]').allTextContents();
      if (toasts.length > 0) {
        console.log('\n   Toast messages:');
        toasts.forEach(toast => console.log(`   - ${toast}`));
      }
    }

    console.log('\n=== Test Complete ===');
    console.log('Browser will stay open for 30 seconds for inspection...');
    await page.waitForTimeout(30000);

  } catch (error) {
    console.error('\n❌ Error during test:', error.message);
    await page.screenshot({ path: '.playwright-mcp/oauth-fix-test-error.png', fullPage: true });
  } finally {
    await browser.close();
  }
})();
