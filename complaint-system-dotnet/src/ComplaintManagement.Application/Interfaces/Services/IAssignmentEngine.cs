using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Core assignment engine for intelligent complaint routing and resource allocation
/// </summary>
public interface IAssignmentEngine
{
    /// <summary>
    /// Automatically assign a complaint using the configured assignment rules and logic
    /// </summary>
    /// <param name="complaintId">The complaint to assign</param>
    /// <param name="context">Assignment context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result with details of the operation</returns>
    Task<AssignmentResult> AutoAssignComplaintAsync(Guid complaintId, AssignmentContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign a complaint to a specific resource pool using intelligent user selection
    /// </summary>
    /// <param name="complaintId">The complaint to assign</param>
    /// <param name="resourcePoolId">The resource pool to assign from</param>
    /// <param name="assignmentMethod">The assignment method to use</param>
    /// <param name="specificUserId">Specific user to assign (for Manual method)</param>
    /// <param name="context">Assignment context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result with details of the operation</returns>
    Task<AssignmentResult> AssignComplaintToPoolAsync(
        Guid complaintId,
        Guid resourcePoolId,
        AdvancedAssignmentMethod assignmentMethod,
        Guid? specificUserId = null,
        AssignmentContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Find all candidate resource pools for a complaint based on the given criteria
    /// </summary>
    /// <param name="criteria">Assignment criteria to filter pools</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Ordered list of suitable resource pool candidates</returns>
    Task<List<ResourcePoolCandidate>> FindCandidatePoolsAsync(AssignmentCriteria criteria, CancellationToken cancellationToken = default);

    /// <summary>
    /// Select the best user from a resource pool based on the assignment method and context
    /// </summary>
    /// <param name="poolId">The resource pool to select from</param>
    /// <param name="assignmentMethod">The assignment method to use</param>
    /// <param name="context">Assignment context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The selected user candidate</returns>
    Task<UserCandidate?> SelectUserFromPoolAsync(
        Guid poolId,
        AdvancedAssignmentMethod assignmentMethod,
        AssignmentContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate whether an assignment would be valid and appropriate
    /// </summary>
    /// <param name="complaintId">The complaint to validate</param>
    /// <param name="userId">The target user (optional)</param>
    /// <param name="poolId">The target resource pool (optional)</param>
    /// <param name="context">Assignment context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with detailed information</returns>
    Task<AssignmentValidationResult> ValidateAssignmentAsync(
        Guid complaintId,
        Guid? userId = null,
        Guid? poolId = null,
        AssignmentContext? context = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get assignment candidates for a complaint with detailed scoring and analysis
    /// </summary>
    /// <param name="complaintId">The complaint to find candidates for</param>
    /// <param name="criteria">Assignment criteria to filter candidates</param>
    /// <param name="includeUserDetails">Whether to include detailed user information</param>
    /// <param name="calculateSkillScores">Whether to calculate skill match scores</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of candidate resource pools with detailed analysis</returns>
    Task<List<ResourcePoolCandidate>> GetAssignmentCandidatesAsync(
        Guid complaintId,
        AssignmentCriteria? criteria = null,
        bool includeUserDetails = true,
        bool calculateSkillScores = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute assignment rules for a complaint and return the best match
    /// </summary>
    /// <param name="complaintId">The complaint to process</param>
    /// <param name="context">Assignment context information</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Assignment result based on rule execution</returns>
    Task<AssignmentResult> ExecuteAssignmentRulesAsync(Guid complaintId, AssignmentContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate a suitability score for a user-pool match
    /// </summary>
    /// <param name="userId">The user to evaluate</param>
    /// <param name="poolId">The resource pool</param>
    /// <param name="criteria">Assignment criteria</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Suitability score (0.0 to 1.0)</returns>
    Task<double> CalculateUserSuitabilityScoreAsync(
        Guid userId,
        Guid poolId,
        AssignmentCriteria criteria,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update workload statistics after assignment completion
    /// </summary>
    /// <param name="userId">The user who was assigned</param>
    /// <param name="poolId">The resource pool</param>
    /// <param name="complaintId">The complaint that was assigned</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the update operation</returns>
    Task UpdateWorkloadStatisticsAsync(
        Guid userId,
        Guid poolId,
        Guid complaintId,
        CancellationToken cancellationToken = default);
}