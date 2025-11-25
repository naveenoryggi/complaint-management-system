using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PasswordController : ControllerBase
{
    private readonly IPasswordService _passwordService;
    private readonly ILogger<PasswordController> _logger;

    public PasswordController(
        IPasswordService passwordService,
        ILogger<PasswordController> logger)
    {
        _passwordService = passwordService;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    #region User Password Operations

    /// <summary>
    /// Change current user's own password
    /// </summary>
    [HttpPost("change")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword([FromBody] PasswordChangeRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized("User not authenticated");

        var result = await _passwordService.ChangeUserPasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            GetIpAddress());

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Message });

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Get password strength for a given password (without saving)
    /// </summary>
    [HttpPost("strength")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetPasswordStrength([FromBody] CheckPasswordStrengthRequest request)
    {
        var strength = _passwordService.CalculatePasswordStrength(request.Password);

        return Ok(new
        {
            score = strength.Score,
            category = strength.CategoryDisplay,
            colorCode = strength.ColorCode
        });
    }

    /// <summary>
    /// Validate password against company policy (without saving)
    /// </summary>
    [HttpPost("validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidatePassword([FromBody] ValidatePasswordPolicyRequest request)
    {
        var result = await _passwordService.ValidatePasswordComplexityAsync(
            request.Password,
            request.CompanyId);

        return Ok(new
        {
            isValid = result.IsValid,
            errors = result.Errors
        });
    }

    /// <summary>
    /// Get password status for current user (days until expiration, must change, etc.)
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPasswordStatus()
    {
        var userId = GetCurrentUserId();
        if (userId == Guid.Empty)
            return Unauthorized("User not authenticated");

        var daysUntilExpiration = await _passwordService.GetDaysUntilPasswordExpiresAsync(userId);
        var isExpired = await _passwordService.IsPasswordExpiredAsync(userId);
        var isLocked = await _passwordService.IsAccountLockedAsync(userId);

        return Ok(new
        {
            daysUntilExpiration,
            isExpired,
            isLocked
        });
    }

    #endregion

    #region Admin Password Operations

    /// <summary>
    /// Set a user's password (admin operation)
    /// </summary>
    [HttpPost("set")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SetUserPassword([FromBody] AdminSetPasswordRequest request)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized("User not authenticated");

        var result = await _passwordService.SetUserPasswordAsync(
            request.UserId,
            request.Password,
            adminId,
            request.MustChangeOnNextLogin,
            request.SendEmail,
            GetIpAddress());

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Message });

        return Ok(new { message = "Password set successfully" });
    }

    /// <summary>
    /// Reset a user's password with auto-generated password (admin operation)
    /// </summary>
    [HttpPost("reset")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetUserPassword([FromBody] AdminResetPasswordRequest request)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized("User not authenticated");

        var result = await _passwordService.ResetUserPasswordAsync(
            request.UserId,
            adminId,
            request.SendEmail,
            GetIpAddress());

        if (!result.IsSuccess)
            return BadRequest(new { error = result.Message });

        return Ok(new
        {
            message = "Password reset successfully",
            newPassword = result.Data // Only return if email sending fails
        });
    }

    /// <summary>
    /// Generate a secure random password
    /// </summary>
    [HttpPost("generate")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GeneratePassword([FromBody] GenerateSecurePasswordRequest? request = null)
    {
        var password = _passwordService.GenerateSecurePassword(
            request?.Length ?? 12,
            request?.IncludeUppercase ?? true,
            request?.IncludeLowercase ?? true,
            request?.IncludeDigits ?? true,
            request?.IncludeSpecialChars ?? true);

        var strength = _passwordService.CalculatePasswordStrength(password);

        return Ok(new
        {
            password,
            strength = new
            {
                score = strength.Score,
                category = strength.CategoryDisplay,
                colorCode = strength.ColorCode
            }
        });
    }

    /// <summary>
    /// Unlock a locked user account (admin operation)
    /// </summary>
    [HttpPost("unlock")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UnlockAccount([FromBody] UnlockUserAccountRequest request)
    {
        var adminId = GetCurrentUserId();
        if (adminId == Guid.Empty)
            return Unauthorized("User not authenticated");

        await _passwordService.UnlockAccountAsync(request.UserId, adminId);

        return Ok(new { message = "Account unlocked successfully" });
    }

    /// <summary>
    /// Get password status for a specific user (admin operation)
    /// </summary>
    [HttpGet("status/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserPasswordStatus(Guid userId)
    {
        var daysUntilExpiration = await _passwordService.GetDaysUntilPasswordExpiresAsync(userId);
        var isExpired = await _passwordService.IsPasswordExpiredAsync(userId);
        var isLocked = await _passwordService.IsAccountLockedAsync(userId);

        return Ok(new
        {
            userId,
            daysUntilExpiration,
            isExpired,
            isLocked
        });
    }

    #endregion
}

#region Request DTOs

public record PasswordChangeRequest(string CurrentPassword, string NewPassword);

public record CheckPasswordStrengthRequest(string Password);

public record ValidatePasswordPolicyRequest(string Password, Guid CompanyId);

public record AdminSetPasswordRequest(
    Guid UserId,
    string Password,
    bool MustChangeOnNextLogin = true,
    bool SendEmail = false);

public record AdminResetPasswordRequest(
    Guid UserId,
    bool SendEmail = true);

public record GenerateSecurePasswordRequest(
    int Length = 12,
    bool IncludeUppercase = true,
    bool IncludeLowercase = true,
    bool IncludeDigits = true,
    bool IncludeSpecialChars = true);

public record UnlockUserAccountRequest(Guid UserId);

#endregion
