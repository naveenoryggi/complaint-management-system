using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.API.Authorization;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Enums.CRM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.CRM;

/// <summary>
/// Controller for managing projects, their milestones, tasks, documents, and team members
/// Requires CRM license module
/// </summary>
[ApiController]
[Route("api/projects")]
[Authorize]
[RequiresLicense(LicenseModule.CRM_Project)]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly ILogger<ProjectsController> _logger;

    public ProjectsController(IProjectService projectService, ILogger<ProjectsController> logger)
    {
        _projectService = projectService;
        _logger = logger;
    }

    #region Project CRUD

    /// <summary>
    /// Get all projects with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<PagedResult<ProjectSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjects(
        [FromQuery] string? searchTerm = null,
        [FromQuery] ProjectStatus? status = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? managerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetProjectsAsync(
                companyId.Value, searchTerm, status, customerId, managerId, page, pageSize, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects");
            return StatusCode(500, new { message = "Error retrieving projects" });
        }
    }

    /// <summary>
    /// Get a project by ID with full details
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetProjectByIdAsync(companyId.Value, id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project {ProjectId}", id);
            return StatusCode(500, new { message = "Error retrieving project" });
        }
    }

    /// <summary>
    /// Get a project by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProjectByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetProjectByCodeAsync(companyId.Value, code, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project by code {Code}", code);
            return StatusCode(500, new { message = "Error retrieving project" });
        }
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.CreateProjectAsync(companyId.Value, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return CreatedAtAction(nameof(GetProject), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating project");
            return StatusCode(500, new { message = "Error creating project" });
        }
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.UpdateProjectAsync(companyId.Value, id, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating project {ProjectId}", id);
            return StatusCode(500, new { message = "Error updating project" });
        }
    }

    /// <summary>
    /// Delete a project (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProject(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.DeleteProjectAsync(companyId.Value, id, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting project {ProjectId}", id);
            return StatusCode(500, new { message = "Error deleting project" });
        }
    }

    /// <summary>
    /// Get project lookup list for dropdowns
    /// </summary>
    [HttpGet("lookup")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<List<ProjectLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectLookup(
        [FromQuery] Guid? customerId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetProjectLookupAsync(companyId.Value, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting project lookup");
            return StatusCode(500, new { message = "Error retrieving projects" });
        }
    }

    /// <summary>
    /// Get projects by customer
    /// </summary>
    [HttpGet("by-customer/{customerId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<List<ProjectSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProjectsByCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetProjectsByCustomerAsync(companyId.Value, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting projects for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving projects" });
        }
    }

    #endregion

    #region Milestones

    /// <summary>
    /// Get all milestones for a project
    /// </summary>
    [HttpGet("{projectId:guid}/milestones")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<List<ProjectMilestoneDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMilestones(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetMilestonesAsync(companyId.Value, projectId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting milestones for project {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error retrieving milestones" });
        }
    }

    /// <summary>
    /// Create a milestone
    /// </summary>
    [HttpPost("{projectId:guid}/milestones")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectMilestoneDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateMilestone(Guid projectId, [FromBody] CreateMilestoneRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.CreateMilestoneAsync(companyId.Value, projectId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Created($"api/projects/{projectId}/milestones/{result.Data!.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating milestone");
            return StatusCode(500, new { message = "Error creating milestone" });
        }
    }

    /// <summary>
    /// Update a milestone
    /// </summary>
    [HttpPut("milestones/{milestoneId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectMilestoneDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateMilestone(Guid milestoneId, [FromBody] UpdateMilestoneRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.UpdateMilestoneAsync(companyId.Value, milestoneId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating milestone {MilestoneId}", milestoneId);
            return StatusCode(500, new { message = "Error updating milestone" });
        }
    }

    /// <summary>
    /// Delete a milestone
    /// </summary>
    [HttpDelete("milestones/{milestoneId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteMilestone(Guid milestoneId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.DeleteMilestoneAsync(companyId.Value, milestoneId, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting milestone {MilestoneId}", milestoneId);
            return StatusCode(500, new { message = "Error deleting milestone" });
        }
    }

    #endregion

    #region Tasks

    /// <summary>
    /// Get all tasks for a project
    /// </summary>
    [HttpGet("{projectId:guid}/tasks")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<List<ProjectTaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks(Guid projectId, [FromQuery] Guid? milestoneId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetTasksAsync(companyId.Value, projectId, milestoneId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tasks for project {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error retrieving tasks" });
        }
    }

    /// <summary>
    /// Create a task
    /// </summary>
    [HttpPost("{projectId:guid}/tasks")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectTaskDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask(Guid projectId, [FromBody] CreateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.CreateTaskAsync(companyId.Value, projectId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Created($"api/projects/{projectId}/tasks/{result.Data!.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating task");
            return StatusCode(500, new { message = "Error creating task" });
        }
    }

    /// <summary>
    /// Update a task
    /// </summary>
    [HttpPut("tasks/{taskId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectTaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTask(Guid taskId, [FromBody] UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.UpdateTaskAsync(companyId.Value, taskId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating task {TaskId}", taskId);
            return StatusCode(500, new { message = "Error updating task" });
        }
    }

    /// <summary>
    /// Delete a task
    /// </summary>
    [HttpDelete("tasks/{taskId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTask(Guid taskId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.DeleteTaskAsync(companyId.Value, taskId, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting task {TaskId}", taskId);
            return StatusCode(500, new { message = "Error deleting task" });
        }
    }

    #endregion

    #region Documents

    /// <summary>
    /// Get all documents for a project
    /// </summary>
    [HttpGet("{projectId:guid}/documents")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<List<ProjectDocumentDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetDocumentsAsync(companyId.Value, projectId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents for project {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error retrieving documents" });
        }
    }

    /// <summary>
    /// Upload a document
    /// </summary>
    [HttpPost("{projectId:guid}/documents")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectDocumentDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadDocument(Guid projectId, [FromBody] UploadDocumentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.UploadDocumentAsync(companyId.Value, projectId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Created($"api/projects/{projectId}/documents/{result.Data!.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, new { message = "Error uploading document" });
        }
    }

    /// <summary>
    /// Delete a document
    /// </summary>
    [HttpDelete("documents/{documentId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteDocument(Guid documentId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.DeleteDocumentAsync(companyId.Value, documentId, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {DocumentId}", documentId);
            return StatusCode(500, new { message = "Error deleting document" });
        }
    }

    #endregion

    #region Team Members

    /// <summary>
    /// Get all team members for a project
    /// </summary>
    [HttpGet("{projectId:guid}/team")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<List<ProjectTeamMemberDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeamMembers(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetTeamMembersAsync(companyId.Value, projectId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team members for project {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error retrieving team members" });
        }
    }

    /// <summary>
    /// Add a team member
    /// </summary>
    [HttpPost("{projectId:guid}/team")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectTeamMemberDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddTeamMember(Guid projectId, [FromBody] AddTeamMemberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.AddTeamMemberAsync(companyId.Value, projectId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return BadRequest(result);

            return Created($"api/projects/{projectId}/team/{result.Data!.Id}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding team member");
            return StatusCode(500, new { message = "Error adding team member" });
        }
    }

    /// <summary>
    /// Update a team member
    /// </summary>
    [HttpPut("team/{memberId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectTeamMemberDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTeamMember(Guid memberId, [FromBody] UpdateTeamMemberRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.UpdateTeamMemberAsync(companyId.Value, memberId, request, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team member {MemberId}", memberId);
            return StatusCode(500, new { message = "Error updating team member" });
        }
    }

    /// <summary>
    /// Remove a team member
    /// </summary>
    [HttpDelete("team/{memberId:guid}")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> RemoveTeamMember(Guid memberId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userId = GetCurrentUserId();
            if (!userId.HasValue)
                return Unauthorized(new { message = "User context not found" });

            var result = await _projectService.RemoveTeamMemberAsync(companyId.Value, memberId, userId.Value, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team member {MemberId}", memberId);
            return StatusCode(500, new { message = "Error removing team member" });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get project statistics
    /// </summary>
    [HttpGet("{projectId:guid}/statistics")]
    [HasPermission("ManageProjects")]
    [ProducesResponseType(typeof(Result<ProjectStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(Guid projectId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _projectService.GetProjectStatisticsAsync(companyId.Value, projectId, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting statistics for project {ProjectId}", projectId);
            return StatusCode(500, new { message = "Error retrieving statistics" });
        }
    }

    #endregion

    #region Helper Methods

    private Guid? GetCurrentCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    #endregion
}
