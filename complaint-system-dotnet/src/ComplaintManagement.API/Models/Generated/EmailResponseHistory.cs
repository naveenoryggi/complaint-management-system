using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EmailResponseHistory
{
    public Guid Id { get; set; }

    public Guid ComplaintId { get; set; }

    public Guid? EmailMessageId { get; set; }

    public Guid SentBy { get; set; }

    public string SentTo { get; set; } = null!;

    public string? CarbonCopy { get; set; }

    public string? BlindCarbonCopy { get; set; }

    public string Subject { get; set; } = null!;

    public string Body { get; set; } = null!;

    public bool IsHtml { get; set; }

    public DateTime SentAt { get; set; }

    public string? DeliveryStatus { get; set; }

    public string? ErrorMessage { get; set; }

    public string? MessageId { get; set; }

    public bool HasAttachments { get; set; }

    public string? AttachmentIds { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Complaint Complaint { get; set; } = null!;

    public virtual EmailMessage? EmailMessage { get; set; }

    public virtual User SentByNavigation { get; set; } = null!;
}
