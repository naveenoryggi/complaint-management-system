using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a contact person within a partner organization
/// Can have portal access for partner management
/// </summary>
public class PartnerContact : BaseEntity
{
    #region Relationship

    /// <summary>
    /// Partner ID (foreign key)
    /// </summary>
    public Guid PartnerId { get; set; }

    #endregion

    #region Personal Information

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name (computed or stored)
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Mobile number
    /// </summary>
    public string? Mobile { get; set; }

    /// <summary>
    /// Job title
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Department within the partner organization
    /// </summary>
    public string? Department { get; set; }

    /// <summary>
    /// Role of this contact
    /// </summary>
    public ContactRole Role { get; set; } = ContactRole.Primary;

    #endregion

    #region Portal Authentication

    /// <summary>
    /// Whether this contact has portal access
    /// </summary>
    public bool IsPortalUser { get; set; }

    /// <summary>
    /// Password hash for portal login
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Portal role (Admin, User, Viewer)
    /// </summary>
    public PortalRole PortalRole { get; set; } = PortalRole.User;

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Is contact active
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Password Management

    /// <summary>
    /// Password expiry date
    /// </summary>
    public DateTime? PasswordExpiresAt { get; set; }

    /// <summary>
    /// Whether user must change password on next login
    /// </summary>
    public bool MustChangePassword { get; set; }

    /// <summary>
    /// Failed login attempt counter
    /// </summary>
    public int FailedLoginAttempts { get; set; }

    /// <summary>
    /// Account locked until this timestamp
    /// </summary>
    public DateTime? LockedUntil { get; set; }

    /// <summary>
    /// Last password change timestamp
    /// </summary>
    public DateTime? LastPasswordChangeAt { get; set; }

    #endregion

    #region SSO/External Authentication

    /// <summary>
    /// Authentication type (Local, SSO, etc.)
    /// </summary>
    public AuthenticationProviderType AuthType { get; set; } = AuthenticationProviderType.Local;

    /// <summary>
    /// External user ID for SSO
    /// </summary>
    public string? ExternalUserId { get; set; }

    #endregion

    #region Preferences

    /// <summary>
    /// Preferred language (e.g., "en-US", "hi-IN")
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Preferred timezone (IANA identifier)
    /// </summary>
    public string? PreferredTimeZone { get; set; }

    /// <summary>
    /// Whether to receive email notifications
    /// </summary>
    public bool ReceiveEmailNotifications { get; set; } = true;

    /// <summary>
    /// Whether to receive SMS notifications
    /// </summary>
    public bool ReceiveSmsNotifications { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Whether this is the primary contact for the partner
    /// </summary>
    public bool IsPrimaryContact { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent partner
    /// </summary>
    public Partner Partner { get; set; } = null!;

    #endregion
}
