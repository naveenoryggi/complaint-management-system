using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service implementation for Asset Request workflow operations
/// </summary>
public class AssetRequestService : IAssetRequestService
{
    private readonly ComplaintDbContext _context;
    private readonly IStoreService _storeService;
    private readonly ILogger<AssetRequestService> _logger;

    public AssetRequestService(
        ComplaintDbContext context,
        IStoreService storeService,
        ILogger<AssetRequestService> logger)
    {
        _context = context;
        _storeService = storeService;
        _logger = logger;
    }

    #region Request Creation

    public async Task<Result<AssetRequestDto>> CreateReturnRequestAsync(CreateReturnRequestDto dto, Guid requestedById, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(new object[] { dto.AssetId }, cancellationToken);
            if (asset == null || asset.IsDeleted)
                return Result<AssetRequestDto>.Failure("Asset not found.");

            if (dto.StoreId.HasValue)
            {
                var store = await _context.Stores.FindAsync(new object[] { dto.StoreId.Value }, cancellationToken);
                if (store == null || store.IsDeleted)
                    return Result<AssetRequestDto>.Failure("Store not found.");
            }

            var requestNumber = await GenerateRequestNumberAsync(AssetRequestType.Return, companyId, cancellationToken);

            var request = new AssetRequest
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                RequestNumber = requestNumber,
                RequestType = AssetRequestType.Return,
                Status = AssetRequestStatus.Draft,
                AssetId = dto.AssetId,
                AssetAssignmentId = dto.AssetAssignmentId,
                StoreId = dto.StoreId,
                RequestedById = requestedById,
                RequestedAt = DateTime.UtcNow,
                ReturnReason = dto.ReturnReason,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AssetRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created return request {RequestNumber}", requestNumber);

            var result = await GetRequestByIdAsync(request.Id, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request created but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating return request for asset {AssetId}", dto.AssetId);
            return Result<AssetRequestDto>.Failure($"Error creating return request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> CreateAssignmentRequestAsync(CreateAssignmentRequestDto dto, Guid requestedById, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(new object[] { dto.AssetId }, cancellationToken);
            if (asset == null || asset.IsDeleted)
                return Result<AssetRequestDto>.Failure("Asset not found.");

            if (dto.StoreId.HasValue)
            {
                var store = await _context.Stores.FindAsync(new object[] { dto.StoreId.Value }, cancellationToken);
                if (store == null || store.IsDeleted)
                    return Result<AssetRequestDto>.Failure("Store not found.");
            }

            if (!dto.AssignToUserId.HasValue && !dto.AssignToCustomerId.HasValue)
                return Result<AssetRequestDto>.Failure("Must specify either a user or customer to assign to.");

            var requestNumber = await GenerateRequestNumberAsync(AssetRequestType.Assignment, companyId, cancellationToken);

            var request = new AssetRequest
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                RequestNumber = requestNumber,
                RequestType = AssetRequestType.Assignment,
                Status = AssetRequestStatus.Draft,
                AssetId = dto.AssetId,
                StoreId = dto.StoreId,
                RequestedById = requestedById,
                RequestedAt = DateTime.UtcNow,
                AssignToUserId = dto.AssignToUserId,
                AssignToCustomerId = dto.AssignToCustomerId,
                Purpose = dto.Purpose,
                ExpectedReturnDate = dto.ExpectedReturnDate,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AssetRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created assignment request {RequestNumber}", requestNumber);

            var result = await GetRequestByIdAsync(request.Id, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request created but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating assignment request for asset {AssetId}", dto.AssetId);
            return Result<AssetRequestDto>.Failure($"Error creating assignment request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> CreateTransferRequestAsync(CreateTransferRequestDto dto, Guid requestedById, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var asset = await _context.Assets.FindAsync(new object[] { dto.AssetId }, cancellationToken);
            if (asset == null || asset.IsDeleted)
                return Result<AssetRequestDto>.Failure("Asset not found.");

            var requestNumber = await GenerateRequestNumberAsync(AssetRequestType.Transfer, companyId, cancellationToken);

            var request = new AssetRequest
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                RequestNumber = requestNumber,
                RequestType = AssetRequestType.Transfer,
                Status = AssetRequestStatus.Draft,
                AssetId = dto.AssetId,
                StoreId = dto.FromStoreId,
                RequestedById = requestedById,
                RequestedAt = DateTime.UtcNow,
                TransferToStoreId = dto.ToStoreId,
                TransferToUserId = dto.TransferToUserId,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.AssetRequests.Add(request);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created transfer request {RequestNumber}", requestNumber);

            var result = await GetRequestByIdAsync(request.Id, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request created but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transfer request for asset {AssetId}", dto.AssetId);
            return Result<AssetRequestDto>.Failure($"Error creating transfer request: {ex.Message}");
        }
    }

    #endregion

    #region Request Query

    public async Task<AssetRequestDto?> GetRequestByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Store)
                .Include(r => r.RequestedBy)
                .Include(r => r.CurrentApprover)
                .Include(r => r.AssignToUser)
                .Include(r => r.AssignToCustomer)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RejectedBy)
                .Include(r => r.ApprovalHistory)
                    .ThenInclude(h => h.Actor)
                .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

            return request != null ? MapToDto(request) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request {RequestId}", id);
            return null;
        }
    }

    public async Task<AssetRequestDto?> GetRequestByNumberAsync(string requestNumber, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Store)
                .Include(r => r.RequestedBy)
                .Include(r => r.CurrentApprover)
                .Include(r => r.AssignToUser)
                .Include(r => r.AssignToCustomer)
                .Include(r => r.ApprovedBy)
                .Include(r => r.RejectedBy)
                .Include(r => r.ApprovalHistory)
                    .ThenInclude(h => h.Actor)
                .FirstOrDefaultAsync(r => r.RequestNumber == requestNumber && r.CompanyId == companyId, cancellationToken);

            return request != null ? MapToDto(request) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request by number {RequestNumber}", requestNumber);
            return null;
        }
    }

    public async Task<PagedResult<AssetRequestListDto>> GetRequestsAsync(AssetRequestFilterDto filter, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Store)
                .Include(r => r.RequestedBy)
                .Include(r => r.CurrentApprover)
                .Where(r => r.CompanyId == companyId)
                .AsQueryable();

            if (filter.RequestType.HasValue)
                query = query.Where(r => r.RequestType == filter.RequestType.Value);

            if (filter.Status.HasValue)
                query = query.Where(r => r.Status == filter.Status.Value);

            if (filter.StoreId.HasValue)
                query = query.Where(r => r.StoreId == filter.StoreId.Value);

            if (filter.RequestedById.HasValue)
                query = query.Where(r => r.RequestedById == filter.RequestedById.Value);

            if (filter.CurrentApproverId.HasValue)
                query = query.Where(r => r.CurrentApproverId == filter.CurrentApproverId.Value);

            if (filter.AssetId.HasValue)
                query = query.Where(r => r.AssetId == filter.AssetId.Value);

            if (filter.IsOverdue == true)
                query = query.Where(r => r.ApprovalDeadline.HasValue && r.ApprovalDeadline.Value < DateTime.UtcNow &&
                    (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated));

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.RequestedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.RequestedAt <= filter.ToDate.Value);

            if (!string.IsNullOrEmpty(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(r => r.RequestNumber.ToLower().Contains(searchTerm) ||
                    r.Asset.AssetTag.ToLower().Contains(searchTerm) ||
                    r.Asset.Name.ToLower().Contains(searchTerm) ||
                    r.RequestedBy.FullName.ToLower().Contains(searchTerm));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(r => r.RequestedAt)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new AssetRequestListDto
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    RequestType = r.RequestType,
                    Status = r.Status,
                    AssetTag = r.Asset.AssetTag,
                    AssetName = r.Asset.Name,
                    StoreName = r.Store != null ? r.Store.Name : null,
                    RequestedByName = r.RequestedBy.FullName,
                    RequestedAt = r.RequestedAt,
                    CurrentApproverName = r.CurrentApprover != null ? r.CurrentApprover.FullName : null,
                    ApprovalDeadline = r.ApprovalDeadline,
                    IsOverdue = r.ApprovalDeadline.HasValue && r.ApprovalDeadline.Value < DateTime.UtcNow &&
                        (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated),
                    EscalationLevel = r.EscalationLevel
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<AssetRequestListDto>(items, filter.Page, filter.PageSize, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting requests for company {CompanyId}", companyId);
            return new PagedResult<AssetRequestListDto>();
        }
    }

    public async Task<PendingApprovalsDto> GetPendingApprovalsAsync(Guid approverId, CancellationToken cancellationToken = default)
    {
        try
        {
            var pendingRequests = await _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Store)
                .Include(r => r.RequestedBy)
                .Where(r => r.CurrentApproverId == approverId &&
                    (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated))
                .OrderByDescending(r => r.ApprovalDeadline.HasValue && r.ApprovalDeadline.Value < DateTime.UtcNow)
                .ThenBy(r => r.ApprovalDeadline)
                .ThenByDescending(r => r.RequestedAt)
                .Select(r => new AssetRequestListDto
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    RequestType = r.RequestType,
                    Status = r.Status,
                    AssetTag = r.Asset.AssetTag,
                    AssetName = r.Asset.Name,
                    StoreName = r.Store != null ? r.Store.Name : null,
                    RequestedByName = r.RequestedBy.FullName,
                    RequestedAt = r.RequestedAt,
                    CurrentApproverName = null,
                    ApprovalDeadline = r.ApprovalDeadline,
                    IsOverdue = r.ApprovalDeadline.HasValue && r.ApprovalDeadline.Value < DateTime.UtcNow,
                    EscalationLevel = r.EscalationLevel
                })
                .ToListAsync(cancellationToken);

            return new PendingApprovalsDto
            {
                TotalPending = pendingRequests.Count,
                OverdueCount = pendingRequests.Count(r => r.IsOverdue),
                ReturnRequests = pendingRequests.Count(r => r.RequestType == AssetRequestType.Return),
                AssignmentRequests = pendingRequests.Count(r => r.RequestType == AssetRequestType.Assignment),
                TransferRequests = pendingRequests.Count(r => r.RequestType == AssetRequestType.Transfer),
                Requests = pendingRequests
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending approvals for user {ApproverId}", approverId);
            return new PendingApprovalsDto();
        }
    }

    public async Task<List<AssetRequestListDto>> GetMyRequestsAsync(Guid userId, AssetRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Store)
                .Include(r => r.CurrentApprover)
                .Where(r => r.RequestedById == userId);

            if (status.HasValue)
                query = query.Where(r => r.Status == status.Value);

            return await query
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new AssetRequestListDto
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    RequestType = r.RequestType,
                    Status = r.Status,
                    AssetTag = r.Asset.AssetTag,
                    AssetName = r.Asset.Name,
                    StoreName = r.Store != null ? r.Store.Name : null,
                    RequestedByName = "",
                    RequestedAt = r.RequestedAt,
                    CurrentApproverName = r.CurrentApprover != null ? r.CurrentApprover.FullName : null,
                    ApprovalDeadline = r.ApprovalDeadline,
                    IsOverdue = r.ApprovalDeadline.HasValue && r.ApprovalDeadline.Value < DateTime.UtcNow &&
                        (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated),
                    EscalationLevel = r.EscalationLevel
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting requests for user {UserId}", userId);
            return new List<AssetRequestListDto>();
        }
    }

    public async Task<List<AssetRequestListDto>> GetAssetRequestHistoryAsync(Guid assetId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.AssetRequests
                .Include(r => r.Asset)
                .Include(r => r.Store)
                .Include(r => r.RequestedBy)
                .Include(r => r.CurrentApprover)
                .Where(r => r.AssetId == assetId)
                .OrderByDescending(r => r.RequestedAt)
                .Select(r => new AssetRequestListDto
                {
                    Id = r.Id,
                    RequestNumber = r.RequestNumber,
                    RequestType = r.RequestType,
                    Status = r.Status,
                    AssetTag = r.Asset.AssetTag,
                    AssetName = r.Asset.Name,
                    StoreName = r.Store != null ? r.Store.Name : null,
                    RequestedByName = r.RequestedBy.FullName,
                    RequestedAt = r.RequestedAt,
                    CurrentApproverName = r.CurrentApprover != null ? r.CurrentApprover.FullName : null,
                    ApprovalDeadline = r.ApprovalDeadline,
                    IsOverdue = r.ApprovalDeadline.HasValue && r.ApprovalDeadline.Value < DateTime.UtcNow &&
                        (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated),
                    EscalationLevel = r.EscalationLevel
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request history for asset {AssetId}", assetId);
            return new List<AssetRequestListDto>();
        }
    }

    #endregion

    #region Approval Workflow

    public async Task<Result<AssetRequestDto>> SubmitRequestAsync(Guid requestId, Guid submittedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests
                .Include(r => r.Store)
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Draft)
                return Result<AssetRequestDto>.Failure("Only draft requests can be submitted.");

            Guid? approverId = await GetNextApproverAsync(requestId, cancellationToken);
            DateTime? deadline = null;

            if (request.Store != null && approverId.HasValue)
                deadline = DateTime.UtcNow.AddHours(request.Store.ApprovalTimeoutHours);

            request.Status = AssetRequestStatus.Pending;
            request.CurrentApproverId = approverId;
            request.ApprovalDeadline = deadline;
            request.UpdatedAt = DateTime.UtcNow;

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = submittedById,
                Action = ApprovalAction.Submitted,
                ActionAt = DateTime.UtcNow,
                Comments = "Request submitted for approval",
                EscalationLevel = 0,
                PreviousStatus = AssetRequestStatus.Draft,
                NewStatus = AssetRequestStatus.Pending,
                NewApproverId = approverId,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Submitted request {RequestNumber} for approval", request.RequestNumber);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request submitted but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error submitting request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> ApproveRequestAsync(Guid requestId, ApproveRequestDto dto, Guid approvedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Pending && request.Status != AssetRequestStatus.Escalated)
                return Result<AssetRequestDto>.Failure("Request is not pending approval.");

            if (request.CurrentApproverId != approvedById)
                return Result<AssetRequestDto>.Failure("You are not authorized to approve this request.");

            var previousStatus = request.Status;
            request.Status = AssetRequestStatus.Approved;
            request.ApprovedById = approvedById;
            request.ApprovedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = approvedById,
                Action = ApprovalAction.Approved,
                ActionAt = DateTime.UtcNow,
                Comments = dto.Comments,
                EscalationLevel = request.EscalationLevel,
                PreviousStatus = previousStatus,
                NewStatus = AssetRequestStatus.Approved,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Approved request {RequestNumber}", request.RequestNumber);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request approved but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error approving request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> RejectRequestAsync(Guid requestId, RejectRequestDto dto, Guid rejectedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Pending && request.Status != AssetRequestStatus.Escalated)
                return Result<AssetRequestDto>.Failure("Request is not pending approval.");

            if (request.CurrentApproverId != rejectedById)
                return Result<AssetRequestDto>.Failure("You are not authorized to reject this request.");

            var previousStatus = request.Status;
            request.Status = AssetRequestStatus.Rejected;
            request.RejectedById = rejectedById;
            request.RejectedAt = DateTime.UtcNow;
            request.RejectionReason = dto.RejectionReason;
            request.UpdatedAt = DateTime.UtcNow;

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = rejectedById,
                Action = ApprovalAction.Rejected,
                ActionAt = DateTime.UtcNow,
                Comments = $"{dto.RejectionReason}{(string.IsNullOrEmpty(dto.Comments) ? "" : $" - {dto.Comments}")}",
                EscalationLevel = request.EscalationLevel,
                PreviousStatus = previousStatus,
                NewStatus = AssetRequestStatus.Rejected,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Rejected request {RequestNumber}", request.RequestNumber);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request rejected but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error rejecting request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> CancelRequestAsync(Guid requestId, string? reason, Guid cancelledById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Draft &&
                request.Status != AssetRequestStatus.Pending &&
                request.Status != AssetRequestStatus.Escalated)
                return Result<AssetRequestDto>.Failure("This request cannot be cancelled.");

            if (request.RequestedById != cancelledById)
                return Result<AssetRequestDto>.Failure("You are not authorized to cancel this request.");

            var previousStatus = request.Status;
            request.Status = AssetRequestStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = cancelledById,
                Action = ApprovalAction.Cancelled,
                ActionAt = DateTime.UtcNow,
                Comments = reason ?? "Cancelled by requester",
                EscalationLevel = request.EscalationLevel,
                PreviousStatus = previousStatus,
                NewStatus = AssetRequestStatus.Cancelled,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cancelled request {RequestNumber}", request.RequestNumber);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request cancelled but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error cancelling request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> EscalateRequestAsync(Guid requestId, EscalateRequestDto dto, Guid escalatedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests
                .Include(r => r.Store)
                .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Pending && request.Status != AssetRequestStatus.Escalated)
                return Result<AssetRequestDto>.Failure("Request is not pending approval.");

            var previousApproverId = request.CurrentApproverId;
            var previousStatus = request.Status;
            Guid? newApproverId = dto.EscalateToUserId;

            if (!newApproverId.HasValue && request.Store != null)
            {
                if (request.EscalationLevel == 0 && request.Store.SecondaryManagerId.HasValue)
                    newApproverId = request.Store.SecondaryManagerId;
            }

            if (!newApproverId.HasValue)
                return Result<AssetRequestDto>.Failure("No escalation target available.");

            request.Status = AssetRequestStatus.Escalated;
            request.CurrentApproverId = newApproverId;
            request.EscalationLevel++;
            request.ApprovalDeadline = request.Store != null
                ? DateTime.UtcNow.AddHours(request.Store.ApprovalTimeoutHours)
                : DateTime.UtcNow.AddHours(48);
            request.UpdatedAt = DateTime.UtcNow;

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = escalatedById,
                Action = ApprovalAction.Escalated,
                ActionAt = DateTime.UtcNow,
                Comments = dto.Reason ?? "Request escalated",
                EscalationLevel = request.EscalationLevel,
                PreviousStatus = previousStatus,
                NewStatus = AssetRequestStatus.Escalated,
                PreviousApproverId = previousApproverId,
                NewApproverId = newApproverId,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Escalated request {RequestNumber} to level {Level}", request.RequestNumber, request.EscalationLevel);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request escalated but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error escalating request: {ex.Message}");
        }
    }

    public async Task<Result<AssetRequestDto>> ReassignRequestAsync(Guid requestId, ReassignRequestDto dto, Guid reassignedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Pending && request.Status != AssetRequestStatus.Escalated)
                return Result<AssetRequestDto>.Failure("Request is not pending approval.");

            var newApprover = await _context.Users.FindAsync(new object[] { dto.NewApproverId }, cancellationToken);
            if (newApprover == null || newApprover.IsDeleted)
                return Result<AssetRequestDto>.Failure("New approver not found.");

            var previousApproverId = request.CurrentApproverId;
            request.CurrentApproverId = dto.NewApproverId;
            request.UpdatedAt = DateTime.UtcNow;

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = reassignedById,
                Action = ApprovalAction.Reassigned,
                ActionAt = DateTime.UtcNow,
                Comments = dto.Reason ?? "Request reassigned",
                EscalationLevel = request.EscalationLevel,
                PreviousStatus = request.Status,
                NewStatus = request.Status,
                PreviousApproverId = previousApproverId,
                NewApproverId = dto.NewApproverId,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Reassigned request {RequestNumber} to user {NewApproverId}", request.RequestNumber, dto.NewApproverId);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request reassigned but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reassigning request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error reassigning request: {ex.Message}");
        }
    }

    public async Task<Result<RequestApprovalHistoryDto>> AddCommentAsync(Guid requestId, AddRequestCommentDto dto, Guid commenterId, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
            if (request == null)
                return Result<RequestApprovalHistoryDto>.Failure("Request not found.");

            var commenter = await _context.Users.FindAsync(new object[] { commenterId }, cancellationToken);

            var history = new RequestApprovalHistory
            {
                Id = Guid.NewGuid(),
                RequestId = requestId,
                ActorId = commenterId,
                Action = ApprovalAction.CommentAdded,
                ActionAt = DateTime.UtcNow,
                Comments = dto.Comments,
                EscalationLevel = request.EscalationLevel,
                PreviousStatus = request.Status,
                NewStatus = request.Status,
                CreatedAt = DateTime.UtcNow
            };

            _context.RequestApprovalHistories.Add(history);
            await _context.SaveChangesAsync(cancellationToken);

            var historyDto = new RequestApprovalHistoryDto
            {
                Id = history.Id,
                Action = history.Action,
                ActionAt = history.ActionAt,
                ActorId = history.ActorId,
                ActorName = commenter?.FullName ?? "",
                ActorEmployeeCode = commenter?.EmployeeCode ?? "",
                Comments = history.Comments,
                EscalationLevel = history.EscalationLevel,
                PreviousStatus = history.PreviousStatus,
                NewStatus = history.NewStatus
            };

            return Result<RequestApprovalHistoryDto>.Success(historyDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment to request {RequestId}", requestId);
            return Result<RequestApprovalHistoryDto>.Failure($"Error adding comment: {ex.Message}");
        }
    }

    #endregion

    #region Auto-Escalation

    public async Task<int> ProcessAutoEscalationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var overdueRequests = await _context.AssetRequests
                .Include(r => r.Store)
                .Where(r => (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated) &&
                    r.ApprovalDeadline.HasValue &&
                    r.ApprovalDeadline.Value < DateTime.UtcNow &&
                    r.Store != null &&
                    r.Store.AutoEscalateToSecondary)
                .ToListAsync(cancellationToken);

            var escalatedCount = 0;

            foreach (var request in overdueRequests)
            {
                if (request.EscalationLevel > 0) continue;

                if (request.Store?.SecondaryManagerId != null &&
                    request.CurrentApproverId != request.Store.SecondaryManagerId)
                {
                    var previousApproverId = request.CurrentApproverId;
                    request.CurrentApproverId = request.Store.SecondaryManagerId;
                    request.Status = AssetRequestStatus.Escalated;
                    request.EscalationLevel = 1;
                    request.ApprovalDeadline = DateTime.UtcNow.AddHours(request.Store.ApprovalTimeoutHours);
                    request.UpdatedAt = DateTime.UtcNow;

                    var history = new RequestApprovalHistory
                    {
                        Id = Guid.NewGuid(),
                        RequestId = request.Id,
                        ActorId = previousApproverId ?? Guid.Empty,
                        Action = ApprovalAction.Escalated,
                        ActionAt = DateTime.UtcNow,
                        Comments = "Auto-escalated due to approval timeout",
                        EscalationLevel = 1,
                        PreviousStatus = AssetRequestStatus.Pending,
                        NewStatus = AssetRequestStatus.Escalated,
                        PreviousApproverId = previousApproverId,
                        NewApproverId = request.Store.SecondaryManagerId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.RequestApprovalHistories.Add(history);
                    escalatedCount++;

                    _logger.LogInformation("Auto-escalated request {RequestNumber}", request.RequestNumber);
                }
            }

            if (escalatedCount > 0)
                await _context.SaveChangesAsync(cancellationToken);

            return escalatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto-escalations");
            return 0;
        }
    }

    #endregion

    #region Request Completion

    public async Task<Result<AssetRequestDto>> CompleteRequestAsync(Guid requestId, Guid completedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
            if (request == null)
                return Result<AssetRequestDto>.Failure("Request not found.");

            if (request.Status != AssetRequestStatus.Approved)
                return Result<AssetRequestDto>.Failure("Only approved requests can be completed.");

            request.CompletedAt = DateTime.UtcNow;
            request.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Completed request {RequestNumber}", request.RequestNumber);

            var result = await GetRequestByIdAsync(requestId, cancellationToken);
            return result != null
                ? Result<AssetRequestDto>.Success(result)
                : Result<AssetRequestDto>.Failure("Request completed but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing request {RequestId}", requestId);
            return Result<AssetRequestDto>.Failure($"Error completing request: {ex.Message}");
        }
    }

    #endregion

    #region Authorization Checks

    public async Task<bool> CanApproveAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
        if (request == null) return false;

        return request.CurrentApproverId == userId &&
            (request.Status == AssetRequestStatus.Pending || request.Status == AssetRequestStatus.Escalated);
    }

    public async Task<bool> CanViewAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = await _context.AssetRequests
            .Include(r => r.Store)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null) return false;
        if (request.RequestedById == userId) return true;
        if (request.CurrentApproverId == userId) return true;

        if (request.Store != null)
        {
            if (request.Store.PrimaryManagerId == userId || request.Store.SecondaryManagerId == userId)
                return true;
        }

        return false;
    }

    public async Task<bool> CanCancelAsync(Guid requestId, Guid userId, CancellationToken cancellationToken = default)
    {
        var request = await _context.AssetRequests.FindAsync(new object[] { requestId }, cancellationToken);
        if (request == null) return false;

        return request.RequestedById == userId &&
            (request.Status == AssetRequestStatus.Draft ||
             request.Status == AssetRequestStatus.Pending ||
             request.Status == AssetRequestStatus.Escalated);
    }

    #endregion

    #region Utility

    public async Task<string> GenerateRequestNumberAsync(AssetRequestType requestType, Guid companyId, CancellationToken cancellationToken = default)
    {
        var prefix = requestType switch
        {
            AssetRequestType.Return => "RET",
            AssetRequestType.Assignment => "ASN",
            AssetRequestType.Transfer => "TRF",
            _ => "REQ"
        };

        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;

        var lastRequest = await _context.AssetRequests
            .Where(r => r.CompanyId == companyId &&
                r.RequestType == requestType &&
                r.RequestNumber.StartsWith($"{prefix}-{year}{month:D2}"))
            .OrderByDescending(r => r.RequestNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var sequence = 1;
        if (lastRequest != null)
        {
            var lastSequence = lastRequest.RequestNumber.Split('-').LastOrDefault();
            if (int.TryParse(lastSequence, out var parsed))
                sequence = parsed + 1;
        }

        return $"{prefix}-{year}{month:D2}-{sequence:D4}";
    }

    public async Task<Guid?> GetNextApproverAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        var request = await _context.AssetRequests
            .Include(r => r.Store)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (request == null) return null;

        if (request.Store != null)
            return request.Store.PrimaryManagerId;

        return null;
    }

    #endregion

    #region Private Mapping Methods

    private static AssetRequestDto MapToDto(AssetRequest request)
    {
        var dto = new AssetRequestDto
        {
            Id = request.Id,
            RequestNumber = request.RequestNumber,
            RequestType = request.RequestType,
            Status = request.Status,
            AssetId = request.AssetId,
            AssetTag = request.Asset?.AssetTag ?? "",
            AssetName = request.Asset?.Name ?? "",
            AssetSerialNumber = request.Asset?.SerialNumber,
            StoreId = request.StoreId,
            StoreName = request.Store?.Name,
            StoreCode = request.Store?.Code,
            RequestedById = request.RequestedById,
            RequestedByName = request.RequestedBy?.FullName ?? "",
            RequestedByEmployeeCode = request.RequestedBy?.EmployeeCode ?? "",
            RequestedAt = request.RequestedAt,
            AssignToUserId = request.AssignToUserId,
            AssignToUserName = request.AssignToUser?.FullName,
            AssignToCustomerId = request.AssignToCustomerId,
            AssignToCustomerName = request.AssignToCustomer?.Name,
            ReturnReason = request.ReturnReason,
            Notes = request.Notes,
            Purpose = request.Purpose,
            ExpectedReturnDate = request.ExpectedReturnDate,
            CurrentApproverId = request.CurrentApproverId,
            CurrentApproverName = request.CurrentApprover?.FullName,
            ApprovalDeadline = request.ApprovalDeadline,
            EscalationLevel = request.EscalationLevel,
            ApprovedAt = request.ApprovedAt,
            ApprovedByName = request.ApprovedBy?.FullName,
            RejectedAt = request.RejectedAt,
            RejectedByName = request.RejectedBy?.FullName,
            RejectionReason = request.RejectionReason,
            CompletedAt = request.CompletedAt,
            CreatedAt = request.CreatedAt,
            UpdatedAt = request.UpdatedAt
        };

        if (request.ApprovalHistory != null)
        {
            dto.ApprovalHistory = request.ApprovalHistory
                .OrderByDescending(h => h.ActionAt)
                .Select(h => new RequestApprovalHistoryDto
                {
                    Id = h.Id,
                    Action = h.Action,
                    ActionAt = h.ActionAt,
                    ActorId = h.ActorId,
                    ActorName = h.Actor?.FullName ?? "",
                    ActorEmployeeCode = h.Actor?.EmployeeCode ?? "",
                    Comments = h.Comments,
                    EscalationLevel = h.EscalationLevel,
                    PreviousStatus = h.PreviousStatus,
                    NewStatus = h.NewStatus,
                    PreviousApproverName = h.PreviousApprover?.FullName,
                    NewApproverName = h.NewApprover?.FullName
                })
                .ToList();
        }

        return dto;
    }

    #endregion
}
