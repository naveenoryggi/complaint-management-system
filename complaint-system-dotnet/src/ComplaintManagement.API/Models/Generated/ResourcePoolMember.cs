using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ResourcePoolMember
{
    public Guid Id { get; set; }

    public Guid ResourcePoolId { get; set; }

    public Guid UserId { get; set; }

    public DateTime AddedAt { get; set; }

    public Guid AddedBy { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ResourcePool ResourcePool { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
