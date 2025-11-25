namespace ComplaintManagement.Application.DTOs.Auth;

/// <summary>
/// Result of password reset token validation
/// </summary>
public class PasswordResetTokenValidationResult
{
    /// <summary>
    /// Whether the token is valid
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// User's email address (only if valid)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// User ID (only if valid)
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Error message if invalid
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// When the token expires (only if valid)
    /// </summary>
    public DateTime? ExpiresAt { get; set; }
}
