using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Features.Auth.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Auth.Handlers;

/// <summary>
/// Handler for LogoutCommand - revokes all active refresh tokens for a user
/// </summary>
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Revoke all active refresh tokens for the user
            await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                request.UserId,
                request.IpAddress,
                "User logout",
                cancellationToken);

            _logger.LogInformation("User {UserId} logged out successfully from IP: {IpAddress}", request.UserId, request.IpAddress);

            return Result<bool>.Success(true, "Logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", request.UserId);
            return Result<bool>.Failure("An error occurred during logout", "LOGOUT_ERROR");
        }
    }
}
