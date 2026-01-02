using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a stock movement/transaction (receipts, issues, transfers, adjustments, etc.)
/// Provides full audit trail of all stock transactions.
/// </summary>
public class StockMovement : BaseEntity
{
    /// <summary>Company that owns this movement</summary>
    public Guid CompanyId { get; set; }

    /// <summary>Stock item being moved (for quantity-based stock)</summary>
    public Guid? StockItemId { get; set; }

    /// <summary>Asset being moved (for serialized items)</summary>
    public Guid? AssetId { get; set; }

    /// <summary>Product being moved</summary>
    public Guid ProductId { get; set; }

    /// <summary>Movement type</summary>
    public StockMovementType MovementType { get; set; }

    /// <summary>Unique movement/transaction number</summary>
    public string MovementNumber { get; set; } = string.Empty;

    #region Locations

    /// <summary>Source location (for transfers, issues)</summary>
    public Guid? FromLocationId { get; set; }

    /// <summary>Destination location (for transfers, receipts)</summary>
    public Guid? ToLocationId { get; set; }

    /// <summary>Source stock category</summary>
    public Guid? FromStockCategoryId { get; set; }

    /// <summary>Destination stock category</summary>
    public Guid? ToStockCategoryId { get; set; }

    #endregion

    #region Quantities

    /// <summary>Quantity moved (positive for in, negative for out)</summary>
    public decimal Quantity { get; set; }

    /// <summary>Unit of measure</summary>
    public string UnitOfMeasure { get; set; } = "Each";

    /// <summary>Quantity before movement</summary>
    public decimal? QuantityBefore { get; set; }

    /// <summary>Quantity after movement</summary>
    public decimal? QuantityAfter { get; set; }

    #endregion

    #region Reference

    /// <summary>Reference type (PO, SO, Transfer, etc.)</summary>
    public StockReferenceType? ReferenceType { get; set; }

    /// <summary>Reference document number</summary>
    public string? ReferenceNumber { get; set; }

    /// <summary>Reference document ID</summary>
    public Guid? ReferenceId { get; set; }

    /// <summary>Related complaint ID (for warranty replacements, etc.)</summary>
    public Guid? ComplaintId { get; set; }

    /// <summary>Related customer ID</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Related project ID</summary>
    public Guid? ProjectId { get; set; }

    #endregion

    #region Valuation

    /// <summary>Unit cost at time of movement</summary>
    public decimal? UnitCost { get; set; }

    /// <summary>Total value of movement</summary>
    public decimal? TotalValue { get; set; }

    /// <summary>Currency code</summary>
    public string Currency { get; set; } = "INR";

    #endregion

    #region Movement Details

    /// <summary>Date and time of movement</summary>
    public DateTime MovementDate { get; set; } = DateTime.UtcNow;

    /// <summary>Reason for movement</summary>
    public string? Reason { get; set; }

    /// <summary>Status of movement</summary>
    public StockMovementStatus Status { get; set; } = StockMovementStatus.Completed;

    /// <summary>Serial number (for serialized items)</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Batch/Lot number</summary>
    public string? BatchNumber { get; set; }

    /// <summary>Expiry date (for perishables)</summary>
    public DateTime? ExpiryDate { get; set; }

    /// <summary>User who performed the movement</summary>
    public Guid? PerformedByUserId { get; set; }

    /// <summary>User who approved the movement</summary>
    public Guid? ApprovedByUserId { get; set; }

    /// <summary>Approval date</summary>
    public DateTime? ApprovalDate { get; set; }

    #endregion

    /// <summary>Additional notes</summary>
    public string? Notes { get; set; }

    #region Navigation Properties

    /// <summary>Parent company</summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>Stock item</summary>
    public virtual StockItem? StockItem { get; set; }

    /// <summary>Asset</summary>
    public virtual Asset? Asset { get; set; }

    /// <summary>Product</summary>
    public virtual Domain.Entities.Product.Product Product { get; set; } = null!;

    /// <summary>Source location</summary>
    public virtual StockLocation? FromLocation { get; set; }

    /// <summary>Destination location</summary>
    public virtual StockLocation? ToLocation { get; set; }

    /// <summary>Source stock category</summary>
    public virtual StockCategory? FromStockCategory { get; set; }

    /// <summary>Destination stock category</summary>
    public virtual StockCategory? ToStockCategory { get; set; }

    /// <summary>Related customer</summary>
    public virtual Customer? Customer { get; set; }

    #endregion
}

/// <summary>
/// Type of stock movement
/// </summary>
public enum StockMovementType
{
    // Incoming movements
    Receipt = 0,            // Receiving stock from purchase
    Return = 1,             // Customer return
    TransferIn = 2,         // Transfer from another location
    Production = 3,         // Finished goods from production
    Adjustment_In = 4,      // Positive adjustment

    // Outgoing movements
    Issue = 10,             // Issue to customer/project
    Sale = 11,              // Sale to customer
    TransferOut = 12,       // Transfer to another location
    Consumption = 13,       // Consumed in production/service
    Adjustment_Out = 14,    // Negative adjustment
    Scrap = 15,             // Scrapped/disposed
    Loss = 16,              // Loss/theft

    // Status changes
    CategoryChange = 20,    // Change stock category (e.g., Sales to Faulty)
    Reservation = 21,       // Reserve stock for order
    Unreservation = 22,     // Release reservation

    // Inventory
    StockCount = 30,        // Physical inventory count
    Revaluation = 31        // Cost revaluation
}

/// <summary>
/// Status of a stock movement
/// </summary>
public enum StockMovementStatus
{
    Draft = 0,
    Pending = 1,
    Approved = 2,
    Completed = 3,
    Cancelled = 4,
    Reversed = 5
}

/// <summary>
/// Type of reference document for stock movement
/// </summary>
public enum StockReferenceType
{
    PurchaseOrder = 0,
    SalesOrder = 1,
    TransferOrder = 2,
    ServiceRequest = 3,
    Complaint = 4,
    Project = 5,
    Contract = 6,
    Warranty = 7,
    Manual = 8
}
