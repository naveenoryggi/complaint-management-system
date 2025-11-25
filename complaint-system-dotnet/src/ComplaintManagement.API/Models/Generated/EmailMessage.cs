using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EmailMessage
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public string MessageId { get; set; } = null!;

    public string? InReplyTo { get; set; }

    public string? References { get; set; }

    public string Subject { get; set; } = null!;

    public string FromEmail { get; set; } = null!;

    public string FromName { get; set; } = null!;

    public string ToEmail { get; set; } = null!;

    public string? ToName { get; set; }

    public string? CcEmails { get; set; }

    public string? BccEmails { get; set; }

    public string TextBody { get; set; } = null!;

    public string? HtmlBody { get; set; }

    public bool IsHtml { get; set; }

    public int Direction { get; set; }

    public int Status { get; set; }

    public DateTime ReceivedAt { get; set; }

    public DateTime ProcessedAt { get; set; }

    public DateTime? SentAt { get; set; }

    public Guid? ComplaintId { get; set; }

    public Guid? SentByUserId { get; set; }

    public Guid? ThreadId { get; set; }

    public int ThreadPosition { get; set; }

    public bool IsAutoAcknowledgement { get; set; }

    public bool IsInternal { get; set; }

    public bool IsRead { get; set; }

    public bool IsSpam { get; set; }

    public bool Failed { get; set; }

    public string? FailureReason { get; set; }

    public string? RawHeaders { get; set; }

    public string? RawBody { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public string? BccRecipientsJson { get; set; }

    public string? CcRecipientsJson { get; set; }

    public DateTime? ReadAt { get; set; }

    public Guid? ReadBy { get; set; }

    public Guid? ReadByUserId { get; set; }

    public string? ToRecipientsJson { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual Complaint? Complaint { get; set; }

    public virtual ICollection<EmailAttachment> EmailAttachments { get; set; } = new List<EmailAttachment>();

    public virtual ICollection<EmailResponseHistory> EmailResponseHistories { get; set; } = new List<EmailResponseHistory>();

    public virtual User? ReadByUser { get; set; }

    public virtual User? SentByUser { get; set; }
}
