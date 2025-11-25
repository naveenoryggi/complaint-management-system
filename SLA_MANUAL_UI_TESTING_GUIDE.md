# SLA System - Manual UI Testing Guide

**Date**: November 1, 2025
**Purpose**: Step-by-step manual testing guide for SLA system
**Prerequisites**: Backend running on http://localhost:5058, Frontend on http://localhost:4200

---

## ⚠️ IMPORTANT: Before You Start

### Get Fresh Token with SLA Permissions

1. **Logout** from the application if you're currently logged in
2. **Login** again as admin@complaintmanagement.com / Admin@123
3. This will generate a fresh JWT token that includes the new SLA permissions we added

**Why?** The old token was created before we added SLA permissions to the database. The new login will include these permissions:
- ViewSLA
- ManageSLA
- CreateSLA
- UpdateSLA
- DeleteSLA

---

## Test Phase 1: Access SLA Management Module

### Step 1.1: Navigate to SLA Management
1. Open browser: http://localhost:4200
2. Login with: admin@complaintmanagement.com / Admin@123
3. Look for Admin menu or navigation
4. Find "SLA Management" option
5. Click to enter SLA Management module

**Expected Result**: ✅ SLA Management page loads without errors

**Screenshot**: Take screenshot if page loads successfully

---

## Test Phase 2: Configure Global SLA Settings

### Step 2.1: Access SLA Settings Tab
1. In SLA Management, locate "Settings" or "Global Settings" tab
2. Click to open settings

**Expected Result**: ✅ Settings form loads

### Step 2.2: Configure SLA Settings
Fill in the following values:

| Field | Value | Notes |
|-------|-------|-------|
| Enable SLA | ✅ Yes | Turn on SLA system |
| Working Hours Only | ✅ Yes | Use working hours calculation |
| Working Hours Start | 09:00 | Start of work day |
| Working Hours End | 17:00 | End of work day |
| Working Days | 1,2,3,4,5 | Monday-Friday |
| Auto Escalate on Breach | ✅ Yes | Enable auto-escalation |
| Escalation Threshold | 80% | Escalate at 80% |
| Notify Before Breach | ✅ Yes | Send warnings |
| Notify Before Breach Minutes | 30 | Warning 30 min before |
| Timezone | UTC | Time zone setting |

### Step 2.3: Save Settings
1. Click "Save" or "Update Settings" button
2. Wait for success message

**Expected Result**: ✅ "Settings saved successfully" message appears

**Test Verification**:
```
GET http://localhost:5058/api/sla/settings
Should return the settings you just configured
```

---

## Test Phase 3: Create SLA Levels

### Step 3.1: Navigate to SLA Levels Tab
1. Click "SLA Levels" tab in SLA Management
2. Look for "Add SLA Level" or "Create New" button

### Step 3.2: Create Gold Level
Click "Add SLA Level" and fill:

| Field | Value |
|-------|-------|
| Name | Gold |
| Description | Premium support - fastest response |
| Order | 1 |
| Active | ✅ Yes |
| Color Code | #FFD700 (gold color) |
| Response Time | 1 |
| Response Time Unit | Hours |
| Resolution Time | 4 |
| Resolution Time Unit | Hours |

**Expected Result**: ✅ Gold level created and appears in list

### Step 3.3: Create Silver Level
Repeat with these values:

| Field | Value |
|-------|-------|
| Name | Silver |
| Description | Standard support - normal response |
| Order | 2 |
| Active | ✅ Yes |
| Color Code | #C0C0C0 (silver color) |
| Response Time | 2 |
| Response Time Unit | Hours |
| Resolution Time | 8 |
| Resolution Time Unit | Hours |

**Expected Result**: ✅ Silver level created and appears in list

### Step 3.4: Create Bronze Level
Repeat with these values:

| Field | Value |
|-------|-------|
| Name | Bronze |
| Description | Basic support - standard response |
| Order | 3 |
| Active | ✅ Yes |
| Color Code | #CD7F32 (bronze color) |
| Response Time | 4 |
| Response Time Unit | Hours |
| Resolution Time | 24 |
| Resolution Time Unit | Hours |

**Expected Result**: ✅ Bronze level created and appears in list

### Step 3.5: Verify All Levels
Look at the SLA Levels list

**Expected Result**: ✅ Should see 3 levels: Gold, Silver, Bronze

---

## Test Phase 4: Create Category-SLA Mappings

### Step 4.1: Navigate to Category Mappings Tab
1. Click "Category Mappings" or "Category SLA" tab
2. Look for "Add Mapping" button

### Step 4.2: Map First Category to Gold
Create mapping:

| Field | Value |
|-------|-------|
| Category | Select first category (e.g., "A" or "IT Support") |
| SLA Level | Gold |
| Override Response Time | (leave empty - use level default) |
| Override Resolution Time | (leave empty - use level default) |
| Active | ✅ Yes |

**Expected Result**: ✅ Category mapping created

### Step 4.3: Map Another Category to Silver
Create second mapping:

| Field | Value |
|-------|-------|
| Category | Select another category |
| SLA Level | Silver |
| Active | ✅ Yes |

**Expected Result**: ✅ Second category mapping created

### Step 4.4: View All Mappings
Check the category mappings list

**Expected Result**: ✅ Should see both mappings with effective resolution times displayed

---

## Test Phase 5: Create Priority-SLA Mappings

### Step 5.1: Navigate to Priority Mappings Tab
1. Click "Priority Mappings" or "Priority SLA" tab
2. Look for "Add Mapping" button

### Step 5.2: Map Critical Priority to Gold (with Overrides)
Create mapping:

| Field | Value |
|-------|-------|
| Priority | Critical |
| SLA Level | Gold |
| Override Response Time | 30 (minutes) |
| Override Resolution Time | 120 (minutes = 2 hours) |
| Active | ✅ Yes |

**Expected Result**: ✅ Critical priority mapping created with override times

### Step 5.3: Map High Priority to Silver
Create mapping:

| Field | Value |
|-------|-------|
| Priority | High |
| SLA Level | Silver |
| Active | ✅ Yes |

**Expected Result**: ✅ High priority mapping created (uses Silver defaults)

### Step 5.4: View All Priority Mappings
Check the priority mappings list

**Expected Result**: ✅ Should see:
- Critical → Gold (30 min response, 2 hr resolution)
- High → Silver (2 hr response, 8 hr resolution)

---

## Test Phase 6: Create Test Complaints with SLA Calculation

### Step 6.1: Navigate to Complaint Creation
1. Go to main navigation
2. Click "Complaints" or "New Complaint"
3. Access complaint creation form

### Step 6.2: Create Critical Priority Complaint
Fill complaint form:

| Field | Value |
|-------|-------|
| Title | Critical Server Outage - SLA Test |
| Description | Testing Priority-SLA mapping with Critical priority |
| Category | Select the category mapped to Gold |
| Priority | Critical |
| Source | Web |

**Before Submitting**:
- Note current time: ___________

**After Submitting**:
- Note complaint number: ___________
- Note due date shown: ___________
- Calculate hours difference: ___________

**Expected Result**: ✅
- Complaint created successfully
- Due date should be approximately 2 hours from submission time (override value)
- Complaint number generated (e.g., CMP-2025-XXXX)

### Step 6.3: Create Normal Priority Complaint
Fill complaint form:

| Field | Value |
|-------|-------|
| Title | Standard Request - SLA Test |
| Description | Testing Category-SLA mapping with Normal priority |
| Category | Select the category mapped to Gold |
| Priority | Normal |
| Source | Web |

**Expected Result**: ✅
- Complaint created successfully
- Due date should use Category-SLA mapping (Gold = 4 hours resolution)

### Step 6.4: Create High Priority Complaint
Fill complaint form:

| Field | Value |
|-------|-------|
| Title | High Priority Issue - SLA Test |
| Description | Testing Priority-SLA mapping with High priority |
| Category | Select the category mapped to Silver |
| Priority | High |
| Source | Web |

**Expected Result**: ✅
- Complaint created successfully
- Due date should be approximately 8 hours (Silver level resolution time)

### Step 6.5: Create Unmapped Complaint (Fallback Test)
Fill complaint form:

| Field | Value |
|-------|-------|
| Title | Fallback Test - Unmapped Category |
| Description | Testing fallback to Priority Master |
| Category | Select category NOT mapped to any SLA level |
| Priority | Low |
| Source | Web |

**Expected Result**: ✅
- Complaint created successfully
- Due date calculated using Priority Master fallback

---

## Test Phase 7: Verify SLA Information Display

### Step 7.1: View Complaint List
1. Navigate to Complaints list
2. Look at the complaints you just created

**Check for**:
- ✅ Due dates displayed for each complaint
- ✅ SLA status indicators (green/yellow/red)
- ✅ Time remaining shown
- ✅ Priority badges visible

### Step 7.2: View Complaint Details
1. Click on the Critical priority complaint
2. View detailed complaint page

**Check for**:
- ✅ Complaint number displayed
- ✅ Submitted date/time shown
- ✅ Due date/time shown
- ✅ Priority displayed correctly
- ✅ SLA information section present
- ✅ Time remaining or percentage complete shown

### Step 7.3: Check Dashboard (if available)
1. Navigate to Dashboard
2. Look for SLA metrics or complaint widgets

**Check for**:
- ✅ Complaints shown with SLA status
- ✅ Overdue complaints highlighted
- ✅ SLA compliance indicators

---

## Test Phase 8: Working Hours Calculation Test

### Step 8.1: Test After Hours Submission
**If current time is after 5 PM**:

1. Create a new complaint with 4-hour SLA
2. Submit after 5 PM (e.g., 6:00 PM)
3. Check due date

**Expected Result**: ✅
- Due date should be next working day
- Should NOT include evening/night hours
- Example: Submitted 6 PM Friday → Due Monday (4 working hours)

### Step 8.2: Test Weekend Skipping
**If current time is Friday afternoon**:

1. Create complaint at 3 PM Friday with 8-hour SLA
2. Check due date calculation

**Expected Result**: ✅
- Should skip Saturday and Sunday
- Example:
  - Friday 3 PM - 5 PM = 2 hours
  - Weekend skipped
  - Monday 9 AM - 3 PM = 6 hours
  - Due: Monday 3 PM

---

## Test Phase 9: Edit and Update Tests

### Step 9.1: Edit SLA Level
1. Go back to SLA Levels
2. Click Edit on Silver level
3. Change resolution time to 10 hours
4. Save

**Expected Result**: ✅ Changes saved, new complaints use new time

### Step 9.2: Edit Category Mapping
1. Go to Category Mappings
2. Edit one mapping
3. Change SLA level or add override time
4. Save

**Expected Result**: ✅ Changes saved successfully

### Step 9.3: Deactivate SLA Level
1. Edit Bronze level
2. Uncheck "Active"
3. Save

**Expected Result**: ✅ Bronze level should not be selectable for new mappings

---

## Test Phase 10: Data Validation

### Step 10.1: API Verification
Open browser developer console and test:

```javascript
// Get SLA Settings
fetch('http://localhost:5058/api/sla/settings', {
  headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
})
.then(r => r.json())
.then(console.log);

// Get SLA Levels
fetch('http://localhost:5058/api/sla/levels', {
  headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
})
.then(r => r.json())
.then(console.log);

// Get Category Mappings
fetch('http://localhost:5058/api/sla/category-mappings', {
  headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
})
.then(r => r.json())
.then(console.log);

// Get Priority Mappings
fetch('http://localhost:5058/api/sla/priority-mappings', {
  headers: { 'Authorization': 'Bearer ' + localStorage.getItem('token') }
})
.then(r => r.json())
.then(console.log);
```

**Expected Result**: ✅ All endpoints return data successfully

---

## Test Results Checklist

### Core Functionality
- [ ] SLA Management module accessible
- [ ] Global SLA settings configured
- [ ] 3 SLA levels created (Gold, Silver, Bronze)
- [ ] Category-SLA mappings created
- [ ] Priority-SLA mappings created
- [ ] Critical complaint created with 2-hour SLA
- [ ] Normal complaint created with category SLA
- [ ] High complaint created with 8-hour SLA
- [ ] Fallback test passed (unmapped category)
- [ ] Complaints list shows SLA information
- [ ] Complaint details show due dates
- [ ] All API endpoints accessible

### UI/UX
- [ ] Forms load without errors
- [ ] Validation working correctly
- [ ] Success messages displayed
- [ ] Error messages clear and helpful
- [ ] Data displayed correctly in lists
- [ ] Edit functionality works
- [ ] Delete/deactivate works

### Calculations
- [ ] Due dates calculated correctly
- [ ] Priority-SLA override working
- [ ] Category-SLA mapping working
- [ ] Priority Master fallback working
- [ ] Working hours calculation functioning
- [ ] Weekend skipping works (if tested)

---

## Known Issues to Watch For

### Issue 1: Token Permissions
**Symptom**: 403 Forbidden errors on SLA endpoints
**Cause**: Old token without SLA permissions
**Solution**: Logout and login again

### Issue 2: Stale Data
**Symptom**: Changes not reflected immediately
**Cause**: Browser caching
**Solution**: Hard refresh (Ctrl+F5) or clear cache

### Issue 3: Validation Errors
**Symptom**: Form won't submit
**Cause**: Required fields missing
**Solution**: Check all required fields filled

---

## Success Criteria

**Test is successful if**:
- ✅ All 3 SLA levels created
- ✅ At least 2 category mappings created
- ✅ At least 2 priority mappings created
- ✅ All 4 test complaints created successfully
- ✅ Due dates calculated and displayed
- ✅ SLA information visible on UI
- ✅ No console errors
- ✅ API endpoints returning correct data

---

## Test Report Template

### Test Execution Summary

**Tester Name**: _________________
**Test Date**: _________________
**Test Duration**: _________________

### Results

| Phase | Status | Notes |
|-------|--------|-------|
| 1. SLA Management Access | ☐ Pass ☐ Fail | |
| 2. Global Settings | ☐ Pass ☐ Fail | |
| 3. SLA Levels Creation | ☐ Pass ☐ Fail | |
| 4. Category Mappings | ☐ Pass ☐ Fail | |
| 5. Priority Mappings | ☐ Pass ☐ Fail | |
| 6. Complaint Creation | ☐ Pass ☐ Fail | |
| 7. SLA Display | ☐ Pass ☐ Fail | |
| 8. Working Hours | ☐ Pass ☐ Fail | |
| 9. Edit/Update | ☐ Pass ☐ Fail | |
| 10. API Verification | ☐ Pass ☐ Fail | |

### Complaints Created

| Complaint Number | Priority | Category | Due Date | Expected SLA | Actual SLA | Status |
|------------------|----------|----------|----------|--------------|------------|--------|
| CMP-2025-____ | Critical | _______ | ________ | 2 hours | _______ | ☐ Pass ☐ Fail |
| CMP-2025-____ | Normal | _______ | ________ | 4 hours | _______ | ☐ Pass ☐ Fail |
| CMP-2025-____ | High | _______ | ________ | 8 hours | _______ | ☐ Pass ☐ Fail |
| CMP-2025-____ | Low | _______ | ________ | Fallback | _______ | ☐ Pass ☐ Fail |

### Issues Found

| Issue | Severity | Description | Screenshot |
|-------|----------|-------------|------------|
| 1. | ☐ High ☐ Medium ☐ Low | | |
| 2. | ☐ High ☐ Medium ☐ Low | | |
| 3. | ☐ High ☐ Medium ☐ Low | | |

### Overall Verdict

**Status**: ☐ PASS ☐ FAIL
**Ready for Production**: ☐ YES ☐ NO ☐ WITH FIXES

**Additional Notes**:
________________________________________________________________
________________________________________________________________
________________________________________________________________

---

**End of Manual UI Testing Guide**

**Remember**: Take screenshots of each successful step for documentation!
