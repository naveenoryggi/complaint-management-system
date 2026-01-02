namespace ComplaintManagement.Application.DTOs.Asset;

/// <summary>
/// Full stock category DTO for API responses
/// </summary>
public class StockCategoryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public string? Icon { get; set; }
    public bool IsForSale { get; set; }
    public bool IsFixedAsset { get; set; }
    public bool IsServiceStock { get; set; }
    public bool IsFaulty { get; set; }
    public bool IsDemo { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public bool AllowsCustomerAssignment { get; set; }
    public bool AllowsEmployeeAssignment { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public int AssetCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Stock category lookup for dropdowns
/// </summary>
public class StockCategoryLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ColorCode { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to create a new stock category
/// </summary>
public class CreateStockCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public string? Icon { get; set; }
    public bool IsForSale { get; set; }
    public bool IsFixedAsset { get; set; }
    public bool IsServiceStock { get; set; }
    public bool IsFaulty { get; set; }
    public bool IsDemo { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public bool AllowsCustomerAssignment { get; set; } = true;
    public bool AllowsEmployeeAssignment { get; set; } = true;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request to update an existing stock category
/// </summary>
public class UpdateStockCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ColorCode { get; set; }
    public string? Icon { get; set; }
    public bool IsForSale { get; set; }
    public bool IsFixedAsset { get; set; }
    public bool IsServiceStock { get; set; }
    public bool IsFaulty { get; set; }
    public bool IsDemo { get; set; }
    public bool RequiresSpecialHandling { get; set; }
    public bool AllowsCustomerAssignment { get; set; }
    public bool AllowsEmployeeAssignment { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
