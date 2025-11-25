using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for self-service password reset functionality
/// All endpoints are public (no authentication required)
/// </summary>
[ApiController]
[Route("api/password-reset")]
public class PasswordResetController : ControllerBase
{
    private readonly IPasswordResetService _passwordResetService;
    private readonly ILogger<PasswordResetController> _logger;

    public PasswordResetController(
        IPasswordResetService passwordResetService,
        ILogger<PasswordResetController> logger)
    {
        _passwordResetService = passwordResetService;
        _logger = logger;
    }

    /// <summary>
    /// Request a password reset token
    /// Sends reset link to user's email if account exists
    /// </summary>
    /// <param name="request">Email address</param>
    /// <returns>Generic success message (security: don't reveal if email exists)</returns>
    [HttpPost("request")]
    [ProducesResponseType(typeof(PasswordResetRequestResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> RequestPasswordReset([FromBody] PasswordResetRequestModel request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return Ok(new PasswordResetRequestResponse
            {
                Success = true,
                Message = "If an account with that email exists, a password reset link has been sent."
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _passwordResetService.RequestPasswordResetAsync(
            request.Email,
            ipAddress,
            userAgent);

        if (result.RateLimitExceeded)
        {
            return StatusCode(429, new
            {
                success = false,
                message = result.Message,
                minutesUntilNextRequest = result.MinutesUntilNextRequest
            });
        }

        return Ok(new PasswordResetRequestResponse
        {
            Success = result.IsSuccess,
            Message = result.Message
        });
    }

    /// <summary>
    /// Validate a password reset token
    /// Checks if token is valid, not expired, and not used
    /// </summary>
    /// <param name="request">Token to validate</param>
    /// <returns>Validation result</returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(PasswordResetTokenValidationResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Ok(new PasswordResetTokenValidationResponse
            {
                IsValid = false,
                ErrorMessage = "Token is required"
            });
        }

        var result = await _passwordResetService.ValidateResetTokenAsync(request.Token);

        return Ok(new PasswordResetTokenValidationResponse
        {
            IsValid = result.IsValid,
            Email = result.Email,
            ErrorMessage = result.ErrorMessage,
            ExpiresAt = result.ExpiresAt
        });
    }

    /// <summary>
    /// Reset password using a valid token
    /// Validates token, checks password policy, and updates password
    /// </summary>
    /// <param name="request">Token and new password</param>
    /// <returns>Reset result</returns>
    [HttpPost("reset")]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(PasswordResetResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new PasswordResetResponse
            {
                Success = false,
                Message = "Token and new password are required"
            });
        }

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _passwordResetService.ResetPasswordAsync(
            request.Token,
            request.NewPassword,
            ipAddress,
            userAgent);

        if (!result.IsSuccess)
        {
            return BadRequest(new PasswordResetResponse
            {
                Success = false,
                Message = result.Message,
                ValidationErrors = result.ValidationErrors,
                TokenExpired = result.TokenExpired,
                TokenAlreadyUsed = result.TokenAlreadyUsed
            });
        }

        return Ok(new PasswordResetResponse
        {
            Success = true,
            Message = result.Message
        });
    }

    #region Request/Response Models

    public class PasswordResetRequestModel
    {
        public string Email { get; set; } = string.Empty;
    }

    public class PasswordResetRequestResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }

    public class PasswordResetTokenValidationResponse
    {
        public bool IsValid { get; set; }
        public string? Email { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class PasswordResetResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> ValidationErrors { get; set; } = new();
        public bool TokenExpired { get; set; }
        public bool TokenAlreadyUsed { get; set; }
    }

    #endregion
}
