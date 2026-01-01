using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a partner (System Integrator, Reseller, Distributor, etc.)
/// Partners can manage customers and have portal access
/// </summary>
public class Partner : BaseEntity
{
    #region Identity

    /// <summary>
    /// Service provider company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Partner code (e.g., PTR-001)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Partner display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Legal/registered name
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// Type of partnership
    /// </summary>
    public PartnerType Type { get; set; } = PartnerType.Reseller;

    /// <summary>
    /// Partner tier/level
    /// </summary>
    public PartnerTier Tier { get; set; } = PartnerTier.Standard;

    #endregion

    #region Contact Information

    /// <summary>
    /// Primary email address
    /// </summary>
    public string PrimaryEmail { get; set; } = string.Empty;

    /// <summary>
    /// Primary phone number
    /// </summary>
    public string? PrimaryPhone { get; set; }

    /// <summary>
    /// Company website
    /// </summary>
    public string? Website { get; set; }

    #endregion

    #region Address

    /// <summary>
    /// Address line 1
    /// </summary>
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Address line 2
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    public string? City { get; set; }

    /// <summary>
    /// State/Province
    /// </summary>
    public string? State { get; set; }

    /// <summary>
    /// Country
    /// </summary>
    public string? Country { get; set; }

    /// <summary>
    /// Postal/ZIP code
    /// </summary>
    public string? PostalCode { get; set; }

    #endregion

    #region Business Information

    /// <summary>
    /// Tax ID (GST/VAT number)
    /// </summary>
    public string? TaxId { get; set; }

    /// <summary>
    /// PAN number (India-specific)
    /// </summary>
    public string? PanNumber { get; set; }

    /// <summary>
    /// Industry segment
    /// </summary>
    public string? IndustrySegment { get; set; }

    /// <summary>
    /// Date when partnership started
    /// </summary>
    public DateTime? PartnerSince { get; set; }

    /// <summary>
    /// Credit limit for the partner
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Payment terms (e.g., "Net 30", "Net 60")
    /// </summary>
    public string? PaymentTerms { get; set; }

    /// <summary>
    /// Default discount percentage for the partner
    /// </summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>
    /// Commission percentage for sales
    /// </summary>
    public decimal? CommissionPercent { get; set; }

    #endregion

    #region Portal Settings

    /// <summary>
    /// Whether partner portal access is enabled
    /// </summary>
    public bool PortalEnabled { get; set; } = true;

    /// <summary>
    /// Whether partner can manage their customers
    /// </summary>
    public bool CanManageCustomers { get; set; } = true;

    /// <summary>
    /// Whether partner can view all their customer tickets
    /// </summary>
    public bool CanViewAllCustomerTickets { get; set; } = true;

    /// <summary>
    /// Whether partner can create tickets on behalf of customers
    /// </summary>
    public bool CanCreateCustomerTickets { get; set; } = true;

    /// <summary>
    /// Custom branding settings (JSON: logo, colors)
    /// </summary>
    public string? PortalBranding { get; set; }

    #endregion

    #region SSO/External Integration

    /// <summary>
    /// Authentication provider for SSO
    /// </summary>
    public Guid? AuthenticationProviderId { get; set; }

    /// <summary>
    /// External partner ID (CRM/ERP reference)
    /// </summary>
    public string? ExternalPartnerId { get; set; }

    #endregion

    #region Account Management

    /// <summary>
    /// Internal account manager for this partner
    /// </summary>
    public Guid? AccountManagerId { get; set; }

    /// <summary>
    /// Secondary/backup account manager
    /// </summary>
    public Guid? SecondaryAccountManagerId { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Partner status
    /// </summary>
    public PartnerStatus Status { get; set; } = PartnerStatus.Active;

    /// <summary>
    /// Status change reason
    /// </summary>
    public string? StatusReason { get; set; }

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
    /// Service provider company
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Authentication provider for SSO
    /// </summary>
    public AuthenticationProvider? AuthenticationProvider { get; set; }

    /// <summary>
    /// Internal account manager
    /// </summary>
    public User? AccountManager { get; set; }

    /// <summary>
    /// Secondary account manager
    /// </summary>
    public User? SecondaryAccountManager { get; set; }

    /// <summary>
    /// Partner-Customer relationships
    /// </summary>
    public ICollection<PartnerCustomer> PartnerCustomers { get; set; } = new List<PartnerCustomer>();

    /// <summary>
    /// Partner contacts
    /// </summary>
    public ICollection<PartnerContact> Contacts { get; set; } = new List<PartnerContact>();

    #endregion
}
