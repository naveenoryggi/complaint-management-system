using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Application.Features.Auth.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Auth.Handlers;

/// <summary>
/// Handler for RefreshTokenCommand with security-focused token rotation
/// Implements defense against token theft through family tracking
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly IConfiguration _configuration;
    private readonly int _refreshTokenExpirationDays;

    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        ILogger<RefreshTokenCommandHandler> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _configuration = configuration;
        _refreshTokenExpirationDays = int.Parse(configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate refresh token format
            if (string.IsNullOrWhiteSpace(request.RefreshToken))
            {
                return Result<LoginResponse>.Failure("Invalid refresh token", "INVALID_TOKEN");
            }

            // Get refresh token from database (including inactive ones for theft detection)
            var refreshToken = await _unitOfWork.RefreshTokens.GetTokenByValueAsync(request.RefreshToken, cancellationToken);

            if (refreshToken == null)
            {
                _logger.LogWarning("Refresh token not found: {Token}", request.RefreshToken.Substring(0, Math.Min(10, request.RefreshToken.Length)));
                return Result<LoginResponse>.Failure("Invalid refresh token", "TOKEN_NOT_FOUND");
            }

            // SECURITY: Check if token was already used (potential theft)
            if (refreshToken.IsUsed)
            {
                _logger.LogWarning(
                    "Token reuse detected! Token family {TokenFamily} for user {UserId}. Revoking entire family.",
                    refreshToken.TokenFamily,
                    refreshToken.UserId);

                // Revoke entire token family to prevent further use
                await _unitOfWork.RefreshTokens.RevokeTokenFamilyAsync(
                    refreshToken.TokenFamily,
                    request.IpAddress,
                    "Token reuse detected - possible theft",
                    cancellationToken);

                return Result<LoginResponse>.Failure("Token has been revoked due to security concerns", "TOKEN_THEFT_DETECTED");
            }

            // Check if token was revoked
            if (refreshToken.IsRevoked)
            {
                _logger.LogWarning("Revoked token used: {TokenId} by user {UserId}", refreshToken.Id, refreshToken.UserId);
                return Result<LoginResponse>.Failure("Token has been revoked", "TOKEN_REVOKED");
            }

            // Check if token is expired
            if (refreshToken.IsExpired)
            {
                _logger.LogInformation("Expired token used: {TokenId} by user {UserId}", refreshToken.Id, refreshToken.UserId);
                return Result<LoginResponse>.Failure("Refresh token has expired. Please login again.", "TOKEN_EXPIRED");
            }

            // Get user with roles and permissions
            var user = await _unitOfWork.Users.GetUserWithRolesAsync(refreshToken.UserId, cancellationToken);

            if (user == null)
            {
                _logger.LogError("User not found for refresh token: {UserId}", refreshToken.UserId);
                return Result<LoginResponse>.Failure("User not found", "USER_NOT_FOUND");
            }

            // Check if user is still active
            if (!user.IsActive)
            {
                _logger.LogWarning("Inactive user attempted token refresh: {UserId}", refreshToken.UserId);

                // Revoke all user tokens
                await _unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(
                    user.Id,
                    request.IpAddress,
                    "User account is inactive",
                    cancellationToken);

                return Result<LoginResponse>.Failure("User account is inactive", "ACCOUNT_INACTIVE");
            }

            // Extract permissions from roles
            var permissions = user.UserComplaintRoles
                .Where(r => r.IsActive)
                .SelectMany(r => r.ComplaintRole.RolePermissions)
                .Where(p => p.IsGranted)
                .Select(p => p.PermissionType.ToString())
                .Distinct()
                .ToList();

            // Extract role codes
            var roleCodes = user.UserComplaintRoles
                .Where(r => r.IsActive)
                .Select(r => r.ComplaintRole.Code)
                .Distinct()
                .ToList();

            // Generate new access token
            var tokenResult = _jwtTokenService.GenerateAccessToken(user, permissions, roleCodes);

            // Generate new refresh token (token rotation)
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = _jwtTokenService.GenerateRefreshToken(),
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
                CreatedByIp = request.IpAddress,
                TokenFamily = refreshToken.TokenFamily // Maintain same family for tracking
            };

            // Rotate tokens: mark old as used, save new one
            await _unitOfWork.RefreshTokens.RotateTokenAsync(refreshToken, newRefreshToken, request.IpAddress, cancellationToken);

            _logger.LogInformation(
                "Token refreshed successfully for user {UserId}. Old token: {OldTokenId}, New token: {NewTokenId}",
                user.Id,
                refreshToken.Id,
                newRefreshToken.Id);

            // Create user DTO
            var userDto = new UserDto
            {
                Id = user.Id,
                EmployeeCode = user.EmployeeCode,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                JobTitle = user.JobTitle,
                CompanyId = user.CompanyId,
                CompanyName = user.Company?.Name ?? string.Empty,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                DepartmentId = user.DepartmentId,
                DepartmentName = user.Department?.Name,
                SectionId = user.SectionId,
                SectionName = user.Section?.Name,
                EmployeeTypeId = user.EmployeeTypeId,
                EmployeeTypeName = null, // EmployeeType navigation not loaded in GetUserWithRolesAsync
                ManagerId = user.ManagerId,
                ManagerName = user.Manager?.FullName,
                IsActive = user.IsActive,
                Roles = user.UserComplaintRoles
                    .Where(r => r.IsActive)
                    .Select(r => new UserRoleDto
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

            var response = new LoginResponse
            {
                Token = tokenResult.Token,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = tokenResult.ExpiresAt,
                User = userDto
            };

            return Result<LoginResponse>.Success(response, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token for IP: {IpAddress}", request.IpAddress);
            return Result<LoginResponse>.Failure("An error occurred while refreshing the token", "REFRESH_ERROR");
        }
    }
}
