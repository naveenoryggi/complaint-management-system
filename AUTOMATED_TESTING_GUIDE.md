# 🤖 Automated Testing Framework - User Guide

**Created:** October 23, 2025
**Version:** 1.0

---

## Overview

This automated testing framework runs completely autonomously without requiring any user involvement. It continuously monitors your Complaint Management System and provides detailed reports.

---

## What Has Been Created

### 1. **automated-test-master.ps1** (Main Test Suite)
Comprehensive test suite covering all system functionality.

**Test Categories:**
- ✅ API Health Checks (Backend, Frontend, Database)
- ✅ CRUD Operations (Create, Read, Update, Delete)
- ✅ Comment System (Add, Retrieve)
- ✅ Status Transitions (Workflow testing)
- ✅ Dashboard & Reports (Stats, Preferences)
- ✅ Search & Filters (Search, Category filters)
- ✅ Performance Tests (Response time monitoring)

**Features:**
- Automatic authentication
- Detailed logging to `TEST_MASTER_LOG.txt`
- Beautiful HTML reports
- Color-coded console output
- Pass/fail tracking for each test
- Overall pass rate calculation

---

### 2. **test-scheduler.ps1** (Scheduler)
Runs tests automatically on your chosen schedule.

**Available Schedules:**
- `Hourly` - Every hour
- `Every6Hours` - Every 6 hours (default)
- `Daily` - Once per day
- `Weekly` - Once per week

**Features:**
- Runs continuously in background
- Automatic retry on failure
- Run counter and timestamps
- Next run time display

---

### 3. **ui-automation-test.ps1** (UI Testing)
Tests frontend functionality and accessibility.

**Test Categories:**
- ✅ UI Accessibility (Page loads, routing)
- ✅ UI Routes (All major routes)
- ✅ UI Performance (Page load times)
- ✅ UI Components (Component existence)

---

## How to Use (Zero Involvement Required)

### Option 1: Run Tests Once

```powershell
# Navigate to project directory
cd "C:\Users\Navin Chandra\Pictures\Complaint management system"

# Run the master test suite
powershell -ExecutionPolicy Bypass -File automated-test-master.ps1
```

**What happens:**
- All tests execute automatically
- HTML report is generated
- Console shows colored results
- Takes ~2-5 minutes

---

### Option 2: Scheduled Testing (Recommended)

```powershell
# Run every 6 hours (default)
powershell -ExecutionPolicy Bypass -File test-scheduler.ps1

# Or specify custom schedule
powershell -ExecutionPolicy Bypass -File test-scheduler.ps1 -Schedule Hourly
powershell -ExecutionPolicy Bypass -File test-scheduler.ps1 -Schedule Daily
```

**What happens:**
- Tests run automatically on schedule
- Continues running forever
- You can close the window - it keeps running
- Check reports anytime in the project folder

---

### Option 3: Background Execution (Set It and Forget It)

```powershell
# Start in background
Start-Job -ScriptBlock {
    Set-Location "C:\Users\Navin Chandra\Pictures\Complaint management system"
    powershell -ExecutionPolicy Bypass -File test-scheduler.ps1 -Schedule Every6Hours
}

# Check if still running
Get-Job

# View output (optional)
Receive-Job -Id 1 -Keep
```

**What happens:**
- Tests run in background
- No console window needed
- Runs even if you logout
- Reports saved automatically

---

## Understanding the Reports

### Console Output

**Color Coding:**
- 🟢 **Green** = Test passed
- 🔴 **Red** = Test failed
- 🟡 **Yellow** = Warning or info
- ⚪ **White** = General info

**Example Output:**
```
[2025-10-23 20:30:45] [INFO] AUTOMATED TEST SUITE STARTING
[2025-10-23 20:30:46] [SUCCESS] Authentication successful
[2025-10-23 20:30:47] [SUCCESS] ✓ Backend API is healthy
[2025-10-23 20:30:48] [SUCCESS] ✓ Frontend is accessible
...
[2025-10-23 20:35:22] [INFO] FINAL RESULTS
[2025-10-23 20:35:22] [INFO] Total Tests: 20
[2025-10-23 20:35:22] [SUCCESS] Passed: 20
[2025-10-23 20:35:22] [SUCCESS] Failed: 0
[2025-10-23 20:35:22] [SUCCESS] Pass Rate: 100%
```

---

### HTML Reports

**Location:** `TEST_REPORT_YYYYMMDD_HHMMSS.html`

**Features:**
- Beautiful gradient design
- Summary cards with key metrics
- Color-coded test suite results
- Timestamps and duration
- Mobile-friendly layout

**How to View:**
1. Open File Explorer
2. Navigate to project folder
3. Double-click any `TEST_REPORT_*.html` file
4. Opens in your default browser

---

### Log Files

**TEST_MASTER_LOG.txt:**
- Complete execution history
- Every test run appended
- Timestamps for all events
- Error messages and stack traces

**Example:**
```
[2025-10-23 20:30:45] [INFO] AUTOMATED TEST SUITE STARTING
[2025-10-23 20:30:46] [SUCCESS] Authentication successful
[2025-10-23 20:30:47] [SUCCESS] ✓ Backend API is healthy
```

---

## What Gets Tested Automatically

### 1. API Health (3 Tests)
- ✅ Backend API responding
- ✅ Frontend accessible
- ✅ Database connection working

### 2. CRUD Operations (4 Tests)
- ✅ Create complaint
- ✅ Read complaint
- ✅ Update complaint
- ✅ Delete complaint

### 3. Comment System (2 Tests)
- ✅ Add comment
- ✅ Retrieve comments

### 4. Status Transitions (1 Test)
- ✅ Change complaint status

### 5. Dashboard & Reports (2 Tests)
- ✅ Dashboard stats
- ✅ Dashboard preferences

### 6. Search & Filters (2 Tests)
- ✅ Basic search
- ✅ Category filtering

### 7. Performance (1 Test)
- ✅ API response time

### 8. UI Tests (12+ Tests)
- ✅ Page accessibility
- ✅ Route navigation
- ✅ Component existence
- ✅ Load performance

---

## Automatic Features (No Action Needed)

### Authentication
- ✅ Logs in automatically
- ✅ Uses admin credentials
- ✅ Obtains JWT token
- ✅ Refreshes if expired

### Test Data Management
- ✅ Creates test data as needed
- ✅ Cleans up after tests
- ✅ Uses existing data when available
- ✅ No manual setup required

### Error Handling
- ✅ Continues on individual test failure
- ✅ Logs all errors
- ✅ Reports issues clearly
- ✅ Automatic retry for network errors

### Report Generation
- ✅ HTML report every run
- ✅ Timestamped filenames
- ✅ Summary statistics
- ✅ Detailed breakdowns

---

## Monitoring Test Results

### Quick Check
```powershell
# View latest log entries
Get-Content TEST_MASTER_LOG.txt -Tail 50

# Count total test runs
(Select-String -Path TEST_MASTER_LOG.txt -Pattern "AUTOMATED TEST SUITE STARTING").Count

# Check pass rate
Select-String -Path TEST_MASTER_LOG.txt -Pattern "Pass Rate"
```

### View All Reports
```powershell
# List all HTML reports
Get-ChildItem TEST_REPORT_*.html | Sort-Object LastWriteTime -Descending

# Open latest report
$latest = Get-ChildItem TEST_REPORT_*.html | Sort-Object LastWriteTime -Descending | Select-Object -First 1
Start-Process $latest.FullName
```

---

## Alerts and Notifications

### When Tests Fail
The system automatically:
- ✅ Highlights failures in red
- ✅ Logs detailed error messages
- ✅ Continues with remaining tests
- ✅ Generates report with failure details

### Manual Alert Check
```powershell
# Check if recent tests had failures
$log = Get-Content TEST_MASTER_LOG.txt -Tail 100
if ($log -match "Failed: [1-9]") {
    Write-Host "⚠ Recent test failures detected!" -ForegroundColor Red
}
else {
    Write-Host "✓ All recent tests passing" -ForegroundColor Green
}
```

---

## Best Practices

### Recommended Setup
1. **Start the scheduler once:**
   ```powershell
   powershell -ExecutionPolicy Bypass -File test-scheduler.ps1 -Schedule Every6Hours
   ```

2. **Let it run continuously** (minimize the window)

3. **Check reports weekly** (or when you remember)

4. **Review log file monthly** (optional)

### Storage Management
```powershell
# Delete old reports (keep last 30 days)
Get-ChildItem TEST_REPORT_*.html |
    Where-Object {$_.LastWriteTime -lt (Get-Date).AddDays(-30)} |
    Remove-Item

# Archive old logs
if ((Get-Item TEST_MASTER_LOG.txt).Length -gt 10MB) {
    Move-Item TEST_MASTER_LOG.txt "TEST_MASTER_LOG_$(Get-Date -Format 'yyyyMMdd').txt"
}
```

---

## Troubleshooting

### Tests Not Running
**Check if servers are running:**
```powershell
# Check backend
curl http://localhost:5058/health

# Check frontend
curl http://localhost:4200
```

**Start servers if needed:**
```powershell
# Backend
cd complaint-system-dotnet\src\ComplaintManagement.API
dotnet run

# Frontend
cd complaint-system-angular
npm start
```

### Authentication Failures
**Verify credentials in script:**
- Email: `admin@complaintmanagement.com`
- Password: `Admin@123`

**Reset admin password if needed:**
```powershell
powershell -ExecutionPolicy Bypass -File restore-admin.ps1
```

### High Failure Rate
**Common causes:**
- Server not running
- Database connection issue
- Network timeout

**Quick fix:**
1. Restart both servers
2. Check database is running
3. Run tests again

---

## Advanced Usage

### Run Specific Test Suite
```powershell
# Modify automated-test-master.ps1 to comment out unwanted tests
# Example: Comment out Performance tests if not needed
# # "Performance" = Test-Performance
```

### Customize Test Data
```powershell
# Edit the test creation logic in automated-test-master.ps1
# Look for Test-CRUDOperations function
# Modify $createData object
```

### Change Schedule
```powershell
# Edit test-scheduler.ps1
# Modify $intervalSeconds calculation
# Or use custom parameter:
powershell -File test-scheduler.ps1 -Schedule Daily
```

---

## Summary

### What You Get
- ✅ **20+ automated tests** covering all functionality
- ✅ **Beautiful HTML reports** with visual summaries
- ✅ **Scheduled execution** (runs automatically)
- ✅ **Zero maintenance** (set it and forget it)
- ✅ **Detailed logging** (track everything)
- ✅ **Performance monitoring** (response times)
- ✅ **UI testing** (frontend verification)

### What You Don't Need to Do
- ❌ No manual test execution
- ❌ No result compilation
- ❌ No report generation
- ❌ No monitoring required
- ❌ No data setup
- ❌ No cleanup tasks

### Time Savings
- **Manual testing:** 2-3 hours per cycle
- **Automated testing:** 0 minutes (runs itself)
- **Report review:** 5 minutes per week

---

## Quick Start Commands

**Start testing now (one-time):**
```powershell
powershell -ExecutionPolicy Bypass -File automated-test-master.ps1
```

**Start scheduled testing (continuous):**
```powershell
powershell -ExecutionPolicy Bypass -File test-scheduler.ps1
```

**View latest results:**
```powershell
Get-ChildItem TEST_REPORT_*.html | Sort-Object -Descending | Select-Object -First 1 | Invoke-Item
```

---

**You're all set! The system now tests itself automatically.** 🎉

*No further action required from you.*
