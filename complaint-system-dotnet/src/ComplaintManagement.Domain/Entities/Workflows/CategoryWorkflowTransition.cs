using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Workflows;

/// <summary>
/// Defines allowed transitions between statuses in a category workflow
/// Provides fine-grained control over complaint status changes
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
    /// Used in UI as button label
    /// </summary>
    public string? TransitionName { get; set; }

    /// <summary>
    /// Transition description
    /// </summary>
    public string? Description { get; set; }

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
    /// Null means all roles can perform this transition
    /// </summary>
    public string? AllowedRoles { get; set; }

    /// <summary>
    /// Display order for UI (when multiple transitions from same status)
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Whether this transition is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is an automatic transition (no user action required)
    /// Example: Auto-close after 24 hours in "Resolved" status
    /// </summary>
    public bool IsAutomatic { get; set; } = false;

    /// <summary>
    /// For automatic transitions: hours to wait before auto-transition
    /// </summary>
    public int? AutoTransitionAfterHours { get; set; }

    /// <summary>
    /// JSON configuration for transition conditions
    /// Example: {"requiresAttachment": true, "minPriority": 2, "maxDaysOpen": 30}
    /// </summary>
    public string? TransitionConditions { get; set; }

    /// <summary>
    /// Button/action color for UI (e.g., "primary", "success", "danger", "warning")
    /// </summary>
    public string? ButtonColor { get; set; }

    /// <summary>
    /// Icon class for UI (e.g., "bi-check-circle", "bi-x-circle")
    /// </summary>
    public string? IconClass { get; set; }

    // Navigation properties
    /// <summary>
    /// Workflow this transition belongs to
    /// </summary>
    public CategoryWorkflow Workflow { get; set; } = null!;

    /// <summary>
    /// Source status
    /// </summary>
    public ComplaintStatusMaster FromStatus { get; set; } = null!;

    /// <summary>
    /// Target status
    /// </summary>
    public ComplaintStatusMaster ToStatus { get; set; } = null!;

    /// <summary>
    /// Get allowed roles as GUID array
    /// </summary>
    public Guid[] GetAllowedRoleIds()
    {
        if (string.IsNullOrWhiteSpace(AllowedRoles))
            return Array.Empty<Guid>();

        try
        {
            return System.Text.Json.JsonSerializer
                .Deserialize<Guid[]>(AllowedRoles) ?? Array.Empty<Guid>();
        }
        catch
        {
            return Array.Empty<Guid>();
        }
    }

    /// <summary>
    /// Set allowed roles from GUID array
    /// </summary>
    public void SetAllowedRoleIds(Guid[] roleIds)
    {
        if (roleIds == null || roleIds.Length == 0)
        {
            AllowedRoles = null;
            return;
        }

        AllowedRoles = System.Text.Json.JsonSerializer.Serialize(roleIds);
    }

    /// <summary>
    /// Get transition conditions as dictionary
    /// </summary>
    public Dictionary<string, object>? GetConditions()
    {
        if (string.IsNullOrWhiteSpace(TransitionConditions))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer
                .Deserialize<Dictionary<string, object>>(TransitionConditions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Set transition conditions from dictionary
    /// </summary>
    public void SetConditions(Dictionary<string, object>? conditions)
    {
        if (conditions == null || conditions.Count == 0)
        {
            TransitionConditions = null;
            return;
        }

        TransitionConditions = System.Text.Json.JsonSerializer.Serialize(conditions);
    }
}
