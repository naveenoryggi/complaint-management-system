using ComplaintManagement.Domain.Entities.Roles;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintRoleRepository : IRepository<ComplaintRole>
{
    Task<ComplaintRole?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintRole>> GetByRoleTypeAsync(RoleType roleType, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintRole>> GetByEscalationLevelAsync(int level, CancellationToken cancellationToken = default);
    Task<ComplaintRole?> GetRoleWithPermissionsAsync(Guid roleId, CancellationToken cancellationToken = default);
}
