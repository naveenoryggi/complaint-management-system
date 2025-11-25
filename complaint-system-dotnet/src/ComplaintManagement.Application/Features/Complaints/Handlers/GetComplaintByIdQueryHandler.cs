using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class GetComplaintByIdQueryHandler : IRequestHandler<GetComplaintByIdQuery, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintDto>> Handle(GetComplaintByIdQuery request, CancellationToken cancellationToken)
    {
        var complaint = await _unitOfWork.Complaints.GetComplaintWithDetailsAsync(request.Id, cancellationToken);

        if (complaint == null)
        {
            return Result<ComplaintDto>.Failure("Complaint not found", $"No complaint found with ID: {request.Id}");
        }

        var dto = new ComplaintDto
        {
            Id = complaint.Id,
            ComplaintNumber = complaint.ComplaintNumber,
            Title = complaint.Title,
            Description = complaint.Description,
            CategoryId = complaint.CategoryId,
            CategoryName = complaint.Category.Name,
            ComplainantId = complaint.ComplainantId,
            ComplainantName = complaint.Complainant.FullName,
            ComplainantEmail = complaint.Complainant.Email,
            CompanyId = complaint.CompanyId,
            CompanyName = complaint.Company.Name,
            BranchId = complaint.BranchId,
            BranchName = complaint.Branch?.Name,
            DepartmentId = complaint.DepartmentId,
            DepartmentName = complaint.Department?.Name,
            SectionId = complaint.SectionId,
            SectionName = complaint.Section?.Name,

            // Complainant organizational info
            ComplainantBranchId = complaint.Complainant.BranchId,
            ComplainantBranchName = complaint.Complainant.Branch?.Name,
            ComplainantDepartmentId = complaint.Complainant.DepartmentId,
            ComplainantDepartmentName = complaint.Complainant.Department?.Name,
            ComplainantSectionId = complaint.Complainant.SectionId,
            ComplainantSectionName = complaint.Complainant.Section?.Name,
            ComplainantEmployeeCode = complaint.Complainant.EmployeeCode,

            // Contact information
            ContactEmail = complaint.ContactEmail,
            ContactPhone = complaint.ContactPhone,
            AlternatePhone = complaint.AlternatePhone,
            PreferredContactMethod = complaint.PreferredContactMethod,

            // Manager details
            ComplainantManagerId = complaint.Complainant.ManagerId,
            ComplainantManagerName = complaint.Complainant.Manager?.FullName,
            ComplainantManagerEmail = complaint.Complainant.Manager?.Email,
            ComplainantManagerPhone = complaint.Complainant.Manager?.Phone,

            Status = complaint.StatusMaster?.Name ?? "Unknown",
            StatusId = complaint.StatusMasterId,
            Priority = complaint.PriorityMaster?.Name ?? "Unknown",
            PriorityId = complaint.PriorityMasterId,
            CurrentEscalationLevel = complaint.CurrentEscalationLevel,
            AssignedToId = complaint.AssignedToId,
            AssignedToName = complaint.AssignedTo?.FullName,
            SubmittedAt = complaint.SubmittedAt,
            DueDate = complaint.DueDate,
            ResolvedAt = complaint.ResolvedAt,
            ClosedAt = complaint.ClosedAt,
            ResolutionNotes = complaint.ResolutionNotes,
            IsAnonymous = complaint.IsAnonymous,
            Tags = complaint.Tags,
            CommentCount = complaint.Comments.Count,
            AttachmentCount = complaint.Attachments.Count
        };

        return Result<ComplaintDto>.Success(dto, "Complaint retrieved successfully");
    }
}
