using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Application.Features.Auth.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Auth.Handlers;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrentUserQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        // Get user with roles and permissions
        var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(request.UserId, cancellationToken);

        if (userWithRoles == null)
        {
            return Result<UserDto>.Failure("User not found", "UserNotFound");
        }

        if (!userWithRoles.IsActive)
        {
            return Result<UserDto>.Failure("User account is inactive", "AccountInactive");
        }

        // Extract permissions from roles
        var permissions = userWithRoles.UserComplaintRoles
            .Where(r => r.IsActive)
            .SelectMany(r => r.ComplaintRole.RolePermissions)
            .Where(p => p.IsGranted)
            .Select(p => p.PermissionType.ToString())
            .Distinct()
            .ToList();

        // Create user DTO
        var userDto = new UserDto
        {
            Id = userWithRoles.Id,
            EmployeeCode = userWithRoles.EmployeeCode,
            FirstName = userWithRoles.FirstName,
            LastName = userWithRoles.LastName,
            FullName = userWithRoles.FullName,
            Email = userWithRoles.Email,
            Phone = userWithRoles.Phone,
            JobTitle = userWithRoles.JobTitle,
            CompanyId = userWithRoles.CompanyId,
            CompanyName = userWithRoles.Company?.Name ?? string.Empty,
            Roles = userWithRoles.UserComplaintRoles.Select(r => new UserRoleDto
            {
                RoleId = r.ComplaintRoleId,
                RoleName = r.ComplaintRole.Name,
                RoleCode = r.ComplaintRole.Code,
                RoleType = r.ComplaintRole.RoleType.ToString(),
                EscalationLevel = r.ComplaintRole.EscalationLevel,
                IsPrimary = r.IsPrimary
            }).ToList(),
            Permissions = permissions
        };

        return Result<UserDto>.Success(userDto, "User retrieved successfully");
    }
}
