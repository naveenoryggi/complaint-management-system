using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Domain.Entities.MasterData;
using System.Security.Claims;

namespace ComplaintManagement.Application.Interfaces.Services;

public interface IJwtTokenService
{
    TokenResult GenerateAccessToken(User user, List<string> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    Guid? GetUserIdFromToken(string token);
}
