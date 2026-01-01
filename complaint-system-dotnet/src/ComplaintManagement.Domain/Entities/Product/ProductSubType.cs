namespace ComplaintManagement.Domain.Entities.Product;

/// <summary>
/// Product Sub-Type entity - child of Category
/// Allows custom categorization within product categories
/// </summary>
public class ProductSubType : BaseEntity
{
    /// <summary>Company ID for multi-tenancy</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Parent Category ID (required)</summary>
    public Guid CategoryId { get; set; }

    /// <summary>Unique code for the sub-type</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Display name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Description</summary>
    public string? Description { get; set; }

    /// <summary>Icon class (e.g., fa-laptop)</summary>
    public string? Icon { get; set; }

    /// <summary>Display color (hex)</summary>
    public string? Color { get; set; }

    /// <summary>Display order for sorting</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Whether this sub-type is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    // Navigation Properties

    /// <summary>Parent company</summary>
    public virtual MasterData.Company? Company { get; set; }

    /// <summary>Parent category</summary>
    public virtual ProductCategory? Category { get; set; }

    /// <summary>Products in this sub-type</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
