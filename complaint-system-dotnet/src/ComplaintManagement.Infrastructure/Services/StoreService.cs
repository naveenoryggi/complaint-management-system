using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service implementation for Store management operations
/// </summary>
public class StoreService : IStoreService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<StoreService> _logger;

    public StoreService(ComplaintDbContext context, ILogger<StoreService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Store CRUD Operations

    public async Task<Result<StoreDto>> CreateStoreAsync(CreateStoreDto dto, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate unique code
            if (!await IsCodeUniqueAsync(dto.Code, companyId, null, cancellationToken))
            {
                return Result<StoreDto>.Failure($"Store code '{dto.Code}' already exists.");
            }

            var store = new Store
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Address = dto.Address,
                City = dto.City,
                Phone = dto.Phone,
                Email = dto.Email,
                PrimaryManagerId = dto.PrimaryManagerId,
                SecondaryManagerId = dto.SecondaryManagerId,
                BranchId = dto.BranchId,
                IsActive = dto.IsActive,
                ApprovalTimeoutHours = dto.ApprovalTimeoutHours,
                AutoEscalateToSecondary = dto.AutoEscalateToSecondary,
                RequireDeptApprovalForAssignment = dto.RequireDeptApprovalForAssignment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Stores.Add(store);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created store {StoreCode} with ID {StoreId}", store.Code, store.Id);

            var result = await GetStoreByIdAsync(store.Id, cancellationToken);
            return result != null
                ? Result<StoreDto>.Success(result)
                : Result<StoreDto>.Failure("Store created but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating store {StoreCode}", dto.Code);
            return Result<StoreDto>.Failure($"Error creating store: {ex.Message}");
        }
    }

    public async Task<StoreDto?> GetStoreByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _context.Stores
                .Include(s => s.PrimaryManager)
                .Include(s => s.SecondaryManager)
                .Include(s => s.Branch)
                .Include(s => s.StoreUserRoles.Where(sur => sur.IsActive && !sur.IsDeleted))
                .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

            if (store == null)
            {
                return null;
            }

            var pendingRequestsCount = await _context.AssetRequests
                .CountAsync(r => r.StoreId == id &&
                    (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated),
                    cancellationToken);

            return MapToDto(store, pendingRequestsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store {StoreId}", id);
            return null;
        }
    }

    public async Task<StoreDto?> GetStoreByCodeAsync(string code, Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _context.Stores
                .Include(s => s.PrimaryManager)
                .Include(s => s.SecondaryManager)
                .Include(s => s.Branch)
                .Include(s => s.StoreUserRoles.Where(sur => sur.IsActive && !sur.IsDeleted))
                .FirstOrDefaultAsync(s => s.Code == code && s.CompanyId == companyId, cancellationToken);

            if (store == null)
            {
                return null;
            }

            var pendingRequestsCount = await _context.AssetRequests
                .CountAsync(r => r.StoreId == store.Id &&
                    (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated),
                    cancellationToken);

            return MapToDto(store, pendingRequestsCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store by code {StoreCode}", code);
            return null;
        }
    }

    public async Task<List<StoreDto>> GetStoresByCompanyAsync(Guid companyId, bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Stores
                .Include(s => s.PrimaryManager)
                .Include(s => s.SecondaryManager)
                .Include(s => s.Branch)
                .Include(s => s.StoreUserRoles.Where(sur => sur.IsActive && !sur.IsDeleted))
                .Where(s => s.CompanyId == companyId);

            if (!includeInactive)
            {
                query = query.Where(s => s.IsActive);
            }

            var stores = await query.OrderBy(s => s.Name).ToListAsync(cancellationToken);

            // Get pending request counts
            var storeIds = stores.Select(s => s.Id).ToList();
            var pendingCounts = await _context.AssetRequests
                .Where(r => storeIds.Contains(r.StoreId!.Value) &&
                    (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated))
                .GroupBy(r => r.StoreId)
                .Select(g => new { StoreId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.StoreId!.Value, x => x.Count, cancellationToken);

            return stores.Select(s => MapToDto(s, pendingCounts.GetValueOrDefault(s.Id, 0))).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting stores for company {CompanyId}", companyId);
            return new List<StoreDto>();
        }
    }

    public async Task<List<StoreLookupDto>> GetStoreLookupAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Stores
                .Where(s => s.CompanyId == companyId && s.IsActive)
                .OrderBy(s => s.Name)
                .Select(s => new StoreLookupDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store lookup for company {CompanyId}", companyId);
            return new List<StoreLookupDto>();
        }
    }

    public async Task<Result<StoreDto>> UpdateStoreAsync(Guid id, UpdateStoreDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _context.Stores.FindAsync(new object[] { id }, cancellationToken);
            if (store == null)
            {
                return Result<StoreDto>.Failure("Store not found.");
            }

            // Note: Code is immutable and not updated
            store.Name = dto.Name;
            store.Description = dto.Description;
            store.Address = dto.Address;
            store.City = dto.City;
            store.Phone = dto.Phone;
            store.Email = dto.Email;
            store.PrimaryManagerId = dto.PrimaryManagerId;
            store.SecondaryManagerId = dto.SecondaryManagerId;
            store.BranchId = dto.BranchId;
            store.IsActive = dto.IsActive;
            store.ApprovalTimeoutHours = dto.ApprovalTimeoutHours;
            store.AutoEscalateToSecondary = dto.AutoEscalateToSecondary;
            store.RequireDeptApprovalForAssignment = dto.RequireDeptApprovalForAssignment;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated store {StoreId}", id);

            var result = await GetStoreByIdAsync(id, cancellationToken);
            return result != null
                ? Result<StoreDto>.Success(result)
                : Result<StoreDto>.Failure("Store updated but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating store {StoreId}", id);
            return Result<StoreDto>.Failure($"Error updating store: {ex.Message}");
        }
    }

    public async Task<Result> DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _context.Stores.FindAsync(new object[] { id }, cancellationToken);
            if (store == null)
            {
                return Result.Failure("Store not found.");
            }

            // Check for pending requests
            var hasPendingRequests = await _context.AssetRequests
                .AnyAsync(r => r.StoreId == id &&
                    (r.Status == AssetRequestStatus.Pending || r.Status == AssetRequestStatus.Escalated),
                    cancellationToken);

            if (hasPendingRequests)
            {
                return Result.Failure("Cannot delete store with pending requests.");
            }

            // Soft delete
            store.IsDeleted = true;
            store.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted store {StoreId}", id);

            return Result.Success("Store deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting store {StoreId}", id);
            return Result.Failure($"Error deleting store: {ex.Message}");
        }
    }

    #endregion

    #region Store Manager Operations

    public async Task<Result<StoreDto>> AssignManagersAsync(Guid storeId, AssignStoreManagersDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var store = await _context.Stores.FindAsync(new object[] { storeId }, cancellationToken);
            if (store == null)
            {
                return Result<StoreDto>.Failure("Store not found.");
            }

            // Validate managers exist
            if (dto.PrimaryManagerId.HasValue)
            {
                var primaryExists = await _context.Users.AnyAsync(u => u.Id == dto.PrimaryManagerId.Value && !u.IsDeleted, cancellationToken);
                if (!primaryExists)
                {
                    return Result<StoreDto>.Failure("Primary manager not found.");
                }
            }

            if (dto.SecondaryManagerId.HasValue)
            {
                var secondaryExists = await _context.Users.AnyAsync(u => u.Id == dto.SecondaryManagerId.Value && !u.IsDeleted, cancellationToken);
                if (!secondaryExists)
                {
                    return Result<StoreDto>.Failure("Secondary manager not found.");
                }
            }

            store.PrimaryManagerId = dto.PrimaryManagerId;
            store.SecondaryManagerId = dto.SecondaryManagerId;
            store.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Assigned managers to store {StoreId}", storeId);

            var result = await GetStoreByIdAsync(storeId, cancellationToken);
            return result != null
                ? Result<StoreDto>.Success(result)
                : Result<StoreDto>.Failure("Managers assigned but store could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning managers to store {StoreId}", storeId);
            return Result<StoreDto>.Failure($"Error assigning managers: {ex.Message}");
        }
    }

    public async Task<List<StoreLookupDto>> GetManagedStoresAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.Stores
                .Where(s => s.IsActive && (s.PrimaryManagerId == userId || s.SecondaryManagerId == userId))
                .OrderBy(s => s.Name)
                .Select(s => new StoreLookupDto
                {
                    Id = s.Id,
                    Code = s.Code,
                    Name = s.Name
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting managed stores for user {UserId}", userId);
            return new List<StoreLookupDto>();
        }
    }

    #endregion

    #region Store Staff Operations

    public async Task<Result<StoreUserRoleDto>> AssignUserRoleAsync(Guid storeId, AssignStoreUserRoleDto dto, Guid assignedById, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate store exists
            var store = await _context.Stores.FindAsync(new object[] { storeId }, cancellationToken);
            if (store == null)
            {
                return Result<StoreUserRoleDto>.Failure("Store not found.");
            }

            // Validate user exists
            var user = await _context.Users.FindAsync(new object[] { dto.UserId }, cancellationToken);
            if (user == null || user.IsDeleted)
            {
                return Result<StoreUserRoleDto>.Failure("User not found.");
            }

            // Check if user already has this role in this store
            var existingRole = await _context.StoreUserRoles
                .FirstOrDefaultAsync(sur => sur.StoreId == storeId &&
                    sur.UserId == dto.UserId &&
                    sur.Role == dto.Role &&
                    sur.IsActive, cancellationToken);

            if (existingRole != null)
            {
                return Result<StoreUserRoleDto>.Failure("User already has this role in this store.");
            }

            var storeUserRole = new StoreUserRole
            {
                Id = Guid.NewGuid(),
                StoreId = storeId,
                UserId = dto.UserId,
                Role = dto.Role,
                IsActive = true,
                AssignedAt = DateTime.UtcNow,
                AssignedById = assignedById,
                Notes = dto.Notes,
                CreatedAt = DateTime.UtcNow
            };

            _context.StoreUserRoles.Add(storeUserRole);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Assigned role {Role} to user {UserId} in store {StoreId}", dto.Role, dto.UserId, storeId);

            // Return the created role with details
            var result = await _context.StoreUserRoles
                .Include(sur => sur.Store)
                .Include(sur => sur.User)
                .Include(sur => sur.AssignedBy)
                .FirstOrDefaultAsync(sur => sur.Id == storeUserRole.Id, cancellationToken);

            return result != null
                ? Result<StoreUserRoleDto>.Success(MapToStoreUserRoleDto(result))
                : Result<StoreUserRoleDto>.Failure("Role assigned but could not be retrieved.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning user role in store {StoreId}", storeId);
            return Result<StoreUserRoleDto>.Failure($"Error assigning user role: {ex.Message}");
        }
    }

    public async Task<Result> RevokeUserRoleAsync(Guid storeUserRoleId, Guid revokedById, CancellationToken cancellationToken = default)
    {
        try
        {
            var role = await _context.StoreUserRoles.FindAsync(new object[] { storeUserRoleId }, cancellationToken);
            if (role == null)
            {
                return Result.Failure("Store user role not found.");
            }

            role.IsActive = false;
            role.RevokedById = revokedById;
            role.RevokedAt = DateTime.UtcNow;
            role.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Revoked store user role {RoleId}", storeUserRoleId);

            return Result.Success("Role revoked successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking store user role {RoleId}", storeUserRoleId);
            return Result.Failure($"Error revoking role: {ex.Message}");
        }
    }

    public async Task<List<StoreStaffDto>> GetStoreStaffAsync(Guid storeId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Use IgnoreQueryFilters to bypass global filters and apply explicit filtering
            return await _context.StoreUserRoles
                .IgnoreQueryFilters()
                .Include(sur => sur.User)
                .Where(sur => sur.StoreId == storeId && sur.IsActive && !sur.IsDeleted)
                .OrderBy(sur => sur.Role)
                .ThenByDescending(sur => sur.AssignedAt)
                .Select(sur => new StoreStaffDto
                {
                    Id = sur.Id,
                    UserId = sur.UserId,
                    EmployeeCode = sur.User != null ? sur.User.EmployeeCode ?? "" : "",
                    UserName = sur.User != null ? sur.User.FullName : "Unknown",
                    UserEmail = sur.User != null ? sur.User.Email : null,
                    Role = sur.Role,
                    IsActive = sur.IsActive,
                    AssignedAt = sur.AssignedAt
                })
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting staff for store {StoreId}", storeId);
            return new List<StoreStaffDto>();
        }
    }

    public async Task<List<StoreUserRoleDto>> GetUserStoreRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = await _context.StoreUserRoles
                .Include(sur => sur.Store)
                .Include(sur => sur.User)
                .Include(sur => sur.AssignedBy)
                .Where(sur => sur.UserId == userId && sur.IsActive)
                .OrderBy(sur => sur.Store.Name)
                .ToListAsync(cancellationToken);

            return roles.Select(MapToStoreUserRoleDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting store roles for user {UserId}", userId);
            return new List<StoreUserRoleDto>();
        }
    }

    public async Task<bool> UserHasStoreRoleAsync(Guid userId, Guid storeId, StoreRole role, CancellationToken cancellationToken = default)
    {
        return await _context.StoreUserRoles
            .AnyAsync(sur => sur.UserId == userId &&
                sur.StoreId == storeId &&
                sur.Role == role &&
                sur.IsActive, cancellationToken);
    }

    public async Task<bool> IsStoreManagerAsync(Guid userId, Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await _context.Stores
            .FirstOrDefaultAsync(s => s.Id == storeId, cancellationToken);

        if (store == null) return false;

        return store.PrimaryManagerId == userId || store.SecondaryManagerId == userId;
    }

    #endregion

    #region Validation

    public async Task<bool> IsCodeUniqueAsync(string code, Guid companyId, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Stores.Where(s => s.Code == code && s.CompanyId == companyId);

        if (excludeId.HasValue)
        {
            query = query.Where(s => s.Id != excludeId.Value);
        }

        return !await query.AnyAsync(cancellationToken);
    }

    #endregion

    #region Private Mapping Methods

    private static StoreDto MapToDto(Store store, int pendingRequestsCount)
    {
        return new StoreDto
        {
            Id = store.Id,
            Code = store.Code,
            Name = store.Name,
            Description = store.Description,
            Address = store.Address,
            City = store.City,
            Phone = store.Phone,
            Email = store.Email,
            PrimaryManagerId = store.PrimaryManagerId,
            PrimaryManagerName = store.PrimaryManager?.FullName,
            PrimaryManagerEmployeeCode = store.PrimaryManager?.EmployeeCode,
            SecondaryManagerId = store.SecondaryManagerId,
            SecondaryManagerName = store.SecondaryManager?.FullName,
            SecondaryManagerEmployeeCode = store.SecondaryManager?.EmployeeCode,
            BranchId = store.BranchId,
            BranchName = store.Branch?.Name,
            IsActive = store.IsActive,
            ApprovalTimeoutHours = store.ApprovalTimeoutHours,
            AutoEscalateToSecondary = store.AutoEscalateToSecondary,
            RequireDeptApprovalForAssignment = store.RequireDeptApprovalForAssignment,
            StaffCount = store.StoreUserRoles?.Count(sur => sur.IsActive) ?? 0,
            PendingRequestsCount = pendingRequestsCount,
            CreatedAt = store.CreatedAt,
            UpdatedAt = store.UpdatedAt
        };
    }

    private static StoreUserRoleDto MapToStoreUserRoleDto(StoreUserRole role)
    {
        return new StoreUserRoleDto
        {
            Id = role.Id,
            StoreId = role.StoreId,
            StoreName = role.Store?.Name ?? "",
            UserId = role.UserId,
            UserName = role.User?.FullName ?? "",
            EmployeeCode = role.User?.EmployeeCode ?? "",
            Role = role.Role,
            IsActive = role.IsActive,
            AssignedAt = role.AssignedAt,
            AssignedByName = role.AssignedBy?.FullName,
            Notes = role.Notes
        };
    }

    #endregion
}
