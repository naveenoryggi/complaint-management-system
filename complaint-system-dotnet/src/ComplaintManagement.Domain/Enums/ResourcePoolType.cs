namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents the type of resource pool based on organizational structure
/// </summary>
public enum ResourcePoolType
{
    /// <summary>
    /// Pool organized by branch
    /// </summary>
    Branch = 0,

    /// <summary>
    /// Pool organized by department
    /// </summary>
    Department = 1,

    /// <summary>
    /// Pool organized by section
    /// </summary>
    Section = 2,

    /// <summary>
    /// Custom pool not tied to organizational structure
    /// </summary>
    Custom = 3
}
