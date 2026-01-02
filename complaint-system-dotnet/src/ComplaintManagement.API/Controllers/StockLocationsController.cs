using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockLocationsController : ControllerBase
{
    private readonly IStockLocationService _stockLocationService;

    public StockLocationsController(IStockLocationService stockLocationService)
    {
        _stockLocationService = stockLocationService;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found in token"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in token"));

    /// <summary>
    /// Get all stock locations with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _stockLocationService.GetAllAsync(
            GetCompanyId(), page, pageSize, searchTerm, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get stock locations for dropdown
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup([FromQuery] bool activeOnly = true)
    {
        var result = await _stockLocationService.GetLookupAsync(GetCompanyId(), activeOnly);
        return Ok(result);
    }

    /// <summary>
    /// Get stock locations hierarchy
    /// </summary>
    [HttpGet("hierarchy")]
    public async Task<IActionResult> GetHierarchy([FromQuery] Guid? parentId = null)
    {
        var result = await _stockLocationService.GetHierarchyAsync(GetCompanyId(), parentId);
        return Ok(result);
    }

    /// <summary>
    /// Get a single stock location by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockLocationService.GetByIdAsync(id, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get stock location by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _stockLocationService.GetByCodeAsync(code, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new stock location
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockLocationService.CreateAsync(request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing stock location
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockLocationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockLocationService.UpdateAsync(id, request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a stock location
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockLocationService.DeleteAsync(id, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Set a stock location as default
    /// </summary>
    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetDefault(Guid id)
    {
        var result = await _stockLocationService.SetDefaultAsync(id, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }
}
