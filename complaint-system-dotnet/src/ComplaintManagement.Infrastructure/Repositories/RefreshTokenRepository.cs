using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for RefreshToken with security-focused operations
/// </summary>
public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(ComplaintDbContext context) : base(context)
    {
    }

    /// <summary>
    /// Get an active refresh token by its token value
    /// Returns null if token doesn't exist, is expired, used, or revoked
    /// </summary>
    public async Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.Token == token &&
                        rt.RevokedAt == null &&
                        rt.UsedAt == null &&
                        rt.ExpiresAt > DateTime.UtcNow)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get refresh token by value including inactive ones (for theft detection)
    /// </summary>
    public async Task<RefreshToken?> GetTokenByValueAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.Token == token)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// Get all tokens in a token family (for revocation on theft detection)
    /// </summary>
    public async Task<List<RefreshToken>> GetTokenFamilyAsync(Guid tokenFamily, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.TokenFamily == tokenFamily)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Get all active refresh tokens for a user
    /// </summary>
    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId &&
                        rt.RevokedAt == null &&
                        rt.UsedAt == null &&
                        rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Revoke a specific token
    /// </summary>
    public async Task RevokeTokenAsync(RefreshToken token, string revokedByIp, string reason, CancellationToken cancellationToken = default)
    {
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = revokedByIp;
        token.RevocationReason = reason;

        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Revoke all tokens in a family (for theft detection)
    /// </summary>
    public async Task RevokeTokenFamilyAsync(Guid tokenFamily, string revokedByIp, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await GetTokenFamilyAsync(tokenFamily, cancellationToken);

        foreach (var token in tokens.Where(t => t.RevokedAt == null))
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            token.RevocationReason = reason;
        }

        _context.RefreshTokens.UpdateRange(tokens);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Revoke all active tokens for a user (for logout)
    /// </summary>
    public async Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default)
    {
        var tokens = await GetActiveTokensByUserIdAsync(userId, cancellationToken);

        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = revokedByIp;
            token.RevocationReason = reason;
        }

        _context.RefreshTokens.UpdateRange(tokens);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Clean up expired tokens (for maintenance)
    /// </summary>
    public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiresAt < DateTime.UtcNow.AddDays(-30)) // Keep for 30 days after expiry for audit
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Mark token as used and replace with new token
    /// </summary>
    public async Task<RefreshToken> RotateTokenAsync(RefreshToken oldToken, RefreshToken newToken, string usedByIp, CancellationToken cancellationToken = default)
    {
        // Mark old token as used
        oldToken.UsedAt = DateTime.UtcNow;
        oldToken.ReplacedByTokenId = newToken.Id;

        // Add new token
        _context.RefreshTokens.Update(oldToken);
        await _context.RefreshTokens.AddAsync(newToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return newToken;
    }
}
