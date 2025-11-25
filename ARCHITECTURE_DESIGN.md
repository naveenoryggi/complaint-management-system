# Advanced Resource Pool Assignment Architecture

## Overview
Designing a highly configurable, multi-dimensional resource pool assignment system that handles:
- Organizational hierarchy (Company → Branch → Department → Section)
- Issue type and priority specialization
- Workload balancing and skill-based routing
- Dynamic escalation paths

## Core Architecture Components

### 1. Enhanced Resource Pool Entities

#### ResourcePoolSpecialization
```csharp
public class ResourcePoolSpecialization
{
    public Guid Id { get; set; }
    public Guid ResourcePoolId { get; set; }
    public Guid CategoryId { get; set; }  // Issue type specialization
    public int MinPriorityLevel { get; set; } = 0;  // Minimum priority level
    public int MaxPriorityLevel { get; set; } = 4;  // Maximum priority level
    public int MinEscalationLevel { get; set; } = 1; // Minimum escalation level
    public int MaxEscalationLevel { get; set; } = 5; // Maximum escalation level
    public bool IsActive { get; set; } = true;
    public int MaxConcurrentComplaints { get; set; } = 10; // Workload limit
    public decimal Weight { get; set; } = 1.0m; // Assignment weight/priority
    public DateTime CreatedAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }

    // Navigation
    public ResourcePool ResourcePool { get; set; }
    public ComplaintCategory Category { get; set; }
}
```

#### ResourcePoolMemberSkills
```csharp
public class ResourcePoolMemberSkills
{
    public Guid Id { get; set; }
    public Guid ResourcePoolId { get; set; }
    public Guid UserId { get; set; }
    public string SkillCode { get; set; }  // Standardized skill codes
    public int SkillLevel { get; set; } = 1; // 1-5 proficiency
    public DateTime CertifiedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ResourcePool ResourcePool { get; set; }
    public User User { get; set; }
}
```

#### ResourcePoolWorkloadTracker
```csharp
public class ResourcePoolWorkloadTracker
{
    public Guid Id { get; set; }
    public Guid ResourcePoolId { get; set; }
    public Guid UserId { get; set; }
    public int CurrentActiveComplaints { get; set; } = 0;
    public int TotalAssigned { get; set; } = 0;
    public int TotalResolved { get; set; } = 0;
    public TimeSpan AverageResolutionTime { get; set; }
    public decimal SuccessRate { get; set; } = 1.0m;
    public DateTime LastAssignedAt { get; set; }
    public DateTime LastActivityAt { get; set; }

    // Navigation
    public ResourcePool ResourcePool { get; set; }
    public User User { get; set; }
}
```

### 2. Assignment Configuration System

#### AssignmentRule
```csharp
public class AssignmentRule
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string RuleName { get; set; }
    public int Priority { get; set; } = 1; // Rule execution order
    public bool IsActive { get; set; } = true;

    // Rule Conditions (JSON stored)
    public string ConditionsJson { get; set; } // Flexible condition structure

    // Assignment Action
    public AssignmentActionType ActionType { get; set; }
    public string ActionParametersJson { get; set; } // Action configuration

    public DateTime CreatedAt { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public Guid? ModifiedBy { get; set; }

    // Navigation
    public Company Company { get; set; }
}
```

#### Assignment Conditions (JSON Schema)
```json
{
  "conditions": {
    "organizational": {
      "companyIds": ["guid1", "guid2"],
      "branchIds": ["guid1"],
      "departmentIds": [],
      "sectionIds": ["guid1"]
    },
    "complaint": {
      "categoryIds": ["hr", "technical"],
      "priorityRange": { "min": 2, "max": 4 },
      "escalationLevel": { "min": 1, "max": 3 }
    },
    "temporal": {
      "businessHoursOnly": true,
      "daysOfWeek": [1, 2, 3, 4, 5],
      "timeRange": { "start": "09:00", "end": "18:00" }
    },
    "workload": {
      "maxCurrentWorkload": 5,
      "minSuccessRate": 0.8
    }
  }
}
```

### 3. Assignment Engine Architecture

#### IAssignmentEngine Interface
```csharp
public interface IAssignmentEngine
{
    Task<AssignmentResult> AssignComplaintAsync(Guid complaintId, AssignmentContext context);
    Task<List<ResourcePoolCandidate>> FindCandidatePoolsAsync(AssignmentCriteria criteria);
    Task<User> SelectUserFromPoolAsync(Guid poolId, AssignmentMethod method, AssignmentContext context);
    Task<bool> ValidateAssignmentAsync(Guid complaintId, Guid userId, AssignmentContext context);
}
```

#### AssignmentEngine Implementation
```csharp
public class AssignmentEngine : IAssignmentEngine
{
    private readonly IAssignmentRuleService _ruleService;
    private readonly IResourcePoolService _poolService;
    private readonly IWorkloadTrackerService _workloadService;
    private readonly IUserService _userService;

    public async Task<AssignmentResult> AssignComplaintAsync(Guid complaintId, AssignmentContext context)
    {
        // 1. Load complaint details
        // 2. Execute assignment rules in priority order
        // 3. Find matching resource pools
        // 4. Select best candidate based on workload/skills
        // 5. Update workload trackers
        // 6. Return assignment result
    }

    public async Task<List<ResourcePoolCandidate>> FindCandidatePoolsAsync(AssignmentCriteria criteria)
    {
        // 1. Filter pools by organizational hierarchy
        // 2. Filter by category/priority specialization
        // 3. Filter by current workload capacity
        // 4. Rank by suitability score
        // 5. Return ordered list of candidates
    }
}
```

### 4. Enhanced Assignment Methods

#### Advanced Assignment Methods
```csharp
public enum AssignmentMethod
{
    Manual = 0,
    RoundRobin = 1,
    LeastBusy = 2,
    SkillBased = 3,        // New: Assign to most skilled user
    PriorityBased = 4,     // New: Consider priority levels
    WorkloadBalanced = 5,  // New: Advanced load balancing
    BestFit = 6           // New: Algorithm-based best match
}

public class SkillBasedAssignmentContext
{
    public string RequiredSkill { get; set; }
    public int MinSkillLevel { get; set; } = 1;
    public Dictionary<string, int> SkillWeights { get; set; } = new();
    public bool ConsiderWorkload { get; set; } = true;
    public double WorkloadWeight { get; set; } = 0.3; // 30% workload, 70% skills
}
```

### 5. API Endpoints Design

#### Assignment Management
```
POST /api/complaints/{id}/assign-to-pool
POST /api/complaints/{id}/auto-assign
POST /api/complaints/{id}/reassign
GET  /api/assignment/candidates/{complaintId}
POST /api/assignment/validate
GET  /api/assignment/rules
POST /api/assignment/rules
PUT  /api/assignment/rules/{id}
```

#### Resource Pool Specialization
```
GET  /api/escalation/resource-pools/{id}/specializations
POST /api/escalation/resource-pools/{id}/specializations
PUT  /api/escalation/resource-pools/{id}/specializations/{specId}
DELETE /api/escalation/resource-pools/{id}/specializations/{specId}
GET  /api/escalation/resource-pools/{id}/workload
GET  /api/escalation/resource-pools/{id}/skills
```

### 6. Frontend Enhancement Design

#### Advanced Assignment Interface
```typescript
interface AssignmentConfig {
  organizationalScope: {
    company?: string;
    branch?: string;
    department?: string;
    section?: string;
  };
  specialization: {
    categories: string[];
    priorityRange: [number, number];
    escalationLevels: [number, number];
  };
  workload: {
    maxConcurrent: number;
    assignmentMethod: AssignmentMethod;
  };
  skills: {
    required: string[];
    preferred: string[];
  };
}
```

#### Assignment Configuration UI
- **Rule Builder**: Visual rule creation with condition builders
- **Pool Dashboard**: Workload monitoring and performance metrics
- **Skill Management**: Member skill tracking and certification
- **Assignment Analytics**: Performance tracking and optimization suggestions

### 7. Database Schema Updates

#### New Tables Required
1. **ResourcePoolSpecializations**
2. **ResourcePoolMemberSkills**
3. **ResourcePoolWorkloadTrackers**
4. **AssignmentRules**
5. **AssignmentHistory** (audit trail)

#### Enhanced Indexes
- Composite indexes on (ResourcePoolId, CategoryId, PriorityRange)
- Workload tracking indexes for performance
- Full-text search on skill codes and descriptions

### 8. Implementation Phases

#### Phase 1: Foundation (Current Sprint)
- [ ] Enhanced ResourcePool entity with specialization
- [ ] Basic assignment rule engine
- [ ] Skill-based assignment method
- [ ] API endpoints for pool management

#### Phase 2: Advanced Features (Next Sprint)
- [ ] Workload tracking and balancing
- [ ] Advanced assignment algorithms
- [ ] Rule builder UI
- [ ] Performance analytics

#### Phase 3: Intelligence (Future)
- [ ] Machine learning for assignment optimization
- [ ] Predictive workload management
- [ ] Advanced reporting and insights
- [ ] Automated rule optimization

### 9. Configuration Examples

#### Example Assignment Rules
```json
{
  "rules": [
    {
      "name": "HR Issues Priority Routing",
      "conditions": {
        "categoryIds": ["hr-policy", "employee-relations"],
        "priorityRange": {"min": 3, "max": 4}
      },
      "action": {
        "type": "AssignToPool",
        "poolId": "hr-priority-pool",
        "method": "SkillBased"
      }
    },
    {
      "name": "Technical Support Overflow",
      "conditions": {
        "categoryIds": ["technical"],
        "workload": {"minActiveComplaints": 8}
      },
      "action": {
        "type": "EscalateToPool",
        "poolId": "l2-technical-pool"
      }
    }
  ]
}
```

This architecture provides a highly configurable, scalable system that can handle complex organizational structures while maintaining optimal resource utilization and response times.