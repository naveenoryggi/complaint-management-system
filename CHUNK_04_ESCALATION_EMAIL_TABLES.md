# CHUNK 4: Escalation & Email Alert Tables

**Part of**: Master Planning Document
**Module**: Escalation System & Email Alerts
**Status**: Active Tables (Managed in Complaint System)

---

## Overview

These tables manage the flexible N-level escalation system (2-5 levels) and comprehensive email alert configuration with dynamic recipient rules.

---

## 4.1 Escalation Matrices

```sql
CREATE TABLE escalation_matrices (
    matrix_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    matrix_name VARCHAR(100) NOT NULL,
    description TEXT,

    -- Applicability
    category_id UUID REFERENCES complaint_categories(category_id),
    company_id UUID REFERENCES companies(company_id),
    branch_id UUID REFERENCES branches(branch_id),
    department_id UUID REFERENCES departments(department_id),
    priority VARCHAR(20), -- null = all priorities

    -- Escalation configuration
    total_levels INT NOT NULL, -- 2 to 5
    is_active BOOLEAN DEFAULT true,
    is_default BOOLEAN DEFAULT false,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_total_levels CHECK (total_levels BETWEEN 2 AND 5),
    CONSTRAINT chk_priority CHECK (priority IN ('LOW', 'MEDIUM', 'HIGH', 'CRITICAL') OR priority IS NULL)
);

CREATE INDEX idx_escalation_matrices_tenant ON escalation_matrices(tenant_id);
CREATE INDEX idx_escalation_matrices_category ON escalation_matrices(category_id);
CREATE INDEX idx_escalation_matrices_company ON escalation_matrices(company_id);
CREATE INDEX idx_escalation_matrices_branch ON escalation_matrices(branch_id);
CREATE INDEX idx_escalation_matrices_active ON escalation_matrices(is_active);
```

---

## 4.2 Escalation Levels

```sql
CREATE TABLE escalation_levels (
    level_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    matrix_id UUID NOT NULL REFERENCES escalation_matrices(matrix_id) ON DELETE CASCADE,

    level_number INT NOT NULL, -- 1, 2, 3, 4, or 5
    level_name VARCHAR(100) NOT NULL,

    -- Assignment Strategy
    assignment_strategy VARCHAR(50) NOT NULL,
    -- REPORTING_CHAIN, SPECIFIC_USER, ROLE, ROUND_ROBIN, LEAST_LOADED, GROUP

    -- Strategy-specific configuration
    assigned_user_id UUID REFERENCES users(user_id), -- For SPECIFIC_USER
    assigned_role_id UUID REFERENCES complaint_roles(role_id), -- For ROLE
    assigned_group_ids UUID[], -- For GROUP

    -- SLA Configuration
    sla_hours INT NOT NULL, -- Hours to resolve at this level
    auto_escalate BOOLEAN DEFAULT true,

    -- Notification configuration
    notify_on_assignment BOOLEAN DEFAULT true,
    notify_on_sla_breach BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_matrix_level UNIQUE(matrix_id, level_number),
    CONSTRAINT chk_level_number CHECK (level_number BETWEEN 1 AND 5),
    CONSTRAINT chk_assignment_strategy CHECK (assignment_strategy IN (
        'REPORTING_CHAIN', 'SPECIFIC_USER', 'ROLE',
        'ROUND_ROBIN', 'LEAST_LOADED', 'GROUP'
    ))
);

CREATE INDEX idx_escalation_levels_matrix ON escalation_levels(matrix_id);
CREATE INDEX idx_escalation_levels_number ON escalation_levels(level_number);
CREATE INDEX idx_escalation_levels_strategy ON escalation_levels(assignment_strategy);
```

### Assignment Strategy Examples

```typescript
// Strategy implementations
class EscalationAssignmentService {

  async assignByStrategy(
    level: EscalationLevel,
    complaint: Complaint
  ): Promise<string> {
    switch (level.assignment_strategy) {
      case 'REPORTING_CHAIN':
        return await this.assignByReportingChain(complaint, level.level_number);

      case 'SPECIFIC_USER':
        return level.assigned_user_id;

      case 'ROLE':
        return await this.assignByRole(level.assigned_role_id, complaint);

      case 'ROUND_ROBIN':
        return await this.assignByRoundRobin(level, complaint);

      case 'LEAST_LOADED':
        return await this.assignByLeastLoaded(level, complaint);

      case 'GROUP':
        return await this.assignByGroup(level.assigned_group_ids, complaint);
    }
  }

  private async assignByReportingChain(
    complaint: Complaint,
    level: number
  ): Promise<string> {
    let currentUser = await this.db.users.findByPk(complaint.created_by_user_id);

    // Traverse up the reporting chain
    for (let i = 0; i < level && currentUser; i++) {
      if (!currentUser.manager_id) break;
      currentUser = await this.db.users.findByPk(currentUser.manager_id);
    }

    return currentUser?.user_id;
  }

  private async assignByRole(
    roleId: string,
    complaint: Complaint
  ): Promise<string> {
    // Find users with this role in the same branch/department
    const users = await this.db.user_complaint_roles.findAll({
      where: {
        role_id: roleId,
        branch_id: complaint.branch_id,
        is_active: true
      }
    });

    // Return first available user
    return users[0]?.user_id;
  }

  private async assignByRoundRobin(
    level: EscalationLevel,
    complaint: Complaint
  ): Promise<string> {
    // Get eligible users for this level
    const users = await this.getEligibleUsers(level, complaint);

    // Get last assigned user for this level
    const lastAssignment = await this.db.escalation_history.findOne({
      where: {
        level_id: level.level_id,
        branch_id: complaint.branch_id
      },
      order: [['created_at', 'DESC']]
    });

    // Find next user in rotation
    let nextIndex = 0;
    if (lastAssignment) {
      const lastIndex = users.findIndex(u => u.user_id === lastAssignment.assigned_to_user_id);
      nextIndex = (lastIndex + 1) % users.length;
    }

    return users[nextIndex]?.user_id;
  }

  private async assignByLeastLoaded(
    level: EscalationLevel,
    complaint: Complaint
  ): Promise<string> {
    // Get eligible users
    const users = await this.getEligibleUsers(level, complaint);

    // Count active complaints per user
    const userLoads = await Promise.all(
      users.map(async (user) => {
        const count = await this.db.complaints.count({
          where: {
            assigned_to_user_id: user.user_id,
            status: { [Op.in]: ['ASSIGNED', 'IN_PROGRESS', 'ESCALATED'] }
          }
        });
        return { user_id: user.user_id, load: count };
      })
    );

    // Sort by load and return least loaded
    userLoads.sort((a, b) => a.load - b.load);
    return userLoads[0]?.user_id;
  }
}
```

---

## 4.3 Escalation History

```sql
CREATE TABLE escalation_history (
    history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id UUID NOT NULL REFERENCES complaints(complaint_id) ON DELETE CASCADE,

    from_level INT, -- null for initial assignment
    to_level INT NOT NULL,

    from_user_id UUID REFERENCES users(user_id),
    to_user_id UUID REFERENCES users(user_id),

    escalation_reason VARCHAR(50) NOT NULL,
    -- AUTO_SLA_BREACH, MANUAL_ESCALATION, REASSIGNMENT, INITIAL_ASSIGNMENT

    escalated_by_user_id UUID REFERENCES users(user_id),
    escalated_at TIMESTAMP DEFAULT NOW(),

    notes TEXT,

    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_escalation_reason CHECK (escalation_reason IN (
        'AUTO_SLA_BREACH', 'MANUAL_ESCALATION', 'REASSIGNMENT', 'INITIAL_ASSIGNMENT'
    ))
);

CREATE INDEX idx_escalation_history_complaint ON escalation_history(complaint_id);
CREATE INDEX idx_escalation_history_to_level ON escalation_history(to_level);
CREATE INDEX idx_escalation_history_escalated_at ON escalation_history(escalated_at);
```

---

## 4.4 Alert Types

```sql
CREATE TABLE alert_types (
    alert_type_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),

    type_code VARCHAR(100) NOT NULL,
    type_name VARCHAR(255) NOT NULL,
    description TEXT,

    -- Category: COMPLAINT, ESCALATION, SLA, RESOLUTION, SYSTEM, CUSTOM
    category VARCHAR(50) NOT NULL,

    -- Trigger event
    trigger_event VARCHAR(100) NOT NULL,
    -- COMPLAINT_CREATED, COMPLAINT_ASSIGNED, COMPLAINT_ESCALATED,
    -- SLA_BREACH_WARNING, COMPLAINT_RESOLVED, etc.

    trigger_conditions JSONB DEFAULT '{}',
    -- Example: {"priority": ["HIGH", "CRITICAL"], "category_id": "uuid"}

    is_enabled BOOLEAN DEFAULT true,
    is_system_default BOOLEAN DEFAULT false,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_alert_type UNIQUE(tenant_id, type_code),
    CONSTRAINT chk_category CHECK (category IN (
        'COMPLAINT', 'ESCALATION', 'SLA', 'RESOLUTION', 'SYSTEM', 'CUSTOM'
    ))
);

CREATE INDEX idx_alert_types_tenant ON alert_types(tenant_id);
CREATE INDEX idx_alert_types_category ON alert_types(category);
CREATE INDEX idx_alert_types_trigger ON alert_types(trigger_event);
CREATE INDEX idx_alert_types_enabled ON alert_types(is_enabled);
```

### Pre-defined Alert Types

```sql
INSERT INTO alert_types (tenant_id, type_code, type_name, category, trigger_event, is_system_default) VALUES
('{tenant_id}', 'COMPLAINT_CREATED', 'New Complaint Created', 'COMPLAINT', 'complaint.created', true),
('{tenant_id}', 'COMPLAINT_ASSIGNED', 'Complaint Assigned', 'COMPLAINT', 'complaint.assigned', true),
('{tenant_id}', 'COMPLAINT_ESCALATED', 'Complaint Escalated', 'ESCALATION', 'complaint.escalated', true),
('{tenant_id}', 'SLA_BREACH_WARNING', 'SLA Breach Warning', 'SLA', 'sla.breach_warning', true),
('{tenant_id}', 'SLA_BREACHED', 'SLA Breached', 'SLA', 'sla.breached', true),
('{tenant_id}', 'COMPLAINT_RESOLVED', 'Complaint Resolved', 'RESOLUTION', 'complaint.resolved', true),
('{tenant_id}', 'FEEDBACK_REQUEST', 'Feedback Request', 'RESOLUTION', 'complaint.feedback_request', true),
('{tenant_id}', 'DAILY_DIGEST', 'Daily Digest', 'SYSTEM', 'system.daily_digest', true);
```

---

## 4.5 Email Alert Templates

```sql
CREATE TABLE email_alert_templates (
    template_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,

    template_name VARCHAR(255) NOT NULL,
    subject_template TEXT NOT NULL,
    body_template TEXT NOT NULL,

    -- Template format: HTML, PLAIN_TEXT
    body_format VARCHAR(20) DEFAULT 'HTML',

    -- Available variables for substitution
    available_variables JSONB DEFAULT '[]',
    -- Example: ["{{complaint_number}}", "{{employee_name}}", "{{category}}", "{{priority}}"]

    -- Email styling
    header_html TEXT,
    footer_html TEXT,
    css_styles TEXT,

    -- Attachments
    include_attachments BOOLEAN DEFAULT false,

    -- Email priority
    email_priority VARCHAR(20) DEFAULT 'NORMAL', -- LOW, NORMAL, HIGH

    -- Reply-to
    reply_to_email VARCHAR(255),

    is_active BOOLEAN DEFAULT true,
    version INT DEFAULT 1,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_body_format CHECK (body_format IN ('HTML', 'PLAIN_TEXT')),
    CONSTRAINT chk_email_priority CHECK (email_priority IN ('LOW', 'NORMAL', 'HIGH'))
);

CREATE INDEX idx_email_templates_tenant ON email_alert_templates(tenant_id);
CREATE INDEX idx_email_templates_alert_type ON email_alert_templates(alert_type_id);
CREATE INDEX idx_email_templates_active ON email_alert_templates(is_active);
```

### Sample Email Templates

```sql
-- Template: Complaint Created
INSERT INTO email_alert_templates (
  tenant_id, alert_type_id, template_name,
  subject_template, body_template, available_variables
) VALUES (
  '{tenant_id}',
  '{complaint_created_alert_id}',
  'New Complaint Created - Default',
  'New Complaint: {{complaint_number}} - {{subject}}',
  '<h2>New Complaint Received</h2>
   <p>Dear {{assigned_user_name}},</p>
   <p>A new complaint has been created and assigned to you.</p>
   <table>
     <tr><td>Complaint Number:</td><td><strong>{{complaint_number}}</strong></td></tr>
     <tr><td>Created By:</td><td>{{employee_name}}</td></tr>
     <tr><td>Category:</td><td>{{category_name}}</td></tr>
     <tr><td>Priority:</td><td>{{priority}}</td></tr>
     <tr><td>Subject:</td><td>{{subject}}</td></tr>
   </table>
   <p><a href="{{complaint_url}}">View Complaint</a></p>',
  '["{{complaint_number}}", "{{subject}}", "{{employee_name}}", "{{assigned_user_name}}", "{{category_name}}", "{{priority}}", "{{complaint_url}}"]'
);

-- Template: Complaint Escalated
INSERT INTO email_alert_templates (
  tenant_id, alert_type_id, template_name,
  subject_template, body_template, available_variables
) VALUES (
  '{tenant_id}',
  '{complaint_escalated_alert_id}',
  'Complaint Escalated - Default',
  '🚨 ESCALATED: {{complaint_number}} - Level {{escalation_level}}',
  '<h2 style="color: #d32f2f;">Complaint Escalated</h2>
   <p>Dear {{assigned_user_name}},</p>
   <p>A complaint has been escalated to you at <strong>Level {{escalation_level}}</strong>.</p>
   <table>
     <tr><td>Complaint Number:</td><td><strong>{{complaint_number}}</strong></td></tr>
     <tr><td>Original Creator:</td><td>{{employee_name}}</td></tr>
     <tr><td>Escalation Reason:</td><td>{{escalation_reason}}</td></tr>
     <tr><td>SLA Due Date:</td><td>{{sla_due_date}}</td></tr>
   </table>
   <p><a href="{{complaint_url}}" style="background: #d32f2f; color: white; padding: 10px 20px;">View Complaint</a></p>',
  '["{{complaint_number}}", "{{escalation_level}}", "{{assigned_user_name}}", "{{employee_name}}", "{{escalation_reason}}", "{{sla_due_date}}", "{{complaint_url}}"]'
);
```

---

## 4.6 Alert Recipients

```sql
CREATE TABLE alert_recipients (
    recipient_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,

    recipient_type VARCHAR(50) NOT NULL,
    -- ASSIGNED_USER, CREATOR, MANAGER, ESCALATION_HANDLER, ROLE,
    -- HR_TEAM, EMAIL_LIST, DEPARTMENT_HEAD, BRANCH_HR

    -- Type-specific configuration
    role_id UUID REFERENCES complaint_roles(role_id), -- For ROLE type
    email_addresses TEXT[], -- For EMAIL_LIST type
    user_ids UUID[], -- For specific users

    -- Recipient inclusion
    include_as VARCHAR(20) DEFAULT 'TO', -- TO, CC, BCC

    -- Conditions (when to include this recipient)
    conditions JSONB DEFAULT '{}',
    -- Example: {"priority": ["HIGH", "CRITICAL"], "branch_id": "uuid"}

    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_recipient_type CHECK (recipient_type IN (
        'ASSIGNED_USER', 'CREATOR', 'MANAGER', 'ESCALATION_HANDLER',
        'ROLE', 'HR_TEAM', 'EMAIL_LIST', 'DEPARTMENT_HEAD', 'BRANCH_HR'
    )),
    CONSTRAINT chk_include_as CHECK (include_as IN ('TO', 'CC', 'BCC'))
);

CREATE INDEX idx_alert_recipients_tenant ON alert_recipients(tenant_id);
CREATE INDEX idx_alert_recipients_alert_type ON alert_recipients(alert_type_id);
CREATE INDEX idx_alert_recipients_type ON alert_recipients(recipient_type);
CREATE INDEX idx_alert_recipients_active ON alert_recipients(is_active);
```

### Recipient Resolution Logic

```typescript
class RecipientResolver {

  async resolveRecipients(
    alertType: AlertType,
    complaint: Complaint,
    context: any
  ): Promise<{ to: string[]; cc: string[]; bcc: string[] }> {
    const recipients = await this.db.alert_recipients.findAll({
      where: { alert_type_id: alertType.alert_type_id, is_active: true }
    });

    const to: string[] = [];
    const cc: string[] = [];
    const bcc: string[] = [];

    for (const recipient of recipients) {
      // Check conditions
      if (!this.matchConditions(recipient.conditions, complaint)) {
        continue;
      }

      const emails = await this.resolveByType(recipient, complaint, context);

      switch (recipient.include_as) {
        case 'TO':
          to.push(...emails);
          break;
        case 'CC':
          cc.push(...emails);
          break;
        case 'BCC':
          bcc.push(...emails);
          break;
      }
    }

    return {
      to: [...new Set(to)], // Remove duplicates
      cc: [...new Set(cc)],
      bcc: [...new Set(bcc)]
    };
  }

  private async resolveByType(
    recipient: AlertRecipient,
    complaint: Complaint,
    context: any
  ): Promise<string[]> {
    switch (recipient.recipient_type) {
      case 'ASSIGNED_USER':
        const assignedUser = await this.db.users.findByPk(complaint.assigned_to_user_id);
        return assignedUser ? [assignedUser.email] : [];

      case 'CREATOR':
        const creator = await this.db.users.findByPk(complaint.created_by_user_id);
        return creator ? [creator.email] : [];

      case 'MANAGER':
        const employee = await this.db.users.findByPk(complaint.created_by_user_id);
        if (employee?.manager_id) {
          const manager = await this.db.users.findByPk(employee.manager_id);
          return manager ? [manager.email] : [];
        }
        return [];

      case 'ROLE':
        const usersWithRole = await this.db.user_complaint_roles.findAll({
          where: {
            role_id: recipient.role_id,
            branch_id: complaint.branch_id,
            is_active: true
          },
          include: ['user']
        });
        return usersWithRole.map(ur => ur.user.email);

      case 'EMAIL_LIST':
        return recipient.email_addresses || [];

      case 'DEPARTMENT_HEAD':
        const dept = await this.db.departments.findByPk(complaint.department_id);
        if (dept?.head_user_id) {
          const head = await this.db.users.findByPk(dept.head_user_id);
          return head ? [head.email] : [];
        }
        return [];

      case 'HR_TEAM':
        const hrUsers = await this.db.user_complaint_roles.findAll({
          where: {
            role_id: { [Op.in]: await this.getHRRoleIds() },
            branch_id: complaint.branch_id,
            is_active: true
          },
          include: ['user']
        });
        return hrUsers.map(hr => hr.user.email);

      default:
        return [];
    }
  }
}
```

---

## 4.7 Alert Schedule Configuration

```sql
CREATE TABLE alert_schedules (
    schedule_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    tenant_id UUID NOT NULL REFERENCES tenants(tenant_id),
    alert_type_id UUID NOT NULL REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,

    schedule_type VARCHAR(50) NOT NULL, -- IMMEDIATE, DELAYED, SCHEDULED, BATCH

    -- For DELAYED
    delay_minutes INT,

    -- For SCHEDULED
    send_at_time TIME, -- e.g., 09:00:00
    send_on_days INT[], -- 1=Monday, 2=Tuesday, etc.

    -- For BATCH
    batch_interval_minutes INT,
    batch_max_items INT,

    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT chk_schedule_type CHECK (schedule_type IN (
        'IMMEDIATE', 'DELAYED', 'SCHEDULED', 'BATCH'
    ))
);

CREATE INDEX idx_alert_schedules_tenant ON alert_schedules(tenant_id);
CREATE INDEX idx_alert_schedules_alert_type ON alert_schedules(alert_type_id);
CREATE INDEX idx_alert_schedules_type ON alert_schedules(schedule_type);
```

---

## 4.8 User Alert Preferences

```sql
CREATE TABLE user_alert_preferences (
    preference_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,

    alert_type_id UUID REFERENCES alert_types(alert_type_id) ON DELETE CASCADE,
    -- null = applies to all alert types

    -- Channels
    email_enabled BOOLEAN DEFAULT true,
    sms_enabled BOOLEAN DEFAULT false,
    in_app_enabled BOOLEAN DEFAULT true,

    -- Frequency
    frequency VARCHAR(50) DEFAULT 'IMMEDIATE',
    -- IMMEDIATE, DAILY_DIGEST, WEEKLY_DIGEST, DISABLED

    -- Quiet hours
    quiet_hours_start TIME,
    quiet_hours_end TIME,

    -- Day preferences
    send_on_weekends BOOLEAN DEFAULT true,

    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT unique_user_alert_pref UNIQUE(user_id, alert_type_id),
    CONSTRAINT chk_frequency CHECK (frequency IN (
        'IMMEDIATE', 'DAILY_DIGEST', 'WEEKLY_DIGEST', 'DISABLED'
    ))
);

CREATE INDEX idx_user_alert_prefs_user ON user_alert_preferences(user_id);
CREATE INDEX idx_user_alert_prefs_alert_type ON user_alert_preferences(alert_type_id);
CREATE INDEX idx_user_alert_prefs_frequency ON user_alert_preferences(frequency);
```

---

## Summary

**Chunk 4 Tables Created**:
1. ✅ escalation_matrices - Escalation configuration
2. ✅ escalation_levels - Level-wise settings (2-5 levels)
3. ✅ escalation_history - Escalation audit trail
4. ✅ alert_types - Alert type definitions
5. ✅ email_alert_templates - Email templates with variables
6. ✅ alert_recipients - Dynamic recipient rules
7. ✅ alert_schedules - Email scheduling configuration
8. ✅ user_alert_preferences - User notification preferences

**Key Features**:
- Flexible N-level escalation (2-5 levels)
- 6 assignment strategies (Reporting Chain, Role, Round Robin, etc.)
- Customizable email templates with variable substitution
- 9 recipient types with dynamic resolution
- Email scheduling (immediate, delayed, batched)
- User preferences for notification control
- SLA-based auto-escalation

---

**Next**: [Chunk 5 - Oryggi Integration Details →](CHUNK_05_ORYGGI_INTEGRATION.md)
