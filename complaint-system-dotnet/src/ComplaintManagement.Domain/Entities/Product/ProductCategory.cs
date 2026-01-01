namespace ComplaintManagement.Domain.Entities.Product;

/// <summary>
/// Product category with self-referencing tree structure for unlimited depth hierarchy.
/// Examples: Hardware > Servers > Rack Servers > 1U Servers
/// </summary>
public class ProductCategory : BaseEntity
{
    #region Identity

    /// <summary>Company that owns this category</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Parent category ID for hierarchy (null for root categories)</summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>Unique category code within company (e.g., CAT-001, HW-SRV)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display name of the category</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Detailed description of the category</summary>
    public string? Description { get; set; }

    #endregion

    #region Hierarchy

    /// <summary>Depth level in the tree (0 = root, 1 = first level, etc.)</summary>
    public int Level { get; set; }

    /// <summary>Materialized path for efficient queries (e.g., "/1/3/7/")</summary>
    public string? Path { get; set; }

    /// <summary>Full path of category names for display (e.g., "Hardware > Servers > Rack")</summary>
    public string? FullPath { get; set; }

    /// <summary>Display order within parent category</summary>
    public int DisplayOrder { get; set; }

    #endregion

    #region Display

    /// <summary>Icon name/class for UI display</summary>
    public string? Icon { get; set; }

    /// <summary>Hex color code for visual identification</summary>
    public string? Color { get; set; }

    /// <summary>URL to category image</summary>
    public string? ImageUrl { get; set; }

    /// <summary>URL to category thumbnail</summary>
    public string? ThumbnailUrl { get; set; }

    #endregion

    #region Defaults

    /// <summary>Default warranty period in months for products in this category</summary>
    public int? DefaultWarrantyMonths { get; set; }

    /// <summary>Default SLA response time in hours</summary>
    public int? DefaultResponseTimeHours { get; set; }

    /// <summary>Default SLA resolution time in hours</summary>
    public int? DefaultResolutionTimeHours { get; set; }

    /// <summary>Default SLA settings to apply to products</summary>
    public Guid? DefaultSLASettingsId { get; set; }

    /// <summary>Default tax rate percentage</summary>
    public decimal? DefaultTaxRate { get; set; }

    /// <summary>Default HSN code for products in this category</summary>
    public string? DefaultHSNCode { get; set; }

    /// <summary>Default SAC code for services in this category</summary>
    public string? DefaultSACCode { get; set; }

    #endregion

    #region Settings

    /// <summary>Whether products in this category require serial number tracking</summary>
    public bool RequireSerialNumber { get; set; }

    /// <summary>Whether products in this category should track inventory</summary>
    public bool TrackInventory { get; set; }

    /// <summary>Whether this category is visible to customers in portal</summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>Whether this category allows products to be added</summary>
    public bool AllowProducts { get; set; } = true;

    /// <summary>Whether this category allows sub-categories</summary>
    public bool AllowSubCategories { get; set; } = true;

    /// <summary>Whether this category is active</summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region SEO & Portal

    /// <summary>URL-friendly slug for portal navigation</summary>
    public string? Slug { get; set; }

    /// <summary>SEO meta title</summary>
    public string? MetaTitle { get; set; }

    /// <summary>SEO meta description</summary>
    public string? MetaDescription { get; set; }

    /// <summary>Searchable keywords/tags (JSON array)</summary>
    public string? Keywords { get; set; }

    #endregion

    #region External

    /// <summary>External category ID from ERP/PIM system</summary>
    public string? ExternalCategoryId { get; set; }

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    /// <summary>Custom attributes (JSON)</summary>
    public string? CustomAttributes { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>Parent company</summary>
    public virtual MasterData.Company? Company { get; set; }

    /// <summary>Parent category (null for root)</summary>
    public virtual ProductCategory? ParentCategory { get; set; }

    /// <summary>Child categories</summary>
    public virtual ICollection<ProductCategory> SubCategories { get; set; } = new List<ProductCategory>();

    /// <summary>Products in this category</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    /// <summary>Product sub-types in this category</summary>
    public virtual ICollection<ProductSubType> SubTypes { get; set; } = new List<ProductSubType>();

    /// <summary>Default SLA settings</summary>
    public virtual SLA.SLASettings? DefaultSLASettings { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>Whether this is a root category</summary>
    public bool IsRoot => ParentCategoryId == null;

    /// <summary>Whether this category has children</summary>
    public bool HasChildren => SubCategories?.Any() == true;

    /// <summary>Number of direct child categories</summary>
    public int ChildCount => SubCategories?.Count ?? 0;

    #endregion
}
