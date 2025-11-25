using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintStatusMaster
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public int DisplayOrder { get; set; }

    public string? ColorCode { get; set; }

    public string? IconClass { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystem { get; set; }

    public bool IsFinal { get; set; }

    public Guid? CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<CategoryWorkflowStatus> CategoryWorkflowStatuses { get; set; } = new List<CategoryWorkflowStatus>();

    public virtual ICollection<CategoryWorkflowTransition> CategoryWorkflowTransitionFromStatuses { get; set; } = new List<CategoryWorkflowTransition>();

    public virtual ICollection<CategoryWorkflowTransition> CategoryWorkflowTransitionToStatuses { get; set; } = new List<CategoryWorkflowTransition>();

    public virtual Company? Company { get; set; }

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();
}
