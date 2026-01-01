using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Represents a service/maintenance record for an asset
/// Tracks all service activities performed on assets
/// </summary>
public class AssetServiceHistory : BaseEntity
{
    /// <summary>
    /// Company that owns this service record
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Asset that was serviced
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Related complaint/ticket if any
    /// </summary>
    public Guid? ComplaintId { get; set; }

    /// <summary>
    /// Contract under which service was performed
    /// </summary>
    public Guid? ContractId { get; set; }

    /// <summary>
    /// Technician who performed the service
    /// </summary>
    public Guid? TechnicianId { get; set; }

    #region Service Details

    /// <summary>
    /// Service record number (auto-generated)
    /// </summary>
    public string ServiceNumber { get; set; } = string.Empty;

    /// <summary>
    /// Type of service performed
    /// </summary>
    public ServiceType ServiceType { get; set; }

    /// <summary>
    /// Service description/summary
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Detailed work performed
    /// </summary>
    public string? WorkPerformed { get; set; }

    /// <summary>
    /// Problem/issue description
    /// </summary>
    public string? ProblemDescription { get; set; }

    /// <summary>
    /// Root cause analysis
    /// </summary>
    public string? RootCause { get; set; }

    /// <summary>
    /// Resolution details
    /// </summary>
    public string? Resolution { get; set; }

    /// <summary>
    /// Result of the service
    /// </summary>
    public ServiceResult Result { get; set; }

    #endregion

    #region Timing

    /// <summary>
    /// Scheduled service date
    /// </summary>
    public DateTime? ScheduledDate { get; set; }

    /// <summary>
    /// Actual service start date/time
    /// </summary>
    public DateTime? ServiceStartDate { get; set; }

    /// <summary>
    /// Actual service end date/time
    /// </summary>
    public DateTime? ServiceEndDate { get; set; }

    /// <summary>
    /// Total service duration in hours
    /// </summary>
    public decimal? DurationHours { get; set; }

    /// <summary>
    /// Travel time in hours
    /// </summary>
    public decimal? TravelTimeHours { get; set; }

    /// <summary>
    /// Downtime during service in hours
    /// </summary>
    public decimal? DowntimeHours { get; set; }

    /// <summary>
    /// Next scheduled service date
    /// </summary>
    public DateTime? NextServiceDate { get; set; }

    #endregion

    #region Location

    /// <summary>
    /// Service location type
    /// </summary>
    public ServiceLocationType LocationType { get; set; } = ServiceLocationType.Onsite;

    /// <summary>
    /// Customer location where service was performed
    /// </summary>
    public Guid? ServiceLocationId { get; set; }

    /// <summary>
    /// Service address if different from asset location
    /// </summary>
    public string? ServiceAddress { get; set; }

    #endregion

    #region Costs

    /// <summary>
    /// Labor cost
    /// </summary>
    public decimal? LaborCost { get; set; }

    /// <summary>
    /// Parts cost
    /// </summary>
    public decimal? PartsCost { get; set; }

    /// <summary>
    /// Travel/transportation cost
    /// </summary>
    public decimal? TravelCost { get; set; }

    /// <summary>
    /// Other miscellaneous costs
    /// </summary>
    public decimal? OtherCosts { get; set; }

    /// <summary>
    /// Total service cost
    /// </summary>
    public decimal? TotalCost { get; set; }

    /// <summary>
    /// Currency code
    /// </summary>
    public string Currency { get; set; } = "INR";

    /// <summary>
    /// Whether service was billable
    /// </summary>
    public bool IsBillable { get; set; }

    /// <summary>
    /// Whether covered under warranty
    /// </summary>
    public bool IsWarrantyCovered { get; set; }

    /// <summary>
    /// Whether covered under contract
    /// </summary>
    public bool IsContractCovered { get; set; }

    /// <summary>
    /// Invoice number if billed
    /// </summary>
    public string? InvoiceNumber { get; set; }

    #endregion

    #region Parts Used

    /// <summary>
    /// Parts used during service (JSON array)
    /// </summary>
    public string? PartsUsed { get; set; }

    /// <summary>
    /// Parts replaced (JSON array)
    /// </summary>
    public string? PartsReplaced { get; set; }

    /// <summary>
    /// Serial numbers of replaced parts
    /// </summary>
    public string? ReplacedPartSerials { get; set; }

    #endregion

    #region Asset Updates

    /// <summary>
    /// Asset status before service
    /// </summary>
    public AssetStatus? StatusBefore { get; set; }

    /// <summary>
    /// Asset status after service
    /// </summary>
    public AssetStatus? StatusAfter { get; set; }

    /// <summary>
    /// Asset condition before service
    /// </summary>
    public AssetCondition? ConditionBefore { get; set; }

    /// <summary>
    /// Asset condition after service
    /// </summary>
    public AssetCondition? ConditionAfter { get; set; }

    /// <summary>
    /// Meter reading before service
    /// </summary>
    public decimal? MeterReadingBefore { get; set; }

    /// <summary>
    /// Meter reading after service
    /// </summary>
    public decimal? MeterReadingAfter { get; set; }

    /// <summary>
    /// Firmware version before update
    /// </summary>
    public string? FirmwareVersionBefore { get; set; }

    /// <summary>
    /// Firmware version after update
    /// </summary>
    public string? FirmwareVersionAfter { get; set; }

    /// <summary>
    /// Software version before update
    /// </summary>
    public string? SoftwareVersionBefore { get; set; }

    /// <summary>
    /// Software version after update
    /// </summary>
    public string? SoftwareVersionAfter { get; set; }

    /// <summary>
    /// Configuration changes made (JSON)
    /// </summary>
    public string? ConfigurationChanges { get; set; }

    #endregion

    #region Signatures & Approval

    /// <summary>
    /// Customer signature/approval
    /// </summary>
    public bool CustomerSignedOff { get; set; }

    /// <summary>
    /// Customer contact who signed off
    /// </summary>
    public string? SignedOffBy { get; set; }

    /// <summary>
    /// Sign off date/time
    /// </summary>
    public DateTime? SignedOffAt { get; set; }

    /// <summary>
    /// Customer feedback/rating
    /// </summary>
    public int? CustomerRating { get; set; }

    /// <summary>
    /// Customer feedback comments
    /// </summary>
    public string? CustomerFeedback { get; set; }

    #endregion

    #region Additional Information

    /// <summary>
    /// Internal notes
    /// </summary>
    public string? InternalNotes { get; set; }

    /// <summary>
    /// Recommendations for future
    /// </summary>
    public string? Recommendations { get; set; }

    /// <summary>
    /// Follow-up actions required
    /// </summary>
    public string? FollowUpActions { get; set; }

    /// <summary>
    /// Attached documents/photos (JSON array of URLs)
    /// </summary>
    public string? Attachments { get; set; }

    /// <summary>
    /// Tags for categorization
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Custom fields (JSON)
    /// </summary>
    public string? CustomFields { get; set; }

    /// <summary>
    /// External reference number
    /// </summary>
    public string? ExternalReferenceNumber { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Company that owns this record
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// Asset that was serviced
    /// </summary>
    public virtual Asset Asset { get; set; } = null!;

    /// <summary>
    /// Contract under which service was performed
    /// </summary>
    public virtual Contract? Contract { get; set; }

    #endregion
}

/// <summary>
/// Location type for service
/// </summary>
public enum ServiceLocationType
{
    /// <summary>
    /// Service performed at customer site
    /// </summary>
    Onsite = 1,

    /// <summary>
    /// Remote service/support
    /// </summary>
    Remote = 2,

    /// <summary>
    /// Asset brought to service depot
    /// </summary>
    Depot = 3,

    /// <summary>
    /// Service performed at workshop
    /// </summary>
    Workshop = 4,

    /// <summary>
    /// Field service
    /// </summary>
    Field = 5
}
