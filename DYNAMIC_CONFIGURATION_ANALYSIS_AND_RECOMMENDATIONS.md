# Dynamic Configuration Analysis & Recommendations

## Executive Summary

**Current State**: The system already supports **dynamic statuses and priorities** but lacks **category-specific workflow configuration**.

**Recommendation**: Implement a flexible workflow engine that allows configuring status transitions per category while maintaining backward compatibility.

---

## Current System Capabilities

### ✅ What's Already Dynamic

#### 1. **Dynamic Statuses** (Already Supported)

**Current Structure**:
```csharp
public class ComplaintStatusMaster : BaseEntity
{
    public string Name { get; set; }                // Custom name
    public string Code { get; set; }                // Custom code
    public int DisplayOrder { get; set; }           // Custom ordering
    public string? ColorCode { get; set; }          // Custom UI color
    public bool IsActive { get; set; }              // Enable/disable
    public bool IsSystem { get; set; }              // System vs custom
    public bool IsFinal { get; set; }               // Terminal status
    public Guid? CompanyId { get; set; }            // Multi-tenant
}
```

**Capabilities**:
- ✅ Create unlimited custom statuses
- ✅ Company-specific statuses (multi-tenant support)
- ✅ 9 system statuses (cannot be deleted, but can be supplemented)
- ✅ Custom colors, icons, ordering
- ✅ Mark statuses as final/terminal

**Example Use Cases**:
- Company A adds: "Awaiting Parts", "Scheduled for Repair"
- Company B adds: "Legal Review", "Compliance Check"
- Company C adds: "Customer Approval", "On Hold"

---

#### 2. **Dynamic Priorities** (Already Supported)

**Current Structure**:
```csharp
public class ComplaintPriorityMaster : BaseEntity
{
    public string Name { get; set; }                // Custom name
    public string Code { get; set; }                // Custom code
    public int Level { get; set; }                  // Priority level (1-10)
    public int DisplayOrder { get; set; }           // Custom ordering
    public string? ColorCode { get; set; }          // Custom UI color
    public bool IsActive { get; set; }              // Enable/disable
    public bool IsSystem { get; set; }              // System vs custom
    public Guid? CompanyId { get; set; }            // Multi-tenant
}
```

**Capabilities**:
- ✅ Create unlimited custom priorities
- ✅ Company-specific priorities (multi-tenant support)
- ✅ 5 system priorities (Low=0, Normal=1, High=2, Critical=3, Urgent=4)
- ✅ Custom priority levels (1-10 scale)
- ✅ Custom colors, icons, ordering

**Example Use Cases**:
- Company A adds: "P1 - Production Down" (level 10), "P2 - Major Impact" (level 8)
- Company B adds: "VIP Customer" (level 9), "Standard Customer" (level 3)
- Company C uses only 3 priorities: "Low", "Medium", "High"

---

#### 3. **Dynamic Categories** (Already Supported)

**Current Structure**:
```csharp
public class ComplaintCategory : BaseEntity
{
    public string Name { get; set; }                // Category name
    public string Code { get; set; }                // Category code
    public Guid? ParentCategoryId { get; set; }     // Hierarchical structure
    public int DefaultPriority { get; set; }        // Default priority
    public bool IsActive { get; set; }              // Enable/disable
    public int DisplayOrder { get; set; }           // Custom ordering
}
```

**Capabilities**:
- ✅ Create unlimited categories
- ✅ Hierarchical structure (parent-child relationships)
- ✅ Default priority per category
- ✅ Custom ordering

**Example Use Cases**:
- HR Department: Payroll → Salary Issues, Benefits Issues
- IT Department: Hardware → Desktop, Laptop, Network
- Customer Service: Billing → Payment Issues, Invoice Disputes

---

### ❌ What's Missing

#### **Category-Specific Workflow Configuration** (NOT Supported)

**Current Limitation**:
- All complaints follow the same global workflow (9 statuses)
- No way to restrict certain statuses to certain categories
- No category-specific status transitions

**Business Impact**:
- HR complaints might need "Background Check", "Offer Letter" statuses
- IT complaints might need "Awaiting Parts", "Scheduled Maintenance" statuses
- Can't enforce different workflows for different complaint types

---

## Proposed Solution: Category-Specific Workflow Engine

### Design Goals

1. **Flexibility**: Each category can have its own workflow
2. **Backward Compatibility**: Existing workflows continue to work
3. **Global Fallback**: Categories without custom workflow use global workflow
4. **Status Reuse**: Same status can be used in multiple category workflows
5. **Transition Rules**: Define allowed status transitions per category
6. **Multi-Tenant**: Company-specific workflows

---

## Proposed Database Schema

### 1. Category Workflow Configuration

```sql
CREATE TABLE CategoryWorkflows (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CategoryId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    IsActive BIT NOT NULL DEFAULT 1,
    IsDefault BIT NOT NULL DEFAULT 0,
    CompanyId UNIQUEIDENTIFIER,

    -- Audit fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2,
    DeletedBy UNIQUEIDENTIFIER,

    CONSTRAINT FK_CategoryWorkflows_Category
        FOREIGN KEY (CategoryId) REFERENCES ComplaintCategories(Id),
    CONSTRAINT FK_CategoryWorkflows_Company
        FOREIGN KEY (CompanyId) REFERENCES Companies(Id)
);

-- Indexes
CREATE INDEX IX_CategoryWorkflows_CategoryId
    ON CategoryWorkflows(CategoryId) WHERE IsDeleted = 0;
CREATE INDEX IX_CategoryWorkflows_CompanyId
    ON CategoryWorkflows(CompanyId) WHERE IsDeleted = 0;
```

---

### 2. Category Workflow Statuses (Many-to-Many)

```sql
CREATE TABLE CategoryWorkflowStatuses (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WorkflowId UNIQUEIDENTIFIER NOT NULL,
    StatusMasterId UNIQUEIDENTIFIER NOT NULL,
    DisplayOrder INT NOT NULL,
    IsInitialStatus BIT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,

    -- SLA Configuration per status per category
    DefaultSLAHours INT,
    EscalationHours INT,
    RequiresApproval BIT NOT NULL DEFAULT 0,

    -- Audit fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_CategoryWorkflowStatuses_Workflow
        FOREIGN KEY (WorkflowId) REFERENCES CategoryWorkflows(Id),
    CONSTRAINT FK_CategoryWorkflowStatuses_StatusMaster
        FOREIGN KEY (StatusMasterId) REFERENCES ComplaintStatusMasters(Id),
    CONSTRAINT UQ_CategoryWorkflowStatuses_WorkflowStatus
        UNIQUE (WorkflowId, StatusMasterId)
);

-- Indexes
CREATE INDEX IX_CategoryWorkflowStatuses_WorkflowId
    ON CategoryWorkflowStatuses(WorkflowId) WHERE IsDeleted = 0;
CREATE INDEX IX_CategoryWorkflowStatuses_StatusMasterId
    ON CategoryWorkflowStatuses(StatusMasterId) WHERE IsDeleted = 0;
```

---

### 3. Status Transition Rules

```sql
CREATE TABLE CategoryWorkflowTransitions (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    WorkflowId UNIQUEIDENTIFIER NOT NULL,
    FromStatusId UNIQUEIDENTIFIER NOT NULL,
    ToStatusId UNIQUEIDENTIFIER NOT NULL,
    TransitionName NVARCHAR(200),
    RequiresComment BIT NOT NULL DEFAULT 0,
    RequiresApproval BIT NOT NULL DEFAULT 0,
    AllowedRoles NVARCHAR(MAX), -- JSON array of role IDs
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,

    -- Conditions (JSON configuration)
    TransitionConditions NVARCHAR(MAX), -- {"requiresAttachment": true, "minPriority": 2}

    -- Audit fields
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsDeleted BIT NOT NULL DEFAULT 0,

    CONSTRAINT FK_CategoryWorkflowTransitions_Workflow
        FOREIGN KEY (WorkflowId) REFERENCES CategoryWorkflows(Id),
    CONSTRAINT FK_CategoryWorkflowTransitions_FromStatus
        FOREIGN KEY (FromStatusId) REFERENCES ComplaintStatusMasters(Id),
    CONSTRAINT FK_CategoryWorkflowTransitions_ToStatus
        FOREIGN KEY (ToStatusId) REFERENCES ComplaintStatusMasters(Id),
    CONSTRAINT UQ_CategoryWorkflowTransitions_FromToStatus
        UNIQUE (WorkflowId, FromStatusId, ToStatusId)
);

-- Indexes
CREATE INDEX IX_CategoryWorkflowTransitions_WorkflowId
    ON CategoryWorkflowTransitions(WorkflowId) WHERE IsDeleted = 0;
CREATE INDEX IX_CategoryWorkflowTransitions_FromStatus
    ON CategoryWorkflowTransitions(FromStatusId) WHERE IsDeleted = 0;
```

---

### 4. Update ComplaintCategory Table

```sql
-- Add workflow reference to ComplaintCategory
ALTER TABLE ComplaintCategories
ADD WorkflowId UNIQUEIDENTIFIER NULL,
    CONSTRAINT FK_ComplaintCategories_Workflow
        FOREIGN KEY (WorkflowId) REFERENCES CategoryWorkflows(Id);

-- Add index
CREATE INDEX IX_ComplaintCategories_WorkflowId
    ON ComplaintCategories(WorkflowId) WHERE IsDeleted = 0;
```

---

## Domain Entity Classes

### 1. CategoryWorkflow.cs

```csharp
namespace ComplaintManagement.Domain.Entities.Workflows;

/// <summary>
/// Defines a workflow configuration for a specific category
/// </summary>
public class CategoryWorkflow : BaseEntity
{
    /// <summary>
    /// Category this workflow applies to
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Workflow name (e.g., "Standard HR Workflow", "IT Ticket Workflow")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Workflow description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this workflow is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the default workflow for the category
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Company ID (for multi-tenant scenarios)
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    public ComplaintCategory Category { get; set; } = null!;
    public Company? Company { get; set; }
    public ICollection<CategoryWorkflowStatus> WorkflowStatuses { get; set; } = new List<CategoryWorkflowStatus>();
    public ICollection<CategoryWorkflowTransition> Transitions { get; set; } = new List<CategoryWorkflowTransition>();
}
```

---

### 2. CategoryWorkflowStatus.cs

```csharp
namespace ComplaintManagement.Domain.Entities.Workflows;

/// <summary>
/// Associates a status with a category workflow
/// </summary>
public class CategoryWorkflowStatus : BaseEntity
{
    /// <summary>
    /// Workflow this status belongs to
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Status master reference
    /// </summary>
    public Guid StatusMasterId { get; set; }

    /// <summary>
    /// Display order in workflow
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this is the initial status for new complaints
    /// </summary>
    public bool IsInitialStatus { get; set; } = false;

    /// <summary>
    /// Whether this status is active in this workflow
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Default SLA hours for this status in this category
    /// </summary>
    public int? DefaultSLAHours { get; set; }

    /// <summary>
    /// Hours before escalation for this status in this category
    /// </summary>
    public int? EscalationHours { get; set; }

    /// <summary>
    /// Whether approval is required when transitioning to this status
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    // Navigation properties
    public CategoryWorkflow Workflow { get; set; } = null!;
    public ComplaintStatusMaster StatusMaster { get; set; } = null!;
}
```

---

### 3. CategoryWorkflowTransition.cs

```csharp
namespace ComplaintManagement.Domain.Entities.Workflows;

/// <summary>
/// Defines allowed transitions between statuses in a category workflow
/// </summary>
public class CategoryWorkflowTransition : BaseEntity
{
    /// <summary>
    /// Workflow this transition belongs to
    /// </summary>
    public Guid WorkflowId { get; set; }

    /// <summary>
    /// Source status ID
    /// </summary>
    public Guid FromStatusId { get; set; }

    /// <summary>
    /// Target status ID
    /// </summary>
    public Guid ToStatusId { get; set; }

    /// <summary>
    /// Transition name/label (e.g., "Approve", "Reject", "Escalate")
    /// </summary>
    public string? TransitionName { get; set; }

    /// <summary>
    /// Whether comment is required for this transition
    /// </summary>
    public bool RequiresComment { get; set; } = false;

    /// <summary>
    /// Whether approval is required for this transition
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// JSON array of role IDs allowed to perform this transition
    /// </summary>
    public string? AllowedRoles { get; set; }

    /// <summary>
    /// Display order for UI (multiple transitions from same status)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this transition is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// JSON configuration for transition conditions
    /// Example: {"requiresAttachment": true, "minPriority": 2, "maxDaysOpen": 30}
    /// </summary>
    public string? TransitionConditions { get; set; }

    // Navigation properties
    public CategoryWorkflow Workflow { get; set; } = null!;
    public ComplaintStatusMaster FromStatus { get; set; } = null!;
    public ComplaintStatusMaster ToStatus { get; set; } = null!;

    /// <summary>
    /// Get allowed roles as GUID array
    /// </summary>
    public Guid[] GetAllowedRoleIds()
    {
        if (string.IsNullOrWhiteSpace(AllowedRoles))
            return Array.Empty<Guid>();

        return System.Text.Json.JsonSerializer
            .Deserialize<Guid[]>(AllowedRoles) ?? Array.Empty<Guid>();
    }
}
```

---

## Service Layer Implementation

### IWorkflowEngine Interface

```csharp
namespace ComplaintManagement.Application.Interfaces.Services;

public interface IWorkflowEngine
{
    /// <summary>
    /// Get workflow for a specific category
    /// </summary>
    Task<CategoryWorkflow?> GetWorkflowForCategoryAsync(Guid categoryId);

    /// <summary>
    /// Get initial status for a category
    /// </summary>
    Task<ComplaintStatusMaster> GetInitialStatusAsync(Guid categoryId);

    /// <summary>
    /// Get allowed status transitions from current status for a category
    /// </summary>
    Task<List<CategoryWorkflowTransition>> GetAllowedTransitionsAsync(
        Guid categoryId,
        Guid currentStatusId,
        Guid userId);

    /// <summary>
    /// Validate if status transition is allowed
    /// </summary>
    Task<bool> IsTransitionAllowedAsync(
        Guid categoryId,
        Guid fromStatusId,
        Guid toStatusId,
        Guid userId);

    /// <summary>
    /// Get all statuses available in a category workflow
    /// </summary>
    Task<List<ComplaintStatusMaster>> GetWorkflowStatusesAsync(Guid categoryId);

    /// <summary>
    /// Transition complaint to new status (with validation)
    /// </summary>
    Task<bool> TransitionComplaintAsync(
        Guid complaintId,
        Guid newStatusId,
        Guid userId,
        string? comment = null);
}
```

---

### WorkflowEngine Implementation

```csharp
namespace ComplaintManagement.Infrastructure.Services;

public class WorkflowEngine : IWorkflowEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ComplaintDbContext _context;

    public WorkflowEngine(IUnitOfWork unitOfWork, ComplaintDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<CategoryWorkflow?> GetWorkflowForCategoryAsync(Guid categoryId)
    {
        // Get custom workflow for category
        var workflow = await _context.CategoryWorkflows
            .Include(w => w.WorkflowStatuses)
                .ThenInclude(ws => ws.StatusMaster)
            .Include(w => w.Transitions)
                .ThenInclude(t => t.FromStatus)
            .Include(w => w.Transitions)
                .ThenInclude(t => t.ToStatus)
            .Where(w => w.CategoryId == categoryId && w.IsActive && !w.IsDeleted)
            .FirstOrDefaultAsync();

        return workflow;
    }

    public async Task<ComplaintStatusMaster> GetInitialStatusAsync(Guid categoryId)
    {
        // Try to get category-specific initial status
        var workflow = await GetWorkflowForCategoryAsync(categoryId);

        if (workflow != null)
        {
            var initialStatus = workflow.WorkflowStatuses
                .FirstOrDefault(ws => ws.IsInitialStatus && ws.IsActive);

            if (initialStatus != null)
                return initialStatus.StatusMaster;
        }

        // Fallback to global "SUBMITTED" status
        return await _context.ComplaintStatusMasters
            .FirstAsync(s => s.Code == "SUBMITTED" && !s.IsDeleted);
    }

    public async Task<List<CategoryWorkflowTransition>> GetAllowedTransitionsAsync(
        Guid categoryId,
        Guid currentStatusId,
        Guid userId)
    {
        var workflow = await GetWorkflowForCategoryAsync(categoryId);

        if (workflow == null)
        {
            // No custom workflow - allow all non-final statuses
            return new List<CategoryWorkflowTransition>();
        }

        // Get user roles
        var userRoles = await GetUserRoleIdsAsync(userId);

        // Get allowed transitions for current status
        var transitions = workflow.Transitions
            .Where(t => t.FromStatusId == currentStatusId && t.IsActive)
            .Where(t => IsUserAuthorizedForTransition(t, userRoles))
            .OrderBy(t => t.DisplayOrder)
            .ToList();

        return transitions;
    }

    public async Task<bool> IsTransitionAllowedAsync(
        Guid categoryId,
        Guid fromStatusId,
        Guid toStatusId,
        Guid userId)
    {
        var workflow = await GetWorkflowForCategoryAsync(categoryId);

        if (workflow == null)
        {
            // No custom workflow - allow all transitions (backward compatible)
            return true;
        }

        var userRoles = await GetUserRoleIdsAsync(userId);

        var transition = workflow.Transitions
            .FirstOrDefault(t =>
                t.FromStatusId == fromStatusId &&
                t.ToStatusId == toStatusId &&
                t.IsActive);

        if (transition == null)
            return false;

        return IsUserAuthorizedForTransition(transition, userRoles);
    }

    private bool IsUserAuthorizedForTransition(
        CategoryWorkflowTransition transition,
        Guid[] userRoles)
    {
        if (string.IsNullOrWhiteSpace(transition.AllowedRoles))
            return true; // No restriction

        var allowedRoles = transition.GetAllowedRoleIds();
        return allowedRoles.Intersect(userRoles).Any();
    }

    private async Task<Guid[]> GetUserRoleIdsAsync(Guid userId)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId && !ur.IsDeleted)
            .Select(ur => ur.RoleId)
            .ToArrayAsync();
    }
}
```

---

## API Endpoints

### WorkflowController.cs

```csharp
[ApiController]
[Route("api/workflows")]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;

    [HttpGet("categories/{categoryId}/statuses")]
    public async Task<IActionResult> GetCategoryStatuses(Guid categoryId)
    {
        var statuses = await _workflowEngine.GetWorkflowStatusesAsync(categoryId);
        return Ok(new { data = statuses });
    }

    [HttpGet("categories/{categoryId}/transitions")]
    public async Task<IActionResult> GetAllowedTransitions(
        Guid categoryId,
        Guid currentStatusId)
    {
        var userId = GetCurrentUserId();
        var transitions = await _workflowEngine.GetAllowedTransitionsAsync(
            categoryId, currentStatusId, userId);
        return Ok(new { data = transitions });
    }

    [HttpPost("categories/{categoryId}/workflows")]
    public async Task<IActionResult> CreateCategoryWorkflow(
        Guid categoryId,
        CreateCategoryWorkflowRequest request)
    {
        // Implementation
    }

    [HttpPost("workflows/{workflowId}/statuses")]
    public async Task<IActionResult> AddStatusToWorkflow(
        Guid workflowId,
        AddWorkflowStatusRequest request)
    {
        // Implementation
    }

    [HttpPost("workflows/{workflowId}/transitions")]
    public async Task<IActionResult> AddTransitionRule(
        Guid workflowId,
        AddTransitionRuleRequest request)
    {
        // Implementation
    }
}
```

---

## Migration Strategy

### Phase 1: Add New Tables (Zero Downtime)

```bash
# Create migration
dotnet ef migrations add AddCategoryWorkflowTables

# Apply migration
dotnet ef database update
```

**Impact**: None - new tables don't affect existing functionality

---

### Phase 2: Implement Workflow Engine (Backward Compatible)

1. Implement `IWorkflowEngine` service
2. Update `ComplaintsController` to use workflow engine
3. Fallback to global workflow if no category workflow exists

**Impact**: None - backward compatible with existing complaints

---

### Phase 3: Add Workflow Management UI

1. Category workflow configuration page
2. Status assignment interface
3. Transition rule builder

**Impact**: New feature, optional to use

---

### Phase 4: Migrate Existing Workflows (Optional)

Create default workflow for each category based on current global workflow

```sql
-- Example migration script
INSERT INTO CategoryWorkflows (Id, CategoryId, Name, IsDefault, IsActive)
SELECT NEWID(), Id, 'Default Workflow', 1, 1
FROM ComplaintCategories
WHERE IsDeleted = 0;
```

---

## Example Workflow Configurations

### Example 1: HR Complaints

**Category**: "Payroll Issues"

**Statuses**:
1. Submitted (initial)
2. Under Review
3. Verification Required
4. Approved for Payment
5. Payment Processed
6. Closed

**Transitions**:
- Submitted → Under Review (Admin, HR Manager)
- Under Review → Verification Required (HR Manager)
- Under Review → Closed (Rejected)
- Verification Required → Approved for Payment (Finance Approval Required)
- Approved for Payment → Payment Processed (Auto-transition)
- Payment Processed → Closed

---

### Example 2: IT Tickets

**Category**: "Hardware Issues"

**Statuses**:
1. Submitted
2. Assigned to Technician
3. Awaiting Parts
4. Scheduled for Repair
5. In Progress
6. Testing
7. Resolved
8. Closed

**Transitions**:
- Submitted → Assigned to Technician (IT Manager)
- Assigned → In Progress (Technician)
- Assigned → Awaiting Parts (Technician, requires comment)
- Awaiting Parts → Scheduled for Repair (Auto when parts arrive)
- Scheduled → In Progress (Technician)
- In Progress → Testing (QA Team)
- Testing → Resolved (QA Pass)
- Testing → In Progress (QA Fail, requires comment)
- Resolved → Closed (Auto after 24 hours)

---

## UI Mockup: Workflow Configuration

```
Category Workflow Configuration
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Category: Hardware Issues
Workflow: IT Hardware Support Workflow

┌─────────────────────────────────────────┐
│ Workflow Statuses                       │
├─────────────────────────────────────────┤
│ ☑ Submitted (Initial)                   │
│ ☑ Assigned to Technician                │
│ ☑ Awaiting Parts                        │
│ ☑ Scheduled for Repair                  │
│ ☑ In Progress                           │
│ ☑ Testing                               │
│ ☑ Resolved                              │
│ ☑ Closed (Final)                        │
│                                         │
│ [+ Add Status]                          │
└─────────────────────────────────────────┘

┌─────────────────────────────────────────┐
│ Transition Rules                        │
├─────────────────────────────────────────┤
│ From: Submitted                         │
│ To: Assigned to Technician              │
│ Allowed Roles: IT Manager, Admin        │
│ Requires Comment: No                    │
│ Requires Approval: No                   │
│ [Edit] [Delete]                         │
├─────────────────────────────────────────┤
│ From: Assigned to Technician            │
│ To: In Progress                         │
│ Allowed Roles: Technician               │
│ Requires Comment: No                    │
│ [Edit] [Delete]                         │
├─────────────────────────────────────────┤
│ [+ Add Transition Rule]                 │
└─────────────────────────────────────────┘

[Save Workflow] [Cancel]
```

---

## Benefits

### Business Benefits

1. **Flexibility**: Each department can define its own workflow
2. **Compliance**: Enforce required approvals and comments
3. **Control**: Restrict status changes by role
4. **Scalability**: Add unlimited custom statuses and workflows
5. **Multi-Tenant**: Each company can have different workflows

### Technical Benefits

1. **Backward Compatible**: Existing complaints work without changes
2. **Extensible**: Easy to add new workflow features
3. **Testable**: Clear separation of workflow logic
4. **Maintainable**: Workflow rules in database, not code
5. **Auditable**: Track all workflow changes

---

## Recommendations

### Phase 1: Essential (Implement Now)

1. ✅ Create workflow database tables
2. ✅ Implement `IWorkflowEngine` service
3. ✅ Update Complaint API to use workflow engine
4. ✅ Add workflow management endpoints

**Timeline**: 2-3 weeks
**Effort**: Medium
**Risk**: Low (backward compatible)

---

### Phase 2: Enhanced (Next Quarter)

1. Build workflow configuration UI
2. Add visual workflow designer
3. Implement workflow analytics
4. Add workflow templates

**Timeline**: 4-6 weeks
**Effort**: High
**Risk**: Low

---

### Phase 3: Advanced (Future)

1. Workflow automation (auto-transitions)
2. Workflow conditions (if-then rules)
3. Parallel workflows (approval chains)
4. Workflow versioning (change history)

**Timeline**: 6-8 weeks
**Effort**: High
**Risk**: Medium

---

## Impact Analysis

### Current Features Affected

#### ✅ SLA System
- **Impact**: Enhanced
- **Change**: Per-category, per-status SLA configuration
- **Migration**: Add `CategoryWorkflowStatus.DefaultSLAHours`

#### ✅ Escalation System
- **Impact**: Enhanced
- **Change**: Category-aware escalation rules
- **Migration**: Link escalation to workflow transitions

#### ✅ Dashboard/Reports
- **Impact**: Enhanced
- **Change**: Filter by category-specific statuses
- **Migration**: Query workflow statuses instead of global

#### ✅ Complaint Assignment
- **Impact**: No change
- **Change**: Works with any status
- **Migration**: None required

---

## Conclusion

**Current State**:
- ✅ Dynamic Statuses: **Fully Supported**
- ✅ Dynamic Priorities: **Fully Supported**
- ✅ Dynamic Categories: **Fully Supported**
- ❌ Category Workflows: **Not Supported**

**Recommendation**:
Implement the **Category-Specific Workflow Engine** to unlock true enterprise configurability while maintaining full backward compatibility.

**Next Steps**:
1. Review and approve this design
2. Create database migration for workflow tables
3. Implement `IWorkflowEngine` service
4. Update APIs to use workflow engine
5. Build workflow configuration UI

**Expected Outcome**:
A fully configurable complaint management system where each category can have:
- Custom statuses
- Custom priorities
- Custom workflow transitions
- Custom SLA rules
- Role-based access control
- Approval workflows

---

**Document Version**: 1.0
**Created**: November 2, 2025
**Status**: Proposal - Awaiting Approval
