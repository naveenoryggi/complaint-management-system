# COMPREHENSIVE TEST PLAN - COMPLAINT MANAGEMENT SYSTEM
**Test Date:** 2025-10-12
**Tester:** Claude AI
**Environment:** Development
**Frontend:** Angular 18 (http://localhost:4200)
**Backend:** ASP.NET Core (.NET 8) (https://localhost:7277, http://localhost:5058)
**Database:** SQL Server LocalDB

---

## TEST EXECUTION SUMMARY

| Module | Total Tests | Passed | Failed | Status |
|--------|-------------|--------|--------|--------|
| Authentication & Authorization | 0 | 0 | 0 | PENDING |
| Complaint Management (CRUD) | 0 | 0 | 0 | PENDING |
| Comment System | 0 | 0 | 0 | PENDING |
| Complaint Workflow & Status | 0 | 0 | 0 | PENDING |
| Category Management | 0 | 0 | 0 | PENDING |
| Company Settings & Logo Upload | 0 | 0 | 0 | PENDING |
| User Permissions & Roles | 0 | 0 | 0 | PENDING |
| Dashboard & Statistics | 0 | 0 | 0 | PENDING |
| Escalation Management | 0 | 0 | 0 | PENDING |
| **TOTAL** | **0** | **0** | **0** | **PENDING** |

---

## MODULE 1: AUTHENTICATION & AUTHORIZATION

### Test Cases:

#### TC-AUTH-001: User Login - Valid Credentials
**Priority:** CRITICAL
**Preconditions:**
- Database seeded with admin user (email: admin@test.com, password: Admin@123)
- User is on login page

**Test Steps:**
1. Navigate to http://localhost:4200
2. Enter email: admin@test.com
3. Enter password: Admin@123
4. Click "Login" button

**Expected Result:**
- User is authenticated
- JWT token is stored in localStorage
- User is redirected to /dashboard
- User object contains: id, email, firstName, lastName, roles[], permissions[], companyId

**Actual Result:**
**Status:** PENDING

---

#### TC-AUTH-002: User Login - Invalid Credentials
**Priority:** HIGH
**Test Steps:**
1. Navigate to http://localhost:4200/login
2. Enter email: admin@test.com
3. Enter password: WrongPassword123
4. Click "Login" button

**Expected Result:**
- Error message displayed: "Invalid credentials"
- User remains on login page
- No token stored

**Actual Result:**
**Status:** PENDING

---

#### TC-AUTH-003: User Login - Empty Fields
**Priority:** MEDIUM
**Test Steps:**
1. Navigate to http://localhost:4200/login
2. Leave email and password empty
3. Click "Login" button

**Expected Result:**
- Validation errors displayed
- Login button may be disabled
- No API call made

**Actual Result:**
**Status:** PENDING

---

#### TC-AUTH-004: Authentication Guard - Unauthenticated Access
**Priority:** CRITICAL
**Test Steps:**
1. Clear localStorage (remove token)
2. Navigate directly to http://localhost:4200/dashboard

**Expected Result:**
- User is redirected to /login
- Dashboard content not displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-AUTH-005: Token Persistence
**Priority:** HIGH
**Test Steps:**
1. Login successfully
2. Refresh the page
3. Check user authentication status

**Expected Result:**
- User remains logged in
- Token persists in localStorage
- User stays on current page

**Actual Result:**
**Status:** PENDING

---

#### TC-AUTH-006: Logout Functionality
**Priority:** HIGH
**Test Steps:**
1. Login successfully
2. Click logout button
3. Verify redirection

**Expected Result:**
- Token removed from localStorage
- User redirected to /login
- Cannot access protected routes

**Actual Result:**
**Status:** PENDING

---

## MODULE 2: COMPLAINT MANAGEMENT (CRUD)

### Test Cases:

#### TC-COMP-001: Create Complaint - Valid Data
**Priority:** CRITICAL
**Preconditions:** User is logged in

**Test Steps:**
1. Navigate to /complaints/new
2. Fill form:
   - Title: "Test Complaint - Network Issue"
   - Description: "Unable to connect to company network from remote location. Need urgent assistance."
   - Category: Select any available category
   - Priority: Normal
3. Click "Submit"

**Expected Result:**
- Complaint created with status "Submitted"
- Unique complaint number generated (format: CMP-YYYYMMDD-XXXXX)
- User redirected to /complaints/{id}
- Success message displayed
- Complaint visible in dashboard

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-002: Create Complaint - Invalid Data (Short Title)
**Priority:** HIGH
**Test Steps:**
1. Navigate to /complaints/new
2. Enter title: "Test" (less than 5 characters)
3. Enter valid description (20+ characters)
4. Click "Submit"

**Expected Result:**
- Validation error: "Title must be at least 5 characters"
- Form not submitted
- User remains on form

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-003: Create Complaint - Invalid Data (Short Description)
**Priority:** HIGH
**Test Steps:**
1. Navigate to /complaints/new
2. Enter valid title (5+ characters)
3. Enter description: "Short" (less than 20 characters)
4. Click "Submit"

**Expected Result:**
- Validation error: "Description must be at least 20 characters"
- Form not submitted

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-004: Create Complaint - Required Fields
**Priority:** HIGH
**Test Steps:**
1. Navigate to /complaints/new
2. Leave all fields empty
3. Click "Submit"

**Expected Result:**
- Validation errors for: Title, Description, Category
- Form not submitted
- Error messages displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-005: Create Complaint - Anonymous Complaint
**Priority:** MEDIUM
**Test Steps:**
1. Navigate to /complaints/new
2. Fill valid data
3. Check "Submit Anonymously" checkbox
4. Submit form

**Expected Result:**
- Complaint created with isAnonymous = true
- Complainant name hidden in some views
- Complaint tracks user internally but displays as anonymous

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-006: View Complaint Details
**Priority:** CRITICAL
**Test Steps:**
1. Create a complaint (TC-COMP-001)
2. Navigate to complaint detail page
3. Verify all fields displayed

**Expected Result:**
- Complaint number displayed
- Title, description, category shown
- Status badge displayed correctly
- Priority badge displayed
- Timestamps (submitted, updated) shown
- Complainant information visible (unless anonymous)

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-007: List Complaints - Pagination
**Priority:** HIGH
**Test Steps:**
1. Navigate to /dashboard
2. Verify complaint list
3. Test pagination controls

**Expected Result:**
- Complaints displayed in paginated list (10 per page)
- Pagination controls visible if > 10 complaints
- Page navigation works correctly
- Total count displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-008: Filter Complaints - By Status
**Priority:** HIGH
**Test Steps:**
1. Navigate to /dashboard
2. Select status filter: "Submitted"
3. Verify results

**Expected Result:**
- Only complaints with status "Submitted" displayed
- Count updated accordingly
- Filter can be cleared

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-009: Filter Complaints - By Priority
**Priority:** HIGH
**Test Steps:**
1. Navigate to /dashboard
2. Select priority filter: "High"
3. Verify results

**Expected Result:**
- Only complaints with priority "High" displayed
- Filter persists across pagination
- Multiple filters can be combined

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-010: Search Complaints
**Priority:** MEDIUM
**Test Steps:**
1. Navigate to /dashboard
2. Enter search term in search box
3. Verify results

**Expected Result:**
- Complaints matching search term displayed
- Search works on title and description
- Results update as user types (debounced)

**Actual Result:**
**Status:** PENDING

---

## MODULE 3: COMMENT SYSTEM

### Test Cases:

#### TC-COMM-001: Add Comment - Public Comment
**Priority:** HIGH
**Preconditions:** User logged in, viewing complaint detail

**Test Steps:**
1. Navigate to complaint detail page
2. Enter comment text: "This issue has been reviewed"
3. Ensure "Internal Comment" is unchecked
4. Click "Add Comment"

**Expected Result:**
- Comment added successfully
- Comment visible in comment list
- Timestamp displayed
- User name displayed
- Comment type: Public

**Actual Result:**
**Status:** PENDING

---

#### TC-COMM-002: Add Comment - Internal Comment
**Priority:** HIGH
**Preconditions:** User with appropriate role (Admin/Manager/Agent)

**Test Steps:**
1. Navigate to complaint detail page
2. Enter comment text
3. Check "Internal Comment" checkbox
4. Click "Add Comment"

**Expected Result:**
- Internal comment added
- Badge shows "Internal"
- Only visible to staff members
- Not visible to regular users

**Actual Result:**
**Status:** PENDING

---

#### TC-COMM-003: Add Comment - Empty Comment
**Priority:** MEDIUM
**Test Steps:**
1. Navigate to complaint detail page
2. Leave comment field empty
3. Click "Add Comment"

**Expected Result:**
- Validation error or button disabled
- Comment not submitted
- Error message displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-COMM-004: View Comments - Chronological Order
**Priority:** MEDIUM
**Test Steps:**
1. Add multiple comments to a complaint
2. View comment list

**Expected Result:**
- Comments displayed in chronological order
- Most recent comment at top or bottom (consistent)
- All comments with timestamps

**Actual Result:**
**Status:** PENDING

---

#### TC-COMM-005: Internal Comments - Visibility Control
**Priority:** HIGH
**Preconditions:** Internal comments exist on complaint

**Test Steps:**
1. Login as regular user
2. View complaint with internal comments
3. Verify internal comments not visible

**Expected Result:**
- Internal comments hidden from regular users
- Only public comments visible
- No indicator of internal comments

**Actual Result:**
**Status:** PENDING

---

## MODULE 4: COMPLAINT WORKFLOW & STATUS

### Test Cases:

#### TC-WORK-001: Assign Complaint
**Priority:** CRITICAL
**Preconditions:** User has "complaints.assign" permission

**Test Steps:**
1. Navigate to complaint detail
2. Click "Assign" button
3. Select user from dropdown
4. Confirm assignment

**Expected Result:**
- Complaint assigned to selected user
- Assigned user name displayed
- Status may change to "Under Review" or "In Progress"
- Notification sent to assigned user (if configured)

**Actual Result:**
**Status:** PENDING

---

#### TC-WORK-002: Escalate Complaint
**Priority:** HIGH
**Preconditions:** User has "complaints.escalate" permission

**Test Steps:**
1. Navigate to complaint detail
2. Click "Escalate" button
3. Enter escalation reason
4. Confirm escalation

**Expected Result:**
- Complaint status changed to "Escalated"
- Escalation level incremented
- Escalation reason recorded
- Alert/notification triggered

**Actual Result:**
**Status:** PENDING

---

#### TC-WORK-003: Close Complaint
**Priority:** HIGH
**Preconditions:** User has "complaints.close" permission

**Test Steps:**
1. Navigate to complaint detail
2. Click "Close/Resolve" button
3. Enter resolution notes
4. Confirm closure

**Expected Result:**
- Complaint status changed to "Resolved" or "Closed"
- Resolution notes saved
- Closure timestamp recorded
- Cannot reopen without permission

**Actual Result:**
**Status:** PENDING

---

#### TC-WORK-004: Status Transitions - Valid Flow
**Priority:** HIGH
**Test Steps:**
1. Create complaint (status: Submitted)
2. Assign complaint (status: UnderReview/InProgress)
3. Escalate if needed (status: Escalated)
4. Resolve complaint (status: Resolved)
5. Close complaint (status: Closed)

**Expected Result:**
- All status transitions valid
- Audit trail maintained
- Timestamps recorded for each transition

**Actual Result:**
**Status:** PENDING

---

#### TC-WORK-005: Permission-Based Actions
**Priority:** CRITICAL
**Preconditions:** Login with different role levels

**Test Steps:**
1. Test actions as regular user
2. Test actions as Agent
3. Test actions as Manager
4. Test actions as Admin

**Expected Result:**
- Actions restricted based on permissions
- Buttons hidden/disabled if no permission
- API returns 403 Forbidden if unauthorized
- Clear permission error messages

**Actual Result:**
**Status:** PENDING

---

## MODULE 5: CATEGORY MANAGEMENT

### Test Cases:

#### TC-CAT-001: Load Categories
**Priority:** HIGH
**Test Steps:**
1. Navigate to complaint form
2. Check category dropdown

**Expected Result:**
- Categories loaded from API
- All active categories displayed
- Categories sorted by display order
- Dropdown functional

**Actual Result:**
**Status:** PENDING

---

#### TC-CAT-002: Select Category
**Priority:** HIGH
**Test Steps:**
1. Open category dropdown
2. Select a category
3. Submit complaint

**Expected Result:**
- Selected category assigned to complaint
- Category name displayed in complaint detail
- Category affects SLA and routing

**Actual Result:**
**Status:** PENDING

---

#### TC-CAT-003: Category in Complaint List
**Priority:** MEDIUM
**Test Steps:**
1. View dashboard complaint list
2. Check category display

**Expected Result:**
- Category name shown for each complaint
- Category badge/label styled appropriately

**Actual Result:**
**Status:** PENDING

---

## MODULE 6: COMPANY SETTINGS & LOGO UPLOAD

### Test Cases:

#### TC-COMP-SET-001: Navigate to Company Settings
**Priority:** HIGH
**Preconditions:** User logged in

**Test Steps:**
1. Navigate to /admin/company-settings
2. Verify page loads

**Expected Result:**
- Company settings page loads
- Form displays with current company data
- Logo section visible
- All fields editable

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-002: Load Company Data
**Priority:** HIGH
**Test Steps:**
1. Navigate to /admin/company-settings
2. Verify form populated

**Expected Result:**
- Company name loaded
- Description loaded
- Contact email loaded
- Contact phone loaded
- Address loaded
- Current logo displayed if exists

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-003: Update Company Details
**Priority:** HIGH
**Test Steps:**
1. Navigate to /admin/company-settings
2. Modify company name: "Updated Company Name"
3. Modify description
4. Click "Save Changes"

**Expected Result:**
- Success message: "Company details updated successfully!"
- Changes saved to database
- Form updates with new values
- Message auto-disappears after 3 seconds

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-004: Upload Logo - Valid Image (PNG)
**Priority:** CRITICAL
**Test Steps:**
1. Navigate to company settings
2. Click "Choose File"
3. Select valid PNG image (< 5MB)
4. Click "Upload Logo"

**Expected Result:**
- File selected and preview shown
- Upload successful
- Logo displayed in preview
- Logo saved to wwwroot/uploads/logos/
- Logo URL returned from API
- Success message displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-005: Upload Logo - Valid Image (JPG)
**Priority:** HIGH
**Test Steps:**
1. Select JPG image (< 5MB)
2. Upload logo

**Expected Result:**
- Upload successful
- Logo displayed correctly

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-006: Upload Logo - Invalid File Type
**Priority:** HIGH
**Test Steps:**
1. Select PDF or TXT file
2. Attempt upload

**Expected Result:**
- Error message: "Invalid file type. Please upload a JPG, PNG, GIF, SVG, or WEBP image."
- Upload prevented
- No API call made

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-007: Upload Logo - File Too Large
**Priority:** HIGH
**Test Steps:**
1. Select image file > 5MB
2. Attempt upload

**Expected Result:**
- Error message: "File size exceeds 5MB limit."
- Upload prevented
- File selection cleared

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-008: Delete Logo
**Priority:** HIGH
**Preconditions:** Company has logo uploaded

**Test Steps:**
1. Navigate to company settings
2. Click "Delete Logo"
3. Confirm deletion in confirmation dialog

**Expected Result:**
- Confirmation dialog: "Are you sure you want to delete the company logo?"
- Logo deleted from server
- Logo removed from database
- Preview shows placeholder
- Success message displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-009: Logo Preview
**Priority:** MEDIUM
**Test Steps:**
1. Select logo file
2. Verify preview before upload

**Expected Result:**
- Selected image previewed immediately
- Preview shows actual image
- Preview maintains aspect ratio
- Preview fits container (200x200px)

**Actual Result:**
**Status:** PENDING

---

#### TC-COMP-SET-010: Cancel Logo Selection
**Priority:** LOW
**Test Steps:**
1. Select a logo file
2. Click "Cancel" before uploading

**Expected Result:**
- Selection cancelled
- Preview reverts to previous logo or placeholder
- No file uploaded

**Actual Result:**
**Status:** PENDING

---

## MODULE 7: USER PERMISSIONS & ROLES

### Test Cases:

#### TC-PERM-001: Load User Permissions
**Priority:** CRITICAL
**Test Steps:**
1. Login as admin
2. Check currentUser object

**Expected Result:**
- User object contains permissions array
- Permissions match assigned role
- Common permissions: complaints.view, complaints.create

**Actual Result:**
**Status:** PENDING

---

#### TC-PERM-002: Role-Based UI Elements
**Priority:** HIGH
**Test Steps:**
1. Login as different roles
2. Check visible UI elements

**Expected Result:**
- Admin: All buttons visible
- Manager: Assign, escalate visible
- Agent: View, comment visible
- User: Create, view own complaints

**Actual Result:**
**Status:** PENDING

---

#### TC-PERM-003: API Authorization
**Priority:** CRITICAL
**Test Steps:**
1. Attempt API call without permission
2. Check response

**Expected Result:**
- 403 Forbidden returned
- Error message clear
- Action not performed

**Actual Result:**
**Status:** PENDING

---

## MODULE 8: DASHBOARD & STATISTICS

### Test Cases:

#### TC-DASH-001: Load Dashboard
**Priority:** CRITICAL
**Test Steps:**
1. Login successfully
2. Verify dashboard loads

**Expected Result:**
- Dashboard displays
- Complaint list visible
- Statistics cards shown
- All data loads without errors

**Actual Result:**
**Status:** PENDING

---

#### TC-DASH-002: Statistics - Total Count
**Priority:** HIGH
**Test Steps:**
1. View dashboard
2. Check "Total" statistic

**Expected Result:**
- Correct total count displayed
- Matches actual complaint count
- Updates when new complaint created

**Actual Result:**
**Status:** PENDING

---

#### TC-DASH-003: Statistics - By Status
**Priority:** HIGH
**Test Steps:**
1. View dashboard statistics
2. Verify counts for: Submitted, In Progress, Resolved

**Expected Result:**
- Each status has accurate count
- Counts sum to total
- Updates in real-time

**Actual Result:**
**Status:** PENDING

---

#### TC-DASH-004: Recent Complaints List
**Priority:** HIGH
**Test Steps:**
1. View dashboard
2. Check complaint list

**Expected Result:**
- Recent complaints displayed (10 per page)
- Sorted by submission date (newest first)
- Status badges color-coded
- Priority badges displayed

**Actual Result:**
**Status:** PENDING

---

#### TC-DASH-005: Quick Actions
**Priority:** MEDIUM
**Test Steps:**
1. Click "Create Complaint" button
2. Click on complaint to view details

**Expected Result:**
- "Create Complaint" navigates to /complaints/new
- Clicking complaint navigates to /complaints/{id}
- Navigation smooth and quick

**Actual Result:**
**Status:** PENDING

---

## MODULE 9: ESCALATION MANAGEMENT

### Test Cases:

#### TC-ESC-001: Auto-Escalation Worker Running
**Priority:** HIGH
**Test Steps:**
1. Check backend logs
2. Verify worker status

**Expected Result:**
- Auto-Escalation Worker started
- Runs every 15 minutes
- Processes overdue complaints
- Logs activity

**Actual Result:**
**Status:** PENDING

---

#### TC-ESC-002: Escalation Matrix Configuration
**Priority:** MEDIUM
**Preconditions:** Access to escalation policy page

**Test Steps:**
1. Navigate to escalation matrix page
2. Verify matrix list

**Expected Result:**
- Escalation matrices listed
- Can view levels
- Can test escalation logic

**Actual Result:**
**Status:** PENDING

---

## ADDITIONAL TEST SCENARIOS

### Security Tests

#### TC-SEC-001: XSS Protection
**Priority:** HIGH
**Test Steps:**
1. Enter script tags in complaint title/description
2. Submit and view

**Expected Result:**
- Script tags escaped/sanitized
- No script execution
- Content displayed safely

**Actual Result:**
**Status:** PENDING

---

#### TC-SEC-002: SQL Injection Protection
**Priority:** CRITICAL
**Test Steps:**
1. Enter SQL injection attempt in search
2. Submit query

**Expected Result:**
- Query parameterized
- No SQL error
- No unauthorized data access

**Actual Result:**
**Status:** PENDING

---

#### TC-SEC-003: JWT Token Validation
**Priority:** CRITICAL
**Test Steps:**
1. Modify JWT token
2. Make API request

**Expected Result:**
- 401 Unauthorized returned
- Invalid token rejected
- No access granted

**Actual Result:**
**Status:** PENDING

---

### Performance Tests

#### TC-PERF-001: Page Load Time
**Priority:** MEDIUM
**Test Steps:**
1. Measure dashboard load time
2. Measure complaint detail load time

**Expected Result:**
- Dashboard loads < 2 seconds
- Detail page loads < 1 second
- API responses < 500ms

**Actual Result:**
**Status:** PENDING

---

#### TC-PERF-002: Pagination Performance
**Priority:** MEDIUM
**Test Steps:**
1. Navigate through multiple pages
2. Measure response time

**Expected Result:**
- Consistent load times
- No performance degradation
- Smooth pagination

**Actual Result:**
**Status:** PENDING

---

### Usability Tests

#### TC-USE-001: Error Messages
**Priority:** MEDIUM
**Test Steps:**
1. Trigger various errors
2. Check error messages

**Expected Result:**
- Error messages clear and helpful
- Suggest corrective action
- Displayed in visible location

**Actual Result:**
**Status:** PENDING

---

#### TC-USE-002: Success Messages
**Priority:** LOW
**Test Steps:**
1. Perform successful actions
2. Verify feedback

**Expected Result:**
- Success messages displayed
- Auto-dismiss after 3 seconds
- Positive, confirming language

**Actual Result:**
**Status:** PENDING

---

#### TC-USE-003: Loading States
**Priority:** LOW
**Test Steps:**
1. Trigger async operations
2. Check loading indicators

**Expected Result:**
- Spinner/loader displayed during API calls
- Buttons disabled while processing
- Clear visual feedback

**Actual Result:**
**Status:** PENDING

---

## TEST ENVIRONMENT

**Software Versions:**
- Angular: 18.x
- ASP.NET Core: .NET 8
- TypeScript: 5.x
- SQL Server: LocalDB

**Browser:**
- Chrome (Latest)

**Test Data:**
- Seeded admin user: admin@test.com / Admin@123
- Seeded categories: 11 categories
- Seeded roles: Admin, Manager, Agent, User

---

## TEST EXECUTION NOTES

*Test execution notes will be added here as tests are performed.*

---

## ISSUES LOG

| Issue ID | Severity | Module | Description | Status |
|----------|----------|--------|-------------|--------|
| - | - | - | - | - |

---

## TEST SIGN-OFF

**Test Lead:** _______________
**Date:** _______________
**Status:** PENDING
**Recommendation:** PENDING

---

*End of Test Plan*
