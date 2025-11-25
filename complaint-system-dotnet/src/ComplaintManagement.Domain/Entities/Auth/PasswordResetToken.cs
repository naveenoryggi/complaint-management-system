using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Represents a password reset token for self-service password recovery
/// Tokens are single-use and expire after a configured time period (default: 24 hours)
/// </summary>
public class PasswordResetToken : BaseEntity
{
    /// <summary>
    /// Unique token (GUID) sent to user's email
    /// URL-safe and cryptographically random
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// User who requested the password reset
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email address where reset link was sent
    /// Stored for audit purposes and validation
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// When the token expires
    /// Default: 24 hours from creation
    /// Configurable via appsettings.json
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Whether the token has been used
    /// Tokens are single-use only for security
    /// </summary>
    public bool IsUsed { get; set; } = false;

    /// <summary>
    /// When the token was used (password reset completed)
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// IP address from which the reset was requested
    /// Used for security auditing and fraud detection
    /// </summary>
    public string? RequestIpAddress { get; set; }

    /// <summary>
    /// IP address from which the password was reset
    /// Used for security auditing and fraud detection
    /// </summary>
    public string? ResetIpAddress { get; set; }

    /// <summary>
    /// User agent (browser) from which the reset was requested
    /// Used for security auditing
    /// </summary>
    public string? RequestUserAgent { get; set; }

    /// <summary>
    /// User agent (browser) from which the password was reset
    /// Used for security auditing
    /// </summary>
    public string? ResetUserAgent { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Check if token is valid (not expired and not used)
    /// </summary>
    public bool IsValid()
    {
        return !IsUsed && DateTime.UtcNow < ExpiresAt;
    }

    /// <summary>
    /// Mark token as used
    /// </summary>
    public void MarkAsUsed(string? ipAddress = null, string? userAgent = null)
    {
        IsUsed = true;
        UsedAt = DateTime.UtcNow;
        ResetIpAddress = ipAddress;
        ResetUserAgent = userAgent;
    }
}
