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
/// Controller for managing customers, their locations and contacts
/// Requires CRM_Customer license module
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
[RequiresLicense(LicenseModule.CRM_Customer)]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomersController> _logger;

    public CustomersController(ICustomerService customerService, ILogger<CustomersController> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    #region Customer CRUD

    /// <summary>
    /// Get all customers with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<PagedResult<CustomerSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? status = null,
        [FromQuery] string? type = null,
        [FromQuery] string? segment = null,
        [FromQuery] Guid? partnerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomersAsync(
                companyId.Value, searchTerm, status, type, segment, partnerId, page, pageSize, cancellationToken);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers");
            return StatusCode(500, new { message = "Error retrieving customers" });
        }
    }

    /// <summary>
    /// Get a customer by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomer(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerByIdAsync(companyId.Value, id, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", id);
            return StatusCode(500, new { message = "Error retrieving customer" });
        }
    }

    /// <summary>
    /// Get a customer by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerByCodeAsync(companyId.Value, code, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by code {Code}", code);
            return StatusCode(500, new { message = "Error retrieving customer" });
        }
    }

    /// <summary>
    /// Create a new customer
    /// </summary>
    [HttpPost]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<CustomerDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.CreateCustomerAsync(companyId.Value, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Customer {Code} created by {User}", request.Code, userEmail);
            return CreatedAtAction(nameof(GetCustomer), new { id = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return StatusCode(500, new { message = "Error creating customer" });
        }
    }

    /// <summary>
    /// Update an existing customer
    /// </summary>
    [HttpPut("{id:guid}")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<CustomerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.UpdateCustomerAsync(companyId.Value, id, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Customer {CustomerId} updated by {User}", id, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", id);
            return StatusCode(500, new { message = "Error updating customer" });
        }
    }

    /// <summary>
    /// Delete a customer (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.DeleteCustomerAsync(companyId.Value, id, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Customer {CustomerId} deleted by {User}", id, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", id);
            return StatusCode(500, new { message = "Error deleting customer" });
        }
    }

    /// <summary>
    /// Get customer lookup list for dropdowns
    /// </summary>
    [HttpGet("lookup")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<List<CustomerLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerLookup(
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? partnerId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerLookupAsync(companyId.Value, searchTerm, partnerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer lookup");
            return StatusCode(500, new { message = "Error retrieving customer lookup" });
        }
    }

    #endregion

    #region Customer Locations

    /// <summary>
    /// Get all locations for a customer
    /// </summary>
    [HttpGet("{customerId:guid}/locations")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<List<CustomerLocationSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerLocations(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerLocationsAsync(companyId.Value, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer locations for {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving customer locations" });
        }
    }

    /// <summary>
    /// Get a customer location by ID
    /// </summary>
    [HttpGet("locations/{locationId:guid}")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<CustomerLocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerLocation(Guid locationId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerLocationByIdAsync(companyId.Value, locationId, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer location {LocationId}", locationId);
            return StatusCode(500, new { message = "Error retrieving customer location" });
        }
    }

    /// <summary>
    /// Create a new customer location
    /// </summary>
    [HttpPost("locations")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<CustomerLocationDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomerLocation([FromBody] CreateCustomerLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.CreateCustomerLocationAsync(companyId.Value, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Customer location {Code} created by {User}", request.Code, userEmail);
            return CreatedAtAction(nameof(GetCustomerLocation), new { locationId = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer location");
            return StatusCode(500, new { message = "Error creating customer location" });
        }
    }

    /// <summary>
    /// Update an existing customer location
    /// </summary>
    [HttpPut("locations/{locationId:guid}")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<CustomerLocationDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerLocation(Guid locationId, [FromBody] UpdateCustomerLocationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.UpdateCustomerLocationAsync(companyId.Value, locationId, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Customer location {LocationId} updated by {User}", locationId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer location {LocationId}", locationId);
            return StatusCode(500, new { message = "Error updating customer location" });
        }
    }

    /// <summary>
    /// Delete a customer location (soft delete)
    /// </summary>
    [HttpDelete("locations/{locationId:guid}")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomerLocation(Guid locationId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.DeleteCustomerLocationAsync(companyId.Value, locationId, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Customer location {LocationId} deleted by {User}", locationId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer location {LocationId}", locationId);
            return StatusCode(500, new { message = "Error deleting customer location" });
        }
    }

    /// <summary>
    /// Get location lookup list for a customer
    /// </summary>
    [HttpGet("{customerId:guid}/locations/lookup")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<List<CustomerLocationLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLocationLookup(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetLocationLookupAsync(companyId.Value, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting location lookup for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving location lookup" });
        }
    }

    #endregion

    #region Customer Contacts

    /// <summary>
    /// Get all contacts for a customer
    /// </summary>
    [HttpGet("{customerId:guid}/contacts")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<List<CustomerContactSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerContacts(Guid customerId, [FromQuery] Guid? locationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerContactsAsync(companyId.Value, customerId, locationId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer contacts for {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving customer contacts" });
        }
    }

    /// <summary>
    /// Get a customer contact by ID
    /// </summary>
    [HttpGet("contacts/{contactId:guid}")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<CustomerContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerContact(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerContactByIdAsync(companyId.Value, contactId, cancellationToken);
            if (!result.IsSuccess)
                return NotFound(result);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error retrieving customer contact" });
        }
    }

    /// <summary>
    /// Create a new customer contact
    /// </summary>
    [HttpPost("contacts")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<CustomerContactDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomerContact([FromBody] CreateCustomerContactRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.CreateCustomerContactAsync(companyId.Value, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return BadRequest(result);

            _logger.LogInformation("Customer contact {Email} created by {User}", request.Email, userEmail);
            return CreatedAtAction(nameof(GetCustomerContact), new { contactId = result.Data!.Id }, result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer contact");
            return StatusCode(500, new { message = "Error creating customer contact" });
        }
    }

    /// <summary>
    /// Update an existing customer contact
    /// </summary>
    [HttpPut("contacts/{contactId:guid}")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<CustomerContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerContact(Guid contactId, [FromBody] UpdateCustomerContactRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.UpdateCustomerContactAsync(companyId.Value, contactId, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
            {
                if (result.Message?.Contains("not found") == true)
                    return NotFound(result);
                return BadRequest(result);
            }

            _logger.LogInformation("Customer contact {ContactId} updated by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error updating customer contact" });
        }
    }

    /// <summary>
    /// Delete a customer contact (soft delete)
    /// </summary>
    [HttpDelete("contacts/{contactId:guid}")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomerContact(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.DeleteCustomerContactAsync(companyId.Value, contactId, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Customer contact {ContactId} deleted by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error deleting customer contact" });
        }
    }

    /// <summary>
    /// Set or reset a customer contact's password
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/password")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetCustomerContactPassword(Guid contactId, [FromBody] SetContactPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.SetCustomerContactPasswordAsync(companyId.Value, contactId, request, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Customer contact {ContactId} password reset by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting customer contact password {ContactId}", contactId);
            return StatusCode(500, new { message = "Error setting customer contact password" });
        }
    }

    /// <summary>
    /// Unlock a locked customer contact account
    /// </summary>
    [HttpPost("contacts/{contactId:guid}/unlock")]
    [HasPermission("ManageCustomers")]
    [ProducesResponseType(typeof(Result<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlockCustomerContact(Guid contactId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var userEmail = GetCurrentUserEmail();
            var result = await _customerService.UnlockCustomerContactAsync(companyId.Value, contactId, userEmail, cancellationToken);

            if (!result.IsSuccess)
                return NotFound(result);

            _logger.LogInformation("Customer contact {ContactId} unlocked by {User}", contactId, userEmail);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unlocking customer contact {ContactId}", contactId);
            return StatusCode(500, new { message = "Error unlocking customer contact" });
        }
    }

    /// <summary>
    /// Get contact lookup list for a customer
    /// </summary>
    [HttpGet("{customerId:guid}/contacts/lookup")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<List<ContactLookupDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContactLookup(Guid customerId, [FromQuery] Guid? locationId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetContactLookupAsync(companyId.Value, customerId, locationId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contact lookup for customer {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving contact lookup" });
        }
    }

    #endregion

    #region Partner Relationships

    /// <summary>
    /// Get all partners assigned to a customer
    /// </summary>
    [HttpGet("{customerId:guid}/partners")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<List<PartnerCustomerDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerPartners(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerPartnersAsync(companyId.Value, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer partners for {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving customer partners" });
        }
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Get customer statistics
    /// </summary>
    [HttpGet("{customerId:guid}/statistics")]
    [HasPermission("ViewCustomers")]
    [ProducesResponseType(typeof(Result<CustomerStatisticsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomerStatistics(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCurrentCompanyId();
            if (!companyId.HasValue)
                return Unauthorized(new { message = "Company context not found" });

            var result = await _customerService.GetCustomerStatisticsAsync(companyId.Value, customerId, cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer statistics for {CustomerId}", customerId);
            return StatusCode(500, new { message = "Error retrieving customer statistics" });
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
