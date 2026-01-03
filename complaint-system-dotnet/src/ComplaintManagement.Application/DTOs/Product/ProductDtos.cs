using ComplaintManagement.Domain.Enums.Product;

namespace ComplaintManagement.Application.DTOs.Product;

/// <summary>
/// Product DTO for API responses
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? CategoryPath { get; set; }
    public Guid? SubTypeId { get; set; }
    public string? SubTypeName { get; set; }
    public Guid? ParentProductId { get; set; }
    public string? ParentProductName { get; set; }

    // Identity
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public ProductType Type { get; set; }
    public string TypeName => Type.ToString();

    // SKU & Identification
    public string? SKU { get; set; }
    public string? PartNumber { get; set; }
    public string? UPC { get; set; }
    public string? EAN { get; set; }
    public string? HSNCode { get; set; }
    public string? SACCode { get; set; }

    // Pricing
    public decimal? UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? MSRP { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? TaxRate { get; set; }
    public bool IsTaxable { get; set; }
    public TaxType TaxType { get; set; }
    public string TaxTypeName => TaxType.ToString();

    // Units
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public string UnitOfMeasureName => UnitOfMeasure.ToString();
    public string? CustomUnitName { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? MaxOrderQuantity { get; set; }

    // Inventory Settings (actual quantities are in Stock Management module)
    public bool TrackInventory { get; set; }
    public bool AllowBackorder { get; set; }
    public Guid? DefaultLocationId { get; set; }
    public string? DefaultLocationName { get; set; }
    public Guid? DefaultStockCategoryId { get; set; }
    public string? DefaultStockCategoryName { get; set; }

    // Computed Stock Quantities (from StockItems)
    public decimal TotalQuantityOnHand { get; set; }
    public decimal TotalQuantityReserved { get; set; }
    public decimal TotalQuantityAvailable { get; set; }
    public bool InStock { get; set; }

    // Stock Item Summary
    public int StockItemCount { get; set; }
    public List<StockItemSummaryDto> StockItems { get; set; } = new();

    // Product Details
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? Specifications { get; set; }
    public string? Features { get; set; }
    public decimal? Weight { get; set; }
    public string? CountryOfOrigin { get; set; }

    // Media
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Images { get; set; }
    public string? Documents { get; set; }

    // Warranty & Support
    public int? DefaultWarrantyMonths { get; set; }
    public int? ExtendedWarrantyMonths { get; set; }
    public string? WarrantyTerms { get; set; }
    public bool IsServiceable { get; set; }
    public bool RequiresInstallation { get; set; }

    // Subscription
    public bool IsSubscription { get; set; }
    public BillingFrequency? BillingFrequency { get; set; }
    public string? BillingFrequencyName => BillingFrequency?.ToString();
    public int? TrialPeriodDays { get; set; }
    public decimal? SetupFee { get; set; }

    // Status
    public ProductStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public DateTime? LaunchDate { get; set; }
    public DateTime? EndOfSaleDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsDiscontinued { get; set; }
    public bool RequireSerialNumber { get; set; }

    // SEO
    public string? Slug { get; set; }
    public string? Tags { get; set; }

    // External
    public string? ExternalProductId { get; set; }
    public string? VendorName { get; set; }

    // Variants
    public bool IsVariant { get; set; }
    public bool HasVariants { get; set; }
    public int VariantCount { get; set; }
    public List<ProductVariantDto> Variants { get; set; } = new();

    // Computed
    public decimal? ProfitMargin { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product variant summary
/// </summary>
public class ProductVariantDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public decimal? UnitPrice { get; set; }
    public bool InStock { get; set; }
    public ProductStatus Status { get; set; }
}

/// <summary>
/// Product summary for list views
/// </summary>
public class ProductSummaryDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public ProductType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? CategoryName { get; set; }
    public string? SubTypeName { get; set; }
    public string? Brand { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public ProductStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public bool TrackInventory { get; set; }
    public bool InStock { get; set; }
    public decimal TotalQuantityAvailable { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; }
}

/// <summary>
/// Stock item summary for embedding in ProductDto
/// </summary>
public class StockItemSummaryDto
{
    public Guid Id { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public Guid StockCategoryId { get; set; }
    public string? StockCategoryName { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal? UnitCost { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

/// <summary>
/// Request to create a new product
/// </summary>
public class CreateProductRequest
{
    public Guid? CategoryId { get; set; }
    public Guid? SubTypeId { get; set; }
    public Guid? ParentProductId { get; set; }

    // Identity
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public ProductType Type { get; set; } = ProductType.Physical;

    // SKU & Identification
    public string? SKU { get; set; }
    public string? PartNumber { get; set; }
    public string? UPC { get; set; }
    public string? EAN { get; set; }
    public string? HSNCode { get; set; }
    public string? SACCode { get; set; }

    // Pricing
    public decimal? UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? MSRP { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? TaxRate { get; set; }
    public bool IsTaxable { get; set; } = true;
    public TaxType TaxType { get; set; } = TaxType.GST;

    // Units
    public UnitOfMeasure UnitOfMeasure { get; set; } = UnitOfMeasure.Each;
    public string? CustomUnitName { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? MaxOrderQuantity { get; set; }
    public decimal? QuantityIncrement { get; set; }

    // Inventory Settings (quantities managed via Stock Management module)
    public bool TrackInventory { get; set; }
    public bool AllowBackorder { get; set; }
    public Guid? DefaultLocationId { get; set; }
    public Guid? DefaultStockCategoryId { get; set; }

    // Initial Stock (optional - creates initial StockItem if provided)
    public decimal? InitialQuantity { get; set; }
    public decimal? InitialUnitCost { get; set; }

    // Product Details
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? Specifications { get; set; }
    public string? Features { get; set; }
    public string? Dimensions { get; set; }
    public decimal? Weight { get; set; }
    public string? CountryOfOrigin { get; set; }

    // Media
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Images { get; set; }
    public string? Videos { get; set; }
    public string? Documents { get; set; }

    // Warranty & Support
    public int? DefaultWarrantyMonths { get; set; }
    public int? ExtendedWarrantyMonths { get; set; }
    public string? WarrantyTerms { get; set; }
    public Guid? DefaultSLASettingsId { get; set; }
    public bool IsServiceable { get; set; } = true;
    public bool RequiresInstallation { get; set; }

    // Subscription
    public bool IsSubscription { get; set; }
    public BillingFrequency? BillingFrequency { get; set; }
    public int? TrialPeriodDays { get; set; }
    public decimal? SetupFee { get; set; }
    public bool AutoRenew { get; set; } = true;

    // Status
    public ProductStatus Status { get; set; } = ProductStatus.Draft;
    public DateTime? LaunchDate { get; set; }
    public DateTime? EndOfSaleDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool RequireSerialNumber { get; set; }

    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }
    public string? Keywords { get; set; }

    // External
    public string? ExternalProductId { get; set; }
    public string? VendorProductId { get; set; }
    public string? VendorName { get; set; }
    public string? CustomFields { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing product
/// </summary>
public class UpdateProductRequest
{
    public Guid? CategoryId { get; set; }
    public Guid? SubTypeId { get; set; }

    // Identity
    public string Name { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public ProductType Type { get; set; }

    // SKU & Identification
    public string? SKU { get; set; }
    public string? PartNumber { get; set; }
    public string? UPC { get; set; }
    public string? EAN { get; set; }
    public string? HSNCode { get; set; }
    public string? SACCode { get; set; }

    // Pricing
    public decimal? UnitPrice { get; set; }
    public decimal? CostPrice { get; set; }
    public decimal? MSRP { get; set; }
    public decimal? MinimumPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public decimal? TaxRate { get; set; }
    public bool IsTaxable { get; set; }
    public TaxType TaxType { get; set; }

    // Units
    public UnitOfMeasure UnitOfMeasure { get; set; }
    public string? CustomUnitName { get; set; }
    public decimal? MinOrderQuantity { get; set; }
    public decimal? MaxOrderQuantity { get; set; }
    public decimal? QuantityIncrement { get; set; }

    // Inventory Settings (quantities managed via Stock Management module)
    public bool TrackInventory { get; set; }
    public bool AllowBackorder { get; set; }
    public Guid? DefaultLocationId { get; set; }
    public Guid? DefaultStockCategoryId { get; set; }

    // Product Details
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? Specifications { get; set; }
    public string? Features { get; set; }
    public string? Dimensions { get; set; }
    public decimal? Weight { get; set; }
    public string? CountryOfOrigin { get; set; }

    // Media
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? Images { get; set; }
    public string? Videos { get; set; }
    public string? Documents { get; set; }

    // Warranty & Support
    public int? DefaultWarrantyMonths { get; set; }
    public int? ExtendedWarrantyMonths { get; set; }
    public string? WarrantyTerms { get; set; }
    public Guid? DefaultSLASettingsId { get; set; }
    public string? SupportUrl { get; set; }
    public bool IsServiceable { get; set; }
    public bool RequiresInstallation { get; set; }

    // Subscription
    public bool IsSubscription { get; set; }
    public BillingFrequency? BillingFrequency { get; set; }
    public int? TrialPeriodDays { get; set; }
    public decimal? SetupFee { get; set; }
    public bool AutoRenew { get; set; }
    public string? CancellationPolicy { get; set; }

    // Status
    public ProductStatus Status { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? LaunchDate { get; set; }
    public DateTime? EndOfSaleDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime? EndOfSupportDate { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPublic { get; set; }
    public bool IsConfigurable { get; set; }
    public bool RequireSerialNumber { get; set; }

    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Tags { get; set; }
    public string? Keywords { get; set; }

    // External
    public string? ExternalProductId { get; set; }
    public string? VendorProductId { get; set; }
    public string? VendorName { get; set; }
    public string? CustomFields { get; set; }
    public string? Notes { get; set; }

    // Replacement
    public Guid? ReplacementProductId { get; set; }
}

/// <summary>
/// Product lookup for dropdowns
/// </summary>
public class ProductLookupDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SKU { get; set; }
    public ProductType Type { get; set; }
    public decimal? UnitPrice { get; set; }
    public string Currency { get; set; } = "INR";
    public bool InStock { get; set; }
}

// Note: UpdateInventoryRequest is removed.
// Inventory quantities are now managed through the Stock Management module.
// Use StockMovementService.CreateStockMovement() for all inventory adjustments.

/// <summary>
/// Request to clone a product
/// </summary>
public class CloneProductRequest
{
    public string NewProductCode { get; set; } = string.Empty;
    public string NewName { get; set; } = string.Empty;
    public string? NewSKU { get; set; }
    public bool ClonePricing { get; set; } = true;
    public bool CloneMedia { get; set; } = true;
    public bool CloneAsVariant { get; set; }
}

/// <summary>
/// Product search/filter request
/// </summary>
public class ProductSearchRequest
{
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public bool IncludeSubCategories { get; set; } = true;
    public ProductType? Type { get; set; }
    public ProductStatus? Status { get; set; }
    public string? Brand { get; set; }
    public string? Manufacturer { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? InStock { get; set; }
    public bool? IsFeatured { get; set; }
    public bool? IsPublic { get; set; }
    public string? Tags { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
