using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Complaints;

public class ComplaintDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid ComplainantId { get; set; }
    public string ComplainantName { get; set; } = string.Empty;
    public string ComplainantEmail { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? SectionId { get; set; }
    public string? SectionName { get; set; }
    public Guid? ComplainantBranchId { get; set; }
    public string? ComplainantBranchName { get; set; }
    public Guid? ComplainantDepartmentId { get; set; }
    public string? ComplainantDepartmentName { get; set; }
    public Guid? ComplainantSectionId { get; set; }
    public string? ComplainantSectionName { get; set; }
    public string? ComplainantEmployeeCode { get; set; }

    // Contact information
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; }

    // Manager details (for handler view)
    public Guid? ComplainantManagerId { get; set; }
    public string? ComplainantManagerName { get; set; }
    public string? ComplainantManagerEmail { get; set; }
    public string? ComplainantManagerPhone { get; set; }

    public string Status { get; set; } = string.Empty;
    public Guid StatusId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public Guid PriorityId { get; set; }
    public int CurrentEscalationLevel { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? AssignedToName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ResolutionNotes { get; set; }
    public bool IsAnonymous { get; set; }
    public string? Tags { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }

    // Email response tracking
    public bool HasCustomerResponse { get; set; }
    public string? LastResponseFrom { get; set; }
    public DateTime? LastResponseAt { get; set; }
}
