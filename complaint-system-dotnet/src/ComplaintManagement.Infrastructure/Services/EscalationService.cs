using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Domain.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing complaint escalation
/// </summary>
public class EscalationService : IEscalationService
{
    private readonly IEscalationMatrixRepository _matrixRepository;
    private readonly IEscalationHistoryRepository _historyRepository;
    private readonly IComplaintRepository _complaintRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserComplaintRoleRepository _userRoleRepository;
    private readonly IResourcePoolService _resourcePoolService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EscalationService> _logger;

    public EscalationService(
        IEscalationMatrixRepository matrixRepository,
        IEscalationHistoryRepository historyRepository,
        IComplaintRepository complaintRepository,
        IUserRepository userRepository,
        IUserComplaintRoleRepository userRoleRepository,
        IResourcePoolService resourcePoolService,
        IUnitOfWork unitOfWork,
        ILogger<EscalationService> logger)
    {
        _matrixRepository = matrixRepository;
        _historyRepository = historyRepository;
        _complaintRepository = complaintRepository;
        _userRepository = userRepository;
        _userRoleRepository = userRoleRepository;
        _resourcePoolService = resourcePoolService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Escalation Matrix Management

    public async Task<EscalationMatrix> CreateMatrixAsync(CreateEscalationMatrixRequest request, Guid companyId, Guid createdBy)
    {
        _logger.LogInformation("Creating escalation matrix '{Name}' for company {CompanyId}", request.Name, companyId);

        var matrix = new EscalationMatrix
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            CompanyId = companyId,
            CategoryId = request.CategoryId,
            BranchId = request.BranchId,
            DepartmentId = request.DepartmentId,
            Priority = request.Priority,
            EnableAutoEscalation = request.EnableAutoEscalation,
            SendEmailNotifications = request.SendEmailNotifications,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Create escalation levels
        foreach (var levelRequest in request.EscalationLevels)
        {
            // Calculate TriggerAfterHours from TriggerAfterValue and TriggerTimeUnit
            // If TriggerAfterValue is provided, use it; otherwise fall back to TriggerAfterHours
            int triggerHours = levelRequest.TriggerAfterValue > 0
                ? (int)levelRequest.TriggerTimeUnit.ToHours(levelRequest.TriggerAfterValue)
                : levelRequest.TriggerAfterHours;

            var level = new EscalationLevel
            {
                Id = Guid.NewGuid(),
                EscalationMatrixId = matrix.Id,
                Level = levelRequest.Level,
                Name = levelRequest.Name,
                Description = levelRequest.Description,
                TriggerAfterHours = triggerHours,
                TriggerAfterValue = levelRequest.TriggerAfterValue,
                TriggerTimeUnit = levelRequest.TriggerTimeUnit,
                AssignmentStrategy = levelRequest.AssignmentStrategy,
                AssignToUserId = levelRequest.AssignToUserId,
                AssignToRole = levelRequest.AssignToRole,
                AssignToUserIds = levelRequest.AssignToUserIds,
                SendNotification = levelRequest.SendNotification,
                EmailTemplateId = levelRequest.EmailTemplateId,
                NotifyPreviousHandler = levelRequest.NotifyPreviousHandler,
                EscalationMessage = levelRequest.EscalationMessage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            matrix.EscalationLevels.Add(level);
        }

        await _matrixRepository.AddAsync(matrix);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created escalation matrix {MatrixId} with {LevelCount} levels",
            matrix.Id, matrix.EscalationLevels.Count);

        return matrix;
    }

    public async Task<EscalationMatrix> UpdateMatrixAsync(Guid matrixId, UpdateEscalationMatrixRequest request, Guid updatedBy)
    {
        _logger.LogInformation("=== START UpdateMatrixAsync for MatrixId: {MatrixId} ===", matrixId);
        _logger.LogInformation("Request received: Name={Name}, LevelCount={LevelCount}", request.Name, request.EscalationLevels.Count);

        // Load matrix WITH levels to get existing levels for deletion
        var matrix = await _matrixRepository.GetWithLevelsAsync(matrixId);
        if (matrix == null)
        {
            throw new KeyNotFoundException($"Escalation matrix {matrixId} not found");
        }

        _logger.LogInformation("Loaded matrix with {ExistingLevelCount} existing levels", matrix.EscalationLevels.Count);

        // Update matrix properties
        matrix.Name = request.Name;
        matrix.Description = request.Description;
        matrix.CategoryId = request.CategoryId;
        matrix.BranchId = request.BranchId;
        matrix.DepartmentId = request.DepartmentId;
        matrix.IsActive = request.IsActive;
        matrix.Priority = request.Priority;
        matrix.EnableAutoEscalation = request.EnableAutoEscalation;
        matrix.SendEmailNotifications = request.SendEmailNotifications;
        matrix.UpdatedAt = DateTime.UtcNow;
        matrix.UpdatedBy = updatedBy;

        // Delete ALL existing escalation levels using UnitOfWork
        var existingLevels = matrix.EscalationLevels.ToList();
        _logger.LogInformation("Removing {Count} existing levels via RemoveRange", existingLevels.Count);
        _unitOfWork.RemoveRange(existingLevels);

        // Save matrix updates and deletions first
        _matrixRepository.Update(matrix);
        await _unitOfWork.SaveChangesAsync();

        // Clear the collection to avoid tracking issues
        matrix.EscalationLevels.Clear();

        // Now add new escalation levels
        _logger.LogInformation("Adding {Count} new levels", request.EscalationLevels.Count);
        foreach (var levelRequest in request.EscalationLevels)
        {
            _logger.LogInformation("Processing level {Level}: TriggerAfterValue={TriggerValue}, TriggerTimeUnit={TimeUnit}, TriggerAfterHours={TriggerHours}",
                levelRequest.Level, levelRequest.TriggerAfterValue, levelRequest.TriggerTimeUnit, levelRequest.TriggerAfterHours);

            // Calculate TriggerAfterHours from TriggerAfterValue and TriggerTimeUnit
            int triggerHours = levelRequest.TriggerAfterValue > 0
                ? (int)levelRequest.TriggerTimeUnit.ToHours(levelRequest.TriggerAfterValue)
                : levelRequest.TriggerAfterHours;

            _logger.LogInformation("Calculated triggerHours: {TriggerHours} (from value={Value}, unit={Unit})",
                triggerHours, levelRequest.TriggerAfterValue, levelRequest.TriggerTimeUnit);

            var level = new EscalationLevel
            {
                Id = Guid.NewGuid(),
                EscalationMatrixId = matrixId,
                Level = levelRequest.Level,
                Name = levelRequest.Name,
                Description = levelRequest.Description,
                TriggerAfterHours = triggerHours,
                TriggerAfterValue = levelRequest.TriggerAfterValue,
                TriggerTimeUnit = levelRequest.TriggerTimeUnit,
                AssignmentStrategy = levelRequest.AssignmentStrategy,
                AssignToUserId = levelRequest.AssignToUserId,
                AssignToRole = levelRequest.AssignToRole,
                AssignToUserIds = levelRequest.AssignToUserIds,
                PrimaryContactId = levelRequest.PrimaryContactId,
                SecondaryContactId = levelRequest.SecondaryContactId,
                HrContactId = levelRequest.HrContactId,
                ResourcePoolId = levelRequest.ResourcePoolId,
                ResourcePoolAssignmentMethod = levelRequest.ResourcePoolAssignmentMethod,
                SendNotification = levelRequest.SendNotification,
                EmailTemplateId = levelRequest.EmailTemplateId,
                NotifyPreviousHandler = levelRequest.NotifyPreviousHandler,
                EscalationMessage = levelRequest.EscalationMessage,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = updatedBy
            };

            _logger.LogInformation("Creating EscalationLevel: TriggerAfterHours={Hours}, TriggerAfterValue={Value}, TriggerTimeUnit={Unit}",
                level.TriggerAfterHours, level.TriggerAfterValue, level.TriggerTimeUnit);

            // Add via UnitOfWork to ensure proper tracking
            await _unitOfWork.AddEntityAsync(level);
        }

        _logger.LogInformation("Calling SaveChangesAsync for new levels");
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("=== COMPLETED UpdateMatrixAsync for MatrixId: {MatrixId} ===", matrixId);

        // Reload matrix with levels to return
        var updatedMatrix = await _matrixRepository.GetWithLevelsAsync(matrixId);
        return updatedMatrix ?? matrix;
    }

    public async Task DeleteMatrixAsync(Guid matrixId, Guid deletedBy)
    {
        var matrix = await _matrixRepository.GetByIdAsync(matrixId);
        if (matrix == null)
        {
            throw new KeyNotFoundException($"Escalation matrix {matrixId} not found");
        }

        _matrixRepository.Delete(matrix);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted escalation matrix {MatrixId}", matrixId);
    }

    public async Task<EscalationMatrix?> GetMatrixByIdAsync(Guid matrixId)
    {
        return await _matrixRepository.GetWithLevelsAsync(matrixId);
    }

    public async Task<List<EscalationMatrix>> GetMatricesByCompanyAsync(Guid companyId)
    {
        return await _matrixRepository.GetByCompanyIdAsync(companyId, includeInactive: true);
    }

    public async Task<EscalationMatrix?> GetApplicableMatrixAsync(Complaint complaint)
    {
        return await _matrixRepository.GetApplicableMatrixAsync(
            complaint.CompanyId,
            complaint.CategoryId,
            complaint.BranchId,
            complaint.DepartmentId);
    }

    #endregion

    #region Escalation Level Management

    public async Task<EscalationLevel> AddLevelAsync(Guid matrixId, CreateEscalationLevelRequest request, Guid createdBy)
    {
        var matrix = await _matrixRepository.GetByIdAsync(matrixId);
        if (matrix == null)
        {
            throw new KeyNotFoundException($"Escalation matrix {matrixId} not found");
        }

        // Calculate TriggerAfterHours from TriggerAfterValue and TriggerTimeUnit
        int triggerHours = request.TriggerAfterValue > 0
            ? (int)request.TriggerTimeUnit.ToHours(request.TriggerAfterValue)
            : request.TriggerAfterHours;

        var level = new EscalationLevel
        {
            Id = Guid.NewGuid(),
            EscalationMatrixId = matrixId,
            Level = request.Level,
            Name = request.Name,
            Description = request.Description,
            TriggerAfterHours = triggerHours,
            TriggerAfterValue = request.TriggerAfterValue,
            TriggerTimeUnit = request.TriggerTimeUnit,
            AssignmentStrategy = request.AssignmentStrategy,
            AssignToUserId = request.AssignToUserId,
            AssignToRole = request.AssignToRole,
            AssignToUserIds = request.AssignToUserIds,
            SendNotification = request.SendNotification,
            EmailTemplateId = request.EmailTemplateId,
            NotifyPreviousHandler = request.NotifyPreviousHandler,
            EscalationMessage = request.EscalationMessage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Add level using repository (would need to add this method to IRepository)
        // For now, we'll add to matrix and update
        matrix.EscalationLevels.Add(level);
        _matrixRepository.Update(matrix);
        await _unitOfWork.SaveChangesAsync();

        return level;
    }

    public async Task<EscalationLevel> UpdateLevelAsync(Guid levelId, UpdateEscalationLevelRequest request, Guid updatedBy)
    {
        // Implementation would require IEscalationLevelRepository
        throw new NotImplementedException("EscalationLevel repository needed");
    }

    public async Task DeleteLevelAsync(Guid levelId, Guid deletedBy)
    {
        // Implementation would require IEscalationLevelRepository
        throw new NotImplementedException("EscalationLevel repository needed");
    }

    #endregion

    #region Complaint Escalation

    public async Task<EscalationHistory> EscalateComplaintAsync(
        Guid complaintId,
        string reason,
        Guid escalatedBy,
        Guid? targetMatrixId = null,
        int? targetLevel = null,
        bool isAutoEscalation = false)
    {
        _logger.LogInformation("Escalating complaint {ComplaintId}, reason: {Reason}, auto: {IsAuto}",
            complaintId, reason, isAutoEscalation);

        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null)
        {
            throw new KeyNotFoundException($"Complaint {complaintId} not found");
        }

        // Get applicable matrix
        EscalationMatrix? matrix;
        if (targetMatrixId.HasValue)
        {
            matrix = await _matrixRepository.GetWithLevelsAsync(targetMatrixId.Value);
        }
        else
        {
            matrix = await GetApplicableMatrixAsync(complaint);
        }

        if (matrix == null)
        {
            throw new InvalidOperationException($"No escalation matrix found for complaint {complaintId}");
        }

        // Determine next level
        int nextLevel = targetLevel ?? (complaint.CurrentEscalationLevel + 1);
        var escalationLevel = matrix.EscalationLevels
            .Where(l => l.IsActive && l.Level == nextLevel)
            .OrderBy(l => l.Level)
            .FirstOrDefault();

        if (escalationLevel == null)
        {
            throw new InvalidOperationException($"No escalation level {nextLevel} found in matrix {matrix.Id}");
        }

        // Determine next handler
        Guid nextHandler = await DetermineNextHandlerAsync(complaint, escalationLevel);

        // Calculate SLA breach info
        int? hoursOverdue = null;
        if (complaint.DueDate.HasValue && DateTime.UtcNow > complaint.DueDate.Value)
        {
            hoursOverdue = (int)(DateTime.UtcNow - complaint.DueDate.Value).TotalHours;
        }

        // Create escalation history record
        var history = new EscalationHistory
        {
            Id = Guid.NewGuid(),
            ComplaintId = complaintId,
            EscalationLevelId = escalationLevel.Id,
            EscalationMatrixId = matrix.Id,
            Level = nextLevel,
            FromUserId = complaint.AssignedToId,
            ToUserId = nextHandler,
            EscalatedBy = escalatedBy,
            EscalatedAt = DateTime.UtcNow,
            Reason = reason,
            IsAutoEscalation = isAutoEscalation,
            AssignmentStrategy = escalationLevel.AssignmentStrategy,
            Status = EscalationStatus.Triggered,
            SlaHoursAtEscalation = complaint.DueDate.HasValue ?
                (int?)(complaint.DueDate.Value - complaint.SubmittedAt).TotalHours : null,
            HoursOverdue = hoursOverdue,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = escalatedBy
        };

        await _historyRepository.AddAsync(history);

        // Update complaint
        complaint.CurrentEscalationLevel = nextLevel;
        complaint.AssignedToId = nextHandler;

        // Get "Escalated" status master
        var escalatedStatus = await _unitOfWork.ComplaintStatusMasters
            .FirstOrDefaultAsync(s => s.Name.Equals("Escalated", StringComparison.OrdinalIgnoreCase) && s.CompanyId == complaint.CompanyId);

        if (escalatedStatus != null)
        {
            complaint.StatusMasterId = escalatedStatus.Id;
        }

        complaint.UpdatedAt = DateTime.UtcNow;
        complaint.UpdatedBy = escalatedBy;

        _complaintRepository.Update(complaint);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Escalated complaint {ComplaintId} to level {Level}, assigned to {UserId}",
            complaintId, nextLevel, nextHandler);

        return history;
    }

    public async Task<EscalationHistory?> AutoEscalateComplaintAsync(Guid complaintId)
    {
        _logger.LogInformation("Auto-escalating complaint {ComplaintId}", complaintId);

        var complaint = await _complaintRepository.GetByIdAsync(complaintId);
        if (complaint == null)
        {
            _logger.LogWarning("Complaint {ComplaintId} not found for auto-escalation", complaintId);
            return null;
        }

        // Get applicable matrix
        var matrix = await GetApplicableMatrixAsync(complaint);
        if (matrix == null || !matrix.EnableAutoEscalation)
        {
            _logger.LogInformation("No auto-escalation matrix found for complaint {ComplaintId}", complaintId);
            return null;
        }

        // Determine next level based on current level and trigger hours
        int nextLevel = complaint.CurrentEscalationLevel + 1;
        var escalationLevel = matrix.EscalationLevels
            .Where(l => l.IsActive && l.Level == nextLevel)
            .OrderBy(l => l.Level)
            .FirstOrDefault();

        if (escalationLevel == null)
        {
            _logger.LogInformation("No more escalation levels for complaint {ComplaintId}", complaintId);
            return null;
        }

        // Check if trigger hours elapsed since DueDate
        if (complaint.DueDate.HasValue)
        {
            var hoursSinceDue = (DateTime.UtcNow - complaint.DueDate.Value).TotalHours;
            if (hoursSinceDue < escalationLevel.TriggerAfterHours)
            {
                _logger.LogDebug("Not yet time to escalate complaint {ComplaintId}, {HoursSinceDue} < {TriggerHours}",
                    complaintId, hoursSinceDue, escalationLevel.TriggerAfterHours);
                return null;
            }
        }

        // Perform escalation using system user (Guid.Empty represents system)
        return await EscalateComplaintAsync(
            complaintId,
            $"Auto-escalation triggered: SLA breach exceeded by {escalationLevel.TriggerAfterHours} hours",
            Guid.Empty, // System user
            matrix.Id,
            nextLevel,
            isAutoEscalation: true);
    }

    public async Task<bool> ShouldAutoEscalateAsync(Guid complaintId)
    {
        var complaint = await _complaintRepository.GetByIdWithIncludesAsync(
            complaintId,
            default,
            c => c.StatusMaster);

        if (complaint == null || complaint.StatusMaster.IsFinal)
        {
            return false;
        }

        if (!complaint.DueDate.HasValue || DateTime.UtcNow <= complaint.DueDate.Value)
        {
            return false;
        }

        var matrix = await GetApplicableMatrixAsync(complaint);
        return matrix != null && matrix.EnableAutoEscalation;
    }

    public async Task<List<Guid>> GetComplaintsEligibleForAutoEscalationAsync()
    {
        // This would typically be implemented as a specialized repository method
        // For now, returning empty list
        _logger.LogWarning("GetComplaintsEligibleForAutoEscalationAsync not fully implemented");
        return new List<Guid>();
    }

    public async Task<int> ProcessAutoEscalationsAsync()
    {
        _logger.LogInformation("Processing auto-escalations");

        var eligibleComplaintIds = await GetComplaintsEligibleForAutoEscalationAsync();
        int escalatedCount = 0;

        foreach (var complaintId in eligibleComplaintIds)
        {
            try
            {
                var result = await AutoEscalateComplaintAsync(complaintId);
                if (result != null)
                {
                    escalatedCount++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-escalating complaint {ComplaintId}", complaintId);
            }
        }

        _logger.LogInformation("Auto-escalated {Count} complaints", escalatedCount);
        return escalatedCount;
    }

    public async Task AcknowledgeEscalationAsync(Guid escalationHistoryId, Guid userId, string? notes = null)
    {
        var history = await _historyRepository.GetByIdAsync(escalationHistoryId);
        if (history == null)
        {
            throw new KeyNotFoundException($"Escalation history {escalationHistoryId} not found");
        }

        history.Status = EscalationStatus.Acknowledged;
        history.AcknowledgedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
        {
            history.Notes = notes;
        }
        history.UpdatedAt = DateTime.UtcNow;
        history.UpdatedBy = userId;

        _historyRepository.Update(history);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Escalation {EscalationId} acknowledged by user {UserId}", escalationHistoryId, userId);
    }

    public async Task<List<EscalationHistory>> GetEscalationHistoryAsync(Guid complaintId)
    {
        return await _historyRepository.GetByComplaintIdAsync(complaintId);
    }

    public async Task<List<EscalationHistory>> GetPendingEscalationsAsync(Guid userId)
    {
        return await _historyRepository.GetPendingEscalationsAsync(userId);
    }

    #endregion

    #region Assignment Strategies

    public async Task<Guid> DetermineNextHandlerAsync(Complaint complaint, EscalationLevel level)
    {
        _logger.LogDebug("Determining next handler for complaint {ComplaintId} using strategy {Strategy}",
            complaint.Id, level.AssignmentStrategy);

        switch (level.AssignmentStrategy)
        {
            case AssignmentStrategy.ReportingManager:
                // Escalate to reporting manager of currently assigned user (or complainant if unassigned)
                var managerId = await GetReportingManagerAsync(complaint.AssignedToId ?? complaint.ComplainantId);
                if (!managerId.HasValue)
                {
                    throw new InvalidOperationException($"No reporting manager found for user {complaint.AssignedToId ?? complaint.ComplainantId}");
                }
                return managerId.Value;

            case AssignmentStrategy.SectionIncharge:
            case AssignmentStrategy.BranchContacts:
            case AssignmentStrategy.DepartmentContacts:
                // Use contact hierarchy: Primary → Secondary → HR
                if (level.PrimaryContactId.HasValue)
                {
                    _logger.LogDebug("Assigning to primary contact {UserId}", level.PrimaryContactId.Value);
                    return level.PrimaryContactId.Value;
                }
                if (level.SecondaryContactId.HasValue)
                {
                    _logger.LogDebug("Primary contact not available, assigning to secondary contact {UserId}", level.SecondaryContactId.Value);
                    return level.SecondaryContactId.Value;
                }
                if (level.HrContactId.HasValue)
                {
                    _logger.LogDebug("Primary and secondary contacts not available, assigning to HR contact {UserId}", level.HrContactId.Value);
                    return level.HrContactId.Value;
                }
                throw new InvalidOperationException($"No contacts configured for {level.AssignmentStrategy} strategy in level {level.Level}");

            case AssignmentStrategy.AdminEscalation:
                // Assign to a predefined admin user (could be configured in level.AssignToUserId)
                if (level.AssignToUserId.HasValue)
                {
                    return level.AssignToUserId.Value;
                }
                // Otherwise, find company admin
                var adminUsers = await GetUsersByRoleAsync("Admin", complaint.CompanyId);
                if (adminUsers.Count == 0)
                {
                    throw new InvalidOperationException($"No admin users found for company {complaint.CompanyId}");
                }
                return adminUsers.First();

            case AssignmentStrategy.ResourcePool:
                if (!level.ResourcePoolId.HasValue)
                {
                    throw new InvalidOperationException("ResourcePool strategy requires ResourcePoolId");
                }

                // Determine assignment method
                var assignmentMethod = level.ResourcePoolAssignmentMethod?.ToString() ?? "Manual";

                if (assignmentMethod == "Manual")
                {
                    throw new InvalidOperationException("Manual resource pool assignment requires explicit user selection");
                }

                // Use ResourcePoolService to get next user
                var poolService = GetResourcePoolService();
                return await poolService.GetNextUserFromPoolAsync(level.ResourcePoolId.Value, assignmentMethod);

            default:
                throw new NotImplementedException($"Assignment strategy {level.AssignmentStrategy} not implemented");
        }
    }

    private IResourcePoolService GetResourcePoolService()
    {
        return _resourcePoolService;
    }

    public async Task<Guid?> GetReportingManagerAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user?.ManagerId;
    }

    public async Task<List<Guid>> GetUsersByRoleAsync(string roleName, Guid? companyId = null)
    {
        // This would query UserComplaintRoles to find users with the specified role
        // For now, returning empty list as placeholder
        _logger.LogWarning("GetUsersByRoleAsync not fully implemented");
        return new List<Guid>();
    }

    public async Task<Guid> GetNextRoundRobinUserAsync(List<Guid> userIds, Guid? lastAssignedUserId = null)
    {
        if (userIds.Count == 0)
        {
            throw new ArgumentException("User IDs list cannot be empty", nameof(userIds));
        }

        if (!lastAssignedUserId.HasValue)
        {
            return userIds.First();
        }

        var lastIndex = userIds.IndexOf(lastAssignedUserId.Value);
        var nextIndex = (lastIndex + 1) % userIds.Count;
        return userIds[nextIndex];
    }

    public async Task<Guid> GetLeastLoadedUserAsync(List<Guid> userIds)
    {
        if (userIds.Count == 0)
        {
            throw new ArgumentException("User IDs list cannot be empty", nameof(userIds));
        }

        // Count active complaints per user and return the one with least
        var userLoads = new Dictionary<Guid, int>();

        foreach (var userId in userIds)
        {
            // Count would be done via repository
            // For now, using simple logic
            userLoads[userId] = 0; // Placeholder
        }

        return userLoads.OrderBy(kvp => kvp.Value).First().Key;
    }

    #endregion
}
