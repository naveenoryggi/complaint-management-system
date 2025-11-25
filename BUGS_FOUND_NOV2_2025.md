# Bug Report - November 2, 2025
## Complaint Management System E2E Testing

---

## BUG-001: Complaint Creation Fails with 400 Error

**Severity:** CRITICAL
**Priority:** HIGH
**Status:** Open
**Found By:** AI QA Automation Engineer
**Date Found:** November 2, 2025

### Description
Users cannot create new complaints. When submitting the complaint creation form, the API returns a 400 Bad Request error.

### Impact
- **User Impact:** HIGH - Users cannot submit complaints (core functionality)
- **Business Impact:** CRITICAL - Primary workflow is broken
- **Workaround:** None available

### Environment
- Frontend: http://localhost:4200
- Backend: http://localhost:5058
- Browser: Chromium (Playwright)

### Steps to Reproduce
1. Login with credentials: admin@complaintmanagement.com / Admin@123
2. Navigate to Dashboard
3. Click "Create New Complaint" button
4. Fill in the form:
   - Title: "E2E Test - Network Connectivity Issue"
   - Description: "This is a comprehensive end-to-end test complaint. The network connectivity in the office has been intermittent for the past 3 days, affecting productivity and causing disruptions to daily operations."
   - Category: Select "Technical Issues"
   - Priority: Select "High"
5. Click "Submit Complaint" button
6. Observe error message

### Expected Result
- Complaint should be created successfully
- User should be redirected to complaint detail page or complaints list
- Success message should be displayed
- New complaint should appear in the complaints list

### Actual Result
- Error banner appears: "Error - Failed to create complaint"
- HTTP 400 Bad Request error logged in console
- User remains on the create complaint page
- No complaint is created

### Technical Details
- **API Endpoint:** POST /api/complaints
- **HTTP Status:** 400 Bad Request
- **Error Message:** "Failed to create complaint"
- **Console Error:** `HttpErrorResponse @ http://localhost:4200/chunk-BE3XIRW5.js:616`

### Evidence
- Screenshot: `07-complaint-submission-error.png`
- Location: `C:\Users\Navin Chandra\Pictures\Complaint management system\.playwright-mcp\`

### Possible Root Causes
1. **Payload Mismatch:** Frontend sending data in format backend doesn't expect
2. **Priority Field Issue:** Sending priority label ("High") instead of priority ID
3. **Category Field Issue:** Sending category name instead of category ID
4. **Missing Required Field:** Backend requires a field frontend isn't sending
5. **Data Type Mismatch:** Field types don't match between frontend and backend
6. **Authentication Issue:** Token not being sent or invalid

### Recommended Fix
1. **Debug API Request:**
   - Log the exact payload being sent from frontend
   - Compare with backend DTO/model expectations
   - Check if priority and category are sending IDs vs labels

2. **Backend Error Messages:**
   - Return specific validation errors (which field failed, why)
   - Use 400 with detailed error response body
   - Add proper error handling in API

3. **Frontend Improvements:**
   - Add request logging in development mode
   - Display specific validation errors from backend
   - Add loading indicator during submission

### Testing Notes
- All other form fields populated correctly
- Form validation passed on frontend
- Character counters working (37/200 for title, 200/2000 for description)
- Dropdowns loaded correctly (23 categories, 8 priorities)
- User authentication working (logged in as admin)

### Related Components
- Frontend: `complaint-system-angular/src/app/components/complaints/complaint-form/complaint-form.component.ts`
- Backend: `complaint-system-dotnet/src/ComplaintManagement.API/Controllers/ComplaintsController.cs`
- Service: `complaint-system-angular/src/app/services/complaint.service.ts`

---

## BUG-002: Dashboard Customization Modal Animation Error

**Severity:** LOW
**Priority:** LOW
**Status:** Open
**Found By:** AI QA Automation Engineer
**Date Found:** November 2, 2025

### Description
When opening the Dashboard Customization modal, a console error appears related to the @slideIn animation property.

### Impact
- **User Impact:** NONE - Modal still functions correctly
- **Business Impact:** LOW - Cosmetic issue only
- **Workaround:** Ignore console error

### Environment
- Frontend: http://localhost:4200
- Browser: Chromium (Playwright)

### Steps to Reproduce
1. Login to the application
2. Navigate to Dashboard
3. Click "Customize Dashboard" button
4. Open browser console
5. Observe error message

### Expected Result
- Modal should open with smooth animation
- No console errors

### Actual Result
- Modal opens and functions correctly
- Console error appears: "Unexpected synthetic property @slideIn found"

### Technical Details
- **Error Type:** RuntimeError NG05105
- **Full Error Message:**
  ```
  ERROR RuntimeError: NG05105: Unexpected synthetic property @slideIn found.
  Please make sure that:
  - Make sure `provideAnimationsAsync()`, `provideAnimations()` or `provideNoopAnimations()`
    call was added to a list of providers used to bootstrap an application.
  - There is a corresponding animation configuration named `@slideIn` defined in the
    `animations` field of the `@Component` decorator
  ```

### Evidence
- Screenshot: `04-dashboard-customize-modal.png`
- Console shows error but modal displays correctly

### Recommended Fix
**Option 1: Add Animation Provider (Recommended)**
```typescript
// In app.config.ts
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

export const appConfig: ApplicationConfig = {
  providers: [
    provideAnimationsAsync(),
    // ... other providers
  ]
};
```

**Option 2: Remove Animation**
- Remove the `@slideIn` animation binding from the modal component
- Use CSS transitions instead

**Option 3: Define Animation**
- Add the `@slideIn` animation definition to the component's `@Component` decorator

### Testing Notes
- Modal functionality not affected
- All customization options work
- Cancel and Save buttons function correctly

### Related Components
- Component: `complaint-system-angular/src/app/components/dashboard/dashboard-customizer/*`
- Config: `complaint-system-angular/src/app/app.config.ts`

---

## Data Quality Issues

### ISSUE-001: Test Data in Production Dropdowns

**Severity:** MEDIUM
**Priority:** MEDIUM
**Type:** Data Quality

### Affected Dropdowns

**Priority Dropdown:**
- "Test Priority" - Should be removed
- "Invalid Priority" - Should be removed
- "Dynamic Test Priority" - Should be removed

**Status (Dashboard Widgets):**
- "Test Status" - Should be removed
- "Duplicate Status" - Should be removed

**Category Dropdown:**
- "Test Cat" - Should be removed
- "Duplicate Test" - Should be removed
- **"Test<script>alert('xss')</script>"** - SECURITY CONCERN - Should be removed immediately

### Recommendations
1. **Immediate:** Remove XSS test payload from categories
2. **Before Production:** Clean all test data from master tables
3. **Add Validation:** Prevent HTML/script tags in category names
4. **Database Script:** Create cleanup script to remove test data

### SQL Cleanup Queries Needed
```sql
-- Remove test priorities
DELETE FROM ComplaintPriorityMaster WHERE Name LIKE '%Test%' OR Name = 'Invalid Priority';

-- Remove test statuses
DELETE FROM ComplaintStatusMaster WHERE Name LIKE '%Test%' OR Name LIKE '%Duplicate%';

-- Remove test categories (including XSS payloads)
DELETE FROM Categories WHERE Name LIKE '%Test%' OR Name LIKE '%<script%';
```

---

## Summary

**Total Bugs Found:** 2
**Critical:** 1
**Minor:** 1
**Data Quality Issues:** Multiple

**Blockers for Production:**
1. BUG-001 (Complaint Creation) - MUST FIX
2. Test data cleanup - MUST DO
3. XSS payload in categories - SECURITY RISK

**Non-Blockers:**
- BUG-002 (Animation error) - Can be fixed later

---

**Report Generated:** November 2, 2025
**Full Test Report:** E2E_TEST_REPORT_NOV2_2025.md
**Screenshots Location:** C:\Users\Navin Chandra\Pictures\Complaint management system\.playwright-mcp\
