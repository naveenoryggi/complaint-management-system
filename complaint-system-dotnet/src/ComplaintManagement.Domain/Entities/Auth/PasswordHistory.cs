using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Tracks password history for users to prevent password reuse
/// Stores the last N passwords (configurable via PasswordPolicy)
/// </summary>
public class PasswordHistory : BaseEntity
{
    /// <summary>
    /// User who owns this password history entry
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Hashed password (bcrypt/Argon2)
    /// Never store plain text passwords
    /// </summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// When this password was set
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who set this password (UserId of admin or self)
    /// Null if set during initial user creation
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// IP address from which the password was changed
    /// Used for security auditing
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// Navigation property to the user
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Navigation property to the user who created this password
    /// </summary>
    public virtual User? Creator { get; set; }
}
