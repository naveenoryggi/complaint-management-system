namespace ComplaintManagement.Application.DTOs.Auth;

/// <summary>
/// Result of a password reset operation
/// </summary>
public class PasswordResetResult
{
    /// <summary>
    /// Whether the reset was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// User-friendly message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Validation errors (if password doesn't meet policy)
    /// </summary>
    public List<string> ValidationErrors { get; set; } = new();

    /// <summary>
    /// Whether the error was due to token expiration
    /// </summary>
    public bool TokenExpired { get; set; }

    /// <summary>
    /// Whether the error was due to token already being used
    /// </summary>
    public bool TokenAlreadyUsed { get; set; }
}
