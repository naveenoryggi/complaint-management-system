# Complaint Management System - Complete Architecture Documentation

**Version:** 2.0
**Date:** October 11, 2025
**Status:** Planning Phase - Enhanced with Email Alert Configuration

---

## Table of Contents

1. [System Overview](#system-overview)
2. [Architecture Design](#architecture-design)
3. [Database Schema](#database-schema)
4. [Escalation System](#escalation-system)
5. [Technology Stack](#technology-stack)
6. [User Interfaces](#user-interfaces)
7. [Integration Layer](#integration-layer)
8. [Security & Compliance](#security--compliance)
9. [Deployment Architecture](#deployment-architecture)
10. [Implementation Roadmap](#implementation-roadmap)

---

## System Overview

### Purpose
A world-class complaint management system integrated with HRMS, enabling employees to log complaints regarding attendance, salary, leave, and other HR-related issues with transparent multi-level escalation and resolution tracking.

### Key Requirements

#### Functional Requirements
- **Complaint Management**: Submit, track, and resolve complaints
- **Multi-level Escalation**: Flexible 2-5 level escalation (configurable)
- **Visibility Control**: Role-based access (Employee, Manager, HR, Admin)
- **Master Data Integration**: Real-time sync with HRMS (company, branch, department, section)
- **SLA Tracking**: Automatic escalation based on SLA timers
- **Multi-tenancy**: Support multiple companies/divisions in single deployment
- **Self-service**: Knowledge base for complaint deflection
- **Surveys**: Post-resolution feedback collection
- **Analytics**: Comprehensive dashboards and reporting

#### Non-Functional Requirements
- **Scalability**: Support thousands of concurrent users
- **Performance**: Sub-second response times for critical operations
- **Availability**: 99.9% uptime SLA
- **Security**: End-to-end encryption, RBAC, audit logging
- **Compliance**: GDPR, CCPA, SOC 2 ready
- **Usability**: Intuitive, world-class user interface
- **Maintainability**: No-code configuration for admins

---

## Architecture Design

### 1. System Architecture (Layered Approach)

```
┌─────────────────────────────────────────────────────────────┐
│                    PRESENTATION LAYER                        │
├─────────────────────────────────────────────────────────────┤
│  Employee Portal  │  Manager Portal  │  HR Portal  │ Admin  │
│  - Submit         │  - Team View     │  - Full Acc │ Console│
│  - Track          │  - Escalate      │  - Analytics│ - Config│
│  - Self-service   │  - Resolve       │  - Reports  │ - SLA   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                      API GATEWAY LAYER                       │
│  - Authentication/SSO  - Rate Limiting  - Tenant Resolution │
│  - Load Balancing      - API Routing    - Request Logging   │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    MICROSERVICES LAYER                       │
├──────────────────┬──────────────────┬──────────────────────┤
│ Tenant Service   │ User Service     │ Complaint Service    │
│ - Multi-tenant   │ - Auth/RBAC      │ - CRUD               │
│   management     │ - Profile sync   │ - Status tracking    │
│ - White-labeling │ - Role mapping   │ - Assignment         │
├──────────────────┼──────────────────┼──────────────────────┤
│ Workflow Service │ Notification Svc │ Integration Service  │
│ - SLA tracking   │ - Email/SMS      │ - HRMS sync          │
│ - Escalation     │ - Push notif     │ - Master data sync   │
│ - Routing logic  │ - In-app alerts  │ - Bi-directional API │
├──────────────────┼──────────────────┼──────────────────────┤
│ Self-Service Svc │ Survey Service   │ Analytics Service    │
│ - FAQ/KB         │ - Feedback forms │ - Dashboards         │
│ - AI suggestions │ - Satisfaction   │ - Reports            │
│ - Deflection     │ - CSAT/NPS       │ - Trends             │
├──────────────────┼──────────────────┼──────────────────────┤
│ Admin Config Svc │ Audit Service    │ File Storage Service │
│ - No-code config │ - Action logs    │ - Document upload    │
│ - Category mgmt  │ - Compliance     │ - Attachments        │
│ - SLA rules      │ - History        │ - Cloud storage      │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                       DATA LAYER                             │
├──────────────────┬──────────────────┬──────────────────────┤
│ Tenant DB        │ Shared Master DB │ Cache Layer (Redis)  │
│ (Per-tenant      │ (Company, Branch,│ - Session cache      │
│  schema/database)│  Dept, Section)  │ - Query cache        │
│                  │ - Read replicas  │ - Rate limit state   │
├──────────────────┼──────────────────┼──────────────────────┤
│ Document Store   │ Search Engine    │ Message Queue        │
│ (S3/Azure Blob)  │ (Elasticsearch)  │ (RabbitMQ/Kafka)     │
│ - Attachments    │ - Full-text      │ - Async processing   │
│ - Per-tenant     │ - Analytics      │ - Event streaming    │
└─────────────────────────────────────────────────────────────┘
                              │
                              ↓
┌─────────────────────────────────────────────────────────────┐
│                    INTEGRATION LAYER                         │
│  HRMS API  │  Attendance API  │  Payroll API  │  SSO/IdP   │
│  (Master   │  (Sync leaves,   │  (Salary      │  (Azure AD,│
│   data)    │   shifts)        │   disputes)   │   Okta)    │
└─────────────────────────────────────────────────────────────┘
```

### 2. Microservices Breakdown

#### Tenant Service
- **Responsibilities**: Multi-tenant management, tenant provisioning, white-labeling
- **Key Functions**:
  - Create/update/delete tenants
  - Manage tenant configurations (branding, features)
  - Tenant isolation enforcement
  - Data residency compliance

#### User Service
- **Responsibilities**: Authentication, authorization, user management
- **Key Functions**:
  - User authentication (JWT, OAuth2, SAML)
  - RBAC implementation
  - Profile synchronization with HRMS
  - Session management

#### Complaint Service
- **Responsibilities**: Core complaint lifecycle management
- **Key Functions**:
  - Create/read/update/delete complaints
  - Status tracking
  - Assignment logic
  - Comment management
  - Attachment handling

#### Workflow Service
- **Responsibilities**: Escalation engine, SLA tracking
- **Key Functions**:
  - Initialize escalation on complaint creation
  - Auto-escalation based on SLA
  - Manual escalation handling
  - Workflow state management
  - Assignment strategy execution

#### Notification Service
- **Responsibilities**: Multi-channel notifications
- **Key Functions**:
  - Email notifications
  - SMS alerts
  - Push notifications (mobile/web)
  - In-app notifications
  - Notification preferences management

#### Integration Service
- **Responsibilities**: External system integration
- **Key Functions**:
  - HRMS data synchronization
  - Attendance system integration
  - Payroll system integration
  - Real-time webhook handling
  - Batch sync scheduling

#### Self-Service Service
- **Responsibilities**: Knowledge base, complaint deflection
- **Key Functions**:
  - KB article management
  - Search and suggestions
  - AI-powered recommendations
  - Deflection tracking

#### Survey Service
- **Responsibilities**: Post-resolution feedback
- **Key Functions**:
  - Survey template management
  - Survey distribution
  - Response collection
  - CSAT/NPS calculation

#### Analytics Service
- **Responsibilities**: Reporting and analytics
- **Key Functions**:
  - Dashboard data aggregation
  - Report generation
  - Trend analysis
  - Export functionality

#### Admin Config Service
- **Responsibilities**: No-code configuration
- **Key Functions**:
  - Escalation matrix CRUD
  - Category management
  - SLA rule configuration
  - Workflow designer
  - Email alert template management
  - Alert type configuration
  - Recipient rule management

#### Audit Service
- **Responsibilities**: Compliance and audit logging
- **Key Functions**:
  - Immutable audit trail
  - Compliance reporting
  - Data retention policies
  - Access logs

#### File Storage Service
- **Responsibilities**: Document management
- **Key Functions**:
  - File upload/download
  - Virus scanning
  - Cloud storage integration
  - Access control

---

## Database Schema

### Master Data Tables (Synced from Oryggi HRMS)

**IMPORTANT**: These tables are **synced from Oryggi database** and are **read-only** in the Complaint Management System. Any changes made in Oryggi (employee updates, org structure changes) will automatically reflect here through real-time webhooks or scheduled sync.

#### Companies (Synced from Oryggi.CompanyMaster)
```sql
CREATE TABLE companies (
    company_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,

    -- Synced from Oryggi
    oryggi_company_id INT UNIQUE NOT NULL,  -- Maps to Oryggi.CompanyMaster.Ccode
    name VARCHAR(255) NOT NULL,             -- From Oryggi.CompanyMaster.CName
    code VARCHAR(50) UNIQUE NOT NULL,       -- From Oryggi.CompanyMaster.Ccode
    address TEXT,                           -- From Oryggi.CompanyMaster.Address
    email VARCHAR(255),                     -- From Oryggi.CompanyMaster.Email
    phone VARCHAR(50),                      -- From Oryggi.CompanyMaster.TelephoneNo

    is_active BOOLEAN DEFAULT true,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_companies_tenant ON companies(tenant_id);
CREATE INDEX idx_companies_oryggi_id ON companies(oryggi_company_id);
```

#### Branches (Synced from Oryggi.BranchMaster)
```sql
CREATE TABLE branches (
    branch_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    company_id UUID NOT NULL REFERENCES companies(company_id),

    -- Synced from Oryggi
    oryggi_branch_id INT UNIQUE NOT NULL,   -- Maps to Oryggi.BranchMaster.BranchCode
    name VARCHAR(255) NOT NULL,             -- From Oryggi.BranchMaster.BranchName
    code VARCHAR(50) NOT NULL,              -- From Oryggi.BranchMaster.BranchCode
    location VARCHAR(255),                  -- From Oryggi.BranchMaster.Location

    timezone VARCHAR(50) DEFAULT 'Asia/Kolkata',
    is_active BOOLEAN DEFAULT true,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_branch_code UNIQUE(company_id, code)
);

CREATE INDEX idx_branches_company ON branches(company_id);
CREATE INDEX idx_branches_oryggi_id ON branches(oryggi_branch_id);
CREATE INDEX idx_branches_active ON branches(is_active);
```

#### Departments (Synced from Oryggi.DeptMaster)
```sql
CREATE TABLE departments (
    department_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    branch_id UUID NOT NULL REFERENCES branches(branch_id),

    -- Synced from Oryggi
    oryggi_dept_id INT UNIQUE NOT NULL,     -- Maps to Oryggi.DeptMaster.Dcode
    name VARCHAR(255) NOT NULL,             -- From Oryggi.DeptMaster.Dname
    code VARCHAR(50) NOT NULL,              -- From Oryggi.DeptMaster.Dcode

    head_user_id UUID,                      -- Will reference users table
    is_active BOOLEAN DEFAULT true,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_dept_code UNIQUE(branch_id, code)
);

CREATE INDEX idx_departments_branch ON departments(branch_id);
CREATE INDEX idx_departments_oryggi_id ON departments(oryggi_dept_id);
CREATE INDEX idx_departments_active ON departments(is_active);
```

#### Sections (Synced from Oryggi.SectionMaster)
```sql
CREATE TABLE sections (
    section_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    department_id UUID NOT NULL REFERENCES departments(department_id),

    -- Synced from Oryggi
    oryggi_section_id INT UNIQUE NOT NULL,  -- Maps to Oryggi.SectionMaster.SecCode
    name VARCHAR(255) NOT NULL,             -- From Oryggi.SectionMaster.SecName
    code VARCHAR(50) NOT NULL,              -- From Oryggi.SectionMaster.SecCode

    supervisor_user_id UUID,                -- Will reference users table
    is_active BOOLEAN DEFAULT true,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_section_code UNIQUE(department_id, code)
);

CREATE INDEX idx_sections_department ON sections(department_id);
CREATE INDEX idx_sections_oryggi_id ON sections(oryggi_section_id);
CREATE INDEX idx_sections_active ON sections(is_active);
```

#### Users (Synced from Oryggi.EmployeeMaster)
```sql
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,

    -- Organization Hierarchy (Synced from Oryggi)
    company_id UUID NOT NULL REFERENCES companies(company_id),
    branch_id UUID REFERENCES branches(branch_id),
    department_id UUID REFERENCES departments(department_id),
    section_id UUID REFERENCES sections(section_id),

    -- Synced from Oryggi.EmployeeMaster
    oryggi_employee_id INT UNIQUE NOT NULL, -- Maps to Oryggi.EmployeeMaster.Ecode
    employee_code VARCHAR(50) UNIQUE NOT NULL, -- From Oryggi.EmployeeMaster.CorpEmpCode
    email VARCHAR(255) UNIQUE NOT NULL,     -- From Oryggi.EmployeeMaster.E_mail
    phone VARCHAR(50),                      -- From Oryggi.EmployeeMaster.Telephone1
    phone_secondary VARCHAR(50),            -- From Oryggi.EmployeeMaster.Telephone2

    first_name VARCHAR(100) NOT NULL,       -- From Oryggi.EmployeeMaster.FName
    last_name VARCHAR(100) NOT NULL,        -- From Oryggi.EmployeeMaster.LName
    full_name VARCHAR(255),                 -- From Oryggi.EmployeeMaster.EmpName

    -- Reporting Structure (Synced from Oryggi)
    manager_id UUID REFERENCES users(user_id), -- From Oryggi.EmployeeMaster.ReportingHeadEcode

    -- Additional Oryggi Fields
    oryggi_designation_id INT,              -- From Oryggi.EmployeeMaster.DesCode
    oryggi_grade_id INT,                    -- From Oryggi.EmployeeMaster.Gcode
    oryggi_category_id INT,                 -- From Oryggi.EmployeeMaster.Catcode
    oryggi_role VARCHAR(50),                -- From Oryggi.EmployeeMaster.Role (for reference only)

    date_of_joining DATE,                   -- From Oryggi.EmployeeMaster.DateofJoin
    date_of_birth DATE,                     -- From Oryggi.EmployeeMaster.DateofBirth

    is_active BOOLEAN DEFAULT true,         -- From Oryggi.EmployeeMaster.Active
    last_synced_at TIMESTAMP,               -- Sync timestamp
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_users_tenant ON users(tenant_id);
CREATE INDEX idx_users_oryggi_id ON users(oryggi_employee_id);
CREATE INDEX idx_users_company ON users(company_id);
CREATE INDEX idx_users_branch ON users(branch_id);
CREATE INDEX idx_users_department ON users(department_id);
CREATE INDEX idx_users_section ON users(section_id);
CREATE INDEX idx_users_manager ON users(manager_id);
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_employee_code ON users(employee_code);
CREATE INDEX idx_users_active ON users(is_active);
```

### Complaint Management Role Tables (Complaint System Specific)

**IMPORTANT**: These tables are **specific to the Complaint Management System** and are managed independently. They reference users from the synced Oryggi data but define complaint-specific roles and permissions.

#### Complaint Roles
```sql
CREATE TABLE complaint_roles (
    role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,

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

-- Insert System Roles
INSERT INTO complaint_roles (tenant_id, role_code, role_name, role_description, role_type, scope_level, is_system_role) VALUES
('{tenant_id}', 'SYS_ADMIN', 'System Administrator', 'Full system access', 'SYSTEM_ADMIN', 'GLOBAL', true),
('{tenant_id}', 'HR_ADMIN', 'HR Administrator', 'Manage complaint system configuration', 'HR_ADMIN', 'COMPANY', true),
('{tenant_id}', 'HR_MANAGER', 'HR Manager', 'Handle escalated complaints', 'HR_MANAGER', 'BRANCH', true),
('{tenant_id}', 'ESCALATION_HANDLER', 'Escalation Handler', 'Handle escalations at specific level', 'ESCALATION_HANDLER', 'BRANCH', true),
('{tenant_id}', 'DEPT_HEAD', 'Department Head', 'Review department complaints', 'DEPARTMENT_HEAD', 'DEPARTMENT', true),
('{tenant_id}', 'SECTION_SUPERVISOR', 'Section Supervisor', 'Review section complaints', 'SECTION_SUPERVISOR', 'SECTION', true),
('{tenant_id}', 'EMPLOYEE', 'Employee', 'Can create and view own complaints', 'EMPLOYEE', 'GLOBAL', true);
```

#### User Complaint Roles (Maps Oryggi Users to Complaint Roles)
```sql
CREATE TABLE user_complaint_roles (
    user_role_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,

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
    CONSTRAINT unique_user_role_scope UNIQUE(user_id, role_id, company_id, branch_id, department_id, section_id)
);

CREATE INDEX idx_user_complaint_roles_tenant ON user_complaint_roles(tenant_id);
CREATE INDEX idx_user_complaint_roles_user ON user_complaint_roles(user_id);
CREATE INDEX idx_user_complaint_roles_role ON user_complaint_roles(role_id);
CREATE INDEX idx_user_complaint_roles_company ON user_complaint_roles(company_id);
CREATE INDEX idx_user_complaint_roles_branch ON user_complaint_roles(branch_id);
CREATE INDEX idx_user_complaint_roles_department ON user_complaint_roles(department_id);
CREATE INDEX idx_user_complaint_roles_section ON user_complaint_roles(section_id);
CREATE INDEX idx_user_complaint_roles_active ON user_complaint_roles(is_active);
```

#### Complaint Role Permissions
```sql
CREATE TABLE complaint_role_permissions (
    permission_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL,
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
```

### Tenant-Specific Tables

#### Tenants
```sql
CREATE TABLE tenants (
    tenant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL UNIQUE,
    subdomain VARCHAR(100) UNIQUE,
    status VARCHAR(20) DEFAULT 'ACTIVE',

    -- White-labeling
    branding JSONB DEFAULT '{}',
    -- Example: {"logo_url": "...", "primary_color": "#1976d2", "secondary_color": "#dc004e"}

    -- Feature flags
    features JSONB DEFAULT '{}',
    -- Example: {"self_service": true, "surveys": true, "ai_suggestions": false}

    -- Compliance
    data_residency VARCHAR(50),
    compliance_tags TEXT[],

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_tenant_status CHECK (status IN ('ACTIVE', 'TRIAL', 'SUSPENDED', 'INACTIVE'))
);

CREATE INDEX idx_tenants_subdomain ON tenants(subdomain);
CREATE INDEX idx_tenants_status ON tenants(status);
```

#### Complaint Categories
```sql
CREATE TABLE complaint_categories (
    category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    parent_category_id UUID REFERENCES complaint_categories(category_id),

    name VARCHAR(255) NOT NULL,
    code VARCHAR(50) NOT NULL,
    description TEXT,
    icon VARCHAR(50),

    is_active BOOLEAN DEFAULT true,
    display_order INTEGER DEFAULT 0,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_category_code UNIQUE(tenant_id, code)
);

CREATE INDEX idx_categories_tenant ON complaint_categories(tenant_id);
CREATE INDEX idx_categories_parent ON complaint_categories(parent_category_id);
CREATE INDEX idx_categories_active ON complaint_categories(is_active);
```

#### Complaints
```sql
CREATE TABLE complaints (
    complaint_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_number VARCHAR(50) UNIQUE NOT NULL, -- Auto-generated: C-2024-001

    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    company_id UUID NOT NULL REFERENCES companies(company_id),
    branch_id UUID NOT NULL REFERENCES branches(branch_id),
    department_id UUID REFERENCES departments(department_id),
    section_id UUID REFERENCES sections(section_id),

    employee_id UUID NOT NULL REFERENCES users(user_id),
    category_id UUID NOT NULL REFERENCES complaint_categories(category_id),

    subject VARCHAR(500) NOT NULL,
    description TEXT NOT NULL,

    priority VARCHAR(20) DEFAULT 'MEDIUM',
    status VARCHAR(50) DEFAULT 'OPEN',

    assigned_to UUID REFERENCES users(user_id),
    escalation_level INTEGER DEFAULT 1,

    -- Pre-populated data from HRMS
    pre_populated_data JSONB DEFAULT '{}',
    -- Example: {"recent_attendance": [...], "recent_leaves": [...], "salary_info": {...}}

    resolved_at TIMESTAMP,
    resolved_by UUID REFERENCES users(user_id),
    resolution_notes TEXT,

    closed_at TIMESTAMP,
    closed_by UUID REFERENCES users(user_id),

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_priority CHECK (priority IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL')),
    CONSTRAINT chk_status CHECK (status IN ('OPEN', 'IN_PROGRESS', 'PENDING_INFO', 'ESCALATED', 'RESOLVED', 'CLOSED', 'REJECTED'))
);

CREATE INDEX idx_complaints_tenant ON complaints(tenant_id);
CREATE INDEX idx_complaints_employee ON complaints(employee_id);
CREATE INDEX idx_complaints_assigned ON complaints(assigned_to);
CREATE INDEX idx_complaints_status ON complaints(status);
CREATE INDEX idx_complaints_branch ON complaints(branch_id);
CREATE INDEX idx_complaints_department ON complaints(department_id);
CREATE INDEX idx_complaints_created ON complaints(created_at DESC);
CREATE INDEX idx_complaints_number ON complaints(complaint_number);

-- Full-text search
CREATE INDEX idx_complaints_search ON complaints USING gin(to_tsvector('english', subject || ' ' || description));
```

#### Complaint History
```sql
CREATE TABLE complaint_history (
    history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id) ON DELETE CASCADE,

    action VARCHAR(100) NOT NULL,
    actor_id UUID REFERENCES users(user_id),

    from_status VARCHAR(50),
    to_status VARCHAR(50),

    from_assignee UUID REFERENCES users(user_id),
    to_assignee UUID REFERENCES users(user_id),

    comments TEXT,
    metadata JSONB DEFAULT '{}',

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_action CHECK (action IN (
        'CREATED', 'ASSIGNED', 'STATUS_CHANGED', 'ESCALATED',
        'RESOLVED', 'CLOSED', 'REOPENED', 'COMMENTED', 'UPDATED'
    ))
);

CREATE INDEX idx_history_complaint ON complaint_history(complaint_id);
CREATE INDEX idx_history_created ON complaint_history(created_at DESC);
```

#### Comments
```sql
CREATE TABLE comments (
    comment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id),

    comment_text TEXT NOT NULL,
    is_internal BOOLEAN DEFAULT false, -- Internal notes not visible to employee

    attachments JSONB DEFAULT '[]',
    -- Example: [{"file_name": "...", "file_path": "...", "size": 1024}]

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_comments_complaint ON comments(complaint_id);
CREATE INDEX idx_comments_user ON comments(user_id);
CREATE INDEX idx_comments_created ON comments(created_at DESC);
```

#### Attachments
```sql
CREATE TABLE attachments (
    attachment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id) ON DELETE CASCADE,

    file_name VARCHAR(255) NOT NULL,
    file_path TEXT NOT NULL,
    file_size BIGINT NOT NULL,
    mime_type VARCHAR(100),

    uploaded_by UUID NOT NULL REFERENCES users(user_id),
    uploaded_at TIMESTAMP DEFAULT NOW(),

    storage_region VARCHAR(50),
    virus_scan_status VARCHAR(20) DEFAULT 'PENDING',
    virus_scan_result TEXT,

    CONSTRAINT chk_virus_scan CHECK (virus_scan_status IN ('PENDING', 'CLEAN', 'INFECTED', 'FAILED'))
);

CREATE INDEX idx_attachments_complaint ON attachments(complaint_id);
CREATE INDEX idx_attachments_uploaded_by ON attachments(uploaded_by);
```

### Escalation Tables

#### Escalation Matrices
```sql
CREATE TABLE escalation_matrices (
    matrix_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    name VARCHAR(255) NOT NULL,
    description TEXT,
    is_active BOOLEAN DEFAULT true,

    -- Scope (where this matrix applies)
    scope_company_ids UUID[],
    scope_branch_ids UUID[],
    scope_department_ids UUID[],
    scope_section_ids UUID[],

    -- Linked complaint categories
    complaint_category_ids UUID[],

    -- Number of levels (2 to 5)
    total_levels INTEGER NOT NULL CHECK (total_levels >= 1 AND total_levels <= 5),

    -- SLA hours per level
    sla_config JSONB NOT NULL DEFAULT '{
        "level_1_hours": 24,
        "level_2_hours": 48,
        "level_3_hours": 72,
        "level_4_hours": 96,
        "level_5_hours": 120
    }',

    created_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_matrix_name_per_tenant UNIQUE(tenant_id, name)
);

CREATE INDEX idx_escalation_matrices_tenant ON escalation_matrices(tenant_id);
CREATE INDEX idx_escalation_matrices_active ON escalation_matrices(is_active);
CREATE INDEX idx_escalation_matrices_scope ON escalation_matrices USING GIN(scope_branch_ids);
CREATE INDEX idx_escalation_matrices_categories ON escalation_matrices USING GIN(complaint_category_ids);
```

#### Escalation Levels
```sql
CREATE TABLE escalation_levels (
    level_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    matrix_id UUID NOT NULL REFERENCES escalation_matrices(matrix_id) ON DELETE CASCADE,

    level_number INTEGER NOT NULL CHECK (level_number >= 1 AND level_number <= 5),
    level_name VARCHAR(100) NOT NULL, -- "L1: Reporting Manager"

    assignment_strategy VARCHAR(50) NOT NULL
        CHECK (assignment_strategy IN ('ROLE', 'SPECIFIC_USER', 'ROUND_ROBIN', 'LEAST_LOADED', 'REPORTING_CHAIN')),

    allowed_actions TEXT[] NOT NULL DEFAULT ARRAY['RESOLVE', 'ESCALATE', 'REASSIGN'],

    auto_escalate_hours INTEGER,
    require_approval_to_escalate BOOLEAN DEFAULT false,

    notification_config JSONB DEFAULT '{
        "immediate": true,
        "reminder_hours": 12,
        "warning_hours": 2
    }',

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_level_per_matrix UNIQUE(matrix_id, level_number)
);

CREATE INDEX idx_escalation_levels_matrix ON escalation_levels(matrix_id);
```

#### Escalation Assignees
```sql
CREATE TABLE escalation_assignees (
    assignee_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    level_id UUID NOT NULL REFERENCES escalation_levels(level_id) ON DELETE CASCADE,

    assignee_type VARCHAR(20) NOT NULL CHECK (assignee_type IN ('USER', 'ROLE', 'GROUP')),

    -- For USER type
    user_id UUID REFERENCES users(user_id),

    -- For ROLE type
    role_name VARCHAR(50), -- 'BRANCH_HR', 'DEPARTMENT_HEAD', etc.

    -- For GROUP type
    group_id UUID REFERENCES user_groups(group_id),

    -- Scope filtering (which branch/dept/section this assignee handles)
    scope_company_id UUID REFERENCES companies(company_id),
    scope_branch_id UUID REFERENCES branches(branch_id),
    scope_department_id UUID REFERENCES departments(department_id),
    scope_section_id UUID REFERENCES sections(section_id),

    -- Fallback assignee
    fallback_assignee_id UUID REFERENCES escalation_assignees(assignee_id),

    -- For load balancing
    weight INTEGER DEFAULT 1,
    is_active BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_escalation_assignees_level ON escalation_assignees(level_id);
CREATE INDEX idx_escalation_assignees_user ON escalation_assignees(user_id);
CREATE INDEX idx_escalation_assignees_scope ON escalation_assignees(scope_branch_id, scope_department_id);
```

#### User Groups
```sql
CREATE TABLE user_groups (
    group_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    name VARCHAR(100) NOT NULL,
    description TEXT,

    -- Scope
    company_id UUID REFERENCES companies(company_id),
    branch_id UUID REFERENCES branches(branch_id),
    department_id UUID REFERENCES departments(department_id),

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_group_name UNIQUE(tenant_id, name, branch_id)
);

CREATE TABLE user_group_members (
    group_id UUID NOT NULL REFERENCES user_groups(group_id) ON DELETE CASCADE,
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    joined_at TIMESTAMP DEFAULT NOW(),

    PRIMARY KEY (group_id, user_id)
);

CREATE INDEX idx_user_groups_tenant ON user_groups(tenant_id);
CREATE INDEX idx_user_groups_branch ON user_groups(branch_id);
CREATE INDEX idx_group_members_user ON user_group_members(user_id);
```

#### Complaint Escalation State
```sql
CREATE TABLE complaint_escalation_state (
    complaint_id UUID PRIMARY KEY REFERENCES complaints(complaint_id) ON DELETE CASCADE,
    matrix_id UUID NOT NULL REFERENCES escalation_matrices(matrix_id),

    current_level INTEGER NOT NULL DEFAULT 1,
    current_level_id UUID REFERENCES escalation_levels(level_id),
    current_assignee_id UUID REFERENCES users(user_id),

    escalation_count INTEGER DEFAULT 0,

    level_started_at TIMESTAMP NOT NULL DEFAULT NOW(),
    level_deadline TIMESTAMP NOT NULL,

    is_sla_breached BOOLEAN DEFAULT false,
    sla_breach_time TIMESTAMP,

    next_reminder_at TIMESTAMP,
    next_escalation_at TIMESTAMP,

    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_complaint_escalation_deadline ON complaint_escalation_state(level_deadline) WHERE is_sla_breached = false;
CREATE INDEX idx_complaint_escalation_reminder ON complaint_escalation_state(next_reminder_at) WHERE next_reminder_at IS NOT NULL;
CREATE INDEX idx_complaint_escalation_assignee ON complaint_escalation_state(current_assignee_id);
```

#### Escalation History
```sql
CREATE TABLE escalation_history (
    history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id),

    from_level INTEGER,
    to_level INTEGER,

    from_assignee_id UUID REFERENCES users(user_id),
    to_assignee_id UUID REFERENCES users(user_id),

    escalation_type VARCHAR(20) CHECK (escalation_type IN ('AUTO', 'MANUAL', 'SLA_BREACH')),
    reason TEXT,

    triggered_by UUID REFERENCES users(user_id),
    triggered_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_escalation_history_complaint ON escalation_history(complaint_id);
CREATE INDEX idx_escalation_history_triggered ON escalation_history(triggered_at DESC);
```

### Self-Service Tables

#### Knowledge Base
```sql
CREATE TABLE knowledge_base (
    kb_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    category_id UUID REFERENCES complaint_categories(category_id),

    title VARCHAR(500) NOT NULL,
    content TEXT NOT NULL,
    tags TEXT[],

    view_count INTEGER DEFAULT 0,
    helpful_count INTEGER DEFAULT 0,
    not_helpful_count INTEGER DEFAULT 0,

    is_published BOOLEAN DEFAULT false,

    created_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    -- For search
    search_vector tsvector GENERATED ALWAYS AS (
        to_tsvector('english', title || ' ' || content || ' ' || array_to_string(tags, ' '))
    ) STORED
);

CREATE INDEX idx_kb_tenant ON knowledge_base(tenant_id);
CREATE INDEX idx_kb_category ON knowledge_base(category_id);
CREATE INDEX idx_kb_search ON knowledge_base USING gin(search_vector);
CREATE INDEX idx_kb_published ON knowledge_base(is_published);
```

#### KB Feedback
```sql
CREATE TABLE kb_feedback (
    feedback_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    kb_id UUID NOT NULL REFERENCES knowledge_base(kb_id) ON DELETE CASCADE,
    user_id UUID REFERENCES users(user_id),

    was_helpful BOOLEAN NOT NULL,
    feedback_text TEXT,

    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_kb_feedback_article ON kb_feedback(kb_id);
```

#### Complaint Deflection
```sql
CREATE TABLE complaint_deflection (
    deflection_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    user_id UUID NOT NULL REFERENCES users(user_id),

    kb_id UUID NOT NULL REFERENCES knowledge_base(kb_id),
    category_id UUID REFERENCES complaint_categories(category_id),

    resolved_issue BOOLEAN NOT NULL,

    created_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_deflection_tenant ON complaint_deflection(tenant_id);
CREATE INDEX idx_deflection_kb ON complaint_deflection(kb_id);
```

### Survey Tables

#### Survey Templates
```sql
CREATE TABLE survey_templates (
    template_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    name VARCHAR(255) NOT NULL,
    description TEXT,

    questions JSONB NOT NULL,
    -- Example: [{"id": "q1", "type": "rating", "question": "...", "scale": 5}]

    is_active BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_survey_templates_tenant ON survey_templates(tenant_id);
```

#### Survey Responses
```sql
CREATE TABLE survey_responses (
    response_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id),
    template_id UUID NOT NULL REFERENCES survey_templates(template_id),
    employee_id UUID NOT NULL REFERENCES users(user_id),

    responses JSONB NOT NULL,
    -- Example: {"q1": 5, "q2": "Great service!", "q3": 9}

    overall_rating DECIMAL(3,2), -- Calculated from responses
    nps_score INTEGER, -- Net Promoter Score (0-10)

    submitted_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_survey_responses_complaint ON survey_responses(complaint_id);
CREATE INDEX idx_survey_responses_template ON survey_responses(template_id);
CREATE INDEX idx_survey_responses_submitted ON survey_responses(submitted_at DESC);
```

### Notification Tables

#### Notifications
```sql
CREATE TABLE notifications (
    notification_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    user_id UUID NOT NULL REFERENCES users(user_id),
    complaint_id UUID REFERENCES complaints(complaint_id),

    notification_type VARCHAR(50) NOT NULL,
    channel VARCHAR(20) NOT NULL,

    subject VARCHAR(500),
    content TEXT NOT NULL,
    metadata JSONB DEFAULT '{}',

    status VARCHAR(20) DEFAULT 'PENDING',
    sent_at TIMESTAMP,
    read_at TIMESTAMP,

    error_message TEXT,

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_notification_type CHECK (notification_type IN (
        'COMPLAINT_CREATED', 'COMPLAINT_ASSIGNED', 'COMPLAINT_UPDATED',
        'COMPLAINT_ESCALATED', 'COMPLAINT_RESOLVED', 'COMPLAINT_CLOSED',
        'SLA_WARNING', 'SLA_BREACH', 'REMINDER', 'SURVEY'
    )),
    CONSTRAINT chk_channel CHECK (channel IN ('EMAIL', 'SMS', 'PUSH', 'IN_APP')),
    CONSTRAINT chk_status CHECK (status IN ('PENDING', 'SENT', 'DELIVERED', 'FAILED', 'READ'))
);

CREATE INDEX idx_notifications_user ON notifications(user_id);
CREATE INDEX idx_notifications_complaint ON notifications(complaint_id);
CREATE INDEX idx_notifications_status ON notifications(status);
CREATE INDEX idx_notifications_created ON notifications(created_at DESC);
CREATE INDEX idx_notifications_unread ON notifications(user_id, read_at) WHERE read_at IS NULL;
```

### Email Alert Configuration Tables

#### Alert Types
```sql
CREATE TABLE alert_types (
    alert_type_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    type_code VARCHAR(100) NOT NULL, -- 'COMPLAINT_CREATED', 'COMPLAINT_ASSIGNED', etc.
    type_name VARCHAR(255) NOT NULL, -- Display name
    description TEXT,

    category VARCHAR(50) NOT NULL, -- 'COMPLAINT', 'ESCALATION', 'SLA', 'RESOLUTION', 'SYSTEM'

    -- Trigger configuration
    trigger_event VARCHAR(100) NOT NULL, -- Event that triggers this alert
    trigger_conditions JSONB DEFAULT '{}', -- Additional conditions
    -- Example: {"priority": ["HIGH", "CRITICAL"], "categories": ["Salary"]}

    -- Default enabled/disabled
    is_enabled BOOLEAN DEFAULT true,
    is_system_default BOOLEAN DEFAULT false, -- Cannot be deleted if true

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_alert_type UNIQUE(tenant_id, type_code),
    CONSTRAINT chk_category CHECK (category IN ('COMPLAINT', 'ESCALATION', 'SLA', 'RESOLUTION', 'SYSTEM', 'CUSTOM'))
);

CREATE INDEX idx_alert_types_tenant ON alert_types(tenant_id);
CREATE INDEX idx_alert_types_enabled ON alert_types(is_enabled);
CREATE INDEX idx_alert_types_category ON alert_types(category);
```

#### Email Alert Templates
```sql
CREATE TABLE email_alert_templates (
    template_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,

    template_name VARCHAR(255) NOT NULL,
    description TEXT,

    -- Email configuration
    subject_template TEXT NOT NULL, -- Can contain variables like {{complaint_number}}
    body_template TEXT NOT NULL, -- HTML or plain text with variables
    body_format VARCHAR(20) DEFAULT 'HTML', -- 'HTML' or 'PLAIN'

    -- Variables available in template
    available_variables JSONB DEFAULT '[]',
    -- Example: ["complaint_number", "employee_name", "category", "assigned_to", etc.]

    -- Styling
    header_html TEXT, -- Custom header
    footer_html TEXT, -- Custom footer
    css_styles TEXT, -- Custom CSS

    -- Attachments
    include_attachments BOOLEAN DEFAULT false,
    attachment_types TEXT[], -- ['PDF_REPORT', 'COMPLAINT_DETAILS']

    -- Priority
    email_priority VARCHAR(20) DEFAULT 'NORMAL', -- 'LOW', 'NORMAL', 'HIGH'

    -- Reply-to configuration
    reply_to_email VARCHAR(255),
    reply_to_name VARCHAR(255),

    -- Status
    is_active BOOLEAN DEFAULT true,
    version INTEGER DEFAULT 1,

    -- Metadata
    created_by UUID REFERENCES users(user_id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_body_format CHECK (body_format IN ('HTML', 'PLAIN')),
    CONSTRAINT chk_email_priority CHECK (email_priority IN ('LOW', 'NORMAL', 'HIGH'))
);

CREATE INDEX idx_email_templates_tenant ON email_alert_templates(tenant_id);
CREATE INDEX idx_email_templates_alert_type ON email_alert_templates(alert_type_id);
CREATE INDEX idx_email_templates_active ON email_alert_templates(is_active);
```

#### Alert Recipient Rules
```sql
CREATE TABLE alert_recipient_rules (
    rule_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,

    rule_name VARCHAR(255) NOT NULL,
    description TEXT,

    -- Recipient type
    recipient_type VARCHAR(50) NOT NULL,
    -- 'EMPLOYEE', 'ASSIGNED_USER', 'MANAGER', 'ESCALATION_CHAIN',
    -- 'ROLE', 'SPECIFIC_USER', 'GROUP', 'EMAIL_LIST', 'DYNAMIC'

    -- Configuration based on recipient type
    config JSONB NOT NULL DEFAULT '{}',
    -- Examples:
    -- For ROLE: {"role": "BRANCH_HR", "scope": "branch"}
    -- For SPECIFIC_USER: {"user_ids": ["uuid1", "uuid2"]}
    -- For EMAIL_LIST: {"emails": ["hr@company.com", "admin@company.com"]}
    -- For DYNAMIC: {"expression": "complaint.assigned_to.manager_id"}

    -- Scope filtering (when to apply this rule)
    scope_filter JSONB DEFAULT '{}',
    -- Example: {"branches": ["branch-123"], "categories": ["cat-456"], "priority": ["HIGH", "CRITICAL"]}

    -- CC and BCC options
    cc_recipients JSONB DEFAULT '[]',
    bcc_recipients JSONB DEFAULT '[]',

    -- Conditional sending
    send_conditions JSONB DEFAULT '{}',
    -- Example: {"only_if_sla_breach": true, "escalation_level_min": 2}

    -- Priority order (lower number = higher priority)
    priority_order INTEGER DEFAULT 100,

    is_active BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_recipient_type CHECK (recipient_type IN (
        'EMPLOYEE', 'ASSIGNED_USER', 'MANAGER', 'ESCALATION_CHAIN',
        'ROLE', 'SPECIFIC_USER', 'GROUP', 'EMAIL_LIST', 'DYNAMIC', 'CUSTOM'
    ))
);

CREATE INDEX idx_recipient_rules_tenant ON alert_recipient_rules(tenant_id);
CREATE INDEX idx_recipient_rules_alert_type ON alert_recipient_rules(alert_type_id);
CREATE INDEX idx_recipient_rules_active ON alert_recipient_rules(is_active);
CREATE INDEX idx_recipient_rules_priority ON alert_recipient_rules(priority_order);
```

#### Alert Schedule Configuration
```sql
CREATE TABLE alert_schedules (
    schedule_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,

    schedule_name VARCHAR(255) NOT NULL,
    description TEXT,

    -- Timing
    send_immediately BOOLEAN DEFAULT true,
    delay_minutes INTEGER DEFAULT 0, -- Delay before sending

    -- Batching (group multiple alerts into one email)
    enable_batching BOOLEAN DEFAULT false,
    batch_interval_minutes INTEGER, -- Collect alerts for X minutes before sending
    batch_max_count INTEGER, -- Max alerts per batch

    -- Rate limiting
    enable_rate_limit BOOLEAN DEFAULT false,
    max_emails_per_user_per_hour INTEGER,
    max_emails_per_user_per_day INTEGER,

    -- Business hours only
    send_during_business_hours_only BOOLEAN DEFAULT false,
    business_hours_config JSONB,
    -- Example: {"start": "09:00", "end": "18:00", "timezone": "Asia/Kolkata", "days": ["MON", "TUE", "WED", "THU", "FRI"]}

    -- Retry configuration
    retry_on_failure BOOLEAN DEFAULT true,
    max_retry_attempts INTEGER DEFAULT 3,
    retry_interval_minutes INTEGER DEFAULT 5,

    is_active BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_alert_schedules_tenant ON alert_schedules(tenant_id);
CREATE INDEX idx_alert_schedules_alert_type ON alert_schedules(alert_type_id);
CREATE INDEX idx_alert_schedules_active ON alert_schedules(is_active);
```

#### Alert Configuration Per Level
```sql
CREATE TABLE escalation_level_alerts (
    level_alert_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    level_id UUID NOT NULL REFERENCES escalation_levels(level_id) ON DELETE CASCADE,
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,
    template_id UUID REFERENCES email_alert_templates(template_id),

    -- Override default recipients for this level
    override_recipients BOOLEAN DEFAULT false,
    custom_recipients JSONB DEFAULT '[]',
    -- Example: [{"type": "ROLE", "role": "BRANCH_HR"}, {"type": "EMAIL", "email": "escalation@company.com"}]

    -- When to send at this level
    trigger_on_assignment BOOLEAN DEFAULT true,
    trigger_on_reminder BOOLEAN DEFAULT true,
    trigger_on_escalation BOOLEAN DEFAULT true,
    trigger_on_sla_warning BOOLEAN DEFAULT true,

    is_active BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_level_alert UNIQUE(level_id, alert_type_id)
);

CREATE INDEX idx_level_alerts_level ON escalation_level_alerts(level_id);
CREATE INDEX idx_level_alerts_alert_type ON escalation_level_alerts(alert_type_id);
```

#### Alert Sending History
```sql
CREATE TABLE alert_sending_history (
    history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id),
    template_id UUID REFERENCES email_alert_templates(template_id),

    complaint_id UUID REFERENCES complaints(complaint_id),
    escalation_level INTEGER,

    -- Recipients
    to_addresses TEXT[] NOT NULL,
    cc_addresses TEXT[],
    bcc_addresses TEXT[],

    -- Content
    subject TEXT NOT NULL,
    body_html TEXT,
    body_plain TEXT,

    -- Metadata
    variables_used JSONB, -- Variables that were replaced

    -- Status
    status VARCHAR(20) DEFAULT 'PENDING',
    sent_at TIMESTAMP,
    delivered_at TIMESTAMP,
    opened_at TIMESTAMP,
    clicked_at TIMESTAMP,

    -- Error handling
    error_message TEXT,
    retry_count INTEGER DEFAULT 0,

    -- Email service response
    external_message_id VARCHAR(255), -- From email provider (SendGrid, SES, etc.)
    email_service_response JSONB,

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_alert_status CHECK (status IN ('PENDING', 'SENT', 'DELIVERED', 'FAILED', 'BOUNCED', 'OPENED', 'CLICKED'))
);

CREATE INDEX idx_alert_history_tenant ON alert_sending_history(tenant_id);
CREATE INDEX idx_alert_history_complaint ON alert_sending_history(complaint_id);
CREATE INDEX idx_alert_history_status ON alert_sending_history(status);
CREATE INDEX idx_alert_history_created ON alert_sending_history(created_at DESC);
CREATE INDEX idx_alert_history_alert_type ON alert_sending_history(alert_type_id);
```

#### User Alert Preferences
```sql
CREATE TABLE user_alert_preferences (
    preference_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    -- Global email preference
    email_enabled BOOLEAN DEFAULT true,

    -- Per alert type preferences
    alert_type_id UUID REFERENCES alert_types(alert_type_id),
    is_enabled BOOLEAN DEFAULT true,

    -- Frequency
    digest_mode BOOLEAN DEFAULT false, -- Receive digest instead of individual emails
    digest_frequency VARCHAR(20), -- 'HOURLY', 'DAILY', 'WEEKLY'

    -- Quiet hours
    enable_quiet_hours BOOLEAN DEFAULT false,
    quiet_hours_start TIME,
    quiet_hours_end TIME,
    quiet_hours_timezone VARCHAR(50) DEFAULT 'UTC',

    -- Alternative contact
    alternative_email VARCHAR(255),

    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_user_alert_pref UNIQUE(user_id, alert_type_id),
    CONSTRAINT chk_digest_frequency CHECK (digest_frequency IN ('HOURLY', 'DAILY', 'WEEKLY'))
);

CREATE INDEX idx_user_alert_prefs_user ON user_alert_preferences(user_id);
CREATE INDEX idx_user_alert_prefs_alert_type ON user_alert_preferences(alert_type_id);
```

### Audit Tables

#### Audit Logs (Immutable)
```sql
CREATE TABLE audit_logs (
    audit_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    user_id UUID REFERENCES users(user_id),
    action VARCHAR(100) NOT NULL,

    resource_type VARCHAR(50) NOT NULL, -- 'COMPLAINT', 'USER', 'MATRIX', etc.
    resource_id UUID NOT NULL,

    ip_address INET,
    user_agent TEXT,

    changes JSONB DEFAULT '{}',
    -- Example: {"before": {...}, "after": {...}}

    timestamp TIMESTAMP DEFAULT NOW()
);

CREATE INDEX idx_audit_logs_tenant ON audit_logs(tenant_id);
CREATE INDEX idx_audit_logs_user ON audit_logs(user_id);
CREATE INDEX idx_audit_logs_resource ON audit_logs(resource_type, resource_id);
CREATE INDEX idx_audit_logs_timestamp ON audit_logs(timestamp DESC);

-- Make table append-only (prevent updates/deletes)
CREATE RULE audit_logs_no_update AS ON UPDATE TO audit_logs DO INSTEAD NOTHING;
CREATE RULE audit_logs_no_delete AS ON DELETE TO audit_logs DO INSTEAD NOTHING;
```

---

## Escalation System

### Overview
The escalation system is the core of the complaint management process, providing flexible, configurable multi-level escalation with automatic SLA tracking and intelligent assignment.

### Key Features
1. **Flexible N-Level Escalation**: 2-5 levels, fully configurable per matrix
2. **Granular Scope Mapping**: Company → Branch → Department → Section
3. **Multiple Assignment Strategies**: Reporting chain, specific user, role-based, round-robin, least-loaded
4. **Per-Assignee Scope Configuration**: Each assignee handles specific branches/departments/sections
5. **Fallback Assignees**: Automatic reassignment if primary unavailable
6. **Auto-Escalation with SLA**: Time-based automatic escalation
7. **Visual Configuration**: No-code admin interface
8. **Complete Audit Trail**: Every escalation tracked and logged

### Assignment Strategies

#### 1. REPORTING_CHAIN
Assigns to the employee's direct manager from HRMS hierarchy.

```typescript
async getReportingManager(employeeId: string): Promise<User> {
  const employee = await this.db.users.findUnique({
    where: { user_id: employeeId },
    include: { manager: true }
  });

  if (!employee.manager) {
    throw new Error('No reporting manager found');
  }

  return employee.manager;
}
```

#### 2. SPECIFIC_USER
Assigns to a specific user configured in the escalation level.

```typescript
async getSpecificUser(assignees: Assignee[]): Promise<User> {
  const assignee = assignees.find(a => a.assignee_type === 'USER' && a.user_id);

  if (!assignee) {
    throw new Error('No specific user configured');
  }

  const user = await this.db.users.findUnique({
    where: { user_id: assignee.user_id }
  });

  if (!user || !user.is_active) {
    // Use fallback
    if (assignee.fallback_assignee_id) {
      return await this.getFallbackUser(assignee.fallback_assignee_id);
    }
    throw new Error('Configured user is not available');
  }

  return user;
}
```

#### 3. ROLE
Assigns to a user with the specified role in the complaint's scope (branch/department).

```typescript
async getUserByRole(
  roleName: string,
  branchId: string,
  departmentId: string
): Promise<User> {
  // Find users with this role in the specified branch/department
  const users = await this.db.users.findMany({
    where: {
      role: roleName,
      branch_id: branchId,
      department_id: departmentId,
      is_active: true
    }
  });

  if (users.length === 0) {
    // Try branch level only
    users = await this.db.users.findMany({
      where: {
        role: roleName,
        branch_id: branchId,
        is_active: true
      }
    });
  }

  if (users.length === 0) {
    throw new Error(`No active user found with role ${roleName}`);
  }

  // Return first user (or use round-robin/least-loaded if multiple)
  return users[0];
}
```

#### 4. ROUND_ROBIN
Distributes complaints evenly across multiple assignees.

```typescript
async getRoundRobinUser(assignees: Assignee[]): Promise<User> {
  // Get last assigned user for this level
  const lastAssignment = await this.cache.get(`rr:${levelId}`);

  // Find next user in rotation
  const currentIndex = assignees.findIndex(a => a.user_id === lastAssignment);
  const nextIndex = (currentIndex + 1) % assignees.length;
  const nextAssignee = assignees[nextIndex];

  // Cache for next assignment
  await this.cache.set(`rr:${levelId}`, nextAssignee.user_id);

  return await this.db.users.findUnique({
    where: { user_id: nextAssignee.user_id }
  });
}
```

#### 5. LEAST_LOADED
Assigns to the user with the fewest active complaints.

```typescript
async getLeastLoadedUser(assignees: Assignee[]): Promise<User> {
  const userIds = assignees.map(a => a.user_id);

  // Count active complaints per user
  const loads = await this.db.complaints.groupBy({
    by: ['assigned_to'],
    where: {
      assigned_to: { in: userIds },
      status: { in: ['OPEN', 'IN_PROGRESS', 'ESCALATED'] }
    },
    _count: { complaint_id: true }
  });

  // Find user with minimum load
  const minLoad = Math.min(...loads.map(l => l._count.complaint_id));
  const leastLoadedUserId = loads.find(l => l._count.complaint_id === minLoad)?.assigned_to;

  if (!leastLoadedUserId) {
    // If no user has complaints, assign to first user
    return await this.db.users.findUnique({
      where: { user_id: userIds[0] }
    });
  }

  return await this.db.users.findUnique({
    where: { user_id: leastLoadedUserId }
  });
}
```

### Escalation Workflow

#### 1. Complaint Creation
```typescript
// When complaint is created
async onComplaintCreated(complaint: Complaint) {
  // Find applicable escalation matrix
  const matrix = await this.findApplicableMatrix(
    complaint.tenant_id,
    complaint.category_id,
    complaint.company_id,
    complaint.branch_id,
    complaint.department_id,
    complaint.section_id
  );

  // Initialize Level 1
  await this.escalationEngine.initializeEscalation(complaint, matrix);
}
```

#### 2. Manual Escalation
```typescript
// Manager escalates complaint
async manualEscalate(
  complaintId: string,
  managerId: string,
  reason: string
) {
  await this.escalationEngine.escalateToNextLevel(
    complaintId,
    managerId,
    reason,
    'MANUAL'
  );
}
```

#### 3. Auto-Escalation (Cron Job)
```typescript
@Cron('*/5 * * * *') // Every 5 minutes
async checkAndAutoEscalate() {
  const now = new Date();

  // Find complaints that need escalation
  const pending = await this.db.complaint_escalation_state.findMany({
    where: {
      next_escalation_at: { lte: now },
      complaint: {
        status: { notIn: ['RESOLVED', 'CLOSED', 'REJECTED'] }
      }
    }
  });

  for (const state of pending) {
    await this.escalationEngine.escalateToNextLevel(
      state.complaint_id,
      'SYSTEM',
      'Auto-escalation due to SLA breach',
      'SLA_BREACH'
    );
  }
}
```

### SLA Calculation

```typescript
calculateDeadline(slaHours: number): Date {
  const now = new Date();
  return new Date(now.getTime() + slaHours * 60 * 60 * 1000);
}

calculateReminderTime(
  deadline: Date,
  config: NotificationConfig
): Date | null {
  if (!config.reminder_hours) return null;

  const reminderTime = new Date(
    deadline.getTime() - config.reminder_hours * 60 * 60 * 1000
  );

  return reminderTime > new Date() ? reminderTime : null;
}
```

---

## Email Alert Configuration System

### Overview
The Email Alert Configuration System provides administrators with complete control over email notifications sent at different escalation levels and for various complaint events. This system allows NO-CODE configuration of alert types, email templates, recipients, and sending rules.

### Key Features
1. **Configurable Alert Types**: Define custom alert types for different events
2. **Template Management**: HTML/Plain text email templates with variable substitution
3. **Dynamic Recipients**: Rule-based recipient configuration
4. **Escalation Level Integration**: Different alerts for different escalation levels
5. **Scheduling & Rate Limiting**: Control when and how often emails are sent
6. **User Preferences**: Allow users to control their email notifications
7. **Tracking & Analytics**: Complete email sending history and open/click tracking

### System Components

#### 1. Alert Type Management

```typescript
interface AlertType {
  alert_type_id: string;
  tenant_id: string;
  type_code: string; // 'COMPLAINT_CREATED', 'ESCALATED_TO_LEVEL_2', etc.
  type_name: string; // Display name
  description: string;
  category: 'COMPLAINT' | 'ESCALATION' | 'SLA' | 'RESOLUTION' | 'SYSTEM' | 'CUSTOM';

  // When this alert triggers
  trigger_event: string; // Event name
  trigger_conditions: {
    priority?: string[]; // Only for HIGH, CRITICAL
    categories?: string[]; // Only for specific categories
    branches?: string[]; // Only for specific branches
    escalation_levels?: number[]; // Only for specific levels
  };

  is_enabled: boolean;
  is_system_default: boolean; // Cannot be deleted
}
```

**Default System Alert Types:**
```typescript
const DEFAULT_ALERT_TYPES = [
  {
    type_code: 'COMPLAINT_CREATED',
    type_name: 'Complaint Created',
    category: 'COMPLAINT',
    trigger_event: 'complaint.created'
  },
  {
    type_code: 'COMPLAINT_ASSIGNED',
    type_name: 'Complaint Assigned to You',
    category: 'COMPLAINT',
    trigger_event: 'complaint.assigned'
  },
  {
    type_code: 'COMPLAINT_ESCALATED',
    type_name: 'Complaint Escalated',
    category: 'ESCALATION',
    trigger_event: 'complaint.escalated'
  },
  {
    type_code: 'SLA_WARNING',
    type_name: 'SLA Breach Warning',
    category: 'SLA',
    trigger_event: 'complaint.sla_warning'
  },
  {
    type_code: 'SLA_BREACH',
    type_name: 'SLA Breached',
    category: 'SLA',
    trigger_event: 'complaint.sla_breach'
  },
  {
    type_code: 'COMPLAINT_RESOLVED',
    type_name: 'Complaint Resolved',
    category: 'RESOLUTION',
    trigger_event: 'complaint.resolved'
  },
  {
    type_code: 'COMPLAINT_CLOSED',
    type_name: 'Complaint Closed',
    category: 'RESOLUTION',
    trigger_event: 'complaint.closed'
  },
  {
    type_code: 'DAILY_DIGEST',
    type_name: 'Daily Complaint Digest',
    category: 'SYSTEM',
    trigger_event: 'system.daily_digest'
  }
];
```

#### 2. Email Template Management

```typescript
interface EmailTemplate {
  template_id: string;
  tenant_id: string;
  alert_type_id: string;

  template_name: string;
  description: string;

  // Email content with variable placeholders
  subject_template: string; // "Complaint #{{complaint_number}} assigned to you"
  body_template: string; // HTML or plain text with {{variables}}
  body_format: 'HTML' | 'PLAIN';

  // Available variables for substitution
  available_variables: string[];
  /*
    Common variables:
    - complaint_number
    - complaint_subject
    - employee_name
    - employee_email
    - category_name
    - priority
    - status
    - assigned_to_name
    - escalation_level
    - sla_deadline
    - created_at
    - branch_name
    - department_name
    - manager_name
  */

  // Customization
  header_html: string;
  footer_html: string;
  css_styles: string;

  // Attachments
  include_attachments: boolean;
  attachment_types: string[]; // ['PDF_REPORT', 'COMPLAINT_DETAILS']

  email_priority: 'LOW' | 'NORMAL' | 'HIGH';
  reply_to_email: string;
  reply_to_name: string;

  is_active: boolean;
  version: number;
}
```

**Example Template:**
```html
<!-- Subject Template -->
[{{priority}}] Complaint #{{complaint_number}} - {{complaint_subject}}

<!-- Body Template (HTML) -->
<!DOCTYPE html>
<html>
<head>
  <style>
    {{css_styles}}
  </style>
</head>
<body>
  {{header_html}}

  <h2>Complaint Assigned to You</h2>

  <p>Dear {{assigned_to_name}},</p>

  <p>A new complaint has been assigned to you:</p>

  <table class="complaint-details">
    <tr>
      <td><strong>Complaint #:</strong></td>
      <td>{{complaint_number}}</td>
    </tr>
    <tr>
      <td><strong>Subject:</strong></td>
      <td>{{complaint_subject}}</td>
    </tr>
    <tr>
      <td><strong>Employee:</strong></td>
      <td>{{employee_name}} ({{employee_email}})</td>
    </tr>
    <tr>
      <td><strong>Category:</strong></td>
      <td>{{category_name}}</td>
    </tr>
    <tr>
      <td><strong>Priority:</strong></td>
      <td><span class="priority-{{priority}}">{{priority}}</span></td>
    </tr>
    <tr>
      <td><strong>SLA Deadline:</strong></td>
      <td>{{sla_deadline}}</td>
    </tr>
    <tr>
      <td><strong>Escalation Level:</strong></td>
      <td>Level {{escalation_level}}</td>
    </tr>
  </table>

  <p><a href="{{complaint_url}}" class="btn-primary">View Complaint</a></p>

  <p>Please review and take action before the SLA deadline.</p>

  {{footer_html}}
</body>
</html>
```

#### 3. Recipient Rules Engine

```typescript
interface RecipientRule {
  rule_id: string;
  tenant_id: string;
  alert_type_id: string;

  rule_name: string;
  description: string;

  // Who receives this alert
  recipient_type:
    | 'EMPLOYEE'          // The complaint creator
    | 'ASSIGNED_USER'     // Current assignee
    | 'MANAGER'           // Employee's manager
    | 'ESCALATION_CHAIN'  // All users in escalation chain
    | 'ROLE'              // Users with specific role
    | 'SPECIFIC_USER'     // Specific user IDs
    | 'GROUP'             // User group
    | 'EMAIL_LIST'        // External email addresses
    | 'DYNAMIC';          // Dynamic expression

  config: any; // Configuration based on recipient_type

  // When to apply this rule
  scope_filter: {
    branches?: string[];
    departments?: string[];
    categories?: string[];
    priority?: string[];
    escalation_levels?: number[];
  };

  // Additional recipients
  cc_recipients: any[];
  bcc_recipients: any[];

  // Conditional sending
  send_conditions: {
    only_if_sla_breach?: boolean;
    escalation_level_min?: number;
    escalation_level_max?: number;
    business_hours_only?: boolean;
  };

  priority_order: number; // Execution order
  is_active: boolean;
}
```

**Recipient Resolution Service:**
```typescript
@Injectable()
export class RecipientResolutionService {

  async resolveRecipients(
    alertType: AlertType,
    complaint: Complaint,
    context: any
  ): Promise<string[]> {
    // Get all active rules for this alert type
    const rules = await this.db.alert_recipient_rules.findMany({
      where: {
        alert_type_id: alertType.alert_type_id,
        is_active: true
      },
      orderBy: { priority_order: 'asc' }
    });

    const recipients: Set<string> = new Set();

    for (const rule of rules) {
      // Check if rule applies to this complaint
      if (!this.matchesScopeFilter(rule.scope_filter, complaint)) {
        continue;
      }

      // Check send conditions
      if (!this.checkSendConditions(rule.send_conditions, complaint, context)) {
        continue;
      }

      // Resolve recipients based on type
      const ruleRecipients = await this.resolveRuleRecipients(rule, complaint);
      ruleRecipients.forEach(r => recipients.add(r));
    }

    return Array.from(recipients);
  }

  private async resolveRuleRecipients(
    rule: RecipientRule,
    complaint: Complaint
  ): Promise<string[]> {
    switch (rule.recipient_type) {
      case 'EMPLOYEE':
        return [complaint.employee.email];

      case 'ASSIGNED_USER':
        return complaint.assigned_to ? [complaint.assigned_to.email] : [];

      case 'MANAGER':
        const manager = await this.getManager(complaint.employee_id);
        return manager ? [manager.email] : [];

      case 'ESCALATION_CHAIN':
        return await this.getEscalationChainEmails(complaint);

      case 'ROLE':
        return await this.getUsersByRole(
          rule.config.role,
          complaint.branch_id,
          complaint.department_id
        );

      case 'SPECIFIC_USER':
        return await this.getUserEmails(rule.config.user_ids);

      case 'GROUP':
        return await this.getGroupMemberEmails(rule.config.group_id);

      case 'EMAIL_LIST':
        return rule.config.emails;

      case 'DYNAMIC':
        return await this.evaluateDynamicExpression(
          rule.config.expression,
          complaint
        );

      default:
        return [];
    }
  }
}
```

#### 4. Email Sending Service

```typescript
@Injectable()
export class AlertEmailService {

  async sendAlert(
    alertType: AlertType,
    complaint: Complaint,
    context: any
  ): Promise<void> {
    // Get template
    const template = await this.getActiveTemplate(alertType.alert_type_id);
    if (!template) {
      console.warn(`No active template for alert type: ${alertType.type_code}`);
      return;
    }

    // Get schedule configuration
    const schedule = await this.getAlertSchedule(alertType.alert_type_id);

    // Check if we should send now or schedule for later
    if (schedule && !schedule.send_immediately) {
      await this.scheduleAlert(alertType, complaint, template, schedule);
      return;
    }

    // Resolve recipients
    const recipients = await this.recipientResolver.resolveRecipients(
      alertType,
      complaint,
      context
    );

    if (recipients.length === 0) {
      console.warn(`No recipients for alert type: ${alertType.type_code}`);
      return;
    }

    // Filter by user preferences
    const finalRecipients = await this.filterByUserPreferences(
      recipients,
      alertType.alert_type_id
    );

    // Check rate limits
    const rateLimitedRecipients = await this.applyRateLimits(
      finalRecipients,
      schedule
    );

    // Render email
    const renderedEmail = await this.renderEmail(template, complaint, context);

    // Send email
    await this.sendEmail({
      to: rateLimitedRecipients,
      subject: renderedEmail.subject,
      html: renderedEmail.html,
      text: renderedEmail.text,
      priority: template.email_priority,
      reply_to: template.reply_to_email,
      attachments: await this.getAttachments(template, complaint)
    });

    // Log sending history
    await this.logAlertHistory(
      alertType,
      template,
      complaint,
      rateLimitedRecipients,
      renderedEmail
    );
  }

  private async renderEmail(
    template: EmailTemplate,
    complaint: Complaint,
    context: any
  ): Promise<RenderedEmail> {
    // Prepare variables for substitution
    const variables = await this.prepareVariables(complaint, context);

    // Render subject
    const subject = this.replaceVariables(template.subject_template, variables);

    // Render body
    const body = this.replaceVariables(template.body_template, variables);

    // Add header and footer
    const html = `
      ${template.header_html || ''}
      ${body}
      ${template.footer_html || ''}
    `;

    // Generate plain text version if needed
    const text = template.body_format === 'PLAIN'
      ? body
      : this.htmlToText(html);

    return { subject, html, text, variables };
  }

  private replaceVariables(
    template: string,
    variables: Record<string, any>
  ): string {
    let result = template;

    for (const [key, value] of Object.entries(variables)) {
      const regex = new RegExp(`{{${key}}}`, 'g');
      result = result.replace(regex, String(value || ''));
    }

    return result;
  }

  private async prepareVariables(
    complaint: Complaint,
    context: any
  ): Promise<Record<string, any>> {
    return {
      // Complaint details
      complaint_id: complaint.complaint_id,
      complaint_number: complaint.complaint_number,
      complaint_subject: complaint.subject,
      complaint_description: complaint.description,
      complaint_url: `${process.env.APP_URL}/complaints/${complaint.complaint_id}`,

      // Employee
      employee_id: complaint.employee.user_id,
      employee_name: complaint.employee.full_name,
      employee_email: complaint.employee.email,
      employee_code: complaint.employee.employee_code,

      // Category
      category_name: complaint.category.name,
      category_code: complaint.category.code,

      // Status
      priority: complaint.priority,
      status: complaint.status,
      escalation_level: context.escalation_level || complaint.escalation_level,

      // Assignment
      assigned_to_name: complaint.assigned_to?.full_name || 'Unassigned',
      assigned_to_email: complaint.assigned_to?.email || '',

      // Manager
      manager_name: complaint.employee.manager?.full_name || 'N/A',
      manager_email: complaint.employee.manager?.email || '',

      // Organization
      company_name: complaint.company.name,
      branch_name: complaint.branch.name,
      department_name: complaint.department?.name || 'N/A',
      section_name: complaint.section?.name || 'N/A',

      // Dates
      created_at: this.formatDate(complaint.created_at),
      sla_deadline: context.sla_deadline
        ? this.formatDate(context.sla_deadline)
        : 'N/A',

      // Current time
      current_date: this.formatDate(new Date()),
      current_year: new Date().getFullYear(),

      // Tenant branding
      company_logo: complaint.tenant.branding?.logo_url || '',
      primary_color: complaint.tenant.branding?.primary_color || '#1976d2',

      // Custom context
      ...context.custom_variables
    };
  }
}
```

#### 5. Alert Scheduling & Batching

```typescript
@Injectable()
export class AlertSchedulingService {

  async scheduleAlert(
    alertType: AlertType,
    complaint: Complaint,
    template: EmailTemplate,
    schedule: AlertSchedule
  ): Promise<void> {
    // Check if batching is enabled
    if (schedule.enable_batching) {
      await this.addToBatch(alertType, complaint, template, schedule);
      return;
    }

    // Schedule for delayed sending
    const sendAt = new Date(Date.now() + schedule.delay_minutes * 60 * 1000);

    await this.queue.add('send-alert', {
      alert_type_id: alertType.alert_type_id,
      complaint_id: complaint.complaint_id,
      template_id: template.template_id
    }, {
      delay: schedule.delay_minutes * 60 * 1000
    });
  }

  @Cron('*/5 * * * *') // Every 5 minutes
  async processBatchedAlerts(): Promise<void> {
    // Get all batching configurations
    const schedules = await this.db.alert_schedules.findMany({
      where: {
        enable_batching: true,
        is_active: true
      }
    });

    for (const schedule of schedules) {
      // Check if batch interval has passed
      const lastBatchSent = await this.cache.get(`batch:${schedule.schedule_id}:last_sent`);
      const now = Date.now();

      if (lastBatchSent && (now - lastBatchSent) < schedule.batch_interval_minutes * 60 * 1000) {
        continue; // Not time yet
      }

      // Get pending alerts for this schedule
      const pendingAlerts = await this.getPendingBatchAlerts(schedule.schedule_id);

      if (pendingAlerts.length === 0) continue;

      // Group by recipient
      const groupedByRecipient = this.groupAlertsByRecipient(pendingAlerts);

      // Send batched emails
      for (const [recipient, alerts] of Object.entries(groupedByRecipient)) {
        await this.sendBatchedEmail(recipient, alerts, schedule);
      }

      // Update last sent time
      await this.cache.set(`batch:${schedule.schedule_id}:last_sent`, now);
    }
  }
}
```

#### 6. User Preferences

```typescript
@Injectable()
export class UserAlertPreferencesService {

  async getUserPreferences(
    userId: string,
    alertTypeId?: string
  ): Promise<UserAlertPreference[]> {
    return await this.db.user_alert_preferences.findMany({
      where: {
        user_id: userId,
        ...(alertTypeId && { alert_type_id: alertTypeId })
      }
    });
  }

  async shouldSendAlert(
    userId: string,
    alertTypeId: string,
    sendTime: Date
  ): Promise<boolean> {
    // Get user preferences
    const prefs = await this.getUserPreferences(userId, alertTypeId);

    if (prefs.length === 0) {
      // No preferences, default to enabled
      return true;
    }

    const pref = prefs[0];

    // Check if email is globally disabled
    if (!pref.email_enabled) return false;

    // Check if this alert type is disabled
    if (!pref.is_enabled) return false;

    // Check if in digest mode (handled separately)
    if (pref.digest_mode) return false;

    // Check quiet hours
    if (pref.enable_quiet_hours) {
      if (this.isInQuietHours(sendTime, pref)) {
        // Schedule for after quiet hours
        await this.scheduleAfterQuietHours(userId, alertTypeId, pref);
        return false;
      }
    }

    return true;
  }

  private isInQuietHours(
    sendTime: Date,
    pref: UserAlertPreference
  ): boolean {
    const userTime = moment(sendTime).tz(pref.quiet_hours_timezone);
    const sendHour = userTime.format('HH:mm');

    return sendHour >= pref.quiet_hours_start && sendHour <= pref.quiet_hours_end;
  }
}
```

### Integration with Escalation System

```typescript
// When complaint is escalated
@Injectable()
export class EscalationEngine {

  async escalateToNextLevel(
    complaintId: string,
    triggeredBy: string,
    reason: string,
    escalationType: 'AUTO' | 'MANUAL' | 'SLA_BREACH'
  ): Promise<void> {
    // ... escalation logic ...

    // Trigger alert for escalation
    await this.alertService.triggerAlert('COMPLAINT_ESCALATED', complaint, {
      escalation_level: nextLevel,
      escalation_type: escalationType,
      triggered_by_name: triggeredBy === 'SYSTEM' ? 'System' : triggeredByUser.full_name,
      reason: reason,
      previous_assignee: state.current_assignee.full_name,
      new_assignee: nextAssignee.full_name,
      sla_deadline: newDeadline
    });

    // Check for level-specific alerts
    const levelAlerts = await this.db.escalation_level_alerts.findMany({
      where: {
        level_id: nextLevelConfig.level_id,
        is_active: true
      }
    });

    for (const levelAlert of levelAlerts) {
      if (levelAlert.trigger_on_escalation) {
        await this.alertService.sendAlertWithTemplate(
          levelAlert.alert_type_id,
          levelAlert.template_id,
          complaint,
          { escalation_level: nextLevel }
        );
      }
    }
  }
}
```

---

## Technology Stack

### Frontend

#### React Application
```json
{
  "dependencies": {
    "react": "^18.2.0",
    "react-dom": "^18.2.0",
    "next": "^14.0.0",
    "typescript": "^5.2.0",

    // UI Library
    "@mui/material": "^5.14.0",
    "@mui/icons-material": "^5.14.0",
    "@emotion/react": "^11.11.0",
    "@emotion/styled": "^11.11.0",

    // State Management
    "@reduxjs/toolkit": "^1.9.7",
    "react-redux": "^8.1.3",

    // API Communication
    "axios": "^1.5.0",
    "@tanstack/react-query": "^5.0.0",

    // Forms
    "react-hook-form": "^7.47.0",
    "yup": "^1.3.2",

    // Routing
    "react-router-dom": "^6.17.0",

    // Rich Text Editor
    "@tiptap/react": "^2.1.0",

    // File Upload
    "react-dropzone": "^14.2.3",

    // Charts
    "recharts": "^2.9.0",

    // Date/Time
    "date-fns": "^2.30.0",

    // Notifications
    "notistack": "^3.0.1"
  }
}
```

### Backend

#### Node.js + NestJS
```json
{
  "dependencies": {
    "@nestjs/common": "^10.2.0",
    "@nestjs/core": "^10.2.0",
    "@nestjs/platform-express": "^10.2.0",

    // Database
    "@prisma/client": "^5.4.0",
    "prisma": "^5.4.0",

    // Authentication
    "@nestjs/jwt": "^10.1.0",
    "@nestjs/passport": "^10.0.0",
    "passport": "^0.6.0",
    "passport-jwt": "^4.0.1",
    "passport-saml": "^3.2.4",

    // Validation
    "class-validator": "^0.14.0",
    "class-transformer": "^0.5.1",

    // GraphQL (optional)
    "@nestjs/graphql": "^12.0.0",
    "@nestjs/apollo": "^12.0.0",
    "graphql": "^16.8.0",

    // Caching
    "cache-manager": "^5.2.3",
    "cache-manager-redis-store": "^3.0.1",

    // Message Queue
    "@nestjs/bull": "^10.0.0",
    "bull": "^4.11.3",

    // Email
    "@nestjs-modules/mailer": "^1.9.1",
    "nodemailer": "^6.9.5",

    // File Storage
    "@aws-sdk/client-s3": "^3.425.0",

    // Logging
    "winston": "^3.11.0",

    // Monitoring
    "@nestjs/terminus": "^10.1.0",
    "prom-client": "^15.0.0"
  }
}
```

### Database

#### PostgreSQL Configuration
```yaml
postgresql:
  version: "15"
  extensions:
    - uuid-ossp
    - pgcrypto
    - pg_trgm  # For fuzzy search

  configuration:
    max_connections: 200
    shared_buffers: 256MB
    effective_cache_size: 1GB
    work_mem: 4MB
    maintenance_work_mem: 128MB

  replication:
    mode: streaming
    replicas: 2
```

### Cache

#### Redis Configuration
```yaml
redis:
  version: "7.0"
  mode: cluster
  nodes: 3

  use_cases:
    - Session storage
    - Query cache
    - Rate limiting
    - Round-robin state
    - Real-time notifications
```

### Message Queue

#### RabbitMQ/Kafka
```yaml
rabbitmq:
  version: "3.12"

  queues:
    - name: notifications
      durable: true
      priority: true
    - name: escalations
      durable: true
    - name: sync_hrms
      durable: true
```

### Search Engine

#### Elasticsearch
```yaml
elasticsearch:
  version: "8.10"
  nodes: 3

  indices:
    - name: complaints
      mappings:
        properties:
          subject: { type: text, analyzer: standard }
          description: { type: text }
          status: { type: keyword }
          created_at: { type: date }

    - name: knowledge_base
      mappings:
        properties:
          title: { type: text, boost: 2.0 }
          content: { type: text }
          tags: { type: keyword }
```

### DevOps Stack

```yaml
infrastructure:
  containerization: Docker
  orchestration: Kubernetes

  ci_cd:
    tool: GitHub Actions
    stages:
      - build
      - test
      - security_scan
      - deploy

  monitoring:
    - Prometheus (metrics)
    - Grafana (dashboards)
    - ELK Stack (logs)
    - Sentry (error tracking)

  cloud_providers:
    - AWS (EKS, RDS, S3, CloudFront)
    - Azure (AKS, PostgreSQL, Blob Storage)
    - GCP (GKE, Cloud SQL, Cloud Storage)
```

---

## User Interfaces

### 1. Employee Portal

#### Features
- Simple complaint submission with guided workflow
- Real-time status tracking
- Comment on complaints
- View escalation history
- Upload attachments
- Self-service knowledge base
- Post-resolution surveys

#### Key Screens
1. **Dashboard**: Overview of all complaints
2. **Submit Complaint**: Multi-step guided form
3. **Complaint Details**: Full complaint view with timeline
4. **Knowledge Base**: Search and browse articles
5. **Notifications**: In-app notification center

### 2. Manager Portal

#### Features
- View team complaints
- Action-oriented dashboard with SLA warnings
- Quick actions (resolve, escalate, request info)
- Comment and internal notes
- Reassign complaints
- View escalation path
- Team analytics

#### Key Screens
1. **Action Dashboard**: Priority-based complaint list
2. **Team View**: Complaints by team member
3. **Complaint Details**: Full context with action buttons
4. **Analytics**: Team performance metrics

### 3. HR Portal

#### Features
- Full visibility (branch/company-wide)
- Advanced filtering and search
- Bulk actions
- Comprehensive analytics
- Export reports
- Trend analysis
- Category-wise breakdown

#### Key Screens
1. **HR Dashboard**: Company/branch-wide overview
2. **Complaints List**: Advanced filters and search
3. **Analytics**: Detailed reports and trends
4. **User Management**: View complaint load by user

### 4. Admin Configuration Console

#### Features
- Escalation matrix CRUD
- Visual workflow designer
- Category management
- SLA configuration
- User group management
- Knowledge base management
- Survey template editor
- **Email Alert Configuration** (NEW)
- **Alert Template Designer** (NEW)
- **Recipient Rules Management** (NEW)
- White-labeling configuration
- Audit log viewer

#### Key Screens

##### Core Configuration
1. **Matrix List**: All escalation matrices
2. **Matrix Wizard**: Step-by-step configuration
3. **Category Manager**: Complaint categories
4. **SLA Rules**: SLA configuration
5. **User Groups**: Group management
6. **Knowledge Base**: Article management
7. **Branding**: White-labeling settings
8. **Audit Logs**: Compliance viewer

##### Email Alert Configuration (New Module)
9. **Alert Types Management**
   - List all alert types (system + custom)
   - Create custom alert types
   - Configure trigger conditions
   - Enable/disable alerts per type

10. **Email Template Designer**
   - Visual HTML email editor
   - Variable insertion helper
   - Template preview with sample data
   - Version management
   - A/B testing support

11. **Recipient Rules**
   - Configure who receives each alert type
   - Scope-based rules (branch/department/category)
   - Priority-based rule execution
   - CC/BCC configuration
   - Conditional sending rules

12. **Alert Scheduling**
   - Configure send timing
   - Batching configuration
   - Rate limiting settings
   - Business hours configuration

13. **Alert Analytics**
   - Sending history and status
   - Open/click rates
   - Delivery success rate
   - Most effective templates
   - User engagement metrics

14. **User Preferences (Admin View)**
   - View all user alert preferences
   - Set default preferences
   - Bulk preference updates

---

## Integration Layer

### 1. HRMS Integration

#### HRMS Database Schema Mapping - Oryggi System

**Database**: Oryggi (SQL Server Express)
**Server**: LAPTOP-NF9BTG7Q\SQLEXPRESS

This section contains the **actual HRMS schema** from your Oryggi database. The Complaint Management System will integrate with these existing tables.

**Actual Oryggi HRMS Schema**:

```sql
-- ============================================
-- ORYGGI HRMS DATABASE SCHEMA
-- ============================================

-- 1. EMPLOYEE MASTER TABLE (Primary Employee Data)
CREATE TABLE EmployeeMaster (
    Ecode INT PRIMARY KEY,                    -- Employee ID (Primary Key)
    CorpEmpCode VARCHAR(20) NOT NULL,         -- Corporate Employee Code
    EmpName NVARCHAR(50) NOT NULL,            -- Full Employee Name
    FName NVARCHAR(100),                      -- First Name
    LName NVARCHAR(100),                      -- Last Name
    E_mail VARCHAR(50),                       -- Email Address
    Telephone1 VARCHAR(50),                   -- Primary Phone
    Telephone2 VARCHAR(50),                   -- Secondary Phone

    -- Reporting Structure
    ReportingHeadEcode INT,                   -- Direct Manager/Reporting Head

    -- Organization Hierarchy
    Gcode INT NOT NULL,                       -- Grade Code (FK to GradeMaster)
    Catcode INT NOT NULL,                     -- Category Code (FK to CatMaster)
    DesCode INT NOT NULL,                     -- Designation Code (FK to DesignationMaster)
    SecCode INT NOT NULL,                     -- Section Code (FK to SectionMaster)

    -- Employment Details
    Active BIT NOT NULL DEFAULT 1,            -- Active Status (1=Active, 0=Inactive)
    DateofBirth DATETIME,                     -- Date of Birth
    DateofJoin DATETIME NOT NULL,             -- Date of Joining
    LeavingDate DATETIME,                     -- Leaving Date (if terminated)
    LeavingReason VARCHAR(50),                -- Reason for Leaving

    -- Additional Employee Info
    Sex BIT NOT NULL DEFAULT 1,               -- Gender (1=Male, 0=Female)
    IsMarried BIT NOT NULL DEFAULT 0,         -- Marital Status
    BloodGroup VARCHAR(3),                    -- Blood Group
    Qualification VARCHAR(50),                -- Educational Qualification
    Address1 VARCHAR(100),                    -- Permanent Address
    Address2 NVARCHAR(150),                   -- Current Address
    GuardianName NVARCHAR(50),                -- Guardian/Parent Name

    -- Authentication & Access
    Password VARCHAR(256),                    -- Password Hash
    Role VARCHAR(20) NOT NULL DEFAULT 'Employee', -- Role (Employee, Manager, HR, Admin)
    UserID VARCHAR(20),                       -- Login User ID

    -- Contractor/Temporary Employee
    ContractorID INT,                         -- Contractor ID (if contract employee)
    PO_Number NVARCHAR(150),                  -- Purchase Order Number

    -- Custom Fields
    [Custom Field 1] NVARCHAR(MAX),
    [Custom Field 2] NVARCHAR(MAX),
    [Custom Field 3] NVARCHAR(MAX),
    [Custom Field 4] NVARCHAR(MAX),
    [Custom Field 5] NVARCHAR(MAX),

    -- Audit Fields
    Created_Date DATETIME,                    -- Record Creation Date
    LastUpdate DATETIME,                      -- Last Update Timestamp

    FOREIGN KEY (GenderID) REFERENCES GenderMaster(GenderID)
);

-- 2. COMPANY MASTER TABLE
CREATE TABLE CompanyMaster (
    Ccode INT PRIMARY KEY,                    -- Company ID (Primary Key)
    CName VARCHAR(100) NOT NULL,              -- Company Name
    Address VARCHAR(200),                     -- Company Address
    PinCode VARCHAR(6),                       -- PIN/ZIP Code
    TelephoneNo VARCHAR(15),                  -- Company Phone
    Email VARCHAR(50),                        -- Company Email
    Logo IMAGE                                -- Company Logo (Binary)
);

-- 3. BRANCH MASTER TABLE
CREATE TABLE BranchMaster (
    BranchCode INT PRIMARY KEY,               -- Branch ID (Primary Key)
    BranchName VARCHAR(50) NOT NULL,          -- Branch Name
    Location VARCHAR(100),                    -- Branch Location/Address
    Ccode INT NOT NULL,                       -- Company ID (FK)

    FOREIGN KEY (Ccode) REFERENCES CompanyMaster(Ccode)
);

-- 4. DEPARTMENT MASTER TABLE
CREATE TABLE DeptMaster (
    Dcode INT PRIMARY KEY,                    -- Department ID (Primary Key)
    Dname VARCHAR(50) NOT NULL,               -- Department Name
    BranchCode INT NOT NULL,                  -- Branch ID (FK)

    FOREIGN KEY (BranchCode) REFERENCES BranchMaster(BranchCode)
);

-- 5. SECTION MASTER TABLE
CREATE TABLE SectionMaster (
    SecCode INT PRIMARY KEY,                  -- Section ID (Primary Key)
    SecName VARCHAR(50) NOT NULL,             -- Section Name
    Dcode INT NOT NULL,                       -- Department ID (FK)

    FOREIGN KEY (Dcode) REFERENCES DeptMaster(Dcode)
);

-- 6. DESIGNATION MASTER TABLE
CREATE TABLE DesignationMaster (
    DesCode INT PRIMARY KEY,                  -- Designation ID (Primary Key)
    DesName VARCHAR(50) NOT NULL,             -- Designation Name
    DesName_hindi NVARCHAR(50)                -- Designation Name in Hindi
);

-- 7. GRADE MASTER TABLE
CREATE TABLE GradeMaster (
    Gcode INT PRIMARY KEY,                    -- Grade ID (Primary Key)
    Gname VARCHAR(50) NOT NULL,               -- Grade Name
    IsNightShift BIT DEFAULT 0,               -- Night Shift Allowed
    Remark1 VARCHAR(MAX) DEFAULT '',          -- Additional Remarks
    Remark2 VARCHAR(MAX) DEFAULT ''
);

-- 8. CATEGORY MASTER TABLE
CREATE TABLE CatMaster (
    CatCode INT PRIMARY KEY,                  -- Category ID (Primary Key)
    CatName VARCHAR(MAX),                     -- Category Name (Staff, Worker, etc.)
    Remark NVARCHAR(MAX)                      -- Additional Remarks
);

-- 9. ROLE MASTER TABLE
CREATE TABLE RoleMaster (
    RoleId INT PRIMARY KEY,                   -- Role ID (Primary Key)
    Role VARCHAR(20),                         -- Role Name (Employee, Manager, HR, Admin)
    Template NVARCHAR(MAX)                    -- Role Permissions Template
);

-- 10. REPORTING HEAD MASTER (Primary Reporting)
CREATE TABLE ReportingHeadMaster (
    Ecode INT,                                -- Employee Code
    ReportingHeadEcode INT                    -- Reporting Head Employee Code
);

-- 11. MULTI REPORTING HEAD RELATION (Matrix Reporting)
CREATE TABLE MultiReportingHeadRelation (
    ECode INT,                                -- Employee Code
    ReportingHeadEcode INT,                   -- Additional Reporting Head
    Remark NVARCHAR(200)                      -- Purpose/Type of Reporting Relationship
);

-- 12. SHIFT MASTER TABLE
CREATE TABLE ShiftMaster (
    Scode INT PRIMARY KEY,                    -- Shift ID (Primary Key)
    Sname VARCHAR(50) NOT NULL,               -- Shift Name
    StartTime DATETIME NOT NULL,              -- Shift Start Time
    EndTime DATETIME NOT NULL,                -- Shift End Time
    LunchStartTime DATETIME,                  -- Lunch Break Start
    LunchEndTime DATETIME,                    -- Lunch Break End
    isRTC BIT NOT NULL,                       -- Is Round-The-Clock
    IsLunchDeduct BIT NOT NULL DEFAULT 0      -- Deduct Lunch Time
);

-- 13. ATTENDANCE TABLE
CREATE TABLE Attendance (
    attendance_id INT PRIMARY KEY,            -- Attendance Record ID
    Ecode INT,                                -- Employee Code
    session_id INT,                           -- Session ID
    status_id INT,                            -- Attendance Status
    timestamp DATETIME,                       -- Attendance Timestamp
    TerminalID INT                            -- Terminal/Device ID
);

-- 14. LEAVE MASTER TABLE
CREATE TABLE LeaveMaster (
    Lcode INT PRIMARY KEY,                    -- Leave Type ID (Primary Key)
    LshortName VARCHAR(15) NOT NULL,          -- Leave Short Name (CL, EL, SL, etc.)
    Ldescription VARCHAR(50),                 -- Leave Description
    IsOffInclude BIT NOT NULL DEFAULT 1,      -- Include Weekly Offs (1=Yes, 0=No)
    IsHolidayInclude BIT NOT NULL DEFAULT 1,  -- Include Holidays (1=Yes, 0=No)
    TreatedAsPresent BIT NOT NULL DEFAULT 0,  -- Count as Present (1=Yes, 0=No)
    LeaveLimit TINYINT NOT NULL DEFAULT 0,    -- Maximum Leave Days Allowed
    IsAutoAcuralAllowed BIT NOT NULL DEFAULT 1, -- Auto Accrual Allowed (1=Yes, 0=No)
    IsCarriedForward BIT NOT NULL DEFAULT 0,  -- Carry Forward to Next Year (1=Yes, 0=No)
    MaxContinuousLeave TINYINT NOT NULL DEFAULT 0 -- Max Continuous Days Allowed
);

-- 15. LEAVE AMOUNT TYPE MASTER (NEW - Leave Amount Types)
CREATE TABLE LeaveAmountTypeMaster (
    LeaveAmountTypeID INT PRIMARY KEY,        -- Leave Amount Type ID (Primary Key)
    LeaveAmountTypeName VARCHAR(100) NOT NULL UNIQUE, -- Type Name (Full Day, Half Day, Short Leave, etc.)
    LeaveAmountTypeValue DECIMAL NOT NULL,    -- Numeric Value (1.0 for Full, 0.5 for Half, etc.)
    Status BIT NOT NULL DEFAULT 1,            -- Active Status (1=Active, 0=Inactive)
    CreateDate DATETIME NOT NULL DEFAULT GETDATE(), -- Creation Date
    Remark VARCHAR(100) DEFAULT ''            -- Additional Remarks
);

-- 16. LEAVE CARD (Leave Applications)
CREATE TABLE LeaveCard (
    LCCode INT PRIMARY KEY,                   -- Leave Card ID (Primary Key)
    LACode INT NOT NULL,                      -- Leave Accural Code (FK to LeaveAccural)
    Lcode INT NOT NULL,                       -- Leave Type Code (FK to LeaveMaster)
    ApplicationDate DATETIME NOT NULL,        -- Application Date
    StartDate DATETIME NOT NULL,              -- Leave Start Date
    EndDate DATETIME NOT NULL,                -- Leave End Date
    LeaveAmount DECIMAL NOT NULL DEFAULT 0,   -- Number of Days/Amount
    LeaveAmtType TINYINT NOT NULL DEFAULT 0,  -- Leave Amount Type (0=Full, 1=Half, etc.)
    DaySegment TINYINT DEFAULT 1,             -- Day Segment (1=First Half, 2=Second Half, etc.)
    LeaveReason VARCHAR(500),                 -- Leave Reason/Purpose
    Status TINYINT NOT NULL DEFAULT 1,        -- Status (0=Cancelled, 1=Pending, 2=Approved, 3=Rejected)
    ApprovalRejectionDate DATETIME,           -- Approval/Rejection Date
    ApprovalRejectionBy VARCHAR(50),          -- Approved/Rejected By (Username)
    ApprovalRejectionReason VARCHAR(500),     -- Approval/Rejection Reason
    CancelationReason VARCHAR(500),           -- Cancellation Reason
    CancelationDate DATETIME,                 -- Cancellation Date
    CancelationBy VARCHAR(50),                -- Cancelled By (Username)
    Updated BIT NOT NULL DEFAULT 0,           -- Is Updated Flag
    Locked BIT NOT NULL DEFAULT 0,            -- Is Locked Flag (prevents modification)

    FOREIGN KEY (LACode) REFERENCES LeaveAccural(LACode)
);

-- 17. LEAVE ACCURAL (Leave Balance/Entitlement)
CREATE TABLE LeaveAccural (
    LACode INT PRIMARY KEY,                   -- Leave Accural Code (Primary Key)
    ECode INT NOT NULL,                       -- Employee Code (FK to EmployeeMaster)
    LCode INT NOT NULL,                       -- Leave Type Code (FK to LeaveMaster)
    LeaveAllowedCYear DECIMAL NOT NULL DEFAULT 0, -- Leave Allowed Current Year
    LeaveAllowedPYear DECIMAL NOT NULL DEFAULT 0, -- Leave Allowed Previous Year (Carried Forward)
    LeaveTaken DECIMAL NOT NULL DEFAULT 0,    -- Leave Taken/Consumed
    LeaveYear DATETIME NOT NULL,              -- Leave Year

    FOREIGN KEY (ECode) REFERENCES EmployeeMaster(Ecode),
    FOREIGN KEY (LCode) REFERENCES LeaveMaster(Lcode)
);

-- 18. HOLIDAY MASTER TABLE
CREATE TABLE HolidayMaster (
    Hcode INT PRIMARY KEY,                    -- Holiday ID (Primary Key)
    Hname VARCHAR(50) NOT NULL,               -- Holiday Name
    HDate DATETIME NOT NULL                   -- Holiday Date
);

-- 19. HOLIDAY DEPARTMENT RELATION
CREATE TABLE HolidayDepartmentRelation (
    HDCode BIGINT PRIMARY KEY,                -- Holiday-Department Relation ID (Primary Key)
    HCode INT NOT NULL,                       -- Holiday Code (FK to HolidayMaster)
    DCode INT NOT NULL,                       -- Department Code (FK to DeptMaster)

    FOREIGN KEY (HCode) REFERENCES HolidayMaster(Hcode),
    FOREIGN KEY (DCode) REFERENCES DeptMaster(Dcode)
);

-- 20. SALARY HEAD MASTER (Salary Components)
CREATE TABLE SalaryHeadMaster (
    SHCode INT PRIMARY KEY,                   -- Salary Head ID (Primary Key)
    SalaryHead VARCHAR(50) NOT NULL,          -- Salary Head Name (Basic, HRA, DA, etc.)
    Description VARCHAR(200),                 -- Description
    Type VARCHAR(10) NOT NULL,                -- Type (Earning/Deduction)
    ExpenseToComp BIT NOT NULL,               -- Expense to Company
    FixedOrCalculated BIT NOT NULL DEFAULT 0, -- Fixed or Calculated
    IsRoundable BIT NOT NULL DEFAULT 1,       -- Is Roundable
    IsBasic BIT NOT NULL DEFAULT 0            -- Is Basic Salary Component
);

-- 21. EMPLOYEE SALARY HEAD RELATION (Employee Salary Structure)
CREATE TABLE EmployeeSalaryHeadRelation (
    ECode INT NOT NULL,                       -- Employee Code (Primary Key)
    SHCode INT NOT NULL,                      -- Salary Head Code (Primary Key)
    GradeValue DECIMAL,                       -- Grade-based Value
    Value DECIMAL NOT NULL,                   -- Actual Value/Amount
    IsValuePercent BIT NOT NULL DEFAULT 0,    -- Is Value in Percentage

    PRIMARY KEY (ECode, SHCode),
    FOREIGN KEY (ECode) REFERENCES EmployeeMaster(Ecode),
    FOREIGN KEY (SHCode) REFERENCES SalaryHeadMaster(SHCode)
);

-- 22. PAY SLIP TABLE
CREATE TABLE PaySlip (
    Id INT PRIMARY KEY,                       -- PaySlip ID (Primary Key)
    Ecode INT,                                -- Employee Code
    Month VARCHAR(25),                        -- Month
    Year VARCHAR(6),                          -- Year
    StartDate DATETIME,                       -- Pay Period Start
    EndDate DATETIME,                         -- Pay Period End
    PayDate DATETIME,                         -- Payment Date
    SalaryGrade VARCHAR(25),                  -- Salary Grade
    TotalEarnings DECIMAL,                    -- Total Earnings
    TotalDeductions DECIMAL,                  -- Total Deductions
    NetPay DECIMAL,                           -- Net Pay Amount
    DaysWorked NUMERIC,                       -- Days Worked
    IsVerified BIT                            -- Verification Status
);
```

**Oryggi Schema Mapping Configuration**:

```typescript
// Actual Oryggi HRMS schema mapping for Complaint Management System
interface OryggiHRMSMapping {
  database: {
    server: string;
    database_name: string;
    type: 'mssql';
  };

  employee_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      employee_id: string;
      employee_code: string;
      full_name: string;
      first_name: string;
      last_name: string;
      email: string;
      phone_primary: string;
      phone_secondary: string;
      manager_id: string;
      grade_id: string;
      category_id: string;
      designation_id: string;
      section_id: string;
      is_active: string;
      date_of_birth: string;
      date_of_joining: string;
      leaving_date: string;
      gender: string;
      role: string;
      user_id: string;
    };
  };

  company_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      company_id: string;
      company_name: string;
      address: string;
      email: string;
      phone: string;
    };
  };

  branch_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      branch_id: string;
      branch_name: string;
      company_id: string;
      location: string;
    };
  };

  department_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      department_id: string;
      department_name: string;
      branch_id: string;
    };
  };

  section_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      section_id: string;
      section_name: string;
      department_id: string;
    };
  };

  designation_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      designation_id: string;
      designation_name: string;
    };
  };

  grade_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      grade_id: string;
      grade_name: string;
    };
  };

  category_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      category_id: string;
      category_name: string;
    };
  };

  reporting_hierarchy: {
    primary_table: string;
    multi_reporting_table: string;
    field_mapping: {
      employee_id: string;
      reporting_head_id: string;
      remark: string;
    };
  };

  leave_master_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      leave_type_id: string;
      leave_short_name: string;
      leave_description: string;
      is_off_include: string;
      is_holiday_include: string;
      treated_as_present: string;
      leave_limit: string;
      is_auto_accrual: string;
      is_carried_forward: string;
      max_continuous_leave: string;
    };
  };

  leave_amount_type_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      leave_amount_type_id: string;
      leave_amount_type_name: string;
      leave_amount_type_value: string;
      status: string;
    };
  };

  leave_application_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      leave_card_id: string;
      leave_accural_id: string;
      leave_type_id: string;
      application_date: string;
      start_date: string;
      end_date: string;
      leave_amount: string;
      leave_amount_type: string;
      day_segment: string;
      leave_reason: string;
      status: string;
      approval_date: string;
      approved_by: string;
    };
  };

  leave_balance_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      leave_accural_id: string;
      employee_id: string;
      leave_type_id: string;
      allowed_current_year: string;
      allowed_previous_year: string;
      leave_taken: string;
      leave_year: string;
    };
  };

  holiday_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      holiday_id: string;
      holiday_name: string;
      holiday_date: string;
    };
  };

  holiday_department_relation_table: {
    table_name: string;
    primary_key: string;
    field_mapping: {
      relation_id: string;
      holiday_id: string;
      department_id: string;
    };
  };
}

// Actual Oryggi HRMS Schema Mapping
const ORYGGI_HRMS_MAPPING: OryggiHRMSMapping = {
  database: {
    server: 'LAPTOP-NF9BTG7Q\\SQLEXPRESS',
    database_name: 'Oryggi',
    type: 'mssql'
  },

  employee_table: {
    table_name: 'EmployeeMaster',
    primary_key: 'Ecode',
    field_mapping: {
      employee_id: 'Ecode',                    // INT Primary Key
      employee_code: 'CorpEmpCode',            // VARCHAR(20)
      full_name: 'EmpName',                    // NVARCHAR(50)
      first_name: 'FName',                     // NVARCHAR(100)
      last_name: 'LName',                      // NVARCHAR(100)
      email: 'E_mail',                         // VARCHAR(50)
      phone_primary: 'Telephone1',             // VARCHAR(50)
      phone_secondary: 'Telephone2',           // VARCHAR(50)
      manager_id: 'ReportingHeadEcode',        // INT (FK)
      grade_id: 'Gcode',                       // INT (FK)
      category_id: 'Catcode',                  // INT (FK)
      designation_id: 'DesCode',               // INT (FK)
      section_id: 'SecCode',                   // INT (FK)
      is_active: 'Active',                     // BIT (1=Active, 0=Inactive)
      date_of_birth: 'DateofBirth',            // DATETIME
      date_of_joining: 'DateofJoin',           // DATETIME
      leaving_date: 'LeavingDate',             // DATETIME
      gender: 'Sex',                           // BIT (1=Male, 0=Female)
      role: 'Role',                            // VARCHAR(20)
      user_id: 'UserID'                        // VARCHAR(20)
    }
  },

  company_table: {
    table_name: 'CompanyMaster',
    primary_key: 'Ccode',
    field_mapping: {
      company_id: 'Ccode',                     // INT Primary Key
      company_name: 'CName',                   // VARCHAR(100)
      address: 'Address',                      // VARCHAR(200)
      email: 'Email',                          // VARCHAR(50)
      phone: 'TelephoneNo'                     // VARCHAR(15)
    }
  },

  branch_table: {
    table_name: 'BranchMaster',
    primary_key: 'BranchCode',
    field_mapping: {
      branch_id: 'BranchCode',                 // INT Primary Key
      branch_name: 'BranchName',               // VARCHAR(50)
      company_id: 'Ccode',                     // INT (FK to CompanyMaster)
      location: 'Location'                     // VARCHAR(100)
    }
  },

  department_table: {
    table_name: 'DeptMaster',
    primary_key: 'Dcode',
    field_mapping: {
      department_id: 'Dcode',                  // INT Primary Key
      department_name: 'Dname',                // VARCHAR(50)
      branch_id: 'BranchCode'                  // INT (FK to BranchMaster)
    }
  },

  section_table: {
    table_name: 'SectionMaster',
    primary_key: 'SecCode',
    field_mapping: {
      section_id: 'SecCode',                   // INT Primary Key
      section_name: 'SecName',                 // VARCHAR(50)
      department_id: 'Dcode'                   // INT (FK to DeptMaster)
    }
  },

  designation_table: {
    table_name: 'DesignationMaster',
    primary_key: 'DesCode',
    field_mapping: {
      designation_id: 'DesCode',               // INT Primary Key
      designation_name: 'DesName'              // VARCHAR(50)
    }
  },

  grade_table: {
    table_name: 'GradeMaster',
    primary_key: 'Gcode',
    field_mapping: {
      grade_id: 'Gcode',                       // INT Primary Key
      grade_name: 'Gname'                      // VARCHAR(50)
    }
  },

  category_table: {
    table_name: 'CatMaster',
    primary_key: 'CatCode',
    field_mapping: {
      category_id: 'CatCode',                  // INT Primary Key
      category_name: 'CatName'                 // VARCHAR(MAX)
    }
  },

  reporting_hierarchy: {
    primary_table: 'ReportingHeadMaster',
    multi_reporting_table: 'MultiReportingHeadRelation',
    field_mapping: {
      employee_id: 'ECode',                    // INT
      reporting_head_id: 'ReportingHeadEcode', // INT
      remark: 'Remark'                         // NVARCHAR(200) - in multi table
    }
  },

  leave_master_table: {
    table_name: 'LeaveMaster',
    primary_key: 'Lcode',
    field_mapping: {
      leave_type_id: 'Lcode',                  // INT Primary Key
      leave_short_name: 'LshortName',          // VARCHAR(15)
      leave_description: 'Ldescription',       // VARCHAR(50)
      is_off_include: 'IsOffInclude',          // BIT
      is_holiday_include: 'IsHolidayInclude',  // BIT
      treated_as_present: 'TreatedAsPresent',  // BIT
      leave_limit: 'LeaveLimit',               // TINYINT
      is_auto_accrual: 'IsAutoAcuralAllowed',  // BIT
      is_carried_forward: 'IsCarriedForward',  // BIT
      max_continuous_leave: 'MaxContinuousLeave' // TINYINT
    }
  },

  leave_amount_type_table: {
    table_name: 'LeaveAmountTypeMaster',
    primary_key: 'LeaveAmountTypeID',
    field_mapping: {
      leave_amount_type_id: 'LeaveAmountTypeID',     // INT Primary Key
      leave_amount_type_name: 'LeaveAmountTypeName', // VARCHAR(100) UNIQUE
      leave_amount_type_value: 'LeaveAmountTypeValue', // DECIMAL
      status: 'Status'                         // BIT (1=Active, 0=Inactive)
    }
  },

  leave_application_table: {
    table_name: 'LeaveCard',
    primary_key: 'LCCode',
    field_mapping: {
      leave_card_id: 'LCCode',                 // INT Primary Key
      leave_accural_id: 'LACode',              // INT (FK)
      leave_type_id: 'Lcode',                  // INT (FK)
      application_date: 'ApplicationDate',     // DATETIME
      start_date: 'StartDate',                 // DATETIME
      end_date: 'EndDate',                     // DATETIME
      leave_amount: 'LeaveAmount',             // DECIMAL
      leave_amount_type: 'LeaveAmtType',       // TINYINT
      day_segment: 'DaySegment',               // TINYINT
      leave_reason: 'LeaveReason',             // VARCHAR(500)
      status: 'Status',                        // TINYINT (0=Cancelled, 1=Pending, 2=Approved, 3=Rejected)
      approval_date: 'ApprovalRejectionDate',  // DATETIME
      approved_by: 'ApprovalRejectionBy'       // VARCHAR(50)
    }
  },

  leave_balance_table: {
    table_name: 'LeaveAccural',
    primary_key: 'LACode',
    field_mapping: {
      leave_accural_id: 'LACode',              // INT Primary Key
      employee_id: 'ECode',                    // INT (FK)
      leave_type_id: 'LCode',                  // INT (FK)
      allowed_current_year: 'LeaveAllowedCYear', // DECIMAL
      allowed_previous_year: 'LeaveAllowedPYear', // DECIMAL
      leave_taken: 'LeaveTaken',               // DECIMAL
      leave_year: 'LeaveYear'                  // DATETIME
    }
  },

  holiday_table: {
    table_name: 'HolidayMaster',
    primary_key: 'Hcode',
    field_mapping: {
      holiday_id: 'Hcode',                     // INT Primary Key
      holiday_name: 'Hname',                   // VARCHAR(50)
      holiday_date: 'HDate'                    // DATETIME
    }
  },

  holiday_department_relation_table: {
    table_name: 'HolidayDepartmentRelation',
    primary_key: 'HDCode',
    field_mapping: {
      relation_id: 'HDCode',                   // BIGINT Primary Key
      holiday_id: 'HCode',                     // INT (FK to HolidayMaster)
      department_id: 'DCode'                   // INT (FK to DeptMaster)
    }
  }
};

// SQL Server Connection Configuration
const MSSQL_CONNECTION_CONFIG = {
  server: 'LAPTOP-NF9BTG7Q\\SQLEXPRESS',
  database: 'Oryggi',
  user: 'sa',
  password: process.env.HRMS_DB_PASSWORD, // Store in environment variable
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

**Organization Hierarchy Structure in Oryggi**:

```
Company (CompanyMaster)
    └─► Branch (BranchMaster)
            └─► Department (DeptMaster)
                    └─► Section (SectionMaster)
                            └─► Employee (EmployeeMaster)
                                    ├─ Grade (GradeMaster)
                                    ├─ Category (CatMaster)
                                    ├─ Designation (DesignationMaster)
                                    └─ Reporting Head (ReportingHeadMaster/MultiReportingHeadRelation)
```

#### Oryggi Integration Strategy

**Architecture Principle**: The Complaint Management System follows a **dual-table architecture**:

1. **Master Data from Oryggi (Read-Only Sync)**
   - Employee, Company, Branch, Department, Section data is **synced** from Oryggi
   - Complaint system maintains local **read-only copies** with foreign key mappings
   - Any changes in Oryggi **automatically reflect** in Complaint System
   - Oryggi remains the **single source of truth** for organizational data

2. **Complaint-Specific Roles (Independent Management)**
   - Complaint roles and permissions are **managed independently**
   - Role tables are **specific to complaint module**
   - Roles **reference** synced Oryggi users via `oryggi_employee_id`
   - Administrators can assign complaint roles without touching Oryggi

**Data Flow**:
```
Oryggi Database (Source of Truth)
        ↓ (Real-time Sync / Scheduled Sync)
Complaint System Master Tables (Read-Only)
        ↓ (References)
Complaint Role Tables (Fully Managed)
```

**Key Benefits**:
- ✅ Oryggi changes automatically reflect in complaint system
- ✅ No data duplication conflicts
- ✅ Independent complaint role management
- ✅ Branch/Department/Section-wise role assignments
- ✅ No impact on Oryggi database structure

#### Data Synchronization from Oryggi

##### Real-time Webhooks (Preferred Method)
```typescript
@Controller('webhooks/hrms')
export class HRMSWebhookController {

  @Post('employee/created')
  async handleEmployeeCreated(@Body() payload: HRMSWebhook) {
    await this.syncService.createEmployee(payload.data);
  }

  @Post('employee/updated')
  async handleEmployeeUpdated(@Body() payload: HRMSWebhook) {
    const { employee_id, changes } = payload.data;

    await this.syncService.updateEmployee(employee_id, changes);

    // Re-evaluate complaints if org structure changed
    if (changes.manager_id || changes.department_id || changes.branch_id) {
      await this.complaintService.reevaluateAssignments(employee_id);
    }
  }

  @Post('employee/terminated')
  async handleEmployeeTerminated(@Body() payload: HRMSWebhook) {
    await this.syncService.deactivateEmployee(payload.data.employee_id);

    // Reassign active complaints
    await this.complaintService.reassignFromTerminatedEmployee(
      payload.data.employee_id
    );
  }

  @Post('organization/structure-changed')
  async handleStructureChange(@Body() payload: HRMSWebhook) {
    // Branch, department, or section changes
    await this.syncService.syncOrganizationStructure();
  }
}
```

##### Scheduled Batch Sync (Fallback)
```typescript
@Injectable()
export class HRMSSyncService {

  @Cron('0 */6 * * *') // Every 6 hours
  async syncMasterData() {
    console.log('Starting HRMS sync...');

    try {
      // Fetch from HRMS API
      const hrmsData = await this.hrmsApiClient.getMasterData();

      // Sync companies
      await this.syncCompanies(hrmsData.companies);

      // Sync branches
      await this.syncBranches(hrmsData.branches);

      // Sync departments
      await this.syncDepartments(hrmsData.departments);

      // Sync sections
      await this.syncSections(hrmsData.sections);

      // Sync employees
      await this.syncEmployees(hrmsData.employees);

      console.log('HRMS sync completed successfully');
    } catch (error) {
      console.error('HRMS sync failed:', error);
      // Alert admin
      await this.alertService.notifyAdmin('HRMS sync failed', error);
    }
  }

  private async syncEmployees(employees: HRMSEmployee[]) {
    for (const emp of employees) {
      await this.db.users.upsert({
        where: { employee_code: emp.code },
        update: {
          email: emp.email,
          first_name: emp.first_name,
          last_name: emp.last_name,
          branch_id: emp.branch_id,
          department_id: emp.department_id,
          section_id: emp.section_id,
          manager_id: emp.manager_id,
          role: emp.role,
          is_active: emp.is_active,
          last_synced_at: new Date()
        },
        create: {
          tenant_id: emp.tenant_id,
          company_id: emp.company_id,
          employee_code: emp.code,
          email: emp.email,
          first_name: emp.first_name,
          last_name: emp.last_name,
          branch_id: emp.branch_id,
          department_id: emp.department_id,
          section_id: emp.section_id,
          manager_id: emp.manager_id,
          role: emp.role,
          is_active: emp.is_active,
          last_synced_at: new Date()
        }
      });
    }
  }
}
```

#### Complaint Role Management (Independent from Oryggi)

The Complaint Management System maintains its own role-based access control that **references** synced Oryggi users but is **managed independently**.

##### Assigning Complaint Roles to Oryggi Users

```typescript
@Injectable()
export class ComplaintRoleService {

  // Assign HR Admin role to a user for specific branch
  async assignBranchHRAdmin(userId: string, branchId: string, assignedBy: string) {
    const hrAdminRole = await this.db.complaint_roles.findOne({
      where: { role_code: 'HR_ADMIN' }
    });

    await this.db.user_complaint_roles.create({
      user_id: userId,           // References synced Oryggi user
      role_id: hrAdminRole.role_id,
      branch_id: branchId,       // Scope to specific branch
      assigned_by: assignedBy,
      is_active: true
    });
  }

  // Assign Department Head role
  async assignDepartmentHead(userId: string, departmentId: string) {
    const deptHeadRole = await this.db.complaint_roles.findOne({
      where: { role_code: 'DEPT_HEAD' }
    });

    await this.db.user_complaint_roles.create({
      user_id: userId,
      role_id: deptHeadRole.role_id,
      department_id: departmentId, // Scope to specific department
      is_active: true
    });
  }

  // Get user's complaint roles with org scope
  async getUserComplaintRoles(userId: string) {
    const roles = await this.db.user_complaint_roles.findAll({
      where: { user_id: userId, is_active: true },
      include: [
        { model: 'complaint_roles' },
        { model: 'companies' },
        { model: 'branches' },
        { model: 'departments' },
        { model: 'sections' }
      ]
    });

    return roles;
  }

  // Check if user has permission for specific action
  async checkPermission(
    userId: string,
    module: string,
    resource: string,
    action: string,
    context?: { branchId?: string; departmentId?: string }
  ): Promise<boolean> {
    const userRoles = await this.getUserComplaintRoles(userId);

    for (const userRole of userRoles) {
      // Check scope match
      if (context?.branchId && userRole.branch_id !== context.branchId) {
        continue;
      }
      if (context?.departmentId && userRole.department_id !== context.departmentId) {
        continue;
      }

      // Check permission
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
        return true;
      }
    }

    return false;
  }
}
```

##### Example: Role Assignment Scenarios

```typescript
// Scenario 1: Assign HR Manager for Mumbai Branch
const mumbaiHRManager = await usersService.findByEmployeeCode('EMP001'); // Synced from Oryggi
const mumbaiBranch = await branchesService.findByCode('MUM001');         // Synced from Oryggi

await complaintRoleService.assignBranchHRAdmin(
  mumbaiHRManager.user_id,
  mumbaiBranch.branch_id,
  'admin-user-id'
);

// Scenario 2: Assign Department Head for IT Department
const itDeptHead = await usersService.findByEmployeeCode('EMP002');
const itDepartment = await departmentsService.findByCode('IT001');

await complaintRoleService.assignDepartmentHead(
  itDeptHead.user_id,
  itDepartment.department_id
);

// Scenario 3: Assign Escalation Handler for multiple branches
const escalationHandler = await usersService.findByEmployeeCode('EMP003');
const branches = ['MUM001', 'DEL001', 'BLR001'];

for (const branchCode of branches) {
  const branch = await branchesService.findByCode(branchCode);
  await complaintRoleService.assignEscalationHandler(
    escalationHandler.user_id,
    branch.branch_id
  );
}

// Scenario 4: Check if user can approve complaints in their department
const canApprove = await complaintRoleService.checkPermission(
  currentUser.user_id,
  'COMPLAINTS',
  'complaint',
  'APPROVE',
  { departmentId: complaint.department_id }
);

if (canApprove) {
  await complaintService.approveComplaint(complaintId);
}
```

##### Syncing Oryggi Changes Impact on Roles

```typescript
@Injectable()
export class OryggiSyncImpactService {

  // When employee transfers to new department
  async handleEmployeeTransfer(employeeId: string, newDepartmentId: string) {
    // 1. Update synced user data (automatic from Oryggi sync)
    const user = await this.db.users.findOne({
      where: { oryggi_employee_id: employeeId }
    });

    // 2. Check if user has department-scoped complaint roles
    const deptRoles = await this.db.user_complaint_roles.findAll({
      where: {
        user_id: user.user_id,
        department_id: { [Op.not]: null }
      }
    });

    // 3. Optionally notify admin about role reassignment needed
    if (deptRoles.length > 0) {
      await this.notificationService.notifyAdmin({
        type: 'ROLE_REVIEW_REQUIRED',
        message: `Employee ${user.employee_code} transferred to new department. Review complaint roles.`,
        user_id: user.user_id,
        roles_affected: deptRoles.length
      });
    }

    // 4. Auto-reassign complaints if user was handling any
    await this.complaintService.reevaluateAssignedComplaints(user.user_id);
  }

  // When employee is deactivated in Oryggi
  async handleEmployeeDeactivation(employeeId: string) {
    const user = await this.db.users.findOne({
      where: { oryggi_employee_id: employeeId }
    });

    // 1. Deactivate user in complaint system (synced automatically)
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
}
```

#### Pre-population on Complaint Creation
```typescript
@Injectable()
export class ComplaintService {

  async createComplaint(
    dto: CreateComplaintDto,
    employeeId: string
  ): Promise<Complaint> {
    // Fetch employee context from HRMS
    const context = await this.hrmsService.getEmployeeContext(employeeId);

    const complaint = await this.db.complaints.create({
      data: {
        ...dto,
        employee_id: employeeId,
        company_id: context.company_id,
        branch_id: context.branch_id,
        department_id: context.department_id,
        section_id: context.section_id,

        // Pre-populate data
        pre_populated_data: {
          recent_attendance: context.attendance_last_30_days,
          recent_leaves: context.leaves_last_30_days,
          salary_info: context.current_salary_cycle,
          shift_info: context.current_shift
        }
      }
    });

    // Initialize escalation
    await this.escalationEngine.initializeEscalation(complaint);

    return complaint;
  }
}
```

### 2. Attendance System Integration

```typescript
@Injectable()
export class AttendanceIntegrationService {

  async getEmployeeAttendance(
    employeeId: string,
    fromDate: Date,
    toDate: Date
  ): Promise<AttendanceRecord[]> {
    return await this.attendanceApi.getRecords({
      employee_id: employeeId,
      from: fromDate,
      to: toDate
    });
  }

  async markManualAttendance(
    complaintId: string,
    date: Date,
    inTime: string,
    outTime: string
  ): Promise<void> {
    const complaint = await this.db.complaints.findUnique({
      where: { complaint_id: complaintId },
      include: { employee: true }
    });

    // Call attendance API to mark manual attendance
    await this.attendanceApi.markManual({
      employee_id: complaint.employee_id,
      date: date,
      in_time: inTime,
      out_time: outTime,
      reason: `Complaint #${complaint.complaint_number}`,
      approved_by: complaint.resolved_by
    });
  }
}
```

### 3. Payroll System Integration

```typescript
@Injectable()
export class PayrollIntegrationService {

  async getSalaryInfo(
    employeeId: string,
    month: number,
    year: number
  ): Promise<SalaryInfo> {
    return await this.payrollApi.getSalarySlip({
      employee_id: employeeId,
      month: month,
      year: year
    });
  }

  async raisePayrollAdjustment(
    complaintId: string,
    adjustmentDetails: PayrollAdjustment
  ): Promise<void> {
    const complaint = await this.db.complaints.findUnique({
      where: { complaint_id: complaintId }
    });

    // Create adjustment request in payroll system
    await this.payrollApi.createAdjustment({
      employee_id: complaint.employee_id,
      type: adjustmentDetails.type,
      amount: adjustmentDetails.amount,
      reason: `Complaint #${complaint.complaint_number}`,
      approved_by: complaint.resolved_by
    });
  }
}
```

---

## Security & Compliance

### Authentication & Authorization

#### JWT-based Authentication
```typescript
@Injectable()
export class AuthService {

  async login(email: string, password: string): Promise<AuthResponse> {
    // Verify credentials
    const user = await this.verifyCredentials(email, password);

    // Generate JWT
    const payload = {
      sub: user.user_id,
      email: user.email,
      role: user.role,
      tenant_id: user.tenant_id,
      company_id: user.company_id,
      branch_id: user.branch_id
    };

    const accessToken = this.jwtService.sign(payload, {
      expiresIn: '1h'
    });

    const refreshToken = this.jwtService.sign(
      { sub: user.user_id },
      { expiresIn: '7d' }
    );

    return { accessToken, refreshToken, user };
  }
}
```

#### SSO Integration (SAML 2.0)
```typescript
@Injectable()
export class SamlAuthService {

  async handleSAMLCallback(
    profile: SamlProfile
  ): Promise<AuthResponse> {
    // Map SAML attributes to user
    const user = await this.findOrCreateUser({
      email: profile.email,
      first_name: profile.firstName,
      last_name: profile.lastName,
      employee_code: profile.employeeId
    });

    // Generate JWT
    return await this.authService.generateTokens(user);
  }
}
```

#### Role-Based Access Control (RBAC)
```typescript
@Injectable()
export class RBACGuard implements CanActivate {

  async canActivate(context: ExecutionContext): Promise<boolean> {
    const request = context.switchToHttp().getRequest();
    const user = request.user;
    const resource = request.params.complaintId;

    // Get complaint
    const complaint = await this.db.complaints.findUnique({
      where: { complaint_id: resource }
    });

    // Check permissions
    return this.checkPermission(user, complaint, 'view');
  }

  private checkPermission(
    user: User,
    complaint: Complaint,
    action: string
  ): boolean {
    // Employee can view own complaints
    if (user.role === 'EMPLOYEE') {
      return complaint.employee_id === user.user_id;
    }

    // Manager can view team complaints
    if (user.role === 'MANAGER') {
      return this.isInReportingChain(user.user_id, complaint.employee_id);
    }

    // Branch HR can view branch complaints
    if (user.role === 'BRANCH_HR') {
      return complaint.branch_id === user.branch_id;
    }

    // Central HR and Admin have full access
    if (['CENTRAL_HR', 'ADMIN'].includes(user.role)) {
      return complaint.tenant_id === user.tenant_id;
    }

    return false;
  }
}
```

### Data Encryption

#### At Rest
- Database encryption using PostgreSQL pgcrypto
- File encryption in S3 (AES-256)
- Sensitive field encryption (PII)

#### In Transit
- TLS 1.3 for all API communication
- Certificate pinning for mobile apps
- Encrypted WebSocket connections

### Audit Logging

```typescript
@Injectable()
export class AuditService {

  async logAction(
    userId: string,
    action: string,
    resourceType: string,
    resourceId: string,
    changes: any,
    request: Request
  ): Promise<void> {
    await this.db.audit_logs.create({
      data: {
        tenant_id: request.user.tenant_id,
        user_id: userId,
        action: action,
        resource_type: resourceType,
        resource_id: resourceId,
        ip_address: request.ip,
        user_agent: request.headers['user-agent'],
        changes: changes
      }
    });
  }
}
```

### Compliance

#### GDPR Compliance
- Right to access (data export)
- Right to erasure (data deletion)
- Data portability
- Consent management
- Data retention policies

#### SOC 2 Compliance
- Access controls
- Audit logging
- Encryption
- Incident response
- Change management

---

## Deployment Architecture

### Cloud-Native Deployment (Kubernetes)

```yaml
# Deployment Configuration
apiVersion: apps/v1
kind: Deployment
metadata:
  name: complaint-management-api
  namespace: production
spec:
  replicas: 3
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 1
      maxUnavailable: 0

  template:
    spec:
      containers:
      - name: api
        image: complaint-api:v1.0.0
        ports:
        - containerPort: 3000

        env:
        - name: NODE_ENV
          value: "production"
        - name: DATABASE_URL
          valueFrom:
            secretKeyRef:
              name: db-credentials
              key: url
        - name: REDIS_URL
          valueFrom:
            secretKeyRef:
              name: redis-credentials
              key: url

        resources:
          requests:
            memory: "512Mi"
            cpu: "500m"
          limits:
            memory: "1Gi"
            cpu: "1000m"

        livenessProbe:
          httpGet:
            path: /health
            port: 3000
          initialDelaySeconds: 30
          periodSeconds: 10

        readinessProbe:
          httpGet:
            path: /ready
            port: 3000
          initialDelaySeconds: 10
          periodSeconds: 5
```

### Multi-Region Setup

```yaml
regions:
  primary:
    region: us-east-1
    services:
      - api
      - database (primary)
      - cache
      - storage

  secondary:
    region: eu-west-1
    services:
      - api
      - database (read-replica)
      - cache
      - storage

  tertiary:
    region: ap-south-1
    services:
      - api
      - database (read-replica)
      - cache
      - storage

# Data Residency Routing
routing_rules:
  - tenant: eu_company_001
    route_to: eu-west-1

  - tenant: us_company_002
    route_to: us-east-1

  - tenant: in_company_003
    route_to: ap-south-1
```

### CI/CD Pipeline

```yaml
# GitHub Actions Workflow
name: Deploy to Production

on:
  push:
    branches:
      - main

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Build Docker Image
        run: |
          docker build -t complaint-api:${{ github.sha }} .

      - name: Run Tests
        run: |
          docker run complaint-api:${{ github.sha }} npm test

      - name: Security Scan
        run: |
          trivy image complaint-api:${{ github.sha }}

      - name: Push to Registry
        run: |
          docker tag complaint-api:${{ github.sha }} ecr.../complaint-api:latest
          docker push ecr.../complaint-api:latest

  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Deploy to Kubernetes
        run: |
          kubectl set image deployment/complaint-api api=ecr.../complaint-api:latest
          kubectl rollout status deployment/complaint-api
```

---

## Implementation Roadmap

### Phase 1: Foundation (Months 1-2)

#### Milestones
- [x] Project setup and repository structure
- [ ] Database schema implementation
- [ ] Multi-tenant architecture setup
- [ ] Authentication and SSO integration
- [ ] Basic RBAC implementation
- [ ] HRMS integration (master data sync)
- [ ] Basic complaint CRUD operations

#### Deliverables
- Working authentication system
- Master data synchronized from HRMS
- Basic complaint creation and viewing
- Development environment setup

---

### Phase 2: Core Features (Months 3-4)

#### Milestones
- [ ] Escalation engine implementation
- [ ] SLA tracking and auto-escalation
- [ ] Workflow service
- [ ] Notification system (email, SMS, push)
- [ ] File upload and storage
- [ ] Comment system
- [ ] Basic dashboard (employee, manager)

#### Deliverables
- Functional escalation system
- Automated notifications
- Employee and manager portals
- File attachment support

---

### Phase 3: Advanced Features (Months 5-6)

#### Milestones
- [ ] No-code admin configuration console
- [ ] Visual escalation matrix builder
- [ ] Self-service knowledge base
- [ ] AI-powered suggestions
- [ ] Post-resolution surveys
- [ ] Advanced analytics and reporting
- [ ] Mobile app (PWA or React Native)

#### Deliverables
- Admin configuration interface
- Knowledge base with search
- Survey system
- Analytics dashboards
- Mobile application

---

### Phase 4: Enterprise Features (Months 7-8)

#### Milestones
- [ ] White-labeling support
- [ ] Multi-language support
- [ ] Advanced reporting and exports
- [ ] Geo-fencing and data residency
- [ ] Compliance features (GDPR, SOC 2)
- [ ] Advanced audit logging

#### Deliverables
- Multi-tenant with white-labeling
- Localization support
- Compliance-ready features
- Advanced audit trails

---

### Phase 5: Scale & Optimize (Months 9-10)

#### Milestones
- [ ] Performance optimization
- [ ] Load testing and auto-scaling
- [ ] Security audit and penetration testing
- [ ] Documentation and training materials
- [ ] Production deployment
- [ ] User acceptance testing
- [ ] Go-live

#### Deliverables
- Production-ready system
- Performance benchmarks met
- Security audit passed
- User training completed
- System documentation

---

## Appendix

### A. Key Metrics to Track

#### Operational Metrics
- Total complaints (by category, branch, department)
- Average resolution time
- SLA compliance rate
- Escalation rate
- Auto-escalation vs. manual escalation
- Complaints by status
- Open complaints by assignee

#### User Experience Metrics
- Complaint deflection rate (via KB)
- Employee satisfaction (CSAT)
- Net Promoter Score (NPS)
- Time to first response
- Time to resolution

#### System Performance Metrics
- API response time
- Database query performance
- Cache hit rate
- Notification delivery rate
- System uptime

### B. API Endpoints (Summary)

```
Authentication
POST   /api/auth/login
POST   /api/auth/logout
POST   /api/auth/refresh
GET    /api/auth/me

Complaints
POST   /api/complaints
GET    /api/complaints
GET    /api/complaints/:id
PATCH  /api/complaints/:id
DELETE /api/complaints/:id
POST   /api/complaints/:id/comments
POST   /api/complaints/:id/attachments
POST   /api/complaints/:id/escalate
POST   /api/complaints/:id/resolve
POST   /api/complaints/:id/close

Escalation Matrices
POST   /api/escalation-matrices
GET    /api/escalation-matrices
GET    /api/escalation-matrices/:id
PATCH  /api/escalation-matrices/:id
DELETE /api/escalation-matrices/:id

Categories
POST   /api/categories
GET    /api/categories
PATCH  /api/categories/:id
DELETE /api/categories/:id

Knowledge Base
POST   /api/kb
GET    /api/kb
GET    /api/kb/:id
GET    /api/kb/search
POST   /api/kb/:id/feedback

Analytics
GET    /api/analytics/dashboard
GET    /api/analytics/complaints
GET    /api/analytics/escalations
GET    /api/analytics/sla-compliance
GET    /api/analytics/trends

Admin
GET    /api/admin/users
GET    /api/admin/audit-logs
GET    /api/admin/system-config
PATCH  /api/admin/system-config
```

---

## Document History

| Version | Date       | Author | Changes |
|---------|------------|--------|---------|
| 1.0     | 2025-10-11 | Team   | Initial complete architecture document |
| 2.0     | 2025-10-11 | Team   | Added comprehensive Email Alert Configuration System including:<br>- 7 new database tables for alert management<br>- Alert type and template management<br>- Recipient rules engine<br>- Scheduling and batching configuration<br>- User preferences<br>- Alert analytics<br>- Admin UI specifications<br>- HRMS schema mapping placeholders |

---

**END OF DOCUMENT**
