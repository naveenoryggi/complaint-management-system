using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Features.MasterData.Queries;
using ComplaintManagement.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComplaintPriorityMasterController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComplaintPriorityMasterController> _logger;

    public ComplaintPriorityMasterController(IMediator mediator, ILogger<ComplaintPriorityMasterController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all complaint priority masters
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "companyId", "isActive", "includeSystem" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] bool? isActive, [FromQuery] bool includeSystem = true)
    {
        var query = new GetComplaintPriorityMastersQuery
        {
            CompanyId = companyId,
            IsActive = isActive,
            IncludeSystem = includeSystem
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a complaint priority by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetComplaintPriorityMasterByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new complaint priority
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> Create([FromBody] CreateComplaintPriorityMasterCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Update an existing complaint priority
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateComplaintPriorityMasterCommand command)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        if (id != command.Id)
        {
            return BadRequest("ID mismatch");
        }

        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Delete a complaint priority
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteComplaintPriorityMasterCommand { Id = id };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all complaint priority enum values for filters
    /// </summary>
    [HttpGet("enum-values")]
    [AllowAnonymous]
    public IActionResult GetPriorityEnumValues()
    {
        var priorityValues = Enum.GetValues(typeof(ComplaintPriority))
            .Cast<ComplaintPriority>()
            .Select(priority => new
            {
                value = (int)priority,
                label = priority.ToString(),
                description = GetPriorityDescription(priority)
            })
            .ToList();

        return Ok(new
        {
            isSuccess = true,
            data = priorityValues
        });
    }

    /// <summary>
    /// Get all complaint priority options (combines enum values and database records)
    /// </summary>
    [HttpGet("all-options")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllPriorityOptions([FromQuery] Guid? companyId, [FromQuery] bool? isActive, [FromQuery] bool includeSystem = true)
    {
        try
        {
            // Get database priority masters
            var query = new GetComplaintPriorityMastersQuery
            {
                CompanyId = companyId,
                IsActive = isActive,
                IncludeSystem = includeSystem
            };

            var dbResult = await _mediator.Send(query);

            var priorityOptions = new List<object>();

            // Add database priorities
            if (dbResult.IsSuccess && dbResult.Data != null)
            {
                foreach (var priority in dbResult.Data)
                {
                    priorityOptions.Add(new
                    {
                        id = priority.Id,
                        value = priority.Level > 0 ? priority.Level : 999, // Use Level as value for filtering
                        label = priority.Name,
                        code = priority.Code,
                        description = priority.Description,
                        level = priority.Level,
                        colorCode = priority.ColorCode,
                        iconClass = priority.IconClass,
                        displayOrder = priority.DisplayOrder,
                        isSystem = priority.IsSystem,
                        slaResponseHours = priority.SlaResponseHours,
                        slaResolutionHours = priority.SlaResolutionHours,
                        isActive = priority.IsActive
                    });
                }
            }

            // Add enum priorities that aren't in the database
            var enumPriorities = Enum.GetValues(typeof(ComplaintPriority))
                .Cast<ComplaintPriority>()
                .ToList();

            foreach (var enumPriority in enumPriorities)
            {
                // Check if this priority already exists in database records
                var existsInDb = dbResult.Data?.Any(p =>
                    p.Code.Equals(enumPriority.ToString(), StringComparison.OrdinalIgnoreCase)) ?? false;

                if (!existsInDb)
                {
                    priorityOptions.Add(new
                    {
                        id = (Guid?)null,
                        value = (int)enumPriority, // Use enum value as value
                        label = enumPriority.ToString(),
                        code = enumPriority.ToString(),
                        description = GetPriorityDescription(enumPriority),
                        level = (int)enumPriority + 1, // Add 1 to make 1-5 range
                        colorCode = GetDefaultPriorityColor(enumPriority),
                        iconClass = GetDefaultPriorityIcon(enumPriority),
                        displayOrder = (int)enumPriority,
                        isSystem = true,
                        slaResponseHours = GetDefaultSlaResponseHours(enumPriority),
                        slaResolutionHours = GetDefaultSlaResolutionHours(enumPriority),
                        isActive = true
                    });
                }
            }

            // Sort by display order, then by level, then by label
            var sortedPriorities = priorityOptions
                .OrderBy(p => ((dynamic)p).displayOrder)
                .ThenBy(p => ((dynamic)p).level)
                .ThenBy(p => ((dynamic)p).label)
                .ToList();

            return Ok(new
            {
                isSuccess = true,
                data = sortedPriorities
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all priority options");
            return BadRequest(new
            {
                isSuccess = false,
                message = "Failed to retrieve priority options",
                errors = new List<string> { ex.Message }
            });
        }
    }

    private string GetPriorityDescription(ComplaintPriority priority)
    {
        return priority switch
        {
            ComplaintPriority.Low => "Low priority - can be addressed during normal operations",
            ComplaintPriority.Normal => "Normal priority - standard processing",
            ComplaintPriority.High => "High priority - requires attention soon",
            ComplaintPriority.Critical => "Critical priority - urgent attention required",
            ComplaintPriority.Urgent => "Urgent priority - immediate attention required",
            _ => priority.ToString()
        };
    }

    private string GetDefaultPriorityColor(ComplaintPriority priority)
    {
        return priority switch
        {
            ComplaintPriority.Low => "#6c757d",      // Gray
            ComplaintPriority.Normal => "#17a2b8",   // Blue
            ComplaintPriority.High => "#ffc107",    // Amber/Yellow
            ComplaintPriority.Critical => "#fd7e14",  // Orange
            ComplaintPriority.Urgent => "#dc3545",   // Red
            _ => "#6c757d"
        };
    }

    private string GetDefaultPriorityIcon(ComplaintPriority priority)
    {
        return priority switch
        {
            ComplaintPriority.Low => "bi-arrow-down-circle",
            ComplaintPriority.Normal => "bi-circle",
            ComplaintPriority.High => "bi-exclamation-triangle",
            ComplaintPriority.Critical => "bi-exclamation-triangle-fill",
            ComplaintPriority.Urgent => "bi-exclamation-octagon-fill",
            _ => "bi-circle"
        };
    }

    private int GetDefaultSlaResponseHours(ComplaintPriority priority)
    {
        return priority switch
        {
            ComplaintPriority.Low => 72,      // 3 days
            ComplaintPriority.Normal => 48,   // 2 days
            ComplaintPriority.High => 24,     // 1 day
            ComplaintPriority.Critical => 8,    // 8 hours
            ComplaintPriority.Urgent => 4,    // 4 hours
            _ => 24
        };
    }

    private int GetDefaultSlaResolutionHours(ComplaintPriority priority)
    {
        return priority switch
        {
            ComplaintPriority.Low => 168,     // 7 days
            ComplaintPriority.Normal => 120,   // 5 days
            ComplaintPriority.High => 72,     // 3 days
            ComplaintPriority.Critical => 24,   // 1 day
            ComplaintPriority.Urgent => 12,    // 12 hours
            _ => 120
        };
    }
}
