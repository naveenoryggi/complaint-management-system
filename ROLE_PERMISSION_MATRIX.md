# Role & Permission Management System

## Overview
This document defines the comprehensive Role-Based Access Control (RBAC) system for the Complaint Management Portal. It clearly outlines what each role can do and ensures proper access control across the application.

## Permission Types

### Current Permissions (PermissionType enum):
1. **ViewComplaints** - View complaints assigned or accessible to user
2. **CreateComplaint** - Create/submit new complaints
3. **EditComplaint** - Edit complaint details
4. **DeleteComplaint** - Delete complaints (soft delete)
5. **AssignComplaint** - Assign complaints to handlers
6. **EscalateComplaint** - Escalate complaints to higher level
7. **CloseComplaint** - Close/resolve complaints
8. **ReopenComplaint** - Reopen closed complaints
9. **AddComment** - Add comments to complaints
10. **ViewComments** - View comments on complaints
11. **AddAttachment** - Attach files to complaints
12. **ViewAttachments** - View attachments on complaints
13. **ManageCategories** - Create, edit, delete complaint categories
14. **ManageRoles** - Create, edit, assign roles
15. **ViewReports** - Access reports and analytics
16. **ManageSettings** - Manage system settings
17. **ManageUsers** - Create, edit, deactivate users
18. **ViewAuditLogs** - View system audit logs
19. **ViewEscalation** - View escalation matrices, policies, resource pools
20. **ManageEscalation** - Manage escalation configuration

---

## Role Definitions & Permission Matrix

### 1. System Administrator (SYSTEM_ADMIN)
**Description**: Full system access across all tenants and companies. Super user role.

**Permissions**: ALL PERMISSIONS
- Full access to all features
- Can manage tenants, companies, all users
- Can configure system-wide settings
- Can view all audit logs
- **Scope**: System-wide (all tenants/companies)

**Use Case**: IT administrators, system owners

---

### 2. Tenant Administrator (TENANT_ADMIN)
**Description**: Full access within tenant scope. Can manage multiple companies under the tenant.

**Permissions**:
- ViewComplaints, CreateComplaint, EditComplaint
- AssignComplaint, EscalateComplaint, CloseComplaint, ReopenComplaint
- AddComment, ViewComments, AddAttachment, ViewAttachments
- ManageCategories, ManageRoles, ManageSettings, ManageUsers
- ViewReports, ViewAuditLogs
- ViewEscalation, ManageEscalation

**Scope**: Tenant-level (can access all companies within tenant)

**Use Case**: Organizational administrators managing multiple companies

---

### 3. Company Administrator (COMPANY_ADMIN)
**Description**: Full access within company scope. Can manage all aspects of their company.

**Permissions**:
- ViewComplaints, CreateComplaint, EditComplaint
- AssignComplaint, EscalateComplaint, CloseComplaint, ReopenComplaint
- AddComment, ViewComments, AddAttachment, ViewAttachments
- ManageCategories, ManageUsers
- ViewReports
- ViewEscalation, ManageEscalation

**Scope**: Company-level (single company)

**Use Case**: Company HR heads, Company administrators

---

### 4. Department Manager (DEPT_MANAGER) / Department Head
**Description**: Manager responsible for a department. Can manage complaints within their department.

**Permissions**:
- ViewComplaints (department scope)
- EditComplaint, AssignComplaint, EscalateComplaint
- AddComment, ViewComments, ViewAttachments
- ViewReports (department scope)
- ViewEscalation

**Scope**: Department-level

**Use Case**: Department heads, functional managers

**Additional Features Needed**:
- Should be able to assign complaints to team members
- Can view team performance metrics
- Can approve/reject escalations from their department

---

### 5. Reporting Manager (REPORTING_MANAGER) - NEW ROLE NEEDED
**Description**: Direct manager of employees. Can view and manage complaints from their direct reports.

**Permissions**:
- ViewComplaints (team scope)
- EditComplaint, AssignComplaint
- EscalateComplaint (to higher management)
- AddComment, ViewComments, ViewAttachments
- ViewReports (team scope)

**Scope**: Team-level (direct reports only)

**Use Case**: Team leads, project managers, direct supervisors

**Hierarchical Relationship**: Reports to Department Manager

---

### 6. Primary Contact (PRIMARY_CONTACT) - NEW ROLE NEEDED
**Description**: Main point of contact for a branch/department. Receives and triages incoming complaints.

**Permissions**:
- ViewComplaints (branch/department scope)
- CreateComplaint (on behalf of others)
- EditComplaint, AssignComplaint
- EscalateComplaint
- CloseComplaint (after resolution)
- AddComment, ViewComments
- AddAttachment, ViewAttachments
- ViewEscalation

**Scope**: Branch or Department-level

**Use Case**: Branch HR representative, Department coordinator

**Responsibility**: First point of contact, complaint triage and routing

---

### 7. Secondary Contact (SECONDARY_CONTACT) - NEW ROLE NEEDED
**Description**: Backup contact when primary contact is unavailable.

**Permissions**: Same as Primary Contact
- ViewComplaints (branch/department scope)
- CreateComplaint (on behalf of others)
- EditComplaint, AssignComplaint
- AddComment, ViewComments, ViewAttachments
- ViewEscalation

**Scope**: Branch or Department-level

**Use Case**: Backup HR representative, alternate coordinator

---

### 8. Level 1-5 Handlers (LEVEL1_HANDLER to LEVEL5_HANDLER)
**Description**: Complaint handlers at different escalation levels.

**Permissions**:
- ViewComplaints (assigned to them)
- EditComplaint (status, notes)
- AssignComplaint (to peers at same level)
- EscalateComplaint (to next level)
- CloseComplaint (if resolved)
- ReopenComplaint
- AddComment, ViewComments
- AddAttachment, ViewAttachments

**Scope**: Assigned complaints only

**Escalation Hierarchy**:
- Level 1: First line support/handler
- Level 2: Senior handler
- Level 3: Specialist/Expert
- Level 4: Manager level
- Level 5: Senior Management/Executive level

---

### 9. HR Representative (HR_REP)
**Description**: HR department staff member with access to sensitive complaints.

**Permissions**:
- ViewComplaints (company-wide)
- EditComplaint
- AddComment, ViewComments, ViewAttachments
- ViewReports
- ViewAuditLogs (for compliance)

**Scope**: Company-level (read-mostly, focused on sensitive complaints)

**Special Access**: Can view sensitive categories (harassment, discrimination, etc.)

---

### 10. Complainant / Employee (COMPLAINANT)
**Description**: Regular employee who can submit and track their own complaints.

**Permissions**:
- ViewComplaints (own complaints only)
- CreateComplaint
- AddComment (own complaints)
- ViewComments (own complaints)
- AddAttachment (own complaints)
- ViewAttachments (own complaints)

**Scope**: Own complaints only

**Use Case**: All employees in the organization

**Features**:
- Can submit complaints anonymously (if enabled)
- Can track status of own complaints
- Gets notifications on complaint updates

---

### 11. Viewer (VIEWER)
**Description**: Read-only access for auditors, observers.

**Permissions**:
- ViewComplaints (based on scope assignment)
- ViewComments
- ViewAttachments

**Scope**: Configurable (company, department, or specific categories)

**Use Case**: Auditors, compliance officers, observers

---

## Hierarchical Escalation Flow

```
Employee (Complainant)
    ↓
Primary Contact (Triage & Route)
    ↓
Level 1 Handler (First Response)
    ↓
Level 2 Handler (Senior Handler)
    ↓
Reporting Manager (Team Lead)
    ↓
Level 3 Handler (Specialist)
    ↓
Department Manager (Department Head)
    ↓
Level 4 Handler (Manager Level)
    ↓
Level 5 Handler (Executive Level)
    ↓
Company Administrator
```

---

## Data Scope & Access Control

### Scope Levels:
1. **System-wide**: Access to all tenants and companies (System Admin)
2. **Tenant-wide**: Access to all companies within a tenant (Tenant Admin)
3. **Company-wide**: Access to all branches and departments in a company (Company Admin, HR Rep)
4. **Branch-wide**: Access to specific branch (Branch Admin, Primary/Secondary Contact)
5. **Department-wide**: Access to specific department (Department Manager, Primary/Secondary Contact)
6. **Team-wide**: Access to direct reports (Reporting Manager)
7. **Assignment-based**: Access to assigned complaints only (Handlers)
8. **Self-only**: Access to own complaints only (Complainant)

---

## Role Assignment Rules

### Multi-Role Assignment:
- Users can have multiple roles (e.g., Employee + Department Manager)
- Primary role determines default dashboard and permissions
- Permissions are additive (union of all role permissions)

### Role Hierarchy:
```
System Admin > Tenant Admin > Company Admin > Department Manager >
Reporting Manager > Primary Contact > Level 5 Handler > Level 4 Handler >
Level 3 Handler > Level 2 Handler > Level 1 Handler > Complainant > Viewer
```

### Role Expiry:
- Roles can have effective dates (EffectiveFrom, EffectiveTo)
- Expired roles are automatically deactivated
- Notifications sent before role expiry

---

## Implementation Requirements

### Backend:
1. ✅ Permission enum with all permission types
2. ✅ ComplaintRole entity with role definitions
3. ✅ ComplaintRolePermission mapping
4. ✅ UserComplaintRole for user-role assignment
5. ✅ HasPermission authorization attribute
6. ✅ Permission-based authorization handler
7. 🔄 Add missing roles to DbSeeder
8. 🔄 Update role permissions with new escalation permissions
9. ❌ Role Management API (CRUD operations)
10. ❌ Permission assignment API

### Frontend:
1. ❌ Role Management UI (Admin panel)
2. ❌ Permission matrix UI (visual permission grid)
3. ❌ User role assignment UI
4. ❌ Role-based menu rendering
5. ❌ Role-based feature toggling

### Database:
1. ✅ Roles table seeded
2. ✅ Role-Permission mapping seeded
3. 🔄 Add missing roles (Reporting Manager, Primary/Secondary Contacts)
4. ❌ Create database migration for role updates

---

## Recommended Next Steps

### Priority 1 (Current Session):
1. ✅ Add ViewEscalation and ManageEscalation permissions to enum
2. ✅ Update controllers to use new permission names
3. 🔄 Update DbSeeder to include new escalation permissions for all roles
4. ❌ Test escalation matrix/policy pages with updated permissions

### Priority 2 (Next Implementation):
1. Add missing roles (Reporting Manager, Primary Contact, Secondary Contact)
2. Create Role Management API endpoints
3. Create user-friendly Role & Permission Management UI
4. Implement role hierarchy and scope validation

### Priority 3 (Future Enhancement):
1. Dynamic role creation (custom roles)
2. Permission templates
3. Role delegation
4. Temporary role assignment
5. Role-based notifications and workflows

---

## Security Considerations

1. **Principle of Least Privilege**: Users get minimum permissions needed
2. **Separation of Duties**: Critical operations require multiple roles
3. **Audit Logging**: All role/permission changes are logged
4. **Role Review**: Periodic review of user role assignments
5. **Automatic Deactivation**: Inactive users lose access automatically
6. **Multi-Factor for Admin**: Admin roles require MFA (future)

---

Last Updated: 2025-10-16
Version: 1.0
