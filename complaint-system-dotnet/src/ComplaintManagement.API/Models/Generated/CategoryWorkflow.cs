using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CategoryWorkflow
{
    public Guid Id { get; set; }

    public Guid CategoryId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public bool IsDefault { get; set; }

    public Guid? CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintCategory Category { get; set; } = null!;

    public virtual ICollection<CategoryWorkflowStatus> CategoryWorkflowStatuses { get; set; } = new List<CategoryWorkflowStatus>();

    public virtual ICollection<CategoryWorkflowTransition> CategoryWorkflowTransitions { get; set; } = new List<CategoryWorkflowTransition>();

    public virtual Company? Company { get; set; }
}
