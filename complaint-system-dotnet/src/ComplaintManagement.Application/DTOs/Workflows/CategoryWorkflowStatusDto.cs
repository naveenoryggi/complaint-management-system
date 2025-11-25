using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Workflows;

/// <summary>
/// DTO for Category Workflow Status
/// </summary>
public class CategoryWorkflowStatusDto
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid StatusMasterId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusCode { get; set; } = string.Empty;
    public string? StatusColorCode { get; set; }
    public string? StatusIconClass { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsInitialStatus { get; set; }
    public bool IsActive { get; set; }
    public int? DefaultSLAHours { get; set; }
    public int? EscalationHours { get; set; }
    public bool RequiresApproval { get; set; }
    public Guid[]? AllowedRoles { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Request DTO for adding a status to a workflow
/// </summary>
public class AddStatusToWorkflowRequest
{
    [Required(ErrorMessage = "Workflow ID is required")]
    public Guid WorkflowId { get; set; }

    [Required(ErrorMessage = "Status Master ID is required")]
    public Guid StatusMasterId { get; set; }

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; } = 0;

    public bool IsInitialStatus { get; set; } = false;

    [Range(1, 8760, ErrorMessage = "Default SLA hours must be between 1 and 8760 (1 year)")]
    public int? DefaultSLAHours { get; set; }

    [Range(1, 8760, ErrorMessage = "Escalation hours must be between 1 and 8760 (1 year)")]
    public int? EscalationHours { get; set; }

    public bool RequiresApproval { get; set; } = false;
    public Guid[]? AllowedRoles { get; set; }
}

/// <summary>
/// Request DTO for updating a workflow status
/// </summary>
public class UpdateWorkflowStatusRequest
{
    [Required(ErrorMessage = "Workflow Status ID is required")]
    public Guid Id { get; set; }

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; }

    public bool IsInitialStatus { get; set; }
    public bool IsActive { get; set; }

    [Range(1, 8760, ErrorMessage = "Default SLA hours must be between 1 and 8760 (1 year)")]
    public int? DefaultSLAHours { get; set; }

    [Range(1, 8760, ErrorMessage = "Escalation hours must be between 1 and 8760 (1 year)")]
    public int? EscalationHours { get; set; }

    public bool RequiresApproval { get; set; }
    public Guid[]? AllowedRoles { get; set; }
}
