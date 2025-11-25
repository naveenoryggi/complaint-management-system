using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Assignment;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Simple assignment engine for basic functionality until advanced engine is fully implemented
/// </summary>
public class SimpleAssignmentEngine : IAssignmentEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<SimpleAssignmentEngine> _logger;

    public SimpleAssignmentEngine(
        IUnitOfWork unitOfWork,
        ILogger<SimpleAssignmentEngine> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<AssignmentResult> AutoAssignComplaintAsync(Guid complaintId, AssignmentContext context, CancellationToken cancellationToken = default)
    {
        // Simplified auto-assignment - assign to first available admin user
        try
        {
            _logger.LogInformation("Starting simple auto-assignment for complaint {ComplaintId}", complaintId);

            // Get complaint details
            var complaint = await _unitOfWork.Complaints.GetByIdWithIncludesAsync(complaintId, cancellationToken,
                c => c.Category, c => c.Company, c => c.Branch, c => c.Department, c => c.Section);

            if (complaint == null)
            {
                return AssignmentResult.Failure("Complaint not found", "COMPLAINT_NOT_FOUND");
            }

            // Get first available admin user
            var users = await _unitOfWork.Users.FindAsync(u => u.IsActive, cancellationToken);
            var firstUser = users.FirstOrDefault();

            if (firstUser == null)
            {
                return AssignmentResult.Failure("No available users found", "NO_USERS_AVAILABLE");
            }

            // Get "In Progress" status master
            var inProgressStatus = await _unitOfWork.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Name.Equals("In Progress", StringComparison.OrdinalIgnoreCase) && s.CompanyId == complaint.CompanyId, cancellationToken);

            if (inProgressStatus != null)
            {
                complaint.StatusMasterId = inProgressStatus.Id;
            }

            // Perform simple assignment
            complaint.AssignedToId = firstUser.Id;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return AssignmentResult.CreateSuccess(
                firstUser.Id,
                firstUser.FullName,
                "Simple Auto-Assignment",
                "Complaint assigned to first available user"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during simple auto-assignment for complaint {ComplaintId}", complaintId);
            return AssignmentResult.Failure($"Assignment failed: {ex.Message}", "ASSIGNMENT_ERROR");
        }
    }

    public async Task<AssignmentResult> AssignComplaintToPoolAsync(
        Guid complaintId,
        Guid resourcePoolId,
        AdvancedAssignmentMethod assignmentMethod,
        Guid? specificUserId = null,
        AssignmentContext? context = null,
        CancellationToken cancellationToken = default)
    {
        // Simplified pool assignment - use direct user assignment
        try
        {
            _logger.LogInformation("Assigning complaint {ComplaintId} to pool {PoolId}", complaintId, resourcePoolId);

            if (specificUserId.HasValue)
            {
                return await DirectAssignment(complaintId, specificUserId.Value, context, cancellationToken);
            }

            // For now, skip pool assignment and use simple user assignment
            // TODO: Implement ResourcePoolMembers repository when ready
            var users = await _unitOfWork.Users.FindAsync(u => u.IsActive, cancellationToken);
            var firstUser = users.FirstOrDefault();

            if (firstUser == null)
            {
                return AssignmentResult.Failure("No available users found", "NO_USERS_AVAILABLE");
            }

            return await DirectAssignment(complaintId, firstUser.Id, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during pool assignment for complaint {ComplaintId}", complaintId);
            return AssignmentResult.Failure($"Pool assignment failed: {ex.Message}", "POOL_ASSIGNMENT_ERROR");
        }
    }

    public async Task<List<ResourcePoolCandidate>> FindCandidatePoolsAsync(AssignmentCriteria criteria, CancellationToken cancellationToken = default)
    {
        // Return empty list for now
        return await Task.FromResult(new List<ResourcePoolCandidate>());
    }

    public async Task<UserCandidate?> SelectUserFromPoolAsync(
        Guid poolId,
        AdvancedAssignmentMethod assignmentMethod,
        AssignmentContext context,
        CancellationToken cancellationToken = default)
    {
        // Return null for now
        return await Task.FromResult<UserCandidate?>(null);
    }

    public async Task<AssignmentValidationResult> ValidateAssignmentAsync(
        Guid complaintId,
        Guid? userId = null,
        Guid? poolId = null,
        AssignmentContext? context = null,
        CancellationToken cancellationToken = default)
    {
        // Return positive validation for now
        return await Task.FromResult(new AssignmentValidationResult
        {
            IsValid = true,
            SuitabilityScore = 0.8,
            ValidationErrors = new List<string>(),
            ValidationWarnings = new List<string>(),
            Recommendations = new List<string> { "Assignment is valid" }
        });
    }

    public async Task<List<ResourcePoolCandidate>> GetAssignmentCandidatesAsync(
        Guid complaintId,
        AssignmentCriteria? criteria = null,
        bool includeUserDetails = true,
        bool calculateSkillScores = true,
        CancellationToken cancellationToken = default)
    {
        // Return empty list for now
        return await Task.FromResult(new List<ResourcePoolCandidate>());
    }

    public async Task<AssignmentResult> ExecuteAssignmentRulesAsync(Guid complaintId, AssignmentContext context, CancellationToken cancellationToken)
    {
        // Return failure to trigger fallback logic
        return await Task.FromResult(AssignmentResult.Failure("No assignment rules configured", "NO_RULES_FOUND"));
    }

    public async Task<double> CalculateUserSuitabilityScoreAsync(
        Guid userId,
        Guid poolId,
        AssignmentCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(0.8);
    }

    public async Task UpdateWorkloadStatisticsAsync(
        Guid userId,
        Guid poolId,
        Guid complaintId,
        CancellationToken cancellationToken = default)
    {
        // Placeholder for workload updates
        await Task.CompletedTask;
    }

    private async Task<AssignmentResult> DirectAssignment(
        Guid complaintId,
        Guid userId,
        AssignmentContext? context,
        CancellationToken cancellationToken)
    {
        try
        {
            var complaint = await _unitOfWork.Complaints.GetByIdAsync(complaintId, cancellationToken);
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

            if (complaint == null)
            {
                return AssignmentResult.Failure("Complaint not found", "COMPLAINT_NOT_FOUND");
            }

            if (user == null)
            {
                return AssignmentResult.Failure("User not found", "USER_NOT_FOUND");
            }

            // Get "In Progress" status master
            var inProgressStatus = await _unitOfWork.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Name.Equals("In Progress", StringComparison.OrdinalIgnoreCase) && s.CompanyId == complaint.CompanyId, cancellationToken);

            if (inProgressStatus != null)
            {
                complaint.StatusMasterId = inProgressStatus.Id;
            }

            complaint.AssignedToId = userId;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return AssignmentResult.CreateSuccess(
                userId,
                user.FullName,
                "Direct Assignment",
                "Complaint assigned successfully"
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during direct assignment for complaint {ComplaintId}", complaintId);
            return AssignmentResult.Failure($"Direct assignment failed: {ex.Message}", "DIRECT_ASSIGNMENT_ERROR");
        }
    }
}