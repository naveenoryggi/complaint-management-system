namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Represents a file attachment from an email message
/// Stores attachment metadata and file location
/// </summary>
public class EmailAttachment : BaseEntity
{
    public Guid EmailMessageId { get; set; }
    public EmailMessage EmailMessage { get; set; } = null!;

    // File Information
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty; // MIME type (e.g., "application/pdf")
    public long FileSizeBytes { get; set; }
    public string FileExtension { get; set; } = string.Empty;

    // Storage Location
    public string StoragePath { get; set; } = string.Empty; // Relative path in file system or blob storage
    public string? StorageUrl { get; set; } // Full URL if using cloud storage

    // Content Identifiers (for inline images)
    public string? ContentId { get; set; } // Content-ID header for embedded images
    public bool IsInline { get; set; } // true for embedded images, false for regular attachments

    // Security
    public bool IsScanned { get; set; } // Virus/malware scan status
    public bool IsSafe { get; set; } = true;
    public string? ScanResult { get; set; }

    // Metadata
    public string? ChecksumMd5 { get; set; } // For integrity verification
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DeletedAt { get; set; } // Soft delete
}
