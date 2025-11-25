using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IBranchRepository : IRepository<Branch>
{
    Task<IEnumerable<Branch>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<Branch?> GetBranchWithDepartmentsAsync(Guid id, CancellationToken cancellationToken = default);
}
