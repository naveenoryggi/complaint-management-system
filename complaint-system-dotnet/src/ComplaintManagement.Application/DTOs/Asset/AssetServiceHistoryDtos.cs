using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

/// <summary>
/// Full service history DTO for API responses
/// </summary>
public class AssetServiceHistoryDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public Guid? ComplaintId { get; set; }
    public string? ComplaintNumber { get; set; }
    public Guid? ContractId { get; set; }
    public string? ContractNumber { get; set; }
    public Guid? TechnicianId { get; set; }
    public string? TechnicianName { get; set; }

    #region Service Details

    public string ServiceNumber { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public string ServiceTypeName => ServiceType.ToString();
    public string Description { get; set; } = string.Empty;
    public string? WorkPerformed { get; set; }
    public string? ProblemDescription { get; set; }
    public string? RootCause { get; set; }
    public string? Resolution { get; set; }
    public ServiceResult Result { get; set; }
    public string ResultName => Result.ToString();

    #endregion

    #region Timing

    public DateTime? ScheduledDate { get; set; }
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    public decimal? DurationHours { get; set; }
    public decimal? TravelTimeHours { get; set; }
    public decimal? DowntimeHours { get; set; }
    public DateTime? NextServiceDate { get; set; }

    #endregion

    #region Location

    public ServiceLocationType LocationType { get; set; }
    public string LocationTypeName => LocationType.ToString();
    public Guid? ServiceLocationId { get; set; }
    public string? ServiceLocationName { get; set; }
    public string? ServiceAddress { get; set; }

    #endregion

    #region Costs

    public decimal? LaborCost { get; set; }
    public decimal? PartsCost { get; set; }
    public decimal? TravelCost { get; set; }
    public decimal? OtherCosts { get; set; }
    public decimal? TotalCost { get; set; }
    public string Currency { get; set; } = "INR";
    public bool IsBillable { get; set; }
    public bool IsWarrantyCovered { get; set; }
    public bool IsContractCovered { get; set; }
    public string? InvoiceNumber { get; set; }

    #endregion

    #region Parts

    public List<PartUsedDto>? PartsUsed { get; set; }
    public List<PartReplacedDto>? PartsReplaced { get; set; }

    #endregion

    #region Asset Updates

    public AssetStatus? StatusBefore { get; set; }
    public string? StatusBeforeName => StatusBefore?.ToString();
    public AssetStatus? StatusAfter { get; set; }
    public string? StatusAfterName => StatusAfter?.ToString();
    public AssetCondition? ConditionBefore { get; set; }
    public string? ConditionBeforeName => ConditionBefore?.ToString();
    public AssetCondition? ConditionAfter { get; set; }
    public string? ConditionAfterName => ConditionAfter?.ToString();
    public decimal? MeterReadingBefore { get; set; }
    public decimal? MeterReadingAfter { get; set; }
    public string? FirmwareVersionBefore { get; set; }
    public string? FirmwareVersionAfter { get; set; }
    public string? SoftwareVersionBefore { get; set; }
    public string? SoftwareVersionAfter { get; set; }

    #endregion

    #region Signatures & Approval

    public bool CustomerSignedOff { get; set; }
    public string? SignedOffBy { get; set; }
    public DateTime? SignedOffAt { get; set; }
    public int? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }

    #endregion

    #region Additional Information

    public string? InternalNotes { get; set; }
    public string? Recommendations { get; set; }
    public string? FollowUpActions { get; set; }
    public List<string>? Attachments { get; set; }
    public List<string>? Tags { get; set; }
    public string? ExternalReferenceNumber { get; set; }

    #endregion

    #region Audit

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    #endregion
}

/// <summary>
/// Service history summary for list views
/// </summary>
public class AssetServiceHistorySummaryDto
{
    public Guid Id { get; set; }
    public string ServiceNumber { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public ServiceType ServiceType { get; set; }
    public string ServiceTypeName => ServiceType.ToString();
    public string Description { get; set; } = string.Empty;
    public ServiceResult Result { get; set; }
    public string ResultName => Result.ToString();
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    public decimal? DurationHours { get; set; }
    public string? TechnicianName { get; set; }
    public ServiceLocationType LocationType { get; set; }
    public string LocationTypeName => LocationType.ToString();
    public decimal? TotalCost { get; set; }
    public string Currency { get; set; } = "INR";
    public bool IsBillable { get; set; }
    public bool IsWarrantyCovered { get; set; }
    public bool IsContractCovered { get; set; }
    public bool CustomerSignedOff { get; set; }
    public int? CustomerRating { get; set; }
}

/// <summary>
/// Request to create a new service history record
/// </summary>
public class CreateServiceHistoryRequest
{
    public Guid AssetId { get; set; }
    public Guid? ComplaintId { get; set; }
    public Guid? ContractId { get; set; }
    public Guid? TechnicianId { get; set; }

    #region Service Details

    public ServiceType ServiceType { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? WorkPerformed { get; set; }
    public string? ProblemDescription { get; set; }
    public string? RootCause { get; set; }
    public string? Resolution { get; set; }
    public ServiceResult Result { get; set; }

    #endregion

    #region Timing

    public DateTime? ScheduledDate { get; set; }
    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    public decimal? DurationHours { get; set; }
    public decimal? TravelTimeHours { get; set; }
    public decimal? DowntimeHours { get; set; }
    public DateTime? NextServiceDate { get; set; }

    #endregion

    #region Location

    public ServiceLocationType LocationType { get; set; } = ServiceLocationType.Onsite;
    public Guid? ServiceLocationId { get; set; }
    public string? ServiceAddress { get; set; }

    #endregion

    #region Costs

    public decimal? LaborCost { get; set; }
    public decimal? PartsCost { get; set; }
    public decimal? TravelCost { get; set; }
    public decimal? OtherCosts { get; set; }
    public string Currency { get; set; } = "INR";
    public bool IsBillable { get; set; }
    public bool IsWarrantyCovered { get; set; }
    public bool IsContractCovered { get; set; }
    public string? InvoiceNumber { get; set; }

    #endregion

    #region Parts

    public List<PartUsedDto>? PartsUsed { get; set; }
    public List<PartReplacedDto>? PartsReplaced { get; set; }

    #endregion

    #region Asset Updates

    public AssetStatus? StatusBefore { get; set; }
    public AssetStatus? StatusAfter { get; set; }
    public AssetCondition? ConditionBefore { get; set; }
    public AssetCondition? ConditionAfter { get; set; }
    public decimal? MeterReadingBefore { get; set; }
    public decimal? MeterReadingAfter { get; set; }
    public string? FirmwareVersionBefore { get; set; }
    public string? FirmwareVersionAfter { get; set; }
    public string? SoftwareVersionBefore { get; set; }
    public string? SoftwareVersionAfter { get; set; }
    public string? ConfigurationChanges { get; set; }

    #endregion

    #region Signatures & Approval

    public bool CustomerSignedOff { get; set; }
    public string? SignedOffBy { get; set; }
    public DateTime? SignedOffAt { get; set; }
    public int? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }

    #endregion

    #region Additional Information

    public string? InternalNotes { get; set; }
    public string? Recommendations { get; set; }
    public string? FollowUpActions { get; set; }
    public List<string>? Attachments { get; set; }
    public List<string>? Tags { get; set; }
    public string? ExternalReferenceNumber { get; set; }

    #endregion

    /// <summary>
    /// Whether to update the asset's status/condition after service
    /// </summary>
    public bool UpdateAssetStatus { get; set; }

    /// <summary>
    /// Whether to update the asset's next service date
    /// </summary>
    public bool UpdateNextServiceDate { get; set; }
}

/// <summary>
/// Request to update service history record
/// </summary>
public class UpdateServiceHistoryRequest
{
    public Guid? TechnicianId { get; set; }

    #region Service Details

    public string Description { get; set; } = string.Empty;
    public string? WorkPerformed { get; set; }
    public string? ProblemDescription { get; set; }
    public string? RootCause { get; set; }
    public string? Resolution { get; set; }
    public ServiceResult Result { get; set; }

    #endregion

    #region Timing

    public DateTime? ServiceStartDate { get; set; }
    public DateTime? ServiceEndDate { get; set; }
    public decimal? DurationHours { get; set; }
    public decimal? TravelTimeHours { get; set; }
    public decimal? DowntimeHours { get; set; }
    public DateTime? NextServiceDate { get; set; }

    #endregion

    #region Costs

    public decimal? LaborCost { get; set; }
    public decimal? PartsCost { get; set; }
    public decimal? TravelCost { get; set; }
    public decimal? OtherCosts { get; set; }
    public bool IsBillable { get; set; }
    public string? InvoiceNumber { get; set; }

    #endregion

    #region Parts

    public List<PartUsedDto>? PartsUsed { get; set; }
    public List<PartReplacedDto>? PartsReplaced { get; set; }

    #endregion

    #region Asset Updates

    public AssetStatus? StatusAfter { get; set; }
    public AssetCondition? ConditionAfter { get; set; }
    public decimal? MeterReadingAfter { get; set; }
    public string? FirmwareVersionAfter { get; set; }
    public string? SoftwareVersionAfter { get; set; }
    public string? ConfigurationChanges { get; set; }

    #endregion

    #region Signatures & Approval

    public bool CustomerSignedOff { get; set; }
    public string? SignedOffBy { get; set; }
    public DateTime? SignedOffAt { get; set; }
    public int? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }

    #endregion

    #region Additional Information

    public string? InternalNotes { get; set; }
    public string? Recommendations { get; set; }
    public string? FollowUpActions { get; set; }
    public List<string>? Attachments { get; set; }
    public List<string>? Tags { get; set; }

    #endregion
}

/// <summary>
/// DTO for part used in service
/// </summary>
public class PartUsedDto
{
    public string PartNumber { get; set; } = string.Empty;
    public string PartName { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal? UnitCost { get; set; }
    public decimal? TotalCost { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// DTO for part replaced during service
/// </summary>
public class PartReplacedDto
{
    public string OldPartNumber { get; set; } = string.Empty;
    public string? OldSerialNumber { get; set; }
    public string NewPartNumber { get; set; } = string.Empty;
    public string? NewSerialNumber { get; set; }
    public string? Reason { get; set; }
    public decimal? PartCost { get; set; }
}

/// <summary>
/// Service history statistics for dashboard
/// </summary>
public class ServiceHistoryStatisticsDto
{
    public int TotalServiceRecords { get; set; }
    public int CompletedServices { get; set; }
    public int FailedServices { get; set; }
    public int InProgressServices { get; set; }
    public int WarrantyCoveredServices { get; set; }
    public int ContractCoveredServices { get; set; }
    public int BillableServices { get; set; }

    public decimal TotalLaborCost { get; set; }
    public decimal TotalPartsCost { get; set; }
    public decimal TotalServiceCost { get; set; }
    public decimal TotalDowntimeHours { get; set; }
    public decimal AverageServiceDuration { get; set; }
    public decimal AverageCustomerRating { get; set; }

    public Dictionary<string, int> ByServiceType { get; set; } = new();
    public Dictionary<string, int> ByResult { get; set; } = new();
    public Dictionary<string, int> ByLocationType { get; set; } = new();

    public string Currency { get; set; } = "INR";
}
