namespace ComplaintManagement.Application.DTOs.Email;

/// <summary>
/// Request to send email reply from complaint ticket
/// </summary>
public class SendEmailReplyRequest
{
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// Original email message being replied to (NULL for new thread)
    /// </summary>
    public Guid? InReplyToEmailMessageId { get; set; }

    /// <summary>
    /// Type of reply (Reply, ReplyAll, Forward, NewEmail, PrivateNote)
    /// </summary>
    public ReplyType ReplyType { get; set; }

    // Recipients
    public List<EmailRecipient> ToRecipients { get; set; } = new();
    public List<EmailRecipient> CcRecipients { get; set; } = new();
    public List<EmailRecipient> BccRecipients { get; set; } = new();

    // Content
    public string Subject { get; set; } = string.Empty;
    public string HtmlBody { get; set; } = string.Empty;
    public string? PlainTextBody { get; set; }

    /// <summary>
    /// If true, message is internal note only (not sent via email)
    /// </summary>
    public bool IsPrivateNote { get; set; } = false;

    /// <summary>
    /// Existing attachment IDs to forward
    /// </summary>
    public List<Guid> ExistingAttachmentIds { get; set; } = new();
}
