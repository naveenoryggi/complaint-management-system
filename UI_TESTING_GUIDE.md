# UI Testing Guide - Ready to Test!

## Pre-Test Verification ✅

**Status**: ALL SYSTEMS OPERATIONAL

| Component | Status | Details |
|-----------|--------|---------|
| Angular App | ✅ RUNNING | http://localhost:4200 (HTTP 200) |
| .NET API | ✅ RUNNING | http://localhost:5058 |
| Status Master API | ✅ TESTED | 9 records retrieved |
| Priority Master API | ✅ TESTED | 5 records retrieved |
| Category API | ✅ TESTED | 11 records retrieved |
| Branch API | ✅ TESTED | 1 record retrieved |
| Department API | ✅ TESTED | 2 records retrieved |
| Section API | ✅ TESTED | 1 record retrieved |

---

## Quick Start Testing (5 minutes)

### Step 1: Open Application

1. Open your web browser (Chrome, Edge, or Firefox recommended)
2. Navigate to: **http://localhost:4200**
3. Log in with admin credentials
4. Open Browser Developer Tools (F12)
5. Go to Console tab - watch for any errors

### Step 2: Navigate to Admin Section

From the dashboard, navigate to Admin → Master Data Management

### Step 3: Quick Test Each Component

Test each component in order:

#### 1. Status Master Management (2 minutes)

**URL**: http://localhost:4200/admin/status-master

**Quick Tests**:
- [ ] Page loads without errors
- [ ] You see 9 system statuses displayed
- [ ] Search works (try searching "Submitted")
- [ ] Click "Show Active Only" toggle - filtering works
- [ ] Click "Create New Status" button - modal opens
- [ ] Try creating without filling form - validation errors appear
- [ ] Close modal - it closes cleanly
- [ ] Click edit on any status - modal opens with data pre-filled
- [ ] Verify code field is READ-ONLY (greyed out)
- [ ] Try editing a system status - warning appears
- [ ] No errors in browser console

**Expected Statuses**:
- Submitted, Under Review, In Progress, Escalated, Pending Info, Resolved, Closed, Rejected, Reopened

#### 2. Priority Master Management (2 minutes)

**URL**: http://localhost:4200/admin/priority-master

**Quick Tests**:
- [ ] Page loads without errors
- [ ] You see 5 system priorities displayed
- [ ] Search works
- [ ] Filter toggle works
- [ ] Create modal opens
- [ ] Validation works
- [ ] Edit modal works
- [ ] Code field is READ-ONLY
- [ ] System priority warning appears when editing system priorities
- [ ] No errors in browser console

**Expected Priorities**:
- Low (Level 1), Normal (Level 3), High (Level 5), Critical (Level 8), Urgent (Level 10)

#### 3. Category Management (2 minutes)

**URL**: http://localhost:4200/admin/category-management

**Quick Tests**:
- [ ] Page loads without errors
- [ ] You see 11 categories displayed
- [ ] Categories sorted by displayOrder
- [ ] Search works
- [ ] Filter works
- [ ] Create modal opens
- [ ] Validation works (priority must be 1-4, SLA 1-720)
- [ ] Edit modal works
- [ ] Can select parent category in form
- [ ] No errors in browser console

**Expected Categories**:
- Attendance Issues, Salary & Payroll, HRMS System, Leave Management, Performance Management, Benefits & Insurance, Workplace Harassment, IT & Technical Support, Facilities & Infrastructure, Policy & Compliance, Other

#### 4. Branch Management (2 minutes)

**URL**: http://localhost:4200/admin/branch-management

**Quick Tests**:
- [ ] Page loads without errors
- [ ] You see "Delhi Branch" displayed
- [ ] Three-way filter buttons work (All/Active/Inactive)
- [ ] Search works
- [ ] Create modal opens
- [ ] Validation works (code max 20 chars)
- [ ] Form has contact fields (email, phone, address, city, country)
- [ ] Edit modal works
- [ ] Code field is READ-ONLY
- [ ] Toggle status button works
- [ ] No errors in browser console

#### 5. Department Management (2 minutes)

**URL**: http://localhost:4200/admin/department-management

**Quick Tests**:
- [ ] Page loads without errors
- [ ] Branch dropdown loads
- [ ] "Delhi Branch" is auto-selected
- [ ] You see 2 departments: "laxmi nagar" and "southx"
- [ ] Changing branch updates department list
- [ ] Three-way filter works
- [ ] Search works
- [ ] Create requires branch selection
- [ ] Create modal opens
- [ ] Validation works
- [ ] Edit modal works
- [ ] Code field is READ-ONLY
- [ ] No errors in browser console

#### 6. Section Management (2 minutes)

**URL**: http://localhost:4200/admin/section-management

**Quick Tests**:
- [ ] Page loads without errors
- [ ] Branch dropdown loads
- [ ] Department dropdown appears after selecting branch
- [ ] Select "Delhi Branch" → departments load
- [ ] Select "laxmi nagar" department → section "nirman vihar" appears
- [ ] "Show Inactive" checkbox works (inverted logic)
- [ ] Search works
- [ ] Create requires both branch AND department
- [ ] Create modal opens
- [ ] Validation works
- [ ] Edit modal works
- [ ] Code field is READ-ONLY
- [ ] No errors in browser console

---

## Comprehensive Testing (30 minutes)

### For Each Component, Test ALL CRUD Operations:

#### CREATE Operation
1. Click "Create New [Entity]" button
2. Leave all fields empty → Click Save
   - ✓ Validation errors should appear
   - ✓ Error message should be clear
3. Fill only Name field → Click Save
   - ✓ "Code is required" error should appear
4. Fill Name and Code → Click Save
   - ✓ Should succeed or show entity-specific validation errors
5. Fill all fields correctly → Click Save
   - ✓ Success message should appear
   - ✓ Modal should close
   - ✓ New item should appear in list
   - ✓ Success message should auto-dismiss after 3 seconds

#### READ Operation
1. Verify all items load on page load
2. Verify items are displayed correctly
3. Verify all columns show proper data
4. Verify hierarchical data (Branch → Department → Section)

#### UPDATE Operation
1. Click Edit icon on any item
2. Verify modal opens with data pre-filled
3. Verify code field is READ-ONLY (greyed out)
4. Modify name field
5. Click Save
   - ✓ Success message should appear
   - ✓ Changes should reflect in list
6. For system entities (Status/Priority), verify warning appears

#### DELETE Operation
1. Click Delete icon on any item
2. Verify confirmation modal appears
3. Click Cancel → Modal closes, item remains
4. Click Delete icon again
5. Click Confirm
   - ✓ Success message should appear
   - ✓ Item should be removed from list
6. For system entities, verify warning about system entity

#### SEARCH Operation
1. Type in search box
2. Verify results filter instantly
3. Test search by:
   - Name (partial match)
   - Code (partial match)
   - Description (if searchable)
4. Clear search → All items should reappear

#### FILTER Operation
1. Toggle "Show Active Only" or equivalent
2. Verify only active items appear
3. Toggle again → All items appear
4. For Branch/Department, test All/Active/Inactive buttons

---

## Entity-Specific Tests

### Status Master - Color & Icon Testing
- [ ] Create status with hex color #FF5733 → Verify color shows correctly
- [ ] Try invalid color "red" → Validation error should appear
- [ ] Try invalid hex #GGG111 → Validation error should appear
- [ ] Test displayOrder field (0-9999)
- [ ] Test isFinal checkbox
- [ ] Verify statuses sort by displayOrder

### Priority Master - SLA Testing
- [ ] Create priority with Level = 3
- [ ] Set SLA Response = 24 hours
- [ ] Set SLA Resolution = 72 hours → Should succeed
- [ ] Try setting SLA Response = 80 hours, SLA Resolution = 72 hours
  - ✓ Validation error: "Response cannot be greater than Resolution"
- [ ] Test level validation (must be 1-5)
- [ ] Verify priorities sort by displayOrder

### Category - Hierarchical Testing
- [ ] Create top-level category (no parent)
- [ ] Create child category (select parent)
- [ ] Verify child shows parent name
- [ ] Test default priority (must be 1-4)
- [ ] Test default SLA hours (must be 1-720)
- [ ] Verify categories sort by displayOrder

### Branch - Contact Information Testing
- [ ] Create branch with all contact fields
- [ ] Verify contact email, phone, address, city, country all save correctly
- [ ] Test 3-way status filter thoroughly
- [ ] Test toggle status button
- [ ] Verify active/inactive counts update

### Department - Branch Dependency Testing
- [ ] Try creating department without selecting branch
  - ✓ Error: "Please select a branch first"
- [ ] Select branch → Create department → Should succeed
- [ ] Change branch → Verify department list updates
- [ ] Verify department only appears under correct branch

### Section - Three-Level Hierarchy Testing
- [ ] Try creating section without branch
  - ✓ Error message should appear
- [ ] Select branch → Department dropdown should populate
- [ ] Try creating section without department
  - ✓ Error: "Please select a department first"
- [ ] Select department → Create section → Should succeed
- [ ] Change branch → Verify departments reset
- [ ] Change department → Verify sections update
- [ ] Test "Show Inactive" checkbox (inverted logic)

---

## Permission Testing

**To test permissions properly**:

1. Log out of admin account
2. Log in with a user WITHOUT "ManageSettings" permission
3. Navigate to each master management page
4. Verify:
   - [ ] Create button is disabled OR shows error when clicked
   - [ ] Edit buttons are disabled OR show error when clicked
   - [ ] Delete buttons are disabled OR show error when clicked
   - [ ] View/search functionality still works
   - [ ] Error message: "You do not have permission..."

---

## Error Scenarios Testing

### Test API Failures

1. **Stop Backend API**:
   - Stop the .NET API server
   - Try any operation in UI
   - ✓ Verify error message: "Failed to load [entity]. Please try again."
   - ✓ Verify user is not left in broken state
   - Restart API and verify recovery

2. **Test Network Delays**:
   - Open Developer Tools → Network tab
   - Set throttling to "Slow 3G"
   - Try loading any component
   - ✓ Verify loading spinner appears
   - ✓ Verify data loads eventually

3. **Test Invalid Data**:
   - Try creating entity with very long name (>500 chars)
   - Try special characters in code field
   - Verify appropriate error messages

---

## Browser Console Monitoring

**CRITICAL**: Monitor browser console throughout ALL tests

Watch for:
- ❌ **Red errors** - These are critical, note them down
- ⚠️ **Yellow warnings** - These are acceptable for now
- 🔵 **Blue info** - These are fine

**Common Expected Messages** (these are OK):
- Deprecation warnings from Angular
- Font loading warnings
- Some styling warnings

**Unexpected Messages** (these are PROBLEMS):
- TypeScript errors
- "Cannot read property of undefined"
- "Failed to load module"
- HTTP 404/500 errors
- CORS errors

---

## Performance Testing

Test each component's performance:

1. **Initial Load Time**:
   - Clear browser cache (Ctrl+Shift+Delete)
   - Navigate to component
   - Note load time (should be < 2 seconds)

2. **Search Performance**:
   - With 10+ items, type in search
   - Results should filter instantly (< 100ms)

3. **CRUD Operation Time**:
   - Create/Update/Delete should complete in < 1 second
   - Loading spinner should show during operation

4. **Hierarchical Loading**:
   - Branch → Department → Section navigation
   - Each level should load in < 500ms

---

## Testing Checklist Summary

### Quick Test (5 min) - Status

- [ ] All 6 components load without errors
- [ ] All APIs return data
- [ ] No console errors
- [ ] Basic CRUD operations work

### Standard Test (30 min) - Status

- [ ] All CRUD operations tested for each component
- [ ] All validation rules tested
- [ ] All filters and search tested
- [ ] Hierarchical loading tested (Branch/Dept/Section)
- [ ] System entity warnings tested
- [ ] Permission enforcement tested
- [ ] Error scenarios tested

### Comprehensive Test (2 hours) - Status

- [ ] Every field validated
- [ ] Every edge case tested
- [ ] Performance benchmarks met
- [ ] Browser compatibility tested (Chrome, Edge, Firefox)
- [ ] Mobile responsiveness tested
- [ ] Accessibility tested
- [ ] All checklist items from UI_TESTING_CHECKLIST.md completed

---

## Reporting Issues

If you find issues, document them in this format:

```
Component: [Component Name]
Operation: [Create/Read/Update/Delete/Search/Filter]
Issue: [Brief description]
Steps to Reproduce:
1.
2.
3.
Expected: [What should happen]
Actual: [What actually happened]
Browser: [Chrome/Edge/Firefox]
Console Errors: [Copy any red errors from console]
Screenshot: [If applicable]
Priority: [High/Medium/Low]
```

---

## Automated Tests Available

You can run automated API tests anytime:

```powershell
# From project root
.\simple-smoke-test.ps1
```

This will test all 6 API endpoints automatically.

---

## Current Test Status

| Test Suite | Status | Notes |
|------------|--------|-------|
| API Health Check | ✅ PASSED | All 6 APIs tested successfully |
| Angular App Accessibility | ✅ PASSED | HTTP 200 confirmed |
| Manual UI Testing | ⏳ PENDING | Ready to begin |
| Permission Testing | ⏳ PENDING | Requires non-admin user |
| Error Scenario Testing | ⏳ PENDING | Requires API shutdown |
| Performance Testing | ⏳ PENDING | Requires measurements |
| Browser Compatibility | ⏳ PENDING | Requires multiple browsers |

---

## Next Steps

1. ✅ **COMPLETED**: API verification (all 6 APIs tested)
2. ✅ **COMPLETED**: Application accessibility confirmed
3. ➡️ **CURRENT**: Follow this guide to test UI in browser
4. ⏳ **NEXT**: Document any issues found
5. ⏳ **FINAL**: Create test results summary

---

## Support Documentation

- Full test checklist: `UI_TESTING_CHECKLIST.md` (430+ test cases)
- Developer guide: `BASE_CLASS_PATTERN_GUIDE.md` (800+ lines)
- API smoke test: `simple-smoke-test.ps1` (automated)

---

## Success Criteria

Testing is complete when:
- [ ] All 6 components tested manually
- [ ] All CRUD operations verified working
- [ ] No critical console errors
- [ ] All validation working correctly
- [ ] Hierarchical loading working
- [ ] Permission enforcement working
- [ ] Issues documented (if any)
- [ ] Test results summary created

---

**Ready to Test!** 🚀

Open http://localhost:4200 in your browser and start testing!
