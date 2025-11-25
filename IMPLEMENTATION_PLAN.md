# Implementation Plan - Complaint Management System

**Date Created:** October 17, 2025
**Status:** Pending Implementation

---

## 1. NOTIFICATION SYSTEM - COMPREHENSIVE PLAN

### 1.1 Overview
Build a professional, event-driven notification system with Email/SMS capabilities, comprehensive admin configuration, and integration with complaint lifecycle and escalation policies.

### 1.2 Notification Events (30+ Events)

#### A. Complaint Lifecycle Events
- **Complaint Created** - When new complaint is submitted
- **Complaint Assigned** - When complaint is assigned to handler
- **Complaint Reassigned** - When complaint is reassigned to different handler
- **Complaint Status Changed** - When status changes (Open → In Progress → Pending → Resolved)
- **Complaint Resolved** - When complaint is marked as resolved
- **Complaint Closed** - When complaint is closed
- **Complaint Reopened** - When closed complaint is reopened
- **Complaint Rejected** - When complaint is rejected

#### B. Escalation Events
- **Escalation Triggered** - When auto-escalation is triggered
- **Escalation Level Changed** - When escalation moves to next level
- **Escalation Manual** - When manager manually escalates
- **Pre-Escalation Warning** - Warning before auto-escalation (e.g., 2 hours before 24-hour deadline)
- **SLA Breach Warning** - When nearing SLA deadline
- **SLA Breach** - When SLA is breached

#### C. Comment & Communication Events
- **Comment Added** - When someone adds a comment
- **Comment Mentioned** - When user is @mentioned in comment
- **Internal Note Added** - When internal note is added
- **Reply Required** - When employee response is needed

#### D. Assignment & Handler Events
- **Handler Assigned** - Notification to assigned handler
- **Handler Changed** - Notification when handler changes
- **Backup Handler Activated** - When backup handler needs to take over
- **Department Head Notified** - Escalation to department head
- **Branch Manager Notified** - Escalation to branch manager
- **HR Contacted** - When HR involvement is required

#### E. Reminder Events
- **Daily Digest** - Daily summary of pending complaints
- **Overdue Reminder** - Reminder for overdue complaints
- **Pending Action Reminder** - Reminder for pending actions
- **Follow-up Reminder** - Follow-up reminders

#### F. Administrative Events
- **Complaint Deleted** - When complaint is deleted (notify stakeholders)
- **Mass Assignment** - When multiple complaints are assigned
- **Report Generated** - When reports are generated
- **System Maintenance** - System maintenance notifications

### 1.3 Notification Channels

#### Email Notifications
- HTML formatted professional emails
- Plain text fallback
- Attachments support (PDFs, reports)
- Reply-to tracking
- Email threading for conversations

#### SMS Notifications
- Short format notifications (160 characters)
- Unicode support for multiple languages
- Delivery status tracking
- Priority SMS for critical events

#### In-App Notifications (Future Phase)
- Real-time browser notifications
- Notification center in app
- Read/unread status

### 1.4 Recipient Groups & Targeting

#### Primary Recipients
- **Complaint Creator** (Employee who filed complaint)
- **Assigned Handler** (Current handler)
- **Previous Handlers** (For escalations)

#### Management Chain
- **Direct Manager** (Creator's immediate supervisor)
- **Department Head** (Department manager)
- **Section Manager** (Section in-charge)
- **Branch Manager** (Branch head)
- **Regional Manager** (Multi-branch oversight)

#### Contact Hierarchy (from Escalation Matrix)
- **Primary Contact** (First point of contact)
- **Secondary Contact** (Backup contact)
- **HR Contact** (HR representative)

#### Special Groups
- **CC Recipients** (Additional stakeholders)
- **BCC Recipients** (Hidden recipients for auditing)
- **External Recipients** (Vendors, partners)

### 1.5 SMTP Configuration Features

```
SMTP Server Configuration:
├── Server Settings
│   ├── SMTP Host (smtp.gmail.com, smtp.office365.com, etc.)
│   ├── SMTP Port (25, 465, 587, 2525)
│   ├── Username/Email
│   ├── Password (encrypted storage)
│   └── From Email Display Name
│
├── Security Settings
│   ├── SSL/TLS Enable (Yes/No)
│   ├── StartTLS Enable (Yes/No)
│   ├── Certificate Validation
│   └── Authentication Method (Plain, Login, CRAM-MD5)
│
├── Advanced Settings
│   ├── Connection Timeout (seconds)
│   ├── Max Retry Attempts
│   ├── Retry Delay (seconds)
│   ├── Max Emails per Hour (Rate limiting)
│   ├── Bounce Email Address
│   └── Reply-To Email Address
│
├── Email Preferences
│   ├── Default Priority (Low/Normal/High)
│   ├── Enable HTML Emails
│   ├── Enable Plain Text Fallback
│   ├── Enable Email Threading
│   └── Enable Read Receipts
│
└── Testing & Validation
    ├── Test Email Configuration
    ├── Connection Test
    └── Send Test Email
```

### 1.6 SMS Gateway Configuration Features

```
SMS Gateway Configuration:
├── Provider Settings
│   ├── Provider Selection (Twilio, AWS SNS, Nexmo, MSG91, etc.)
│   ├── API Endpoint
│   ├── Account SID / API Key
│   ├── Auth Token / API Secret
│   └── Sender ID / Phone Number
│
├── Gateway Features
│   ├── Support Unicode (Yes/No)
│   ├── Support Flash SMS
│   ├── Delivery Report Webhook URL
│   ├── Max SMS Length (160/306/459)
│   └── Concatenation Support
│
├── Rate Limiting & Cost
│   ├── Max SMS per Hour
│   ├── Max SMS per Day
│   ├── Cost per SMS (for tracking)
│   └── Monthly Budget Limit
│
├── Regional Settings
│   ├── Default Country Code
│   ├── Allowed Country Codes
│   └── Blocked Country Codes
│
├── Fallback Configuration
│   ├── Enable Fallback Gateway
│   ├── Secondary Provider
│   └── Fallback Triggers
│
└── Testing & Validation
    ├── Test SMS Configuration
    ├── Connection Test
    └── Send Test SMS
```

### 1.7 Notification Template System

#### Template Variables (Placeholders)
```
Complaint Variables:
- {{ComplaintId}}
- {{ComplaintNumber}}
- {{ComplaintTitle}}
- {{ComplaintDescription}}
- {{ComplaintCategory}}
- {{ComplaintPriority}}
- {{ComplaintStatus}}
- {{CreatedDate}}
- {{DueDate}}
- {{DaysOverdue}}

User Variables:
- {{EmployeeName}}
- {{EmployeeCode}}
- {{EmployeeEmail}}
- {{EmployeePhone}}
- {{HandlerName}}
- {{ManagerName}}
- {{DepartmentName}}
- {{BranchName}}
- {{SectionName}}
- {{CompanyName}}

Escalation Variables:
- {{EscalationLevel}}
- {{EscalationMatrix}}
- {{TriggerTime}}
- {{NextEscalationLevel}}
- {{EscalationReason}}

System Variables:
- {{SystemURL}}
- {{SupportEmail}}
- {{SupportPhone}}
- {{CurrentDate}}
- {{CurrentTime}}
```

### 1.8 Database Schema (New Tables)

```sql
-- Email Configuration
NotificationEmailSettings
- Id (PK)
- CompanyId (FK)
- SmtpHost
- SmtpPort
- SmtpUsername
- SmtpPassword (encrypted)
- FromEmail
- FromDisplayName
- EnableSsl
- EnableStartTls
- EnableAuthentication
- AuthenticationMethod
- ConnectionTimeout
- MaxRetryAttempts
- RetryDelaySeconds
- MaxEmailsPerHour
- BounceEmail
- ReplyToEmail
- EnableHtmlEmails
- EnablePlainTextFallback
- EnableEmailThreading
- DefaultPriority
- IsActive
- CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

-- SMS Gateway Configuration
NotificationSmsSettings
- Id (PK)
- CompanyId (FK)
- ProviderName (Twilio, AWS SNS, Nexmo, MSG91, Custom)
- ApiEndpoint
- AccountSid
- AuthToken (encrypted)
- SenderId
- SupportUnicode
- SupportFlashSms
- DeliveryReportWebhook
- MaxSmsLength
- MaxSmsPerHour
- MaxSmsPerDay
- CostPerSms
- MonthlyBudgetLimit
- DefaultCountryCode
- EnableFallback
- FallbackProviderId
- IsActive
- CreatedAt, UpdatedAt, CreatedBy, UpdatedBy

-- Notification Templates
NotificationTemplates
- Id (PK)
- CompanyId (FK)
- TemplateName
- TemplateCode (unique identifier)
- EventType (enum: ComplaintCreated, ComplaintAssigned, etc.)
- ChannelType (Email/SMS)
- SubjectLine (for email)
- MessageBody (text with variables)
- HeaderHtml (for email)
- FooterHtml (for email)
- ButtonText
- ButtonUrl
- Language (English/Hindi/etc.)
- IsDefault
- IsActive
- CreatedBy, CreatedAt, UpdatedBy, UpdatedAt

-- Notification Rules
NotificationRules
- Id (PK)
- CompanyId (FK)
- RuleName
- RuleDescription
- EventType (enum)
- Priority (1-10, higher = higher priority)
- IsActive
- CreatedBy, CreatedAt, UpdatedBy, UpdatedAt

-- Notification Rule Conditions
NotificationRuleConditions
- Id (PK)
- RuleId (FK)
- ConditionType (Category/Priority/Department/Branch/EscalationLevel)
- ConditionOperator (Equals/NotEquals/In/NotIn)
- ConditionValue (JSON array for multi-select)
- CreatedAt

-- Notification Rule Recipients
NotificationRuleRecipients
- Id (PK)
- RuleId (FK)
- RecipientType (Employee/Handler/Manager/DepartmentHead/BranchManager/HR/PrimaryContact/SecondaryContact)
- ChannelType (Email/SMS/Both)
- EmailTemplateId (FK, nullable)
- SmsTemplateId (FK, nullable)
- SendImmediately (bool)
- DelayMinutes (int)
- AdditionalCcEmails (JSON array)
- AdditionalBccEmails (JSON array)
- CreatedAt

-- Notification Queue
NotificationQueue
- Id (PK)
- CompanyId (FK)
- ComplaintId (FK)
- EventType (enum)
- RecipientType (enum)
- RecipientUserId (FK, nullable)
- RecipientEmail (nullable)
- RecipientPhone (nullable)
- ChannelType (Email/SMS)
- TemplateId (FK)
- Subject (for email)
- MessageBody
- Priority (Low/Normal/High)
- Status (Pending/Sent/Failed/Retry/Cancelled)
- ScheduledAt
- SentAt
- ErrorMessage
- RetryCount
- MaxRetries
- CreatedAt, UpdatedAt

-- Notification Logs
NotificationLogs
- Id (PK)
- QueueId (FK)
- ComplaintId (FK)
- RecipientUserId (FK, nullable)
- RecipientContact (email or phone)
- ChannelType (Email/SMS)
- EventType
- Status (Sent/Delivered/Bounced/Failed/Opened/Clicked)
- ProviderResponse (JSON)
- DeliveryStatus (JSON from provider)
- SentAt
- DeliveredAt
- OpenedAt (for emails)
- ClickedAt (for email links)
- BouncedAt
- ErrorMessage
- CreatedAt
```

### 1.9 Admin UI Pages

#### Page 1: Notification Dashboard
- Overview cards (Total sent, delivery rates, failed notifications)
- Quick actions (Test email, Test SMS, View queue, View logs)
- Configuration status indicators
- Recent notification activity

#### Page 2: SMTP Configuration
- Server settings form (Host, Port, Credentials)
- Security settings (SSL/TLS toggles, Certificate validation)
- Advanced settings (Timeouts, Retry logic, Rate limits)
- Test connection & Send test email buttons
- Save/Reset configuration

#### Page 3: SMS Gateway Configuration
- Provider selection dropdown (Dynamic form based on provider)
- Gateway settings (API credentials, Sender ID)
- SMS settings (Unicode, Length, Delivery reports)
- Rate limiting & Cost tracking
- Test connection & Send test SMS buttons
- Save/Reset configuration

#### Page 4: Template Management
- Template list (filterable by channel, event type, status)
- Create/Edit template modal
  - Template details (Name, Event type, Channel)
  - Email editor (Subject, Rich text body, Header/Footer)
  - SMS editor (Text area with character counter)
  - Variable picker (drag & drop or click to insert)
  - Live preview with sample data
  - Test send functionality
- Duplicate/Delete templates
- Activate/Deactivate templates

#### Page 5: Notification Rules
- Rules list (sortable by priority, filterable by status)
- Create/Edit rule wizard (Multi-tab)
  - Tab 1: Rule Details (Name, Description, Priority)
  - Tab 2: Trigger Conditions (Event types, Categories, Priorities, Departments)
  - Tab 3: Recipients (Checkboxes for recipient types, Additional CC/BCC)
  - Tab 4: Notification Settings (Email/SMS toggles, Template selection, Timing)
- Test rule functionality
- Activate/Deactivate rules
- Delete rules

#### Page 6: Notification Queue & Logs
- Queue view (Pending, Sent, Failed notifications)
- Filters (Status, Channel, Date range, Search)
- Retry failed notifications
- View detailed logs
- Export logs to CSV/Excel
- Statistics & Charts (Delivery rates, Failure reasons)

### 1.10 Implementation Phases

#### Phase 1: Database & Backend Core (Week 1)
**Files to Create:**
```
Backend/Domain:
- Domain/Enums/NotificationEventType.cs
- Domain/Enums/NotificationChannelType.cs
- Domain/Enums/NotificationStatus.cs
- Domain/Enums/RecipientType.cs
- Domain/Enums/SmsProvider.cs
- Domain/Entities/NotificationEmailSettings.cs
- Domain/Entities/NotificationSmsSettings.cs
- Domain/Entities/NotificationTemplate.cs
- Domain/Entities/NotificationRule.cs
- Domain/Entities/NotificationRuleCondition.cs
- Domain/Entities/NotificationRuleRecipient.cs
- Domain/Entities/NotificationQueue.cs
- Domain/Entities/NotificationLog.cs

Backend/Infrastructure:
- Infrastructure/Data/Migrations/AddNotificationSystem.cs
- Infrastructure/Services/EmailService.cs
- Infrastructure/Services/SmsService.cs
- Infrastructure/Services/NotificationTemplateService.cs
- Infrastructure/Services/NotificationQueueService.cs
```

**Tasks:**
- [ ] Create all enums
- [ ] Create all domain entities
- [ ] Create database migration
- [ ] Run migration and verify tables
- [ ] Create IEmailService interface
- [ ] Implement EmailService with SMTP support
- [ ] Create ISmsService interface
- [ ] Implement SmsService with multiple provider support (Twilio, AWS SNS, MSG91)
- [ ] Create template rendering engine
- [ ] Create notification queue service

#### Phase 2: Configuration APIs (Week 2)
**Files to Create:**
```
Backend/Application:
- Application/DTOs/Notification/EmailSettingsDto.cs
- Application/DTOs/Notification/SmsSettingsDto.cs
- Application/DTOs/Notification/NotificationTemplateDto.cs
- Application/DTOs/Notification/NotificationRuleDto.cs
- Application/Features/Notification/Commands/ConfigureEmailCommand.cs
- Application/Features/Notification/Commands/ConfigureSmsCommand.cs
- Application/Features/Notification/Commands/CreateTemplateCommand.cs
- Application/Features/Notification/Commands/CreateRuleCommand.cs
- Application/Features/Notification/Queries/GetEmailSettingsQuery.cs
- Application/Features/Notification/Queries/GetSmsSettingsQuery.cs
- Application/Features/Notification/Queries/GetTemplatesQuery.cs
- Application/Features/Notification/Queries/GetRulesQuery.cs

Backend/API:
- API/Controllers/NotificationConfigController.cs
- API/Controllers/NotificationTemplateController.cs
- API/Controllers/NotificationRuleController.cs
```

**Tasks:**
- [ ] Create all DTOs
- [ ] Create SMTP configuration CRUD commands/queries
- [ ] Create SMS gateway configuration CRUD commands/queries
- [ ] Create template management CRUD commands/queries
- [ ] Create notification rule CRUD commands/queries
- [ ] Create test email endpoint
- [ ] Create test SMS endpoint
- [ ] Create test connection endpoints
- [ ] Add validation for all configurations

#### Phase 3: Notification Engine (Week 3)
**Files to Create:**
```
Backend/Infrastructure:
- Infrastructure/Services/NotificationEngine.cs
- Infrastructure/Services/NotificationRuleEvaluator.cs
- Infrastructure/BackgroundServices/NotificationWorker.cs
- Infrastructure/EventHandlers/ComplaintCreatedEventHandler.cs
- Infrastructure/EventHandlers/ComplaintAssignedEventHandler.cs
- Infrastructure/EventHandlers/ComplaintEscalatedEventHandler.cs
- Infrastructure/EventHandlers/ComplaintStatusChangedEventHandler.cs
```

**Tasks:**
- [ ] Create notification engine service
- [ ] Implement rule evaluation logic
- [ ] Implement template rendering with variables
- [ ] Create background worker for queue processing
- [ ] Implement retry mechanism
- [ ] Create event handlers for complaint lifecycle events
- [ ] Integrate with escalation system
- [ ] Add logging and error handling
- [ ] Implement rate limiting
- [ ] Add delivery tracking

#### Phase 4: Admin UI - Configuration (Week 4)
**Files to Create:**
```
Frontend:
- src/app/components/admin/notifications/notification-dashboard/
  - notification-dashboard.component.ts
  - notification-dashboard.component.html
  - notification-dashboard.component.scss

- src/app/components/admin/notifications/smtp-config/
  - smtp-config.component.ts
  - smtp-config.component.html
  - smtp-config.component.scss

- src/app/components/admin/notifications/sms-config/
  - sms-config.component.ts
  - sms-config.component.html
  - sms-config.component.scss

- src/app/components/admin/notifications/template-management/
  - template-list.component.ts/html/scss
  - template-editor.component.ts/html/scss

- src/app/components/admin/notifications/notification-rules/
  - rule-list.component.ts/html/scss
  - rule-editor.component.ts/html/scss

- src/app/components/admin/notifications/notification-logs/
  - notification-logs.component.ts/html/scss

- src/app/services/notification.service.ts
- src/app/models/notification.model.ts
```

**Tasks:**
- [ ] Create notification models
- [ ] Create notification service
- [ ] Build notification dashboard
- [ ] Build SMTP configuration page
- [ ] Build SMS gateway configuration page
- [ ] Build template management UI
  - Template list with filters
  - Template editor with rich text for email
  - Template editor with character counter for SMS
  - Variable picker/inserter
  - Live preview
- [ ] Build notification rules UI
  - Rules list
  - Rule editor wizard
  - Condition builder
  - Recipient selector
- [ ] Build notification logs page
  - Queue view
  - Log viewer
  - Statistics charts
- [ ] Add form validations
- [ ] Add loading states
- [ ] Add error handling

#### Phase 5: Integration & Testing (Week 5)
**Tasks:**
- [ ] Integrate notification events with complaint creation
- [ ] Integrate notification events with complaint assignment
- [ ] Integrate notification events with complaint status changes
- [ ] Integrate notification events with escalation triggers
- [ ] Integrate pre-escalation warnings
- [ ] Test all notification templates
- [ ] Test SMTP with various providers (Gmail, Office365, custom)
- [ ] Test SMS with Twilio
- [ ] Test notification rules evaluation
- [ ] Test queue processing and retry mechanism
- [ ] Test rate limiting
- [ ] Performance testing (bulk notifications)
- [ ] Security testing (credential encryption)
- [ ] Create admin documentation
- [ ] Create user documentation

### 1.11 Key Features Summary

✅ **30+ Event Types** covering complete complaint lifecycle
✅ **Multi-Channel** notifications (Email, SMS, In-App future)
✅ **Flexible Recipient Targeting** (8+ recipient types)
✅ **Professional SMTP Configuration** with SSL/TLS, authentication, rate limiting
✅ **Multiple SMS Gateway Support** (Twilio, AWS SNS, Nexmo, MSG91, custom)
✅ **Rich Template System** with 30+ variables and placeholders
✅ **Powerful Rules Engine** with conditions and targeting
✅ **Notification Queue & Retry** mechanism for reliability
✅ **Comprehensive Logging** with delivery tracking and analytics
✅ **Admin Dashboard** with statistics and monitoring
✅ **Test Capabilities** for SMTP, SMS, and templates
✅ **Security** - Encrypted storage of credentials
✅ **Rate Limiting** to prevent spam and manage costs
✅ **Fallback Mechanisms** for reliability

---

## 2. COMPLAINT REGISTRATION IMPROVEMENTS

### 2.1 Current Issue
During complaint registration, the system is not capturing organizational hierarchy information (Company, Branch, Department, Section) and employee contact details are not readily available to ticket handlers.

### 2.2 Required Changes

#### A. Enhanced Complaint Registration Form

**New Fields to Add:**
```
Employee Information Section:
├── Company (Dropdown - Auto-populated if user logged in)
├── Branch (Dropdown - Auto-populated if available)
├── Department (Dropdown - Auto-populated if available)
├── Section (Dropdown - Auto-populated if available)
├── Employee Code (Auto-populated if logged in)
├── Employee Name (Auto-populated if logged in)
├── Email (Auto-populated if logged in, readonly)
├── Phone (Auto-populated if logged in, editable)
└── Alternate Phone (Optional)

Complaint Details Section:
├── Complaint Title
├── Complaint Description
├── Category
├── Priority
├── Attachments
└── Preferred Contact Method (Email/Phone/Both)
```

#### B. Auto-Population Logic

**When User is Logged In:**
1. Fetch user profile data from Users table
2. Auto-populate:
   - Company (from user.CompanyId)
   - Branch (from user.BranchId)
   - Department (from user.DepartmentId)
   - Section (from user.SectionId)
   - Employee Code (from user.EmployeeCode)
   - Employee Name (from user.FirstName + LastName)
   - Email (from user.Email)
   - Phone (from user.Phone)
3. Make fields readonly if data exists
4. Allow override if needed (with confirmation)

**When User is NOT Logged In (Anonymous/Guest):**
1. Show all fields as editable
2. Make Company/Branch/Department/Section required
3. Validate email and phone format
4. Optionally: Allow guest to search by Employee Code to auto-populate

#### C. Information Visibility to Handlers

**Handler View Enhancement:**
When handler opens complaint, they should see:

```
Complainant Information Card:
├── Personal Details
│   ├── Name
│   ├── Employee Code
│   ├── Email (clickable to send email)
│   ├── Phone (clickable to call)
│   ├── Alternate Phone
│   └── Preferred Contact Method
│
├── Organizational Hierarchy
│   ├── Company
│   ├── Branch
│   ├── Department
│   ├── Section
│   └── Direct Manager (name, phone, email)
│
└── Additional Context
    ├── Job Title
    ├── Date of Joining
    ├── Previous Complaints Count
    └── Average Response Time
```

#### D. Configurable Information Display

**Admin Settings Page: "Complaint Information Visibility"**

```
Configuration Options:
├── Information Visible to Handlers
│   ├── □ Employee Name (Always visible)
│   ├── □ Employee Code
│   ├── □ Email Address
│   ├── □ Phone Number
│   ├── □ Alternate Phone
│   ├── □ Company Name
│   ├── □ Branch Name
│   ├── □ Department Name
│   ├── □ Section Name
│   ├── □ Job Title
│   ├── □ Manager Details
│   ├── □ Date of Joining
│   └── □ Previous Complaints History
│
├── Information Visible to Management
│   ├── All handler information +
│   ├── □ Employee Address
│   ├── □ Emergency Contact
│   ├── □ Performance Metrics
│   └── □ Attendance Records
│
├── Information Visible in Reports
│   ├── Select fields to include in exports
│   └── Data masking options (e.g., mask phone, email)
│
└── Privacy Settings
    ├── □ Mask Personal Info in Logs
    ├── □ Redact Info After Closure
    └── Retention Period (days)
```

### 2.3 Database Changes Required

#### Update Complaints Table
```sql
ALTER TABLE Complaints ADD:
- CompanyId (FK) - if not already present
- BranchId (FK) - if not already present
- DepartmentId (FK) - if not already present
- SectionId (FK) - if not already present
- EmployeeCode (string)
- ContactEmail (string)
- ContactPhone (string)
- AlternatePhone (string, nullable)
- PreferredContactMethod (enum: Email/Phone/Both)
```

#### Create New Table: ComplaintInformationSettings
```sql
CREATE TABLE ComplaintInformationSettings (
    Id GUID PRIMARY KEY,
    CompanyId GUID FK,

    -- Visibility for Handlers
    ShowEmployeeCodeToHandlers BIT,
    ShowEmailToHandlers BIT,
    ShowPhoneToHandlers BIT,
    ShowAlternatePhoneToHandlers BIT,
    ShowCompanyToHandlers BIT,
    ShowBranchToHandlers BIT,
    ShowDepartmentToHandlers BIT,
    ShowSectionToHandlers BIT,
    ShowJobTitleToHandlers BIT,
    ShowManagerDetailsToHandlers BIT,
    ShowDateOfJoiningToHandlers BIT,
    ShowPreviousComplaintsToHandlers BIT,

    -- Visibility for Management
    ShowEmployeeAddressToManagement BIT,
    ShowEmergencyContactToManagement BIT,
    ShowPerformanceMetricsToManagement BIT,

    -- Privacy Settings
    MaskPersonalInfoInLogs BIT,
    RedactInfoAfterClosure BIT,
    DataRetentionDays INT,

    CreatedAt DATETIME,
    UpdatedAt DATETIME,
    CreatedBy GUID,
    UpdatedBy GUID
)
```

### 2.4 Backend Implementation

#### Files to Create/Update:

```
Domain:
- Domain/Enums/PreferredContactMethod.cs (NEW)
- Domain/Entities/ComplaintInformationSettings.cs (NEW)

Application:
- Application/DTOs/Complaint/CreateComplaintRequest.cs (UPDATE - add new fields)
- Application/DTOs/Complaint/ComplaintDto.cs (UPDATE - add complainant info)
- Application/DTOs/Settings/ComplaintInfoSettingsDto.cs (NEW)

Infrastructure:
- Infrastructure/Data/Migrations/AddComplaintOrgHierarchy.cs (NEW)
- Infrastructure/Data/Migrations/AddComplaintInfoSettings.cs (NEW)

API:
- API/Controllers/ComplaintInfoSettingsController.cs (NEW)
```

#### Key Methods to Implement:

```csharp
// Auto-populate user data when creating complaint
public async Task<ComplaintDto> CreateComplaintAsync(CreateComplaintRequest request, Guid userId)
{
    // If userId provided, fetch user details
    var user = await _userRepository.GetByIdAsync(userId);

    // Auto-populate if not provided in request
    request.CompanyId ??= user.CompanyId;
    request.BranchId ??= user.BranchId;
    request.DepartmentId ??= user.DepartmentId;
    request.SectionId ??= user.SectionId;
    request.ContactEmail ??= user.Email;
    request.ContactPhone ??= user.Phone;

    // Create complaint with all details...
}

// Get complaint with visibility filtering
public async Task<ComplaintDto> GetComplaintByIdAsync(Guid complaintId, Guid requestingUserId)
{
    var complaint = await _repository.GetByIdAsync(complaintId);
    var settings = await _settingsRepository.GetByCompanyIdAsync(complaint.CompanyId);

    // Apply visibility rules based on requesting user's role
    var dto = MapToDto(complaint, settings, requestingUserId);

    return dto;
}
```

### 2.5 Frontend Implementation

#### Files to Create/Update:

```
Frontend:
- src/app/components/complaint/create-complaint/
  - create-complaint.component.ts (UPDATE - add new fields)
  - create-complaint.component.html (UPDATE - add org hierarchy section)

- src/app/components/complaint/complaint-detail/
  - complaint-detail.component.ts (UPDATE - show complainant info)
  - complaint-detail.component.html (UPDATE - add info card)

- src/app/components/admin/complaint-info-settings/
  - complaint-info-settings.component.ts (NEW)
  - complaint-info-settings.component.html (NEW)
  - complaint-info-settings.component.scss (NEW)

- src/app/models/complaint.model.ts (UPDATE)
```

#### Form Changes:

```typescript
// In create-complaint.component.ts
ngOnInit() {
    // Fetch current user data
    this.authService.getCurrentUser().subscribe(user => {
        if (user) {
            // Auto-populate form
            this.complaintForm.patchValue({
                companyId: user.companyId,
                branchId: user.branchId,
                departmentId: user.departmentId,
                sectionId: user.sectionId,
                employeeCode: user.employeeCode,
                contactEmail: user.email,
                contactPhone: user.phone
            });

            // Make certain fields readonly
            this.complaintForm.get('contactEmail')?.disable();
        }
    });
}
```

### 2.6 UI Mockup - Complainant Information Card

```html
<!-- In complaint-detail.component.html -->
<div class="complainant-info-card" *ngIf="complaint">
    <!-- Personal Details Section -->
    <div class="info-section">
        <h3>Complainant Information</h3>
        <div class="info-row">
            <label>Name:</label>
            <span>{{ complaint.createdByName }}</span>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showEmployeeCode">
            <label>Employee Code:</label>
            <span>{{ complaint.employeeCode }}</span>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showEmail">
            <label>Email:</label>
            <a href="mailto:{{ complaint.contactEmail }}">{{ complaint.contactEmail }}</a>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showPhone">
            <label>Phone:</label>
            <a href="tel:{{ complaint.contactPhone }}">{{ complaint.contactPhone }}</a>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showAlternatePhone && complaint.alternatePhone">
            <label>Alternate Phone:</label>
            <a href="tel:{{ complaint.alternatePhone }}">{{ complaint.alternatePhone }}</a>
        </div>
        <div class="info-row">
            <label>Preferred Contact:</label>
            <span class="badge">{{ complaint.preferredContactMethod }}</span>
        </div>
    </div>

    <!-- Organizational Hierarchy Section -->
    <div class="info-section" *ngIf="visibilitySettings.showOrgHierarchy">
        <h3>Organization</h3>
        <div class="info-row" *ngIf="visibilitySettings.showCompany">
            <label>Company:</label>
            <span>{{ complaint.companyName }}</span>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showBranch">
            <label>Branch:</label>
            <span>{{ complaint.branchName }}</span>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showDepartment">
            <label>Department:</label>
            <span>{{ complaint.departmentName }}</span>
        </div>
        <div class="info-row" *ngIf="visibilitySettings.showSection">
            <label>Section:</label>
            <span>{{ complaint.sectionName }}</span>
        </div>
    </div>

    <!-- Manager Details Section -->
    <div class="info-section" *ngIf="visibilitySettings.showManagerDetails && complaint.managerDetails">
        <h3>Reporting Manager</h3>
        <div class="info-row">
            <label>Name:</label>
            <span>{{ complaint.managerDetails.name }}</span>
        </div>
        <div class="info-row">
            <label>Email:</label>
            <a href="mailto:{{ complaint.managerDetails.email }}">{{ complaint.managerDetails.email }}</a>
        </div>
        <div class="info-row">
            <label>Phone:</label>
            <a href="tel:{{ complaint.managerDetails.phone }}">{{ complaint.managerDetails.phone }}</a>
        </div>
    </div>
</div>
```

### 2.7 Implementation Timeline

#### Week 1: Backend Changes
- [ ] Create database migrations for new fields
- [ ] Update Complaint entity
- [ ] Create ComplaintInformationSettings entity
- [ ] Update CreateComplaintRequest DTO
- [ ] Implement auto-population logic
- [ ] Implement visibility filtering logic
- [ ] Create settings management API

#### Week 2: Frontend Changes
- [ ] Update complaint registration form
- [ ] Add org hierarchy dropdowns
- [ ] Implement auto-population
- [ ] Update complaint detail view
- [ ] Add complainant information card
- [ ] Create admin settings page for visibility config
- [ ] Add form validations

#### Week 3: Testing
- [ ] Test auto-population with logged-in users
- [ ] Test manual entry for anonymous users
- [ ] Test visibility settings
- [ ] Test with different user roles
- [ ] Test data masking in reports
- [ ] Security testing for privacy

---

## 3. IMPLEMENTATION PRIORITY

### High Priority (Start Immediately)
1. **Complaint Registration Improvements** (2-3 weeks)
   - Critical for user experience
   - Improves data quality
   - Enhances handler productivity

### Medium Priority (Start After Complaint Registration)
2. **Notification System** (5 weeks)
   - Enhances communication
   - Improves escalation effectiveness
   - Professional system appearance

### Future Enhancements
3. In-app real-time notifications
4. Multi-language support
5. Advanced analytics dashboard
6. Mobile app integration

---

## 4. CURRENT SYSTEM STATUS

### Completed Features
✅ Complaint CRUD operations
✅ Escalation matrix system
✅ Escalation policy configuration
✅ Role-based access control
✅ User management
✅ Category management
✅ Comment system
✅ Audit logging

### Recently Fixed
✅ Escalation trigger value saving issue (TriggerAfterValue, TriggerTimeUnit mapping)

### In Progress
🔄 Planning notification system
🔄 Planning complaint registration improvements

### Pending
⏳ Notification system implementation
⏳ Complaint registration enhancements
⏳ SMTP/SMS gateway configuration
⏳ Information visibility settings

---

## 5. NOTES & REMINDERS

### Important Points
- **Security:** All credentials (SMTP passwords, SMS tokens) must be encrypted in database
- **Rate Limiting:** Implement rate limiting to prevent spam and manage costs
- **Testing:** Always provide test email/SMS functionality before going live
- **Scalability:** Use background workers for notification processing to avoid blocking
- **Logging:** Comprehensive logging for debugging and compliance
- **Privacy:** Respect user privacy with configurable visibility settings
- **Performance:** Batch notifications when possible to improve performance
- **Reliability:** Implement retry mechanism for failed notifications

### Next Session Actions
1. Review this plan
2. Confirm scope and priorities
3. Start with either:
   - Complaint Registration Improvements (recommended - quick wins)
   - OR Notification System (comprehensive but longer timeline)
4. Begin Phase 1 implementation

---

**Document Version:** 1.0
**Last Updated:** October 17, 2025
**Next Review:** Before starting implementation
