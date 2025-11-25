using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Comprehensive audit log for all password-related operations
/// Used for security monitoring, compliance, and troubleshooting
/// </summary>
public class PasswordAuditLog : BaseEntity
{
    /// <summary>
    /// User whose password was affected
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Type of password action performed
    /// </summary>
    public PasswordAction Action { get; set; }

    /// <summary>
    /// Who performed the action (admin UserId or self)
    /// Null for system-initiated actions (e.g., auto-expiration)
    /// </summary>
    public Guid? PerformedBy { get; set; }

    /// <summary>
    /// Whether the action succeeded or failed
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Additional details about the action
    /// e.g., failure reason, email sent confirmation
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// IP address from which the action was performed
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User agent string from the request
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// When the action occurred
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Navigation property to the affected user
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Navigation property to the user who performed the action
    /// </summary>
    public virtual User? Performer { get; set; }
}
