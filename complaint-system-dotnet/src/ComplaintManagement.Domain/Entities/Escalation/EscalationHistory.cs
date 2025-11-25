using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Represents a historical record of a complaint escalation event
/// Tracks when, why, and to whom a complaint was escalated
/// </summary>
public class EscalationHistory : BaseEntity
{
    /// <summary>
    /// Complaint ID (foreign key)
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// Escalation level ID (foreign key) - which level rule was applied
    /// </summary>
    public Guid EscalationLevelId { get; set; }

    /// <summary>
    /// Escalation matrix ID (foreign key) - which matrix was used
    /// </summary>
    public Guid EscalationMatrixId { get; set; }

    /// <summary>
    /// Level number at time of escalation (1-5)
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Previous handler user ID (who it was escalated from)
    /// </summary>
    public Guid? FromUserId { get; set; }

    /// <summary>
    /// New handler user ID (who it was escalated to)
    /// </summary>
    public Guid ToUserId { get; set; }

    /// <summary>
    /// User ID who triggered the escalation (may be system/auto)
    /// </summary>
    public Guid? EscalatedBy { get; set; }

    /// <summary>
    /// Timestamp when escalation occurred
    /// </summary>
    public DateTime EscalatedAt { get; set; }

    /// <summary>
    /// Reason for escalation
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Whether this was an automatic escalation (SLA breach) or manual
    /// </summary>
    public bool IsAutoEscalation { get; set; } = false;

    /// <summary>
    /// Assignment strategy used for this escalation
    /// </summary>
    public AssignmentStrategy AssignmentStrategy { get; set; }

    /// <summary>
    /// Current status of this escalation
    /// </summary>
    public EscalationStatus Status { get; set; } = EscalationStatus.Triggered;

    /// <summary>
    /// Timestamp when escalation was acknowledged by new handler
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Timestamp when escalation was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// SLA hours at time of escalation
    /// </summary>
    public int? SlaHoursAtEscalation { get; set; }

    /// <summary>
    /// Hours overdue at time of escalation (if SLA was breached)
    /// </summary>
    public int? HoursOverdue { get; set; }

    /// <summary>
    /// Email notification sent successfully
    /// </summary>
    public bool EmailSent { get; set; } = false;

    /// <summary>
    /// Timestamp when email was sent
    /// </summary>
    public DateTime? EmailSentAt { get; set; }

    /// <summary>
    /// Additional notes or comments about this escalation
    /// </summary>
    public string? Notes { get; set; }

    // Navigation properties
    /// <summary>
    /// Complaint that was escalated
    /// </summary>
    public Complaint Complaint { get; set; } = null!;

    /// <summary>
    /// Escalation level configuration that was applied
    /// </summary>
    public EscalationLevel EscalationLevel { get; set; } = null!;

    /// <summary>
    /// Escalation matrix that was used
    /// </summary>
    public EscalationMatrix EscalationMatrix { get; set; } = null!;

    /// <summary>
    /// Previous handler (escalated from)
    /// </summary>
    public User? FromUser { get; set; }

    /// <summary>
    /// New handler (escalated to)
    /// </summary>
    public User ToUser { get; set; } = null!;

    /// <summary>
    /// User who triggered the escalation
    /// </summary>
    public User? EscalatedByUser { get; set; }
}
