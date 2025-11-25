const { chromium } = require('playwright');
const fs = require('fs');
const axios = require('axios');

const baseUrl = 'http://localhost:4200';
const apiUrl = 'http://localhost:5000/api';

const testUsers = [
    {
        name: 'Complainant',
        email: 'nav_nainital@yahoo.com',
        password: 'Nav@12345',
        expectedComplaints: 5,
        expectedRole: 'Complainant'
    },
    {
        name: 'Admin',
        email: 'admin@complaintmanagement.com',
        password: 'Admin@123',
        expectedComplaints: 5,
        expectedRole: 'Admin'
    },
    {
        name: 'Handler',
        email: 'naveen.chandra@oryggitech.com',
        password: 'Naveen@12345',
        expectedComplaints: 0,
        expectedRole: 'Handler'
    }
];

const screenshotDir = '.playwright-mcp/dashboard-e2e';

// Create screenshot directory
if (!fs.existsSync(screenshotDir)) {
    fs.mkdirSync(screenshotDir, { recursive: true });
}

async function getAuthToken(email, password) {
    try {
        const response = await axios.post(`${apiUrl}/Auth/login`, {
            email: email,
            password: password
        });
        return response.data.token;
    } catch (error) {
        console.error(`Failed to get auth token for ${email}:`, error.response?.data || error.message);
        return null;
    }
}

async function getDashboardStatistics(token) {
    try {
        const response = await axios.get(`${apiUrl}/Complaints/dashboard-statistics`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        return response.data;
    } catch (error) {
        console.error('Failed to get dashboard statistics:', error.response?.data || error.message);
        return null;
    }
}

async function getComplaints(token) {
    try {
        const response = await axios.get(`${apiUrl}/Complaints`, {
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });
        return response.data;
    } catch (error) {
        console.error('Failed to get complaints:', error.response?.data || error.message);
        return null;
    }
}

async function runTests() {
    console.log('\n=== COMPREHENSIVE DASHBOARD E2E TEST SUITE ===\n');

    // Step 1: Verify backend API for all users
    console.log('=== BACKEND API VERIFICATION ===\n');
    const apiResults = {};

    for (const user of testUsers) {
        console.log(`Testing API for ${user.name} (${user.email})...`);

        const token = await getAuthToken(user.email, user.password);
        if (token) {
            console.log('  ✅ Authentication successful');

            const stats = await getDashboardStatistics(token);
            if (stats) {
                console.log('  ✅ Dashboard statistics retrieved');
                console.log(`    Total: ${stats.total}`);
                console.log(`    Open: ${stats.open}`);
                console.log(`    In Progress: ${stats.inProgress}`);
                console.log(`    Resolved: ${stats.resolved}`);
                console.log(`    Closed: ${stats.closed}`);
            }

            const complaints = await getComplaints(token);
            const complaintCount = Array.isArray(complaints) ? complaints.length : (complaints ? 1 : 0);
            console.log(`  ✅ Complaints retrieved: ${complaintCount} items`);

            apiResults[user.name] = {
                token: token,
                statistics: stats,
                complaints: complaints,
                complaintCount: complaintCount
            };
        } else {
            console.log('  ❌ Authentication failed');
        }
        console.log('');
    }

    // Step 2: Frontend E2E Testing with Playwright
    console.log('\n=== FRONTEND E2E TESTING WITH PLAYWRIGHT ===\n');

    const browser = await chromium.launch({
        headless: false,
        slowMo: 500
    });

    const context = await browser.newContext({
        viewport: { width: 1920, height: 1080 }
    });

    const page = await context.newPage();
    const testResults = [];

    for (const user of testUsers) {
        console.log(`\n=== Testing ${user.name} Dashboard ===`);

        const testResult = {
            role: user.name,
            email: user.email,
            tests: [],
            screenshots: [],
            dashboardStats: {},
            complaintListCount: 0,
            roleIndicator: null
        };

        try {
            // Navigate to login page
            console.log('1. Navigating to login page...');
            await page.goto(`${baseUrl}/auth/login`, { waitUntil: 'networkidle', timeout: 30000 });
            await page.waitForTimeout(1000);

            // Take screenshot of login page
            const loginScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-01-login-page.png`;
            await page.screenshot({ path: loginScreenshot, fullPage: true });
            testResult.screenshots.push(loginScreenshot);
            console.log(`   Screenshot: ${loginScreenshot}`);

            // Fill login form
            console.log('2. Filling login form...');
            await page.fill('input[type="email"], input[formControlName="email"]', user.email);
            await page.fill('input[type="password"], input[formControlName="password"]', user.password);
            await page.waitForTimeout(500);

            // Take screenshot of filled form
            const formScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-02-login-form-filled.png`;
            await page.screenshot({ path: formScreenshot, fullPage: true });
            testResult.screenshots.push(formScreenshot);
            console.log(`   Screenshot: ${formScreenshot}`);

            // Click login button
            console.log('3. Clicking login button...');
            await page.click('button[type="submit"]');

            // Wait for navigation to dashboard
            console.log('4. Waiting for dashboard...');
            await page.waitForURL('**/dashboard', { timeout: 15000 });
            await page.waitForTimeout(3000); // Wait for data to load

            // Take screenshot of dashboard
            const dashboardScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-03-dashboard-loaded.png`;
            await page.screenshot({ path: dashboardScreenshot, fullPage: true });
            testResult.screenshots.push(dashboardScreenshot);
            console.log(`   Screenshot: ${dashboardScreenshot}`);

            testResult.tests.push({
                name: 'Login and Dashboard Load',
                status: 'PASS',
                message: 'Successfully logged in and dashboard loaded'
            });

            // Extract dashboard statistics
            console.log('5. Extracting dashboard statistics...');
            await page.waitForTimeout(2000);

            const dashboardStats = await page.evaluate(() => {
                const stats = {};

                // Try multiple selectors for statistics
                const allText = document.body.innerText;

                // Look for numbers near keywords
                const totalMatch = allText.match(/Total.*?(\d+)/i) || allText.match(/(\d+).*?Total/i);
                if (totalMatch) stats.total = parseInt(totalMatch[1]);

                const openMatch = allText.match(/Open.*?(\d+)/i) || allText.match(/(\d+).*?Open/i);
                if (openMatch) stats.open = parseInt(openMatch[1]);

                const inProgressMatch = allText.match(/In Progress.*?(\d+)/i) || allText.match(/(\d+).*?In Progress/i);
                if (inProgressMatch) stats.inProgress = parseInt(inProgressMatch[1]);

                const resolvedMatch = allText.match(/Resolved.*?(\d+)/i) || allText.match(/(\d+).*?Resolved/i);
                if (resolvedMatch) stats.resolved = parseInt(resolvedMatch[1]);

                const closedMatch = allText.match(/Closed.*?(\d+)/i) || allText.match(/(\d+).*?Closed/i);
                if (closedMatch) stats.closed = parseInt(closedMatch[1]);

                return stats;
            });

            console.log('   Dashboard statistics:', dashboardStats);
            testResult.dashboardStats = dashboardStats;

            // Extract complaint list count
            console.log('6. Checking complaint list...');
            const complaintListCount = await page.evaluate(() => {
                const rows = document.querySelectorAll('table tbody tr:not(.mat-mdc-no-data-row), .complaint-item, mat-list-item.complaint');
                return rows.length;
            });

            console.log(`   Complaint list count: ${complaintListCount}`);
            testResult.complaintListCount = complaintListCount;

            // Take final screenshot with statistics visible
            const statsScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-04-dashboard-with-statistics.png`;
            await page.screenshot({ path: statsScreenshot, fullPage: true });
            testResult.screenshots.push(statsScreenshot);
            console.log(`   Screenshot: ${statsScreenshot}`);

            // Verify statistics count
            const expectedTotal = user.expectedComplaints;
            const actualTotal = dashboardStats.total || 0;

            console.log(`7. Verifying statistics: Expected ${expectedTotal}, Found ${actualTotal}`);

            if (actualTotal === expectedTotal) {
                testResult.tests.push({
                    name: 'Statistics Count Verification',
                    status: 'PASS',
                    message: `Expected ${expectedTotal} complaints, found ${actualTotal}`
                });
                console.log('   ✅ PASS: Statistics count matches expected');
            } else {
                testResult.tests.push({
                    name: 'Statistics Count Verification',
                    status: 'FAIL',
                    message: `Expected ${expectedTotal} complaints, but found ${actualTotal}`
                });
                console.log('   ❌ FAIL: Statistics count mismatch');
            }

            // Compare with API results
            const apiData = apiResults[user.name];
            if (apiData && apiData.statistics) {
                const apiStats = apiData.statistics;
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

                if (apiMatch) {
                    console.log('   ✅ PASS: Dashboard matches API response');
                } else {
                    console.log('   ❌ FAIL: Dashboard does not match API');
                    console.log('   API:', apiStats);
                    console.log('   Dashboard:', dashboardStats);
                }
            }

            // Logout
            console.log('8. Logging out...');
            try {
                // Try to find and click logout
                const logoutSelectors = [
                    'button:has-text("Logout")',
                    'a:has-text("Logout")',
                    '[mat-menu-item]:has-text("Logout")',
                    '.logout-button'
                ];

                let loggedOut = false;
                for (const selector of logoutSelectors) {
                    try {
                        const element = await page.waitForSelector(selector, { timeout: 2000 });
                        if (element) {
                            await element.click();
                            loggedOut = true;
                            break;
                        }
                    } catch (e) {
                        // Try next selector
                    }
                }

                if (!loggedOut) {
                    console.log('   Logout button not found, clearing storage...');
                    await page.evaluate(() => {
                        localStorage.clear();
                        sessionStorage.clear();
                    });
                }

                await page.waitForTimeout(1000);
            } catch (e) {
                console.log('   Navigating to login page...');
                await page.goto(`${baseUrl}/auth/login`);
            }

            console.log(`✅ ${user.name} dashboard test completed\n`);

        } catch (error) {
            console.error(`❌ Error testing ${user.name} dashboard:`, error.message);
            testResult.tests.push({
                name: 'Dashboard Test',
                status: 'ERROR',
                message: error.message
            });

            // Take error screenshot
            const errorScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-error.png`;
            await page.screenshot({ path: errorScreenshot, fullPage: true });
            testResult.screenshots.push(errorScreenshot);
        }

        testResults.push(testResult);
    }

    await browser.close();

    // Save results to JSON
    const resultsJson = {
        testResults: testResults,
        apiResults: apiResults,
        timestamp: new Date().toISOString()
    };

    fs.writeFileSync('dashboard-e2e-results.json', JSON.stringify(resultsJson, null, 2));
    console.log('\n✅ Test results saved to dashboard-e2e-results.json\n');

    // Generate summary
    console.log('\n=== TEST SUMMARY ===\n');
    let totalTests = 0;
    let passedTests = 0;
    let failedTests = 0;

    testResults.forEach(result => {
        console.log(`${result.role}:`);
        result.tests.forEach(test => {
            totalTests++;
            if (test.status === 'PASS') {
                passedTests++;
                console.log(`  ✅ ${test.name}: ${test.message}`);
            } else if (test.status === 'FAIL') {
                failedTests++;
                console.log(`  ❌ ${test.name}: ${test.message}`);
            } else {
                console.log(`  ⚠️  ${test.name}: ${test.message}`);
            }
        });
        console.log('');
    });

    console.log(`\nTotal: ${totalTests} | Passed: ${passedTests} | Failed: ${failedTests}`);
    console.log(`Success Rate: ${((passedTests / totalTests) * 100).toFixed(2)}%\n`);

    return resultsJson;
}

// Run the tests
runTests().catch(console.error);
