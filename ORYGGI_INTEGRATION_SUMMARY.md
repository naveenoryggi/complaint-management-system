# Oryggi HRMS Integration Summary

## Overview

The Complaint Management System integrates with the Oryggi HRMS database using a **dual-table architecture** that separates organizational master data (synced from Oryggi) from complaint-specific role management.

---

## Architecture Approach

### 1. Master Data Sync from Oryggi (Read-Only)

**Oryggi as Source of Truth**:
- Employee, Company, Branch, Department, Section data **synced** from Oryggi
- Complaint system maintains **local read-only copies**
- Any changes in Oryggi **automatically reflect** in Complaint System
- Foreign key mappings preserve relationships

**Synced Tables**:

| Complaint Table | Oryggi Source | Mapping Field |
|----------------|---------------|---------------|
| `companies` | `CompanyMaster` | `oryggi_company_id` → `Ccode` |
| `branches` | `BranchMaster` | `oryggi_branch_id` → `BranchCode` |
| `departments` | `DeptMaster` | `oryggi_dept_id` → `Dcode` |
| `sections` | `SectionMaster` | `oryggi_section_id` → `SecCode` |
| `users` | `EmployeeMaster` | `oryggi_employee_id` → `Ecode` |

### 2. Complaint-Specific Role Management (Independent)

**Managed Independently**:
- Complaint roles and permissions **created within** complaint system
- Role tables **reference** synced Oryggi users
- Administrators assign complaint roles **without touching Oryggi**
- Branch/Department/Section-wise role scoping

**Complaint-Specific Tables**:

| Table | Purpose | References |
|-------|---------|------------|
| `complaint_roles` | Define complaint system roles | None (independent) |
| `user_complaint_roles` | Map users to roles with org scope | `users.user_id` (synced from Oryggi) |
| `complaint_role_permissions` | Define role permissions | `complaint_roles.role_id` |

---

## Data Flow

```
┌─────────────────────────────────────┐
│   Oryggi Database (SQL Server)      │
│   - EmployeeMaster                  │
│   - CompanyMaster                   │
│   - BranchMaster                    │
│   - DeptMaster                      │
│   - SectionMaster                   │
└──────────────┬──────────────────────┘
               │
               │ Real-time Webhooks
               │ or Scheduled Sync
               ↓
┌─────────────────────────────────────┐
│  Complaint System Master Tables     │
│  (Read-Only Synced Data)            │
│  - companies (oryggi_company_id)    │
│  - branches (oryggi_branch_id)      │
│  - departments (oryggi_dept_id)     │
│  - sections (oryggi_section_id)     │
│  - users (oryggi_employee_id)       │
└──────────────┬──────────────────────┘
               │
               │ References via
               │ user_id, branch_id, etc.
               ↓
┌─────────────────────────────────────┐
│  Complaint Role Tables              │
│  (Fully Managed in Complaint System)│
│  - complaint_roles                  │
│  - user_complaint_roles             │
│  - complaint_role_permissions       │
└─────────────────────────────────────┘
```

---

## Key Benefits

### ✅ Benefits of This Approach

1. **No Data Duplication Conflicts**
   - Oryggi remains single source of truth
   - Changes in Oryggi automatically sync
   - No manual data entry in complaint system

2. **Independent Complaint Role Management**
   - Complaint roles managed separately
   - No impact on Oryggi database
   - Flexible permission system

3. **Organizational Scope Support**
   - Branch-wise role assignments
   - Department-wise role assignments
   - Section-wise role assignments
   - Company-wide role assignments

4. **Automatic Data Consistency**
   - Employee transfers reflected automatically
   - Deactivated employees handled gracefully
   - Org structure changes synced in real-time

---

## Complaint Role Types

### System Roles (Pre-defined)

| Role Code | Role Name | Scope Level | Description |
|-----------|-----------|-------------|-------------|
| `SYS_ADMIN` | System Administrator | GLOBAL | Full system access |
| `HR_ADMIN` | HR Administrator | COMPANY | Manage complaint configuration |
| `HR_MANAGER` | HR Manager | BRANCH | Handle escalated complaints |
| `ESCALATION_HANDLER` | Escalation Handler | BRANCH | Handle escalations |
| `DEPT_HEAD` | Department Head | DEPARTMENT | Review department complaints |
| `SECTION_SUPERVISOR` | Section Supervisor | SECTION | Review section complaints |
| `EMPLOYEE` | Employee | GLOBAL | Create and view own complaints |

### Custom Roles (Can be added by administrators)

Administrators can create custom roles like:
- Regional HR Coordinator
- Complaint Auditor
- Quality Assurance Reviewer
- Branch Compliance Officer

---

## Sync Mechanisms

### Option 1: Real-time Webhooks (Recommended)

Oryggi triggers webhooks when data changes:
- `employee/created` - New employee added
- `employee/updated` - Employee data changed
- `employee/terminated` - Employee deactivated
- `organization/structure-changed` - Org structure modified

**Implementation**:
```typescript
@Post('webhooks/oryggi/employee/updated')
async handleEmployeeUpdated(@Body() payload) {
  await this.syncService.updateEmployee(payload.employee_id, payload.changes);

  // Re-evaluate complaint assignments if org changed
  if (payload.changes.department_id || payload.changes.branch_id) {
    await this.complaintService.reevaluateAssignments(payload.employee_id);
  }
}
```

### Option 2: Scheduled Batch Sync (Fallback)

Periodic sync every 6 hours:
- Query Oryggi database directly
- Compare and update changed records
- Log sync results

**Implementation**:
```typescript
@Cron('0 */6 * * *') // Every 6 hours
async syncMasterData() {
  await this.syncCompanies();
  await this.syncBranches();
  await this.syncDepartments();
  await this.syncSections();
  await this.syncEmployees();
}
```

---

## Role Assignment Examples

### Example 1: Assign Branch HR Manager

```typescript
// Find synced Oryggi user
const user = await usersService.findByEmployeeCode('EMP001');

// Find synced branch
const branch = await branchesService.findByCode('MUM001');

// Assign complaint role
await complaintRoleService.assignRole({
  user_id: user.user_id,
  role_code: 'HR_MANAGER',
  branch_id: branch.branch_id
});
```

### Example 2: Assign Department Head

```typescript
const user = await usersService.findByEmployeeCode('EMP002');
const dept = await departmentsService.findByCode('IT001');

await complaintRoleService.assignRole({
  user_id: user.user_id,
  role_code: 'DEPT_HEAD',
  department_id: dept.department_id
});
```

### Example 3: Assign Multi-Branch Escalation Handler

```typescript
const user = await usersService.findByEmployeeCode('EMP003');
const branches = ['MUM001', 'DEL001', 'BLR001'];

for (const branchCode of branches) {
  const branch = await branchesService.findByCode(branchCode);

  await complaintRoleService.assignRole({
    user_id: user.user_id,
    role_code: 'ESCALATION_HANDLER',
    branch_id: branch.branch_id
  });
}
```

---

## Handling Oryggi Changes

### Employee Transfer Scenario

**Oryggi Action**: Employee transferred from IT to HR department

**Complaint System Response**:
1. Sync service updates `users.department_id` automatically
2. Check if user has department-scoped complaint roles
3. Notify admin to review role assignments
4. Re-evaluate assigned complaints
5. Optionally auto-reassign if configured

```typescript
async handleEmployeeTransfer(employeeId: string) {
  // 1. Update synced data (automatic)
  const user = await this.db.users.findOne({
    where: { oryggi_employee_id: employeeId }
  });

  // 2. Check department-scoped roles
  const deptRoles = await this.db.user_complaint_roles.findAll({
    where: { user_id: user.user_id, department_id: { [Op.not]: null } }
  });

  // 3. Notify admin if roles affected
  if (deptRoles.length > 0) {
    await this.notificationService.notifyAdmin({
      message: `Employee transferred. Review complaint roles.`,
      affected_roles: deptRoles.length
    });
  }

  // 4. Re-evaluate complaints
  await this.complaintService.reevaluateAssignedComplaints(user.user_id);
}
```

### Employee Deactivation Scenario

**Oryggi Action**: Employee marked as inactive (left company)

**Complaint System Response**:
1. Sync service sets `users.is_active = false`
2. Deactivate all complaint roles for user
3. Reassign all active complaints to backup handlers
4. Send notification to affected stakeholders

```typescript
async handleEmployeeDeactivation(employeeId: string) {
  const user = await this.db.users.findOne({
    where: { oryggi_employee_id: employeeId }
  });

  // 1. Deactivate user (synced)
  await this.db.users.update(
    { is_active: false },
    { where: { user_id: user.user_id } }
  );

  // 2. Deactivate all complaint roles
  await this.db.user_complaint_roles.update(
    { is_active: false },
    { where: { user_id: user.user_id } }
  );

  // 3. Reassign active complaints
  await this.complaintService.reassignComplaintsFromDeactivatedUser(user.user_id);
}
```

---

## Permission Checking

### Permission Check Flow

```typescript
async checkPermission(
  userId: string,
  module: string,
  resource: string,
  action: string,
  context?: { branchId?: string; departmentId?: string }
): Promise<boolean> {
  // 1. Get user's complaint roles
  const userRoles = await this.getUserComplaintRoles(userId);

  // 2. Check each role
  for (const userRole of userRoles) {
    // 3. Verify scope match
    if (context?.branchId && userRole.branch_id !== context.branchId) {
      continue; // Wrong branch
    }

    if (context?.departmentId && userRole.department_id !== context.departmentId) {
      continue; // Wrong department
    }

    // 4. Check permission
    const hasPermission = await this.db.complaint_role_permissions.findOne({
      where: {
        role_id: userRole.role_id,
        module,
        resource,
        action,
        is_allowed: true
      }
    });

    if (hasPermission) {
      return true; // Permission granted
    }
  }

  return false; // No permission found
}
```

### Usage Example

```typescript
// Check if current user can approve complaint in their department
const canApprove = await complaintRoleService.checkPermission(
  currentUser.user_id,
  'COMPLAINTS',      // module
  'complaint',       // resource
  'APPROVE',         // action
  { departmentId: complaint.department_id }  // context
);

if (!canApprove) {
  throw new ForbiddenException('You cannot approve complaints in this department');
}

await complaintService.approveComplaint(complaintId);
```

---

## Database Connection Configuration

### SQL Server Connection to Oryggi

```typescript
const ORYGGI_DB_CONFIG = {
  server: 'LAPTOP-NF9BTG7Q\\SQLEXPRESS',
  database: 'Oryggi',
  user: 'sa',
  password: process.env.ORYGGI_DB_PASSWORD, // From environment variable
  options: {
    encrypt: false,
    trustServerCertificate: true,
    enableArithAbort: true
  },
  pool: {
    max: 10,
    min: 0,
    idleTimeoutMillis: 30000
  }
};
```

### Recommended: Read-Only User for Complaint System

```sql
-- Create read-only user for complaint system sync
CREATE LOGIN complaint_sync_user WITH PASSWORD = 'SecurePassword123!';
CREATE USER complaint_sync_user FOR LOGIN complaint_sync_user;

-- Grant read-only access to required tables
GRANT SELECT ON EmployeeMaster TO complaint_sync_user;
GRANT SELECT ON CompanyMaster TO complaint_sync_user;
GRANT SELECT ON BranchMaster TO complaint_sync_user;
GRANT SELECT ON DeptMaster TO complaint_sync_user;
GRANT SELECT ON SectionMaster TO complaint_sync_user;
GRANT SELECT ON DesignationMaster TO complaint_sync_user;
GRANT SELECT ON GradeMaster TO complaint_sync_user;
GRANT SELECT ON CatMaster TO complaint_sync_user;
GRANT SELECT ON ReportingHeadMaster TO complaint_sync_user;
GRANT SELECT ON MultiReportingHeadRelation TO complaint_sync_user;
```

---

## Security Considerations

### 1. Database Access
- ✅ Use **read-only database user** for sync operations
- ✅ Store credentials in **environment variables**
- ✅ Use **connection pooling** to prevent connection exhaustion
- ✅ Implement **retry logic** with exponential backoff

### 2. Data Synchronization
- ✅ Log all sync operations for audit trail
- ✅ Implement **idempotent sync** (safe to re-run)
- ✅ Handle partial sync failures gracefully
- ✅ Alert admins on sync failures

### 3. Role Management
- ✅ Require **administrator approval** for role changes
- ✅ Maintain **audit log** of role assignments
- ✅ Support **time-bound role assignments** (valid_from/valid_to)
- ✅ Prevent **privilege escalation** (users can't assign higher roles)

---

## Admin Configuration UI

### Role Assignment Page

**Features**:
- Search synced Oryggi users
- Select complaint role
- Choose organizational scope (Company/Branch/Department/Section)
- Set validity period (optional)
- View current role assignments
- Deactivate/revoke roles

**Mockup Flow**:
```
1. Admin navigates to "Manage Complaint Roles"
2. Searches for employee by name/code (from synced Oryggi data)
3. Selects employee "John Doe (EMP001)"
4. Clicks "Assign Role"
5. Selects role: "HR Manager"
6. Selects scope: "Branch → Mumbai Office"
7. Sets validity: "2024-01-01 to 2024-12-31"
8. Saves assignment
9. System confirms: "Role assigned successfully"
```

---

## Summary

### ✅ What Gets Synced from Oryggi
- Employee master data (EmployeeMaster)
- Company structure (CompanyMaster)
- Branch data (BranchMaster)
- Department data (DeptMaster)
- Section data (SectionMaster)
- Reporting relationships (ReportingHeadMaster)
- Leave balances (for leave-related complaints)
- Salary structure (for salary-related complaints)

### ✅ What Is Managed in Complaint System
- Complaint-specific roles
- User → Role mappings with org scope
- Role permissions (CRUD, Approve, Escalate, etc.)
- Escalation matrices
- Email alert configurations
- Complaint categories and workflows
- SLA configurations

### ✅ Integration Points
1. **Data Sync**: Real-time webhooks or scheduled batch
2. **Role Reference**: Complaint roles reference synced users
3. **Org Scope**: Roles scoped to synced branches/departments/sections
4. **Change Handling**: Auto-handle employee transfers and deactivations
5. **Permission Check**: Context-aware permission validation

---

## Next Steps

1. **Set up database connection** to Oryggi (read-only user)
2. **Implement sync service** (webhooks or scheduled)
3. **Create complaint role tables** in complaint database
4. **Build role assignment UI** for administrators
5. **Test sync scenarios** (employee transfer, deactivation)
6. **Configure permissions** for each complaint role
7. **Deploy and monitor** sync operations

---

**Document Version**: 1.0
**Last Updated**: 2025-10-11
**Database**: Oryggi (SQL Server Express)
**Integration Type**: Dual-table architecture with role separation
