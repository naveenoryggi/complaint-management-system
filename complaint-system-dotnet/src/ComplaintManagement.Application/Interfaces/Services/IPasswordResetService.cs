using ComplaintManagement.Application.DTOs.Auth;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for handling self-service password reset functionality
/// Provides secure token-based password recovery
/// </summary>
public interface IPasswordResetService
{
    /// <summary>
    /// Request a password reset token
    /// Generates a token and sends reset link via email
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="ipAddress">IP address of the requester (for audit)</param>
    /// <param name="userAgent">User agent of the requester (for audit)</param>
    /// <returns>Result with success status and message</returns>
    Task<PasswordResetRequestResult> RequestPasswordResetAsync(
        string email,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Validate a password reset token
    /// Checks if token exists, is not expired, and has not been used
    /// </summary>
    /// <param name="token">Reset token</param>
    /// <returns>Validation result with user email if valid</returns>
    Task<PasswordResetTokenValidationResult> ValidateResetTokenAsync(string token);

    /// <summary>
    /// Reset password using a valid token
    /// Validates token, updates password, and marks token as used
    /// </summary>
    /// <param name="token">Reset token</param>
    /// <param name="newPassword">New password</param>
    /// <param name="ipAddress">IP address of the requester (for audit)</param>
    /// <param name="userAgent">User agent of the requester (for audit)</param>
    /// <returns>Result with success status and message</returns>
    Task<PasswordResetResult> ResetPasswordAsync(
        string token,
        string newPassword,
        string? ipAddress = null,
        string? userAgent = null);

    /// <summary>
    /// Clean up expired password reset tokens
    /// Should be called periodically by a background job
    /// </summary>
    /// <returns>Number of tokens cleaned up</returns>
    Task<int> CleanupExpiredTokensAsync();

    /// <summary>
    /// Check rate limiting for password reset requests
    /// Prevents abuse by limiting requests per email per time period
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <returns>True if request is allowed, false if rate limit exceeded</returns>
    Task<bool> CheckRateLimitAsync(string email);
}
