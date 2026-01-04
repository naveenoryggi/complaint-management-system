using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a request for asset return, assignment, or transfer.
/// Supports configurable approval workflows with escalation.
/// </summary>
public class AssetRequest : BaseEntity
{
    /// <summary>
    /// Company this request belongs to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Auto-generated request number (e.g., "REQ-2026-00001")
    /// </summary>
    public string RequestNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of request (Return, Assignment, Transfer)
    /// </summary>
    public AssetRequestType RequestType { get; set; }

    /// <summary>
    /// Current status in the workflow
    /// </summary>
    public AssetRequestStatus Status { get; set; } = AssetRequestStatus.Draft;

    /// <summary>
    /// Asset being requested for return/assignment/transfer
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Related asset assignment (for return requests)
    /// </summary>
    public Guid? AssetAssignmentId { get; set; }

    /// <summary>
    /// Store handling this request
    /// </summary>
    public Guid? StoreId { get; set; }

    /// <summary>
    /// User who raised the request
    /// </summary>
    public Guid RequestedById { get; set; }

    /// <summary>
    /// When the request was submitted
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// Target user for assignment requests
    /// </summary>
    public Guid? AssignToUserId { get; set; }

    /// <summary>
    /// Target customer for external assignment requests
    /// </summary>
    public Guid? AssignToCustomerId { get; set; }

    /// <summary>
    /// Reason for return (for return requests)
    /// </summary>
    public string? ReturnReason { get; set; }

    /// <summary>
    /// Additional notes or comments
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Current approver responsible for this request
    /// </summary>
    public Guid? CurrentApproverId { get; set; }

    /// <summary>
    /// Deadline for current approver to take action
    /// </summary>
    public DateTime? ApprovalDeadline { get; set; }

    /// <summary>
    /// Current escalation level (0 = initial, 1 = secondary, etc.)
    /// </summary>
    public int EscalationLevel { get; set; } = 0;

    /// <summary>
    /// When the request was approved (if approved)
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// User who approved the request
    /// </summary>
    public Guid? ApprovedById { get; set; }

    /// <summary>
    /// When the request was rejected (if rejected)
    /// </summary>
    public DateTime? RejectedAt { get; set; }

    /// <summary>
    /// User who rejected the request
    /// </summary>
    public Guid? RejectedById { get; set; }

    /// <summary>
    /// Reason for rejection
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// When the request was completed/processed
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Purpose of assignment (for assignment requests)
    /// </summary>
    public AssignmentPurpose? Purpose { get; set; }

    /// <summary>
    /// Expected return date (for temporary assignments)
    /// </summary>
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>
    /// Target store for transfer requests
    /// </summary>
    public Guid? TransferToStoreId { get; set; }

    /// <summary>
    /// Target user for transfer requests
    /// </summary>
    public Guid? TransferToUserId { get; set; }

    // Navigation Properties
    public virtual Asset? Asset { get; set; }
    public virtual AssetAssignment? AssetAssignment { get; set; }
    public virtual Store? Store { get; set; }
    public virtual User? RequestedBy { get; set; }
    public virtual User? AssignToUser { get; set; }
    public virtual User? CurrentApprover { get; set; }
    public virtual User? ApprovedBy { get; set; }
    public virtual User? RejectedBy { get; set; }
    public virtual Customer? AssignToCustomer { get; set; }
    public virtual Store? TransferToStore { get; set; }
    public virtual User? TransferToUser { get; set; }
    public virtual ICollection<RequestApprovalHistory> ApprovalHistory { get; set; } = new List<RequestApprovalHistory>();
}
