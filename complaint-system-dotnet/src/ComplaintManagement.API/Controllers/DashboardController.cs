using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Dashboard;
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing user dashboard preferences and statistics
/// </summary>
[ApiController]
[Route("api/dashboard")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get dashboard preferences for the current user
    /// </summary>
    /// <returns>Dashboard preferences or default preferences if none exist</returns>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(Result<DashboardPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _dashboardService.GetPreferencesAsync(userId.Value);

            if (!result.IsSuccess)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving dashboard preferences");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard preferences" });
        }
    }

    /// <summary>
    /// Save or update dashboard preferences for the current user
    /// </summary>
    /// <param name="request">Dashboard preferences to save</param>
    /// <returns>Saved dashboard preferences</returns>
    [HttpPut("preferences")]
    [ProducesResponseType(typeof(Result<DashboardPreferencesDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SavePreferences([FromBody] SaveDashboardPreferencesRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _dashboardService.SavePreferencesAsync(userId.Value, request);

            if (!result.IsSuccess)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving dashboard preferences");
            return StatusCode(500, new { message = "An error occurred while saving dashboard preferences" });
        }
    }

    /// <summary>
    /// Get dashboard statistics with status widgets
    /// </summary>
    /// <param name="dateRangeDays">Optional date range in days (default: user preference or 30)</param>
    /// <param name="statusIds">Optional list of status IDs to include (default: user preferences or all)</param>
    /// <returns>Dashboard statistics with status widgets</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(Result<DashboardStatisticsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] int? dateRangeDays = null,
        [FromQuery] List<Guid>? statusIds = null)
    {
        try
        {
            // SECURITY: Get current user ID and roles from JWT claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid currentUserId))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            // SECURITY: Determine user role to enforce role-based filtering
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            bool isAdmin = permissions.Contains("ManageUsers") ||
                          permissions.Contains("ManageSettings") ||
                          permissions.Contains("ManageCompany");
            bool isHandler = permissions.Contains("AssignComplaint") ||
                            permissions.Contains("EscalateComplaint");

            // SECURITY: Apply role-based filtering to statistics
            Guid? assignedToId = null;
            Guid? complainantId = null;

            if (isAdmin)
            {
                // Admin: Can see all statistics (no filtering)
                _logger.LogInformation("Admin user {UserId} accessing dashboard statistics with full access", currentUserId);
            }
            else if (isHandler)
            {
                // Handler: Can ONLY see statistics for complaints assigned to them
                assignedToId = currentUserId;
                _logger.LogInformation("Handler user {UserId} accessing dashboard statistics for assigned complaints", currentUserId);
            }
            else
            {
                // Complainant: Can ONLY see statistics for their own complaints
                complainantId = currentUserId;
                _logger.LogInformation("Complainant user {UserId} accessing dashboard statistics for own complaints", currentUserId);
            }

            var result = await _dashboardService.GetStatisticsAsync(
                currentUserId,
                dateRangeDays,
                statusIds,
                assignedToId,
                complainantId);

            if (!result.IsSuccess)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving dashboard statistics");
            return StatusCode(500, new { message = "An error occurred while retrieving dashboard statistics" });
        }
    }

    /// <summary>
    /// Reset dashboard preferences to default for the current user
    /// </summary>
    /// <returns>Success result</returns>
    [HttpDelete("preferences")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPreferences()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var result = await _dashboardService.ResetPreferencesAsync(userId.Value);

            if (!result.IsSuccess)
            {
                return StatusCode(500, result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting dashboard preferences");
            return StatusCode(500, new { message = "An error occurred while resetting dashboard preferences" });
        }
    }

    #region Helper Methods

    /// <summary>
    /// Gets the current user ID from JWT claims
    /// </summary>
    /// <returns>User ID or null if not found</returns>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }
        return null;
    }

    #endregion
}
