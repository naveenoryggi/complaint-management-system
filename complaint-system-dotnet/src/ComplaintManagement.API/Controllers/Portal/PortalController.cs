using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Portal;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.Portal;

/// <summary>
/// Customer Portal Controller
/// Handles all portal operations for external customer contacts
/// </summary>
[ApiController]
[Route("api/portal")]
[Authorize]
[RequiresLicense(LicenseModule.CustomerPortal)]
public class PortalController : ControllerBase
{
    private readonly IPortalService _portalService;
    private readonly ILogger<PortalController> _logger;

    public PortalController(
        IPortalService portalService,
        ILogger<PortalController> logger)
    {
        _portalService = portalService;
        _logger = logger;
    }

    #region Dashboard

    /// <summary>
    /// Get portal dashboard data
    /// </summary>
    /// <returns>Dashboard with ticket stats, recent tickets, and summaries</returns>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(PortalDashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetDashboard()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var dashboard = await _portalService.GetDashboardAsync(contactId.Value);
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal dashboard");
            return StatusCode(500, new { message = "An error occurred while loading dashboard" });
        }
    }

    #endregion

    #region Tickets

    /// <summary>
    /// Get paginated list of tickets
    /// </summary>
    /// <param name="query">Filter and pagination parameters</param>
    /// <returns>Paginated list of tickets</returns>
    [HttpGet("tickets")]
    [ProducesResponseType(typeof(PagedResultDto<PortalTicketSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTickets([FromQuery] PortalTicketListQuery query)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var result = await _portalService.GetTicketsAsync(contactId.Value, query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal tickets");
            return StatusCode(500, new { message = "An error occurred while loading tickets" });
        }
    }

    /// <summary>
    /// Get ticket details by ID
    /// </summary>
    /// <param name="id">Ticket ID</param>
    /// <returns>Ticket details with comments and attachments</returns>
    [HttpGet("tickets/{id:guid}")]
    [ProducesResponseType(typeof(PortalTicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketById(Guid id)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var ticket = await _portalService.GetTicketByIdAsync(contactId.Value, id);
            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal ticket {TicketId}", id);
            return StatusCode(500, new { message = "An error occurred while loading ticket" });
        }
    }

    /// <summary>
    /// Get ticket details by complaint number
    /// </summary>
    /// <param name="complaintNumber">Complaint number</param>
    /// <returns>Ticket details</returns>
    [HttpGet("tickets/by-number/{complaintNumber}")]
    [ProducesResponseType(typeof(PortalTicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketByNumber(string complaintNumber)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var ticket = await _portalService.GetTicketByNumberAsync(contactId.Value, complaintNumber);
            if (ticket == null)
            {
                return NotFound(new { message = "Ticket not found" });
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal ticket by number {ComplaintNumber}", complaintNumber);
            return StatusCode(500, new { message = "An error occurred while loading ticket" });
        }
    }

    /// <summary>
    /// Create a new ticket from portal
    /// </summary>
    /// <param name="request">Ticket creation request</param>
    /// <returns>Created ticket details</returns>
    [HttpPost("tickets")]
    [ProducesResponseType(typeof(PortalTicketDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTicket([FromBody] PortalCreateTicketRequest request)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            // Check permission
            if (!HasPermission("CreateTickets"))
            {
                return StatusCode(403, new { message = "You don't have permission to create tickets" });
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Validation failed", errors = ModelState });
            }

            var result = await _portalService.CreateTicketAsync(contactId.Value, request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Portal ticket created by contact {ContactId}: {TicketId}",
                contactId, result.Data?.Id);

            return CreatedAtAction(
                nameof(GetTicketById),
                new { id = result.Data?.Id },
                result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating portal ticket");
            return StatusCode(500, new { message = "An error occurred while creating ticket" });
        }
    }

    /// <summary>
    /// Add a comment to a ticket
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="request">Comment request</param>
    /// <returns>Created comment</returns>
    [HttpPost("tickets/{ticketId:guid}/comments")]
    [ProducesResponseType(typeof(PortalTicketCommentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddComment(Guid ticketId, [FromBody] PortalAddCommentRequest request)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Content))
            {
                return BadRequest(new { message = "Comment content is required" });
            }

            var result = await _portalService.AddCommentAsync(contactId.Value, ticketId, request);

            if (!result.IsSuccess)
            {
                if (result.Message?.Contains("not found") == true || result.Message?.Contains("access") == true)
                {
                    return NotFound(new { message = result.Message });
                }
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Portal comment added to ticket {TicketId} by contact {ContactId}",
                ticketId, contactId);

            return Created($"api/portal/tickets/{ticketId}", result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding portal comment to ticket {TicketId}", ticketId);
            return StatusCode(500, new { message = "An error occurred while adding comment" });
        }
    }

    /// <summary>
    /// Rate a resolved/closed ticket
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="request">Rating request</param>
    /// <returns>Success message</returns>
    [HttpPost("tickets/{ticketId:guid}/rate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RateTicket(Guid ticketId, [FromBody] PortalRateTicketRequest request)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (request.Rating < 1 || request.Rating > 5)
            {
                return BadRequest(new { message = "Rating must be between 1 and 5" });
            }

            var result = await _portalService.RateTicketAsync(contactId.Value, ticketId, request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message });
            }

            _logger.LogInformation("Portal ticket {TicketId} rated {Rating} by contact {ContactId}",
                ticketId, request.Rating, contactId);

            return Ok(new { isSuccess = true, message = "Thank you for your feedback" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while rating portal ticket {TicketId}", ticketId);
            return StatusCode(500, new { message = "An error occurred while submitting rating" });
        }
    }

    /// <summary>
    /// Reopen a closed ticket
    /// </summary>
    /// <param name="ticketId">Ticket ID</param>
    /// <param name="request">Reopen request with reason</param>
    /// <returns>Reopened ticket details</returns>
    [HttpPost("tickets/{ticketId:guid}/reopen")]
    [ProducesResponseType(typeof(PortalTicketDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReopenTicket(Guid ticketId, [FromBody] PortalReopenTicketRequest request)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { message = "Reason for reopening is required" });
            }

            var result = await _portalService.ReopenTicketAsync(contactId.Value, ticketId, request);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.Message, errors = result.Errors });
            }

            _logger.LogInformation("Portal ticket {TicketId} reopened by contact {ContactId}",
                ticketId, contactId);

            return Ok(result.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while reopening portal ticket {TicketId}", ticketId);
            return StatusCode(500, new { message = "An error occurred while reopening ticket" });
        }
    }

    #endregion

    #region Assets

    /// <summary>
    /// Get paginated list of assets
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="searchTerm">Optional search term</param>
    /// <returns>Paginated list of assets</returns>
    [HttpGet("assets")]
    [ProducesResponseType(typeof(PagedResultDto<PortalAssetSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!HasPermission("ViewAssets"))
            {
                return StatusCode(403, new { message = "You don't have permission to view assets" });
            }

            var result = await _portalService.GetAssetsAsync(contactId.Value, page, pageSize, searchTerm);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal assets");
            return StatusCode(500, new { message = "An error occurred while loading assets" });
        }
    }

    /// <summary>
    /// Get asset details by ID
    /// </summary>
    /// <param name="id">Asset ID</param>
    /// <returns>Asset details</returns>
    [HttpGet("assets/{id:guid}")]
    [ProducesResponseType(typeof(PortalAssetDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssetById(Guid id)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!HasPermission("ViewAssets"))
            {
                return StatusCode(403, new { message = "You don't have permission to view assets" });
            }

            var asset = await _portalService.GetAssetByIdAsync(contactId.Value, id);
            if (asset == null)
            {
                return NotFound(new { message = "Asset not found" });
            }

            return Ok(asset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal asset {AssetId}", id);
            return StatusCode(500, new { message = "An error occurred while loading asset" });
        }
    }

    /// <summary>
    /// Get assets at a specific location
    /// </summary>
    /// <param name="locationId">Location ID</param>
    /// <returns>List of assets at the location</returns>
    [HttpGet("locations/{locationId:guid}/assets")]
    [ProducesResponseType(typeof(List<PortalAssetSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAssetsByLocation(Guid locationId)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!HasPermission("ViewAssets"))
            {
                return StatusCode(403, new { message = "You don't have permission to view assets" });
            }

            var assets = await _portalService.GetAssetsByLocationAsync(contactId.Value, locationId);
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting assets for location {LocationId}", locationId);
            return StatusCode(500, new { message = "An error occurred while loading assets" });
        }
    }

    #endregion

    #region Contracts

    /// <summary>
    /// Get paginated list of contracts
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20)</param>
    /// <param name="activeOnly">Filter active contracts only (default: true)</param>
    /// <returns>Paginated list of contracts</returns>
    [HttpGet("contracts")]
    [ProducesResponseType(typeof(PagedResultDto<PortalContractSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContracts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] bool? activeOnly = true)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!HasPermission("ViewContracts"))
            {
                return StatusCode(403, new { message = "You don't have permission to view contracts" });
            }

            var result = await _portalService.GetContractsAsync(contactId.Value, page, pageSize, activeOnly);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal contracts");
            return StatusCode(500, new { message = "An error occurred while loading contracts" });
        }
    }

    /// <summary>
    /// Get contract details by ID
    /// </summary>
    /// <param name="id">Contract ID</param>
    /// <returns>Contract details</returns>
    [HttpGet("contracts/{id:guid}")]
    [ProducesResponseType(typeof(PortalContractDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContractById(Guid id)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            if (!HasPermission("ViewContracts"))
            {
                return StatusCode(403, new { message = "You don't have permission to view contracts" });
            }

            var contract = await _portalService.GetContractByIdAsync(contactId.Value, id);
            if (contract == null)
            {
                return NotFound(new { message = "Contract not found" });
            }

            return Ok(contract);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal contract {ContractId}", id);
            return StatusCode(500, new { message = "An error occurred while loading contract" });
        }
    }

    #endregion

    #region Locations

    /// <summary>
    /// Get list of customer locations
    /// </summary>
    /// <returns>List of locations</returns>
    [HttpGet("locations")]
    [ProducesResponseType(typeof(List<PortalLocationSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLocations()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var locations = await _portalService.GetLocationsAsync(contactId.Value);
            return Ok(locations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal locations");
            return StatusCode(500, new { message = "An error occurred while loading locations" });
        }
    }

    #endregion

    #region Lookup Data

    /// <summary>
    /// Get categories for ticket creation
    /// </summary>
    /// <returns>List of categories</returns>
    [HttpGet("lookups/categories")]
    [ProducesResponseType(typeof(List<PortalCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategories()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var categories = await _portalService.GetCategoriesAsync(contactId.Value);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal categories");
            return StatusCode(500, new { message = "An error occurred while loading categories" });
        }
    }

    /// <summary>
    /// Get priorities for ticket creation
    /// </summary>
    /// <returns>List of priorities</returns>
    [HttpGet("lookups/priorities")]
    [ProducesResponseType(typeof(List<PortalPriorityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPriorities()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var priorities = await _portalService.GetPrioritiesAsync(contactId.Value);
            return Ok(priorities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal priorities");
            return StatusCode(500, new { message = "An error occurred while loading priorities" });
        }
    }

    /// <summary>
    /// Get statuses for filtering
    /// </summary>
    /// <returns>List of statuses</returns>
    [HttpGet("lookups/statuses")]
    [ProducesResponseType(typeof(List<PortalStatusDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStatuses()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var statuses = await _portalService.GetStatusesAsync(contactId.Value);
            return Ok(statuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal statuses");
            return StatusCode(500, new { message = "An error occurred while loading statuses" });
        }
    }

    /// <summary>
    /// Get assets for ticket creation (dropdown)
    /// </summary>
    /// <param name="locationId">Optional location filter</param>
    /// <returns>List of assets for dropdown</returns>
    [HttpGet("lookups/assets")]
    [ProducesResponseType(typeof(List<PortalAssetSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAssetLookups([FromQuery] Guid? locationId = null)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var assets = await _portalService.GetAssetLookupsAsync(contactId.Value, locationId);
            return Ok(assets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal asset lookups");
            return StatusCode(500, new { message = "An error occurred while loading assets" });
        }
    }

    /// <summary>
    /// Get product categories for ticket creation (dropdown)
    /// </summary>
    /// <param name="parentId">Optional parent category filter</param>
    /// <returns>List of product categories</returns>
    [HttpGet("lookups/product-categories")]
    [ProducesResponseType(typeof(List<PortalProductCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProductCategories([FromQuery] Guid? parentId = null)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var categories = await _portalService.GetProductCategoriesAsync(contactId.Value, parentId);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal product categories");
            return StatusCode(500, new { message = "An error occurred while loading product categories" });
        }
    }

    /// <summary>
    /// Get products for ticket creation (dropdown)
    /// </summary>
    /// <param name="categoryId">Optional category filter</param>
    /// <param name="brandId">Optional brand filter</param>
    /// <returns>List of products</returns>
    [HttpGet("lookups/products")]
    [ProducesResponseType(typeof(List<PortalProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProducts([FromQuery] Guid? categoryId = null, [FromQuery] Guid? brandId = null)
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var products = await _portalService.GetProductsAsync(contactId.Value, categoryId, brandId);
            return Ok(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal products");
            return StatusCode(500, new { message = "An error occurred while loading products" });
        }
    }

    /// <summary>
    /// Get brands (OEM) for ticket creation (dropdown)
    /// </summary>
    /// <returns>List of brands</returns>
    [HttpGet("lookups/brands")]
    [ProducesResponseType(typeof(List<PortalBrandDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBrands()
    {
        try
        {
            var contactId = GetContactIdFromClaims();
            if (contactId == null)
            {
                return Unauthorized(new { message = "Invalid portal session" });
            }

            var brands = await _portalService.GetBrandsAsync(contactId.Value);
            return Ok(brands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting portal brands");
            return StatusCode(500, new { message = "An error occurred while loading brands" });
        }
    }

    #endregion

    #region Private Methods

    private Guid? GetContactIdFromClaims()
    {
        // Check for portal-specific claim first
        var contactIdClaim = User.FindFirst("ContactId")?.Value;
        if (!string.IsNullOrEmpty(contactIdClaim) && Guid.TryParse(contactIdClaim, out var contactId))
        {
            return contactId;
        }

        // Fallback to NameIdentifier for portal users
        var isPortalUser = User.FindFirst("IsPortalUser")?.Value;
        if (isPortalUser == "true")
        {
            var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(nameIdClaim) && Guid.TryParse(nameIdClaim, out var id))
            {
                return id;
            }
        }

        return null;
    }

    private bool HasPermission(string permission)
    {
        var permissionClaims = User.FindAll("PortalPermission").Select(c => c.Value);
        return permissionClaims.Contains(permission);
    }

    #endregion
}

/// <summary>
/// Generic paged result DTO for API responses
/// </summary>
public class PagedResultDto<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}
