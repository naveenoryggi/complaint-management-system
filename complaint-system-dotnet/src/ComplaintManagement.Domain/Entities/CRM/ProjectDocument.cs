using ComplaintManagement.Domain.Enums.CRM;

namespace ComplaintManagement.Domain.Entities.CRM;

/// <summary>
/// Represents a document attached to a project
/// </summary>
public class ProjectDocument : BaseEntity
{
    #region Identity

    /// <summary>
    /// Project ID (foreign key)
    /// </summary>
    public Guid ProjectId { get; set; }

    /// <summary>
    /// Original file name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Stored file path
    /// </summary>
    public string FilePath { get; set; } = string.Empty;

    #endregion

    #region File Information

    /// <summary>
    /// File MIME type (e.g., application/pdf)
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long FileSize { get; set; }

    #endregion

    #region Categorization

    /// <summary>
    /// Document category
    /// </summary>
    public DocumentCategory Category { get; set; } = DocumentCategory.Other;

    /// <summary>
    /// Document description
    /// </summary>
    public string? Description { get; set; }

    #endregion

    #region Upload Information

    /// <summary>
    /// User who uploaded the document
    /// </summary>
    public Guid UploadedById { get; set; }

    /// <summary>
    /// Upload timestamp
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    #endregion

    #region Navigation Properties

    /// <summary>
    /// Parent project
    /// </summary>
    public Project Project { get; set; } = null!;

    #endregion
}
