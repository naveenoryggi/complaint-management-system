using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class SectionRepository : Repository<Section>, ISectionRepository
{
    public SectionRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Section>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.DepartmentId == departmentId)
            .Include(s => s.Head)
            .Include(s => s.SecondaryHead)
            .Include(s => s.HrResponsible)
            .ToListAsync(cancellationToken);
    }
}
