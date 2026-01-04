using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Maps users to stores with specific roles.
/// Allows flexible role assignments beyond just primary/secondary managers.
/// </summary>
public class StoreUserRole : BaseEntity
{
    /// <summary>
    /// Store this role assignment belongs to
    /// </summary>
    public Guid StoreId { get; set; }

    /// <summary>
    /// User assigned to the store
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Role the user has in this store
    /// </summary>
    public StoreRole Role { get; set; }

    /// <summary>
    /// Whether this role assignment is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When the role was assigned
    /// </summary>
    public DateTime AssignedAt { get; set; }

    /// <summary>
    /// User who made this role assignment
    /// </summary>
    public Guid AssignedById { get; set; }

    /// <summary>
    /// When the role was revoked (if applicable)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// User who revoked this role (if applicable)
    /// </summary>
    public Guid? RevokedById { get; set; }

    /// <summary>
    /// Notes about this role assignment
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties
    public virtual Store? Store { get; set; }
    public virtual User? User { get; set; }
    public virtual User? AssignedBy { get; set; }
    public virtual User? RevokedBy { get; set; }
}
