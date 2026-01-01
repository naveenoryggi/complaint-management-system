using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Enums.CRM;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing projects and their milestones, tasks, documents, and team members
/// </summary>
public class ProjectService : IProjectService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(ComplaintDbContext context, ILogger<ProjectService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Project CRUD

    public async Task<Result<PagedResult<ProjectSummaryDto>>> GetProjectsAsync(
        Guid companyId,
        string? searchTerm = null,
        ProjectStatus? status = null,
        Guid? customerId = null,
        Guid? managerId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectManager)
                .Where(p => p.CompanyId == companyId && !p.IsDeleted)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(p =>
                    p.ProjectCode.ToLower().Contains(search) ||
                    p.Name.ToLower().Contains(search) ||
                    p.Customer.Name.ToLower().Contains(search));
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.Status == status.Value);
            }

            if (customerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == customerId.Value);
            }

            if (managerId.HasValue)
            {
                query = query.Where(p => p.ProjectManagerId == managerId.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProjectSummaryDto
                {
                    Id = p.Id,
                    ProjectCode = p.ProjectCode,
                    Name = p.Name,
                    Status = p.Status,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.Name,
                    PlannedStartDate = p.PlannedStartDate,
                    PlannedEndDate = p.PlannedEndDate,
                    ProgressPercentage = p.ProgressPercentage,
                    Budget = p.Budget,
                    ActualCost = p.ActualCost,
                    ProjectManagerName = p.ProjectManager != null ? p.ProjectManager.FullName : null,
                    MilestoneCount = p.Milestones.Count(m => !m.IsDeleted),
                    TaskCount = p.Tasks.Count(t => !t.IsDeleted),
                    TeamMemberCount = p.TeamMembers.Count(tm => !tm.IsDeleted && tm.IsActive)
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<ProjectSummaryDto>(items, totalCount, page, pageSize);
            return Result<PagedResult<ProjectSummaryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for company {CompanyId}", companyId);
            return Result<PagedResult<ProjectSummaryDto>>.Failure("Error retrieving projects", ex.Message);
        }
    }

    public async Task<Result<ProjectDto>> GetProjectByIdAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectManager)
                .Include(p => p.Milestones.Where(m => !m.IsDeleted))
                .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                    .ThenInclude(t => t.Assignee)
                .Include(p => p.Documents.Where(d => !d.IsDeleted))
                .Include(p => p.TeamMembers.Where(tm => !tm.IsDeleted))
                    .ThenInclude(tm => tm.Employee)
                .Where(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
                return Result<ProjectDto>.Failure("Project not found", "NOT_FOUND");

            return Result<ProjectDto>.Success(MapToDto(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", projectId);
            return Result<ProjectDto>.Failure("Error retrieving project", ex.Message);
        }
    }

    public async Task<Result<ProjectDto>> GetProjectByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectManager)
                .Where(p => p.ProjectCode == code && p.CompanyId == companyId && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
                return Result<ProjectDto>.Failure("Project not found", "NOT_FOUND");

            return Result<ProjectDto>.Success(MapToDto(project));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project by code {Code}", code);
            return Result<ProjectDto>.Failure("Error retrieving project", ex.Message);
        }
    }

    public async Task<Result<ProjectDto>> CreateProjectAsync(Guid companyId, CreateProjectRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate code
            var existingCode = await _context.Projects
                .AnyAsync(p => p.ProjectCode == request.ProjectCode && p.CompanyId == companyId && !p.IsDeleted, cancellationToken);

            if (existingCode)
                return Result<ProjectDto>.Failure("Project code already exists", "DUPLICATE_CODE");

            // Verify customer exists
            var customerExists = await _context.Customers
                .AnyAsync(c => c.Id == request.CustomerId && c.CompanyId == companyId && !c.IsDeleted, cancellationToken);

            if (!customerExists)
                return Result<ProjectDto>.Failure("Customer not found", "CUSTOMER_NOT_FOUND");

            var project = new Project
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ProjectCode = request.ProjectCode,
                Name = request.Name,
                Description = request.Description,
                Status = request.Status,
                CustomerId = request.CustomerId,
                PlannedStartDate = request.PlannedStartDate,
                PlannedEndDate = request.PlannedEndDate,
                Budget = request.Budget,
                Currency = request.Currency,
                ProjectManagerId = request.ProjectManagerId,
                Priority = request.Priority,
                Tags = request.Tags,
                Notes = request.Notes,
                CustomFields = request.CustomFields,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetProjectByIdAsync(companyId, project.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return Result<ProjectDto>.Failure("Error creating project", ex.Message);
        }
    }

    public async Task<Result<ProjectDto>> UpdateProjectAsync(Guid companyId, Guid projectId, UpdateProjectRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _context.Projects
                .Where(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
                return Result<ProjectDto>.Failure("Project not found", "NOT_FOUND");

            project.Name = request.Name;
            project.Description = request.Description;
            project.Status = request.Status;
            project.PlannedStartDate = request.PlannedStartDate;
            project.PlannedEndDate = request.PlannedEndDate;
            project.ActualStartDate = request.ActualStartDate;
            project.ActualEndDate = request.ActualEndDate;
            project.ProgressPercentage = request.ProgressPercentage;
            project.Budget = request.Budget;
            project.ActualCost = request.ActualCost;
            project.Currency = request.Currency;
            project.ProjectManagerId = request.ProjectManagerId;
            project.Priority = request.Priority;
            project.Tags = request.Tags;
            project.Notes = request.Notes;
            project.CustomFields = request.CustomFields;
            project.UpdatedBy = updatedBy;
            project.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetProjectByIdAsync(companyId, projectId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", projectId);
            return Result<ProjectDto>.Failure("Error updating project", ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteProjectAsync(Guid companyId, Guid projectId, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _context.Projects
                .Where(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
                return Result<bool>.Failure("Project not found", "NOT_FOUND");

            project.IsDeleted = true;
            project.DeletedBy = deletedBy;
            project.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", projectId);
            return Result<bool>.Failure("Error deleting project", ex.Message);
        }
    }

    public async Task<Result<List<ProjectLookupDto>>> GetProjectLookupAsync(Guid companyId, Guid? customerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Projects
                .Include(p => p.Customer)
                .Where(p => p.CompanyId == companyId && !p.IsDeleted && p.Status != ProjectStatus.Cancelled)
                .AsQueryable();

            if (customerId.HasValue)
            {
                query = query.Where(p => p.CustomerId == customerId.Value);
            }

            var projects = await query
                .OrderBy(p => p.Name)
                .Select(p => new ProjectLookupDto
                {
                    Id = p.Id,
                    ProjectCode = p.ProjectCode,
                    Name = p.Name,
                    Status = p.Status,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.Name
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProjectLookupDto>>.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project lookup");
            return Result<List<ProjectLookupDto>>.Failure("Error retrieving projects", ex.Message);
        }
    }

    public async Task<Result<List<ProjectSummaryDto>>> GetProjectsByCustomerAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projects = await _context.Projects
                .Include(p => p.Customer)
                .Include(p => p.ProjectManager)
                .Where(p => p.CompanyId == companyId && p.CustomerId == customerId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new ProjectSummaryDto
                {
                    Id = p.Id,
                    ProjectCode = p.ProjectCode,
                    Name = p.Name,
                    Status = p.Status,
                    CustomerId = p.CustomerId,
                    CustomerName = p.Customer.Name,
                    PlannedStartDate = p.PlannedStartDate,
                    PlannedEndDate = p.PlannedEndDate,
                    ProgressPercentage = p.ProgressPercentage,
                    Budget = p.Budget,
                    ActualCost = p.ActualCost,
                    ProjectManagerName = p.ProjectManager != null ? p.ProjectManager.FullName : null,
                    MilestoneCount = p.Milestones.Count(m => !m.IsDeleted),
                    TaskCount = p.Tasks.Count(t => !t.IsDeleted),
                    TeamMemberCount = p.TeamMembers.Count(tm => !tm.IsDeleted && tm.IsActive)
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProjectSummaryDto>>.Success(projects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for customer {CustomerId}", customerId);
            return Result<List<ProjectSummaryDto>>.Failure("Error retrieving projects", ex.Message);
        }
    }

    #endregion

    #region Milestones

    public async Task<Result<List<ProjectMilestoneDto>>> GetMilestonesAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var milestones = await _context.ProjectMilestones
                .Include(m => m.Tasks.Where(t => !t.IsDeleted))
                .Where(m => m.ProjectId == projectId && m.Project.CompanyId == companyId && !m.IsDeleted)
                .OrderBy(m => m.SortOrder)
                .ThenBy(m => m.DueDate)
                .Select(m => new ProjectMilestoneDto
                {
                    Id = m.Id,
                    ProjectId = m.ProjectId,
                    Name = m.Name,
                    Description = m.Description,
                    Status = m.Status,
                    DueDate = m.DueDate,
                    CompletedDate = m.CompletedDate,
                    SortOrder = m.SortOrder,
                    Notes = m.Notes,
                    TaskCount = m.Tasks.Count(t => !t.IsDeleted),
                    CompletedTaskCount = m.Tasks.Count(t => !t.IsDeleted && t.Status == ProjectTaskStatus.Completed),
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProjectMilestoneDto>>.Success(milestones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting milestones for project {ProjectId}", projectId);
            return Result<List<ProjectMilestoneDto>>.Failure("Error retrieving milestones", ex.Message);
        }
    }

    public async Task<Result<ProjectMilestoneDto>> GetMilestoneByIdAsync(Guid companyId, Guid milestoneId, CancellationToken cancellationToken = default)
    {
        try
        {
            var milestone = await _context.ProjectMilestones
                .Include(m => m.Project)
                .Include(m => m.Tasks.Where(t => !t.IsDeleted))
                .Where(m => m.Id == milestoneId && m.Project.CompanyId == companyId && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (milestone == null)
                return Result<ProjectMilestoneDto>.Failure("Milestone not found", "NOT_FOUND");

            return Result<ProjectMilestoneDto>.Success(MapMilestoneToDto(milestone));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting milestone {MilestoneId}", milestoneId);
            return Result<ProjectMilestoneDto>.Failure("Error retrieving milestone", ex.Message);
        }
    }

    public async Task<Result<ProjectMilestoneDto>> CreateMilestoneAsync(Guid companyId, Guid projectId, CreateMilestoneRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted, cancellationToken);

            if (!projectExists)
                return Result<ProjectMilestoneDto>.Failure("Project not found", "PROJECT_NOT_FOUND");

            var milestone = new ProjectMilestone
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                Name = request.Name,
                Description = request.Description,
                DueDate = request.DueDate,
                SortOrder = request.SortOrder,
                Notes = request.Notes,
                Status = MilestoneStatus.Pending,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectMilestones.Add(milestone);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetMilestoneByIdAsync(companyId, milestone.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating milestone");
            return Result<ProjectMilestoneDto>.Failure("Error creating milestone", ex.Message);
        }
    }

    public async Task<Result<ProjectMilestoneDto>> UpdateMilestoneAsync(Guid companyId, Guid milestoneId, UpdateMilestoneRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var milestone = await _context.ProjectMilestones
                .Include(m => m.Project)
                .Where(m => m.Id == milestoneId && m.Project.CompanyId == companyId && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (milestone == null)
                return Result<ProjectMilestoneDto>.Failure("Milestone not found", "NOT_FOUND");

            milestone.Name = request.Name;
            milestone.Description = request.Description;
            milestone.Status = request.Status;
            milestone.DueDate = request.DueDate;
            milestone.CompletedDate = request.CompletedDate;
            milestone.SortOrder = request.SortOrder;
            milestone.Notes = request.Notes;
            milestone.UpdatedBy = updatedBy;
            milestone.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetMilestoneByIdAsync(companyId, milestoneId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating milestone {MilestoneId}", milestoneId);
            return Result<ProjectMilestoneDto>.Failure("Error updating milestone", ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteMilestoneAsync(Guid companyId, Guid milestoneId, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var milestone = await _context.ProjectMilestones
                .Include(m => m.Project)
                .Where(m => m.Id == milestoneId && m.Project.CompanyId == companyId && !m.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (milestone == null)
                return Result<bool>.Failure("Milestone not found", "NOT_FOUND");

            milestone.IsDeleted = true;
            milestone.DeletedBy = deletedBy;
            milestone.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting milestone {MilestoneId}", milestoneId);
            return Result<bool>.Failure("Error deleting milestone", ex.Message);
        }
    }

    #endregion

    #region Tasks

    public async Task<Result<List<ProjectTaskDto>>> GetTasksAsync(Guid companyId, Guid projectId, Guid? milestoneId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ProjectTasks
                .Include(t => t.Assignee)
                .Include(t => t.Milestone)
                .Where(t => t.ProjectId == projectId && t.Project.CompanyId == companyId && !t.IsDeleted);

            if (milestoneId.HasValue)
            {
                query = query.Where(t => t.MilestoneId == milestoneId.Value);
            }

            var tasks = await query
                .OrderBy(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Select(t => new ProjectTaskDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    MilestoneId = t.MilestoneId,
                    MilestoneName = t.Milestone != null ? t.Milestone.Name : null,
                    Title = t.Title,
                    Description = t.Description,
                    Status = t.Status,
                    Priority = t.Priority,
                    AssigneeId = t.AssigneeId,
                    AssigneeName = t.Assignee != null ? t.Assignee.FullName : null,
                    DueDate = t.DueDate,
                    CompletedDate = t.CompletedDate,
                    EstimatedHours = t.EstimatedHours,
                    ActualHours = t.ActualHours,
                    Tags = t.Tags,
                    Notes = t.Notes,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProjectTaskDto>>.Success(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for project {ProjectId}", projectId);
            return Result<List<ProjectTaskDto>>.Failure("Error retrieving tasks", ex.Message);
        }
    }

    public async Task<Result<ProjectTaskDto>> GetTaskByIdAsync(Guid companyId, Guid taskId, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Include(t => t.Assignee)
                .Include(t => t.Milestone)
                .Where(t => t.Id == taskId && t.Project.CompanyId == companyId && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                return Result<ProjectTaskDto>.Failure("Task not found", "NOT_FOUND");

            return Result<ProjectTaskDto>.Success(MapTaskToDto(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting task {TaskId}", taskId);
            return Result<ProjectTaskDto>.Failure("Error retrieving task", ex.Message);
        }
    }

    public async Task<Result<ProjectTaskDto>> CreateTaskAsync(Guid companyId, Guid projectId, CreateTaskRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted, cancellationToken);

            if (!projectExists)
                return Result<ProjectTaskDto>.Failure("Project not found", "PROJECT_NOT_FOUND");

            var task = new ProjectTask
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                MilestoneId = request.MilestoneId,
                Title = request.Title,
                Description = request.Description,
                Priority = request.Priority,
                AssigneeId = request.AssigneeId,
                DueDate = request.DueDate,
                EstimatedHours = request.EstimatedHours,
                Tags = request.Tags,
                Notes = request.Notes,
                Status = ProjectTaskStatus.ToDo,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTasks.Add(task);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetTaskByIdAsync(companyId, task.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return Result<ProjectTaskDto>.Failure("Error creating task", ex.Message);
        }
    }

    public async Task<Result<ProjectTaskDto>> UpdateTaskAsync(Guid companyId, Guid taskId, UpdateTaskRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Where(t => t.Id == taskId && t.Project.CompanyId == companyId && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                return Result<ProjectTaskDto>.Failure("Task not found", "NOT_FOUND");

            task.MilestoneId = request.MilestoneId;
            task.Title = request.Title;
            task.Description = request.Description;
            task.Status = request.Status;
            task.Priority = request.Priority;
            task.AssigneeId = request.AssigneeId;
            task.DueDate = request.DueDate;
            task.CompletedDate = request.CompletedDate;
            task.EstimatedHours = request.EstimatedHours;
            task.ActualHours = request.ActualHours;
            task.Tags = request.Tags;
            task.Notes = request.Notes;
            task.UpdatedBy = updatedBy;
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetTaskByIdAsync(companyId, taskId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", taskId);
            return Result<ProjectTaskDto>.Failure("Error updating task", ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteTaskAsync(Guid companyId, Guid taskId, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var task = await _context.ProjectTasks
                .Include(t => t.Project)
                .Where(t => t.Id == taskId && t.Project.CompanyId == companyId && !t.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (task == null)
                return Result<bool>.Failure("Task not found", "NOT_FOUND");

            task.IsDeleted = true;
            task.DeletedBy = deletedBy;
            task.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
            return Result<bool>.Failure("Error deleting task", ex.Message);
        }
    }

    #endregion

    #region Documents

    public async Task<Result<List<ProjectDocumentDto>>> GetDocumentsAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = await _context.ProjectDocuments
                .Where(d => d.ProjectId == projectId && d.Project.CompanyId == companyId && !d.IsDeleted)
                .OrderByDescending(d => d.UploadedAt)
                .Select(d => new ProjectDocumentDto
                {
                    Id = d.Id,
                    ProjectId = d.ProjectId,
                    FileName = d.FileName,
                    FilePath = d.FilePath,
                    FileType = d.FileType,
                    FileSize = d.FileSize,
                    Category = d.Category,
                    Description = d.Description,
                    UploadedById = d.UploadedById,
                    UploadedAt = d.UploadedAt
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProjectDocumentDto>>.Success(documents);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for project {ProjectId}", projectId);
            return Result<List<ProjectDocumentDto>>.Failure("Error retrieving documents", ex.Message);
        }
    }

    public async Task<Result<ProjectDocumentDto>> GetDocumentByIdAsync(Guid companyId, Guid documentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _context.ProjectDocuments
                .Include(d => d.Project)
                .Where(d => d.Id == documentId && d.Project.CompanyId == companyId && !d.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (document == null)
                return Result<ProjectDocumentDto>.Failure("Document not found", "NOT_FOUND");

            return Result<ProjectDocumentDto>.Success(MapDocumentToDto(document));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting document {DocumentId}", documentId);
            return Result<ProjectDocumentDto>.Failure("Error retrieving document", ex.Message);
        }
    }

    public async Task<Result<ProjectDocumentDto>> UploadDocumentAsync(Guid companyId, Guid projectId, UploadDocumentRequest request, Guid uploadedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted, cancellationToken);

            if (!projectExists)
                return Result<ProjectDocumentDto>.Failure("Project not found", "PROJECT_NOT_FOUND");

            var document = new ProjectDocument
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                FileName = request.FileName,
                FilePath = request.FilePath,
                FileType = request.FileType,
                FileSize = request.FileSize,
                Category = request.Category,
                Description = request.Description,
                UploadedById = uploadedBy,
                UploadedAt = DateTime.UtcNow,
                CreatedBy = uploadedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectDocuments.Add(document);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetDocumentByIdAsync(companyId, document.Id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return Result<ProjectDocumentDto>.Failure("Error uploading document", ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteDocumentAsync(Guid companyId, Guid documentId, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var document = await _context.ProjectDocuments
                .Include(d => d.Project)
                .Where(d => d.Id == documentId && d.Project.CompanyId == companyId && !d.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (document == null)
                return Result<bool>.Failure("Document not found", "NOT_FOUND");

            document.IsDeleted = true;
            document.DeletedBy = deletedBy;
            document.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return Result<bool>.Failure("Error deleting document", ex.Message);
        }
    }

    #endregion

    #region Team Members

    public async Task<Result<List<ProjectTeamMemberDto>>> GetTeamMembersAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await _context.ProjectTeamMembers
                .Include(tm => tm.Employee)
                .Where(tm => tm.ProjectId == projectId && tm.Project.CompanyId == companyId && !tm.IsDeleted)
                .OrderBy(tm => tm.JoinedDate)
                .Select(tm => new ProjectTeamMemberDto
                {
                    Id = tm.Id,
                    ProjectId = tm.ProjectId,
                    EmployeeId = tm.EmployeeId,
                    EmployeeCode = tm.Employee.EmployeeCode,
                    EmployeeName = tm.Employee.FullName,
                    Role = tm.Role,
                    AllocationPercentage = tm.AllocationPercentage,
                    JoinedDate = tm.JoinedDate,
                    LeftDate = tm.LeftDate,
                    IsActive = tm.IsActive,
                    Notes = tm.Notes
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProjectTeamMemberDto>>.Success(members);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members for project {ProjectId}", projectId);
            return Result<List<ProjectTeamMemberDto>>.Failure("Error retrieving team members", ex.Message);
        }
    }

    public async Task<Result<ProjectTeamMemberDto>> AddTeamMemberAsync(Guid companyId, Guid projectId, AddTeamMemberRequest request, Guid addedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectExists = await _context.Projects
                .AnyAsync(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted, cancellationToken);

            if (!projectExists)
                return Result<ProjectTeamMemberDto>.Failure("Project not found", "PROJECT_NOT_FOUND");

            // Check if member already exists
            var existingMember = await _context.ProjectTeamMembers
                .AnyAsync(tm => tm.ProjectId == projectId && tm.EmployeeId == request.EmployeeId && !tm.IsDeleted, cancellationToken);

            if (existingMember)
                return Result<ProjectTeamMemberDto>.Failure("Team member already exists in project", "DUPLICATE_MEMBER");

            var member = new ProjectTeamMember
            {
                Id = Guid.NewGuid(),
                ProjectId = projectId,
                EmployeeId = request.EmployeeId,
                Role = request.Role,
                AllocationPercentage = request.AllocationPercentage,
                JoinedDate = request.JoinedDate ?? DateTime.UtcNow,
                Notes = request.Notes,
                IsActive = true,
                CreatedBy = addedBy,
                CreatedAt = DateTime.UtcNow
            };

            _context.ProjectTeamMembers.Add(member);
            await _context.SaveChangesAsync(cancellationToken);

            // Fetch the complete member with employee details
            var result = await _context.ProjectTeamMembers
                .Include(tm => tm.Employee)
                .Where(tm => tm.Id == member.Id)
                .Select(tm => new ProjectTeamMemberDto
                {
                    Id = tm.Id,
                    ProjectId = tm.ProjectId,
                    EmployeeId = tm.EmployeeId,
                    EmployeeCode = tm.Employee.EmployeeCode,
                    EmployeeName = tm.Employee.FullName,
                    Role = tm.Role,
                    AllocationPercentage = tm.AllocationPercentage,
                    JoinedDate = tm.JoinedDate,
                    LeftDate = tm.LeftDate,
                    IsActive = tm.IsActive,
                    Notes = tm.Notes
                })
                .FirstOrDefaultAsync(cancellationToken);

            return Result<ProjectTeamMemberDto>.Success(result!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team member");
            return Result<ProjectTeamMemberDto>.Failure("Error adding team member", ex.Message);
        }
    }

    public async Task<Result<ProjectTeamMemberDto>> UpdateTeamMemberAsync(Guid companyId, Guid teamMemberId, UpdateTeamMemberRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _context.ProjectTeamMembers
                .Include(tm => tm.Project)
                .Include(tm => tm.Employee)
                .Where(tm => tm.Id == teamMemberId && tm.Project.CompanyId == companyId && !tm.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (member == null)
                return Result<ProjectTeamMemberDto>.Failure("Team member not found", "NOT_FOUND");

            member.Role = request.Role;
            member.AllocationPercentage = request.AllocationPercentage;
            member.IsActive = request.IsActive;
            member.LeftDate = request.LeftDate;
            member.Notes = request.Notes;
            member.UpdatedBy = updatedBy;
            member.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<ProjectTeamMemberDto>.Success(new ProjectTeamMemberDto
            {
                Id = member.Id,
                ProjectId = member.ProjectId,
                EmployeeId = member.EmployeeId,
                EmployeeCode = member.Employee.EmployeeCode,
                EmployeeName = member.Employee.FullName,
                Role = member.Role,
                AllocationPercentage = member.AllocationPercentage,
                JoinedDate = member.JoinedDate,
                LeftDate = member.LeftDate,
                IsActive = member.IsActive,
                Notes = member.Notes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team member {TeamMemberId}", teamMemberId);
            return Result<ProjectTeamMemberDto>.Failure("Error updating team member", ex.Message);
        }
    }

    public async Task<Result<bool>> RemoveTeamMemberAsync(Guid companyId, Guid teamMemberId, Guid removedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var member = await _context.ProjectTeamMembers
                .Include(tm => tm.Project)
                .Where(tm => tm.Id == teamMemberId && tm.Project.CompanyId == companyId && !tm.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (member == null)
                return Result<bool>.Failure("Team member not found", "NOT_FOUND");

            member.IsDeleted = true;
            member.DeletedBy = removedBy;
            member.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team member {TeamMemberId}", teamMemberId);
            return Result<bool>.Failure("Error removing team member", ex.Message);
        }
    }

    #endregion

    #region Statistics

    public async Task<Result<ProjectStatisticsDto>> GetProjectStatisticsAsync(Guid companyId, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var project = await _context.Projects
                .Include(p => p.Milestones.Where(m => !m.IsDeleted))
                .Include(p => p.Tasks.Where(t => !t.IsDeleted))
                .Include(p => p.TeamMembers.Where(tm => !tm.IsDeleted))
                .Include(p => p.Documents.Where(d => !d.IsDeleted))
                .Include(p => p.Complaints.Where(c => !c.IsDeleted))
                .Where(p => p.Id == projectId && p.CompanyId == companyId && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (project == null)
                return Result<ProjectStatisticsDto>.Failure("Project not found", "NOT_FOUND");

            var stats = new ProjectStatisticsDto
            {
                TotalMilestones = project.Milestones.Count,
                CompletedMilestones = project.Milestones.Count(m => m.Status == MilestoneStatus.Completed),
                DelayedMilestones = project.Milestones.Count(m => m.Status == MilestoneStatus.Delayed),
                TotalTasks = project.Tasks.Count,
                CompletedTasks = project.Tasks.Count(t => t.Status == ProjectTaskStatus.Completed),
                InProgressTasks = project.Tasks.Count(t => t.Status == ProjectTaskStatus.InProgress),
                BlockedTasks = project.Tasks.Count(t => t.Status == ProjectTaskStatus.Blocked),
                TotalTeamMembers = project.TeamMembers.Count,
                ActiveTeamMembers = project.TeamMembers.Count(tm => tm.IsActive),
                TotalDocuments = project.Documents.Count,
                TotalEstimatedHours = project.Tasks.Sum(t => t.EstimatedHours ?? 0),
                TotalActualHours = project.Tasks.Sum(t => t.ActualHours ?? 0),
                RelatedTickets = project.Complaints.Count
            };

            return Result<ProjectStatisticsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for project {ProjectId}", projectId);
            return Result<ProjectStatisticsDto>.Failure("Error retrieving statistics", ex.Message);
        }
    }

    #endregion

    #region Private Mapping Methods

    private ProjectDto MapToDto(Project project)
    {
        return new ProjectDto
        {
            Id = project.Id,
            CompanyId = project.CompanyId,
            ProjectCode = project.ProjectCode,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            CustomerId = project.CustomerId,
            CustomerCode = project.Customer?.Code ?? string.Empty,
            CustomerName = project.Customer?.Name ?? string.Empty,
            PlannedStartDate = project.PlannedStartDate,
            PlannedEndDate = project.PlannedEndDate,
            ActualStartDate = project.ActualStartDate,
            ActualEndDate = project.ActualEndDate,
            ProgressPercentage = project.ProgressPercentage,
            Budget = project.Budget,
            ActualCost = project.ActualCost,
            Currency = project.Currency,
            ProjectManagerId = project.ProjectManagerId,
            ProjectManagerName = project.ProjectManager?.FullName,
            Priority = project.Priority,
            Tags = project.Tags,
            Notes = project.Notes,
            CustomFields = project.CustomFields,
            MilestoneCount = project.Milestones?.Count ?? 0,
            CompletedMilestoneCount = project.Milestones?.Count(m => m.Status == MilestoneStatus.Completed) ?? 0,
            TaskCount = project.Tasks?.Count ?? 0,
            CompletedTaskCount = project.Tasks?.Count(t => t.Status == ProjectTaskStatus.Completed) ?? 0,
            DocumentCount = project.Documents?.Count ?? 0,
            TeamMemberCount = project.TeamMembers?.Count(tm => tm.IsActive) ?? 0,
            TicketCount = project.Complaints?.Count ?? 0,
            Milestones = project.Milestones?.OrderBy(m => m.SortOrder).ThenBy(m => m.DueDate).Select(MapMilestoneToDto).ToList() ?? new List<ProjectMilestoneDto>(),
            Tasks = project.Tasks?.OrderBy(t => t.Priority).ThenBy(t => t.DueDate).Select(MapTaskToDto).ToList() ?? new List<ProjectTaskDto>(),
            Documents = project.Documents?.OrderByDescending(d => d.UploadedAt).Select(MapDocumentToDto).ToList() ?? new List<ProjectDocumentDto>(),
            TeamMembers = project.TeamMembers?.OrderBy(tm => tm.JoinedDate).Select(MapTeamMemberToDto).ToList() ?? new List<ProjectTeamMemberDto>(),
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };
    }

    private ProjectMilestoneDto MapMilestoneToDto(ProjectMilestone milestone)
    {
        return new ProjectMilestoneDto
        {
            Id = milestone.Id,
            ProjectId = milestone.ProjectId,
            Name = milestone.Name,
            Description = milestone.Description,
            Status = milestone.Status,
            DueDate = milestone.DueDate,
            CompletedDate = milestone.CompletedDate,
            SortOrder = milestone.SortOrder,
            Notes = milestone.Notes,
            TaskCount = milestone.Tasks?.Count ?? 0,
            CompletedTaskCount = milestone.Tasks?.Count(t => t.Status == ProjectTaskStatus.Completed) ?? 0,
            CreatedAt = milestone.CreatedAt
        };
    }

    private ProjectTaskDto MapTaskToDto(ProjectTask task)
    {
        return new ProjectTaskDto
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            MilestoneId = task.MilestoneId,
            MilestoneName = task.Milestone?.Name,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            AssigneeId = task.AssigneeId,
            AssigneeName = task.Assignee?.FullName,
            DueDate = task.DueDate,
            CompletedDate = task.CompletedDate,
            EstimatedHours = task.EstimatedHours,
            ActualHours = task.ActualHours,
            Tags = task.Tags,
            Notes = task.Notes,
            CreatedAt = task.CreatedAt
        };
    }

    private ProjectDocumentDto MapDocumentToDto(ProjectDocument document)
    {
        return new ProjectDocumentDto
        {
            Id = document.Id,
            ProjectId = document.ProjectId,
            FileName = document.FileName,
            FilePath = document.FilePath,
            FileType = document.FileType,
            FileSize = document.FileSize,
            Category = document.Category,
            Description = document.Description,
            UploadedById = document.UploadedById,
            UploadedAt = document.UploadedAt
        };
    }

    private ProjectTeamMemberDto MapTeamMemberToDto(ProjectTeamMember member)
    {
        return new ProjectTeamMemberDto
        {
            Id = member.Id,
            ProjectId = member.ProjectId,
            EmployeeId = member.EmployeeId,
            EmployeeCode = member.Employee?.EmployeeCode ?? string.Empty,
            EmployeeName = member.Employee?.FullName ?? string.Empty,
            Role = member.Role,
            AllocationPercentage = member.AllocationPercentage,
            JoinedDate = member.JoinedDate,
            LeftDate = member.LeftDate,
            IsActive = member.IsActive,
            Notes = member.Notes
        };
    }

    #endregion
}
