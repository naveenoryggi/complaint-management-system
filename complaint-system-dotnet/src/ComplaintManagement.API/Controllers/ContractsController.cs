using ComplaintManagement.Application.DTOs.Contract;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing service contracts (AMC, Warranty, SLA, Support agreements)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ContractsController : ControllerBase
{
    private readonly IContractService _contractService;
    private readonly ILogger<ContractsController> _logger;

    public ContractsController(IContractService contractService, ILogger<ContractsController> logger)
    {
        _contractService = contractService;
        _logger = logger;
    }

    #region Helpers

    private Guid GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            throw new UnauthorizedAccessException("Company ID not found in claims");
        }
        return companyId;
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAccessException("User ID not found in claims");
        }
        return userId;
    }

    #endregion

    #region Contract CRUD

    /// <summary>
    /// Get all contracts with optional filtering and pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContracts([FromQuery] ContractSearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.GetContractsAsync(companyId, request, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contracts");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving contracts" });
        }
    }

    /// <summary>
    /// Get a specific contract by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContract(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var contract = await _contractService.GetContractByIdAsync(companyId, id, cancellationToken);

            if (contract == null)
                return NotFound(new { isSuccess = false, message = $"Contract {id} not found" });

            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving the contract" });
        }
    }

    /// <summary>
    /// Get a contract by contract number
    /// </summary>
    [HttpGet("by-number/{contractNumber}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContractByNumber(string contractNumber, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var contract = await _contractService.GetContractByNumberAsync(companyId, contractNumber, cancellationToken);

            if (contract == null)
                return NotFound(new { isSuccess = false, message = $"Contract {contractNumber} not found" });

            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract by number {ContractNumber}", contractNumber);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving the contract" });
        }
    }

    /// <summary>
    /// Create a new contract
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.CreateContractAsync(companyId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetContract), new { id = contract.Id }, new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating contract");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while creating the contract" });
        }
    }

    /// <summary>
    /// Update an existing contract
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContract(Guid id, [FromBody] UpdateContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.UpdateContractAsync(companyId, id, request, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while updating the contract" });
        }
    }

    /// <summary>
    /// Delete a contract (only draft contracts)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContract(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var result = await _contractService.DeleteContractAsync(companyId, id, userId, cancellationToken);

            if (!result)
                return NotFound(new { isSuccess = false, message = $"Contract {id} not found" });

            return Ok(new { isSuccess = true, data = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while deleting the contract" });
        }
    }

    #endregion

    #region Contract Status Operations

    /// <summary>
    /// Activate a draft or pending contract
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivateContract(Guid id, [FromBody] ActivateContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.ActivateContractAsync(companyId, id, request, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while activating the contract" });
        }
    }

    /// <summary>
    /// Renew a contract
    /// </summary>
    [HttpPost("{id:guid}/renew")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RenewContract(Guid id, [FromBody] RenewContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.RenewContractAsync(companyId, id, request, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error renewing contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while renewing the contract" });
        }
    }

    /// <summary>
    /// Terminate a contract
    /// </summary>
    [HttpPost("{id:guid}/terminate")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> TerminateContract(Guid id, [FromBody] TerminateContractRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.TerminateContractAsync(companyId, id, request, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error terminating contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while terminating the contract" });
        }
    }

    /// <summary>
    /// Suspend an active contract
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SuspendContract(Guid id, [FromBody] SuspendRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.SuspendContractAsync(companyId, id, request.Reason, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error suspending contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while suspending the contract" });
        }
    }

    /// <summary>
    /// Reactivate a suspended contract
    /// </summary>
    [HttpPost("{id:guid}/reactivate")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReactivateContract(Guid id, [FromBody] ReactivateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.ReactivateContractAsync(companyId, id, request.Notes, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reactivating contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while reactivating the contract" });
        }
    }

    #endregion

    #region Contract Items

    /// <summary>
    /// Get all items for a contract
    /// </summary>
    [HttpGet("{id:guid}/items")]
    [ProducesResponseType(typeof(List<ContractItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContractItems(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var items = await _contractService.GetContractItemsAsync(companyId, id, cancellationToken);
            return Ok(new { isSuccess = true, data = items });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract items for {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving contract items" });
        }
    }

    /// <summary>
    /// Add an item to a contract
    /// </summary>
    [HttpPost("{id:guid}/items")]
    [ProducesResponseType(typeof(ContractItemDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> AddContractItem(Guid id, [FromBody] CreateContractItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var item = await _contractService.AddContractItemAsync(companyId, id, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetContractItems), new { id }, new { isSuccess = true, data = item });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding item to contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while adding the contract item" });
        }
    }

    /// <summary>
    /// Update a contract item
    /// </summary>
    [HttpPut("{contractId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(typeof(ContractItemDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContractItem(Guid contractId, Guid itemId, [FromBody] UpdateContractItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var item = await _contractService.UpdateContractItemAsync(companyId, contractId, itemId, request, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = item });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating contract item {ItemId}", itemId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while updating the contract item" });
        }
    }

    /// <summary>
    /// Remove an item from a contract
    /// </summary>
    [HttpDelete("{contractId:guid}/items/{itemId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveContractItem(Guid contractId, Guid itemId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var result = await _contractService.RemoveContractItemAsync(companyId, contractId, itemId, userId, cancellationToken);

            if (!result)
                return NotFound(new { isSuccess = false, message = $"Contract item {itemId} not found" });

            return Ok(new { isSuccess = true, data = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing contract item {ItemId}", itemId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while removing the contract item" });
        }
    }

    #endregion

    #region Coverage Checks

    /// <summary>
    /// Check if a customer has active coverage
    /// </summary>
    [HttpGet("coverage/customer/{customerId:guid}")]
    [ProducesResponseType(typeof(CoverageCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckCustomerCoverage(Guid customerId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.CheckCustomerCoverageAsync(companyId, customerId, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking customer coverage for {CustomerId}", customerId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while checking coverage" });
        }
    }

    /// <summary>
    /// Check if a product is covered for a customer
    /// </summary>
    [HttpGet("coverage/customer/{customerId:guid}/product/{productId:guid}")]
    [ProducesResponseType(typeof(CoverageCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckProductCoverage(Guid customerId, Guid productId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.CheckProductCoverageAsync(companyId, customerId, productId, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking product coverage");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while checking coverage" });
        }
    }

    /// <summary>
    /// Check if an asset is covered
    /// </summary>
    [HttpGet("coverage/asset/{assetId:guid}")]
    [ProducesResponseType(typeof(CoverageCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckAssetCoverage(Guid assetId, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.CheckAssetCoverageAsync(companyId, assetId, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking asset coverage for {AssetId}", assetId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while checking coverage" });
        }
    }

    #endregion

    #region Reporting & Statistics

    /// <summary>
    /// Get contracts expiring within specified days
    /// </summary>
    [HttpGet("expiring")]
    [ProducesResponseType(typeof(ExpiringContractsReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringContracts([FromQuery] int days = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.GetExpiringContractsAsync(companyId, days, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expiring contracts");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving expiring contracts" });
        }
    }

    /// <summary>
    /// Get contracts for a specific customer
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(typeof(List<ContractSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContractsByCustomer(Guid customerId, [FromQuery] bool activeOnly = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCompanyId();
            var contracts = await _contractService.GetContractsByCustomerAsync(companyId, customerId, activeOnly, cancellationToken);
            return Ok(new { isSuccess = true, data = contracts });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contracts for customer {CustomerId}", customerId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving customer contracts" });
        }
    }

    /// <summary>
    /// Get renewal history for a contract
    /// </summary>
    [HttpGet("{id:guid}/renewals")]
    [ProducesResponseType(typeof(List<ContractRenewalSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRenewalHistory(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var history = await _contractService.GetRenewalHistoryAsync(companyId, id, cancellationToken);
            return Ok(new { isSuccess = true, data = history });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting renewal history for contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving renewal history" });
        }
    }

    /// <summary>
    /// Get contract statistics
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ContractStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var stats = await _contractService.GetContractStatisticsAsync(companyId, cancellationToken);
            return Ok(new { isSuccess = true, data = stats });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract statistics");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving statistics" });
        }
    }

    /// <summary>
    /// Get contract lookup list for dropdowns
    /// </summary>
    [HttpGet("lookup")]
    [ProducesResponseType(typeof(List<ContractLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLookup([FromQuery] Guid? customerId = null, [FromQuery] ContractStatus? status = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCompanyId();
            var contracts = await _contractService.GetContractLookupAsync(companyId, customerId, status, cancellationToken);
            return Ok(new { isSuccess = true, data = contracts });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting contract lookup");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving contract lookup" });
        }
    }

    #endregion

    #region Usage Tracking

    /// <summary>
    /// Record support hours used
    /// </summary>
    [HttpPost("{id:guid}/usage/hours")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordUsageHours(Guid id, [FromBody] RecordUsageRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.RecordUsageHoursAsync(companyId, id, request.Hours, request.Notes, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording usage hours for contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while recording usage" });
        }
    }

    /// <summary>
    /// Record an incident against a contract
    /// </summary>
    [HttpPost("{id:guid}/usage/incident")]
    [ProducesResponseType(typeof(ContractDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RecordIncident(Guid id, [FromBody] RecordIncidentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var contract = await _contractService.RecordIncidentAsync(companyId, id, request.Notes, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = contract });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording incident for contract {ContractId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while recording the incident" });
        }
    }

    /// <summary>
    /// Process auto-renewals for due contracts
    /// </summary>
    [HttpPost("process-auto-renewals")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ProcessAutoRenewals(CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var count = await _contractService.ProcessAutoRenewalsAsync(companyId, cancellationToken);
            return Ok(new { isSuccess = true, data = new { message = $"Processed {count} auto-renewals", renewedCount = count } });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing auto-renewals");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while processing auto-renewals" });
        }
    }

    #endregion

    #region Warranty CRUD

    /// <summary>
    /// Get all warranties with optional filtering and pagination
    /// </summary>
    [HttpGet("warranties")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarranties([FromQuery] WarrantySearchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.GetWarrantiesAsync(companyId, request, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warranties");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving warranties" });
        }
    }

    /// <summary>
    /// Get a specific warranty by ID
    /// </summary>
    [HttpGet("warranties/{id:guid}")]
    [ProducesResponseType(typeof(WarrantyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWarranty(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var warranty = await _contractService.GetWarrantyByIdAsync(companyId, id, cancellationToken);

            if (warranty == null)
                return NotFound(new { isSuccess = false, message = $"Warranty {id} not found" });

            return Ok(new { isSuccess = true, data = warranty });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warranty {WarrantyId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving the warranty" });
        }
    }

    /// <summary>
    /// Get a warranty by code
    /// </summary>
    [HttpGet("warranties/by-code/{code}")]
    [ProducesResponseType(typeof(WarrantyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetWarrantyByCode(string code, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var warranty = await _contractService.GetWarrantyByCodeAsync(companyId, code, cancellationToken);

            if (warranty == null)
                return NotFound(new { isSuccess = false, message = $"Warranty with code {code} not found" });

            return Ok(new { isSuccess = true, data = warranty });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warranty by code {Code}", code);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving the warranty" });
        }
    }

    /// <summary>
    /// Create a new warranty
    /// </summary>
    [HttpPost("warranties")]
    [ProducesResponseType(typeof(WarrantyDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateWarranty([FromBody] CreateWarrantyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var warranty = await _contractService.CreateWarrantyAsync(companyId, request, userId, cancellationToken);
            return CreatedAtAction(nameof(GetWarranty), new { id = warranty.Id }, new { isSuccess = true, data = warranty });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating warranty");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while creating the warranty" });
        }
    }

    /// <summary>
    /// Update an existing warranty
    /// </summary>
    [HttpPut("warranties/{id:guid}")]
    [ProducesResponseType(typeof(WarrantyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateWarranty(Guid id, [FromBody] UpdateWarrantyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var warranty = await _contractService.UpdateWarrantyAsync(companyId, id, request, userId, cancellationToken);
            return Ok(new { isSuccess = true, data = warranty });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating warranty {WarrantyId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while updating the warranty" });
        }
    }

    /// <summary>
    /// Delete a warranty
    /// </summary>
    [HttpDelete("warranties/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteWarranty(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var userId = GetUserId();

            var result = await _contractService.DeleteWarrantyAsync(companyId, id, userId, cancellationToken);

            if (!result)
                return NotFound(new { isSuccess = false, message = $"Warranty {id} not found" });

            return Ok(new { isSuccess = true, data = true });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting warranty {WarrantyId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while deleting the warranty" });
        }
    }

    /// <summary>
    /// Get warranty lookup list for dropdowns
    /// </summary>
    [HttpGet("warranties/lookup")]
    [ProducesResponseType(typeof(List<WarrantyLookupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWarrantyLookup([FromQuery] Guid? productId = null, [FromQuery] Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var companyId = GetCompanyId();
            var warranties = await _contractService.GetWarrantyLookupAsync(companyId, productId, categoryId, cancellationToken);
            return Ok(new { isSuccess = true, data = warranties });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting warranty lookup");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while retrieving warranty lookup" });
        }
    }

    /// <summary>
    /// Check warranty coverage for a product
    /// </summary>
    [HttpGet("warranties/check/{productId:guid}")]
    [ProducesResponseType(typeof(WarrantyCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckWarrantyCoverage(Guid productId, [FromQuery] DateTime purchaseDate, CancellationToken cancellationToken)
    {
        try
        {
            var companyId = GetCompanyId();
            var result = await _contractService.CheckWarrantyCoverageAsync(companyId, productId, purchaseDate, cancellationToken);
            return Ok(new { isSuccess = true, data = result });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking warranty coverage for product {ProductId}", productId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while checking warranty coverage" });
        }
    }

    #endregion
}

#region Request DTOs

public class SuspendRequest
{
    public string? Reason { get; set; }
}

public class ReactivateRequest
{
    public string? Notes { get; set; }
}

public class RecordUsageRequest
{
    public int Hours { get; set; }
    public string? Notes { get; set; }
}

public class RecordIncidentRequest
{
    public string? Notes { get; set; }
}

#endregion
