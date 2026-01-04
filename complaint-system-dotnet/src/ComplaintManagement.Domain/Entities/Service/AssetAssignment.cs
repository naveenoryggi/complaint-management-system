using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Domain.Entities.Service;

/// <summary>
/// Tracks the assignment history of assets to employees
/// Maintains a complete audit trail of who had what asset and when
/// </summary>
public class AssetAssignment : BaseEntity
{
    /// <summary>
    /// Company that owns this record
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// The asset being assigned
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Employee (User) to whom the asset is assigned (for internal assignments)
    /// Null when assigned to a customer/SI
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Customer/System Integrator to whom the asset is assigned (for external assignments)
    /// Null when assigned to an internal employee
    /// </summary>
    public Guid? AssignedToCustomerId { get; set; }

    /// <summary>
    /// Internal user responsible for tracking this assignment (required for external assignments)
    /// E.g., Sales rep, Account manager who is accountable for the demo unit
    /// </summary>
    public Guid? ResponsibleUserId { get; set; }

    /// <summary>
    /// Employee (User) who performed the assignment
    /// </summary>
    public Guid AssignedByUserId { get; set; }

    #region Assignment Details

    /// <summary>
    /// Type of assignment action
    /// </summary>
    public AssetAssignmentAction Action { get; set; }

    /// <summary>
    /// Purpose of the assignment
    /// </summary>
    public AssetAssignmentPurpose Purpose { get; set; } = AssetAssignmentPurpose.Permanent;

    /// <summary>
    /// Date when assignment started
    /// </summary>
    public DateTime AssignmentDate { get; set; }

    /// <summary>
    /// Expected return date (for temporary assignments)
    /// </summary>
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>
    /// Actual return date (when asset was returned)
    /// </summary>
    public DateTime? ActualReturnDate { get; set; }

    /// <summary>
    /// User who processed the return
    /// </summary>
    public Guid? ReturnedToUserId { get; set; }

    /// <summary>
    /// Whether assignment is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;

    #endregion

    #region Condition Tracking

    /// <summary>
    /// Condition of asset at assignment time
    /// </summary>
    public AssetCondition ConditionAtAssignment { get; set; }

    /// <summary>
    /// Condition of asset at return time
    /// </summary>
    public AssetCondition? ConditionAtReturn { get; set; }

    /// <summary>
    /// Notes about condition at assignment
    /// </summary>
    public string? ConditionNotesAtAssignment { get; set; }

    /// <summary>
    /// Notes about condition at return
    /// </summary>
    public string? ConditionNotesAtReturn { get; set; }

    #endregion

    #region Location

    /// <summary>
    /// Location/department where asset will be used
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Department ID
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Cost center for this assignment
    /// </summary>
    public string? CostCenter { get; set; }

    #endregion

    #region Documentation

    /// <summary>
    /// Assignment reference number (e.g., ASN-2024-00001)
    /// </summary>
    public string? AssignmentNumber { get; set; }

    /// <summary>
    /// Notes about the assignment
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether employee acknowledged receipt (signed)
    /// </summary>
    public bool IsAcknowledged { get; set; } = false;

    /// <summary>
    /// Date of acknowledgement
    /// </summary>
    public DateTime? AcknowledgedAt { get; set; }

    /// <summary>
    /// Digital signature or acknowledgement reference
    /// </summary>
    public string? AcknowledgementReference { get; set; }

    /// <summary>
    /// Attached documents (JSON array of URLs)
    /// </summary>
    public string? Documents { get; set; }

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Company
    /// </summary>
    public virtual Company Company { get; set; } = null!;

    /// <summary>
    /// The asset
    /// </summary>
    public virtual Asset Asset { get; set; } = null!;

    /// <summary>
    /// Customer/System Integrator (for external assignments)
    /// </summary>
    public virtual Customer? AssignedToCustomer { get; set; }

    /// <summary>
    /// Internal user responsible for tracking the external assignment
    /// </summary>
    public virtual User? ResponsibleUser { get; set; }

    #endregion

    #region Computed Properties

    /// <summary>
    /// Whether this is an external assignment (to customer/SI)
    /// </summary>
    public bool IsExternalAssignment => AssignedToCustomerId.HasValue;

    /// <summary>
    /// Whether this assignment is overdue for return
    /// </summary>
    public bool IsOverdue => IsActive && ExpectedReturnDate.HasValue && ExpectedReturnDate < DateTime.UtcNow;

    #endregion
}

/// <summary>
/// Types of asset assignment actions
/// </summary>
public enum AssetAssignmentAction
{
    /// <summary>
    /// New assignment to employee
    /// </summary>
    Assigned = 0,

    /// <summary>
    /// Asset returned from employee
    /// </summary>
    Returned = 1,

    /// <summary>
    /// Asset transferred to another employee
    /// </summary>
    Transferred = 2,

    /// <summary>
    /// Assignment extended (new expected return date)
    /// </summary>
    Extended = 3,

    /// <summary>
    /// Asset reported lost by employee
    /// </summary>
    ReportedLost = 4,

    /// <summary>
    /// Asset reported damaged
    /// </summary>
    ReportedDamaged = 5,

    /// <summary>
    /// Asset sent for repair
    /// </summary>
    SentForRepair = 6,

    /// <summary>
    /// Asset returned from repair
    /// </summary>
    ReturnedFromRepair = 7
}
