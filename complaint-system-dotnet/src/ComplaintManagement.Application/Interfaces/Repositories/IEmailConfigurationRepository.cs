using ComplaintManagement.Domain.Entities.Communication;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for EmailConfiguration entity
/// </summary>
public interface IEmailConfigurationRepository : IRepository<EmailConfiguration>
{
    /// <summary>
    /// Get email configurations by company ID
    /// </summary>
    Task<IEnumerable<EmailConfiguration>> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get enabled email configurations for polling
    /// </summary>
    Task<IEnumerable<EmailConfiguration>> GetEnabledConfigurationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get email configuration by company ID (single)
    /// </summary>
    Task<EmailConfiguration?> GetByCompanyIdSingleAsync(Guid companyId, CancellationToken cancellationToken = default);
}
