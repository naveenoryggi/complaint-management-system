# Oryggi Database Connection & Integration Guide

**Date**: 2025-10-12
**Current System**: .NET 8 + Angular 18 Complaint Management System
**Oryggi Database**: SQL Server Express (OryggiHRMS)
**Connection String**: Already configured in appsettings.json

---

## 1. CURRENT CONFIGURATION STATUS

### ✅ Already Configured

Your `appsettings.json` already has the Oryggi connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=ComplaintManagementDB;...",
    "OryggiConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=OryggiHRMS;User Id=sa;Password=admin@123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Location**: `complaint-system-dotnet\src\ComplaintManagement.API\appsettings.json`

### Database Details

- **Server**: LAPTOP-NF9BTG7Q\\SQLEXPRESS
- **Oryggi Database**: OryggiHRMS
- **Complaint Database**: ComplaintManagementDB
- **User**: sa
- **Password**: admin@123

---

## 2. ORYGGI TABLE MAPPINGS (From Your Architecture)

### Company/Branch/Department/Section Hierarchy

```
Company (CompanyMaster)
  └── Branch (BranchMaster)
      └── Department (DeptMaster)
          └── Section (SectionMaster)
              └── Employee (EmployeeMaster)
```

### Table Mapping Configuration

| Oryggi Table | Oryggi Key | Complaint Table | Complaint Key | Mapping Field |
|--------------|------------|-----------------|---------------|---------------|
| `CompanyMaster` | `Ccode` | `Companies` | `CompanyId` | `OryggiCompanyId` |
| `BranchMaster` | `BranchCode` | `Branches` | `BranchId` | `OryggiBranchId` |
| `DeptMaster` | `Dcode` | `Departments` | `DepartmentId` | `OryggiDeptId` |
| `SectionMaster` | `SecCode` | `Sections` | `SectionId` | `OryggiSectionId` |
| `EmployeeMaster` | `Ecode` | `Users` | `UserId` | `OryggiEmployeeId` |

### Field Mappings

#### CompanyMaster → Companies
```sql
Oryggi.Ccode → OryggiCompanyId (stored as string/int)
Oryggi.CName → Name
Oryggi.Address → Address
Oryggi.Email → Email
Oryggi.TelephoneNo → Phone
```

#### BranchMaster → Branches
```sql
Oryggi.BranchCode → OryggiBranchId
Oryggi.BranchName → Name
Oryggi.Location → Location
Oryggi.Ccode → CompanyId (FK reference)
```

#### DeptMaster → Departments
```sql
Oryggi.Dcode → OryggiDeptId
Oryggi.Dname → Name
Oryggi.BranchCode → BranchId (FK reference)
```

#### SectionMaster → Sections
```sql
Oryggi.SecCode → OryggiSectionId
Oryggi.SecName → Name
Oryggi.Dcode → DepartmentId (FK reference)
```

#### EmployeeMaster → Users
```sql
Oryggi.Ecode → OryggiEmployeeId
Oryggi.CorpEmpCode → EmployeeCode
Oryggi.E_mail → Email
Oryggi.Telephone1 → Phone
Oryggi.Telephone2 → PhoneSecondary
Oryggi.FName → FirstName
Oryggi.LName → LastName
Oryggi.EmpName → FullName
Oryggi.ReportingHeadEcode → ManagerId (FK reference)
Oryggi.SecCode → SectionId (FK reference)
Oryggi.DateofJoin → DateOfJoining
Oryggi.DateofBirth → DateOfBirth
Oryggi.Active → IsActive
```

---

## 3. WHAT YOU NEED TO DO

### Step 1: Update Oryggi Database Name (If Different)

**Check your actual Oryggi database name**:
```sql
-- Run this in SQL Server Management Studio
SELECT name FROM sys.databases WHERE name LIKE '%Oryggi%'
```

If your database name is **Oryggi** instead of **OryggiHRMS**, update `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "OryggiConnection": "Server=LAPTOP-NF9BTG7Q\\SQLEXPRESS;Database=Oryggi;User Id=sa;Password=admin@123;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Step 2: Verify Oryggi Tables Exist

Run these queries to check your Oryggi schema:

```sql
USE Oryggi  -- or OryggiHRMS

-- Check if tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN (
    'CompanyMaster',
    'BranchMaster',
    'DeptMaster',
    'SectionMaster',
    'EmployeeMaster'
)

-- Check CompanyMaster structure
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'CompanyMaster'

-- Check BranchMaster structure
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'BranchMaster'

-- Check DeptMaster structure
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'DeptMaster'

-- Check SectionMaster structure
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'SectionMaster'

-- Check EmployeeMaster structure
SELECT COLUMN_NAME, DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'EmployeeMaster'
```

### Step 3: Sample Data Check

Verify you have data in Oryggi:

```sql
-- Count records
SELECT 'Companies' as TableName, COUNT(*) as RecordCount FROM CompanyMaster
UNION ALL
SELECT 'Branches', COUNT(*) FROM BranchMaster
UNION ALL
SELECT 'Departments', COUNT(*) FROM DeptMaster
UNION ALL
SELECT 'Sections', COUNT(*) FROM SectionMaster
UNION ALL
SELECT 'Employees', COUNT(*) FROM EmployeeMaster
```

---

## 4. ORGANIZATIONAL HIERARCHY IN YOUR SYSTEM

Based on the architecture documents, here's what will be implemented:

### Current Complaint System Tables (Already Exist)

These tables already exist in `ComplaintManagementDB`:

1. **Tenants** - Multi-tenancy support
2. **Companies** - Has `OryggiCompanyId` field for mapping
3. **Branches** - Has `OryggiBranchId` field
4. **Departments** - Has `OryggiDeptId` field
5. **Sections** - Has `OryggiSectionId` field
6. **Users** - Has `OryggiEmployeeId` field

### How Sync Will Work

```
┌─────────────────────────────────────┐
│   ORYGGI DATABASE (Source)          │
│   Server: LAPTOP-NF9BTG7Q\SQLEXPRESS│
│   Database: Oryggi (or OryggiHRMS)  │
│                                     │
│   Tables:                           │
│   - CompanyMaster                   │
│   - BranchMaster                    │
│   - DeptMaster                      │
│   - SectionMaster                   │
│   - EmployeeMaster                  │
└──────────────┬──────────────────────┘
               │
               │ Sync Service
               │ (To be created)
               ↓
┌─────────────────────────────────────┐
│   COMPLAINT SYSTEM (Target)         │
│   Database: ComplaintManagementDB   │
│                                     │
│   Synced Tables (Read-Only):        │
│   - Companies                       │
│   - Branches                        │
│   - Departments                     │
│   - Sections                        │
│   - Users                           │
│                                     │
│   Complaint Tables (Managed):       │
│   - ComplaintRoles                  │
│   - UserComplaintRoles              │
│   - Complaints                      │
│   - EscalationMatrices              │
└─────────────────────────────────────┘
```

---

## 5. IMPLEMENTATION STEPS (What I Will Create)

### Backend Components to Create:

1. **OryggiDbContext.cs** - Separate EF Core context for Oryggi database
2. **Oryggi Entity Models** - Read-only models for Oryggi tables:
   - `CompanyMaster.cs`
   - `BranchMaster.cs`
   - `DeptMaster.cs`
   - `SectionMaster.cs`
   - `EmployeeMaster.cs`

3. **Sync Service** - `OryggiSyncService.cs`:
   - `SyncCompanies()` - Sync from CompanyMaster
   - `SyncBranches()` - Sync from BranchMaster
   - `SyncDepartments()` - Sync from DeptMaster
   - `SyncSections()` - Sync from SectionMaster
   - `SyncEmployees()` - Sync from EmployeeMaster

4. **Background Worker** - `OryggiSyncBackgroundService.cs`:
   - Scheduled sync every 6 hours
   - Manual sync API endpoint
   - Sync status logging

5. **API Controller** - `OryggiSyncController.cs`:
   - `POST /api/oryggi/sync/manual` - Trigger manual sync
   - `GET /api/oryggi/sync/status` - Check sync status
   - `GET /api/oryggi/sync/history` - View sync logs

### Frontend Components to Create:

6. **Oryggi Sync Dashboard** - `oryggi-sync.component.ts`:
   - View sync status
   - Trigger manual sync
   - View sync history logs
   - Display synced record counts

7. **Admin Panel Menu Update**:
   - Add "Oryggi Sync" menu item

---

## 6. SYNC STRATEGY

### Option A: Scheduled Batch Sync (Recommended to Start)

**Frequency**: Every 6 hours
**Method**: Full table sync with upsert logic

**Advantages**:
- Simple to implement
- No changes needed in Oryggi
- Complete data consistency

**Implementation**:
```csharp
[Scheduled(Cron = "0 */6 * * *")] // Every 6 hours
public async Task ScheduledSync()
{
    await SyncCompanies();
    await SyncBranches();
    await SyncDepartments();
    await SyncSections();
    await SyncEmployees();
}
```

### Option B: Real-time Webhooks (Future Enhancement)

**Method**: Oryggi triggers webhooks on data changes
**Requires**: Modifications to Oryggi system

---

## 7. DATA FLOW EXAMPLE

### Example: Syncing Employee with Full Hierarchy

```csharp
// 1. Fetch from Oryggi
var oryggEmployee = await _oryggiDb.EmployeeMaster
    .FirstOrDefaultAsync(e => e.Ecode == 12345);

// Employee belongs to: Section → Department → Branch → Company

// 2. Resolve Section
var oryggSection = await _oryggiDb.SectionMaster
    .FirstOrDefaultAsync(s => s.SecCode == oryggEmployee.SecCode);

// 3. Resolve Department
var oryggDept = await _oryggiDb.DeptMaster
    .FirstOrDefaultAsync(d => d.Dcode == oryggSection.Dcode);

// 4. Resolve Branch
var oryggBranch = await _oryggiDb.BranchMaster
    .FirstOrDefaultAsync(b => b.BranchCode == oryggDept.BranchCode);

// 5. Resolve Company
var oryggCompany = await _oryggiDb.CompanyMaster
    .FirstOrDefaultAsync(c => c.Ccode == oryggBranch.Ccode);

// 6. Sync to Complaint System (in order)
var company = await SyncCompany(oryggCompany);
var branch = await SyncBranch(oryggBranch, company.CompanyId);
var dept = await SyncDepartment(oryggDept, branch.BranchId);
var section = await SyncSection(oryggSection, dept.DepartmentId);
var user = await SyncEmployee(oryggEmployee, company, branch, dept, section);
```

---

## 8. NEXT STEPS - WHAT TO PROVIDE ME

To proceed with implementation, please provide:

### Required Information:

1. **Confirm Oryggi Database Name**:
   - Is it `Oryggi` or `OryggiHRMS`?
   - Run: `SELECT name FROM sys.databases WHERE name LIKE '%Oryggi%'`

2. **Oryggi Table Schema Details**:
   - Run the INFORMATION_SCHEMA queries from Step 2 above
   - Paste the results so I can confirm field names match

3. **Sample Data**:
   - Run the COUNT queries from Step 3
   - Confirm you have data in all 5 tables

4. **Field Name Confirmation**:
   - Are field names exactly as documented? (Ccode, CName, BranchCode, etc.)
   - Or do they differ? (e.g., CompanyCode instead of Ccode)

### Optional Information:

5. **Special Requirements**:
   - Should sync run automatically on startup?
   - Do you want to filter inactive employees? (Active = 1)
   - Any specific departments/branches to exclude?

6. **Reporting Hierarchy**:
   - Does `EmployeeMaster.ReportingHeadEcode` contain manager ID?
   - Should we sync multi-level reporting chains?

---

## 9. WHAT I WILL CREATE AFTER YOU CONFIRM

Once you provide the above information, I will:

1. ✅ Create **OryggiDbContext** with correct table/field mappings
2. ✅ Create **Oryggi entity models** (read-only)
3. ✅ Create **OryggiSyncService** with full sync logic
4. ✅ Create **Background sync worker** (runs every 6 hours)
5. ✅ Create **Sync API controller** (manual triggers)
6. ✅ Update **Program.cs** to register Oryggi connection
7. ✅ Create **Sync status logging** (track last sync, errors)
8. ✅ Create **Frontend sync dashboard** (Angular component)
9. ✅ Add **"Oryggi Sync" to admin menu**
10. ✅ Test **full sync workflow** with your data

---

## 10. TESTING PLAN

After implementation, we will test:

### Phase 1: Connection Test
- Verify Oryggi database connection
- List all companies, branches, departments, sections

### Phase 2: Single Record Sync
- Sync 1 company
- Sync 1 branch under that company
- Verify FK relationships

### Phase 3: Full Sync Test
- Sync all records from Oryggi
- Verify counts match
- Check data consistency

### Phase 4: Incremental Sync Test
- Run sync twice
- Verify no duplicates
- Check update detection

### Phase 5: Employee Sync Test
- Sync employees with full org hierarchy
- Verify manager relationships
- Test active/inactive filtering

---

## SUMMARY

**Current Status**:
- ✅ Oryggi connection string configured
- ✅ Complaint system tables have Oryggi mapping fields
- ✅ Architecture documented
- ❌ Sync service not implemented yet
- ❌ No data currently synced

**Next Actions**:
1. **You**: Confirm database name and run verification queries
2. **You**: Provide schema and sample data details
3. **Me**: Implement complete Oryggi sync system
4. **We**: Test and verify sync works correctly

**Expected Timeline**:
- Setup verification: 30 minutes (your queries)
- Implementation: 4-6 hours (my coding)
- Testing & debugging: 2-3 hours
- **Total**: 1 day for complete working sync

---

**Questions?** Ask me anything about the sync process, architecture, or next steps!
