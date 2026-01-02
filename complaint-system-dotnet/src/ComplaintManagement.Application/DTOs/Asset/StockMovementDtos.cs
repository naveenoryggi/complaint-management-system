using ComplaintManagement.Domain.Entities.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

public class StockMovementDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? StockItemId { get; set; }
    public Guid? AssetId { get; set; }
    public string? AssetTag { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public StockMovementType MovementType { get; set; }
    public string MovementNumber { get; set; } = string.Empty;

    // Locations
    public Guid? FromLocationId { get; set; }
    public string? FromLocationName { get; set; }
    public Guid? ToLocationId { get; set; }
    public string? ToLocationName { get; set; }
    public Guid? FromStockCategoryId { get; set; }
    public string? FromStockCategoryName { get; set; }
    public Guid? ToStockCategoryId { get; set; }
    public string? ToStockCategoryName { get; set; }

    // Quantities
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = "Each";
    public decimal? QuantityBefore { get; set; }
    public decimal? QuantityAfter { get; set; }

    // Reference
    public StockReferenceType? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid? ReferenceId { get; set; }
    public Guid? ComplaintId { get; set; }
    public string? ComplaintNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }

    // Valuation
    public decimal? UnitCost { get; set; }
    public decimal? TotalValue { get; set; }
    public string Currency { get; set; } = "INR";

    // Movement Details
    public DateTime MovementDate { get; set; }
    public string? Reason { get; set; }
    public StockMovementStatus Status { get; set; }
    public string? SerialNumber { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public Guid? PerformedByUserId { get; set; }
    public string? PerformedByUserName { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public string? ApprovedByUserName { get; set; }
    public DateTime? ApprovalDate { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateStockMovementRequest
{
    public Guid? StockItemId { get; set; }
    public Guid? AssetId { get; set; }
    public Guid ProductId { get; set; }
    public StockMovementType MovementType { get; set; }
    public Guid? FromLocationId { get; set; }
    public Guid? ToLocationId { get; set; }
    public Guid? FromStockCategoryId { get; set; }
    public Guid? ToStockCategoryId { get; set; }
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = "Each";
    public StockReferenceType? ReferenceType { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid? ReferenceId { get; set; }
    public Guid? ComplaintId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? ProjectId { get; set; }
    public decimal? UnitCost { get; set; }
    public string Currency { get; set; } = "INR";
    public DateTime? MovementDate { get; set; }
    public string? Reason { get; set; }
    public string? SerialNumber { get; set; }
    public string? BatchNumber { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? Notes { get; set; }
}

public class StockMovementFilterRequest
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public StockMovementType? MovementType { get; set; }
    public StockMovementStatus? Status { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? StockCategoryId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? SearchTerm { get; set; }
}
