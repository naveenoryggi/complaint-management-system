using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class CannedResponse
{
    public Guid Id { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string? ShortCode { get; set; }

    public string? Subject { get; set; }

    public string Body { get; set; } = null!;

    public bool IsActive { get; set; }

    public int UsageCount { get; set; }

    public Guid CreatedBy { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual ComplaintCategory? Category { get; set; }

    public virtual Company Company { get; set; } = null!;

    public virtual User CreatedByUser { get; set; } = null!;
}
