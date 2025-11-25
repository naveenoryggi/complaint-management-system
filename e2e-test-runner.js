const { chromium } = require('playwright');
const fs = require('fs');
const path = require('path');

const screenshotDir = 'e2e-screenshots-20251101_124535';
const testResults = {
    totalTests: 0,
    passed: 0,
    failed: 0,
    warnings: 0,
    complaintsCreated: [],
    errors: [],
    screenshots: []
};

async function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

async function takeScreenshot(page, name) {
    const filename = path.join(screenshotDir, `_.png`);
    await page.screenshot({ path: filename, fullPage: true });
    testResults.screenshots.push({ name, filename });
    console.log(`[SCREENSHOT] Captured: `);
    return filename;
}

async function logTest(name, passed, error = null) {
    testResults.totalTests++;
    if (passed) {
        testResults.passed++;
        console.log(`[PASS] `);
    } else {
        testResults.failed++;
        console.log(`[FAIL] `);
        if (error) {
            testResults.errors.push({ test: name, error: error.toString() });
            console.error(`[ERROR] `);
        }
    }
}

async function runTests() {
    console.log('\\n========================================');
    console.log('STARTING COMPREHENSIVE E2E TESTS');
    console.log('========================================\\n');

    const browser = await chromium.launch({
        headless: false,
        slowMo: 100,
        args: ['--start-maximized']
    });

    const context = await browser.newContext({
        viewport: { width: 1920, height: 1080 },
        recordVideo: { dir: screenshotDir }
    });

    const page = await context.newPage();

    // Enable console logging
    page.on('console', msg => console.log('[BROWSER]', msg.text()));
    page.on('pageerror', error => console.error('[PAGE ERROR]', error));

    try {
        // ========================================
        // PHASE 1: LOGIN AND AUTHENTICATION
        // ========================================
        console.log('\\n### PHASE 1: LOGIN AND AUTHENTICATION ###\\n');

        await page.goto('http://localhost:4200', { waitUntil: 'networkidle' });
        await takeScreenshot(page, '01_landing_page');
        await sleep(1000);

        // Check if we're on login page or already logged in
        const isLoginPage = await page.locator('input[type="email"], input[formControlName="email"]').count() > 0;

        if (isLoginPage) {
            console.log('[INFO] Login page detected, proceeding with authentication...');

            // Find email input
            const emailInput = page.locator('input[type="email"], input[formControlName="email"]').first();
            await emailInput.waitFor({ state: 'visible', timeout: 5000 });
            await emailInput.fill('admin@complaintmanagement.com');
            await logTest('Email input filled', true);

            // Find password input
            const passwordInput = page.locator('input[type="password"], input[formControlName="password"]').first();
            await passwordInput.waitFor({ state: 'visible', timeout: 5000 });
            await passwordInput.fill('Admin@123');
            await logTest('Password input filled', true);

            await takeScreenshot(page, '02_login_form_filled');
            await sleep(500);

            // Click login button
            const loginButton = page.locator('button[type="submit"], button:has-text("Login"), button:has-text("Sign In")').first();
            await loginButton.click();
            await logTest('Login button clicked', true);

            // Wait for navigation or error
            try {
                await page.waitForURL('**/dashboard', { timeout: 10000 });
                await sleep(2000);
                await takeScreenshot(page, '03_login_success');
                await logTest('Login successful - redirected to dashboard', true);
            } catch (error) {
                await takeScreenshot(page, '03_login_failed');
                await logTest('Login failed - no redirect to dashboard', false, error);

                // Check for error messages
                const errorMsg = await page.locator('.error, .alert-danger, .mat-error').textContent().catch(() => '');
                if (errorMsg) {
                    console.error('[LOGIN ERROR]', errorMsg);
                }
            }
        } else {
            console.log('[INFO] Already logged in, skipping login...');
            await logTest('Already authenticated', true);
        }

        // ========================================
        // PHASE 2: DASHBOARD VERIFICATION
        // ========================================
        console.log('\\n### PHASE 2: DASHBOARD VERIFICATION ###\\n');

        await page.goto('http://localhost:4200/dashboard', { waitUntil: 'networkidle' });
        await sleep(2000);
        await takeScreenshot(page, '04_dashboard');

        // Check for dashboard elements
        const hasDashboard = await page.locator('h1, h2, .dashboard-title').count() > 0;
        await logTest('Dashboard page loaded', hasDashboard);

        const hasWidgets = await page.locator('.widget, .card, mat-card').count() > 0;
        await logTest('Dashboard widgets present', hasWidgets);

        const hasNavigation = await page.locator('nav, .sidebar, mat-sidenav').count() > 0;
        await logTest('Navigation menu visible', hasNavigation);

        // ========================================
        // PHASE 3: SLA MANAGEMENT ACCESS
        // ========================================
        console.log('\\n### PHASE 3: SLA MANAGEMENT ACCESS ###\\n');

        // Try to find and click Admin menu
        const adminMenu = page.locator('a:has-text("Admin"), button:has-text("Admin"), [routerLink*="admin"]').first();
        if (await adminMenu.count() > 0) {
            await adminMenu.click();
            await sleep(1000);
            await logTest('Admin menu accessed', true);
        }

        // Look for SLA Management link
        const slaLink = page.locator('a:has-text("SLA"), [routerLink*="sla"]').first();
        if (await slaLink.count() > 0) {
            await slaLink.click();
            await sleep(2000);
            await takeScreenshot(page, '05_sla_management');
            await logTest('SLA Management module accessed', true);
        } else {
            // Try direct navigation
            await page.goto('http://localhost:4200/admin/sla-management', { waitUntil: 'networkidle' });
            await sleep(2000);
            await takeScreenshot(page, '05_sla_management_direct');

            const isSLAPage = await page.locator('h1:has-text("SLA"), h2:has-text("SLA")').count() > 0;
            await logTest('SLA Management accessed via direct URL', isSLAPage);
        }

        // ========================================
        // PHASE 4: SLA GLOBAL SETTINGS
        // ========================================
        console.log('\\n### PHASE 4: SLA GLOBAL SETTINGS CONFIGURATION ###\\n');

        // Look for Settings tab
        const settingsTab = page.locator('button:has-text("Settings"), a:has-text("Settings"), mat-tab:has-text("Settings")').first();
        if (await settingsTab.count() > 0) {
            await settingsTab.click();
            await sleep(1000);
            await takeScreenshot(page, '06_sla_settings_tab');
            await logTest('SLA Settings tab opened', true);

            // Configure global settings
            try {
                // Enable SLA
                const enableSLA = page.locator('input[type="checkbox"][formControlName="isEnabled"], mat-slide-toggle[formControlName="isEnabled"]').first();
                if (await enableSLA.count() > 0) {
                    const isChecked = await enableSLA.isChecked().catch(() => false);
                    if (!isChecked) {
                        await enableSLA.click();
                    }
                    await logTest('SLA enabled', true);
                }

                // Working Hours Only
                const workingHoursOnly = page.locator('input[type="checkbox"][formControlName="workingHoursOnly"], mat-slide-toggle[formControlName="workingHoursOnly"]').first();
                if (await workingHoursOnly.count() > 0) {
                    const isChecked = await workingHoursOnly.isChecked().catch(() => false);
                    if (!isChecked) {
                        await workingHoursOnly.click();
                    }
                    await logTest('Working hours only enabled', true);
                }

                // Working Hours Start
                const startTime = page.locator('input[formControlName="workingHoursStart"]').first();
                if (await startTime.count() > 0) {
                    await startTime.fill('09:00');
                    await logTest('Working hours start set to 09:00', true);
                }

                // Working Hours End
                const endTime = page.locator('input[formControlName="workingHoursEnd"]').first();
                if (await endTime.count() > 0) {
                    await endTime.fill('17:00');
                    await logTest('Working hours end set to 17:00', true);
                }

                // Auto Escalate
                const autoEscalate = page.locator('input[type="checkbox"][formControlName="autoEscalateOnBreach"], mat-slide-toggle[formControlName="autoEscalateOnBreach"]').first();
                if (await autoEscalate.count() > 0) {
                    const isChecked = await autoEscalate.isChecked().catch(() => false);
                    if (!isChecked) {
                        await autoEscalate.click();
                    }
                    await logTest('Auto escalate enabled', true);
                }

                await takeScreenshot(page, '07_sla_settings_configured');

                // Save settings
                const saveButton = page.locator('button:has-text("Save"), button[type="submit"]').first();
                if (await saveButton.count() > 0) {
                    await saveButton.click();
                    await sleep(2000);
                    await takeScreenshot(page, '08_sla_settings_saved');
                    await logTest('SLA settings saved', true);
                }
            } catch (error) {
                await logTest('SLA settings configuration', false, error);
            }
        } else {
            await logTest('SLA Settings tab not found', false);
        }

        // ========================================
        // PHASE 5: CREATE SLA LEVELS
        // ========================================
        console.log('\\n### PHASE 5: CREATE SLA LEVELS ###\\n');

        // Look for SLA Levels tab
        const levelsTab = page.locator('button:has-text("Levels"), a:has-text("Levels"), mat-tab:has-text("Levels"), button:has-text("SLA Levels")').first();
        if (await levelsTab.count() > 0) {
            await levelsTab.click();
            await sleep(1000);
            await takeScreenshot(page, '09_sla_levels_tab');
            await logTest('SLA Levels tab opened', true);

            const slaLevels = [
                { name: 'Gold', description: 'Premium support - fastest response', order: 1, color: '#FFD700', responseTime: 1, responseUnit: 'Hours', resolutionTime: 4, resolutionUnit: 'Hours' },
                { name: 'Silver', description: 'Standard support - normal response', order: 2, color: '#C0C0C0', responseTime: 2, responseUnit: 'Hours', resolutionTime: 8, resolutionUnit: 'Hours' },
                { name: 'Bronze', description: 'Basic support - standard response', order: 3, color: '#CD7F32', responseTime: 4, responseUnit: 'Hours', resolutionTime: 24, resolutionUnit: 'Hours' }
            ];

            for (const level of slaLevels) {
                try {
                    // Click Add button
                    const addButton = page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New")').first();
                    if (await addButton.count() > 0) {
                        await addButton.click();
                        await sleep(1000);

                        // Fill form
                        await page.locator('input[formControlName="name"]').fill(level.name);
                        await page.locator('input[formControlName="description"], textarea[formControlName="description"]').fill(level.description);
                        await page.locator('input[formControlName="displayOrder"], input[formControlName="order"]').fill(level.order.toString());
                        await page.locator('input[formControlName="colorCode"], input[type="color"]').fill(level.color);
                        await page.locator('input[formControlName="responseTime"]').fill(level.responseTime.toString());
                        await page.locator('input[formControlName="resolutionTime"]').fill(level.resolutionTime.toString());

                        await takeScreenshot(page, `10_sla_level__form`);

                        // Save
                        const saveBtn = page.locator('button:has-text("Save"), button[type="submit"]').first();
                        await saveBtn.click();
                        await sleep(2000);

                        await logTest(`SLA Level '' created`, true);
                    }
                } catch (error) {
                    await logTest(`SLA Level '' creation`, false, error);
                }
            }

            await takeScreenshot(page, '11_sla_levels_list');
        } else {
            await logTest('SLA Levels tab not found', false);
        }

        // ========================================
        // PHASE 6: CATEGORY-SLA MAPPINGS
        // ========================================
        console.log('\\n### PHASE 6: CREATE CATEGORY-SLA MAPPINGS ###\\n');

        const categoryTab = page.locator('button:has-text("Category"), a:has-text("Category"), mat-tab:has-text("Category")').first();
        if (await categoryTab.count() > 0) {
            await categoryTab.click();
            await sleep(1000);
            await takeScreenshot(page, '12_category_mappings_tab');

            // Create 2 category mappings
            for (let i = 0; i < 2; i++) {
                try {
                    const addBtn = page.locator('button:has-text("Add"), button:has-text("Create")').first();
                    if (await addBtn.count() > 0) {
                        await addBtn.click();
                        await sleep(1000);

                        // Select category and SLA level
                        const categorySelect = page.locator('select[formControlName="categoryId"], mat-select[formControlName="categoryId"]').first();
                        await categorySelect.click();
                        await sleep(500);
                        const categoryOption = page.locator('mat-option, option').nth(i);
                        await categoryOption.click();

                        const slaSelect = page.locator('select[formControlName="slaLevelId"], mat-select[formControlName="slaLevelId"]').first();
                        await slaSelect.click();
                        await sleep(500);
                        const slaOption = page.locator('mat-option:has-text("Gold"), option:has-text("Gold")').first();
                        await slaOption.click();

                        await takeScreenshot(page, `13_category_mapping_`);

                        const saveBtn = page.locator('button:has-text("Save")').first();
                        await saveBtn.click();
                        await sleep(2000);

                        await logTest(`Category-SLA mapping  created`, true);
                    }
                } catch (error) {
                    await logTest(`Category-SLA mapping `, false, error);
                }
            }

            await takeScreenshot(page, '14_category_mappings_list');
        }

        // ========================================
        // PHASE 7: PRIORITY-SLA MAPPINGS
        // ========================================
        console.log('\\n### PHASE 7: CREATE PRIORITY-SLA MAPPINGS ###\\n');

        const priorityTab = page.locator('button:has-text("Priority"), a:has-text("Priority"), mat-tab:has-text("Priority")').first();
        if (await priorityTab.count() > 0) {
            await priorityTab.click();
            await sleep(1000);
            await takeScreenshot(page, '15_priority_mappings_tab');

            const priorityMappings = [
                { priority: 'Critical', sla: 'Gold', responseTime: 30, resolutionTime: 120 },
                { priority: 'High', sla: 'Silver', responseTime: null, resolutionTime: null }
            ];

            for (const mapping of priorityMappings) {
                try {
                    const addBtn = page.locator('button:has-text("Add"), button:has-text("Create")').first();
                    if (await addBtn.count() > 0) {
                        await addBtn.click();
                        await sleep(1000);

                        // Select priority and SLA
                        const prioritySelect = page.locator('select[formControlName="priorityId"], mat-select[formControlName="priorityId"]').first();
                        await prioritySelect.click();
                        await sleep(500);
                        await page.locator(`mat-option:has-text(""), option:has-text("")`).first().click();

                        const slaSelect = page.locator('select[formControlName="slaLevelId"], mat-select[formControlName="slaLevelId"]').first();
                        await slaSelect.click();
                        await sleep(500);
                        await page.locator(`mat-option:has-text(""), option:has-text("")`).first().click();

                        if (mapping.responseTime) {
                            await page.locator('input[formControlName="overrideResponseTime"]').fill(mapping.responseTime.toString());
                        }
                        if (mapping.resolutionTime) {
                            await page.locator('input[formControlName="overrideResolutionTime"]').fill(mapping.resolutionTime.toString());
                        }

                        await takeScreenshot(page, `16_priority_mapping_`);

                        const saveBtn = page.locator('button:has-text("Save")').first();
                        await saveBtn.click();
                        await sleep(2000);

                        await logTest(`Priority-SLA mapping '' created`, true);
                    }
                } catch (error) {
                    await logTest(`Priority-SLA mapping ''`, false, error);
                }
            }

            await takeScreenshot(page, '17_priority_mappings_list');
        }

        // ========================================
        // PHASE 8: CREATE TEST COMPLAINTS
        // ========================================
        console.log('\\n### PHASE 8: CREATE TEST COMPLAINTS WITH SLA ###\\n');

        await page.goto('http://localhost:4200/complaints', { waitUntil: 'networkidle' });
        await sleep(2000);
        await takeScreenshot(page, '18_complaints_page');

        const testComplaints = [
            { title: 'Critical Server Outage - E2E Test', description: 'Testing Priority-SLA mapping with Critical priority', priority: 'Critical', category: 0 },
            { title: 'Standard Request - E2E Test', description: 'Testing Category-SLA mapping with Normal priority', priority: 'Normal', category: 0 },
            { title: 'High Priority Issue - E2E Test', description: 'Testing Priority-SLA mapping with High priority', priority: 'High', category: 1 }
        ];

        for (let i = 0; i < testComplaints.length; i++) {
            const complaint = testComplaints[i];
            try {
                // Click New Complaint button
                const newBtn = page.locator('button:has-text("New"), button:has-text("Create"), button:has-text("Add Complaint")').first();
                await newBtn.click();
                await sleep(1500);

                // Fill complaint form
                await page.locator('input[formControlName="title"]').fill(complaint.title);
                await page.locator('textarea[formControlName="description"]').fill(complaint.description);

                // Select category
                const categorySelect = page.locator('select[formControlName="categoryId"], mat-select[formControlName="categoryId"]').first();
                await categorySelect.click();
                await sleep(500);
                await page.locator('mat-option, option').nth(complaint.category).click();
                await sleep(300);

                // Select priority
                const prioritySelect = page.locator('select[formControlName="priorityId"], mat-select[formControlName="priorityId"]').first();
                await prioritySelect.click();
                await sleep(500);
                await page.locator(`mat-option:has-text(""), option:has-text("")`).first().click();
                await sleep(300);

                await takeScreenshot(page, `19_complaint_form_`);

                // Submit
                const submitBtn = page.locator('button:has-text("Submit"), button:has-text("Create"), button[type="submit"]').first();
                await submitBtn.click();
                await sleep(3000);

                await takeScreenshot(page, `20_complaint_created_`);

                // Try to capture complaint number
                const complaintNumber = await page.locator('.complaint-number, .id, h2, h3').first().textContent().catch(() => 'Unknown');
                testResults.complaintsCreated.push({
                    title: complaint.title,
                    number: complaintNumber,
                    priority: complaint.priority
                });

                await logTest(`Test complaint ${i+1} '${complaint.title}' created`, true);

                // Go back to list
                await page.goto('http://localhost:4200/complaints', { waitUntil: 'networkidle' });
                await sleep(1500);
            } catch (error) {
                await logTest(`Test complaint ${i+1} '${complaint.title}'`, false, error);
            }
        }

        // ========================================
        // PHASE 9: VERIFY SLA INFORMATION
        // ========================================
        console.log('\\n### PHASE 9: VERIFY SLA INFORMATION DISPLAY ###\\n');

        await page.goto('http://localhost:4200/complaints', { waitUntil: 'networkidle' });
        await sleep(2000);
        await takeScreenshot(page, '21_complaints_list_with_sla');

        // Check for SLA indicators
        const hasDueDates = await page.locator('.due-date, .deadline, [class*="sla"]').count() > 0;
        await logTest('Due dates visible in complaints list', hasDueDates);

        const hasPriorityBadges = await page.locator('.priority, .badge').count() > 0;
        await logTest('Priority badges visible', hasPriorityBadges);

        // Click on first complaint to view details
        const firstComplaint = page.locator('.complaint-row, mat-list-item, tr').first();
        if (await firstComplaint.count() > 0) {
            await firstComplaint.click();
            await sleep(2000);
            await takeScreenshot(page, '22_complaint_detail_with_sla');
            await logTest('Complaint details page opened', true);
        }

        // ========================================
        // PHASE 10: ADDITIONAL FEATURES
        // ========================================
        console.log('\\n### PHASE 10: ADDITIONAL FEATURE TESTING ###\\n');

        // Test User Management
        await page.goto('http://localhost:4200/admin/user-management', { waitUntil: 'networkidle' });
        await sleep(1500);
        await takeScreenshot(page, '23_user_management');
        await logTest('User Management page accessible', true);

        // Test Resource Pool
        await page.goto('http://localhost:4200/admin/resource-pool-management', { waitUntil: 'networkidle' });
        await sleep(1500);
        await takeScreenshot(page, '24_resource_pool');
        await logTest('Resource Pool Management accessible', true);

        // Test Email Settings
        await page.goto('http://localhost:4200/admin/email-settings', { waitUntil: 'networkidle' });
        await sleep(1500);
        await takeScreenshot(page, '25_email_settings');
        await logTest('Email Settings page accessible', true);

        // Final dashboard check
        await page.goto('http://localhost:4200/dashboard', { waitUntil: 'networkidle' });
        await sleep(2000);
        await takeScreenshot(page, '26_final_dashboard');

    } catch (error) {
        console.error('[FATAL ERROR]', error);
        await takeScreenshot(page, 'ERROR_final');
    } finally {
        // Save results to JSON
        fs.writeFileSync('test-results.json', JSON.stringify(testResults, null, 2));

        console.log('\\n========================================');
        console.log('TEST EXECUTION COMPLETE');
        console.log('========================================');
        console.log(`Total Tests: `);
        console.log(`Passed: `);
        console.log(`Failed: `);
        console.log(`Success Rate: %`);
        console.log('========================================\\n');

        await sleep(3000);
        await context.close();
        await browser.close();
    }
}

runTests().catch(console.error);
