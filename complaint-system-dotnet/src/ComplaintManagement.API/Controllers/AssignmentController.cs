using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for intelligent complaint assignment operations
/// </summary>
[Authorize]
[ApiController]
[Route("api/assignment")]
[Produces("application/json")]
public class AssignmentController : ControllerBase
{
    private readonly IAssignmentEngine _assignmentEngine;
    private readonly ILogger<AssignmentController> _logger;

    public AssignmentController(
        IAssignmentEngine assignmentEngine,
        ILogger<AssignmentController> logger)
    {
        _assignmentEngine = assignmentEngine;
        _logger = logger;
    }

    /// <summary>
    /// Automatically assign a complaint using intelligent routing
    /// </summary>
    /// <param name="complaintId">The complaint to assign</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result</returns>
    [HttpPost("auto-assign/{complaintId}")]
    [HasPermission("AssignComplaint")]
    [ProducesResponseType(typeof(AssignmentResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AutoAssign(
        Guid complaintId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Auto-assigning complaint {ComplaintId}", complaintId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var context = new AssignmentContext
            {
                AssignedByUserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                AssignmentReason = "Auto-assignment via API",
                IsInitialAssignment = true
            };

            var result = await _assignmentEngine.AutoAssignComplaintAsync(
                complaintId,
                context,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { isSuccess = false, message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(new
            {
                isSuccess = true,
                message = result.Message,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-assigning complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "Auto-assignment failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Assign a complaint to a specific resource pool
    /// </summary>
    /// <param name="complaintId">The complaint to assign</param>
    /// <param name="request">Assignment request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result</returns>
    [HttpPost("assign-to-pool/{complaintId}")]
    [HasPermission("AssignComplaint")]
    [ProducesResponseType(typeof(AssignmentResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> AssignToPool(
        Guid complaintId,
        [FromBody] AssignToPoolRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation(
                "Assigning complaint {ComplaintId} to pool {PoolId} using method {Method}",
                complaintId, request.ResourcePoolId, request.AssignmentMethod);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var context = new AssignmentContext
            {
                AssignedByUserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                AssignmentReason = request.AssignmentReason ?? "Pool assignment via API",
                ForceAssignment = request.ForceAssignment ?? false
            };

            var result = await _assignmentEngine.AssignComplaintToPoolAsync(
                complaintId,
                request.ResourcePoolId,
                request.AssignmentMethod ?? AdvancedAssignmentMethod.BestFit,
                request.SpecificUserId,
                context,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { isSuccess = false, message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(new
            {
                isSuccess = true,
                message = result.Message,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning complaint {ComplaintId} to pool", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "Pool assignment failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Get assignment candidates for a complaint
    /// </summary>
    /// <param name="complaintId">The complaint to find candidates for</param>
    /// <param name="includeUserDetails">Include user details in response</param>
    /// <param name="calculateSkillScores">Calculate skill match scores</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of candidate resource pools</returns>
    [HttpGet("candidates/{complaintId}")]
    [HasPermission("ViewComplaints")]
    [ProducesResponseType(typeof(List<ResourcePoolCandidate>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetCandidates(
        Guid complaintId,
        [FromQuery] bool includeUserDetails = true,
        [FromQuery] bool calculateSkillScores = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting assignment candidates for complaint {ComplaintId}", complaintId);

            var candidates = await _assignmentEngine.GetAssignmentCandidatesAsync(
                complaintId,
                null,
                includeUserDetails,
                calculateSkillScores,
                cancellationToken);

            return Ok(new
            {
                isSuccess = true,
                message = $"Found {candidates.Count} candidate pool(s)",
                data = candidates,
                count = candidates.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment candidates for complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "Failed to get candidates", error = ex.Message });
        }
    }

    /// <summary>
    /// Validate an assignment before executing it
    /// </summary>
    /// <param name="complaintId">The complaint to validate</param>
    /// <param name="request">Validation request details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate/{complaintId}")]
    [HasPermission("ViewComplaints")]
    [ProducesResponseType(typeof(AssignmentValidationResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ValidateAssignment(
        Guid complaintId,
        [FromBody] ValidateAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Validating assignment for complaint {ComplaintId}", complaintId);

            var result = await _assignmentEngine.ValidateAssignmentAsync(
                complaintId,
                request.UserId,
                request.PoolId,
                null,
                cancellationToken);

            return Ok(new
            {
                isSuccess = result.IsValid,
                message = result.IsValid ? "Assignment is valid" : "Assignment has validation issues",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating assignment for complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "Validation failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Execute assignment rules for a complaint
    /// </summary>
    /// <param name="complaintId">The complaint to process</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result based on rules</returns>
    [HttpPost("execute-rules/{complaintId}")]
    [HasPermission("AssignComplaint")]
    [ProducesResponseType(typeof(AssignmentResult), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> ExecuteRules(
        Guid complaintId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Executing assignment rules for complaint {ComplaintId}", complaintId);

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var context = new AssignmentContext
            {
                AssignedByUserId = string.IsNullOrEmpty(userId) ? null : Guid.Parse(userId),
                AssignmentReason = "Rule-based assignment",
                IsInitialAssignment = true
            };

            var result = await _assignmentEngine.ExecuteAssignmentRulesAsync(
                complaintId,
                context,
                cancellationToken);

            if (!result.Success)
            {
                return BadRequest(new { isSuccess = false, message = result.Message, errorCode = result.ErrorCode });
            }

            return Ok(new
            {
                isSuccess = true,
                message = result.Message,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing assignment rules for complaint {ComplaintId}", complaintId);
            return StatusCode(500, new { isSuccess = false, message = "Rule execution failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Calculate suitability score for a user-pool combination
    /// </summary>
    /// <param name="userId">The user to evaluate</param>
    /// <param name="poolId">The resource pool</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Suitability score</returns>
    [HttpGet("suitability-score")]
    [HasPermission("ViewComplaints")]
    [ProducesResponseType(typeof(double), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> CalculateSuitabilityScore(
        [FromQuery] Guid userId,
        [FromQuery] Guid poolId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Calculating suitability score for user {UserId} in pool {PoolId}",
                userId, poolId);

            var criteria = new AssignmentCriteria(); // Default criteria
            var score = await _assignmentEngine.CalculateUserSuitabilityScoreAsync(
                userId,
                poolId,
                criteria,
                cancellationToken);

            return Ok(new
            {
                isSuccess = true,
                message = "Suitability score calculated",
                data = new
                {
                    userId,
                    poolId,
                    suitabilityScore = score,
                    rating = GetScoreRating(score)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating suitability score");
            return StatusCode(500, new { isSuccess = false, message = "Score calculation failed", error = ex.Message });
        }
    }

    /// <summary>
    /// Get user selection from a pool using a specific assignment method
    /// </summary>
    /// <param name="poolId">The resource pool</param>
    /// <param name="method">The assignment method to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Selected user candidate</returns>
    [HttpGet("select-user/{poolId}")]
    [HasPermission("ViewComplaints")]
    [ProducesResponseType(typeof(UserCandidate), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> SelectUser(
        Guid poolId,
        [FromQuery] AdvancedAssignmentMethod method = AdvancedAssignmentMethod.BestFit,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Selecting user from pool {PoolId} using method {Method}",
                poolId, method);

            var context = new AssignmentContext
            {
                AssignmentReason = "User selection preview"
            };

            var selectedUser = await _assignmentEngine.SelectUserFromPoolAsync(
                poolId,
                method,
                context,
                cancellationToken);

            if (selectedUser == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "No suitable user found in pool"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                message = "User selected successfully",
                data = selectedUser
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting user from pool {PoolId}", poolId);
            return StatusCode(500, new { isSuccess = false, message = "User selection failed", error = ex.Message });
        }
    }

    #region Helper Methods

    private string GetScoreRating(double score)
    {
        return score switch
        {
            >= 0.9 => "Excellent",
            >= 0.75 => "Very Good",
            >= 0.6 => "Good",
            >= 0.4 => "Fair",
            >= 0.2 => "Poor",
            _ => "Very Poor"
        };
    }

    #endregion
}
