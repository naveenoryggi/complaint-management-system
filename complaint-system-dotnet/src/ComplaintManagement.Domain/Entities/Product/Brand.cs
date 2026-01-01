namespace ComplaintManagement.Domain.Entities.Product;

/// <summary>
/// Brand/manufacturer entity for categorizing products.
/// </summary>
public class Brand : BaseEntity
{
    /// <summary>Company that owns this brand entry</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Unique brand code (e.g., BRD-001, APPLE, SAMSUNG)</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Brand display name</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Brand description</summary>
    public string? Description { get; set; }

    /// <summary>Brand logo URL</summary>
    public string? LogoUrl { get; set; }

    /// <summary>Brand website URL</summary>
    public string? WebsiteUrl { get; set; }

    /// <summary>Country of origin</summary>
    public string? Country { get; set; }

    /// <summary>Whether this brand is active</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Display order for sorting</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    #region Navigation Properties

    /// <summary>Parent company</summary>
    public virtual MasterData.Company? Company { get; set; }

    /// <summary>Products of this brand</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();

    #endregion
}
