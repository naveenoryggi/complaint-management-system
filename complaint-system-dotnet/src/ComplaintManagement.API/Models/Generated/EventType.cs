using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EventType
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string? Description { get; set; }

    public string EntityType { get; set; } = null!;

    public string? Category { get; set; }

    public bool IsActive { get; set; }

    public bool IsSystem { get; set; }

    public string? AvailableFields { get; set; }

    public string? IconClass { get; set; }

    public Guid? CompanyId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<EventCommunicationRule> EventCommunicationRules { get; set; } = new List<EventCommunicationRule>();
}
