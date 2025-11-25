using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Represents a resource pool - a group of users that can be assigned to handle escalated complaints
/// Pools can be organized by Branch, Department, Section, or as Custom groups
/// </summary>
public class ResourcePool : BaseEntity
{
    /// <summary>
    /// Company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Pool name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Pool description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of resource pool (Branch, Department, Section, Custom)
    /// </summary>
    public ResourcePoolType PoolType { get; set; } = ResourcePoolType.Custom;

    /// <summary>
    /// Branch ID (for Branch-type pools)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Department ID (for Department-type pools)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Section ID (for Section-type pools)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Whether this resource pool is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigation properties

    /// <summary>
    /// Company this resource pool belongs to
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Branch this pool is associated with (if applicable)
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Department this pool is associated with (if applicable)
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Section this pool is associated with (if applicable)
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Members of this resource pool
    /// </summary>
    public ICollection<ResourcePoolMember> Members { get; set; } = new List<ResourcePoolMember>();

    /// <summary>
    /// Escalation levels that use this resource pool
    /// </summary>
    public ICollection<EscalationLevel> EscalationLevels { get; set; } = new List<EscalationLevel>();
}
