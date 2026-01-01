namespace ComplaintManagement.Application.DTOs.Product;

/// <summary>
/// Brand DTO for API responses
/// </summary>
public class BrandDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }

    // Statistics
    public int ProductCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Brand summary for list views
/// </summary>
public class BrandSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>
/// Brand lookup for dropdowns
/// </summary>
public class BrandLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request to create a new brand
/// </summary>
public class CreateBrandRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Country { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing brand
/// </summary>
public class UpdateBrandRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? Country { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }
}
