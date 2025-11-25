using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Tracks all participants in a complaint's email thread
/// Used for Reply All functionality and participant history
/// </summary>
public class ComplaintEmailParticipant : BaseEntity
{
    public Guid ComplaintId { get; set; }
    public Complaint Complaint { get; set; } = null!;

    public string EmailAddress { get; set; } = string.Empty;
    public string? DisplayName { get; set; }

    /// <summary>
    /// Type of participation: To, CC, BCC, From
    /// </summary>
    public string ParticipantType { get; set; } = string.Empty;

    /// <summary>
    /// User who added this participant to the thread
    /// </summary>
    public Guid? AddedBy { get; set; }
    public User? AddedByUser { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Can be removed from thread if no longer needed
    /// </summary>
    public bool IsActive { get; set; } = true;
}
