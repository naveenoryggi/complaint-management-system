namespace ComplaintManagement.Domain.Enums.Service;

/// <summary>
/// Status of an asset in its lifecycle
/// </summary>
public enum AssetStatus
{
    /// <summary>
    /// Asset is ordered but not yet received
    /// </summary>
    Ordered = 0,

    /// <summary>
    /// Asset has been received and is being prepared
    /// </summary>
    Received = 1,

    /// <summary>
    /// Asset is in stock/inventory
    /// </summary>
    InStock = 2,

    /// <summary>
    /// Asset is deployed and active
    /// </summary>
    Deployed = 3,

    /// <summary>
    /// Asset is currently being serviced
    /// </summary>
    InService = 4,

    /// <summary>
    /// Asset is under repair
    /// </summary>
    UnderRepair = 5,

    /// <summary>
    /// Asset is temporarily out of service
    /// </summary>
    OutOfService = 6,

    /// <summary>
    /// Asset has been retired from use
    /// </summary>
    Retired = 7,

    /// <summary>
    /// Asset has been disposed/scrapped
    /// </summary>
    Disposed = 8,

    /// <summary>
    /// Asset is lost or missing
    /// </summary>
    Lost = 9,

    /// <summary>
    /// Asset is in transit between locations
    /// </summary>
    InTransit = 10,

    /// <summary>
    /// Asset is on loan to another party
    /// </summary>
    OnLoan = 11,

    /// <summary>
    /// Asset has been returned (for leased assets)
    /// </summary>
    Returned = 12,

    /// <summary>
    /// Asset is reserved for a specific purpose
    /// </summary>
    Reserved = 13
}

/// <summary>
/// Physical condition of an asset
/// </summary>
public enum AssetCondition
{
    /// <summary>
    /// Brand new, never used
    /// </summary>
    New = 1,

    /// <summary>
    /// Excellent condition, like new
    /// </summary>
    Excellent = 2,

    /// <summary>
    /// Good working condition with minor wear
    /// </summary>
    Good = 3,

    /// <summary>
    /// Fair condition, functional but shows wear
    /// </summary>
    Fair = 4,

    /// <summary>
    /// Poor condition, needs attention
    /// </summary>
    Poor = 5,

    /// <summary>
    /// Non-functional, requires repair
    /// </summary>
    NonFunctional = 6,

    /// <summary>
    /// Damaged beyond economical repair
    /// </summary>
    BeyondRepair = 7,

    /// <summary>
    /// Refurbished/reconditioned
    /// </summary>
    Refurbished = 8
}

/// <summary>
/// Type of asset ownership
/// </summary>
public enum AssetOwnershipType
{
    /// <summary>
    /// Company owned asset
    /// </summary>
    Owned = 1,

    /// <summary>
    /// Leased from a vendor
    /// </summary>
    Leased = 2,

    /// <summary>
    /// Rented on short-term basis
    /// </summary>
    Rented = 3,

    /// <summary>
    /// On loan from another party
    /// </summary>
    Loaned = 4,

    /// <summary>
    /// Financed/under payment plan
    /// </summary>
    Financed = 5,

    /// <summary>
    /// Customer owned (service depot asset)
    /// </summary>
    CustomerOwned = 6,

    /// <summary>
    /// Vendor consignment
    /// </summary>
    Consignment = 7,

    /// <summary>
    /// Asset for demo purposes (temporary assignment)
    /// </summary>
    Demo = 8,

    /// <summary>
    /// Asset for development/testing purposes
    /// </summary>
    Development = 9
}

/// <summary>
/// Purpose of asset assignment
/// </summary>
public enum AssetAssignmentPurpose
{
    /// <summary>
    /// Permanent assignment
    /// </summary>
    Permanent = 1,

    /// <summary>
    /// Demo/evaluation purpose
    /// </summary>
    Demo = 2,

    /// <summary>
    /// Development/testing purpose
    /// </summary>
    Development = 3,

    /// <summary>
    /// Temporary loan
    /// </summary>
    TemporaryLoan = 4,

    /// <summary>
    /// Training purpose
    /// </summary>
    Training = 5,

    /// <summary>
    /// Proof of concept
    /// </summary>
    POC = 6,

    /// <summary>
    /// Replacement while other asset is repaired
    /// </summary>
    Replacement = 7
}

/// <summary>
/// Stock category classification for inventory management
/// Determines the business purpose/classification of asset inventory
/// </summary>
public enum AssetStockCategory
{
    /// <summary>
    /// Not categorized as stock
    /// </summary>
    None = 0,

    /// <summary>
    /// Sales stock - Available for sale to customers
    /// </summary>
    SalesStock = 1,

    /// <summary>
    /// Fixed company asset - Company-owned assets for internal use
    /// </summary>
    FixedAsset = 2,

    /// <summary>
    /// Aftersales stock - Replacement/warranty stock for service
    /// </summary>
    AftersalesStock = 3,

    /// <summary>
    /// Faulty stock - Defective items awaiting repair/disposal
    /// </summary>
    FaultyStock = 4,

    /// <summary>
    /// Demo stock - Assets used for demonstrations
    /// </summary>
    DemoStock = 5,

    /// <summary>
    /// Development stock - Assets for R&D/development purposes
    /// </summary>
    DevelopmentStock = 6,

    /// <summary>
    /// Spare parts - Components kept for repairs
    /// </summary>
    SpareParts = 7,

    /// <summary>
    /// Consignment stock - Stock held on behalf of vendor
    /// </summary>
    ConsignmentStock = 8,

    /// <summary>
    /// Rental stock - Assets available for rental
    /// </summary>
    RentalStock = 9,

    /// <summary>
    /// Refurbished stock - Repaired/reconditioned items
    /// </summary>
    RefurbishedStock = 10,

    /// <summary>
    /// Transit stock - Items in transit between locations
    /// </summary>
    TransitStock = 11,

    /// <summary>
    /// Reserved stock - Reserved for specific purpose/order
    /// </summary>
    ReservedStock = 12,

    /// <summary>
    /// Obsolete stock - Outdated items for write-off
    /// </summary>
    ObsoleteStock = 13
}

/// <summary>
/// Type of asset
/// </summary>
public enum AssetType
{
    /// <summary>
    /// Hardware equipment
    /// </summary>
    Hardware = 1,

    /// <summary>
    /// Software license
    /// </summary>
    Software = 2,

    /// <summary>
    /// Network equipment
    /// </summary>
    Network = 3,

    /// <summary>
    /// Peripheral device
    /// </summary>
    Peripheral = 4,

    /// <summary>
    /// Mobile device
    /// </summary>
    Mobile = 5,

    /// <summary>
    /// Furniture
    /// </summary>
    Furniture = 6,

    /// <summary>
    /// Vehicle
    /// </summary>
    Vehicle = 7,

    /// <summary>
    /// Machinery/equipment
    /// </summary>
    Machinery = 8,

    /// <summary>
    /// Tool
    /// </summary>
    Tool = 9,

    /// <summary>
    /// Office equipment
    /// </summary>
    OfficeEquipment = 10,

    /// <summary>
    /// Medical equipment
    /// </summary>
    MedicalEquipment = 11,

    /// <summary>
    /// Lab equipment
    /// </summary>
    LabEquipment = 12,

    /// <summary>
    /// Other/miscellaneous
    /// </summary>
    Other = 99
}

/// <summary>
/// Type of service performed on an asset
/// </summary>
public enum ServiceType
{
    /// <summary>
    /// Initial installation
    /// </summary>
    Installation = 1,

    /// <summary>
    /// Preventive maintenance
    /// </summary>
    PreventiveMaintenance = 2,

    /// <summary>
    /// Corrective maintenance/repair
    /// </summary>
    CorrectiveMaintenance = 3,

    /// <summary>
    /// Calibration
    /// </summary>
    Calibration = 4,

    /// <summary>
    /// Inspection
    /// </summary>
    Inspection = 5,

    /// <summary>
    /// Software update/upgrade
    /// </summary>
    SoftwareUpdate = 6,

    /// <summary>
    /// Firmware update
    /// </summary>
    FirmwareUpdate = 7,

    /// <summary>
    /// Hardware upgrade
    /// </summary>
    HardwareUpgrade = 8,

    /// <summary>
    /// Configuration change
    /// </summary>
    Configuration = 9,

    /// <summary>
    /// Relocation/move
    /// </summary>
    Relocation = 10,

    /// <summary>
    /// Decommissioning
    /// </summary>
    Decommission = 11,

    /// <summary>
    /// Warranty repair
    /// </summary>
    WarrantyRepair = 12,

    /// <summary>
    /// Emergency repair
    /// </summary>
    EmergencyRepair = 13,

    /// <summary>
    /// Cleaning/sanitization
    /// </summary>
    Cleaning = 14,

    /// <summary>
    /// Testing
    /// </summary>
    Testing = 15,

    /// <summary>
    /// Refurbishment
    /// </summary>
    Refurbishment = 16
}

/// <summary>
/// Result of a service operation
/// </summary>
public enum ServiceResult
{
    /// <summary>
    /// Service completed successfully
    /// </summary>
    Completed = 1,

    /// <summary>
    /// Service completed with issues noted
    /// </summary>
    CompletedWithIssues = 2,

    /// <summary>
    /// Service partially completed
    /// </summary>
    PartiallyCompleted = 3,

    /// <summary>
    /// Service failed
    /// </summary>
    Failed = 4,

    /// <summary>
    /// Service cancelled
    /// </summary>
    Cancelled = 5,

    /// <summary>
    /// Service rescheduled
    /// </summary>
    Rescheduled = 6,

    /// <summary>
    /// Waiting for parts
    /// </summary>
    WaitingForParts = 7,

    /// <summary>
    /// Waiting for customer
    /// </summary>
    WaitingForCustomer = 8,

    /// <summary>
    /// In progress
    /// </summary>
    InProgress = 9,

    /// <summary>
    /// Asset replaced instead of repaired
    /// </summary>
    AssetReplaced = 10
}

/// <summary>
/// Depreciation method for asset valuation
/// </summary>
public enum DepreciationMethod
{
    /// <summary>
    /// No depreciation
    /// </summary>
    None = 0,

    /// <summary>
    /// Straight line depreciation
    /// </summary>
    StraightLine = 1,

    /// <summary>
    /// Declining balance method
    /// </summary>
    DecliningBalance = 2,

    /// <summary>
    /// Double declining balance
    /// </summary>
    DoubleDecliningBalance = 3,

    /// <summary>
    /// Sum of years digits
    /// </summary>
    SumOfYearsDigits = 4,

    /// <summary>
    /// Units of production
    /// </summary>
    UnitsOfProduction = 5
}

/// <summary>
/// Maintenance schedule frequency
/// </summary>
public enum MaintenanceFrequency
{
    /// <summary>
    /// No scheduled maintenance
    /// </summary>
    None = 0,

    /// <summary>
    /// Daily maintenance
    /// </summary>
    Daily = 1,

    /// <summary>
    /// Weekly maintenance
    /// </summary>
    Weekly = 2,

    /// <summary>
    /// Bi-weekly (every 2 weeks)
    /// </summary>
    BiWeekly = 3,

    /// <summary>
    /// Monthly maintenance
    /// </summary>
    Monthly = 4,

    /// <summary>
    /// Quarterly maintenance
    /// </summary>
    Quarterly = 5,

    /// <summary>
    /// Semi-annual maintenance
    /// </summary>
    SemiAnnual = 6,

    /// <summary>
    /// Annual maintenance
    /// </summary>
    Annual = 7,

    /// <summary>
    /// As needed/on-demand
    /// </summary>
    AsNeeded = 8,

    /// <summary>
    /// Based on usage metrics
    /// </summary>
    UsageBased = 9
}
