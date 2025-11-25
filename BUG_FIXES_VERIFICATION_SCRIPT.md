# BUG FIXES VERIFICATION SCRIPT

**Date**: 2025-11-17
**Tester**: QA Automation Engineer
**Status**: BLOCKED - Playwright MCP Server Lock Issue

## Technical Issue Encountered

**Problem**: Playwright MCP browser instance locked
```
Error: Browser is already in use for C:\Users\Navin Chandra\AppData\Local\ms-playwright\mcp-chrome-da4447b
```

**Required Action**: Restart Claude Code session to clear browser lock, OR execute manual testing

---

## MANUAL TESTING SCRIPT

### Prerequisites
1. Backend server running on port (check your configuration)
2. Angular app running on http://localhost:4200
3. Valid user credentials for login

---

## TEST 1: SET AS DEFAULT BUG FIX

### Steps:
1. **Navigate**: http://localhost:4200/email-settings
2. **Login** if required
3. **Identify Target**: Find any email server that does NOT have "Default" badge
   - Look for "Gmail SMTP Server - Production" or similar
4. **Execute Action**: Click "Set as Default" button on that server
5. **Verify Success**:
   - ✅ SUCCESS toast message appears (no 404 error)
   - ✅ The server now shows "Default" badge
   - ✅ Previous default server loses its badge
6. **Screenshot**: Save as `bug-fix-1-set-default-VERIFIED.png`

### Expected Backend Call:
```
PUT /api/email-server-settings/{id}/set-default
Response: 200 OK
```

### PASS Criteria:
- [ ] No 404 error in browser console
- [ ] Success message displayed
- [ ] Default badge appears on clicked server
- [ ] Only ONE server has default badge

### FAIL Indicators:
- ❌ 404 Not Found error
- ❌ No success message
- ❌ Badge doesn't update
- ❌ Multiple servers show default badge

---

## TEST 2: DELETE OPERATION BUG FIX

### Steps:
1. **Still on**: /email-settings page
2. **Create Test Server**:
   - Click "Add Email Server" or similar button
   - Fill in test data:
     ```
     Name: TEST-DELETE-ME
     Host: smtp.test.com
     Port: 587
     Username: test@test.com
     Password: test123
     ```
   - Save the server
3. **Count Before**: Note the total number of servers (e.g., "5 servers")
4. **Execute Delete**:
   - Find "TEST-DELETE-ME" server in the list
   - Click Delete/Trash icon
   - Confirm deletion in modal/dialog
5. **Verify Immediate Update**:
   - ✅ Server disappears from list IMMEDIATELY
   - ✅ Server count decreases by 1
   - ✅ No page refresh needed
   - ✅ No ghost entry remains
6. **Screenshot**: Save as `bug-fix-2-delete-VERIFIED.png`

### Expected Backend Call:
```
DELETE /api/email-server-settings/{id}
Response: 200 OK or 204 No Content
```

### PASS Criteria:
- [ ] Server removed from UI immediately
- [ ] Count updates without refresh
- [ ] No console errors
- [ ] Deletion confirmed in backend (refresh page - server stays gone)

### FAIL Indicators:
- ❌ Server still visible after delete
- ❌ Need to refresh page to see deletion
- ❌ Count doesn't update
- ❌ Server reappears after refresh

---

## TEST 3: PERSISTENCE VERIFICATION (BONUS)

### Steps:
1. **Refresh Browser**: F5 or Ctrl+R
2. **Verify**:
   - Default server badge persists
   - Deleted server is still gone
   - Changes are permanent

---

## RESULTS TEMPLATE

### Bug Fix #1: Set as Default
**Status**: ⬜ PASS / ⬜ FAIL / ⬜ BLOCKED

**Evidence**:
- Screenshot: `bug-fix-1-set-default-VERIFIED.png`
- Console logs: [No errors / Errors listed]
- Network request: [200 OK / Error code]

**Notes**:
```
[Your observations here]
```

---

### Bug Fix #2: Delete Operation
**Status**: ⬜ PASS / ⬜ FAIL / ⬜ BLOCKED

**Evidence**:
- Screenshot: `bug-fix-2-delete-VERIFIED.png`
- Console logs: [No errors / Errors listed]
- Network request: [200 OK / Error code]

**Notes**:
```
[Your observations here]
```

---

## FINAL VERDICT

### Overall Status: ⬜ PRODUCTION READY / ⬜ ISSUES FOUND / ⬜ TESTING BLOCKED

### Issues Remaining:
1. [List any issues found]

### Recommendation:
```
[APPROVE FOR PRODUCTION / NEEDS FIXES / NEEDS INVESTIGATION]
```

---

## AUTOMATED TEST (When Playwright Available)

```typescript
// Test Case 1: Set as Default
test('Bug Fix #1: Set as Default should return 200, not 404', async () => {
  await page.goto('http://localhost:4200/email-settings');

  // Find non-default server
  const nonDefaultServer = page.locator('[data-test-id="email-server-card"]')
    .filter({ hasNot: page.locator('.badge:has-text("Default")') })
    .first();

  // Click Set as Default
  await nonDefaultServer.locator('button:has-text("Set as Default")').click();

  // Verify success
  await expect(page.locator('.toast-success')).toBeVisible();
  await expect(nonDefaultServer.locator('.badge:has-text("Default")')).toBeVisible();

  // Verify no 404 in console
  const errors = await page.evaluate(() =>
    window.performance.getEntries()
      .filter(e => e.name.includes('set-default') && e.responseStatus === 404)
  );
  expect(errors).toHaveLength(0);
});

// Test Case 2: Delete Operation
test('Bug Fix #2: Delete should remove server immediately', async () => {
  await page.goto('http://localhost:4200/email-settings');

  // Create test server
  await page.click('button:has-text("Add Email Server")');
  await page.fill('[name="serverName"]', 'TEST-DELETE-ME');
  await page.fill('[name="host"]', 'smtp.test.com');
  await page.fill('[name="port"]', '587');
  await page.fill('[name="username"]', 'test@test.com');
  await page.fill('[name="password"]', 'test123');
  await page.click('button:has-text("Save")');

  // Count before
  const countBefore = await page.locator('[data-test-id="email-server-card"]').count();

  // Delete
  const testServer = page.locator('[data-test-id="email-server-card"]:has-text("TEST-DELETE-ME")');
  await testServer.locator('button[aria-label="Delete"]').click();
  await page.click('button:has-text("Confirm")'); // Confirm modal

  // Verify immediate removal
  await expect(testServer).not.toBeVisible({ timeout: 2000 });

  const countAfter = await page.locator('[data-test-id="email-server-card"]').count();
  expect(countAfter).toBe(countBefore - 1);
});
```

---

## NEXT STEPS

1. **If PASS**: Document in production deployment notes
2. **If FAIL**: Create detailed bug report with reproduction steps
3. **If BLOCKED**: Restart Claude Code session and re-run automated tests

**Testing Time Estimate**: 5-7 minutes manual testing
