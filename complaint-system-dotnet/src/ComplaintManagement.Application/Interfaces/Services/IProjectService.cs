using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;
using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for managing projects and their milestones, tasks, documents, and team members
/// </summary>
public interface IProjectService
{
    #region Project CRUD

    /// <summary>
    /// Gets all projects for a company with optional filtering
    /// </summary>
    Task<Result<PagedResult<ProjectSummaryDto>>> GetProjectsAsync(
        Guid companyId,
        string? searchTerm = null,
        ProjectStatus? status = null,
        Guid? customerId = null,
        Guid? managerId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by ID with full details
    /// </summary>
    Task<Result<ProjectDto>> GetProjectByIdAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a project by code
    /// </summary>
    Task<Result<ProjectDto>> GetProjectByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new project
    /// </summary>
    Task<Result<ProjectDto>> CreateProjectAsync(Guid companyId, CreateProjectRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing project
    /// </summary>
    Task<Result<ProjectDto>> UpdateProjectAsync(Guid companyId, Guid projectId, UpdateProjectRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a project
    /// </summary>
    Task<Result<bool>> DeleteProjectAsync(Guid companyId, Guid projectId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets project lookup list for dropdowns (filtered by customer)
    /// </summary>
    Task<Result<List<ProjectLookupDto>>> GetProjectLookupAsync(Guid companyId, Guid? customerId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets projects for a specific customer
    /// </summary>
    Task<Result<List<ProjectSummaryDto>>> GetProjectsByCustomerAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    #endregion

    #region Milestones

    /// <summary>
    /// Gets all milestones for a project
    /// </summary>
    Task<Result<List<ProjectMilestoneDto>>> GetMilestonesAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a milestone by ID
    /// </summary>
    Task<Result<ProjectMilestoneDto>> GetMilestoneByIdAsync(Guid companyId, Guid milestoneId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new milestone
    /// </summary>
    Task<Result<ProjectMilestoneDto>> CreateMilestoneAsync(Guid companyId, Guid projectId, CreateMilestoneRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing milestone
    /// </summary>
    Task<Result<ProjectMilestoneDto>> UpdateMilestoneAsync(Guid companyId, Guid milestoneId, UpdateMilestoneRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a milestone
    /// </summary>
    Task<Result<bool>> DeleteMilestoneAsync(Guid companyId, Guid milestoneId, Guid deletedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Tasks

    /// <summary>
    /// Gets all tasks for a project
    /// </summary>
    Task<Result<List<ProjectTaskDto>>> GetTasksAsync(Guid companyId, Guid projectId, Guid? milestoneId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a task by ID
    /// </summary>
    Task<Result<ProjectTaskDto>> GetTaskByIdAsync(Guid companyId, Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new task
    /// </summary>
    Task<Result<ProjectTaskDto>> CreateTaskAsync(Guid companyId, Guid projectId, CreateTaskRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing task
    /// </summary>
    Task<Result<ProjectTaskDto>> UpdateTaskAsync(Guid companyId, Guid taskId, UpdateTaskRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a task
    /// </summary>
    Task<Result<bool>> DeleteTaskAsync(Guid companyId, Guid taskId, Guid deletedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Documents

    /// <summary>
    /// Gets all documents for a project
    /// </summary>
    Task<Result<List<ProjectDocumentDto>>> GetDocumentsAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a document by ID
    /// </summary>
    Task<Result<ProjectDocumentDto>> GetDocumentByIdAsync(Guid companyId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a new document
    /// </summary>
    Task<Result<ProjectDocumentDto>> UploadDocumentAsync(Guid companyId, Guid projectId, UploadDocumentRequest request, Guid uploadedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a document
    /// </summary>
    Task<Result<bool>> DeleteDocumentAsync(Guid companyId, Guid documentId, Guid deletedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Team Members

    /// <summary>
    /// Gets all team members for a project
    /// </summary>
    Task<Result<List<ProjectTeamMemberDto>>> GetTeamMembersAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a team member to a project
    /// </summary>
    Task<Result<ProjectTeamMemberDto>> AddTeamMemberAsync(Guid companyId, Guid projectId, AddTeamMemberRequest request, Guid addedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a team member
    /// </summary>
    Task<Result<ProjectTeamMemberDto>> UpdateTeamMemberAsync(Guid companyId, Guid teamMemberId, UpdateTeamMemberRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a team member from a project
    /// </summary>
    Task<Result<bool>> RemoveTeamMemberAsync(Guid companyId, Guid teamMemberId, Guid removedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets project statistics
    /// </summary>
    Task<Result<ProjectStatisticsDto>> GetProjectStatisticsAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Project statistics DTO
/// </summary>
public class ProjectStatisticsDto
{
    public int TotalMilestones { get; set; }
    public int CompletedMilestones { get; set; }
    public int DelayedMilestones { get; set; }
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int BlockedTasks { get; set; }
    public int TotalTeamMembers { get; set; }
    public int ActiveTeamMembers { get; set; }
    public int TotalDocuments { get; set; }
    public int TotalEstimatedHours { get; set; }
    public int TotalActualHours { get; set; }
    public int RelatedTickets { get; set; }
}
