namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for automatic escalation of complaints based on SLA breaches
/// </summary>
public interface IAutoEscalationService
{
    /// <summary>
    /// Process all complaints eligible for auto-escalation
    /// </summary>
    Task ProcessAutoEscalationsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check and escalate a specific complaint if needed
    /// </summary>
    Task<bool> CheckAndEscalateComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get complaints eligible for auto-escalation
    /// </summary>
    Task<List<Guid>> GetEligibleComplaintsAsync(CancellationToken cancellationToken = default);
}
