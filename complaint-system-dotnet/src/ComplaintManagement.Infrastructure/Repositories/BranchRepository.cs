using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class BranchRepository : Repository<Branch>, IBranchRepository
{
    public BranchRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Branch>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Manager)
            .Include(b => b.SecondaryManager)
            .Include(b => b.HrResponsible)
            .Where(b => b.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Branch?> GetBranchWithDepartmentsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Departments)
            .Include(b => b.Manager)
            .Include(b => b.SecondaryManager)
            .Include(b => b.HrResponsible)
            .Include(b => b.Users)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public override async Task<Branch?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(b => b.Manager)
            .Include(b => b.SecondaryManager)
            .Include(b => b.HrResponsible)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }
}
