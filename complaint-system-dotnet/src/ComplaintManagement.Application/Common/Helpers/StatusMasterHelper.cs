using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.Common.Helpers;

/// <summary>
/// Helper class to map ComplaintStatus enum to StatusMasterId GUIDs
/// This ensures dashboard and reports always show correct counts
/// </summary>
public static class StatusMasterHelper
{
    // System status IDs - these are seeded in the database
    private static readonly Guid SubmittedStatusId = new Guid("10000000-0000-0000-0000-000000000001");
    private static readonly Guid UnderReviewStatusId = new Guid("10000000-0000-0000-0000-000000000002");
    private static readonly Guid InProgressStatusId = new Guid("10000000-0000-0000-0000-000000000003");
    private static readonly Guid EscalatedStatusId = new Guid("10000000-0000-0000-0000-000000000004");
    private static readonly Guid PendingInfoStatusId = new Guid("10000000-0000-0000-0000-000000000005");
    private static readonly Guid ResolvedStatusId = new Guid("10000000-0000-0000-0000-000000000006");
    private static readonly Guid ClosedStatusId = new Guid("10000000-0000-0000-0000-000000000007");
    private static readonly Guid RejectedStatusId = new Guid("10000000-0000-0000-0000-000000000008");
    private static readonly Guid ReopenedStatusId = new Guid("10000000-0000-0000-0000-000000000009");

    /// <summary>
    /// Maps ComplaintStatus enum to StatusMasterId
    /// </summary>
    public static Guid? GetStatusMasterId(ComplaintStatus status)
    {
        return status switch
        {
            ComplaintStatus.Submitted => SubmittedStatusId,
            ComplaintStatus.UnderReview => UnderReviewStatusId,
            ComplaintStatus.InProgress => InProgressStatusId,
            ComplaintStatus.Escalated => EscalatedStatusId,
            ComplaintStatus.PendingInfo => PendingInfoStatusId,
            ComplaintStatus.Resolved => ResolvedStatusId,
            ComplaintStatus.Closed => ClosedStatusId,
            ComplaintStatus.Rejected => RejectedStatusId,
            ComplaintStatus.Reopened => ReopenedStatusId,
            _ => null
        };
    }

    /// <summary>
    /// Maps StatusMasterId to ComplaintStatus enum
    /// </summary>
    public static ComplaintStatus? GetComplaintStatus(Guid? statusMasterId)
    {
        if (!statusMasterId.HasValue)
            return null;

        if (statusMasterId == SubmittedStatusId) return ComplaintStatus.Submitted;
        if (statusMasterId == UnderReviewStatusId) return ComplaintStatus.UnderReview;
        if (statusMasterId == InProgressStatusId) return ComplaintStatus.InProgress;
        if (statusMasterId == EscalatedStatusId) return ComplaintStatus.Escalated;
        if (statusMasterId == PendingInfoStatusId) return ComplaintStatus.PendingInfo;
        if (statusMasterId == ResolvedStatusId) return ComplaintStatus.Resolved;
        if (statusMasterId == ClosedStatusId) return ComplaintStatus.Closed;
        if (statusMasterId == RejectedStatusId) return ComplaintStatus.Rejected;
        if (statusMasterId == ReopenedStatusId) return ComplaintStatus.Reopened;

        return null;
    }
}
