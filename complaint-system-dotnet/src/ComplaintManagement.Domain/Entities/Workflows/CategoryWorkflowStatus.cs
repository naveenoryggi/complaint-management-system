using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Workflows;

/// <summary>
/// Associates a status with a category workflow
/// Defines which statuses are available for a specific category's workflow
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
    /// Whether this is the initial status for new complaints in this workflow
    /// </summary>
    public bool IsInitialStatus { get; set; } = false;

    /// <summary>
    /// Whether this status is active in this workflow
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Default SLA hours for this status in this category
    /// Overrides the global SLA if set
    /// </summary>
    public int? DefaultSLAHours { get; set; }

    /// <summary>
    /// Hours before escalation for this status in this category
    /// </summary>
    public int? EscalationHours { get; set; }

    /// <summary>
    /// Whether approval is required when transitioning TO this status
    /// </summary>
    public bool RequiresApproval { get; set; } = false;

    /// <summary>
    /// Allowed roles that can assign this status (JSON array of role IDs)
    /// Null means all roles can use this status
    /// </summary>
    public string? AllowedRoles { get; set; }

    // Navigation properties
    /// <summary>
    /// Workflow this status belongs to
    /// </summary>
    public CategoryWorkflow Workflow { get; set; } = null!;

    /// <summary>
    /// Status master definition
    /// </summary>
    public ComplaintStatusMaster StatusMaster { get; set; } = null!;

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
}
