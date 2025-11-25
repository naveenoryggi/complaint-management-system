using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CustomFieldDefinition
{
    public Guid Id { get; set; }

    public string FieldName { get; set; } = null!;

    public string FieldKey { get; set; } = null!;

    public string? Description { get; set; }

    public int FieldType { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsRequired { get; set; }

    public bool IsActive { get; set; }

    public bool IsSearchable { get; set; }

    public string? DefaultValue { get; set; }

    public string? ValidationRules { get; set; }

    public string? Options { get; set; }

    public string? Placeholder { get; set; }

    public string? HelpText { get; set; }

    public string EntityType { get; set; } = null!;

    public Guid? CompanyId { get; set; }

    public string? Section { get; set; }

    public string? IconClass { get; set; }

    public bool IsVisibleToComplainant { get; set; }

    public bool IsVisibleToHandler { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Company? Company { get; set; }

    public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();
}
