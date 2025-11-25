using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Application.Features.Auth.Commands;
using ComplaintManagement.Application.Features.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IMediator mediator, ILogger<AuthController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// Supports login with Email, Employee Code, or Phone Number
    /// </summary>
    /// <param name="request">Login credentials (Email/Employee Code/Phone Number and Password)</param>
    /// <returns>Login response with JWT token and user information</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validate request model
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

            var command = new LoginCommand
            {
                UserIdentifier = request.Email, // Keep property name for backward compatibility
                Password = request.Password
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Login failed for email: {Email}. Reason: {Reason}", request.Email, result.Message);
                return Unauthorized(new { message = result.Message });
            }

            _logger.LogInformation("User logged in successfully: {Email}", request.Email);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during login for email: {Email}", request.Email);
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    /// <summary>
    /// Get current authenticated user information
    /// </summary>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [HttpGet("profile")] // Alias for backward compatibility
    [Authorize]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "User information not found" });
            }

            var query = new GetCurrentUserQuery { UserId = userId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting current user");
            return StatusCode(500, new { message = "An error occurred while retrieving user information" });
        }
    }

    /// <summary>
    /// Refresh access token using refresh token
    /// Implements token rotation for enhanced security
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New access token and new refresh token</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            // Validate request
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return BadRequest(new { isSuccess = false, message = "Refresh token is required" });
            }

            // Get client IP address for audit logging
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var command = new RefreshTokenCommand
            {
                RefreshToken = request.RefreshToken,
                IpAddress = ipAddress
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Token refresh failed from IP {IpAddress}. Reason: {Reason}", ipAddress, result.Message);
                return Unauthorized(result);
            }

            _logger.LogInformation("Token refreshed successfully from IP: {IpAddress}", ipAddress);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during token refresh");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logout current user and revoke all active refresh tokens
    /// </summary>
    /// <returns>Success message</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        try
        {
            // Get user ID from claims
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { isSuccess = false, message = "User information not found" });
            }

            // Get client IP address for audit logging
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // Revoke all active refresh tokens for this user
            var command = new LogoutCommand
            {
                UserId = userId,
                IpAddress = ipAddress
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Logout failed for user {UserId}. Reason: {Reason}", userId, result.Message);
                return BadRequest(result);
            }

            _logger.LogInformation("User {UserId} logged out successfully from IP: {IpAddress}", userId, ipAddress);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during logout");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred during logout" });
        }
    }
}
