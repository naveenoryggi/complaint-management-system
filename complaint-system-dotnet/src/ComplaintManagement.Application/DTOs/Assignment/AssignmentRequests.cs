using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Assignment;

/// <summary>
/// Request model for auto-assigning a complaint
/// </summary>
public class AutoAssignComplaintRequest
{
    /// <summary>
    /// Whether to force assignment even if no perfect match is found
    /// </summary>
    public bool ForceAssignment { get; set; } = false;

    /// <summary>
    /// Assignment criteria to use (overrides default)
    /// </summary>
    public AssignmentCriteriaRequest? Criteria { get; set; }

    /// <summary>
    /// Reason for the assignment
    /// </summary>
    public string? AssignmentReason { get; set; }

    /// <summary>
    /// Whether to bypass assignment rules
    /// </summary>
    public bool BypassRules { get; set; } = false;

    /// <summary>
    /// Override assignment method
    /// </summary>
    public string? OverrideAssignmentMethod { get; set; }

    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for assigning a complaint to a resource pool
/// </summary>
public class AssignToPoolRequest
{
    /// <summary>
    /// The resource pool to assign from
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// The assignment method to use
    /// </summary>
    public AdvancedAssignmentMethod? AssignmentMethod { get; set; }

    /// <summary>
    /// Specific user to assign within the pool (for Manual assignment)
    /// </summary>
    public Guid? SpecificUserId { get; set; }

    /// <summary>
    /// Whether to validate the assignment before executing
    /// </summary>
    public bool? ValidateBeforeAssignment { get; set; } = true;

    /// <summary>
    /// Reason for the assignment
    /// </summary>
    public string? AssignmentReason { get; set; }

    /// <summary>
    /// Whether this assignment is part of an escalation
    /// </summary>
    public bool? IsEscalation { get; set; } = false;

    /// <summary>
    /// Whether to bypass certain assignment rules
    /// </summary>
    public bool? BypassRules { get; set; } = false;

    /// <summary>
    /// Whether to force assignment even if user is at capacity
    /// </summary>
    public bool? ForceAssignment { get; set; } = false;

    /// <summary>
    /// Optional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for reassigning a complaint
/// </summary>
public class ReassignComplaintRequest
{
    /// <summary>
    /// The current assigned user (for validation)
    /// </summary>
    public Guid? CurrentAssignedUserId { get; set; }

    /// <summary>
    /// The new user to assign to
    /// </summary>
    public Guid? NewAssignedUserId { get; set; }

    /// <summary>
    /// The new resource pool to assign from
    /// </summary>
    public Guid? NewResourcePoolId { get; set; }

    /// <summary>
    /// Reason for reassignment
    /// </summary>
    public string ReassignmentReason { get; set; } = string.Empty;

    /// <summary>
    /// Assignment method to use for the new assignment
    /// </summary>
    public AdvancedAssignmentMethod? AssignmentMethod { get; set; }

    /// <summary>
    /// Whether to keep the original assignment in history
    /// </summary>
    public bool? KeepAssignmentHistory { get; set; } = true;

    /// <summary>
    /// Whether to bypass assignment rules
    /// </summary>
    public bool? BypassRules { get; set; } = false;

    /// <summary>
    /// Optional notes about the reassignment
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request model for validating an assignment
/// </summary>
public class ValidateAssignmentRequest
{
    /// <summary>
    /// The user to validate assignment to
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// The resource pool to validate assignment from
    /// </summary>
    public Guid? PoolId { get; set; }

    /// <summary>
    /// Whether this is an escalation assignment
    /// </summary>
    public bool? IsEscalation { get; set; } = false;

    /// <summary>
    /// Whether to force assignment even if validation shows issues
    /// </summary>
    public bool? ForceAssignment { get; set; } = false;

    /// <summary>
    /// Whether to perform detailed validation
    /// </summary>
    public bool? DetailedValidation { get; set; } = true;
}

/// <summary>
/// Request model for assignment criteria
/// </summary>
public class AssignmentCriteriaRequest
{
    // Organizational scope
    public List<Guid>? CompanyIds { get; set; }
    public List<Guid>? BranchIds { get; set; }
    public List<Guid>? DepartmentIds { get; set; }
    public List<Guid>? SectionIds { get; set; }

    // Complaint criteria
    public List<Guid>? CategoryIds { get; set; }
    public int? MinPriorityLevel { get; set; } = 0;
    public int? MaxPriorityLevel { get; set; } = 4;
    public int? MinEscalationLevel { get; set; } = 1;
    public int? MaxEscalationLevel { get; set; } = 5;

    // Workload criteria
    public int? MaxCurrentWorkload { get; set; } = 10;
    public decimal? MinSuccessRate { get; set; } = 0.7m;
    public TimeSpan? MaxAverageResolutionTime { get; set; }
    public int? RequireRecentActivityDays { get; set; } = 7;
    public bool? IncludeOnLeave { get; set; } = false;

    // Skill criteria
    public List<string>? RequiredSkills { get; set; }
    public int? MinSkillLevel { get; set; } = 1;
    public List<string>? PreferredSkills { get; set; }
    public double? SkillWeight { get; set; } = 0.7;

    // Temporal criteria
    public bool? BusinessHoursOnly { get; set; } = false;
    public List<int>? AllowedDaysOfWeek { get; set; }
    public string? AllowedTimeStart { get; set; } // HH:mm format
    public string? AllowedTimeEnd { get; set; } // HH:mm format
    public bool? ConsiderUserTimeZone { get; set; } = true;

    // Result limits
    public int? MaxCandidates { get; set; } = 10;
}