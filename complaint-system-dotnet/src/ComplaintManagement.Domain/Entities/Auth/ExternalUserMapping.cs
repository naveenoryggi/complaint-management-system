using ComplaintManagement.Domain.Entities;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Auth;

/// <summary>
/// Maps external identities (AD, SSO, OAuth) to local user accounts
/// Supports Just-in-Time (JIT) user provisioning and attribute synchronization
/// </summary>
public class ExternalUserMapping : BaseEntity
{
    /// <summary>
    /// Local user account ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Authentication provider this mapping belongs to
    /// </summary>
    public Guid AuthenticationProviderId { get; set; }

    /// <summary>
    /// External user ID from the provider
    /// e.g., AD objectGUID, Azure AD user ID, SAML NameID
    /// </summary>
    public string ExternalUserId { get; set; } = string.Empty;

    /// <summary>
    /// External username
    /// e.g., AD sAMAccountName, email from OAuth
    /// </summary>
    public string? ExternalUsername { get; set; }

    /// <summary>
    /// External user's email address
    /// </summary>
    public string? ExternalEmail { get; set; }

    /// <summary>
    /// External user's display name
    /// </summary>
    public string? ExternalDisplayName { get; set; }

    /// <summary>
    /// Additional attributes from external provider (JSON format)
    /// Stores raw attributes for reference and debugging
    /// Example: {"department": "IT", "title": "Manager", "phone": "+1234567890"}
    /// </summary>
    public string? Attributes { get; set; }

    /// <summary>
    /// External groups/roles the user belongs to (JSON array)
    /// Example: ["Domain Admins", "IT Department", "All Users"]
    /// </summary>
    public string? ExternalGroups { get; set; }

    /// <summary>
    /// When this mapping was created (first login via this provider)
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When user attributes were last synced from external provider
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }

    /// <summary>
    /// Whether the last sync was successful
    /// </summary>
    public bool? LastSyncSuccess { get; set; }

    /// <summary>
    /// Details about last sync (success message or error)
    /// </summary>
    public string? LastSyncDetails { get; set; }

    /// <summary>
    /// Whether this mapping is currently active
    /// Can be disabled if user is removed from external system
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// When this external identity was last used to login
    /// </summary>
    public DateTime? LastLoginAt { get; set; }

    // ========== Navigation Properties ==========

    /// <summary>
    /// Navigation property to the local user
    /// </summary>
    public virtual User? User { get; set; }

    /// <summary>
    /// Navigation property to the authentication provider
    /// </summary>
    public virtual AuthenticationProvider? AuthenticationProvider { get; set; }
}
