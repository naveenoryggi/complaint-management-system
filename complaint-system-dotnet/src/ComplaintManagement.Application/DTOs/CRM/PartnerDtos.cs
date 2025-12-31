using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.DTOs.CRM;

/// <summary>
/// Partner information DTO for API responses
/// </summary>
public class PartnerDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public PartnerType Type { get; set; }
    public string TypeName => Type.ToString();
    public PartnerTier Tier { get; set; }
    public string TierName => Tier.ToString();
    public PartnerStatus Status { get; set; }
    public string StatusName => Status.ToString();

    // Contact Information
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Business Information
    public string? TaxId { get; set; }
    public string? PanNumber { get; set; }
    public string? IndustrySegment { get; set; }
    public DateTime? PartnerSince { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? CommissionPercent { get; set; }

    // Portal Settings
    public bool PortalEnabled { get; set; }
    public bool CanManageCustomers { get; set; }
    public bool CanViewAllCustomerTickets { get; set; }
    public bool CanCreateTicketsForCustomers { get; set; }

    // Account Management
    public Guid? AccountManagerId { get; set; }
    public string? AccountManagerName { get; set; }

    // External References
    public string? ExternalPartnerId { get; set; }
    public Guid? AuthenticationProviderId { get; set; }

    // Metadata
    public string? Notes { get; set; }
    public string? Tags { get; set; }

    // Statistics
    public int CustomerCount { get; set; }
    public int ContactCount { get; set; }
    public int ActiveTicketCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Partner summary for list views
/// </summary>
public class PartnerSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PartnerType Type { get; set; }
    public string TypeName => Type.ToString();
    public PartnerTier Tier { get; set; }
    public string TierName => Tier.ToString();
    public PartnerStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Country { get; set; }
    public int CustomerCount { get; set; }
    public DateTime? PartnerSince { get; set; }
}

/// <summary>
/// Request to create a new partner
/// </summary>
public class CreatePartnerRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public PartnerType Type { get; set; } = PartnerType.Reseller;
    public PartnerTier Tier { get; set; } = PartnerTier.Standard;

    // Contact Information
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Business Information
    public string? TaxId { get; set; }
    public string? PanNumber { get; set; }
    public string? IndustrySegment { get; set; }
    public DateTime? PartnerSince { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? CommissionPercent { get; set; }

    // Portal Settings
    public bool PortalEnabled { get; set; } = true;
    public bool CanManageCustomers { get; set; } = true;
    public bool CanViewAllCustomerTickets { get; set; } = true;
    public bool CanCreateTicketsForCustomers { get; set; } = true;

    // Account Management
    public Guid? AccountManagerId { get; set; }

    // External References
    public string? ExternalPartnerId { get; set; }
    public Guid? AuthenticationProviderId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Request to update an existing partner
/// </summary>
public class UpdatePartnerRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public PartnerType Type { get; set; }
    public PartnerTier Tier { get; set; }
    public PartnerStatus Status { get; set; }
    public string? StatusReason { get; set; }

    // Contact Information
    public string PrimaryEmail { get; set; } = string.Empty;
    public string? PrimaryPhone { get; set; }
    public string? Website { get; set; }

    // Address
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }

    // Business Information
    public string? TaxId { get; set; }
    public string? PanNumber { get; set; }
    public string? IndustrySegment { get; set; }
    public DateTime? PartnerSince { get; set; }
    public decimal? CreditLimit { get; set; }
    public string? PaymentTerms { get; set; }
    public decimal? DiscountPercent { get; set; }
    public decimal? CommissionPercent { get; set; }

    // Portal Settings
    public bool PortalEnabled { get; set; }
    public bool CanManageCustomers { get; set; }
    public bool CanViewAllCustomerTickets { get; set; }
    public bool CanCreateTicketsForCustomers { get; set; }

    // Account Management
    public Guid? AccountManagerId { get; set; }

    // External References
    public string? ExternalPartnerId { get; set; }
    public Guid? AuthenticationProviderId { get; set; }

    // Notes
    public string? Notes { get; set; }
    public string? Tags { get; set; }
}

/// <summary>
/// Partner dropdown/lookup item
/// </summary>
public class PartnerLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PartnerType Type { get; set; }
    public PartnerTier Tier { get; set; }
}
