# Oryggi Sync Scheduling - Test Plan

## Overview
This document outlines the test plan for the Oryggi HRMS sync scheduling functionality.

## Test Environment
- **Backend API**: http://localhost:5058
- **Frontend (Angular)**: http://localhost:4200
- **Test User**: admin@complaintmanagement.com / Admin@123

## Features Implemented

### Backend
✅ SyncSchedule entity with support for Daily, Weekly, and Monthly schedules
✅ Database migration completed and applied
✅ API endpoints for schedule CRUD operations
✅ Background service checking schedules every minute
✅ Employee filtering (excludes CorpEmpCode with underscore and Admin user with Ecode=1)

### Frontend
✅ Complete Angular UI for schedule management
✅ Form for creating/editing schedules with validation
✅ Schedule list with enable/disable/delete actions
✅ Real-time status updates

## Manual Test Cases

### 1. Access the Oryggi Sync Page
**Steps**:
1. Open browser and navigate to http://localhost:4200
2. Login with admin@complaintmanagement.com / Admin@123
3. Navigate to Admin → Oryggi Sync page

**Expected Result**:
- Should see the Oryggi Database Sync page
- Should see "Sync Schedules" card
- Should see "Add Schedule" button (if no schedules exist)

---

### 2. Create a Daily Schedule
**Steps**:
1. Click "Add Schedule" button
2. Select "Daily" from Schedule Type dropdown
3. Set Time of Day to "02:00" (or any time)
4. Enter description: "Daily sync at 2 AM"
5. Ensure "Enabled" checkbox is checked
6. Click "Create Schedule"

**Expected Result**:
- Success message: "Schedule created successfully"
- Schedule appears in the list with:
  - Green "Daily" badge (enabled)
  - Description: "Daily at 02:00"
  - "Next run" timestamp displayed
  - Edit, Play/Pause, Delete buttons visible

---

### 3. Create a Weekly Schedule
**Steps**:
1. Click "Add Schedule" button
2. Select "Weekly" from Schedule Type dropdown
3. Set Time of Day to "03:00"
4. Select "Monday" from Day of Week dropdown
5. Enter description: "Weekly sync every Monday"
6. Ensure "Enabled" checkbox is checked
7. Click "Create Schedule"

**Expected Result**:
- Success message: "Schedule created successfully"
- Schedule appears in list showing:
  - "Weekly at 03:00 on Monday"
  - Next run calculated for next Monday at 3 AM

---

### 4. Create a Monthly Schedule
**Steps**:
1. Click "Add Schedule" button
2. Select "Monthly" from Schedule Type dropdown
3. Set Time of Day to "04:00"
4. Enter Day of Month: "15"
5. Enter description: "Monthly sync on 15th"
6. Ensure "Enabled" checkbox is checked
7. Click "Create Schedule"

**Expected Result**:
- Success message: "Schedule created successfully"
- Schedule appears showing:
  - "Monthly at 04:00 on day 15"
  - Next run calculated for 15th of next month at 4 AM

---

### 5. Edit a Schedule
**Steps**:
1. Click the Edit button (✏️) on any schedule
2. Change the time from current value to a new time (e.g., "05:00")
3. Update the description
4. Click "Update Schedule"

**Expected Result**:
- Success message: "Schedule updated successfully"
- Schedule list refreshes with updated values
- Form closes and returns to list view

---

### 6. Disable/Enable a Schedule
**Steps**:
1. Click the Pause button (⏸️) on an enabled schedule
2. Observe the changes
3. Click the Play button (▶️) to re-enable

**Expected Result**:
- When disabled:
  - Success message: "Schedule disabled successfully"
  - Badge changes to gray with "Disabled" status
  - Button changes from ⏸️ to ▶️
- When re-enabled:
  - Success message: "Schedule enabled successfully"
  - Badge changes to green with "Enabled" status
  - Button changes from ▶️ to ⏸️

---

### 7. Delete a Schedule
**Steps**:
1. Click the Delete button (🗑️) on any schedule
2. Confirm the deletion in the confirmation dialog

**Expected Result**:
- Confirmation dialog appears: "Are you sure you want to delete this [type] schedule?"
- After confirming:
  - Success message: "Schedule deleted successfully"
  - Schedule removed from the list

---

### 8. Form Validation Tests

**Test 8.1: Weekly without Day Selection**
1. Create new schedule
2. Select "Weekly"
3. Leave day of week unselected
4. Click "Create Schedule"

**Expected**: Error message: "For Weekly schedules, please select a day of week (Sunday-Saturday)"

**Test 8.2: Monthly without Day Number**
1. Create new schedule
2. Select "Monthly"
3. Leave day of month empty
4. Click "Create Schedule"

**Expected**: Error message: "For Monthly schedules, please select a day (1-31)"

**Test 8.3: Time Required**
1. Create new schedule
2. Clear the time field
3. Click "Create Schedule"

**Expected**: Error message: "Time of day is required"

---

### 9. Background Service Verification

**Steps**:
1. Create a schedule with next run time in the next 2 minutes
2. Wait for the scheduled time
3. Check the Sync History card for new sync entry

**Expected Result**:
- Background service should execute the sync at scheduled time
- New entry appears in Sync History with:
  - Type: "SCHEDULED"
  - Status: "SUCCESS"
  - Duration and stats displayed

**Note**: Background service checks every minute, so schedule will run within 1 minute of scheduled time.

---

### 10. Manual Sync Trigger Test

**Steps**:
1. Click "Trigger Sync" button in Manual Sync card
2. Wait for sync to complete

**Expected Result**:
- Button shows "⏳ Syncing..." during sync
- Success message appears with sync statistics
- Latest Sync Status card updates with new sync results
- Sync History table shows new entry with "MANUAL" type

---

## API Endpoint Tests (Optional - using Postman/curl)

### Get All Schedules
```
GET http://localhost:5058/api/OryggiSync/schedules
Authorization: Bearer <token>
```

### Create Schedule
```
POST http://localhost:5058/api/OryggiSync/schedules
Authorization: Bearer <token>
Content-Type: application/json

{
  "scheduleType": "Daily",
  "timeOfDay": "02:00",
  "isEnabled": true,
  "description": "Test daily schedule"
}
```

### Update Schedule
```
PUT http://localhost:5058/api/OryggiSync/schedules/{id}
Authorization: Bearer <token>
Content-Type: application/json

{
  "scheduleType": "Daily",
  "timeOfDay": "03:00",
  "isEnabled": false,
  "description": "Updated schedule"
}
```

### Delete Schedule
```
DELETE http://localhost:5058/api/OryggiSync/schedules/{id}
Authorization: Bearer <token>
```

---

## Database Verification

### Check Schedules in Database
```sql
SELECT
    Id,
    TenantId,
    ScheduleType,
    TimeOfDay,
    DayValue,
    IsEnabled,
    LastRunAt,
    NextRunAt,
    Description,
    CreatedAt
FROM SyncSchedules
WHERE IsDeleted = 0
ORDER BY CreatedAt DESC;
```

### Check Sync Logs
```sql
SELECT TOP 10
    SyncLogId,
    TenantId,
    SyncType,
    Status,
    StartedAt,
    CompletedAt,
    Duration,
    EmployeesCreated,
    EmployeesUpdated
FROM SyncLogs
ORDER BY StartedAt DESC;
```

---

## Known Issues / Notes

1. **Time Zone**: All times are stored and displayed in UTC
2. **Schedule Execution**: Background service checks every minute
3. **Next Run Calculation**: Automatically calculated when schedule is created/updated
4. **Employee Filtering**: System excludes:
   - Employees with CorpEmpCode containing underscore
   - Admin user (Ecode = 1)

---

## Test Status

| Test Case | Status | Notes |
|-----------|--------|-------|
| Access Oryggi Sync Page | ⏳ Pending | |
| Create Daily Schedule | ⏳ Pending | |
| Create Weekly Schedule | ⏳ Pending | |
| Create Monthly Schedule | ⏳ Pending | |
| Edit Schedule | ⏳ Pending | |
| Disable/Enable Schedule | ⏳ Pending | |
| Delete Schedule | ⏳ Pending | |
| Form Validation | ⏳ Pending | |
| Background Service | ⏳ Pending | |
| Manual Sync Trigger | ⏳ Pending | |

---

## Completion Checklist

- [ ] All schedule types (Daily, Weekly, Monthly) can be created
- [ ] Schedules can be edited and changes are saved
- [ ] Schedules can be enabled/disabled
- [ ] Schedules can be deleted
- [ ] Form validation works correctly
- [ ] Next run times are calculated correctly
- [ ] Background service executes schedules on time
- [ ] Manual sync works independently
- [ ] UI displays all information correctly
- [ ] Error messages are clear and helpful

---

## Next Steps After Testing

1. Document any bugs found during testing
2. Verify all schedules execute correctly at their scheduled times
3. Monitor system logs for any errors
4. Consider adding email notifications for sync failures (future enhancement)
5. Consider adding retry logic for failed syncs (future enhancement)
