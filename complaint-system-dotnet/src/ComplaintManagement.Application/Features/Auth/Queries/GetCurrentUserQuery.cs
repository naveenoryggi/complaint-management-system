using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using MediatR;

namespace ComplaintManagement.Application.Features.Auth.Queries;

public class GetCurrentUserQuery : IRequest<Result<UserDto>>
{
    public Guid UserId { get; set; }
}
