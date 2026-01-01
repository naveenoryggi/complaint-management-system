using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Product;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.Product;

/// <summary>
/// Controller for managing product brands
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiresLicense(LicenseModule.CRM_Product)]
public class BrandsController : ControllerBase
{
    private readonly IBrandService _brandService;
    private readonly ILogger<BrandsController> _logger;

    public BrandsController(IBrandService brandService, ILogger<BrandsController> logger)
    {
        _brandService = brandService;
        _logger = logger;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found"));

    /// <summary>
    /// Gets all brands with pagination and filtering
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetBrands(
        [FromQuery] string? search = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _brandService.GetBrandsAsync(GetCompanyId(), search, isActive, pageNumber, pageSize, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a brand by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBrandById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _brandService.GetBrandByIdAsync(GetCompanyId(), id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a brand by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetBrandByCode(string code, CancellationToken cancellationToken = default)
    {
        var result = await _brandService.GetBrandByCodeAsync(GetCompanyId(), code, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Creates a new brand
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _brandService.CreateBrandAsync(GetCompanyId(), request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(GetBrandById), new { id = result.Data!.Id }, new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Updates an existing brand
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _brandService.UpdateBrandAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Deletes a brand
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteBrand(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _brandService.DeleteBrandAsync(GetCompanyId(), id, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = "Brand deleted successfully" });
    }

    /// <summary>
    /// Gets brand lookup for dropdowns
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> GetBrandLookup(CancellationToken cancellationToken = default)
    {
        var result = await _brandService.GetBrandLookupAsync(GetCompanyId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Checks if a brand code exists
    /// </summary>
    [HttpGet("code-exists/{code}")]
    public async Task<IActionResult> CheckCodeExists(string code, [FromQuery] Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var exists = await _brandService.BrandCodeExistsAsync(GetCompanyId(), code, excludeId, cancellationToken);
        return Ok(new { isSuccess = true, data = exists });
    }
}
