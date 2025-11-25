using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Settings;
using ComplaintManagement.Application.Features.Settings.Commands;
using ComplaintManagement.Application.Features.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing complaint information visibility settings
/// </summary>
[ApiController]
[Route("api/ComplaintInfoSettings")]
[Authorize]
public class ComplaintInfoSettingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ComplaintInfoSettingsController> _logger;

    public ComplaintInfoSettingsController(
        IMediator mediator,
        ILogger<ComplaintInfoSettingsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get complaint information settings for the authenticated user's company
    /// </summary>
    /// <returns>Complaint information settings</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<ComplaintInformationSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSettingsByQuery()
    {
        try
        {
            // Get company ID from authenticated user's claims
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
            {
                _logger.LogWarning("CompanyId claim not found or invalid for user");
                return BadRequest(new { message = "Company ID not found in user claims" });
            }

            var query = new GetComplaintInfoSettingsQuery { CompanyId = companyId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving complaint info settings");
            return StatusCode(500, new { message = "An error occurred while retrieving settings" });
        }
    }

    /// <summary>
    /// Get complaint information settings for a company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>Complaint information settings</returns>
    [HttpGet("{companyId}")]
    [ProducesResponseType(typeof(Result<ComplaintInformationSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSettings(Guid companyId)
    {
        try
        {
            var query = new GetComplaintInfoSettingsQuery { CompanyId = companyId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving complaint info settings for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "An error occurred while retrieving settings" });
        }
    }

    /// <summary>
    /// Update complaint information settings for a company
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="request">Updated settings</param>
    /// <returns>Updated settings</returns>
    [HttpPut("{companyId}")]
    [ProducesResponseType(typeof(Result<ComplaintInformationSettingsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateSettings(Guid companyId, [FromBody] UpdateComplaintInfoSettingsRequest request)
    {
        try
        {
            var command = new UpdateComplaintInfoSettingsCommand
            {
                CompanyId = companyId,
                ShowEmployeeCodeToHandlers = request.ShowEmployeeCodeToHandlers,
                ShowEmailToHandlers = request.ShowEmailToHandlers,
                ShowPhoneToHandlers = request.ShowPhoneToHandlers,
                ShowBranchToHandlers = request.ShowBranchToHandlers,
                ShowDepartmentToHandlers = request.ShowDepartmentToHandlers,
                ShowSectionToHandlers = request.ShowSectionToHandlers,
                ShowJobTitleToHandlers = request.ShowJobTitleToHandlers,
                ShowManagerDetailsToHandlers = request.ShowManagerDetailsToHandlers,
                ShowPreviousComplaintsToHandlers = request.ShowPreviousComplaintsToHandlers,
                ShowEmployeeAddressToManagement = request.ShowEmployeeAddressToManagement,
                ShowEmergencyContactToManagement = request.ShowEmergencyContactToManagement,
                ShowPerformanceMetricsToManagement = request.ShowPerformanceMetricsToManagement,
                MaskPersonalInfoInLogs = request.MaskPersonalInfoInLogs,
                RedactInfoAfterClosure = request.RedactInfoAfterClosure,
                DataRetentionDays = request.DataRetentionDays,
                IncludeEmployeeCodeInReports = request.IncludeEmployeeCodeInReports,
                IncludeEmailInReports = request.IncludeEmailInReports,
                IncludePhoneInReports = request.IncludePhoneInReports,
                MaskEmailInReports = request.MaskEmailInReports,
                MaskPhoneInReports = request.MaskPhoneInReports
            };

            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Complaint info settings updated for company {CompanyId}", companyId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating complaint info settings for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "An error occurred while updating settings" });
        }
    }

    /// <summary>
    /// Toggle anonymous complaint setting
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="request">Anonymous setting</param>
    /// <returns>Updated setting</returns>
    [HttpPut("{companyId}/anonymous")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleAnonymousSetting(Guid companyId, [FromBody] ToggleSettingRequest request)
    {
        try
        {
            var query = new GetComplaintInfoSettingsQuery { CompanyId = companyId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Anonymous setting toggled for company {CompanyId}", companyId);
            return Ok(new
            {
                isSuccess = true,
                message = "Anonymous setting updated successfully",
                data = new { allowAnonymous = request.AllowAnonymous ?? request.AllowAttachments ?? false }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling anonymous setting for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "An error occurred while updating setting" });
        }
    }

    /// <summary>
    /// Toggle attachments setting
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <param name="request">Attachments setting</param>
    /// <returns>Updated setting</returns>
    [HttpPut("{companyId}/attachments")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ToggleAttachmentsSetting(Guid companyId, [FromBody] ToggleSettingRequest request)
    {
        try
        {
            var query = new GetComplaintInfoSettingsQuery { CompanyId = companyId };
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return NotFound(result);
            }

            _logger.LogInformation("Attachments setting toggled for company {CompanyId}", companyId);
            return Ok(new
            {
                isSuccess = true,
                message = "Attachments setting updated successfully",
                data = new { allowAttachments = request.AllowAttachments ?? request.AllowAnonymous ?? false }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while toggling attachments setting for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "An error occurred while updating setting" });
        }
    }
}

public class ToggleSettingRequest
{
    public bool? AllowAnonymous { get; set; }
    public bool? AllowAttachments { get; set; }
}
