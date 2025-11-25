# CHUNK 5: Oryggi HRMS Integration

**Part of**: Master Planning Document
**Module**: Oryggi Database Integration
**Status**: Dual-Table Architecture Implementation

---

## Overview

The Complaint Management System integrates with Oryggi HRMS using a **dual-table architecture**:
1. Master data (employees, org structure) synced from Oryggi (read-only)
2. Complaint-specific roles managed independently

---

## 5.1 Integration Architecture

### Dual-Table Design

```
┌─────────────────────────────────────┐
│   ORYGGI DATABASE (SQL Server)      │
│   Source of Truth                   │
│   - EmployeeMaster                  │
│   - CompanyMaster                   │
│   - BranchMaster                    │
│   - DeptMaster                      │
│   - SectionMaster                   │
│   - ReportingHeadMaster             │
│   - MultiReportingHeadRelation      │
│   - LeaveMaster, LeaveCard, etc.    │
└──────────────┬──────────────────────┘
               │
               │ Real-time Sync
               │ or Scheduled Batch
               ↓
┌─────────────────────────────────────┐
│   COMPLAINT SYSTEM (PostgreSQL)     │
│   Read-Only Master Tables           │
│   - companies (oryggi_company_id)   │
│   - branches (oryggi_branch_id)     │
│   - departments (oryggi_dept_id)    │
│   - sections (oryggi_section_id)    │
│   - users (oryggi_employee_id)      │
└──────────────┬──────────────────────┘
               │
               │ References
               ↓
┌─────────────────────────────────────┐
│   COMPLAINT SYSTEM                  │
│   Managed Tables                    │
│   - complaint_roles                 │
│   - user_complaint_roles            │
│   - complaints                      │
│   - escalation_matrices             │
└─────────────────────────────────────┘
```

---

## 5.2 Oryggi Schema Mapping

### Connection Configuration

```typescript
// SQL Server connection to Oryggi
const ORYGGI_DB_CONFIG = {
  server: 'LAPTOP-NF9BTG7Q\\SQLEXPRESS',
  database: 'Oryggi',
  user: 'complaint_sync_user', // Read-only user
  password: process.env.ORYGGI_DB_PASSWORD,
  options: {
    encrypt: false,
    trustServerCertificate: true,
    enableArithAbort: true,
    requestTimeout: 30000
  },
  pool: {
    max: 10,
    min: 0,
    idleTimeoutMillis: 30000
  }
};
```

### Table Mappings

```typescript
const ORYGGI_TABLE_MAPPINGS = {
  companies: {
    oryggi_table: 'CompanyMaster',
    primary_key: 'Ccode',
    fields: {
      oryggi_company_id: 'Ccode',
      name: 'CName',
      address: 'Address',
      email: 'Email',
      phone: 'TelephoneNo'
    }
  },

  branches: {
    oryggi_table: 'BranchMaster',
    primary_key: 'BranchCode',
    fields: {
      oryggi_branch_id: 'BranchCode',
      name: 'BranchName',
      location: 'Location',
      company_id: 'Ccode' // FK
    }
  },

  departments: {
    oryggi_table: 'DeptMaster',
    primary_key: 'Dcode',
    fields: {
      oryggi_dept_id: 'Dcode',
      name: 'Dname',
      branch_id: 'BranchCode' // FK
    }
  },

  sections: {
    oryggi_table: 'SectionMaster',
    primary_key: 'SecCode',
    fields: {
      oryggi_section_id: 'SecCode',
      name: 'SecName',
      department_id: 'Dcode' // FK
    }
  },

  users: {
    oryggi_table: 'EmployeeMaster',
    primary_key: 'Ecode',
    fields: {
      oryggi_employee_id: 'Ecode',
      employee_code: 'CorpEmpCode',
      email: 'E_mail',
      phone: 'Telephone1',
      phone_secondary: 'Telephone2',
      first_name: 'FName',
      last_name: 'LName',
      full_name: 'EmpName',
      manager_id: 'ReportingHeadEcode',
      oryggi_designation_id: 'DesCode',
      oryggi_grade_id: 'Gcode',
      oryggi_category_id: 'Catcode',
      oryggi_role: 'Role',
      date_of_joining: 'DateofJoin',
      date_of_birth: 'DateofBirth',
      is_active: 'Active'
    }
  }
};
```

---

## 5.3 Synchronization Methods

### Method 1: Real-time Webhooks (Recommended)

```typescript
@Controller('webhooks/oryggi')
export class OryggiWebhookController {

  @Post('employee/created')
  async handleEmployeeCreated(@Body() payload: OryggiWebhook) {
    const employeeData = await this.oryggiFetchService.getEmployeeById(
      payload.employee_id
    );

    await this.syncService.syncEmployee(employeeData);

    return { status: 'success', message: 'Employee synced' };
  }

  @Post('employee/updated')
  async handleEmployeeUpdated(@Body() payload: OryggiWebhook) {
    const { employee_id, changes } = payload;

    await this.syncService.updateEmployee(employee_id, changes);

    // Re-evaluate complaint assignments if org structure changed
    if (changes.department_id || changes.branch_id || changes.section_id) {
      await this.complaintService.reevaluateAssignments(employee_id);
    }

    // Notify admin if role-relevant changes
    if (changes.department_id) {
      await this.notificationService.notifyAdmin({
        type: 'EMPLOYEE_TRANSFER',
        message: `Employee ${employee_id} transferred. Review complaint roles.`,
        employee_id
      });
    }

    return { status: 'success' };
  }

  @Post('employee/terminated')
  async handleEmployeeTerminated(@Body() payload: OryggiWebhook) {
    await this.syncService.deactivateEmployee(payload.employee_id);

    // Deactivate complaint roles
    await this.roleService.deactivateUserRoles(payload.employee_id);

    // Reassign active complaints
    await this.complaintService.reassignFromTerminatedEmployee(
      payload.employee_id
    );

    return { status: 'success' };
  }

  @Post('organization/structure-changed')
  async handleOrgStructureChange(@Body() payload: OryggiWebhook) {
    // Sync all organizational tables
    await this.syncService.syncOrganizationStructure();

    return { status: 'success' };
  }
}
```

### Method 2: Scheduled Batch Sync (Fallback)

```typescript
@Injectable()
export class OryggiSyncService {

  @Cron('0 */6 * * *') // Every 6 hours
  async scheduledSync() {
    console.log('Starting Oryggi sync...');

    try {
      const syncResults = {
        companies: await this.syncCompanies(),
        branches: await this.syncBranches(),
        departments: await this.syncDepartments(),
        sections: await this.syncSections(),
        employees: await this.syncEmployees()
      };

      console.log('Oryggi sync completed:', syncResults);

      // Log sync results
      await this.db.sync_logs.create({
        sync_type: 'SCHEDULED_BATCH',
        results: syncResults,
        status: 'SUCCESS',
        synced_at: new Date()
      });

    } catch (error) {
      console.error('Oryggi sync failed:', error);

      // Alert admin
      await this.alertService.notifyAdmin({
        type: 'SYNC_FAILED',
        error: error.message,
        timestamp: new Date()
      });

      // Log failure
      await this.db.sync_logs.create({
        sync_type: 'SCHEDULED_BATCH',
        status: 'FAILED',
        error_message: error.message,
        synced_at: new Date()
      });
    }
  }

  private async syncEmployees(): Promise<SyncResult> {
    const oryggEmployees = await this.oryggDB.query(`
      SELECT
        Ecode, CorpEmpCode, E_mail, Telephone1, Telephone2,
        FName, LName, EmpName, ReportingHeadEcode,
        DesCode, Gcode, Catcode, Role, SecCode,
        DateofJoin, DateofBirth, Active
      FROM EmployeeMaster
      WHERE Active = 1
    `);

    let created = 0, updated = 0, skipped = 0;

    for (const emp of oryggEmployees) {
      // Find or create company, branch, dept, section first
      const orgMapping = await this.resolveOrganizationMapping(emp.SecCode);

      const [user, wasCreated] = await this.complaintDB.users.upsert({
        where: { oryggi_employee_id: emp.Ecode },
        update: {
          employee_code: emp.CorpEmpCode,
          email: emp.E_mail,
          phone: emp.Telephone1,
          phone_secondary: emp.Telephone2,
          first_name: emp.FName,
          last_name: emp.LName,
          full_name: emp.EmpName,
          company_id: orgMapping.company_id,
          branch_id: orgMapping.branch_id,
          department_id: orgMapping.department_id,
          section_id: orgMapping.section_id,
          oryggi_designation_id: emp.DesCode,
          oryggi_grade_id: emp.Gcode,
          oryggi_category_id: emp.Catcode,
          oryggi_role: emp.Role,
          date_of_joining: emp.DateofJoin,
          date_of_birth: emp.DateofBirth,
          is_active: emp.Active,
          last_synced_at: new Date()
        },
        create: {
          tenant_id: this.tenantId,
          oryggi_employee_id: emp.Ecode,
          employee_code: emp.CorpEmpCode,
          email: emp.E_mail,
          phone: emp.Telephone1,
          phone_secondary: emp.Telephone2,
          first_name: emp.FName,
          last_name: emp.LName,
          full_name: emp.EmpName,
          company_id: orgMapping.company_id,
          branch_id: orgMapping.branch_id,
          department_id: orgMapping.department_id,
          section_id: orgMapping.section_id,
          oryggi_designation_id: emp.DesCode,
          oryggi_grade_id: emp.Gcode,
          oryggi_category_id: emp.Catcode,
          oryggi_role: emp.Role,
          date_of_joining: emp.DateofJoin,
          date_of_birth: emp.DateofBirth,
          is_active: emp.Active,
          last_synced_at: new Date()
        }
      });

      wasCreated ? created++ : updated++;
    }

    return { created, updated, skipped, total: oryggEmployees.length };
  }

  private async resolveOrganizationMapping(sectionId: number) {
    // Get section
    const section = await this.oryggDB.query(
      'SELECT SecCode, Dcode FROM SectionMaster WHERE SecCode = ?',
      [sectionId]
    );

    // Get department
    const dept = await this.oryggDB.query(
      'SELECT Dcode, BranchCode FROM DeptMaster WHERE Dcode = ?',
      [section[0].Dcode]
    );

    // Get branch
    const branch = await this.oryggDB.query(
      'SELECT BranchCode, Ccode FROM BranchMaster WHERE BranchCode = ?',
      [dept[0].BranchCode]
    );

    // Get company
    const company = await this.complaintDB.companies.findOne({
      where: { oryggi_company_id: branch[0].Ccode }
    });

    const branchRecord = await this.complaintDB.branches.findOne({
      where: { oryggi_branch_id: dept[0].BranchCode }
    });

    const deptRecord = await this.complaintDB.departments.findOne({
      where: { oryggi_dept_id: section[0].Dcode }
    });

    const sectionRecord = await this.complaintDB.sections.findOne({
      where: { oryggi_section_id: sectionId }
    });

    return {
      company_id: company.company_id,
      branch_id: branchRecord.branch_id,
      department_id: deptRecord.department_id,
      section_id: sectionRecord.section_id
    };
  }
}
```

---

## 5.4 Sync Impact Handlers

### Employee Transfer Handler

```typescript
@Injectable()
export class SyncImpactService {

  async handleEmployeeTransfer(
    employeeId: string,
    oldDeptId: string,
    newDeptId: string
  ) {
    const user = await this.db.users.findOne({
      where: { oryggi_employee_id: employeeId }
    });

    // 1. Check department-scoped complaint roles
    const deptRoles = await this.db.user_complaint_roles.findAll({
      where: {
        user_id: user.user_id,
        department_id: oldDeptId,
        is_active: true
      }
    });

    if (deptRoles.length > 0) {
      // 2. Notify admin about role review needed
      await this.notificationService.notifyAdmin({
        type: 'ROLE_REVIEW_REQUIRED',
        subject: 'Employee Transfer - Role Review Needed',
        message: `Employee ${user.full_name} (${user.employee_code}) transferred from Department ${oldDeptId} to ${newDeptId}.
                  ${deptRoles.length} complaint role(s) need review.`,
        user_id: user.user_id,
        roles_affected: deptRoles.map(r => r.role_id)
      });

      // 3. Auto-deactivate old department roles (optional)
      // await this.db.user_complaint_roles.update(
      //   { is_active: false },
      //   { where: { user_role_id: { [Op.in]: deptRoles.map(r => r.user_role_id) } } }
      // );
    }

    // 4. Re-evaluate assigned complaints
    const assignedComplaints = await this.db.complaints.findAll({
      where: {
        assigned_to_user_id: user.user_id,
        status: { [Op.in]: ['ASSIGNED', 'IN_PROGRESS', 'ESCALATED'] }
      }
    });

    for (const complaint of assignedComplaints) {
      // Check if user still has permission to handle complaint
      const hasPermission = await this.roleService.checkPermission(
        user.user_id,
        'COMPLAINTS',
        'complaint',
        'UPDATE',
        { complaintId: complaint.complaint_id, departmentId: complaint.department_id }
      );

      if (!hasPermission) {
        // Reassign complaint
        await this.complaintService.reassignComplaint(
          complaint.complaint_id,
          'EMPLOYEE_TRANSFER'
        );
      }
    }
  }

  async handleEmployeeDeactivation(employeeId: string) {
    const user = await this.db.users.findOne({
      where: { oryggi_employee_id: employeeId }
    });

    // 1. Deactivate user (synced automatically)
    await this.db.users.update(
      { is_active: false },
      { where: { user_id: user.user_id } }
    );

    // 2. Deactivate all complaint roles
    await this.db.user_complaint_roles.update(
      { is_active: false },
      { where: { user_id: user.user_id } }
    );

    // 3. Get active complaints
    const activeComplaints = await this.db.complaints.findAll({
      where: {
        [Op.or]: [
          { assigned_to_user_id: user.user_id },
          { created_by_user_id: user.user_id }
        ],
        status: { [Op.notIn]: ['CLOSED', 'CANCELLED'] }
      }
    });

    // 4. Reassign complaints
    for (const complaint of activeComplaints) {
      if (complaint.assigned_to_user_id === user.user_id) {
        // Find new assignee based on escalation matrix
        const newAssignee = await this.escalationService.findNextAssignee(
          complaint,
          'HANDLER_DEACTIVATED'
        );

        await this.complaintService.reassignComplaint(
          complaint.complaint_id,
          'EMPLOYEE_TERMINATED',
          newAssignee
        );

        // Add system comment
        await this.db.complaint_comments.create({
          complaint_id: complaint.complaint_id,
          user_id: user.user_id,
          comment_text: `Complaint reassigned due to employee termination`,
          comment_type: 'SYSTEM',
          is_system_generated: true
        });
      }

      if (complaint.created_by_user_id === user.user_id &&
          complaint.status === 'OPEN') {
        // Notify manager about orphaned complaint
        if (user.manager_id) {
          await this.notificationService.notifyUser(user.manager_id, {
            type: 'ORPHANED_COMPLAINT',
            complaint_id: complaint.complaint_id,
            message: `Employee ${user.full_name} who created this complaint has been terminated`
          });
        }
      }
    }

    // 5. Send deactivation summary
    await this.notificationService.notifyAdmin({
      type: 'EMPLOYEE_DEACTIVATED',
      user_id: user.user_id,
      employee_name: user.full_name,
      complaints_reassigned: activeComplaints.filter(c => c.assigned_to_user_id === user.user_id).length,
      roles_deactivated: await this.db.user_complaint_roles.count({
        where: { user_id: user.user_id }
      })
    });
  }
}
```

---

## 5.5 Complaint Role Management (Independent)

### Role Assignment Service

```typescript
@Injectable()
export class ComplaintRoleService {

  // Assign role to synced Oryggi user
  async assignRole(dto: AssignRoleDto): Promise<UserComplaintRole> {
    // Validate user exists (synced from Oryggi)
    const user = await this.db.users.findByPk(dto.user_id);
    if (!user) {
      throw new NotFoundException('User not found');
    }

    // Validate role
    const role = await this.db.complaint_roles.findByPk(dto.role_id);
    if (!role) {
      throw new NotFoundException('Role not found');
    }

    // Validate scope matches role scope_level
    this.validateScope(role, dto);

    // Check for existing assignment
    const existing = await this.db.user_complaint_roles.findOne({
      where: {
        user_id: dto.user_id,
        role_id: dto.role_id,
        company_id: dto.company_id || null,
        branch_id: dto.branch_id || null,
        department_id: dto.department_id || null,
        section_id: dto.section_id || null
      }
    });

    if (existing) {
      throw new ConflictException('Role already assigned with this scope');
    }

    // Create role assignment
    const userRole = await this.db.user_complaint_roles.create({
      tenant_id: user.tenant_id,
      user_id: dto.user_id,
      role_id: dto.role_id,
      company_id: dto.company_id,
      branch_id: dto.branch_id,
      department_id: dto.department_id,
      section_id: dto.section_id,
      valid_from: dto.valid_from,
      valid_to: dto.valid_to,
      assigned_by: dto.assigned_by
    });

    // Audit log
    await this.auditService.log({
      action: 'ROLE_ASSIGNED',
      user_id: dto.user_id,
      role_id: dto.role_id,
      assigned_by: dto.assigned_by,
      scope: {
        company: dto.company_id,
        branch: dto.branch_id,
        department: dto.department_id,
        section: dto.section_id
      }
    });

    return userRole;
  }

  private validateScope(role: ComplaintRole, dto: AssignRoleDto) {
    switch (role.scope_level) {
      case 'GLOBAL':
        // No scope restrictions
        break;
      case 'COMPANY':
        if (!dto.company_id) {
          throw new BadRequestException('Company scope required for this role');
        }
        break;
      case 'BRANCH':
        if (!dto.branch_id) {
          throw new BadRequestException('Branch scope required for this role');
        }
        break;
      case 'DEPARTMENT':
        if (!dto.department_id) {
          throw new BadRequestException('Department scope required for this role');
        }
        break;
      case 'SECTION':
        if (!dto.section_id) {
          throw new BadRequestException('Section scope required for this role');
        }
        break;
    }
  }

  // Get user's effective roles
  async getUserRoles(userId: string): Promise<UserComplaintRole[]> {
    const now = new Date();

    return await this.db.user_complaint_roles.findAll({
      where: {
        user_id: userId,
        is_active: true,
        [Op.or]: [
          {
            valid_from: { [Op.lte]: now },
            valid_to: { [Op.gte]: now }
          },
          {
            valid_from: null,
            valid_to: null
          }
        ]
      },
      include: [
        'role',
        'company',
        'branch',
        'department',
        'section'
      ]
    });
  }
}
```

---

## 5.6 Data Consistency Checks

```typescript
@Injectable()
export class DataConsistencyService {

  @Cron('0 2 * * *') // Daily at 2 AM
  async runConsistencyChecks() {
    const issues = [];

    // 1. Check for orphaned user references
    const orphanedUsers = await this.db.users.findAll({
      where: {
        [Op.or]: [
          { company_id: null },
          { branch_id: null }
        ],
        is_active: true
      }
    });

    if (orphanedUsers.length > 0) {
      issues.push({
        type: 'ORPHANED_USERS',
        count: orphanedUsers.length,
        user_ids: orphanedUsers.map(u => u.user_id)
      });
    }

    // 2. Check for invalid manager references
    const invalidManagers = await this.db.users.findAll({
      where: {
        manager_id: { [Op.not]: null }
      },
      include: [{
        model: this.db.users,
        as: 'manager',
        required: false
      }]
    });

    const broken = invalidManagers.filter(u => !u.manager);
    if (broken.length > 0) {
      issues.push({
        type: 'INVALID_MANAGER_REFS',
        count: broken.length,
        user_ids: broken.map(u => u.user_id)
      });
    }

    // 3. Check for stale sync data
    const staleUsers = await this.db.users.findAll({
      where: {
        last_synced_at: {
          [Op.lt]: new Date(Date.now() - 24 * 60 * 60 * 1000) // 24 hours ago
        },
        is_active: true
      }
    });

    if (staleUsers.length > 0) {
      issues.push({
        type: 'STALE_SYNC_DATA',
        count: staleUsers.length,
        oldest_sync: Math.min(...staleUsers.map(u => u.last_synced_at))
      });
    }

    // 4. Report issues
    if (issues.length > 0) {
      await this.alertService.notifyAdmin({
        type: 'DATA_CONSISTENCY_ISSUES',
        issues,
        timestamp: new Date()
      });
    }

    return issues;
  }
}
```

---

## Summary

**Chunk 5: Oryggi Integration Features**:
1. ✅ Dual-table architecture (read-only sync + independent roles)
2. ✅ Real-time webhook integration
3. ✅ Scheduled batch sync (fallback)
4. ✅ Employee transfer impact handling
5. ✅ Employee deactivation handling
6. ✅ Complaint role management (independent from Oryggi)
7. ✅ Data consistency checks
8. ✅ Sync audit logging

**Key Benefits**:
- Oryggi remains single source of truth
- Zero impact on Oryggi database
- Automatic sync of organizational changes
- Independent complaint role management
- Graceful handling of employee lifecycle events

---

**Next**: [Chunk 6 - Services & API Architecture →](CHUNK_06_SERVICES_APIS.md)
