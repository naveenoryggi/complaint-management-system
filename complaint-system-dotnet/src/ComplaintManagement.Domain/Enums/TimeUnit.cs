namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Time units for escalation and SLA configuration
/// </summary>
public enum TimeUnit
{
    /// <summary>
    /// Minutes (for short-term SLAs)
    /// </summary>
    Minutes = 0,

    /// <summary>
    /// Hours (for same-day SLAs)
    /// </summary>
    Hours = 1,

    /// <summary>
    /// Days (for multi-day SLAs)
    /// </summary>
    Days = 2,

    /// <summary>
    /// Weeks (for long-term SLAs)
    /// </summary>
    Weeks = 3
}
