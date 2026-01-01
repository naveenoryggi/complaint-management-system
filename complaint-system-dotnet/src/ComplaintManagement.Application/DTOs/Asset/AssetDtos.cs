using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

/// <summary>
/// Full asset DTO for API responses
/// </summary>
public class AssetDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ContractId { get; set; }
    public string? ContractNumber { get; set; }
    public Guid? WarrantyId { get; set; }

    #region Identity

    public string AssetTag { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetType Type { get; set; }
    public string TypeName => Type.ToString();
    public string? Barcode { get; set; }
    public string? RFIDTag { get; set; }

    #endregion

    #region Product Details

    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? SKU { get; set; }

    #endregion

    #region Lifecycle Dates

    public DateTime? ManufactureDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InstallationDate { get; set; }
    public DateTime? CommissionedDate { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? ExtendedWarrantyEndDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime? RetiredDate { get; set; }
    public DateTime? DisposedDate { get; set; }

    // Computed warranty status
    public bool IsUnderWarranty { get; set; }
    public int? WarrantyDaysRemaining { get; set; }
    public bool IsWarrantyExpiringSoon { get; set; }

    #endregion

    #region Financials

    public decimal? PurchasePrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal? ReplacementCost { get; set; }
    public string Currency { get; set; } = "INR";
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Vendor { get; set; }

    #endregion

    #region Depreciation

    public DepreciationMethod DepreciationMethod { get; set; }
    public string DepreciationMethodName => DepreciationMethod.ToString();
    public int? UsefulLifeMonths { get; set; }
    public decimal? DepreciationRate { get; set; }
    public decimal? AccumulatedDepreciation { get; set; }

    #endregion

    #region Ownership

    public AssetOwnershipType OwnershipType { get; set; }
    public string OwnershipTypeName => OwnershipType.ToString();
    public string? LeaseAgreementNumber { get; set; }
    public DateTime? LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal? LeasePayment { get; set; }

    #endregion

    #region Status

    public AssetStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public AssetCondition Condition { get; set; }
    public string ConditionName => Condition.ToString();
    public string? StatusReason { get; set; }
    public bool IsOperational { get; set; }

    #endregion

    #region Configuration

    public string? FirmwareVersion { get; set; }
    public string? SoftwareVersion { get; set; }
    public string? OperatingSystem { get; set; }
    public string? IPAddress { get; set; }
    public string? MACAddress { get; set; }
    public string? Hostname { get; set; }

    #endregion

    #region Physical Location

    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }
    public string? Rack { get; set; }
    public string? RackUnit { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    #endregion

    #region Maintenance

    public MaintenanceFrequency MaintenanceFrequency { get; set; }
    public string MaintenanceFrequencyName => MaintenanceFrequency.ToString();
    public int? MaintenanceIntervalDays { get; set; }
    public int ServiceCallCount { get; set; }
    public decimal? TotalDowntimeHours { get; set; }
    public decimal? MTBF { get; set; }
    public decimal? MTTR { get; set; }

    #endregion

    #region Usage Tracking

    public decimal? CurrentMeterReading { get; set; }
    public string? MeterUnit { get; set; }
    public DateTime? LastMeterReadingDate { get; set; }
    public decimal? AverageDailyUsage { get; set; }

    #endregion

    #region Assignment

    public Guid? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? CostCenter { get; set; }

    #endregion

    #region External References

    public string? ExternalAssetId { get; set; }
    public Guid? ParentAssetId { get; set; }
    public string? ParentAssetTag { get; set; }

    #endregion

    #region Additional Information

    public string? Notes { get; set; }
    public List<string> Tags { get; set; } = new();

    #endregion

    #region Related Data

    public int ChildAssetCount { get; set; }
    public int ServiceHistoryCount { get; set; }
    public int ContractItemCount { get; set; }
    public List<AssetServiceHistorySummaryDto> RecentServiceHistory { get; set; } = new();

    #endregion

    #region Audit

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    #endregion
}

/// <summary>
/// Asset summary for list views
/// </summary>
public class AssetSummaryDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssetType Type { get; set; }
    public string TypeName => Type.ToString();
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string? LocationName { get; set; }
    public string? ProductName { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public AssetStatus Status { get; set; }
    public string StatusName => Status.ToString();
    public AssetCondition Condition { get; set; }
    public string ConditionName => Condition.ToString();
    public bool IsOperational { get; set; }
    public bool IsUnderWarranty { get; set; }
    public bool IsUnderContract { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public decimal? CurrentValue { get; set; }
    public string Currency { get; set; } = "INR";
    public int ServiceCallCount { get; set; }
}

/// <summary>
/// Asset lookup for dropdowns
/// </summary>
public class AssetLookupDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName => string.IsNullOrEmpty(SerialNumber)
        ? $"{AssetTag} - {Name}"
        : $"{AssetTag} ({SerialNumber}) - {Name}";
    public AssetType Type { get; set; }
    public AssetStatus Status { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public bool IsOperational { get; set; }
    public bool IsUnderWarranty { get; set; }
}

/// <summary>
/// Request to create a new asset
/// </summary>
public class CreateAssetRequest
{
    public Guid CustomerId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? WarrantyId { get; set; }

    #region Identity

    public string? AssetTag { get; set; } // Auto-generated if not provided
    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetType Type { get; set; }
    public string? Barcode { get; set; }
    public string? RFIDTag { get; set; }

    #endregion

    #region Product Details

    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? SKU { get; set; }

    #endregion

    #region Lifecycle Dates

    public DateTime? ManufactureDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InstallationDate { get; set; }
    public DateTime? CommissionedDate { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? ExtendedWarrantyEndDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }

    #endregion

    #region Financials

    public decimal? PurchasePrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal? ReplacementCost { get; set; }
    public string Currency { get; set; } = "INR";
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Vendor { get; set; }

    #endregion

    #region Depreciation

    public DepreciationMethod DepreciationMethod { get; set; } = DepreciationMethod.StraightLine;
    public int? UsefulLifeMonths { get; set; }
    public decimal? DepreciationRate { get; set; }

    #endregion

    #region Ownership

    public AssetOwnershipType OwnershipType { get; set; } = AssetOwnershipType.Owned;
    public string? LeaseAgreementNumber { get; set; }
    public DateTime? LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal? LeasePayment { get; set; }

    #endregion

    #region Status

    public AssetStatus Status { get; set; } = AssetStatus.InStock;
    public AssetCondition Condition { get; set; } = AssetCondition.New;
    public string? StatusReason { get; set; }
    public bool IsOperational { get; set; } = true;

    #endregion

    #region Configuration

    public string? Specifications { get; set; }
    public string? Configuration { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? SoftwareVersion { get; set; }
    public string? OperatingSystem { get; set; }
    public string? IPAddress { get; set; }
    public string? MACAddress { get; set; }
    public string? Hostname { get; set; }

    #endregion

    #region Physical Location

    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }
    public string? Rack { get; set; }
    public string? RackUnit { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    #endregion

    #region Maintenance

    public MaintenanceFrequency MaintenanceFrequency { get; set; } = MaintenanceFrequency.None;
    public int? MaintenanceIntervalDays { get; set; }

    #endregion

    #region Usage Tracking

    public decimal? CurrentMeterReading { get; set; }
    public string? MeterUnit { get; set; }

    #endregion

    #region Assignment

    public Guid? AssignedUserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? CostCenter { get; set; }

    #endregion

    #region External References

    public string? ExternalAssetId { get; set; }
    public Guid? ParentAssetId { get; set; }

    #endregion

    #region Additional Information

    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public string? CustomFields { get; set; }

    #endregion
}

/// <summary>
/// Request to update an existing asset
/// </summary>
public class UpdateAssetRequest
{
    public Guid? LocationId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? WarrantyId { get; set; }

    #region Identity

    public string? SerialNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public AssetType Type { get; set; }
    public string? Barcode { get; set; }
    public string? RFIDTag { get; set; }

    #endregion

    #region Product Details

    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }
    public string? SKU { get; set; }

    #endregion

    #region Lifecycle Dates

    public DateTime? ManufactureDate { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public DateTime? InstallationDate { get; set; }
    public DateTime? CommissionedDate { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? ExtendedWarrantyEndDate { get; set; }
    public DateTime? EndOfLifeDate { get; set; }
    public DateTime? RetiredDate { get; set; }
    public DateTime? DisposedDate { get; set; }

    #endregion

    #region Financials

    public decimal? PurchasePrice { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? SalvageValue { get; set; }
    public decimal? ReplacementCost { get; set; }
    public string Currency { get; set; } = "INR";
    public string? PONumber { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? Vendor { get; set; }

    #endregion

    #region Depreciation

    public DepreciationMethod DepreciationMethod { get; set; }
    public int? UsefulLifeMonths { get; set; }
    public decimal? DepreciationRate { get; set; }
    public decimal? AccumulatedDepreciation { get; set; }

    #endregion

    #region Ownership

    public AssetOwnershipType OwnershipType { get; set; }
    public string? LeaseAgreementNumber { get; set; }
    public DateTime? LeaseStartDate { get; set; }
    public DateTime? LeaseEndDate { get; set; }
    public decimal? LeasePayment { get; set; }

    #endregion

    #region Status

    public AssetStatus Status { get; set; }
    public AssetCondition Condition { get; set; }
    public string? StatusReason { get; set; }
    public bool IsOperational { get; set; }

    #endregion

    #region Configuration

    public string? Specifications { get; set; }
    public string? Configuration { get; set; }
    public string? FirmwareVersion { get; set; }
    public string? SoftwareVersion { get; set; }
    public string? OperatingSystem { get; set; }
    public string? IPAddress { get; set; }
    public string? MACAddress { get; set; }
    public string? Hostname { get; set; }

    #endregion

    #region Physical Location

    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }
    public string? Rack { get; set; }
    public string? RackUnit { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    #endregion

    #region Maintenance

    public MaintenanceFrequency MaintenanceFrequency { get; set; }
    public int? MaintenanceIntervalDays { get; set; }
    public DateTime? NextServiceDate { get; set; }

    #endregion

    #region Usage Tracking

    public decimal? CurrentMeterReading { get; set; }
    public string? MeterUnit { get; set; }
    public decimal? AverageDailyUsage { get; set; }

    #endregion

    #region Assignment

    public Guid? AssignedUserId { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? CostCenter { get; set; }

    #endregion

    #region External References

    public string? ExternalAssetId { get; set; }
    public Guid? ParentAssetId { get; set; }

    #endregion

    #region Additional Information

    public string? Notes { get; set; }
    public List<string>? Tags { get; set; }
    public string? CustomFields { get; set; }

    #endregion
}

/// <summary>
/// Request to update asset status
/// </summary>
public class UpdateAssetStatusRequest
{
    public AssetStatus Status { get; set; }
    public AssetCondition? Condition { get; set; }
    public string? StatusReason { get; set; }
    public bool? IsOperational { get; set; }
}

/// <summary>
/// Request to transfer asset to another customer/location
/// </summary>
public class TransferAssetRequest
{
    public Guid NewCustomerId { get; set; }
    public Guid? NewLocationId { get; set; }
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;
    public string? TransferReason { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Request to record meter reading
/// </summary>
public class RecordMeterReadingRequest
{
    public decimal MeterReading { get; set; }
    public string? MeterUnit { get; set; }
    public DateTime ReadingDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}

/// <summary>
/// Asset statistics for dashboard
/// </summary>
public class AssetStatisticsDto
{
    public int TotalAssets { get; set; }
    public int OperationalAssets { get; set; }
    public int NonOperationalAssets { get; set; }
    public int AssetsUnderWarranty { get; set; }
    public int AssetsUnderContract { get; set; }
    public int AssetsNeedingService { get; set; }
    public int RetiredAssets { get; set; }

    public Dictionary<string, int> ByStatus { get; set; } = new();
    public Dictionary<string, int> ByType { get; set; } = new();
    public Dictionary<string, int> ByCondition { get; set; } = new();
    public Dictionary<string, int> ByCustomer { get; set; } = new();

    public decimal TotalAssetValue { get; set; }
    public decimal TotalAccumulatedDepreciation { get; set; }
    public string Currency { get; set; } = "INR";
}
