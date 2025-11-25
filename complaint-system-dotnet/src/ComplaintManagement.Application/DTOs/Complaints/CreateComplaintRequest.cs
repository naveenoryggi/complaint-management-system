using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Complaints;

public class CreateComplaintRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ComplaintPriority Priority { get; set; }

    // Organizational hierarchy (will be auto-populated if user is logged in)
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }

    // Contact information (will be auto-populated from user profile)
    public string? EmployeeCode { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; } = PreferredContactMethod.Both;

    public bool IsAnonymous { get; set; }
    public string? Tags { get; set; }
}
