using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public int DefaultPriority { get; set; }

    public bool IsActive { get; set; }

    public int DisplayOrder { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public Guid? WorkflowId { get; set; }

    public virtual ICollection<CannedResponse> CannedResponses { get; set; } = new List<CannedResponse>();

    public virtual CategorySla? CategorySla { get; set; }

    public virtual ICollection<CategoryWorkflow> CategoryWorkflows { get; set; } = new List<CategoryWorkflow>();

    public virtual ICollection<Complaint> Complaints { get; set; } = new List<Complaint>();

    public virtual ICollection<EscalationMatrix> EscalationMatrices { get; set; } = new List<EscalationMatrix>();

    public virtual ICollection<EscalationPolicy> EscalationPolicies { get; set; } = new List<EscalationPolicy>();

    public virtual ICollection<ComplaintCategory> InverseParentCategory { get; set; } = new List<ComplaintCategory>();

    public virtual ComplaintCategory? ParentCategory { get; set; }
}
