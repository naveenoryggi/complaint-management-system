const { chromium } = require('playwright');

async function analyzeDashboard() {
    const browser = await chromium.launch({ headless: false, slowMo: 500 });
    const page = await browser.newPage();

    // Login as admin
    await page.goto('http://localhost:4200/auth/login');
    await page.fill('input[type="email"]', 'admin@complaintmanagement.com');
    await page.fill('input[type="password"]', 'Admin@123');
    await page.click('button[type="submit"]');

    await page.waitForURL('**/dashboard', { timeout: 15000 });
    await page.waitForTimeout(3000);

    // Get the HTML structure
    const dashboardHTML = await page.evaluate(() => {
        const statsSection = document.querySelector('[class*="statistics"], [class*="dashboard"]');
        return statsSection ? statsSection.outerHTML : document.body.innerHTML;
    });

    console.log('Dashboard HTML structure (first 5000 chars):');
    console.log(dashboardHTML.substring(0, 5000));

    // Try to get statistics with a more direct approach
    const stats = await page.evaluate(() => {
        const result = {};

        // Method 1: Look for all elements that might contain statistics
        const allElements = document.querySelectorAll('*');

        allElements.forEach(el => {
            const text = el.textContent;

            // Check if element contains "Submitted" and a number
            if (text && text.includes('Submitted')) {
                const parentText = el.parentElement ? el.parentElement.textContent : '';
                console.log('Found "Submitted" element:', {
                    text: text.substring(0, 100),
                    parentText: parentText.substring(0, 100),
                    className: el.className,
                    tagName: el.tagName
                });
            }
        });

        // Method 2: Try to find mat-card elements
        const cards = document.querySelectorAll('mat-card');
        console.log(`Found ${cards.length} mat-card elements`);

        cards.forEach((card, index) => {
            console.log(`Card ${index}:`, card.textContent.substring(0, 150));
        });

        return result;
    });

    console.log('\nExtracted stats:', stats);

    // Take screenshot
    await page.screenshot({ path: 'dashboard-analysis.png', fullPage: true });
    console.log('Screenshot saved to dashboard-analysis.png');

    // Wait before closing
    await page.waitForTimeout(10000);
    await browser.close();
}

analyzeDashboard().catch(console.error);
