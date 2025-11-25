using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.DTOs.Workflows;
using ComplaintManagement.Application.Features.Complaints.Queries;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/workflows")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly IWorkflowEngine _workflowEngine;
    private readonly ILogger<WorkflowController> _logger;
    private readonly IMediator _mediator;

    public WorkflowController(
        IWorkflowEngine workflowEngine,
        ILogger<WorkflowController> logger,
        IMediator mediator)
    {
        _workflowEngine = workflowEngine;
        _logger = logger;
        _mediator = mediator;
    }

    #region Workflow Management

    /// <summary>
    /// Get all workflows for the company
    /// </summary>
    [HttpGet]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> GetAllWorkflows([FromQuery] Guid? companyId)
    {
        try
        {
            // If no companyId provided, get from token
            if (!companyId.HasValue)
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var tokenCompanyId))
                {
                    companyId = tokenCompanyId;
                }
            }

            var workflows = await _workflowEngine.GetAllWorkflowsAsync(companyId);

            var workflowDtos = workflows.Select(w => new CategoryWorkflowDto
            {
                Id = w.Id,
                CategoryId = w.CategoryId,
                CategoryName = w.Category?.Name ?? string.Empty,
                Name = w.Name,
                Description = w.Description,
                IsActive = w.IsActive,
                IsDefault = w.IsDefault,
                CompanyId = w.CompanyId,
                CompanyName = w.Company?.Name,
                WorkflowStatuses = w.WorkflowStatuses.Select(ws => new CategoryWorkflowStatusDto
                {
                    Id = ws.Id,
                    WorkflowId = ws.WorkflowId,
                    StatusMasterId = ws.StatusMasterId,
                    StatusName = ws.StatusMaster?.Name ?? string.Empty,
                    StatusCode = ws.StatusMaster?.Code ?? string.Empty,
                    StatusColorCode = ws.StatusMaster?.ColorCode,
                    StatusIconClass = ws.StatusMaster?.IconClass,
                    DisplayOrder = ws.DisplayOrder,
                    IsInitialStatus = ws.IsInitialStatus,
                    IsActive = ws.IsActive,
                    DefaultSLAHours = ws.DefaultSLAHours,
                    EscalationHours = ws.EscalationHours,
                    RequiresApproval = ws.RequiresApproval,
                    AllowedRoles = ws.GetAllowedRoleIds(),
                    CreatedAt = ws.CreatedAt
                }).ToList(),
                Transitions = w.Transitions.Select(t => new CategoryWorkflowTransitionDto
                {
                    Id = t.Id,
                    WorkflowId = t.WorkflowId,
                    FromStatusId = t.FromStatusId,
                    FromStatusName = t.FromStatus?.Name ?? string.Empty,
                    FromStatusCode = t.FromStatus?.Code ?? string.Empty,
                    ToStatusId = t.ToStatusId,
                    ToStatusName = t.ToStatus?.Name ?? string.Empty,
                    ToStatusCode = t.ToStatus?.Code ?? string.Empty,
                    TransitionName = t.TransitionName,
                    Description = t.Description,
                    RequiresComment = t.RequiresComment,
                    RequiresApproval = t.RequiresApproval,
                    AllowedRoles = t.GetAllowedRoleIds(),
                    DisplayOrder = t.DisplayOrder,
                    IsActive = t.IsActive,
                    IsAutomatic = t.IsAutomatic,
                    AutoTransitionAfterHours = t.AutoTransitionAfterHours,
                    TransitionConditions = t.GetConditions(),
                    ButtonColor = t.ButtonColor,
                    IconClass = t.IconClass,
                    CreatedAt = t.CreatedAt
                }).ToList(),
                CreatedAt = w.CreatedAt,
                UpdatedAt = w.UpdatedAt
            }).ToList();

            return Ok(new
            {
                isSuccess = true,
                data = workflowDtos,
                message = $"Retrieved {workflowDtos.Count} workflows"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflows");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error retrieving workflows",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get workflow for a specific category
    /// </summary>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetWorkflowForCategory(Guid categoryId)
    {
        try
        {
            var workflow = await _workflowEngine.GetWorkflowForCategoryAsync(categoryId);

            if (workflow == null)
            {
                return Ok(new
                {
                    isSuccess = true,
                    data = (object?)null,
                    message = "No custom workflow configured for this category. Using global workflow."
                });
            }

            var workflowDto = new CategoryWorkflowDto
            {
                Id = workflow.Id,
                CategoryId = workflow.CategoryId,
                CategoryName = workflow.Category?.Name ?? string.Empty,
                Name = workflow.Name,
                Description = workflow.Description,
                IsActive = workflow.IsActive,
                IsDefault = workflow.IsDefault,
                CompanyId = workflow.CompanyId,
                CompanyName = workflow.Company?.Name,
                WorkflowStatuses = workflow.WorkflowStatuses.Select(ws => new CategoryWorkflowStatusDto
                {
                    Id = ws.Id,
                    WorkflowId = ws.WorkflowId,
                    StatusMasterId = ws.StatusMasterId,
                    StatusName = ws.StatusMaster?.Name ?? string.Empty,
                    StatusCode = ws.StatusMaster?.Code ?? string.Empty,
                    StatusColorCode = ws.StatusMaster?.ColorCode,
                    StatusIconClass = ws.StatusMaster?.IconClass,
                    DisplayOrder = ws.DisplayOrder,
                    IsInitialStatus = ws.IsInitialStatus,
                    IsActive = ws.IsActive,
                    DefaultSLAHours = ws.DefaultSLAHours,
                    EscalationHours = ws.EscalationHours,
                    RequiresApproval = ws.RequiresApproval,
                    AllowedRoles = ws.GetAllowedRoleIds(),
                    CreatedAt = ws.CreatedAt
                }).ToList(),
                Transitions = workflow.Transitions.Select(t => new CategoryWorkflowTransitionDto
                {
                    Id = t.Id,
                    WorkflowId = t.WorkflowId,
                    FromStatusId = t.FromStatusId,
                    FromStatusName = t.FromStatus?.Name ?? string.Empty,
                    FromStatusCode = t.FromStatus?.Code ?? string.Empty,
                    ToStatusId = t.ToStatusId,
                    ToStatusName = t.ToStatus?.Name ?? string.Empty,
                    ToStatusCode = t.ToStatus?.Code ?? string.Empty,
                    TransitionName = t.TransitionName,
                    Description = t.Description,
                    RequiresComment = t.RequiresComment,
                    RequiresApproval = t.RequiresApproval,
                    AllowedRoles = t.GetAllowedRoleIds(),
                    DisplayOrder = t.DisplayOrder,
                    IsActive = t.IsActive,
                    IsAutomatic = t.IsAutomatic,
                    AutoTransitionAfterHours = t.AutoTransitionAfterHours,
                    TransitionConditions = t.GetConditions(),
                    ButtonColor = t.ButtonColor,
                    IconClass = t.IconClass,
                    CreatedAt = t.CreatedAt
                }).ToList(),
                CreatedAt = workflow.CreatedAt,
                UpdatedAt = workflow.UpdatedAt
            };

            return Ok(new
            {
                isSuccess = true,
                data = workflowDto,
                message = "Workflow retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving workflow for category {CategoryId}", categoryId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error retrieving workflow",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Create a new workflow for a category
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> CreateWorkflow([FromBody] CreateCategoryWorkflowRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        try
        {
            // Get companyId from token if not provided
            var companyId = request.CompanyId;
            if (!companyId.HasValue)
            {
                var companyIdClaim = User.FindFirst("CompanyId")?.Value;
                if (!string.IsNullOrEmpty(companyIdClaim) && Guid.TryParse(companyIdClaim, out var tokenCompanyId))
                {
                    companyId = tokenCompanyId;
                }
            }

            var workflow = await _workflowEngine.CreateWorkflowAsync(
                request.CategoryId,
                request.Name,
                request.Description,
                companyId
            );

            return Ok(new
            {
                isSuccess = true,
                data = new { id = workflow.Id, name = workflow.Name },
                message = "Workflow created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating workflow");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error creating workflow",
                errors = new[] { ex.Message }
            });
        }
    }

    #endregion

    #region Workflow Statuses

    /// <summary>
    /// Get available statuses for a category
    /// </summary>
    [HttpGet("categories/{categoryId}/statuses")]
    public async Task<IActionResult> GetWorkflowStatuses(Guid categoryId)
    {
        try
        {
            var statuses = await _workflowEngine.GetWorkflowStatusesAsync(categoryId);

            var statusDtos = statuses.Select(s => new ComplaintStatusMasterDto
            {
                Id = s.Id,
                Name = s.Name,
                Code = s.Code,
                Description = s.Description,
                ColorCode = s.ColorCode,
                IconClass = s.IconClass,
                DisplayOrder = s.DisplayOrder,
                IsActive = s.IsActive,
                IsSystem = s.IsSystem
            }).ToList();

            return Ok(new
            {
                isSuccess = true,
                data = statusDtos,
                message = $"Retrieved {statusDtos.Count} statuses for category"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving statuses for category {CategoryId}", categoryId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error retrieving statuses",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Add a status to a workflow
    /// </summary>
    [HttpPost("{workflowId}/statuses")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> AddStatusToWorkflow(Guid workflowId, [FromBody] AddStatusToWorkflowRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        try
        {
            var workflowStatus = await _workflowEngine.AddStatusToWorkflowAsync(
                workflowId,
                request.StatusMasterId,
                request.DisplayOrder,
                request.IsInitialStatus,
                request.DefaultSLAHours
            );

            return Ok(new
            {
                isSuccess = true,
                data = new { id = workflowStatus.Id },
                message = "Status added to workflow successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding status to workflow {WorkflowId}", workflowId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error adding status to workflow",
                errors = new[] { ex.Message }
            });
        }
    }

    #endregion

    #region Workflow Transitions

    /// <summary>
    /// Add a transition rule to a workflow
    /// </summary>
    [HttpPost("{workflowId}/transitions")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> AddTransitionRule(Guid workflowId, [FromBody] AddTransitionRuleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        try
        {
            var transition = await _workflowEngine.AddTransitionRuleAsync(
                workflowId,
                request.FromStatusId,
                request.ToStatusId,
                request.TransitionName,
                request.RequiresComment,
                request.RequiresApproval,
                request.AllowedRoles
            );

            return Ok(new
            {
                isSuccess = true,
                data = new { id = transition.Id },
                message = "Transition rule added successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding transition rule to workflow {WorkflowId}", workflowId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error adding transition rule",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get allowed transitions for a user from a specific status
    /// </summary>
    [HttpGet("allowed-transitions")]
    public async Task<IActionResult> GetAllowedTransitions(
        [FromQuery] Guid categoryId,
        [FromQuery] Guid currentStatusId)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var transitions = await _workflowEngine.GetAllowedTransitionsAsync(categoryId, currentStatusId, userId);

            var transitionDtos = transitions.Select(t => new CategoryWorkflowTransitionDto
            {
                Id = t.Id,
                WorkflowId = t.WorkflowId,
                FromStatusId = t.FromStatusId,
                FromStatusName = t.FromStatus?.Name ?? string.Empty,
                FromStatusCode = t.FromStatus?.Code ?? string.Empty,
                ToStatusId = t.ToStatusId,
                ToStatusName = t.ToStatus?.Name ?? string.Empty,
                ToStatusCode = t.ToStatus?.Code ?? string.Empty,
                TransitionName = t.TransitionName,
                Description = t.Description,
                RequiresComment = t.RequiresComment,
                RequiresApproval = t.RequiresApproval,
                AllowedRoles = t.GetAllowedRoleIds(),
                DisplayOrder = t.DisplayOrder,
                IsActive = t.IsActive,
                IsAutomatic = t.IsAutomatic,
                AutoTransitionAfterHours = t.AutoTransitionAfterHours,
                TransitionConditions = t.GetConditions(),
                ButtonColor = t.ButtonColor,
                IconClass = t.IconClass,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(new
            {
                isSuccess = true,
                data = new AllowedTransitionsResponse
                {
                    Transitions = transitionDtos,
                    Count = transitionDtos.Count
                },
                message = $"Retrieved {transitionDtos.Count} allowed transitions"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving allowed transitions");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error retrieving allowed transitions",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Check if a specific transition is allowed
    /// </summary>
    [HttpPost("check-transition")]
    public async Task<IActionResult> CheckTransitionAllowed([FromBody] CheckTransitionAllowedRequest request)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                userId = request.UserId;
            }

            var isAllowed = await _workflowEngine.IsTransitionAllowedAsync(
                request.CategoryId,
                request.FromStatusId,
                request.ToStatusId,
                userId
            );

            // Check if comment is required
            var workflow = await _workflowEngine.GetWorkflowForCategoryAsync(request.CategoryId);
            bool requiresComment = false;
            bool requiresApproval = false;

            if (workflow != null)
            {
                requiresComment = await _workflowEngine.TransitionRequiresCommentAsync(
                    workflow.Id,
                    request.FromStatusId,
                    request.ToStatusId
                );

                requiresApproval = await _workflowEngine.TransitionRequiresApprovalAsync(
                    workflow.Id,
                    request.FromStatusId,
                    request.ToStatusId
                );
            }

            return Ok(new
            {
                isSuccess = true,
                data = new TransitionValidationResponse
                {
                    IsAllowed = isAllowed,
                    Reason = isAllowed ? null : "User does not have permission for this transition",
                    RequiresComment = requiresComment,
                    RequiresApproval = requiresApproval
                },
                message = isAllowed ? "Transition is allowed" : "Transition is not allowed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking transition permission");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error checking transition permission",
                errors = new[] { ex.Message }
            });
        }
    }

    #endregion

    #region Complaint Operations

    /// <summary>
    /// Transition a complaint to a new status
    /// </summary>
    [HttpPost("complaints/{complaintId}/transition")]
    [HasPermission("EditComplaint")]
    public async Task<IActionResult> TransitionComplaint(Guid complaintId, [FromBody] TransitionComplaintRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList()
            });
        }

        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("User ID not found in token");
            }

            var success = await _workflowEngine.TransitionComplaintAsync(
                complaintId,
                request.NewStatusId,
                userId,
                request.Comment
            );

            if (success)
            {
                // Retrieve the updated complaint using the existing query
                var query = new GetComplaintByIdQuery { Id = complaintId };
                var result = await _mediator.Send(query);

                if (!result.IsSuccess || result.Data == null)
                {
                    return NotFound(new
                    {
                        isSuccess = false,
                        message = "Complaint not found after transition"
                    });
                }

                return Ok(new
                {
                    isSuccess = true,
                    data = result.Data,
                    message = "Complaint status transitioned successfully"
                });
            }
            else
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Failed to transition complaint. Transition may not be allowed or comment may be required."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transitioning complaint {ComplaintId}", complaintId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error transitioning complaint",
                errors = new[] { ex.Message }
            });
        }
    }

    /// <summary>
    /// Get the initial status for a new complaint in a category
    /// </summary>
    [HttpGet("categories/{categoryId}/initial-status")]
    public async Task<IActionResult> GetInitialStatus(Guid categoryId)
    {
        try
        {
            var initialStatus = await _workflowEngine.GetInitialStatusAsync(categoryId);

            var statusDto = new ComplaintStatusMasterDto
            {
                Id = initialStatus.Id,
                Name = initialStatus.Name,
                Code = initialStatus.Code,
                Description = initialStatus.Description,
                ColorCode = initialStatus.ColorCode,
                IconClass = initialStatus.IconClass,
                DisplayOrder = initialStatus.DisplayOrder,
                IsActive = initialStatus.IsActive,
                IsSystem = initialStatus.IsSystem
            };

            return Ok(new
            {
                isSuccess = true,
                data = statusDto,
                message = "Initial status retrieved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving initial status for category {CategoryId}", categoryId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "Error retrieving initial status",
                errors = new[] { ex.Message }
            });
        }
    }

    #endregion
}
