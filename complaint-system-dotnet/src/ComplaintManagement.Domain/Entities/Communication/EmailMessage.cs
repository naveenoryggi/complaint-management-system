using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Represents an individual email message (incoming or outgoing)
/// Stores complete email metadata and content for ticketing system
/// </summary>
public class EmailMessage : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    // Email Headers and Metadata
    public string MessageId { get; set; } = string.Empty; // Unique email Message-ID from headers
    public string? InReplyTo { get; set; } // In-Reply-To header for threading
    public string? References { get; set; } // References header for threading
    public string Subject { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string ToEmail { get; set; } = string.Empty;
    public string? ToName { get; set; }
    public string? CcEmails { get; set; } // Comma-separated
    public string? BccEmails { get; set; } // Comma-separated

    // Email Content
    public string TextBody { get; set; } = string.Empty; // Plain text version
    public string? HtmlBody { get; set; } // HTML version
    public bool IsHtml { get; set; }

    // Direction and Status
    public EmailDirection Direction { get; set; } // Inbound or Outbound
    public EmailStatus Status { get; set; } = EmailStatus.Received;

    // Timestamps
    public DateTime ReceivedAt { get; set; } // When email was received/sent
    public DateTime ProcessedAt { get; set; } = DateTime.UtcNow; // When we processed it
    public DateTime? SentAt { get; set; } // When outbound email was sent (null for inbound)

    // Ticket/Complaint Association
    public Guid? ComplaintId { get; set; }
    public Complaint? Complaint { get; set; }

    // Agent who sent this email (for outbound messages)
    public Guid? SentByUserId { get; set; }
    public User? SentByUser { get; set; }

    // Email Threading
    public Guid? ThreadId { get; set; } // Groups related emails together
    public int ThreadPosition { get; set; } // Order within thread (0-based)

    // Attachments
    public ICollection<EmailAttachment> Attachments { get; set; } = new List<EmailAttachment>();

    // Flags
    public bool IsAutoAcknowledgement { get; set; }
    public bool IsInternal { get; set; } // Internal note visible only to agents
    public bool IsRead { get; set; }
    public bool IsSpam { get; set; }
    public bool Failed { get; set; }
    public string? FailureReason { get; set; }

    // Read Tracking (for unread email indicators)
    public Guid? ReadBy { get; set; }
    public User? ReadByUser { get; set; }
    public DateTime? ReadAt { get; set; }

    // Enhanced Recipients (JSON arrays for Reply All functionality)
    // Format: [{"emailAddress":"user@example.com","displayName":"User Name"}]
    public string? ToRecipientsJson { get; set; } // JSON array of recipients
    public string? CcRecipientsJson { get; set; } // JSON array of CC recipients
    public string? BccRecipientsJson { get; set; } // JSON array of BCC recipients

    // Raw Email Data (for debugging/audit)
    public string? RawHeaders { get; set; }
    public string? RawBody { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Email direction (incoming from customer or outgoing from agent)
/// </summary>
public enum EmailDirection
{
    Inbound = 1,  // From customer to system
    Outbound = 2  // From agent/system to customer
}

/// <summary>
/// Email processing status
/// </summary>
public enum EmailStatus
{
    Received = 1,     // Email received, not yet processed
    Processing = 2,   // Currently being processed
    Processed = 3,    // Successfully processed and ticket created/updated
    Sending = 4,      // Outbound email being sent
    Sent = 5,         // Outbound email successfully sent
    Failed = 6,       // Processing or sending failed
    Spam = 7,         // Marked as spam
    Ignored = 8       // Intentionally ignored (e.g., auto-reply, delivery report)
}
