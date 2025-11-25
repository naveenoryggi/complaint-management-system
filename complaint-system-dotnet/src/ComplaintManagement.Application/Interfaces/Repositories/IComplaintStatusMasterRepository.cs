using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintStatusMasterRepository : IRepository<ComplaintStatusMaster>
{
    /// <summary>
    /// Get all complaint status masters with optional filtering
    /// </summary>
    /// <param name="companyId">Filter by company (null includes all system statuses)</param>
    /// <param name="isActive">Filter by active status</param>
    /// <param name="includeSystem">Include system-level statuses</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task<List<ComplaintStatusMaster>> GetAllAsync(
        Guid? companyId,
        bool? isActive,
        bool includeSystem,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a status code already exists for a company
    /// </summary>
    Task<bool> CodeExistsAsync(string code, Guid? companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get status by code for a specific company
    /// </summary>
    Task<ComplaintStatusMaster?> GetByCodeAsync(string code, Guid? companyId, CancellationToken cancellationToken = default);
}
