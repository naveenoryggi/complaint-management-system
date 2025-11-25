using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CommunicationTemplate
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public int Channel { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;

    public string? HtmlBody { get; set; }

    public string? AvailablePlaceholders { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystem { get; set; }

    public Guid? CompanyId { get; set; }

    public string? Language { get; set; }

    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ICollection<CommunicationLog> CommunicationLogs { get; set; } = new List<CommunicationLog>();

    public virtual Company? Company { get; set; }

    public virtual ICollection<EmailConfiguration> EmailConfigurations { get; set; } = new List<EmailConfiguration>();

    public virtual ICollection<EventCommunicationRule> EventCommunicationRules { get; set; } = new List<EventCommunicationRule>();
}
