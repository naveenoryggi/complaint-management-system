using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Refresh token entity for secure token rotation and long-lived authentication
/// Implements single-use tokens with family tracking to detect token theft
/// </summary>
public class RefreshToken : BaseEntity
{
    /// <summary>
    /// The actual refresh token value (cryptographically secure random string)
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// User ID that this refresh token belongs to
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation property to User
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// Timestamp when token was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when token expires (typically 7 days)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Timestamp when token was used to generate a new token
    /// Single-use tokens should be marked as used immediately
    /// </summary>
    public DateTime? UsedAt { get; set; }

    /// <summary>
    /// Timestamp when token was explicitly revoked (e.g., logout)
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// ID of the token that replaced this one (for token family tracking)
    /// Used to detect token theft - if a used token is presented, revoke entire family
    /// </summary>
    public Guid? ReplacedByTokenId { get; set; }

    /// <summary>
    /// Reason for revocation (e.g., "Logout", "Token theft detected", "Expired")
    /// </summary>
    public string? RevocationReason { get; set; }

    /// <summary>
    /// IP address from which token was created
    /// </summary>
    public string? CreatedByIp { get; set; }

    /// <summary>
    /// IP address from which token was revoked
    /// </summary>
    public string? RevokedByIp { get; set; }

    /// <summary>
    /// Token family ID - all tokens in refresh chain share same family ID
    /// Used to revoke entire token family if theft is detected
    /// </summary>
    public Guid TokenFamily { get; set; }

    /// <summary>
    /// Check if token is currently active (not expired, used, or revoked)
    /// </summary>
    public bool IsActive => RevokedAt == null && UsedAt == null && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Check if token is expired
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;

    /// <summary>
    /// Check if token has been revoked
    /// </summary>
    public bool IsRevoked => RevokedAt != null;

    /// <summary>
    /// Check if token has been used
    /// </summary>
    public bool IsUsed => UsedAt != null;
}
