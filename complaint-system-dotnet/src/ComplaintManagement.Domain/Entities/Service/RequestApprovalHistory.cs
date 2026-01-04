using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Audit trail for all actions taken on an asset request.
/// Records who did what and when during the approval workflow.
/// </summary>
public class RequestApprovalHistory : BaseEntity
{
    /// <summary>
    /// The request this history entry belongs to
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// User who performed the action
    /// </summary>
    public Guid ActorId { get; set; }

    /// <summary>
    /// Type of action performed
    /// </summary>
    public ApprovalAction Action { get; set; }

    /// <summary>
    /// When the action was performed
    /// </summary>
    public DateTime ActionAt { get; set; }

    /// <summary>
    /// Comments or notes provided with the action
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Escalation level at the time of action
    /// </summary>
    public int EscalationLevel { get; set; }

    /// <summary>
    /// Previous status before this action
    /// </summary>
    public AssetRequestStatus? PreviousStatus { get; set; }

    /// <summary>
    /// New status after this action
    /// </summary>
    public AssetRequestStatus? NewStatus { get; set; }

    /// <summary>
    /// Previous approver (for reassignment actions)
    /// </summary>
    public Guid? PreviousApproverId { get; set; }

    /// <summary>
    /// New approver (for reassignment/escalation actions)
    /// </summary>
    public Guid? NewApproverId { get; set; }

    /// <summary>
    /// IP address of the actor (for audit purposes)
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string (for audit purposes)
    /// </summary>
    public string? UserAgent { get; set; }

    // Navigation Properties
    public virtual AssetRequest? Request { get; set; }
    public virtual User? Actor { get; set; }
    public virtual User? PreviousApprover { get; set; }
    public virtual User? NewApprover { get; set; }
}
