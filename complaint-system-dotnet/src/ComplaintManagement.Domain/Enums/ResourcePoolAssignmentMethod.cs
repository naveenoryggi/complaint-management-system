namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the method used to assign complaints within a resource pool
/// </summary>
public enum ResourcePoolAssignmentMethod
{
    /// <summary>
    /// Pool members can manually pick up complaints
    /// </summary>
    Manual = 0,

    /// <summary>
    /// Distribute complaints evenly among pool members in rotation
    /// </summary>
    RoundRobin = 1,

    /// <summary>
    /// Assign to the pool member with the least active complaints
    /// </summary>
    LeastBusy = 2
}
