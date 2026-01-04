using System.Security.Claims;
using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiresLicense(Domain.Enums.LicenseModule.AssetManagement)]
public class AssetRequestsController : ControllerBase
{
    private readonly IAssetRequestService _requestService;
    private readonly ILogger<AssetRequestsController> _logger;

    public AssetRequestsController(IAssetRequestService requestService, ILogger<AssetRequestsController> logger)
    {
        _requestService = requestService;
        _logger = logger;
    }

    #region Helper Methods

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private Guid? GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }

    #endregion

    #region Request Creation

    /// <summary>
    /// Creates a return request for an asset
    /// </summary>
    [HttpPost("return")]
    public async Task<IActionResult> CreateReturnRequest([FromBody] CreateReturnRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var companyId = GetCompanyId();
        if (!userId.HasValue || !companyId.HasValue)
        {
            return Unauthorized("User or company context not found.");
        }

        var result = await _requestService.CreateReturnRequestAsync(dto, userId.Value, companyId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates an assignment request for an asset
    /// </summary>
    [HttpPost("assignment")]
    public async Task<IActionResult> CreateAssignmentRequest([FromBody] CreateAssignmentRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var companyId = GetCompanyId();
        if (!userId.HasValue || !companyId.HasValue)
        {
            return Unauthorized("User or company context not found.");
        }

        var result = await _requestService.CreateAssignmentRequestAsync(dto, userId.Value, companyId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Creates a transfer request for an asset
    /// </summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> CreateTransferRequest([FromBody] CreateTransferRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        var companyId = GetCompanyId();
        if (!userId.HasValue || !companyId.HasValue)
        {
            return Unauthorized("User or company context not found.");
        }

        var result = await _requestService.CreateTransferRequestAsync(dto, userId.Value, companyId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    #endregion

    #region Request Query

    /// <summary>
    /// Gets requests with filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetRequests([FromQuery] AssetRequestFilterDto filter, CancellationToken cancellationToken = default)
    {
        var companyId = GetCompanyId();
        if (!companyId.HasValue)
        {
            return Unauthorized("Company context not found.");
        }

        var result = await _requestService.GetRequestsAsync(filter, companyId.Value, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a request by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetRequest(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _requestService.GetRequestByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound("Request not found.");
        }
        return Ok(Result<AssetRequestDto>.Success(result));
    }

    /// <summary>
    /// Gets a request by request number
    /// </summary>
    [HttpGet("by-number/{requestNumber}")]
    public async Task<IActionResult> GetRequestByNumber(string requestNumber, CancellationToken cancellationToken = default)
    {
        var companyId = GetCompanyId();
        if (!companyId.HasValue)
        {
            return Unauthorized("Company context not found.");
        }

        var result = await _requestService.GetRequestByNumberAsync(requestNumber, companyId.Value, cancellationToken);
        if (result == null)
        {
            return NotFound("Request not found.");
        }
        return Ok(Result<AssetRequestDto>.Success(result));
    }

    /// <summary>
    /// Gets pending approvals for the current user
    /// </summary>
    [HttpGet("pending-approvals")]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.GetPendingApprovalsAsync(userId.Value, cancellationToken);
        return Ok(Result<PendingApprovalsDto>.Success(result));
    }

    /// <summary>
    /// Gets requests submitted by the current user
    /// </summary>
    [HttpGet("my-requests")]
    public async Task<IActionResult> GetMyRequests([FromQuery] AssetRequestStatus? status = null, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.GetMyRequestsAsync(userId.Value, status, cancellationToken);
        return Ok(Result<List<AssetRequestListDto>>.Success(result));
    }

    /// <summary>
    /// Gets request history for an asset
    /// </summary>
    [HttpGet("asset/{assetId:guid}/history")]
    public async Task<IActionResult> GetAssetRequestHistory(Guid assetId, CancellationToken cancellationToken = default)
    {
        var result = await _requestService.GetAssetRequestHistoryAsync(assetId, cancellationToken);
        return Ok(Result<List<AssetRequestListDto>>.Success(result));
    }

    #endregion

    #region Approval Workflow

    /// <summary>
    /// Submits a draft request for approval
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitRequest(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.SubmitRequestAsync(id, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Approves a request
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> ApproveRequest(Guid id, [FromBody] ApproveRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.ApproveRequestAsync(id, dto, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Rejects a request
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectRequest(Guid id, [FromBody] RejectRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.RejectRequestAsync(id, dto, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Cancels a request
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> CancelRequest(Guid id, [FromBody] CancelRequestDto? dto = null, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.CancelRequestAsync(id, dto?.Reason, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Escalates a request
    /// </summary>
    [HttpPost("{id:guid}/escalate")]
    public async Task<IActionResult> EscalateRequest(Guid id, [FromBody] EscalateRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.EscalateRequestAsync(id, dto, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Reassigns a request to another approver
    /// </summary>
    [HttpPost("{id:guid}/reassign")]
    public async Task<IActionResult> ReassignRequest(Guid id, [FromBody] ReassignRequestDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.ReassignRequestAsync(id, dto, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Adds a comment to a request
    /// </summary>
    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> AddComment(Guid id, [FromBody] AddRequestCommentDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.AddCommentAsync(id, dto, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    #endregion

    #region Request Completion

    /// <summary>
    /// Marks an approved request as completed
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> CompleteRequest(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _requestService.CompleteRequestAsync(id, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    #endregion

    #region Authorization Checks

    /// <summary>
    /// Checks if the current user can approve a request
    /// </summary>
    [HttpGet("{id:guid}/can-approve")]
    public async Task<IActionResult> CanApprove(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var canApprove = await _requestService.CanApproveAsync(id, userId.Value, cancellationToken);
        return Ok(new { canApprove });
    }

    /// <summary>
    /// Checks if the current user can cancel a request
    /// </summary>
    [HttpGet("{id:guid}/can-cancel")]
    public async Task<IActionResult> CanCancel(Guid id, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var canCancel = await _requestService.CanCancelAsync(id, userId.Value, cancellationToken);
        return Ok(new { canCancel });
    }

    #endregion
}

/// <summary>
/// DTO for cancelling a request
/// </summary>
public class CancelRequestDto
{
    public string? Reason { get; set; }
}
