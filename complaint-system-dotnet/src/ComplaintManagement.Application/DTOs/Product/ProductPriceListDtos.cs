using ComplaintManagement.Domain.Enums.Product;

namespace ComplaintManagement.Application.DTOs.Product;

/// <summary>
/// Product price list DTO for API responses
/// </summary>
public class ProductPriceListDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    // Identity
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PriceListType Type { get; set; }
    public string TypeName => Type.ToString();

    // Target
    public Guid? PartnerId { get; set; }
    public string? PartnerName { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? PartnerTier { get; set; }
    public string? CustomerSegment { get; set; }

    // Pricing
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? MarkupPercent { get; set; }
    public decimal? EffectiveDiscount { get; set; }

    // Quantity Tiers
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }

    // Validity
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsValid { get; set; }
    public int Priority { get; set; }

    // Conditions
    public decimal? MinOrderValue { get; set; }
    public string? PromoCode { get; set; }
    public Guid? ContractId { get; set; }

    // Metadata
    public string? ApprovalStatus { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? Notes { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Price list summary for list views
/// </summary>
public class ProductPriceListSummaryDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public PriceListType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? TargetName { get; set; }  // Partner or Customer name
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? DiscountPercent { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public bool IsValid { get; set; }
    public int Priority { get; set; }
}

/// <summary>
/// Request to create a new price list
/// </summary>
public class CreateProductPriceListRequest
{
    public Guid ProductId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PriceListType Type { get; set; } = PriceListType.Standard;

    // Target
    public Guid? PartnerId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? PartnerTier { get; set; }
    public string? CustomerSegment { get; set; }

    // Pricing
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? MarkupPercent { get; set; }

    // Quantity Tiers
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public string? TierPricing { get; set; }

    // Validity
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public int Priority { get; set; }

    // Conditions
    public decimal? MinOrderValue { get; set; }
    public string? PromoCode { get; set; }
    public Guid? ContractId { get; set; }
    public string? Conditions { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing price list
/// </summary>
public class UpdateProductPriceListRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public PriceListType Type { get; set; }

    // Target
    public Guid? PartnerId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? PartnerTier { get; set; }
    public string? CustomerSegment { get; set; }

    // Pricing
    public decimal UnitPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? DiscountPercent { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? MarkupPercent { get; set; }

    // Quantity Tiers
    public decimal? MinQuantity { get; set; }
    public decimal? MaxQuantity { get; set; }
    public string? TierPricing { get; set; }

    // Validity
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }

    // Conditions
    public decimal? MinOrderValue { get; set; }
    public string? PromoCode { get; set; }
    public Guid? ContractId { get; set; }
    public string? Conditions { get; set; }

    // Notes
    public string? Notes { get; set; }
}

/// <summary>
/// Calculated price result for a product
/// </summary>
public class ProductPriceDto
{
    public Guid ProductId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;

    // Base price
    public decimal BasePrice { get; set; }
    public string Currency { get; set; } = "INR";

    // Applied price list (if any)
    public Guid? AppliedPriceListId { get; set; }
    public string? AppliedPriceListName { get; set; }
    public PriceListType? AppliedPriceListType { get; set; }

    // Final pricing
    public decimal FinalPrice { get; set; }
    public decimal? DiscountAmount { get; set; }
    public decimal? DiscountPercent { get; set; }

    // Tax
    public decimal? TaxRate { get; set; }
    public decimal? TaxAmount { get; set; }
    public decimal? PriceWithTax { get; set; }

    // Additional info
    public decimal Quantity { get; set; } = 1;
    public decimal TotalPrice { get; set; }
    public decimal TotalPriceWithTax { get; set; }

    // Promo
    public string? PromoCode { get; set; }
    public bool PromoApplied { get; set; }
}

/// <summary>
/// Request to calculate price for a product
/// </summary>
public class CalculatePriceRequest
{
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; } = 1;
    public Guid? PartnerId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? PromoCode { get; set; }
    public Guid? ContractId { get; set; }
    public bool IncludeTax { get; set; } = true;
}

/// <summary>
/// Bulk price update request
/// </summary>
public class BulkPriceUpdateRequest
{
    public List<Guid> ProductIds { get; set; } = new();
    public decimal? PercentageChange { get; set; }
    public decimal? FixedAmount { get; set; }
    public bool ApplyToBasePrice { get; set; } = true;
    public bool ApplyToCostPrice { get; set; }
    public bool ApplyToMSRP { get; set; }
    public string? Reason { get; set; }
}
