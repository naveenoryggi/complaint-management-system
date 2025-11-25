const { chromium } = require('playwright');
const fs = require('fs');

const testUsers = [
    {
        "Email":  "nav_nainital@yahoo.com",
        "Name":  "Complainant",
        "Password":  "Nav@12345",
        "ExpectedRole":  "Complainant",
        "ExpectedComplaints":  5
    },
    {
        "Email":  "admin@complaintmanagement.com",
        "Name":  "Admin",
        "Password":  "Admin@123",
        "ExpectedRole":  "Admin",
        "ExpectedComplaints":  5
    },
    {
        "Email":  "naveen.chandra@oryggitech.com",
        "Name":  "Handler",
        "Password":  "Naveen@12345",
        "ExpectedRole":  "Handler",
        "ExpectedComplaints":  0
    }
];
const apiResults = {

};

async function runTests() {
    const browser = await chromium.launch({ headless: false, slowMo: 500 });
    const context = await browser.newContext({
        viewport: { width: 1920, height: 1080 }
    });
    const page = await context.newPage();

    const testResults = [];

    for (const user of testUsers) {
        console.log(\\n=== Testing \ Dashboard ===\);

        const testResult = {
            role: user.Name,
            email: user.Email,
            tests: [],
            screenshots: []
        };

        try {
            // Navigate to login page
            console.log('Navigating to login page...');
            await page.goto('http://localhost:4200/auth/login', { waitUntil: 'networkidle' });
            await page.waitForTimeout(1000);

            // Take screenshot of login page
            const loginScreenshot = \$screenshotDir/\-01-login-page.png\;
            await page.screenshot({ path: loginScreenshot, fullPage: true });
            testResult.screenshots.push(loginScreenshot);
            console.log(\Screenshot saved: \\);

            // Fill login form
            console.log('Filling login form...');
            await page.fill('input[type="email"], input[formControlName="email"]', user.Email);
            await page.fill('input[type="password"], input[formControlName="password"]', user.Password);
            await page.waitForTimeout(500);

            // Take screenshot of filled form
            const formScreenshot = \$screenshotDir/\-02-login-form-filled.png\;
            await page.screenshot({ path: formScreenshot, fullPage: true });
            testResult.screenshots.push(formScreenshot);

            // Click login button
            console.log('Clicking login button...');
            await page.click('button[type="submit"]');

            // Wait for navigation to dashboard
            console.log('Waiting for dashboard...');
            await page.waitForURL('**/dashboard', { timeout: 10000 });
            await page.waitForTimeout(2000);

            // Take screenshot of dashboard
            const dashboardScreenshot = \$screenshotDir/\-03-dashboard-loaded.png\;
            await page.screenshot({ path: dashboardScreenshot, fullPage: true });
            testResult.screenshots.push(dashboardScreenshot);
            console.log(\Screenshot saved: \\);

            testResult.tests.push({
                name: 'Login and Dashboard Load',
                status: 'PASS',
                message: 'Successfully logged in and dashboard loaded'
            });

            // Wait for statistics to load
            console.log('Waiting for statistics widgets...');
            await page.waitForTimeout(2000);

            // Extract dashboard statistics
            console.log('Extracting dashboard statistics...');
            const dashboardStats = await page.evaluate(() => {
                const stats = {};

                // Try to find statistics cards/widgets
                const statElements = document.querySelectorAll('.stat-card, .statistics-card, .dashboard-stat, mat-card');

                statElements.forEach(el => {
                    const text = el.innerText || el.textContent;

                    // Look for total complaints
                    if (text.match(/total.*complaint/i)) {
                        const match = text.match(/(\d+)/);
                        if (match) stats.total = parseInt(match[1]);
                    }

                    // Look for open complaints
                    if (text.match(/open/i) && !text.match(/in progress/i)) {
                        const match = text.match(/(\d+)/);
                        if (match) stats.open = parseInt(match[1]);
                    }

                    // Look for in progress
                    if (text.match(/in.*progress/i)) {
                        const match = text.match(/(\d+)/);
                        if (match) stats.inProgress = parseInt(match[1]);
                    }

                    // Look for resolved
                    if (text.match(/resolved/i)) {
                        const match = text.match(/(\d+)/);
                        if (match) stats.resolved = parseInt(match[1]);
                    }

                    // Look for closed
                    if (text.match(/closed/i)) {
                        const match = text.match(/(\d+)/);
                        if (match) stats.closed = parseInt(match[1]);
                    }
                });

                return stats;
            });

            console.log('Dashboard statistics:', dashboardStats);
            testResult.dashboardStats = dashboardStats;

            // Extract complaint list count
            console.log('Checking complaint list...');
            const complaintListCount = await page.evaluate(() => {
                const rows = document.querySelectorAll('table tbody tr, .complaint-item, mat-list-item');
                return rows.length;
            });

            console.log(\Complaint list count: \\);
            testResult.complaintListCount = complaintListCount;

            // Take final screenshot with statistics visible
            const statsScreenshot = \$screenshotDir/\-04-dashboard-with-statistics.png\;
            await page.screenshot({ path: statsScreenshot, fullPage: true });
            testResult.screenshots.push(statsScreenshot);

            // Verify role indicator
            const roleIndicator = await page.evaluate(() => {
                const roleElement = document.querySelector('.user-role, .role-badge, [class*="role"]');
                return roleElement ? roleElement.innerText : null;
            });

            console.log(\Role indicator: \\);
            testResult.roleIndicator = roleIndicator;

            // Compare with expected values
            const expectedTotal = user.ExpectedComplaints;
            const actualTotal = dashboardStats.total || 0;

            if (actualTotal === expectedTotal) {
                testResult.tests.push({
                    name: 'Statistics Count Verification',
                    status: 'PASS',
                    message: \Expected \ complaints, found \\
                });
            } else {
                testResult.tests.push({
                    name: 'Statistics Count Verification',
                    status: 'FAIL',
                    message: \Expected \ complaints, but found \\
                });
            }

            // Compare with API results
            const apiStats = apiResults[user.Name]?.Statistics;
            if (apiStats) {
                const apiMatch = (
                    dashboardStats.total === apiStats.total &&
                    dashboardStats.open === apiStats.open &&
                    dashboardStats.inProgress === apiStats.inProgress &&
                    dashboardStats.resolved === apiStats.resolved &&
                    dashboardStats.closed === apiStats.closed
                );

                testResult.tests.push({
                    name: 'API-Frontend Consistency',
                    status: apiMatch ? 'PASS' : 'FAIL',
                    message: apiMatch ?
                        'Dashboard statistics match API response' :
                        'Dashboard statistics do not match API response',
                    apiStats: apiStats,
                    dashboardStats: dashboardStats
                });
            }

            // Logout
            console.log('Logging out...');
            try {
                const logoutButton = await page.waitForSelector('button:has-text("Logout"), a:has-text("Logout"), [mat-menu-item]:has-text("Logout")', { timeout: 3000 });
                await logoutButton.click();
                await page.waitForTimeout(1000);
            } catch (e) {
                console.log('Logout button not found, navigating to login page...');
                await page.goto('http://localhost:4200/auth/login');
            }

        } catch (error) {
            console.error(\Error testing \ dashboard:\, error.message);
            testResult.tests.push({
                name: 'Dashboard Test',
                status: 'ERROR',
                message: error.message
            });
        }

        testResults.push(testResult);
    }

    await browser.close();

    // Save results to JSON
    fs.writeFileSync('dashboard-e2e-results.json', JSON.stringify(testResults, null, 2));
    console.log('\nTest results saved to dashboard-e2e-results.json');

    return testResults;
}

runTests().catch(console.error);
