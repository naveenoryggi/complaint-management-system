using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Defines specialization parameters for a resource pool
/// Configures which types of complaints and priority levels the pool can handle
/// </summary>
public class ResourcePoolSpecialization
{
    public Guid Id { get; set; }

    /// <summary>
    /// The resource pool this specialization belongs to
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// The complaint category this specialization applies to
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Minimum priority level this pool can handle (0-4)
    /// 0=Low, 1=Normal, 2=High, 3=Critical, 4=Urgent
    /// </summary>
    public int MinPriorityLevel { get; set; } = 0;

    /// <summary>
    /// Maximum priority level this pool can handle (0-4)
    /// </summary>
    public int MaxPriorityLevel { get; set; } = 4;

    /// <summary>
    /// Minimum escalation level this pool can handle (1-5)
    /// </summary>
    public int MinEscalationLevel { get; set; } = 1;

    /// <summary>
    /// Maximum escalation level this pool can handle (1-5)
    /// </summary>
    public int MaxEscalationLevel { get; set; } = 5;

    /// <summary>
    /// Maximum number of concurrent complaints this pool can handle
    /// </summary>
    public int MaxConcurrentComplaints { get; set; } = 10;

    /// <summary>
    /// Assignment weight/priority for this specialization
    /// Higher values get preference in assignment decisions
    /// </summary>
    public decimal Weight { get; set; } = 1.0m;

    /// <summary>
    /// Whether this specialization is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this specialization was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this specialization was deactivated (if applicable)
    /// </summary>
    public DateTime? DeactivatedAt { get; set; }

    /// <summary>
    /// Who deactivated this specialization (audit trail)
    /// </summary>
    public Guid? DeactivatedBy { get; set; }

    /// <summary>
    /// Reason for deactivation
    /// </summary>
    public string? DeactivationReason { get; set; }

    /// <summary>
    /// Optional notes about this specialization
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    public ResourcePool ResourcePool { get; set; } = null!;
    public ComplaintCategory Category { get; set; } = null!;

    /// <summary>
    /// Check if this specialization can handle a specific priority level
    /// </summary>
    public bool CanHandlePriority(int priority)
    {
        return priority >= MinPriorityLevel && priority <= MaxPriorityLevel;
    }

    /// <summary>
    /// Check if this specialization can handle a specific escalation level
    /// </summary>
    public bool CanHandleEscalationLevel(int escalationLevel)
    {
        return escalationLevel >= MinEscalationLevel && escalationLevel <= MaxEscalationLevel;
    }

    /// <summary>
    /// Check if this specialization is currently active
    /// </summary>
    public bool IsCurrentlyActive()
    {
        return IsActive && !DeactivatedAt.HasValue;
    }
}