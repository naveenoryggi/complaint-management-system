using ComplaintManagement.Domain.Entities.Settings;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for ComplaintInformationSettings
/// </summary>
public interface IComplaintInfoSettingsRepository : IRepository<ComplaintInformationSettings>
{
    /// <summary>
    /// Get settings by company ID
    /// </summary>
    Task<ComplaintInformationSettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get settings by company ID with company details
    /// </summary>
    Task<ComplaintInformationSettings?> GetByCompanyIdWithDetailsAsync(Guid companyId, CancellationToken cancellationToken = default);
}
