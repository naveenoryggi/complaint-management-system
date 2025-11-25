# Phase 1: Organizational Hierarchy Escalation Model - Implementation Guide

## Overview
This document captures the complete implementation of Phase 1, which redesigns the escalation assignment strategy from a call center model to an organizational hierarchy model.

**Date:** October 2025
**Status:** Backend and Frontend Implementation Complete - Database Migration Pending

---

## Table of Contents
1. [Strategic Goals](#strategic-goals)
2. [Frontend Changes](#frontend-changes)
3. [Backend Changes](#backend-changes)
4. [API Endpoints](#api-endpoints)
5. [Database Schema Changes](#database-schema-changes)
6. [Remaining Tasks](#remaining-tasks)
7. [Testing Strategy](#testing-strategy)

---

## Strategic Goals

### Original Problem
The system used call center-focused assignment strategies:
- Round Robin
- Least Busy
- Specific User
- Role Based
- Group Assignment

### Solution
Replace with organizational hierarchy strategies:
- **Reporting Manager**: Assign to employee's direct manager
- **Section Incharge**: Assign to section in-charge with contact hierarchy fallback
- **Branch Contacts**: Assign to branch-level contacts (Primary → Secondary → HR)
- **Department Contacts**: Assign to department-level contacts (Primary → Secondary → HR)
- **Admin Escalation**: Escalate to admin-level users
- **Resource Pool**: Assign to a resource pool using configured method (Manual/Round Robin/Least Busy)

### Key Features
1. **Dynamic Configuration**: Admins can choose different strategies for L1, L2, L3, etc.
2. **Contact Hierarchy Pattern**: Primary → Secondary → HR fallback system
3. **Resource Pool Flexibility**: Supports Branch/Department/Section-based or Custom pools
4. **Multiple Assignment Methods**: Manual selection, Round Robin, or Least Busy

---

## Frontend Changes

### 1. Models Updated (escalation.model.ts)

**Location:** `complaint-system-angular/src/app/models/escalation.model.ts`

#### New AssignmentStrategy Enum
```typescript
export enum AssignmentStrategy {
  ReportingManager = 0,
  SectionIncharge = 1,
  BranchContacts = 2,
  DepartmentContacts = 3,
  AdminEscalation = 4,
  ResourcePool = 5
}
```

#### Contact Hierarchy Interface
```typescript
export interface ContactHierarchy {
  primaryContactId?: string;
  secondaryContactId?: string;
  hrContactId?: string;
}
```

#### Resource Pool Enums and Interfaces
```typescript
export enum ResourcePoolType {
  Branch = 'Branch',
  Department = 'Department',
  Section = 'Section',
  Custom = 'Custom'
}

export enum ResourcePoolAssignmentMethod {
  Manual = 'Manual',
  RoundRobin = 'RoundRobin',
  LeastBusy = 'LeastBusy'
}

export interface ResourcePool {
  id: string;
  companyId: string;
  name: string;
  description?: string;
  poolType: ResourcePoolType;
  branchId?: string;
  branchName?: string;
  departmentId?: string;
  departmentName?: string;
  sectionId?: string;
  sectionName?: string;
  isActive: boolean;
  memberCount: number;
  members: ResourcePoolMember[];
  createdAt: Date;
  updatedAt?: Date;
}
```

#### Extended EscalationLevel Interface
Added new optional fields:
- `primaryContactId?: string`
- `secondaryContactId?: string`
- `hrContactId?: string`
- `branchId?: string`
- `departmentId?: string`
- `resourcePoolId?: string`
- `resourcePoolAssignmentMethod?: ResourcePoolAssignmentMethod`

### 2. Resource Pool Service

**Location:** `complaint-system-angular/src/app/services/resource-pool.service.ts`

**API Base URL:** `${environment.apiUrl}/escalation/resource-pools`

**Methods:**
- `getAllPools(companyId?: string): Observable<ResourcePool[]>`
- `getPoolById(poolId: string): Observable<ResourcePool>`
- `createPool(request: CreateResourcePoolRequest): Observable<ResourcePool>`
- `updatePool(poolId: string, request: UpdateResourcePoolRequest): Observable<ResourcePool>`
- `deletePool(poolId: string): Observable<void>`
- `addMember(poolId: string, request: AddResourcePoolMemberRequest): Observable<ResourcePool>`
- `removeMember(poolId: string, userId: string): Observable<void>`

### 3. Resource Pool Management Component

**Location:** `complaint-system-angular/src/app/components/admin/resource-pool-management/`

**Features:**
- Full CRUD operations for resource pools
- Pool types: Branch, Department, Section, Custom
- Member management (add/remove users from pools)
- Search and filtering by name, description, or type
- Active/Inactive pool filtering
- Modal-based UI for create/edit/delete/add members
- Displays pool cards with member counts and details

**Key Implementation Details:**
- Uses `poolFormIsActive` helper property to handle TypeScript union type for isActive field
- Loads branches, departments, sections dynamically based on pool type
- Validates required organizational unit fields based on pool type
- Shows confirmation dialogs for destructive operations

**Files:**
- `resource-pool-management.component.ts` (414 lines)
- `resource-pool-management.component.html` (444 lines)
- `resource-pool-management.component.scss`

### 4. Enhanced Escalation Wizard

**Location:** `complaint-system-angular/src/app/components/admin/escalation-wizard/`

**New Features:**
- Dynamic form fields that show/hide based on selected assignment strategy
- Loads users, branches, departments, and resource pools on initialization
- Helper methods to determine which fields to display

**Helper Methods Added:**
```typescript
onAssignmentStrategyChange(levelIndex: number): void
needsContactHierarchy(strategy: AssignmentStrategy): boolean
needsBranchSelector(strategy: AssignmentStrategy): boolean
needsDepartmentSelector(strategy: AssignmentStrategy): boolean
needsResourcePool(strategy: AssignmentStrategy): boolean
getLevelStrategy(levelIndex: number): AssignmentStrategy
```

**Dynamic Form Fields in HTML:**
- Contact Hierarchy fields (Primary, Secondary, HR contacts)
- Branch selector for BranchContacts strategy
- Department selector for DepartmentContacts strategy
- Resource Pool selector with assignment method radio buttons

**Form Controls Added to Each Level:**
```typescript
primaryContactId: [''],
secondaryContactId: [''],
hrContactId: [''],
branchId: [''],
departmentId: [''],
resourcePoolId: [''],
resourcePoolAssignmentMethod: [ResourcePoolAssignmentMethod.Manual]
```

### 5. Escalation Matrix Component Updated

**Location:** `complaint-system-angular/src/app/components/admin/escalation-matrix/`

**Changes:**
- Updated `assignmentStrategies` array to use new enum values
- Changed default strategy from `RoleBased` to `ReportingManager`

---

## Backend Changes

### 1. Domain Enums Updated

#### AssignmentStrategy Enum
**Location:** `ComplaintManagement.Domain/Enums/AssignmentStrategy.cs`

```csharp
public enum AssignmentStrategy
{
    ReportingManager = 0,
    SectionIncharge = 1,
    BranchContacts = 2,
    DepartmentContacts = 3,
    AdminEscalation = 4,
    ResourcePool = 5
}
```

#### New Enums Created
**ResourcePoolType.cs:**
```csharp
public enum ResourcePoolType
{
    Branch = 0,
    Department = 1,
    Section = 2,
    Custom = 3
}
```

**ResourcePoolAssignmentMethod.cs:**
```csharp
public enum ResourcePoolAssignmentMethod
{
    Manual = 0,
    RoundRobin = 1,
    LeastBusy = 2
}
```

### 2. Entity Updates

#### EscalationLevel Entity
**Location:** `ComplaintManagement.Domain/Entities/Escalation/EscalationLevel.cs`

**New Properties Added:**
```csharp
// Contact Hierarchy fields
public Guid? PrimaryContactId { get; set; }
public Guid? SecondaryContactId { get; set; }
public Guid? HrContactId { get; set; }

// Branch/Department fields
public Guid? BranchId { get; set; }
public Guid? DepartmentId { get; set; }

// Resource Pool fields
public Guid? ResourcePoolId { get; set; }
public ResourcePoolAssignmentMethod? ResourcePoolAssignmentMethod { get; set; }
```

**New Navigation Properties:**
```csharp
public User? PrimaryContact { get; set; }
public User? SecondaryContact { get; set; }
public User? HrContact { get; set; }
public Branch? Branch { get; set; }
public Department? Department { get; set; }
public ResourcePool? ResourcePool { get; set; }
```

**Default Value Changed:**
```csharp
public AssignmentStrategy AssignmentStrategy { get; set; } = AssignmentStrategy.ReportingManager;
```

#### New Entities Created

**ResourcePool Entity:**
**Location:** `ComplaintManagement.Domain/Entities/Escalation/ResourcePool.cs`

```csharp
public class ResourcePool : BaseEntity
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ResourcePoolType PoolType { get; set; } = ResourcePoolType.Custom;
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Company Company { get; set; } = null!;
    public Branch? Branch { get; set; }
    public Department? Department { get; set; }
    public Section? Section { get; set; }
    public ICollection<ResourcePoolMember> Members { get; set; } = new List<ResourcePoolMember>();
    public ICollection<EscalationLevel> EscalationLevels { get; set; } = new List<EscalationLevel>();
}
```

**ResourcePoolMember Entity:**
**Location:** `ComplaintManagement.Domain/Entities/Escalation/ResourcePoolMember.cs`

```csharp
public class ResourcePoolMember : BaseEntity
{
    public Guid ResourcePoolId { get; set; }
    public Guid UserId { get; set; }
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    public Guid AddedBy { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ResourcePool ResourcePool { get; set; } = null!;
    public User User { get; set; } = null!;
}
```

### 3. DTOs Updated/Created

#### EscalationLevelDto Updated
**Location:** `ComplaintManagement.Application/DTOs/Escalation/EscalationLevelDto.cs`

**New Properties Added to all DTO classes:**
```csharp
// Contact Hierarchy fields
public Guid? PrimaryContactId { get; set; }
public string? PrimaryContactName { get; set; }
public Guid? SecondaryContactId { get; set; }
public string? SecondaryContactName { get; set; }
public Guid? HrContactId { get; set; }
public string? HrContactName { get; set; }

// Branch/Department fields
public Guid? BranchId { get; set; }
public string? BranchName { get; set; }
public Guid? DepartmentId { get; set; }
public string? DepartmentName { get; set; }

// Resource Pool fields
public Guid? ResourcePoolId { get; set; }
public string? ResourcePoolName { get; set; }
public ResourcePoolAssignmentMethod? ResourcePoolAssignmentMethod { get; set; }
```

#### New ResourcePool DTOs Created
**Location:** `ComplaintManagement.Application/DTOs/Escalation/ResourcePoolDto.cs`

```csharp
public class ResourcePoolDto
public class ResourcePoolMemberDto
public class CreateResourcePoolRequest
public class UpdateResourcePoolRequest
public class AddResourcePoolMemberRequest
```

### 4. Service Layer

#### IResourcePoolService Interface
**Location:** `ComplaintManagement.Application/Interfaces/Services/IResourcePoolService.cs`

**Methods:**
- Pool Management: Create, Update, Delete, GetById, GetByCompany, GetByType
- Member Management: AddMember, RemoveMember, GetMembers, IsMember
- Assignment Logic: GetNextUserFromPool, GetLeastBusyMember, GetNextRoundRobinMember

#### ResourcePoolService Implementation
**Location:** `ComplaintManagement.Infrastructure/Services/ResourcePoolService.cs`

**Key Implementation Details:**
- Uses `ComplaintDbContext` for database operations
- Implements soft delete pattern
- LeastBusy: Queries active complaint counts for each member
- RoundRobin: Tracks last assignment and rotates through members
- Includes proper validation and error handling

### 5. Database Context Updated

**Location:** `ComplaintManagement.Infrastructure/Data/ComplaintDbContext.cs`

**New DbSets Added:**
```csharp
public DbSet<ResourcePool> ResourcePools { get; set; }
public DbSet<ResourcePoolMember> ResourcePoolMembers { get; set; }
```

---

## API Endpoints

### Resource Pool Controller
**Base Route:** `/api/escalation/resource-pools`
**Authorization:** Required
**Permissions:** `escalation.view` for read, `escalation.manage` for write

#### Endpoints:

| Method | Endpoint | Description | Permission |
|--------|----------|-------------|------------|
| GET | `/api/escalation/resource-pools` | Get all pools for company | escalation.view |
| GET | `/api/escalation/resource-pools/{id}` | Get pool by ID | escalation.view |
| POST | `/api/escalation/resource-pools` | Create new pool | escalation.manage |
| PUT | `/api/escalation/resource-pools/{id}` | Update pool | escalation.manage |
| DELETE | `/api/escalation/resource-pools/{id}` | Delete pool | escalation.manage |
| POST | `/api/escalation/resource-pools/{id}/members` | Add member to pool | escalation.manage |
| DELETE | `/api/escalation/resource-pools/{poolId}/members/{userId}` | Remove member | escalation.manage |
| GET | `/api/escalation/resource-pools/{id}/members` | Get pool members | escalation.view |

#### Request/Response Examples:

**Create Pool Request:**
```json
{
  "name": "IT Support Team",
  "description": "Technical support specialists",
  "poolType": "Custom",
  "memberUserIds": ["guid1", "guid2"]
}
```

**Update Pool Request:**
```json
{
  "name": "IT Support Team",
  "description": "Technical support specialists",
  "poolType": "Custom",
  "branchId": null,
  "departmentId": null,
  "sectionId": null,
  "isActive": true
}
```

**Add Member Request:**
```json
{
  "userId": "guid"
}
```

**Response Format:**
```json
{
  "isSuccess": true,
  "message": "Resource pool created successfully",
  "data": { ... }
}
```

---

## Database Schema Changes

### New Tables Required

#### ResourcePools Table
```sql
CREATE TABLE ResourcePools (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    CompanyId UNIQUEIDENTIFIER NOT NULL,
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(500),
    PoolType INT NOT NULL,
    BranchId UNIQUEIDENTIFIER,
    DepartmentId UNIQUEIDENTIFIER,
    SectionId UNIQUEIDENTIFIER,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2,
    DeletedBy UNIQUEIDENTIFIER,
    FOREIGN KEY (CompanyId) REFERENCES Companies(Id),
    FOREIGN KEY (BranchId) REFERENCES Branches(Id),
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id),
    FOREIGN KEY (SectionId) REFERENCES Sections(Id)
);
```

#### ResourcePoolMembers Table
```sql
CREATE TABLE ResourcePoolMembers (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ResourcePoolId UNIQUEIDENTIFIER NOT NULL,
    UserId UNIQUEIDENTIFIER NOT NULL,
    AddedAt DATETIME2 NOT NULL,
    AddedBy UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL,
    CreatedBy UNIQUEIDENTIFIER,
    UpdatedAt DATETIME2,
    UpdatedBy UNIQUEIDENTIFIER,
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedAt DATETIME2,
    DeletedBy UNIQUEIDENTIFIER,
    FOREIGN KEY (ResourcePoolId) REFERENCES ResourcePools(Id),
    FOREIGN KEY (UserId) REFERENCES Users(Id)
);
```

### Modified Tables

#### EscalationLevels Table - New Columns
```sql
ALTER TABLE EscalationLevels ADD PrimaryContactId UNIQUEIDENTIFIER;
ALTER TABLE EscalationLevels ADD SecondaryContactId UNIQUEIDENTIFIER;
ALTER TABLE EscalationLevels ADD HrContactId UNIQUEIDENTIFIER;
ALTER TABLE EscalationLevels ADD BranchId UNIQUEIDENTIFIER;
ALTER TABLE EscalationLevels ADD DepartmentId UNIQUEIDENTIFIER;
ALTER TABLE EscalationLevels ADD ResourcePoolId UNIQUEIDENTIFIER;
ALTER TABLE EscalationLevels ADD ResourcePoolAssignmentMethod INT;

-- Add foreign keys
ALTER TABLE EscalationLevels ADD CONSTRAINT FK_EscalationLevels_PrimaryContact
    FOREIGN KEY (PrimaryContactId) REFERENCES Users(Id);
ALTER TABLE EscalationLevels ADD CONSTRAINT FK_EscalationLevels_SecondaryContact
    FOREIGN KEY (SecondaryContactId) REFERENCES Users(Id);
ALTER TABLE EscalationLevels ADD CONSTRAINT FK_EscalationLevels_HrContact
    FOREIGN KEY (HrContactId) REFERENCES Users(Id);
ALTER TABLE EscalationLevels ADD CONSTRAINT FK_EscalationLevels_Branch
    FOREIGN KEY (BranchId) REFERENCES Branches(Id);
ALTER TABLE EscalationLevels ADD CONSTRAINT FK_EscalationLevels_Department
    FOREIGN KEY (DepartmentId) REFERENCES Departments(Id);
ALTER TABLE EscalationLevels ADD CONSTRAINT FK_EscalationLevels_ResourcePool
    FOREIGN KEY (ResourcePoolId) REFERENCES ResourcePools(Id);
```

### Indexes Recommended
```sql
CREATE INDEX IX_ResourcePools_CompanyId ON ResourcePools(CompanyId);
CREATE INDEX IX_ResourcePools_IsActive ON ResourcePools(IsActive);
CREATE INDEX IX_ResourcePoolMembers_ResourcePoolId ON ResourcePoolMembers(ResourcePoolId);
CREATE INDEX IX_ResourcePoolMembers_UserId ON ResourcePoolMembers(UserId);
CREATE INDEX IX_EscalationLevels_ResourcePoolId ON EscalationLevels(ResourcePoolId);
```

---

## Remaining Tasks

### 1. Database Migration ⏳
**Priority:** HIGH
**Status:** Pending

**Steps:**
1. Navigate to the .NET project directory
2. Create EF Core migration:
   ```bash
   cd "complaint-system-dotnet/src/ComplaintManagement.Infrastructure"
   dotnet ef migrations add Phase1_OrganizationalHierarchy --startup-project ../ComplaintManagement.API
   ```
3. Review the generated migration file
4. Apply migration:
   ```bash
   dotnet ef database update --startup-project ../ComplaintManagement.API
   ```

### 2. Service Registration ⏳
**Priority:** HIGH
**Status:** Needs Verification

Ensure `IResourcePoolService` is registered in DI container:

**Location:** `ComplaintManagement.API/Program.cs` or `Startup.cs`

```csharp
services.AddScoped<IResourcePoolService, ResourcePoolService>();
```

### 3. AutoMapper Configuration ⏳
**Priority:** MEDIUM
**Status:** Pending

Create AutoMapper profiles for ResourcePool entities:

```csharp
CreateMap<ResourcePool, ResourcePoolDto>()
    .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
    .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : null))
    .ForMember(dest => dest.SectionName, opt => opt.MapFrom(src => src.Section != null ? src.Section.Name : null))
    .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count(m => m.IsActive)));

CreateMap<ResourcePoolMember, ResourcePoolMemberDto>()
    .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.FirstName} {src.User.LastName}"))
    .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User.Email));
```

### 4. Update EscalationService ⏳
**Priority:** HIGH
**Status:** Pending

The `DetermineNextHandlerAsync` method in `EscalationService` needs to be updated to handle the new strategies:

```csharp
public async Task<Guid> DetermineNextHandlerAsync(Complaint complaint, EscalationLevel level)
{
    return level.AssignmentStrategy switch
    {
        AssignmentStrategy.ReportingManager => await GetReportingManagerAsync(complaint.CreatedBy),
        AssignmentStrategy.SectionIncharge => await GetSectionInchargeAsync(complaint, level),
        AssignmentStrategy.BranchContacts => await GetBranchContactAsync(complaint, level),
        AssignmentStrategy.DepartmentContacts => await GetDepartmentContactAsync(complaint, level),
        AssignmentStrategy.AdminEscalation => await GetAdminUserAsync(complaint.CompanyId),
        AssignmentStrategy.ResourcePool => await GetResourcePoolAssignmentAsync(level),
        _ => throw new NotImplementedException($"Assignment strategy {level.AssignmentStrategy} not implemented")
    };
}

private async Task<Guid> GetResourcePoolAssignmentAsync(EscalationLevel level)
{
    if (level.ResourcePoolId == null)
        throw new InvalidOperationException("ResourcePoolId is required for ResourcePool strategy");

    var method = level.ResourcePoolAssignmentMethod?.ToString() ?? "Manual";
    return await _resourcePoolService.GetNextUserFromPoolAsync(level.ResourcePoolId.Value, method);
}

private async Task<Guid> GetSectionInchargeAsync(Complaint complaint, EscalationLevel level)
{
    // Try primary contact first
    if (level.PrimaryContactId.HasValue)
        return level.PrimaryContactId.Value;

    // Fallback to secondary contact
    if (level.SecondaryContactId.HasValue)
        return level.SecondaryContactId.Value;

    // Final fallback to HR contact
    if (level.HrContactId.HasValue)
        return level.HrContactId.Value;

    throw new InvalidOperationException("No valid contact found for SectionIncharge strategy");
}

// Similar implementations for GetBranchContactAsync and GetDepartmentContactAsync
```

### 5. Entity Framework Configuration ⏳
**Priority:** MEDIUM
**Status:** Pending

Create entity configurations for the new entities:

**ResourcePoolConfiguration.cs:**
```csharp
public class ResourcePoolConfiguration : IEntityTypeConfiguration<ResourcePool>
{
    public void Configure(EntityTypeBuilder<ResourcePool> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasOne(p => p.Company)
            .WithMany()
            .HasForeignKey(p => p.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Branch)
            .WithMany()
            .HasForeignKey(p => p.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Department)
            .WithMany()
            .HasForeignKey(p => p.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Section)
            .WithMany()
            .HasForeignKey(p => p.SectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Members)
            .WithOne(m => m.ResourcePool)
            .HasForeignKey(m => m.ResourcePoolId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**ResourcePoolMemberConfiguration.cs:**
```csharp
public class ResourcePoolMemberConfiguration : IEntityTypeConfiguration<ResourcePoolMember>
{
    public void Configure(EntityTypeBuilder<ResourcePoolMember> builder)
    {
        builder.HasKey(m => m.Id);

        builder.HasOne(m => m.ResourcePool)
            .WithMany(p => p.Members)
            .HasForeignKey(m => m.ResourcePoolId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.User)
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate members
        builder.HasIndex(m => new { m.ResourcePoolId, m.UserId })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
```

**Update EscalationLevelConfiguration.cs:**
```csharp
// Add new navigation property configurations
builder.HasOne(l => l.PrimaryContact)
    .WithMany()
    .HasForeignKey(l => l.PrimaryContactId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(l => l.SecondaryContact)
    .WithMany()
    .HasForeignKey(l => l.SecondaryContactId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(l => l.HrContact)
    .WithMany()
    .HasForeignKey(l => l.HrContactId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(l => l.Branch)
    .WithMany()
    .HasForeignKey(l => l.BranchId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(l => l.Department)
    .WithMany()
    .HasForeignKey(l => l.DepartmentId)
    .OnDelete(DeleteBehavior.Restrict);

builder.HasOne(l => l.ResourcePool)
    .WithMany(p => p.EscalationLevels)
    .HasForeignKey(l => l.ResourcePoolId)
    .OnDelete(DeleteBehavior.Restrict);
```

---

## Testing Strategy

### Unit Tests Needed

#### 1. ResourcePoolService Tests
```csharp
[TestClass]
public class ResourcePoolServiceTests
{
    [TestMethod]
    public async Task CreatePoolAsync_ValidRequest_CreatesPool()

    [TestMethod]
    public async Task AddMemberAsync_NewMember_AddsMember()

    [TestMethod]
    public async Task AddMemberAsync_ExistingInactiveMember_ReactivatesMember()

    [TestMethod]
    public async Task GetLeastBusyMemberAsync_ReturnsUserWithFewestComplaints()

    [TestMethod]
    public async Task GetNextRoundRobinMemberAsync_RotatesThroughMembers()
}
```

#### 2. EscalationService Tests
```csharp
[TestClass]
public class EscalationServiceTests
{
    [TestMethod]
    public async Task DetermineNextHandlerAsync_ReportingManager_ReturnsManager()

    [TestMethod]
    public async Task DetermineNextHandlerAsync_SectionIncharge_UsesPrimaryContact()

    [TestMethod]
    public async Task DetermineNextHandlerAsync_SectionIncharge_FallsBackToSecondary()

    [TestMethod]
    public async Task DetermineNextHandlerAsync_ResourcePool_AssignsFromPool()
}
```

### Integration Tests Needed

#### 1. ResourcePoolController Integration Tests
- Test GET all pools returns correct data
- Test POST creates pool and members
- Test PUT updates pool correctly
- Test DELETE soft deletes pool
- Test member add/remove operations

#### 2. End-to-End Escalation Tests
- Create a complaint
- Configure escalation matrix with different strategies
- Trigger escalation
- Verify correct user assignment for each strategy

### Manual Testing Checklist

#### Resource Pool Management
- [ ] Create a Custom pool
- [ ] Create a Branch-based pool
- [ ] Add members to pool
- [ ] Remove members from pool
- [ ] Search pools by name
- [ ] Filter active/inactive pools
- [ ] Edit pool details
- [ ] Delete pool

#### Escalation Wizard
- [ ] Create matrix with ReportingManager strategy
- [ ] Create matrix with SectionIncharge strategy
  - [ ] Verify Primary/Secondary/HR contact fields appear
- [ ] Create matrix with BranchContacts strategy
  - [ ] Verify Branch selector appears
  - [ ] Verify contact hierarchy fields appear
- [ ] Create matrix with DepartmentContacts strategy
  - [ ] Verify Department selector appears
  - [ ] Verify contact hierarchy fields appear
- [ ] Create matrix with ResourcePool strategy
  - [ ] Verify Resource Pool selector appears
  - [ ] Verify Assignment Method radio buttons appear
- [ ] Save and verify all fields persist correctly

#### Complaint Escalation
- [ ] Submit complaint
- [ ] Trigger manual escalation
- [ ] Verify ReportingManager assignment works
- [ ] Verify SectionIncharge with contact hierarchy works
- [ ] Verify BranchContacts assignment works
- [ ] Verify DepartmentContacts assignment works
- [ ] Verify AdminEscalation assignment works
- [ ] Verify ResourcePool Manual assignment works
- [ ] Verify ResourcePool RoundRobin assignment works
- [ ] Verify ResourcePool LeastBusy assignment works

---

## File Locations Summary

### Frontend Files Modified/Created:
```
complaint-system-angular/
├── src/app/
│   ├── models/
│   │   └── escalation.model.ts (MODIFIED - Added new enums and interfaces)
│   ├── services/
│   │   └── resource-pool.service.ts (NEW)
│   └── components/admin/
│       ├── resource-pool-management/
│       │   ├── resource-pool-management.component.ts (NEW)
│       │   ├── resource-pool-management.component.html (NEW)
│       │   └── resource-pool-management.component.scss (NEW)
│       ├── escalation-wizard/
│       │   ├── escalation-wizard.component.ts (MODIFIED)
│       │   └── escalation-wizard.component.html (MODIFIED)
│       └── escalation-matrix/
│           └── escalation-matrix.component.ts (MODIFIED)
```

### Backend Files Modified/Created:
```
complaint-system-dotnet/
├── src/ComplaintManagement.Domain/
│   ├── Enums/
│   │   ├── AssignmentStrategy.cs (MODIFIED)
│   │   ├── ResourcePoolType.cs (NEW)
│   │   └── ResourcePoolAssignmentMethod.cs (NEW)
│   └── Entities/Escalation/
│       ├── EscalationLevel.cs (MODIFIED)
│       ├── ResourcePool.cs (NEW)
│       └── ResourcePoolMember.cs (NEW)
├── src/ComplaintManagement.Application/
│   ├── DTOs/Escalation/
│   │   ├── EscalationLevelDto.cs (MODIFIED)
│   │   └── ResourcePoolDto.cs (NEW)
│   └── Interfaces/Services/
│       └── IResourcePoolService.cs (NEW)
├── src/ComplaintManagement.Infrastructure/
│   ├── Data/
│   │   └── ComplaintDbContext.cs (MODIFIED)
│   └── Services/
│       └── ResourcePoolService.cs (NEW)
└── src/ComplaintManagement.API/
    └── Controllers/
        └── ResourcePoolController.cs (NEW)
```

---

## Deployment Checklist

### Pre-Deployment:
- [ ] Run all unit tests
- [ ] Run integration tests
- [ ] Create and review database migration
- [ ] Update API documentation (Swagger)
- [ ] Update user documentation
- [ ] Backup production database

### Deployment Steps:
1. [ ] Deploy backend changes
2. [ ] Run database migrations
3. [ ] Verify API endpoints
4. [ ] Deploy frontend changes
5. [ ] Verify frontend connectivity
6. [ ] Test critical paths in production

### Post-Deployment:
- [ ] Monitor application logs
- [ ] Verify no errors in console
- [ ] Test escalation functionality
- [ ] Verify resource pool operations
- [ ] Gather user feedback

---

## Key Design Decisions

### 1. Contact Hierarchy Pattern
**Decision:** Use Primary → Secondary → HR fallback system
**Rationale:** Provides flexibility and ensures escalations don't fail if primary contact is unavailable

### 2. Resource Pool Flexibility
**Decision:** Support both organizational structure-based and custom pools
**Rationale:** Some teams may not align with org structure, need flexibility

### 3. Backward Compatibility
**Decision:** Keep legacy assignment fields (AssignToUserId, AssignToRole, etc.)
**Rationale:** Smooth migration path, existing configurations continue to work

### 4. Dynamic Form Rendering
**Decision:** Show/hide fields based on assignment strategy selection
**Rationale:** Reduces clutter, only show relevant fields for each strategy

### 5. Three Assignment Methods for Pools
**Decision:** Manual, Round Robin, Least Busy
**Rationale:** Different teams have different workflows, provide options

---

## Known Limitations & Future Enhancements

### Current Limitations:
1. Manual pool assignment requires user to pick from available members
2. Round robin doesn't persist last assignment index (recalculates each time)
3. No workload balancing across resource pools
4. No time-based availability for pool members

### Future Enhancements (Phase 2+):
1. **Advanced Pool Assignment:**
   - Skill-based routing
   - Time zone awareness
   - Availability calendars
   - Workload balancing across multiple pools

2. **Contact Hierarchy Improvements:**
   - Auto-escalate if contact doesn't respond within timeframe
   - Out-of-office detection
   - Delegate assignment

3. **Analytics & Reporting:**
   - Pool utilization metrics
   - Average response time by pool
   - Member performance tracking
   - Escalation pattern analysis

4. **Smart Assignment:**
   - ML-based assignment recommendations
   - Historical performance data
   - Complaint type matching to specialist pools

---

## Support & Maintenance

### Common Issues & Solutions:

#### Issue: "Resource pool not found"
**Solution:** Verify pool is active and belongs to the user's company

#### Issue: "No active members in pool"
**Solution:** Ensure pool has at least one active member before using ResourcePool strategy

#### Issue: "Contact hierarchy not working"
**Solution:** Verify at least one of Primary/Secondary/HR contact is configured

#### Issue: "Database migration fails"
**Solution:** Check for existing columns, may need to adjust migration script

### Monitoring:
- Monitor escalation assignment failures
- Track pool member workload distribution
- Alert on pools with no active members
- Track average escalation resolution time by strategy

---

## Version History

| Version | Date | Description |
|---------|------|-------------|
| 1.0 | Oct 2025 | Phase 1 Implementation Complete - Backend and Frontend |
| 0.9 | Oct 2025 | Frontend implementation completed |
| 0.5 | Oct 2025 | Backend entities and services created |
| 0.1 | Oct 2025 | Planning and design phase |

---

## Contact & Contributors

**Implementation Team:**
- Backend Development: .NET Core 6+
- Frontend Development: Angular 18
- Database: SQL Server with Entity Framework Core
- Architecture: Clean Architecture / CQRS pattern

**Documentation Maintained By:** Development Team
**Last Updated:** October 2025

---

## Appendix

### A. Glossary
- **Resource Pool:** Group of users that can handle escalated complaints
- **Contact Hierarchy:** Fallback system (Primary → Secondary → HR)
- **Assignment Strategy:** Method for determining who handles an escalation
- **Pool Type:** Organizational structure alignment (Branch/Dept/Section/Custom)
- **Assignment Method:** How work is distributed within a pool (Manual/RoundRobin/LeastBusy)

### B. Related Documentation
- API Documentation: `/api/swagger`
- Entity Relationship Diagrams: `/docs/ERD.md`
- User Guide: `/docs/UserGuide.md`
- Architecture Decision Records: `/docs/ADR/`

---

**END OF PHASE 1 IMPLEMENTATION GUIDE**
