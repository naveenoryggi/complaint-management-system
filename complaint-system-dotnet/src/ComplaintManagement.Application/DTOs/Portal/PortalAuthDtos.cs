using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.DTOs.Portal;

/// <summary>
/// Portal login request
/// </summary>
public class PortalLoginRequest
{
    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Password
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Remember me flag for extended session
    /// </summary>
    public bool RememberMe { get; set; }
}

/// <summary>
/// Portal login response
/// </summary>
public class PortalLoginResponse
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public PortalUserDto? User { get; set; }
    public bool MustChangePassword { get; set; }
    public bool IsAccountLocked { get; set; }
    public DateTime? LockedUntil { get; set; }
}

/// <summary>
/// Portal user information returned after login
/// </summary>
public class PortalUserDto
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public PortalRole PortalRole { get; set; }
    public string PortalRoleName => PortalRole.ToString();
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public PortalPermissionsDto Permissions { get; set; } = new();
}

/// <summary>
/// Portal user permissions
/// </summary>
public class PortalPermissionsDto
{
    public bool CanCreateTickets { get; set; }
    public bool CanViewAllLocationTickets { get; set; }
    public bool CanViewAllCustomerTickets { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; }
    public bool CanViewAssets { get; set; }
    public bool CanApproveEscalations { get; set; }
    public bool CanDownloadReports { get; set; }
}

/// <summary>
/// Portal token refresh request
/// </summary>
public class PortalRefreshTokenRequest
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Portal change password request
/// </summary>
public class PortalChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Portal forgot password request
/// </summary>
public class PortalForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Portal reset password request
/// </summary>
public class PortalResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}

/// <summary>
/// Portal profile update request
/// </summary>
public class UpdatePortalProfileRequest
{
    public string? Phone { get; set; }
    public string? Mobile { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }
    public bool ReceiveEmailNotifications { get; set; }
    public bool ReceiveSmsNotifications { get; set; }
}

/// <summary>
/// Portal refresh token entity for persistence
/// </summary>
public class PortalRefreshTokenDto
{
    public Guid Id { get; set; }
    public Guid ContactId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
