using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using MediatR;

namespace ComplaintManagement.Application.Features.Auth.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    /// <summary>
    /// User identifier: can be Email, Employee Code, or Phone Number
    /// </summary>
    public string UserIdentifier { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}
