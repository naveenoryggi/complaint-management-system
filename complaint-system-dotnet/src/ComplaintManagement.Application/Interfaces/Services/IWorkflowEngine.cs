using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Workflows;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Workflow engine service for managing category-specific complaint workflows
/// Handles workflow retrieval, transition validation, and status management
/// </summary>
public interface IWorkflowEngine
{
    /// <summary>
    /// Get the active workflow for a specific category
    /// Returns null if no custom workflow is configured (fallback to global workflow)
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Active workflow for the category, or null if using global workflow</returns>
    Task<CategoryWorkflow?> GetWorkflowForCategoryAsync(Guid categoryId);

    /// <summary>
    /// Get the initial status for a new complaint in a specific category
    /// Uses category workflow if available, otherwise returns global "SUBMITTED" status
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>Initial status for new complaints</returns>
    Task<ComplaintStatusMaster> GetInitialStatusAsync(Guid categoryId);

    /// <summary>
    /// Get all allowed status transitions from the current status for a specific category
    /// Filtered by user's roles and permissions
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="currentStatusId">Current complaint status ID</param>
    /// <param name="userId">User ID requesting transitions</param>
    /// <returns>List of allowed transitions for the user</returns>
    Task<List<CategoryWorkflowTransition>> GetAllowedTransitionsAsync(
        Guid categoryId,
        Guid currentStatusId,
        Guid userId);

    /// <summary>
    /// Validate if a specific status transition is allowed
    /// Checks workflow configuration and user permissions
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="fromStatusId">Current status ID</param>
    /// <param name="toStatusId">Target status ID</param>
    /// <param name="userId">User ID attempting the transition</param>
    /// <returns>True if transition is allowed, false otherwise</returns>
    Task<bool> IsTransitionAllowedAsync(
        Guid categoryId,
        Guid fromStatusId,
        Guid toStatusId,
        Guid userId);

    /// <summary>
    /// Get all statuses available in a category's workflow
    /// Returns global statuses if no custom workflow is configured
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <returns>List of available statuses for the category</returns>
    Task<List<ComplaintStatusMaster>> GetWorkflowStatusesAsync(Guid categoryId);

    /// <summary>
    /// Transition a complaint to a new status with validation
    /// Validates workflow rules, user permissions, and required fields
    /// </summary>
    /// <param name="complaintId">Complaint ID</param>
    /// <param name="newStatusId">Target status ID</param>
    /// <param name="userId">User ID performing the transition</param>
    /// <param name="comment">Optional comment (required for some transitions)</param>
    /// <returns>True if transition was successful, false otherwise</returns>
    Task<bool> TransitionComplaintAsync(
        Guid complaintId,
        Guid newStatusId,
        Guid userId,
        string? comment = null);

    /// <summary>
    /// Get all workflows configured in the system
    /// Optionally filtered by company for multi-tenant scenarios
    /// </summary>
    /// <param name="companyId">Company ID (null for all workflows)</param>
    /// <returns>List of workflows</returns>
    Task<List<CategoryWorkflow>> GetAllWorkflowsAsync(Guid? companyId = null);

    /// <summary>
    /// Create a new workflow for a category
    /// </summary>
    /// <param name="categoryId">Category ID</param>
    /// <param name="name">Workflow name</param>
    /// <param name="description">Workflow description</param>
    /// <param name="companyId">Company ID (for multi-tenant)</param>
    /// <returns>Created workflow</returns>
    Task<CategoryWorkflow> CreateWorkflowAsync(
        Guid categoryId,
        string name,
        string? description = null,
        Guid? companyId = null);

    /// <summary>
    /// Add a status to a workflow
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <param name="statusMasterId">Status master ID</param>
    /// <param name="displayOrder">Display order</param>
    /// <param name="isInitialStatus">Whether this is the initial status</param>
    /// <param name="defaultSLAHours">Default SLA hours for this status in this workflow</param>
    /// <returns>Created workflow status</returns>
    Task<CategoryWorkflowStatus> AddStatusToWorkflowAsync(
        Guid workflowId,
        Guid statusMasterId,
        int displayOrder,
        bool isInitialStatus = false,
        int? defaultSLAHours = null);

    /// <summary>
    /// Add a transition rule to a workflow
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <param name="fromStatusId">Source status ID</param>
    /// <param name="toStatusId">Target status ID</param>
    /// <param name="transitionName">Transition name/label</param>
    /// <param name="requiresComment">Whether comment is required</param>
    /// <param name="requiresApproval">Whether approval is required</param>
    /// <param name="allowedRoles">Roles allowed to perform this transition</param>
    /// <returns>Created transition</returns>
    Task<CategoryWorkflowTransition> AddTransitionRuleAsync(
        Guid workflowId,
        Guid fromStatusId,
        Guid toStatusId,
        string? transitionName = null,
        bool requiresComment = false,
        bool requiresApproval = false,
        Guid[]? allowedRoles = null);

    /// <summary>
    /// Check if a transition requires a comment
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <param name="fromStatusId">Source status ID</param>
    /// <param name="toStatusId">Target status ID</param>
    /// <returns>True if comment is required</returns>
    Task<bool> TransitionRequiresCommentAsync(
        Guid workflowId,
        Guid fromStatusId,
        Guid toStatusId);

    /// <summary>
    /// Check if a transition requires approval
    /// </summary>
    /// <param name="workflowId">Workflow ID</param>
    /// <param name="fromStatusId">Source status ID</param>
    /// <param name="toStatusId">Target status ID</param>
    /// <returns>True if approval is required</returns>
    Task<bool> TransitionRequiresApprovalAsync(
        Guid workflowId,
        Guid fromStatusId,
        Guid toStatusId);
}
