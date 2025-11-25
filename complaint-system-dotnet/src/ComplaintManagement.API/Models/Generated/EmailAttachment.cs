using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class EmailAttachment
{
    public Guid Id { get; set; }

    public Guid EmailMessageId { get; set; }

    public string FileName { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSizeBytes { get; set; }

    public string FileExtension { get; set; } = null!;

    public string StoragePath { get; set; } = null!;

    public string? StorageUrl { get; set; }

    public string? ContentId { get; set; }

    public bool IsInline { get; set; }

    public bool IsScanned { get; set; }

    public bool IsSafe { get; set; }

    public string? ScanResult { get; set; }

    public string? ChecksumMd5 { get; set; }

    public DateTime UploadedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual EmailMessage EmailMessage { get; set; } = null!;
}
