# Workflow Management - Quick Reference Guide

**Last Updated:** November 3, 2025

---

## Three Critical Questions - ANSWERED

### ❓ Question 1: Can we delete a workflow?

**Answer: ❌ NO**

**Evidence:**
- No DELETE endpoint in API (`WorkflowController.cs`)
- No delete button in UI (`workflow-management.component.html`)
- API returns 404 when DELETE is attempted

**Why not?**
- Workflows are linked to complaints
- Deleting would break data integrity
- Would destroy audit trail

**What to do instead?**
✅ Use soft delete: Set `isActive = false`
✅ Rename: "ARCHIVED - {Original Name}"
✅ Document reason in description field

---

### ❓ Question 2: How to associate a workflow with a category?

**Answer: ✅ Select category from dropdown when creating workflow**

**Steps:**
1. Click "Create Workflow" button
2. **Select category from dropdown** ← THIS IS THE ASSOCIATION!
3. Enter workflow name
4. Enter description (optional)
5. Check "Active" and "Default" if needed
6. Click "Create Workflow"

**Key Points:**
- ONE workflow = ONE category selection
- Association is automatic when you select the category
- The workflow applies to ALL complaints in that category
- You can see the category name next to the workflow name in the list

**Example:**
```
When you select "IT Support" from category dropdown:
→ You're creating a workflow FOR "IT Support"
→ All IT Support complaints will use this workflow
→ The system shows "Category: IT Support" in workflow details
```

---

### ❓ Question 3: What is SLA in workflow?

**Answer: ✅ Service Level Agreement - Time limit for each status**

**Definition:**
SLA = Maximum time allowed for a complaint to stay in a status

**Where to configure:**
1. Select workflow
2. Click "Add Status"
3. Enter value in "Default SLA (hours)" field

**Example SLA values:**
```
Status: SUBMITTED      → SLA: 24 hours (must acknowledge in 24h)
Status: IN PROGRESS    → SLA: 48 hours (must work on it in 48h)
Status: ESCALATED      → SLA: 4 hours  (urgent, needs quick action)
Status: RESOLVED       → SLA: 72 hours (waiting for confirmation)
```

**What happens when SLA is breached?**
- System marks complaint as overdue
- Automatic escalation can be triggered
- Alerts sent to managers
- Reported in SLA breach reports

---

## Common Workflows

### Customer Support Workflow
```
SUBMITTED (2h SLA)
    ↓ [Acknowledge]
ASSIGNED (4h SLA)
    ↓ [Start Work]
IN PROGRESS (24h SLA)
    ↓ [Resolve]
RESOLVED (48h SLA)
    ↓ [Close]
CLOSED
```

### IT Support Workflow
```
SUBMITTED (4h SLA)
    ↓ [Assign]
ASSIGNED (8h SLA)
    ↓ [Start Work]
IN PROGRESS (48h SLA)
    ↓ [Escalate if needed]
ESCALATED (2h SLA) ⚠️
    ↓ [Resolve]
RESOLVED (24h SLA)
    ↓ [Close]
CLOSED
```

### HR Workflow
```
SUBMITTED (24h SLA)
    ↓ [Review]
UNDER REVIEW (72h SLA)
    ↓ [Investigate]
PENDING (120h SLA)
    ↓ [Resolve]
RESOLVED (48h SLA)
    ↓ [Close]
CLOSED
```

---

## Quick Actions

### Create a New Workflow
1. Admin > Workflow Management
2. Click "+ Create Workflow"
3. Select Category (REQUIRED)
4. Enter Name
5. Click "Create Workflow"

### Add Status to Workflow
1. Select workflow from list
2. Click "+ Add Status"
3. Select status from dropdown
4. Enter Display Order (1, 2, 3...)
5. **Enter Default SLA (hours)** ⏰
6. Check "Initial Status" if first status
7. Click "Add Status"

### Add Transition Between Statuses
1. Select workflow from list
2. Click "+ Add Transition"
3. Select "From Status"
4. Select "To Status"
5. Enter Transition Name (e.g., "Start Work")
6. Check "Requires Comment" if needed
7. Check "Requires Approval" if needed
8. Click "Add Transition"

### Deactivate (Soft Delete) a Workflow
1. Currently not supported in UI
2. Must be done via database:
   ```sql
   UPDATE CategoryWorkflow
   SET IsActive = 0
   WHERE Id = '{workflow-id}'
   ```

---

## API Endpoints

### Get All Workflows
```
GET /api/workflows
Authorization: Bearer {token}
```

### Get Workflow for Category
```
GET /api/workflows/category/{categoryId}
Authorization: Bearer {token}
```

### Create Workflow
```
POST /api/workflows
Authorization: Bearer {token}
Content-Type: application/json

{
  "categoryId": "guid",
  "name": "string",
  "description": "string",
  "isActive": true,
  "isDefault": true
}
```

### Add Status to Workflow
```
POST /api/workflows/{workflowId}/statuses
Authorization: Bearer {token}
Content-Type: application/json

{
  "statusMasterId": "guid",
  "displayOrder": 1,
  "isInitialStatus": true,
  "defaultSLAHours": 24
}
```

### Add Transition to Workflow
```
POST /api/workflows/{workflowId}/transitions
Authorization: Bearer {token}
Content-Type: application/json

{
  "fromStatusId": "guid",
  "toStatusId": "guid",
  "transitionName": "Start Work",
  "requiresComment": false,
  "requiresApproval": false,
  "allowedRoles": []
}
```

---

## Database Tables

### CategoryWorkflow
```
Id (PK)
CategoryId (FK) ← Links to category
Name
Description
IsActive ← Use this for soft delete
IsDefault
CompanyId (FK)
CreatedAt
UpdatedAt
```

### CategoryWorkflowStatus
```
Id (PK)
WorkflowId (FK)
StatusMasterId (FK)
DisplayOrder
IsInitialStatus
DefaultSLAHours ← SLA configuration
EscalationHours
RequiresApproval
AllowedRoles
CreatedAt
```

### CategoryWorkflowTransition
```
Id (PK)
WorkflowId (FK)
FromStatusId (FK)
ToStatusId (FK)
TransitionName
RequiresComment
RequiresApproval
AllowedRoles
DisplayOrder
CreatedAt
```

---

## Best Practices

### Workflow Design
✅ Keep it simple (4-6 statuses)
✅ Use clear status names
✅ Always have one initial status
✅ Include a final/closed status
✅ Name workflows clearly: "{Category} - {Purpose} Workflow"

### SLA Configuration
✅ Be realistic with time limits
✅ Shorter SLA for escalated statuses
✅ Consider business hours vs calendar hours
✅ Set escalation 10-20% before SLA breach
✅ Monitor and adjust based on performance

### Transitions
✅ Require comments for important transitions
✅ Require approval for critical actions
✅ Restrict sensitive transitions to managers
✅ Don't create circular transitions
✅ Use descriptive transition names

### Category Association
✅ One default workflow per category
✅ Test workflow before activating
✅ Document workflow purpose
✅ Train users on new workflows
✅ Review workflows quarterly

---

## Troubleshooting

### Problem: Workflow not appearing for category
**Solution:**
- Check if workflow `IsActive = true`
- Verify `CategoryId` matches
- Check if workflow is marked as default

### Problem: Can't transition between statuses
**Solution:**
- Verify transition exists in workflow
- Check user has required role
- Verify status is in the workflow
- Check if comment is required

### Problem: SLA not triggering
**Solution:**
- Verify SLA hours > 0
- Check if status is active
- Verify SLA monitoring service is running
- Check database for SLA configuration

### Problem: Want to delete workflow
**Solution:**
- Cannot delete, use soft delete instead
- Set `IsActive = false`
- Rename to "ARCHIVED - {Name}"
- Update description with reason

---

## Real System Data (as of Nov 3, 2025)

### Current Workflows:
1. **E2E Test Workflow 20251103085011**
   - Category: Attendance Issues
   - Statuses: 1 (Submitted, 24h SLA)
   - Active: Yes

2. **E2E Test Workflow 20251103085227**
   - Category: Attendance Issues
   - Statuses: 1 (Submitted, 24h SLA)
   - Active: Yes

3. **Test Workflow 155358**
   - Category: Attendance Issues
   - Statuses: 3
     - Submitted (4h SLA)
     - In Progress (24h SLA)
     - Escalated (1h SLA)
   - Transitions: 2
   - Active: Yes

### Available Categories:
- Attendance Issues
- Product Quality Issues
- Salary & Payroll
- Service Delays
- Billing Problems
- HRMS System
- Leave Management
- (and more...)

---

## Visual References

See companion documents:
- **WORKFLOW_MANAGEMENT_VISUAL_GUIDE.md** - Detailed explanations with examples
- **WORKFLOW_VISUAL_DIAGRAMS.md** - 15 comprehensive diagrams

---

## Key Takeaways

1. **No deletion** - Use IsActive flag for soft delete
2. **Category selection** - Happens at workflow creation time
3. **SLA is critical** - Set realistic time limits for each status
4. **One workflow per category** - But you can have multiple workflows for same category
5. **Transitions matter** - Define clear paths between statuses
6. **Test before deploying** - Always test workflows with sample complaints

---

## Support

- **Documentation**: See full guide in WORKFLOW_MANAGEMENT_VISUAL_GUIDE.md
- **Diagrams**: See WORKFLOW_VISUAL_DIAGRAMS.md for visual explanations
- **API Reference**: Check WorkflowController.cs for latest endpoints
- **Training**: Contact IT admin for hands-on training

---

**Quick Reference Version:** 1.0
**Date:** November 3, 2025
**System:** Complaint Management System

---

*Print this guide for quick reference!*
