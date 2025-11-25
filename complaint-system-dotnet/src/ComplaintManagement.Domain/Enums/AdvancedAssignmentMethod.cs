namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Advanced methods for assigning complaints to resource pool members
/// </summary>
public enum AdvancedAssignmentMethod
{
    /// <summary>
    /// Manual assignment - pool members pick up complaints themselves
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Round-robin assignment - distribute evenly among available members
    /// </summary>
    RoundRobin = 1,

    /// <summary>
    /// Assign to member with fewest active complaints
    /// </summary>
    LeastBusy = 2,

    /// <summary>
    /// Assign to member with highest skill level for the complaint type
    /// </summary>
    SkillBased = 3,

    /// <summary>
    /// Assign based on priority levels and member capabilities
    /// High priority complaints go to most skilled members
    /// </summary>
    PriorityBased = 4,

    /// <summary>
    /// Advanced load balancing considering multiple factors:
    /// - Current workload
    /// - Success rate
    /// - Average resolution time
    /// - Recent activity
    /// </summary>
    WorkloadBalanced = 5,

    /// <summary>
    /// Algorithm-based best match considering:
    /// - Skills match score
    /// - Workload availability
    /// - Performance metrics
    /// - Priority level requirements
    /// </summary>
    BestFit = 6,

    /// <summary>
    /// Assign to member who has previously handled similar complaints
    /// Based on category history and success rates
    /// </summary>
    ExperienceBased = 7,

    /// <summary>
    /// Random assignment among qualified members
    /// Useful for load distribution when all other factors are equal
    /// </summary>
    Random = 8
}