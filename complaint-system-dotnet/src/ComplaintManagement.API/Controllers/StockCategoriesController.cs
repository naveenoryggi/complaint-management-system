using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockCategoriesController : ControllerBase
{
    private readonly IStockCategoryService _stockCategoryService;

    public StockCategoriesController(IStockCategoryService stockCategoryService)
    {
        _stockCategoryService = stockCategoryService;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found in token"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in token"));

    /// <summary>
    /// Get all stock categories with pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] bool? isActive = null)
    {
        var result = await _stockCategoryService.GetAllAsync(
            GetCompanyId(), page, pageSize, searchTerm, isActive);
        return Ok(result);
    }

    /// <summary>
    /// Get stock categories for dropdown
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup([FromQuery] bool activeOnly = true)
    {
        var result = await _stockCategoryService.GetLookupAsync(GetCompanyId(), activeOnly);
        return Ok(result);
    }

    /// <summary>
    /// Get a single stock category by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockCategoryService.GetByIdAsync(id, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get stock category by code
    /// </summary>
    [HttpGet("code/{code}")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _stockCategoryService.GetByCodeAsync(code, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new stock category
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockCategoryService.CreateAsync(request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing stock category
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockCategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockCategoryService.UpdateAsync(id, request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a stock category
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockCategoryService.DeleteAsync(id, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Seed default stock categories
    /// </summary>
    [HttpPost("seed")]
    public async Task<IActionResult> SeedDefaults()
    {
        await _stockCategoryService.SeedDefaultCategoriesAsync(GetCompanyId(), GetUserId());
        return Ok(new { message = "Default stock categories seeded successfully" });
    }
}
