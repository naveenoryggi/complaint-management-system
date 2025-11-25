using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Temp;

public partial class ComplaintCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public int DefaultPriority { get; set; }

    public int DefaultSlaHours { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<ComplaintCategory> InverseParentCategory { get; set; } = new List<ComplaintCategory>();

    public virtual ComplaintCategory? ParentCategory { get; set; }
}
