using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Product;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a physical or virtual asset tracked in the system
/// Assets can be owned by customers and covered under contracts/warranties
/// </summary>
public class Asset : BaseEntity
{
    /// <summary>
    /// Company that owns this asset record
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Customer who owns/uses this asset (null for internal assets)
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Whether this is an internal company asset (not customer-owned)
    /// Internal assets are assigned to employees and not visible in customer-facing views
    /// </summary>
    public bool IsInternal { get; set; } = false;

    /// <summary>
    /// Location where asset is installed
    /// </summary>
    public Guid? LocationId { get; set; }

    /// <summary>
    /// Product this asset is an instance of
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Active contract covering this asset
    /// </summary>
    public Guid? ContractId { get; set; }

    /// <summary>
    /// Warranty definition applicable to this asset
    /// </summary>
    public Guid? WarrantyId { get; set; }

    #region Identity

    /// <summary>
    /// Internal asset tag (e.g., AST-2024-00001)
    /// </summary>
    public string AssetTag { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturer serial number
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Asset name/description
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Type of asset
    /// </summary>
    public AssetType Type { get; set; }

    /// <summary>
    /// Barcode/QR code value
    /// </summary>
    public string? Barcode { get; set; }

    /// <summary>
    /// RFID tag value
    /// </summary>
    public string? RFIDTag { get; set; }

    #endregion

    #region Product Details (Denormalized for History)

    /// <summary>
    /// Product name at time of asset creation
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Product code at time of asset creation
    /// </summary>
    public string? ProductCode { get; set; }

    /// <summary>
    /// Manufacturer name
    /// </summary>
    public string? Manufacturer { get; set; }

    /// <summary>
    /// Model number
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Product version/revision
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// SKU (Stock Keeping Unit)
    /// </summary>
    public string? SKU { get; set; }

    #endregion

    #region Lifecycle Dates

    /// <summary>
    /// Date of manufacture
    /// </summary>
    public DateTime? ManufactureDate { get; set; }

    /// <summary>
    /// Date asset was purchased
    /// </summary>
    public DateTime? PurchaseDate { get; set; }

    /// <summary>
    /// Date asset was received
    /// </summary>
    public DateTime? ReceivedDate { get; set; }

    /// <summary>
    /// Date asset was installed/deployed
    /// </summary>
    public DateTime? InstallationDate { get; set; }

    /// <summary>
    /// Date asset was commissioned
    /// </summary>
    public DateTime? CommissionedDate { get; set; }

    /// <summary>
    /// Warranty start date
    /// </summary>
    public DateTime? WarrantyStartDate { get; set; }

    /// <summary>
    /// Warranty end date
    /// </summary>
    public DateTime? WarrantyEndDate { get; set; }

    /// <summary>
    /// Extended warranty end date
    /// </summary>
    public DateTime? ExtendedWarrantyEndDate { get; set; }

    /// <summary>
    /// Last service/maintenance date
    /// </summary>
    public DateTime? LastServiceDate { get; set; }

    /// <summary>
    /// Next scheduled service date
    /// </summary>
    public DateTime? NextServiceDate { get; set; }

    /// <summary>
    /// Expected end of life date
    /// </summary>
    public DateTime? EndOfLifeDate { get; set; }

    /// <summary>
    /// Date asset was retired
    /// </summary>
    public DateTime? RetiredDate { get; set; }

    /// <summary>
    /// Date asset was disposed
    /// </summary>
    public DateTime? DisposedDate { get; set; }

    #endregion

    #region Financials

    /// <summary>
    /// Original purchase price
    /// </summary>
    public decimal? PurchasePrice { get; set; }

    /// <summary>
    /// Current book value
    /// </summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>
    /// Salvage value at end of life
    /// </summary>
    public decimal? SalvageValue { get; set; }

    /// <summary>
    /// Replacement cost
    /// </summary>
    public decimal? ReplacementCost { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Purchase order number
    /// </summary>
    public string? PONumber { get; set; }

    /// <summary>
    /// Invoice number
    /// </summary>
    public string? InvoiceNumber { get; set; }

    /// <summary>
    /// Vendor/supplier name
    /// </summary>
    public string? Vendor { get; set; }

    #endregion

    #region Depreciation

    /// <summary>
    /// Depreciation method used
    /// </summary>
    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;

    /// <summary>
    /// Useful life in months for depreciation
    /// </summary>
    public int? UsefulLifeMonths { get; set; }

    /// <summary>
    /// Annual depreciation rate (percentage)
    /// </summary>
    public decimal? DepreciationRate { get; set; }

    /// <summary>
    /// Accumulated depreciation to date
    /// </summary>
    public decimal? AccumulatedDepreciation { get; set; }

    #endregion

    #region Ownership

    /// <summary>
    /// Type of ownership
    /// </summary>
    public AssetOwnershipType OwnershipType { get; set; } = AssetOwnershipType.Owned;

    /// <summary>
    /// Lease/rental agreement number
    /// </summary>
    public string? LeaseAgreementNumber { get; set; }

    /// <summary>
    /// Lease start date
    /// </summary>
    public DateTime? LeaseStartDate { get; set; }

    /// <summary>
    /// Lease end date
    /// </summary>
    public DateTime? LeaseEndDate { get; set; }

    /// <summary>
    /// Monthly lease payment
    /// </summary>
    public decimal? LeasePayment { get; set; }

    #endregion

    #region Status

    /// <summary>
    /// Current asset status
    /// </summary>
    public AssetStatus Status { get; set; } = AssetStatus.InStock;

    /// <summary>
    /// Physical condition
    /// </summary>
    public AssetCondition Condition { get; set; } = AssetCondition.New;

    /// <summary>
    /// Reason for current status
    /// </summary>
    public string? StatusReason { get; set; }

    /// <summary>
    /// Whether asset is currently operational
    /// </summary>
    public bool IsOperational { get; set; } = true;

    /// <summary>
    /// Reference to dynamic stock category (Sales Stock, Fixed Asset, Aftersales, Faulty, etc.)
    /// </summary>
    public Guid? StockCategoryId { get; set; }

    #endregion

    #region Configuration

    /// <summary>
    /// Technical specifications (JSON)
    /// </summary>
    public string? Specifications { get; set; }

    /// <summary>
    /// Configuration details (JSON)
    /// </summary>
    public string? Configuration { get; set; }

    /// <summary>
    /// Current firmware version
    /// </summary>
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// Current software version
    /// </summary>
    public string? SoftwareVersion { get; set; }

    /// <summary>
    /// Operating system
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// IP address (if applicable)
    /// </summary>
    public string? IPAddress { get; set; }

    /// <summary>
    /// MAC address (if applicable)
    /// </summary>
    public string? MACAddress { get; set; }

    /// <summary>
    /// Hostname (if applicable)
    /// </summary>
    public string? Hostname { get; set; }

    #endregion

    #region Physical Location

    /// <summary>
    /// Building name/number
    /// </summary>
    public string? Building { get; set; }

    /// <summary>
    /// Floor number
    /// </summary>
    public string? Floor { get; set; }

    /// <summary>
    /// Room number
    /// </summary>
    public string? Room { get; set; }

    /// <summary>
    /// Rack/cabinet identifier
    /// </summary>
    public string? Rack { get; set; }

    /// <summary>
    /// Rack unit position (for rack-mounted equipment)
    /// </summary>
    public string? RackUnit { get; set; }

    /// <summary>
    /// GPS latitude
    /// </summary>
    public double? Latitude { get; set; }

    /// <summary>
    /// GPS longitude
    /// </summary>
    public double? Longitude { get; set; }

    #endregion

    #region Maintenance

    /// <summary>
    /// Maintenance frequency
    /// </summary>
    public MaintenanceFrequency MaintenanceFrequency { get; set; } = MaintenanceFrequency.None;

    /// <summary>
    /// Maintenance interval in days
    /// </summary>
    public int? MaintenanceIntervalDays { get; set; }

    /// <summary>
    /// Total service calls for this asset
    /// </summary>
    public int ServiceCallCount { get; set; }

    /// <summary>
    /// Total downtime in hours
    /// </summary>
    public decimal? TotalDowntimeHours { get; set; }

    /// <summary>
    /// Mean time between failures (hours)
    /// </summary>
    public decimal? MTBF { get; set; }

    /// <summary>
    /// Mean time to repair (hours)
    /// </summary>
    public decimal? MTTR { get; set; }

    #endregion

    #region Usage Tracking

    /// <summary>
    /// Current usage meter reading
    /// </summary>
    public decimal? CurrentMeterReading { get; set; }

    /// <summary>
    /// Unit for meter reading (hours, miles, cycles, etc.)
    /// </summary>
    public string? MeterUnit { get; set; }

    /// <summary>
    /// Last meter reading date
    /// </summary>
    public DateTime? LastMeterReadingDate { get; set; }

    /// <summary>
    /// Average daily usage
    /// </summary>
    public decimal? AverageDailyUsage { get; set; }

    #endregion

    #region Assignment

    /// <summary>
    /// User currently assigned to this asset (employee)
    /// </summary>
    public Guid? AssignedUserId { get; set; }

    /// <summary>
    /// Department owning this asset
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Cost center for this asset
    /// </summary>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Purpose of assignment (Demo, Development, Permanent, etc.)
    /// </summary>
    public AssetAssignmentPurpose AssignmentPurpose { get; set; } = AssetAssignmentPurpose.Permanent;

    /// <summary>
    /// Date when asset was assigned
    /// </summary>
    public DateTime? AssignmentDate { get; set; }

    /// <summary>
    /// Expected return date (for temporary assignments)
    /// </summary>
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>
    /// Actual return date
    /// </summary>
    public DateTime? ActualReturnDate { get; set; }

    /// <summary>
    /// Notes about the assignment
    /// </summary>
    public string? AssignmentNotes { get; set; }

    #endregion

    #region External References

    /// <summary>
    /// External asset ID (ERP/CMMS reference)
    /// </summary>
    public string? ExternalAssetId { get; set; }

    /// <summary>
    /// Parent asset ID (for component tracking)
    /// </summary>
    public Guid? ParentAssetId { get; set; }

    #endregion

    #region Additional Information

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Tags for categorization (JSON array)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Custom fields (JSON)
    /// </summary>
    public string? CustomFields { get; set; }

    /// <summary>
    /// Document URLs (JSON array)
    /// </summary>
    public string? Documents { get; set; }

    /// <summary>
    /// Image URLs (JSON array)
    /// </summary>
    public string? Images { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Company that owns this asset record
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// Customer who owns/uses this asset (null for internal assets)
    /// </summary>
    public virtual Customer? Customer { get; set; }

    /// <summary>
    /// Location where asset is installed
    /// </summary>
    public virtual CustomerLocation? Location { get; set; }

    /// <summary>
    /// Product this asset is an instance of
    /// </summary>
    public virtual Domain.Entities.Product.Product? Product { get; set; }

    /// <summary>
    /// Active contract covering this asset
    /// </summary>
    public virtual Contract? Contract { get; set; }

    /// <summary>
    /// Warranty definition applicable to this asset
    /// </summary>
    public virtual Warranty? Warranty { get; set; }

    /// <summary>
    /// Stock category for this asset
    /// </summary>
    public virtual StockCategory? StockCategory { get; set; }

    /// <summary>
    /// Parent asset (for component hierarchy)
    /// </summary>
    public virtual Asset? ParentAsset { get; set; }

    /// <summary>
    /// Child assets (components)
    /// </summary>
    public virtual ICollection<Asset> ChildAssets { get; set; } = new List<Asset>();

    /// <summary>
    /// Service history records
    /// </summary>
    public virtual ICollection<AssetServiceHistory> ServiceHistory { get; set; } = new List<AssetServiceHistory>();

    /// <summary>
    /// Contract items referencing this asset
    /// </summary>
    public virtual ICollection<ContractItem> ContractItems { get; set; } = new List<ContractItem>();

    /// <summary>
    /// Assignment history (for internal assets assigned to employees)
    /// </summary>
    public virtual ICollection<AssetAssignment> AssignmentHistory { get; set; } = new List<AssetAssignment>();

    #endregion
}
