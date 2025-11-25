using ComplaintManagement.Domain.Entities.Auth;

namespace ComplaintManagement.Application.Interfaces.Repositories;

/// <summary>
/// Repository interface for RefreshToken operations with security-focused methods
/// </summary>
public interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    /// <summary>
    /// Get an active refresh token by its token value
    /// Returns null if token doesn't exist, is expired, used, or revoked
    /// </summary>
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get refresh token by value including inactive ones (for theft detection)
    /// </summary>
    Task<RefreshToken?> GetTokenByValueAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all tokens in a token family (for revocation on theft detection)
    /// </summary>
    Task<List<RefreshToken>> GetTokenFamilyAsync(Guid tokenFamily, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all active refresh tokens for a user
    /// </summary>
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke a specific token
    /// </summary>
    Task RevokeTokenAsync(RefreshToken token, string revokedByIp, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke all tokens in a family (for theft detection)
    /// </summary>
    Task RevokeTokenFamilyAsync(Guid tokenFamily, string revokedByIp, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoke all active tokens for a user (for logout)
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up expired tokens (for maintenance)
    /// </summary>
    Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark token as used and replace with new token
    /// </summary>
    Task<RefreshToken> RotateTokenAsync(RefreshToken oldToken, RefreshToken newToken, string usedByIp, CancellationToken cancellationToken = default);
}
