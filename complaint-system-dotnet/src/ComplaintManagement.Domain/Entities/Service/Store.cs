using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a store/warehouse location where assets are managed.
/// Each store can have primary and secondary managers for approval workflows.
/// </summary>
public class Store : BaseEntity
{
    /// <summary>
    /// Company this store belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Unique store code (e.g., "STR-001")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Store name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Store description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Physical address of the store
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// City where store is located
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// Contact phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Contact email address
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Primary store manager responsible for approvals
    /// </summary>
    public Guid? PrimaryManagerId { get; set; }

    /// <summary>
    /// Secondary store manager for escalations
    /// </summary>
    public Guid? SecondaryManagerId { get; set; }

    /// <summary>
    /// Branch this store is associated with (optional)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Whether this store is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Hours before a pending request auto-escalates to secondary manager
    /// </summary>
    public int ApprovalTimeoutHours { get; set; } = 48;

    /// <summary>
    /// Whether to automatically escalate to secondary manager on timeout
    /// </summary>
    public bool AutoEscalateToSecondary { get; set; } = true;

    /// <summary>
    /// Whether department manager approval is required for asset assignments
    /// </summary>
    public bool RequireDeptApprovalForAssignment { get; set; } = false;

    // Navigation Properties
    public virtual User? PrimaryManager { get; set; }
    public virtual User? SecondaryManager { get; set; }
    public virtual Branch? Branch { get; set; }
    public virtual ICollection<StoreUserRole> StoreUserRoles { get; set; } = new List<StoreUserRole>();
    public virtual ICollection<AssetRequest> AssetRequests { get; set; } = new List<AssetRequest>();
}
