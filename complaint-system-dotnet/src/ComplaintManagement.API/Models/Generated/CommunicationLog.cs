using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CommunicationLog
{
    public Guid Id { get; set; }

    public int Channel { get; set; }

    public Guid? TemplateId { get; set; }

    public Guid? EventCommunicationRuleId { get; set; }

    public string? RecipientEmail { get; set; }

    public string? RecipientPhone { get; set; }

    public Guid? RecipientUserId { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;

    public int Status { get; set; }

    public DateTime? SentAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    public DateTime? ReadAt { get; set; }

    public string? ErrorMessage { get; set; }

    public int RetryCount { get; set; }

    public string? ExternalMessageId { get; set; }

    public Guid? EntityId { get; set; }

    public string? EntityType { get; set; }

    public Guid? CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company? Company { get; set; }

    public virtual User? RecipientUser { get; set; }

    public virtual CommunicationTemplate? Template { get; set; }
}
