namespace ComplaintManagement.Application.DTOs.Email;

/// <summary>
/// Email thread item for display in complaint ticket
/// </summary>
public class EmailThreadItemDto
{
    public Guid Id { get; set; }
    public string MessageId { get; set; } = string.Empty;

    // Sender
    public string FromEmail { get; set; } = string.Empty;
    public string? FromName { get; set; }

    // Recipients
    public List<EmailRecipient> ToRecipients { get; set; } = new();
    public List<EmailRecipient> CcRecipients { get; set; } = new();

    // Content
    public string Subject { get; set; } = string.Empty;
    public string? HtmlBody { get; set; }
    public string TextBody { get; set; } = string.Empty;

    // Timestamps
    public DateTime ReceivedAt { get; set; }
    public DateTime? SentAt { get; set; }

    // Flags
    public bool IsOutbound { get; set; }
    public bool IsPrivateNote { get; set; }
    public bool IsRead { get; set; }

    // User info
    public string? SentByUserName { get; set; }
    public Guid? SentByUserId { get; set; }

    // Attachments
    public int AttachmentCount { get; set; }
}
