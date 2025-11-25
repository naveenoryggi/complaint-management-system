using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.Escalation;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for complaint escalation operations
/// </summary>
public interface IEscalationService
{
    #region Escalation Matrix Management

    /// <summary>
    /// Create a new escalation matrix
    /// </summary>
    Task<EscalationMatrix> CreateMatrixAsync(CreateEscalationMatrixRequest request, Guid companyId, Guid createdBy);

    /// <summary>
    /// Update an existing escalation matrix
    /// </summary>
    Task<EscalationMatrix> UpdateMatrixAsync(Guid matrixId, UpdateEscalationMatrixRequest request, Guid updatedBy);

    /// <summary>
    /// Delete an escalation matrix
    /// </summary>
    Task DeleteMatrixAsync(Guid matrixId, Guid deletedBy);

    /// <summary>
    /// Get escalation matrix by ID
    /// </summary>
    Task<EscalationMatrix?> GetMatrixByIdAsync(Guid matrixId);

    /// <summary>
    /// Get all matrices for a company
    /// </summary>
    Task<List<EscalationMatrix>> GetMatricesByCompanyAsync(Guid companyId);

    /// <summary>
    /// Get applicable escalation matrix for a complaint
    /// </summary>
    Task<EscalationMatrix?> GetApplicableMatrixAsync(Complaint complaint);

    #endregion

    #region Escalation Level Management

    /// <summary>
    /// Add a level to an escalation matrix
    /// </summary>
    Task<EscalationLevel> AddLevelAsync(Guid matrixId, CreateEscalationLevelRequest request, Guid createdBy);

    /// <summary>
    /// Update an escalation level
    /// </summary>
    Task<EscalationLevel> UpdateLevelAsync(Guid levelId, UpdateEscalationLevelRequest request, Guid updatedBy);

    /// <summary>
    /// Delete an escalation level
    /// </summary>
    Task DeleteLevelAsync(Guid levelId, Guid deletedBy);

    #endregion

    #region Complaint Escalation

    /// <summary>
    /// Escalate a complaint (manual or auto escalation)
    /// </summary>
    Task<EscalationHistory> EscalateComplaintAsync(
        Guid complaintId,
        string reason,
        Guid escalatedBy,
        Guid? targetMatrixId = null,
        int? targetLevel = null,
        bool isAutoEscalation = false);

    /// <summary>
    /// Auto-escalate a complaint (triggered by SLA breach)
    /// </summary>
    Task<EscalationHistory?> AutoEscalateComplaintAsync(Guid complaintId);

    /// <summary>
    /// Acknowledge an escalation
    /// </summary>
    Task AcknowledgeEscalationAsync(Guid escalationHistoryId, Guid userId, string? notes = null);

    /// <summary>
    /// Get escalation history for a complaint
    /// </summary>
    Task<List<EscalationHistory>> GetEscalationHistoryAsync(Guid complaintId);

    /// <summary>
    /// Get pending escalations for a user
    /// </summary>
    Task<List<EscalationHistory>> GetPendingEscalationsAsync(Guid userId);

    #endregion

    #region Assignment Strategies

    /// <summary>
    /// Determine next handler based on escalation level assignment strategy
    /// </summary>
    Task<Guid> DetermineNextHandlerAsync(Complaint complaint, EscalationLevel level);

    /// <summary>
    /// Get user by reporting chain (manager)
    /// </summary>
    Task<Guid?> GetReportingManagerAsync(Guid userId);

    /// <summary>
    /// Get users by role
    /// </summary>
    Task<List<Guid>> GetUsersByRoleAsync(string roleName, Guid? companyId = null);

    /// <summary>
    /// Get next user in round-robin rotation
    /// </summary>
    Task<Guid> GetNextRoundRobinUserAsync(List<Guid> userIds, Guid? lastAssignedUserId = null);

    /// <summary>
    /// Get user with least active complaints
    /// </summary>
    Task<Guid> GetLeastLoadedUserAsync(List<Guid> userIds);

    #endregion

    #region SLA & Auto-Escalation

    /// <summary>
    /// Check if complaint should be auto-escalated based on SLA
    /// </summary>
    Task<bool> ShouldAutoEscalateAsync(Guid complaintId);

    /// <summary>
    /// Get complaints eligible for auto-escalation
    /// </summary>
    Task<List<Guid>> GetComplaintsEligibleForAutoEscalationAsync();

    /// <summary>
    /// Process auto-escalation for all eligible complaints
    /// </summary>
    Task<int> ProcessAutoEscalationsAsync();

    #endregion
}
