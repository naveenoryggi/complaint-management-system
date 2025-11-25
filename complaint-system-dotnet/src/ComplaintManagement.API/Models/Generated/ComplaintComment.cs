using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintComment
{
    public Guid Id { get; set; }

    public Guid ComplaintId { get; set; }

    public Guid CommentedBy { get; set; }

    public string CommentText { get; set; } = null!;

    public bool IsInternal { get; set; }

    public Guid? ParentCommentId { get; set; }

    public DateTime CommentedAt { get; set; }

    public bool IsEdited { get; set; }

    public DateTime? EditedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual User CommentedByNavigation { get; set; } = null!;

    public virtual Complaint Complaint { get; set; } = null!;

    public virtual ICollection<ComplaintComment> InverseParentComment { get; set; } = new List<ComplaintComment>();

    public virtual ComplaintComment? ParentComment { get; set; }
}
