using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EscalationHistory
{
    public Guid Id { get; set; }

    public Guid ComplaintId { get; set; }

    public Guid EscalationLevelId { get; set; }

    public Guid EscalationMatrixId { get; set; }

    public int Level { get; set; }

    public Guid? FromUserId { get; set; }

    public Guid ToUserId { get; set; }

    public Guid? EscalatedBy { get; set; }

    public DateTime EscalatedAt { get; set; }

    public string Reason { get; set; } = null!;

    public bool IsAutoEscalation { get; set; }

    public string AssignmentStrategy { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? AcknowledgedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public int? SlaHoursAtEscalation { get; set; }

    public int? HoursOverdue { get; set; }

    public bool EmailSent { get; set; }

    public DateTime? EmailSentAt { get; set; }

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Complaint Complaint { get; set; } = null!;

    public virtual User? EscalatedByNavigation { get; set; }

    public virtual EscalationLevel EscalationLevel { get; set; } = null!;

    public virtual EscalationMatrix EscalationMatrix { get; set; } = null!;

    public virtual User? FromUser { get; set; }

    public virtual User ToUser { get; set; } = null!;
}
