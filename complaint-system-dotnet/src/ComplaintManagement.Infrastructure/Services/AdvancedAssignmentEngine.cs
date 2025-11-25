using ComplaintManagement.Application.DTOs.Assignment;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Assignment;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.Json;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Advanced assignment engine with intelligent complaint routing and resource allocation
/// Implements sophisticated algorithms for optimal user selection
/// </summary>
public class AdvancedAssignmentEngine : IAssignmentEngine
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ComplaintDbContext _context;
    private readonly ILogger<AdvancedAssignmentEngine> _logger;
    private const int DEFAULT_MAX_WORKLOAD = 10;
    private const double SKILL_MATCH_THRESHOLD = 0.6;
    private const double WORKLOAD_BALANCE_WEIGHT = 0.3;
    private const double SKILL_WEIGHT = 0.4;
    private const double PERFORMANCE_WEIGHT = 0.3;

    public AdvancedAssignmentEngine(
        IUnitOfWork unitOfWork,
        ComplaintDbContext context,
        ILogger<AdvancedAssignmentEngine> logger)
    {
        _unitOfWork = unitOfWork;
        _context = context;
        _logger = logger;
    }

    #region Main Assignment Methods

    public async Task<AssignmentResult> AutoAssignComplaintAsync(
        Guid complaintId,
        AssignmentContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation("Starting auto-assignment for complaint {ComplaintId}", complaintId);

            // Step 1: Load complaint with all related entities
            var complaint = await LoadComplaintWithDetails(complaintId, cancellationToken);
            if (complaint == null)
            {
                return AssignmentResult.Failure("Complaint not found", "COMPLAINT_NOT_FOUND");
            }

            // Step 2: Try to execute assignment rules first
            if (!context.BypassRules)
            {
                var ruleResult = await ExecuteAssignmentRulesAsync(complaintId, context, cancellationToken);
                if (ruleResult.Success)
                {
                    stopwatch.Stop();
                    ruleResult.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
                    return ruleResult;
                }
                _logger.LogInformation("No matching assignment rules found, falling back to pool-based assignment");
            }

            // Step 3: Build assignment criteria from complaint
            var criteria = BuildCriteriaFromComplaint(complaint);

            // Step 4: Find candidate resource pools
            var candidatePools = await FindCandidatePoolsAsync(criteria, cancellationToken);
            if (candidatePools.Count == 0)
            {
                _logger.LogWarning("No suitable resource pools found for complaint {ComplaintId}", complaintId);
                return AssignmentResult.Failure("No suitable resource pools found", "NO_POOLS_AVAILABLE");
            }

            // Step 5: Get the best pool
            var bestPool = candidatePools.OrderByDescending(p => p.SuitabilityScore).First();

            // Step 6: Assign to the best pool using appropriate method
            var assignmentMethod = DetermineOptimalAssignmentMethod(bestPool);
            var result = await AssignComplaintToPoolAsync(
                complaintId,
                bestPool.ResourcePoolId,
                assignmentMethod,
                null,
                context,
                cancellationToken);

            stopwatch.Stop();
            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.ConsideredCandidates = candidatePools;

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-assignment for complaint {ComplaintId}", complaintId);
            return AssignmentResult.Failure($"Auto-assignment failed: {ex.Message}", "AUTO_ASSIGNMENT_ERROR");
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
        var stopwatch = Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
                "Assigning complaint {ComplaintId} to pool {PoolId} using method {Method}",
                complaintId, resourcePoolId, assignmentMethod);

            context ??= new AssignmentContext();

            // Step 1: Validate complaint and pool
            var complaint = await LoadComplaintWithDetails(complaintId, cancellationToken);
            if (complaint == null)
            {
                return AssignmentResult.Failure("Complaint not found", "COMPLAINT_NOT_FOUND");
            }

            var pool = await _context.Set<ResourcePool>()
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == resourcePoolId, cancellationToken);
            if (pool == null)
            {
                return AssignmentResult.Failure("Resource pool not found", "POOL_NOT_FOUND");
            }

            if (!pool.IsActive)
            {
                return AssignmentResult.Failure("Resource pool is not active", "POOL_INACTIVE");
            }

            // Step 2: Handle specific user assignment
            if (specificUserId.HasValue)
            {
                return await AssignToSpecificUser(complaint, specificUserId.Value, pool, context, cancellationToken);
            }

            // Step 3: Select best user from pool using assignment method
            var assignmentContext = BuildContextFromComplaint(complaint, context);
            var selectedUser = await SelectUserFromPoolAsync(
                resourcePoolId,
                assignmentMethod,
                assignmentContext,
                cancellationToken);

            if (selectedUser == null || !selectedUser.IsAvailable)
            {
                _logger.LogWarning("No available user found in pool {PoolId} for complaint {ComplaintId}",
                    resourcePoolId, complaintId);
                return AssignmentResult.Failure("No available users in pool", "NO_AVAILABLE_USERS");
            }

            // Step 4: Perform the assignment
            complaint.AssignedToId = selectedUser.UserId;

            // Set status dynamically from StatusMaster (user-configurable)
            var assignedStatus = await GetStatusByCodeAsync("IN_PROGRESS", complaint.CompanyId, cancellationToken);
            if (assignedStatus != null)
            {
                complaint.StatusMasterId = assignedStatus.Id;
            }
            else
            {
                _logger.LogWarning("StatusMaster with code IN_PROGRESS not found for company {CompanyId}", complaint.CompanyId);
            }

            complaint.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Step 5: Update workload statistics
            await UpdateWorkloadStatisticsAsync(selectedUser.UserId, resourcePoolId, complaintId, cancellationToken);

            stopwatch.Stop();

            var result = AssignmentResult.CreateSuccess(
                selectedUser.UserId,
                selectedUser.FullName,
                assignmentMethod.ToString(),
                $"Complaint assigned successfully using {assignmentMethod} method",
                resourcePoolId,
                pool.Name);

            result.ExecutionTimeMs = stopwatch.ElapsedMilliseconds;
            result.Metadata = new Dictionary<string, object>
            {
                ["SkillMatchScore"] = selectedUser.SkillMatchScore,
                ["UserWorkload"] = selectedUser.CurrentWorkload,
                ["UserSuccessRate"] = selectedUser.SuccessRate,
                ["OverallScore"] = selectedUser.OverallScore
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning complaint {ComplaintId} to pool {PoolId}",
                complaintId, resourcePoolId);
            return AssignmentResult.Failure($"Pool assignment failed: {ex.Message}", "POOL_ASSIGNMENT_ERROR");
        }
    }

    #endregion

    #region Find Candidate Pools

    public async Task<List<ResourcePoolCandidate>> FindCandidatePoolsAsync(
        AssignmentCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Finding candidate resource pools with criteria");

            // Get all active resource pools with members
            var query = _context.Set<ResourcePool>()
                .Where(p => p.IsActive && !p.IsDeleted)
                .Include(p => p.Members)
                .ThenInclude(m => m.User)
                .Include(p => p.Company)
                .Include(p => p.Branch)
                .Include(p => p.Department)
                .Include(p => p.Section)
                .AsQueryable();

            // Filter by organizational scope
            if (criteria.OrganizationalScope.CompanyIds.Any())
            {
                query = query.Where(p => criteria.OrganizationalScope.CompanyIds.Contains(p.CompanyId));
            }

            if (criteria.OrganizationalScope.BranchIds.Any())
            {
                query = query.Where(p => p.BranchId.HasValue &&
                                        criteria.OrganizationalScope.BranchIds.Contains(p.BranchId.Value));
            }

            if (criteria.OrganizationalScope.DepartmentIds.Any())
            {
                query = query.Where(p => p.DepartmentId.HasValue &&
                                        criteria.OrganizationalScope.DepartmentIds.Contains(p.DepartmentId.Value));
            }

            if (criteria.OrganizationalScope.SectionIds.Any())
            {
                query = query.Where(p => p.SectionId.HasValue &&
                                        criteria.OrganizationalScope.SectionIds.Contains(p.SectionId.Value));
            }

            var pools = await query.ToListAsync(cancellationToken);

            // Build candidate list with scoring
            var candidates = new List<ResourcePoolCandidate>();

            foreach (var pool in pools)
            {
                var candidate = await BuildPoolCandidate(pool, criteria, cancellationToken);
                if (candidate.CanHandleComplaint)
                {
                    candidates.Add(candidate);
                }
            }

            // Sort by suitability score and limit results
            return candidates
                .OrderByDescending(c => c.SuitabilityScore)
                .Take(criteria.MaxCandidates)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding candidate pools");
            return new List<ResourcePoolCandidate>();
        }
    }

    #endregion

    #region User Selection Algorithms

    public async Task<UserCandidate?> SelectUserFromPoolAsync(
        Guid poolId,
        AdvancedAssignmentMethod assignmentMethod,
        AssignmentContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Selecting user from pool {PoolId} using method {Method}", poolId, assignmentMethod);

            // Get pool with members
            var pool = await _context.Set<ResourcePool>()
                .Include(p => p.Members)
                .FirstOrDefaultAsync(p => p.Id == poolId, cancellationToken);

            if (pool == null || !pool.IsActive || pool.Members.Count == 0)
            {
                return null;
            }

            // Get active users from pool
            var activeUserIds = pool.Members
                .Where(m => !m.IsDeleted)
                .Select(m => m.UserId)
                .ToList();

            if (!activeUserIds.Any())
            {
                return null;
            }

            // Get user details with workload information
            var users = await _unitOfWork.Users
                .FindAsync(u => activeUserIds.Contains(u.Id) && u.IsActive && !u.IsDeleted,
                    cancellationToken);

            if (!users.Any())
            {
                return null;
            }

            // Build user candidates with scoring
            var userCandidates = await BuildUserCandidates(users.ToList(), context, cancellationToken);

            // Filter out unavailable users
            userCandidates = userCandidates.Where(u => u.IsAvailable).ToList();

            if (!userCandidates.Any())
            {
                return null;
            }

            // Select user based on assignment method
            return assignmentMethod switch
            {
                AdvancedAssignmentMethod.Manual => null, // Manual assignment handled separately
                AdvancedAssignmentMethod.RoundRobin => await SelectRoundRobin(userCandidates, poolId, cancellationToken),
                AdvancedAssignmentMethod.LeastBusy => SelectLeastBusy(userCandidates),
                AdvancedAssignmentMethod.SkillBased => SelectSkillBased(userCandidates),
                AdvancedAssignmentMethod.PriorityBased => SelectPriorityBased(userCandidates, context),
                AdvancedAssignmentMethod.WorkloadBalanced => SelectWorkloadBalanced(userCandidates),
                AdvancedAssignmentMethod.BestFit => SelectBestFit(userCandidates),
                AdvancedAssignmentMethod.ExperienceBased => SelectExperienceBased(userCandidates),
                AdvancedAssignmentMethod.Random => SelectRandom(userCandidates),
                _ => SelectLeastBusy(userCandidates) // Default to least busy
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting user from pool {PoolId}", poolId);
            return null;
        }
    }

    /// <summary>
    /// Round-robin assignment - distribute evenly among members
    /// </summary>
    private async Task<UserCandidate> SelectRoundRobin(
        List<UserCandidate> candidates,
        Guid poolId,
        CancellationToken cancellationToken)
    {
        // Find user who was assigned least recently
        var userWithWorkload = candidates.Select(c => new
        {
            Candidate = c,
            LastAssignedAt = c.LastActivityAt ?? DateTime.MinValue
        }).OrderBy(x => x.LastAssignedAt).ToList();

        return userWithWorkload.First().Candidate;
    }

    /// <summary>
    /// Least busy assignment - assign to user with fewest active complaints
    /// </summary>
    private UserCandidate SelectLeastBusy(List<UserCandidate> candidates)
    {
        return candidates.OrderBy(c => c.CurrentWorkload)
            .ThenByDescending(c => c.SuccessRate)
            .First();
    }

    /// <summary>
    /// Skill-based assignment - assign to user with highest skill match
    /// </summary>
    private UserCandidate SelectSkillBased(List<UserCandidate> candidates)
    {
        return candidates.OrderByDescending(c => c.SkillMatchScore)
            .ThenBy(c => c.CurrentWorkload)
            .First();
    }

    /// <summary>
    /// Priority-based assignment - high priority goes to most skilled/successful users
    /// </summary>
    private UserCandidate SelectPriorityBased(List<UserCandidate> candidates, AssignmentContext context)
    {
        if (context.Priority >= 3) // High priority
        {
            return candidates.OrderByDescending(c => c.SuccessRate)
                .ThenByDescending(c => c.SkillMatchScore)
                .ThenBy(c => c.CurrentWorkload)
                .First();
        }
        else // Normal/Low priority
        {
            return SelectWorkloadBalanced(candidates);
        }
    }

    /// <summary>
    /// Workload-balanced assignment - considers multiple factors
    /// </summary>
    private UserCandidate SelectWorkloadBalanced(List<UserCandidate> candidates)
    {
        // Calculate composite score
        var maxWorkload = candidates.Max(c => c.CurrentWorkload);
        if (maxWorkload == 0) maxWorkload = 1;

        foreach (var candidate in candidates)
        {
            var workloadScore = 1.0 - ((double)candidate.CurrentWorkload / maxWorkload);
            var performanceScore = (double)candidate.SuccessRate;
            var skillScore = candidate.SkillMatchScore;

            candidate.OverallScore =
                (workloadScore * WORKLOAD_BALANCE_WEIGHT) +
                (performanceScore * PERFORMANCE_WEIGHT) +
                (skillScore * SKILL_WEIGHT);
        }

        return candidates.OrderByDescending(c => c.OverallScore).First();
    }

    /// <summary>
    /// Best fit assignment - optimal match considering all factors
    /// </summary>
    private UserCandidate SelectBestFit(List<UserCandidate> candidates)
    {
        return candidates.OrderByDescending(c => c.OverallScore).First();
    }

    /// <summary>
    /// Experience-based assignment - prefer users who have handled similar complaints
    /// </summary>
    private UserCandidate SelectExperienceBased(List<UserCandidate> candidates)
    {
        // For now, use success rate as proxy for experience
        // TODO: Implement category-specific experience tracking
        return candidates.OrderByDescending(c => c.SuccessRate)
            .ThenByDescending(c => c.SkillMatchScore)
            .ThenBy(c => c.CurrentWorkload)
            .First();
    }

    /// <summary>
    /// Random assignment - useful when all factors are equal
    /// </summary>
    private UserCandidate SelectRandom(List<UserCandidate> candidates)
    {
        var random = new Random();
        var index = random.Next(candidates.Count);
        return candidates[index];
    }

    #endregion

    #region Validation

    public async Task<AssignmentValidationResult> ValidateAssignmentAsync(
        Guid complaintId,
        Guid? userId = null,
        Guid? poolId = null,
        AssignmentContext? context = null,
        CancellationToken cancellationToken = default)
    {
        var result = new AssignmentValidationResult
        {
            ValidationErrors = new List<string>(),
            ValidationWarnings = new List<string>(),
            Recommendations = new List<string>()
        };

        try
        {
            // Validate complaint exists
            var complaint = await _unitOfWork.Complaints.GetByIdAsync(complaintId, cancellationToken);
            if (complaint == null)
            {
                result.ValidationErrors.Add("Complaint not found");
                result.IsValid = false;
                return result;
            }

            // Validate user if specified
            if (userId.HasValue)
            {
                var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);
                if (user == null)
                {
                    result.ValidationErrors.Add("User not found");
                    result.IsValid = false;
                    return result;
                }

                if (!user.IsActive)
                {
                    result.ValidationErrors.Add("User is not active");
                    result.IsValid = false;
                    return result;
                }

                // Check workload
                var userWorkload = await GetUserCurrentWorkload(userId.Value, cancellationToken);
                if (userWorkload >= DEFAULT_MAX_WORKLOAD)
                {
                    result.ValidationWarnings.Add($"User is at or near capacity ({userWorkload} active complaints)");
                    result.SuitabilityScore -= 0.3;
                }
            }

            // Validate pool if specified
            if (poolId.HasValue)
            {
                var pool = await _context.Set<ResourcePool>()
                    .FirstOrDefaultAsync(p => p.Id == poolId.Value, cancellationToken);
                if (pool == null)
                {
                    result.ValidationErrors.Add("Resource pool not found");
                    result.IsValid = false;
                    return result;
                }

                if (!pool.IsActive)
                {
                    result.ValidationErrors.Add("Resource pool is not active");
                    result.IsValid = false;
                    return result;
                }
            }

            result.IsValid = result.ValidationErrors.Count == 0;
            result.SuitabilityScore = Math.Max(0, Math.Min(1.0, 0.8 + result.SuitabilityScore));

            if (result.IsValid)
            {
                result.Recommendations.Add("Assignment is valid and recommended");
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating assignment for complaint {ComplaintId}", complaintId);
            result.ValidationErrors.Add($"Validation failed: {ex.Message}");
            result.IsValid = false;
            return result;
        }
    }

    #endregion

    #region Get Assignment Candidates

    public async Task<List<ResourcePoolCandidate>> GetAssignmentCandidatesAsync(
        Guid complaintId,
        AssignmentCriteria? criteria = null,
        bool includeUserDetails = true,
        bool calculateSkillScores = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var complaint = await LoadComplaintWithDetails(complaintId, cancellationToken);
            if (complaint == null)
            {
                return new List<ResourcePoolCandidate>();
            }

            criteria ??= BuildCriteriaFromComplaint(complaint);

            var candidates = await FindCandidatePoolsAsync(criteria, cancellationToken);

            if (includeUserDetails)
            {
                foreach (var candidate in candidates)
                {
                    var context = BuildContextFromComplaint(complaint, new AssignmentContext());
                    candidate.BestUserCandidate = await SelectUserFromPoolAsync(
                        candidate.ResourcePoolId,
                        AdvancedAssignmentMethod.BestFit,
                        context,
                        cancellationToken);
                }
            }

            return candidates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting assignment candidates for complaint {ComplaintId}", complaintId);
            return new List<ResourcePoolCandidate>();
        }
    }

    #endregion

    #region Execute Assignment Rules

    public async Task<AssignmentResult> ExecuteAssignmentRulesAsync(
        Guid complaintId,
        AssignmentContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var complaint = await LoadComplaintWithDetails(complaintId, cancellationToken);
            if (complaint == null)
            {
                return AssignmentResult.Failure("Complaint not found", "COMPLAINT_NOT_FOUND");
            }

            // Get active assignment rules for the company, ordered by priority
            var rules = await _context.Set<AssignmentRule>()
                .Where(r => r.CompanyId == complaint.CompanyId &&
                           r.IsActive &&
                           !r.DeactivatedAt.HasValue)
                .ToListAsync(cancellationToken);

            var orderedRules = rules.OrderBy(r => r.Priority).ToList();

            if (!orderedRules.Any())
            {
                return AssignmentResult.Failure("No active assignment rules found", "NO_RULES_FOUND");
            }

            // Evaluate each rule until one matches
            foreach (var rule in orderedRules)
            {
                var matches = await EvaluateRule(rule, complaint, context, cancellationToken);
                if (matches)
                {
                    _logger.LogInformation("Rule {RuleId} - '{RuleName}' matched for complaint {ComplaintId}",
                        rule.Id, rule.RuleName, complaintId);

                    // Execute the rule action
                    var result = await ExecuteRuleAction(rule, complaint, context, cancellationToken);

                    // Record rule execution
                    rule.RecordExecution(result.Success);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    if (result.Success)
                    {
                        result.AppliedRuleId = rule.Id;
                        result.AppliedRuleName = rule.RuleName;
                        return result;
                    }
                }
            }

            return AssignmentResult.Failure("No matching rules found", "NO_MATCHING_RULES");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing assignment rules for complaint {ComplaintId}", complaintId);
            return AssignmentResult.Failure($"Rule execution failed: {ex.Message}", "RULE_EXECUTION_ERROR");
        }
    }

    #endregion

    #region Calculate User Suitability Score

    public async Task<double> CalculateUserSuitabilityScoreAsync(
        Guid userId,
        Guid poolId,
        AssignmentCriteria criteria,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
            if (user == null || !user.IsActive)
            {
                return 0.0;
            }

            var userCandidate = await BuildUserCandidate(user, new AssignmentContext(), cancellationToken);

            // Calculate composite score
            var workloadScore = CalculateWorkloadScore(userCandidate, criteria.WorkloadCriteria.MaxCurrentWorkload);
            var performanceScore = (double)userCandidate.SuccessRate;
            var skillScore = userCandidate.SkillMatchScore;

            return (workloadScore * WORKLOAD_BALANCE_WEIGHT) +
                   (performanceScore * PERFORMANCE_WEIGHT) +
                   (skillScore * SKILL_WEIGHT);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating suitability score for user {UserId}", userId);
            return 0.0;
        }
    }

    #endregion

    #region Update Workload Statistics

    public async Task UpdateWorkloadStatisticsAsync(
        Guid userId,
        Guid poolId,
        Guid complaintId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Updating workload statistics for user {UserId} in pool {PoolId}",
                userId, poolId);

            // This is a placeholder for future workload tracking implementation
            // TODO: Implement ResourcePoolWorkloadTracker updates when entity is created

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating workload statistics for user {UserId}", userId);
            // Don't throw - workload tracking is non-critical
        }
    }

    #endregion

    #region Helper Methods

    private async Task<Complaint?> LoadComplaintWithDetails(Guid complaintId, CancellationToken cancellationToken)
    {
        return await _unitOfWork.Complaints.GetByIdWithIncludesAsync(
            complaintId,
            cancellationToken,
            c => c.Category,
            c => c.PriorityMaster,
            c => c.Company,
            c => c.Branch,
            c => c.Department,
            c => c.Section);
    }

    /// <summary>
    /// Get status from ComplaintStatusMaster by code for dynamic status management
    /// Falls back to finding by name if code not found
    /// </summary>
    private async Task<ComplaintStatusMaster?> GetStatusByCodeAsync(
        string statusCode,
        Guid companyId,
        CancellationToken cancellationToken)
    {
        // Try to find by code (preferred method)
        var status = await _context.Set<ComplaintStatusMaster>()
            .Where(s => !s.IsDeleted &&
                       s.IsActive &&
                       s.Code.ToUpper() == statusCode.ToUpper() &&
                       (s.CompanyId == companyId || s.CompanyId == null))
            .OrderByDescending(s => s.CompanyId) // Prefer company-specific over global
            .FirstOrDefaultAsync(cancellationToken);

        // If not found by code, try finding by name (e.g., "In Progress" for "IN_PROGRESS")
        if (status == null)
        {
            var statusName = statusCode.Replace("_", " ");
            status = await _context.Set<ComplaintStatusMaster>()
                .Where(s => !s.IsDeleted &&
                           s.IsActive &&
                           s.Name.ToUpper() == statusName.ToUpper() &&
                           (s.CompanyId == companyId || s.CompanyId == null))
                .OrderByDescending(s => s.CompanyId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return status;
    }

    private AssignmentCriteria BuildCriteriaFromComplaint(Complaint complaint)
    {
        var criteria = new AssignmentCriteria
        {
            OrganizationalScope = new OrganizationalScope
            {
                CompanyIds = new List<Guid> { complaint.CompanyId }
            },
            ComplaintCriteria = new ComplaintCriteria
            {
                CategoryIds = complaint.CategoryId != Guid.Empty
                    ? new List<Guid> { complaint.CategoryId }
                    : new List<Guid>(),
                PriorityRange = new PriorityRange
                {
                    Min = complaint.PriorityMaster?.DisplayOrder ?? 1,
                    Max = 4
                }
            }
        };

        if (complaint.BranchId.HasValue)
        {
            criteria.OrganizationalScope.BranchIds.Add(complaint.BranchId.Value);
        }

        if (complaint.DepartmentId.HasValue)
        {
            criteria.OrganizationalScope.DepartmentIds.Add(complaint.DepartmentId.Value);
        }

        if (complaint.SectionId.HasValue)
        {
            criteria.OrganizationalScope.SectionIds.Add(complaint.SectionId.Value);
        }

        return criteria;
    }

    private AssignmentContext BuildContextFromComplaint(Complaint complaint, AssignmentContext baseContext)
    {
        baseContext.Priority = complaint.PriorityMaster?.DisplayOrder ?? 1;
        baseContext.IsInitialAssignment = !complaint.AssignedToId.HasValue;
        return baseContext;
    }

    private async Task<ResourcePoolCandidate> BuildPoolCandidate(
        ResourcePool pool,
        AssignmentCriteria criteria,
        CancellationToken cancellationToken)
    {
        var candidate = new ResourcePoolCandidate
        {
            ResourcePoolId = pool.Id,
            ResourcePoolName = pool.Name,
            PoolType = pool.PoolType.ToString(),
            TotalMemberCount = pool.Members.Count,
            AvailableMemberCount = pool.Members.Count(m => !m.IsDeleted),
            CanHandleComplaint = true,
            DisqualificationReasons = new List<string>(),
            SuitabilityScore = 0.5 // Base score
        };

        // Check if pool has members
        if (candidate.AvailableMemberCount == 0)
        {
            candidate.CanHandleComplaint = false;
            candidate.DisqualificationReasons.Add("No available members in pool");
            return candidate;
        }

        // Calculate suitability score based on various factors
        // TODO: Enhance with specialization matching when entity is available

        // Factor 1: Member availability (30%)
        var availabilityScore = Math.Min(1.0, (double)candidate.AvailableMemberCount / 5);

        // Factor 2: Organizational alignment (40%)
        var orgScore = CalculateOrganizationalAlignment(pool, criteria);

        // Factor 3: Pool capacity (30%)
        var capacityScore = CalculatePoolCapacity(pool);

        candidate.SuitabilityScore = (availabilityScore * 0.3) + (orgScore * 0.4) + (capacityScore * 0.3);
        candidate.RecommendedAssignmentMethod = DetermineOptimalAssignmentMethod(candidate).ToString();

        return candidate;
    }

    private double CalculateOrganizationalAlignment(ResourcePool pool, AssignmentCriteria criteria)
    {
        double score = 0.0;

        // Exact matches get higher scores
        if (criteria.OrganizationalScope.SectionIds.Any() && pool.SectionId.HasValue &&
            criteria.OrganizationalScope.SectionIds.Contains(pool.SectionId.Value))
        {
            score += 1.0; // Section match is most specific
        }
        else if (criteria.OrganizationalScope.DepartmentIds.Any() && pool.DepartmentId.HasValue &&
                criteria.OrganizationalScope.DepartmentIds.Contains(pool.DepartmentId.Value))
        {
            score += 0.8; // Department match
        }
        else if (criteria.OrganizationalScope.BranchIds.Any() && pool.BranchId.HasValue &&
                criteria.OrganizationalScope.BranchIds.Contains(pool.BranchId.Value))
        {
            score += 0.6; // Branch match
        }
        else if (criteria.OrganizationalScope.CompanyIds.Any() &&
                criteria.OrganizationalScope.CompanyIds.Contains(pool.CompanyId))
        {
            score += 0.4; // Company match (least specific)
        }

        return Math.Min(1.0, score);
    }

    private double CalculatePoolCapacity(ResourcePool pool)
    {
        // Simple capacity calculation
        // TODO: Enhance with actual workload data
        var memberCount = pool.Members.Count;
        if (memberCount == 0) return 0.0;
        if (memberCount >= 5) return 1.0;
        return memberCount / 5.0;
    }

    private AdvancedAssignmentMethod DetermineOptimalAssignmentMethod(ResourcePoolCandidate candidate)
    {
        // Simple heuristic to determine best method
        if (candidate.AvailableMemberCount <= 2)
        {
            return AdvancedAssignmentMethod.LeastBusy;
        }

        if (candidate.AverageWorkload > 5)
        {
            return AdvancedAssignmentMethod.WorkloadBalanced;
        }

        return AdvancedAssignmentMethod.BestFit;
    }

    private async Task<List<UserCandidate>> BuildUserCandidates(
        List<User> users,
        AssignmentContext context,
        CancellationToken cancellationToken)
    {
        var candidates = new List<UserCandidate>();

        foreach (var user in users)
        {
            var candidate = await BuildUserCandidate(user, context, cancellationToken);
            candidates.Add(candidate);
        }

        return candidates;
    }

    private async Task<UserCandidate> BuildUserCandidate(
        User user,
        AssignmentContext context,
        CancellationToken cancellationToken)
    {
        var workload = await GetUserCurrentWorkload(user.Id, cancellationToken);

        var candidate = new UserCandidate
        {
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            CurrentWorkload = workload,
            IsAvailable = user.IsActive && workload < DEFAULT_MAX_WORKLOAD,
            SuccessRate = 0.85m, // TODO: Calculate from actual data
            AverageResolutionTime = TimeSpan.FromHours(24), // TODO: Calculate from actual data
            SkillMatchScore = 0.7, // TODO: Calculate from actual skills
            OverallScore = 0.7,
            LastActivityAt = DateTime.UtcNow.AddDays(-1) // TODO: Get from actual data
        };

        if (workload >= DEFAULT_MAX_WORKLOAD)
        {
            candidate.DisqualificationReasons.Add($"User at capacity ({workload} active complaints)");
            candidate.IsAvailable = false;
        }

        return candidate;
    }

    private async Task<int> GetUserCurrentWorkload(Guid userId, CancellationToken cancellationToken)
    {
        var activeComplaints = await _context.Complaints
            .Include(c => c.StatusMaster)
            .Where(c => c.AssignedToId == userId &&
                       !c.StatusMaster.IsFinal &&
                       !c.IsDeleted)
            .CountAsync(cancellationToken);

        return activeComplaints;
    }

    private double CalculateWorkloadScore(UserCandidate candidate, int maxWorkload)
    {
        if (candidate.CurrentWorkload >= maxWorkload) return 0.0;
        return 1.0 - ((double)candidate.CurrentWorkload / maxWorkload);
    }

    private async Task<AssignmentResult> AssignToSpecificUser(
        Complaint complaint,
        Guid userId,
        ResourcePool pool,
        AssignmentContext context,
        CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user == null)
        {
            return AssignmentResult.Failure("Specified user not found", "USER_NOT_FOUND");
        }

        if (!user.IsActive)
        {
            return AssignmentResult.Failure("Specified user is not active", "USER_INACTIVE");
        }

        // Check if user is member of pool
        var isMember = pool.Members.Any(m => m.UserId == userId && !m.IsDeleted);
        if (!isMember && !context.ForceAssignment)
        {
            return AssignmentResult.Failure("User is not a member of the specified pool", "USER_NOT_IN_POOL");
        }

        complaint.AssignedToId = userId;

        // Set status dynamically from StatusMaster (user-configurable)
        var assignedStatus = await GetStatusByCodeAsync("IN_PROGRESS", complaint.CompanyId, cancellationToken);
        if (assignedStatus != null)
        {
            complaint.StatusMasterId = assignedStatus.Id;
        }
        else
        {
            _logger.LogWarning("StatusMaster with code IN_PROGRESS not found for company {CompanyId}", complaint.CompanyId);
        }

        complaint.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await UpdateWorkloadStatisticsAsync(userId, pool.Id, complaint.Id, cancellationToken);

        return AssignmentResult.CreateSuccess(
            userId,
            user.FullName,
            "Manual",
            "Complaint assigned to specific user",
            pool.Id,
            pool.Name);
    }

    private async Task<bool> EvaluateRule(
        AssignmentRule rule,
        Complaint complaint,
        AssignmentContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            // Parse conditions JSON
            var conditions = JsonSerializer.Deserialize<RuleConditions>(rule.ConditionsJson);
            if (conditions == null) return false;

            // Evaluate organizational conditions
            if (conditions.Organizational != null)
            {
                if (conditions.Organizational.CompanyIds?.Any() == true &&
                    !conditions.Organizational.CompanyIds.Contains(complaint.CompanyId))
                {
                    return false;
                }

                if (conditions.Organizational.BranchIds?.Any() == true &&
                    (!complaint.BranchId.HasValue ||
                     !conditions.Organizational.BranchIds.Contains(complaint.BranchId.Value)))
                {
                    return false;
                }
            }

            // Evaluate complaint conditions
            if (conditions.Complaint != null)
            {
                if (conditions.Complaint.CategoryIds?.Any() == true &&
                    (complaint.CategoryId == Guid.Empty ||
                     !conditions.Complaint.CategoryIds.Contains(complaint.CategoryId)))
                {
                    return false;
                }

                if (conditions.Complaint.PriorityRange != null &&
                    !conditions.Complaint.PriorityRange.Contains(complaint.PriorityMaster?.DisplayOrder ?? 1))
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error evaluating rule {RuleId}", rule.Id);
            return false;
        }
    }

    private async Task<AssignmentResult> ExecuteRuleAction(
        AssignmentRule rule,
        Complaint complaint,
        AssignmentContext context,
        CancellationToken cancellationToken)
    {
        try
        {
            var actionParams = JsonSerializer.Deserialize<RuleActionParameters>(rule.ActionParametersJson);
            if (actionParams == null)
            {
                return AssignmentResult.Failure("Invalid action parameters", "INVALID_ACTION_PARAMS");
            }

            switch (rule.ActionType)
            {
                case AssignmentActionType.AssignToPool:
                    if (actionParams.PoolId.HasValue)
                    {
                        var method = Enum.TryParse<AdvancedAssignmentMethod>(
                            actionParams.Method ?? "BestFit", out var parsedMethod)
                            ? parsedMethod
                            : AdvancedAssignmentMethod.BestFit;

                        return await AssignComplaintToPoolAsync(
                            complaint.Id,
                            actionParams.PoolId.Value,
                            method,
                            null,
                            context,
                            cancellationToken);
                    }
                    break;

                case AssignmentActionType.AssignToUser:
                    if (actionParams.UserId.HasValue)
                    {
                        complaint.AssignedToId = actionParams.UserId.Value;

                        // Set status dynamically from StatusMaster (user-configurable)
                        var assignedStatus = await GetStatusByCodeAsync("IN_PROGRESS", complaint.CompanyId, cancellationToken);
                        if (assignedStatus != null)
                        {
                            complaint.StatusMasterId = assignedStatus.Id;
                        }
                        else
                        {
                            _logger.LogWarning("StatusMaster with code IN_PROGRESS not found for company {CompanyId}", complaint.CompanyId);
                        }

                        await _unitOfWork.SaveChangesAsync(cancellationToken);

                        var user = await _unitOfWork.Users.GetByIdAsync(actionParams.UserId.Value, cancellationToken);
                        return AssignmentResult.CreateSuccess(
                            actionParams.UserId.Value,
                            user?.FullName ?? "Unknown",
                            "Rule-Based Direct Assignment",
                            $"Assigned by rule: {rule.RuleName}");
                    }
                    break;

                default:
                    return AssignmentResult.Failure($"Unsupported action type: {rule.ActionType}", "UNSUPPORTED_ACTION");
            }

            return AssignmentResult.Failure("Action execution failed", "ACTION_FAILED");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing rule action for rule {RuleId}", rule.Id);
            return AssignmentResult.Failure($"Action execution error: {ex.Message}", "ACTION_ERROR");
        }
    }

    #endregion

    #region Rule DTOs

    private class RuleConditions
    {
        public OrganizationalConditions? Organizational { get; set; }
        public ComplaintConditions? Complaint { get; set; }
        public TemporalConditions? Temporal { get; set; }
        public WorkloadConditions? Workload { get; set; }
    }

    private class OrganizationalConditions
    {
        public List<Guid>? CompanyIds { get; set; }
        public List<Guid>? BranchIds { get; set; }
        public List<Guid>? DepartmentIds { get; set; }
        public List<Guid>? SectionIds { get; set; }
    }

    private class ComplaintConditions
    {
        public List<Guid>? CategoryIds { get; set; }
        public PriorityRange? PriorityRange { get; set; }
        public EscalationLevelRange? EscalationLevel { get; set; }
    }

    private class TemporalConditions
    {
        public bool BusinessHoursOnly { get; set; }
        public List<int>? DaysOfWeek { get; set; }
        public TimeRangeJson? TimeRange { get; set; }
    }

    private class TimeRangeJson
    {
        public string? Start { get; set; }
        public string? End { get; set; }
    }

    private class WorkloadConditions
    {
        public int MaxCurrentWorkload { get; set; }
        public decimal MinSuccessRate { get; set; }
    }

    private class RuleActionParameters
    {
        public Guid? PoolId { get; set; }
        public Guid? UserId { get; set; }
        public string? Method { get; set; }
    }

    #endregion
}
