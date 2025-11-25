using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Complaints;

/// <summary>
/// Represents a file attachment to a complaint
/// </summary>
public class ComplaintAttachment : BaseEntity
{
    /// <summary>
    /// Complaint ID (foreign key)
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// Original filename
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Stored filename (on disk/cloud)
    /// </summary>
    public string StoredFileName { get; set; } = string.Empty;

    /// <summary>
    /// File path/URL
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// File extension
    /// </summary>
    public string FileExtension { get; set; } = string.Empty;

    /// <summary>
    /// User ID who uploaded the file
    /// </summary>
    public Guid UploadedBy { get; set; }

    /// <summary>
    /// Upload timestamp
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// File description/notes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Is file a thumbnail/preview
    /// </summary>
    public bool IsThumbnail { get; set; } = false;

    // Navigation properties
    /// <summary>
    /// Parent complaint
    /// </summary>
    public Complaint Complaint { get; set; } = null!;

    /// <summary>
    /// User who uploaded the file
    /// </summary>
    public User Uploader { get; set; } = null!;
}
