using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Escalation.Commands;
using ComplaintManagement.Application.Features.Escalation.Queries;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/escalation")]
[Authorize]
public class EscalationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEscalationService _escalationService;
    private readonly ILogger<EscalationController> _logger;

    public EscalationController(
        IMediator mediator,
        IEscalationService escalationService,
        ILogger<EscalationController> logger)
    {
        _mediator = mediator;
        _escalationService = escalationService;
        _logger = logger;
    }

    #region Escalation Matrix Management (Admin Only)

    /// <summary>
    /// Get all escalations (combines matrices, policies, and pending escalations)
    /// </summary>
    [HttpGet]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetAllEscalations()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            return Unauthorized("Company ID not found in token");
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Guid.TryParse(userIdClaim, out var userId);

            // Get matrices for the company
            var matricesQuery = new GetEscalationMatricesQuery { CompanyId = companyId };
            var matricesResult = await _mediator.Send(matricesQuery);

            // Get pending escalations for user if available
            object pendingEscalations;
            int pendingCount;
            if (userId != Guid.Empty)
            {
                var escalations = await _escalationService.GetPendingEscalationsAsync(userId);
                pendingEscalations = escalations;
                pendingCount = escalations.Count;
            }
            else
            {
                pendingEscalations = new List<object>();
                pendingCount = 0;
            }

            return Ok(new
            {
                isSuccess = true,
                data = new
                {
                    matrices = (object)matricesResult.Data ?? (object)new List<object>(),
                    pendingEscalations = pendingEscalations,
                    matricesCount = matricesResult.Data?.Count ?? 0,
                    pendingCount = pendingCount
                },
                message = "Escalation data retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving escalation data for company {CompanyId}", companyId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving escalation data"
            });
        }
    }

    /// <summary>
    /// Get all escalation matrices for the company
    /// </summary>
    [HttpGet("matrices")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetMatrices()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            return Unauthorized("Company ID not found in token");
        }

        var query = new GetEscalationMatricesQuery { CompanyId = companyId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Get escalation matrix by ID
    /// </summary>
    [HttpGet("matrices/{id}")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetMatrixById(Guid id)
    {
        var query = new GetEscalationMatrixByIdQuery { MatrixId = id };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new escalation matrix (Admin only)
    /// </summary>
    [HttpPost("matrices")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> CreateMatrix([FromBody] CreateEscalationMatrixRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            return Unauthorized("Company ID not found in token");
        }

        var command = new CreateEscalationMatrixCommand
        {
            Request = request,
            CompanyId = companyId,
            CreatedBy = userId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetMatrixById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing escalation matrix (Admin only)
    /// </summary>
    [HttpPut("matrices/{id}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> UpdateMatrix(Guid id, [FromBody] UpdateEscalationMatrixRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new UpdateEscalationMatrixCommand
        {
            MatrixId = id,
            Request = request,
            UpdatedBy = userId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Delete an escalation matrix (Admin only)
    /// </summary>
    [HttpDelete("matrices/{id}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> DeleteMatrix(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new DeleteEscalationMatrixCommand
        {
            MatrixId = id,
            DeletedBy = userId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Add a level to an escalation matrix (Admin only)
    /// </summary>
    [HttpPost("matrices/{matrixId}/levels")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> AddLevel(Guid matrixId, [FromBody] CreateEscalationLevelRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var command = new AddEscalationLevelCommand
        {
            MatrixId = matrixId,
            Request = request,
            CreatedBy = userId
        };

        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetMatrixById), new { id = matrixId }, result);
    }

    #endregion

    #region Complaint Escalation Operations

    /// <summary>
    /// Escalate a complaint (manual escalation)
    /// </summary>
    [HttpPost("complaints/{complaintId}/escalate")]
    [HasPermission("complaints.escalate")]
    public async Task<IActionResult> EscalateComplaint(Guid complaintId, [FromBody] EscalateComplaintRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        try
        {
            var escalationHistory = await _escalationService.EscalateComplaintAsync(
                complaintId,
                request.Reason,
                userId,
                request.EscalationMatrixId,
                request.TargetLevel
            );

            return Ok(new
            {
                isSuccess = true,
                message = $"Complaint escalated to level {escalationHistory.Level}",
                data = escalationHistory
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escalating complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while escalating the complaint" });
        }
    }

    /// <summary>
    /// Get escalation history for a complaint
    /// </summary>
    [HttpGet("complaints/{complaintId}/history")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetEscalationHistory(Guid complaintId)
    {
        var query = new GetEscalationHistoryQuery { ComplaintId = complaintId };
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Acknowledge an escalation
    /// </summary>
    [HttpPost("history/{escalationHistoryId}/acknowledge")]
    [HasPermission("complaints.acknowledge")]
    public async Task<IActionResult> AcknowledgeEscalation(Guid escalationHistoryId, [FromBody] AcknowledgeEscalationRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        try
        {
            await _escalationService.AcknowledgeEscalationAsync(escalationHistoryId, userId, request.Notes);

            return Ok(new
            {
                isSuccess = true,
                message = "Escalation acknowledged successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error acknowledging escalation {EscalationHistoryId}", escalationHistoryId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while acknowledging the escalation" });
        }
    }

    /// <summary>
    /// Get pending escalations for the current user
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> GetPendingEscalations()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("User ID not found in token");
        }

        var escalations = await _escalationService.GetPendingEscalationsAsync(userId);

        return Ok(new
        {
            isSuccess = true,
            data = escalations,
            count = escalations.Count
        });
    }

    #endregion
}
