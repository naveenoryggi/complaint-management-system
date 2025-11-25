namespace ComplaintManagement.Application.DTOs.Escalation;

/// <summary>
/// DTO for Escalation Policy
/// </summary>
public class EscalationPolicyDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Scope
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }

    // Configuration
    public bool EnableAutoEscalation { get; set; }
    public bool RequireManualApproval { get; set; }
    public Guid? DefaultEscalationMatrixId { get; set; }
    public string? DefaultEscalationMatrixName { get; set; }
    public int? MinimumSeverityForAutoEscalation { get; set; }
    public int? MaxAutoEscalationLevels { get; set; }

    // Priority
    public int Priority { get; set; }
    public int SpecificityScore { get; set; }
    public bool IsActive { get; set; }

    // Effective dates
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    // Computed properties
    public string ScopeDescription { get; set; } = string.Empty;
    public bool IsCurrentlyEffective { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating an escalation policy
/// </summary>
public class CreateEscalationPolicyRequest
{
    public Guid CompanyId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Scope (null = applies to all)
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? CategoryId { get; set; }

    // Configuration
    public bool EnableAutoEscalation { get; set; } = true;
    public bool RequireManualApproval { get; set; } = false;
    public Guid? DefaultEscalationMatrixId { get; set; }
    public int? MinimumSeverityForAutoEscalation { get; set; }
    public int? MaxAutoEscalationLevels { get; set; }

    // Priority
    public int Priority { get; set; } = 0;

    // Effective dates
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

/// <summary>
/// Request DTO for updating an escalation policy
/// </summary>
public class UpdateEscalationPolicyRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Configuration
    public bool EnableAutoEscalation { get; set; }
    public bool RequireManualApproval { get; set; }
    public Guid? DefaultEscalationMatrixId { get; set; }
    public int? MinimumSeverityForAutoEscalation { get; set; }
    public int? MaxAutoEscalationLevels { get; set; }

    // Priority
    public int Priority { get; set; }
    public bool IsActive { get; set; }

    // Effective dates
    public DateTime? EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
}

/// <summary>
/// Request DTO for testing policy resolution
/// </summary>
public class PolicyResolutionTestRequest
{
    public Guid CompanyId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Result DTO for policy resolution test
/// </summary>
public class PolicyResolutionResultDto
{
    public EscalationPolicyDto? ResolvedPolicy { get; set; }
    public List<EscalationPolicyDto> AllMatchingPolicies { get; set; } = new();
    public string ResolutionExplanation { get; set; } = string.Empty;
}
