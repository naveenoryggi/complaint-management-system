using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CustomFieldValue
{
    public Guid Id { get; set; }

    public Guid CustomFieldDefinitionId { get; set; }

    public Guid EntityId { get; set; }

    public string EntityType { get; set; } = null!;

    public string? Value { get; set; }

    public decimal? NumericValue { get; set; }

    public DateTime? DateValue { get; set; }

    public bool? BooleanValue { get; set; }

    public string? JsonValue { get; set; }

    public Guid? ComplaintId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Complaint? Complaint { get; set; }

    public virtual CustomFieldDefinition CustomFieldDefinition { get; set; } = null!;
}
