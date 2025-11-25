# 🎯 ROOT CAUSE ANALYSIS - Statistics Bug Investigation

**Date**: November 11, 2025
**Status**: 🔴 **BUG CONFIRMED AND IDENTIFIED**
**Investigator**: Claude Code Assistant

---

## Executive Summary

**Original Report**: User saw "32 submitted, 4 reopened, 1 closed" in statistics, but only 5 complaints in the system.

**Investigation Outcome**:
- ❌ **NOT A BUG in Statistics API** - Statistics API is CORRECT
- ✅ **BUG FOUND in Complaints API** - Complaints API is returning wrong count

---

## Ground Truth (Database Reality)

### Direct SQL Query Results:
```sql
SELECT COUNT(*) FROM Complaints WHERE ISNULL(IsDeleted, 0) = 0
-- Result: 37 active complaints

SELECT COUNT(*) FROM Complaints WHERE IsDeleted = 1
-- Result: 1110 soft-deleted complaints

SELECT ComplainantId, COUNT(*) FROM Complaints WHERE ISNULL(IsDeleted, 0) = 0 GROUP BY ComplainantId
-- Result:
--   f56d8d03-e382-454b-bf7d-fa8236c125c3 (ADMIN): 32 complaints
--   fd0073b8-fc95-4a49-867c-6ffb38b7d177 (OTHER):  5 complaints
```

**Database Ground Truth**: **37 active complaints** exist in the system

---

## API Response Comparison

| API Endpoint | Expected | Actual | Status |
|-------------|----------|--------|--------|
| Statistics API | 37 | 37 | ✅ **CORRECT** |
| Complaints API | 37 | 5 | ❌ **WRONG** |
| **Discrepancy** | 0 | **32 missing** | 🔴 **BUG** |

---

## Detailed Findings

### 1. Statistics API Analysis ✅
**File**: `DashboardService.cs:246`
**Query**: `var allComplaints = await baseQuery.ToListAsync(cancellationToken);`

**Result**: Returns **37 complaints** (correct)

**Verification**:
- ✅ Uses `_context.Complaints.AsQueryable()` directly
- ✅ Applies global query filter (`IsDeleted = 0`)
- ✅ For admin: No role-based filtering applied
- ✅ Returns actual database count

**Status**: **WORKING CORRECTLY**

---

### 2. Complaints API Analysis ❌
**File**: `GetComplaintsQueryHandler.cs:24`
**Query**: `await _unitOfWork.Complaints.GetAllWithIncludesAsync(...)`

**Result**: Returns **5 complaints** (wrong)

**Complaints Returned**:
- CMP-2025-1147: Parking pass request
- CMP-2025-1146: Printer issues
- CMP-2025-1145: Office AC not working
- CMP-2025-1144: Payroll discrepancy
- CMP-2025-1143: Cannot access employee portal

**All 5 created**: November 10, 2025

**Complaints NOT Returned** (32 missing):
- CMP-2025-1101, 1100, 1098, 1097, 1094, etc.
- Created: November 1, 2025 (and earlier)

**Status**: **BUG - FILTERING OUT 32 COMPLAINTS**

---

### 3. Admin Token Verification ✅
**User ID**: `f56d8d03-e382-454b-bf7d-fa8236c125c3`
**Email**: `admin@complaintmanagement.com`
**Name**: Updated Admin

**Permissions** (26 total):
- ✅ ManageUsers
- ✅ ManageSettings
- ✅ ManageCompany
- ✅ ManageRoles
- ✅ ViewComplaints
- ✅ EditComplaint
- ✅ DeleteComplaint
- ✅ (and 19 more...)

**Admin Detection**:
```csharp
bool isAdmin = permissions.Contains("ManageUsers") ||
              permissions.Contains("ManageSettings") ||
              permissions.Contains("ManageCompany");
// Should evaluate to: TRUE
```

**Status**: **TOKEN IS VALID ADMIN**

---

## Root Cause Hypothesis

### Most Likely Cause: Hidden Filtering in GetComplaintsQueryHandler

**Evidence**:
1. Only returns 5 most recent complaints (created Nov 10)
2. Excludes 32 older complaints (created Nov 1 and earlier)
3. Pattern suggests date-based filtering OR ownership filtering

**Possible Locations**:
1. **Line 79**: `var totalCount = allComplaints.Count();`
   - The `allComplaints` collection only has 5 items

2. **Line 24**: `GetAllWithIncludesAsync` might be applying hidden filter

3. **Repository Implementation**: May have date range or user-specific filtering

---

## Code Analysis Required

### Files to Investigate:
1. ✅ `GetComplaintsQueryHandler.cs` - CQRS handler (already reviewed)
2. ✅ `Repository.cs` - Base repository (already reviewed)
3. ❓ **ComplaintsRepository.cs** - Custom repository (NOT FOUND - using base)
4. ❓ **IComplaintsRepository.cs** - Interface (need to check for custom methods)
5. ❓ **GetComplaintsQuery.cs** - Query object (need to verify no hidden filters)

### Specific Code Sections:
```csharp
// GetComplaintsQueryHandler.cs:24-41
var allComplaints = (await _unitOfWork.Complaints.GetAllWithIncludesAsync(
    cancellationToken,
    c => c.Category,
    c => c.Complainant,
    ... // many includes
)).AsQueryable();

// Apply filters (lines 44-76)
if (request.StatusMasterId.HasValue) { /* filter */ }
if (request.PriorityMasterId.HasValue) { /* filter */ }
if (request.CompanyId.HasValue) { /* filter */ }
if (request.AssignedToId.HasValue) { /* filter */ }
if (request.ComplainantId.HasValue) { /* filter */ }  // ← Suspect
if (!string.IsNullOrWhiteSpace(request.SearchTerm)) { /* filter */ }

// Line 79: Count
var totalCount = allComplaints.Count();  // ← Returns 5, should be 37
```

**Question**: Why does `allComplaints.Count()` return 5 instead of 37?

---

## Impact Assessment

### Business Impact: 🔴 **CRITICAL**
- ✅ Dashboard statistics are CORRECT
- ❌ Complaint list is INCOMPLETE
- ❌ Admin cannot see 32 out of 37 complaints (86% of data hidden!)
- ❌ Older complaints are inaccessible via API
- ❌ Data integrity appears compromised to users

### Technical Impact: 🔴 **HIGH**
- ✅ Statistics endpoint working correctly
- ❌ Complaints list endpoint has data loss bug
- ❌ Role-based access control may be overly restrictive
- ❌ Cannot validate full system functionality

---

## Next Steps

### Priority 1: Enable SQL Logging
```csharp
// In appsettings.Development.json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

**Goal**: Capture exact SQL query executed by GetComplaintsQueryHandler

### Priority 2: Compare SQL Queries
- Run Complaints API call
- Run Statistics API call
- Compare WHERE clauses
- Identify the additional filter in Complaints API

### Priority 3: Check for Date Range Filtering
**Hypothesis**: GetComplaintsQueryHandler might be applying a default date range (e.g., last 7 days)
**Test**: Explicitly request different date ranges

### Priority 4: Check ComplainantId Override
**Hypothesis**: Admin's ComplainantId might be getting set even though isAdmin=true
**Test**: Add logging to see what filters are being applied

---

## Test Evidence

### Test 1: Database Query ✅
```bash
sqlcmd -S "LAPTOP-NF9BTG7Q\SQLEXPRESS" -d ComplaintManagementDB
Result: 37 active complaints, 1110 deleted
```

### Test 2: Statistics API ✅
```powershell
GET /api/dashboard/statistics (Admin Token)
Result: { totalComplaints: 37, activeComplaints: 36, completedComplaints: 1 }
```

### Test 3: Complaints API ❌
```powershell
GET /api/complaints?page=1&pageSize=200 (Admin Token)
Result: { totalCount: 5, items: [5 complaints from Nov 10] }
```

### Test 4: Token Validation ✅
```powershell
JWT Claims: ManageUsers=YES, ManageSettings=YES, ManageCompany=YES
```

---

## Conclusion

**Bug Confirmed**: Complaints API is incorrectly filtering out 32 complaints

**Root Cause**: Unknown - requires SQL logging and further investigation

**Severity**: 🔴 **P0 - CRITICAL**

**Blocked Tasks**:
- ✅ Statistics API testing (working correctly - no issue)
- ❌ Full complaint management testing (cannot access 86% of data)
- ❌ Admin functionality validation
- ❌ Production deployment

**Recommendation**: **DO NOT DEPLOY** until Complaints API bug is resolved

---

**Investigation Status**: **ONGOING**
**Next Action**: Enable EF Core SQL logging to capture exact queries
**Estimated Time to Resolution**: 30-60 minutes once SQL logs are reviewed

