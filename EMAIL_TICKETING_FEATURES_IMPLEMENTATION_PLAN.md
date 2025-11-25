# Advanced Email Ticketing Features - Implementation Plan

**Project**: Complaint Management System
**Feature**: Advanced Email Ticketing Features
**Timeline**: 3-4 weeks
**Started**: November 14, 2025
**Status**: Planning Phase

---

## Executive Summary

This document outlines the implementation plan for advanced email ticketing features that will automate and enhance email-based complaint management.

---

## Feature Breakdown

### Phase 1: Email Thread Integration (Week 1)
**Goal**: Display full email conversation and enable replies from system

**Features:**
1. Display full email thread in complaint detail page
2. Reply to complainants directly from system
3. Forward emails to other departments
4. CC/BCC support
5. Email signature templates
6. HTML email support

**Database Changes:**
- Already have: `EmailMessages` table
- Already have: `EmailConfiguration` table
- Need to add: Email thread viewer component

**API Endpoints Needed:**
- GET /api/email-threads/{complaintId}
- POST /api/email-threads/{complaintId}/reply
- POST /api/email-threads/{complaintId}/forward

---

### Phase 2: Auto-Response System (Week 2)
**Goal**: Automated email notifications for complaint lifecycle events

**Features:**
1. Acknowledgment emails (ticket received)
2. Status update notifications
3. Assignment notifications
4. Resolution confirmations
5. Custom email templates
6. Template variables ({{complainantName}}, {{complaintNumber}}, etc.)

**Database Changes:**
- Already have: `CommunicationTemplates` table
- Already have: `NotificationRules` table
- Enhance: Template system with email-specific features

**API Endpoints Needed:**
- POST /api/auto-responses/send
- GET /api/templates/email
- POST /api/templates/email/preview

---

### Phase 3: Email Parsing Intelligence (Week 3)
**Goal**: Extract information from emails automatically

**Features:**
1. Auto-categorize from email content
2. Extract attachments automatically
3. Detect priority from keywords
4. Identify duplicate emails
5. Parse customer information
6. Extract issue description

**Implementation:**
- NLP library (if needed) or keyword matching
- Attachment extraction (already exists in EmailTicketingService)
- Priority detection rules
- Duplicate detection algorithm

**Database Changes:**
- Add: `EmailParsingRules` table
- Add: `EmailKeywords` table

---

### Phase 4: Email Rules & Automation (Week 4)
**Goal**: Route and process emails automatically

**Features:**
1. Route to specific handlers based on keywords
2. Auto-assign based on email domain
3. Auto-prioritize based on SLA
4. Spam filtering
5. Auto-close resolved threads

**Database Changes:**
- Add: `EmailRoutingRules` table
- Add: `EmailAutomationRules` table

**API Endpoints Needed:**
- GET /api/email-rules
- POST /api/email-rules
- PUT /api/email-rules/{id}
- DELETE /api/email-rules/{id}

---

## Database Schema Design

### New Tables Required

#### 1. EmailParsingRules
```sql
CREATE TABLE EmailParsingRules (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    RuleName NVARCHAR(200) NOT NULL,
    RuleType NVARCHAR(50) NOT NULL, -- 'Category', 'Priority', 'Assignment'
    TriggerKeywords NVARCHAR(MAX), -- JSON array
    TargetValue NVARCHAR(500), -- Category ID, Priority level, User ID
    IsEnabled BIT NOT NULL DEFAULT 1,
    Priority INT NOT NULL DEFAULT 0, -- Rule execution order
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);
```

#### 2. EmailRoutingRules
```sql
CREATE TABLE EmailRoutingRules (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    RuleName NVARCHAR(200) NOT NULL,
    Conditions NVARCHAR(MAX), -- JSON conditions
    TargetUserId UNIQUEIDENTIFIER, -- Auto-assign to user
    TargetCategoryId UNIQUEIDENTIFIER, -- Auto-categorize
    TargetPriorityId UNIQUEIDENTIFIER, -- Auto-prioritize
    IsEnabled BIT NOT NULL DEFAULT 1,
    ExecutionOrder INT NOT NULL DEFAULT 0,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    IsDeleted BIT NOT NULL DEFAULT 0,
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (TargetUserId) REFERENCES Users(Id),
    FOREIGN KEY (TargetCategoryId) REFERENCES Categories(Id)
);
```

#### 3. EmailThreads (Enhancement)
Already exists via EmailMessages table, but add relationship tracking:

```sql
-- Add column to EmailMessages table
ALTER TABLE EmailMessages ADD ThreadId UNIQUEIDENTIFIER;
ALTER TABLE EmailMessages ADD InReplyToMessageId NVARCHAR(500);
ALTER TABLE EmailMessages ADD References NVARCHAR(MAX); -- Email References header
```

#### 4. EmailResponseHistory
```sql
CREATE TABLE EmailResponseHistory (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    ComplaintId UNIQUEIDENTIFIER NOT NULL,
    EmailMessageId UNIQUEIDENTIFIER,
    SentBy UNIQUEIDENTIFIER NOT NULL, -- User who sent
    SentTo NVARCHAR(500) NOT NULL, -- Email addresses
    CarbonCopy NVARCHAR(500), -- CC
    BlindCarbonCopy NVARCHAR(500), -- BCC
    Subject NVARCHAR(500),
    Body NVARCHAR(MAX),
    IsHtml BIT NOT NULL DEFAULT 1,
    SentAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    DeliveryStatus NVARCHAR(50), -- 'Sent', 'Delivered', 'Failed', 'Bounced'
    ErrorMessage NVARCHAR(MAX),
    FOREIGN KEY (ComplaintId) REFERENCES Complaints(Id),
    FOREIGN KEY (EmailMessageId) REFERENCES EmailMessages(Id),
    FOREIGN KEY (SentBy) REFERENCES Users(Id)
);
```

---

## API Design

### Email Thread Integration

#### GET /api/email-threads/{complaintId}
**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "complaintId": "guid",
    "complaintNumber": "CMP-20251114-0001",
    "messages": [
      {
        "id": "guid",
        "from": "customer@example.com",
        "to": "support@company.com",
        "subject": "Issue with product",
        "body": "I'm having an issue...",
        "isHtml": true,
        "receivedAt": "2025-11-14T10:00:00Z",
        "direction": "Inbound",
        "attachments": [
          {
            "fileName": "screenshot.png",
            "fileSize": 102400,
            "contentType": "image/png"
          }
        ]
      }
    ]
  }
}
```

#### POST /api/email-threads/{complaintId}/reply
**Request:**
```json
{
  "to": ["customer@example.com"],
  "cc": [],
  "bcc": [],
  "subject": "Re: Issue with product",
  "body": "<p>Thank you for contacting us...</p>",
  "isHtml": true,
  "attachments": []
}
```

#### POST /api/email-threads/{complaintId}/forward
**Request:**
```json
{
  "to": ["technical@company.com"],
  "subject": "Fwd: Issue with product",
  "body": "<p>Forwarding for technical review...</p>",
  "includeOriginalThread": true
}
```

### Auto-Response System

#### POST /api/auto-responses/send
**Request:**
```json
{
  "complaintId": "guid",
  "templateType": "Acknowledgment", // or "StatusUpdate", "Assignment", "Resolution"
  "customData": {
    "estimatedResolutionTime": "24 hours"
  }
}
```

#### POST /api/templates/email/preview
**Request:**
```json
{
  "templateId": "guid",
  "complaintId": "guid",
  "previewData": {
    "complainantName": "John Doe",
    "complaintNumber": "CMP-20251114-0001"
  }
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "subject": "Your complaint CMP-20251114-0001 has been received",
    "htmlBody": "<html>...</html>",
    "textBody": "Dear John Doe..."
  }
}
```

### Email Parsing Intelligence

#### POST /api/email-parsing/analyze
**Request:**
```json
{
  "emailSubject": "URGENT: System down",
  "emailBody": "Our production system is completely down...",
  "senderEmail": "customer@enterprise.com"
}
```

**Response:**
```json
{
  "isSuccess": true,
  "data": {
    "suggestedCategory": "Technical Issue",
    "suggestedPriority": "Critical",
    "suggestedAssignee": "technical-team@company.com",
    "extractedKeywords": ["urgent", "system down", "production"],
    "sentiment": "Negative",
    "confidence": 0.85
  }
}
```

### Email Rules

#### GET /api/email-rules
**Response:**
```json
{
  "isSuccess": true,
  "data": [
    {
      "id": "guid",
      "ruleName": "Route Enterprise Customers to Senior Team",
      "ruleType": "Routing",
      "conditions": {
        "senderDomain": ["enterprise.com", "bigclient.com"],
        "keywordsAny": ["urgent", "critical"]
      },
      "actions": {
        "assignTo": "senior-team@company.com",
        "setPriority": "High",
        "sendNotification": true
      },
      "isEnabled": true,
      "executionOrder": 1
    }
  ]
}
```

---

## Frontend Components

### 1. Email Thread Viewer Component
**Location:** `src/app/components/shared/email-thread-viewer/`

**Features:**
- Display email conversation thread
- Show sender, receiver, timestamps
- HTML email rendering
- Attachment preview/download
- Expand/collapse messages
- Reply/Forward buttons

### 2. Email Composer Component
**Location:** `src/app/components/shared/email-composer/`

**Features:**
- Rich text editor (CKEditor or TinyMCE)
- To/CC/BCC fields
- File attachment
- Template selection
- Variable insertion
- Preview mode

### 3. Email Template Manager
**Location:** `src/app/components/admin/email-template-manager/`

**Features:**
- CRUD for email templates
- Template variables list
- Preview functionality
- HTML editor
- Category organization

### 4. Email Rules Manager
**Location:** `src/app/components/admin/email-rules-manager/`

**Features:**
- Create/edit routing rules
- Condition builder (visual)
- Action configurator
- Test rule functionality
- Enable/disable rules
- Execution order management

---

## Implementation Order

### Week 1: Email Thread Integration

**Days 1-2: Backend**
- [x] Review existing EmailMessages table
- [ ] Add ThreadId and InReplyToMessageId columns
- [ ] Create EmailResponseHistory table
- [ ] Implement GET /api/email-threads/{complaintId} endpoint
- [ ] Implement POST /api/email-threads/{complaintId}/reply endpoint
- [ ] Add email sending service (use existing SMTP from EmailConfiguration)

**Days 3-5: Frontend**
- [ ] Create EmailThreadViewerComponent
- [ ] Create EmailComposerComponent
- [ ] Integrate into complaint-detail page
- [ ] Add reply/forward functionality
- [ ] Style with existing theme
- [ ] Test end-to-end

### Week 2: Auto-Response System

**Days 6-7: Backend**
- [ ] Enhance CommunicationTemplates for email
- [ ] Create template variable replacement service
- [ ] Implement POST /api/auto-responses/send endpoint
- [ ] Create background service for automatic sending
- [ ] Add email queue for reliability

**Days 8-10: Frontend**
- [ ] Enhance template management UI
- [ ] Add template preview
- [ ] Add auto-response configuration
- [ ] Test with different templates
- [ ] Create default templates

### Week 3: Email Parsing Intelligence

**Days 11-12: Backend**
- [ ] Create EmailParsingRules table
- [ ] Implement keyword-based categorization
- [ ] Implement priority detection
- [ ] Create duplicate detection algorithm
- [ ] Add parsing to email polling service

**Days 13-15: Frontend**
- [ ] Create parsing rules management UI
- [ ] Add keyword configuration
- [ ] Test parsing accuracy
- [ ] Add manual override option
- [ ] Show parsing suggestions in UI

### Week 4: Email Rules & Automation

**Days 16-17: Backend**
- [ ] Create EmailRoutingRules table
- [ ] Implement rule engine
- [ ] Add condition evaluation service
- [ ] Integrate with complaint creation
- [ ] Add spam filtering

**Days 18-20: Frontend**
- [ ] Create email rules manager UI
- [ ] Build visual condition builder
- [ ] Add action configurator
- [ ] Test rule execution
- [ ] Create default rules

**Days 21: Testing & Documentation**
- [ ] End-to-end testing
- [ ] Performance testing
- [ ] Documentation
- [ ] User training materials

---

## Technical Stack

### Backend
- **Language**: C# .NET 8
- **Email**: MailKit (already in use)
- **Background Jobs**: Hosted Services
- **Parsing**: Regex + custom logic
- **Storage**: SQL Server

### Frontend
- **Framework**: Angular 18
- **Rich Editor**: CKEditor 5 or TinyMCE
- **Email Rendering**: DOMPurify for sanitization
- **UI Components**: Existing component library

---

## Dependencies & Libraries

### Backend NuGet Packages (Already Installed)
- MailKit (for SMTP)
- MimeKit (for email parsing)
- Entity Framework Core

### Frontend NPM Packages (To Install)
```json
{
  "@ckeditor/ckeditor5-angular": "^7.0.0",
  "@ckeditor/ckeditor5-build-classic": "^40.0.0",
  "dompurify": "^3.0.0",
  "@types/dompurify": "^3.0.0"
}
```

---

## Security Considerations

### Email Sending
- Rate limiting (prevent spam)
- SPF/DKIM configuration
- Email validation
- Attachment scanning
- Size limits

### Email Parsing
- XSS prevention (sanitize HTML)
- SQL injection prevention
- Email header validation
- Attachment type restrictions

### Auto-Responses
- Prevent response loops
- Throttle notifications
- Validate email addresses
- Audit trail for all emails sent

---

## Performance Considerations

### Email Thread Loading
- Paginate messages (load 20 at a time)
- Lazy load attachments
- Cache frequently accessed threads

### Email Sending
- Queue-based sending (prevent blocking)
- Retry logic for failures
- Batch processing for bulk emails

### Parsing
- Cache parsing results
- Async processing
- Background jobs for heavy operations

---

## Testing Strategy

### Unit Tests
- Template variable replacement
- Parsing logic
- Rule evaluation
- Email validation

### Integration Tests
- Email sending/receiving
- Thread retrieval
- Auto-response triggering
- Rule execution

### E2E Tests
- Send reply from UI
- Forward email
- Auto-response sent on complaint creation
- Parsing accuracy
- Rule-based routing

---

## Success Metrics

### Adoption
- % of complaints with email threads
- Number of replies sent per day
- Auto-response delivery rate

### Efficiency
- Time saved in email management
- Reduction in manual categorization
- Auto-routing accuracy

### Quality
- Email delivery success rate
- User satisfaction with templates
- Parsing accuracy

---

## Risks & Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Email delivery failures | High | Queue + retry logic |
| Parsing inaccuracy | Medium | Manual override option |
| Spam responses | High | Rate limiting + validation |
| Performance issues | Medium | Async processing + caching |
| Security vulnerabilities | High | Input sanitization + validation |

---

## Next Steps

1. **Get Approval** - Review this plan
2. **Set Up Environment** - Install required packages
3. **Create Database Migrations** - Add new tables
4. **Start Week 1** - Email Thread Integration

---

**Document Status**: Ready for Review
**Estimated Completion**: December 12, 2025
**Resource Required**: 1 senior developer (full-time)
