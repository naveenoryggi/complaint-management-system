using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Slalevel
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int Order { get; set; }

    public bool IsActive { get; set; }

    public string ColorCode { get; set; } = null!;

    public int DefaultResponseTime { get; set; }

    public string ResponseTimeUnit { get; set; } = null!;

    public int DefaultResolutionTime { get; set; }

    public string ResolutionTimeUnit { get; set; } = null!;

    public Guid? CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<CategorySla> CategorySlas { get; set; } = new List<CategorySla>();

    public virtual ICollection<PrioritySla> PrioritySlas { get; set; } = new List<PrioritySla>();
}
