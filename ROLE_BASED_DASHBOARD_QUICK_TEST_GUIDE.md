# Role-Based Dashboard - Quick Verification Test Guide

## Quick Test Steps (5 Minutes)

### Prerequisites
- Backend API running on http://localhost:5000
- Frontend Angular app running on http://localhost:4200
- Browser with Developer Tools (F12)

---

## Test 1: Complainant View (2 minutes)

### Login Credentials:
- **Email:** nav_nainital@yahoo.com
- **Password:** ComplaintSystem@123

### Expected Results:
1. **Role Indicator Badge:**
   ```
   View: Complainant (My Complaints)
   ```

2. **Dashboard Statistics:**
   - Total Complaints: **10** (only test complaints created by this user)
   - Submitted: Count of this user's submitted complaints
   - In Progress: Count of this user's in-progress complaints
   - Resolved: Count of this user's resolved complaints

3. **Complaint List:**
   - Shows ONLY complaints where this user is the complainant
   - Should see 10 test complaints in the list

4. **Browser Console (F12):**
   ```
   User is complainant - filtering by complainantId: <guid>
   Complaints loaded in parallel with role-based filtering
   ```

5. **Network Tab (F12) → Filter: "complaints":**
   ```
   GET /api/complaints?page=1&pageSize=10&complainantId=<guid>
   ```

✅ **PASS CRITERIA:** User sees only 10 test complaints, not all 1,093+ complaints in system

---

## Test 2: Admin View (2 minutes)

### Login Credentials:
- **Email:** admin@complaintmanagement.com
- **Password:** Admin@123

### Expected Results:
1. **Role Indicator Badge:**
   ```
   View: Administrator (All Complaints)
   ```

2. **Dashboard Statistics:**
   - Total Complaints: **1,093+** (all complaints in system)
   - Submitted: Count of all submitted complaints system-wide
   - In Progress: Count of all in-progress complaints system-wide
   - Resolved: Count of all resolved complaints system-wide

3. **Complaint List:**
   - Shows ALL complaints regardless of complainant or assignment
   - Pagination shows system-wide total count

4. **Browser Console (F12):**
   ```
   User is admin - showing all complaints (no role-based filtering)
   Complaints loaded in parallel
   ```

5. **Network Tab (F12) → Filter: "complaints":**
   ```
   GET /api/complaints?page=1&pageSize=10
   ```
   **Notice:** NO complainantId or assignedToId parameter

✅ **PASS CRITERIA:** User sees ALL complaints (1,093+), significantly more than complainant view

---

## Test 3: Handler View (1 minute)

### Login Credentials:
- **Email:** naveen.chandra@oryggitech.com
- **Password:** ComplaintSystem@123
- **NOTE:** This test only works if this user has Handler/Technician role assigned

### Expected Results:
1. **Role Indicator Badge:**
   ```
   View: Handler (Assigned Complaints)
   ```
   OR if not a handler role:
   ```
   View: Complainant (My Complaints)
   ```

2. **Dashboard Statistics:**
   - Total Complaints: Count of complaints assigned TO this user
   - Statistics reflect only assigned complaints

3. **Browser Console (F12):**
   ```
   User is handler - filtering by assignedToId: <guid>
   ```
   OR:
   ```
   User is complainant - filtering by complainantId: <guid>
   ```

4. **Network Tab (F12):**
   ```
   GET /api/complaints?page=1&pageSize=10&assignedToId=<guid>
   ```

✅ **PASS CRITERIA:** User sees only complaints assigned to them, or own complaints if not a handler

---

## Visual Verification Checklist

### Dashboard Header Area
- [ ] Role indicator badge displays prominently below welcome subtitle
- [ ] Badge has gradient blue background with eye icon
- [ ] Badge text clearly states current view type
- [ ] Badge is visually distinct and easy to locate

### Statistics Cards
- [ ] Numbers match expected counts for user role
- [ ] Statistics update when filters change
- [ ] All four stat cards show role-filtered counts

### Complaint List
- [ ] Shows correct complaints for user role
- [ ] Pagination total count is role-specific
- [ ] No unauthorized complaints appear

### Console Logs
- [ ] Role detection message appears on page load
- [ ] Filtering message confirms role-based logic applied
- [ ] No error messages related to filtering

### Network Requests
- [ ] API calls include correct filter parameters
- [ ] No requests without proper role filtering (except admin)
- [ ] All 5 API calls complete successfully on dashboard load

---

## Troubleshooting

### Issue: Role indicator shows wrong role
**Solution:**
1. Logout and login again
2. Clear browser cache (Ctrl+Shift+Delete)
3. Check user roles in database

### Issue: Dashboard shows all complaints for non-admin
**Solution:**
1. Check browser console for error messages
2. Verify API is running and accessible
3. Check network tab for failed requests
4. Verify backend changes were deployed

### Issue: Statistics show zero
**Solution:**
1. Check if user has any complaints/assignments
2. Verify database connection
3. Check browser console for errors
4. Try different test user account

### Issue: Role indicator badge not visible
**Solution:**
1. Clear browser cache
2. Hard refresh (Ctrl+Shift+R)
3. Rebuild Angular app: `npm run build`
4. Check for CSS loading errors in console

---

## Success Criteria Summary

| Test | Complainant | Handler | Admin |
|------|-------------|---------|-------|
| **Role Badge** | "Complainant (My Complaints)" | "Handler (Assigned Complaints)" | "Administrator (All Complaints)" |
| **Total Count** | 10 (test complaints) | Assigned count | 1,093+ (all) |
| **API Filter** | complainantId=<guid> | assignedToId=<guid> | No filter |
| **Console Log** | "User is complainant" | "User is handler" | "User is admin" |

---

## Quick Pass/Fail Test

### 30-Second Verification:

1. Login as complainant (nav_nainital@yahoo.com)
2. Check total complaints count: Should be **10**
3. Login as admin (admin@complaintmanagement.com)
4. Check total complaints count: Should be **1,093+**

**If both counts are different and correct → PASS ✅**
**If both show same count → FAIL ❌ (role filtering not working)**

---

## Next Steps After Verification

### If Tests Pass:
1. Document verification results
2. Mark feature as complete
3. Deploy to production
4. Monitor user feedback

### If Tests Fail:
1. Review console errors
2. Check network tab for failed requests
3. Verify backend is running latest code
4. Review ROLE_BASED_DASHBOARD_FIX_COMPLETE_REPORT.md
5. Contact development team

---

**Quick Test Guide Version:** 1.0
**Last Updated:** November 10, 2025
**Estimated Test Time:** 5 minutes
