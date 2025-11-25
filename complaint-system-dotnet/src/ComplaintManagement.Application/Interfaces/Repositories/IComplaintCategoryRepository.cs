using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintCategoryRepository : IRepository<ComplaintCategory>
{
    Task<IEnumerable<ComplaintCategory>> GetRootCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintCategory>> GetSubCategoriesAsync(Guid parentId, CancellationToken cancellationToken = default);
    Task<ComplaintCategory?> GetCategoryWithSubCategoriesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> IsCategoryCodeUniqueAsync(string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
