using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Escalation;

/// <summary>
/// DTO for Escalation History
/// </summary>
public class EscalationHistoryDto
{
    public Guid Id { get; set; }
    public Guid ComplaintId { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public Guid EscalationLevelId { get; set; }
    public Guid EscalationMatrixId { get; set; }
    public string EscalationMatrixName { get; set; } = string.Empty;
    public int Level { get; set; }
    public Guid? FromUserId { get; set; }
    public string? FromUserName { get; set; }
    public Guid ToUserId { get; set; }
    public string ToUserName { get; set; } = string.Empty;
    public Guid? EscalatedBy { get; set; }
    public string? EscalatedByName { get; set; }
    public DateTime EscalatedAt { get; set; }
    public string Reason { get; set; } = string.Empty;
    public bool IsAutoEscalation { get; set; }
    public AssignmentStrategy AssignmentStrategy { get; set; }
    public EscalationStatus Status { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? SlaHoursAtEscalation { get; set; }
    public int? HoursOverdue { get; set; }
    public bool EmailSent { get; set; }
    public DateTime? EmailSentAt { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request DTO for escalating a complaint
/// </summary>
public class EscalateComplaintRequest
{
    public Guid ComplaintId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Guid? EscalationMatrixId { get; set; }
    public int? TargetLevel { get; set; }
}

/// <summary>
/// Request DTO for acknowledging an escalation
/// </summary>
public class AcknowledgeEscalationRequest
{
    public Guid EscalationHistoryId { get; set; }
    public string? Notes { get; set; }
}
