using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ComplaintInformationSettings
/// </summary>
public class ComplaintInfoSettingsRepository : Repository<ComplaintInformationSettings>, IComplaintInfoSettingsRepository
{
    public ComplaintInfoSettingsRepository(ComplaintDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get settings by company ID
    /// </summary>
    public async Task<ComplaintInformationSettings?> GetByCompanyIdAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);
    }

    /// <summary>
    /// Get settings by company ID with company details
    /// </summary>
    public async Task<ComplaintInformationSettings?> GetByCompanyIdWithDetailsAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Company)
            .FirstOrDefaultAsync(s => s.CompanyId == companyId, cancellationToken);
    }
}
