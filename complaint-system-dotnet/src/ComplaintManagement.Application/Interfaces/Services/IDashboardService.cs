using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Dashboard;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for managing user dashboard preferences and statistics
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Gets dashboard preferences for the current user
    /// </summary>
    Task<Result<DashboardPreferencesDto>> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Saves or updates dashboard preferences for the current user
    /// </summary>
    Task<Result<DashboardPreferencesDto>> SavePreferencesAsync(
        Guid userId,
        SaveDashboardPreferencesRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets dashboard statistics with status widgets
    /// </summary>
    Task<Result<DashboardStatisticsDto>> GetStatisticsAsync(
        Guid userId,
        int? dateRangeDays = null,
        List<Guid>? statusIds = null,
        Guid? assignedToId = null,
        Guid? complainantId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets dashboard preferences to default for a user
    /// </summary>
    Task<Result> ResetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
