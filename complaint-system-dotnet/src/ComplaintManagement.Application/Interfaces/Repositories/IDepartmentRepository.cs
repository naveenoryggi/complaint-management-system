using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IDepartmentRepository : IRepository<Department>
{
    Task<IEnumerable<Department>> GetByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    Task<Department?> GetDepartmentWithSectionsAsync(Guid id, CancellationToken cancellationToken = default);
}
