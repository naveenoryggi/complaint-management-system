using ComplaintManagement.Domain.Enums.Product;

namespace ComplaintManagement.Domain.Entities.Product;

/// <summary>
/// Product price list for partner/customer-specific pricing, volume discounts, and promotional pricing.
/// Supports tiered pricing and time-limited offers.
/// </summary>
public class ProductPriceList : BaseEntity
{
    #region Identity

    /// <summary>Product this price applies to</summary>
    public Guid ProductId { get; set; }

    /// <summary>Price list name (e.g., "Standard", "Partner Gold", "Enterprise")</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description of the price list</summary>
    public string? Description { get; set; }

    /// <summary>Type of price list</summary>
    public PriceListType Type { get; set; } = PriceListType.Standard;

    #endregion

    #region Target Audience

    /// <summary>Partner ID for partner-specific pricing (null = all partners)</summary>
    public Guid? PartnerId { get; set; }

    /// <summary>Customer ID for customer-specific pricing (null = all customers)</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Partner tier for tier-based pricing</summary>
    public string? PartnerTier { get; set; }

    /// <summary>Customer segment for segment-based pricing</summary>
    public string? CustomerSegment { get; set; }

    #endregion

    #region Pricing

    /// <summary>Unit price for this price list</summary>
    public decimal UnitPrice { get; set; }

    /// <summary>Currency code (ISO 4217)</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Discount percentage from base price</summary>
    public decimal? DiscountPercent { get; set; }

    /// <summary>Fixed discount amount</summary>
    public decimal? DiscountAmount { get; set; }

    /// <summary>Markup percentage (for cost-plus pricing)</summary>
    public decimal? MarkupPercent { get; set; }

    #endregion

    #region Quantity Tiers

    /// <summary>Minimum quantity for this price tier</summary>
    public decimal? MinQuantity { get; set; }

    /// <summary>Maximum quantity for this price tier</summary>
    public decimal? MaxQuantity { get; set; }

    /// <summary>Tiered pricing configuration (JSON for complex tiers)</summary>
    public string? TierPricing { get; set; }

    #endregion

    #region Validity

    /// <summary>Start date for price validity</summary>
    public DateTime? ValidFrom { get; set; }

    /// <summary>End date for price validity</summary>
    public DateTime? ValidTo { get; set; }

    /// <summary>Whether this price list is currently active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Priority when multiple price lists apply (higher = preferred)</summary>
    public int Priority { get; set; }

    #endregion

    #region Conditions

    /// <summary>Minimum order value to qualify</summary>
    public decimal? MinOrderValue { get; set; }

    /// <summary>Promotion/campaign code</summary>
    public string? PromoCode { get; set; }

    /// <summary>Contract ID this price is linked to</summary>
    public Guid? ContractId { get; set; }

    /// <summary>Additional conditions (JSON)</summary>
    public string? Conditions { get; set; }

    #endregion

    #region Metadata

    /// <summary>Approval status</summary>
    public string? ApprovalStatus { get; set; }

    /// <summary>Approved by user</summary>
    public string? ApprovedBy { get; set; }

    /// <summary>Approval date</summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>Notes</summary>
    public string? Notes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>Product this price applies to</summary>
    public virtual Product? Product { get; set; }

    /// <summary>Partner for partner-specific pricing</summary>
    public virtual CRM.Partner? Partner { get; set; }

    /// <summary>Customer for customer-specific pricing</summary>
    public virtual CRM.Customer? Customer { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>Whether price list is currently valid</summary>
    public bool IsValid
    {
        get
        {
            if (!IsActive) return false;
            var now = DateTime.UtcNow;
            if (ValidFrom.HasValue && now < ValidFrom.Value) return false;
            if (ValidTo.HasValue && now > ValidTo.Value) return false;
            return true;
        }
    }

    /// <summary>Whether this is a promotional price</summary>
    public bool IsPromotional => Type == PriceListType.Promotional;

    /// <summary>Effective discount percentage (calculated)</summary>
    public decimal? EffectiveDiscount
    {
        get
        {
            if (DiscountPercent.HasValue) return DiscountPercent;
            if (DiscountAmount.HasValue && Product?.UnitPrice.HasValue == true && Product.UnitPrice > 0)
                return Math.Round((DiscountAmount.Value / Product.UnitPrice.Value) * 100, 2);
            return null;
        }
    }

    #endregion
}
