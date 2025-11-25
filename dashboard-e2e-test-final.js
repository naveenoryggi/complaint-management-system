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
    console.log('\n=== COMPREHENSIVE DASHBOARD E2E TEST SUITE ===');
    console.log('Testing Role-Based Statistics Filtering');
    console.log('Timestamp:', new Date().toISOString());
    console.log('');

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
                console.log(`    Submitted: ${stats.submitted || 0}`);
                console.log(`    Under Review: ${stats.underReview || 0}`);
                console.log(`    In Progress: ${stats.inProgress || 0}`);
                console.log(`    Resolved: ${stats.resolved || 0}`);
                console.log(`    Closed: ${stats.closed || 0}`);
                console.log(`    Escalated: ${stats.escalated || 0}`);
                console.log(`    Pending Info: ${stats.pendingInfo || 0}`);
                console.log(`    Rejected: ${stats.rejected || 0}`);
                console.log(`    Reopened: ${stats.reopened || 0}`);
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
        slowMo: 300
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

            // Extract dashboard statistics with improved selectors
            console.log('5. Extracting dashboard statistics...');
            await page.waitForTimeout(2000);

            const dashboardStats = await page.evaluate(() => {
                const stats = {};

                // Get all stat cards
                const statCards = document.querySelectorAll('.stat-card, .statistics-card, mat-card, .dashboard-stat, [class*="card"]');

                statCards.forEach(card => {
                    const cardText = card.innerText || card.textContent;
                    const cardHtml = card.innerHTML;

                    // Extract status name and count
                    const lines = cardText.split('\n').map(l => l.trim()).filter(l => l);

                    // Look for patterns like "Submitted" followed by a number
                    for (let i = 0; i < lines.length; i++) {
                        const line = lines[i];
                        const nextLine = lines[i + 1];

                        // Check if this line is a status name
                        if (line.match(/^(Test|Submitted|Under Review|In Progress|Resolved|Closed|Escalated|Pending Info|Rejected|Reopened)$/i)) {
                            const statusName = line.toLowerCase().replace(/\s+/g, '');

                            // Look for number in next lines
                            for (let j = i + 1; j < Math.min(i + 5, lines.length); j++) {
                                const numMatch = lines[j].match(/^(\d+)$/);
                                if (numMatch) {
                                    const count = parseInt(numMatch[1]);

                                    // Map status names to keys
                                    const statusMap = {
                                        'test': 'test',
                                        'submitted': 'submitted',
                                        'underreview': 'underReview',
                                        'inprogress': 'inProgress',
                                        'resolved': 'resolved',
                                        'closed': 'closed',
                                        'escalated': 'escalated',
                                        'pendinginfo': 'pendingInfo',
                                        'rejected': 'rejected',
                                        'reopened': 'reopened'
                                    };

                                    const key = statusMap[statusName];
                                    if (key) {
                                        stats[key] = count;
                                    }
                                    break;
                                }
                            }
                        }
                    }
                });

                // Calculate total from all statuses
                stats.total = (stats.test || 0) +
                             (stats.submitted || 0) +
                             (stats.underReview || 0) +
                             (stats.inProgress || 0) +
                             (stats.resolved || 0) +
                             (stats.closed || 0) +
                             (stats.escalated || 0) +
                             (stats.pendingInfo || 0) +
                             (stats.rejected || 0) +
                             (stats.reopened || 0);

                return stats;
            });

            console.log('   Dashboard statistics:', dashboardStats);
            testResult.dashboardStats = dashboardStats;

            // Extract complaint list count from Recent Complaints section
            console.log('6. Checking complaint list...');
            const complaintListCount = await page.evaluate(() => {
                // Count complaint cards in the Recent Complaints section
                const complaintCards = document.querySelectorAll('.complaint-card, [class*="complaint-item"], mat-card:has(.complaint)');

                // Also try looking for table rows
                const tableRows = document.querySelectorAll('table tbody tr:not(.mat-mdc-no-data-row):not(:empty)');

                // Also count CMP- prefixed items
                const cmpItems = Array.from(document.querySelectorAll('*')).filter(el => {
                    const text = el.innerText || el.textContent;
                    return text && text.match(/CMP-\d{4}-\d+/);
                });

                // Use the maximum count found
                return Math.max(complaintCards.length, tableRows.length, cmpItems.length > 0 ? cmpItems.length : 0);
            });

            console.log(`   Complaint list count: ${complaintListCount}`);
            testResult.complaintListCount = complaintListCount;

            // Scroll down to see complaints
            await page.evaluate(() => window.scrollTo(0, document.body.scrollHeight));
            await page.waitForTimeout(1000);

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

            // Verify complaint list count
            if (complaintListCount === expectedTotal) {
                testResult.tests.push({
                    name: 'Complaint List Verification',
                    status: 'PASS',
                    message: `Complaint list shows ${complaintListCount} items as expected`
                });
                console.log('   ✅ PASS: Complaint list count matches expected');
            } else {
                testResult.tests.push({
                    name: 'Complaint List Verification',
                    status: 'FAIL',
                    message: `Expected ${expectedTotal} complaints in list, but found ${complaintListCount}`
                });
                console.log('   ❌ FAIL: Complaint list count mismatch');
            }

            // Compare with API results
            const apiData = apiResults[user.name];
            if (apiData && apiData.statistics) {
                const apiStats = apiData.statistics;

                // Compare key statistics
                const statsMatch = (
                    dashboardStats.total === apiStats.total &&
                    (dashboardStats.submitted || 0) === (apiStats.submitted || 0) &&
                    (dashboardStats.underReview || 0) === (apiStats.underReview || 0) &&
                    (dashboardStats.inProgress || 0) === (apiStats.inProgress || 0) &&
                    (dashboardStats.resolved || 0) === (apiStats.resolved || 0) &&
                    (dashboardStats.closed || 0) === (apiStats.closed || 0)
                );

                testResult.tests.push({
                    name: 'API-Frontend Consistency',
                    status: statsMatch ? 'PASS' : 'FAIL',
                    message: statsMatch ?
                        'Dashboard statistics match API response' :
                        'Dashboard statistics do not match API response',
                    apiStats: apiStats,
                    dashboardStats: dashboardStats
                });

                if (statsMatch) {
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
                await page.evaluate(() => {
                    localStorage.clear();
                    sessionStorage.clear();
                });
                await page.waitForTimeout(500);
            } catch (e) {
                console.log('   Error during logout:', e.message);
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
        console.log(`  Dashboard Total: ${result.dashboardStats.total || 0}`);
        console.log(`  Complaint List: ${result.complaintListCount}`);
        console.log('');
    });

    console.log(`\nTotal: ${totalTests} | Passed: ${passedTests} | Failed: ${failedTests}`);
    console.log(`Success Rate: ${((passedTests / totalTests) * 100).toFixed(2)}%\n`);

    return resultsJson;
}

// Run the tests
runTests().catch(console.error);
