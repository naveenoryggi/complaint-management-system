using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EventCommunicationRule
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public Guid EventTypeId { get; set; }

    public int Channel { get; set; }

    public Guid? TemplateId { get; set; }

    public int RecipientType { get; set; }

    public string? SpecificUserIds { get; set; }

    public string? SpecificRoleIds { get; set; }

    public string? SpecificEmails { get; set; }

    public string? Conditions { get; set; }

    public bool IsActive { get; set; }

    public int Priority { get; set; }

    public int DelayMinutes { get; set; }

    public bool SendOnlyOnce { get; set; }

    public Guid? CompanyId { get; set; }

    public string? AdditionalData { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company? Company { get; set; }

    public virtual EventType EventType { get; set; } = null!;

    public virtual CommunicationTemplate? Template { get; set; }
}
