using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Tracks emails sent FROM the system (replies, forwards, notifications)
/// Provides complete audit trail of outbound email communications
/// </summary>
public class EmailResponseHistory : BaseEntity
{
    /// <summary>
    /// Complaint this email response is associated with
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// Email message this is replying to (null for new outbound emails)
    /// </summary>
    public Guid? EmailMessageId { get; set; }

    /// <summary>
    /// User who sent this email
    /// </summary>
    public Guid SentBy { get; set; }

    /// <summary>
    /// Recipient email addresses (comma-separated for multiple)
    /// </summary>
    public string SentTo { get; set; } = string.Empty;

    /// <summary>
    /// Carbon copy recipients (comma-separated)
    /// </summary>
    public string? CarbonCopy { get; set; }

    /// <summary>
    /// Blind carbon copy recipients (comma-separated)
    /// </summary>
    public string? BlindCarbonCopy { get; set; }

    /// <summary>
    /// Email subject line
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Email body content
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Whether the body is HTML formatted
    /// </summary>
    public bool IsHtml { get; set; } = true;

    /// <summary>
    /// Timestamp when email was sent
    /// </summary>
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Delivery status tracking
    /// Values: 'Sent', 'Delivered', 'Failed', 'Bounced', 'Pending'
    /// </summary>
    public string? DeliveryStatus { get; set; } = "Sent";

    /// <summary>
    /// Error message if delivery failed
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// SMTP Message-ID header value (for tracking)
    /// </summary>
    public string? MessageId { get; set; }

    /// <summary>
    /// If this email includes attachments (referenced via ComplaintAttachment)
    /// </summary>
    public bool HasAttachments { get; set; }

    /// <summary>
    /// Attachment IDs (comma-separated GUIDs) if any were included
    /// </summary>
    public string? AttachmentIds { get; set; }

    // Navigation properties

    /// <summary>
    /// Complaint this response belongs to
    /// </summary>
    public Complaint Complaint { get; set; } = null!;

    /// <summary>
    /// Email message being replied to (if this is a reply)
    /// </summary>
    public EmailMessage? EmailMessage { get; set; }

    /// <summary>
    /// User who sent this email
    /// </summary>
    public User SentByUser { get; set; } = null!;
}
