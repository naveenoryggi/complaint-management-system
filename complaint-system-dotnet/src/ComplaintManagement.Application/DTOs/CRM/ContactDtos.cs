using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.DTOs.CRM;

/// <summary>
/// Customer contact DTO for API responses
/// </summary>
public class CustomerContactDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }
    public string RoleName => Role.ToString();

    // Portal Access
    public bool IsPortalUser { get; set; }
    public PortalRole PortalRole { get; set; }
    public string PortalRoleName => PortalRole.ToString();
    public bool IsActive { get; set; }
    public bool IsPrimaryContact { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }

    // Authentication
    public AuthenticationProviderType AuthType { get; set; }
    public string AuthTypeName => AuthType.ToString();
    public string? ExternalUserId { get; set; }

    // Permissions
    public bool CanCreateTickets { get; set; }
    public bool CanViewAllLocationTickets { get; set; }
    public bool CanViewAllCustomerTickets { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; }
    public bool CanViewAssets { get; set; }
    public bool CanApproveEscalations { get; set; }
    public bool CanDownloadReports { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }
    public string? NotificationPreferences { get; set; }

    // Metadata
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Customer contact summary for list views
/// </summary>
public class CustomerContactSummaryDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public ContactRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsPortalUser { get; set; }
    public PortalRole PortalRole { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimaryContact { get; set; }
}

/// <summary>
/// Request to create a new customer contact
/// </summary>
public class CreateCustomerContactRequest
{
    public Guid CustomerId { get; set; }
    public Guid? LocationId { get; set; }

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; } = ContactRole.Technical;

    // Portal Access
    public bool IsPortalUser { get; set; }
    public string? Password { get; set; }  // Only if creating portal user
    public PortalRole PortalRole { get; set; } = PortalRole.User;
    public bool IsPrimaryContact { get; set; }

    // Authentication
    public AuthenticationProviderType AuthType { get; set; } = AuthenticationProviderType.Local;
    public string? ExternalUserId { get; set; }

    // Permissions
    public bool CanCreateTickets { get; set; } = true;
    public bool CanViewAllLocationTickets { get; set; }
    public bool CanViewAllCustomerTickets { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; } = true;
    public bool CanViewAssets { get; set; } = true;
    public bool CanApproveEscalations { get; set; }
    public bool CanDownloadReports { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; } = true;
    public bool ReceiveSmsNotifications { get; set; }
    public string? NotificationPreferences { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing customer contact
/// </summary>
public class UpdateCustomerContactRequest
{
    public Guid? LocationId { get; set; }

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }

    // Portal Access
    public bool IsPortalUser { get; set; }
    public PortalRole PortalRole { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimaryContact { get; set; }

    // Authentication
    public AuthenticationProviderType AuthType { get; set; }
    public string? ExternalUserId { get; set; }

    // Permissions
    public bool CanCreateTickets { get; set; }
    public bool CanViewAllLocationTickets { get; set; }
    public bool CanViewAllCustomerTickets { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; }
    public bool CanViewAssets { get; set; }
    public bool CanApproveEscalations { get; set; }
    public bool CanDownloadReports { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }
    public string? NotificationPreferences { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Partner contact DTO for API responses
/// </summary>
public class PartnerContactDto
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public string PartnerName { get; set; } = string.Empty;

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }
    public string RoleName => Role.ToString();

    // Portal Access
    public bool IsPortalUser { get; set; }
    public PortalRole PortalRole { get; set; }
    public string PortalRoleName => PortalRole.ToString();
    public bool IsActive { get; set; }
    public bool IsPrimaryContact { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockedUntil { get; set; }

    // Authentication
    public AuthenticationProviderType AuthType { get; set; }
    public string AuthTypeName => AuthType.ToString();
    public string? ExternalUserId { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }

    // Metadata
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Partner contact summary for list views
/// </summary>
public class PartnerContactSummaryDto
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public ContactRole Role { get; set; }
    public string RoleName => Role.ToString();
    public bool IsPortalUser { get; set; }
    public PortalRole PortalRole { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimaryContact { get; set; }
}

/// <summary>
/// Request to create a new partner contact
/// </summary>
public class CreatePartnerContactRequest
{
    public Guid PartnerId { get; set; }

    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; } = ContactRole.Technical;

    // Portal Access
    public bool IsPortalUser { get; set; }
    public string? Password { get; set; }  // Only if creating portal user
    public PortalRole PortalRole { get; set; } = PortalRole.User;
    public bool IsPrimaryContact { get; set; }

    // Authentication
    public AuthenticationProviderType AuthType { get; set; } = AuthenticationProviderType.Local;
    public string? ExternalUserId { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; } = true;
    public bool ReceiveSmsNotifications { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing partner contact
/// </summary>
public class UpdatePartnerContactRequest
{
    // Personal Information
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? JobTitle { get; set; }
    public string? Department { get; set; }
    public ContactRole Role { get; set; }

    // Portal Access
    public bool IsPortalUser { get; set; }
    public PortalRole PortalRole { get; set; }
    public bool IsActive { get; set; }
    public bool IsPrimaryContact { get; set; }

    // Authentication
    public AuthenticationProviderType AuthType { get; set; }
    public string? ExternalUserId { get; set; }

    // Preferences
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Contact dropdown/lookup item (works for both customer and partner contacts)
/// </summary>
public class ContactLookupDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public string Email { get; set; } = string.Empty;
    public ContactRole Role { get; set; }
    public bool IsPortalUser { get; set; }
    public bool IsPrimaryContact { get; set; }
}

/// <summary>
/// Request to set/change contact password
/// </summary>
public class SetContactPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
    public bool MustChangeOnLogin { get; set; }
}

/// <summary>
/// Request to reset contact password (generates reset link/token)
/// </summary>
public class ResetContactPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request to unlock a locked contact account
/// </summary>
public class UnlockContactRequest
{
    public bool ResetFailedAttempts { get; set; } = true;
}
