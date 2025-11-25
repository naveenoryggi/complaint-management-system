using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class UserComplaintRoleRepository : Repository<UserComplaintRole>, IUserComplaintRoleRepository
{
    public UserComplaintRoleRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<UserComplaintRole>> GetActiveRolesByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(r => r.UserId == userId &&
                       r.IsActive &&
                       r.EffectiveFrom <= now &&
                       (r.EffectiveTo == null || r.EffectiveTo >= now))
            .Include(r => r.ComplaintRole)
                .ThenInclude(cr => cr.RolePermissions)
            .Include(r => r.Company)
            .Include(r => r.Branch)
            .Include(r => r.Department)
            .Include(r => r.Section)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<UserComplaintRole>> GetRolesByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(r => r.CompanyId == companyId)
            .Include(r => r.User)
            .Include(r => r.ComplaintRole)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserComplaintRole?> GetPrimaryRoleAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Where(r => r.UserId == userId &&
                       r.IsPrimary &&
                       r.IsActive &&
                       r.EffectiveFrom <= now &&
                       (r.EffectiveTo == null || r.EffectiveTo >= now))
            .Include(r => r.ComplaintRole)
                .ThenInclude(cr => cr.RolePermissions)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
