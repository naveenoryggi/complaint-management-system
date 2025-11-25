using AutoMapper;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Mappings;

public class ComplaintMappingProfile : Profile
{
    public ComplaintMappingProfile()
    {
        CreateMap<Complaint, ComplaintDto>()
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
            .ForMember(dest => dest.ComplainantName, opt => opt.MapFrom(src => src.Complainant != null ? src.Complainant.FullName : string.Empty))
            .ForMember(dest => dest.ComplainantEmail, opt => opt.MapFrom(src => src.Complainant != null ? src.Complainant.Email : string.Empty))
            .ForMember(dest => dest.AssignedToName, opt => opt.MapFrom(src => src.AssignedTo != null ? src.AssignedTo.FullName : string.Empty))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
            .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.StatusMaster != null ? src.StatusMaster.Name : "Unknown"))
            .ForMember(dest => dest.StatusId, opt => opt.MapFrom(src => src.StatusMasterId))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.PriorityMaster != null ? src.PriorityMaster.Name : "Unknown"))
            .ForMember(dest => dest.PriorityId, opt => opt.MapFrom(src => src.PriorityMasterId))
            .ForMember(dest => dest.CommentCount, opt => opt.MapFrom(src => src.Comments != null ? src.Comments.Count : 0))
            .ForMember(dest => dest.AttachmentCount, opt => opt.MapFrom(src => src.Attachments != null ? src.Attachments.Count : 0));

        CreateMap<CreateComplaintRequest, Complaint>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.ComplaintNumber, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentEscalationLevel, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DueDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<UpdateComplaintRequest, Complaint>()
            .ForMember(dest => dest.ComplaintNumber, opt => opt.Ignore())
            .ForMember(dest => dest.ComplainantId, opt => opt.Ignore())
            .ForMember(dest => dest.CompanyId, opt => opt.Ignore())
            .ForMember(dest => dest.CurrentEscalationLevel, opt => opt.Ignore())
            .ForMember(dest => dest.SubmittedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DueDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());
    }
}
