using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Represents a member (user) of a resource pool
/// </summary>
public class ResourcePoolMember : BaseEntity
{
    /// <summary>
    /// Resource pool ID (foreign key)
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// User ID (foreign key)
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// When this member was added to the pool
    /// </summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Who added this member to the pool
    /// </summary>
    public Guid AddedBy { get; set; }

    /// <summary>
    /// Whether this member is currently active in the pool
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// Resource pool this member belongs to
    /// </summary>
    public ResourcePool ResourcePool { get; set; } = null!;

    /// <summary>
    /// User who is a member of this pool
    /// </summary>
    public User User { get; set; } = null!;
}
