using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for Store management operations
/// </summary>
public interface IStoreService
{
    #region Store CRUD Operations

    /// <summary>
    /// Creates a new store
    /// </summary>
    Task<Result<StoreDto>> CreateStoreAsync(CreateStoreDto dto, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a store by ID
    /// </summary>
    Task<StoreDto?> GetStoreByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a store by code within a company
    /// </summary>
    Task<StoreDto?> GetStoreByCodeAsync(string code, Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stores for a company
    /// </summary>
    Task<List<StoreDto>> GetStoresByCompanyAsync(Guid companyId, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stores for dropdown/lookup
    /// </summary>
    Task<List<StoreLookupDto>> GetStoreLookupAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing store
    /// </summary>
    Task<Result<StoreDto>> UpdateStoreAsync(Guid id, UpdateStoreDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a store (soft delete)
    /// </summary>
    Task<Result> DeleteStoreAsync(Guid id, CancellationToken cancellationToken = default);

    #endregion

    #region Store Manager Operations

    /// <summary>
    /// Assigns primary and/or secondary manager to a store
    /// </summary>
    Task<Result<StoreDto>> AssignManagersAsync(Guid storeId, AssignStoreManagersDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stores managed by a user (as primary or secondary manager)
    /// </summary>
    Task<List<StoreLookupDto>> GetManagedStoresAsync(Guid userId, CancellationToken cancellationToken = default);

    #endregion

    #region Store Staff Operations

    /// <summary>
    /// Assigns a user role in a store
    /// </summary>
    Task<Result<StoreUserRoleDto>> AssignUserRoleAsync(Guid storeId, AssignStoreUserRoleDto dto, Guid assignedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a user's role in a store
    /// </summary>
    Task<Result> RevokeUserRoleAsync(Guid storeUserRoleId, Guid revokedById, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all staff members of a store
    /// </summary>
    Task<List<StoreStaffDto>> GetStoreStaffAsync(Guid storeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stores a user is assigned to
    /// </summary>
    Task<List<StoreUserRoleDto>> GetUserStoreRolesAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user has a specific role in a store
    /// </summary>
    Task<bool> UserHasStoreRoleAsync(Guid userId, Guid storeId, Domain.Enums.Service.StoreRole role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if user is a manager (primary or secondary) of a store
    /// </summary>
    Task<bool> IsStoreManagerAsync(Guid userId, Guid storeId, CancellationToken cancellationToken = default);

    #endregion

    #region Validation

    /// <summary>
    /// Validates if store code is unique within company
    /// </summary>
    Task<bool> IsCodeUniqueAsync(string code, Guid companyId, Guid? excludeId = null, CancellationToken cancellationToken = default);

    #endregion
}
