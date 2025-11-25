using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EscalationPolicy
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? SectionId { get; set; }

    public Guid? CategoryId { get; set; }

    public bool EnableAutoEscalation { get; set; }

    public bool RequireManualApproval { get; set; }

    public Guid? DefaultEscalationMatrixId { get; set; }

    public int? MinimumSeverityForAutoEscalation { get; set; }

    public int? MaxAutoEscalationLevels { get; set; }

    public int Priority { get; set; }

    public bool IsActive { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

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

    public virtual EscalationMatrix? DefaultEscalationMatrix { get; set; }

    public virtual Department? Department { get; set; }

    public virtual Section? Section { get; set; }
}
