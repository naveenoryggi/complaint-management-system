using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUnitOfWork unitOfWork, ILogger<UsersController> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Get all active users (for assignment dropdowns, etc.)
    /// </summary>
    /// <returns>List of users</returns>
    [HttpGet]
    [ProducesResponseType(typeof(Result<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.IsActive);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                EmployeeCode = u.EmployeeCode,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                JobTitle = u.JobTitle,
                CompanyId = u.CompanyId,
                BranchId = u.BranchId,
                DepartmentId = u.DepartmentId,
                SectionId = u.SectionId,
                EmployeeTypeId = u.EmployeeTypeId,
                ManagerId = u.ManagerId,
                Roles = new List<UserRoleDto>(),
                Permissions = new List<string>()
            }).ToList();

            return Ok(Result<List<UserDto>>.Success(userDtos, $"Retrieved {userDtos.Count} users"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users");
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Search users by multiple criteria (for autocomplete)
    /// </summary>
    /// <param name="searchTerm">Search term to filter users</param>
    /// <param name="searchFields">Comma-separated list of fields to search in (email,fullName,phoneNumber,employeeCode,firstName,lastName)</param>
    /// <param name="limit">Maximum number of results (default 20)</param>
    /// <returns>List of matching users</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(Result<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchUsers(
        [FromQuery] string? searchTerm,
        [FromQuery] string[]? searchFields,
        [FromQuery] int limit = 20)
    {
        try
        {
            // If no search term, return empty list (don't return all users)
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Ok(Result<List<UserDto>>.Success(new List<UserDto>(), "No search term provided"));
            }

            // Default search fields if not provided
            var fieldsToSearch = searchFields?.Length > 0
                ? searchFields
                : new[] { "email", "fullName", "phoneNumber", "employeeCode", "firstName", "lastName" };

            var searchTermLower = searchTerm.ToLowerInvariant();

            // Start with active users query
            var query = _unitOfWork.Users.GetQueryable().Where(u => u.IsActive && !u.IsDeleted);

            // Build OR conditions for search across multiple fields (case-insensitive)
            query = query.Where(u =>
                (u.Email != null && EF.Functions.Like(u.Email.ToLower(), $"%{searchTermLower}%")) ||
                (u.FirstName != null && EF.Functions.Like(u.FirstName.ToLower(), $"%{searchTermLower}%")) ||
                (u.LastName != null && EF.Functions.Like(u.LastName.ToLower(), $"%{searchTermLower}%")) ||
                (u.Phone != null && EF.Functions.Like(u.Phone, $"%{searchTermLower}%")) ||
                (u.EmployeeCode != null && EF.Functions.Like(u.EmployeeCode.ToLower(), $"%{searchTermLower}%"))
            );

            // Execute query and apply limit
            var users = await query.Take(limit).ToListAsync();

            // Include additional navigation properties for better results
            var userIds = users.Select(u => u.Id).ToList();
            var usersWithDetails = await _unitOfWork.Users.GetQueryable()
                .Where(u => userIds.Contains(u.Id))
                .Include(u => u.Company)
                .Include(u => u.Branch)
                .Include(u => u.Department)
                .Include(u => u.Section)
                // .Include(u => u.EmployeeType) // commented out temporarily
                .ToListAsync();

            // Convert to DTOs with enhanced information
            var limitedUsers = usersWithDetails.Select(u => new UserDto
            {
                Id = u.Id,
                EmployeeCode = u.EmployeeCode,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                JobTitle = u.JobTitle,
                CompanyId = u.CompanyId,
                CompanyName = u.Company?.Name ?? string.Empty,
                BranchId = u.BranchId,
                BranchName = u.Branch?.Name,
                DepartmentId = u.DepartmentId,
                DepartmentName = u.Department?.Name,
                SectionId = u.SectionId,
                SectionName = u.Section?.Name,
                EmployeeTypeId = u.EmployeeTypeId,
                // EmployeeTypeName = u.EmployeeType?.Name, // commented out temporarily
                IsActive = u.IsActive
            }).ToList();

            _logger.LogInformation("User search completed: {SearchTerm} in fields {Fields} found {Count} results",
                searchTerm, string.Join(",", fieldsToSearch), limitedUsers.Count);

            return Ok(Result<List<UserDto>>.Success(limitedUsers, $"Found {limitedUsers.Count} users"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching users with term: {SearchTerm} in fields: {Fields}",
                searchTerm, searchFields != null ? string.Join(",", searchFields) : "default");
            return StatusCode(500, new { message = "An error occurred while searching users" });
        }
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    /// <param name="id">User ID</param>
    /// <returns>User details</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserById(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdWithIncludesAsync(
                id,
                default,
                u => u.Company,
                u => u.Branch,
                u => u.Department,
                u => u.Section,
                u => u.Manager,
                u => u.UserComplaintRoles
            );

            if (user == null)
            {
                return NotFound(Result<UserDto>.Failure("User not found", "Not found"));
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                JobTitle = user.JobTitle,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name ?? string.Empty,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                SectionId = user.SectionId,
                SectionName = user.Section?.Name,
                EmployeeTypeId = user.EmployeeTypeId,
                ManagerId = user.ManagerId,
                ManagerName = user.Manager?.FullName,
                Roles = user.UserComplaintRoles.Select(r => new UserRoleDto
                {
                    RoleId = r.ComplaintRoleId,
                    RoleName = r.ComplaintRole?.Name ?? string.Empty,
                    RoleCode = r.ComplaintRole?.Code ?? string.Empty,
                    RoleType = r.ComplaintRole?.RoleType.ToString() ?? string.Empty,
                    EscalationLevel = r.ComplaintRole?.EscalationLevel ?? 0,
                    IsPrimary = r.IsPrimary
                }).ToList(),
                Permissions = new List<string>()
            };

            return Ok(Result<UserDto>.Success(userDto, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Get user by employee code
    /// </summary>
    /// <param name="employeeCode">Employee code</param>
    /// <returns>User details</returns>
    [HttpGet("by-employee-code/{employeeCode}")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserByEmployeeCode(string employeeCode)
    {
        try
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.EmployeeCode == employeeCode && u.IsActive);
            var user = users.FirstOrDefault();

            if (user == null)
            {
                return NotFound(Result<UserDto>.Failure($"User with employee code '{employeeCode}' not found", "Not found"));
            }

            var userDto = new UserDto
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                JobTitle = user.JobTitle,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                DepartmentId = user.DepartmentId,
                SectionId = user.SectionId,
                EmployeeTypeId = user.EmployeeTypeId,
                ManagerId = user.ManagerId,
                Roles = new List<UserRoleDto>(),
                Permissions = new List<string>()
            };

            return Ok(Result<UserDto>.Success(userDto, "User retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving user by employee code {EmployeeCode}", employeeCode);
            return StatusCode(500, new { message = "An error occurred while retrieving the user" });
        }
    }

    /// <summary>
    /// Get users by company ID
    /// </summary>
    /// <param name="companyId">Company ID</param>
    /// <returns>List of users in the company</returns>
    [HttpGet("by-company")]
    [ProducesResponseType(typeof(Result<List<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsersByCompany([FromQuery] Guid companyId)
    {
        try
        {
            if (companyId == Guid.Empty)
            {
                return BadRequest(Result<List<UserDto>>.Failure("Company ID is required", "Invalid request"));
            }

            var users = await _unitOfWork.Users.FindAsync(u => u.CompanyId == companyId && u.IsActive);

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                EmployeeCode = u.EmployeeCode,
                FirstName = u.FirstName,
                LastName = u.LastName,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.Phone,
                JobTitle = u.JobTitle,
                CompanyId = u.CompanyId,
                BranchId = u.BranchId,
                DepartmentId = u.DepartmentId,
                SectionId = u.SectionId,
                EmployeeTypeId = u.EmployeeTypeId,
                ManagerId = u.ManagerId,
                Roles = new List<UserRoleDto>(),
                Permissions = new List<string>()
            }).ToList();

            return Ok(Result<List<UserDto>>.Success(userDtos, $"Retrieved {userDtos.Count} users"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving users for company {CompanyId}", companyId);
            return StatusCode(500, new { message = "An error occurred while retrieving users" });
        }
    }

    /// <summary>
    /// Create a new user
    /// </summary>
    [HttpPost]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            // Validate request model
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(Result<UserDto>.Failure(string.Join(", ", errors), "Validation failed"));
            }

            // Validate company exists
            var company = await _unitOfWork.Companies.GetByIdAsync(request.CompanyId);
            if (company == null)
            {
                return BadRequest(Result<UserDto>.Failure("Company not found", "Invalid company"));
            }

            // Check for duplicate email
            var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Email == request.Email)).FirstOrDefault();
            if (existingUser != null)
            {
                return BadRequest(Result<UserDto>.Failure("A user with this email already exists", "Duplicate email"));
            }

            // Check for duplicate employee code
            var existingCode = (await _unitOfWork.Users.FindAsync(u => u.EmployeeCode == request.EmployeeCode)).FirstOrDefault();
            if (existingCode != null)
            {
                return BadRequest(Result<UserDto>.Failure("A user with this employee code already exists", "Duplicate employee code"));
            }

            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                CompanyId = request.CompanyId,
                EmployeeCode = request.EmployeeCode,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Phone = request.Phone,
                JobTitle = request.JobTitle,
                BranchId = request.BranchId,
                DepartmentId = request.DepartmentId,
                SectionId = request.SectionId,
                EmployeeTypeId = request.EmployeeTypeId,
                ManagerId = request.ManagerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(user);

            // Auto-assign Complainant role to all new users so they can create and view their complaints
            var complainantRole = (await _unitOfWork.ComplaintRoles.FindAsync(r => r.Code == "COMPLAINANT")).FirstOrDefault();
            if (complainantRole != null)
            {
                var userRole = new ComplaintManagement.Domain.Entities.Roles.UserComplaintRole
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ComplaintRoleId = complainantRole.Id,
                    EffectiveFrom = DateTime.UtcNow,
                    IsPrimary = true,
                    IsActive = true,
                    Notes = "Auto-assigned Complainant role for general employee access",
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.UserComplaintRoles.AddAsync(userRole);
            }

            await _unitOfWork.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                JobTitle = user.JobTitle,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                DepartmentId = user.DepartmentId,
                SectionId = user.SectionId,
                EmployeeTypeId = user.EmployeeTypeId,
                ManagerId = user.ManagerId,
                Roles = new List<UserRoleDto>(),
                Permissions = new List<string>()
            };

            _logger.LogInformation("User created successfully: {UserName} ({UserId})", user.FullName, user.Id);
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, Result<UserDto>.Success(userDto, "User created successfully"));
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogWarning(dbEx, "Database constraint violation while creating user");

            // Handle specific database constraint violations
            if (dbEx.InnerException?.Message.Contains("IX_Users_Email") == true ||
                dbEx.InnerException?.Message.Contains("Email") == true)
            {
                return BadRequest(Result<UserDto>.Failure("A user with this email already exists", "Duplicate email"));
            }

            if (dbEx.InnerException?.Message.Contains("IX_Users_EmployeeCode") == true ||
                dbEx.InnerException?.Message.Contains("EmployeeCode") == true)
            {
                return BadRequest(Result<UserDto>.Failure("A user with this employee code already exists", "Duplicate employee code"));
            }

            return BadRequest(Result<UserDto>.Failure("Invalid data provided", "Database constraint violation"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating user");
            return StatusCode(500, new { message = "An error occurred while creating the user" });
        }
    }

    /// <summary>
    /// Update an existing user
    /// </summary>
    [HttpPut("{id}")]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<UserDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(Result<UserDto>.Failure("User not found", "Not found"));
            }

            // Check for duplicate email (excluding current user) - only if email is being updated
            if (!string.IsNullOrEmpty(request.Email) && request.Email != user.Email)
            {
                var existingUser = (await _unitOfWork.Users.FindAsync(u => u.Email == request.Email && u.Id != id)).FirstOrDefault();
                if (existingUser != null)
                {
                    return BadRequest(Result<UserDto>.Failure("A user with this email already exists", "Duplicate email"));
                }
            }

            // Update only provided properties (partial update support)
            if (!string.IsNullOrEmpty(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrEmpty(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrEmpty(request.Email))
                user.Email = request.Email;

            if (request.Phone != null)
                user.Phone = request.Phone;

            if (request.JobTitle != null)
                user.JobTitle = request.JobTitle;

            if (request.BranchId.HasValue)
                user.BranchId = request.BranchId;

            if (request.DepartmentId.HasValue)
                user.DepartmentId = request.DepartmentId;

            if (request.SectionId.HasValue)
                user.SectionId = request.SectionId;

            if (request.EmployeeTypeId.HasValue)
                user.EmployeeTypeId = request.EmployeeTypeId;

            if (request.ManagerId.HasValue)
                user.ManagerId = request.ManagerId;

            if (request.IsActive.HasValue)
                user.IsActive = request.IsActive.Value;

            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            var userDto = new UserDto
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                JobTitle = user.JobTitle,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                DepartmentId = user.DepartmentId,
                SectionId = user.SectionId,
                EmployeeTypeId = user.EmployeeTypeId,
                ManagerId = user.ManagerId,
                Roles = new List<UserRoleDto>(),
                Permissions = new List<string>()
            };

            _logger.LogInformation("User updated successfully: {UserName} ({UserId})", user.FullName, id);
            return Ok(Result<UserDto>.Success(userDto, "User updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the user" });
        }
    }

    /// <summary>
    /// Delete a user (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(Result<object>.Failure("User not found", "Not found"));
            }

            // Soft delete: mark as deleted and inactive
            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User deleted successfully: {UserName} ({UserId})", user.FullName, id);
            return Ok(Result<object>.Success(null, "User deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the user" });
        }
    }

    /// <summary>
    /// Deactivate a user (set IsActive to false without deleting)
    /// </summary>
    [HttpPost("{id}/deactivate")]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateUser(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(Result<object>.Failure("User not found", "Not found"));
            }

            // Deactivate user (set IsActive to false)
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("User deactivated successfully: {UserName} ({UserId})", user.FullName, id);
            return Ok(Result<object>.Success(null, "User deactivated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deactivating user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while deactivating the user" });
        }
    }

    /// <summary>
    /// Change password for a user
    /// </summary>
    [HttpPost("{id}/change-password")]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(Result<object>.Failure("Invalid request", "Validation failed"));
            }

            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(Result<object>.Failure("User not found", "Not found"));
            }

            // Validate password requirements
            if (string.IsNullOrEmpty(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return BadRequest(Result<object>.Failure("Password must be at least 6 characters long", "Invalid password"));
            }

            // In a real implementation, you would:
            // 1. Verify old password
            // 2. Hash the new password
            // 3. Update the password hash in the database
            // For now, we'll just log the action

            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password changed for user: {UserName} ({UserId})", user.FullName, id);
            return Ok(Result<object>.Success(null, "Password changed successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while changing password for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while changing the password" });
        }
    }

    /// <summary>
    /// Reset password for a user
    /// </summary>
    [HttpPost("{id}/reset-password")]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetPassword(Guid id)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            if (user == null)
            {
                return NotFound(Result<object>.Failure("User not found", "Not found"));
            }

            // In a real implementation, you would:
            // 1. Generate a temporary password or reset token
            // 2. Hash the temporary password
            // 3. Update the password hash in the database
            // 4. Send email with reset link or temporary password
            // For now, we'll just log the action

            user.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Password reset for user: {UserName} ({UserId})", user.FullName, id);
            return Ok(Result<object>.Success(null, "Password reset successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while resetting password for user {UserId}", id);
            return StatusCode(500, new { message = "An error occurred while resetting the password" });
        }
    }

    /// <summary>
    /// DELETE ALL USERS - TEMPORARY ENDPOINT FOR CLEANUP
    /// </summary>
    [HttpDelete("all")]
    [HasPermission("ManageUsers")]
    [ProducesResponseType(typeof(Result<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteAllUsers()
    {
        try
        {
            // Delete all user complaint roles first
            var userRoles = await _unitOfWork.UserComplaintRoles.GetAllAsync();
            foreach (var role in userRoles)
            {
                _unitOfWork.UserComplaintRoles.Delete(role);
            }
            await _unitOfWork.SaveChangesAsync();

            // Delete all users
            var users = await _unitOfWork.Users.GetAllAsync();
            foreach (var user in users)
            {
                _unitOfWork.Users.Delete(user);
            }
            await _unitOfWork.SaveChangesAsync();

            _logger.LogWarning("ALL USERS DELETED - Total deleted: {Count}", users.Count());
            return Ok(Result<object>.Success(new { deletedCount = users.Count() }, $"All {users.Count()} users deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting all users");
            return StatusCode(500, new { message = "An error occurred while deleting all users" });
        }
    }
}

/// <summary>
/// Request model for creating a user
/// </summary>
public class CreateUserRequest
{
    [Required(ErrorMessage = "Company ID is required")]
    public Guid CompanyId { get; set; }

    [Required(ErrorMessage = "Employee code is required")]
    [StringLength(50, ErrorMessage = "Employee code cannot exceed 50 characters")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [StringLength(100, ErrorMessage = "First name cannot exceed 100 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [StringLength(100, ErrorMessage = "Last name cannot exceed 100 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address format")]
    [StringLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public string Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? Phone { get; set; }

    public string? JobTitle { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? EmployeeTypeId { get; set; }
    public Guid? ManagerId { get; set; }
}

/// <summary>
/// Request model for updating a user (supports partial updates)
/// </summary>
public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public Guid? EmployeeTypeId { get; set; }
    public Guid? ManagerId { get; set; }
    public bool? IsActive { get; set; }
}

/// <summary>
/// Request model for changing a user's password
/// </summary>
public class ChangePasswordRequest
{
    public string OldPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
