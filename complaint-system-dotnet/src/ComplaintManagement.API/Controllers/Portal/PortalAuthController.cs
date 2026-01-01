using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Portal;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.Portal;

/// <summary>
/// Customer Portal Authentication Controller
/// Handles authentication for external customer contacts
/// </summary>
[ApiController]
[Route("api/portal/auth")]
[RequiresLicense(LicenseModule.CustomerPortal)]
public class PortalAuthController : ControllerBase
{
    private readonly IPortalService _portalService;
    private readonly ILogger<PortalAuthController> _logger;

    public PortalAuthController(
        IPortalService portalService,
        ILogger<PortalAuthController> logger)
    {
        _portalService = portalService;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate portal user (customer contact)
    /// </summary>
    /// <param name="request">Portal login credentials</param>
    /// <returns>Login response with JWT token</returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PortalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] PortalLoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Validation failed"
                });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _portalService.LoginAsync(request, ipAddress);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Portal login failed for email: {Email}. Reason: {Reason}",
                    request.Email, result.Message);
                return Unauthorized(result);
            }

            _logger.LogInformation("Portal user logged in successfully: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during portal login for email: {Email}", request.Email);
            return StatusCode(500, new PortalLoginResponse
            {
                IsSuccess = false,
                Message = "An error occurred during login"
            });
        }
    }

    /// <summary>
    /// Refresh portal access token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token and refresh token</returns>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PortalLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] PortalRefreshTokenRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new PortalLoginResponse
                {
                    IsSuccess = false,
                    Message = "Refresh token is required"
                });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _portalService.RefreshTokenAsync(request, ipAddress);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Portal token refresh failed. Reason: {Reason}", result.Message);
                return Unauthorized(result);
            }

            _logger.LogInformation("Portal token refreshed successfully");
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during portal token refresh");
            return StatusCode(500, new PortalLoginResponse
            {
                IsSuccess = false,
                Message = "An error occurred during token refresh"
            });
        }
    }

    /// <summary>
    /// Logout portal user and revoke refresh token
    /// </summary>
    /// <param name="request">Optional refresh token to revoke</param>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] PortalRefreshTokenRequest? request = null)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { isSuccess = false, message = "Invalid portal session" });
            }

            var result = await _portalService.LogoutAsync(contactId.Value, request?.RefreshToken);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Portal logout failed for contact {ContactId}. Reason: {Reason}",
                    contactId, result.Message);
                return BadRequest(new { isSuccess = false, message = result.Message });
            }

            _logger.LogInformation("Portal user {ContactId} logged out successfully", contactId);
            return Ok(new { isSuccess = true, message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during portal logout");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Get current portal user profile
    /// </summary>
    /// <returns>Portal user profile information</returns>
    [HttpGet("profile")]
    [Authorize]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var profile = await _portalService.GetProfileAsync(contactId.Value);
            if (profile == null)
            {
                return NotFound(new { message = "Profile not found" });
            }

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal profile");
            return StatusCode(500, new { message = "An error occurred while retrieving profile" });
        }
    }

    /// <summary>
    /// Update portal user profile
    /// </summary>
    /// <param name="request">Profile update request</param>
    /// <returns>Updated profile</returns>
    [HttpPut("profile")]
    [Authorize]
    [ProducesResponseType(typeof(PortalUserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdatePortalProfileRequest request)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            var result = await _portalService.UpdateProfileAsync(contactId.Value, request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating portal profile");
            return StatusCode(500, new { message = "An error occurred while updating profile" });
        }
    }

    /// <summary>
    /// Change portal user password
    /// </summary>
    /// <param name="request">Password change request</param>
    /// <returns>Success message</returns>
    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] PortalChangePasswordRequest request)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { isSuccess = false, message = "Invalid portal session" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { isSuccess = false, message = "Validation failed" });
            }

            var result = await _portalService.ChangePasswordAsync(contactId.Value, request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { isSuccess = false, message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Portal user {ContactId} changed password successfully", contactId);
            return Ok(new { isSuccess = true, message = "Password changed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing portal password");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while changing password" });
        }
    }

    /// <summary>
    /// Initiate forgot password flow
    /// </summary>
    /// <param name="request">Forgot password request with email</param>
    /// <returns>Success message (always returns success for security)</returns>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] PortalForgotPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { isSuccess = false, message = "Email is required" });
            }

            // Always return success to prevent email enumeration
            await _portalService.ForgotPasswordAsync(request);

            return Ok(new
            {
                isSuccess = true,
                message = "If an account exists with this email, a password reset link will be sent."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during portal forgot password for email: {Email}", request.Email);
            // Still return success to prevent enumeration
            return Ok(new
            {
                isSuccess = true,
                message = "If an account exists with this email, a password reset link will be sent."
            });
        }
    }

    /// <summary>
    /// Reset password using token
    /// </summary>
    /// <param name="request">Reset password request with token and new password</param>
    /// <returns>Success or failure message</returns>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] PortalResetPasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { isSuccess = false, message = "Validation failed" });
            }

            var result = await _portalService.ResetPasswordAsync(request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { isSuccess = false, message = result.Message });
            }

            _logger.LogInformation("Portal password reset completed successfully");
            return Ok(new { isSuccess = true, message = "Password reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during portal password reset");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while resetting password" });
        }
    }

    #region Private Methods

    private Guid? GetContactIdFromClaims()
    {
        // Check for portal-specific claim first
        var contactIdClaim = User.FindFirst("ContactId")?.Value;
        if (!string.IsNullOrEmpty(contactIdClaim) && Guid.TryParse(contactIdClaim, out var contactId))
        {
            return contactId;
        }

        // Fallback to NameIdentifier for portal users
        var isPortalUser = User.FindFirst("IsPortalUser")?.Value;
        if (isPortalUser == "true")
        {
            var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(nameIdClaim) && Guid.TryParse(nameIdClaim, out var id))
            {
                return id;
            }
        }

        return null;
    }

    #endregion
}
