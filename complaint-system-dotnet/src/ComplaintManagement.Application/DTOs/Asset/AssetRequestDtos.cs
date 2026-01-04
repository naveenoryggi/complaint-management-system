using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

#region Asset Request DTOs

/// <summary>
/// DTO for creating a return request
/// </summary>
public class CreateReturnRequestDto
{
    public Guid AssetId { get; set; }
    public Guid? AssetAssignmentId { get; set; }
    public Guid? StoreId { get; set; }
    public string? ReturnReason { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating an assignment request
/// </summary>
public class CreateAssignmentRequestDto
{
    public Guid AssetId { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? AssignToUserId { get; set; }
    public Guid? AssignToCustomerId { get; set; }
    public AssignmentPurpose Purpose { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for creating a transfer request
/// </summary>
public class CreateTransferRequestDto
{
    public Guid AssetId { get; set; }
    public Guid? FromStoreId { get; set; }
    public Guid? ToStoreId { get; set; }
    public Guid? TransferToUserId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for approving a request
/// </summary>
public class ApproveRequestDto
{
    public string? Comments { get; set; }
}

/// <summary>
/// DTO for rejecting a request
/// </summary>
public class RejectRequestDto
{
    public string RejectionReason { get; set; } = string.Empty;
    public string? Comments { get; set; }
}

/// <summary>
/// DTO for escalating a request
/// </summary>
public class EscalateRequestDto
{
    public Guid? EscalateToUserId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for reassigning a request
/// </summary>
public class ReassignRequestDto
{
    public Guid NewApproverId { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// DTO for adding a comment to a request
/// </summary>
public class AddRequestCommentDto
{
    public string Comments { get; set; } = string.Empty;
}

/// <summary>
/// DTO for asset request response
/// </summary>
public class AssetRequestDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public AssetRequestType RequestType { get; set; }
    public string RequestTypeName => RequestType.ToString();
    public AssetRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Asset info
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetSerialNumber { get; set; }

    // Store info
    public Guid? StoreId { get; set; }
    public string? StoreName { get; set; }
    public string? StoreCode { get; set; }

    // Requester info
    public Guid RequestedById { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public string RequestedByEmployeeCode { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }

    // Assignment target info
    public Guid? AssignToUserId { get; set; }
    public string? AssignToUserName { get; set; }
    public Guid? AssignToCustomerId { get; set; }
    public string? AssignToCustomerName { get; set; }

    // Request details
    public string? ReturnReason { get; set; }
    public string? Notes { get; set; }
    public AssignmentPurpose? Purpose { get; set; }
    public string? PurposeName => Purpose?.ToString();
    public DateTime? ExpectedReturnDate { get; set; }

    // Approval info
    public Guid? CurrentApproverId { get; set; }
    public string? CurrentApproverName { get; set; }
    public DateTime? ApprovalDeadline { get; set; }
    public int EscalationLevel { get; set; }
    public bool IsOverdue => ApprovalDeadline.HasValue && ApprovalDeadline.Value < DateTime.UtcNow &&
                             Status == AssetRequestStatus.Pending || Status == AssetRequestStatus.Escalated;

    // Resolution info
    public DateTime? ApprovedAt { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedByName { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? CompletedAt { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Related approval history
    public List<RequestApprovalHistoryDto> ApprovalHistory { get; set; } = new();
}

/// <summary>
/// DTO for asset request list (simplified)
/// </summary>
public class AssetRequestListDto
{
    public Guid Id { get; set; }
    public string RequestNumber { get; set; } = string.Empty;
    public AssetRequestType RequestType { get; set; }
    public string RequestTypeName => RequestType.ToString();
    public AssetRequestStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? StoreName { get; set; }
    public string RequestedByName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string? CurrentApproverName { get; set; }
    public DateTime? ApprovalDeadline { get; set; }
    public bool IsOverdue { get; set; }
    public int EscalationLevel { get; set; }
}

/// <summary>
/// DTO for pending approvals summary
/// </summary>
public class PendingApprovalsDto
{
    public int TotalPending { get; set; }
    public int OverdueCount { get; set; }
    public int ReturnRequests { get; set; }
    public int AssignmentRequests { get; set; }
    public int TransferRequests { get; set; }
    public List<AssetRequestListDto> Requests { get; set; } = new();
}

#endregion

#region Request Approval History DTOs

/// <summary>
/// DTO for approval history entry
/// </summary>
public class RequestApprovalHistoryDto
{
    public Guid Id { get; set; }
    public ApprovalAction Action { get; set; }
    public string ActionName => Action.ToString();
    public DateTime ActionAt { get; set; }
    public Guid ActorId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActorEmployeeCode { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public int EscalationLevel { get; set; }
    public AssetRequestStatus? PreviousStatus { get; set; }
    public string? PreviousStatusName => PreviousStatus?.ToString();
    public AssetRequestStatus? NewStatus { get; set; }
    public string? NewStatusName => NewStatus?.ToString();
    public string? PreviousApproverName { get; set; }
    public string? NewApproverName { get; set; }
}

#endregion

#region Filter DTOs

/// <summary>
/// Filter parameters for asset requests
/// </summary>
public class AssetRequestFilterDto
{
    public AssetRequestType? RequestType { get; set; }
    public AssetRequestStatus? Status { get; set; }
    public Guid? StoreId { get; set; }
    public Guid? RequestedById { get; set; }
    public Guid? CurrentApproverId { get; set; }
    public Guid? AssetId { get; set; }
    public bool? IsOverdue { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

#endregion
