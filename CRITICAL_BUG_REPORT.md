# 🔴 CRITICAL BUG REPORT: Statistics vs Complaints API Mismatch

**Date**: November 11, 2025
**Severity**: HIGH
**Status**: 🔴 UNRESOLVED

---

## Bug Summary

There is a **data inconsistency** between two APIs when queried with admin credentials:
- **Complaints API** (`/api/complaints`): Returns **5 complaints**
- **Statistics API** (`/api/dashboard/statistics`): Returns **37 complaints**
- **Discrepancy**: **32 complaints** difference

---

## Investigation Results

### Test 1: Handler Role Statistics
- **Handler**: naveen.chandra@oryggitech.com
- **Complaints Assigned**: 0
- **Statistics Count**: 0
- **Status**: ✅ PASS - Filtering works correctly (no complaints to test with)
- **Finding**: ⚠️ Handler has zero complaints assigned, cannot fully test handler filtering

### Test 2: Database Complaint Count
- **Total Complaints via Complaints API (Admin)**: 5
- **Complainants**: Nav Nainital (5 complaints)
- **Status**: All "Submitted"
- **Complaint Numbers**: CMP-2025-1143, CMP-2025-1144, CMP-2025-1145, CMP-2025-1146, CMP-2025-1147

### Test 3: Statistics API Count
- **Total Complaints via Statistics API (Admin)**: 37
- **Breakdown**:
  - Submitted: 32
  - Closed: 1
  - Reopened: 4
- **Active**: 36
- **Completed**: 1

---

## Root Cause Analysis

### Hypothesis 1: Database Actually Has 37 Complaints ✅ LIKELY
**Evidence**:
- EF Core logs show `WHERE [c].[IsDeleted] = CAST(0 AS bit)` is applied correctly
- Soft-delete filtering is working
- Statistics service queries the database and finds 37 active records

**Counter-Evidence**:
- Complaints API with admin token returns only 5
- Admin should have NO filtering applied

### Hypothesis 2: Complaints API Has Hidden Filtering Bug ⚠️ INVESTIGATING
**Evidence**:
- Admin logs show "accessing complaints with admin privileges"
- Yet only 5 are returned
- Possible pagination issue or additional WHERE clause

### Hypothesis 3: Two Different Databases/Connection Strings ❌ UNLIKELY
**Evidence**: Both APIs use same DbContext and connection string

---

## Technical Details

### Backend Logs Confirmed:
```
Admin user f56d8d03-e382-454b-bf7d-fa8236c125c3 accessing complaints with admin privileges
Admin user f56d8d03-e382-454b-bf7d-fa8236c125c3 accessing dashboard statistics with full access
```

### SQL Query Filter Applied:
```sql
WHERE [c].[IsDeleted] = CAST(0 AS bit)
```

This confirms soft-delete filter is working correctly.

---

## Impact Assessment

### Business Impact
- **Data Integrity**: Users cannot trust dashboard statistics
- **Reporting**: Financial/operational reports will be inaccurate
- **User Confidence**: Admins see mismatched numbers

### Technical Impact
- **Statistics Dashboard**: Shows incorrect aggregate data
- **Complaints List**: May be missing 32 complaints
- **Role-Based Filtering**: Cannot be fully validated

---

## Recommended Next Steps

### Immediate Actions Required:
1. **Direct Database Query**: Run SQL query directly against database to count actual records
   ```sql
   SELECT COUNT(*) FROM Complaints WHERE ISNULL(IsDeleted, 0) = 0
   ```

2. **Check GetComplaintsQueryHandler**: Examine if there's additional filtering logic in the CQRS handler

3. **Verify Pagination**: Check if pageSize=200 is limiting results

4. **Database Backup**: Ensure we can roll back if data corruption occurred

### Debugging Steps:
1. Enable EF Core detailed logging for both endpoints
2. Compare exact SQL queries being executed
3. Check if there are orphaned records (foreign key issues)
4. Verify database connection strings are identical

---

## Workaround

**None Available** - This is a data integrity issue that must be resolved before production deployment.

---

## Files Involved

### Controllers:
- `ComplaintsController.cs` (lines 56-125) - Admin should have no filtering
- `DashboardController.cs` (lines 110-176) - Statistics endpoint

### Services:
- `DashboardService.cs` (lines 112-247) - Statistics calculation
- `GetComplaintsQueryHandler.cs` - Complaints query logic

### Database:
- `ComplaintDbContext.cs` (lines 324-338) - Global query filters

---

## Test Evidence

**Complaints API Response** (Admin):
```json
{
  "data": {
    "totalCount": 5,
    "items": [...5 complaints...]
  }
}
```

**Statistics API Response** (Admin):
```json
{
  "data": {
    "totalComplaints": 37,
    "activeComplaints": 36,
    "completedComplaints": 1,
    "statusWidgets": [
      { "name": "Submitted", "currentCount": 32 },
      { "name": "Closed", "currentCount": 1 },
      { "name": "Reopened", "currentCount": 4 }
    ]
  }
}
```

---

## Status

**🔴 CRITICAL - REQUIRES IMMEDIATE INVESTIGATION**

This bug must be resolved before:
- ✅ Handler filtering testing can be completed
- ✅ Frontend dashboard testing can be validated
- ✅ System can be deployed to production

---

**Report Generated**: November 11, 2025
**Investigation Status**: IN PROGRESS
**Priority**: P0 - CRITICAL
