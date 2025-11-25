using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EscalationMatrix
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public bool IsActive { get; set; }

    public int Priority { get; set; }

    public bool EnableAutoEscalation { get; set; }

    public bool SendEmailNotifications { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ComplaintCategory? Category { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Department? Department { get; set; }

    public virtual ICollection<EscalationHistory> EscalationHistories { get; set; } = new List<EscalationHistory>();

    public virtual ICollection<EscalationLevel> EscalationLevels { get; set; } = new List<EscalationLevel>();

    public virtual ICollection<EscalationPolicy> EscalationPolicies { get; set; } = new List<EscalationPolicy>();
}
