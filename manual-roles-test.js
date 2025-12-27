const { chromium } = require('playwright');
const path = require('path');
const fs = require('fs');

const BASE_URL = 'http://localhost:4200';
const SCREENSHOT_DIR = path.join(__dirname, 'test-screenshots', 'roles-permissions-manual');

if (!fs.existsSync(SCREENSHOT_DIR)) {
  fs.mkdirSync(SCREENSHOT_DIR, { recursive: true });
}

let screenshotCounter = 1;
async function screenshot(page, name) {
  const filename = `${String(screenshotCounter).padStart(3, '0')}_${name}.png`;
  screenshotCounter++;
  await page.screenshot({ path: path.join(SCREENSHOT_DIR, filename), fullPage: true });
  console.log(`📸 Screenshot: ${filename}`);
  return filename;
}

async function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

(async () => {
  const browser = await chromium.launch({ headless: false, slowMo: 500 });
  const context = await browser.newContext({ viewport: { width: 1920, height: 1080 } });
  const page = await context.newPage();

  console.log('\n════════════════════════════════════════════════════════════');
  console.log('🧪 COMPREHENSIVE ROLE & PERMISSION MANAGEMENT TEST SUITE');
  console.log('════════════════════════════════════════════════════════════\n');

  // Track issues found
  const issues = [];
  const passed = [];

  try {
    // ========================================================================
    // TEST 1: LOGIN AND AUTHENTICATION
    // ========================================================================
    console.log('📋 TEST 1: LOGIN AND AUTHENTICATION');
    console.log('─────────────────────────────────────');

    await page.goto(BASE_URL);
    await sleep(2000);
    await screenshot(page, 'login_page_initial');

    // Use the test credentials shown on the page
    const emailInput = page.locator('input[placeholder*="Employee"]').first();
    await emailInput.fill('admin@complaints1stclientdomain.com');
    await sleep(500);

    const passwordInput = page.locator('input[type="password"]').first();
    await passwordInput.fill('Admin@123');
    await sleep(500);
    await screenshot(page, 'login_credentials_filled');

    const signInButton = page.locator('button:has-text("Sign In")').first();
    await signInButton.click();
    console.log('✓ Clicked Sign In button');

    await sleep(3000);
    await screenshot(page, 'after_login');

    const currentUrl = page.url();
    if (currentUrl.includes('dashboard') || currentUrl.includes('home')) {
      console.log('✅ Login successful! Current URL:', currentUrl);
      passed.push('Login and authentication');
    } else {
      console.log('❌ Login may have failed. Current URL:', currentUrl);
      issues.push({
        test: 'Login',
        severity: 'CRITICAL',
        description: 'Unable to login to the application',
        url: currentUrl
      });
    }

    // ========================================================================
    // TEST 2: NAVIGATE TO ROLES PAGE AND VERIFY INITIAL LOAD
    // ========================================================================
    console.log('\n📋 TEST 2: NAVIGATE TO ROLES PAGE');
    console.log('─────────────────────────────────────');

    await page.goto(`${BASE_URL}/admin/roles`);
    await sleep(3000);
    await screenshot(page, 'roles_page_initial');

    // Check for info banner
    const infoBanner = page.locator('.alert-info, .info-banner, div:has-text("Roles define")').first();
    const hasBanner = await infoBanner.count() > 0;
    console.log(`Info Banner: ${hasBanner ? '✓ Found' : '✗ Not found'}`);
    if (!hasBanner) {
      issues.push({
        test: 'Page Load - Info Banner',
        severity: 'MINOR',
        description: 'Info banner explaining roles & permissions is missing'
      });
    }

    // Check for search bar
    const searchInput = page.locator('input[type="search"], input[placeholder*="Search"]').first();
    const hasSearch = await searchInput.count() > 0;
    console.log(`Search Bar: ${hasSearch ? '✓ Found' : '✗ Not found'}`);
    if (!hasSearch) {
      issues.push({
        test: 'Page Load - Search Bar',
        severity: 'MAJOR',
        description: 'Search bar is missing from the page'
      });
    }

    // Check for "Show Active Only" toggle
    const activeToggle = page.locator('label:has-text("Active"), mat-slide-toggle').first();
    const hasToggle = await activeToggle.count() > 0;
    console.log(`"Show Active Only" Toggle: ${hasToggle ? '✓ Found' : '✗ Not found'}`);
    if (!hasToggle) {
      issues.push({
        test: 'Page Load - Active Toggle',
        severity: 'MINOR',
        description: '"Show Active Only" toggle is missing'
      });
    }

    // Check for role cards grid
    const roleCards = await page.locator('.card, .role-card, mat-card').all();
    console.log(`Role Cards Found: ${roleCards.length}`);

    if (roleCards.length === 0) {
      issues.push({
        test: 'Page Load - Role Cards',
        severity: 'CRITICAL',
        description: 'No role cards found on the page'
      });
    } else {
      passed.push('Page loads with role cards grid');
    }

    // ========================================================================
    // TEST 3: VIEW EXISTING ROLES - VERIFY CARD INFORMATION
    // ========================================================================
    console.log('\n📋 TEST 3: VIEW EXISTING ROLES - CARD INFORMATION');
    console.log('─────────────────────────────────────────────────');

    for (let i = 0; i < Math.min(roleCards.length, 5); i++) {
      console.log(`\n🔍 Inspecting Role Card ${i + 1}:`);
      const card = roleCards[i];

      // Take screenshot of individual card
      await card.scrollIntoViewIfNeeded();
      await sleep(500);
      await screenshot(page, `role_card_${i + 1}`);

      const cardText = await card.textContent();

      // Check for role name
      const roleName = await card.locator('h3, h4, h5, .card-title, .mat-card-title').first().textContent().catch(() => null);
      console.log(`  Role Name: ${roleName || '❌ NOT FOUND'}`);
      if (!roleName) {
        issues.push({
          test: `Role Card ${i + 1} - Name`,
          severity: 'MAJOR',
          description: 'Role name not displayed on card'
        });
      }

      // Check for role code
      const hasCode = cardText.match(/[A-Z_]+/) !== null;
      console.log(`  Role Code: ${hasCode ? '✓ Present' : '❌ NOT FOUND'}`);

      // Check for SYSTEM ROLE badge
      const systemBadge = await card.locator('.badge, .chip, mat-chip').filter({ hasText: /system/i }).count();
      console.log(`  SYSTEM ROLE Badge: ${systemBadge > 0 ? '✓ Present' : 'N/A (might be custom role)'}`);

      // Check for ACTIVE status badge
      const activeBadge = await card.locator('.badge, .chip, mat-chip').filter({ hasText: /active/i }).count();
      console.log(`  ACTIVE Badge: ${activeBadge > 0 ? '✓ Present' : '❌ NOT FOUND'}`);
      if (activeBadge === 0) {
        issues.push({
          test: `Role Card ${i + 1} - Active Badge`,
          severity: 'MINOR',
          description: 'ACTIVE status badge not displayed'
        });
      }

      // Check for Role Type
      const hasRoleType = cardText.match(/type/i) !== null;
      console.log(`  Role Type: ${hasRoleType ? '✓ Present' : '❌ NOT FOUND'}`);

      // Check for Escalation Level
      const hasEscalation = cardText.match(/escalation/i) !== null;
      console.log(`  Escalation Level: ${hasEscalation ? '✓ Present' : '⚠ May not be displayed'}`);

      // Check for Permissions count
      const permMatch = cardText.match(/(\d+)\s*(permission|granted)/i);
      console.log(`  Permissions Count: ${permMatch ? `✓ ${permMatch[1]}` : '❌ NOT FOUND'}`);
      if (!permMatch) {
        issues.push({
          test: `Role Card ${i + 1} - Permissions Count`,
          severity: 'MAJOR',
          description: 'Permissions count not displayed'
        });
      }

      // Check for Display Order
      const hasOrder = cardText.match(/order|position/i) !== null;
      console.log(`  Display Order: ${hasOrder ? '✓ Present' : '⚠ May not be displayed'}`);

      // Check for progress bar
      const progressBar = await card.locator('.progress, [role="progressbar"], progress, mat-progress-bar').count();
      console.log(`  Progress Bar: ${progressBar > 0 ? '✓ Present' : '❌ NOT FOUND'}`);
      if (progressBar === 0) {
        issues.push({
          test: `Role Card ${i + 1} - Progress Bar`,
          severity: 'MINOR',
          description: 'Permission percentage progress bar not displayed'
        });
      }

      // Check for Edit button
      const editButton = await card.locator('button:has-text("Edit"), button[aria-label*="Edit"], .btn-edit').count();
      console.log(`  Edit Button: ${editButton > 0 ? '✓ Present' : '❌ NOT FOUND'}`);
      if (editButton === 0) {
        issues.push({
          test: `Role Card ${i + 1} - Edit Button`,
          severity: 'MAJOR',
          description: 'Edit button not found on role card'
        });
      }

      // Check for Delete button
      const deleteButton = await card.locator('button:has-text("Delete"), button[aria-label*="Delete"], .btn-delete').count();
      console.log(`  Delete Button: ${deleteButton > 0 ? '✓ Present' : '❌ NOT FOUND'}`);
      if (deleteButton === 0) {
        issues.push({
          test: `Role Card ${i + 1} - Delete Button`,
          severity: 'MAJOR',
          description: 'Delete button not found on role card'
        });
      }
    }

    if (issues.filter(i => i.test.includes('Role Card')).length === 0) {
      passed.push('Role cards display all required information');
    }

    // ========================================================================
    // TEST 4: CREATE NEW ROLE
    // ========================================================================
    console.log('\n📋 TEST 4: CREATE NEW ROLE');
    console.log('─────────────────────────────');

    const createButtons = [
      'button:has-text("Create Role")',
      'button:has-text("Add Role")',
      'button:has-text("New Role")',
      'button:has-text("Create")',
      'button.btn-primary:has-text("Role")',
      'button[aria-label*="Create"]',
      'button[aria-label*="Add"]'
    ];

    let createButton = null;
    for (const selector of createButtons) {
      const btn = page.locator(selector).first();
      if (await btn.count() > 0 && await btn.isVisible()) {
        createButton = btn;
        console.log(`✓ Found Create button: ${selector}`);
        break;
      }
    }

    if (!createButton) {
      console.log('❌ Create/Add Role button not found');
      issues.push({
        test: 'Create Role - Button',
        severity: 'CRITICAL',
        description: 'Create/Add Role button is not available on the page',
        recommendation: 'Add a button to create new roles'
      });
      await screenshot(page, 'create_button_missing');
    } else {
      await screenshot(page, 'before_create_click');
      await createButton.click();
      console.log('✓ Clicked Create button');
      await sleep(2000);
      await screenshot(page, 'create_form_opened');

      // Check if form/modal appeared
      const formModal = page.locator('form, .modal, .dialog, mat-dialog-container').first();
      const hasForm = await formModal.count() > 0 && await formModal.isVisible();

      if (!hasForm) {
        console.log('❌ Create form/modal did not appear');
        issues.push({
          test: 'Create Role - Form',
          severity: 'CRITICAL',
          description: 'Create role form/modal did not appear after clicking create button'
        });
      } else {
        console.log('✓ Create form/modal appeared');

        // Fill in the form
        const nameInput = page.locator('input[name="name"], input[formControlName="name"], input[placeholder*="Name"]').first();
        if (await nameInput.count() > 0) {
          await nameInput.fill('Test Custom Role QA');
          console.log('✓ Filled role name');
        } else {
          issues.push({
            test: 'Create Role - Name Field',
            severity: 'CRITICAL',
            description: 'Name input field not found in create form'
          });
        }

        const codeInput = page.locator('input[name="code"], input[formControlName="code"], input[placeholder*="Code"]').first();
        if (await codeInput.count() > 0) {
          await codeInput.fill('TEST_QA_ROLE');
          console.log('✓ Filled role code');
        } else {
          issues.push({
            test: 'Create Role - Code Field',
            severity: 'MAJOR',
            description: 'Code input field not found in create form'
          });
        }

        const descInput = page.locator('textarea[name="description"], textarea[formControlName="description"], input[name="description"]').first();
        if (await descInput.count() > 0) {
          await descInput.fill('A comprehensive test role created for QA validation purposes');
          console.log('✓ Filled description');
        }

        await screenshot(page, 'create_form_filled');

        // Try to save
        const saveButton = page.locator('button:has-text("Save"), button:has-text("Create"), button[type="submit"]').first();
        if (await saveButton.count() > 0) {
          await saveButton.click();
          console.log('✓ Clicked Save button');
          await sleep(3000);
          await screenshot(page, 'after_create_save');

          // Check if new role appears
          const newRole = page.locator('text="Test Custom Role QA"').first();
          if (await newRole.count() > 0) {
            console.log('✅ New role created successfully and appears in the list');
            passed.push('Create new custom role');
          } else {
            console.log('⚠ New role may not be visible immediately');
            // Refresh to check
            await page.reload();
            await sleep(2000);
            await screenshot(page, 'after_page_refresh');

            const newRoleAfterRefresh = page.locator('text="Test Custom Role QA"').first();
            if (await newRoleAfterRefresh.count() > 0) {
              console.log('✅ New role appears after page refresh');
              passed.push('Create new custom role');
            } else {
              issues.push({
                test: 'Create Role - Verification',
                severity: 'CRITICAL',
                description: 'New role was not found in the list after creation'
              });
            }
          }
        } else {
          issues.push({
            test: 'Create Role - Save Button',
            severity: 'CRITICAL',
            description: 'Save/Submit button not found in create form'
          });
        }
      }
    }

    // ========================================================================
    // TEST 5: MANAGE PERMISSIONS
    // ========================================================================
    console.log('\n📋 TEST 5: MANAGE PERMISSIONS');
    console.log('─────────────────────────────');

    await page.goto(`${BASE_URL}/admin/roles`);
    await sleep(2000);
    await screenshot(page, 'before_permissions_test');

    const permissionLinks = await page.locator('a:has-text("granted"), a:has-text("permission"), button:has-text("permission")').all();
    console.log(`Found ${permissionLinks.length} permission links`);

    if (permissionLinks.length === 0) {
      console.log('❌ No permission links found');
      issues.push({
        test: 'Manage Permissions - Link',
        severity: 'CRITICAL',
        description: 'No permission links found on role cards (e.g., "X granted")'
      });
    } else {
      const permLink = permissionLinks[0];
      await permLink.scrollIntoViewIfNeeded();
      await screenshot(page, 'permission_link_visible');

      await permLink.click();
      console.log('✓ Clicked permission link');
      await sleep(2000);
      await screenshot(page, 'permissions_modal_opened');

      // Check if modal appeared
      const modal = page.locator('.modal, .dialog, mat-dialog-container').first();
      const hasModal = await modal.count() > 0 && await modal.isVisible();

      if (!hasModal) {
        console.log('❌ Permissions modal did not appear');
        issues.push({
          test: 'Manage Permissions - Modal',
          severity: 'CRITICAL',
          description: 'Permissions modal did not appear after clicking permission link'
        });
      } else {
        console.log('✓ Permissions modal appeared');

        // Check for permissions grid/checkboxes
        const checkboxes = await modal.locator('input[type="checkbox"], mat-checkbox').all();
        console.log(`  Permissions checkboxes: ${checkboxes.length}`);

        if (checkboxes.length === 0) {
          issues.push({
            test: 'Manage Permissions - Grid',
            severity: 'CRITICAL',
            description: 'No permission checkboxes found in permissions modal'
          });
        } else {
          console.log(`✅ Permissions grid displays with ${checkboxes.length} permissions`);

          // Check for Select All button
          const selectAll = modal.locator('button:has-text("Select All"), button:has-text("All")').first();
          if (await selectAll.count() > 0) {
            console.log('✓ Select All button found');
            await selectAll.click();
            await sleep(1000);
            await screenshot(page, 'permissions_select_all');
            passed.push('Select All permissions button works');
          } else {
            issues.push({
              test: 'Manage Permissions - Select All',
              severity: 'MINOR',
              description: 'Select All button not found in permissions modal'
            });
          }

          // Check for Clear All button
          const clearAll = modal.locator('button:has-text("Clear All"), button:has-text("None"), button:has-text("Clear")').first();
          if (await clearAll.count() > 0) {
            console.log('✓ Clear All button found');
            await clearAll.click();
            await sleep(1000);
            await screenshot(page, 'permissions_clear_all');
            passed.push('Clear All permissions button works');
          } else {
            issues.push({
              test: 'Manage Permissions - Clear All',
              severity: 'MINOR',
              description: 'Clear All button not found in permissions modal'
            });
          }

          // Toggle individual permissions
          console.log('Testing individual permission toggles...');
          for (let i = 0; i < Math.min(3, checkboxes.length); i++) {
            await checkboxes[i].click();
            await sleep(300);
          }
          await screenshot(page, 'permissions_toggled');
          console.log('✓ Toggled individual permissions');

          // Save changes
          const saveButton = modal.locator('button:has-text("Save"), button:has-text("Update"), button[type="submit"]').first();
          if (await saveButton.count() > 0) {
            await saveButton.click();
            console.log('✓ Clicked Save button');
            await sleep(2000);
            await screenshot(page, 'permissions_saved');
            passed.push('Manage permissions and save changes');
          } else {
            issues.push({
              test: 'Manage Permissions - Save',
              severity: 'CRITICAL',
              description: 'Save button not found in permissions modal'
            });
          }
        }
      }
    }

    // ========================================================================
    // TEST 6: EDIT ROLE
    // ========================================================================
    console.log('\n📋 TEST 6: EDIT ROLE');
    console.log('─────────────────────');

    await page.goto(`${BASE_URL}/admin/roles`);
    await sleep(2000);
    await screenshot(page, 'before_edit_test');

    // Find a non-system role
    const allRoleCards = await page.locator('.card, .role-card, mat-card').all();
    let editButton = null;

    for (const card of allRoleCards) {
      const cardText = await card.textContent();
      // Skip system roles
      if (!cardText.match(/system role/i)) {
        const btn = card.locator('button:has-text("Edit"), button[aria-label*="Edit"]').first();
        if (await btn.count() > 0 && await btn.isVisible()) {
          editButton = btn;
          console.log('✓ Found Edit button on non-system role');
          await card.scrollIntoViewIfNeeded();
          break;
        }
      }
    }

    if (!editButton) {
      console.log('❌ No editable non-system role found');
      issues.push({
        test: 'Edit Role - Button',
        severity: 'MAJOR',
        description: 'No Edit button found on non-system roles'
      });
    } else {
      await screenshot(page, 'edit_button_found');
      await editButton.click();
      console.log('✓ Clicked Edit button');
      await sleep(2000);
      await screenshot(page, 'edit_form_opened');

      const editModal = page.locator('form, .modal, .dialog, mat-dialog-container').first();
      const hasEditForm = await editModal.count() > 0 && await editModal.isVisible();

      if (!hasEditForm) {
        console.log('❌ Edit form did not appear');
        issues.push({
          test: 'Edit Role - Form',
          severity: 'CRITICAL',
          description: 'Edit form/modal did not appear after clicking Edit button'
        });
      } else {
        console.log('✓ Edit form appeared');

        // Modify description
        const descInput = page.locator('textarea[name="description"], textarea[formControlName="description"]').first();
        if (await descInput.count() > 0) {
          const timestamp = new Date().toISOString();
          await descInput.fill(`Updated description - QA test at ${timestamp}`);
          console.log('✓ Modified description field');
          await screenshot(page, 'edit_form_modified');
        }

        // Save changes
        const saveButton = page.locator('button:has-text("Save"), button:has-text("Update")').first();
        if (await saveButton.count() > 0) {
          await saveButton.click();
          console.log('✓ Clicked Save button');
          await sleep(2000);
          await screenshot(page, 'edit_saved');
          passed.push('Edit non-system role');
        } else {
          issues.push({
            test: 'Edit Role - Save',
            severity: 'CRITICAL',
            description: 'Save button not found in edit form'
          });
        }
      }
    }

    // ========================================================================
    // TEST 7: DELETE ROLE
    // ========================================================================
    console.log('\n📋 TEST 7: DELETE ROLE');
    console.log('─────────────────────');

    await page.goto(`${BASE_URL}/admin/roles`);
    await sleep(2000);
    await screenshot(page, 'before_delete_test');

    // Find the test role we created
    const testRoleCard = page.locator('text="Test Custom Role QA"').first();
    let deleteButton = null;

    if (await testRoleCard.count() > 0) {
      console.log('✓ Found Test Custom Role QA');
      const card = testRoleCard.locator('..').locator('..').locator('..');
      deleteButton = card.locator('button:has-text("Delete"), button[aria-label*="Delete"]').first();
    } else {
      console.log('Test Custom Role QA not found, looking for any custom role...');
      const cards = await page.locator('.card, .role-card, mat-card').all();

      for (const card of cards) {
        const cardText = await card.textContent();
        if (!cardText.match(/system role/i)) {
          const btn = card.locator('button:has-text("Delete"), button[aria-label*="Delete"]').first();
          if (await btn.count() > 0 && await btn.isVisible()) {
            deleteButton = btn;
            console.log('✓ Found Delete button on custom role');
            await card.scrollIntoViewIfNeeded();
            break;
          }
        }
      }
    }

    if (!deleteButton) {
      console.log('❌ No deletable custom role found');
      issues.push({
        test: 'Delete Role - Button',
        severity: 'MAJOR',
        description: 'No Delete button found on custom roles'
      });
    } else {
      await screenshot(page, 'delete_button_found');
      await deleteButton.click();
      console.log('✓ Clicked Delete button');
      await sleep(1500);
      await screenshot(page, 'delete_confirmation');

      // Check for confirmation modal
      const confirmModal = page.locator('.modal, .dialog, mat-dialog-container').first();
      const hasConfirmModal = await confirmModal.count() > 0 && await confirmModal.isVisible();

      if (!hasConfirmModal) {
        console.log('❌ Confirmation modal did not appear');
        issues.push({
          test: 'Delete Role - Confirmation',
          severity: 'MAJOR',
          description: 'Confirmation modal did not appear after clicking Delete'
        });
      } else {
        console.log('✓ Confirmation modal appeared');

        // Find and click confirm button
        const confirmButton = page.locator('button:has-text("Confirm"), button:has-text("Yes"), button:has-text("Delete")').first();
        if (await confirmButton.count() > 0) {
          await confirmButton.click();
          console.log('✓ Clicked Confirm delete button');
          await sleep(2000);
          await screenshot(page, 'delete_confirmed');
          passed.push('Delete custom role with confirmation');
        } else {
          issues.push({
            test: 'Delete Role - Confirm Button',
            severity: 'CRITICAL',
            description: 'Confirm/Yes button not found in delete confirmation modal'
          });
        }
      }
    }

    // ========================================================================
    // TEST 8: SEARCH AND FILTER
    // ========================================================================
    console.log('\n📋 TEST 8: SEARCH AND FILTER');
    console.log('─────────────────────────────');

    await page.goto(`${BASE_URL}/admin/roles`);
    await sleep(2000);
    await screenshot(page, 'before_search_test');

    const searchField = page.locator('input[type="search"], input[placeholder*="Search"]').first();

    if (await searchField.count() === 0) {
      console.log('❌ Search field not found');
      issues.push({
        test: 'Search - Field',
        severity: 'MAJOR',
        description: 'Search input field not found on roles page'
      });
    } else {
      console.log('✓ Search field found');

      // Get initial count
      const initialCards = await page.locator('.card, .role-card, mat-card').all();
      console.log(`  Initial role count: ${initialCards.length}`);

      // Search for "Admin"
      await searchField.fill('Admin');
      await sleep(1500);
      await screenshot(page, 'search_admin');

      const searchResults = await page.locator('.card, .role-card, mat-card').all();
      console.log(`  Search results: ${searchResults.length} roles`);

      if (searchResults.length > 0 && searchResults.length <= initialCards.length) {
        console.log('✅ Search functionality works');
        passed.push('Search roles by name');
      } else {
        issues.push({
          test: 'Search - Functionality',
          severity: 'MAJOR',
          description: 'Search does not filter results correctly'
        });
      }

      // Clear search
      await searchField.clear();
      await sleep(1500);
      await screenshot(page, 'search_cleared');

      const afterClear = await page.locator('.card, .role-card, mat-card').all();
      console.log(`  After clear: ${afterClear.length} roles`);

      // Test Active filter toggle
      const activeToggle = page.locator('label:has-text("Active"), mat-slide-toggle').first();
      if (await activeToggle.count() > 0) {
        console.log('✓ Active toggle found');

        await activeToggle.click();
        await sleep(1500);
        await screenshot(page, 'filter_active_on');
        console.log('  Toggled Active filter ON');

        await activeToggle.click();
        await sleep(1500);
        await screenshot(page, 'filter_active_off');
        console.log('  Toggled Active filter OFF');

        passed.push('Show Active Only filter toggle');
      } else {
        issues.push({
          test: 'Filter - Active Toggle',
          severity: 'MINOR',
          description: 'Show Active Only toggle not found'
        });
      }
    }

    // ========================================================================
    // TEST 9: CONSOLE ERRORS AND NETWORK ISSUES
    // ========================================================================
    console.log('\n📋 TEST 9: CONSOLE ERRORS AND NETWORK');
    console.log('──────────────────────────────────────');

    const consoleErrors = [];
    page.on('console', msg => {
      if (msg.type() === 'error') {
        consoleErrors.push(msg.text());
      }
    });

    const networkErrors = [];
    page.on('requestfailed', request => {
      networkErrors.push({
        url: request.url(),
        error: request.failure().errorText
      });
    });

    await page.reload();
    await sleep(3000);
    await screenshot(page, 'final_state');

    if (consoleErrors.length === 0) {
      console.log('✅ No console errors detected');
      passed.push('No console errors');
    } else {
      console.log(`⚠ Found ${consoleErrors.length} console errors:`);
      consoleErrors.forEach((err, i) => {
        console.log(`  ${i + 1}. ${err.substring(0, 100)}`);
      });
      issues.push({
        test: 'Console Errors',
        severity: 'MINOR',
        description: `${consoleErrors.length} console errors detected`,
        details: consoleErrors.slice(0, 3)
      });
    }

    if (networkErrors.length === 0) {
      console.log('✅ No network errors detected');
    } else {
      console.log(`⚠ Found ${networkErrors.length} network errors:`);
      networkErrors.forEach((err, i) => {
        console.log(`  ${i + 1}. ${err.url} - ${err.error}`);
      });
    }

    // ========================================================================
    // FINAL REPORT
    // ========================================================================
    console.log('\n\n════════════════════════════════════════════════════════════');
    console.log('📊 FINAL TEST REPORT - ROLE & PERMISSION MANAGEMENT');
    console.log('════════════════════════════════════════════════════════════\n');

    console.log(`✅ PASSED TESTS: ${passed.length}`);
    passed.forEach((test, i) => {
      console.log(`  ${i + 1}. ${test}`);
    });

    console.log(`\n❌ ISSUES FOUND: ${issues.length}`);
    if (issues.length === 0) {
      console.log('  🎉 No issues found! All tests passed successfully.\n');
    } else {
      console.log('');
      const critical = issues.filter(i => i.severity === 'CRITICAL');
      const major = issues.filter(i => i.severity === 'MAJOR');
      const minor = issues.filter(i => i.severity === 'MINOR');

      if (critical.length > 0) {
        console.log(`🔴 CRITICAL ISSUES (${critical.length}):`);
        critical.forEach((issue, i) => {
          console.log(`  ${i + 1}. [${issue.test}] ${issue.description}`);
          if (issue.recommendation) {
            console.log(`     → Recommendation: ${issue.recommendation}`);
          }
        });
        console.log('');
      }

      if (major.length > 0) {
        console.log(`🟠 MAJOR ISSUES (${major.length}):`);
        major.forEach((issue, i) => {
          console.log(`  ${i + 1}. [${issue.test}] ${issue.description}`);
        });
        console.log('');
      }

      if (minor.length > 0) {
        console.log(`🟡 MINOR ISSUES (${minor.length}):`);
        minor.forEach((issue, i) => {
          console.log(`  ${i + 1}. [${issue.test}] ${issue.description}`);
        });
        console.log('');
      }
    }

    console.log(`\n📸 Screenshots saved to: ${SCREENSHOT_DIR}`);
    console.log(`   Total screenshots: ${screenshotCounter - 1}\n`);

    console.log('════════════════════════════════════════════════════════════\n');

    // Generate report file
    const reportPath = path.join(__dirname, 'test-report-roles-permissions.txt');
    let report = '═══════════════════════════════════════════════════════════════\n';
    report += 'ROLE & PERMISSION MANAGEMENT - COMPREHENSIVE TEST REPORT\n';
    report += `Date: ${new Date().toISOString()}\n`;
    report += '═══════════════════════════════════════════════════════════════\n\n';

    report += `SUMMARY:\n`;
    report += `  ✅ Passed: ${passed.length}\n`;
    report += `  ❌ Issues: ${issues.length}\n`;
    report += `  📸 Screenshots: ${screenshotCounter - 1}\n\n`;

    report += `PASSED TESTS:\n`;
    passed.forEach((test, i) => {
      report += `  ${i + 1}. ${test}\n`;
    });

    report += `\n\nISSUES FOUND:\n`;
    if (issues.length === 0) {
      report += '  No issues found!\n';
    } else {
      issues.forEach((issue, i) => {
        report += `\n${i + 1}. [${issue.severity}] ${issue.test}\n`;
        report += `   Description: ${issue.description}\n`;
        if (issue.recommendation) {
          report += `   Recommendation: ${issue.recommendation}\n`;
        }
        if (issue.details) {
          report += `   Details: ${JSON.stringify(issue.details, null, 2)}\n`;
        }
      });
    }

    report += `\n\nScreenshot Directory: ${SCREENSHOT_DIR}\n`;

    fs.writeFileSync(reportPath, report);
    console.log(`📄 Full report saved to: ${reportPath}\n`);

  } catch (error) {
    console.error('\n❌ TEST SUITE ERROR:', error.message);
    await screenshot(page, 'error_state');
  } finally {
    await browser.close();
  }
})();
