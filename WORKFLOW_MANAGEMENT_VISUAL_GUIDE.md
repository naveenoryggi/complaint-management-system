# Workflow Management - Comprehensive Visual Guide

**Date:** November 3, 2025
**System:** Complaint Management System
**Purpose:** Answer three critical questions about workflow management

---

## Table of Contents
1. [Question 1: Can We Delete a Workflow?](#question-1-can-we-delete-a-workflow)
2. [Question 2: How to Associate a Workflow with a Category?](#question-2-how-to-associate-a-workflow-with-a-category)
3. [Question 3: What is SLA in Workflow?](#question-3-what-is-sla-in-workflow)
4. [Workflow Architecture Diagram](#workflow-architecture-diagram)
5. [Best Practices and Recommendations](#best-practices-and-recommendations)

---

## Question 1: Can We Delete a Workflow?

### Answer: NO - Workflow deletion is NOT currently supported

#### Evidence from Code Analysis:

**Backend API Controller** (`WorkflowController.cs`):
- The controller contains the following endpoints:
  - ✅ `GET /api/workflows` - Get all workflows
  - ✅ `GET /api/workflows/category/{categoryId}` - Get workflow for category
  - ✅ `POST /api/workflows` - Create workflow
  - ✅ `POST /api/workflows/{workflowId}/statuses` - Add status to workflow
  - ✅ `POST /api/workflows/{workflowId}/transitions` - Add transition rule
  - ❌ **NO DELETE endpoint exists**

**API Test Result:**
```bash
DELETE /api/workflows/{id}
Response: 404 Not Found
```

**Frontend UI** (`workflow-management.component.html`):
- The workflow list displays workflows with their status
- There is NO delete button or delete option in the UI
- The only available actions are:
  - View workflow details
  - Create new workflow
  - Add statuses to workflow
  - Add transitions to workflow

### Visual Representation:

```
┌─────────────────────────────────────────────────────┐
│  Workflow Management UI                             │
├─────────────────────────────────────────────────────┤
│                                                      │
│  [Create Workflow] ← Only Creation Button           │
│                                                      │
│  ┌──────────────────────┐  ┌─────────────────────┐ │
│  │ Workflow List        │  │ Workflow Details    │ │
│  ├──────────────────────┤  ├─────────────────────┤ │
│  │ ☑ IT Support        │  │ Name: IT Support    │ │
│  │   (Active)          │  │ Category: IT Issues │ │
│  │                     │  │                     │ │
│  │ ☑ HR Workflow       │  │ [Add Status]        │ │
│  │   (Active)          │  │ [Add Transition]    │ │
│  │                     │  │                     │ │
│  │ NO DELETE BUTTON ❌ │  │ NO DELETE OPTION ❌ │ │
│  └──────────────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────┘
```

### Current Workflow Data:
The system has **3 workflows** currently active:
1. **E2E Test Workflow 20251103085011** - Attendance Issues
2. **E2E Test Workflow 20251103085227** - Attendance Issues
3. **Test Workflow 155358** - Attendance Issues

### Why Can't We Delete?

**Possible Reasons:**
1. **Data Integrity**: Workflows are linked to complaints. Deleting a workflow would orphan complaints.
2. **Audit Trail**: Workflows define the history of complaint status changes. Deletion would break audit trails.
3. **Design Decision**: The system uses an "IsActive" flag instead of hard deletion (soft delete pattern).

### Recommendation:

Instead of deletion, use the **Deactivation Pattern**:
- Set `isActive = false` to "soft delete" a workflow
- This preserves data integrity while removing it from active use
- Historical complaints maintain their workflow references

---

## Question 2: How to Associate a Workflow with a Category?

### Answer: ONE Workflow = ONE Category (Direct Association)

### The Category-Workflow Relationship:

```
Category (1) ────────────> (1) Workflow
   │
   └─ When creating a workflow, you SELECT which category it belongs to
   └─ Each workflow can only be associated with ONE category
   └─ Multiple workflows can share the same category
```

### Step-by-Step: Creating a Workflow with Category Association

#### Step 1: Click "Create Workflow" Button
![Create Workflow Button Location]
- Located at the top-right of the Workflow Management page
- Button text: "+ Create Workflow"

#### Step 2: Category Selection (Critical Step!)
![Create Workflow Modal]

**The Create Workflow Modal contains:**

```html
┌──────────────────────────────────────────┐
│  Create New Workflow                  [X]│
├──────────────────────────────────────────┤
│                                          │
│  Category *        [Select Category ▼]   │  ← THIS IS THE ASSOCIATION!
│                    ├─ Attendance Issues  │
│                    ├─ Salary & Payroll   │
│                    ├─ IT Support         │
│                    ├─ Leave Management   │
│                    └─ Product Quality    │
│                                          │
│  Workflow Name *   [________________]    │
│                                          │
│  Description       [________________]    │
│                    [________________]    │
│                                          │
│  [✓] Active                             │
│  [✓] Set as Default Workflow            │
│                                          │
│  [Cancel]  [Create Workflow]            │
└──────────────────────────────────────────┘
```

#### Step 3: Understanding the Association

**When you select "IT Support" from the category dropdown:**
- You are creating a workflow **FOR** the "IT Support" category
- This workflow will **ONLY** be used when complaints are created under "IT Support"
- The workflow defines the **status lifecycle** for IT Support complaints

**Example:**
```javascript
// Create Workflow Request
{
  "categoryId": "a4e6d993-ea9b-442f-a803-e61356c56760",  // Attendance Issues
  "name": "Standard Attendance Workflow",
  "description": "Workflow for handling attendance-related complaints",
  "isActive": true,
  "isDefault": true
}

// Result: This workflow is now LINKED to "Attendance Issues" category
```

#### Step 4: Verify the Association

After creating the workflow, you can verify it:

**Method 1: In Workflow List**
```
┌──────────────────────────────────┐
│ Workflow: Standard Attendance    │
│ Category: Attendance Issues ←────│ Association visible here!
│ Status: Active                   │
└──────────────────────────────────┘
```

**Method 2: Via API**
```bash
GET /api/workflows/category/{categoryId}
# Returns the workflow associated with that category
```

**Method 3: In Workflow Details**
```
Workflow Information
├─ Name: Standard Attendance Workflow
├─ Category: Attendance Issues ← Shows the linked category
├─ Status: Active
└─ Default: Yes
```

### Real-World Example:

**Scenario: IT Department needs a specialized workflow**

1. **IT Manager logs in**
2. **Navigates to**: Admin > Workflow Management
3. **Clicks**: "Create Workflow" button
4. **Selects Category**: "IT Support" from dropdown
5. **Enters Name**: "IT Support Escalation Workflow"
6. **Enters Description**: "Fast-track workflow for critical IT issues"
7. **Checks**: "Active" and "Set as Default"
8. **Clicks**: "Create Workflow"

**Result:**
- All new complaints created under "IT Support" category will use this workflow
- The workflow defines which statuses are available (e.g., Submitted → Assigned → In Progress → Resolved)
- SLA timings are enforced based on this workflow's status configurations

### Database Relationship:

```sql
-- CategoryWorkflow Table
CREATE TABLE CategoryWorkflow (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CategoryId UNIQUEIDENTIFIER NOT NULL,  ← FOREIGN KEY to Category
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT NOT NULL,
    IsDefault BIT NOT NULL,
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    CONSTRAINT FK_CategoryWorkflow_Category
        FOREIGN KEY (CategoryId) REFERENCES ComplaintCategory(Id)
);
```

### Key Insights:

✅ **One-to-One Selection**: When creating a workflow, you MUST select exactly one category
✅ **Visual Confirmation**: The category name is displayed next to the workflow name
✅ **Automatic Linking**: The system automatically uses the workflow when a complaint is created in that category
✅ **Multiple Workflows Per Category**: You can have multiple workflows for the same category (e.g., "Standard Workflow" and "Express Workflow" both for IT Support)

---

## Question 3: What is SLA in Workflow?

### Answer: SLA = Service Level Agreement (Time-based Performance Target)

### What is SLA?

**Definition:**
SLA (Service Level Agreement) defines the **maximum time allowed** for a complaint to remain in a specific status before it must be addressed or escalated.

**In Simple Terms:**
- "How long can a complaint stay in this status before we're breaking our promise to the customer?"
- It's a **deadline** or **time limit** for each stage of complaint handling

### Where SLA is Configured:

#### Location 1: When Adding a Status to a Workflow

![Add Status Modal]

```html
┌──────────────────────────────────────────┐
│  Add Status to Workflow               [X]│
├──────────────────────────────────────────┤
│                                          │
│  Status *          [Submitted ▼]         │
│                                          │
│  Display Order *   [1]                   │
│                                          │
│  ┌────────────────────────────────────┐ │
│  │  Default SLA (hours)  [24]   ←─────│─│─ THIS IS SLA!
│  └────────────────────────────────────┘ │
│                                          │
│  Escalation Hours  [4]                   │
│                                          │
│  [✓] Set as Initial Status              │
│  [ ] Requires Approval                  │
│                                          │
│  [Cancel]  [Add Status]                 │
└──────────────────────────────────────────┘
```

**Field Explanation:**
- **Default SLA (hours)**: Number of hours allowed for this status
- **Escalation Hours**: Hours before automatic escalation (if enabled)

### Real-World SLA Examples:

#### Example 1: Customer Support Workflow

```
┌─────────────────────────────────────────────────────────┐
│  Status: SUBMITTED                                      │
│  ├─ Default SLA: 24 hours                              │
│  └─ Meaning: Complaint must be acknowledged within 24h │
│                                                         │
│  Status: IN PROGRESS                                    │
│  ├─ Default SLA: 48 hours                              │
│  └─ Meaning: Work must be actively done within 48h     │
│                                                         │
│  Status: ESCALATED                                      │
│  ├─ Default SLA: 4 hours                               │
│  └─ Meaning: Urgent! Must be addressed within 4h       │
│                                                         │
│  Status: RESOLVED                                       │
│  ├─ Default SLA: 72 hours                              │
│  └─ Meaning: Customer must confirm resolution in 72h   │
└─────────────────────────────────────────────────────────┘
```

#### Example 2: IT Support Workflow (from actual data)

**Current System Data:**
```json
{
  "workflow": "Test Workflow 155358",
  "category": "Attendance Issues",
  "statuses": [
    {
      "status": "Submitted",
      "displayOrder": 1,
      "defaultSLAHours": 4,  ← Must be acknowledged in 4 hours
      "isInitialStatus": true
    },
    {
      "status": "In Progress",
      "displayOrder": 2,
      "defaultSLAHours": 24,  ← Must show progress in 24 hours
      "isInitialStatus": false
    },
    {
      "status": "Escalated",
      "displayOrder": 3,
      "defaultSLAHours": 1,  ← URGENT! Only 1 hour allowed
      "isInitialStatus": false
    }
  ]
}
```

### How SLA Affects Complaint Management:

#### Visual SLA Timeline:

```
Complaint Created: Monday 9:00 AM
Status: SUBMITTED (SLA: 24 hours)

Timeline:
├─ Monday 9:00 AM  ─────────────────────> Complaint submitted
│                                          SLA Clock starts ⏰
│
├─ Monday 10:00 AM ─────────────────────> Assigned to agent
│                                          Status: IN PROGRESS
│                                          New SLA: 48 hours ⏰
│
├─ Tuesday 2:00 PM ─────────────────────> Issue escalated
│                                          Status: ESCALATED
│                                          New SLA: 4 hours ⏰⚠️
│
├─ Tuesday 5:00 PM ─────────────────────> Issue resolved
│                                          Status: RESOLVED
│                                          SLA Met ✅
└─ Timeline end
```

#### SLA Violation Example:

```
❌ SLA BREACH ALERT
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Complaint ID: CMP-1234
Category: IT Support
Status: SUBMITTED
Time in Status: 28 hours
SLA Limit: 24 hours
Breach: 4 hours overdue ⚠️

Action Required: Immediate attention!
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

### SLA Benefits:

✅ **Performance Measurement**: Track if team is meeting commitments
✅ **Customer Expectations**: Set clear expectations for resolution times
✅ **Automatic Escalation**: System can auto-escalate when SLA is about to breach
✅ **Reporting**: Generate reports on SLA compliance
✅ **Resource Planning**: Identify bottlenecks based on SLA breaches

### SLA vs Escalation Hours:

```
┌────────────────────────────────────────────────────────┐
│  Default SLA Hours: 24                                 │
│  ├─ Total time allowed in this status                 │
│  └─ Complaint is "overdue" after 24 hours             │
│                                                        │
│  Escalation Hours: 4                                   │
│  ├─ Warning time before SLA breach                    │
│  └─ System escalates to manager at 20 hours (24-4)    │
│                                                        │
│  Timeline:                                             │
│  ├─ 0h:  Complaint enters status                      │
│  ├─ 20h: ⚠️ Escalation triggered (4h before SLA)      │
│  ├─ 24h: ❌ SLA breach (deadline reached)             │
│  └─ 24h+: Overdue (breach continues)                  │
└────────────────────────────────────────────────────────┘
```

### Configuring SLA: Step-by-Step

**Step 1: Select Workflow**
- Navigate to Workflow Management
- Select the workflow you want to configure

**Step 2: Click "Add Status"**
- Click the "+ Add Status" button in the Workflow Statuses section

**Step 3: Choose Status and Set SLA**
```
1. Select Status: "In Progress"
2. Set Display Order: 2
3. ★ Enter Default SLA (hours): 48
4. Enter Escalation Hours: 8
5. Check "Requires Approval" if needed
6. Click "Add Status"
```

**Step 4: Repeat for All Statuses**
```
Submitted:    SLA = 24h (First response)
In Progress:  SLA = 48h (Active work)
Pending:      SLA = 72h (Waiting for info)
Escalated:    SLA = 4h  (Urgent attention)
Resolved:     SLA = 24h (Confirmation)
Closed:       SLA = 0h  (Final state)
```

### Best Practices for SLA:

✅ **Shorter SLA for Escalated**: Escalated issues should have shorter SLA
✅ **Realistic Times**: Don't set SLA so tight that they're impossible to meet
✅ **Consider Business Hours**: 24 hours could mean 3 business days
✅ **Different SLA for Categories**: Critical categories need shorter SLA
✅ **Monitor Compliance**: Regularly review SLA breach reports
✅ **Escalation Buffer**: Set escalation 10-20% before SLA breach

### SLA in the UI:

When viewing a complaint, the system shows:
```
┌────────────────────────────────────────┐
│ Complaint: CMP-1234                    │
├────────────────────────────────────────┤
│ Status: In Progress                    │
│ Time in Status: 18 hours               │
│ SLA Limit: 48 hours                    │
│ Time Remaining: 30 hours ✅            │
│ Status: On Track                       │
└────────────────────────────────────────┘

┌────────────────────────────────────────┐
│ Complaint: CMP-5678                    │
├────────────────────────────────────────┤
│ Status: Submitted                      │
│ Time in Status: 28 hours               │
│ SLA Limit: 24 hours                    │
│ Time Overdue: 4 hours ❌               │
│ Status: SLA BREACH ⚠️                 │
└────────────────────────────────────────┘
```

---

## Workflow Architecture Diagram

### Complete Workflow System Architecture:

```
┌─────────────────────────────────────────────────────────────────────┐
│                     COMPLAINT MANAGEMENT SYSTEM                      │
│                          Workflow Architecture                       │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────┐
│    COMPANY       │  (1 company can have many categories)
│  "TechCorp Inc"  │
└────────┬─────────┘
         │
         │ 1:N
         │
         ▼
┌──────────────────┐
│    CATEGORY      │  (Each category defines a type of complaint)
│  "IT Support"    │
│  "HR Issues"     │
│  "Facilities"    │
└────────┬─────────┘
         │
         │ 1:1 (One workflow per category)
         │
         ▼
┌──────────────────────────────────────────────────────────────┐
│                    WORKFLOW                                   │
│  Name: "IT Support Workflow"                                 │
│  Category: IT Support                                        │
│  IsActive: true                                              │
│  IsDefault: true                                             │
└────────┬─────────────────────────────────────────────────────┘
         │
         │ 1:N (One workflow has many statuses)
         │
         ▼
┌──────────────────────────────────────────────────────────────┐
│                 WORKFLOW STATUSES                             │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Status 1: SUBMITTED                            │          │
│  │ ├─ Display Order: 1                            │          │
│  │ ├─ Initial Status: Yes                         │          │
│  │ ├─ Default SLA: 24 hours ⏰                    │          │
│  │ ├─ Escalation: 4 hours                         │          │
│  │ └─ Requires Approval: No                       │          │
│  └────────────────────────────────────────────────┘          │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Status 2: IN PROGRESS                          │          │
│  │ ├─ Display Order: 2                            │          │
│  │ ├─ Initial Status: No                          │          │
│  │ ├─ Default SLA: 48 hours ⏰                    │          │
│  │ ├─ Escalation: 8 hours                         │          │
│  │ └─ Requires Approval: No                       │          │
│  └────────────────────────────────────────────────┘          │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Status 3: ESCALATED                            │          │
│  │ ├─ Display Order: 3                            │          │
│  │ ├─ Initial Status: No                          │          │
│  │ ├─ Default SLA: 4 hours ⏰⚠️                   │          │
│  │ ├─ Escalation: 1 hour                          │          │
│  │ └─ Requires Approval: Yes                      │          │
│  └────────────────────────────────────────────────┘          │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Status 4: RESOLVED                             │          │
│  │ ├─ Display Order: 4                            │          │
│  │ ├─ Initial Status: No                          │          │
│  │ ├─ Default SLA: 72 hours ⏰                    │          │
│  │ ├─ Escalation: N/A                             │          │
│  │ └─ Requires Approval: No                       │          │
│  └────────────────────────────────────────────────┘          │
└────────┬─────────────────────────────────────────────────────┘
         │
         │ 1:N (Statuses are connected by transitions)
         │
         ▼
┌──────────────────────────────────────────────────────────────┐
│                 WORKFLOW TRANSITIONS                          │
│                (Define allowed status changes)                │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Transition 1: "Start Work"                     │          │
│  │ ├─ From: SUBMITTED                             │          │
│  │ ├─ To: IN PROGRESS                             │          │
│  │ ├─ Requires Comment: No                        │          │
│  │ ├─ Requires Approval: No                       │          │
│  │ └─ Allowed Roles: Agent, Manager               │          │
│  └────────────────────────────────────────────────┘          │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Transition 2: "Escalate"                       │          │
│  │ ├─ From: IN PROGRESS                           │          │
│  │ ├─ To: ESCALATED                               │          │
│  │ ├─ Requires Comment: Yes ✍                    │          │
│  │ ├─ Requires Approval: No                       │          │
│  │ └─ Allowed Roles: Agent, Manager               │          │
│  └────────────────────────────────────────────────┘          │
│                                                               │
│  ┌────────────────────────────────────────────────┐          │
│  │ Transition 3: "Resolve"                        │          │
│  │ ├─ From: ESCALATED                             │          │
│  │ ├─ To: RESOLVED                                │          │
│  │ ├─ Requires Comment: Yes ✍                    │          │
│  │ ├─ Requires Approval: Yes ✓                   │          │
│  │ └─ Allowed Roles: Manager, Admin               │          │
│  └────────────────────────────────────────────────┘          │
└────────┬─────────────────────────────────────────────────────┘
         │
         │ Applied to
         │
         ▼
┌──────────────────────────────────────────────────────────────┐
│                    COMPLAINTS                                 │
│                                                               │
│  Complaint: CMP-1234                                         │
│  ├─ Category: IT Support                                     │
│  ├─ Workflow: IT Support Workflow (automatically assigned)   │
│  ├─ Current Status: IN PROGRESS                              │
│  ├─ Time in Status: 18 hours                                 │
│  ├─ SLA Limit: 48 hours                                      │
│  ├─ Time Remaining: 30 hours ✅                              │
│  └─ Available Transitions:                                   │
│      ├─ "Escalate" → ESCALATED                              │
│      └─ "Resolve" → RESOLVED (if approved)                  │
└──────────────────────────────────────────────────────────────┘
```

### Data Flow: Creating a Complaint with Workflow

```
┌─────────────────────────────────────────────────────────────────┐
│  Step 1: User Creates Complaint                                 │
│  ├─ User selects category: "IT Support"                        │
│  └─ System finds workflow for "IT Support" category            │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 2: System Applies Workflow                                │
│  ├─ Workflow: "IT Support Workflow"                            │
│  ├─ Initial Status: "SUBMITTED" (from workflow config)         │
│  ├─ SLA: 24 hours (from status config)                         │
│  └─ SLA Timer starts ⏰                                        │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 3: Agent Takes Action                                     │
│  ├─ Agent views complaint                                       │
│  ├─ System shows available transitions:                        │
│  │   └─ "Start Work" → IN PROGRESS                            │
│  ├─ Agent clicks "Start Work"                                  │
│  └─ System validates transition is allowed                     │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 4: Status Changes                                         │
│  ├─ Old Status: SUBMITTED                                       │
│  ├─ New Status: IN PROGRESS                                     │
│  ├─ Old SLA: 24 hours (completed in 2 hours ✅)                │
│  ├─ New SLA: 48 hours (timer resets ⏰)                        │
│  └─ History logged for audit                                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│  Step 5: SLA Monitoring (Continuous)                            │
│  ├─ System checks every 5 minutes                              │
│  ├─ Current time in status: 18 hours                           │
│  ├─ SLA limit: 48 hours                                        │
│  ├─ Escalation threshold: 40 hours (48 - 8)                   │
│  ├─ Status: ✅ On Track (18 < 40)                              │
│  └─ No action needed yet                                       │
└─────────────────────────────────────────────────────────────────┘
```

---

## Best Practices and Recommendations

### 1. Workflow Design Best Practices

✅ **Keep It Simple**: Start with 4-6 statuses maximum
✅ **Logical Flow**: Status transitions should follow natural progression
✅ **Clear Names**: Use intuitive status names (Submitted, In Progress, Resolved)
✅ **Initial Status**: Always have exactly one initial status marked
✅ **Terminal Status**: Have a final status where workflow ends (Closed, Completed)

### 2. SLA Configuration Best Practices

✅ **Realistic Timings**: Set SLA based on actual capability, not wishes
✅ **Business Hours**: Consider business hours vs calendar hours
✅ **Category-Specific**: Different categories may need different SLA
✅ **Escalation Buffer**: Set escalation 10-20% before SLA breach
✅ **Monitor & Adjust**: Regularly review SLA compliance and adjust

### 3. Category-Workflow Association Best Practices

✅ **One Default Workflow**: Each category should have one default workflow
✅ **Naming Convention**: Name workflows clearly: "{Category} - {Type} Workflow"
✅ **Documentation**: Document the purpose of each workflow
✅ **Review Regularly**: Quarterly review if workflows are still appropriate
✅ **Version Control**: Use dates or versions in workflow names for tracking

### 4. Workflow Deletion Strategy

Since deletion is not supported:

✅ **Use IsActive Flag**: Deactivate instead of delete
✅ **Soft Delete Pattern**: Set `isActive = false`
✅ **Archive Pattern**: Move old workflows to "Archive" category
✅ **Naming Convention**: Prefix with "ARCHIVED - " when deactivating
✅ **Document Reason**: Add reason in description field before deactivating

Example:
```
Before:
Name: "Old IT Workflow"
IsActive: true

After:
Name: "ARCHIVED - Old IT Workflow (Replaced 2025-11-01)"
Description: "Archived: Replaced by new workflow with improved SLA"
IsActive: false
```

### 5. Transition Configuration Best Practices

✅ **Require Comments**: Critical transitions should require comments
✅ **Approval Gates**: High-impact transitions need approval
✅ **Role-Based**: Restrict sensitive transitions to managers/admins
✅ **Prevent Loops**: Don't allow backwards transitions unless necessary
✅ **Visual Feedback**: Use button colors to indicate transition severity

### 6. SLA Examples by Industry

#### Customer Support:
```
Submitted:   2 hours  (First response)
In Progress: 24 hours (Active work)
Escalated:   1 hour   (Urgent)
Resolved:    48 hours (Confirmation)
```

#### IT Support:
```
Submitted:   4 hours  (Acknowledge)
Assigned:    8 hours  (Start work)
In Progress: 48 hours (Fix issue)
Escalated:   2 hours  (Critical)
Resolved:    24 hours (Verify fix)
```

#### HR Department:
```
Submitted:   24 hours (Review)
Under Review: 72 hours (Investigation)
Pending:     120 hours (Gathering info)
Resolved:    48 hours (Communicate decision)
```

### 7. Common Pitfalls to Avoid

❌ **Too Many Statuses**: More than 8 statuses becomes confusing
❌ **No Initial Status**: Workflow won't work without initial status
❌ **Unrealistic SLA**: Setting 1-hour SLA for complex issues
❌ **No Transitions**: Forgetting to create transitions between statuses
❌ **Missing Approval**: Not requiring approval for critical actions
❌ **Circular Transitions**: Creating infinite loops in workflow

### 8. Testing Your Workflow

Before deploying a new workflow:

✅ **Test All Paths**: Try every possible transition
✅ **Test SLA**: Verify SLA timers work correctly
✅ **Test Escalation**: Ensure escalation triggers properly
✅ **Test Permissions**: Verify role-based restrictions work
✅ **Test Edge Cases**: What happens at midnight? Weekends?
✅ **User Acceptance**: Have actual users test the workflow

### 9. Monitoring and Maintenance

Regular monitoring tasks:

✅ **Weekly**: Review SLA breach reports
✅ **Monthly**: Analyze workflow bottlenecks
✅ **Quarterly**: Review and update SLA targets
✅ **Annually**: Complete workflow redesign review
✅ **Continuous**: Monitor user feedback and pain points

### 10. Documentation Requirements

For each workflow, document:

✅ **Purpose**: Why this workflow exists
✅ **Scope**: Which types of complaints use it
✅ **Statuses**: Each status meaning and purpose
✅ **SLA Justification**: Why these specific SLA values
✅ **Transitions**: Which roles can perform which transitions
✅ **Escalation Path**: How escalation works
✅ **Success Metrics**: How to measure workflow effectiveness

---

## Summary: Quick Answers

### Question 1: Can we delete a workflow?
**Answer:** NO - Deletion is not supported. Use `isActive = false` to deactivate instead.

### Question 2: How to associate a workflow with a category?
**Answer:** When creating a workflow, select the category from the dropdown. This creates a 1:1 association. The workflow will automatically apply to all complaints in that category.

### Question 3: What is SLA in workflow?
**Answer:** SLA (Service Level Agreement) is the maximum time allowed for a complaint to stay in a specific status. Set during status configuration (e.g., "Submitted" status has 24-hour SLA). Used to measure performance and trigger escalations.

---

## Appendix: Current System Data

### Existing Workflows:
1. **E2E Test Workflow 20251103085011**
   - Category: Attendance Issues
   - Status: Active
   - Statuses: 1 (Submitted with 24h SLA)

2. **E2E Test Workflow 20251103085227**
   - Category: Attendance Issues
   - Status: Active
   - Statuses: 1 (Submitted with 24h SLA)

3. **Test Workflow 155358**
   - Category: Attendance Issues
   - Status: Active
   - Statuses: 3
     - Submitted (4h SLA)
     - In Progress (24h SLA)
     - Escalated (1h SLA)
   - Transitions: 2
     - "Start Work": Submitted → In Progress
     - "Resolve": In Progress → Escalated

### Available Categories:
- Attendance Issues
- Product Quality Issues
- Salary & Payroll
- Service Delays
- Billing Problems
- HRMS System
- Leave Management

---

**Document Created:** November 3, 2025
**System Version:** 1.0
**Last Updated:** November 3, 2025
**Author:** System Administrator

---

## Additional Resources

- **API Documentation**: `/api/workflows` endpoints
- **User Guide**: Workflow Management section
- **Admin Training**: Contact IT for workflow configuration training
- **Support**: helpdesk@complaintmanagement.com

---

*End of Workflow Management Visual Guide*
