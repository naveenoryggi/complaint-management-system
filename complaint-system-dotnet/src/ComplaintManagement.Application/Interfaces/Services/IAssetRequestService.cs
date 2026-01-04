using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for Asset Request workflow operations
/// </summary>
public interface IAssetRequestService
{
    #region Request Creation

    /// <summary>
    /// Creates a return request for an asset
    /// </summary>
    Task<Result<AssetRequestDto>> CreateReturnRequestAsync(CreateReturnRequestDto dto, Guid requestedById, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates an assignment request for an asset
    /// </summary>
    Task<Result<AssetRequestDto>> CreateAssignmentRequestAsync(CreateAssignmentRequestDto dto, Guid requestedById, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a transfer request for an asset
    /// </summary>
    Task<Result<AssetRequestDto>> CreateTransferRequestAsync(CreateTransferRequestDto dto, Guid requestedById, Guid companyId, CancellationToken cancellationToken = default);

    #endregion

    #region Request Query

    /// <summary>
    /// Gets a request by ID
    /// </summary>
    Task<AssetRequestDto?> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a request by request number
    /// </summary>
    Task<AssetRequestDto?> GetRequestByNumberAsync(string requestNumber, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets requests with filtering
    /// </summary>
    Task<PagedResult<AssetRequestListDto>> GetRequestsAsync(AssetRequestFilterDto filter, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets requests pending approval for a user
    /// </summary>
    Task<PendingApprovalsDto> GetPendingApprovalsAsync(Guid approverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets requests submitted by a user
    /// </summary>
    Task<List<AssetRequestListDto>> GetMyRequestsAsync(Guid userId, AssetRequestStatus? status = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets request history for an asset
    /// </summary>
    Task<List<AssetRequestListDto>> GetAssetRequestHistoryAsync(Guid assetId, CancellationToken cancellationToken = default);

    #endregion

    #region Approval Workflow

    /// <summary>
    /// Submits a draft request for approval
    /// </summary>
    Task<Result<AssetRequestDto>> SubmitRequestAsync(Guid requestId, Guid submittedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Approves a request
    /// </summary>
    Task<Result<AssetRequestDto>> ApproveRequestAsync(Guid requestId, ApproveRequestDto dto, Guid approvedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects a request
    /// </summary>
    Task<Result<AssetRequestDto>> RejectRequestAsync(Guid requestId, RejectRequestDto dto, Guid rejectedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a request (by requester)
    /// </summary>
    Task<Result<AssetRequestDto>> CancelRequestAsync(Guid requestId, string? reason, Guid cancelledById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Escalates a request to secondary manager or higher
    /// </summary>
    Task<Result<AssetRequestDto>> EscalateRequestAsync(Guid requestId, EscalateRequestDto dto, Guid escalatedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reassigns a request to a different approver
    /// </summary>
    Task<Result<AssetRequestDto>> ReassignRequestAsync(Guid requestId, ReassignRequestDto dto, Guid reassignedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a comment to a request
    /// </summary>
    Task<Result<RequestApprovalHistoryDto>> AddCommentAsync(Guid requestId, AddRequestCommentDto dto, Guid commenterId, CancellationToken cancellationToken = default);

    #endregion

    #region Auto-Escalation

    /// <summary>
    /// Processes auto-escalation for overdue requests
    /// </summary>
    Task<int> ProcessAutoEscalationsAsync(CancellationToken cancellationToken = default);

    #endregion

    #region Request Completion

    /// <summary>
    /// Marks an approved request as completed (asset action executed)
    /// </summary>
    Task<Result<AssetRequestDto>> CompleteRequestAsync(Guid requestId, Guid completedById, CancellationToken cancellationToken = default);

    #endregion

    #region Authorization Checks

    /// <summary>
    /// Checks if user can approve a specific request
    /// </summary>
    Task<bool> CanApproveAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can view a specific request
    /// </summary>
    Task<bool> CanViewAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user can cancel a specific request
    /// </summary>
    Task<bool> CanCancelAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default);

    #endregion

    #region Utility

    /// <summary>
    /// Generates a unique request number
    /// </summary>
    Task<string> GenerateRequestNumberAsync(AssetRequestType requestType, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next approver based on workflow configuration
    /// </summary>
    Task<Guid?> GetNextApproverAsync(Guid requestId, CancellationToken cancellationToken = default);

    #endregion
}
