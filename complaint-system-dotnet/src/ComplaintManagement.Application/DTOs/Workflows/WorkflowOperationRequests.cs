using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Workflows;

/// <summary>
/// Request DTO for transitioning a complaint to a new status
/// </summary>
public class TransitionComplaintRequest
{
    [Required(ErrorMessage = "Complaint ID is required")]
    public Guid ComplaintId { get; set; }

    [Required(ErrorMessage = "New Status ID is required")]
    public Guid NewStatusId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }

    [MaxLength(2000, ErrorMessage = "Comment cannot exceed 2000 characters")]
    public string? Comment { get; set; }
}

/// <summary>
/// Request DTO for getting allowed transitions for a complaint
/// </summary>
public class GetAllowedTransitionsRequest
{
    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Current Status ID is required")]
    public Guid CurrentStatusId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }
}

/// <summary>
/// Request DTO for checking if a transition is allowed
/// </summary>
public class CheckTransitionAllowedRequest
{
    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "From Status ID is required")]
    public Guid FromStatusId { get; set; }

    [Required(ErrorMessage = "To Status ID is required")]
    public Guid ToStatusId { get; set; }

    [Required(ErrorMessage = "User ID is required")]
    public Guid UserId { get; set; }
}

/// <summary>
/// Response DTO for allowed transitions
/// </summary>
public class AllowedTransitionsResponse
{
    public List<CategoryWorkflowTransitionDto> Transitions { get; set; } = new();
    public int Count { get; set; }
}

/// <summary>
/// Response DTO for transition validation
/// </summary>
public class TransitionValidationResponse
{
    public bool IsAllowed { get; set; }
    public string? Reason { get; set; }
    public bool RequiresComment { get; set; }
    public bool RequiresApproval { get; set; }
}
