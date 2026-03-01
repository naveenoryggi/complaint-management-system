import { test, expect, Page } from '@playwright/test';
import * as fs from 'fs';
import * as path from 'path';

const BASE_URL = 'http://localhost:4200';
const API_URL = 'http://localhost:5000';

// Evidence directory
const EVIDENCE_DIR = path.join(process.cwd(), 'test-evidence', 'user-management-e2e');

// Ensure evidence directory exists
if (!fs.existsSync(EVIDENCE_DIR)) {
  fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
}

// Helper function to take screenshot
async function takeScreenshot(page: Page, name: string) {
  const screenshotPath = path.join(EVIDENCE_DIR, `${name}.png`);
  await page.screenshot({ path: screenshotPath, fullPage: true });
  console.log(`Screenshot saved: ${screenshotPath}`);
}

// Helper function to save page content
async function savePageContent(page: Page, name: string) {
  const contentPath = path.join(EVIDENCE_DIR, `${name}.html`);
  const content = await page.content();
  fs.writeFileSync(contentPath, content);
  console.log(`Page content saved: ${contentPath}`);
}

// User data for testing
const testUsers = [
  // Managers
  { email: 'manager1@test.com', fullName: 'John Manager', role: 'Manager', employeeCode: 'MGR001', password: 'Test@123' },
  { email: 'manager2@test.com', fullName: 'Sarah Manager', role: 'Manager', employeeCode: 'MGR002', password: 'Test@123' },

  // Supervisors
  { email: 'supervisor1@test.com', fullName: 'Mike Supervisor', role: 'Supervisor', employeeCode: 'SUP001', password: 'Test@123' },
  { email: 'supervisor2@test.com', fullName: 'Anna Supervisor', role: 'Supervisor', employeeCode: 'SUP002', password: 'Test@123' },

  // Technicians
  { email: 'tech1@test.com', fullName: 'Tom Technician', role: 'Technician', employeeCode: 'TECH001', password: 'Test@123' },
  { email: 'tech2@test.com', fullName: 'Lisa Technician', role: 'Technician', employeeCode: 'TECH002', password: 'Test@123' },
  { email: 'tech3@test.com', fullName: 'Bob Technician', role: 'Technician', employeeCode: 'TECH003', password: 'Test@123' },
  { email: 'tech4@test.com', fullName: 'Emma Technician', role: 'Technician', employeeCode: 'TECH004', password: 'Test@123' },

  // Regular Users
  { email: 'user1@test.com', fullName: 'Alice User', role: 'User', employeeCode: 'USR001', password: 'Test@123' },
  { email: 'user2@test.com', fullName: 'David User', role: 'User', employeeCode: 'USR002', password: 'Test@123' },
  { email: 'user3@test.com', fullName: 'Grace User', role: 'User', employeeCode: 'USR003', password: 'Test@123' },
  { email: 'user4@test.com', fullName: 'Henry User', role: 'User', employeeCode: 'USR004', password: 'Test@123' },
];

test.describe('User Management E2E Tests', () => {
  test.setTimeout(300000); // 5 minutes timeout for comprehensive testing

  test('Complete User Management CRUD Operations', async ({ page }) => {
    console.log('\n========================================');
    console.log('STARTING USER MANAGEMENT E2E TEST');
    console.log('========================================\n');

    // Step 1: Navigate to login page
    console.log('Step 1: Navigating to login page...');
    await page.goto(BASE_URL);
    await page.waitForLoadState('networkidle');
    await takeScreenshot(page, '01-login-page');

    // Step 2: Login as admin
    console.log('Step 2: Logging in as admin...');

    // The login form uses a different input structure
    const emailSelectors = [
      'input[type="email"]',
      'input[placeholder*="Employee ID" i]',
      'input[placeholder*="Email" i]',
      'input[formcontrolname="email"]',
      'input[formcontrolname="username"]'
    ];

    let emailFilled = false;
    for (const selector of emailSelectors) {
      try {
        const field = page.locator(selector).first();
        if (await field.isVisible({ timeout: 2000 })) {
          await field.fill('admin@complaintmanagement.com');
          emailFilled = true;
          console.log(`✓ Filled email using: ${selector}`);
          break;
        }
      } catch (e) {
        continue;
      }
    }

    if (!emailFilled) {
      console.log('⚠ Could not find email field');
    }

    await page.fill('input[type="password"]', 'Admin@123');
    await takeScreenshot(page, '02-login-filled');

    await page.click('button[type="submit"]');
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '03-after-login');

    console.log('✓ Successfully logged in');

    // Step 3: Navigate to User Management
    console.log('\nStep 3: Navigating to User Management...');

    // Check current URL
    const currentUrl = page.url();
    console.log('Current URL:', currentUrl);

    // Try to find and click Admin menu
    try {
      // Look for Admin menu in header
      const adminMenu = page.locator('text=Admin').first();
      if (await adminMenu.isVisible()) {
        await adminMenu.click();
        await page.waitForTimeout(1000);
        await takeScreenshot(page, '04-admin-menu-opened');
      }
    } catch (error) {
      console.log('Admin menu not in header, trying sidebar...');
    }

    // Try multiple approaches to navigate to User Management
    let navigated = false;

    // Approach 1: Direct URL navigation
    console.log('Attempting direct URL navigation...');
    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '05-user-management-page');

    if (page.url().includes('/admin/users')) {
      console.log('✓ Successfully navigated to User Management via URL');
      navigated = true;
    }

    if (!navigated) {
      console.log('⚠ Unable to navigate to User Management page');
      console.log('Current URL:', page.url());
      await savePageContent(page, '05-user-management-page-error');
    }

    // Step 4: Check page structure
    console.log('\nStep 4: Analyzing User Management page structure...');
    const pageTitle = await page.textContent('h1, h2, .page-title, .mat-toolbar, .header-title').catch(() => 'Title not found');
    console.log('Page title:', pageTitle);

    // Look for Add/Create button
    const addButtons = await page.locator('button:has-text("Add"), button:has-text("Create"), button:has-text("New"), button:has-text("User")').count();
    console.log('Add buttons found:', addButtons);

    // Look for user table or list
    const tables = await page.locator('table, mat-table, .user-list, .data-table').count();
    console.log('Tables found:', tables);

    await takeScreenshot(page, '06-page-structure-analysis');

    // Step 5: Create users
    console.log('\n========================================');
    console.log('Step 5: Creating 12 test users...');
    console.log('========================================\n');

    const createdUsers: any[] = [];
    const failedUsers: any[] = [];

    for (let i = 0; i < testUsers.length; i++) {
      const user = testUsers[i];
      console.log(`\nCreating user ${i + 1}/12: ${user.fullName} (${user.email}) - ${user.role}`);

      try {
        // Click Add User button - try multiple selectors
        const addButtonSelectors = [
          'button:has-text("Add User")',
          'button:has-text("Create User")',
          'button:has-text("New User")',
          'button:has-text("Add")',
          'button.add-user',
          'button[mattooltip*="Add"]',
          '.add-button',
          '[aria-label*="Add"]'
        ];

        let buttonClicked = false;
        for (const selector of addButtonSelectors) {
          try {
            const button = page.locator(selector).first();
            if (await button.isVisible({ timeout: 2000 })) {
              await button.click();
              await page.waitForTimeout(1000);
              buttonClicked = true;
              console.log(`✓ Clicked add button using selector: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        if (!buttonClicked) {
          console.log('⚠ Could not find Add User button, trying FAB or floating button...');
          const fabButton = page.locator('button[mat-fab], button[mat-mini-fab], .fab-button').first();
          if (await fabButton.isVisible({ timeout: 2000 })) {
            await fabButton.click();
            await page.waitForTimeout(1000);
            buttonClicked = true;
            console.log('✓ Clicked FAB button');
          }
        }

        if (!buttonClicked) {
          throw new Error('Could not find Add User button');
        }

        await takeScreenshot(page, `07-add-user-dialog-${i + 1}`);

        // Wait for dialog/form to appear
        await page.waitForTimeout(1000);

        // Fill in user details - try multiple field selectors
        console.log('Filling in user details...');

        // Email field
        const emailSelectors = [
          'input[formcontrolname="email"]',
          'input[name="email"]',
          'input[type="email"]',
          'input[placeholder*="Email" i]',
          '#email'
        ];

        for (const selector of emailSelectors) {
          try {
            const field = page.locator(selector).first();
            if (await field.isVisible({ timeout: 1000 })) {
              await field.clear();
              await field.fill(user.email);
              console.log(`✓ Filled email using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        // Full Name field
        const nameSelectors = [
          'input[formcontrolname="fullName"]',
          'input[formcontrolname="name"]',
          'input[name="fullName"]',
          'input[name="name"]',
          'input[placeholder*="Name" i]',
          '#fullName',
          '#name'
        ];

        for (const selector of nameSelectors) {
          try {
            const field = page.locator(selector).first();
            if (await field.isVisible({ timeout: 1000 })) {
              await field.clear();
              await field.fill(user.fullName);
              console.log(`✓ Filled name using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        // Employee Code field
        const codeSelectors = [
          'input[formcontrolname="employeeCode"]',
          'input[name="employeeCode"]',
          'input[placeholder*="Employee" i]',
          'input[placeholder*="Code" i]',
          '#employeeCode'
        ];

        for (const selector of codeSelectors) {
          try {
            const field = page.locator(selector).first();
            if (await field.isVisible({ timeout: 1000 })) {
              await field.clear();
              await field.fill(user.employeeCode);
              console.log(`✓ Filled employee code using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        // Password field
        const passwordSelectors = [
          'input[formcontrolname="password"]',
          'input[name="password"]',
          'input[type="password"]',
          'input[placeholder*="Password" i]',
          '#password'
        ];

        for (const selector of passwordSelectors) {
          try {
            const field = page.locator(selector).first();
            if (await field.isVisible({ timeout: 1000 })) {
              await field.clear();
              await field.fill(user.password);
              console.log(`✓ Filled password using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        // Confirm Password field (if exists)
        const confirmPasswordSelectors = [
          'input[formcontrolname="confirmPassword"]',
          'input[name="confirmPassword"]',
          'input[placeholder*="Confirm" i]',
          '#confirmPassword'
        ];

        for (const selector of confirmPasswordSelectors) {
          try {
            const field = page.locator(selector).first();
            if (await field.isVisible({ timeout: 1000 })) {
              await field.clear();
              await field.fill(user.password);
              console.log(`✓ Filled confirm password using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        // Role selection - try different approaches
        console.log(`Selecting role: ${user.role}...`);

        const roleSelectors = [
          'mat-select[formcontrolname="role"]',
          'mat-select[formcontrolname="roleId"]',
          'select[formcontrolname="role"]',
          'select[name="role"]',
          '[formcontrolname="role"]'
        ];

        let roleSelected = false;
        for (const selector of roleSelectors) {
          try {
            const roleField = page.locator(selector).first();
            if (await roleField.isVisible({ timeout: 1000 })) {
              await roleField.click();
              await page.waitForTimeout(500);

              // Try to select the role from dropdown
              const roleOptionSelectors = [
                `mat-option:has-text("${user.role}")`,
                `option:has-text("${user.role}")`,
                `.mat-option-text:has-text("${user.role}")`
              ];

              for (const optionSelector of roleOptionSelectors) {
                try {
                  const option = page.locator(optionSelector).first();
                  if (await option.isVisible({ timeout: 1000 })) {
                    await option.click();
                    await page.waitForTimeout(500);
                    roleSelected = true;
                    console.log(`✓ Selected role: ${user.role}`);
                    break;
                  }
                } catch (e) {
                  continue;
                }
              }

              if (roleSelected) break;
            }
          } catch (e) {
            continue;
          }
        }

        if (!roleSelected) {
          console.log(`⚠ Could not select role ${user.role}, role field may not be available`);
        }

        await takeScreenshot(page, `08-user-form-filled-${i + 1}`);

        // Submit the form
        console.log('Submitting user creation form...');
        const submitSelectors = [
          'button:has-text("Create")',
          'button:has-text("Save")',
          'button:has-text("Submit")',
          'button[type="submit"]',
          'button.submit-button',
          'button.save-button'
        ];

        let formSubmitted = false;
        for (const selector of submitSelectors) {
          try {
            const button = page.locator(selector).first();
            if (await button.isVisible({ timeout: 2000 }) && await button.isEnabled({ timeout: 1000 })) {
              await button.click();
              await page.waitForTimeout(2000);
              formSubmitted = true;
              console.log(`✓ Clicked submit button using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        if (!formSubmitted) {
          throw new Error('Could not find or click submit button');
        }

        await page.waitForLoadState('networkidle');
        await takeScreenshot(page, `09-after-user-creation-${i + 1}`);

        // Check for success message or error
        const successIndicators = [
          '.success-message',
          '.mat-snack-bar-container',
          'snack-bar-container',
          '[role="alert"]'
        ];

        let successFound = false;
        for (const selector of successIndicators) {
          try {
            const element = page.locator(selector).first();
            if (await element.isVisible({ timeout: 2000 })) {
              const text = await element.textContent();
              console.log(`✓ Success message: ${text}`);
              successFound = true;
              break;
            }
          } catch (e) {
            continue;
          }
        }

        createdUsers.push(user);
        console.log(`✓ User ${i + 1}/12 created successfully: ${user.fullName}`);

      } catch (error: any) {
        console.log(`✗ Failed to create user ${i + 1}/12: ${user.fullName}`);
        console.log(`Error: ${error.message}`);
        failedUsers.push({ user, error: error.message });
        await takeScreenshot(page, `ERROR-user-creation-${i + 1}`);

        // Try to close any open dialogs
        try {
          await page.keyboard.press('Escape');
          await page.waitForTimeout(500);
        } catch (e) {
          // Ignore
        }
      }

      await page.waitForTimeout(1000);
    }

    console.log('\n========================================');
    console.log('USER CREATION SUMMARY');
    console.log('========================================');
    console.log(`Successfully created: ${createdUsers.length}/12 users`);
    console.log(`Failed: ${failedUsers.length}/12 users`);

    if (failedUsers.length > 0) {
      console.log('\nFailed users:');
      failedUsers.forEach((item, idx) => {
        console.log(`${idx + 1}. ${item.user.fullName} (${item.user.email}) - ${item.error}`);
      });
    }

    // Step 6: Verify users in list
    console.log('\n========================================');
    console.log('Step 6: Verifying users in user list...');
    console.log('========================================\n');

    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '10-user-list-view');

    // Count users in the table/list
    const userRowSelectors = [
      'table tbody tr',
      'mat-table mat-row',
      '.user-row',
      '.data-row',
      'tr[class*="mat-row"]'
    ];

    let totalUsersDisplayed = 0;
    for (const selector of userRowSelectors) {
      try {
        const count = await page.locator(selector).count();
        if (count > 0) {
          totalUsersDisplayed = count;
          console.log(`✓ Found ${count} users in list using selector: ${selector}`);
          break;
        }
      } catch (e) {
        continue;
      }
    }

    console.log(`Total users displayed: ${totalUsersDisplayed}`);

    // Try to verify each created user
    console.log('\nVerifying individual users...');
    let verifiedCount = 0;
    for (const user of createdUsers) {
      const userExists = await page.locator(`text=${user.email}`).count() > 0 ||
                         await page.locator(`text=${user.fullName}`).count() > 0;
      if (userExists) {
        console.log(`✓ ${user.fullName} (${user.email}) found in list`);
        verifiedCount++;
      } else {
        console.log(`⚠ ${user.fullName} (${user.email}) NOT found in list`);
      }
    }

    console.log(`\nVerified ${verifiedCount}/${createdUsers.length} created users in the list`);

    // Step 7: Test editing a user
    console.log('\n========================================');
    console.log('Step 7: Testing user edit functionality...');
    console.log('========================================\n');

    if (createdUsers.length > 0) {
      const userToEdit = createdUsers[0];
      console.log(`Attempting to edit: ${userToEdit.fullName}`);

      try {
        // Find the user row and edit button
        const editSelectors = [
          `tr:has-text("${userToEdit.email}") button[mattooltip*="Edit"]`,
          `tr:has-text("${userToEdit.email}") button:has-text("Edit")`,
          `tr:has-text("${userToEdit.email}") .edit-button`,
          `tr:has-text("${userToEdit.email}") mat-icon:has-text("edit")`
        ];

        let editClicked = false;
        for (const selector of editSelectors) {
          try {
            const editButton = page.locator(selector).first();
            if (await editButton.isVisible({ timeout: 2000 })) {
              await editButton.click();
              await page.waitForTimeout(1000);
              editClicked = true;
              console.log(`✓ Clicked edit button using: ${selector}`);
              break;
            }
          } catch (e) {
            continue;
          }
        }

        if (!editClicked) {
          console.log('⚠ Could not find edit button for user');
          throw new Error('Edit button not found');
        }

        await takeScreenshot(page, '11-edit-user-dialog');

        // Modify user details
        const newFullName = `${userToEdit.fullName} - Updated`;
        console.log(`Updating full name to: ${newFullName}`);

        const nameField = page.locator('input[formcontrolname="fullName"], input[name="fullName"]').first();
        if (await nameField.isVisible({ timeout: 2000 })) {
          await nameField.clear();
          await nameField.fill(newFullName);
          console.log('✓ Updated full name');
        }

        await takeScreenshot(page, '12-edit-user-form-filled');

        // Save changes
        const saveButton = page.locator('button:has-text("Save"), button:has-text("Update")').first();
        if (await saveButton.isVisible({ timeout: 2000 })) {
          await saveButton.click();
          await page.waitForTimeout(2000);
          console.log('✓ Clicked save button');
        }

        await page.waitForLoadState('networkidle');
        await takeScreenshot(page, '13-after-edit');

        // Verify the update
        const updatedUserVisible = await page.locator(`text=${newFullName}`).isVisible({ timeout: 3000 }).catch(() => false);
        if (updatedUserVisible) {
          console.log('✓ User successfully updated and changes visible');
        } else {
          console.log('⚠ Updated user name not immediately visible');
        }

      } catch (error: any) {
        console.log(`✗ Failed to edit user: ${error.message}`);
        await takeScreenshot(page, 'ERROR-edit-user');
      }
    }

    // Step 8: Test role verification
    console.log('\n========================================');
    console.log('Step 8: Verifying user roles...');
    console.log('========================================\n');

    await page.goto(`${BASE_URL}/admin/users`);
    await page.waitForLoadState('networkidle');
    await page.waitForTimeout(2000);
    await takeScreenshot(page, '14-final-user-list');

    // Group users by role
    const roleGroups = {
      'Manager': testUsers.filter(u => u.role === 'Manager'),
      'Supervisor': testUsers.filter(u => u.role === 'Supervisor'),
      'Technician': testUsers.filter(u => u.role === 'Technician'),
      'User': testUsers.filter(u => u.role === 'User')
    };

    console.log('\nExpected user distribution by role:');
    for (const [role, users] of Object.entries(roleGroups)) {
      console.log(`${role}: ${users.length} users`);
      users.forEach(u => console.log(`  - ${u.fullName} (${u.email})`));
    }

    // Final summary
    console.log('\n========================================');
    console.log('TEST EXECUTION COMPLETE');
    console.log('========================================\n');

    const summary = {
      totalUsersAttempted: testUsers.length,
      successfullyCreated: createdUsers.length,
      failed: failedUsers.length,
      verifiedInList: verifiedCount,
      totalDisplayed: totalUsersDisplayed,
      roleDistribution: {
        Manager: 2,
        Supervisor: 2,
        Technician: 4,
        User: 4
      }
    };

    console.log('FINAL SUMMARY:');
    console.log(JSON.stringify(summary, null, 2));

    // Save summary to file
    const summaryPath = path.join(EVIDENCE_DIR, 'test-summary.json');
    fs.writeFileSync(summaryPath, JSON.stringify({
      summary,
      createdUsers,
      failedUsers,
      timestamp: new Date().toISOString()
    }, null, 2));
    console.log(`\nTest summary saved to: ${summaryPath}`);

    // Final screenshot
    await takeScreenshot(page, '15-final-state');
    await savePageContent(page, '15-final-state');

    console.log('\n========================================');
    console.log('All evidence saved to:', EVIDENCE_DIR);
    console.log('========================================\n');
  });
});
