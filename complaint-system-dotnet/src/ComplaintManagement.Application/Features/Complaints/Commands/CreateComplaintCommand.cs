using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Domain.Enums;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class CreateComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public ComplaintPriority Priority { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? SectionId { get; set; }
    public bool IsAnonymous { get; set; }
    public string? Tags { get; set; }
    public Guid ComplainantId { get; set; } // Set from authenticated user
    public Guid CompanyId { get; set; } // Set from authenticated user

    // Contact information (optional - will be auto-populated from user if not provided)
    public string? EmployeeCode { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? AlternatePhone { get; set; }
    public PreferredContactMethod PreferredContactMethod { get; set; } = PreferredContactMethod.Both;
}
