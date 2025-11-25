using ComplaintManagement.Domain.Entities.Escalation;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for EscalationMatrix entity
/// </summary>
public interface IEscalationMatrixRepository : IRepository<EscalationMatrix>
{
    /// <summary>
    /// Get escalation matrices by company ID
    /// </summary>
    Task<List<EscalationMatrix>> GetByCompanyIdAsync(Guid companyId, bool includeInactive = false);

    /// <summary>
    /// Get active escalation matrices by company ID with levels included
    /// </summary>
    Task<List<EscalationMatrix>> GetActiveMatricesByCompanyIdAsync(Guid companyId);

    /// <summary>
    /// Get applicable escalation matrix for a complaint
    /// Returns the most specific matrix that matches the complaint's attributes
    /// </summary>
    Task<EscalationMatrix?> GetApplicableMatrixAsync(
        Guid companyId,
        Guid? categoryId = null,
        Guid? branchId = null,
        Guid? departmentId = null);

    /// <summary>
    /// Get escalation matrix with levels included
    /// </summary>
    Task<EscalationMatrix?> GetWithLevelsAsync(Guid id);
}
