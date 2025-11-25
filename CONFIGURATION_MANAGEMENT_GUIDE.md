# Configuration Management Guide

**Complaint Management System - Admin Configuration Module**

**Version:** 1.0
**Date:** October 11, 2025
**Purpose:** Complete guide to all configuration pages and their management

---

## Table of Contents

1. [Overview](#overview)
2. [Configuration Module Structure](#configuration-module-structure)
3. [Core Configuration Pages](#core-configuration-pages)
4. [Email Alert Configuration](#email-alert-configuration)
5. [Access Control & Permissions](#access-control--permissions)
6. [Best Practices](#best-practices)

---

## Overview

The Configuration Management Module provides administrators with NO-CODE, intuitive interfaces to configure all aspects of the complaint management system. This module is organized into separate, focused configuration pages, each handling a specific aspect of the system.

### Key Principles
- **Separation of Concerns**: Each configuration page handles one specific area
- **User-Friendly**: Intuitive UI with wizards and guided workflows
- **Validation**: Real-time validation to prevent configuration errors
- **Audit Trail**: All configuration changes are logged
- **Role-Based Access**: Different admins can access different configuration areas
- **Version Control**: Configuration snapshots and rollback capability

---

## Configuration Module Structure

### Main Navigation
```
┌─────────────────────────────────────────────────────────────┐
│  Configuration Dashboard                      [Admin: User]  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  CORE CONFIGURATION                                   │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │  • Escalation Matrices                               │  │
│  │  • Complaint Categories                              │  │
│  │  • SLA Rules                                          │  │
│  │  • User Groups                                        │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  EMAIL ALERT CONFIGURATION (NEW)                      │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │  • Alert Types                                        │  │
│  │  • Email Templates                                    │  │
│  │  • Recipient Rules                                    │  │
│  │  • Alert Scheduling                                   │  │
│  │  • Alert Analytics                                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  CONTENT MANAGEMENT                                   │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │  • Knowledge Base                                     │  │
│  │  • Survey Templates                                   │  │
│  │  • Branding & Customization                          │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │  SYSTEM ADMINISTRATION                                │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │  • Tenant Management                                  │  │
│  │  • HRMS Integration                                   │  │
│  │  • Audit Logs                                         │  │
│  │  • System Settings                                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Core Configuration Pages

### 1. Escalation Matrix Configuration

**Purpose**: Configure multi-level escalation workflows

**Access**: Admin, Super Admin
**Location**: `/admin/config/escalation-matrices`

#### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Escalation Matrices                    [+ Create Matrix]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Search: [___________] Filter by: [All Branches ▼] [Apply]  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Name                  | Scope      | Levels | Status  │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ HR Issues - 3 Level   | All        | 3      | ✓ Active│  │
│  │ [Edit] [Clone] [Deactivate]                          │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Salary Disputes       | Mumbai     | 4      | ✓ Active│  │
│  │ [Edit] [Clone] [Deactivate]                          │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Attendance Issues     | Engineering| 2      | ○ Draft │  │
│  │ [Edit] [Clone] [Delete]                              │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [1] [2] [3] ... [10] Next                                  │
└─────────────────────────────────────────────────────────────┘
```

#### Features
- **List View**: See all escalation matrices at a glance
- **Quick Actions**: Edit, clone, activate/deactivate
- **Filtering**: By scope, category, status
- **Search**: Find matrix by name
- **Bulk Actions**: Activate/deactivate multiple matrices

#### Create/Edit Matrix Wizard
See main architecture document for detailed wizard steps.

---

### 2. Complaint Categories Configuration

**Purpose**: Manage complaint categories and subcategories

**Access**: Admin, Super Admin, HR Manager
**Location**: `/admin/config/categories`

#### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Complaint Categories                    [+ Add Category]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Hierarchical View     [Toggle to List View]                │
│                                                              │
│  ├─ 📋 Attendance & Timing                        [Edit]    │
│  │   ├─ Biometric Issues                          [Edit]    │
│  │   ├─ Late Entry                                 [Edit]    │
│  │   └─ Manual Attendance Request                  [Edit]    │
│  │                                                           │
│  ├─ 💰 Salary & Payroll                           [Edit]    │
│  │   ├─ Salary Discrepancy                        [Edit]    │
│  │   ├─ Missing Allowance                          [Edit]    │
│  │   └─ Bonus Issues                               [Edit]    │
│  │                                                           │
│  ├─ 🏖️  Leave Management                          [Edit]    │
│  │   ├─ Leave Credit Issues                        [Edit]    │
│  │   ├─ Leave Approval Delay                       [Edit]    │
│  │   └─ Leave Balance Discrepancy                  [Edit]    │
│  │                                                           │
│  ├─ 👥 Workplace Issues                            [Edit]    │
│  │   ├─ Harassment                                  [Edit]    │
│  │   ├─ Discrimination                              [Edit]    │
│  │   └─ Conflict with Colleague                    [Edit]    │
│  │                                                           │
│  └─ ❓ Other HR Issues                             [Edit]    │
│      └─ General Query                              [Edit]    │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

#### Category Editor Modal
```
┌─────────────────────────────────────────────────────────────┐
│  Edit Category: Attendance & Timing            [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Category Name: [Attendance & Timing                ]       │
│  Category Code: [attendance-timing               ]          │
│  Icon: [🕐] [Choose Icon]                                   │
│                                                              │
│  Description:                                                │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Issues related to attendance marking, biometric,     │  │
│  │ late entries, and time tracking.                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Parent Category: [None (Top Level) ▼]                     │
│                                                              │
│  Display Order: [1                  ]                        │
│                                                              │
│  Status: ☑ Active                                           │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Default Escalation Matrix:                            │  │
│  │ [Select Matrix... ▼]                                  │  │
│  │ ○ Use global default                                  │  │
│  │ ● Use custom matrix: [Attendance - 2 Level ▼]       │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [Delete Category]                  [Save Changes]          │
└─────────────────────────────────────────────────────────────┘
```

#### Features
- **Hierarchical Management**: Drag-and-drop reordering
- **Icon Selection**: Choose from icon library
- **Category-Specific Escalation**: Override default escalation per category
- **Bulk Operations**: Import/export categories
- **Usage Statistics**: See how many complaints per category

---

### 3. SLA Rules Configuration

**Purpose**: Configure Service Level Agreement timeframes

**Access**: Admin, Super Admin
**Location**: `/admin/config/sla-rules`

#### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│  SLA Rules Configuration                     [+ Add Rule]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Global Default SLA: 24 hours [Edit]                        │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Category-Specific SLA Rules                           │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │                                                        │  │
│  │ Category          | Priority | Resolution Time | Auto-│  │
│  │                   |          |                 | Escalate│  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Salary & Payroll  | HIGH     | 12 hours        | 8h   │  │
│  │                   | MEDIUM   | 24 hours        | 20h  │  │
│  │                   | LOW      | 48 hours        | 40h  │  │
│  │ [Edit]                                                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Harassment        | CRITICAL | 4 hours         | 2h   │  │
│  │                   | HIGH     | 8 hours         | 6h   │  │
│  │ [Edit]                                                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Attendance        | ALL      | 24 hours        | 20h  │  │
│  │ [Edit]                                                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Business Hours Configuration                          │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Calculate SLA based on:                               │  │
│  │ ● Calendar hours (24x7)                               │  │
│  │ ○ Business hours only                                 │  │
│  │                                                        │  │
│  │ Business Hours: 09:00 AM - 06:00 PM                  │  │
│  │ Working Days: ☑ Mon ☑ Tue ☑ Wed ☑ Thu ☑ Fri ☐ Sat ☐ Sun│  │
│  │ Timezone: [Asia/Kolkata ▼]                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

#### SLA Rule Editor
```
┌─────────────────────────────────────────────────────────────┐
│  Edit SLA Rule: Salary & Payroll              [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Category: [Salary & Payroll ▼]                            │
│                                                              │
│  Priority-Based SLA:                                         │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ ⚠️  CRITICAL Priority                                 │  │
│  │ First Response:   [2] hours                           │  │
│  │ Resolution Time:  [8] hours                           │  │
│  │ Auto-escalate:    [6] hours after assignment          │  │
│  │ Warning at:       [2] hours before deadline           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ 🔴 HIGH Priority                                       │  │
│  │ First Response:   [4] hours                           │  │
│  │ Resolution Time:  [12] hours                          │  │
│  │ Auto-escalate:    [8] hours after assignment          │  │
│  │ Warning at:       [2] hours before deadline           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ 🟡 MEDIUM Priority                                     │  │
│  │ First Response:   [8] hours                           │  │
│  │ Resolution Time:  [24] hours                          │  │
│  │ Auto-escalate:    [20] hours after assignment         │  │
│  │ Warning at:       [4] hours before deadline           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ 🟢 LOW Priority                                        │  │
│  │ First Response:   [24] hours                          │  │
│  │ Resolution Time:  [48] hours                          │  │
│  │ Auto-escalate:    [40] hours after assignment         │  │
│  │ Warning at:       [8] hours before deadline           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [Delete Rule]                         [Save Changes]        │
└─────────────────────────────────────────────────────────────┘
```

#### Features
- **Priority-Based SLA**: Different SLA for each priority level
- **Category-Specific**: Override global SLA per category
- **Business Hours**: Consider only business hours for SLA
- **Auto-Escalation**: Configure when to auto-escalate
- **Warning Triggers**: Send warning before SLA breach
- **Visual Timeline**: See SLA timeline graphically

---

### 4. User Groups Management

**Purpose**: Create and manage user groups for assignment

**Access**: Admin, Super Admin, HR Manager
**Location**: `/admin/config/user-groups`

#### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│  User Groups                                [+ Create Group] │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Filter: [All Branches ▼] [All Departments ▼]              │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Group Name        | Branch    | Members | Actions     │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Mumbai Branch HR  | Mumbai    | 5       | [Edit] [Del]│  │
│  │ Bangalore HR Team | Bangalore | 3       | [Edit] [Del]│  │
│  │ Engineering Mgrs  | All       | 12      | [Edit] [Del]│  │
│  │ Regional HR West  | Multiple  | 8       | [Edit] [Del]│  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

#### Group Editor
```
┌─────────────────────────────────────────────────────────────┐
│  Edit Group: Mumbai Branch HR                 [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Group Name: [Mumbai Branch HR                      ]       │
│                                                              │
│  Description:                                                │
│  [HR team members handling Mumbai branch complaints]        │
│                                                              │
│  Scope:                                                      │
│  Company: [ABC Corporation ▼]                               │
│  Branch:  [Mumbai Office ▼]                                 │
│  Department: [All Departments]                              │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Group Members                             [+ Add User]│  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Name              | Role        | Email       | [×] │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Priya Sharma      | BRANCH_HR   | priya@...   | [×] │  │
│  │ Rajesh Kumar      | HR_MANAGER  | rajesh@...  | [×] │  │
│  │ Anjali Desai      | HR_EXECUTIVE| anjali@...  | [×] │  │
│  │ Vikram Singh      | BRANCH_HR   | vikram@...  | [×] │  │
│  │ Sunita Patel      | HR_EXECUTIVE| sunita@...  | [×] │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [Delete Group]                            [Save Changes]    │
└─────────────────────────────────────────────────────────────┘
```

---

## Email Alert Configuration

### 5. Alert Types Management

**Purpose**: Define and manage email alert types

**Access**: Admin, Super Admin
**Location**: `/admin/config/email-alerts/types`

#### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Email Alert Types                        [+ Create Alert]  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Filter by Category: [All Categories ▼]                     │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ SYSTEM DEFAULT ALERTS (Cannot be deleted)             │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Alert Type            | Category   | Status | Actions │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Complaint Created     | COMPLAINT  | ✓ ON   | [Edit] │  │
│  │ Complaint Assigned    | COMPLAINT  | ✓ ON   | [Edit] │  │
│  │ Complaint Escalated   | ESCALATION | ✓ ON   | [Edit] │  │
│  │ SLA Warning           | SLA        | ✓ ON   | [Edit] │  │
│  │ SLA Breach            | SLA        | ✓ ON   | [Edit] │  │
│  │ Complaint Resolved    | RESOLUTION | ✓ ON   | [Edit] │  │
│  │ Daily Digest          | SYSTEM     | ○ OFF  | [Edit] │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ CUSTOM ALERTS                                         │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ High Priority Salary  | CUSTOM     | ✓ ON   | [Edit][×]│  │
│  │ Weekend Escalation    | CUSTOM     | ○ OFF  | [Edit][×]│  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

#### Alert Type Editor
```
┌─────────────────────────────────────────────────────────────┐
│  Create Custom Alert Type                     [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Alert Type Name: [High Priority Salary Issues        ]     │
│  Type Code: [high_priority_salary              ]            │
│  Category: [CUSTOM ▼]                                       │
│                                                              │
│  Description:                                                │
│  [Send immediate alert for high priority salary complaints] │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Trigger Configuration                                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │                                                        │  │
│  │ Trigger Event: [complaint.created ▼]                  │  │
│  │                                                        │  │
│  │ Additional Conditions:                                 │  │
│  │ ☑ Only for specific categories:                       │  │
│  │    [✓] Salary & Payroll                               │  │
│  │    [✓] Bonus Issues                                    │  │
│  │                                                        │  │
│  │ ☑ Only for specific priorities:                       │  │
│  │    [✓] HIGH  [✓] CRITICAL                             │  │
│  │                                                        │  │
│  │ ☑ Only for specific branches:                         │  │
│  │    [Select Branches...] (3 selected)                  │  │
│  │                                                        │  │
│  │ ☑ Only for specific escalation levels:                │  │
│  │    [✓] Level 2  [✓] Level 3                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Status: ☑ Enabled                                          │
│                                                              │
│  [Delete Alert Type]                      [Save Alert Type]  │
└─────────────────────────────────────────────────────────────┘
```

---

### 6. Email Template Designer

**Purpose**: Create and edit HTML email templates

**Access**: Admin, Super Admin, Marketing (Template Creator)
**Location**: `/admin/config/email-alerts/templates`

#### Main Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Email Templates                          [+ Create Template]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Filter: [Alert Type: All ▼] [Status: All ▼] [Search...]   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Template Name       | Alert Type      | Ver | Status │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Default Assigned    | Complaint       | v2  | Active │  │
│  │ [Edit] [Preview] [Clone] [History]                   │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Escalation Notice   | Escalation      | v1  | Active │  │
│  │ [Edit] [Preview] [Clone] [History]                   │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ SLA Breach Warning  | SLA             | v3  | Active │  │
│  │ [Edit] [Preview] [Clone] [History]                   │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

#### Template Editor (Rich HTML Editor)
```
┌─────────────────────────────────────────────────────────────┐
│  Edit Template: Complaint Assigned            [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌────────────────────────────────────────────────────────┐│
│  │ TAB: Basic Info | Content | Design | Preview | Test   ││
│  └────────────────────────────────────────────────────────┘│
│                                                              │
│  Template Name: [Complaint Assigned to You           ]      │
│  Alert Type: [Complaint Assigned ▼]                         │
│  Version: 2  [View Version History]                         │
│                                                              │
│  Subject Line:                                               │
│  [[{{priority}}] Complaint #{{complaint_number}} - {{complaint_subject}}]│
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Email Body (HTML Editor)                              │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │                                                        │  │
│  │ [B] [I] [U] | [📝] [🖼️] [🔗] [📋] | [{{}} Insert Var]│  │
│  │                                                        │  │
│  │ Dear {{assigned_to_name}},                            │  │
│  │                                                        │  │
│  │ A new complaint has been assigned to you:             │  │
│  │                                                        │  │
│  │ Complaint #: {{complaint_number}}                     │  │
│  │ Subject: {{complaint_subject}}                        │  │
│  │ Employee: {{employee_name}}                           │  │
│  │ Priority: {{priority}}                                │  │
│  │ SLA Deadline: {{sla_deadline}}                        │  │
│  │                                                        │  │
│  │ [View Complaint Button]                               │  │
│  │                                                        │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Available Variables: [Insert Variable ▼]                   │
│  • complaint_number       • branch_name                     │
│  • complaint_subject      • department_name                 │
│  • employee_name          • assigned_to_name                │
│  • employee_email         • manager_name                    │
│  • category_name          • sla_deadline                    │
│  • priority               • created_at                      │
│  • status                 • complaint_url                   │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Email Settings                                         │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Format: ● HTML  ○ Plain Text                          │  │
│  │ Priority: [Normal ▼]                                   │  │
│  │ Reply-To: [hr-support@company.com           ]         │  │
│  │ Include Attachments: ☐ PDF Report  ☐ Details         │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  [Delete Template]  [Save as Draft]  [Save & Activate]     │
└─────────────────────────────────────────────────────────────┘
```

#### Preview Tab
```
┌─────────────────────────────────────────────────────────────┐
│  Preview Template                              [Send Test]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Preview with Sample Data: [Use Sample Complaint ▼]         │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Email Preview (Desktop View)                          │  │
│  │                                                        │  │
│  │ From: Complaint System <no-reply@company.com>        │  │
│  │ To: manager@company.com                               │  │
│  │ Subject: [HIGH] Complaint #C-2024-001 - Biometric... │  │
│  │                                                        │  │
│  │ ┌──────────────────────────────────────────────────┐ │  │
│  │ │ [Company Logo]                                    │ │  │
│  │ │                                                    │ │  │
│  │ │ Dear Rajesh Kumar,                                │ │  │
│  │ │                                                    │ │  │
│  │ │ A new complaint has been assigned to you:         │ │  │
│  │ │                                                    │ │  │
│  │ │ Complaint #: C-2024-001                           │ │  │
│  │ │ Subject: Biometric not working on 8th Oct         │ │  │
│  │ │ Employee: Neha Singh                              │ │  │
│  │ │ Priority: HIGH                                     │ │  │
│  │ │ SLA Deadline: Oct 9, 2024 10:00 AM               │ │  │
│  │ │                                                    │ │  │
│  │ │ [View Complaint]                                   │ │  │
│  │ │                                                    │ │  │
│  │ │ © 2024 ABC Corporation                            │ │  │
│  │ └──────────────────────────────────────────────────┘ │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  View: [Desktop] [Tablet] [Mobile]                          │
│                                                              │
│  [Send Test Email To:] [test@company.com     ] [Send]       │
└─────────────────────────────────────────────────────────────┘
```

---

### 7. Recipient Rules Management

**Purpose**: Define who receives each type of alert

**Access**: Admin, Super Admin
**Location**: `/admin/config/email-alerts/recipients`

#### Main Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Recipient Rules                              [+ Add Rule]   │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Filter: [Alert Type: All ▼] [Active Only ☑]               │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Alert: Complaint Created                              │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Rule Name         | Recipients      | Scope  | Active │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Send to Employee  | EMPLOYEE        | All    | ✓     │  │
│  │ [Edit] [Deactivate] [Priority: 1]                    │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ CC to Manager     | MANAGER         | All    | ✓     │  │
│  │ [Edit] [Deactivate] [Priority: 2]                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Alert: Complaint Escalated                            │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Rule Name         | Recipients      | Scope  | Active │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ New Assignee      | ASSIGNED_USER   | All    | ✓     │  │
│  │ [Edit] [Deactivate] [Priority: 1]                    │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Previous Assignee | ROLE:MANAGER    | All    | ✓     │  │
│  │ [Edit] [Deactivate] [Priority: 2]                    │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ HR Team           | GROUP:Branch_HR | Mumbai | ✓     │  │
│  │ [Edit] [Deactivate] [Priority: 3]                    │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

#### Recipient Rule Editor
```
┌─────────────────────────────────────────────────────────────┐
│  Create Recipient Rule                        [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Rule Name: [High Priority to Branch HR               ]     │
│  Alert Type: [Complaint Created ▼]                          │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Recipient Configuration                               │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │                                                        │  │
│  │ Recipient Type: [ROLE ▼]                              │  │
│  │                                                        │  │
│  │ Options:                                               │  │
│  │ • EMPLOYEE - The complaint creator                    │  │
│  │ • ASSIGNED_USER - Current assignee                    │  │
│  │ • MANAGER - Employee's manager                        │  │
│  │ • ESCALATION_CHAIN - All in escalation path           │  │
│  │ • ROLE - Users with specific role                     │  │
│  │ • SPECIFIC_USER - Named individuals                   │  │
│  │ • GROUP - User group                                   │  │
│  │ • EMAIL_LIST - External email addresses               │  │
│  │ • DYNAMIC - Custom expression                         │  │
│  │                                                        │  │
│  │ Selected: ROLE                                         │  │
│  │                                                        │  │
│  │ Role: [BRANCH_HR ▼]                                   │  │
│  │ Scope: ● Branch of complaint  ○ Company-wide         │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Apply This Rule When (Scope Filter):                  │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │                                                        │  │
│  │ ☑ Only for specific branches:                         │  │
│  │    [✓] Mumbai  [✓] Bangalore  [✓] Delhi              │  │
│  │                                                        │  │
│  │ ☑ Only for specific categories:                       │  │
│  │    [✓] Salary & Payroll  [✓] Attendance              │  │
│  │                                                        │  │
│  │ ☑ Only for specific priorities:                       │  │
│  │    [✓] HIGH  [✓] CRITICAL                             │  │
│  │                                                        │  │
│  │ ☑ Only for escalation levels:                         │  │
│  │    [✓] Level 2  [✓] Level 3                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Additional Recipients (CC/BCC)                        │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ CC: [Add email or user...]                            │  │
│  │     • hr-team@company.com                             │  │
│  │                                                        │  │
│  │ BCC: [Add email or user...]                           │  │
│  │     • audit@company.com                               │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Conditional Sending                                    │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ☑ Only if SLA is breached                             │  │
│  │ ☐ Only during business hours                          │  │
│  │ ☐ Only if escalation level >= [2]                     │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Priority Order: [10               ] (Lower = Higher Priority) │
│                                                              │
│  Status: ☑ Active                                            │
│                                                              │
│  [Delete Rule]                               [Save Rule]     │
└─────────────────────────────────────────────────────────────┘
```

---

### 8. Alert Scheduling Configuration

**Purpose**: Configure when and how alerts are sent

**Access**: Admin, Super Admin
**Location**: `/admin/config/email-alerts/scheduling`

#### Page Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Alert Scheduling                            [+ Add Schedule]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Global Settings                                       │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Default Behavior: ● Send Immediately                  │  │
│  │                   ○ Batch Alerts                       │  │
│  │                                                        │  │
│  │ Business Hours:   ☑ Respect user business hours      │  │
│  │ Rate Limiting:    ☑ Enable global rate limiting      │  │
│  │                                                        │  │
│  │ [Edit Global Settings]                                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Alert-Specific Schedules                              │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Alert Type         | Schedule      | Batch  | Active │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Complaint Created  | Immediate     | No     | ✓     │  │
│  │ [Edit]                                                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ SLA Warning        | Immediate     | No     | ✓     │  │
│  │ [Edit]                                                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Daily Digest       | Custom (8 AM) | Yes    | ✓     │  │
│  │ [Edit]                                                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Status Updates     | Batched (30m) | Yes    | ✓     │  │
│  │ [Edit]                                                 │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

#### Schedule Editor
```
┌─────────────────────────────────────────────────────────────┐
│  Edit Alert Schedule: Daily Digest           [Save] [Cancel]│
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Alert Type: [Daily Digest ▼]                              │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Sending Behavior                                      │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ○ Send immediately                                     │  │
│  │ ● Send with delay: [0] minutes                        │  │
│  │ ● Send at specific time: [08:00 AM]                  │  │
│  │   Days: ☑ Mon ☑ Tue ☑ Wed ☑ Thu ☑ Fri ☐ Sat ☐ Sun  │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Batching Configuration                                │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ☑ Enable batching                                     │  │
│  │                                                        │  │
│  │ Batch Interval: [60] minutes                          │  │
│  │ Max Alerts per Batch: [50]                            │  │
│  │                                                        │  │
│  │ Grouping: ● By recipient  ○ By category  ○ By branch│  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Rate Limiting                                         │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ☑ Enable rate limiting for this alert type           │  │
│  │                                                        │  │
│  │ Max emails per user:                                   │  │
│  │ • Per hour: [10]                                      │  │
│  │ • Per day:  [50]                                      │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Business Hours                                        │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ☑ Send only during business hours                    │  │
│  │                                                        │  │
│  │ Business Hours: [09:00 AM] to [06:00 PM]            │  │
│  │ Timezone: [Asia/Kolkata ▼]                           │  │
│  │                                                        │  │
│  │ If outside business hours:                             │  │
│  │ ● Queue for next business day                         │  │
│  │ ○ Send anyway                                          │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Retry Configuration                                    │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ☑ Retry on failure                                    │  │
│  │                                                        │  │
│  │ Max Retry Attempts: [3]                               │  │
│  │ Retry Interval: [5] minutes                           │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  Status: ☑ Active                                            │
│                                                              │
│  [Delete Schedule]                          [Save Changes]   │
└─────────────────────────────────────────────────────────────┘
```

---

### 9. Alert Analytics Dashboard

**Purpose**: Monitor email alert performance and engagement

**Access**: Admin, Super Admin
**Location**: `/admin/config/email-alerts/analytics`

#### Dashboard Layout
```
┌─────────────────────────────────────────────────────────────┐
│  Email Alert Analytics                      [Export Report]  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Date Range: [Last 30 Days ▼]  Branch: [All ▼]             │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Overview Metrics                                      │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ ┌─────────────┬─────────────┬─────────────┬────────┐│  │
│  │ │ Total Sent  │ Delivered   │ Opened      │ Clicked││  │
│  │ ├─────────────┼─────────────┼─────────────┼────────┤│  │
│  │ │   12,450    │ 12,320 (99%)│ 8,540 (69%) │ 3,240  ││  │
│  │ └─────────────┴─────────────┴─────────────┴────────┘│  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌────────────────────────────────────────────────┐         │
│  │ Delivery Rate Over Time                        │         │
│  │ [Line Chart showing delivery trends]           │         │
│  └────────────────────────────────────────────────┘         │
│                                                              │
│  ┌────────────────────────────────────────────────┐         │
│  │ Alerts by Type                                 │         │
│  ├────────────────────────────────────────────────┤         │
│  │ Alert Type            | Sent  | Open Rate      │         │
│  ├────────────────────────────────────────────────┤         │
│  │ Complaint Assigned    | 3,450 | 85%    [████]  │         │
│  │ Complaint Escalated   | 1,240 | 92%    [████]  │         │
│  │ SLA Warning           |   890 | 78%    [███ ]  │         │
│  │ SLA Breach            |   234 | 95%    [████]  │         │
│  │ Complaint Resolved    | 2,890 | 65%    [███ ]  │         │
│  │ Daily Digest          | 3,746 | 45%    [██  ]  │         │
│  └────────────────────────────────────────────────┘         │
│                                                              │
│  ┌────────────────────────────────────────────────┐         │
│  │ Failed Deliveries (120)                 [View All]│      │
│  ├────────────────────────────────────────────────┤         │
│  │ Reason                   | Count               │         │
│  ├────────────────────────────────────────────────┤         │
│  │ Invalid Email Address    | 45                  │         │
│  │ Mailbox Full             | 28                  │         │
│  │ Spam Filter Blocked      | 32                  │         │
│  │ Server Error             | 15                  │         │
│  └────────────────────────────────────────────────┘         │
│                                                              │
│  ┌────────────────────────────────────────────────┐         │
│  │ Most Effective Templates                       │         │
│  ├────────────────────────────────────────────────┤         │
│  │ 1. Escalation Notice v2     - 92% open rate   │         │
│  │ 2. SLA Breach Warning v3    - 88% open rate   │         │
│  │ 3. Assignment Notification  - 85% open rate   │         │
│  └────────────────────────────────────────────────┘         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

### 10. User Alert Preferences (Admin View)

**Purpose**: View and manage user email preferences

**Access**: Admin, Super Admin
**Location**: `/admin/config/email-alerts/user-preferences`

#### Layout
```
┌─────────────────────────────────────────────────────────────┐
│  User Alert Preferences                       [Bulk Update]  │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  Search User: [_____________]  Filter: [All Users ▼]        │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ User               | Email    | Digest | Quiet Hours │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Rajesh Kumar       | Enabled  | No     | Yes         │  │
│  │ [View/Edit Preferences]                               │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Priya Sharma       | Enabled  | Daily  | No          │  │
│  │ [View/Edit Preferences]                               │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Vikram Singh       | Disabled | -      | -           │  │
│  │ [View/Edit Preferences]                               │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                              │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ Set Default Preferences for New Users                 │  │
│  ├──────────────────────────────────────────────────────┤  │
│  │ Email Notifications: ☑ Enabled by default            │  │
│  │ Digest Mode:         ☐ Enable digest by default      │  │
│  │ Quiet Hours:         ☐ Enable quiet hours           │  │
│  │ [Save Defaults]                                        │  │
│  └──────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

---

## Access Control & Permissions

### Configuration Page Permissions Matrix

| Configuration Page            | Admin | Super Admin | HR Manager | Branch HR | Manager |
|-------------------------------|-------|-------------|------------|-----------|---------|
| Escalation Matrices           | ✓     | ✓           | View Only  | ×         | ×       |
| Complaint Categories          | ✓     | ✓           | ✓          | View Only | ×       |
| SLA Rules                     | ✓     | ✓           | ×          | ×         | ×       |
| User Groups                   | ✓     | ✓           | ✓          | Branch Only| ×      |
| Alert Types                   | ✓     | ✓           | ×          | ×         | ×       |
| Email Templates               | ✓     | ✓           | View Only  | ×         | ×       |
| Recipient Rules               | ✓     | ✓           | ×          | ×         | ×       |
| Alert Scheduling              | ✓     | ✓           | ×          | ×         | ×       |
| Alert Analytics               | ✓     | ✓           | Branch Only| Branch Only| ×      |
| User Preferences (Admin View) | ✓     | ✓           | ×          | ×         | ×       |
| Knowledge Base                | ✓     | ✓           | ✓          | ✓         | View Only|
| Survey Templates              | ✓     | ✓           | ✓          | ×         | ×       |
| Branding                      | ✓     | ✓           | ×          | ×         | ×       |
| Tenant Management             | ×     | ✓           | ×          | ×         | ×       |
| HRMS Integration              | ✓     | ✓           | ×          | ×         | ×       |
| Audit Logs                    | ✓     | ✓           | Branch Only| Branch Only| ×      |
| System Settings               | ×     | ✓           | ×          | ×         | ×       |

**Legend**:
- ✓ = Full Access (Read + Write)
- View Only = Read Access Only
- Branch Only = Access limited to own branch
- × = No Access

---

## Best Practices

### 1. Configuration Management

✅ **DO**:
- Test configuration changes in a staging environment first
- Use descriptive names for all configurations
- Document why specific configurations were chosen
- Review and update configurations quarterly
- Enable audit logging for all configuration changes
- Create configuration backups before major changes

❌ **DON'T**:
- Make changes directly in production without testing
- Use default system configurations without review
- Delete configurations without checking dependencies
- Grant configuration access to unauthorized users

### 2. Email Alert Configuration

✅ **DO**:
- Start with system default alert types
- Test email templates before activating
- Use clear, action-oriented subject lines
- Keep templates mobile-friendly
- Monitor email delivery rates
- Respect user preferences and quiet hours
- Use meaningful variable names in templates

❌ **DON'T**:
- Send too many alerts (causes alert fatigue)
- Use overly complex HTML templates
- Ignore user unsubscribe requests
- Send alerts for trivial events
- Use ALL CAPS in subject lines

### 3. Escalation Matrix Design

✅ **DO**:
- Keep escalation paths simple (2-3 levels for most cases)
- Define clear SLA timeframes
- Configure fallback assignees
- Test escalation workflows end-to-end
- Document escalation rationale

❌ **DON'T**:
- Create overly complex matrices with 4-5 levels
- Set unrealistic SLA timeframes
- Forget to configure auto-escalation
- Create circular escalation paths

### 4. User Experience

✅ **DO**:
- Provide inline help and tooltips
- Show preview before saving
- Use validation to prevent errors
- Provide undo/rollback functionality
- Show configuration impact (e.g., "This affects 150 complaints")

❌ **DON'T**:
- Force users to read long documentation
- Use technical jargon in UI
- Allow saving invalid configurations
- Hide important warnings

---

## Maintenance Checklist

### Daily
- [ ] Check alert delivery success rate
- [ ] Review failed email deliveries
- [ ] Monitor system health dashboard

### Weekly
- [ ] Review alert analytics
- [ ] Check for configuration errors in logs
- [ ] Verify HRMS sync status

### Monthly
- [ ] Audit configuration changes
- [ ] Review and optimize email templates (based on analytics)
- [ ] Update recipient rules if org structure changed
- [ ] Clean up unused configurations

### Quarterly
- [ ] Review all active escalation matrices
- [ ] Update SLA rules based on performance data
- [ ] Conduct user feedback survey on alerts
- [ ] Update documentation

---

## Support & Troubleshooting

### Common Issues

#### Issue: Emails not being sent
**Check**:
1. Alert type is enabled
2. Template is active
3. Recipient rules are configured
4. User preferences allow emails
5. No rate limiting in effect

#### Issue: Wrong recipients receiving alerts
**Check**:
1. Recipient rules scope filters
2. Rule priority order
3. HRMS data is up-to-date
4. User mappings are correct

#### Issue: Template variables not replaced
**Check**:
1. Variable names match exactly (case-sensitive)
2. Required data is available in complaint
3. Template format is correct

---

## Changelog

| Version | Date       | Changes |
|---------|------------|---------|
| 1.0     | 2025-10-11 | Initial Configuration Management Guide |

---

**END OF DOCUMENT**
