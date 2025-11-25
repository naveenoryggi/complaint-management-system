using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Represents a single level in an escalation matrix
/// Defines when and to whom a complaint should be escalated at this level
/// </summary>
public class EscalationLevel : BaseEntity
{
    /// <summary>
    /// Escalation matrix ID (foreign key)
    /// </summary>
    public Guid EscalationMatrixId { get; set; }

    /// <summary>
    /// Level number (1-5, where 1 is first escalation, 5 is highest)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Level name (e.g., "Supervisor", "Manager", "Director")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of this escalation level
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Hours after SLA breach to trigger this escalation level
    /// For auto-escalation: 0 = immediate, 24 = 1 day after breach, etc.
    /// </summary>
    public int TriggerAfterHours { get; set; } = 0;

    /// <summary>
    /// Time value for escalation trigger (use with TriggerTimeUnit)
    /// More flexible than TriggerAfterHours - allows days, weeks, etc.
    /// </summary>
    public int TriggerAfterValue { get; set; } = 0;

    /// <summary>
    /// Time unit for TriggerAfterValue (Minutes, Hours, Days, Weeks)
    /// </summary>
    public TimeUnit TriggerTimeUnit { get; set; } = TimeUnit.Hours;

    /// <summary>
    /// Assignment strategy for this level
    /// </summary>
    public AssignmentStrategy AssignmentStrategy { get; set; } = AssignmentStrategy.ReportingManager;

    /// <summary>
    /// Specific user ID to assign to (legacy - used with SpecificUser strategy)
    /// </summary>
    public Guid? AssignToUserId { get; set; }

    /// <summary>
    /// Role type to assign to (legacy - used with RoleBased strategy)
    /// Stored as string to reference RoleType enum value
    /// </summary>
    public string? AssignToRole { get; set; }

    /// <summary>
    /// Comma-separated list of user IDs (legacy - used with RoundRobin or GroupAssignment strategies)
    /// </summary>
    public string? AssignToUserIds { get; set; }

    // Contact Hierarchy fields (for SectionIncharge, BranchContacts, DepartmentContacts strategies)

    /// <summary>
    /// Primary contact user ID for this escalation level
    /// Used as first contact in the hierarchy for Section/Branch/Department strategies
    /// </summary>
    public Guid? PrimaryContactId { get; set; }

    /// <summary>
    /// Secondary contact user ID for this escalation level
    /// Used as backup if primary contact is unavailable
    /// </summary>
    public Guid? SecondaryContactId { get; set; }

    /// <summary>
    /// HR contact user ID for this escalation level
    /// Used as final fallback in the contact hierarchy
    /// </summary>
    public Guid? HrContactId { get; set; }

    // Branch/Department fields (for organizational unit-based strategies)

    /// <summary>
    /// Branch ID for branch-level escalations (used with BranchContacts strategy)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Department ID for department-level escalations (used with DepartmentContacts strategy)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    // Resource Pool fields (for ResourcePool strategy)

    /// <summary>
    /// Resource pool ID (used with ResourcePool strategy)
    /// </summary>
    public Guid? ResourcePoolId { get; set; }

    /// <summary>
    /// Assignment method to use when assigning from a resource pool
    /// Determines how complaints are distributed among pool members (Manual, RoundRobin, LeastBusy)
    /// </summary>
    public ResourcePoolAssignmentMethod? ResourcePoolAssignmentMethod { get; set; }

    /// <summary>
    /// Whether this level is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether to send email notification at this level
    /// </summary>
    public bool SendNotification { get; set; } = true;

    /// <summary>
    /// Email template ID to use for notifications (optional)
    /// </summary>
    public Guid? EmailTemplateId { get; set; }

    /// <summary>
    /// Whether to notify the previous handler as well
    /// </summary>
    public bool NotifyPreviousHandler { get; set; } = true;

    /// <summary>
    /// Custom message/instructions for this escalation level
    /// </summary>
    public string? EscalationMessage { get; set; }

    // Navigation properties
    /// <summary>
    /// Escalation matrix this level belongs to
    /// </summary>
    public EscalationMatrix EscalationMatrix { get; set; } = null!;

    /// <summary>
    /// Specific user to assign to (if applicable - legacy)
    /// </summary>
    public User? AssignToUser { get; set; }

    /// <summary>
    /// Primary contact user (for contact hierarchy strategies)
    /// </summary>
    public User? PrimaryContact { get; set; }

    /// <summary>
    /// Secondary contact user (for contact hierarchy strategies)
    /// </summary>
    public User? SecondaryContact { get; set; }

    /// <summary>
    /// HR contact user (for contact hierarchy strategies)
    /// </summary>
    public User? HrContact { get; set; }

    /// <summary>
    /// Branch for branch-level escalations
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Department for department-level escalations
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Resource pool for pool-based escalations
    /// </summary>
    public ResourcePool? ResourcePool { get; set; }

    /// <summary>
    /// Escalation history records for this level
    /// </summary>
    public ICollection<EscalationHistory> EscalationHistories { get; set; } = new List<EscalationHistory>();
}
