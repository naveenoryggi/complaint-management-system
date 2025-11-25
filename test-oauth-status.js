const { chromium } = require('playwright');

(async () => {
  const browser = await chromium.launch({ headless: false });
  const context = await browser.newContext();
  const page = await context.newPage();

  console.log('=== Email Ticketing OAuth Status Test ===');

  try {
    // Navigate to login
    console.log('1. Navigating to login page...');
    await page.goto('http://localhost:4200/login');
    await page.waitForTimeout(2000);

    // Login as admin
    console.log('2. Logging in as admin...');
    await page.fill('input[type="email"]', 'admin@complaintmanagement.com');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button[type="submit"]');
    await page.waitForTimeout(3000);

    // Navigate to email ticketing config
    console.log('3. Navigating to Email Ticketing Config...');
    await page.goto('http://localhost:4200/admin/email-ticketing-config');
    await page.waitForTimeout(3000);

    // Take screenshot of initial state
    await page.screenshot({ path: '.playwright-mcp/oauth-status-01-initial.png', fullPage: true });
    console.log('Screenshot saved: oauth-status-01-initial.png');

    // Check authentication type badge
    const authBadges = await page.locator('.badge').allTextContents();
    console.log('\n=== Authentication Badges Found ===');
    authBadges.forEach((badge, i) => console.log(`Badge ${i + 1}: ${badge}`));

    // Check if OAuth badge is present
    const oauthBadge = await page.locator('.badge:has-text("OAuth")').count();
    const basicAuthBadge = await page.locator('.badge:has-text("Basic")').count();

    console.log(`\nOAuth badges: ${oauthBadge}`);
    console.log(`Basic Auth badges: ${basicAuthBadge}`);

    // Find the email configuration card
    const cards = await page.locator('.card').all();
    console.log(`\nTotal cards found: ${cards.length}`);

    if (cards.length > 0) {
      // Check first card
      console.log('\n4. Checking first configuration card...');
      const firstCard = cards[0];

      // Take screenshot of card
      await firstCard.screenshot({ path: '.playwright-mcp/oauth-status-02-card.png' });

      // Look for Poll Now button
      const pollNowButton = await page.locator('button:has-text("Poll Now")').first();
      const isPollNowVisible = await pollNowButton.isVisible().catch(() => false);

      console.log(`\nPoll Now button visible: ${isPollNowVisible}`);

      if (isPollNowVisible) {
        console.log('5. Clicking Poll Now button...');

        // Take screenshot before clicking
        await page.screenshot({ path: '.playwright-mcp/oauth-status-03-before-poll.png', fullPage: true });

        // Listen for network requests
        page.on('response', async (response) => {
          if (response.url().includes('/poll')) {
            console.log(`\nPoll API Response: ${response.status()}`);
            try {
              const body = await response.text();
              console.log(`Response body: ${body.substring(0, 200)}`);
            } catch (e) {}
          }
        });

        await pollNowButton.click();
        console.log('Poll Now clicked, waiting for response...');

        // Wait for loading state or response
        await page.waitForTimeout(10000);

        // Take screenshot after clicking
        await page.screenshot({ path: '.playwright-mcp/oauth-status-04-after-poll.png', fullPage: true });

        // Check for any success/error messages
        const toasts = await page.locator('.toast, .alert, .notification, [role="alert"]').allTextContents();
        if (toasts.length > 0) {
          console.log('\n=== Toast/Alert Messages ===');
          toasts.forEach(toast => console.log(toast));
        }
      }

      // Check backend API status
      console.log('\n6. Checking email configuration via API...');
      const response = await page.evaluate(async () => {
        const token = localStorage.getItem('token');
        const res = await fetch('http://localhost:5000/api/EmailConfiguration', {
          headers: { 'Authorization': `Bearer ${token}` }
        });
        return await res.json();
      });

      console.log('\n=== Email Configuration from API ===');
      if (response.data && response.data.length > 0) {
        const config = response.data[0];
        console.log(`ID: ${config.id}`);
        console.log(`From Email: ${config.fromEmail}`);
        console.log(`Authentication Type: ${config.authenticationType} (1=Basic, 2=OAuth2)`);
        console.log(`OAuth Client ID: ${config.oAuthClientId ? 'Set' : 'Not Set'}`);
        console.log(`OAuth Refresh Token: ${config.oAuthRefreshToken ? (config.oAuthRefreshToken.length + ' chars') : 'Missing'}`);
        console.log(`Token Expires At: ${config.oAuthTokenExpiresAt}`);
        console.log(`Is Enabled: ${config.isEnabled}`);
        console.log(`Last Polled At: ${config.lastPolledAt}`);
      } else {
        console.log('No configurations found or API error');
        console.log(JSON.stringify(response, null, 2));
      }
    }

    // Keep browser open for inspection
    console.log('\n=== Test Complete - Browser will stay open for 30 seconds ===');
    await page.waitForTimeout(30000);

  } catch (error) {
    console.error('Error during test:', error.message);
    await page.screenshot({ path: '.playwright-mcp/oauth-status-error.png', fullPage: true });
  } finally {
    await browser.close();
  }
})();
