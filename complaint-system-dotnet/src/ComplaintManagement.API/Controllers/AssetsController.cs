using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Enums.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for asset management operations
/// Requires AssetManagement license module
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiresLicense(LicenseModule.AssetManagement)]
public class AssetsController : ControllerBase
{
    private readonly IAssetService _assetService;
    private readonly ILogger<AssetsController> _logger;

    public AssetsController(IAssetService assetService, ILogger<AssetsController> logger)
    {
        _assetService = assetService;
        _logger = logger;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirst("CompanyId")?.Value ?? throw new UnauthorizedAccessException("Company ID not found in token"));

    #region Asset CRUD Endpoints

    /// <summary>
    /// Gets a paginated list of assets with optional filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAssets(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] Guid? contractId = null,
        [FromQuery] AssetType? type = null,
        [FromQuery] AssetStatus? status = null,
        [FromQuery] AssetCondition? condition = null,
        [FromQuery] bool? isOperational = null,
        [FromQuery] bool? isUnderWarranty = null,
        [FromQuery] bool? isUnderContract = null,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var companyId = GetCompanyId();
        var result = await _assetService.GetAssetsAsync(
            companyId, page, pageSize, customerId, locationId, productId, contractId,
            type, status, condition, isOperational, isUnderWarranty, isUnderContract,
            searchTerm, sortBy, sortDescending);

        return Ok(result);
    }

    /// <summary>
    /// Gets an asset by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetAsset(Guid id)
    {
        var companyId = GetCompanyId();
        var asset = await _assetService.GetAssetByIdAsync(companyId, id);

        if (asset == null)
            return NotFound(new { message = "Asset not found" });

        return Ok(asset);
    }

    /// <summary>
    /// Gets an asset by asset tag
    /// </summary>
    [HttpGet("by-tag/{assetTag}")]
    public async Task<IActionResult> GetAssetByTag(string assetTag)
    {
        var companyId = GetCompanyId();
        var asset = await _assetService.GetAssetByTagAsync(companyId, assetTag);

        if (asset == null)
            return NotFound(new { message = "Asset not found" });

        return Ok(asset);
    }

    /// <summary>
    /// Gets an asset by serial number
    /// </summary>
    [HttpGet("by-serial/{serialNumber}")]
    public async Task<IActionResult> GetAssetBySerialNumber(string serialNumber)
    {
        var companyId = GetCompanyId();
        var asset = await _assetService.GetAssetBySerialNumberAsync(companyId, serialNumber);

        if (asset == null)
            return NotFound(new { message = "Asset not found" });

        return Ok(asset);
    }

    /// <summary>
    /// Creates a new asset
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateAsset([FromBody] CreateAssetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var companyId = GetCompanyId();
        var result = await _assetService.CreateAssetAsync(companyId, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return CreatedAtAction(nameof(GetAsset), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// Updates an existing asset
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateAsset(Guid id, [FromBody] UpdateAssetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var companyId = GetCompanyId();
        var result = await _assetService.UpdateAssetAsync(companyId, id, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Updates asset status
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateAssetStatus(Guid id, [FromBody] UpdateAssetStatusRequest request)
    {
        var companyId = GetCompanyId();
        var result = await _assetService.UpdateAssetStatusAsync(companyId, id, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes an asset
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteAsset(Guid id)
    {
        var companyId = GetCompanyId();
        var result = await _assetService.DeleteAssetAsync(companyId, id);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return NoContent();
    }

    #endregion

    #region Asset Lookup Endpoints

    /// <summary>
    /// Gets assets for dropdown lookups
    /// This endpoint skips the class-level license requirement to allow asset lookups
    /// from related modules (like Asset Assignments) without requiring the full AssetManagement license
    /// </summary>
    [HttpGet("lookup")]
    [SkipLicenseCheck]  // Allow access from related modules without AssetManagement license
    public async Task<IActionResult> GetAssetLookups(
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] AssetStatus? status = null,
        [FromQuery] bool? isOperational = null)
    {
        var companyId = GetCompanyId();
        var lookups = await _assetService.GetAssetLookupsAsync(companyId, customerId, locationId, status, isOperational);
        return Ok(Result<List<AssetLookupDto>>.Success(lookups));
    }

    /// <summary>
    /// Searches assets by various criteria
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> SearchAssets([FromQuery] string term, [FromQuery] int maxResults = 10)
    {
        var companyId = GetCompanyId();
        var results = await _assetService.SearchAssetsAsync(companyId, term, maxResults);
        return Ok(results);
    }

    #endregion

    #region Asset Operations Endpoints

    /// <summary>
    /// Transfers an asset to another customer/location
    /// </summary>
    [HttpPost("{id:guid}/transfer")]
    public async Task<IActionResult> TransferAsset(Guid id, [FromBody] TransferAssetRequest request)
    {
        var companyId = GetCompanyId();
        var result = await _assetService.TransferAssetAsync(companyId, id, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Records a meter reading for an asset
    /// </summary>
    [HttpPost("{id:guid}/meter-reading")]
    public async Task<IActionResult> RecordMeterReading(Guid id, [FromBody] RecordMeterReadingRequest request)
    {
        var companyId = GetCompanyId();
        var result = await _assetService.RecordMeterReadingAsync(companyId, id, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Gets child assets of a parent asset
    /// </summary>
    [HttpGet("{id:guid}/children")]
    public async Task<IActionResult> GetChildAssets(Guid id)
    {
        var companyId = GetCompanyId();
        var children = await _assetService.GetChildAssetsAsync(companyId, id);
        return Ok(children);
    }

    /// <summary>
    /// Gets assets by customer
    /// </summary>
    [HttpGet("customer/{customerId:guid}")]
    public async Task<IActionResult> GetCustomerAssets(Guid customerId)
    {
        var companyId = GetCompanyId();
        var assets = await _assetService.GetCustomerAssetsAsync(companyId, customerId);
        return Ok(assets);
    }

    /// <summary>
    /// Gets assets at a specific location
    /// </summary>
    [HttpGet("location/{locationId:guid}")]
    public async Task<IActionResult> GetLocationAssets(Guid locationId)
    {
        var companyId = GetCompanyId();
        var assets = await _assetService.GetLocationAssetsAsync(companyId, locationId);
        return Ok(assets);
    }

    /// <summary>
    /// Gets assets covered under a specific contract
    /// </summary>
    [HttpGet("contract/{contractId:guid}")]
    public async Task<IActionResult> GetContractAssets(Guid contractId)
    {
        var companyId = GetCompanyId();
        var assets = await _assetService.GetContractAssetsAsync(companyId, contractId);
        return Ok(assets);
    }

    /// <summary>
    /// Gets assets needing service
    /// </summary>
    [HttpGet("needing-service")]
    public async Task<IActionResult> GetAssetsNeedingService([FromQuery] int daysAhead = 7)
    {
        var companyId = GetCompanyId();
        var assets = await _assetService.GetAssetsNeedingServiceAsync(companyId, daysAhead);
        return Ok(assets);
    }

    /// <summary>
    /// Gets assets with warranty expiring soon
    /// </summary>
    [HttpGet("expiring-warranty")]
    public async Task<IActionResult> GetAssetsWithExpiringWarranty([FromQuery] int daysAhead = 30)
    {
        var companyId = GetCompanyId();
        var assets = await _assetService.GetAssetsWithExpiringWarrantyAsync(companyId, daysAhead);
        return Ok(assets);
    }

    #endregion

    #region Asset Statistics Endpoints

    /// <summary>
    /// Gets asset statistics for dashboard
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetAssetStatistics([FromQuery] Guid? customerId = null)
    {
        var companyId = GetCompanyId();
        var stats = await _assetService.GetAssetStatisticsAsync(companyId, customerId);
        return Ok(stats);
    }

    #endregion

    #region Service History Endpoints

    /// <summary>
    /// Gets service history for a specific asset
    /// </summary>
    [HttpGet("{id:guid}/service-history")]
    public async Task<IActionResult> GetAssetServiceHistory(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] ServiceType? serviceType = null,
        [FromQuery] ServiceResult? result = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var companyId = GetCompanyId();
        var history = await _assetService.GetAssetServiceHistoryAsync(
            companyId, id, page, pageSize, serviceType, result, fromDate, toDate);
        return Ok(history);
    }

    /// <summary>
    /// Gets all service history records
    /// </summary>
    [HttpGet("service-history")]
    public async Task<IActionResult> GetAllServiceHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? customerId = null,
        [FromQuery] Guid? technicianId = null,
        [FromQuery] ServiceType? serviceType = null,
        [FromQuery] ServiceResult? result = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var companyId = GetCompanyId();
        var history = await _assetService.GetAllServiceHistoryAsync(
            companyId, page, pageSize, customerId, technicianId,
            serviceType, result, fromDate, toDate, sortBy, sortDescending);
        return Ok(history);
    }

    /// <summary>
    /// Gets a specific service history record
    /// </summary>
    [HttpGet("service-history/{serviceHistoryId:guid}")]
    public async Task<IActionResult> GetServiceHistoryById(Guid serviceHistoryId)
    {
        var companyId = GetCompanyId();
        var history = await _assetService.GetServiceHistoryByIdAsync(companyId, serviceHistoryId);

        if (history == null)
            return NotFound(new { message = "Service history record not found" });

        return Ok(history);
    }

    /// <summary>
    /// Creates a new service history record
    /// </summary>
    [HttpPost("service-history")]
    public async Task<IActionResult> CreateServiceHistory([FromBody] CreateServiceHistoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var companyId = GetCompanyId();
        var result = await _assetService.CreateServiceHistoryAsync(companyId, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return CreatedAtAction(
            nameof(GetServiceHistoryById),
            new { serviceHistoryId = result.Data!.Id },
            result.Data);
    }

    /// <summary>
    /// Updates a service history record
    /// </summary>
    [HttpPut("service-history/{serviceHistoryId:guid}")]
    public async Task<IActionResult> UpdateServiceHistory(
        Guid serviceHistoryId,
        [FromBody] UpdateServiceHistoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var companyId = GetCompanyId();
        var result = await _assetService.UpdateServiceHistoryAsync(companyId, serviceHistoryId, request);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return Ok(result.Data);
    }

    /// <summary>
    /// Deletes a service history record
    /// </summary>
    [HttpDelete("service-history/{serviceHistoryId:guid}")]
    public async Task<IActionResult> DeleteServiceHistory(Guid serviceHistoryId)
    {
        var companyId = GetCompanyId();
        var result = await _assetService.DeleteServiceHistoryAsync(companyId, serviceHistoryId);

        if (!result.IsSuccess)
            return BadRequest(new { message = result.Message, errors = result.Errors });

        return NoContent();
    }

    /// <summary>
    /// Gets service history statistics
    /// </summary>
    [HttpGet("service-history/statistics")]
    public async Task<IActionResult> GetServiceHistoryStatistics(
        [FromQuery] Guid? assetId = null,
        [FromQuery] Guid? customerId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var companyId = GetCompanyId();
        var stats = await _assetService.GetServiceHistoryStatisticsAsync(
            companyId, assetId, customerId, fromDate, toDate);
        return Ok(stats);
    }

    #endregion

    #region Validation Endpoints

    /// <summary>
    /// Validates if an asset tag is unique
    /// </summary>
    [HttpGet("validate/asset-tag")]
    public async Task<IActionResult> ValidateAssetTag(
        [FromQuery] string assetTag,
        [FromQuery] Guid? excludeId = null)
    {
        var companyId = GetCompanyId();
        var isUnique = await _assetService.IsAssetTagUniqueAsync(companyId, assetTag, excludeId);
        return Ok(new { isUnique });
    }

    /// <summary>
    /// Validates if a serial number is unique
    /// </summary>
    [HttpGet("validate/serial-number")]
    public async Task<IActionResult> ValidateSerialNumber(
        [FromQuery] string serialNumber,
        [FromQuery] Guid? excludeId = null)
    {
        var companyId = GetCompanyId();
        var isUnique = await _assetService.IsSerialNumberUniqueAsync(companyId, serialNumber, excludeId);
        return Ok(new { isUnique });
    }

    /// <summary>
    /// Checks if an asset is under warranty
    /// </summary>
    [HttpGet("{id:guid}/check-warranty")]
    public async Task<IActionResult> CheckWarranty(Guid id)
    {
        var isUnderWarranty = await _assetService.IsAssetUnderWarrantyAsync(id);
        return Ok(new { isUnderWarranty });
    }

    /// <summary>
    /// Checks if an asset is covered under a contract
    /// </summary>
    [HttpGet("{id:guid}/check-contract")]
    public async Task<IActionResult> CheckContract(Guid id)
    {
        var isUnderContract = await _assetService.IsAssetUnderContractAsync(id);
        return Ok(new { isUnderContract });
    }

    /// <summary>
    /// Generates a new asset tag
    /// </summary>
    [HttpGet("generate-tag")]
    public async Task<IActionResult> GenerateAssetTag()
    {
        var companyId = GetCompanyId();
        var assetTag = await _assetService.GenerateAssetTagAsync(companyId);
        return Ok(new { assetTag });
    }

    /// <summary>
    /// Generates a new service number
    /// </summary>
    [HttpGet("generate-service-number")]
    public async Task<IActionResult> GenerateServiceNumber()
    {
        var companyId = GetCompanyId();
        var serviceNumber = await _assetService.GenerateServiceNumberAsync(companyId);
        return Ok(new { serviceNumber });
    }

    #endregion
}
