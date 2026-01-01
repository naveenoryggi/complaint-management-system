using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.API.Authorization;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.CRM;

/// <summary>
/// Controller for managing partners (resellers, system integrators, distributors)
/// Requires CRM_Customer license module
/// </summary>
[ApiController]
[Route("api/partners")]
[Authorize]
[RequiresLicense(LicenseModule.CRM_Customer)]
public class PartnersController : ControllerBase
{
    private readonly IPartnerService _partnerService;
    private readonly ILogger<PartnersController> _logger;

    public PartnersController(IPartnerService partnerService, ILogger<PartnersController> logger)
    {
        _partnerService = partnerService;
        _logger = logger;
    }

    #region Partner CRUD

    /// <summary>
    /// Get all partners with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<PagedResult<PartnerSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartners(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? tier = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnersAsync(
                companyId.Value, searchTerm, status, type, tier, page, pageSize, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partners");
            return StatusCode(500, new { message = "Error retrieving partners" });
        }
    }

    /// <summary>
    /// Get a partner by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<PartnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPartner(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerByIdAsync(companyId.Value, id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner {PartnerId}", id);
            return StatusCode(500, new { message = "Error retrieving partner" });
        }
    }

    /// <summary>
    /// Get a partner by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<PartnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPartnerByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerByCodeAsync(companyId.Value, code, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner by code {Code}", code);
            return StatusCode(500, new { message = "Error retrieving partner" });
        }
    }

    /// <summary>
    /// Create a new partner
    /// </summary>
    [HttpPost]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<PartnerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePartner([FromBody] CreatePartnerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.CreatePartnerAsync(companyId.Value, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Partner {Code} created by {User}", request.Code, userEmail);
            return CreatedAtAction(nameof(GetPartner), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating partner");
            return StatusCode(500, new { message = "Error creating partner" });
        }
    }

    /// <summary>
    /// Update an existing partner
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<PartnerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartner(Guid id, [FromBody] UpdatePartnerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.UpdatePartnerAsync(companyId.Value, id, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Partner {PartnerId} updated by {User}", id, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating partner {PartnerId}", id);
            return StatusCode(500, new { message = "Error updating partner" });
        }
    }

    /// <summary>
    /// Delete a partner (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePartner(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.DeletePartnerAsync(companyId.Value, id, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Partner {PartnerId} deleted by {User}", id, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting partner {PartnerId}", id);
            return StatusCode(500, new { message = "Error deleting partner" });
        }
    }

    /// <summary>
    /// Get partner lookup list for dropdowns
    /// </summary>
    [HttpGet("lookup")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<List<PartnerLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerLookup([FromQuery] string? searchTerm = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerLookupAsync(companyId.Value, searchTerm, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner lookup");
            return StatusCode(500, new { message = "Error retrieving partner lookup" });
        }
    }

    #endregion

    #region Partner Contacts

    /// <summary>
    /// Get all contacts for a partner
    /// </summary>
    [HttpGet("{partnerId:guid}/contacts")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<List<PartnerContactSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerContacts(Guid partnerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerContactsAsync(companyId.Value, partnerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner contacts for {PartnerId}", partnerId);
            return StatusCode(500, new { message = "Error retrieving partner contacts" });
        }
    }

    /// <summary>
    /// Get a partner contact by ID
    /// </summary>
    [HttpGet("contacts/{contactId:guid}")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<PartnerContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPartnerContact(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerContactByIdAsync(companyId.Value, contactId, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error retrieving partner contact" });
        }
    }

    /// <summary>
    /// Create a new partner contact
    /// </summary>
    [HttpPost("contacts")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<PartnerContactDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePartnerContact([FromBody] CreatePartnerContactRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.CreatePartnerContactAsync(companyId.Value, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Partner contact {Email} created by {User}", request.Email, userEmail);
            return CreatedAtAction(nameof(GetPartnerContact), new { contactId = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating partner contact");
            return StatusCode(500, new { message = "Error creating partner contact" });
        }
    }

    /// <summary>
    /// Update an existing partner contact
    /// </summary>
    [HttpPut("contacts/{contactId:guid}")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<PartnerContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartnerContact(Guid contactId, [FromBody] UpdatePartnerContactRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.UpdatePartnerContactAsync(companyId.Value, contactId, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Partner contact {ContactId} updated by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating partner contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error updating partner contact" });
        }
    }

    /// <summary>
    /// Delete a partner contact (soft delete)
    /// </summary>
    [HttpDelete("contacts/{contactId:guid}")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePartnerContact(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.DeletePartnerContactAsync(companyId.Value, contactId, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Partner contact {ContactId} deleted by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting partner contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error deleting partner contact" });
        }
    }

    /// <summary>
    /// Set or reset a partner contact's password
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/password")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPartnerContactPassword(Guid contactId, [FromBody] SetContactPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.SetPartnerContactPasswordAsync(companyId.Value, contactId, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Partner contact {ContactId} password reset by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting partner contact password {ContactId}", contactId);
            return StatusCode(500, new { message = "Error setting partner contact password" });
        }
    }

    /// <summary>
    /// Unlock a locked partner contact account
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/unlock")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockPartnerContact(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.UnlockPartnerContactAsync(companyId.Value, contactId, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Partner contact {ContactId} unlocked by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking partner contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error unlocking partner contact" });
        }
    }

    #endregion

    #region Partner-Customer Relationships

    /// <summary>
    /// Get all customers assigned to a partner
    /// </summary>
    [HttpGet("{partnerId:guid}/customers")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<List<PartnerCustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerCustomers(Guid partnerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerCustomersAsync(companyId.Value, partnerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner customers for {PartnerId}", partnerId);
            return StatusCode(500, new { message = "Error retrieving partner customers" });
        }
    }

    /// <summary>
    /// Assign a customer to a partner
    /// </summary>
    [HttpPost("customers")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<PartnerCustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignCustomerToPartner([FromBody] AssignPartnerToCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.AssignCustomerToPartnerAsync(companyId.Value, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Customer {CustomerId} assigned to partner {PartnerId} by {User}",
                request.CustomerId, request.PartnerId, userEmail);
            return StatusCode(201, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning customer to partner");
            return StatusCode(500, new { message = "Error assigning customer to partner" });
        }
    }

    /// <summary>
    /// Update a partner-customer relationship
    /// </summary>
    [HttpPut("customers/{partnerCustomerId:guid}")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<PartnerCustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePartnerCustomer(Guid partnerCustomerId, [FromBody] UpdatePartnerCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.UpdatePartnerCustomerAsync(companyId.Value, partnerCustomerId, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Partner-customer relationship {PartnerCustomerId} updated by {User}", partnerCustomerId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating partner-customer relationship {PartnerCustomerId}", partnerCustomerId);
            return StatusCode(500, new { message = "Error updating partner-customer relationship" });
        }
    }

    /// <summary>
    /// Remove a customer from a partner
    /// </summary>
    [HttpDelete("customers/{partnerCustomerId:guid}")]
    [HasPermission("ManagePartners")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveCustomerFromPartner(Guid partnerCustomerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _partnerService.RemoveCustomerFromPartnerAsync(companyId.Value, partnerCustomerId, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Partner-customer relationship {PartnerCustomerId} removed by {User}", partnerCustomerId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing partner-customer relationship {PartnerCustomerId}", partnerCustomerId);
            return StatusCode(500, new { message = "Error removing partner-customer relationship" });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get partner statistics
    /// </summary>
    [HttpGet("{partnerId:guid}/statistics")]
    [HasPermission("ViewPartners")]
    [ProducesResponseType(typeof(Result<PartnerStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPartnerStatistics(Guid partnerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _partnerService.GetPartnerStatisticsAsync(companyId.Value, partnerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting partner statistics for {PartnerId}", partnerId);
            return StatusCode(500, new { message = "Error retrieving partner statistics" });
        }
    }

    #endregion

    #region Private Methods

    private Guid? GetCurrentCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }

    private string GetCurrentUserEmail()
    {
        return User.FindFirst(ClaimTypes.Email)?.Value ?? "Unknown";
    }

    #endregion
}
