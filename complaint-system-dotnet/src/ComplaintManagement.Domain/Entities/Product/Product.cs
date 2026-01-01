using ComplaintManagement.Domain.Enums.Product;

namespace ComplaintManagement.Domain.Entities.Product;

/// <summary>
/// Product entity following CRM best practices (Salesforce/HubSpot/Zoho patterns).
/// Supports physical goods, digital products, services, and subscriptions.
/// </summary>
public class Product : BaseEntity
{
    #region Identity

    /// <summary>Company that owns this product</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Category this product belongs to</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Sub-type within the category (optional)</summary>
    public Guid? SubTypeId { get; set; }

    /// <summary>Parent product ID for variants/bundles</summary>
    public Guid? ParentProductId { get; set; }

    /// <summary>Internal product code (e.g., PRD-001)</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>Product display name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short description for listings</summary>
    public string? ShortDescription { get; set; }

    /// <summary>Full product description</summary>
    public string? Description { get; set; }

    /// <summary>Type of product</summary>
    public ProductType Type { get; set; } = ProductType.Physical;

    #endregion

    #region SKU & Identification

    /// <summary>Stock Keeping Unit - unique identifier for inventory</summary>
    public string? SKU { get; set; }

    /// <summary>Manufacturer's part number</summary>
    public string? PartNumber { get; set; }

    /// <summary>Universal Product Code (barcode)</summary>
    public string? UPC { get; set; }

    /// <summary>European Article Number</summary>
    public string? EAN { get; set; }

    /// <summary>International Standard Book Number (for publications)</summary>
    public string? ISBN { get; set; }

    /// <summary>Harmonized System Nomenclature code for tax/customs (goods)</summary>
    public string? HSNCode { get; set; }

    /// <summary>Service Accounting Code for tax (services)</summary>
    public string? SACCode { get; set; }

    #endregion

    #region Pricing

    /// <summary>Base unit price</summary>
    public decimal? UnitPrice { get; set; }

    /// <summary>Internal cost price</summary>
    public decimal? CostPrice { get; set; }

    /// <summary>Manufacturer's suggested retail price</summary>
    public decimal? MSRP { get; set; }

    /// <summary>Minimum selling price</summary>
    public decimal? MinimumPrice { get; set; }

    /// <summary>Currency code (ISO 4217)</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Default tax rate percentage</summary>
    public decimal? TaxRate { get; set; }

    /// <summary>Whether product is taxable</summary>
    public bool IsTaxable { get; set; } = true;

    /// <summary>Tax type</summary>
    public TaxType TaxType { get; set; } = TaxType.GST;

    /// <summary>Tiered pricing configuration (JSON)</summary>
    public string? PriceBook { get; set; }

    #endregion

    #region Units & Quantity

    /// <summary>Unit of measure</summary>
    public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Each;

    /// <summary>Custom unit of measure name</summary>
    public string? CustomUnitName { get; set; }

    /// <summary>Minimum order quantity</summary>
    public decimal? MinOrderQuantity { get; set; }

    /// <summary>Maximum order quantity</summary>
    public decimal? MaxOrderQuantity { get; set; }

    /// <summary>Quantity increment for ordering</summary>
    public decimal? QuantityIncrement { get; set; }

    /// <summary>Quantity per pack/box</summary>
    public int? QuantityPerPack { get; set; }

    #endregion

    #region Inventory (for physical products)

    /// <summary>Whether to track inventory for this product</summary>
    public bool TrackInventory { get; set; }

    /// <summary>Current quantity in stock</summary>
    public int? QuantityInStock { get; set; }

    /// <summary>Quantity reserved for orders</summary>
    public int? QuantityReserved { get; set; }

    /// <summary>Quantity available (InStock - Reserved)</summary>
    public int? QuantityAvailable => QuantityInStock - (QuantityReserved ?? 0);

    /// <summary>Reorder point - when to reorder</summary>
    public int? ReorderLevel { get; set; }

    /// <summary>Quantity to reorder</summary>
    public int? ReorderQuantity { get; set; }

    /// <summary>Lead time for reorder in days</summary>
    public int? LeadTimeDays { get; set; }

    /// <summary>Whether to allow backorders</summary>
    public bool AllowBackorder { get; set; }

    /// <summary>Default warehouse/location</summary>
    public string? DefaultWarehouse { get; set; }

    #endregion

    #region Product Details

    /// <summary>Brand ID (foreign key)</summary>
    public Guid? BrandId { get; set; }

    /// <summary>Brand name (legacy - for backward compatibility)</summary>
    public string? Brand { get; set; }

    /// <summary>Manufacturer name</summary>
    public string? Manufacturer { get; set; }

    /// <summary>Model number/name</summary>
    public string? Model { get; set; }

    /// <summary>Product version</summary>
    public string? Version { get; set; }

    /// <summary>Technical specifications (JSON)</summary>
    public string? Specifications { get; set; }

    /// <summary>Feature list (JSON array)</summary>
    public string? Features { get; set; }

    /// <summary>Product dimensions (JSON: length, width, height)</summary>
    public string? Dimensions { get; set; }

    /// <summary>Product weight in kg</summary>
    public decimal? Weight { get; set; }

    /// <summary>Country of origin</summary>
    public string? CountryOfOrigin { get; set; }

    #endregion

    #region Media

    /// <summary>Primary product image URL</summary>
    public string? ImageUrl { get; set; }

    /// <summary>Thumbnail image URL</summary>
    public string? ThumbnailUrl { get; set; }

    /// <summary>Additional images (JSON array of URLs)</summary>
    public string? Images { get; set; }

    /// <summary>Video URLs (JSON array)</summary>
    public string? Videos { get; set; }

    /// <summary>Document URLs - datasheets, manuals (JSON array)</summary>
    public string? Documents { get; set; }

    #endregion

    #region Warranty & Support

    /// <summary>Default warranty period in months</summary>
    public int? DefaultWarrantyMonths { get; set; }

    /// <summary>Extended warranty available months</summary>
    public int? ExtendedWarrantyMonths { get; set; }

    /// <summary>Warranty terms and conditions</summary>
    public string? WarrantyTerms { get; set; }

    /// <summary>Default SLA settings for this product</summary>
    public Guid? DefaultSLASettingsId { get; set; }

    /// <summary>Support URL/portal link</summary>
    public string? SupportUrl { get; set; }

    /// <summary>Whether product is serviceable</summary>
    public bool IsServiceable { get; set; } = true;

    /// <summary>Whether product requires installation</summary>
    public bool RequiresInstallation { get; set; }

    #endregion

    #region Subscription (for recurring products)

    /// <summary>Whether this is a subscription product</summary>
    public bool IsSubscription { get; set; }

    /// <summary>Billing frequency for subscriptions</summary>
    public BillingFrequency? BillingFrequency { get; set; }

    /// <summary>Trial period in days</summary>
    public int? TrialPeriodDays { get; set; }

    /// <summary>Subscription setup fee</summary>
    public decimal? SetupFee { get; set; }

    /// <summary>Whether subscription auto-renews</summary>
    public bool AutoRenew { get; set; } = true;

    /// <summary>Cancellation policy</summary>
    public string? CancellationPolicy { get; set; }

    #endregion

    #region Status & Lifecycle

    /// <summary>Product status</summary>
    public ProductStatus Status { get; set; } = ProductStatus.Draft;

    /// <summary>Reason for current status</summary>
    public string? StatusReason { get; set; }

    /// <summary>Product launch/availability date</summary>
    public DateTime? LaunchDate { get; set; }

    /// <summary>End of sale date</summary>
    public DateTime? EndOfSaleDate { get; set; }

    /// <summary>End of life date</summary>
    public DateTime? EndOfLifeDate { get; set; }

    /// <summary>End of support date</summary>
    public DateTime? EndOfSupportDate { get; set; }

    /// <summary>Whether product is featured/highlighted</summary>
    public bool IsFeatured { get; set; }

    /// <summary>Whether product is visible to customers</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>Whether this is a configurable product</summary>
    public bool IsConfigurable { get; set; }

    /// <summary>Whether serial number is required</summary>
    public bool RequireSerialNumber { get; set; }

    #endregion

    #region SEO & Portal

    /// <summary>URL-friendly slug</summary>
    public string? Slug { get; set; }

    /// <summary>SEO meta title</summary>
    public string? MetaTitle { get; set; }

    /// <summary>SEO meta description</summary>
    public string? MetaDescription { get; set; }

    /// <summary>Searchable tags (JSON array)</summary>
    public string? Tags { get; set; }

    /// <summary>Search keywords</summary>
    public string? Keywords { get; set; }

    #endregion

    #region External References

    /// <summary>External product ID from ERP/PIM</summary>
    public string? ExternalProductId { get; set; }

    /// <summary>Vendor's product ID</summary>
    public string? VendorProductId { get; set; }

    /// <summary>Vendor/supplier name</summary>
    public string? VendorName { get; set; }

    /// <summary>Custom fields (JSON)</summary>
    public string? CustomFields { get; set; }

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    #endregion

    #region Related Products

    /// <summary>Related product IDs (JSON array)</summary>
    public string? RelatedProductIds { get; set; }

    /// <summary>Cross-sell product IDs (JSON array)</summary>
    public string? CrossSellProductIds { get; set; }

    /// <summary>Up-sell product IDs (JSON array)</summary>
    public string? UpSellProductIds { get; set; }

    /// <summary>Accessory product IDs (JSON array)</summary>
    public string? AccessoryProductIds { get; set; }

    /// <summary>Replacement product ID (for discontinued products)</summary>
    public Guid? ReplacementProductId { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>Parent company</summary>
    public virtual MasterData.Company? Company { get; set; }

    /// <summary>Product category</summary>
    public virtual ProductCategory? Category { get; set; }

    /// <summary>Product sub-type</summary>
    public virtual ProductSubType? SubType { get; set; }

    /// <summary>Product brand</summary>
    public virtual Brand? BrandEntity { get; set; }

    /// <summary>Parent product (for variants)</summary>
    public virtual Product? ParentProduct { get; set; }

    /// <summary>Product variants</summary>
    public virtual ICollection<Product> Variants { get; set; } = new List<Product>();

    /// <summary>Price lists for this product</summary>
    public virtual ICollection<ProductPriceList> PriceLists { get; set; } = new List<ProductPriceList>();

    /// <summary>Default SLA settings</summary>
    public virtual SLA.SLASettings? DefaultSLASettings { get; set; }

    /// <summary>Replacement product</summary>
    public virtual Product? ReplacementProduct { get; set; }

    /// <summary>Assets that are instances of this product</summary>
    public virtual ICollection<Service.Asset> Assets { get; set; } = new List<Service.Asset>();

    #endregion

    #region Computed Properties

    /// <summary>Whether product is in stock</summary>
    public bool InStock => !TrackInventory || (QuantityAvailable ?? 0) > 0;

    /// <summary>Whether product is a variant of another product</summary>
    public bool IsVariant => ParentProductId.HasValue;

    /// <summary>Whether product has variants</summary>
    public bool HasVariants => Variants?.Any() == true;

    /// <summary>Whether product is available for purchase</summary>
    public bool IsAvailable => Status == ProductStatus.Active && IsPublic && (LaunchDate == null || LaunchDate <= DateTime.UtcNow);

    /// <summary>Whether product is discontinued</summary>
    public bool IsDiscontinued => Status == ProductStatus.Discontinued || Status == ProductStatus.EndOfLife;

    /// <summary>Profit margin percentage</summary>
    public decimal? ProfitMargin => CostPrice.HasValue && UnitPrice.HasValue && CostPrice > 0
        ? Math.Round(((UnitPrice.Value - CostPrice.Value) / CostPrice.Value) * 100, 2)
        : null;

    #endregion
}
