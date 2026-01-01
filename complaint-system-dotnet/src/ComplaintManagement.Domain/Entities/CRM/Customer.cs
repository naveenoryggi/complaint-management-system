using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents an end customer (can be direct or partner-managed)
/// Customers can have multiple locations and contacts
/// </summary>
public class Customer : BaseEntity
{
    #region Identity

    /// <summary>
    /// Service provider company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Customer code (e.g., CUST-001)
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Customer display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Legal/registered name
    /// </summary>
    public string? LegalName { get; set; }

    /// <summary>
    /// Type of customer relationship
    /// </summary>
    public CustomerType Type { get; set; } = CustomerType.Direct;

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

    #region Billing Address

    /// <summary>
    /// Billing address line 1
    /// </summary>
    public string? BillingAddressLine1 { get; set; }

    /// <summary>
    /// Billing address line 2
    /// </summary>
    public string? BillingAddressLine2 { get; set; }

    /// <summary>
    /// Billing city
    /// </summary>
    public string? BillingCity { get; set; }

    /// <summary>
    /// Billing state/province
    /// </summary>
    public string? BillingState { get; set; }

    /// <summary>
    /// Billing country
    /// </summary>
    public string? BillingCountry { get; set; }

    /// <summary>
    /// Billing postal/ZIP code
    /// </summary>
    public string? BillingPostalCode { get; set; }

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
    /// Industry vertical
    /// </summary>
    public string? Industry { get; set; }

    /// <summary>
    /// Customer segment (SMB, Enterprise, Government, etc.)
    /// </summary>
    public CustomerSegment? Segment { get; set; }

    /// <summary>
    /// Number of employees
    /// </summary>
    public int? EmployeeCount { get; set; }

    /// <summary>
    /// Annual revenue
    /// </summary>
    public decimal? AnnualRevenue { get; set; }

    /// <summary>
    /// Currency for transactions
    /// </summary>
    public string Currency { get; set; } = "INR";

    #endregion

    #region Account Information

    /// <summary>
    /// Date when customer relationship started
    /// </summary>
    public DateTime? CustomerSince { get; set; }

    /// <summary>
    /// Internal account manager for this customer
    /// </summary>
    public Guid? AccountManagerId { get; set; }

    /// <summary>
    /// Secondary/backup account manager
    /// </summary>
    public Guid? SecondaryAccountManagerId { get; set; }

    /// <summary>
    /// Customer status
    /// </summary>
    public CustomerStatus Status { get; set; } = CustomerStatus.Active;

    /// <summary>
    /// Credit terms (e.g., "Net 30")
    /// </summary>
    public string? CreditTerms { get; set; }

    /// <summary>
    /// Credit limit
    /// </summary>
    public decimal? CreditLimit { get; set; }

    /// <summary>
    /// Customer rating/score (1-5)
    /// </summary>
    public int? CustomerRating { get; set; }

    #endregion

    #region Portal Settings

    /// <summary>
    /// Whether customer portal access is enabled
    /// </summary>
    public bool PortalEnabled { get; set; } = true;

    /// <summary>
    /// Preferred language (e.g., "en-US", "hi-IN")
    /// </summary>
    public string? PreferredLanguage { get; set; }

    /// <summary>
    /// Preferred timezone (IANA identifier)
    /// </summary>
    public string? PreferredTimeZone { get; set; }

    #endregion

    #region SSO/External Integration

    /// <summary>
    /// Authentication provider for SSO
    /// </summary>
    public Guid? AuthenticationProviderId { get; set; }

    /// <summary>
    /// External customer ID (CRM/ERP reference)
    /// </summary>
    public string? ExternalCustomerId { get; set; }

    #endregion

    #region Additional Information

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

    /// <summary>
    /// Custom fields (JSON object)
    /// </summary>
    public string? CustomFields { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Service provider company
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Internal account manager
    /// </summary>
    public User? AccountManager { get; set; }

    /// <summary>
    /// Secondary account manager
    /// </summary>
    public User? SecondaryAccountManager { get; set; }

    /// <summary>
    /// Authentication provider for SSO
    /// </summary>
    public AuthenticationProvider? AuthenticationProvider { get; set; }

    /// <summary>
    /// Partner-Customer relationships
    /// </summary>
    public ICollection<PartnerCustomer> PartnerCustomers { get; set; } = new List<PartnerCustomer>();

    /// <summary>
    /// Customer locations
    /// </summary>
    public ICollection<CustomerLocation> Locations { get; set; } = new List<CustomerLocation>();

    /// <summary>
    /// Customer contacts
    /// </summary>
    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();

    /// <summary>
    /// Service contracts for this customer
    /// </summary>
    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    /// <summary>
    /// Assets owned by this customer
    /// </summary>
    public ICollection<Asset> Assets { get; set; } = new List<Asset>();

    /// <summary>
    /// Projects for this customer
    /// </summary>
    public ICollection<Project> Projects { get; set; } = new List<Project>();

    #endregion
}
