using ComplaintManagement.Domain.Entities.Escalation;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for EscalationHistory entity
/// </summary>
public interface IEscalationHistoryRepository : IRepository<EscalationHistory>
{
    /// <summary>
    /// Get escalation history for a complaint
    /// </summary>
    Task<List<EscalationHistory>> GetByComplaintIdAsync(Guid complaintId);

    /// <summary>
    /// Get current escalation level for a complaint
    /// </summary>
    Task<EscalationHistory?> GetCurrentEscalationAsync(Guid complaintId);

    /// <summary>
    /// Get pending escalations (triggered but not acknowledged)
    /// </summary>
    Task<List<EscalationHistory>> GetPendingEscalationsAsync(Guid userId);

    /// <summary>
    /// Get escalation statistics for a user
    /// </summary>
    Task<Dictionary<string, int>> GetEscalationStatsAsync(Guid userId, DateTime? fromDate = null);
}
