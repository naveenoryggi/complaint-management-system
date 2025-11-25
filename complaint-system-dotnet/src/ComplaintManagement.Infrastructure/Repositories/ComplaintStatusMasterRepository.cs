using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class ComplaintStatusMasterRepository : Repository<ComplaintStatusMaster>, IComplaintStatusMasterRepository
{
    public ComplaintStatusMasterRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<List<ComplaintStatusMaster>> GetAllAsync(
        Guid? companyId,
        bool? isActive,
        bool includeSystem,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        // Filter by company (include null for system statuses)
        if (companyId.HasValue)
        {
            query = query.Where(x => x.CompanyId == companyId || x.CompanyId == null);
        }
        else if (!includeSystem)
        {
            query = query.Where(x => x.CompanyId != null);
        }

        // Filter by active status
        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        return await query
            .Where(x => !x.IsDeleted)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => !x.IsDeleted)
            .Where(x => x.Code == code)
            .Where(x => x.CompanyId == companyId)
            .AnyAsync(cancellationToken);
    }

    public async Task<ComplaintStatusMaster?> GetByCodeAsync(string code, Guid? companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(x => !x.IsDeleted)
            .Where(x => x.Code == code)
            .Where(x => x.CompanyId == companyId || (companyId == null && x.CompanyId == null))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
