using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface ICompanyRepository : IRepository<Company>
{
    Task<IEnumerable<Company>> GetByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<Company?> GetCompanyWithBranchesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Company?> GetCompanyWithManagersAsync(Guid id, CancellationToken cancellationToken = default);
}
