using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class TenantRepository : Repository<Tenant>, ITenantRepository
{
    public TenantRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(t => t.Code == code, cancellationToken);
    }

    public async Task<Tenant?> GetTenantWithCompaniesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Companies)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }
}
