using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Dashboard;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing user dashboard preferences and statistics
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ComplaintDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<DashboardPreferencesDto>> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _context.DashboardPreferences
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (preferences == null)
            {
                // Get user's company ID to load default preferences with all active statuses
                var user = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.CompanyId })
                    .FirstOrDefaultAsync(cancellationToken);

                if (user == null)
                {
                    _logger.LogWarning("User {UserId} not found when loading default dashboard preferences", userId);
                    return Result<DashboardPreferencesDto>.Failure("User not found");
                }

                // Return default preferences with all active statuses if none exist
                var defaultPreferences = await GetDefaultPreferencesAsync(userId, user.CompanyId, cancellationToken);
                return Result<DashboardPreferencesDto>.Success(defaultPreferences);
            }

            var dto = MapToDto(preferences);
            return Result<DashboardPreferencesDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard preferences for user {UserId}", userId);
            return Result<DashboardPreferencesDto>.Failure("Failed to retrieve dashboard preferences");
        }
    }

    public async Task<Result<DashboardPreferencesDto>> SavePreferencesAsync(
        Guid userId,
        SaveDashboardPreferencesRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify that all status IDs exist
            var validStatusIds = await _context.ComplaintStatusMasters
                .Where(s => request.StatusWidgets.Contains(s.Id) && s.IsActive)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            if (validStatusIds.Count != request.StatusWidgets.Count)
            {
                return Result<DashboardPreferencesDto>.Failure("One or more status IDs are invalid");
            }

            var preferences = await _context.DashboardPreferences
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (preferences == null)
            {
                // Create new preferences
                preferences = new DashboardPreferences
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedBy = userId
                };
                _context.DashboardPreferences.Add(preferences);
            }
            else
            {
                preferences.UpdatedBy = userId;
            }

            // Update preferences
            preferences.StatusWidgets = JsonSerializer.Serialize(request.StatusWidgets);
            preferences.Layout = request.Layout;
            preferences.ShowTrends = request.ShowTrends;
            preferences.ShowPercentages = request.ShowPercentages;
            preferences.AutoRefreshInterval = request.AutoRefreshInterval;
            preferences.DateRangeDays = request.DateRangeDays;
            preferences.Theme = request.Theme;
            preferences.WidgetConfig = request.WidgetConfig;

            await _context.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(preferences);
            return Result<DashboardPreferencesDto>.Success(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving dashboard preferences for user {UserId}", userId);
            return Result<DashboardPreferencesDto>.Failure("Failed to save dashboard preferences");
        }
    }

    public async Task<Result<DashboardStatisticsDto>> GetStatisticsAsync(
        Guid userId,
        int? dateRangeDays = null,
        List<Guid>? statusIds = null,
        Guid? assignedToId = null,
        Guid? complainantId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // CRITICAL FIX: Get user's company ID for proper data isolation
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.CompanyId })
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("User {UserId} not found when loading dashboard statistics", userId);
                return Result<DashboardStatisticsDto>.Failure("User not found");
            }

            var companyId = user.CompanyId;
            _logger.LogInformation("Loading dashboard statistics for user {UserId} in company {CompanyId}", userId, companyId);

            // Get user preferences to determine date range and status widgets
            var preferences = await _context.DashboardPreferences
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            int rangeDays = dateRangeDays ?? preferences?.DateRangeDays ?? 30;
            var startDate = DateTime.UtcNow.AddDays(-rangeDays);
            var previousPeriodStart = startDate.AddDays(-rangeDays);

            // If no specific status IDs requested, use user preferences or all active statuses
            List<Guid> targetStatusIds;
            if (statusIds != null && statusIds.Any())
            {
                targetStatusIds = statusIds;
            }
            else if (preferences != null && !string.IsNullOrEmpty(preferences.StatusWidgets))
            {
                targetStatusIds = JsonSerializer.Deserialize<List<Guid>>(preferences.StatusWidgets) ?? new List<Guid>();
            }
            else
            {
                // Use all active statuses for this company (including system statuses)
                targetStatusIds = await _context.ComplaintStatusMasters
                    .Where(s => (s.CompanyId == companyId || s.CompanyId == null) && s.IsActive && !s.IsDeleted)
                    .Select(s => s.Id)
                    .ToListAsync(cancellationToken);
            }

            // CRITICAL FIX: Validate targetStatusIds to filter out invalid/fake GUIDs
            // Only use statusIds that actually exist in the database for this company (including system statuses)
            var validStatusIds = await _context.ComplaintStatusMasters
                .Where(s => (s.CompanyId == companyId || s.CompanyId == null) && targetStatusIds.Contains(s.Id) && s.IsActive && !s.IsDeleted)
                .Select(s => s.Id)
                .ToListAsync(cancellationToken);

            // If no valid status IDs found (all were fake/invalid), fall back to ALL active statuses
            if (!validStatusIds.Any())
            {
                _logger.LogWarning("No valid status IDs found in request for user {UserId}, falling back to all active statuses", userId);
                validStatusIds = await _context.ComplaintStatusMasters
                    .Where(s => (s.CompanyId == companyId || s.CompanyId == null) && s.IsActive && !s.IsDeleted)
                    .Select(s => s.Id)
                    .ToListAsync(cancellationToken);
            }
            else if (validStatusIds.Count < targetStatusIds.Count)
            {
                _logger.LogWarning("Filtered out {InvalidCount} invalid status IDs for user {UserId}",
                    targetStatusIds.Count - validStatusIds.Count, userId);
            }

            // Get status masters for this company using only VALID statusIds (including system statuses)
            var statuses = await _context.ComplaintStatusMasters
                .Where(s => (s.CompanyId == companyId || s.CompanyId == null) && validStatusIds.Contains(s.Id) && s.IsActive && !s.IsDeleted)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync(cancellationToken);

            // If no statuses found, return empty statistics instead of null
            if (!statuses.Any())
            {
                _logger.LogWarning("No active status masters found for company {CompanyId}", companyId);
                return Result<DashboardStatisticsDto>.Success(new DashboardStatisticsDto
                {
                    StatusWidgets = new List<StatusWidgetDto>(),
                    TotalComplaints = 0,
                    ActiveComplaints = 0,
                    CompletedComplaints = 0,
                    OverdueComplaints = 0,
                    TodayComplaints = 0,
                    WeekComplaints = 0,
                    MonthComplaints = 0,
                    AverageResolutionTime = null,
                    DateRangeDays = rangeDays,
                    GeneratedAt = DateTime.UtcNow
                }, "No status masters configured. Please configure status masters in admin settings.");
            }

            // SECURITY: Build base query with company ID and role-based filtering
            var baseQuery = _context.Complaints
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .AsQueryable();

            if (assignedToId.HasValue)
            {
                // Handler: Filter by assigned complaints
                baseQuery = baseQuery.Where(c => c.AssignedToId == assignedToId.Value);
                _logger.LogInformation("Filtering statistics for handler {HandlerId}", assignedToId.Value);
            }
            else if (complainantId.HasValue)
            {
                // Complainant: Filter by own complaints
                baseQuery = baseQuery.Where(c => c.ComplainantId == complainantId.Value);
                _logger.LogInformation("Filtering statistics for complainant {ComplainantId}", complainantId.Value);
            }
            else
            {
                // Admin: No filtering (all complaints)
                _logger.LogInformation("Loading statistics for admin user {UserId} - no filtering", userId);
            }

            // Get complaints for current period with role-based filtering
            var currentComplaints = await baseQuery
                .Where(c => c.CreatedAt >= startDate)
                .ToListAsync(cancellationToken);

            // Get complaints for previous period with role-based filtering
            var previousComplaints = await baseQuery
                .Where(c => c.CreatedAt >= previousPeriodStart && c.CreatedAt < startDate)
                .ToListAsync(cancellationToken);

            // Build status widgets
            var widgets = new List<StatusWidgetDto>();
            foreach (var status in statuses)
            {
                var currentCount = currentComplaints.Count(c => c.StatusMasterId == status.Id);
                var previousCount = previousComplaints.Count(c => c.StatusMasterId == status.Id);

                var percentageChange = previousCount > 0
                    ? ((decimal)(currentCount - previousCount) / previousCount) * 100
                    : currentCount > 0 ? 100 : 0;

                var trend = percentageChange > 5 ? "up"
                    : percentageChange < -5 ? "down"
                    : "stable";

                // Calculate average time in status with role-based filtering
                var complaintsInStatus = await baseQuery
                    .Where(c => c.StatusMasterId == status.Id && c.CreatedAt >= startDate)
                    .ToListAsync(cancellationToken);

                double? avgTime = null;
                if (complaintsInStatus.Any())
                {
                    var times = complaintsInStatus
                        .Where(c => c.UpdatedAt.HasValue)
                        .Select(c => (c.UpdatedAt!.Value - c.CreatedAt).TotalHours)
                        .ToList();

                    if (times.Any())
                    {
                        avgTime = times.Average();
                    }
                }

                widgets.Add(new StatusWidgetDto
                {
                    StatusId = status.Id,
                    Code = status.Code,
                    Name = status.Name,
                    Description = status.Description,
                    DisplayOrder = status.DisplayOrder,
                    ColorCode = status.ColorCode,
                    IconClass = status.IconClass,
                    CurrentCount = currentCount,
                    PreviousCount = previousCount,
                    PercentageChange = Math.Round(percentageChange, 2),
                    Trend = trend,
                    IsFinal = status.IsFinal,
                    AverageTimeInStatus = avgTime.HasValue ? Math.Round(avgTime.Value, 2) : null
                });
            }

            // Calculate overall statistics with role-based filtering
            var now = DateTime.UtcNow;
            var todayStart = now.Date;
            var weekStart = now.AddDays(-7);
            var monthStart = now.AddMonths(-1);

            // SECURITY: Use role-filtered base query instead of all complaints
            var allComplaints = await baseQuery.ToListAsync(cancellationToken);
            var finalStatusIds = statuses.Where(s => s.IsFinal).Select(s => s.Id).ToList();

            var statistics = new DashboardStatisticsDto
            {
                StatusWidgets = widgets,
                TotalComplaints = allComplaints.Count,
                ActiveComplaints = allComplaints.Count(c => !finalStatusIds.Contains(c.StatusMasterId)),
                CompletedComplaints = allComplaints.Count(c => finalStatusIds.Contains(c.StatusMasterId)),
                OverdueComplaints = 0, // TODO: Implement SLA calculation
                TodayComplaints = allComplaints.Count(c => c.CreatedAt >= todayStart),
                WeekComplaints = allComplaints.Count(c => c.CreatedAt >= weekStart),
                MonthComplaints = allComplaints.Count(c => c.CreatedAt >= monthStart),
                AverageResolutionTime = null, // TODO: Implement resolution time calculation
                DateRangeDays = rangeDays,
                GeneratedAt = DateTime.UtcNow
            };

            return Result<DashboardStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard statistics for user {UserId}", userId);
            return Result<DashboardStatisticsDto>.Failure("Failed to retrieve dashboard statistics");
        }
    }

    public async Task<Result> ResetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var preferences = await _context.DashboardPreferences
                .Where(p => p.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);

            if (preferences != null)
            {
                _context.DashboardPreferences.Remove(preferences);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting dashboard preferences for user {UserId}", userId);
            return Result.Failure("Failed to reset dashboard preferences");
        }
    }

    #region Private Helper Methods

    private DashboardPreferencesDto MapToDto(DashboardPreferences preferences)
    {
        List<Guid> statusWidgets;
        try
        {
            statusWidgets = JsonSerializer.Deserialize<List<Guid>>(preferences.StatusWidgets) ?? new List<Guid>();
        }
        catch
        {
            statusWidgets = new List<Guid>();
        }

        return new DashboardPreferencesDto
        {
            Id = preferences.Id,
            UserId = preferences.UserId,
            StatusWidgets = statusWidgets,
            Layout = preferences.Layout,
            ShowTrends = preferences.ShowTrends,
            ShowPercentages = preferences.ShowPercentages,
            AutoRefreshInterval = preferences.AutoRefreshInterval,
            DateRangeDays = preferences.DateRangeDays,
            Theme = preferences.Theme,
            WidgetConfig = preferences.WidgetConfig,
            CreatedAt = preferences.CreatedAt,
            UpdatedAt = preferences.UpdatedAt
        };
    }

    private async Task<DashboardPreferencesDto> GetDefaultPreferencesAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // Load all active statuses for the company as default widgets
        // Include both company-specific statuses AND system statuses (CompanyId = NULL)
        var allActiveStatuses = await _context.ComplaintStatusMasters
            .Where(s => (s.CompanyId == companyId || s.CompanyId == null) && s.IsActive && !s.IsDeleted)
            .OrderBy(s => s.DisplayOrder)
            .Select(s => s.Id)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Creating default preferences for user {UserId} with {StatusCount} active statuses",
            userId,
            allActiveStatuses.Count);

        return new DashboardPreferencesDto
        {
            Id = Guid.Empty,
            UserId = userId,
            StatusWidgets = allActiveStatuses,  // ALL active statuses instead of empty list
            Layout = "grid-4",
            ShowTrends = true,
            ShowPercentages = true,
            AutoRefreshInterval = 0,
            DateRangeDays = 30,
            Theme = null,
            WidgetConfig = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };
    }

    #endregion
}
