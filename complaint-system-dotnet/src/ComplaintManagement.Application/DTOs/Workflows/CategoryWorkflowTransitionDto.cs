using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Workflows;

/// <summary>
/// DTO for Category Workflow Transition
/// </summary>
public class CategoryWorkflowTransitionDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid FromStatusId { get; set; }
    public string FromStatusName { get; set; } = string.Empty;
    public string FromStatusCode { get; set; } = string.Empty;
    public Guid ToStatusId { get; set; }
    public string ToStatusName { get; set; } = string.Empty;
    public string ToStatusCode { get; set; } = string.Empty;
    public string? TransitionName { get; set; }
    public string? Description { get; set; }
    public bool RequiresComment { get; set; }
    public bool RequiresApproval { get; set; }
    public Guid[]? AllowedRoles { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsAutomatic { get; set; }
    public int? AutoTransitionAfterHours { get; set; }
    public Dictionary<string, object>? TransitionConditions { get; set; }
    public string? ButtonColor { get; set; }
    public string? IconClass { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for adding a transition rule to a workflow
/// </summary>
public class AddTransitionRuleRequest
{
    [Required(ErrorMessage = "Workflow ID is required")]
    public Guid WorkflowId { get; set; }

    [Required(ErrorMessage = "From Status ID is required")]
    public Guid FromStatusId { get; set; }

    [Required(ErrorMessage = "To Status ID is required")]
    public Guid ToStatusId { get; set; }

    [MaxLength(200, ErrorMessage = "Transition name cannot exceed 200 characters")]
    public string? TransitionName { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool RequiresComment { get; set; } = false;
    public bool RequiresApproval { get; set; } = false;
    public Guid[]? AllowedRoles { get; set; }

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;
    public bool IsAutomatic { get; set; } = false;

    [Range(1, 8760, ErrorMessage = "Auto transition hours must be between 1 and 8760 (1 year)")]
    public int? AutoTransitionAfterHours { get; set; }

    public Dictionary<string, object>? TransitionConditions { get; set; }

    [MaxLength(50, ErrorMessage = "Button color cannot exceed 50 characters")]
    public string? ButtonColor { get; set; }

    [MaxLength(100, ErrorMessage = "Icon class cannot exceed 100 characters")]
    public string? IconClass { get; set; }
}

/// <summary>
/// Request DTO for updating a transition rule
/// </summary>
public class UpdateTransitionRuleRequest
{
    [Required(ErrorMessage = "Transition ID is required")]
    public Guid Id { get; set; }

    [MaxLength(200, ErrorMessage = "Transition name cannot exceed 200 characters")]
    public string? TransitionName { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool RequiresComment { get; set; }
    public bool RequiresApproval { get; set; }
    public Guid[]? AllowedRoles { get; set; }

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }
    public bool IsAutomatic { get; set; }

    [Range(1, 8760, ErrorMessage = "Auto transition hours must be between 1 and 8760 (1 year)")]
    public int? AutoTransitionAfterHours { get; set; }

    public Dictionary<string, object>? TransitionConditions { get; set; }

    [MaxLength(50, ErrorMessage = "Button color cannot exceed 50 characters")]
    public string? ButtonColor { get; set; }

    [MaxLength(100, ErrorMessage = "Icon class cannot exceed 100 characters")]
    public string? IconClass { get; set; }
}
