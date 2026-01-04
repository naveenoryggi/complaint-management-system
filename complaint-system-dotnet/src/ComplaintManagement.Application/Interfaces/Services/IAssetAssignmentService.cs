using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for managing asset assignments to employees
/// </summary>
public interface IAssetAssignmentService
{
    #region Assignment Operations

    /// <summary>
    /// Assign an asset to an employee
    /// </summary>
    Task<AssetAssignmentDto> AssignAssetAsync(
        AssignAssetToEmployeeRequest request,
        Guid companyId,
        Guid assignedByUserId);

    /// <summary>
    /// Return an assigned asset
    /// </summary>
    Task<AssetAssignmentDto> ReturnAssetAsync(
        Guid assignmentId,
        ReturnAssetFromEmployeeRequest request,
        Guid companyId,
        Guid returnedToUserId);

    /// <summary>
    /// Transfer asset to another employee
    /// </summary>
    Task<AssetAssignmentDto> TransferAssetAsync(
        Guid assignmentId,
        TransferAssetToEmployeeRequest request,
        Guid companyId,
        Guid transferredByUserId);

    /// <summary>
    /// Extend an assignment's expected return date
    /// </summary>
    Task<AssetAssignmentDto> ExtendAssignmentAsync(
        Guid assignmentId,
        ExtendAssignmentRequest request,
        Guid companyId,
        Guid userId);

    /// <summary>
    /// Employee acknowledges receipt of an asset
    /// </summary>
    Task<AssetAssignmentDto> AcknowledgeAssetAsync(
        Guid assignmentId,
        AcknowledgeAssetRequest request,
        Guid companyId,
        Guid userId);

    /// <summary>
    /// Report an issue with an assigned asset (lost, damaged, etc.)
    /// </summary>
    Task<AssetAssignmentDto> ReportIssueAsync(
        Guid assignmentId,
        ReportAssetIssueRequest request,
        Guid companyId,
        Guid userId);

    #endregion

    #region Query Operations

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    Task<AssetAssignmentDto?> GetByIdAsync(Guid id, Guid companyId);

    /// <summary>
    /// Get assignment by assignment number
    /// </summary>
    Task<AssetAssignmentDto?> GetByAssignmentNumberAsync(string assignmentNumber, Guid companyId);

    /// <summary>
    /// Get all assignments for a company with optional filters
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetAssignmentsAsync(
        Guid companyId,
        bool? isActive = null,
        Guid? assetId = null,
        Guid? assignedToUserId = null,
        Guid? assignedToCustomerId = null,
        Guid? responsibleUserId = null,
        Guid? departmentId = null,
        AssetAssignmentPurpose? purpose = null,
        bool? isExternalAssignment = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Get assignment history for a specific asset
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetAssetHistoryAsync(Guid assetId, Guid companyId);

    /// <summary>
    /// Get all active assignments for an employee
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetEmployeeActiveAssignmentsAsync(Guid userId, Guid companyId);

    /// <summary>
    /// Get assignment history for an employee
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetEmployeeAssignmentHistoryAsync(
        Guid userId,
        Guid companyId,
        int? limit = null);

    /// <summary>
    /// Get overdue assignments
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetOverdueAssignmentsAsync(Guid companyId);

    /// <summary>
    /// Get assignments pending acknowledgement
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetPendingAcknowledgementsAsync(Guid companyId, Guid? userId = null);

    /// <summary>
    /// Get upcoming returns (within specified days)
    /// </summary>
    Task<List<AssetAssignmentSummaryDto>> GetUpcomingReturnsAsync(Guid companyId, int days = 7);

    #endregion

    #region Dashboard Operations

    /// <summary>
    /// Get employee's asset dashboard
    /// </summary>
    Task<EmployeeAssetDashboardDto> GetEmployeeDashboardAsync(Guid userId, Guid companyId);

    /// <summary>
    /// Get company-wide assignment statistics
    /// </summary>
    Task<AssetAssignmentStatisticsDto> GetStatisticsAsync(Guid companyId);

    #endregion

    #region Validation

    /// <summary>
    /// Check if an asset is available for assignment
    /// </summary>
    Task<bool> IsAssetAvailableForAssignmentAsync(Guid assetId, Guid companyId);

    /// <summary>
    /// Get the current active assignment for an asset (if any)
    /// </summary>
    Task<AssetAssignmentDto?> GetCurrentAssignmentAsync(Guid assetId, Guid companyId);

    #endregion
}
