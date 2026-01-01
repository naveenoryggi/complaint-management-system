using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Portal;
using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;
using System.Security.Claims;

namespace ComplaintManagement.Application.Interfaces.Services;

public interface IJwtTokenService
{
    /// <summary>
    /// Generates access token for internal users (employees)
    /// </summary>
    TokenResult GenerateAccessToken(User user, List<string> permissions, List<string>? roleCodes = null);

    /// <summary>
    /// Generates access token for portal users (customer contacts)
    /// </summary>
    TokenResult GeneratePortalAccessToken(CustomerContact contact, Customer customer);

    /// <summary>
    /// Generates a cryptographically secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates and extracts principal from an expired token
    /// </summary>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);

    /// <summary>
    /// Gets user ID from token
    /// </summary>
    Guid? GetUserIdFromToken(string token);

    /// <summary>
    /// Gets portal contact ID from token
    /// </summary>
    Guid? GetPortalContactIdFromToken(string token);

    /// <summary>
    /// Checks if the token is a portal token
    /// </summary>
    bool IsPortalToken(string token);

    /// <summary>
    /// Gets company ID from token (works for both internal and portal tokens)
    /// </summary>
    Guid? GetCompanyIdFromToken(string token);
}
