using ComplaintManagement.Domain.Entities.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

public class StockLocationDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? ParentLocationId { get; set; }
    public string? ParentLocationName { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StockLocationType Type { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public int? MaxCapacity { get; set; }
    public string? CapacityUnit { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
    public int StockItemCount { get; set; }
    public int ChildLocationCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StockLocationLookupDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public StockLocationType Type { get; set; }
    public Guid? ParentLocationId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateStockLocationRequest
{
    public Guid? ParentLocationId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public StockLocationType Type { get; set; } = StockLocationType.Warehouse;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public int? MaxCapacity { get; set; }
    public string? CapacityUnit { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; }
    public int SortOrder { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockLocationRequest : CreateStockLocationRequest
{
}
