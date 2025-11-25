using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EscalationLevel
{
    public Guid Id { get; set; }

    public Guid EscalationMatrixId { get; set; }

    public int Level { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public int TriggerAfterHours { get; set; }

    public string AssignmentStrategy { get; set; } = null!;

    public Guid? AssignToUserId { get; set; }

    public string? AssignToRole { get; set; }

    public string? AssignToUserIds { get; set; }

    public bool IsActive { get; set; }

    public bool SendNotification { get; set; }

    public Guid? EmailTemplateId { get; set; }

    public bool NotifyPreviousHandler { get; set; }

    public string? EscalationMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public int TriggerAfterValue { get; set; }

    public int TriggerTimeUnit { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? HrContactId { get; set; }

    public Guid? PrimaryContactId { get; set; }

    public int? ResourcePoolAssignmentMethod { get; set; }

    public Guid? ResourcePoolId { get; set; }

    public Guid? SecondaryContactId { get; set; }

    public virtual User? AssignToUser { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual Department? Department { get; set; }

    public virtual ICollection<EscalationHistory> EscalationHistories { get; set; } = new List<EscalationHistory>();

    public virtual EscalationMatrix EscalationMatrix { get; set; } = null!;

    public virtual User? HrContact { get; set; }

    public virtual User? PrimaryContact { get; set; }

    public virtual ResourcePool? ResourcePool { get; set; }

    public virtual User? SecondaryContact { get; set; }
}
