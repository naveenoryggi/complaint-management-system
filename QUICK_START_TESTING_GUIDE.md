# Quick Start Testing Guide

## How to Run the Tests

### Prerequisites
- SQL Server accessible at: `LAPTOP-NF9BTG7Q\SQLEXPRESS`
- Database: `ComplaintManagementDB`
- PowerShell 5.1 or higher
- Windows Integrated Authentication

### Step 1: Verify Database Connection
```powershell
cd "C:\Users\Navin Chandra\Pictures\Complaint management system"
.\test-db-connection.ps1
```

**Expected Output:**
- ✅ Database connected successfully
- ✅ Total Complaints count
- ✅ Status distribution

### Step 2: Discover Database Schema
```powershell
.\get-schema.ps1
```

**Expected Output:**
- List of all 38 tables
- Complaints table column structure

### Step 3: Run Data Integrity Tests
```powershell
.\run-data-integrity-tests.ps1
```

**Expected Output:**
- Test execution progress
- Pass/Fail status for each test
- Summary with success rate
- Issues found (if any)

**Results Saved To:**
- `DATA_INTEGRITY_TEST_RESULTS_[timestamp].txt`
- `data-integrity-issues.json`

### Step 4: Review Results
```powershell
# View the latest test results
Get-Content .\DATA_INTEGRITY_TEST_RESULTS_*.txt | Select-Object -Last 50

# View issues in JSON format
Get-Content .\data-integrity-issues.json | ConvertFrom-Json | Format-List
```

---

## Test Files Overview

| File Name | Purpose | Lines | Status |
|-----------|---------|-------|--------|
| `comprehensive-data-integrity-tests.ps1` | Full data integrity suite | 592 | ⚠️ Needs schema updates |
| `comprehensive-dashboard-tests.ps1` | Dashboard validation | 468 | ⚠️ Needs API running |
| `comprehensive-workflow-tests.ps1` | Business workflow tests | 684 | ⚠️ Needs API running |
| `comprehensive-edge-case-tests.ps1` | Edge case testing | 769 | ⚠️ Needs API running |
| `run-data-integrity-tests.ps1` | Corrected data integrity | 404 | ✅ Ready to run |
| `test-db-connection.ps1` | Database connectivity test | 33 | ✅ Ready to run |
| `get-schema.ps1` | Schema discovery | 25 | ✅ Ready to run |

---

## Understanding Test Results

### Test Status Codes

- ✅ **PASS** - Test passed successfully
- ❌ **FAIL** - Test failed, issue found
- ⚠️ **ERROR** - SQL error (usually schema mismatch)

### Severity Levels

- 🔴 **CRITICAL** - Must fix immediately (data corruption risk)
- 🟡 **HIGH** - Should fix soon (functionality impact)
- 🔵 **MEDIUM** - Can fix in next sprint (improvement)

---

## Common Issues

### Issue 1: SQL Error - Invalid Object Name

**Error:** `Invalid object name 'StatusMaster'`

**Cause:** Test uses old table name

**Solution:** Update test to use `ComplaintStatusMasters`

### Issue 2: SQL Error - Invalid Column Name

**Error:** `Invalid column name 'Subject'`

**Cause:** Test uses old column name

**Solution:** Update test to use `Title`

### Issue 3: API Not Running

**Error:** `Connection refused` or `404`

**Cause:** Backend API not started

**Solution:**
```powershell
cd "C:\Users\Navin Chandra\Pictures\Complaint management system\complaint-system-dotnet"
# Start the backend API
dotnet run --project src/ComplaintManagementSystem.API
```

---

## Schema Reference (Quick Look)

### Table Name Mapping

| Old Name (Tests) | Actual Name (DB) |
|------------------|------------------|
| StatusMaster | ComplaintStatusMasters |
| PriorityMaster | ComplaintPriorityMasters |
| CategoryMaster | ComplaintCategories |
| ComplaintEscalations | EscalationHistories |
| ComplaintStatusHistory | EscalationHistories |

### Column Name Mapping

| Old Name (Tests) | Actual Name (DB) |
|------------------|------------------|
| Subject | Title |
| CreatedDate | CreatedAt |
| UpdatedDate | UpdatedAt |
| AssignedTo | AssignedToId |
| CategoryMasterId | CategoryId |

---

## Fixing the Tests

### Option 1: Quick Fix (One Test)

Edit `run-data-integrity-tests.ps1` and update:
- Replace `StatusMaster` with `ComplaintStatusMasters`
- Replace `PriorityMaster` with `ComplaintPriorityMasters`
- Replace `Subject` with `Title`
- Replace `CreatedDate` with `CreatedAt`
- etc.

### Option 2: Bulk Fix (All Tests)

Use PowerShell to update all files:
```powershell
$files = @(
    "comprehensive-data-integrity-tests.ps1",
    "comprehensive-dashboard-tests.ps1",
    "comprehensive-workflow-tests.ps1",
    "comprehensive-edge-case-tests.ps1"
)

$replacements = @{
    'StatusMaster' = 'ComplaintStatusMasters'
    'PriorityMaster' = 'ComplaintPriorityMasters'
    'CategoryMaster' = 'ComplaintCategories'
    'Subject' = 'Title'
    'CreatedDate' = 'CreatedAt'
    'UpdatedDate' = 'UpdatedAt'
    'AssignedTo' = 'AssignedToId'
    'CategoryMasterId' = 'CategoryId'
}

foreach ($file in $files) {
    $content = Get-Content $file -Raw
    foreach ($old in $replacements.Keys) {
        $new = $replacements[$old]
        $content = $content -replace $old, $new
    }
    Set-Content $file $content
}
```

---

## Interpreting Results

### Scenario 1: All Tests Pass (100%)
**Meaning:** Data integrity is good
**Action:** Continue with dashboard and workflow tests

### Scenario 2: Some Tests Fail (< 95%)
**Meaning:** Data integrity issues found
**Action:** Review failed tests, fix data issues, re-run

### Scenario 3: Many SQL Errors (Schema Issues)
**Meaning:** Tests don't match actual database schema
**Action:** Update test scripts with correct names, re-run

---

## What to Do with Results

### If Tests Pass
1. ✅ Archive results for reference
2. ✅ Move on to next test suite
3. ✅ Update documentation
4. ✅ Schedule regular testing

### If Tests Fail
1. ❌ Review failure details in log file
2. ❌ Check database for actual issues
3. ❌ Fix data problems
4. ❌ Re-run tests to verify fixes
5. ❌ Document root cause

### If Schema Errors Occur
1. ⚠️ Update test scripts
2. ⚠️ Update application code if needed
3. ⚠️ Update documentation
4. ⚠️ Re-run tests with correct schema
5. ⚠️ Implement schema validation

---

## Troubleshooting

### Cannot Connect to Database

**Problem:** `Login failed` or `Connection failed`

**Solutions:**
1. Verify SQL Server is running
2. Check Windows Authentication is enabled
3. Confirm database name is correct
4. Test with SQL Server Management Studio first

### Tests Take Too Long

**Problem:** Tests running for more than 5 minutes

**Solutions:**
1. Check database indexes
2. Reduce test data volume
3. Run tests on smaller dataset
4. Optimize SQL queries in tests

### Permission Denied

**Problem:** `Access denied` or `Permission error`

**Solutions:**
1. Run PowerShell as Administrator
2. Set execution policy: `Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process`
3. Check database user permissions

---

## Next Steps After Testing

1. **Review Reports**
   - `AUTONOMOUS_TEST_EXECUTION_REPORT.md` - Full details
   - `EXECUTIVE_SUMMARY.md` - Quick overview
   - `DATA_INTEGRITY_TEST_RESULTS_[timestamp].txt` - Test logs

2. **Fix Issues**
   - Address CRITICAL issues first
   - Then HIGH priority
   - Schedule MEDIUM priority

3. **Update Code**
   - Fix schema mismatches
   - Update API endpoints
   - Align frontend models

4. **Re-Run Tests**
   - Verify fixes work
   - Ensure all tests pass
   - Document results

5. **Implement Monitoring**
   - Add schema validation to CI/CD
   - Schedule regular test runs
   - Set up alerting for failures

---

## Support & Resources

**Documentation:**
- Full Report: `AUTONOMOUS_TEST_EXECUTION_REPORT.md`
- Executive Summary: `EXECUTIVE_SUMMARY.md`
- This Guide: `QUICK_START_TESTING_GUIDE.md`

**Test Files Location:**
```
C:\Users\Navin Chandra\Pictures\Complaint management system\
```

**Database Info:**
- Server: `LAPTOP-NF9BTG7Q\SQLEXPRESS`
- Database: `ComplaintManagementDB`
- Tables: 38
- Complaints: 1,054

---

## Quick Reference Commands

```powershell
# Test database connectivity
.\test-db-connection.ps1

# Get database schema
.\get-schema.ps1

# Run data integrity tests
.\run-data-integrity-tests.ps1

# View latest results (last 50 lines)
Get-Content .\DATA_INTEGRITY_TEST_RESULTS_*.txt | Select-Object -Last 50

# Count total tests
(Select-String -Path .\DATA_INTEGRITY_TEST_RESULTS_*.txt -Pattern "PASS:|FAIL:").Count

# Count failed tests
(Select-String -Path .\DATA_INTEGRITY_TEST_RESULTS_*.txt -Pattern "FAIL:").Count

# View critical issues only
Select-String -Path .\DATA_INTEGRITY_TEST_RESULTS_*.txt -Pattern "CRITICAL" -Context 0,2

# Check database status distribution
.\test-db-connection.ps1 | Select-String -Pattern "Status Distribution:" -Context 0,10
```

---

**Last Updated:** October 24, 2025
**Version:** 1.0
**Status:** Ready for Use ✅
