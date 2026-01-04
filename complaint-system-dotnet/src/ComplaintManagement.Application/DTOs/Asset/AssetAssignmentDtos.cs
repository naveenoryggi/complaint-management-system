using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.DTOs.Asset;

/// <summary>
/// Full asset assignment DTO for API responses
/// </summary>
public class AssetAssignmentDto
{
    public Guid Id { get; set; }
    public Guid CompanyId { get; set; }
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetSerialNumber { get; set; }
    public AssetType AssetType { get; set; }
    public string AssetTypeName => AssetType.ToString();

    #region Assignment Details

    /// <summary>
    /// Internal employee assigned (for internal assignments)
    /// </summary>
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? AssignedToUserEmail { get; set; }
    public string? AssignedToEmployeeCode { get; set; }

    /// <summary>
    /// Customer/System Integrator assigned (for external assignments)
    /// </summary>
    public Guid? AssignedToCustomerId { get; set; }
    public string? AssignedToCustomerName { get; set; }
    public string? AssignedToCustomerCode { get; set; }

    /// <summary>
    /// Internal user responsible for tracking (for external assignments)
    /// </summary>
    public Guid? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public string? ResponsibleUserEmail { get; set; }

    /// <summary>
    /// Whether this is an external assignment (to customer/SI)
    /// </summary>
    public bool IsExternalAssignment => AssignedToCustomerId.HasValue;

    /// <summary>
    /// Display name for assignee (customer or employee name)
    /// </summary>
    public string AssigneeName => IsExternalAssignment
        ? AssignedToCustomerName ?? "Unknown Customer"
        : AssignedToUserName ?? "Unknown User";

    public Guid AssignedByUserId { get; set; }
    public string AssignedByUserName { get; set; } = string.Empty;

    public AssetAssignmentAction Action { get; set; }
    public string ActionName => Action.ToString();

    public AssetAssignmentPurpose Purpose { get; set; }
    public string PurposeName => Purpose.ToString();

    public DateTime AssignmentDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public Guid? ReturnedToUserId { get; set; }
    public string? ReturnedToUserName { get; set; }

    public bool IsActive { get; set; }

    #endregion

    #region Condition Tracking

    public AssetCondition ConditionAtAssignment { get; set; }
    public string ConditionAtAssignmentName => ConditionAtAssignment.ToString();

    public AssetCondition? ConditionAtReturn { get; set; }
    public string? ConditionAtReturnName => ConditionAtReturn?.ToString();

    public string? ConditionNotesAtAssignment { get; set; }
    public string? ConditionNotesAtReturn { get; set; }

    #endregion

    #region Location

    public string? Location { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? CostCenter { get; set; }

    #endregion

    #region Documentation

    public string? AssignmentNumber { get; set; }
    public string? Notes { get; set; }
    public bool IsAcknowledged { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public string? AcknowledgementReference { get; set; }

    #endregion

    #region Computed Properties

    public bool IsOverdue => ExpectedReturnDate.HasValue
        && ExpectedReturnDate < DateTime.UtcNow
        && !ActualReturnDate.HasValue
        && IsActive;

    public int? DaysUntilDue => ExpectedReturnDate.HasValue && IsActive
        ? (int)(ExpectedReturnDate.Value - DateTime.UtcNow).TotalDays
        : null;

    public int? DaysOverdue => IsOverdue
        ? (int)(DateTime.UtcNow - ExpectedReturnDate!.Value).TotalDays
        : null;

    #endregion

    #region Audit

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    #endregion
}

/// <summary>
/// Summary DTO for list views
/// </summary>
public class AssetAssignmentSummaryDto
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetSerialNumber { get; set; }
    public AssetType AssetType { get; set; }
    public string AssetTypeName => AssetType.ToString();

    // Internal assignment
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }
    public string? AssignedToEmployeeCode { get; set; }

    // Customer/SI assignment
    public Guid? AssignedToCustomerId { get; set; }
    public string? AssignedToCustomerName { get; set; }
    public string? AssignedToCustomerCode { get; set; }

    // Responsible internal user
    public Guid? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }

    /// <summary>
    /// Whether this is an external assignment
    /// </summary>
    public bool IsExternalAssignment => AssignedToCustomerId.HasValue;

    /// <summary>
    /// Display name for assignee
    /// </summary>
    public string AssigneeName => IsExternalAssignment
        ? AssignedToCustomerName ?? "Unknown Customer"
        : AssignedToUserName ?? "Unknown User";

    public AssetAssignmentAction Action { get; set; }
    public string ActionName => Action.ToString();

    public AssetAssignmentPurpose Purpose { get; set; }
    public string PurposeName => Purpose.ToString();

    public DateTime AssignmentDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    public bool IsActive { get; set; }
    public bool IsOverdue => ExpectedReturnDate.HasValue
        && ExpectedReturnDate < DateTime.UtcNow
        && !ActualReturnDate.HasValue
        && IsActive;

    public AssetCondition ConditionAtAssignment { get; set; }
    public string ConditionAtAssignmentName => ConditionAtAssignment.ToString();

    public bool IsAcknowledged { get; set; }
    public string? AssignmentNumber { get; set; }
}

/// <summary>
/// Request to assign an asset to an employee or customer/SI
/// </summary>
public class AssignAssetToEmployeeRequest
{
    /// <summary>
    /// Asset to assign
    /// </summary>
    public Guid AssetId { get; set; }

    /// <summary>
    /// Employee (User) to assign the asset to (for internal assignments)
    /// Required if AssignedToCustomerId is null
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Customer/System Integrator to assign the asset to (for external assignments)
    /// Required if AssignedToUserId is null
    /// </summary>
    public Guid? AssignedToCustomerId { get; set; }

    /// <summary>
    /// Internal user responsible for tracking the external assignment
    /// Required when assigning to customer/SI (AssignedToCustomerId is set)
    /// </summary>
    public Guid? ResponsibleUserId { get; set; }

    /// <summary>
    /// Purpose of the assignment
    /// </summary>
    public AssetAssignmentPurpose Purpose { get; set; } = AssetAssignmentPurpose.Permanent;

    /// <summary>
    /// Date of assignment (defaults to now)
    /// </summary>
    public DateTime? AssignmentDate { get; set; }

    /// <summary>
    /// Expected return date (for temporary assignments)
    /// </summary>
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>
    /// Condition of asset at assignment
    /// </summary>
    public AssetCondition ConditionAtAssignment { get; set; } = AssetCondition.Good;

    /// <summary>
    /// Notes about condition at assignment
    /// </summary>
    public string? ConditionNotesAtAssignment { get; set; }

    /// <summary>
    /// Location where asset will be used
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Department ID
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Cost center
    /// </summary>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Notes about the assignment
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Whether this is an external assignment
    /// </summary>
    public bool IsExternalAssignment => AssignedToCustomerId.HasValue;
}

/// <summary>
/// Request to return an assigned asset
/// </summary>
public class ReturnAssetFromEmployeeRequest
{
    /// <summary>
    /// Condition of asset at return
    /// </summary>
    public AssetCondition ConditionAtReturn { get; set; } = AssetCondition.Good;

    /// <summary>
    /// Notes about condition at return
    /// </summary>
    public string? ConditionNotesAtReturn { get; set; }

    /// <summary>
    /// Actual return date (defaults to now)
    /// </summary>
    public DateTime? ReturnDate { get; set; }

    /// <summary>
    /// Notes about the return
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request to transfer asset to another employee
/// </summary>
public class TransferAssetToEmployeeRequest
{
    /// <summary>
    /// New employee (User) to transfer the asset to
    /// </summary>
    public Guid NewAssignedToUserId { get; set; }

    /// <summary>
    /// Purpose of the new assignment
    /// </summary>
    public AssetAssignmentPurpose Purpose { get; set; } = AssetAssignmentPurpose.Permanent;

    /// <summary>
    /// Date of transfer (defaults to now)
    /// </summary>
    public DateTime? TransferDate { get; set; }

    /// <summary>
    /// Expected return date (for temporary assignments)
    /// </summary>
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>
    /// Condition of asset at transfer
    /// </summary>
    public AssetCondition ConditionAtTransfer { get; set; } = AssetCondition.Good;

    /// <summary>
    /// Notes about condition
    /// </summary>
    public string? ConditionNotes { get; set; }

    /// <summary>
    /// New location
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// New department
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// New cost center
    /// </summary>
    public string? CostCenter { get; set; }

    /// <summary>
    /// Transfer reason/notes
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request to extend an assignment's expected return date
/// </summary>
public class ExtendAssignmentRequest
{
    /// <summary>
    /// New expected return date
    /// </summary>
    public DateTime NewExpectedReturnDate { get; set; }

    /// <summary>
    /// Reason for extension
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request to acknowledge receipt of an asset
/// </summary>
public class AcknowledgeAssetRequest
{
    /// <summary>
    /// Digital signature or acknowledgement reference
    /// </summary>
    public string? AcknowledgementReference { get; set; }

    /// <summary>
    /// Any notes from the employee
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Request to report an issue with an assigned asset
/// </summary>
public class ReportAssetIssueRequest
{
    /// <summary>
    /// Type of issue
    /// </summary>
    public AssetAssignmentAction IssueType { get; set; }

    /// <summary>
    /// Current condition of the asset
    /// </summary>
    public AssetCondition CurrentCondition { get; set; }

    /// <summary>
    /// Description of the issue
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Date issue was discovered
    /// </summary>
    public DateTime? IssueDate { get; set; }
}

/// <summary>
/// Employee's asset dashboard summary
/// </summary>
public class EmployeeAssetDashboardDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Currently assigned assets (directly to this user)
    /// </summary>
    public List<AssetAssignmentSummaryDto> CurrentAssignments { get; set; } = new();

    /// <summary>
    /// Assets assigned to customers/SIs that this user is responsible for tracking
    /// </summary>
    public List<AssetAssignmentSummaryDto> ResponsibleForAssignments { get; set; } = new();

    /// <summary>
    /// Assignments pending acknowledgement
    /// </summary>
    public List<AssetAssignmentSummaryDto> PendingAcknowledgement { get; set; } = new();

    /// <summary>
    /// Overdue returns (both direct and responsible)
    /// </summary>
    public List<AssetAssignmentSummaryDto> OverdueReturns { get; set; } = new();

    /// <summary>
    /// Upcoming returns (next 7 days)
    /// </summary>
    public List<AssetAssignmentSummaryDto> UpcomingReturns { get; set; } = new();

    /// <summary>
    /// Recent assignment history
    /// </summary>
    public List<AssetAssignmentSummaryDto> RecentHistory { get; set; } = new();

    /// <summary>
    /// Statistics for own assignments
    /// </summary>
    public EmployeeAssignmentStatisticsDto Statistics { get; set; } = new();
}

/// <summary>
/// Statistics for employee's asset assignments
/// </summary>
public class EmployeeAssignmentStatisticsDto
{
    public int TotalActiveAssignments { get; set; }
    public int PermanentAssignments { get; set; }
    public int TemporaryAssignments { get; set; }
    public int OverdueCount { get; set; }
    public int PendingAcknowledgementCount { get; set; }

    /// <summary>
    /// Number of external assignments this user is responsible for
    /// </summary>
    public int ResponsibleForCount { get; set; }

    /// <summary>
    /// Number of overdue external assignments this user is responsible for
    /// </summary>
    public int ResponsibleForOverdueCount { get; set; }
}

/// <summary>
/// Company-wide asset assignment statistics
/// </summary>
public class AssetAssignmentStatisticsDto
{
    public int TotalAssignments { get; set; }
    public int ActiveAssignments { get; set; }
    public int ReturnedAssignments { get; set; }
    public int OverdueAssignments { get; set; }
    public int PendingAcknowledgements { get; set; }

    public Dictionary<string, int> ByPurpose { get; set; } = new();
    public Dictionary<string, int> ByAssetType { get; set; } = new();
    public Dictionary<string, int> ByDepartment { get; set; } = new();

    /// <summary>
    /// Top employees by number of assigned assets
    /// </summary>
    public List<EmployeeAssignmentCountDto> TopAssignees { get; set; } = new();
}

/// <summary>
/// Employee assignment count for statistics
/// </summary>
public class EmployeeAssignmentCountDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public int AssignmentCount { get; set; }
}
