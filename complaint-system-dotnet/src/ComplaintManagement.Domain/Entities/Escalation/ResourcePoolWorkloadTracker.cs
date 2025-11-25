using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Escalation;

/// <summary>
/// Tracks workload and performance metrics for resource pool members
/// Used for intelligent assignment decisions and performance monitoring
/// </summary>
public class ResourcePoolWorkloadTracker
{
    public Guid Id { get; set; }

    /// <summary>
    /// The resource pool being tracked
    /// </summary>
    public Guid ResourcePoolId { get; set; }

    /// <summary>
    /// The user whose workload is being tracked
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Current number of active complaints assigned to this user
    /// </summary>
    public int CurrentActiveComplaints { get; set; } = 0;

    /// <summary>
    /// Total number of complaints ever assigned to this user
    /// </summary>
    public int TotalAssigned { get; set; } = 0;

    /// <summary>
    /// Total number of complaints successfully resolved by this user
    /// </summary>
    public int TotalResolved { get; set; } = 0;

    /// <summary>
    /// Total number of complaints escalated away from this user
    /// </summary>
    public int TotalEscalated { get; set; } = 0;

    /// <summary>
    /// Average resolution time for resolved complaints
    /// </summary>
    public TimeSpan AverageResolutionTime { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Success rate (resolved / assigned)
    /// Range: 0.0 to 1.0
    /// </summary>
    public decimal SuccessRate { get; set; } = 1.0m;

    /// <summary>
    /// Average satisfaction rating (if feedback is collected)
    /// Range: 1.0 to 5.0
    /// </summary>
    public decimal AverageRating { get; set; } = 5.0m;

    /// <summary>
    /// When this user was last assigned a complaint
    /// </summary>
    public DateTime? LastAssignedAt { get; set; }

    /// <summary>
    /// When this user last had activity on any complaint
    /// </summary>
    public DateTime? LastActivityAt { get; set; }

    /// <summary>
    /// Total time spent on all resolved complaints
    /// </summary>
    public TimeSpan TotalResolutionTime { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// When this workload tracking started
    /// </summary>
    public DateTime TrackingStartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// When this record was last updated
    /// </summary>
    public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ResourcePool ResourcePool { get; set; } = null!;
    public User User { get; set; } = null!;

    /// <summary>
    /// Calculate the workload score (lower is better for assignment)
    /// Considers current active complaints and success rate
    /// </summary>
    public double GetWorkloadScore()
    {
        double baseScore = CurrentActiveComplaints;

        // Penalize users with low success rates
        double successRatePenalty = (1.0 - (double)SuccessRate) * 5.0;

        // Prefer users with recent activity (available/engaged)
        double recencyBonus = 0;
        if (LastActivityAt.HasValue)
        {
            var daysSinceActivity = (DateTime.UtcNow - LastActivityAt.Value).TotalDays;
            if (daysSinceActivity <= 1) recencyBonus = -0.5; // Very recent activity bonus
            else if (daysSinceActivity <= 7) recencyBonus = -0.2; // Recent activity bonus
            else if (daysSinceActivity > 30) recencyBonus = 1.0; // Inactivity penalty
        }

        return baseScore + successRatePenalty + recencyBonus;
    }

    /// <summary>
    /// Check if this user is available for new assignments
    /// Based on current workload and recent activity
    /// </summary>
    public bool IsAvailableForAssignment()
    {
        // User is unavailable if they have too many active complaints
        if (CurrentActiveComplaints >= 10) return false;

        // User is unavailable if they haven't been active recently (inactive for 7 days)
        if (LastActivityAt.HasValue &&
            (DateTime.UtcNow - LastActivityAt.Value).TotalDays > 7) return false;

        return true;
    }

    /// <summary>
    /// Update the success rate based on new assignment/completion data
    /// </summary>
    public void UpdateSuccessRate()
    {
        if (TotalAssigned > 0)
        {
            SuccessRate = (decimal)TotalResolved / TotalAssigned;
        }
    }

    /// <summary>
    /// Update the average resolution time based on new completion
    /// </summary>
    public void UpdateAverageResolutionTime(TimeSpan newResolutionTime)
    {
        if (TotalResolved == 1)
        {
            AverageResolutionTime = newResolutionTime;
            TotalResolutionTime = newResolutionTime;
        }
        else
        {
            TotalResolutionTime = TotalResolutionTime.Add(newResolutionTime);
            AverageResolutionTime = TimeSpan.FromTicks(TotalResolutionTime.Ticks / TotalResolved);
        }
    }

    /// <summary>
    /// Increment the active complaints count
    /// </summary>
    public void IncrementActiveComplaints()
    {
        CurrentActiveComplaints++;
        TotalAssigned++;
        LastAssignedAt = DateTime.UtcNow;
        LastActivityAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrement the active complaints count and mark as resolved
    /// </summary>
    public void DecrementActiveComplaints(TimeSpan resolutionTime)
    {
        CurrentActiveComplaints = Math.Max(0, CurrentActiveComplaints - 1);
        TotalResolved++;
        UpdateAverageResolutionTime(resolutionTime);
        UpdateSuccessRate();
        LastActivityAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Handle complaint escalation (decrement active but don't count as resolved)
    /// </summary>
    public void HandleEscalation()
    {
        CurrentActiveComplaints = Math.Max(0, CurrentActiveComplaints - 1);
        TotalEscalated++;
        UpdateSuccessRate();
        LastActivityAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }
}