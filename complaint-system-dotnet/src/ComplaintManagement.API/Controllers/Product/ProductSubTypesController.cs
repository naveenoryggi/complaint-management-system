using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Product;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.Product;

/// <summary>
/// Controller for managing product sub-types (linked to categories)
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiresLicense(LicenseModule.CRM_Product)]
public class ProductSubTypesController : ControllerBase
{
    private readonly IProductSubTypeService _subTypeService;
    private readonly ILogger<ProductSubTypesController> _logger;

    public ProductSubTypesController(IProductSubTypeService subTypeService, ILogger<ProductSubTypesController> logger)
    {
        _subTypeService = subTypeService;
        _logger = logger;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found"));

    /// <summary>
    /// Gets all sub-types with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetSubTypes(
        [FromQuery] string? search = null,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _subTypeService.GetSubTypesAsync(GetCompanyId(), search, categoryId, isActive, pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a sub-type by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetSubTypeById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _subTypeService.GetSubTypeByIdAsync(GetCompanyId(), id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a sub-type by code within a category
    /// </summary>
    [HttpGet("by-code/{categoryId:guid}/{code}")]
    public async Task<IActionResult> GetSubTypeByCode(Guid categoryId, string code, CancellationToken cancellationToken = default)
    {
        var result = await _subTypeService.GetSubTypeByCodeAsync(GetCompanyId(), categoryId, code, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Creates a new sub-type
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateSubType([FromBody] CreateProductSubTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _subTypeService.CreateSubTypeAsync(GetCompanyId(), request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(GetSubTypeById), new { id = result.Data!.Id }, new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Updates an existing sub-type
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateSubType(Guid id, [FromBody] UpdateProductSubTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _subTypeService.UpdateSubTypeAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Deletes a sub-type
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteSubType(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _subTypeService.DeleteSubTypeAsync(GetCompanyId(), id, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = "Sub-type deleted successfully" });
    }

    /// <summary>
    /// Gets sub-type lookup for dropdowns (optionally filtered by category)
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> GetSubTypeLookup([FromQuery] Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        var result = await _subTypeService.GetSubTypeLookupAsync(GetCompanyId(), categoryId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Checks if a sub-type code exists within a category
    /// </summary>
    [HttpGet("code-exists/{categoryId:guid}/{code}")]
    public async Task<IActionResult> CheckCodeExists(Guid categoryId, string code, [FromQuery] Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var exists = await _subTypeService.SubTypeCodeExistsAsync(GetCompanyId(), categoryId, code, excludeId, cancellationToken);
        return Ok(new { isSuccess = true, data = exists });
    }
}
