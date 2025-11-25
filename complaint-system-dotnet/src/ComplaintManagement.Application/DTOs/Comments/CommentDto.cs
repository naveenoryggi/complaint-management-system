namespace ComplaintManagement.Application.DTOs.Comments;

public class CommentDto
{
    public Guid Id { get; set; }
    public Guid ComplaintId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string? UserPhone { get; set; }
    public string? BranchName { get; set; }
    public string? DepartmentName { get; set; }
    public string? SectionName { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public DateTime CreatedAt { get; set; }
}
