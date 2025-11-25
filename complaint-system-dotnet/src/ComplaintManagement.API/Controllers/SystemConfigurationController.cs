using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// System Configuration API - Manage system-wide settings
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class SystemConfigurationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SystemConfigurationController> _logger;

    public SystemConfigurationController(
        IUnitOfWork unitOfWork,
        ILogger<SystemConfigurationController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get system configuration for the current user's company
    /// </summary>
    /// <returns>System configuration settings</returns>
    [HttpGet]
    [ProducesResponseType(typeof(SystemConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SystemConfiguration>> GetConfiguration()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(new { message = "Company ID not found in token" });
            }

            var config = await _unitOfWork.Repository<SystemConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (config == null)
            {
                // Create default configuration if it doesn't exist
                config = new SystemConfiguration
                {
                    CompanyId = companyId,
                    Id = Guid.NewGuid(),
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<SystemConfiguration>().AddAsync(config);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Created default system configuration for company {CompanyId}", companyId);
            }

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving system configuration");
            return StatusCode(500, new { message = "An error occurred while retrieving configuration" });
        }
    }

    /// <summary>
    /// Update system configuration (Admin only)
    /// </summary>
    /// <param name="request">Updated configuration</param>
    /// <returns>Updated configuration</returns>
    [HttpPut]
    [Authorize(Roles = "Admin")] // Only admins can modify system settings
    [ProducesResponseType(typeof(SystemConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemConfiguration>> UpdateConfiguration([FromBody] SystemConfiguration request)
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(new { message = "Company ID not found in token" });
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            Guid? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            // Validate configuration
            if (!request.Validate(out var errors))
            {
                return BadRequest(new { message = "Validation failed", errors });
            }

            var existingConfig = await _unitOfWork.Repository<SystemConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (existingConfig == null)
            {
                // Create new configuration
                request.Id = Guid.NewGuid();
                request.CompanyId = companyId;
                request.CreatedAt = DateTime.UtcNow;
                request.CreatedBy = userId;

                await _unitOfWork.Repository<SystemConfiguration>().AddAsync(request);
                _logger.LogInformation("Created system configuration for company {CompanyId} by user {UserId}", companyId, userId);
            }
            else
            {
                // Update existing configuration
                existingConfig.OAuthTokenRefreshIntervalMinutes = request.OAuthTokenRefreshIntervalMinutes;
                existingConfig.OAuthTokenExpiryWarningDays = request.OAuthTokenExpiryWarningDays;
                existingConfig.DefaultEmailPollingIntervalSeconds = request.DefaultEmailPollingIntervalSeconds;
                existingConfig.MaxEmailsFetchPerPoll = request.MaxEmailsFetchPerPoll;
                existingConfig.AutoResponseEnabled = request.AutoResponseEnabled;
                existingConfig.AutoResponseMaxRetryAttempts = request.AutoResponseMaxRetryAttempts;
                existingConfig.AutoResponseRetryDelaySeconds = request.AutoResponseRetryDelaySeconds;
                existingConfig.EmailRateLimitingEnabled = request.EmailRateLimitingEnabled;
                existingConfig.MaxEmailsPerHour = request.MaxEmailsPerHour;
                existingConfig.StatusChangeNotificationsEnabled = request.StatusChangeNotificationsEnabled;
                existingConfig.AssignmentNotificationsEnabled = request.AssignmentNotificationsEnabled;
                existingConfig.EscalationNotificationsEnabled = request.EscalationNotificationsEnabled;
                existingConfig.DefaultTimezone = request.DefaultTimezone;
                existingConfig.DateFormat = request.DateFormat;
                existingConfig.TimeFormat = request.TimeFormat;
                existingConfig.UpdatedAt = DateTime.UtcNow;
                existingConfig.UpdatedBy = userId;

                _unitOfWork.Repository<SystemConfiguration>().Update(existingConfig);
                _logger.LogInformation("Updated system configuration for company {CompanyId} by user {UserId}", companyId, userId);

                request = existingConfig; // Return the updated entity
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating system configuration");
            return StatusCode(500, new { message = "An error occurred while updating configuration" });
        }
    }

    /// <summary>
    /// Reset system configuration to defaults (Admin only)
    /// </summary>
    /// <returns>Default configuration</returns>
    [HttpPost("reset")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SystemConfiguration), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SystemConfiguration>> ResetConfiguration()
    {
        try
        {
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                return BadRequest(new { message = "Company ID not found in token" });
            }

            var userIdClaim = User.FindFirst("UserId")?.Value;
            Guid? userId = null;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
            {
                userId = parsedUserId;
            }

            var existingConfig = await _unitOfWork.Repository<SystemConfiguration>()
                .GetQueryable()
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (existingConfig != null)
            {
                // Delete existing configuration
                _unitOfWork.Repository<SystemConfiguration>().Delete(existingConfig);
            }

            // Create new default configuration
            var defaultConfig = new SystemConfiguration
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
                // All other properties will use their default values
            };

            await _unitOfWork.Repository<SystemConfiguration>().AddAsync(defaultConfig);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Reset system configuration to defaults for company {CompanyId} by user {UserId}", companyId, userId);

            return Ok(defaultConfig);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting system configuration");
            return StatusCode(500, new { message = "An error occurred while resetting configuration" });
        }
    }
}
