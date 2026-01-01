using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Portal;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Entities.MasterData;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ComplaintManagement.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;
    private readonly int _portalExpirationMinutes;

    // Custom claim types for portal
    private const string PortalUserClaim = "IsPortalUser";
    private const string ContactIdClaim = "ContactId";
    private const string CustomerIdClaim = "CustomerId";
    private const string CustomerCodeClaim = "CustomerCode";
    private const string LocationIdClaim = "LocationId";
    private const string PortalRoleClaim = "PortalRole";
    private const string PortalPermissionClaim = "PortalPermission";

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
        _secretKey = configuration["JwtSettings:SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");
        _issuer = configuration["JwtSettings:Issuer"] ?? "ComplaintManagementSystem";
        _audience = configuration["JwtSettings:Audience"] ?? "ComplaintManagementAPI";
        _expirationMinutes = int.Parse(configuration["JwtSettings:ExpirationMinutes"] ?? "60");
        _portalExpirationMinutes = int.Parse(configuration["JwtSettings:PortalExpirationMinutes"] ?? "120");
    }

    public TokenResult GenerateAccessToken(User user, List<string> permissions, List<string>? roleCodes = null)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("EmployeeCode", user.EmployeeCode),
            new Claim("CompanyId", user.CompanyId.ToString()),
            new Claim(PortalUserClaim, "false")
        };

        // Add role codes as claims
        if (roleCodes != null)
        {
            foreach (var roleCode in roleCodes)
            {
                claims.Add(new Claim("RoleCode", roleCode));
            }
        }

        // Add permissions as claims
        foreach (var permission in permissions)
        {
            claims.Add(new Claim("Permission", permission));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new TokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }

    public TokenResult GeneratePortalAccessToken(CustomerContact contact, Customer customer)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, contact.Id.ToString()),
            new Claim(ClaimTypes.Email, contact.Email),
            new Claim(ClaimTypes.Name, contact.FullName),
            new Claim(PortalUserClaim, "true"),
            new Claim(ContactIdClaim, contact.Id.ToString()),
            new Claim(CustomerIdClaim, contact.CustomerId.ToString()),
            new Claim(CustomerCodeClaim, customer.Code),
            new Claim("CompanyId", customer.CompanyId.ToString()),
            new Claim(PortalRoleClaim, contact.PortalRole.ToString())
        };

        // Add location if specified
        if (contact.LocationId.HasValue)
        {
            claims.Add(new Claim(LocationIdClaim, contact.LocationId.Value.ToString()));
        }

        // Add portal permissions
        AddPortalPermissionClaims(claims, contact);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expiresAt = DateTime.UtcNow.AddMinutes(_portalExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new TokenResult
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAt = expiresAt
        };
    }

    private void AddPortalPermissionClaims(List<Claim> claims, CustomerContact contact)
    {
        if (contact.CanCreateTickets)
            claims.Add(new Claim(PortalPermissionClaim, "CreateTickets"));
        if (contact.CanViewAllLocationTickets)
            claims.Add(new Claim(PortalPermissionClaim, "ViewAllLocationTickets"));
        if (contact.CanViewAllCustomerTickets)
            claims.Add(new Claim(PortalPermissionClaim, "ViewAllCustomerTickets"));
        if (contact.CanManageContacts)
            claims.Add(new Claim(PortalPermissionClaim, "ManageContacts"));
        if (contact.CanViewContracts)
            claims.Add(new Claim(PortalPermissionClaim, "ViewContracts"));
        if (contact.CanViewAssets)
            claims.Add(new Claim(PortalPermissionClaim, "ViewAssets"));
        if (contact.CanApproveEscalations)
            claims.Add(new Claim(PortalPermissionClaim, "ApproveEscalations"));
        if (contact.CanDownloadReports)
            claims.Add(new Claim(PortalPermissionClaim, "DownloadReports"));
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            return null;
        }

        return principal;
    }

    public Guid? GetUserIdFromToken(string token)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return userId;
        }

        return null;
    }

    public Guid? GetPortalContactIdFromToken(string token)
    {
        var principal = GetPrincipalFromExpiredToken(token);

        // Check if it's a portal token
        var isPortalClaim = principal?.FindFirst(PortalUserClaim);
        if (isPortalClaim?.Value != "true")
        {
            return null;
        }

        var contactIdClaim = principal?.FindFirst(ContactIdClaim);
        if (contactIdClaim != null && Guid.TryParse(contactIdClaim.Value, out var contactId))
        {
            return contactId;
        }

        return null;
    }

    public bool IsPortalToken(string token)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            var isPortalClaim = principal?.FindFirst(PortalUserClaim);
            return isPortalClaim?.Value == "true";
        }
        catch
        {
            return false;
        }
    }

    public Guid? GetCompanyIdFromToken(string token)
    {
        var principal = GetPrincipalFromExpiredToken(token);
        var companyIdClaim = principal?.FindFirst("CompanyId");

        if (companyIdClaim != null && Guid.TryParse(companyIdClaim.Value, out var companyId))
        {
            return companyId;
        }

        return null;
    }
}
