using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface ISectionRepository : IRepository<Section>
{
    Task<IEnumerable<Section>> GetByDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);
}
