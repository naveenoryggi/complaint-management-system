namespace ComplaintManagement.Application.DTOs.Auth;

/// <summary>
/// Result of a password reset request
/// </summary>
public class PasswordResetRequestResult
{
    /// <summary>
    /// Whether the request was successful
    /// Always returns true for security (don't reveal if email exists)
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// User-friendly message
    /// Generic message for security
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Internal error details (not sent to client)
    /// Used for logging and debugging
    /// </summary>
    public string? InternalError { get; set; }

    /// <summary>
    /// Whether rate limit was exceeded
    /// </summary>
    public bool RateLimitExceeded { get; set; }

    /// <summary>
    /// Minutes until next request is allowed
    /// Only set if rate limit was exceeded
    /// </summary>
    public int? MinutesUntilNextRequest { get; set; }
}
