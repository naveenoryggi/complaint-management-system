# Organizational Structure - Practical Implementation Sequence

## Overview
This document outlines the step-by-step implementation approach for the organizational structure and escalation management system.

---

## Implementation Flow

### Step 1: Setup Master Data (Admin Configuration)
**Goal**: Allow administrators to define the organizational structure

```
Admin configures:
1. Company (already exists) ✓
2. Branches (e.g., "Dubai Office", "London Office")
3. Departments (e.g., "IT", "HR", "Finance")
4. Sections (e.g., "Development Team", "Support Team")
5. Employee Types (e.g., "Permanent", "Contract", "Intern", "Consultant")
```

**Admin UI Location**: `Admin > Organizational Structure > [Branch/Department/Section Management]`

---

### Step 2: Map Employees to Organizational Units
**Goal**: Assign each employee/user to their respective organizational unit

```
For each Employee/User, assign:
- Company: Acme Inc. ✓ (already assigned)
- Branch: Dubai Office
- Department: IT Department
- Section: Development Team
- Employee Type: Permanent
- Manager: John Doe (their direct supervisor)
```

**Admin UI Location**: `Admin > User Management > Edit User > Organizational Assignment`

---

### Step 3: Create Escalation Policies at Organizational Levels
**Goal**: Define escalation rules at different organizational levels

```
Escalation Policy Examples:

Policy 1: Company-wide Default
  - Scope: Company-wide
  - When: All complaints (fallback)
  - Matrix: Standard Escalation Matrix

Policy 2: IT Department Policy
  - Scope: IT Department Only
  - When: Complaints from IT department users
  - Matrix: IT-specific Escalation Matrix
  - Priority: Higher than company-wide

Policy 3: Dubai Branch Policy
  - Scope: Dubai Branch Only
  - When: Complaints from Dubai office
  - Matrix: Dubai-specific Escalation Matrix
  - Priority: Higher than company-wide

Policy 4: IT Department + Bug Category
  - Scope: IT Department + Bug Category
  - When: Bug complaints from IT department
  - Matrix: Bug-specific Escalation Matrix
  - Priority: HIGHEST (most specific)
```

**Admin UI Location**: `Admin > Escalation Policies > Create Policy`

---

### Step 4: Define Escalation Levels with User Assignment
**Goal**: For each escalation matrix, assign specific users at each level

```
Example: IT Department Escalation Matrix

Level 1: First Response (Target: 4 hours)
  ├─ Assigned Users:
  │  ├─ Ahmed Ali (IT Support Agent)
  │  ├─ Sara Khan (IT Support Agent)
  │  └─ Mohammed Rashid (IT Support Agent)
  └─ Auto-assign: Round-robin between these users

Level 2: Supervisor Review (Target: 24 hours)
  ├─ Assigned Users:
  │  ├─ Fatima Hassan (IT Supervisor)
  │  └─ Ali Ahmed (IT Team Lead)
  └─ Auto-assign: First available

Level 3: Department Head (Target: 48 hours)
  ├─ Assigned Users:
  │  └─ John Smith (IT Department Head)
  └─ Auto-assign: Department head

Level 4: Branch Manager (Target: 72 hours)
  ├─ Assigned Users:
  │  └─ Jane Williams (Dubai Branch Manager)
  └─ Auto-assign: Branch manager
```

**Admin UI Location**: `Admin > Escalation Matrix > Edit Matrix > Assign Users to Levels`

---

## Detailed Data Model

### 1. Employee Type Entity (NEW - To Be Created)

```csharp
public class EmployeeType : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; }          // "Permanent", "Contract", "Intern"
    public string Code { get; set; }          // "PERM", "CONT", "INTN"
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation
    public Company Company { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Employee> Employees { get; set; }
}
```

**Purpose**: Categorize employees (Permanent, Contract, Intern, etc.)
**Usage**:
- Reporting by employee type
- Different escalation policies by employee type
- Access control based on employee type

---

### 2. Enhanced User Entity (UPDATE)

```csharp
public class User : BaseEntity
{
    // Organizational Mappings
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? EmployeeTypeId { get; set; }  // NEW

    // ... other fields ...

    // Navigation
    public Company Company { get; set; }
    public Branch? Branch { get; set; }
    public Department? Department { get; set; }
    public Section? Section { get; set; }
    public EmployeeType? EmployeeType { get; set; }  // NEW
}
```

---

### 3. Escalation Level User Assignment (NEW - Key Component!)

```csharp
/// <summary>
/// Maps users to specific escalation levels within an escalation matrix
/// This allows selecting which users handle complaints at each escalation level
/// </summary>
public class EscalationLevelUserAssignment : BaseEntity
{
    /// <summary>
    /// Escalation matrix level ID (foreign key)
    /// </summary>
    public Guid EscalationMatrixLevelId { get; set; }

    /// <summary>
    /// User assigned to this escalation level
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Is this user active for this level
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority order for assignment (lower = higher priority)
    /// Used for round-robin or priority-based assignment
    /// </summary>
    public int AssignmentPriority { get; set; } = 0;

    /// <summary>
    /// Maximum concurrent complaints this user can handle at this level
    /// 0 = unlimited
    /// </summary>
    public int MaxConcurrentComplaints { get; set; } = 0;

    /// <summary>
    /// Optional: Specific organizational scope
    /// If set, this user only handles complaints from this branch
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Optional: Department scope
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Optional: Section scope
    /// </summary>
    public Guid? SectionId { get; set; }

    // Navigation properties
    public EscalationMatrixLevel EscalationMatrixLevel { get; set; } = null!;
    public User User { get; set; } = null!;
    public Branch? Branch { get; set; }
    public Department? Department { get; set; }
    public Section? Section { get; set; }
}
```

**Purpose**: This is the CRITICAL entity that links users to escalation levels!

**Example**:
```json
{
  "escalationMatrixLevelId": "Level1-FirstResponse-Id",
  "userId": "Ahmed-Ali-Id",
  "isActive": true,
  "assignmentPriority": 1,
  "maxConcurrentComplaints": 10,
  "branchId": "Dubai-Office-Id",  // Only handle Dubai complaints
  "departmentId": "IT-Dept-Id"     // Only handle IT complaints
}
```

---

### 4. Enhanced Escalation Matrix Level (UPDATE)

```csharp
public class EscalationMatrixLevel : BaseEntity
{
    public Guid EscalationMatrixId { get; set; }
    public int LevelNumber { get; set; }
    public string LevelName { get; set; }
    public int EscalationTimeInHours { get; set; }
    public bool RequiresApproval { get; set; }

    // Assignment strategy
    public AssignmentStrategy AssignmentStrategy { get; set; } = AssignmentStrategy.RoundRobin;

    // Navigation
    public EscalationMatrix EscalationMatrix { get; set; }
    public ICollection<EscalationLevelUserAssignment> UserAssignments { get; set; }  // NEW
}

public enum AssignmentStrategy
{
    RoundRobin,        // Distribute evenly among assigned users
    FirstAvailable,    // Assign to first user with capacity
    PriorityBased,     // Use AssignmentPriority field
    LeastLoad,         // Assign to user with least active complaints
    Manual             // Require manual assignment
}
```

---

## Admin Workflow Examples

### Example 1: Setting Up IT Department Escalation

**Step 1**: Admin creates "IT Department" in Department Master
```
Name: IT Department
Code: IT
Branch: Dubai Office
Manager: John Smith
```

**Step 2**: Admin assigns users to IT Department
```
- Ahmed Ali → IT Department, Section: Support
- Sara Khan → IT Department, Section: Support
- Mohammed Rashid → IT Department, Section: Support
- Fatima Hassan → IT Department (Supervisor)
- John Smith → IT Department (Head)
```

**Step 3**: Admin creates "IT Escalation Matrix"
```
Matrix Name: IT Department Escalation Matrix
Description: For all IT-related complaints

Level 1: First Response (4 hours)
  - Assignment Strategy: Round Robin
  - Assigned Users:
    ✓ Ahmed Ali (Priority: 1)
    ✓ Sara Khan (Priority: 2)
    ✓ Mohammed Rashid (Priority: 3)

Level 2: Supervisor Review (24 hours)
  - Assignment Strategy: First Available
  - Assigned Users:
    ✓ Fatima Hassan

Level 3: Department Head (48 hours)
  - Assignment Strategy: Manual
  - Assigned Users:
    ✓ John Smith
```

**Step 4**: Admin creates "IT Department Policy"
```
Policy Name: IT Department Escalation Policy
Scope: Department
Department: IT Department
Escalation Matrix: IT Department Escalation Matrix
Priority: 10 (higher than company-wide)
```

---

### Example 2: Complaint Flow with Organizational Structure

**Scenario**: User submits IT complaint

```
1. Complaint Submitted:
   - Submitted By: Employee (Dubai Office, IT Dept, Dev Section)
   - Category: Bug Report
   - Description: "Application crashing"

2. System Determines Organizational Context:
   - Company: Acme Inc
   - Branch: Dubai Office
   - Department: IT Department
   - Section: Development Section
   - Category: Bug Report

3. System Resolves Escalation Policy:
   Available Policies:
   ✓ IT Dept + Bug Category (Score: 5) ← SELECTED
   - IT Department Only (Score: 3)
   - Dubai Branch (Score: 2)
   - Company-wide (Score: 1)

   Selected: "IT Dept + Bug Category" policy

4. System Assigns Based on Matrix Level 1:
   Matrix: "IT Bug Escalation Matrix"
   Level 1: First Response
   Strategy: Round Robin
   Available Users: Ahmed Ali, Sara Khan, Mohammed Rashid

   Current Round Robin State: Last assigned = Sara
   Next Assignment: Mohammed Rashid ← ASSIGNED

5. Complaint Now:
   - Status: Open
   - Assigned To: Mohammed Rashid
   - Current Level: Level 1 (First Response)
   - SLA: 4 hours from now
   - Escalation Path: Mohammed → Fatima → John Smith → Branch Manager
```

---

## API Endpoints - Implementation Priority

### Priority 1: Master Data Management

```http
### Branch Management
GET    /api/branches?companyId={id}
GET    /api/branches/{id}
POST   /api/branches
PUT    /api/branches/{id}
DELETE /api/branches/{id}

### Department Management
GET    /api/departments?branchId={id}
GET    /api/departments/{id}
POST   /api/departments
PUT    /api/departments/{id}
DELETE /api/departments/{id}

### Section Management
GET    /api/sections?departmentId={id}
GET    /api/sections/{id}
POST   /api/sections
PUT    /api/sections/{id}
DELETE /api/sections/{id}

### Employee Type Management (NEW)
GET    /api/employee-types?companyId={id}
GET    /api/employee-types/{id}
POST   /api/employee-types
PUT    /api/employee-types/{id}
DELETE /api/employee-types/{id}
```

### Priority 2: User Assignment

```http
### Update user organizational assignment
PUT /api/users/{id}/organizational-assignment
{
  "branchId": "guid",
  "departmentId": "guid",
  "sectionId": "guid",
  "employeeTypeId": "guid",
  "managerId": "guid"
}

### Get users by organizational unit
GET /api/users/by-branch/{branchId}
GET /api/users/by-department/{departmentId}
GET /api/users/by-section/{sectionId}
```

### Priority 3: Escalation Level User Assignment (KEY!)

```http
### Get users assigned to escalation level
GET /api/escalation-matrix-levels/{levelId}/users

### Assign user to escalation level
POST /api/escalation-matrix-levels/{levelId}/users
{
  "userId": "guid",
  "assignmentPriority": 1,
  "maxConcurrentComplaints": 10,
  "branchId": "guid",      // optional scope
  "departmentId": "guid"   // optional scope
}

### Update user assignment
PUT /api/escalation-matrix-levels/{levelId}/users/{userId}

### Remove user from escalation level
DELETE /api/escalation-matrix-levels/{levelId}/users/{userId}

### Get available users for assignment (filtered by org unit)
GET /api/escalation-matrix-levels/{levelId}/available-users?branchId={}&departmentId={}
```

---

## Frontend Components - Implementation Priority

### Priority 1: Master Data Management UI

```
1. Branch Management Component
   - List view with CRUD operations
   - Similar to Category Management component

2. Department Management Component
   - Grouped by Branch
   - Select Branch → Show Departments

3. Section Management Component
   - Grouped by Department
   - Select Branch → Department → Sections

4. Employee Type Management Component
   - Simple CRUD like Category Management
```

### Priority 2: User Assignment UI

```
5. Enhanced User Form
   - Add dropdowns for:
     * Branch selection
     * Department selection (filtered by branch)
     * Section selection (filtered by department)
     * Employee Type selection
     * Manager selection (from same dept/section)
```

### Priority 3: Escalation User Assignment UI (CRITICAL!)

```
6. Escalation Matrix Level User Assignment Component

   UI Flow:
   a) Admin edits Escalation Matrix
   b) For each Level, click "Assign Users"
   c) Modal opens:
      - Search/Filter users by:
        * Branch
        * Department
        * Section
        * Role
      - Multi-select users
      - Set assignment priority for each
      - Set max concurrent complaints
      - Set optional org scope
   d) Save assignments
   e) View assigned users in table
```

---

## Database Migration Script

```sql
-- 1. Create EmployeeType table
CREATE TABLE EmployeeTypes (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    CompanyId UNIQUEIDENTIFIER NOT NULL FOREIGN KEY REFERENCES Companies(Id),
    Name NVARCHAR(100) NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT NOT NULL DEFAULT 1,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(100),
    CONSTRAINT UK_EmployeeTypes_Code UNIQUE (CompanyId, Code)
);

-- 2. Add EmployeeTypeId to Users table
ALTER TABLE Users
ADD EmployeeTypeId UNIQUEIDENTIFIER NULL
    FOREIGN KEY REFERENCES EmployeeTypes(Id);

-- 3. Create EscalationLevelUserAssignments table
CREATE TABLE EscalationLevelUserAssignments (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    EscalationMatrixLevelId UNIQUEIDENTIFIER NOT NULL
        FOREIGN KEY REFERENCES EscalationMatrixLevels(Id) ON DELETE CASCADE,
    UserId UNIQUEIDENTIFIER NOT NULL
        FOREIGN KEY REFERENCES Users(Id) ON DELETE CASCADE,
    IsActive BIT NOT NULL DEFAULT 1,
    AssignmentPriority INT NOT NULL DEFAULT 0,
    MaxConcurrentComplaints INT NOT NULL DEFAULT 0,
    BranchId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Branches(Id),
    DepartmentId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Departments(Id),
    SectionId UNIQUEIDENTIFIER NULL FOREIGN KEY REFERENCES Sections(Id),
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CreatedBy NVARCHAR(100),
    UpdatedAt DATETIME2,
    UpdatedBy NVARCHAR(100),
    CONSTRAINT UK_EscalationLevelUser UNIQUE (EscalationMatrixLevelId, UserId)
);

-- 4. Create indexes
CREATE INDEX IX_Users_EmployeeTypeId ON Users(EmployeeTypeId);
CREATE INDEX IX_Users_OrgUnits ON Users(CompanyId, BranchId, DepartmentId, SectionId);
CREATE INDEX IX_EscalationUserAssignments_Level ON EscalationLevelUserAssignments(EscalationMatrixLevelId);
CREATE INDEX IX_EscalationUserAssignments_User ON EscalationLevelUserAssignments(UserId);
CREATE INDEX IX_EscalationUserAssignments_OrgUnits
    ON EscalationLevelUserAssignments(BranchId, DepartmentId, SectionId);
```

---

## Implementation Checklist

### Phase 1: Backend - Master Data (Week 1)
- [ ] Create EmployeeType entity
- [ ] Create EmployeeType repository
- [ ] Create EmployeeType service
- [ ] Create EmployeeType controller/API
- [ ] Create Branch repository & service (if not exists)
- [ ] Create Department repository & service (if not exists)
- [ ] Create Section repository & service (if not exists)
- [ ] Write unit tests

### Phase 2: Backend - User Mapping (Week 1-2)
- [ ] Update User entity with EmployeeTypeId
- [ ] Create migration scripts
- [ ] Update User service for org assignment
- [ ] Create API endpoints for user org assignment
- [ ] Add validation rules
- [ ] Write unit tests

### Phase 3: Backend - Escalation User Assignment (Week 2)
- [ ] Create EscalationLevelUserAssignment entity
- [ ] Create repository
- [ ] Create service with assignment logic
- [ ] Create API endpoints
- [ ] Implement assignment strategies (Round Robin, etc.)
- [ ] Update complaint assignment logic to use user assignments
- [ ] Write unit tests

### Phase 4: Frontend - Master Data UI (Week 3)
- [ ] Create Branch management component
- [ ] Create Department management component
- [ ] Create Section management component
- [ ] Create Employee Type management component
- [ ] Add navigation menu items
- [ ] Add validation and error handling

### Phase 5: Frontend - User Assignment UI (Week 3)
- [ ] Update User form with org unit dropdowns
- [ ] Add cascading filters (Branch → Dept → Section)
- [ ] Add Employee Type selection
- [ ] Add Manager selection (filtered by org)
- [ ] Update user list to show org information

### Phase 6: Frontend - Escalation User Assignment UI (Week 4)
- [ ] Create Escalation Level User Assignment component
- [ ] Add user search/filter by org units
- [ ] Implement multi-select with priorities
- [ ] Add assignment strategy configuration
- [ ] Show current assignments in matrix view
- [ ] Add drag-drop for priority ordering

### Phase 7: Testing & Refinement (Week 4-5)
- [ ] Integration testing
- [ ] Test complaint routing with org structure
- [ ] Test escalation with user assignments
- [ ] Performance testing
- [ ] User acceptance testing
- [ ] Bug fixes and refinements

---

## Success Criteria

✅ **Phase 1 Complete When:**
- Admin can create/edit Branches, Departments, Sections, Employee Types
- Changes are saved to database
- Active/inactive filtering works

✅ **Phase 2 Complete When:**
- Admin can assign users to organizational units
- Cascading dropdowns work correctly
- Users show their organizational assignment in list

✅ **Phase 3 Complete When:**
- Admin can assign users to escalation levels
- Assignment strategies work (Round Robin, etc.)
- Complaints are auto-assigned to correct users
- Escalation flows correctly through assigned users

---

**Document Version**: 1.0
**Last Updated**: 2025-10-12
**Status**: Ready for Implementation
