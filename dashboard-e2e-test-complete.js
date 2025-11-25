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

const screenshotDir = '.playwright-mcp/dashboard-e2e-final';

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
    console.log('\n╔════════════════════════════════════════════════════════════════════╗');
    console.log('║   COMPREHENSIVE DASHBOARD E2E TEST SUITE                           ║');
    console.log('║   Role-Based Statistics Filtering Verification                     ║');
    console.log('╚════════════════════════════════════════════════════════════════════╝\n');
    console.log(`Timestamp: ${new Date().toLocaleString()}\n`);

    // Step 1: Verify backend API for all users
    console.log('═══ BACKEND API VERIFICATION ═══\n');
    const apiResults = {};

    for (const user of testUsers) {
        console.log(`┌─ Testing API for ${user.name} (${user.email})`);

        const token = await getAuthToken(user.email, user.password);
        if (token) {
            console.log('├─ ✅ Authentication successful');

            const stats = await getDashboardStatistics(token);
            if (stats) {
                console.log('├─ ✅ Dashboard statistics retrieved');
                console.log(`│  ├─ Total: ${stats.total}`);
                Object.keys(stats).forEach(key => {
                    if (key !== 'total' && key !== 'statusWidgets') {
                        console.log(`│  ├─ ${key}: ${stats[key]}`);
                    }
                });
            }

            const complaints = await getComplaints(token);
            const complaintCount = Array.isArray(complaints) ? complaints.length : (complaints ? 1 : 0);
            console.log(`└─ ✅ Complaints retrieved: ${complaintCount} items\n`);

            apiResults[user.name] = {
                token: token,
                statistics: stats,
                complaints: complaints,
                complaintCount: complaintCount
            };
        } else {
            console.log('└─ ❌ Authentication failed\n');
        }
    }

    // Step 2: Frontend E2E Testing with Playwright
    console.log('\n═══ FRONTEND E2E TESTING WITH PLAYWRIGHT ═══\n');

    const browser = await chromium.launch({
        headless: false,
        slowMo: 300
    });

    const context = await browser.newContext({
        viewport: { width: 1920, height: 1080 }
    });

    const page = await context.newPage();
    const testResults = [];

    for (const user of testUsers) {
        console.log(`\n╔═══ Testing ${user.name} Dashboard ═══╗\n`);

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
            console.log('  [1/8] Navigating to login page...');
            await page.goto(`${baseUrl}/auth/login`, { waitUntil: 'networkidle', timeout: 30000 });
            await page.waitForTimeout(1000);

            const loginScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-01-login-page.png`;
            await page.screenshot({ path: loginScreenshot, fullPage: true });
            testResult.screenshots.push(loginScreenshot);
            console.log(`        Screenshot: ${loginScreenshot}`);

            // Fill login form
            console.log('  [2/8] Filling login form...');
            await page.fill('input[type="email"], input[formControlName="email"]', user.email);
            await page.fill('input[type="password"], input[formControlName="password"]', user.password);
            await page.waitForTimeout(500);

            const formScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-02-login-form-filled.png`;
            await page.screenshot({ path: formScreenshot, fullPage: true });
            testResult.screenshots.push(formScreenshot);

            // Click login button
            console.log('  [3/8] Logging in...');
            await page.click('button[type="submit"]');

            // Wait for navigation to dashboard
            await page.waitForURL('**/dashboard', { timeout: 15000 });
            await page.waitForTimeout(4000); // Wait for widgets to load

            const dashboardScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-03-dashboard-loaded.png`;
            await page.screenshot({ path: dashboardScreenshot, fullPage: true });
            testResult.screenshots.push(dashboardScreenshot);
            console.log('        ✅ Dashboard loaded successfully');

            testResult.tests.push({
                name: 'Login and Dashboard Load',
                status: 'PASS',
                message: 'Successfully logged in and dashboard loaded'
            });

            // Extract dashboard statistics using proper selectors for status-widget component
            console.log('  [4/8] Extracting dashboard statistics...');

            const dashboardStats = await page.evaluate(() => {
                const stats = {};

                // Find all status-widget components
                const widgets = document.querySelectorAll('.status-widget');

                widgets.forEach(widget => {
                    // Get the status name from widget-title
                    const titleEl = widget.querySelector('.widget-title');
                    const statusName = titleEl ? titleEl.textContent.trim() : null;

                    // Get the current count from count-number
                    const countEl = widget.querySelector('.count-number');
                    const count = countEl ? parseInt(countEl.textContent.trim()) : 0;

                    if (statusName) {
                        // Normalize status name to camelCase
                        const normalizedName = statusName
                            .toLowerCase()
                            .replace(/\s+(.)/g, (match, char) => char.toUpperCase());

                        stats[normalizedName] = count;
                    }
                });

                // Calculate total
                stats.total = Object.values(stats).reduce((sum, val) => sum + val, 0);

                return stats;
            });

            console.log('        Dashboard Statistics:', dashboardStats);
            testResult.dashboardStats = dashboardStats;

            // Extract complaint list count
            console.log('  [5/8] Counting complaints in recent complaints list...');

            const complaintListCount = await page.evaluate(() => {
                // Look for "X results" text
                const resultsText = Array.from(document.querySelectorAll('*')).find(el => {
                    const text = el.textContent;
                    return text && text.match(/^\d+\s+results?$/i);
                });

                if (resultsText) {
                    const match = resultsText.textContent.match(/^(\d+)/);
                    return match ? parseInt(match[1]) : 0;
                }

                // Fallback: Count CMP- complaint IDs
                const cmpMatches = document.body.innerHTML.match(/CMP-\d{4}-\d+/g);
                return cmpMatches ? new Set(cmpMatches).size : 0;
            });

            console.log(`        Complaint List Count: ${complaintListCount}`);
            testResult.complaintListCount = complaintListCount;

            // Scroll to see full dashboard
            await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
            await page.waitForTimeout(1000);
            await page.evaluate(() => window.scrollTo(0, 0));
            await page.waitForTimeout(500);

            const statsScreenshot = `${screenshotDir}/${user.name.toLowerCase()}-04-dashboard-with-statistics.png`;
            await page.screenshot({ path: statsScreenshot, fullPage: true });
            testResult.screenshots.push(statsScreenshot);

            // Verify statistics count
            console.log('  [6/8] Verifying statistics count...');
            const expectedTotal = user.expectedComplaints;
            const actualTotal = dashboardStats.total || 0;

            if (actualTotal === expectedTotal) {
                testResult.tests.push({
                    name: 'Statistics Count Verification',
                    status: 'PASS',
                    message: `Expected ${expectedTotal} complaints, found ${actualTotal}`
                });
                console.log(`        ✅ PASS: Statistics count matches (${actualTotal})`);
            } else {
                testResult.tests.push({
                    name: 'Statistics Count Verification',
                    status: 'FAIL',
                    message: `Expected ${expectedTotal} complaints, but found ${actualTotal}`
                });
                console.log(`        ❌ FAIL: Expected ${expectedTotal}, found ${actualTotal}`);
            }

            // Verify complaint list count
            console.log('  [7/8] Verifying complaint list count...');
            if (complaintListCount === expectedTotal) {
                testResult.tests.push({
                    name: 'Complaint List Verification',
                    status: 'PASS',
                    message: `Complaint list shows ${complaintListCount} items as expected`
                });
                console.log(`        ✅ PASS: Complaint list count matches (${complaintListCount})`);
            } else {
                testResult.tests.push({
                    name: 'Complaint List Verification',
                    status: 'WARN',
                    message: `Expected ${expectedTotal} complaints in list, but found ${complaintListCount}`
                });
                console.log(`        ⚠️  WARN: Expected ${expectedTotal}, found ${complaintListCount}`);
            }

            // Compare with API results
            console.log('  [8/8] Comparing with API results...');
            const apiData = apiResults[user.name];
            if (apiData && apiData.statistics) {
                const apiStats = apiData.statistics;

                // Compare total
                const totalMatch = dashboardStats.total === apiStats.total;

                testResult.tests.push({
                    name: 'API-Frontend Consistency',
                    status: totalMatch ? 'PASS' : 'FAIL',
                    message: totalMatch ?
                        `Dashboard statistics match API response (Total: ${apiStats.total})` :
                        `Dashboard shows ${dashboardStats.total}, API shows ${apiStats.total}`,
                    apiStats: apiStats,
                    dashboardStats: dashboardStats
                });

                if (totalMatch) {
                    console.log(`        ✅ PASS: Dashboard matches API (Total: ${apiStats.total})`);
                } else {
                    console.log(`        ❌ FAIL: Dashboard=${dashboardStats.total}, API=${apiStats.total}`);
                }
            }

            // Logout
            await page.evaluate(() => {
                localStorage.clear();
                sessionStorage.clear();
            });

            console.log(`\n╚═══ ${user.name} Test Complete ═══╝\n`);

        } catch (error) {
            console.error(`❌ Error testing ${user.name} dashboard:`, error.message);
            testResult.tests.push({
                name: 'Dashboard Test',
                status: 'ERROR',
                message: error.message
            });

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
        timestamp: new Date().toISOString(),
        summary: {
            totalTests: 0,
            passed: 0,
            failed: 0,
            warnings: 0,
            errors: 0
        }
    };

    testResults.forEach(result => {
        result.tests.forEach(test => {
            resultsJson.summary.totalTests++;
            if (test.status === 'PASS') resultsJson.summary.passed++;
            else if (test.status === 'FAIL') resultsJson.summary.failed++;
            else if (test.status === 'WARN') resultsJson.summary.warnings++;
            else if (test.status === 'ERROR') resultsJson.summary.errors++;
        });
    });

    fs.writeFileSync('dashboard-e2e-results-final.json', JSON.stringify(resultsJson, null, 2));

    // Generate summary
    console.log('\n╔════════════════════════════════════════════════════════════════════╗');
    console.log('║                        TEST SUMMARY                                ║');
    console.log('╚════════════════════════════════════════════════════════════════════╝\n');

    testResults.forEach(result => {
        console.log(`\n┌─ ${result.role} (${result.email})`);
        console.log(`│  Dashboard Total: ${result.dashboardStats.total || 0}`);
        console.log(`│  Complaint List: ${result.complaintListCount}`);
        console.log('│  Test Results:');

        result.tests.forEach(test => {
            const icon = test.status === 'PASS' ? '✅' :
                        test.status === 'FAIL' ? '❌' :
                        test.status === 'WARN' ? '⚠️' : '🔴';
            console.log(`│  ${icon} ${test.name}: ${test.message}`);
        });
        console.log('└─');
    });

    console.log(`\n╔═══ FINAL RESULTS ═══╗`);
    console.log(`║ Total Tests: ${resultsJson.summary.totalTests}`);
    console.log(`║ Passed:      ${resultsJson.summary.passed}`);
    console.log(`║ Failed:      ${resultsJson.summary.failed}`);
    console.log(`║ Warnings:    ${resultsJson.summary.warnings}`);
    console.log(`║ Errors:      ${resultsJson.summary.errors}`);
    console.log(`║ Success Rate: ${((resultsJson.summary.passed / resultsJson.summary.totalTests) * 100).toFixed(2)}%`);
    console.log(`╚═════════════════════╝\n`);

    console.log(`Results saved to: dashboard-e2e-results-final.json`);
    console.log(`Screenshots saved to: ${screenshotDir}/\n`);

    return resultsJson;
}

// Run the tests
runTests().catch(console.error);
