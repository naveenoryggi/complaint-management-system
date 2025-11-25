using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintAttachment
{
    public Guid Id { get; set; }

    public Guid ComplaintId { get; set; }

    public string FileName { get; set; } = null!;

    public string StoredFileName { get; set; } = null!;

    public string FilePath { get; set; } = null!;

    public long FileSize { get; set; }

    public string ContentType { get; set; } = null!;

    public string FileExtension { get; set; } = null!;

    public Guid UploadedBy { get; set; }

    public DateTime UploadedAt { get; set; }

    public string? Description { get; set; }

    public bool IsThumbnail { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual Complaint Complaint { get; set; } = null!;

    public virtual User UploadedByNavigation { get; set; } = null!;
}
