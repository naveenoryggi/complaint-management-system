using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class PrioritySla
{
    public Guid Id { get; set; }

    public Guid PriorityId { get; set; }

    public Guid SlalevelId { get; set; }

    public int? OverrideResponseTime { get; set; }

    public int? OverrideResolutionTime { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintPriorityMaster Priority { get; set; } = null!;

    public virtual Slalevel Slalevel { get; set; } = null!;
}
