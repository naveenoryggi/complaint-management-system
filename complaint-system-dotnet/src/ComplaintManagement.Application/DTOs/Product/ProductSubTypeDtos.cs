namespace ComplaintManagement.Application.DTOs.Product;

/// <summary>
/// Product Sub-Type DTO for API responses
/// </summary>
public class ProductSubTypeDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public string? Notes { get; set; }

    // Statistics
    public int ProductCount { get; set; }

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Product Sub-Type summary for list views
/// </summary>
public class ProductSubTypeSummaryDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public int ProductCount { get; set; }
}

/// <summary>
/// Product Sub-Type lookup for dropdowns
/// </summary>
public class ProductSubTypeLookupDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Request to create a new product sub-type
/// </summary>
public class CreateProductSubTypeRequest
{
    public Guid CategoryId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to update an existing product sub-type
/// </summary>
public class UpdateProductSubTypeRequest
{
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
    public string? Notes { get; set; }
}
