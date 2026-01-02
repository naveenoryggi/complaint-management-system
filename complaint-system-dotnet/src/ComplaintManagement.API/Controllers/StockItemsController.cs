using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockItemsController : ControllerBase
{
    private readonly IStockItemService _stockItemService;

    public StockItemsController(IStockItemService stockItemService)
    {
        _stockItemService = stockItemService;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found in token"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found in token"));

    /// <summary>
    /// Get all stock items with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? locationId = null,
        [FromQuery] Guid? stockCategoryId = null,
        [FromQuery] Guid? productId = null,
        [FromQuery] bool? lowStock = null)
    {
        var result = await _stockItemService.GetAllAsync(
            GetCompanyId(), page, pageSize, searchTerm, locationId, stockCategoryId, productId, lowStock);
        return Ok(result);
    }

    /// <summary>
    /// Get stock items for dropdown selection
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> GetLookup(
        [FromQuery] Guid? locationId = null,
        [FromQuery] Guid? stockCategoryId = null,
        [FromQuery] bool availableOnly = true)
    {
        var result = await _stockItemService.GetLookupAsync(GetCompanyId(), locationId, stockCategoryId, availableOnly);
        return Ok(result);
    }

    /// <summary>
    /// Get a single stock item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _stockItemService.GetByIdAsync(id, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get stock item by product, location, and category combination
    /// </summary>
    [HttpGet("by-product-location")]
    public async Task<IActionResult> GetByProductAndLocation(
        [FromQuery] Guid productId,
        [FromQuery] Guid? locationId,
        [FromQuery] Guid stockCategoryId)
    {
        var result = await _stockItemService.GetByProductAndLocationAsync(
            productId, locationId, stockCategoryId, GetCompanyId());
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Create a new stock item
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStockItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockItemService.CreateAsync(request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Update an existing stock item
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockItemRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _stockItemService.UpdateAsync(id, request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Delete a stock item (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _stockItemService.DeleteAsync(id, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Adjust stock quantity (increase or decrease)
    /// </summary>
    [HttpPost("{id}/adjust")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        request.StockItemId = id;
        var result = await _stockItemService.AdjustStockAsync(request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Transfer stock to another location
    /// </summary>
    [HttpPost("{id}/transfer")]
    public async Task<IActionResult> TransferStock(Guid id, [FromBody] TransferStockRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        request.StockItemId = id;
        var result = await _stockItemService.TransferStockAsync(request, GetCompanyId(), GetUserId());
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Get items with low stock (below minimum quantity)
    /// </summary>
    [HttpGet("low-stock")]
    public async Task<IActionResult> GetLowStockItems()
    {
        var result = await _stockItemService.GetLowStockItemsAsync(GetCompanyId());
        return Ok(result);
    }

    /// <summary>
    /// Get items expiring within specified days
    /// </summary>
    [HttpGet("expiring")]
    public async Task<IActionResult> GetExpiringItems([FromQuery] int daysAhead = 30)
    {
        var result = await _stockItemService.GetExpiringItemsAsync(GetCompanyId(), daysAhead);
        return Ok(result);
    }
}
