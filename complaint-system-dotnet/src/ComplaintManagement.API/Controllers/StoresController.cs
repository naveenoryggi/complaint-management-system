using System.Security.Claims;
using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiresLicense(Domain.Enums.LicenseModule.AssetManagement)]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;
    private readonly ILogger<StoresController> _logger;

    public StoresController(IStoreService storeService, ILogger<StoresController> logger)
    {
        _storeService = storeService;
        _logger = logger;
    }

    #region Helper Methods

    private Guid? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    private Guid? GetCompanyId()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        return Guid.TryParse(companyIdClaim, out var companyId) ? companyId : null;
    }

    #endregion

    #region Store CRUD

    /// <summary>
    /// Gets all stores for the current company
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetStores([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var companyId = GetCompanyId();
        if (!companyId.HasValue)
        {
            return Unauthorized("Company context not found.");
        }

        var result = await _storeService.GetStoresByCompanyAsync(companyId.Value, includeInactive, cancellationToken);
        return Ok(Result<List<StoreDto>>.Success(result));
    }

    /// <summary>
    /// Gets stores for dropdown/lookup
    /// </summary>
    [HttpGet("lookup")]
    [SkipLicenseCheck]
    public async Task<IActionResult> GetStoreLookup(CancellationToken cancellationToken = default)
    {
        var companyId = GetCompanyId();
        if (!companyId.HasValue)
        {
            return Unauthorized("Company context not found.");
        }

        var result = await _storeService.GetStoreLookupAsync(companyId.Value, cancellationToken);
        return Ok(Result<List<StoreLookupDto>>.Success(result));
    }

    /// <summary>
    /// Gets a store by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetStore(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _storeService.GetStoreByIdAsync(id, cancellationToken);
        if (result == null)
        {
            return NotFound("Store not found.");
        }
        return Ok(Result<StoreDto>.Success(result));
    }

    /// <summary>
    /// Creates a new store
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateStore([FromBody] CreateStoreDto dto, CancellationToken cancellationToken = default)
    {
        var companyId = GetCompanyId();
        if (!companyId.HasValue)
        {
            return Unauthorized("Company context not found.");
        }

        var result = await _storeService.CreateStoreAsync(dto, companyId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetStore), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Updates an existing store
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateStore(Guid id, [FromBody] UpdateStoreDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _storeService.UpdateStoreAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Deletes a store (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteStore(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _storeService.DeleteStoreAsync(id, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    #endregion

    #region Store Managers

    /// <summary>
    /// Assigns managers to a store
    /// </summary>
    [HttpPut("{id:guid}/managers")]
    public async Task<IActionResult> AssignManagers(Guid id, [FromBody] AssignStoreManagersDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _storeService.AssignManagersAsync(id, dto, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets stores managed by the current user
    /// </summary>
    [HttpGet("managed")]
    public async Task<IActionResult> GetManagedStores(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _storeService.GetManagedStoresAsync(userId.Value, cancellationToken);
        return Ok(Result<List<StoreLookupDto>>.Success(result));
    }

    #endregion

    #region Store Staff

    /// <summary>
    /// Gets all staff members of a store
    /// </summary>
    [HttpGet("{id:guid}/staff")]
    public async Task<IActionResult> GetStoreStaff(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _storeService.GetStoreStaffAsync(id, cancellationToken);
        return Ok(Result<List<StoreStaffDto>>.Success(result));
    }

    /// <summary>
    /// Assigns a user role in a store
    /// </summary>
    [HttpPost("{id:guid}/staff")]
    public async Task<IActionResult> AssignStoreRole(Guid id, [FromBody] AssignStoreUserRoleDto dto, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _storeService.AssignUserRoleAsync(id, dto, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Revokes a user's role in a store
    /// </summary>
    [HttpDelete("staff/{roleId:guid}")]
    public async Task<IActionResult> RevokeStoreRole(Guid roleId, CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _storeService.RevokeUserRoleAsync(roleId, userId.Value, cancellationToken);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// Gets all store roles for the current user
    /// </summary>
    [HttpGet("my-roles")]
    public async Task<IActionResult> GetMyStoreRoles(CancellationToken cancellationToken = default)
    {
        var userId = GetUserId();
        if (!userId.HasValue)
        {
            return Unauthorized("User context not found.");
        }

        var result = await _storeService.GetUserStoreRolesAsync(userId.Value, cancellationToken);
        return Ok(Result<List<StoreUserRoleDto>>.Success(result));
    }

    #endregion
}
