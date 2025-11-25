using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintPriorityMasterRepository : IRepository<ComplaintPriorityMaster>
{
    /// <summary>
    /// Get all complaint priority masters with optional filtering
    /// </summary>
    /// <param name="companyId">Filter by company (null includes all system priorities)</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="includeSystem">Include system-level priorities</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<ComplaintPriorityMaster>> GetAllAsync(
        Guid? companyId,
        bool? isActive,
        bool includeSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a priority code already exists for a company
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get priority by code for a specific company
    /// </summary>
    Task<ComplaintPriorityMaster?> GetByCodeAsync(string code, Guid? companyId, CancellationToken cancellationToken = default);
}
