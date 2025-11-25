using AutoMapper;
using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Entities.Roles;

namespace ComplaintManagement.Application.Mappings;

public class AuthMappingProfile : Profile
{
    public AuthMappingProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dest => dest.CompanyName, opt => opt.MapFrom(src => src.Company != null ? src.Company.Name : string.Empty))
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserComplaintRoles))
            .ForMember(dest => dest.Permissions, opt => opt.Ignore()); // Permissions are calculated separately

        CreateMap<UserComplaintRole, UserRoleDto>()
            .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.ComplaintRoleId))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.ComplaintRole.Name))
            .ForMember(dest => dest.RoleCode, opt => opt.MapFrom(src => src.ComplaintRole.Code))
            .ForMember(dest => dest.RoleType, opt => opt.MapFrom(src => src.ComplaintRole.RoleType.ToString()))
            .ForMember(dest => dest.EscalationLevel, opt => opt.MapFrom(src => src.ComplaintRole.EscalationLevel))
            .ForMember(dest => dest.IsPrimary, opt => opt.MapFrom(src => src.IsPrimary));
    }
}
