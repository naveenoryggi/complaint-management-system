using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents a user in the system (synced from Oryggi HRMS)
/// </summary>
public class User : BaseEntity
{
    /// <summary>
    /// Company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Branch ID (foreign key)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Department ID (foreign key)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Section ID (foreign key)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Employee type ID (foreign key)
    /// </summary>
    public Guid? EmployeeTypeId { get; set; }

    /// <summary>
    /// Employee code/number from Oryggi
    /// </summary>
    public string EmployeeCode { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Phone number
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Alternate phone number
    /// </summary>
    public string? AlternatePhone { get; set; }

    /// <summary>
    /// Job title/position
    /// </summary>
    public string? JobTitle { get; set; }

    /// <summary>
    /// Date of joining
    /// </summary>
    public DateTime? DateOfJoining { get; set; }

    /// <summary>
    /// Date of birth
    /// </summary>
    public DateTime? DateOfBirth { get; set; }

    /// <summary>
    /// Direct manager/supervisor user ID
    /// </summary>
    public Guid? ManagerId { get; set; }

    /// <summary>
    /// Is user active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Password hash for authentication
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// Last login timestamp
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    /// <summary>
    /// Oryggi employee ID for sync
    /// </summary>
    public string? OryggiEmployeeId { get; set; }

    /// <summary>
    /// Last sync timestamp from Oryggi
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    // ========== Password Management Properties ==========

    /// <summary>
    /// When the password expires (based on PasswordPolicy)
    /// Null if password never expires
    /// </summary>
    public DateTime? PasswordExpiresAt { get; set; }

    /// <summary>
    /// User must change password on next login
    /// Set by admin when setting/resetting password
    /// </summary>
    public bool MustChangePasswordOnNextLogin { get; set; } = false;

    /// <summary>
    /// Password never expires (overrides PasswordPolicy)
    /// Typically set for service accounts
    /// </summary>
    public bool PasswordNeverExpires { get; set; } = false;

    /// <summary>
    /// When the password was last changed
    /// </summary>
    public DateTime? PasswordChangedAt { get; set; }

    /// <summary>
    /// Who changed the password last (UserId)
    /// Null if user changed their own password
    /// </summary>
    public Guid? PasswordChangedBy { get; set; }

    /// <summary>
    /// Number of consecutive failed login attempts
    /// Reset to 0 on successful login
    /// </summary>
    public int FailedLoginAttempts { get; set; } = 0;

    /// <summary>
    /// When the account will be unlocked
    /// Null if account is not locked
    /// </summary>
    public DateTime? AccountLockedUntil { get; set; }

    /// <summary>
    /// When the last password expiration notification was sent
    /// Used to avoid spamming users with warnings
    /// </summary>
    public DateTime? LastPasswordChangeRequiredNotificationSentAt { get; set; }

    // ========== Authentication Provider Properties ==========

    /// <summary>
    /// Primary authentication provider for this user
    /// Default: Local (email + password)
    /// </summary>
    public AuthenticationProviderType AuthenticationProviderType { get; set; } = AuthenticationProviderType.Local;

    /// <summary>
    /// External user ID from authentication provider
    /// e.g., AD objectGUID, Azure AD user ID, SAML NameID
    /// </summary>
    public string? ExternalUserId { get; set; }

    /// <summary>
    /// External username
    /// e.g., AD sAMAccountName
    /// </summary>
    public string? ExternalUsername { get; set; }

    /// <summary>
    /// Identity provider name/domain
    /// e.g., "company.com", "AzureAD", "GoogleSSO"
    /// </summary>
    public string? IdentityProvider { get; set; }

    /// <summary>
    /// When user attributes were last synced from external provider
    /// </summary>
    public DateTime? LastExternalSyncAt { get; set; }

    /// <summary>
    /// Enable synchronization with external provider
    /// </summary>
    public bool ExternalSyncEnabled { get; set; } = true;

    /// <summary>
    /// Enable SSO for this user
    /// </summary>
    public bool SSOEnabled { get; set; } = true;

    /// <summary>
    /// Enable local password authentication
    /// Can be used as fallback if SSO is unavailable
    /// </summary>
    public bool LocalPasswordEnabled { get; set; } = true;

    /// <summary>
    /// User's preferred authentication method
    /// e.g., "AzureAD", "Local", "SAML"
    /// </summary>
    public string? PreferredAuthMethod { get; set; }

    // ========== Timezone & Localization Properties ==========

    /// <summary>
    /// User's preferred timezone (IANA timezone identifier)
    /// e.g., "America/New_York", "Europe/London", "Asia/Tokyo"
    /// If null, falls back to Branch timezone, then Company default
    /// </summary>
    public string? PreferredTimeZone { get; set; }

    /// <summary>
    /// User's preferred locale/language
    /// e.g., "en-US", "fr-FR", "ar-SA"
    /// </summary>
    public string? PreferredLocale { get; set; }

    /// <summary>
    /// User's preferred date format
    /// e.g., "MM/dd/yyyy", "dd/MM/yyyy", "yyyy-MM-dd"
    /// </summary>
    public string? PreferredDateFormat { get; set; }

    /// <summary>
    /// User's preferred time format (12-hour or 24-hour)
    /// e.g., "hh:mm a", "HH:mm"
    /// </summary>
    public string? PreferredTimeFormat { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent company
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Parent branch
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Parent department
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Parent section
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Direct manager
    /// </summary>
    public User? Manager { get; set; }

    /// <summary>
    /// Subordinates (direct reports)
    /// </summary>
    public ICollection<User> Subordinates { get; set; } = new List<User>();

    /// <summary>
    /// Complaints submitted by this user
    /// </summary>
    public ICollection<Complaint> SubmittedComplaints { get; set; } = new List<Complaint>();

    /// <summary>
    /// Complaints assigned to this user
    /// </summary>
    public ICollection<Complaint> AssignedComplaints { get; set; } = new List<Complaint>();

    /// <summary>
    /// Complaint roles assigned to this user
    /// </summary>
    public ICollection<UserComplaintRole> UserComplaintRoles { get; set; } = new List<UserComplaintRole>();

    /// <summary>
    /// Comments made by this user
    /// </summary>
    public ICollection<ComplaintComment> Comments { get; set; } = new List<ComplaintComment>();

    /// <summary>
    /// Password history for this user
    /// </summary>
    public ICollection<PasswordHistory> PasswordHistories { get; set; } = new List<PasswordHistory>();

    /// <summary>
    /// Password audit logs for this user
    /// </summary>
    public ICollection<PasswordAuditLog> PasswordAuditLogs { get; set; } = new List<PasswordAuditLog>();

    /// <summary>
    /// External user mappings for this user (SSO/AD identities)
    /// </summary>
    public ICollection<ExternalUserMapping> ExternalUserMappings { get; set; } = new List<ExternalUserMapping>();
}
