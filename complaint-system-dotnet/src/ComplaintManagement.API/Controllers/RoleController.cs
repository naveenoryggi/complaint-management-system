using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Roles;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing roles and permissions
/// </summary>
[ApiController]
[Route("api/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<RoleController> _logger;

    public RoleController(ComplaintDbContext context, ILogger<RoleController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all roles
    /// </summary>
    [HttpGet]
    [HasPermission("ManageRoles")]
    [ResponseCache(Duration = 600, VaryByQueryKeys = new[] { "includeInactive" }, VaryByHeader = "Authorization")]
    public async Task<IActionResult> GetAllRoles([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.ComplaintRoles
                .Include(r => r.RolePermissions)
                .AsQueryable();

            if (!includeInactive)
            {
                query = query.Where(r => r.IsActive);
            }

            var roles = await query
                .OrderBy(r => r.DisplayOrder)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    RoleType = r.RoleType,
                    EscalationLevel = r.EscalationLevel,
                    IsSystemRole = r.IsSystemRole,
                    IsActive = r.IsActive,
                    DisplayOrder = r.DisplayOrder,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Permissions = r.RolePermissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        PermissionType = p.PermissionType,
                        PermissionName = p.PermissionType.ToString(),
                        IsGranted = p.IsGranted
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                isSuccess = true,
                data = roles,
                count = roles.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving roles");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving roles"
            });
        }
    }

    /// <summary>
    /// Get role by ID
    /// </summary>
    [HttpGet("{id}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        try
        {
            var role = await _context.ComplaintRoles
                .Include(r => r.RolePermissions)
                .Where(r => r.Id == id)
                .Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Code = r.Code,
                    Description = r.Description,
                    RoleType = r.RoleType,
                    EscalationLevel = r.EscalationLevel,
                    IsSystemRole = r.IsSystemRole,
                    IsActive = r.IsActive,
                    DisplayOrder = r.DisplayOrder,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Permissions = r.RolePermissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        PermissionType = p.PermissionType,
                        PermissionName = p.PermissionType.ToString(),
                        IsGranted = p.IsGranted
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            return Ok(new
            {
                isSuccess = true,
                data = role
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving the role"
            });
        }
    }

    /// <summary>
    /// Get permissions for a role
    /// </summary>
    [HttpGet("{id}/permissions")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> GetRolePermissions(Guid id)
    {
        try
        {
            var role = await _context.ComplaintRoles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            var permissions = role.RolePermissions.Select(p => new PermissionDto
            {
                Id = p.Id,
                PermissionType = p.PermissionType,
                PermissionName = p.PermissionType.ToString(),
                IsGranted = p.IsGranted
            }).ToList();

            return Ok(new
            {
                isSuccess = true,
                data = permissions,
                count = permissions.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions for role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving role permissions"
            });
        }
    }

    /// <summary>
    /// Get users assigned to a role
    /// </summary>
    [HttpGet("{id}/users")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> GetRoleUsers(Guid id)
    {
        try
        {
            var role = await _context.ComplaintRoles.FindAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            var users = await _context.UserComplaintRoles
                .Include(ur => ur.User)
                .Where(ur => ur.ComplaintRoleId == id && ur.IsActive)
                .Select(ur => new
                {
                    userId = ur.UserId,
                    userRoleId = ur.Id,
                    fullName = ur.User.FullName,
                    email = ur.User.Email,
                    employeeCode = ur.User.EmployeeCode,
                    isPrimary = ur.IsPrimary,
                    effectiveFrom = ur.EffectiveFrom,
                    effectiveTo = ur.EffectiveTo
                })
                .ToListAsync();

            return Ok(new
            {
                isSuccess = true,
                data = users,
                count = users.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving role users"
            });
        }
    }

    /// <summary>
    /// Assign permissions to a role (POST method)
    /// </summary>
    [HttpPost("{id}/permissions")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> AssignRolePermissions(Guid id, [FromBody] AssignRolePermissionsRequest request)
    {
        try
        {
            var role = await _context.ComplaintRoles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            if (request.PermissionIds == null || !request.PermissionIds.Any())
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Permission IDs are required"
                });
            }

            // Add new permissions
            foreach (var permissionType in request.PermissionIds)
            {
                var existing = role.RolePermissions
                    .FirstOrDefault(p => p.PermissionType == permissionType);

                if (existing == null)
                {
                    var newPermission = new ComplaintRolePermission
                    {
                        Id = Guid.NewGuid(),
                        ComplaintRoleId = role.Id,
                        PermissionType = permissionType,
                        IsGranted = true,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ComplaintRolePermissions.Add(newPermission);
                }
                else
                {
                    existing.IsGranted = true;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
            }

            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned permissions to role: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return Ok(new
            {
                isSuccess = true,
                message = "Permissions assigned to role successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning permissions to role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while assigning permissions"
            });
        }
    }

    /// <summary>
    /// Create a new role
    /// </summary>
    [HttpPost]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new
            {
                isSuccess = false,
                message = "Validation failed",
                errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList()
            });
        }

        try
        {
            // Check if code already exists
            if (await _context.ComplaintRoles.AnyAsync(r => r.Code == request.Code))
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"Role with code '{request.Code}' already exists"
                });
            }

            var role = new ComplaintRole
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                RoleType = request.RoleType,
                EscalationLevel = request.EscalationLevel,
                IsSystemRole = false, // Custom roles are never system roles
                IsActive = true,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.ComplaintRoles.Add(role);

            // Add permissions
            foreach (var permissionType in request.Permissions)
            {
                var permission = new ComplaintRolePermission
                {
                    Id = Guid.NewGuid(),
                    ComplaintRoleId = role.Id,
                    PermissionType = permissionType,
                    IsGranted = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ComplaintRolePermissions.Add(permission);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Created new role: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return CreatedAtAction(nameof(GetRoleById), new { id = role.Id }, new
            {
                isSuccess = true,
                message = "Role created successfully",
                data = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Code = role.Code,
                    Description = role.Description,
                    RoleType = role.RoleType,
                    EscalationLevel = role.EscalationLevel,
                    IsSystemRole = role.IsSystemRole,
                    IsActive = role.IsActive,
                    DisplayOrder = role.DisplayOrder,
                    CreatedAt = role.CreatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating role");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while creating the role"
            });
        }
    }

    /// <summary>
    /// Update an existing role
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
    {
        try
        {
            var role = await _context.ComplaintRoles.FindAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            // System roles cannot be modified
            if (role.IsSystemRole)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "System roles cannot be modified"
                });
            }

            // Update fields
            if (request.Name != null) role.Name = request.Name;
            if (request.Description != null) role.Description = request.Description;
            if (request.EscalationLevel.HasValue) role.EscalationLevel = request.EscalationLevel.Value;
            if (request.DisplayOrder.HasValue) role.DisplayOrder = request.DisplayOrder.Value;
            if (request.IsActive.HasValue) role.IsActive = request.IsActive.Value;

            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated role: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return Ok(new
            {
                isSuccess = true,
                message = "Role updated successfully",
                data = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Code = role.Code,
                    Description = role.Description,
                    RoleType = role.RoleType,
                    EscalationLevel = role.EscalationLevel,
                    IsSystemRole = role.IsSystemRole,
                    IsActive = role.IsActive,
                    DisplayOrder = role.DisplayOrder,
                    CreatedAt = role.CreatedAt,
                    UpdatedAt = role.UpdatedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating the role"
            });
        }
    }

    /// <summary>
    /// Delete a role (soft delete by deactivating)
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> DeleteRole(Guid id)
    {
        try
        {
            var role = await _context.ComplaintRoles.FindAsync(id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            // System roles cannot be deleted
            if (role.IsSystemRole)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "System roles cannot be deleted"
                });
            }

            // Check if role is assigned to any users
            var assignedUsersCount = await _context.UserComplaintRoles
                .Where(ur => ur.ComplaintRoleId == id && ur.IsActive)
                .CountAsync();

            if (assignedUsersCount > 0)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = $"Cannot delete role. It is currently assigned to {assignedUsersCount} user(s)"
                });
            }

            // Soft delete
            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted role: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return Ok(new
            {
                isSuccess = true,
                message = "Role deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while deleting the role"
            });
        }
    }

    /// <summary>
    /// Remove a specific permission from a role
    /// </summary>
    [HttpDelete("{id}/permissions/{permissionId}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> RemovePermissionFromRole(Guid id, int permissionId)
    {
        try
        {
            var role = await _context.ComplaintRoles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            // System roles cannot be modified
            if (role.IsSystemRole)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "System roles cannot be modified"
                });
            }

            var permission = role.RolePermissions
                .FirstOrDefault(p => (int)p.PermissionType == permissionId);

            if (permission == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Permission with ID {permissionId} not found in role"
                });
            }

            _context.ComplaintRolePermissions.Remove(permission);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed permission {PermissionId} from role: {RoleName} (ID: {RoleId})",
                permissionId, role.Name, role.Id);

            return Ok(new
            {
                isSuccess = true,
                message = "Permission removed from role successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing permission {PermissionId} from role {RoleId}", permissionId, id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while removing the permission"
            });
        }
    }

    /// <summary>
    /// Assign a role to a user
    /// </summary>
    [HttpPost("{roleId}/users/{userId}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> AssignRoleToUser(Guid roleId, Guid userId)
    {
        try
        {
            var role = await _context.ComplaintRoles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {roleId} not found"
                });
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"User with ID {userId} not found or inactive"
                });
            }

            // Check if user already has this role
            var existingAssignment = await _context.UserComplaintRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.ComplaintRoleId == roleId && ur.IsActive);

            if (existingAssignment != null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "User already has this role"
                });
            }

            // Create new user-role assignment
            var userRole = new UserComplaintRole
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ComplaintRoleId = roleId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserComplaintRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned role {RoleName} (ID: {RoleId}) to user {UserId}",
                role.Name, roleId, userId);

            return Ok(new
            {
                isSuccess = true,
                message = "Role assigned to user successfully",
                data = new
                {
                    userRoleId = userRole.Id,
                    userId = userId,
                    roleId = roleId
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role {RoleId} to user {UserId}", roleId, userId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while assigning the role"
            });
        }
    }

    /// <summary>
    /// Remove a role from a user
    /// </summary>
    [HttpDelete("{roleId}/users/{userId}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid roleId, Guid userId)
    {
        try
        {
            var userRole = await _context.UserComplaintRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.ComplaintRoleId == roleId && ur.IsActive);

            if (userRole == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "User does not have this role or assignment not found"
                });
            }

            // Soft delete by marking as inactive
            userRole.IsActive = false;
            userRole.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed role {RoleId} from user {UserId}", roleId, userId);

            return Ok(new
            {
                isSuccess = true,
                message = "Role removed from user successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role {RoleId} from user {UserId}", roleId, userId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while removing the role"
            });
        }
    }

    /// <summary>
    /// Update permissions for a role
    /// </summary>
    [HttpPut("{id}/permissions")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> UpdateRolePermissions(Guid id, [FromBody] UpdateRolePermissionsRequest request)
    {
        try
        {
            var role = await _context.ComplaintRoles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {id} not found"
                });
            }

            // Update permissions
            foreach (var permissionUpdate in request.Permissions)
            {
                var existing = role.RolePermissions
                    .FirstOrDefault(p => p.PermissionType == permissionUpdate.PermissionType);

                if (existing != null)
                {
                    existing.IsGranted = permissionUpdate.IsGranted;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Add new permission
                    var newPermission = new ComplaintRolePermission
                    {
                        Id = Guid.NewGuid(),
                        ComplaintRoleId = role.Id,
                        PermissionType = permissionUpdate.PermissionType,
                        IsGranted = permissionUpdate.IsGranted,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.ComplaintRolePermissions.Add(newPermission);
                }
            }

            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated permissions for role: {RoleName} (ID: {RoleId})", role.Name, role.Id);

            return Ok(new
            {
                isSuccess = true,
                message = "Role permissions updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating permissions for role {RoleId}", id);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while updating role permissions"
            });
        }
    }

    /// <summary>
    /// Get all available permissions
    /// </summary>
    [HttpGet("permissions")]
    [HasPermission("ManageRoles")]
    public IActionResult GetAllPermissions()
    {
        try
        {
            var permissions = Enum.GetValues(typeof(PermissionType))
                .Cast<PermissionType>()
                .Select(p => new
                {
                    value = (int)p,
                    name = p.ToString(),
                    displayName = System.Text.RegularExpressions.Regex.Replace(p.ToString(), "([a-z])([A-Z])", "$1 $2")
                })
                .ToList();

            return Ok(new
            {
                isSuccess = true,
                data = permissions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving permissions");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving permissions"
            });
        }
    }

    /// <summary>
    /// Get users assigned to a role by query parameter
    /// </summary>
    [HttpGet("users")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> GetUsersByRole([FromQuery] Guid roleId)
    {
        try
        {
            if (roleId == Guid.Empty)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "RoleId is required"
                });
            }

            var role = await _context.ComplaintRoles.FindAsync(roleId);
            if (role == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = $"Role with ID {roleId} not found"
                });
            }

            var users = await _context.UserComplaintRoles
                .Include(ur => ur.User)
                .Where(ur => ur.ComplaintRoleId == roleId && ur.IsActive)
                .Select(ur => new
                {
                    userId = ur.UserId,
                    userRoleId = ur.Id,
                    fullName = ur.User.FullName,
                    email = ur.User.Email,
                    employeeCode = ur.User.EmployeeCode,
                    isPrimary = ur.IsPrimary,
                    effectiveFrom = ur.EffectiveFrom,
                    effectiveTo = ur.EffectiveTo
                })
                .ToListAsync();

            return Ok(new
            {
                isSuccess = true,
                data = users,
                count = users.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving users for role {RoleId}", roleId);
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while retrieving users"
            });
        }
    }

    /// <summary>
    /// Assign role to user
    /// </summary>
    [HttpPost("assign")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleToUserRequest request)
    {
        try
        {
            // Validate user exists
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId && u.IsActive);
            if (!userExists)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "User not found"
                });
            }

            // Validate role exists
            var roleExists = await _context.ComplaintRoles.AnyAsync(r => r.Id == request.RoleId && r.IsActive);
            if (!roleExists)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "Role not found"
                });
            }

            // Check if already assigned
            var existingAssignment = await _context.UserComplaintRoles
                .FirstOrDefaultAsync(ur => ur.UserId == request.UserId &&
                                          ur.ComplaintRoleId == request.RoleId &&
                                          ur.IsActive);

            if (existingAssignment != null)
            {
                return BadRequest(new
                {
                    isSuccess = false,
                    message = "Role is already assigned to this user"
                });
            }

            var userRole = new UserComplaintRole
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                ComplaintRoleId = request.RoleId,
                EffectiveFrom = request.EffectiveFrom ?? DateTime.UtcNow,
                EffectiveTo = request.EffectiveTo,
                IsPrimary = request.IsPrimary,
                BranchId = request.BranchId,
                DepartmentId = request.DepartmentId,
                SectionId = request.SectionId,
                Notes = request.Notes,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.UserComplaintRoles.Add(userRole);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned role {RoleId} to user {UserId}", request.RoleId, request.UserId);

            return Ok(new
            {
                isSuccess = true,
                message = "Role assigned to user successfully",
                data = new
                {
                    id = userRole.Id,
                    userId = userRole.UserId,
                    roleId = userRole.ComplaintRoleId,
                    effectiveFrom = userRole.EffectiveFrom,
                    effectiveTo = userRole.EffectiveTo,
                    isPrimary = userRole.IsPrimary
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning role to user");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while assigning the role"
            });
        }
    }

    /// <summary>
    /// Remove role from user
    /// </summary>
    [HttpDelete("assign/{userRoleId}")]
    [HasPermission("ManageRoles")]
    public async Task<IActionResult> RemoveRoleFromUser(Guid userRoleId)
    {
        try
        {
            var userRole = await _context.UserComplaintRoles.FindAsync(userRoleId);

            if (userRole == null)
            {
                return NotFound(new
                {
                    isSuccess = false,
                    message = "User role assignment not found"
                });
            }

            userRole.IsActive = false;
            userRole.EffectiveTo = DateTime.UtcNow;
            userRole.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Removed role assignment {UserRoleId}", userRoleId);

            return Ok(new
            {
                isSuccess = true,
                message = "Role removed from user successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing role from user");
            return StatusCode(500, new
            {
                isSuccess = false,
                message = "An error occurred while removing the role"
            });
        }
    }
}

// Request models
public class AssignRolePermissionsRequest
{
    public List<PermissionType> PermissionIds { get; set; } = new();
}
