using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Application.Features.Auth.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Auth;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace ComplaintManagement.Application.Features.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEncryptionService _encryptionService;
    private readonly IConfiguration _configuration;
    private readonly int _refreshTokenExpirationDays;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService,
        IEncryptionService encryptionService,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
        _encryptionService = encryptionService;
        _configuration = configuration;
        _refreshTokenExpirationDays = int.Parse(configuration["JwtSettings:RefreshTokenExpirationDays"] ?? "7");
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Get user by email, employee code, or phone number
        var user = await _unitOfWork.Users.GetByIdentifierAsync(request.UserIdentifier, cancellationToken);

        if (user == null)
        {
            return Result<LoginResponse>.Failure("Invalid credentials", "Authentication failed");
        }

        // Verify password using AES encryption
        if (!_encryptionService.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid credentials", "Authentication failed");
        }

        if (!user.IsActive)
        {
            return Result<LoginResponse>.Failure("User account is inactive", "Account disabled");
        }

        // Get user with roles and permissions
        var userWithRoles = await _unitOfWork.Users.GetUserWithRolesAsync(user.Id, cancellationToken);

        if (userWithRoles == null)
        {
            return Result<LoginResponse>.Failure("User data not found", "Data error");
        }

        // Extract permissions from roles
        var permissions = userWithRoles.UserComplaintRoles
            .Where(r => r.IsActive)
            .SelectMany(r => r.ComplaintRole.RolePermissions)
            .Where(p => p.IsGranted)
            .Select(p => p.PermissionType.ToString())
            .Distinct()
            .ToList();

        // Extract role codes
        var roleCodes = userWithRoles.UserComplaintRoles
            .Where(r => r.IsActive)
            .Select(r => r.ComplaintRole.Code)
            .Distinct()
            .ToList();

        // Generate tokens
        var tokenResult = _jwtTokenService.GenerateAccessToken(userWithRoles, permissions, roleCodes);
        var refreshTokenValue = _jwtTokenService.GenerateRefreshToken();

        // Create refresh token entity and save to database
        var tokenFamily = Guid.NewGuid(); // New token family for this login session
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            Token = refreshTokenValue,
            UserId = userWithRoles.Id,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            CreatedByIp = "Unknown", // IP address should be passed from controller in production
            TokenFamily = tokenFamily
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken, cancellationToken);

        // Resolve timezone using fallback chain: User > Branch > Company > Default
        string resolvedTimeZone = userWithRoles.PreferredTimeZone
            ?? userWithRoles.Branch?.TimeZone
            ?? userWithRoles.Company?.DefaultTimeZone
            ?? "Asia/Kolkata";

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

            // Timezone & Localization (resolved via fallback chain)
            TimeZone = resolvedTimeZone,
            PreferredTimeZone = userWithRoles.PreferredTimeZone,
            PreferredLocale = userWithRoles.PreferredLocale,
            PreferredDateFormat = userWithRoles.PreferredDateFormat,
            PreferredTimeFormat = userWithRoles.PreferredTimeFormat,

            Roles = userWithRoles.UserComplaintRoles
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
            RefreshToken = refreshToken.Token, // Return the token value, not the entity
            ExpiresAt = tokenResult.ExpiresAt,
            User = userDto
        };

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LoginResponse>.Success(response, "Login successful");
    }
}
