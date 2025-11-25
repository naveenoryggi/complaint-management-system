using ComplaintManagement.Domain.Entities.Roles;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IUserComplaintRoleRepository : IRepository<UserComplaintRole>
{
    Task<IEnumerable<UserComplaintRole>> GetActiveRolesByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserComplaintRole>> GetRolesByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<UserComplaintRole?> GetPrimaryRoleAsync(Guid userId, CancellationToken cancellationToken = default);
}
