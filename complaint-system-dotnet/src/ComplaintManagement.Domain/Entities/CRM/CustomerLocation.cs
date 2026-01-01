using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a physical location of a customer (multi-site support)
/// Assets and contacts can be associated with specific locations
/// </summary>
public class CustomerLocation : BaseEntity
{
    #region Relationship

    /// <summary>
    /// Customer ID (foreign key)
    /// </summary>
    public Guid CustomerId { get; set; }

    #endregion

    #region Identity

    /// <summary>
    /// Location code (e.g., LOC-001)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Location name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Location type
    /// </summary>
    public LocationType Type { get; set; } = LocationType.Branch;

    /// <summary>
    /// Location description
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Address

    /// <summary>
    /// Address line 1
    /// </summary>
    public string AddressLine1 { get; set; } = string.Empty;

    /// <summary>
    /// Address line 2
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string City { get; set; } = string.Empty;

    /// <summary>
    /// State/Province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string Country { get; set; } = string.Empty;

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string? PostalCode { get; set; }

    #endregion

    #region Geolocation

    /// <summary>
    /// Latitude coordinate
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// Longitude coordinate
    /// </summary>
    public double? Longitude { get; set; }

    /// <summary>
    /// Timezone (IANA identifier)
    /// </summary>
    public string? TimeZone { get; set; }

    #endregion

    #region Contact Information

    /// <summary>
    /// Location contact email
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Location contact phone
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Location fax number
    /// </summary>
    public string? Fax { get; set; }

    #endregion

    #region Status Flags

    /// <summary>
    /// Whether this is the primary/main location
    /// </summary>
    public bool IsPrimary { get; set; }

    /// <summary>
    /// Whether location is active
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Whether this is the billing address
    /// </summary>
    public bool IsBillingAddress { get; set; }

    /// <summary>
    /// Whether this is the shipping address
    /// </summary>
    public bool IsShippingAddress { get; set; }

    #endregion

    #region Operational Details

    /// <summary>
    /// Operating hours (JSON: { "monday": "9:00-18:00", ... })
    /// </summary>
    public string? OperatingHours { get; set; }

    /// <summary>
    /// Site access instructions
    /// </summary>
    public string? AccessInstructions { get; set; }

    /// <summary>
    /// Emergency contact name
    /// </summary>
    public string? EmergencyContactName { get; set; }

    /// <summary>
    /// Emergency contact phone
    /// </summary>
    public string? EmergencyContactPhone { get; set; }

    #endregion

    #region Additional Information

    /// <summary>
    /// External location ID (ERP/CRM reference)
    /// </summary>
    public string? ExternalLocationId { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Tags for categorization (JSON array)
    /// </summary>
    public string? Tags { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent customer
    /// </summary>
    public Customer Customer { get; set; } = null!;

    /// <summary>
    /// Contacts at this location
    /// </summary>
    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();

    /// <summary>
    /// Assets installed at this location
    /// </summary>
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();

    #endregion
}
