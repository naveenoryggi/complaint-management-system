using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CategoryWorkflowStatus
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public Guid StatusMasterId { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsInitialStatus { get; set; }

    public bool IsActive { get; set; }

    public int? DefaultSlahours { get; set; }

    public int? EscalationHours { get; set; }

    public bool RequiresApproval { get; set; }

    public string? AllowedRoles { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintStatusMaster StatusMaster { get; set; } = null!;

    public virtual CategoryWorkflow Workflow { get; set; } = null!;
}
