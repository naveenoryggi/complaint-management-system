using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents quantity-based inventory at a specific location.
/// Unlike Asset (which tracks serialized items), StockItem tracks quantities of non-serialized items.
/// Each product/location/category combination has its own StockItem record.
/// </summary>
public class StockItem : BaseEntity
{
    /// <summary>Company that owns this stock</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Product this stock item represents</summary>
    public Guid ProductId { get; set; }

    /// <summary>Stock category (Sales, Fixed, Aftersales, Faulty, etc.)</summary>
    public Guid StockCategoryId { get; set; }

    /// <summary>Location where this stock is stored</summary>
    public Guid? LocationId { get; set; }

    #region Quantities

    /// <summary>Total quantity on hand</summary>
    public decimal QuantityOnHand { get; set; }

    /// <summary>Quantity reserved for pending orders/allocations</summary>
    public decimal QuantityReserved { get; set; }

    /// <summary>Quantity available (OnHand - Reserved)</summary>
    public decimal QuantityAvailable => QuantityOnHand - QuantityReserved;

    /// <summary>Quantity in transit to this location</summary>
    public decimal QuantityInTransit { get; set; }

    /// <summary>Quantity on order (expected incoming)</summary>
    public decimal QuantityOnOrder { get; set; }

    /// <summary>Minimum stock level (reorder point)</summary>
    public decimal? MinimumQuantity { get; set; }

    /// <summary>Maximum stock level</summary>
    public decimal? MaximumQuantity { get; set; }

    /// <summary>Reorder quantity when stock falls below minimum</summary>
    public decimal? ReorderQuantity { get; set; }

    /// <summary>Safety stock buffer</summary>
    public decimal? SafetyStock { get; set; }

    #endregion

    #region Product Info (Denormalized for Performance)

    /// <summary>Product code (cached)</summary>
    public string? ProductCode { get; set; }

    /// <summary>Product name (cached)</summary>
    public string? ProductName { get; set; }

    /// <summary>SKU (cached)</summary>
    public string? SKU { get; set; }

    /// <summary>Unit of measure</summary>
    public string UnitOfMeasure { get; set; } = "Each";

    #endregion

    #region Valuation

    /// <summary>Unit cost (for valuation)</summary>
    public decimal? UnitCost { get; set; }

    /// <summary>Total value (QuantityOnHand * UnitCost)</summary>
    public decimal? TotalValue => QuantityOnHand * (UnitCost ?? 0);

    /// <summary>Currency code</summary>
    public string Currency { get; set; } = "INR";

    /// <summary>Costing method for valuation</summary>
    public CostingMethod CostingMethod { get; set; } = CostingMethod.AverageCost;

    /// <summary>Last purchase price</summary>
    public decimal? LastPurchasePrice { get; set; }

    /// <summary>Date of last purchase</summary>
    public DateTime? LastPurchaseDate { get; set; }

    #endregion

    #region Status

    /// <summary>Stock status</summary>
    public StockItemStatus Status { get; set; } = StockItemStatus.Active;

    /// <summary>Physical condition of stock</summary>
    public AssetCondition Condition { get; set; } = AssetCondition.New;

    /// <summary>Whether stock requires attention (low stock, expired, etc.)</summary>
    public bool RequiresAttention { get; set; }

    /// <summary>Reason for requiring attention</summary>
    public string? AttentionReason { get; set; }

    #endregion

    #region Tracking

    /// <summary>Last stock count date</summary>
    public DateTime? LastCountDate { get; set; }

    /// <summary>Quantity at last count</summary>
    public decimal? LastCountQuantity { get; set; }

    /// <summary>Variance from last count</summary>
    public decimal? LastCountVariance { get; set; }

    /// <summary>Last movement date</summary>
    public DateTime? LastMovementDate { get; set; }

    /// <summary>Days since last movement</summary>
    public int? DaysSinceLastMovement => LastMovementDate.HasValue
        ? (int)(DateTime.UtcNow - LastMovementDate.Value).TotalDays
        : null;

    /// <summary>Expiry date (for perishables)</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>Batch/Lot number</summary>
    public string? BatchNumber { get; set; }

    #endregion

    #region Assignment

    /// <summary>Customer ID if stock is allocated to a specific customer</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Project ID if stock is allocated to a specific project</summary>
    public Guid? ProjectId { get; set; }

    #endregion

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    #region Navigation Properties

    /// <summary>Parent company</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Product</summary>
    public virtual Domain.Entities.Product.Product Product { get; set; } = null!;

    /// <summary>Stock category</summary>
    public virtual StockCategory StockCategory { get; set; } = null!;

    /// <summary>Stock location</summary>
    public virtual StockLocation? Location { get; set; }

    /// <summary>Stock movements for this item</summary>
    public virtual ICollection<StockMovement> Movements { get; set; } = new List<StockMovement>();

    #endregion
}

/// <summary>
/// Status of a stock item
/// </summary>
public enum StockItemStatus
{
    Active = 0,
    Inactive = 1,
    Discontinued = 2,
    OnHold = 3,
    Quarantine = 4
}

/// <summary>
/// Costing method for inventory valuation
/// </summary>
public enum CostingMethod
{
    AverageCost = 0,
    FIFO = 1,
    LIFO = 2,
    StandardCost = 3,
    SpecificIdentification = 4
}
