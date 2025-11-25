# Section Management Inactive Filter Bug Test Report

**Date:** November 1, 2025
**Tester:** QA Automation Engineer (Claude)
**Test Environment:** http://localhost:4200
**Status:** BUG NOT FIXED - INCORRECT FIX APPLIED

---

## Executive Summary

The bug fix that was claimed to be applied did NOT resolve the inactive sections filter issue. After comprehensive testing, I have identified that the **WRONG FIX** was applied, and the bug still exists in the codebase.

### Severity: HIGH
### Priority: HIGH
### Bug Status: OPEN (Fix needs to be re-applied correctly)

---

## Test Scenario

**Objective:** Verify that inactive sections appear when the "Show Inactive Sections" checkbox is enabled.

**Test Steps:**
1. Navigate to http://localhost:4200 and login as admin@complaintmanagement.com
2. Navigate to Section Management (/admin/sections)
3. Select "Branch 001 (BR001)" and "Test Department for Sections (TEST-DEPT)"
4. Observe with "Show Inactive Sections" checkbox UNCHECKED (baseline)
5. Enable the "Show Inactive Sections" checkbox
6. Verify if inactive sections appear

---

## Test Results

### Test Evidence

#### Screenshot 1: Checkbox Unchecked (Baseline)
**File:** `1_dashboard_logged_in.png`, `2_section_management_checkbox_unchecked.png`
- Branch: Branch 001 (BR001)
- Department: Test Department for Sections (TEST-DEPT)
- Show Inactive Sections: UNCHECKED
- Result: "No Sections Found"
- **EXPECTED BEHAVIOR:** Only active sections should be shown (none exist)

#### Screenshot 2: Checkbox Checked (After "Fix")
**File:** `3_checkbox_checked_but_no_sections.png`
- Branch: Branch 001 (BR001)
- Department: Test Department for Sections (TEST-DEPT)
- Show Inactive Sections: CHECKED ✓
- Result: "No Sections Found"
- **EXPECTED BEHAVIOR:** Inactive sections should appear (including "E2E Test Section (Updated)")
- **ACTUAL BEHAVIOR:** Still shows "No Sections Found"

### Network Request Analysis

**API Call Made When Checkbox is CHECKED:**
```
GET http://localhost:5058/api/sections?departmentId=08dde87d-7d8d-4181-a5fb-a1325dcf4970&activeOnly=true
```

**CRITICAL ISSUE:** The API is being called with `activeOnly=true` even though the checkbox is checked!

**Expected API Call:**
```
GET http://localhost:5058/api/sections?departmentId=08dde87d-7d8d-4181-a5fb-a1325dcf4970&activeOnly=false
```

---

## Root Cause Analysis

### Claimed Fix (INCORRECT)
**Location:** `complaint-system-angular/src/app/components/admin/section-management/section-management.component.ts` Line 199

**What Was Claimed:**
> Changed `getSections(departmentId, false)` to `getSections(departmentId, true)` to load ALL sections (including inactive).

**Why This is WRONG:**
This is the OPPOSITE of what should have been done! Let me explain:

### Service Method Signature
**File:** `complaint-system-angular/src/app/services/section.service.ts` Line 28
```typescript
getSections(departmentId: string, activeOnly: boolean = false): Observable<Section[]>
```

**Parameter:** `activeOnly: boolean`
- `activeOnly = true` → Only fetch ACTIVE sections
- `activeOnly = false` → Fetch ALL sections (active + inactive)

### Current Code (BUGGY)
**File:** `section-management.component.ts` Line 199
```typescript
protected override loadItems(): void {
  if (!this.selectedDepartmentId) {
    this.items = [];
    this.filteredItems = [];
    return;
  }

  this.loading = true;
  this.errorMessage = '';

  this.sectionService.getSections(this.selectedDepartmentId, true).subscribe({  // ← BUG HERE!
    next: (data) => {
      this.items = data;
      this.filterItems();
      this.loading = false;
    },
    error: (error) => {
      this.errorMessage = 'Failed to load sections. Please try again.';
      this.loading = false;
      this.logger.error('Error loading sections', error, 'SectionManagementComponent');
    }
  });
}
```

**Problem:** The second parameter is HARDCODED to `true`, meaning it ALWAYS fetches only active sections, completely ignoring the checkbox state!

### The Component Has the Right Property!
**File:** `section-management.component.ts` Lines 88-95
```typescript
// Map base class showActiveOnly to Section's showInactive (inverted logic)
get showInactive(): boolean {
  return !this.showActiveOnly;
}

set showInactive(value: boolean) {
  this.showActiveOnly = !value;
}
```

The component has `this.showActiveOnly` property that tracks the checkbox state, but it's NOT being used in the `loadItems()` method!

---

## Correct Fix

### Solution

**File:** `complaint-system-angular/src/app/components/admin/section-management/section-management.component.ts`
**Line:** 199

**Change FROM:**
```typescript
this.sectionService.getSections(this.selectedDepartmentId, true).subscribe({
```

**Change TO:**
```typescript
this.sectionService.getSections(this.selectedDepartmentId, this.showActiveOnly).subscribe({
```

### Explanation

1. When checkbox is UNCHECKED (Show Inactive = false):
   - `this.showInactive = false`
   - `this.showActiveOnly = true` (inverted)
   - API called with `activeOnly=true`
   - Result: Only active sections shown ✓

2. When checkbox is CHECKED (Show Inactive = true):
   - `this.showInactive = true`
   - `this.showActiveOnly = false` (inverted)
   - API called with `activeOnly=false`
   - Result: All sections (active + inactive) shown ✓

---

## Additional Filtering

The component also has client-side filtering at lines 213-232:

```typescript
protected override filterItems(): void {
  let filtered = this.items;

  // Filter by active status (uses showActiveOnly from base class)
  if (this.showActiveOnly) {
    filtered = filtered.filter(s => s.isActive);
  }

  // Filter by search term
  if (this.searchTerm) {
    const search = this.searchTerm.toLowerCase();
    filtered = filtered.filter(s =>
      s.name.toLowerCase().includes(search) ||
      s.code.toLowerCase().includes(search) ||
      (s.description && s.description.toLowerCase().includes(search))
    );
  }

  this.filteredItems = filtered;
}
```

This filtering is CORRECT and works as expected, but it can't filter data that was never fetched from the API!

---

## Impact Assessment

### User Impact
- **CRITICAL:** Users cannot view, edit, or delete inactive sections
- **BLOCKER:** Users cannot manage section lifecycle (activate/deactivate)
- **DATA VISIBILITY:** Inactive sections are completely hidden from the UI

### Business Impact
- Incomplete section management functionality
- Users may think sections are deleted when they are just inactive
- No way to reactivate sections through the UI

---

## Test Verdict

**FAILED** ❌

The bug fix that was claimed to be applied is:
1. **INCORRECT** - The wrong change was made
2. **INEFFECTIVE** - The bug still exists
3. **COUNTERPRODUCTIVE** - Made it worse by hardcoding to show only active

---

## Recommendations

### Immediate Action Required

1. **Revert the incorrect fix** (if it was actually applied)
2. **Apply the correct fix** as documented above (line 199)
3. **Re-test** the functionality with the correct fix
4. **Verify API calls** show `activeOnly=false` when checkbox is checked

### Testing Plan After Fix

1. Navigate to Section Management
2. Select branch and department with inactive sections
3. Verify checkbox UNCHECKED shows only active sections
4. Click checkbox to enable "Show Inactive Sections"
5. Verify API call shows `activeOnly=false`
6. Verify inactive sections appear in the table
7. Verify statistics show correct Active/Inactive counts
8. Test editing an inactive section
9. Test deleting an inactive section
10. Test search functionality with inactive sections

### Code Review Recommendation

Before applying fixes:
1. **Read the service method signature** to understand parameter names and types
2. **Understand the parameter semantics** (what `true` vs `false` means)
3. **Test the fix** before claiming it's resolved
4. **Verify network requests** to ensure correct API calls

---

## Attachments

### Screenshots
- `C:\Users\Navin Chandra\Pictures\Complaint management system\.playwright-mcp\1_dashboard_logged_in.png`
- `C:\Users\Navin Chandra\Pictures\Complaint management system\.playwright-mcp\2_section_management_checkbox_unchecked.png`
- `C:\Users\Navin Chandra\Pictures\Complaint management system\.playwright-mcp\3_checkbox_checked_but_no_sections.png`

### Source Files Referenced
- `complaint-system-angular/src/app/components/admin/section-management/section-management.component.ts`
- `complaint-system-angular/src/app/services/section.service.ts`

### Network Evidence
```
API Call with Checkbox CHECKED (WRONG):
GET /api/sections?departmentId=08dde87d-7d8d-4181-a5fb-a1325dcf4970&activeOnly=true

API Call with Checkbox CHECKED (EXPECTED):
GET /api/sections?departmentId=08dde87d-7d8d-4181-a5fb-a1325dcf4970&activeOnly=false
```

---

## Sign-off

**Test Execution Date:** November 1, 2025, 11:42 PM IST
**Tested By:** QA Automation Engineer (Claude)
**Status:** Bug Still Open - Incorrect Fix Applied
**Next Steps:** Apply correct fix and retest

---

**END OF REPORT**
