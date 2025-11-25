using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for calculating SLA deadlines based on complaint properties and SLA configurations
/// </summary>
public interface ISLACalculatorService
{
    /// <summary>
    /// Calculate SLA deadline for a complaint based on its category, priority, and SLA settings
    /// </summary>
    /// <param name="categoryId">Complaint category ID</param>
    /// <param name="priorityMasterId">Priority master ID (optional)</param>
    /// <param name="companyId">Company ID for multi-tenant SLA configuration</param>
    /// <param name="startTime">Time when SLA clock starts (typically submission time)</param>
    /// <param name="timeZoneId">IANA timezone identifier for timezone-aware calculations (optional, defaults to UTC)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SLA calculation result with deadlines</returns>
    Task<SLACalculationResult> CalculateSLADeadlineAsync(
        Guid categoryId,
        Guid? priorityMasterId,
        Guid companyId,
        DateTime startTime,
        string? timeZoneId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a complaint has breached its SLA deadline
    /// </summary>
    /// <param name="dueDate">SLA due date</param>
    /// <param name="currentTime">Current time (optional, defaults to UTC now)</param>
    /// <returns>True if SLA is breached</returns>
    bool IsSLABreached(DateTime? dueDate, DateTime? currentTime = null);

    /// <summary>
    /// Calculate time remaining until SLA breach
    /// </summary>
    /// <param name="dueDate">SLA due date</param>
    /// <param name="currentTime">Current time (optional, defaults to UTC now)</param>
    /// <returns>Time remaining in minutes (negative if breached)</returns>
    double GetTimeRemainingMinutes(DateTime? dueDate, DateTime? currentTime = null);

    /// <summary>
    /// Calculate SLA percentage complete (0-100+, >100 means breached)
    /// </summary>
    /// <param name="startTime">SLA start time</param>
    /// <param name="dueDate">SLA due date</param>
    /// <param name="currentTime">Current time (optional, defaults to UTC now)</param>
    /// <returns>Percentage complete</returns>
    double GetSLAPercentageComplete(DateTime startTime, DateTime? dueDate, DateTime? currentTime = null);
}

/// <summary>
/// Result of SLA calculation
/// </summary>
public class SLACalculationResult
{
    /// <summary>
    /// Response deadline (when initial response is due)
    /// </summary>
    public DateTime? ResponseDeadline { get; set; }

    /// <summary>
    /// Resolution deadline (when resolution is due)
    /// </summary>
    public DateTime? ResolutionDeadline { get; set; }

    /// <summary>
    /// Primary deadline (typically resolution deadline, or response if resolution not set)
    /// This is what should be stored in Complaint.DueDate for backward compatibility
    /// </summary>
    public DateTime? PrimaryDeadline { get; set; }

    /// <summary>
    /// SLA Level ID that was applied (if using new SLA system)
    /// </summary>
    public Guid? SLALevelId { get; set; }

    /// <summary>
    /// SLA Level name (for logging/display)
    /// </summary>
    public string? SLALevelName { get; set; }

    /// <summary>
    /// Response time in minutes
    /// </summary>
    public int ResponseTimeMinutes { get; set; }

    /// <summary>
    /// Resolution time in minutes
    /// </summary>
    public int ResolutionTimeMinutes { get; set; }

    /// <summary>
    /// Source of the SLA configuration
    /// </summary>
    public SLASource Source { get; set; }

    /// <summary>
    /// Working hours calculation was applied
    /// </summary>
    public bool WorkingHoursApplied { get; set; }

    /// <summary>
    /// Calculation notes/debug info
    /// </summary>
    public string? Notes { get; set; }
}

/// <summary>
/// Source of SLA configuration
/// </summary>
public enum SLASource
{
    /// <summary>
    /// No SLA configured, using system default
    /// </summary>
    SystemDefault,

    /// <summary>
    /// From category's default SLA hours (legacy)
    /// </summary>
    CategoryDefault,

    /// <summary>
    /// From priority master's SLA hours (legacy)
    /// </summary>
    PriorityMaster,

    /// <summary>
    /// From Category-SLA mapping (new system)
    /// </summary>
    CategoryMapping,

    /// <summary>
    /// From Priority-SLA mapping (new system)
    /// </summary>
    PriorityMapping,

    /// <summary>
    /// From SLA Level (new system, no specific mapping)
    /// </summary>
    SLALevel
}
