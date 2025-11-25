namespace ComplaintManagement.Domain.Entities.MasterData;

/// <summary>
/// Represents a tenant in the multi-tenant system
/// </summary>
public class Tenant : BaseEntity
{
    /// <summary>
    /// Tenant name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Tenant code/identifier
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Tenant description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Contact email
    /// </summary>
    public string ContactEmail { get; set; } = string.Empty;

    /// <summary>
    /// Contact phone
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Tenant address
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Is tenant active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oryggi tenant ID for sync
    /// </summary>
    public string? OryggiTenantId { get; set; }

    // Navigation properties
    /// <summary>
    /// Companies belonging to this tenant
    /// </summary>
    public ICollection<Company> Companies { get; set; } = new List<Company>();
}
