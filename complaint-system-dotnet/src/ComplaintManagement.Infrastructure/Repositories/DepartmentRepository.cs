using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class DepartmentRepository : Repository<Department>, IDepartmentRepository
{
    public DepartmentRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Department>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(d => d.BranchId == branchId)
            .Include(d => d.Manager)
            .Include(d => d.SecondaryManager)
            .Include(d => d.HrResponsible)
            .ToListAsync(cancellationToken);
    }

    public async Task<Department?> GetDepartmentWithSectionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(d => d.Sections)
            .Include(d => d.Manager)
            .Include(d => d.SecondaryManager)
            .Include(d => d.HrResponsible)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
    }
}
