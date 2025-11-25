using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class EmployeeTypeRepository : Repository<EmployeeType>, IEmployeeTypeRepository
{
    public EmployeeTypeRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<EmployeeType>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(e => e.CompanyId == companyId)
            .ToListAsync(cancellationToken);
    }

    public async Task<EmployeeType?> GetEmployeeTypeWithUsersAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(e => e.Users)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }
}
