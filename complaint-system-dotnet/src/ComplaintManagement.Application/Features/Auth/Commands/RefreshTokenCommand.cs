using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using MediatR;

namespace ComplaintManagement.Application.Features.Auth.Commands;

/// <summary>
/// Command to refresh an expired access token using a valid refresh token
/// Implements token rotation for enhanced security
/// </summary>
public class RefreshTokenCommand : IRequest<Result<LoginResponse>>
{
    /// <summary>
    /// The refresh token provided by the client
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// IP address of the client making the request (for audit and theft detection)
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;
}
