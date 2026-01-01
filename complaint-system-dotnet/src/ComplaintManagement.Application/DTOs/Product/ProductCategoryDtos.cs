namespace ComplaintManagement.Application.DTOs.Product;

/// <summary>
/// Product category DTO for API responses
/// </summary>
public class ProductCategoryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Hierarchy
    public int Level { get; set; }
    public string? Path { get; set; }
    public string? FullPath { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsRoot { get; set; }

    // Display
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Defaults
    public int? DefaultWarrantyMonths { get; set; }
    public int? DefaultResponseTimeHours { get; set; }
    public int? DefaultResolutionTimeHours { get; set; }
    public decimal? DefaultTaxRate { get; set; }
    public string? DefaultHSNCode { get; set; }
    public string? DefaultSACCode { get; set; }

    // Settings
    public bool RequireSerialNumber { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsPublic { get; set; }
    public bool AllowProducts { get; set; }
    public bool AllowSubCategories { get; set; }
    public bool IsActive { get; set; }

    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    // Statistics
    public int ProductCount { get; set; }
    public int SubCategoryCount { get; set; }

    // Children
    public List<ProductCategoryDto> SubCategories { get; set; } = new();

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product category summary for list views
/// </summary>
public class ProductCategorySummaryDto
{
    public Guid Id { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FullPath { get; set; }
    public int Level { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
    public int SubCategoryCount { get; set; }
}

/// <summary>
/// Product category tree node for hierarchical display
/// </summary>
public class ProductCategoryTreeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int Level { get; set; }
    public bool IsActive { get; set; }
    public bool HasChildren { get; set; }
    public int ProductCount { get; set; }
    public List<ProductCategoryTreeDto> Children { get; set; } = new();
}

/// <summary>
/// Request to create a new product category
/// </summary>
public class CreateProductCategoryRequest
{
    public Guid? ParentCategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Display
    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Defaults
    public int? DefaultWarrantyMonths { get; set; }
    public int? DefaultResponseTimeHours { get; set; }
    public int? DefaultResolutionTimeHours { get; set; }
    public Guid? DefaultSLASettingsId { get; set; }
    public decimal? DefaultTaxRate { get; set; }
    public string? DefaultHSNCode { get; set; }
    public string? DefaultSACCode { get; set; }

    // Settings
    public bool RequireSerialNumber { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsPublic { get; set; } = true;
    public bool AllowProducts { get; set; } = true;
    public bool AllowSubCategories { get; set; } = true;

    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }

    // External
    public string? ExternalCategoryId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing product category
/// </summary>
public class UpdateProductCategoryRequest
{
    public Guid? ParentCategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Display
    public int DisplayOrder { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }

    // Defaults
    public int? DefaultWarrantyMonths { get; set; }
    public int? DefaultResponseTimeHours { get; set; }
    public int? DefaultResolutionTimeHours { get; set; }
    public Guid? DefaultSLASettingsId { get; set; }
    public decimal? DefaultTaxRate { get; set; }
    public string? DefaultHSNCode { get; set; }
    public string? DefaultSACCode { get; set; }

    // Settings
    public bool RequireSerialNumber { get; set; }
    public bool TrackInventory { get; set; }
    public bool IsPublic { get; set; }
    public bool AllowProducts { get; set; }
    public bool AllowSubCategories { get; set; }
    public bool IsActive { get; set; }

    // SEO
    public string? Slug { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? Keywords { get; set; }

    // External
    public string? ExternalCategoryId { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Product category lookup for dropdowns
/// </summary>
public class ProductCategoryLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? FullPath { get; set; }
    public int Level { get; set; }
    public bool AllowProducts { get; set; }
}

/// <summary>
/// Request to move a category to a new parent
/// </summary>
public class MoveCategoryRequest
{
    public Guid? NewParentCategoryId { get; set; }
    public int? NewDisplayOrder { get; set; }
}
