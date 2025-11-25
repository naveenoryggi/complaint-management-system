namespace ComplaintManagement.Application.DTOs.Assignment;

/// <summary>
/// Criteria for finding suitable resource pools and users for assignment
/// </summary>
public class AssignmentCriteria
{
    /// <summary>
    /// Organizational scope constraints
    /// </summary>
    public OrganizationalScope OrganizationalScope { get; set; } = new();

    /// <summary>
    /// Complaint-specific criteria
    /// </summary>
    public ComplaintCriteria ComplaintCriteria { get; set; } = new();

    /// <summary>
    /// Workload and availability criteria
    /// </summary>
    public WorkloadCriteria WorkloadCriteria { get; set; } = new();

    /// <summary>
    /// Skill and expertise requirements
    /// </summary>
    public SkillCriteria SkillCriteria { get; set; } = new();

    /// <summary>
    /// Temporal constraints (time-based restrictions)
    /// </summary>
    public TemporalCriteria TemporalCriteria { get; set; } = new();

    /// <summary>
    /// Maximum number of candidates to return
    /// </summary>
    public int MaxCandidates { get; set; } = 10;
}

/// <summary>
/// Organizational scope constraints
/// </summary>
public class OrganizationalScope
{
    /// <summary>
    /// Company IDs to consider (empty = all companies)
    /// </summary>
    public List<Guid> CompanyIds { get; set; } = new();

    /// <summary>
    /// Branch IDs to consider (empty = all branches)
    /// </summary>
    public List<Guid> BranchIds { get; set; } = new();

    /// <summary>
    /// Department IDs to consider (empty = all departments)
    /// </summary>
    public List<Guid> DepartmentIds { get; set; } = new();

    /// <summary>
    /// Section IDs to consider (empty = all sections)
    /// </summary>
    public List<Guid> SectionIds { get; set; } = new();
}

/// <summary>
/// Complaint-specific criteria
/// </summary>
public class ComplaintCriteria
{
    /// <summary>
    /// Category IDs the resource pool should handle
    /// </summary>
    public List<Guid> CategoryIds { get; set; } = new();

    /// <summary>
    /// Priority range the resource pool should handle
    /// </summary>
    public PriorityRange PriorityRange { get; set; } = new();

    /// <summary>
    /// Escalation level range the resource pool should handle
    /// </summary>
    public EscalationLevelRange EscalationLevelRange { get; set; } = new();
}

/// <summary>
/// Workload and availability criteria
/// </summary>
public class WorkloadCriteria
{
    /// <summary>
    /// Maximum current active complaints per user
    /// </summary>
    public int MaxCurrentWorkload { get; set; } = 10;

    /// <summary>
    /// Minimum success rate required (0.0 to 1.0)
    /// </summary>
    public decimal MinSuccessRate { get; set; } = 0.7m;

    /// <summary>
    /// Maximum average resolution time (optional)
    /// </summary>
    public TimeSpan? MaxAverageResolutionTime { get; set; }

    /// <summary>
    /// Require recent activity within these many days
    /// </summary>
    public int RequireRecentActivityDays { get; set; } = 7;

    /// <summary>
    /// Whether to consider users currently on leave
    /// </summary>
    public bool IncludeOnLeave { get; set; } = false;
}

/// <summary>
/// Skill and expertise requirements
/// </summary>
public class SkillCriteria
{
    /// <summary>
    /// Required skill codes
    /// </summary>
    public List<string> RequiredSkills { get; set; } = new();

    /// <summary>
    /// Minimum skill level required (1-5)
    /// </summary>
    public int MinSkillLevel { get; set; } = 1;

    /// <summary>
    /// Preferred skill codes (nice to have)
    /// </summary>
    public List<string> PreferredSkills { get; set; } = new();

    /// <summary>
    /// Weight for skill-based scoring (0.0 to 1.0)
    /// </summary>
    public double SkillWeight { get; set; } = 0.7;
}

/// <summary>
/// Temporal constraints
/// </summary>
public class TemporalCriteria
{
    /// <summary>
    /// Only assign during business hours
    /// </summary>
    public bool BusinessHoursOnly { get; set; } = false;

    /// <summary>
    /// Days of week when assignment is allowed (0=Sunday, 6=Saturday)
    /// </summary>
    public List<int> AllowedDaysOfWeek { get; set; } = new() { 1, 2, 3, 4, 5 };

    /// <summary>
    /// Time range when assignment is allowed
    /// </summary>
    public TimeRange? AllowedTimeRange { get; set; }

    /// <summary>
    /// Consider user's time zone for business hours
    /// </summary>
    public bool ConsiderUserTimeZone { get; set; } = true;
}

/// <summary>
/// Priority range specification
/// </summary>
public class PriorityRange
{
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 4;

    public bool Contains(int priority)
    {
        return priority >= Min && priority <= Max;
    }
}

/// <summary>
/// Escalation level range specification
/// </summary>
public class EscalationLevelRange
{
    public int Min { get; set; } = 1;
    public int Max { get; set; } = 5;

    public bool Contains(int level)
    {
        return level >= Min && level <= Max;
    }
}

/// <summary>
/// Time range specification
/// </summary>
public class TimeRange
{
    public TimeOnly Start { get; set; }
    public TimeOnly End { get; set; }

    public bool Contains(TimeOnly time)
    {
        if (Start <= End)
        {
            return time >= Start && time <= End;
        }
        else // Overnight range (e.g., 22:00 to 06:00)
        {
            return time >= Start || time <= End;
        }
    }
}