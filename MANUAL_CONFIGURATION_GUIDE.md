# Quick Manual Configuration Guide

**Time Required:** 2-3 hours
**Prerequisite:** Workflows and Escalation Matrices already created by automation scripts

---

## Step 1: Add Escalation Levels (30-45 minutes)

Navigate to: **Admin → Escalation Matrix Management**

### Matrix 1: Critical Issues (Urgent Priority)
Matrix ID: `7e7a40c4-6cb4-4168-acb9-85e7e32efe5c`

**Add these levels:**
| Level | Hours | Email | Name |
|-------|-------|-------|------|
| 1 | 4 | naveen.chandra@oryggitech.com | Level 1 - Naveen |
| 2 | 8 | himanshu.singh@oryggitech.com | Level 2 - Himanshu |
| 3 | 12 | marketing@oryggitech.com | Level 3 - Management |

**For each level:**
1. Click "Add Level" button
2. Enter Level Number (1, 2, or 3)
3. Enter Escalate After Hours
4. Enter Handler Email
5. Enter Handler Name
6. Check "Notify All Previous Levels"
7. Check "Active"
8. Click Save

---

### Matrix 2: Critical Priority
Matrix ID: `3e0d0a46-cded-453d-bf7b-13e06ccd5f52`

**Add the same 3 levels as Matrix 1** (copy the table above)

---

### Matrix 3: Standard (Normal Priority)
Matrix ID: `87d39635-342c-493a-ac04-75e46398b03b`

**Add these levels:**
| Level | Hours | Email | Name |
|-------|-------|-------|------|
| 1 | 24 | naveen.chandra@oryggitech.com | Support Team |
| 2 | 48 | support@oryggitech.com | Senior Support |

---

### Matrix 4: High Priority
Matrix ID: `95ee785b-d1c7-4bde-92db-ee45b64456a2`

**Add the same 2 levels as Matrix 3** (copy the table above)

---

### Matrix 5: Low Priority
Matrix ID: `3f468eba-0ff5-496e-bd46-5125201ff5b9`

**Add these levels:**
| Level | Hours | Email | Name |
|-------|-------|-------|------|
| 1 | 72 | support@oryggitech.com | Support Team |
| 2 | 120 | naveen.chandra@oryggitech.com | Management |

---

## Step 2: Create Notification Rules (45-60 minutes)

Navigate to: **Admin → Notification Rule Management**

### Rule 1: Complaint Created
- **Name:** Complaint Created Notification
- **Event Type:** COMPLAINT_CREATED
- **Description:** Notify complainant when complaint is submitted
- **Channel:** Email
- **Recipient Type:** Complainant
- **Priority:** High (2)
- **Status:** Active
- **Template:** Select or use default

### Rule 2: Complaint Assigned
- **Name:** Complaint Assigned Notification
- **Event Type:** COMPLAINT_ASSIGNED
- **Description:** Notify handler when complaint is assigned
- **Channel:** Email
- **Recipient Type:** Assigned Handler
- **Priority:** High (2)
- **Status:** Active

### Rule 3: Complaint Closed
- **Name:** Complaint Closed Notification
- **Event Type:** COMPLAINT_CLOSED
- **Description:** Notify parties when complaint is closed
- **Channel:** Email
- **Recipient Type:** Both (Complainant + Handler)
- **Priority:** High (2)
- **Status:** Active

### Rule 4: Complaint Escalated
- **Name:** Complaint Escalated Notification
- **Event Type:** COMPLAINT_ESCALATED
- **Description:** Alert escalation handlers
- **Channel:** Email
- **Recipient Type:** Escalation Handlers
- **CC Recipients:** support@oryggitech.com
- **Priority:** Urgent (1)
- **Status:** Active

### Rule 5: Complaint Overdue
- **Name:** Complaint Overdue Alert
- **Event Type:** COMPLAINT_OVERDUE
- **Description:** Alert when complaint exceeds SLA
- **Channel:** Email
- **Recipient Type:** Handler + Manager
- **Priority:** Urgent (1)
- **Status:** Active

### Rule 6: Status Changed
- **Name:** Status Changed Notification
- **Event Type:** COMPLAINT_STATUS_CHANGED
- **Description:** Notify complainant of status changes
- **Channel:** Email
- **Recipient Type:** Complainant
- **Priority:** Normal (3)
- **Status:** Active

### Rule 7: Comment Added
- **Name:** Comment Added Notification
- **Event Type:** COMPLAINT_COMMENTED
- **Description:** Notify when new comment is added
- **Channel:** Email
- **Recipient Type:** Complainant
- **Priority:** Normal (3)
- **Status:** Active

---

## Step 3: Verify Configuration (15-30 minutes)

### Check Workflows
1. Go to **Admin → Workflow Management**
2. Verify all 3 workflows exist:
   - Standard Complaint Workflow
   - Fast Track Workflow
   - Escalation Required Workflow
3. Click each workflow and verify statuses and transitions

### Check Escalation Matrices
1. Go to **Admin → Escalation Matrix**
2. Verify 5 matrices exist with correct priority mappings
3. Verify each matrix has the correct number of levels
4. Test escalation resolution (if available)

### Check Notification Rules
1. Go to **Admin → Notification Rule Management**
2. Verify 7 notification rules are listed
3. Verify all are set to Active
4. Check event type mappings are correct

---

## Step 4: Initial Testing (30-60 minutes)

### Test Workflow Transitions
1. Create a test complaint in "Attendance Issues" category
2. Verify it starts in "Submitted" status
3. Click transition buttons to move through workflow:
   - Start Review → Under Review
   - Begin Work → In Progress
   - Mark Resolved → Resolved
   - Close → Closed
4. Verify each transition works

### Test Fast Track Workflow
1. Create complaint in "Service Delays" category
2. Verify fast track workflow is used
3. Test: Submitted → In Progress → Resolved → Closed

### Test Escalation Workflow
1. Create complaint in "Technical Issues" category
2. Test: Submitted → Under Review → Escalated → In Progress → Resolved → Closed

### Test Notifications (if email configured)
1. Create a complaint - check for creation email
2. Assign complaint - check for assignment email
3. Add comment - check for comment email
4. Close complaint - check for closure email

### Test Escalation (requires waiting)
1. Create an urgent priority complaint
2. Wait 4+ hours
3. Check if escalation level 1 is triggered
4. Verify escalation notification sent

---

## Quick Checklist

### Escalation Levels Added
- [ ] Matrix 1 - Critical Issues (3 levels)
- [ ] Matrix 2 - Critical Priority (3 levels)
- [ ] Matrix 3 - Standard/Normal (2 levels)
- [ ] Matrix 4 - High Priority (2 levels)
- [ ] Matrix 5 - Low Priority (2 levels)

### Notification Rules Created
- [ ] Complaint Created
- [ ] Complaint Assigned
- [ ] Complaint Closed
- [ ] Complaint Escalated
- [ ] Complaint Overdue
- [ ] Status Changed
- [ ] Comment Added

### Testing Completed
- [ ] Workflow 1 - Standard tested
- [ ] Workflow 2 - Fast Track tested
- [ ] Workflow 3 - Escalation tested
- [ ] Notifications verified (if email configured)
- [ ] Escalation matrix verified

---

## Troubleshooting

### Can't See Escalation Matrix Menu
- Check user has "ManageEscalation" permission
- Try refreshing the page
- Check Admin menu is expanded

### Can't Create Notification Rule
- Verify event types are loaded
- Check required fields are filled
- Ensure email server is configured (for email rules)

### Workflow Not Appearing for Category
- Verify category is assigned to workflow
- Check workflow is set to Active
- Refresh complaint creation form

### Escalation Not Triggering
- Verify escalation levels are added and active
- Check priority mapping is correct
- Ensure auto-escalation service is running
- Check complaint is not already escalated

---

## After Configuration

### Enable Email Notifications
1. Go to **Admin → Email Settings**
2. Configure SMTP server details
3. Test email connectivity
4. Verify "Send Notifications" is enabled

### Review User Permissions
1. Go to **Admin → Role Management**
2. Verify roles have appropriate permissions:
   - ViewComplaints
   - CreateComplaint
   - EditComplaint
   - ManageEscalation (for admins)
   - ViewEscalation

### Set Up Resource Pools (Optional)
1. Go to **Admin → Resource Pool Management**
2. Create pools for different complaint categories
3. Assign users to pools
4. Configure auto-assignment rules

---

## Need Help?

- **Workflows Already Created:** IDs in main report
- **Matrix IDs:** Listed in Step 1 above
- **Event Type Codes:** Listed in Step 2
- **Full Documentation:** See `SYSTEM_CONFIGURATION_COMPLETE_REPORT.md`

---

**Last Updated:** November 10, 2025
**Estimated Total Time:** 2-3 hours
**Difficulty:** Easy to Medium
