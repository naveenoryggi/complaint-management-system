# CHUNK 3: Complaint & Role Tables

**Part of**: Master Planning Document
**Module**: Database Schema
**Status**: Active Tables (Managed in Complaint System)

---

## Overview

These tables are **managed within the Complaint Management System** and are independent from Oryggi. They handle complaint data, user roles, and permissions specific to the complaint module.

---

## 3.1 Complaint Categories

```sql
CREATE TABLE complaint_categories (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    category_code VARCHAR(50) UNIQUE NOT NULL,
    category_name VARCHAR(100) NOT NULL,
    description TEXT,

    -- ATTENDANCE, SALARY, LEAVE, GENERAL_HRMS, SYSTEM_ACCESS, etc.
    category_type VARCHAR(50) NOT NULL,

    -- Icon and color for UI
    icon VARCHAR(50),
    color VARCHAR(20),

    is_active BOOLEAN DEFAULT true,
    display_order INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_category_type CHECK (category_type IN (
        'ATTENDANCE', 'SALARY', 'LEAVE', 'GENERAL_HRMS',
        'SYSTEM_ACCESS', 'PERFORMANCE', 'BENEFITS', 'OTHER'
    ))
);

CREATE INDEX idx_complaint_categories_tenant ON complaint_categories(tenant_id);
CREATE INDEX idx_complaint_categories_code ON complaint_categories(category_code);
CREATE INDEX idx_complaint_categories_type ON complaint_categories(category_type);
CREATE INDEX idx_complaint_categories_active ON complaint_categories(is_active);
```

### Pre-populated Categories

```sql
INSERT INTO complaint_categories (tenant_id, category_code, category_name, description, category_type, icon, color) VALUES
('{tenant_id}', 'ATT_MISS', 'Attendance Missing', 'Attendance not marked or missing punch', 'ATTENDANCE', 'calendar-x', '#f44336'),
('{tenant_id}', 'ATT_WRONG', 'Attendance Incorrect', 'Wrong attendance marked', 'ATTENDANCE', 'calendar-alert', '#ff9800'),
('{tenant_id}', 'SAL_SHORT', 'Salary Short Payment', 'Salary amount is less than expected', 'SALARY', 'currency-inr', '#e91e63'),
('{tenant_id}', 'SAL_DELAY', 'Salary Payment Delay', 'Salary not received on time', 'SALARY', 'clock-alert', '#9c27b0'),
('{tenant_id}', 'LV_REJECT', 'Leave Not Approved', 'Leave application not approved', 'LEAVE', 'account-cancel', '#673ab7'),
('{tenant_id}', 'LV_BALANCE', 'Leave Balance Issue', 'Leave balance showing incorrect', 'LEAVE', 'counter', '#3f51b5'),
('{tenant_id}', 'SYS_ACCESS', 'System Access Issue', 'Unable to login or access system', 'SYSTEM_ACCESS', 'lock-alert', '#2196f3'),
('{tenant_id}', 'GEN_OTHER', 'Other HRMS Issue', 'General HRMS related complaint', 'OTHER', 'help-circle', '#607d8b');
```

---

## 3.2 Complaints (Core Table)

```sql
CREATE TABLE complaints (
    complaint_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    complaint_number VARCHAR(50) UNIQUE NOT NULL, -- Auto-generated: CMP-2025-000001

    -- Who created the complaint
    created_by_user_id UUID NOT NULL REFERENCES users(user_id),

    -- Organizational context
    company_id UUID NOT NULL REFERENCES companies(company_id),
    branch_id UUID REFERENCES branches(branch_id),
    department_id UUID REFERENCES departments(department_id),
    section_id UUID REFERENCES sections(section_id),

    -- Complaint details
    category_id UUID NOT NULL REFERENCES complaint_categories(category_id),
    subject VARCHAR(255) NOT NULL,
    description TEXT NOT NULL,
    priority VARCHAR(20) NOT NULL DEFAULT 'MEDIUM', -- LOW, MEDIUM, HIGH, CRITICAL

    -- Current status
    status VARCHAR(50) NOT NULL DEFAULT 'OPEN',
    -- Status values: OPEN, ASSIGNED, IN_PROGRESS, ESCALATED, RESOLVED, CLOSED, CANCELLED

    -- Current assignment
    assigned_to_user_id UUID REFERENCES users(user_id),
    assigned_at TIMESTAMP,

    -- Current escalation level
    current_escalation_level INT DEFAULT 0,
    escalation_matrix_id UUID, -- References escalation_matrices table

    -- SLA tracking
    sla_due_date TIMESTAMP,
    sla_breached BOOLEAN DEFAULT false,
    sla_breach_count INT DEFAULT 0,

    -- Resolution
    resolved_at TIMESTAMP,
    resolved_by_user_id UUID REFERENCES users(user_id),
    resolution_summary TEXT,
    resolution_category VARCHAR(50), -- FIXED, WORKAROUND, DUPLICATE, WONT_FIX, etc.

    -- Closure
    closed_at TIMESTAMP,
    closed_by_user_id UUID REFERENCES users(user_id),

    -- Feedback
    satisfaction_rating INT, -- 1 to 5
    feedback_comments TEXT,

    -- Metadata
    tags TEXT[],
    metadata JSONB DEFAULT '{}',

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_priority CHECK (priority IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')),
    CONSTRAINT chk_status CHECK (status IN (
        'OPEN', 'ASSIGNED', 'IN_PROGRESS', 'ESCALATED',
        'RESOLVED', 'CLOSED', 'CANCELLED'
    )),
    CONSTRAINT chk_rating CHECK (satisfaction_rating BETWEEN 1 AND 5),
    CONSTRAINT chk_resolution_category CHECK (resolution_category IN (
        'FIXED', 'WORKAROUND', 'DUPLICATE', 'WONT_FIX',
        'CANNOT_REPRODUCE', 'BY_DESIGN'
    ))
);

CREATE INDEX idx_complaints_tenant ON complaints(tenant_id);
CREATE INDEX idx_complaints_number ON complaints(complaint_number);
CREATE INDEX idx_complaints_created_by ON complaints(created_by_user_id);
CREATE INDEX idx_complaints_assigned_to ON complaints(assigned_to_user_id);
CREATE INDEX idx_complaints_company ON complaints(company_id);
CREATE INDEX idx_complaints_branch ON complaints(branch_id);
CREATE INDEX idx_complaints_department ON complaints(department_id);
CREATE INDEX idx_complaints_section ON complaints(section_id);
CREATE INDEX idx_complaints_category ON complaints(category_id);
CREATE INDEX idx_complaints_status ON complaints(status);
CREATE INDEX idx_complaints_priority ON complaints(priority);
CREATE INDEX idx_complaints_sla_due ON complaints(sla_due_date);
CREATE INDEX idx_complaints_created_at ON complaints(created_at);
CREATE INDEX idx_complaints_escalation_level ON complaints(current_escalation_level);
```

### Complaint Number Generation

```typescript
// Auto-generate complaint number
async generateComplaintNumber(tenantId: string): Promise<string> {
  const year = new Date().getFullYear();
  const prefix = `CMP-${year}-`;

  const lastComplaint = await this.db.complaints.findOne({
    where: {
      tenant_id: tenantId,
      complaint_number: { [Op.like]: `${prefix}%` }
    },
    order: [['created_at', 'DESC']]
  });

  let sequence = 1;
  if (lastComplaint) {
    const lastNumber = parseInt(lastComplaint.complaint_number.split('-')[2]);
    sequence = lastNumber + 1;
  }

  return `${prefix}${sequence.toString().padStart(6, '0')}`;
  // Example: CMP-2025-000001
}
```

---

## 3.3 Complaint Comments/Activity

```sql
CREATE TABLE complaint_comments (
    comment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id) ON DELETE CASCADE,

    user_id UUID NOT NULL REFERENCES users(user_id),
    comment_text TEXT NOT NULL,
    comment_type VARCHAR(50) DEFAULT 'COMMENT',
    -- Types: COMMENT, STATUS_CHANGE, ESCALATION, ASSIGNMENT, RESOLUTION, SYSTEM

    is_internal BOOLEAN DEFAULT false, -- Internal notes not visible to employee
    is_system_generated BOOLEAN DEFAULT false,

    -- References for specific types
    mentioned_users UUID[], -- @mentions
    previous_value TEXT,    -- For status changes
    new_value TEXT,         -- For status changes

    metadata JSONB DEFAULT '{}',

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_comment_type CHECK (comment_type IN (
        'COMMENT', 'STATUS_CHANGE', 'ESCALATION', 'ASSIGNMENT',
        'RESOLUTION', 'SYSTEM', 'FEEDBACK'
    ))
);

CREATE INDEX idx_complaint_comments_complaint ON complaint_comments(complaint_id);
CREATE INDEX idx_complaint_comments_user ON complaint_comments(user_id);
CREATE INDEX idx_complaint_comments_type ON complaint_comments(comment_type);
CREATE INDEX idx_complaint_comments_created_at ON complaint_comments(created_at);
```

### Auto-Generated Comments

```typescript
// System-generated comment examples
async addSystemComment(complaintId: string, type: string, data: any) {
  switch (type) {
    case 'STATUS_CHANGE':
      await this.db.complaint_comments.create({
        complaint_id: complaintId,
        user_id: data.changed_by,
        comment_text: `Status changed from ${data.old_status} to ${data.new_status}`,
        comment_type: 'STATUS_CHANGE',
        is_system_generated: true,
        previous_value: data.old_status,
        new_value: data.new_status
      });
      break;

    case 'ESCALATION':
      await this.db.complaint_comments.create({
        complaint_id: complaintId,
        user_id: data.escalated_by,
        comment_text: `Complaint escalated to Level ${data.new_level}`,
        comment_type: 'ESCALATION',
        is_system_generated: true,
        new_value: data.new_level.toString()
      });
      break;

    case 'ASSIGNMENT':
      await this.db.complaint_comments.create({
        complaint_id: complaintId,
        user_id: data.assigned_by,
        comment_text: `Assigned to ${data.assignee_name}`,
        comment_type: 'ASSIGNMENT',
        is_system_generated: true,
        new_value: data.assignee_id
      });
      break;
  }
}
```

---

## 3.4 Complaint Attachments

```sql
CREATE TABLE complaint_attachments (
    attachment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id) ON DELETE CASCADE,

    uploaded_by_user_id UUID NOT NULL REFERENCES users(user_id),

    file_name VARCHAR(255) NOT NULL,
    file_path VARCHAR(500) NOT NULL,
    file_size_bytes BIGINT,
    mime_type VARCHAR(100),

    -- S3/Cloud storage details
    storage_bucket VARCHAR(255),
    storage_key VARCHAR(500),

    description TEXT,

    -- Virus scan status
    scan_status VARCHAR(20) DEFAULT 'PENDING', -- PENDING, CLEAN, INFECTED
    scan_date TIMESTAMP,

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_scan_status CHECK (scan_status IN ('PENDING', 'CLEAN', 'INFECTED', 'FAILED'))
);

CREATE INDEX idx_complaint_attachments_complaint ON complaint_attachments(complaint_id);
CREATE INDEX idx_complaint_attachments_uploaded_by ON complaint_attachments(uploaded_by_user_id);
CREATE INDEX idx_complaint_attachments_scan_status ON complaint_attachments(scan_status);
```

### File Upload Configuration

```typescript
// File upload constraints
const UPLOAD_CONFIG = {
  maxFileSize: 10 * 1024 * 1024, // 10 MB
  allowedMimeTypes: [
    'image/jpeg',
    'image/png',
    'image/gif',
    'application/pdf',
    'application/msword',
    'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
    'application/vnd.ms-excel',
    'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
  ],
  maxAttachmentsPerComplaint: 5
};

// Upload function
async uploadAttachment(complaintId: string, file: File, userId: string) {
  // Validate file
  if (file.size > UPLOAD_CONFIG.maxFileSize) {
    throw new Error('File size exceeds 10 MB limit');
  }

  if (!UPLOAD_CONFIG.allowedMimeTypes.includes(file.mimetype)) {
    throw new Error('File type not allowed');
  }

  // Check existing attachments count
  const count = await this.db.complaint_attachments.count({
    where: { complaint_id: complaintId }
  });

  if (count >= UPLOAD_CONFIG.maxAttachmentsPerComplaint) {
    throw new Error('Maximum 5 attachments allowed per complaint');
  }

  // Upload to S3
  const s3Key = `complaints/${complaintId}/${Date.now()}-${file.originalname}`;
  await this.s3Service.upload(s3Key, file.buffer);

  // Save to database
  return await this.db.complaint_attachments.create({
    complaint_id: complaintId,
    uploaded_by_user_id: userId,
    file_name: file.originalname,
    file_path: s3Key,
    file_size_bytes: file.size,
    mime_type: file.mimetype,
    storage_bucket: process.env.S3_BUCKET,
    storage_key: s3Key
  });
}
```

---

## 3.5 Complaint Roles (Complaint System Specific)

```sql
CREATE TABLE complaint_roles (
    role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    role_code VARCHAR(50) UNIQUE NOT NULL,
    role_name VARCHAR(100) NOT NULL,
    role_description TEXT,

    -- Role Type
    role_type VARCHAR(50) NOT NULL,
    -- Types: SYSTEM_ADMIN, HR_ADMIN, HR_MANAGER, ESCALATION_HANDLER,
    --        COMPLAINT_REVIEWER, BRANCH_COORDINATOR, DEPARTMENT_HEAD,
    --        SECTION_SUPERVISOR, EMPLOYEE

    -- Scope Level
    scope_level VARCHAR(50) NOT NULL,
    -- Levels: GLOBAL, COMPANY, BRANCH, DEPARTMENT, SECTION

    is_system_role BOOLEAN DEFAULT false,   -- Cannot be deleted if true
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_role_type CHECK (role_type IN (
        'SYSTEM_ADMIN', 'HR_ADMIN', 'HR_MANAGER', 'ESCALATION_HANDLER',
        'COMPLAINT_REVIEWER', 'BRANCH_COORDINATOR', 'DEPARTMENT_HEAD',
        'SECTION_SUPERVISOR', 'EMPLOYEE'
    )),
    CONSTRAINT chk_scope_level CHECK (scope_level IN (
        'GLOBAL', 'COMPANY', 'BRANCH', 'DEPARTMENT', 'SECTION'
    ))
);

CREATE INDEX idx_complaint_roles_tenant ON complaint_roles(tenant_id);
CREATE INDEX idx_complaint_roles_type ON complaint_roles(role_type);
CREATE INDEX idx_complaint_roles_code ON complaint_roles(role_code);
CREATE INDEX idx_complaint_roles_active ON complaint_roles(is_active);
```

### Pre-populated System Roles

```sql
INSERT INTO complaint_roles (tenant_id, role_code, role_name, role_description, role_type, scope_level, is_system_role) VALUES
('{tenant_id}', 'SYS_ADMIN', 'System Administrator', 'Full system access', 'SYSTEM_ADMIN', 'GLOBAL', true),
('{tenant_id}', 'HR_ADMIN', 'HR Administrator', 'Manage complaint system configuration', 'HR_ADMIN', 'COMPANY', true),
('{tenant_id}', 'HR_MANAGER', 'HR Manager', 'Handle escalated complaints', 'HR_MANAGER', 'BRANCH', true),
('{tenant_id}', 'ESCALATION_HANDLER', 'Escalation Handler', 'Handle escalations at specific level', 'ESCALATION_HANDLER', 'BRANCH', true),
('{tenant_id}', 'DEPT_HEAD', 'Department Head', 'Review department complaints', 'DEPARTMENT_HEAD', 'DEPARTMENT', true),
('{tenant_id}', 'SECTION_SUPERVISOR', 'Section Supervisor', 'Review section complaints', 'SECTION_SUPERVISOR', 'SECTION', true),
('{tenant_id}', 'EMPLOYEE', 'Employee', 'Can create and view own complaints', 'EMPLOYEE', 'GLOBAL', true);
```

---

## 3.6 User Complaint Roles (Maps Users to Roles)

```sql
CREATE TABLE user_complaint_roles (
    user_role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    role_id UUID NOT NULL REFERENCES complaint_roles(role_id) ON DELETE CASCADE,

    -- Scope Assignment (Optional - based on role scope_level)
    company_id UUID REFERENCES companies(company_id),
    branch_id UUID REFERENCES branches(branch_id),
    department_id UUID REFERENCES departments(department_id),
    section_id UUID REFERENCES sections(section_id),

    -- Validity Period (Optional)
    valid_from DATE,
    valid_to DATE,

    is_active BOOLEAN DEFAULT true,
    assigned_by UUID REFERENCES users(user_id),
    assigned_at TIMESTAMP DEFAULT NOW(),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    -- Prevent duplicate role assignments
    CONSTRAINT unique_user_role_scope UNIQUE(
        user_id, role_id, company_id, branch_id, department_id, section_id
    )
);

CREATE INDEX idx_user_complaint_roles_tenant ON user_complaint_roles(tenant_id);
CREATE INDEX idx_user_complaint_roles_user ON user_complaint_roles(user_id);
CREATE INDEX idx_user_complaint_roles_role ON user_complaint_roles(role_id);
CREATE INDEX idx_user_complaint_roles_company ON user_complaint_roles(company_id);
CREATE INDEX idx_user_complaint_roles_branch ON user_complaint_roles(branch_id);
CREATE INDEX idx_user_complaint_roles_department ON user_complaint_roles(department_id);
CREATE INDEX idx_user_complaint_roles_section ON user_complaint_roles(section_id);
CREATE INDEX idx_user_complaint_roles_active ON user_complaint_roles(is_active);
CREATE INDEX idx_user_complaint_roles_validity ON user_complaint_roles(valid_from, valid_to);
```

---

## 3.7 Complaint Role Permissions

```sql
CREATE TABLE complaint_role_permissions (
    permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    role_id UUID NOT NULL REFERENCES complaint_roles(role_id) ON DELETE CASCADE,

    -- Permission Categories
    module VARCHAR(50) NOT NULL,
    -- Modules: COMPLAINTS, ESCALATIONS, REPORTS, CONFIGURATION, USERS, ALERTS

    resource VARCHAR(100) NOT NULL,
    -- Resources: complaint, escalation_matrix, email_template, user_roles, reports, etc.

    action VARCHAR(50) NOT NULL,
    -- Actions: CREATE, READ, UPDATE, DELETE, APPROVE, REJECT, ESCALATE, ASSIGN, EXPORT

    -- Conditional Permissions
    conditions JSONB DEFAULT '{}',
    -- Example: {"own_only": true, "department_only": true, "status": ["OPEN", "IN_PROGRESS"]}

    is_allowed BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_role_permission UNIQUE(role_id, module, resource, action),
    CONSTRAINT chk_permission_module CHECK (module IN (
        'COMPLAINTS', 'ESCALATIONS', 'REPORTS', 'CONFIGURATION', 'USERS', 'ALERTS'
    )),
    CONSTRAINT chk_permission_action CHECK (action IN (
        'CREATE', 'READ', 'UPDATE', 'DELETE', 'APPROVE', 'REJECT',
        'ESCALATE', 'ASSIGN', 'EXPORT', 'CONFIGURE'
    ))
);

CREATE INDEX idx_complaint_role_perms_tenant ON complaint_role_permissions(tenant_id);
CREATE INDEX idx_complaint_role_perms_role ON complaint_role_permissions(role_id);
CREATE INDEX idx_complaint_role_perms_module ON complaint_role_permissions(module);
CREATE INDEX idx_complaint_role_perms_resource ON complaint_role_permissions(resource);
CREATE INDEX idx_complaint_role_perms_action ON complaint_role_permissions(action);
```

### Default Permission Matrix

```sql
-- EMPLOYEE role permissions
INSERT INTO complaint_role_permissions (tenant_id, role_id, module, resource, action, conditions) VALUES
('{tenant_id}', '{employee_role_id}', 'COMPLAINTS', 'complaint', 'CREATE', '{"own_only": false}'),
('{tenant_id}', '{employee_role_id}', 'COMPLAINTS', 'complaint', 'READ', '{"own_only": true}'),
('{tenant_id}', '{employee_role_id}', 'COMPLAINTS', 'complaint', 'UPDATE', '{"own_only": true, "status": ["OPEN"]}'),
('{tenant_id}', '{employee_role_id}', 'COMPLAINTS', 'complaint_comment', 'CREATE', '{"own_only": true}');

-- HR_MANAGER role permissions
INSERT INTO complaint_role_permissions (tenant_id, role_id, module, resource, action, conditions) VALUES
('{tenant_id}', '{hr_manager_role_id}', 'COMPLAINTS', 'complaint', 'READ', '{"branch_only": true}'),
('{tenant_id}', '{hr_manager_role_id}', 'COMPLAINTS', 'complaint', 'UPDATE', '{"branch_only": true}'),
('{tenant_id}', '{hr_manager_role_id}', 'COMPLAINTS', 'complaint', 'ASSIGN', '{"branch_only": true}'),
('{tenant_id}', '{hr_manager_role_id}', 'COMPLAINTS', 'complaint', 'ESCALATE', '{"branch_only": true}'),
('{tenant_id}', '{hr_manager_role_id}', 'REPORTS', 'complaint_report', 'EXPORT', '{"branch_only": true}');

-- SYSTEM_ADMIN role permissions (full access)
INSERT INTO complaint_role_permissions (tenant_id, role_id, module, resource, action, conditions) VALUES
('{tenant_id}', '{sys_admin_role_id}', 'COMPLAINTS', 'complaint', 'CREATE', '{}'),
('{tenant_id}', '{sys_admin_role_id}', 'COMPLAINTS', 'complaint', 'READ', '{}'),
('{tenant_id}', '{sys_admin_role_id}', 'COMPLAINTS', 'complaint', 'UPDATE', '{}'),
('{tenant_id}', '{sys_admin_role_id}', 'COMPLAINTS', 'complaint', 'DELETE', '{}'),
('{tenant_id}', '{sys_admin_role_id}', 'CONFIGURATION', 'escalation_matrix', 'CONFIGURE', '{}'),
('{tenant_id}', '{sys_admin_role_id}', 'CONFIGURATION', 'email_template', 'CONFIGURE', '{}'),
('{tenant_id}', '{sys_admin_role_id}', 'USERS', 'user_roles', 'CONFIGURE', '{}');
```

---

## Permission Check Logic

```typescript
async checkPermission(
  userId: string,
  module: string,
  resource: string,
  action: string,
  context?: {
    complaintId?: string;
    branchId?: string;
    departmentId?: string;
  }
): Promise<boolean> {
  // Get user's active roles
  const userRoles = await this.db.user_complaint_roles.findAll({
    where: {
      user_id: userId,
      is_active: true,
      [Op.or]: [
        { valid_from: { [Op.lte]: new Date() }, valid_to: { [Op.gte]: new Date() } },
        { valid_from: null, valid_to: null }
      ]
    },
    include: ['role', 'branch', 'department']
  });

  for (const userRole of userRoles) {
    // Check permission
    const permission = await this.db.complaint_role_permissions.findOne({
      where: {
        role_id: userRole.role_id,
        module,
        resource,
        action,
        is_allowed: true
      }
    });

    if (!permission) continue;

    // Evaluate conditions
    const conditions = permission.conditions || {};

    // Check own_only condition
    if (conditions.own_only && context?.complaintId) {
      const complaint = await this.db.complaints.findByPk(context.complaintId);
      if (complaint.created_by_user_id !== userId) continue;
    }

    // Check branch_only condition
    if (conditions.branch_only && context?.branchId) {
      if (userRole.branch_id !== context.branchId) continue;
    }

    // Check department_only condition
    if (conditions.department_only && context?.departmentId) {
      if (userRole.department_id !== context.departmentId) continue;
    }

    // Check status condition
    if (conditions.status && context?.complaintId) {
      const complaint = await this.db.complaints.findByPk(context.complaintId);
      if (!conditions.status.includes(complaint.status)) continue;
    }

    // All conditions passed
    return true;
  }

  return false; // No matching permission found
}
```

---

## Summary

**Chunk 3 Tables Created**:
1. ✅ complaint_categories - Predefined complaint types
2. ✅ complaints - Core complaint data
3. ✅ complaint_comments - Activity log and comments
4. ✅ complaint_attachments - File uploads
5. ✅ complaint_roles - Complaint-specific roles
6. ✅ user_complaint_roles - User-role mappings
7. ✅ complaint_role_permissions - Granular permissions

**Key Features**:
- Auto-generated complaint numbers (CMP-2025-000001)
- System-generated activity comments
- File upload with virus scanning
- Role-based access control (RBAC)
- Context-aware permission checks
- Organizational scope support

---

**Next**: [Chunk 4 - Escalation & Email Alert Tables →](CHUNK_04_ESCALATION_EMAIL_TABLES.md)
