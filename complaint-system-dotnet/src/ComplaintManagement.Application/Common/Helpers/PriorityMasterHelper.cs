using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.Common.Helpers;

/// <summary>
/// Helper class to map ComplaintPriority enum to PriorityMasterId GUIDs
/// This ensures SLA calculations and priority-based routing work correctly
/// </summary>
public static class PriorityMasterHelper
{
    // System priority IDs - these are seeded in the database
    private static readonly Guid LowPriorityId = new Guid("20000000-0000-0000-0000-000000000001");
    private static readonly Guid NormalPriorityId = new Guid("20000000-0000-0000-0000-000000000002");
    private static readonly Guid HighPriorityId = new Guid("20000000-0000-0000-0000-000000000003");
    private static readonly Guid CriticalPriorityId = new Guid("20000000-0000-0000-0000-000000000004");
    private static readonly Guid UrgentPriorityId = new Guid("20000000-0000-0000-0000-000000000005");

    /// <summary>
    /// Maps ComplaintPriority enum to PriorityMasterId
    /// </summary>
    public static Guid? GetPriorityMasterId(ComplaintPriority priority)
    {
        return priority switch
        {
            ComplaintPriority.Low => LowPriorityId,
            ComplaintPriority.Normal => NormalPriorityId,
            ComplaintPriority.High => HighPriorityId,
            ComplaintPriority.Critical => CriticalPriorityId,
            ComplaintPriority.Urgent => UrgentPriorityId,
            _ => null
        };
    }

    /// <summary>
    /// Maps PriorityMasterId to ComplaintPriority enum
    /// </summary>
    public static ComplaintPriority? GetComplaintPriority(Guid? priorityMasterId)
    {
        if (!priorityMasterId.HasValue)
            return null;

        if (priorityMasterId == LowPriorityId) return ComplaintPriority.Low;
        if (priorityMasterId == NormalPriorityId) return ComplaintPriority.Normal;
        if (priorityMasterId == HighPriorityId) return ComplaintPriority.High;
        if (priorityMasterId == CriticalPriorityId) return ComplaintPriority.Critical;
        if (priorityMasterId == UrgentPriorityId) return ComplaintPriority.Urgent;

        return null;
    }
}
