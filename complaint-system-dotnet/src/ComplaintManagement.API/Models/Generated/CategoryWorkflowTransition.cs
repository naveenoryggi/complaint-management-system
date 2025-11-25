using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CategoryWorkflowTransition
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public Guid FromStatusId { get; set; }

    public Guid ToStatusId { get; set; }

    public string? TransitionName { get; set; }

    public string? Description { get; set; }

    public bool RequiresComment { get; set; }

    public bool RequiresApproval { get; set; }

    public string? AllowedRoles { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; }

    public bool IsAutomatic { get; set; }

    public int? AutoTransitionAfterHours { get; set; }

    public string? TransitionConditions { get; set; }

    public string? ButtonColor { get; set; }

    public string? IconClass { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintStatusMaster FromStatus { get; set; } = null!;

    public virtual ComplaintStatusMaster ToStatus { get; set; } = null!;

    public virtual CategoryWorkflow Workflow { get; set; } = null!;
}
