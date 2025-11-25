namespace ComplaintManagement.Domain.Entities;

/// <summary>
/// Base entity with common audit fields
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Primary key
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID who created this record
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// Last modification timestamp
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// User ID who last modified this record
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete flag
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Deletion timestamp
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who deleted this record
    /// </summary>
    public Guid? DeletedBy { get; set; }
}
