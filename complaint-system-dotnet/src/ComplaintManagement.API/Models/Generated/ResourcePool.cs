using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ResourcePool
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int PoolType { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? SectionId { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<EscalationLevel> EscalationLevels { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<ResourcePoolMember> ResourcePoolMembers { get; set; } = new List<ResourcePoolMember>();

    public virtual Section? Section { get; set; }
}
