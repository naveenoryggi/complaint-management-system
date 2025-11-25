using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Complaints;

/// <summary>
/// Represents a comment/note on a complaint
/// </summary>
public class ComplaintComment : BaseEntity
{
    /// <summary>
    /// Complaint ID (foreign key)
    /// </summary>
    public Guid ComplaintId { get; set; }

    /// <summary>
    /// User ID who wrote the comment (foreign key)
    /// </summary>
    public Guid CommentedBy { get; set; }

    /// <summary>
    /// Comment text
    /// </summary>
    public string CommentText { get; set; } = string.Empty;

    /// <summary>
    /// Is internal note (not visible to complainant)
    /// </summary>
    public bool IsInternal { get; set; } = false;

    /// <summary>
    /// Parent comment ID (for threaded comments/replies)
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Timestamp when comment was posted
    /// </summary>
    public DateTime CommentedAt { get; set; }

    /// <summary>
    /// Is comment edited
    /// </summary>
    public bool IsEdited { get; set; } = false;

    /// <summary>
    /// Last edit timestamp
    /// </summary>
    public DateTime? EditedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Parent complaint
    /// </summary>
    public Complaint Complaint { get; set; } = null!;

    /// <summary>
    /// User who wrote the comment
    /// </summary>
    public User Commenter { get; set; } = null!;

    /// <summary>
    /// Parent comment (if this is a reply)
    /// </summary>
    public ComplaintComment? ParentComment { get; set; }

    /// <summary>
    /// Replies to this comment
    /// </summary>
    public ICollection<ComplaintComment> Replies { get; set; } = new List<ComplaintComment>();
}
