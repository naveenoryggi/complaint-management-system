namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Represents different strategies for assigning escalated complaints based on organizational hierarchy
/// </summary>
public enum AssignmentStrategy
{
    /// <summary>
    /// Assign to the employee's reporting manager in organizational hierarchy
    /// </summary>
    ReportingManager = 0,

    /// <summary>
    /// Assign to section in-charge with fallback to primary/secondary/HR contacts
    /// </summary>
    SectionIncharge = 1,

    /// <summary>
    /// Assign to branch-level contacts (primary/secondary/HR)
    /// </summary>
    BranchContacts = 2,

    /// <summary>
    /// Assign to department-level contacts (primary/secondary/HR)
    /// </summary>
    DepartmentContacts = 3,

    /// <summary>
    /// Escalate to admin-level users
    /// </summary>
    AdminEscalation = 4,

    /// <summary>
    /// Assign to a resource pool using configured assignment method (Manual/RoundRobin/LeastBusy)
    /// </summary>
    ResourcePool = 5
}
