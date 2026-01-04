using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// API endpoints for managing asset assignments to employees
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AssetAssignmentsController : ControllerBase
{
    private readonly IAssetAssignmentService _assignmentService;
    private readonly ILogger<AssetAssignmentsController> _logger;

    public AssetAssignmentsController(
        IAssetAssignmentService assignmentService,
        ILogger<AssetAssignmentsController> logger)
    {
        _assignmentService = assignmentService;
        _logger = logger;
    }

    #region Helpers

    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            throw new UnauthorizedAccessException("Company ID not found in claims");
        }
        return companyId;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }

    #endregion

    #region Assignment Operations

    /// <summary>
    /// Assign an asset to an employee
    /// </summary>
    [HttpPost]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> AssignAsset(
        [FromBody] AssignAssetToEmployeeRequest request)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var assignment = await _assignmentService.AssignAssetAsync(request, companyId, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { id = assignment.Id },
                assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Return an assigned asset
    /// </summary>
    [HttpPost("{id}/return")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> ReturnAsset(
        Guid id,
        [FromBody] ReturnAssetFromEmployeeRequest request)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var assignment = await _assignmentService.ReturnAssetAsync(id, request, companyId, userId);

            return Ok(assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Transfer asset to another employee
    /// </summary>
    [HttpPost("{id}/transfer")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> TransferAsset(
        Guid id,
        [FromBody] TransferAssetToEmployeeRequest request)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var assignment = await _assignmentService.TransferAssetAsync(id, request, companyId, userId);

            return Ok(assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Extend an assignment's expected return date
    /// </summary>
    [HttpPost("{id}/extend")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> ExtendAssignment(
        Guid id,
        [FromBody] ExtendAssignmentRequest request)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var assignment = await _assignmentService.ExtendAssignmentAsync(id, request, companyId, userId);

            return Ok(assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Employee acknowledges receipt of an asset
    /// </summary>
    [HttpPost("{id}/acknowledge")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AssetAssignmentDto>> AcknowledgeAsset(
        Guid id,
        [FromBody] AcknowledgeAssetRequest request)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var assignment = await _assignmentService.AcknowledgeAssetAsync(id, request, companyId, userId);

            return Ok(assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Report an issue with an assigned asset (lost, damaged, etc.)
    /// </summary>
    [HttpPost("{id}/report-issue")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> ReportIssue(
        Guid id,
        [FromBody] ReportAssetIssueRequest request)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var assignment = await _assignmentService.ReportIssueAsync(id, request, companyId, userId);

            return Ok(assignment);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    #endregion

    #region Query Operations

    /// <summary>
    /// Get assignment by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> GetById(Guid id)
    {
        var companyId = GetCompanyId();

        var assignment = await _assignmentService.GetByIdAsync(id, companyId);

        if (assignment == null)
            return NotFound(new { message = "Assignment not found" });

        return Ok(assignment);
    }

    /// <summary>
    /// Get assignment by assignment number
    /// </summary>
    [HttpGet("by-number/{assignmentNumber}")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> GetByAssignmentNumber(string assignmentNumber)
    {
        var companyId = GetCompanyId();

        var assignment = await _assignmentService.GetByAssignmentNumberAsync(assignmentNumber, companyId);

        if (assignment == null)
            return NotFound(new { message = "Assignment not found" });

        return Ok(assignment);
    }

    /// <summary>
    /// Get all assignments with optional filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetAssignments(
        [FromQuery] bool? isActive = null,
        [FromQuery] Guid? assetId = null,
        [FromQuery] Guid? assignedToUserId = null,
        [FromQuery] Guid? assignedToCustomerId = null,
        [FromQuery] Guid? responsibleUserId = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] AssetAssignmentPurpose? purpose = null,
        [FromQuery] bool? isExternalAssignment = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetAssignmentsAsync(
            companyId,
            isActive,
            assetId,
            assignedToUserId,
            assignedToCustomerId,
            responsibleUserId,
            departmentId,
            purpose,
            isExternalAssignment,
            fromDate,
            toDate);

        return Ok(assignments);
    }

    /// <summary>
    /// Get assignment history for a specific asset
    /// </summary>
    [HttpGet("asset/{assetId}/history")]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetAssetHistory(Guid assetId)
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetAssetHistoryAsync(assetId, companyId);

        return Ok(assignments);
    }

    /// <summary>
    /// Get current active assignment for an asset
    /// </summary>
    [HttpGet("asset/{assetId}/current")]
    [ProducesResponseType(typeof(AssetAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssetAssignmentDto>> GetCurrentAssignment(Guid assetId)
    {
        var companyId = GetCompanyId();

        var assignment = await _assignmentService.GetCurrentAssignmentAsync(assetId, companyId);

        if (assignment == null)
            return NotFound(new { message = "No active assignment found for this asset" });

        return Ok(assignment);
    }

    /// <summary>
    /// Check if an asset is available for assignment
    /// </summary>
    [HttpGet("asset/{assetId}/available")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<ActionResult<object>> IsAssetAvailable(Guid assetId)
    {
        var companyId = GetCompanyId();

        var isAvailable = await _assignmentService.IsAssetAvailableForAssignmentAsync(assetId, companyId);

        return Ok(new { assetId, isAvailable });
    }

    /// <summary>
    /// Get all active assignments for an employee
    /// </summary>
    [HttpGet("employee/{userId}/active")]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetEmployeeActiveAssignments(Guid userId)
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetEmployeeActiveAssignmentsAsync(userId, companyId);

        return Ok(assignments);
    }

    /// <summary>
    /// Get assignment history for an employee
    /// </summary>
    [HttpGet("employee/{userId}/history")]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetEmployeeHistory(
        Guid userId,
        [FromQuery] int? limit = null)
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetEmployeeAssignmentHistoryAsync(userId, companyId, limit);

        return Ok(assignments);
    }

    /// <summary>
    /// Get overdue assignments
    /// </summary>
    [HttpGet("overdue")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetOverdueAssignments()
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetOverdueAssignmentsAsync(companyId);

        return Ok(assignments);
    }

    /// <summary>
    /// Get assignments pending acknowledgement
    /// </summary>
    [HttpGet("pending-acknowledgement")]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetPendingAcknowledgements(
        [FromQuery] Guid? userId = null)
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetPendingAcknowledgementsAsync(companyId, userId);

        return Ok(assignments);
    }

    /// <summary>
    /// Get upcoming returns
    /// </summary>
    [HttpGet("upcoming-returns")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(List<AssetAssignmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssetAssignmentSummaryDto>>> GetUpcomingReturns(
        [FromQuery] int days = 7)
    {
        var companyId = GetCompanyId();

        var assignments = await _assignmentService.GetUpcomingReturnsAsync(companyId, days);

        return Ok(assignments);
    }

    #endregion

    #region Dashboard Operations

    /// <summary>
    /// Get current user's asset dashboard
    /// </summary>
    [HttpGet("my-dashboard")]
    [ProducesResponseType(typeof(EmployeeAssetDashboardDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<EmployeeAssetDashboardDto>> GetMyDashboard()
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var dashboard = await _assignmentService.GetEmployeeDashboardAsync(userId, companyId);

            return Ok(dashboard);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get employee's asset dashboard (admin view)
    /// </summary>
    [HttpGet("employee/{userId}/dashboard")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(EmployeeAssetDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EmployeeAssetDashboardDto>> GetEmployeeDashboard(Guid userId)
    {
        try
        {
            var companyId = GetCompanyId();

            var dashboard = await _assignmentService.GetEmployeeDashboardAsync(userId, companyId);

            return Ok(dashboard);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    /// <summary>
    /// Get company-wide assignment statistics
    /// </summary>
    [HttpGet("statistics")]
    [HasPermission("ManageAssets")]
    [ProducesResponseType(typeof(AssetAssignmentStatisticsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssetAssignmentStatisticsDto>> GetStatistics()
    {
        var companyId = GetCompanyId();

        var statistics = await _assignmentService.GetStatisticsAsync(companyId);

        return Ok(statistics);
    }

    #endregion
}
