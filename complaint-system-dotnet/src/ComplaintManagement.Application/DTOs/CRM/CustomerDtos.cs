using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.DTOs.CRM;

/// <summary>
/// Customer information DTO for API responses
/// </summary>
public class CustomerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public CustomerType Type { get; set; }
    public string TypeName => Type.ToString();
    public CustomerStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Contact Information
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    // Billing Address
    public string? BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }

    // Business Information
    public string? TaxId { get; set; }
    public string? PanNumber { get; set; }
    public string? Industry { get; set; }
    public CustomerSegment? Segment { get; set; }
    public string? SegmentName => Segment?.ToString();
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string Currency { get; set; } = "INR";

    // Account Information
    public DateTime? CustomerSince { get; set; }
    public string? CreditTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public int CustomerRating { get; set; }
    public string? StatusReason { get; set; }

    // Account Management
    public Guid? AccountManagerId { get; set; }
    public string? AccountManagerName { get; set; }
    public Guid? SecondaryAccountManagerId { get; set; }
    public string? SecondaryAccountManagerName { get; set; }

    // Portal Settings
    public bool PortalEnabled { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }

    // External References
    public string? ExternalCustomerId { get; set; }
    public Guid? AuthenticationProviderId { get; set; }

    // Partners (if partner-managed)
    public List<PartnerCustomerDto> Partners { get; set; } = new();
    public PartnerSummaryDto? PrimaryPartner { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }

    // Statistics
    public int LocationCount { get; set; }
    public int ContactCount { get; set; }
    public int ActiveTicketCount { get; set; }
    public int ContractCount { get; set; }
    public int AssetCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Customer summary for list views
/// </summary>
public class CustomerSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CustomerType Type { get; set; }
    public string TypeName => Type.ToString();
    public CustomerStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? BillingCity { get; set; }
    public string? BillingCountry { get; set; }
    public CustomerSegment? Segment { get; set; }
    public string? SegmentName => Segment?.ToString();
    public string? PrimaryPartnerName { get; set; }
    public int LocationCount { get; set; }
    public int ActiveTicketCount { get; set; }
    public DateTime? CustomerSince { get; set; }
}

/// <summary>
/// Request to create a new customer
/// </summary>
public class CreateCustomerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public CustomerType Type { get; set; } = CustomerType.Direct;

    // Contact Information
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    // Billing Address
    public string? BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }

    // Business Information
    public string? TaxId { get; set; }
    public string? PanNumber { get; set; }
    public string? Industry { get; set; }
    public CustomerSegment? Segment { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string Currency { get; set; } = "INR";

    // Account Information
    public DateTime? CustomerSince { get; set; }
    public string? CreditTerms { get; set; }
    public decimal? CreditLimit { get; set; }

    // Account Management
    public Guid? AccountManagerId { get; set; }
    public Guid? SecondaryAccountManagerId { get; set; }

    // Portal Settings
    public bool PortalEnabled { get; set; } = true;
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }

    // External References
    public string? ExternalCustomerId { get; set; }
    public Guid? AuthenticationProviderId { get; set; }

    // Partner Assignment (if partner-managed)
    public Guid? PrimaryPartnerId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
}

/// <summary>
/// Request to update an existing customer
/// </summary>
public class UpdateCustomerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
    public string? StatusReason { get; set; }

    // Contact Information
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    // Billing Address
    public string? BillingAddressLine1 { get; set; }
    public string? BillingAddressLine2 { get; set; }
    public string? BillingCity { get; set; }
    public string? BillingState { get; set; }
    public string? BillingCountry { get; set; }
    public string? BillingPostalCode { get; set; }

    // Business Information
    public string? TaxId { get; set; }
    public string? PanNumber { get; set; }
    public string? Industry { get; set; }
    public CustomerSegment? Segment { get; set; }
    public int? EmployeeCount { get; set; }
    public decimal? AnnualRevenue { get; set; }
    public string Currency { get; set; } = "INR";

    // Account Information
    public DateTime? CustomerSince { get; set; }
    public string? CreditTerms { get; set; }
    public decimal? CreditLimit { get; set; }
    public int CustomerRating { get; set; }

    // Account Management
    public Guid? AccountManagerId { get; set; }
    public Guid? SecondaryAccountManagerId { get; set; }

    // Portal Settings
    public bool PortalEnabled { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? PreferredTimeZone { get; set; }

    // External References
    public string? ExternalCustomerId { get; set; }
    public Guid? AuthenticationProviderId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Tags { get; set; }
    public string? CustomFields { get; set; }
}

/// <summary>
/// Customer dropdown/lookup item
/// </summary>
public class CustomerLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public CustomerType Type { get; set; }
    public CustomerStatus Status { get; set; }
}

/// <summary>
/// Partner-Customer relationship DTO
/// </summary>
public class PartnerCustomerDto
{
    public Guid Id { get; set; }
    public Guid PartnerId { get; set; }
    public string PartnerCode { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public bool IsPrimaryPartner { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RelationshipStart { get; set; }
    public DateTime? RelationshipEnd { get; set; }
    public decimal? CommissionPercent { get; set; }
    public decimal? DiscountPercent { get; set; }

    // Permissions
    public bool CanCreateTickets { get; set; }
    public bool CanViewAllTickets { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; }
    public bool CanViewAssets { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// Request to assign a partner to a customer
/// </summary>
public class AssignPartnerToCustomerRequest
{
    public Guid PartnerId { get; set; }
    public Guid CustomerId { get; set; }
    public bool IsPrimaryPartner { get; set; } = true;
    public DateTime? RelationshipStart { get; set; }
    public decimal? CommissionPercent { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool CanCreateTickets { get; set; } = true;
    public bool CanViewAllTickets { get; set; } = true;
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; } = true;
    public bool CanViewAssets { get; set; } = true;
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update a partner-customer relationship
/// </summary>
public class UpdatePartnerCustomerRequest
{
    public bool IsPrimaryPartner { get; set; }
    public bool IsActive { get; set; }
    public DateTime? RelationshipEnd { get; set; }
    public string? EndReason { get; set; }
    public decimal? CommissionPercent { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool CanCreateTickets { get; set; }
    public bool CanViewAllTickets { get; set; }
    public bool CanManageContacts { get; set; }
    public bool CanViewContracts { get; set; }
    public bool CanViewAssets { get; set; }
    public string? Notes { get; set; }
}
