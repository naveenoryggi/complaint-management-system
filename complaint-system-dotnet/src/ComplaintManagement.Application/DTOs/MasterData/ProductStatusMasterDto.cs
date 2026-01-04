namespace ComplaintManagement.Application.DTOs.MasterData;

public class ProductStatusMasterDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? BackgroundColor { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
    public bool IsSystem { get; set; }
    public bool IsDefault { get; set; }
    public Guid? CompanyId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CreateProductStatusMasterRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? BackgroundColor { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false;
}

public class UpdateProductStatusMasterRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorCode { get; set; }
    public string? BackgroundColor { get; set; }
    public string? IconClass { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
