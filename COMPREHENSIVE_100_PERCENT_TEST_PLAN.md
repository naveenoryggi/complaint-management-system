# 🎯 COMPREHENSIVE 100% FEATURE TEST PLAN
**Date:** November 10, 2025
**Objective:** Test ALL features with complete email notification integration
**Target:** 100% working software with full feature demonstration

---

## 📋 EXECUTIVE SUMMARY

### Current System State
- ✅ **Users:** 10,613 users (including Naveen.chandra@oryggitech.com)
- ✅ **Email Templates:** 77 templates (15 system templates)
- ✅ **Event Types:** 11 event types configured
- ✅ **Priorities:** 6 priority levels
- ✅ **Statuses:** 11 status types
- ✅ **Categories:** 19 complaint categories
- ✅ **Roles:** 17 roles with complete hierarchy
- ❌ **SLA Policies:** 0 (needs creation)
- ❌ **Workflows:** 0 (needs creation)
- ❌ **Escalation Policies:** 0 (needs creation)
- ❌ **Email Server:** Not configured (needs setup)
- ❌ **User nav_nainital@yahoo.com:** Not found (needs creation)

---

## 🎯 TEST OBJECTIVES

### Phase 1: System Configuration (Setup)
1. Configure Gmail SMTP email server
2. Create test user (nav_nainital@yahoo.com)
3. Create SLA policies for all priority levels
4. Create workflows for complaint lifecycle
5. Create escalation policies with multi-level escalation
6. Configure notification rules for all events
7. Verify email templates are properly linked

### Phase 2: Complaint Creation & Assignment
8. Create complaints with different priorities
9. Create complaints with different categories
10. Test automatic assignment based on category
11. Test manual assignment to specific user
12. Verify email notifications for creation & assignment

### Phase 3: SLA Testing
13. Test priority-based SLA (Low, Normal, High, Critical, Urgent)
14. Test category-based SLA
15. Test SLA breach notifications
16. Test SLA pause/resume functionality
17. Test due date calculations

### Phase 4: Workflow Testing
18. Test workflow transitions (status changes)
19. Test workflow permissions and constraints
20. Test workflow-based notifications
21. Test workflow history tracking

### Phase 5: Escalation Testing
22. Test time-based auto-escalation
23. Test manual escalation
24. Test escalation levels (Level 1 → Level 2 → Level 3)
25. Test escalation notifications to multiple users
26. Test escalation matrix routing

### Phase 6: Email Notification Testing
27. Test all 11 event type notifications
28. Test multi-channel notifications (Email, SMS, WhatsApp placeholders)
29. Test notification rules with conditions
30. Test template placeholder replacement
31. Verify emails sent to correct recipients

### Phase 7: End-to-End User Journey
32. Login as complainant (nav_nainital@yahoo.com)
33. Create complaint and track emails
34. Login as handler (Naveen.chandra@oryggitech.com)
35. View assigned complaints
36. Update complaint status and verify emails
37. Add comments and verify notifications
38. Close complaint and verify closure emails

---

## 📊 TEST COMBINATIONS MATRIX

### A. Priority vs Category SLA Combinations (5×19 = 95 scenarios)

| Priority | Categories (Sample) | Expected SLA | Test Status |
|----------|---------------------|--------------|-------------|
| Low | Attendance Issues | 48h response, 120h resolution | Pending |
| Low | Product Quality | 48h response, 120h resolution | Pending |
| Normal | Salary & Payroll | 24h response, 72h resolution | Pending |
| Normal | Service Delays | 24h response, 72h resolution | Pending |
| High | Technical Issues | 8h response, 24h resolution | Pending |
| High | Delivery Problems | 8h response, 24h resolution | Pending |
| Critical | Billing Problems | 4h response, 12h resolution | Pending |
| Critical | HRMS System | 4h response, 12h resolution | Pending |
| Urgent | Leave Management | 2h response, 8h resolution | Pending |
| Urgent | Performance Mgmt | 2h response, 8h resolution | Pending |

### B. Workflow vs Status Combinations (5 workflows × 11 statuses)

| Workflow | Allowed Transitions | Test Status |
|----------|---------------------|-------------|
| Standard Workflow | Submitted → Under Review → In Progress → Resolved → Closed | Pending |
| Fast Track Workflow | Submitted → In Progress → Resolved → Closed | Pending |
| Escalation Workflow | Submitted → Under Review → Escalated → In Progress → Resolved | Pending |
| Review Workflow | Submitted → Under Review → Pending Info → In Progress → Resolved | Pending |
| Complex Workflow | All status transitions with approval gates | Pending |

### C. Escalation Policy Combinations (3 levels × 5 priorities)

| Priority | Level 1 Handler | Level 2 Handler | Level 3 Handler | Escalation Time |
|----------|----------------|----------------|----------------|-----------------|
| Low | Naveen.chandra | support@ | marketing@ | 48h → 96h → 144h |
| Normal | Naveen.chandra | support@ | marketing@ | 24h → 48h → 72h |
| High | Naveen.chandra | himanshu.singh@ | support@ | 8h → 16h → 24h |
| Critical | Naveen.chandra | himanshu.singh@ | marketing@ | 4h → 8h → 12h |
| Urgent | Naveen.chandra | support@ | himanshu.singh@ | 2h → 4h → 6h |

### D. Notification Event Combinations (11 events × 3 channels)

| Event Type | Email Template | SMS Template | WhatsApp Template | Test Status |
|------------|---------------|--------------|-------------------|-------------|
| COMPLAINT_CREATED | ✅ Exists | ✅ Exists | ✅ Exists | Pending |
| COMPLAINT_ASSIGNED | ✅ Exists | ✅ Exists | ✅ Exists | Pending |
| COMPLAINT_CLOSED | ✅ Exists | ✅ Exists | ✅ Exists | Pending |
| COMPLAINT_ESCALATED | ✅ Exists | ✅ Exists | ✅ Exists | Pending |
| COMPLAINT_OVERDUE | ✅ Exists | ✅ Exists | ✅ Exists | Pending |
| COMPLAINT_DUE_SOON | Not configured | Not configured | Not configured | Needs Template |
| COMPLAINT_COMMENTED | Not configured | Not configured | Not configured | Needs Template |
| COMPLAINT_REOPENED | Not configured | Not configured | Not configured | Needs Template |
| COMPLAINT_STATUS_CHANGED | Not configured | Not configured | Not configured | Needs Template |

---

## 🔧 DETAILED IMPLEMENTATION PLAN

### PHASE 1: EMAIL SERVER CONFIGURATION (Priority: CRITICAL)

**Step 1.1: Configure Gmail SMTP Server**
```json
{
  "serverName": "Gmail SMTP Server",
  "smtpServer": "smtp.gmail.com",
  "port": 587,
  "username": "oryggiserver@gmail.com",
  "password": "veaa mwlw hbbq nbzz",
  "enableSsl": true,
  "fromEmail": "oryggiserver@gmail.com",
  "fromName": "Complaint Management System",
  "isActive": true
}
```

**Validation:**
- ✅ Test connection to Gmail SMTP
- ✅ Send test email to verify credentials
- ✅ Confirm SSL/TLS configuration

---

### PHASE 2: USER CREATION

**Step 2.1: Create Complainant User (nav_nainital@yahoo.com)**
```json
{
  "email": "nav_nainital@yahoo.com",
  "fullName": "Nav Nainital",
  "employeeCode": "NAV001",
  "password": "Nav@12345",
  "roleIds": ["<complainant-role-id>"],
  "isActive": true
}
```

**Step 2.2: Verify Handler User (Naveen.chandra@oryggitech.com)**
- ✅ Already exists (Employee Code: 218819771403)
- ✅ Verify role: Level 1 Handler or higher
- ✅ Verify email is valid

**Step 2.3: Create/Verify Escalation Users**
- marketing@oryggitech.com (Level 2/3 Handler)
- support@oryggitech.com (Level 2/3 Handler)
- himanshu.singh@oryggitech.com (Level 2/3 Handler)

---

### PHASE 3: SLA POLICY CONFIGURATION

**Step 3.1: Create Priority-Based SLA Policies**

**Policy 1: Low Priority SLA**
```json
{
  "name": "Low Priority SLA",
  "description": "SLA for low priority complaints",
  "priorityMasterId": "<low-priority-id>",
  "responseTimeHours": 48,
  "resolutionTimeHours": 120,
  "escalationTimeHours": 96,
  "isActive": true
}
```

**Policy 2: Normal Priority SLA**
```json
{
  "name": "Normal Priority SLA",
  "priorityMasterId": "<normal-priority-id>",
  "responseTimeHours": 24,
  "resolutionTimeHours": 72,
  "escalationTimeHours": 48
}
```

**Policy 3: High Priority SLA**
```json
{
  "name": "High Priority SLA",
  "priorityMasterId": "<high-priority-id>",
  "responseTimeHours": 8,
  "resolutionTimeHours": 24,
  "escalationTimeHours": 16
}
```

**Policy 4: Critical Priority SLA**
```json
{
  "name": "Critical Priority SLA",
  "priorityMasterId": "<critical-priority-id>",
  "responseTimeHours": 4,
  "resolutionTimeHours": 12,
  "escalationTimeHours": 8
}
```

**Policy 5: Urgent Priority SLA**
```json
{
  "name": "Urgent Priority SLA",
  "priorityMasterId": "<urgent-priority-id>",
  "responseTimeHours": 2,
  "resolutionTimeHours": 8,
  "escalationTimeHours": 4
}
```

**Step 3.2: Create Category-Based SLA Policies**

**Policy 6: Technical Issues SLA**
```json
{
  "name": "Technical Issues SLA",
  "categoryId": "<technical-issues-category-id>",
  "responseTimeHours": 4,
  "resolutionTimeHours": 16,
  "escalationTimeHours": 12
}
```

**Policy 7: Billing Problems SLA**
```json
{
  "name": "Billing Problems SLA",
  "categoryId": "<billing-category-id>",
  "responseTimeHours": 6,
  "resolutionTimeHours": 24,
  "escalationTimeHours": 18
}
```

---

### PHASE 4: WORKFLOW CONFIGURATION

**Step 4.1: Create Standard Workflow**
```json
{
  "name": "Standard Complaint Workflow",
  "description": "Default workflow for all complaints",
  "isActive": true,
  "states": [
    {"statusMasterId": "<submitted-id>", "order": 1, "isInitial": true},
    {"statusMasterId": "<under-review-id>", "order": 2},
    {"statusMasterId": "<in-progress-id>", "order": 3},
    {"statusMasterId": "<resolved-id>", "order": 4},
    {"statusMasterId": "<closed-id>", "order": 5, "isFinal": true}
  ],
  "transitions": [
    {"from": "Submitted", "to": "Under Review", "requiresApproval": false},
    {"from": "Under Review", "to": "In Progress", "requiresApproval": false},
    {"from": "In Progress", "to": "Resolved", "requiresApproval": false},
    {"from": "Resolved", "to": "Closed", "requiresApproval": true}
  ]
}
```

**Step 4.2: Create Fast Track Workflow**
```json
{
  "name": "Fast Track Workflow",
  "description": "Quick resolution workflow for simple complaints",
  "isActive": true,
  "states": [
    {"statusMasterId": "<submitted-id>", "order": 1, "isInitial": true},
    {"statusMasterId": "<in-progress-id>", "order": 2},
    {"statusMasterId": "<resolved-id>", "order": 3},
    {"statusMasterId": "<closed-id>", "order": 4, "isFinal": true}
  ]
}
```

**Step 4.3: Create Escalation Workflow**
```json
{
  "name": "Escalation Workflow",
  "description": "Workflow with escalation path",
  "isActive": true,
  "allowsEscalation": true,
  "states": [
    {"statusMasterId": "<submitted-id>", "order": 1, "isInitial": true},
    {"statusMasterId": "<under-review-id>", "order": 2},
    {"statusMasterId": "<escalated-id>", "order": 3},
    {"statusMasterId": "<in-progress-id>", "order": 4},
    {"statusMasterId": "<resolved-id>", "order": 5},
    {"statusMasterId": "<closed-id>", "order": 6, "isFinal": true}
  ]
}
```

**Step 4.4: Assign Workflows to Categories**
- Technical Issues → Standard Workflow
- Billing Problems → Standard Workflow
- Attendance Issues → Fast Track Workflow
- HRMS System → Escalation Workflow

---

### PHASE 5: ESCALATION POLICY CONFIGURATION

**Step 5.1: Create Multi-Level Escalation Policy**

**Escalation Policy: Critical Issues**
```json
{
  "name": "Critical Issues Escalation",
  "description": "Three-level escalation for critical complaints",
  "isActive": true,
  "levels": [
    {
      "level": 1,
      "assignToUserId": "<naveen-chandra-id>",
      "escalationAfterHours": 4,
      "notifyEmails": ["naveen.chandra@oryggitech.com"]
    },
    {
      "level": 2,
      "assignToUserId": "<himanshu-id>",
      "escalationAfterHours": 8,
      "notifyEmails": ["himanshu.singh@oryggitech.com", "support@oryggitech.com"]
    },
    {
      "level": 3,
      "assignToUserId": "<marketing-team-id>",
      "escalationAfterHours": 12,
      "notifyEmails": ["marketing@oryggitech.com", "support@oryggitech.com"]
    }
  ]
}
```

**Step 5.2: Create Escalation Matrix**
```json
{
  "matrixName": "Priority-Based Escalation Matrix",
  "rules": [
    {
      "priorityMasterId": "<urgent-id>",
      "escalationPolicyId": "<critical-escalation-id>",
      "level1Hours": 2,
      "level2Hours": 4,
      "level3Hours": 6
    },
    {
      "priorityMasterId": "<critical-id>",
      "escalationPolicyId": "<critical-escalation-id>",
      "level1Hours": 4,
      "level2Hours": 8,
      "level3Hours": 12
    },
    {
      "priorityMasterId": "<high-id>",
      "escalationPolicyId": "<standard-escalation-id>",
      "level1Hours": 8,
      "level2Hours": 16,
      "level3Hours": 24
    }
  ]
}
```

---

### PHASE 6: NOTIFICATION RULES CONFIGURATION

**Step 6.1: Configure Notification Rules for Each Event**

**Rule 1: Complaint Created Notification**
```json
{
  "eventTypeId": "<complaint-created-event-id>",
  "templateId": "<complaint-created-template-id>",
  "channel": 0, // Email
  "recipientType": 1, // Complainant
  "isActive": true,
  "priority": 1
}
```

**Rule 2: Complaint Assigned Notification**
```json
{
  "eventTypeId": "<complaint-assigned-event-id>",
  "templateId": "<complaint-assigned-template-id>",
  "channel": 0, // Email
  "recipientType": 1, // AssignedHandler
  "specificEmails": "[]",
  "isActive": true,
  "priority": 1
}
```

**Rule 3: Complaint Escalated Notification**
```json
{
  "eventTypeId": "<complaint-escalated-event-id>",
  "templateId": "<complaint-escalated-template-id>",
  "channel": 0, // Email
  "recipientType": 2, // EscalationHandlers
  "specificEmails": "[\"marketing@oryggitech.com\",\"support@oryggitech.com\",\"himanshu.singh@oryggitech.com\"]",
  "isActive": true,
  "priority": 1
}
```

**Step 6.2: Create Missing Email Templates**

**Template: Due Soon Notification**
```json
{
  "name": "Complaint Due Soon",
  "subject": "Reminder: Complaint {complaintNumber} is due soon",
  "body": "Dear {assignedToName},\n\nThis is a reminder that complaint {complaintNumber} - {title} is due in 2 hours.\n\nPriority: {priorityName}\nDue Date: {dueDate}\n\nPlease take action promptly.\n\nBest regards,\nComplaint Management System",
  "channel": 0, // Email
  "isActive": true
}
```

---

### PHASE 7: TEST EXECUTION PLAN

#### Test Scenario 1: Create Low Priority Complaint with Standard Workflow
**Steps:**
1. Login as nav_nainital@yahoo.com
2. Create complaint:
   - Title: "Attendance marking issue"
   - Category: Attendance Issues
   - Priority: Low
   - Description: "Unable to mark attendance in the system"
3. System auto-assigns to Naveen.chandra@oryggitech.com
4. Verify emails sent:
   - ✅ nav_nainital@yahoo.com receives "Complaint Created"
   - ✅ Naveen.chandra@oryggitech.com receives "Complaint Assigned"

**Expected SLA:**
- Response: 48 hours
- Resolution: 120 hours
- Escalation: 96 hours

**Expected Workflow:** Submitted → Under Review → In Progress → Resolved → Closed

---

#### Test Scenario 2: Create Critical Priority Complaint with Escalation
**Steps:**
1. Login as nav_nainital@yahoo.com
2. Create complaint:
   - Title: "Payroll system down"
   - Category: Billing Problems
   - Priority: Critical
   - Description: "Unable to access payroll system for salary processing"
3. Assign to Naveen.chandra@oryggitech.com
4. Wait 4 hours (or simulate time passage)
5. System auto-escalates to Level 2 (himanshu.singh@oryggitech.com)
6. Wait 4 more hours
7. System auto-escalates to Level 3 (marketing@oryggitech.com)

**Verify Emails:**
- ✅ nav_nainital@yahoo.com: Creation, Assignment, Escalation Level 2, Escalation Level 3
- ✅ Naveen.chandra@oryggitech.com: Assignment, Escalation warning
- ✅ himanshu.singh@oryggitech.com: Escalation Level 2 assignment
- ✅ marketing@oryggitech.com: Escalation Level 3 assignment
- ✅ support@oryggitech.com: CC on escalation emails

---

#### Test Scenario 3: Complete Workflow Transition with Status Emails
**Steps:**
1. Login as Naveen.chandra@oryggitech.com
2. View assigned complaints
3. Change status: Submitted → Under Review
   - Verify "Status Changed" email sent
4. Change status: Under Review → In Progress
   - Verify "Status Changed" email sent
5. Add comment: "Working on resolution"
   - Verify "Comment Added" email sent to complainant
6. Change status: In Progress → Resolved
   - Add resolution notes
   - Verify "Complaint Resolved" email sent
7. Change status: Resolved → Closed
   - Verify "Complaint Closed" email sent

**Total Expected Emails:** 7+ emails

---

#### Test Scenario 4: Category-Specific SLA Testing
**Steps:**
1. Create 5 complaints with different categories:
   - Technical Issues (4h SLA)
   - Billing Problems (6h SLA)
   - Attendance Issues (48h SLA)
   - HRMS System (4h SLA)
   - Leave Management (2h SLA)
2. Verify each has correct SLA applied
3. Monitor SLA breach warnings
4. Verify "Due Soon" and "Overdue" emails sent

---

#### Test Scenario 5: Multi-Channel Notification Testing
**Steps:**
1. Create notification rules for Email, SMS, WhatsApp
2. Create complaint
3. Verify all channels triggered (check logs/database)
4. Verify template placeholders replaced:
   - {complaintNumber} → "CMP-1001"
   - {title} → Actual complaint title
   - {priorityName} → "Critical"
   - {assignedToName} → "Naveen Chandra"
   - {complainantEmail} → "nav_nainital@yahoo.com"

---

#### Test Scenario 6: Bulk Complaint Testing
**Steps:**
1. Create 20 complaints with various combinations:
   - 4 complaints × 5 priorities = 20 complaints
   - Each with different categories
   - Each with different workflows
2. Monitor email queue
3. Verify all notifications sent correctly
4. Check email delivery logs
5. Verify no email failures

---

## 📊 SUCCESS CRITERIA

### Email Delivery Metrics
- ✅ 100% email delivery success rate
- ✅ All templates render correctly with placeholders
- ✅ All recipients receive emails within 1 minute
- ✅ No duplicate emails sent
- ✅ Email logs show all sent emails

### SLA Compliance
- ✅ All complaints have correct SLA applied
- ✅ Due dates calculated correctly
- ✅ Breach warnings sent on time
- ✅ Escalation triggered at correct intervals

### Workflow Validation
- ✅ All state transitions work correctly
- ✅ Workflow constraints enforced
- ✅ Workflow history tracked accurately

### Escalation Validation
- ✅ Auto-escalation triggers at correct times
- ✅ Manual escalation works correctly
- ✅ Multi-level escalation follows matrix
- ✅ All escalation handlers notified

### User Experience
- ✅ Complainant can create and track complaints
- ✅ Handler can view and update assigned complaints
- ✅ Status changes reflected immediately
- ✅ Email notifications provide complete context

---

## 🔧 IMPLEMENTATION SCRIPTS

### Script 1: Complete System Setup
```powershell
# File: setup-complete-system.ps1
# Purpose: Configure all system components

# 1. Configure Email Server
# 2. Create Users
# 3. Create SLA Policies
# 4. Create Workflows
# 5. Create Escalation Policies
# 6. Configure Notification Rules
# 7. Run Validation Tests
```

### Script 2: Create Test Complaints
```powershell
# File: create-test-complaints.ps1
# Purpose: Create comprehensive test complaint set

# Create 20+ complaints covering all combinations
# Priority × Category × Workflow
```

### Script 3: Verify Email Delivery
```powershell
# File: verify-email-delivery.ps1
# Purpose: Check email logs and delivery status

# Query email logs
# Verify all emails sent
# Check for failures
# Generate report
```

---

## 📈 EXPECTED RESULTS

### Email Notifications Summary (Per Complaint Lifecycle)

**Minimum Expected Emails per Complaint:**
1. Complaint Created → Complainant
2. Complaint Assigned → Handler
3. Status Changed (×4) → Complainant & Handler
4. Comment Added (×2) → Complainant
5. Complaint Resolved → Complainant & Handler
6. Complaint Closed → Complainant & Handler

**Total:** ~11 emails per complaint lifecycle

**For 20 test complaints:** 220+ emails expected

**With Escalation:** +3 emails per escalated complaint

**With SLA Warnings:** +2 emails per complaint approaching due date

**Grand Total:** 250-300 emails for comprehensive testing

---

## ✅ VALIDATION CHECKLIST

### Phase 1: Setup ✅
- [ ] Email server configured and tested
- [ ] Test user created (nav_nainital@yahoo.com)
- [ ] Handler user verified (Naveen.chandra@oryggitech.com)
- [ ] Escalation users created/verified

### Phase 2: Configuration ✅
- [ ] 5 Priority-based SLA policies created
- [ ] 2+ Category-based SLA policies created
- [ ] 3+ Workflows created and assigned
- [ ] Escalation policy with 3 levels created
- [ ] Escalation matrix configured

### Phase 3: Notifications ✅
- [ ] 11 notification rules created (one per event)
- [ ] Missing email templates created
- [ ] Template placeholders tested
- [ ] Multi-channel rules configured

### Phase 4: Testing ✅
- [ ] 20+ test complaints created
- [ ] All priority levels tested
- [ ] All categories tested
- [ ] All workflows tested
- [ ] Escalation scenarios tested

### Phase 5: Validation ✅
- [ ] 250+ emails sent successfully
- [ ] All email logs verified
- [ ] All recipients received emails
- [ ] Placeholder replacement verified
- [ ] Email timing verified (< 1 min delivery)

### Phase 6: User Journey ✅
- [ ] Complainant can create complaints
- [ ] Complainant receives all notifications
- [ ] Handler can view assigned complaints
- [ ] Handler receives assignment notifications
- [ ] Status updates trigger emails correctly
- [ ] Comments trigger emails correctly

---

## 🎯 FINAL DELIVERABLES

1. **Comprehensive Test Report** - All test results documented
2. **Email Delivery Report** - All emails sent and delivered
3. **SLA Compliance Report** - SLA metrics and breach analysis
4. **Workflow Execution Report** - All workflow transitions tracked
5. **Escalation Report** - All escalation events documented
6. **User Experience Report** - Screenshots and user feedback
7. **System Configuration Backup** - All configurations exported

---

## 📞 QUICK START EXECUTION

**To execute this complete test plan:**

1. Run: `powershell -File setup-complete-system.ps1`
2. Run: `powershell -File create-test-complaints.ps1`
3. Monitor: Email logs and system activity
4. Validate: `powershell -File verify-email-delivery.ps1`
5. Review: Generated test reports

**Estimated Execution Time:** 2-3 hours for complete testing

---

**Document Created:** November 10, 2025
**Status:** Ready for Execution
**Target:** 100% Feature Coverage with Full Email Integration
