using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Workflows;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Workflow engine implementation for category-specific complaint workflows
/// </summary>
public class WorkflowEngine : IWorkflowEngine
{
    private readonly ComplaintDbContext _context;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<WorkflowEngine> _logger;

    public WorkflowEngine(
        ComplaintDbContext context,
        IUnitOfWork unitOfWork,
        ILogger<WorkflowEngine> logger)
    {
        _context = context;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CategoryWorkflow?> GetWorkflowForCategoryAsync(Guid categoryId)
    {
        try
        {
            // Get active workflow for the category
            var workflow = await _context.CategoryWorkflows
                .Include(w => w.WorkflowStatuses.Where(ws => ws.IsActive))
                    .ThenInclude(ws => ws.StatusMaster)
                .Include(w => w.Transitions.Where(t => t.IsActive))
                    .ThenInclude(t => t.FromStatus)
                .Include(w => w.Transitions.Where(t => t.IsActive))
                    .ThenInclude(t => t.ToStatus)
                .Where(w => w.CategoryId == categoryId && w.IsActive && w.IsDefault)
                .OrderByDescending(w => w.CreatedAt)
                .FirstOrDefaultAsync();

            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow for category {CategoryId}", categoryId);
            return null;
        }
    }

    public async Task<ComplaintStatusMaster> GetInitialStatusAsync(Guid categoryId)
    {
        try
        {
            // Try to get category-specific initial status
            var workflow = await GetWorkflowForCategoryAsync(categoryId);

            if (workflow != null)
            {
                var initialStatus = workflow.WorkflowStatuses
                    .FirstOrDefault(ws => ws.IsInitialStatus && ws.IsActive);

                if (initialStatus != null)
                {
                    _logger.LogInformation(
                        "Using category-specific initial status {StatusName} for category {CategoryId}",
                        initialStatus.StatusMaster.Name,
                        categoryId);
                    return initialStatus.StatusMaster;
                }
            }

            // Fallback to global "SUBMITTED" status
            var submittedStatus = await _context.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Code == "SUBMITTED" && s.IsActive);

            if (submittedStatus == null)
            {
                throw new InvalidOperationException(
                    "No initial status found. Please configure a SUBMITTED status or category workflow.");
            }

            _logger.LogInformation(
                "Using global SUBMITTED status for category {CategoryId}",
                categoryId);

            return submittedStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting initial status for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<List<CategoryWorkflowTransition>> GetAllowedTransitionsAsync(
        Guid categoryId,
        Guid currentStatusId,
        Guid userId)
    {
        try
        {
            var workflow = await GetWorkflowForCategoryAsync(categoryId);

            if (workflow == null)
            {
                _logger.LogInformation(
                    "No custom workflow for category {CategoryId}, returning all transitions",
                    categoryId);
                return new List<CategoryWorkflowTransition>();
            }

            // Get user roles
            var userRoles = await GetUserRoleIdsAsync(userId);

            // Get allowed transitions for current status
            var transitions = workflow.Transitions
                .Where(t => t.FromStatusId == currentStatusId && t.IsActive)
                .Where(t => IsUserAuthorizedForTransition(t, userRoles))
                .OrderBy(t => t.DisplayOrder)
                .ToList();

            _logger.LogInformation(
                "Found {Count} allowed transitions for user {UserId} from status {StatusId}",
                transitions.Count,
                userId,
                currentStatusId);

            return transitions;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error getting allowed transitions for category {CategoryId}, status {StatusId}, user {UserId}",
                categoryId,
                currentStatusId,
                userId);
            return new List<CategoryWorkflowTransition>();
        }
    }

    public async Task<bool> IsTransitionAllowedAsync(
        Guid categoryId,
        Guid fromStatusId,
        Guid toStatusId,
        Guid userId)
    {
        try
        {
            var workflow = await GetWorkflowForCategoryAsync(categoryId);

            if (workflow == null)
            {
                // No custom workflow - allow all transitions
                _logger.LogInformation(
                    "No custom workflow for category {CategoryId}, allowing transition",
                    categoryId);
                return true;
            }

            var userRoles = await GetUserRoleIdsAsync(userId);

            var transition = workflow.Transitions
                .FirstOrDefault(t =>
                    t.FromStatusId == fromStatusId &&
                    t.ToStatusId == toStatusId &&
                    t.IsActive);

            if (transition == null)
            {
                _logger.LogWarning(
                    "No transition configured from {FromStatusId} to {ToStatusId} in workflow {WorkflowId}",
                    fromStatusId,
                    toStatusId,
                    workflow.Id);
                return false;
            }

            var isAuthorized = IsUserAuthorizedForTransition(transition, userRoles);

            _logger.LogInformation(
                "Transition from {FromStatusId} to {ToStatusId} is {Result} for user {UserId}",
                fromStatusId,
                toStatusId,
                isAuthorized ? "ALLOWED" : "DENIED",
                userId);

            return isAuthorized;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking transition permission for category {CategoryId}, from {FromStatusId} to {ToStatusId}, user {UserId}",
                categoryId,
                fromStatusId,
                toStatusId,
                userId);
            return false;
        }
    }

    public async Task<List<ComplaintStatusMaster>> GetWorkflowStatusesAsync(Guid categoryId)
    {
        try
        {
            var workflow = await GetWorkflowForCategoryAsync(categoryId);

            if (workflow == null)
            {
                // Return all global statuses
                var globalStatuses = await _context.ComplaintStatusMasters
                    .Where(s => s.IsActive)
                    .OrderBy(s => s.DisplayOrder)
                    .ToListAsync();

                _logger.LogInformation(
                    "No custom workflow for category {CategoryId}, returning {Count} global statuses",
                    categoryId,
                    globalStatuses.Count);

                return globalStatuses;
            }

            var statuses = workflow.WorkflowStatuses
                .Where(ws => ws.IsActive)
                .OrderBy(ws => ws.DisplayOrder)
                .Select(ws => ws.StatusMaster)
                .ToList();

            _logger.LogInformation(
                "Found {Count} statuses in workflow for category {CategoryId}",
                statuses.Count,
                categoryId);

            return statuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting workflow statuses for category {CategoryId}", categoryId);
            return new List<ComplaintStatusMaster>();
        }
    }

    public async Task<bool> TransitionComplaintAsync(
        Guid complaintId,
        Guid newStatusId,
        Guid userId,
        string? comment = null)
    {
        try
        {
            var complaint = await _context.Complaints
                .Include(c => c.Category)
                .FirstOrDefaultAsync(c => c.Id == complaintId);

            if (complaint == null)
            {
                _logger.LogWarning("Complaint {ComplaintId} not found", complaintId);
                return false;
            }

            var currentStatusId = complaint.StatusMasterId;

            // Check if transition is allowed
            var isAllowed = await IsTransitionAllowedAsync(
                complaint.CategoryId,
                currentStatusId,
                newStatusId,
                userId);

            if (!isAllowed)
            {
                _logger.LogWarning(
                    "Transition denied for complaint {ComplaintId} from {FromStatusId} to {ToStatusId} for user {UserId}",
                    complaintId,
                    currentStatusId,
                    newStatusId,
                    userId);
                return false;
            }

            // Check if comment is required
            var workflow = await GetWorkflowForCategoryAsync(complaint.CategoryId);
            if (workflow != null)
            {
                var requiresComment = await TransitionRequiresCommentAsync(
                    workflow.Id,
                    currentStatusId,
                    newStatusId);

                if (requiresComment && string.IsNullOrWhiteSpace(comment))
                {
                    _logger.LogWarning(
                        "Comment required for transition from {FromStatusId} to {ToStatusId}",
                        currentStatusId,
                        newStatusId);
                    return false;
                }
            }

            // Perform the transition
            complaint.StatusMasterId = newStatusId;
            complaint.UpdatedAt = DateTime.UtcNow;
            complaint.UpdatedBy = userId;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Successfully transitioned complaint {ComplaintId} from {FromStatusId} to {ToStatusId} by user {UserId}",
                complaintId,
                currentStatusId,
                newStatusId,
                userId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error transitioning complaint {ComplaintId} to status {StatusId}",
                complaintId,
                newStatusId);
            return false;
        }
    }

    public async Task<List<CategoryWorkflow>> GetAllWorkflowsAsync(Guid? companyId = null)
    {
        try
        {
            var query = _context.CategoryWorkflows
                .Include(w => w.Category)
                .Include(w => w.WorkflowStatuses)
                    .ThenInclude(ws => ws.StatusMaster)
                .Include(w => w.Transitions)
                .Where(w => w.IsActive);

            if (companyId.HasValue)
            {
                query = query.Where(w => w.CompanyId == companyId.Value || w.CompanyId == null);
            }

            var workflows = await query
                .OrderBy(w => w.Category.Name)
                .ThenBy(w => w.Name)
                .ToListAsync();

            return workflows;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all workflows");
            return new List<CategoryWorkflow>();
        }
    }

    public async Task<CategoryWorkflow> CreateWorkflowAsync(
        Guid categoryId,
        string name,
        string? description = null,
        Guid? companyId = null)
    {
        try
        {
            var workflow = new CategoryWorkflow
            {
                Id = Guid.NewGuid(),
                CategoryId = categoryId,
                Name = name,
                Description = description,
                CompanyId = companyId,
                IsActive = true,
                IsDefault = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.CategoryWorkflows.Add(workflow);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created workflow {WorkflowId} for category {CategoryId}",
                workflow.Id,
                categoryId);

            return workflow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow for category {CategoryId}", categoryId);
            throw;
        }
    }

    public async Task<CategoryWorkflowStatus> AddStatusToWorkflowAsync(
        Guid workflowId,
        Guid statusMasterId,
        int displayOrder,
        bool isInitialStatus = false,
        int? defaultSLAHours = null)
    {
        try
        {
            var workflowStatus = new CategoryWorkflowStatus
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                StatusMasterId = statusMasterId,
                DisplayOrder = displayOrder,
                IsInitialStatus = isInitialStatus,
                DefaultSLAHours = defaultSLAHours,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.CategoryWorkflowStatuses.Add(workflowStatus);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Added status {StatusId} to workflow {WorkflowId} at position {DisplayOrder}",
                statusMasterId,
                workflowId,
                displayOrder);

            return workflowStatus;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error adding status {StatusId} to workflow {WorkflowId}",
                statusMasterId,
                workflowId);
            throw;
        }
    }

    public async Task<CategoryWorkflowTransition> AddTransitionRuleAsync(
        Guid workflowId,
        Guid fromStatusId,
        Guid toStatusId,
        string? transitionName = null,
        bool requiresComment = false,
        bool requiresApproval = false,
        Guid[]? allowedRoles = null)
    {
        try
        {
            var transition = new CategoryWorkflowTransition
            {
                Id = Guid.NewGuid(),
                WorkflowId = workflowId,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId,
                TransitionName = transitionName,
                RequiresComment = requiresComment,
                RequiresApproval = requiresApproval,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            if (allowedRoles != null && allowedRoles.Length > 0)
            {
                transition.SetAllowedRoleIds(allowedRoles);
            }

            _context.CategoryWorkflowTransitions.Add(transition);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Added transition from {FromStatusId} to {ToStatusId} in workflow {WorkflowId}",
                fromStatusId,
                toStatusId,
                workflowId);

            return transition;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error adding transition from {FromStatusId} to {ToStatusId} in workflow {WorkflowId}",
                fromStatusId,
                toStatusId,
                workflowId);
            throw;
        }
    }

    public async Task<bool> TransitionRequiresCommentAsync(
        Guid workflowId,
        Guid fromStatusId,
        Guid toStatusId)
    {
        try
        {
            var transition = await _context.CategoryWorkflowTransitions
                .FirstOrDefaultAsync(t =>
                    t.WorkflowId == workflowId &&
                    t.FromStatusId == fromStatusId &&
                    t.ToStatusId == toStatusId &&
                    t.IsActive);

            return transition?.RequiresComment ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking comment requirement for transition in workflow {WorkflowId}",
                workflowId);
            return false;
        }
    }

    public async Task<bool> TransitionRequiresApprovalAsync(
        Guid workflowId,
        Guid fromStatusId,
        Guid toStatusId)
    {
        try
        {
            var transition = await _context.CategoryWorkflowTransitions
                .FirstOrDefaultAsync(t =>
                    t.WorkflowId == workflowId &&
                    t.FromStatusId == fromStatusId &&
                    t.ToStatusId == toStatusId &&
                    t.IsActive);

            return transition?.RequiresApproval ?? false;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error checking approval requirement for transition in workflow {WorkflowId}",
                workflowId);
            return false;
        }
    }

    #region Private Helper Methods

    private bool IsUserAuthorizedForTransition(
        CategoryWorkflowTransition transition,
        Guid[] userRoles)
    {
        if (string.IsNullOrWhiteSpace(transition.AllowedRoles))
        {
            // No restriction - all users allowed
            return true;
        }

        var allowedRoles = transition.GetAllowedRoleIds();
        if (allowedRoles.Length == 0)
        {
            // Empty array means all users allowed
            return true;
        }

        // Check if user has any of the allowed roles
        return allowedRoles.Intersect(userRoles).Any();
    }

    private async Task<Guid[]> GetUserRoleIdsAsync(Guid userId)
    {
        try
        {
            var roleIds = await _context.UserComplaintRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Select(ur => ur.ComplaintRoleId)
                .ToArrayAsync();

            _logger.LogDebug("User {UserId} has {Count} roles", userId, roleIds.Length);

            return roleIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting role IDs for user {UserId}", userId);
            return Array.Empty<Guid>();
        }
    }

    #endregion
}
