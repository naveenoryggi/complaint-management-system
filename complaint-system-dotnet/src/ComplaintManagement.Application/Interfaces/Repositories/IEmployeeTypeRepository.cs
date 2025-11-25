using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IEmployeeTypeRepository : IRepository<EmployeeType>
{
    Task<IEnumerable<EmployeeType>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<EmployeeType?> GetEmployeeTypeWithUsersAsync(Guid id, CancellationToken cancellationToken = default);
}
