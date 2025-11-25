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
public class ComplaintStatusMasterController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComplaintStatusMasterController> _logger;

    public ComplaintStatusMasterController(IMediator mediator, ILogger<ComplaintStatusMasterController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all complaint status masters
    /// </summary>
    [HttpGet]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "companyId", "isActive", "includeSystem" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetAll([FromQuery] Guid? companyId, [FromQuery] bool? isActive, [FromQuery] bool includeSystem = true)
    {
        var query = new GetComplaintStatusMastersQuery
        {
            CompanyId = companyId,
            IsActive = isActive,
            IncludeSystem = includeSystem
        };

        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get a complaint status by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetComplaintStatusMasterByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    /// <summary>
    /// Create a new complaint status
    /// </summary>
    [HttpPost]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> Create([FromBody] CreateComplaintStatusMasterCommand command)
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
    /// Update an existing complaint status
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateComplaintStatusMasterCommand command)
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
    /// Delete a complaint status
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageSettings")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var command = new DeleteComplaintStatusMasterCommand { Id = id };
        var result = await _mediator.Send(command);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    /// <summary>
    /// Get all complaint status enum values for filters
    /// </summary>
    [HttpGet("enum-values")]
    [AllowAnonymous]
    public IActionResult GetStatusEnumValues()
    {
        var statusValues = Enum.GetValues(typeof(ComplaintStatus))
            .Cast<ComplaintStatus>()
            .Select(status => new
            {
                value = (int)status,
                label = status.ToString(),
                description = GetStatusDescription(status)
            })
            .ToList();

        return Ok(new
        {
            isSuccess = true,
            data = statusValues
        });
    }

    private string GetStatusDescription(ComplaintStatus status)
    {
        return status switch
        {
            ComplaintStatus.Submitted => "Complaint has been submitted but not yet reviewed",
            ComplaintStatus.UnderReview => "Complaint is being reviewed by the assigned handler",
            ComplaintStatus.InProgress => "Complaint is currently being investigated",
            ComplaintStatus.Escalated => "Complaint has been escalated to a higher level",
            ComplaintStatus.PendingInfo => "Complaint is awaiting information from the complainant",
            ComplaintStatus.Resolved => "Complaint has been resolved",
            ComplaintStatus.Closed => "Complaint has been closed (final state)",
            ComplaintStatus.Rejected => "Complaint has been rejected/dismissed",
            ComplaintStatus.Reopened => "Complaint has been reopened after closure",
            _ => status.ToString()
        };
    }
}
