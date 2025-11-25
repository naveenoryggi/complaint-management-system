using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

public class AutoEscalationService : IAutoEscalationService
{
    private readonly IComplaintRepository _complaintRepository;
    private readonly IEscalationMatrixRepository _matrixRepository;
    private readonly IEscalationHistoryRepository _historyRepository;
    private readonly IEscalationService _escalationService;
    private readonly ITimeZoneService _timeZoneService;
    private readonly ComplaintDbContext _dbContext;
    private readonly ILogger<AutoEscalationService> _logger;

    public AutoEscalationService(
        IComplaintRepository complaintRepository,
        IEscalationMatrixRepository matrixRepository,
        IEscalationHistoryRepository historyRepository,
        IEscalationService escalationService,
        ITimeZoneService timeZoneService,
        ComplaintDbContext dbContext,
        ILogger<AutoEscalationService> logger)
    {
        _complaintRepository = complaintRepository;
        _matrixRepository = matrixRepository;
        _historyRepository = historyRepository;
        _escalationService = escalationService;
        _timeZoneService = timeZoneService;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task ProcessAutoEscalationsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting auto-escalation process at {Time}", DateTime.UtcNow);

            var eligibleComplaintIds = await GetEligibleComplaintsAsync(cancellationToken);

            _logger.LogInformation("Found {Count} complaints eligible for auto-escalation", eligibleComplaintIds.Count);

            var successCount = 0;
            var failureCount = 0;

            foreach (var complaintId in eligibleComplaintIds)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogWarning("Auto-escalation process cancelled");
                    break;
                }

                try
                {
                    var escalated = await CheckAndEscalateComplaintAsync(complaintId, cancellationToken);
                    if (escalated)
                    {
                        successCount++;
                        _logger.LogInformation("Successfully auto-escalated complaint {ComplaintId}", complaintId);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    _logger.LogError(ex, "Failed to auto-escalate complaint {ComplaintId}", complaintId);
                }
            }

            _logger.LogInformation(
                "Auto-escalation process completed. Success: {Success}, Failed: {Failed}, Total: {Total}",
                successCount, failureCount, eligibleComplaintIds.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto-escalation process");
            throw;
        }
    }

    public async Task<bool> CheckAndEscalateComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        try
        {
            var complaint = await _complaintRepository.GetByIdWithIncludesAsync(
                complaintId,
                default,
                c => c.StatusMaster);

            if (complaint == null)
            {
                _logger.LogWarning("Complaint {ComplaintId} not found", complaintId);
                return false;
            }

            // Skip if complaint is already resolved or closed
            if (complaint.StatusMaster.IsFinal)
            {
                return false;
            }

            // Get active escalation matrices for the company
            var matrices = await _matrixRepository.GetActiveMatricesByCompanyIdAsync(complaint.CompanyId);
            if (matrices == null || !matrices.Any())
            {
                _logger.LogDebug("No active escalation matrices found for company {CompanyId}", complaint.CompanyId);
                return false;
            }

            // Find applicable matrix (could be based on priority, category, etc.)
            var applicableMatrix = FindApplicableMatrix(complaint, matrices);
            if (applicableMatrix == null || !applicableMatrix.EnableAutoEscalation)
            {
                return false;
            }

            // Get current escalation level for this complaint
            var currentEscalationLevel = await GetCurrentEscalationLevel(complaintId);

            // Find next eligible escalation level
            var nextLevel = FindNextEscalationLevel(applicableMatrix, currentEscalationLevel, complaint);
            if (nextLevel == null)
            {
                _logger.LogDebug("No next escalation level found for complaint {ComplaintId}", complaintId);
                return false;
            }

            // Calculate hours since complaint was created or last escalated
            var hoursSinceLastAction = CalculateHoursSinceLastAction(complaint, currentEscalationLevel);

            // Check if SLA breach occurred
            if (hoursSinceLastAction >= nextLevel.TriggerAfterHours)
            {
                _logger.LogInformation(
                    "SLA breach detected for complaint {ComplaintId}. Hours: {Hours}, Trigger: {Trigger}",
                    complaintId, hoursSinceLastAction, nextLevel.TriggerAfterHours);

                // Perform auto-escalation (system user ID: empty guid represents system)
                var systemUserId = Guid.Empty;
                var reason = $"Auto-escalation triggered after {hoursSinceLastAction:F2} hours (SLA: {nextLevel.TriggerAfterHours} hours)";

                await _escalationService.EscalateComplaintAsync(
                    complaintId,
                    reason,
                    systemUserId,
                    applicableMatrix.Id,
                    nextLevel.Level,
                    isAutoEscalation: true
                );

                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking and escalating complaint {ComplaintId}", complaintId);
            throw;
        }
    }

    public async Task<List<Guid>> GetEligibleComplaintsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all complaints that are:
            // 1. Not resolved or closed
            // 2. Have active escalation matrices in their company
            // 3. Not already at maximum escalation level
            var complaints = await _complaintRepository.GetAllWithIncludesAsync(
                default,
                c => c.StatusMaster);

            var eligibleComplaints = complaints
                .Where(c => !c.StatusMaster.IsFinal && !c.IsDeleted)
                .Select(c => c.Id)
                .ToList();

            return eligibleComplaints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting eligible complaints for auto-escalation");
            throw;
        }
    }

    private Domain.Entities.Escalation.EscalationMatrix? FindApplicableMatrix(
        Domain.Entities.Complaints.Complaint complaint,
        IEnumerable<Domain.Entities.Escalation.EscalationMatrix> matrices)
    {
        // Find matrix based on priority (highest priority first)
        // In future, this could be enhanced to match by category, branch, etc.
        return matrices
            .Where(m => m.IsActive && m.EnableAutoEscalation)
            .OrderByDescending(m => m.Priority)
            .FirstOrDefault();
    }

    private async Task<int?> GetCurrentEscalationLevel(Guid complaintId)
    {
        var histories = await _historyRepository.GetByComplaintIdAsync(complaintId);
        var latestHistory = histories
            .OrderByDescending(h => h.EscalatedAt)
            .FirstOrDefault();

        return latestHistory?.Level;
    }

    private Domain.Entities.Escalation.EscalationLevel? FindNextEscalationLevel(
        Domain.Entities.Escalation.EscalationMatrix matrix,
        int? currentLevel,
        Domain.Entities.Complaints.Complaint complaint)
    {
        var levels = matrix.EscalationLevels
            .Where(l => l.IsActive)
            .OrderBy(l => l.Level)
            .ToList();

        if (currentLevel == null)
        {
            // No escalation yet, return first level
            return levels.FirstOrDefault();
        }

        // Return next level after current
        return levels.FirstOrDefault(l => l.Level > currentLevel.Value);
    }

    private double CalculateHoursSinceLastAction(Domain.Entities.Complaints.Complaint complaint, int? currentLevel)
    {
        DateTime referenceTime;

        if (currentLevel == null)
        {
            // No escalation yet, calculate from complaint creation
            referenceTime = complaint.CreatedAt;
        }
        else
        {
            // Calculate from last escalation
            var lastEscalation = _historyRepository
                .GetByComplaintIdAsync(complaint.Id)
                .Result
                .OrderByDescending(h => h.EscalatedAt)
                .FirstOrDefault();

            referenceTime = lastEscalation?.EscalatedAt ?? complaint.CreatedAt;
        }

        // Get SLA settings to check if working hours should be considered
        var slaSettings = _dbContext.SLASettings
            .Where(s => s.CompanyId == complaint.CompanyId && !s.IsDeleted)
            .FirstOrDefault();

        if (slaSettings?.WorkingHoursOnly == true)
        {
            // Use timezone-aware working hours calculation
            var company = _dbContext.Companies.Find(complaint.CompanyId);
            string timeZone = company?.DefaultTimeZone ?? "UTC";

            var workingHours = CalculateWorkingHoursElapsed(
                referenceTime,
                DateTime.UtcNow,
                slaSettings,
                timeZone);

            _logger.LogDebug(
                "Calculated working hours for complaint {ComplaintId}: {Hours} hours (from {Start} to {End} in {TimeZone})",
                complaint.Id,
                workingHours,
                referenceTime,
                DateTime.UtcNow,
                timeZone);

            return workingHours;
        }

        // Simple calendar hours calculation (backward compatible)
        var hoursSince = (DateTime.UtcNow - referenceTime).TotalHours;
        return hoursSince;
    }

    /// <summary>
    /// Calculate working hours elapsed between two UTC datetimes, considering timezone and business hours
    /// </summary>
    private double CalculateWorkingHoursElapsed(
        DateTime startUtc,
        DateTime endUtc,
        Domain.Entities.SLA.SLASettings settings,
        string timeZoneId)
    {
        // Convert to local time for business hours calculation
        var startLocal = _timeZoneService.ConvertFromUtc(startUtc, timeZoneId);
        var endLocal = _timeZoneService.ConvertFromUtc(endUtc, timeZoneId);

        var workStart = settings.WorkingHoursStart ?? new TimeSpan(9, 0, 0);
        var workEnd = settings.WorkingHoursEnd ?? new TimeSpan(17, 0, 0);
        var workingDays = ParseWorkingDays(settings.WorkingDays);

        if (workingDays.Count == 0)
        {
            workingDays = new HashSet<DayOfWeek> {
                DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                DayOfWeek.Thursday, DayOfWeek.Friday
            };
        }

        double totalMinutes = 0;
        var currentDay = startLocal.Date;

        while (currentDay <= endLocal.Date)
        {
            if (workingDays.Contains(currentDay.DayOfWeek))
            {
                var dayStart = currentDay.Add(workStart);
                var dayEnd = currentDay.Add(workEnd);

                var periodStart = currentDay == startLocal.Date
                    ? (startLocal > dayStart ? startLocal : dayStart)
                    : dayStart;

                var periodEnd = currentDay == endLocal.Date
                    ? (endLocal < dayEnd ? endLocal : dayEnd)
                    : dayEnd;

                if (periodEnd > periodStart)
                {
                    totalMinutes += (periodEnd - periodStart).TotalMinutes;
                }
            }

            currentDay = currentDay.AddDays(1);
        }

        return totalMinutes / 60.0;
    }

    /// <summary>
    /// Parse working days string (e.g., "1,2,3,4,5" for Mon-Fri)
    /// </summary>
    private HashSet<DayOfWeek> ParseWorkingDays(string workingDaysString)
    {
        var workingDays = new HashSet<DayOfWeek>();

        if (string.IsNullOrEmpty(workingDaysString))
            return workingDays;

        var days = workingDaysString.Split(',');
        foreach (var day in days)
        {
            if (int.TryParse(day.Trim(), out int dayNum))
            {
                // Convert 0-6 to DayOfWeek (0=Sunday, 1=Monday, etc.)
                if (dayNum >= 0 && dayNum <= 6)
                {
                    workingDays.Add((DayOfWeek)dayNum);
                }
            }
        }

        return workingDays;
    }
}
