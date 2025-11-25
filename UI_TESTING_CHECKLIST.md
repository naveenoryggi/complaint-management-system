# UI Testing Checklist - Base Class Refactored Components

## Testing Prerequisites

- [ ] Angular dev server running on http://localhost:4200
- [ ] .NET API running on http://localhost:5058
- [ ] Logged in as admin user with ManageSettings permission
- [ ] Token: eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9... (valid until expiry)

---

## 1. Status Master Management Component

### Initial Load
- [ ] Navigate to Status Master Management page
- [ ] Verify page loads without errors
- [ ] Verify 9 system statuses are displayed
- [ ] Verify all statuses show correct color codes and icons
- [ ] Verify search box is functional

### Filtering & Search
- [ ] Toggle "Show Active Only" - verify filtering works
- [ ] Test search by status name (e.g., "Submitted")
- [ ] Test search by status code (e.g., "SUBMITTED")
- [ ] Verify filtered results update immediately

### Create Operation
- [ ] Click "Create New Status" button
- [ ] Verify modal opens with title "Create New Status"
- [ ] Test validation: Leave name empty → should show error
- [ ] Test validation: Leave code empty → should show error
- [ ] Test validation: Enter invalid hex color (e.g., "red") → should show error
- [ ] Test validation: Enter valid hex color (e.g., "#FF5733") → should accept
- [ ] Fill all required fields and click Save
- [ ] Verify success message appears
- [ ] Verify new status appears in the list
- [ ] Verify modal closes automatically

### Edit Operation
- [ ] Click edit icon on a NON-SYSTEM status
- [ ] Verify modal opens with title "Edit Status"
- [ ] Verify form is pre-filled with existing values
- [ ] Verify code field is READ-ONLY (greyed out)
- [ ] Modify name and description
- [ ] Change color code
- [ ] Toggle isActive checkbox
- [ ] Click Save
- [ ] Verify success message appears
- [ ] Verify changes are reflected in the list
- [ ] **SYSTEM STATUS EDIT**: Click edit on a SYSTEM status (e.g., "Submitted")
- [ ] Verify code field is READ-ONLY
- [ ] Verify warning message: "This is a system-defined status..."
- [ ] Verify can still edit name, description, display order

### Delete Operation
- [ ] Click delete icon on a NON-SYSTEM status
- [ ] Verify delete confirmation modal appears
- [ ] Click Cancel → verify modal closes, status remains
- [ ] Click delete icon again
- [ ] Click Confirm → verify success message
- [ ] Verify status is removed from list
- [ ] **SYSTEM STATUS DELETE**: Attempt to delete a SYSTEM status
- [ ] Verify warning message appears about deleting system status
- [ ] Confirm deletion if warning accepted

### Toggle Status
- [ ] Click toggle button on an active status
- [ ] Verify status changes to inactive
- [ ] Verify success message appears
- [ ] Click toggle button on inactive status
- [ ] Verify status changes to active

### Display Order
- [ ] Verify statuses are displayed in correct displayOrder sequence
- [ ] Edit a status and change displayOrder
- [ ] Verify list reorders after save

---

## 2. Priority Master Management Component

### Initial Load
- [ ] Navigate to Priority Master Management page
- [ ] Verify 5 system priorities are displayed (Low, Normal, High, Critical, Urgent)
- [ ] Verify all priorities show correct level, SLA hours, and color codes

### Filtering & Search
- [ ] Toggle "Show Active Only" - verify filtering works
- [ ] Test search by priority name
- [ ] Test search by priority code
- [ ] Verify filtered results update immediately

### Create Operation
- [ ] Click "Create New Priority" button
- [ ] Test validation: Leave name empty → should show error
- [ ] Test validation: Leave code empty → should show error
- [ ] Test validation: Enter level < 1 → should show error
- [ ] Test validation: Enter level > 5 → should show error
- [ ] Test validation: SLA Response > SLA Resolution → should show error
- [ ] Fill valid data:
  - Name: "Test Priority"
  - Code: "TEST_PRIORITY"
  - Level: 3
  - SLA Response: 12 hours
  - SLA Resolution: 48 hours
- [ ] Click Save → verify success

### Edit Operation
- [ ] Edit NON-SYSTEM priority
- [ ] Verify code field is READ-ONLY
- [ ] Modify SLA hours
- [ ] Test cross-field validation: Set Response hours > Resolution hours → should error
- [ ] Fix validation and save → verify success
- [ ] **SYSTEM PRIORITY EDIT**: Edit a SYSTEM priority (e.g., "High")
- [ ] Verify warning message appears
- [ ] Verify can still modify all fields except code

### Delete Operation
- [ ] Delete NON-SYSTEM priority → verify success
- [ ] **SYSTEM PRIORITY DELETE**: Attempt to delete SYSTEM priority
- [ ] Verify warning appears

---

## 3. Category Management Component

### Initial Load
- [ ] Navigate to Category Management page
- [ ] Verify 11 categories are displayed
- [ ] Verify categories are sorted by displayOrder

### Filtering & Search
- [ ] Toggle "Show Active Only"
- [ ] Test search functionality
- [ ] Verify results filter correctly

### Create Operation
- [ ] Click "Create New Category"
- [ ] Test validation: Leave name empty → error
- [ ] Test validation: Leave code empty → error
- [ ] Test validation: defaultPriority < 1 → error
- [ ] Test validation: defaultPriority > 4 → error
- [ ] Test validation: defaultSlaHours < 1 → error
- [ ] Test validation: defaultSlaHours > 720 → error
- [ ] Fill valid data:
  - Name: "Test Category"
  - Code: "TEST_CAT"
  - Default Priority: 2
  - Default SLA Hours: 48
  - Display Order: 100
- [ ] Click Save → verify success

### Hierarchical Categories
- [ ] Create a parent category (no parent selected)
- [ ] Create a child category (select parent category)
- [ ] Verify child category shows parent category name
- [ ] Test filtering of parent categories (should only show active top-level categories)

### Edit Operation
- [ ] Edit a category
- [ ] Verify code field is READ-ONLY
- [ ] Change parent category
- [ ] Modify default priority and SLA hours
- [ ] Verify validation works
- [ ] Save and verify changes

### Delete Operation
- [ ] Delete a category without children → verify success
- [ ] Attempt to delete a category WITH children → verify appropriate error/warning

---

## 4. Branch Management Component

### Initial Load
- [ ] Navigate to Branch Management page
- [ ] Verify branch list loads (should show "Delhi Branch")
- [ ] Verify companyId is automatically set from current user

### Three-Way Status Filter
- [ ] Click "All" filter → verify shows both active and inactive branches
- [ ] Click "Active" filter → verify shows only active branches
- [ ] Click "Inactive" filter → verify shows only inactive branches

### Filtering & Search
- [ ] Test search by name
- [ ] Test search by code
- [ ] Test search by city
- [ ] Test search by country

### Create Operation
- [ ] Click "Create New Branch"
- [ ] Verify companyId is pre-filled (not editable)
- [ ] Test validation: Leave name empty → error
- [ ] Test validation: Leave code empty → error
- [ ] Test validation: Code > 20 characters → error
- [ ] Fill valid data:
  - Name: "Test Branch"
  - Code: "TEST001"
  - Contact Email: "test@example.com"
  - Contact Phone: "+91-1234567890"
  - Address: "Test Address"
  - City: "Test City"
  - Country: "India"
- [ ] Click Save → verify success

### Edit Operation
- [ ] Edit "Delhi Branch"
- [ ] Verify code field is READ-ONLY
- [ ] Modify contact email, phone, address
- [ ] Verify all 9 branch-specific fields are editable
- [ ] Save and verify changes

### Delete Operation
- [ ] Create a test branch
- [ ] Delete the test branch → verify success
- [ ] Attempt to delete branch with departments → verify appropriate warning

### Toggle Status
- [ ] Toggle branch status active/inactive
- [ ] Verify success message
- [ ] Verify status changes in UI

### Utility Methods
- [ ] Verify getActiveBranchesCount() displays correct count
- [ ] Verify getInactiveBranchesCount() displays correct count

---

## 5. Department Management Component

### Initial Load
- [ ] Navigate to Department Management page
- [ ] Verify branch dropdown loads with branches
- [ ] Verify first branch is auto-selected
- [ ] Verify departments for selected branch load automatically

### Hierarchical Loading
- [ ] Change selected branch in dropdown
- [ ] Verify department list updates immediately
- [ ] Verify departments belong to selected branch
- [ ] Select empty branch (no departments) → verify empty state

### Three-Way Status Filter
- [ ] Test All/Active/Inactive filters
- [ ] Verify filtering works correctly

### Create Operation
- [ ] Ensure a branch is selected
- [ ] Click "Create New Department"
- [ ] Verify branchId is pre-filled from selected branch
- [ ] Test validation: Leave name empty → error
- [ ] Test validation: Leave code empty → error
- [ ] Test validation: Code > 20 characters → error
- [ ] Fill valid data:
  - Name: "Test Department"
  - Code: "TEST_DEPT"
  - Description: "Test description"
- [ ] Click Save → verify success

### Branch Dependency
- [ ] Try creating department without selecting branch → verify error message
- [ ] Verify department form includes branchId validation

### Edit Operation
- [ ] Edit "laxmi nagar" department
- [ ] Verify code field is READ-ONLY
- [ ] Modify name, description
- [ ] Add managerId, secondaryManagerId, hrResponsibleId (if applicable)
- [ ] Save and verify changes

### Delete Operation
- [ ] Delete test department → verify success
- [ ] Attempt to delete department with sections → verify warning

### Toggle Status
- [ ] Toggle department status
- [ ] Verify success message and UI update

---

## 6. Section Management Component

### Initial Load
- [ ] Navigate to Section Management page
- [ ] Verify branch dropdown loads
- [ ] Select a branch → verify department dropdown populates
- [ ] Select a department → verify sections list loads

### Three-Level Hierarchical Loading
- [ ] Change branch → verify departments reset and reload
- [ ] Verify sections list clears when branch changes
- [ ] Change department → verify sections reload for new department
- [ ] Test navigation: Branch → Department → Section

### showInactive vs showActiveOnly Mapping
- [ ] Toggle "Show Inactive" checkbox
- [ ] Verify filtering works (inverse logic of showActiveOnly)
- [ ] Verify only active sections shown when unchecked
- [ ] Verify all sections shown when checked

### Create Operation
- [ ] Try creating without selecting department → verify error
- [ ] Select branch and department
- [ ] Click "Create New Section"
- [ ] Verify departmentId is pre-filled
- [ ] Test validation: Leave name empty → error
- [ ] Test validation: Leave code empty → error
- [ ] Test validation: Code > 20 characters → error
- [ ] Fill valid data:
  - Name: "Test Section"
  - Code: "TEST_SEC"
  - Description: "Test description"
- [ ] Click Save → verify success

### Edit Operation
- [ ] Edit "nirman vihar" section
- [ ] Verify code field is READ-ONLY
- [ ] Modify name, description
- [ ] Add headId, secondaryHeadId, hrResponsibleId (if applicable)
- [ ] Save and verify changes

### Delete Operation
- [ ] Delete test section → verify success

### Toggle Status
- [ ] Toggle section status
- [ ] Verify success message and UI update

### Utility Methods
- [ ] Verify getSelectedDepartmentName() displays correct department name
- [ ] Verify getActiveSectionsCount() displays correct count
- [ ] Verify getInactiveSectionsCount() displays correct count

---

## Cross-Component Tests

### Navigation
- [ ] Test navigation between all 6 management pages
- [ ] Verify no errors in browser console during navigation
- [ ] Verify data persists correctly after navigation

### Permission Enforcement
- [ ] Test with user WITHOUT ManageSettings permission
- [ ] Verify Create buttons are disabled
- [ ] Verify Edit buttons are disabled
- [ ] Verify Delete buttons are disabled
- [ ] Verify appropriate error messages appear

### Error Handling
- [ ] Simulate API errors (stop backend)
- [ ] Verify graceful error messages appear
- [ ] Verify user is not left in broken state
- [ ] Restart backend and verify recovery

### Modal Behavior
- [ ] Test ESC key to close modals
- [ ] Test clicking outside modal to close
- [ ] Test Cancel button in modals
- [ ] Verify form resets when modal closes

### Loading States
- [ ] Verify loading spinners appear during API calls
- [ ] Verify loading states don't block UI unnecessarily
- [ ] Verify loading states clear on error

### Success Messages
- [ ] Verify success messages appear for all operations
- [ ] Verify success messages auto-dismiss after 3 seconds
- [ ] Verify success messages don't stack

---

## Browser Console Tests

### Throughout All Tests
- [ ] Monitor browser console for errors
- [ ] Verify no TypeScript compilation errors
- [ ] Verify no Angular template errors
- [ ] Verify no API 404/500 errors
- [ ] Verify LoggerService is used (no console.log statements)

---

## Performance Tests

### Component Load Time
- [ ] Measure time to load Status Master (should be < 1s)
- [ ] Measure time to load Priority Master (should be < 1s)
- [ ] Measure time to load Category (should be < 1s)
- [ ] Measure time to load Branch (should be < 1s)
- [ ] Measure time to load Department (should be < 2s with branch loading)
- [ ] Measure time to load Section (should be < 2s with hierarchical loading)

### Search Performance
- [ ] Test search with 100+ items (if available)
- [ ] Verify search is responsive and doesn't freeze UI

---

## Edge Cases

### Empty States
- [ ] Test Branch management with 0 branches
- [ ] Test Department management with branch having 0 departments
- [ ] Test Section management with department having 0 sections
- [ ] Verify appropriate empty state messages

### Special Characters
- [ ] Test name with special characters: "Test & Co."
- [ ] Test description with quotes: 'Test "quoted" text'
- [ ] Test code with underscores: "TEST_CODE_123"
- [ ] Verify special characters are handled correctly

### Long Content
- [ ] Test very long name (>100 characters) → should truncate or show properly
- [ ] Test very long description (>500 characters)
- [ ] Verify UI doesn't break with long content

### Rapid Operations
- [ ] Create multiple items rapidly
- [ ] Edit and save rapidly
- [ ] Delete multiple items in quick succession
- [ ] Verify no race conditions or data corruption

---

## Regression Tests

### After Each Change
- [ ] Re-run all CRUD operations for changed component
- [ ] Verify no impact on other components
- [ ] Check browser console for new errors

---

## Final Verification

- [ ] All 6 components working correctly
- [ ] No console errors
- [ ] All API calls successful
- [ ] All validations working
- [ ] All permissions enforced
- [ ] All success messages appearing
- [ ] All error messages appropriate
- [ ] All hierarchical loading working
- [ ] All filters and search working
- [ ] All modal behaviors correct

---

## Test Results Summary

| Component | Create | Read | Update | Delete | Search | Filter | Notes |
|-----------|--------|------|--------|--------|--------|--------|-------|
| Status Master | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Priority Master | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Category | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Branch | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Department | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | |
| Section | ☐ | ☐ | ☐ | ☐ | ☐ | ☐ | |

---

## Issues Found During Testing

### Issue Template
```
Component: [Component Name]
Operation: [Create/Read/Update/Delete]
Issue: [Description]
Steps to Reproduce:
1.
2.
3.
Expected: [Expected behavior]
Actual: [Actual behavior]
Priority: [High/Medium/Low]
Status: [Open/In Progress/Resolved]
```

---

## Testing Sign-off

- [ ] All critical tests passed
- [ ] All bugs documented
- [ ] Ready for production deployment

Tested by: _______________
Date: _______________
Signature: _______________
