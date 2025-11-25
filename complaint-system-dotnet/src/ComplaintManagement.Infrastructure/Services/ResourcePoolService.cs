using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for resource pool operations
/// </summary>
public class ResourcePoolService : IResourcePoolService
{
    private readonly ComplaintDbContext _context;

    public ResourcePoolService(ComplaintDbContext context)
    {
        _context = context;
    }

    #region Resource Pool Management

    public async Task<ResourcePool> CreatePoolAsync(CreateResourcePoolRequest request, Guid companyId, Guid createdBy)
    {
        var pool = new ResourcePool
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Name = request.Name,
            Description = request.Description,
            PoolType = request.PoolType,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            SectionId = request.SectionId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _context.ResourcePools.Add(pool);
        await _context.SaveChangesAsync();

        // Add initial members if provided
        if (request.MemberUserIds != null && request.MemberUserIds.Any())
        {
            foreach (var userId in request.MemberUserIds)
            {
                await AddMemberAsync(pool.Id, userId, createdBy);
            }
        }

        return pool;
    }

    public async Task<ResourcePool> UpdatePoolAsync(Guid poolId, UpdateResourcePoolRequest request, Guid updatedBy)
    {
        var pool = await _context.ResourcePools.FindAsync(poolId);
        if (pool == null)
            throw new KeyNotFoundException($"Resource pool with ID {poolId} not found");

        pool.Name = request.Name;
        pool.Description = request.Description;
        pool.PoolType = request.PoolType;
        pool.BranchId = request.BranchId;
        pool.DepartmentId = request.DepartmentId;
        pool.SectionId = request.SectionId;
        pool.IsActive = request.IsActive;
        pool.UpdatedAt = DateTime.UtcNow;
        pool.UpdatedBy = updatedBy;

        await _context.SaveChangesAsync();
        return pool;
    }

    public async Task DeletePoolAsync(Guid poolId, Guid deletedBy)
    {
        var pool = await _context.ResourcePools.FindAsync(poolId);
        if (pool == null)
            throw new KeyNotFoundException($"Resource pool with ID {poolId} not found");

        pool.IsDeleted = true;
        pool.DeletedAt = DateTime.UtcNow;
        pool.DeletedBy = deletedBy;

        await _context.SaveChangesAsync();
    }

    public async Task<ResourcePool?> GetPoolByIdAsync(Guid poolId)
    {
        return await _context.ResourcePools
            .Include(p => p.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Include(p => p.Branch)
            .Include(p => p.Department)
            .Include(p => p.Section)
            .FirstOrDefaultAsync(p => p.Id == poolId);
    }

    public async Task<List<ResourcePool>> GetPoolsByCompanyAsync(Guid companyId, bool activeOnly = true)
    {
        var query = _context.ResourcePools
            .Include(p => p.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Include(p => p.Branch)
            .Include(p => p.Department)
            .Include(p => p.Section)
            .Where(p => p.CompanyId == companyId);

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<List<ResourcePool>> GetPoolsByTypeAsync(Guid companyId, string poolType, bool activeOnly = true)
    {
        if (!Enum.TryParse<ResourcePoolType>(poolType, true, out var type))
            throw new ArgumentException($"Invalid pool type: {poolType}");

        var query = _context.ResourcePools
            .Include(p => p.Members.Where(m => m.IsActive))
                .ThenInclude(m => m.User)
            .Where(p => p.CompanyId == companyId && p.PoolType == type);

        if (activeOnly)
            query = query.Where(p => p.IsActive);

        return await query.OrderBy(p => p.Name).ToListAsync();
    }

    #endregion

    #region Resource Pool Member Management

    public async Task<ResourcePoolMember> AddMemberAsync(Guid poolId, Guid userId, Guid addedBy)
    {
        // Check if already a member
        var existing = await _context.ResourcePoolMembers
            .FirstOrDefaultAsync(m => m.ResourcePoolId == poolId && m.UserId == userId && !m.IsDeleted);

        if (existing != null)
        {
            if (!existing.IsActive)
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.UpdatedBy = addedBy;
                await _context.SaveChangesAsync();
            }
            return existing;
        }

        var member = new ResourcePoolMember
        {
            Id = Guid.NewGuid(),
            ResourcePoolId = poolId,
            UserId = userId,
            AddedAt = DateTime.UtcNow,
            AddedBy = addedBy,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = addedBy
        };

        _context.ResourcePoolMembers.Add(member);
        await _context.SaveChangesAsync();
        return member;
    }

    public async Task RemoveMemberAsync(Guid poolId, Guid userId, Guid removedBy)
    {
        var member = await _context.ResourcePoolMembers
            .FirstOrDefaultAsync(m => m.ResourcePoolId == poolId && m.UserId == userId);

        if (member != null)
        {
            member.IsActive = false;
            member.UpdatedAt = DateTime.UtcNow;
            member.UpdatedBy = removedBy;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<ResourcePoolMember>> GetPoolMembersAsync(Guid poolId, bool activeOnly = true)
    {
        var query = _context.ResourcePoolMembers
            .Include(m => m.User)
            .Where(m => m.ResourcePoolId == poolId);

        if (activeOnly)
            query = query.Where(m => m.IsActive);

        return await query.ToListAsync();
    }

    public async Task<bool> IsMemberAsync(Guid poolId, Guid userId)
    {
        return await _context.ResourcePoolMembers
            .AnyAsync(m => m.ResourcePoolId == poolId && m.UserId == userId && m.IsActive);
    }

    #endregion

    #region Assignment from Pool

    public async Task<Guid> GetNextUserFromPoolAsync(Guid poolId, string assignmentMethod)
    {
        if (!Enum.TryParse<ResourcePoolAssignmentMethod>(assignmentMethod, true, out var method))
            throw new ArgumentException($"Invalid assignment method: {assignmentMethod}");

        return method switch
        {
            ResourcePoolAssignmentMethod.RoundRobin => await GetNextRoundRobinMemberAsync(poolId),
            ResourcePoolAssignmentMethod.LeastBusy => await GetLeastBusyMemberAsync(poolId),
            _ => throw new NotImplementedException($"Assignment method {assignmentMethod} requires manual selection")
        };
    }

    public async Task<Guid> GetLeastBusyMemberAsync(Guid poolId)
    {
        var members = await GetPoolMembersAsync(poolId, activeOnly: true);
        if (!members.Any())
            throw new InvalidOperationException($"No active members in pool {poolId}");

        // Get complaint counts for each member
        var memberWorkload = new Dictionary<Guid, int>();
        foreach (var member in members)
        {
            var activeComplaints = await _context.Complaints
                .Include(c => c.StatusMaster)
                .CountAsync(c => c.AssignedToId == member.UserId && !c.StatusMaster.IsFinal);
            memberWorkload[member.UserId] = activeComplaints;
        }

        // Return user with least complaints
        return memberWorkload.OrderBy(kv => kv.Value).First().Key;
    }

    public async Task<Guid> GetNextRoundRobinMemberAsync(Guid poolId)
    {
        var members = await GetPoolMembersAsync(poolId, activeOnly: true);
        if (!members.Any())
            throw new InvalidOperationException($"No active members in pool {poolId}");

        // Simple round-robin: get last assigned and pick next
        // This is a simplified implementation - in production you'd track last assignment
        var lastAssignment = await _context.Complaints
            .Where(c => c.ResourcePoolId == poolId)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => c.AssignedToId)
            .FirstOrDefaultAsync();

        if (lastAssignment == null || lastAssignment == Guid.Empty)
            return members.First().UserId;

        var currentIndex = members.FindIndex(m => m.UserId == lastAssignment);
        var nextIndex = (currentIndex + 1) % members.Count;
        return members[nextIndex].UserId;
    }

    #endregion
}
