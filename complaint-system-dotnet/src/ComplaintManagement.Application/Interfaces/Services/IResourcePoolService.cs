using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Domain.Entities.Escalation;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for resource pool operations
/// </summary>
public interface IResourcePoolService
{
    #region Resource Pool Management

    /// <summary>
    /// Create a new resource pool
    /// </summary>
    Task<ResourcePool> CreatePoolAsync(CreateResourcePoolRequest request, Guid companyId, Guid createdBy);

    /// <summary>
    /// Update an existing resource pool
    /// </summary>
    Task<ResourcePool> UpdatePoolAsync(Guid poolId, UpdateResourcePoolRequest request, Guid updatedBy);

    /// <summary>
    /// Delete a resource pool
    /// </summary>
    Task DeletePoolAsync(Guid poolId, Guid deletedBy);

    /// <summary>
    /// Get resource pool by ID
    /// </summary>
    Task<ResourcePool?> GetPoolByIdAsync(Guid poolId);

    /// <summary>
    /// Get all active resource pools for a company
    /// </summary>
    Task<List<ResourcePool>> GetPoolsByCompanyAsync(Guid companyId, bool activeOnly = true);

    /// <summary>
    /// Get resource pools by type
    /// </summary>
    Task<List<ResourcePool>> GetPoolsByTypeAsync(Guid companyId, string poolType, bool activeOnly = true);

    #endregion

    #region Resource Pool Member Management

    /// <summary>
    /// Add a member to a resource pool
    /// </summary>
    Task<ResourcePoolMember> AddMemberAsync(Guid poolId, Guid userId, Guid addedBy);

    /// <summary>
    /// Remove a member from a resource pool
    /// </summary>
    Task RemoveMemberAsync(Guid poolId, Guid userId, Guid removedBy);

    /// <summary>
    /// Get all members of a resource pool
    /// </summary>
    Task<List<ResourcePoolMember>> GetPoolMembersAsync(Guid poolId, bool activeOnly = true);

    /// <summary>
    /// Check if a user is a member of a resource pool
    /// </summary>
    Task<bool> IsMemberAsync(Guid poolId, Guid userId);

    #endregion

    #region Assignment from Pool

    /// <summary>
    /// Get next user from pool based on assignment method
    /// </summary>
    Task<Guid> GetNextUserFromPoolAsync(Guid poolId, string assignmentMethod);

    /// <summary>
    /// Get user with least active complaints from pool
    /// </summary>
    Task<Guid> GetLeastBusyMemberAsync(Guid poolId);

    /// <summary>
    /// Get next user in round-robin rotation from pool
    /// </summary>
    Task<Guid> GetNextRoundRobinMemberAsync(Guid poolId);

    #endregion
}
