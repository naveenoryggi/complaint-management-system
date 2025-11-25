# COMPLAINT MANAGEMENT SYSTEM - COMPLETE PLANNING & ARCHITECTURE

**Document Version**: 2.0 (Final)
**Last Updated**: 2025-10-11
**Project**: HRMS Complaint Management Module
**Database**: Oryggi (SQL Server Express)
**Integration**: Dual-table architecture with Oryggi HRMS

---

## DOCUMENT STRUCTURE

This master planning document is organized into separate detailed chunks for better readability and maintenance. Each chunk is a standalone file with comprehensive documentation.

### Available Chunks

| Chunk | File | Description | Status |
|-------|------|-------------|--------|
| **Chunks 1-2** | *This File* | Executive Summary, Requirements, Architecture, Master Data Tables | ✅ Complete |
| **Chunk 3** | [CHUNK_03_COMPLAINT_ROLE_TABLES.md](CHUNK_03_COMPLAINT_ROLE_TABLES.md) | Complaint tables, Role tables, Permission system (7 tables) | ✅ Complete |
| **Chunk 4** | [CHUNK_04_ESCALATION_EMAIL_TABLES.md](CHUNK_04_ESCALATION_EMAIL_TABLES.md) | Escalation matrix, Email alert system (8 tables) | ✅ Complete |
| **Chunk 5** | [CHUNK_05_ORYGGI_INTEGRATION.md](CHUNK_05_ORYGGI_INTEGRATION.md) | Dual-table architecture, Sync mechanisms, Integration strategies | ✅ Complete |
| **Chunk 6** | [CHUNK_06_TECHNOLOGY_STACK.md](CHUNK_06_TECHNOLOGY_STACK.md) | Complete technology stack, tools, infrastructure | ✅ Complete |
| **Chunk 7** | [CHUNK_07_UI_UX_DESIGN.md](CHUNK_07_UI_UX_DESIGN.md) | UI/UX design, user personas, dashboards, components | ✅ Complete |
| **Chunk 8** | [CHUNK_08_SECURITY_DEPLOYMENT.md](CHUNK_08_SECURITY_DEPLOYMENT.md) | Security architecture, deployment strategy, CI/CD | ✅ Complete |

---

## TABLE OF CONTENTS

### Part 1: Overview & Foundation (This Document)
1. [Executive Summary](#executive-summary)
2. [System Overview](#system-overview)
3. [Business Requirements](#business-requirements)
4. [Architecture Design](#architecture-design)
5. [Database Schema - Master Tables](#database-schema)

### Part 2: Detailed Documentation (Separate Files)
6. **Complaint & Role Tables** → [CHUNK_03_COMPLAINT_ROLE_TABLES.md](CHUNK_03_COMPLAINT_ROLE_TABLES.md)
7. **Escalation & Email System** → [CHUNK_04_ESCALATION_EMAIL_TABLES.md](CHUNK_04_ESCALATION_EMAIL_TABLES.md)
8. **Oryggi Integration** → [CHUNK_05_ORYGGI_INTEGRATION.md](CHUNK_05_ORYGGI_INTEGRATION.md)
9. **Technology Stack** → [CHUNK_06_TECHNOLOGY_STACK.md](CHUNK_06_TECHNOLOGY_STACK.md)
10. **UI/UX Design** → [CHUNK_07_UI_UX_DESIGN.md](CHUNK_07_UI_UX_DESIGN.md)
11. **Security & Deployment** → [CHUNK_08_SECURITY_DEPLOYMENT.md](CHUNK_08_SECURITY_DEPLOYMENT.md)

---

## EXECUTIVE SUMMARY

### Project Vision

Develop a world-class **Complaint Management System** as the core component of the HRMS solution, enabling employees to log and track complaints regarding attendance, salary, leave, and other HRMS-related issues with a flexible multi-level escalation mechanism.

### Key Objectives

1. **Employee Self-Service**: Enable employees to easily log complaints about HRMS irregularities
2. **Transparent Escalation**: Implement configurable N-level escalation (2-5 levels) with intelligent routing
3. **Real-time Visibility**: Provide instant visibility to managers, HR teams, and relevant stakeholders
4. **Automated Notifications**: Send email alerts at different escalation levels with customizable templates
5. **Organizational Mapping**: Support company-wise, branch-wise, department-wise, and section-wise user mapping
6. **Intuitive UX**: Deliver world-class, intuitive user interface for all user personas
7. **HRMS Integration**: Seamless integration with Oryggi HRMS database for user and organizational data

### Solution Highlights

✅ **Multi-Tenant Architecture** - Support multiple organizations
✅ **Flexible N-Level Escalation** - Administrator-configurable (2-5 levels)
✅ **6 Assignment Strategies** - Reporting Chain, Specific User, Role, Round Robin, Least Loaded, Group
✅ **Email Alert System** - Customizable templates with dynamic recipient rules
✅ **Oryggi Integration** - Dual-table architecture with automatic sync
✅ **Role-Based Access Control** - Complaint-specific roles independent from Oryggi
✅ **SLA Tracking** - Auto-escalation based on configurable SLAs
✅ **Mobile-Responsive** - Works seamlessly on desktop, tablet, and mobile
✅ **Real-time Dashboard** - Live complaint tracking and analytics
✅ **Audit Trail** - Complete history of all actions and changes

---

## SYSTEM OVERVIEW

### What is the Complaint Management System?

The Complaint Management System is a comprehensive HRMS module that allows employees to:
- Log complaints about attendance irregularities
- Report salary discrepancies
- Raise leave-related issues
- Track complaint status in real-time
- Escalate unresolved issues through defined levels

### Core Features

#### 1. **Complaint Logging**
- **Categories**: Attendance, Salary, Leave, General HRMS Issues
- **Priority Levels**: Low, Medium, High, Critical
- **Attachments**: Support for documents, screenshots, evidence
- **Pre-population**: Auto-fill employee details from Oryggi HRMS
- **Templates**: Pre-defined complaint templates for common issues

#### 2. **Multi-Level Escalation**
- **Configurable Levels**: 2 to 5 escalation levels
- **Assignment Strategies**:
  - Reporting Chain (follows manager hierarchy)
  - Specific User (designated person)
  - Role-based (HR Manager, Branch Head, etc.)
  - Round Robin (distribute evenly)
  - Least Loaded (assign to least busy handler)
  - Group Assignment (team-based)
- **SLA-Based Auto-Escalation**: Automatic escalation on SLA breach
- **Manual Escalation**: Users can escalate before SLA breach

#### 3. **Email Alert System**
- **Alert Types**: 8+ configurable alert types
  - Complaint Created
  - Complaint Assigned
  - Complaint Escalated
  - SLA Breach Warning
  - Complaint Resolved
  - Feedback Request
  - Daily Digest
  - Custom Alerts
- **Template Designer**: Visual email template builder with variables
- **Dynamic Recipients**: 9 recipient types (Assigned User, Manager, Role, Email List, etc.)
- **Scheduling**: Immediate, delayed, or batched emails
- **User Preferences**: Users control their notification settings

#### 4. **Oryggi HRMS Integration**
- **Master Data Sync**: Automatic sync of employees, companies, branches, departments, sections
- **Read-Only Architecture**: Oryggi remains source of truth
- **Real-time Updates**: Changes in Oryggi reflect immediately
- **Dual-Table Design**: Separate complaint roles from Oryggi data
- **No Impact**: Zero changes required in Oryggi database

#### 5. **Role-Based Access Control**
- **Complaint-Specific Roles**: Independent role management
- **Organizational Scope**: Branch/Department/Section-wise assignments
- **Granular Permissions**: Module, Resource, Action-level control
- **System Roles**: 7 pre-defined roles (Admin, HR Manager, Employee, etc.)
- **Custom Roles**: Administrators can create custom roles

#### 6. **Reporting & Analytics**
- **Real-time Dashboards**: Live complaint metrics
- **Trend Analysis**: Identify patterns and recurring issues
- **Performance Metrics**: Resolution time, SLA compliance, user satisfaction
- **Custom Reports**: Export data in multiple formats
- **Scheduled Reports**: Auto-generated and emailed reports

---

## BUSINESS REQUIREMENTS

### Functional Requirements

#### FR-1: Complaint Creation
- **FR-1.1**: Employees must be able to log complaints with category, priority, description, and attachments
- **FR-1.2**: System must auto-populate employee details from Oryggi HRMS
- **FR-1.3**: System must validate complaint data before submission
- **FR-1.4**: Employees must receive confirmation with unique complaint number

#### FR-2: Complaint Assignment
- **FR-2.1**: System must support 6 assignment strategies (Reporting Chain, Specific User, Role, Round Robin, Least Loaded, Group)
- **FR-2.2**: Administrator must be able to configure assignment rules per complaint category
- **FR-2.3**: System must support company/branch/department/section-wise assignment
- **FR-2.4**: Assigned users must be notified immediately via email

#### FR-3: Escalation Management
- **FR-3.1**: Administrator must be able to configure 2-5 escalation levels
- **FR-3.2**: Each level must support custom assignment strategy
- **FR-3.3**: System must auto-escalate on SLA breach
- **FR-3.4**: Users must be able to manually escalate complaints
- **FR-3.5**: Escalation must trigger email alerts to new assignees

#### FR-4: Email Notifications
- **FR-4.1**: Administrator must be able to define alert types with email templates
- **FR-4.2**: Templates must support dynamic variables (complaint number, employee name, etc.)
- **FR-4.3**: System must support 9 recipient types with dynamic resolution
- **FR-4.4**: Users must be able to set their notification preferences
- **FR-4.5**: System must support email scheduling and batching

#### FR-5: Oryggi Integration
- **FR-5.1**: System must sync employee data from Oryggi.EmployeeMaster
- **FR-5.2**: System must sync organizational structure (Company, Branch, Department, Section)
- **FR-5.3**: Changes in Oryggi must reflect in complaint system within 5 minutes
- **FR-5.4**: System must handle employee transfers and deactivations gracefully
- **FR-5.5**: Complaint roles must reference Oryggi users but be managed independently

#### FR-6: Role & Permission Management
- **FR-6.1**: Administrator must be able to create complaint-specific roles
- **FR-6.2**: Roles must support organizational scope (Global, Company, Branch, Department, Section)
- **FR-6.3**: System must provide granular permissions per module, resource, and action
- **FR-6.4**: Permission checks must be context-aware (branch, department constraints)
- **FR-6.5**: System must maintain audit trail of all role assignments

### Non-Functional Requirements

#### NFR-1: Performance
- System must handle 10,000+ concurrent users
- Complaint creation response time < 2 seconds
- Dashboard load time < 3 seconds
- Email delivery within 1 minute of trigger

#### NFR-2: Scalability
- Support multi-tenant architecture
- Scale horizontally for increased load
- Handle 100,000+ complaints per month
- Support 50+ branches with 5,000+ employees each

#### NFR-3: Security
- Role-based access control (RBAC)
- Data encryption at rest and in transit
- Audit logging for all actions
- Compliance with data protection regulations

#### NFR-4: Availability
- 99.9% uptime SLA
- Automated failover and recovery
- Regular backups (hourly incremental, daily full)
- Disaster recovery plan

#### NFR-5: Usability
- Intuitive, world-class UI/UX
- Mobile-responsive design
- Accessibility compliance (WCAG 2.1 Level AA)
- Multi-language support (English, Hindi)

---

## ARCHITECTURE DESIGN

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        CLIENT LAYER                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│  │  Web App     │  │  Mobile App  │  │  Admin Panel │          │
│  │  (React)     │  │  (PWA)       │  │  (React)     │          │
│  └──────────────┘  └──────────────┘  └──────────────┘          │
└──────────────────────────────┬──────────────────────────────────┘
                               │ HTTPS/REST APIs
┌──────────────────────────────▼──────────────────────────────────┐
│                     API GATEWAY LAYER                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Authentication │ Rate Limiting │ Request Routing        │  │
│  └──────────────────────────────────────────────────────────┘  │
└──────────────────────────────┬──────────────────────────────────┘
                               │
┌──────────────────────────────▼──────────────────────────────────┐
│                     SERVICE LAYER (Node.js/NestJS)               │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐            │
│  │  Complaint  │  │  Escalation │  │  Email Alert │            │
│  │  Service    │  │  Service    │  │  Service     │            │
│  └─────────────┘  └─────────────┘  └──────────────┘            │
│  ┌─────────────┐  ┌─────────────┐  ┌──────────────┐            │
│  │  Role &     │  │  Oryggi     │  │  Notification│            │
│  │  Permission │  │  Sync       │  │  Service     │            │
│  └─────────────┘  └─────────────┘  └──────────────┘            │
└──────────────────────────────┬──────────────────────────────────┘
                               │
          ┌────────────────────┼────────────────────┐
          │                    │                    │
┌─────────▼─────────┐  ┌───────▼────────┐  ┌───────▼────────────┐
│  Complaint DB     │  │  Oryggi HRMS   │  │  Redis Cache       │
│  (PostgreSQL)     │  │  (SQL Server)  │  │  + Message Queue   │
│  - Master Data    │  │  - Read Only   │  └────────────────────┘
│  - Complaints     │  │  - Source of   │
│  - Roles          │  │    Truth       │
│  - Email Config   │  └────────────────┘
└───────────────────┘
```

### Key Architectural Principles

1. **Separation of Concerns**: Modular services with clear responsibilities
2. **Dual-Table Architecture**: Oryggi master data synced, complaint roles managed independently
3. **Event-Driven**: Async processing for emails, notifications, escalations
4. **Scalable**: Horizontal scaling with load balancers
5. **Resilient**: Circuit breakers, retries, fallbacks
6. **Observable**: Comprehensive logging, monitoring, tracing

---

## DATABASE SCHEMA

### Overview

The Complaint Management System uses **two database systems**:

1. **Complaint Database (PostgreSQL)**: Primary complaint system database
   - Master data tables (synced from Oryggi)
   - Complaint-specific tables
   - Role and permission tables
   - Email alert configuration

2. **Oryggi Database (SQL Server)**: External HRMS database (Read-Only)
   - Employee master data
   - Organization structure
   - Leave, attendance, salary data

### Schema Architecture

```
┌─────────────────────────────────────┐
│   COMPLAINT SYSTEM DATABASE         │
│   (PostgreSQL)                      │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  MASTER DATA                │  │
│   │  (Synced from Oryggi)       │  │
│   │  - companies                │  │
│   │  - branches                 │  │
│   │  - departments              │  │
│   │  - sections                 │  │
│   │  - users                    │  │
│   └─────────────────────────────┘  │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  COMPLAINT TABLES           │  │
│   │  - tenants                  │  │
│   │  - complaints               │  │
│   │  - complaint_statuses       │  │
│   │  - complaint_comments       │  │
│   │  - complaint_attachments    │  │
│   └─────────────────────────────┘  │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  ROLE & PERMISSION TABLES   │  │
│   │  - complaint_roles          │  │
│   │  - user_complaint_roles     │  │
│   │  - complaint_role_permissions│ │
│   └─────────────────────────────┘  │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  ESCALATION TABLES          │  │
│   │  - escalation_matrices      │  │
│   │  - escalation_levels        │  │
│   │  - escalation_history       │  │
│   └─────────────────────────────┘  │
│                                     │
│   ┌─────────────────────────────┐  │
│   │  EMAIL ALERT TABLES         │  │
│   │  - alert_types              │  │
│   │  - email_alert_templates    │  │
│   │  - alert_recipients         │  │
│   │  - user_alert_preferences   │  │
│   └─────────────────────────────┘  │
└─────────────────────────────────────┘
```

### CHUNK 2: Master Data Tables (Synced from Oryggi)

**IMPORTANT**: These tables are **synced from Oryggi database** and are **read-only** in the Complaint Management System.

#### 2.1 Tenants
```sql
CREATE TABLE tenants (
    tenant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(255) NOT NULL UNIQUE,
    subdomain VARCHAR(100) UNIQUE,
    status VARCHAR(20) DEFAULT 'ACTIVE',

    -- White-labeling
    branding JSONB DEFAULT '{}',
    -- Example: {"logo_url": "...", "primary_color": "#1976d2"}

    -- Feature flags
    features JSONB DEFAULT '{}',

    -- Compliance
    data_residency VARCHAR(50),
    compliance_tags TEXT[],

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_tenant_status CHECK (status IN ('ACTIVE', 'TRIAL', 'SUSPENDED', 'INACTIVE'))
);

CREATE INDEX idx_tenants_status ON tenants(status);
CREATE INDEX idx_tenants_subdomain ON tenants(subdomain);
```

#### 2.2 Companies (Synced from Oryggi.CompanyMaster)
```sql
CREATE TABLE companies (
    company_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

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
CREATE INDEX idx_companies_code ON companies(code);
CREATE INDEX idx_companies_active ON companies(is_active);
```

#### 2.3 Branches (Synced from Oryggi.BranchMaster)
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
CREATE INDEX idx_branches_code ON branches(code);
CREATE INDEX idx_branches_active ON branches(is_active);
```

#### 2.4 Departments (Synced from Oryggi.DeptMaster)
```sql
CREATE TABLE departments (
    department_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    branch_id UUID NOT NULL REFERENCES branches(branch_id),

    -- Synced from Oryggi
    oryggi_dept_id INT UNIQUE NOT NULL,     -- Maps to Oryggi.DeptMaster.Dcode
    name VARCHAR(255) NOT NULL,             -- From Oryggi.DeptMaster.Dname
    code VARCHAR(50) NOT NULL,              -- From Oryggi.DeptMaster.Dcode

    head_user_id UUID,                      -- References users table
    is_active BOOLEAN DEFAULT true,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_dept_code UNIQUE(branch_id, code)
);

CREATE INDEX idx_departments_branch ON departments(branch_id);
CREATE INDEX idx_departments_oryggi_id ON departments(oryggi_dept_id);
CREATE INDEX idx_departments_code ON departments(code);
CREATE INDEX idx_departments_active ON departments(is_active);
```

#### 2.5 Sections (Synced from Oryggi.SectionMaster)
```sql
CREATE TABLE sections (
    section_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    department_id UUID NOT NULL REFERENCES departments(department_id),

    -- Synced from Oryggi
    oryggi_section_id INT UNIQUE NOT NULL,  -- Maps to Oryggi.SectionMaster.SecCode
    name VARCHAR(255) NOT NULL,             -- From Oryggi.SectionMaster.SecName
    code VARCHAR(50) NOT NULL,              -- From Oryggi.SectionMaster.SecCode

    supervisor_user_id UUID,                -- References users table
    is_active BOOLEAN DEFAULT true,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_section_code UNIQUE(department_id, code)
);

CREATE INDEX idx_sections_department ON sections(department_id);
CREATE INDEX idx_sections_oryggi_id ON sections(oryggi_section_id);
CREATE INDEX idx_sections_code ON sections(code);
CREATE INDEX idx_sections_active ON sections(is_active);
```

#### 2.6 Users (Synced from Oryggi.EmployeeMaster)
```sql
CREATE TABLE users (
    user_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

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
    oryggi_role VARCHAR(50),                -- From Oryggi.EmployeeMaster.Role

    date_of_joining DATE,                   -- From Oryggi.EmployeeMaster.DateofJoin
    date_of_birth DATE,                     -- From Oryggi.EmployeeMaster.DateofBirth

    is_active BOOLEAN DEFAULT true,         -- From Oryggi.EmployeeMaster.Active
    last_synced_at TIMESTAMP,
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

---

*End of Chunk 2 - Master Data Tables*

---

## CONTINUE TO DETAILED DOCUMENTATION

This document contains the foundation (Chunks 1-2). For detailed implementation documentation, please refer to the following files:

### 📋 Chunk 3: Complaint & Role Tables
**File**: [CHUNK_03_COMPLAINT_ROLE_TABLES.md](CHUNK_03_COMPLAINT_ROLE_TABLES.md)
**Content**:
- Complaint Categories table
- Complaints table with auto-numbering (CMP-2025-000001)
- Complaint Comments & Activity Log
- Complaint Attachments with virus scanning
- Complaint Roles (7 system roles)
- User Complaint Role Assignments
- Role Permissions (granular access control)

### 📊 Chunk 4: Escalation & Email Alert System
**File**: [CHUNK_04_ESCALATION_EMAIL_TABLES.md](CHUNK_04_ESCALATION_EMAIL_TABLES.md)
**Content**:
- Escalation Matrices (configurable 2-5 levels)
- Escalation Levels with 6 assignment strategies
- Escalation History & Audit Trail
- Alert Types (8+ configurable types)
- Email Alert Templates with variable substitution
- Alert Recipients (9 dynamic recipient types)
- Alert Schedules (immediate/delayed/batch)
- User Alert Preferences

### 🔄 Chunk 5: Oryggi HRMS Integration
**File**: [CHUNK_05_ORYGGI_INTEGRATION.md](CHUNK_05_ORYGGI_INTEGRATION.md)
**Content**:
- Dual-table architecture design
- Oryggi table mapping configuration
- Real-time webhook integration
- Scheduled batch sync service
- Employee transfer impact handling
- Employee deactivation handling
- Complaint role management
- Data consistency checks
- Sync health monitoring

### 🛠️ Chunk 6: Technology Stack
**File**: [CHUNK_06_TECHNOLOGY_STACK.md](CHUNK_06_TECHNOLOGY_STACK.md)
**Content**:
- Backend stack (Node.js, NestJS, TypeScript, PostgreSQL)
- Frontend stack (React, Next.js, Material-UI)
- Infrastructure (Docker, Kubernetes, AWS)
- Security tools (JWT, Passport.js, Helmet)
- Email services (NodeMailer, AWS SES)
- Testing framework (Jest, Playwright)
- Monitoring (Prometheus, Grafana, ELK Stack)
- Complete package structure

### 🎨 Chunk 7: UI/UX Design
**File**: [CHUNK_07_UI_UX_DESIGN.md](CHUNK_07_UI_UX_DESIGN.md)
**Content**:
- Design principles (Simplicity, Intuitive, Responsive, Accessible)
- 4 User persona dashboards (Employee, Manager, HR Manager, Admin)
- Key UI screens with mockups
- Mobile responsive design
- Color scheme & typography
- Component library
- Accessibility features (WCAG 2.1 AA)
- Notification patterns
- Search & filters
- Data visualization

### 🔒 Chunk 8: Security & Deployment
**File**: [CHUNK_08_SECURITY_DEPLOYMENT.md](CHUNK_08_SECURITY_DEPLOYMENT.md)
**Content**:
- Defense-in-depth security architecture
- Authentication & authorization (JWT, MFA, SSO)
- Data protection & encryption (at rest, in transit)
- API security (rate limiting, input validation)
- File upload security (virus scanning)
- Comprehensive audit logging
- Deployment architecture (AWS ECS, RDS, ElastiCache)
- CI/CD pipeline (GitHub Actions)
- Monitoring & observability (Prometheus, Grafana)
- Backup & disaster recovery
- Scalability & performance optimization
- Security compliance checklist

---

## DOCUMENT SUMMARY

This **Complaint Management System** planning document provides a complete blueprint for developing an enterprise-grade HRMS complaint module with the following key features:

✅ **Multi-level Escalation** (2-5 configurable levels)
✅ **Email Alert System** (8+ alert types with templates)
✅ **Oryggi HRMS Integration** (automatic sync, zero impact)
✅ **Role-Based Access Control** (organizational scopes)
✅ **World-Class UI/UX** (mobile-responsive, accessible)
✅ **Enterprise Security** (encryption, audit logs, compliance)
✅ **Production-Ready Deployment** (auto-scaling, 99.9% uptime)

### Key Statistics
- **Total Tables**: 23+ tables across 6 functional areas
- **Lines of Documentation**: 2,500+ lines
- **Code Examples**: 50+ TypeScript/SQL snippets
- **UI Mockups**: 10+ screen designs
- **Architecture Diagrams**: 8+ visual diagrams

### Implementation Readiness
- **Database Schema**: Complete (all tables defined with indexes)
- **Integration Strategy**: Complete (Oryggi sync mechanisms)
- **Technology Stack**: Complete (all tools specified)
- **UI/UX Design**: Complete (all personas and screens)
- **Security Architecture**: Complete (compliance checklist)
- **Deployment Plan**: Complete (CI/CD pipeline, DR plan)

**Status**: ✅ **Ready for Development**

---

**Version History**:
- v1.0 (Initial) - Basic architecture and requirements
- v2.0 (Current) - Complete planning with all chunks, Oryggi integration, and production-ready specifications

**Last Updated**: 2025-10-11

---

