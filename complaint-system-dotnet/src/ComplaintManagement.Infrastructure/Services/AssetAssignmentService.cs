using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing asset assignments to employees
/// </summary>
public class AssetAssignmentService : IAssetAssignmentService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<AssetAssignmentService> _logger;

    public AssetAssignmentService(
        ComplaintDbContext context,
        ILogger<AssetAssignmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Assignment Operations

    public async Task<AssetAssignmentDto> AssignAssetAsync(
        AssignAssetToEmployeeRequest request,
        Guid companyId,
        Guid assignedByUserId)
    {
        // Validate that either employee or customer is specified
        if (!request.AssignedToUserId.HasValue && !request.AssignedToCustomerId.HasValue)
            throw new InvalidOperationException("Either AssignedToUserId or AssignedToCustomerId must be specified");

        if (request.AssignedToUserId.HasValue && request.AssignedToCustomerId.HasValue)
            throw new InvalidOperationException("Cannot assign to both employee and customer simultaneously");

        // For external assignments, responsible user is required
        if (request.AssignedToCustomerId.HasValue && !request.ResponsibleUserId.HasValue)
            throw new InvalidOperationException("ResponsibleUserId is required for external (customer) assignments");

        // Validate asset exists and belongs to company
        var asset = await _context.Assets
            .Include(a => a.Customer)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId && a.CompanyId == companyId);

        if (asset == null)
            throw new InvalidOperationException("Asset not found");

        // Check if asset is already assigned
        var existingAssignment = await _context.AssetAssignments
            .FirstOrDefaultAsync(a => a.AssetId == request.AssetId
                && a.CompanyId == companyId
                && a.IsActive);

        if (existingAssignment != null)
            throw new InvalidOperationException($"Asset is already assigned. Please return it first.");

        // Validate employee exists (for internal assignments)
        if (request.AssignedToUserId.HasValue)
        {
            var employee = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.AssignedToUserId && u.CompanyId == companyId);

            if (employee == null)
                throw new InvalidOperationException("Employee not found");
        }

        // Validate customer exists (for external assignments)
        if (request.AssignedToCustomerId.HasValue)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == request.AssignedToCustomerId && c.CompanyId == companyId);

            if (customer == null)
                throw new InvalidOperationException("Customer/System Integrator not found");
        }

        // Validate responsible user exists (for external assignments)
        if (request.ResponsibleUserId.HasValue)
        {
            var responsibleUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == request.ResponsibleUserId && u.CompanyId == companyId);

            if (responsibleUser == null)
                throw new InvalidOperationException("Responsible user not found");
        }

        // Generate assignment number
        var assignmentNumber = await GenerateAssignmentNumberAsync(companyId);

        // Create assignment record
        var assignment = new AssetAssignment
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            AssetId = request.AssetId,
            AssignedToUserId = request.AssignedToUserId,
            AssignedToCustomerId = request.AssignedToCustomerId,
            ResponsibleUserId = request.ResponsibleUserId,
            AssignedByUserId = assignedByUserId,
            Action = AssetAssignmentAction.Assigned,
            Purpose = request.Purpose,
            AssignmentDate = request.AssignmentDate ?? DateTime.UtcNow,
            ExpectedReturnDate = request.ExpectedReturnDate,
            IsActive = true,
            ConditionAtAssignment = request.ConditionAtAssignment,
            ConditionNotesAtAssignment = request.ConditionNotesAtAssignment,
            Location = request.Location,
            DepartmentId = request.DepartmentId,
            CostCenter = request.CostCenter,
            AssignmentNumber = assignmentNumber,
            Notes = request.Notes,
            IsAcknowledged = false
        };

        _context.AssetAssignments.Add(assignment);

        // Update asset with assignment info
        asset.AssignedUserId = request.AssignedToUserId;
        asset.CustomerId = request.AssignedToCustomerId;
        asset.AssignmentPurpose = request.Purpose;
        asset.AssignmentDate = assignment.AssignmentDate;
        asset.ExpectedReturnDate = request.ExpectedReturnDate;
        asset.ActualReturnDate = null;
        asset.AssignmentNotes = request.Notes;
        asset.DepartmentId = request.DepartmentId;
        asset.CostCenter = request.CostCenter;
        asset.Status = AssetStatus.Deployed;
        asset.IsInternal = !request.AssignedToCustomerId.HasValue; // Internal only if assigned to employee

        await _context.SaveChangesAsync();

        var assigneeType = request.AssignedToCustomerId.HasValue ? "customer" : "employee";
        var assigneeId = request.AssignedToCustomerId ?? request.AssignedToUserId;

        _logger.LogInformation(
            "Asset {AssetTag} assigned to {AssigneeType} {AssigneeId} with assignment number {AssignmentNumber}",
            asset.AssetTag, assigneeType, assigneeId, assignmentNumber);

        return await GetByIdAsync(assignment.Id, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve assignment after creation");
    }

    public async Task<AssetAssignmentDto> ReturnAssetAsync(
        Guid assignmentId,
        ReturnAssetFromEmployeeRequest request,
        Guid companyId,
        Guid returnedToUserId)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CompanyId == companyId);

        if (assignment == null)
            throw new InvalidOperationException("Assignment not found");

        if (!assignment.IsActive)
            throw new InvalidOperationException("Assignment is not active");

        // Update assignment record
        assignment.Action = AssetAssignmentAction.Returned;
        assignment.ActualReturnDate = request.ReturnDate ?? DateTime.UtcNow;
        assignment.ReturnedToUserId = returnedToUserId;
        assignment.IsActive = false;
        assignment.ConditionAtReturn = request.ConditionAtReturn;
        assignment.ConditionNotesAtReturn = request.ConditionNotesAtReturn;
        if (!string.IsNullOrEmpty(request.Notes))
            assignment.Notes = string.IsNullOrEmpty(assignment.Notes)
                ? request.Notes
                : $"{assignment.Notes}\n\nReturn Notes: {request.Notes}";

        // Update asset
        var asset = assignment.Asset;
        asset.AssignedUserId = null;
        asset.ActualReturnDate = assignment.ActualReturnDate;
        asset.Status = AssetStatus.InStock;
        asset.Condition = request.ConditionAtReturn;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Asset {AssetTag} returned from employee. Assignment {AssignmentNumber} closed.",
            asset.AssetTag, assignment.AssignmentNumber);

        return await GetByIdAsync(assignment.Id, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve assignment after update");
    }

    public async Task<AssetAssignmentDto> TransferAssetAsync(
        Guid assignmentId,
        TransferAssetToEmployeeRequest request,
        Guid companyId,
        Guid transferredByUserId)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CompanyId == companyId);

        if (assignment == null)
            throw new InvalidOperationException("Assignment not found");

        if (!assignment.IsActive)
            throw new InvalidOperationException("Assignment is not active");

        // Validate new employee exists
        var newEmployee = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.NewAssignedToUserId && u.CompanyId == companyId);

        if (newEmployee == null)
            throw new InvalidOperationException("New employee not found");

        // Close current assignment
        assignment.Action = AssetAssignmentAction.Transferred;
        assignment.ActualReturnDate = request.TransferDate ?? DateTime.UtcNow;
        assignment.ReturnedToUserId = transferredByUserId;
        assignment.IsActive = false;
        assignment.ConditionAtReturn = request.ConditionAtTransfer;
        assignment.ConditionNotesAtReturn = request.ConditionNotes;
        if (!string.IsNullOrEmpty(request.Notes))
            assignment.Notes = string.IsNullOrEmpty(assignment.Notes)
                ? request.Notes
                : $"{assignment.Notes}\n\nTransfer Notes: {request.Notes}";

        // Generate new assignment number
        var newAssignmentNumber = await GenerateAssignmentNumberAsync(companyId);

        // Create new assignment for the new employee
        var newAssignment = new AssetAssignment
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            AssetId = assignment.AssetId,
            AssignedToUserId = request.NewAssignedToUserId,
            AssignedByUserId = transferredByUserId,
            Action = AssetAssignmentAction.Assigned,
            Purpose = request.Purpose,
            AssignmentDate = request.TransferDate ?? DateTime.UtcNow,
            ExpectedReturnDate = request.ExpectedReturnDate,
            IsActive = true,
            ConditionAtAssignment = request.ConditionAtTransfer,
            ConditionNotesAtAssignment = request.ConditionNotes,
            Location = request.Location,
            DepartmentId = request.DepartmentId,
            CostCenter = request.CostCenter,
            AssignmentNumber = newAssignmentNumber,
            Notes = $"Transferred from previous assignment {assignment.AssignmentNumber}. {request.Notes}",
            IsAcknowledged = false
        };

        _context.AssetAssignments.Add(newAssignment);

        // Update asset
        var asset = assignment.Asset;
        asset.AssignedUserId = request.NewAssignedToUserId;
        asset.AssignmentPurpose = request.Purpose;
        asset.AssignmentDate = newAssignment.AssignmentDate;
        asset.ExpectedReturnDate = request.ExpectedReturnDate;
        asset.ActualReturnDate = null;
        asset.DepartmentId = request.DepartmentId;
        asset.CostCenter = request.CostCenter;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Asset {AssetTag} transferred from employee {OldEmployee} to {NewEmployee}. New assignment: {AssignmentNumber}",
            asset.AssetTag, assignment.AssignedToUserId, request.NewAssignedToUserId, newAssignmentNumber);

        return await GetByIdAsync(newAssignment.Id, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve assignment after transfer");
    }

    public async Task<AssetAssignmentDto> ExtendAssignmentAsync(
        Guid assignmentId,
        ExtendAssignmentRequest request,
        Guid companyId,
        Guid userId)
    {
        var assignment = await _context.AssetAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CompanyId == companyId);

        if (assignment == null)
            throw new InvalidOperationException("Assignment not found");

        if (!assignment.IsActive)
            throw new InvalidOperationException("Assignment is not active");

        var oldDate = assignment.ExpectedReturnDate;
        assignment.ExpectedReturnDate = request.NewExpectedReturnDate;
        assignment.Action = AssetAssignmentAction.Extended;

        var extensionNote = $"Extended from {oldDate?.ToString("yyyy-MM-dd") ?? "N/A"} to {request.NewExpectedReturnDate:yyyy-MM-dd}";
        if (!string.IsNullOrEmpty(request.Reason))
            extensionNote += $". Reason: {request.Reason}";

        assignment.Notes = string.IsNullOrEmpty(assignment.Notes)
            ? extensionNote
            : $"{assignment.Notes}\n\n{extensionNote}";

        // Update asset
        var asset = await _context.Assets.FindAsync(assignment.AssetId);
        if (asset != null)
            asset.ExpectedReturnDate = request.NewExpectedReturnDate;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Assignment {AssignmentNumber} extended to {NewDate}",
            assignment.AssignmentNumber, request.NewExpectedReturnDate);

        return await GetByIdAsync(assignment.Id, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve assignment after extension");
    }

    public async Task<AssetAssignmentDto> AcknowledgeAssetAsync(
        Guid assignmentId,
        AcknowledgeAssetRequest request,
        Guid companyId,
        Guid userId)
    {
        var assignment = await _context.AssetAssignments
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CompanyId == companyId);

        if (assignment == null)
            throw new InvalidOperationException("Assignment not found");

        // For internal assignments, only the assigned employee can acknowledge
        // For external assignments (customer/SI), the responsible user can acknowledge on behalf of customer
        var canAcknowledge = assignment.AssignedToUserId == userId  // Assigned employee
            || assignment.ResponsibleUserId == userId;              // Responsible user for external

        if (!canAcknowledge)
            throw new InvalidOperationException("Only the assigned employee or responsible user can acknowledge the asset");

        if (assignment.IsAcknowledged)
            throw new InvalidOperationException("Assignment already acknowledged");

        assignment.IsAcknowledged = true;
        assignment.AcknowledgedAt = DateTime.UtcNow;
        assignment.AcknowledgementReference = request.AcknowledgementReference;

        var acknowledgedBy = assignment.AssignedToCustomerId.HasValue
            ? "responsible user on behalf of customer"
            : "employee";

        if (!string.IsNullOrEmpty(request.Notes))
            assignment.Notes = string.IsNullOrEmpty(assignment.Notes)
                ? $"Acknowledgement Notes ({acknowledgedBy}): {request.Notes}"
                : $"{assignment.Notes}\n\nAcknowledgement Notes ({acknowledgedBy}): {request.Notes}";

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Assignment {AssignmentNumber} acknowledged by {AcknowledgedBy} {UserId}",
            assignment.AssignmentNumber, acknowledgedBy, userId);

        return await GetByIdAsync(assignment.Id, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve assignment after acknowledgement");
    }

    public async Task<AssetAssignmentDto> ReportIssueAsync(
        Guid assignmentId,
        ReportAssetIssueRequest request,
        Guid companyId,
        Guid userId)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && a.CompanyId == companyId);

        if (assignment == null)
            throw new InvalidOperationException("Assignment not found");

        if (!assignment.IsActive)
            throw new InvalidOperationException("Assignment is not active");

        // Validate issue type
        var validIssueTypes = new[]
        {
            AssetAssignmentAction.ReportedLost,
            AssetAssignmentAction.ReportedDamaged,
            AssetAssignmentAction.SentForRepair
        };

        if (!validIssueTypes.Contains(request.IssueType))
            throw new InvalidOperationException("Invalid issue type");

        assignment.Action = request.IssueType;
        assignment.ConditionAtReturn = request.CurrentCondition;

        var issueNote = $"Issue reported on {(request.IssueDate ?? DateTime.UtcNow):yyyy-MM-dd}: {request.Description}";
        assignment.Notes = string.IsNullOrEmpty(assignment.Notes)
            ? issueNote
            : $"{assignment.Notes}\n\n{issueNote}";

        // Update asset status based on issue type
        var asset = assignment.Asset;
        switch (request.IssueType)
        {
            case AssetAssignmentAction.ReportedLost:
                asset.Status = AssetStatus.Lost;
                asset.Condition = request.CurrentCondition;
                assignment.IsActive = false;
                assignment.ActualReturnDate = request.IssueDate ?? DateTime.UtcNow;
                break;

            case AssetAssignmentAction.ReportedDamaged:
                asset.Condition = request.CurrentCondition;
                asset.StatusReason = $"Damaged: {request.Description}";
                break;

            case AssetAssignmentAction.SentForRepair:
                asset.Status = AssetStatus.UnderRepair;
                asset.Condition = request.CurrentCondition;
                break;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Issue {IssueType} reported for asset {AssetTag} in assignment {AssignmentNumber}",
            request.IssueType, asset.AssetTag, assignment.AssignmentNumber);

        return await GetByIdAsync(assignment.Id, companyId)
            ?? throw new InvalidOperationException("Failed to retrieve assignment after reporting issue");
    }

    #endregion

    #region Query Operations

    public async Task<AssetAssignmentDto?> GetByIdAsync(Guid id, Guid companyId)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Company)
            .Where(a => a.Id == id && a.CompanyId == companyId)
            .FirstOrDefaultAsync();

        if (assignment == null)
            return null;

        return await MapToDto(assignment);
    }

    public async Task<AssetAssignmentDto?> GetByAssignmentNumberAsync(string assignmentNumber, Guid companyId)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Company)
            .Where(a => a.AssignmentNumber == assignmentNumber && a.CompanyId == companyId)
            .FirstOrDefaultAsync();

        if (assignment == null)
            return null;

        return await MapToDto(assignment);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetAssignmentsAsync(
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
        DateTime? toDate = null)
    {
        var query = _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.CompanyId == companyId);

        if (isActive.HasValue)
            query = query.Where(a => a.IsActive == isActive.Value);

        if (assetId.HasValue)
            query = query.Where(a => a.AssetId == assetId.Value);

        if (assignedToUserId.HasValue)
            query = query.Where(a => a.AssignedToUserId == assignedToUserId.Value);

        if (assignedToCustomerId.HasValue)
            query = query.Where(a => a.AssignedToCustomerId == assignedToCustomerId.Value);

        if (responsibleUserId.HasValue)
            query = query.Where(a => a.ResponsibleUserId == responsibleUserId.Value);

        if (isExternalAssignment.HasValue)
        {
            if (isExternalAssignment.Value)
                query = query.Where(a => a.AssignedToCustomerId != null);
            else
                query = query.Where(a => a.AssignedToCustomerId == null);
        }

        if (departmentId.HasValue)
            query = query.Where(a => a.DepartmentId == departmentId.Value);

        if (purpose.HasValue)
            query = query.Where(a => a.Purpose == purpose.Value);

        if (fromDate.HasValue)
            query = query.Where(a => a.AssignmentDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.AssignmentDate <= toDate.Value);

        var assignments = await query
            .OrderByDescending(a => a.AssignmentDate)
            .ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetAssetHistoryAsync(Guid assetId, Guid companyId)
    {
        var assignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.AssetId == assetId && a.CompanyId == companyId)
            .OrderByDescending(a => a.AssignmentDate)
            .ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetEmployeeActiveAssignmentsAsync(Guid userId, Guid companyId)
    {
        var assignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.AssignedToUserId == userId
                && a.CompanyId == companyId
                && a.IsActive)
            .OrderByDescending(a => a.AssignmentDate)
            .ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetEmployeeAssignmentHistoryAsync(
        Guid userId,
        Guid companyId,
        int? limit = null)
    {
        var query = _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.AssignedToUserId == userId && a.CompanyId == companyId)
            .OrderByDescending(a => a.AssignmentDate);

        var assignments = limit.HasValue
            ? await query.Take(limit.Value).ToListAsync()
            : await query.ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetOverdueAssignmentsAsync(Guid companyId)
    {
        var now = DateTime.UtcNow;
        var assignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.CompanyId == companyId
                && a.IsActive
                && a.ExpectedReturnDate != null
                && a.ExpectedReturnDate < now)
            .OrderBy(a => a.ExpectedReturnDate)
            .ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetPendingAcknowledgementsAsync(Guid companyId, Guid? userId = null)
    {
        var query = _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.CompanyId == companyId
                && a.IsActive
                && !a.IsAcknowledged);

        if (userId.HasValue)
            query = query.Where(a => a.AssignedToUserId == userId.Value);

        var assignments = await query
            .OrderByDescending(a => a.AssignmentDate)
            .ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    public async Task<List<AssetAssignmentSummaryDto>> GetUpcomingReturnsAsync(Guid companyId, int days = 7)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(days);

        var assignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.CompanyId == companyId
                && a.IsActive
                && a.ExpectedReturnDate != null
                && a.ExpectedReturnDate >= now
                && a.ExpectedReturnDate <= endDate)
            .OrderBy(a => a.ExpectedReturnDate)
            .ToListAsync();

        return await MapToSummaryDtos(assignments);
    }

    #endregion

    #region Dashboard Operations

    public async Task<EmployeeAssetDashboardDto> GetEmployeeDashboardAsync(Guid userId, Guid companyId)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == userId && u.CompanyId == companyId);

        if (user == null)
            throw new InvalidOperationException("User not found");

        var now = DateTime.UtcNow;
        var sevenDaysFromNow = now.AddDays(7);

        // Get all direct assignments for employee (internal assignments)
        var allAssignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.AssignedToUserId == userId && a.CompanyId == companyId)
            .ToListAsync();

        // Get all assignments where user is responsible for tracking (external assignments)
        var responsibleForAssignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.ResponsibleUserId == userId && a.CompanyId == companyId && a.IsActive)
            .ToListAsync();

        var activeAssignments = allAssignments.Where(a => a.IsActive).ToList();
        var pendingAck = activeAssignments.Where(a => !a.IsAcknowledged).ToList();

        // Overdue: include both direct and responsible-for assignments
        var overdue = activeAssignments
            .Where(a => a.ExpectedReturnDate.HasValue && a.ExpectedReturnDate < now)
            .ToList();
        var responsibleOverdue = responsibleForAssignments
            .Where(a => a.ExpectedReturnDate.HasValue && a.ExpectedReturnDate < now)
            .ToList();
        var allOverdue = overdue.Concat(responsibleOverdue).DistinctBy(a => a.Id).ToList();

        // Upcoming: include both direct and responsible-for assignments
        var upcoming = activeAssignments
            .Where(a => a.ExpectedReturnDate.HasValue
                && a.ExpectedReturnDate >= now
                && a.ExpectedReturnDate <= sevenDaysFromNow)
            .ToList();
        var responsibleUpcoming = responsibleForAssignments
            .Where(a => a.ExpectedReturnDate.HasValue
                && a.ExpectedReturnDate >= now
                && a.ExpectedReturnDate <= sevenDaysFromNow)
            .ToList();
        var allUpcoming = upcoming.Concat(responsibleUpcoming).DistinctBy(a => a.Id).ToList();

        var recent = allAssignments
            .Where(a => !a.IsActive)
            .OrderByDescending(a => a.ActualReturnDate ?? a.AssignmentDate)
            .Take(10)
            .ToList();

        return new EmployeeAssetDashboardDto
        {
            UserId = userId,
            UserName = user.FullName,
            EmployeeCode = user.EmployeeCode,
            CurrentAssignments = await MapToSummaryDtos(activeAssignments),
            ResponsibleForAssignments = await MapToSummaryDtos(responsibleForAssignments),
            PendingAcknowledgement = await MapToSummaryDtos(pendingAck),
            OverdueReturns = await MapToSummaryDtos(allOverdue),
            UpcomingReturns = await MapToSummaryDtos(allUpcoming),
            RecentHistory = await MapToSummaryDtos(recent),
            Statistics = new EmployeeAssignmentStatisticsDto
            {
                TotalActiveAssignments = activeAssignments.Count,
                PermanentAssignments = activeAssignments.Count(a => a.Purpose == AssetAssignmentPurpose.Permanent),
                TemporaryAssignments = activeAssignments.Count(a => a.Purpose != AssetAssignmentPurpose.Permanent),
                OverdueCount = overdue.Count,
                PendingAcknowledgementCount = pendingAck.Count,
                ResponsibleForCount = responsibleForAssignments.Count,
                ResponsibleForOverdueCount = responsibleOverdue.Count
            }
        };
    }

    public async Task<AssetAssignmentStatisticsDto> GetStatisticsAsync(Guid companyId)
    {
        var now = DateTime.UtcNow;

        var allAssignments = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Where(a => a.CompanyId == companyId)
            .ToListAsync();

        var activeAssignments = allAssignments.Where(a => a.IsActive).ToList();

        var stats = new AssetAssignmentStatisticsDto
        {
            TotalAssignments = allAssignments.Count,
            ActiveAssignments = activeAssignments.Count,
            ReturnedAssignments = allAssignments.Count(a => !a.IsActive),
            OverdueAssignments = activeAssignments
                .Count(a => a.ExpectedReturnDate.HasValue && a.ExpectedReturnDate < now),
            PendingAcknowledgements = activeAssignments.Count(a => !a.IsAcknowledged),
            ByPurpose = activeAssignments
                .GroupBy(a => a.Purpose.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            ByAssetType = activeAssignments
                .GroupBy(a => a.Asset.Type.ToString())
                .ToDictionary(g => g.Key, g => g.Count())
        };

        // Get department names
        var departmentIds = activeAssignments
            .Where(a => a.DepartmentId.HasValue)
            .Select(a => a.DepartmentId!.Value)
            .Distinct()
            .ToList();

        var departments = await _context.Departments
            .Where(d => departmentIds.Contains(d.Id))
            .ToDictionaryAsync(d => d.Id, d => d.Name);

        stats.ByDepartment = activeAssignments
            .Where(a => a.DepartmentId.HasValue)
            .GroupBy(a => departments.GetValueOrDefault(a.DepartmentId!.Value, "Unknown"))
            .ToDictionary(g => g.Key, g => g.Count());

        // Top assignees (internal employee assignments only)
        var topAssignees = activeAssignments
            .Where(a => a.AssignedToUserId.HasValue)
            .GroupBy(a => a.AssignedToUserId!.Value)
            .OrderByDescending(g => g.Count())
            .Take(10)
            .ToList();

        var userIds = topAssignees.Select(g => g.Key).ToList();
        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);

        stats.TopAssignees = topAssignees.Select(g => new EmployeeAssignmentCountDto
        {
            UserId = g.Key,
            UserName = users.GetValueOrDefault(g.Key)?.FullName ?? "Unknown",
            EmployeeCode = users.GetValueOrDefault(g.Key)?.EmployeeCode,
            AssignmentCount = g.Count()
        }).ToList();

        return stats;
    }

    #endregion

    #region Validation

    public async Task<bool> IsAssetAvailableForAssignmentAsync(Guid assetId, Guid companyId)
    {
        var hasActiveAssignment = await _context.AssetAssignments
            .AnyAsync(a => a.AssetId == assetId
                && a.CompanyId == companyId
                && a.IsActive);

        return !hasActiveAssignment;
    }

    public async Task<AssetAssignmentDto?> GetCurrentAssignmentAsync(Guid assetId, Guid companyId)
    {
        var assignment = await _context.AssetAssignments
            .Include(a => a.Asset)
            .Include(a => a.Company)
            .Where(a => a.AssetId == assetId
                && a.CompanyId == companyId
                && a.IsActive)
            .FirstOrDefaultAsync();

        if (assignment == null)
            return null;

        return await MapToDto(assignment);
    }

    #endregion

    #region Private Helpers

    private async Task<string> GenerateAssignmentNumberAsync(Guid companyId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"ASN-{year}-";

        var lastNumber = await _context.AssetAssignments
            .Where(a => a.CompanyId == companyId && a.AssignmentNumber != null && a.AssignmentNumber.StartsWith(prefix))
            .OrderByDescending(a => a.AssignmentNumber)
            .Select(a => a.AssignmentNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastNumber != null)
        {
            var numPart = lastNumber.Substring(prefix.Length);
            if (int.TryParse(numPart, out int lastNum))
                nextNumber = lastNum + 1;
        }

        return $"{prefix}{nextNumber:D5}";
    }

    private async Task<AssetAssignmentDto> MapToDto(AssetAssignment assignment)
    {
        var asset = assignment.Asset;

        // Get user details (for internal assignments)
        var assignedToUser = assignment.AssignedToUserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == assignment.AssignedToUserId)
            : null;

        // Get customer details (for external assignments)
        var assignedToCustomer = assignment.AssignedToCustomerId.HasValue
            ? await _context.Customers.FirstOrDefaultAsync(c => c.Id == assignment.AssignedToCustomerId)
            : null;

        // Get responsible user details (for external assignments)
        var responsibleUser = assignment.ResponsibleUserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == assignment.ResponsibleUserId)
            : null;

        var assignedByUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == assignment.AssignedByUserId);

        var returnedToUser = assignment.ReturnedToUserId.HasValue
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == assignment.ReturnedToUserId.Value)
            : null;

        var department = assignment.DepartmentId.HasValue
            ? await _context.Departments.FirstOrDefaultAsync(d => d.Id == assignment.DepartmentId.Value)
            : null;

        return new AssetAssignmentDto
        {
            Id = assignment.Id,
            CompanyId = assignment.CompanyId,
            AssetId = assignment.AssetId,
            AssetTag = asset.AssetTag,
            AssetName = asset.Name,
            AssetSerialNumber = asset.SerialNumber,
            AssetType = asset.Type,
            // Internal assignment
            AssignedToUserId = assignment.AssignedToUserId,
            AssignedToUserName = assignedToUser?.FullName,
            AssignedToUserEmail = assignedToUser?.Email,
            AssignedToEmployeeCode = assignedToUser?.EmployeeCode,
            // External assignment (customer/SI)
            AssignedToCustomerId = assignment.AssignedToCustomerId,
            AssignedToCustomerName = assignedToCustomer?.Name,
            AssignedToCustomerCode = assignedToCustomer?.Code,
            // Responsible user for external assignment
            ResponsibleUserId = assignment.ResponsibleUserId,
            ResponsibleUserName = responsibleUser?.FullName,
            ResponsibleUserEmail = responsibleUser?.Email,
            AssignedByUserId = assignment.AssignedByUserId,
            AssignedByUserName = assignedByUser?.FullName ?? "Unknown",
            Action = assignment.Action,
            Purpose = assignment.Purpose,
            AssignmentDate = assignment.AssignmentDate,
            ExpectedReturnDate = assignment.ExpectedReturnDate,
            ActualReturnDate = assignment.ActualReturnDate,
            ReturnedToUserId = assignment.ReturnedToUserId,
            ReturnedToUserName = returnedToUser?.FullName,
            IsActive = assignment.IsActive,
            ConditionAtAssignment = assignment.ConditionAtAssignment,
            ConditionAtReturn = assignment.ConditionAtReturn,
            ConditionNotesAtAssignment = assignment.ConditionNotesAtAssignment,
            ConditionNotesAtReturn = assignment.ConditionNotesAtReturn,
            Location = assignment.Location,
            DepartmentId = assignment.DepartmentId,
            DepartmentName = department?.Name,
            CostCenter = assignment.CostCenter,
            AssignmentNumber = assignment.AssignmentNumber,
            Notes = assignment.Notes,
            IsAcknowledged = assignment.IsAcknowledged,
            AcknowledgedAt = assignment.AcknowledgedAt,
            AcknowledgementReference = assignment.AcknowledgementReference,
            CreatedAt = assignment.CreatedAt,
            UpdatedAt = assignment.UpdatedAt
        };
    }

    private async Task<List<AssetAssignmentSummaryDto>> MapToSummaryDtos(List<AssetAssignment> assignments)
    {
        if (!assignments.Any())
            return new List<AssetAssignmentSummaryDto>();

        // Get all user IDs (assigned to, responsible)
        var userIds = assignments
            .SelectMany(a => new[] { a.AssignedToUserId, a.ResponsibleUserId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var users = await _context.Users
            .Where(u => userIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u);

        // Get customer IDs
        var customerIds = assignments
            .Where(a => a.AssignedToCustomerId.HasValue)
            .Select(a => a.AssignedToCustomerId!.Value)
            .Distinct()
            .ToList();

        var customers = customerIds.Any()
            ? await _context.Customers
                .Where(c => customerIds.Contains(c.Id))
                .ToDictionaryAsync(c => c.Id, c => c)
            : new Dictionary<Guid, Domain.Entities.CRM.Customer>();

        return assignments.Select(a => new AssetAssignmentSummaryDto
        {
            Id = a.Id,
            AssetId = a.AssetId,
            AssetTag = a.Asset.AssetTag,
            AssetName = a.Asset.Name,
            AssetSerialNumber = a.Asset.SerialNumber,
            AssetType = a.Asset.Type,
            // Internal assignment
            AssignedToUserId = a.AssignedToUserId,
            AssignedToUserName = a.AssignedToUserId.HasValue
                ? users.GetValueOrDefault(a.AssignedToUserId.Value)?.FullName
                : null,
            AssignedToEmployeeCode = a.AssignedToUserId.HasValue
                ? users.GetValueOrDefault(a.AssignedToUserId.Value)?.EmployeeCode
                : null,
            // Customer/SI assignment
            AssignedToCustomerId = a.AssignedToCustomerId,
            AssignedToCustomerName = a.AssignedToCustomerId.HasValue
                ? customers.GetValueOrDefault(a.AssignedToCustomerId.Value)?.Name
                : null,
            AssignedToCustomerCode = a.AssignedToCustomerId.HasValue
                ? customers.GetValueOrDefault(a.AssignedToCustomerId.Value)?.Code
                : null,
            // Responsible user for external assignments
            ResponsibleUserId = a.ResponsibleUserId,
            ResponsibleUserName = a.ResponsibleUserId.HasValue
                ? users.GetValueOrDefault(a.ResponsibleUserId.Value)?.FullName
                : null,
            Action = a.Action,
            Purpose = a.Purpose,
            AssignmentDate = a.AssignmentDate,
            ExpectedReturnDate = a.ExpectedReturnDate,
            ActualReturnDate = a.ActualReturnDate,
            IsActive = a.IsActive,
            ConditionAtAssignment = a.ConditionAtAssignment,
            IsAcknowledged = a.IsAcknowledged,
            AssignmentNumber = a.AssignmentNumber
        }).ToList();
    }

    #endregion
}
