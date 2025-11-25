// Create a complaint through the Angular UI to ensure it appears in the list
const { chromium } = require('playwright');

async function createComplaintViaUI() {
    console.log('Creating test complaint via UI...\n');

    const browser = await chromium.launch({ headless: false });
    const context = await browser.newContext();
    const page = await context.newPage();

    try {
        // Login as complainant
        console.log('[1/3] Logging in as complainant...');
        await page.goto('http://localhost:4200/login');
        await page.waitForLoadState('networkidle');
        await page.fill('#email', 'test.complainant@e2e.local');
        await page.fill('#password', 'Nav@123');
        await page.click('button[type="submit"]');
        await page.waitForURL(/dashboard|complaints/, { timeout: 15000 });
        console.log('  ✓ Logged in\n');

        // Navigate to create complaint (if there's a create button)
        console.log('[2/3] Creating complaint...');
        // Try to find create complaint button or navigate directly
        try {
            await page.goto('http://localhost:4200/complaints/new', { waitUntil: 'networkidle', timeout: 10000 });
        } catch {
            // If no create page, might need to click a button
            await page.goto('http://localhost:4200/complaints');
            const createBtn = await page.$('button:has-text("Create"), button:has-text("New"), a:has-text("Create")');
            if (createBtn) {
                await createBtn.click();
                await page.waitForLoadState('networkidle');
            }
        }

        await page.waitForTimeout(2000);

        // Fill complaint form (assuming standard form fields)
        const titleField = await page.$('input[formControlName="title"], input[name="title"], #title');
        if (titleField) {
            await titleField.fill('E2E Test Complaint - System Performance Issue');

            const descField = await page.$('textarea[formControlName="description"], textarea[name="description"], #description');
            if (descField) {
                await descField.fill('This test complaint documents system performance degradation during peak hours. Response times exceed acceptable thresholds.');
            }

            // Submit
            const submitBtn = await page.$('button[type="submit"]:not([disabled])');
            if (submitBtn) {
                await submitBtn.click();
                await page.waitForTimeout(3000);
                console.log('  ✓ Complaint created\n');
            } else {
                console.log('  ⚠ Could not find submit button\n');
            }
        } else {
            console.log('  ⚠ Create complaint form not found - might already have complaints\n');
        }

        // Verify by checking complaints list
        console.log('[3/3] Verifying complaint in list...');
        await page.goto('http://localhost:4200/complaints');
        await page.waitForTimeout(2000);

        const complaintLinks = await page.$$('a[href*="/complaints/"]');
        console.log(`  Found ${complaintLinks.length} complaint link(s)\n`);

        if (complaintLinks.length > 0) {
            console.log('✅ SUCCESS! Complaints are now available for detail view test');
        } else {
            console.log('⚠ No complaint links found - detail view test may still fail');
        }

    } catch (error) {
        console.log(`❌ Error: ${error.message}`);
    } finally {
        await browser.close();
    }
}

createComplaintViaUI().catch(console.error);
