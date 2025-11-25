using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Company-wide password policy configuration
/// Controls password complexity, expiration, and security settings
/// </summary>
public class PasswordPolicy : BaseEntity
{
    /// <summary>
    /// Company this policy applies to
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Minimum password length
    /// Default: 8 characters
    /// </summary>
    public int MinimumLength { get; set; } = 8;

    /// <summary>
    /// Require at least one uppercase letter
    /// Default: true
    /// </summary>
    public bool RequireUppercase { get; set; } = true;

    /// <summary>
    /// Require at least one lowercase letter
    /// Default: true
    /// </summary>
    public bool RequireLowercase { get; set; } = true;

    /// <summary>
    /// Require at least one digit (0-9)
    /// Default: true
    /// </summary>
    public bool RequireDigit { get; set; } = true;

    /// <summary>
    /// Require at least one special character (!@#$%^&*, etc.)
    /// Default: true
    /// </summary>
    public bool RequireSpecialCharacter { get; set; } = true;

    /// <summary>
    /// Number of days until password expires
    /// 0 = never expires
    /// Default: 90 days
    /// </summary>
    public int PasswordExpirationDays { get; set; } = 90;

    /// <summary>
    /// Number of days before expiration to start sending warnings
    /// Default: 7 days
    /// </summary>
    public int PasswordExpirationWarningDays { get; set; } = 7;

    /// <summary>
    /// Maximum number of failed login attempts before account lockout
    /// Default: 5 attempts
    /// </summary>
    public int MaxFailedLoginAttempts { get; set; } = 5;

    /// <summary>
    /// Number of minutes to lock account after max failed attempts
    /// Default: 15 minutes
    /// </summary>
    public int AccountLockoutDurationMinutes { get; set; } = 15;

    /// <summary>
    /// Number of previous passwords to remember and prevent reuse
    /// Default: 5 passwords
    /// </summary>
    public int PasswordHistoryCount { get; set; } = 5;

    /// <summary>
    /// Minimum number of days before user can change password again
    /// Prevents rapid password cycling to bypass history
    /// 0 = can change anytime
    /// Default: 0
    /// </summary>
    public int MinimumPasswordAgeDays { get; set; } = 0;

    /// <summary>
    /// Enable password complexity checking
    /// Default: true
    /// </summary>
    public bool EnablePasswordComplexity { get; set; } = true;

    /// <summary>
    /// Allow users to skip password change when forced
    /// Default: false (users must change)
    /// </summary>
    public bool AllowSkipPasswordChange { get; set; } = false;

    /// <summary>
    /// Send email notifications for password expiration warnings
    /// Default: true
    /// </summary>
    public bool SendPasswordExpirationEmails { get; set; } = true;

    /// <summary>
    /// Send email when admin sets/resets password
    /// Default: true
    /// </summary>
    public bool SendPasswordSetEmails { get; set; } = true;

    /// <summary>
    /// When this policy was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this policy
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// When this policy was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Who last updated this policy
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Navigation property to the company
    /// </summary>
    public virtual Company? Company { get; set; }
}
