# Complaint Management System - Comprehensive Architecture

## 🏗️ Overview
This document outlines the complete, modular, and extensible architecture for the Complaint Management System. The system is designed to be **user-configurable**, **event-driven**, and **highly scalable**.

---

## 📦 Core Modules

### 1. Master Data Management Module
**Purpose:** User-configurable master data instead of hardcoded values

#### Entities:
- **ComplaintStatusMaster** - Define custom complaint statuses
  - Properties: Name, Code, Description, ColorCode, IconClass, DisplayOrder, IsFinal, IsActive
  - Features: System statuses + company-specific statuses

- **ComplaintPriorityMaster** - Define custom priority levels
  - Properties: Name, Code, Description, Level, ColorCode, SLA times, IsActive
  - Features: System priorities + company-specific priorities

#### Features:
- ✅ CRUD operations for all master data
- ✅ System-level (global) + Company-level (custom) data
- ✅ Color codes and icons for UI display
- ✅ Display ordering
- ✅ Active/inactive toggle

---

### 2. Custom Fields System
**Purpose:** Add dynamic fields to complaints without code changes

#### Entities:
- **CustomFieldDefinition** - Defines what fields exist
  - Field types: Text, TextArea, Number, Date, DateTime, Email, Phone, URL, Dropdown, MultiSelect, Radio, Checkbox, Boolean, File, RichText, Color, Rating
  - Validation rules (JSON)
  - Options for dropdowns (JSON)
  - Company-specific or global fields

- **CustomFieldValue** - Stores actual field values
  - Multiple storage columns: Value (string), NumericValue, DateValue, BooleanValue, JsonValue
  - Linked to Complaint or other entities

#### Features:
- ✅ 16 different field types
- ✅ Required/optional fields
- ✅ Validation rules
- ✅ Searchable fields
- ✅ Default values
- ✅ Placeholder and help text
- ✅ Section grouping
- ✅ Visibility control (complainant vs handler)

---

### 3. Event System
**Purpose:** Trigger-based actions when things happen in the system

#### Entities:
- **EventType** - Defines system events
  - Examples: "ComplaintCreated", "StatusChanged", "AssignmentChanged", "EscalationTriggered"
  - Available fields for each event (JSON)
  - System events + custom events

- **EventLog** - Tracks when events occur
  - Event data/payload
  - Processing status
  - Actions triggered count

#### Pre-defined Events:
- Complaint Created
- Complaint Updated
- Status Changed
- Priority Changed
- Assignment Changed
- Comment Added
- Escalation Triggered
- Resolution Added
- Complaint Closed
- Complaint Reopened
- SLA Breached
- Due Date Approaching

---

### 4. Communication Module
**Purpose:** Send emails, SMS, WhatsApp based on events

#### Entities:
- **CommunicationTemplate** - Message templates
  - Supports placeholders: `{{complaintNumber}}`, `{{complainantName}}`, etc.
  - Multi-channel: Email, SMS, WhatsApp, Push, InApp
  - Multi-language support
  - HTML and plain text versions

- **CommunicationLog** - Tracks all sent messages
  - Status: Pending, Sending, Sent, Delivered, Read, Failed, Bounced
  - Retry tracking
  - Delivery confirmation
  - External message IDs

#### Supported Channels:
- ✅ Email
- ✅ SMS
- ✅ WhatsApp
- ✅ Push Notifications
- ✅ In-App Notifications
- ✅ Slack (future)
- ✅ Microsoft Teams (future)

---

### 5. Configuration Module
**Purpose:** Configure communication providers and servers

#### Entities:
- **EmailServerSettings** - SMTP configuration
  - Host, Port, SSL, Authentication
  - From email/name
  - Rate limiting
  - Test connection

- **SmsGatewaySettings** - SMS provider configuration
  - Providers: Twilio, Vonage, AWS SNS, etc.
  - API credentials
  - From number/sender name
  - Cost tracking

- **WhatsAppSettings** - WhatsApp Business API
  - Providers: Twilio, MessageBird, Official API
  - Business Account ID
  - Phone Number ID
  - Webhook configuration

#### Features:
- ✅ Multiple configurations per company
- ✅ Default configuration selection
- ✅ Test connections before saving
- ✅ Rate limiting
- ✅ Encrypted credentials
- ✅ Last tested timestamp

---

### 6. Event-Communication Mapping (Rules Engine)
**Purpose:** Define what happens when events occur

#### Entity:
- **EventCommunicationRule** - Maps events to actions
  - Event Type → Communication Channel → Template → Recipients
  - Conditions (JSON): Only trigger if certain conditions are met
  - Delay: Send after X minutes
  - Send only once option
  - Priority for execution order

#### Recipient Types:
- Complainant
- Assigned Handler
- Creator
- Specific Users
- Specific Roles
- Specific Emails
- Complainant's Manager
- Handler's Manager
- Department Users
- Section Users
- All Administrators

#### Example Rules:
```json
{
  "name": "Notify complainant when resolved",
  "event": "StatusChanged",
  "conditions": {"newStatus": "Resolved"},
  "channel": "Email",
  "template": "complaint-resolved-template",
  "recipients": "Complainant"
}
```

```json
{
  "name": "Alert admin on critical complaints",
  "event": "ComplaintCreated",
  "conditions": {"priority": "Critical"},
  "channel": "SMS",
  "recipients": "Administrators",
  "delay": 0
}
```

---

### 7. Dashboard & Reporting Module (NEW)
**Purpose:** User-defined dashboards with custom widgets and SLA reporting

#### Entities (To be created):
- **DashboardTemplate** - Predefined dashboard templates
- **UserDashboard** - Custom dashboards created by users
- **DashboardWidget** - Widget definitions (charts, tables, metrics)
- **WidgetConfiguration** - User's widget settings
- **Report** - Saved report definitions
- **ReportSchedule** - Scheduled report generation
- **SLAReport** - SLA compliance reports

#### Widget Types:
- Complaint count by status (pie chart)
- Complaints over time (line chart)
- Priority distribution (bar chart)
- Average resolution time (metric)
- SLA compliance rate (gauge)
- Top categories (bar chart)
- Assigned complaints (table)
- Recent complaints (list)
- **Custom widgets** - Users define SQL/filters

#### Features:
- ✅ Drag-and-drop dashboard builder
- ✅ Widget library (fixed + custom)
- ✅ Real-time data refresh
- ✅ Date range filters
- ✅ Export to PDF, Excel, CSV
- ✅ Scheduled email reports
- ✅ SLA tracking and reporting
- ✅ Multi-user access control

---

## 🔄 Data Flow

### Example: Complaint Status Change
1. **User changes complaint status** → API receives request
2. **Update complaint** in database
3. **Trigger event** → Create EventLog entry
4. **Event processor** checks EventCommunicationRule table
5. **Find matching rules** based on event type and conditions
6. **For each matching rule:**
   - Determine recipients based on RecipientType
   - Load communication template
   - Replace placeholders with actual data
   - Queue message to CommunicationLog
7. **Background worker** sends queued messages
8. **Update CommunicationLog** with delivery status

---

## 🎯 Key Architectural Principles

### 1. **User-Configurable**
- No hardcoded values
- Everything can be customized per company
- Admin UI for all configurations

### 2. **Extensible**
- Add custom fields without code changes
- Create custom events
- Define custom widgets
- Extend with new communication channels

### 3. **Modular**
- Each module is independent
- Clean separation of concerns
- Easy to add new modules

### 4. **Event-Driven**
- Everything triggers events
- Events drive communications
- Audit trail of all actions

### 5. **Multi-Tenant**
- Company-level isolation
- System-level defaults
- Company-specific customization

### 6. **Scalable**
- Background processing for heavy operations
- Queue-based communication
- Efficient database indexes
- Caching where appropriate

---

## 📊 Database Schema Summary

### Master Data Tables:
- ComplaintStatusMasters
- ComplaintPriorityMasters

### Custom Fields Tables:
- CustomFieldDefinitions
- CustomFieldValues

### Event Tables:
- EventTypes
- EventLogs
- EventCommunicationRules

### Communication Tables:
- CommunicationTemplates
- CommunicationLogs

### Configuration Tables:
- EmailServerSettings
- SmsGatewaySettings
- WhatsAppSettings

### Dashboard Tables (Future):
- DashboardTemplates
- UserDashboards
- DashboardWidgets
- WidgetConfigurations
- Reports
- ReportSchedules
- SLAReports

---

## 🚀 Implementation Roadmap

### Phase 1: Foundation ✓
- [x] Create all domain entities
- [x] Create enumerations
- [ ] Create database migration
- [ ] Seed default system data

### Phase 2: Backend API
- [ ] DTOs for all modules
- [ ] Command/Query handlers (CQRS)
- [ ] API controllers
- [ ] Background services (event processor, communication sender)
- [ ] Validation

### Phase 3: Admin UI
- [ ] Master Data Management screens
- [ ] Custom Fields configuration
- [ ] Event configuration
- [ ] Communication template editor
- [ ] Server configuration screens
- [ ] Event-Communication rule builder

### Phase 4: Integration
- [ ] Update complaint form to use custom fields
- [ ] Update complaint form to use dynamic status/priority
- [ ] Implement event triggering
- [ ] Implement communication sending
- [ ] Testing and validation

### Phase 5: Dashboard & Reporting
- [ ] Dashboard builder UI
- [ ] Widget library
- [ ] Report designer
- [ ] SLA tracking
- [ ] Export functionality

---

## 🔐 Security Considerations

- All sensitive data (passwords, tokens) encrypted at rest
- Role-based access control for all admin functions
- Audit logging for configuration changes
- Rate limiting on communication channels
- Input validation and sanitization
- SQL injection prevention
- XSS protection

---

## 📝 Next Steps

1. Complete database migration
2. Implement DTOs and handlers
3. Build API controllers
4. Create Angular admin components
5. Implement background services
6. Testing and deployment

---

**Last Updated:** 2025-10-20
**Architecture Version:** 2.0
