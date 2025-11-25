using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Logs all communications sent from the system
/// </summary>
public class CommunicationLog : BaseEntity
{
    /// <summary>
    /// Communication channel used
    /// </summary>
    public CommunicationChannel Channel { get; set; }

    /// <summary>
    /// Template ID used (if any)
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Event communication rule that triggered this (if any)
    /// </summary>
    public Guid? EventCommunicationRuleId { get; set; }

    /// <summary>
    /// Recipient email address
    /// </summary>
    public string? RecipientEmail { get; set; }

    /// <summary>
    /// Recipient phone number (for SMS/WhatsApp)
    /// </summary>
    public string? RecipientPhone { get; set; }

    /// <summary>
    /// Recipient user ID (if applicable)
    /// </summary>
    public Guid? RecipientUserId { get; set; }

    /// <summary>
    /// Subject (for Email)
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Message body/content sent
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Status of the communication
    /// </summary>
    public CommunicationStatus Status { get; set; } = CommunicationStatus.Pending;

    /// <summary>
    /// When the communication was sent
    /// </summary>
    public DateTime? SentAt { get; set; }

    /// <summary>
    /// When the communication was delivered (if known)
    /// </summary>
    public DateTime? DeliveredAt { get; set; }

    /// <summary>
    /// When the communication was read/opened (if tracked)
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    /// Error message if sending failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 0;

    /// <summary>
    /// External message ID (from email/SMS provider)
    /// </summary>
    public string? ExternalMessageId { get; set; }

    /// <summary>
    /// Related entity ID (e.g., ComplaintId)
    /// </summary>
    public Guid? EntityId { get; set; }

    /// <summary>
    /// Related entity type (e.g., "Complaint")
    /// </summary>
    public string? EntityType { get; set; }

    /// <summary>
    /// Company ID
    /// </summary>
    public Guid? CompanyId { get; set; }

    // Navigation properties
    /// <summary>
    /// Template used
    /// </summary>
    public CommunicationTemplate? Template { get; set; }

    /// <summary>
    /// Recipient user
    /// </summary>
    public User? RecipientUser { get; set; }

    /// <summary>
    /// Company
    /// </summary>
    public Company? Company { get; set; }
}
