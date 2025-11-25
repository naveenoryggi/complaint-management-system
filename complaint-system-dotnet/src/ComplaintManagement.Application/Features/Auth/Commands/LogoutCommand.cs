using ComplaintManagement.Application.Common.Models;
using MediatR;

namespace ComplaintManagement.Application.Features.Auth.Commands;

/// <summary>
/// Command to logout user and revoke all active refresh tokens
/// </summary>
public class LogoutCommand : IRequest<Result<bool>>
{
    /// <summary>
    /// User ID to logout
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// IP address of the client making the request (for audit)
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}
