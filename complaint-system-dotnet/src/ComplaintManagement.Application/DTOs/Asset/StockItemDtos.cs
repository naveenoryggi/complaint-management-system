using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

public class StockItemDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? SKU { get; set; }
    public Guid StockCategoryId { get; set; }
    public string? StockCategoryName { get; set; }
    public string? StockCategoryCode { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? LocationCode { get; set; }

    // Quantities
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal QuantityAvailable { get; set; }
    public decimal QuantityInTransit { get; set; }
    public decimal QuantityOnOrder { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public decimal? SafetyStock { get; set; }

    public string UnitOfMeasure { get; set; } = "Each";

    // Valuation
    public decimal? UnitCost { get; set; }
    public decimal? TotalValue { get; set; }
    public string Currency { get; set; } = "INR";
    public CostingMethod CostingMethod { get; set; }
    public decimal? LastPurchasePrice { get; set; }
    public DateTime? LastPurchaseDate { get; set; }

    // Status
    public StockItemStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
    public bool RequiresAttention { get; set; }
    public string? AttentionReason { get; set; }

    // Tracking
    public DateTime? LastCountDate { get; set; }
    public decimal? LastCountQuantity { get; set; }
    public decimal? LastCountVariance { get; set; }
    public DateTime? LastMovementDate { get; set; }
    public int? DaysSinceLastMovement { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string? BatchNumber { get; set; }

    // Assignment
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectName { get; set; }

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class StockItemLookupDto
{
    public Guid Id { get; set; }
    public string? ProductCode { get; set; }
    public string? ProductName { get; set; }
    public string? SKU { get; set; }
    public string? LocationName { get; set; }
    public string? StockCategoryName { get; set; }
    public decimal QuantityAvailable { get; set; }
}

public class CreateStockItemRequest
{
    public Guid ProductId { get; set; }
    public Guid StockCategoryId { get; set; }
    public Guid? LocationId { get; set; }
    public decimal QuantityOnHand { get; set; }
    public decimal QuantityReserved { get; set; }
    public decimal? MinimumQuantity { get; set; }
    public decimal? MaximumQuantity { get; set; }
    public decimal? ReorderQuantity { get; set; }
    public decimal? SafetyStock { get; set; }
    public string UnitOfMeasure { get; set; } = "Each";
    public decimal? UnitCost { get; set; }
    public string Currency { get; set; } = "INR";
    public CostingMethod CostingMethod { get; set; } = CostingMethod.AverageCost;
    public StockItemStatus Status { get; set; } = StockItemStatus.Active;
    public AssetCondition Condition { get; set; } = AssetCondition.New;
    public DateTime? ExpiryDate { get; set; }
    public string? BatchNumber { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? ProjectId { get; set; }
    public string? Notes { get; set; }
}

public class UpdateStockItemRequest : CreateStockItemRequest
{
}

public class AdjustStockRequest
{
    public Guid StockItemId { get; set; }
    public decimal AdjustmentQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Notes { get; set; }
}

public class TransferStockRequest
{
    public Guid StockItemId { get; set; }
    public Guid ToLocationId { get; set; }
    public Guid? ToStockCategoryId { get; set; }
    public decimal Quantity { get; set; }
    public string? Reason { get; set; }
    public string? Notes { get; set; }
}
