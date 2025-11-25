using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class ComplaintRoleRepository : Repository<ComplaintRole>, IComplaintRoleRepository
{
    public ComplaintRoleRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<ComplaintRole?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(r => r.Code == code, cancellationToken);
    }

    public async Task<IEnumerable<ComplaintRole>> GetByRoleTypeAsync(RoleType roleType, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.RoleType == roleType)
            .OrderBy(r => r.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ComplaintRole>> GetByEscalationLevelAsync(int level, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.EscalationLevel == level)
            .ToListAsync(cancellationToken);
    }

    public async Task<ComplaintRole?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(r => r.RolePermissions)
            .FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken);
    }
}
