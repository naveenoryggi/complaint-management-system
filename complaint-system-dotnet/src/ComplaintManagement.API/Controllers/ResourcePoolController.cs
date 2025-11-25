using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/resource-pools")]
[Authorize]
public class ResourcePoolController : ControllerBase
{
    private readonly IResourcePoolService _poolService;
    private readonly ILogger<ResourcePoolController> _logger;

    public ResourcePoolController(
        IResourcePoolService poolService,
        ILogger<ResourcePoolController> logger)
    {
        _poolService = poolService;
        _logger = logger;
    }

    #region Resource Pool Management

    /// <summary>
    /// Get all resource pools for the company
    /// </summary>
    [HttpGet]
    [HasPermission("ViewEscalation")]
    [ResponseCache(Duration = 300, VaryByQueryKeys = new[] { "companyId" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetPools([FromQuery] Guid? companyId)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var userCompanyId))
        {
            return Unauthorized(new { isSuccess = false, message = "Company ID not found in token" });
        }

        // Use provided companyId if user has permission, otherwise use their company
        var targetCompanyId = companyId ?? userCompanyId;

        try
        {
            var pools = await _poolService.GetPoolsByCompanyAsync(targetCompanyId);

            // Map to DTOs with member counts and details
            var poolDtos = pools.Select(p => new ResourcePoolDto
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                Name = p.Name,
                Description = p.Description,
                PoolType = p.PoolType,
                BranchId = p.BranchId,
                BranchName = p.Branch?.Name,
                DepartmentId = p.DepartmentId,
                DepartmentName = p.Department?.Name,
                SectionId = p.SectionId,
                SectionName = p.Section?.Name,
                IsActive = p.IsActive,
                MemberCount = p.Members.Count(m => m.IsActive),
                Members = p.Members.Where(m => m.IsActive).Select(m => new ResourcePoolMemberDto
                {
                    Id = m.Id,
                    ResourcePoolId = m.ResourcePoolId,
                    UserId = m.UserId,
                    UserName = $"{m.User.FirstName} {m.User.LastName}",
                    UserEmail = m.User.Email,
                    AddedAt = m.AddedAt,
                    AddedBy = m.AddedBy,
                    IsActive = m.IsActive
                }).ToList(),
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            }).ToList();

            return Ok(new { isSuccess = true, data = poolDtos });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource pools for company {CompanyId}", targetCompanyId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while fetching resource pools" });
        }
    }

    /// <summary>
    /// Get resource pool by ID
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetPoolById(Guid id)
    {
        try
        {
            var pool = await _poolService.GetPoolByIdAsync(id);
            if (pool == null)
            {
                return NotFound(new { isSuccess = false, message = "Resource pool not found" });
            }

            var poolDto = new ResourcePoolDto
            {
                Id = pool.Id,
                CompanyId = pool.CompanyId,
                Name = pool.Name,
                Description = pool.Description,
                PoolType = pool.PoolType,
                BranchId = pool.BranchId,
                BranchName = pool.Branch?.Name,
                DepartmentId = pool.DepartmentId,
                DepartmentName = pool.Department?.Name,
                SectionId = pool.SectionId,
                SectionName = pool.Section?.Name,
                IsActive = pool.IsActive,
                MemberCount = pool.Members.Count(m => m.IsActive),
                Members = pool.Members.Where(m => m.IsActive).Select(m => new ResourcePoolMemberDto
                {
                    Id = m.Id,
                    ResourcePoolId = m.ResourcePoolId,
                    UserId = m.UserId,
                    UserName = $"{m.User.FirstName} {m.User.LastName}",
                    UserEmail = m.User.Email,
                    AddedAt = m.AddedAt,
                    AddedBy = m.AddedBy,
                    IsActive = m.IsActive
                }).ToList(),
                CreatedAt = pool.CreatedAt,
                UpdatedAt = pool.UpdatedAt
            };

            return Ok(new { isSuccess = true, data = poolDto });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource pool {PoolId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while fetching the resource pool" });
        }
    }

    /// <summary>
    /// Create a new resource pool (Admin only)
    /// </summary>
    [HttpPost]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> CreatePool([FromBody] CreateResourcePoolRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { isSuccess = false, message = "User ID not found in token" });
        }

        if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out var companyId))
        {
            return Unauthorized(new { isSuccess = false, message = "Company ID not found in token" });
        }

        try
        {
            var pool = await _poolService.CreatePoolAsync(request, companyId, userId);

            return CreatedAtAction(nameof(GetPoolById), new { id = pool.Id }, new
            {
                isSuccess = true,
                message = "Resource pool created successfully",
                data = pool
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource pool");
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while creating the resource pool" });
        }
    }

    /// <summary>
    /// Update an existing resource pool (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> UpdatePool(Guid id, [FromBody] UpdateResourcePoolRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { isSuccess = false, message = "User ID not found in token" });
        }

        try
        {
            var pool = await _poolService.UpdatePoolAsync(id, request, userId);

            return Ok(new
            {
                isSuccess = true,
                message = "Resource pool updated successfully",
                data = pool
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource pool {PoolId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while updating the resource pool" });
        }
    }

    /// <summary>
    /// Delete a resource pool (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> DeletePool(Guid id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { isSuccess = false, message = "User ID not found in token" });
        }

        try
        {
            await _poolService.DeletePoolAsync(id, userId);

            return Ok(new
            {
                isSuccess = true,
                message = "Resource pool deleted successfully"
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { isSuccess = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource pool {PoolId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while deleting the resource pool" });
        }
    }

    #endregion

    #region Resource Pool Member Management

    /// <summary>
    /// Add a member to a resource pool (Admin only)
    /// </summary>
    [HttpPost("{id}/members")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddResourcePoolMemberRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { isSuccess = false, message = "User ID not found in token" });
        }

        try
        {
            var member = await _poolService.AddMemberAsync(id, request.UserId, userId);

            return Ok(new
            {
                isSuccess = true,
                message = "Member added to resource pool successfully",
                data = member
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding member to resource pool {PoolId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while adding the member to the resource pool" });
        }
    }

    /// <summary>
    /// Remove a member from a resource pool (Admin only)
    /// </summary>
    [HttpDelete("{poolId}/members/{userId}")]
    [HasPermission("ManageEscalation")]
    public async Task<IActionResult> RemoveMember(Guid poolId, Guid userId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized(new { isSuccess = false, message = "User ID not found in token" });
        }

        try
        {
            await _poolService.RemoveMemberAsync(poolId, userId, currentUserId);

            return Ok(new
            {
                isSuccess = true,
                message = "Member removed from resource pool successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing member from resource pool {PoolId}", poolId);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while removing the member from the resource pool" });
        }
    }

    /// <summary>
    /// Get members of a resource pool
    /// </summary>
    [HttpGet("{id}/members")]
    [HasPermission("ViewEscalation")]
    public async Task<IActionResult> GetPoolMembers(Guid id)
    {
        try
        {
            var members = await _poolService.GetPoolMembersAsync(id);

            var memberDtos = members.Select(m => new ResourcePoolMemberDto
            {
                Id = m.Id,
                ResourcePoolId = m.ResourcePoolId,
                UserId = m.UserId,
                UserName = $"{m.User.FirstName} {m.User.LastName}",
                UserEmail = m.User.Email,
                AddedAt = m.AddedAt,
                AddedBy = m.AddedBy,
                IsActive = m.IsActive
            }).ToList();

            return Ok(new
            {
                isSuccess = true,
                data = memberDtos,
                count = memberDtos.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting members for resource pool {PoolId}", id);
            return StatusCode(500, new { isSuccess = false, message = "An error occurred while fetching pool members" });
        }
    }

    #endregion
}
