using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for comprehensive password management operations
/// Handles password hashing, validation, history, expiration, and lockout using AES encryption
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly ComplaintDbContext _dbContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(
        ComplaintDbContext dbContext,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService,
        ILogger<PasswordService> logger)
    {
        _dbContext = dbContext;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    #region Password Hashing & Verification

    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        return _encryptionService.EncryptPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
            return false;

        return _encryptionService.VerifyPassword(password, passwordHash);
    }

    public bool IsLegacyPasswordHash(string passwordHash)
    {
        // All passwords use AES encryption, so there's no legacy format
        // This method is here for future compatibility if we migrate to bcrypt
        return false;
    }

    #endregion

    #region Password Generation

    public string GenerateSecurePassword(
        int length = 12,
        bool includeUppercase = true,
        bool includeLowercase = true,
        bool includeDigits = true,
        bool includeSpecialChars = true)
    {
        if (length < 4)
            throw new ArgumentException("Password length must be at least 4 characters", nameof(length));

        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string digits = "0123456789";
        const string specialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

        var characterSet = "";
        var requiredChars = new List<char>();

        if (includeUppercase)
        {
            characterSet += uppercase;
            requiredChars.Add(uppercase[Random.Shared.Next(uppercase.Length)]);
        }

        if (includeLowercase)
        {
            characterSet += lowercase;
            requiredChars.Add(lowercase[Random.Shared.Next(lowercase.Length)]);
        }

        if (includeDigits)
        {
            characterSet += digits;
            requiredChars.Add(digits[Random.Shared.Next(digits.Length)]);
        }

        if (includeSpecialChars)
        {
            characterSet += specialChars;
            requiredChars.Add(specialChars[Random.Shared.Next(specialChars.Length)]);
        }

        if (string.IsNullOrEmpty(characterSet))
            throw new ArgumentException("At least one character type must be enabled");

        // Generate remaining characters
        var passwordChars = new List<char>(requiredChars);
        for (int i = requiredChars.Count; i < length; i++)
        {
            passwordChars.Add(characterSet[Random.Shared.Next(characterSet.Length)]);
        }

        // Shuffle the password characters
        var shuffled = passwordChars.OrderBy(x => Random.Shared.Next()).ToArray();
        return new string(shuffled);
    }

    #endregion

    #region Password Complexity Validation

    public async Task<PasswordValidationResult> ValidatePasswordComplexityAsync(
        string password,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password))
            return PasswordValidationResult.Failure("Password cannot be empty");

        // Get company password policy
        var policy = await _dbContext.PasswordPolicies
            .Where(p => p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        // If no policy exists, use default settings
        if (policy == null || !policy.EnablePasswordComplexity)
            return PasswordValidationResult.Success();

        var errors = new List<string>();

        // Check minimum length
        if (password.Length < policy.MinimumLength)
            errors.Add($"Password must be at least {policy.MinimumLength} characters long");

        // Check uppercase requirement
        if (policy.RequireUppercase && !password.Any(char.IsUpper))
            errors.Add("Password must contain at least one uppercase letter");

        // Check lowercase requirement
        if (policy.RequireLowercase && !password.Any(char.IsLower))
            errors.Add("Password must contain at least one lowercase letter");

        // Check digit requirement
        if (policy.RequireDigit && !password.Any(char.IsDigit))
            errors.Add("Password must contain at least one number");

        // Check special character requirement
        if (policy.RequireSpecialCharacter && !password.Any(ch => !char.IsLetterOrDigit(ch)))
            errors.Add("Password must contain at least one special character");

        return errors.Count > 0
            ? PasswordValidationResult.Failure(errors.ToArray())
            : PasswordValidationResult.Success();
    }

    public PasswordStrengthResult CalculatePasswordStrength(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            return new PasswordStrengthResult { Score = 0, Category = PasswordStrengthCategory.VeryWeak };

        int score = 0;

        // Length scoring (0-30 points)
        if (password.Length >= 8) score += 10;
        if (password.Length >= 12) score += 10;
        if (password.Length >= 16) score += 10;

        // Character variety (0-40 points)
        if (password.Any(char.IsUpper)) score += 10;
        if (password.Any(char.IsLower)) score += 10;
        if (password.Any(char.IsDigit)) score += 10;
        if (password.Any(ch => !char.IsLetterOrDigit(ch))) score += 10;

        // Complexity patterns (0-30 points)
        var distinctChars = password.Distinct().Count();
        if (distinctChars >= password.Length * 0.5) score += 10; // At least 50% unique chars
        if (distinctChars >= password.Length * 0.75) score += 10; // At least 75% unique chars

        // Check for common patterns (deduct points)
        if (ContainsCommonPattern(password)) score -= 20;

        // Ensure score is between 0 and 100
        score = Math.Max(0, Math.Min(100, score));

        // Determine category
        var category = score switch
        {
            < 20 => PasswordStrengthCategory.VeryWeak,
            < 40 => PasswordStrengthCategory.Weak,
            < 60 => PasswordStrengthCategory.Fair,
            < 80 => PasswordStrengthCategory.Good,
            < 90 => PasswordStrengthCategory.Strong,
            _ => PasswordStrengthCategory.VeryStrong
        };

        return new PasswordStrengthResult { Score = score, Category = category };
    }

    private bool ContainsCommonPattern(string password)
    {
        var lowerPassword = password.ToLower();
        var commonPatterns = new[] { "password", "123456", "qwerty", "abc123", "admin", "letmein", "welcome" };
        return commonPatterns.Any(pattern => lowerPassword.Contains(pattern));
    }

    #endregion

    #region Password History

    public async Task<bool> IsPasswordInHistoryAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return false;

        // Get password policy to determine how many passwords to check
        var policy = await _dbContext.PasswordPolicies
            .Where(p => p.CompanyId == user.CompanyId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (policy == null || policy.PasswordHistoryCount == 0)
            return false;

        // Get the last N password hashes
        var historyCount = policy.PasswordHistoryCount;
        var passwordHistory = await _dbContext.PasswordHistories
            .Where(ph => ph.UserId == userId && !ph.IsDeleted)
            .OrderByDescending(ph => ph.CreatedAt)
            .Take(historyCount)
            .Select(ph => ph.PasswordHash)
            .ToListAsync(cancellationToken);

        // Check if the new password matches any in history
        foreach (var historicalHash in passwordHistory)
        {
            if (VerifyPassword(password, historicalHash))
                return true;
        }

        return false;
    }

    public async Task AddPasswordToHistoryAsync(
        Guid userId,
        string passwordHash,
        Guid? changedBy,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var passwordHistory = new PasswordHistory
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = changedBy,
            IpAddress = ipAddress,
            IsDeleted = false
        };

        await _dbContext.PasswordHistories.AddAsync(passwordHistory, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Password added to history for user {UserId}", userId);
    }

    public async Task CleanupPasswordHistoryAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        // Get password policy
        var policy = await _dbContext.PasswordPolicies
            .Where(p => p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (policy == null)
            return;

        var historyCount = policy.PasswordHistoryCount;

        // Get all password history entries for user
        var allHistory = await _dbContext.PasswordHistories
            .Where(ph => ph.UserId == userId && !ph.IsDeleted)
            .OrderByDescending(ph => ph.CreatedAt)
            .ToListAsync(cancellationToken);

        // Keep only the last N entries, soft-delete the rest
        if (allHistory.Count > historyCount)
        {
            var toDelete = allHistory.Skip(historyCount).ToList();
            foreach (var entry in toDelete)
            {
                entry.IsDeleted = true;
                entry.DeletedAt = DateTime.UtcNow;
                _dbContext.PasswordHistories.Update(entry);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Cleaned up {Count} old password history entries for user {UserId}",
                toDelete.Count, userId);
        }
    }

    #endregion

    #region Password Expiration

    public async Task<bool> IsPasswordExpiredAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return false;

        // Password never expires flag
        if (user.PasswordNeverExpires)
            return false;

        // Check if password expiration date is set and has passed
        if (user.PasswordExpiresAt.HasValue && user.PasswordExpiresAt.Value < DateTime.UtcNow)
            return true;

        return false;
    }

    public async Task<int?> GetDaysUntilPasswordExpiresAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.PasswordNeverExpires || !user.PasswordExpiresAt.HasValue)
            return null;

        var daysUntilExpiration = (user.PasswordExpiresAt.Value - DateTime.UtcNow).Days;
        return daysUntilExpiration;
    }

    public async Task UpdatePasswordExpirationAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null || user.PasswordNeverExpires)
            return;

        // Get password policy
        var policy = await _dbContext.PasswordPolicies
            .Where(p => p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (policy == null || policy.PasswordExpirationDays == 0)
        {
            user.PasswordExpiresAt = null;
        }
        else
        {
            user.PasswordExpiresAt = DateTime.UtcNow.AddDays(policy.PasswordExpirationDays);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated password expiration for user {UserId} to {ExpiresAt}",
            userId, user.PasswordExpiresAt);
    }

    #endregion

    #region Account Lockout

    public async Task<bool> IsAccountLockedAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return false;

        // Check if account is locked and lockout hasn't expired
        if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value > DateTime.UtcNow)
            return true;

        // If lockout has expired, clear it
        if (user.AccountLockedUntil.HasValue && user.AccountLockedUntil.Value <= DateTime.UtcNow)
        {
            user.AccountLockedUntil = null;
            user.FailedLoginAttempts = 0;
            _unitOfWork.Users.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return false;
    }

    public async Task<bool> IncrementFailedLoginAttemptAsync(
        Guid userId,
        Guid companyId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return false;

        // Get password policy
        var policy = await _dbContext.PasswordPolicies
            .Where(p => p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (policy == null)
            return false;

        user.FailedLoginAttempts++;

        bool accountLocked = false;

        // Check if max attempts exceeded
        if (user.FailedLoginAttempts >= policy.MaxFailedLoginAttempts)
        {
            user.AccountLockedUntil = DateTime.UtcNow.AddMinutes(policy.AccountLockoutDurationMinutes);
            accountLocked = true;

            await LogPasswordActionAsync(
                userId,
                PasswordAction.AccountLocked,
                null,
                true,
                $"Account locked after {user.FailedLoginAttempts} failed login attempts",
                ipAddress,
                null,
                cancellationToken);

            _logger.LogWarning("Account locked for user {UserId} after {Attempts} failed login attempts",
                userId, user.FailedLoginAttempts);
        }

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return accountLocked;
    }

    public async Task ResetFailedLoginAttemptsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return;

        user.FailedLoginAttempts = 0;
        user.AccountLockedUntil = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task UnlockAccountAsync(
        Guid userId,
        Guid unlockedBy,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return;

        user.AccountLockedUntil = null;
        user.FailedLoginAttempts = 0;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await LogPasswordActionAsync(
            userId,
            PasswordAction.AccountUnlocked,
            unlockedBy,
            true,
            "Account manually unlocked by administrator",
            null,
            null,
            cancellationToken);

        _logger.LogInformation("Account unlocked for user {UserId} by {UnlockedBy}", userId, unlockedBy);
    }

    #endregion

    #region Password Operations (Admin)

    public async Task<Result> SetUserPasswordAsync(
        Guid userId,
        string password,
        Guid setBy,
        bool mustChangeOnNextLogin = true,
        bool sendEmail = false,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        // Validate password complexity
        var validationResult = await ValidatePasswordComplexityAsync(password, user.CompanyId, cancellationToken);
        if (!validationResult.IsValid)
            return Result.Failure(string.Join(", ", validationResult.Errors));

        // Check password history
        var inHistory = await IsPasswordInHistoryAsync(userId, password, cancellationToken);
        if (inHistory)
            return Result.Failure("Password has been used recently. Please choose a different password.");

        // Hash and set password
        var passwordHash = HashPassword(password);
        user.PasswordHash = passwordHash;
        user.MustChangePasswordOnNextLogin = mustChangeOnNextLogin;
        user.PasswordChangedAt = DateTime.UtcNow;
        user.PasswordChangedBy = setBy;

        // Update password expiration
        await UpdatePasswordExpirationAsync(userId, user.CompanyId, cancellationToken);

        // Reset lockout
        user.FailedLoginAttempts = 0;
        user.AccountLockedUntil = null;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add to password history
        await AddPasswordToHistoryAsync(userId, passwordHash, setBy, ipAddress, cancellationToken);

        // Cleanup old history
        await CleanupPasswordHistoryAsync(userId, user.CompanyId, cancellationToken);

        // Log the action
        await LogPasswordActionAsync(
            userId,
            PasswordAction.SetByAdmin,
            setBy,
            true,
            $"Password set by admin. Must change on next login: {mustChangeOnNextLogin}",
            ipAddress,
            null,
            cancellationToken);

        _logger.LogInformation("Password set for user {UserId} by admin {SetBy}", userId, setBy);

        // TODO: Send email if requested
        if (sendEmail)
        {
            // Email sending will be implemented in a future update
            _logger.LogInformation("Email notification requested for user {UserId} (not yet implemented)", userId);
        }

        return Result.Success();
    }

    public async Task<Result<string>> ResetUserPasswordAsync(
        Guid userId,
        Guid resetBy,
        bool sendEmail = true,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result<string>.Failure("User not found");

        // Generate a secure random password
        var newPassword = GenerateSecurePassword(12, true, true, true, true);

        // Set the password
        var result = await SetUserPasswordAsync(
            userId,
            newPassword,
            resetBy,
            mustChangeOnNextLogin: true,
            sendEmail: false,
            ipAddress: ipAddress,
            cancellationToken: cancellationToken);

        if (!result.IsSuccess)
            return Result<string>.Failure(result.Message);

        // Log the reset action
        await LogPasswordActionAsync(
            userId,
            PasswordAction.ResetByAdmin,
            resetBy,
            true,
            "Password auto-generated and reset by administrator",
            ipAddress,
            null,
            cancellationToken);

        _logger.LogInformation("Password reset for user {UserId} by admin {ResetBy}", userId, resetBy);

        // TODO: Send email if requested
        if (sendEmail)
        {
            await LogPasswordActionAsync(
                userId,
                PasswordAction.SentViaEmail,
                resetBy,
                true,
                "Password sent via email (not yet implemented)",
                ipAddress,
                null,
                cancellationToken);
        }

        return Result<string>.Success(newPassword);
    }

    #endregion

    #region Password Operations (User)

    public async Task<Result> ChangeUserPasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);

        if (user == null)
            return Result.Failure("User not found");

        // Verify current password
        if (!VerifyPassword(currentPassword, user.PasswordHash))
        {
            await LogPasswordActionAsync(
                userId,
                PasswordAction.ChangeFailed,
                userId,
                false,
                "Invalid current password",
                ipAddress,
                null,
                cancellationToken);

            return Result.Failure("Current password is incorrect");
        }

        // Validate new password complexity
        var validationResult = await ValidatePasswordComplexityAsync(newPassword, user.CompanyId, cancellationToken);
        if (!validationResult.IsValid)
        {
            await LogPasswordActionAsync(
                userId,
                PasswordAction.ChangeFailed,
                userId,
                false,
                $"Password complexity validation failed: {string.Join(", ", validationResult.Errors)}",
                ipAddress,
                null,
                cancellationToken);

            return Result.Failure(string.Join(", ", validationResult.Errors));
        }

        // Check if new password is same as current
        if (currentPassword == newPassword)
            return Result.Failure("New password must be different from current password");

        // Check password history
        var inHistory = await IsPasswordInHistoryAsync(userId, newPassword, cancellationToken);
        if (inHistory)
        {
            await LogPasswordActionAsync(
                userId,
                PasswordAction.ChangeFailed,
                userId,
                false,
                "Password in recent history",
                ipAddress,
                null,
                cancellationToken);

            return Result.Failure("Password has been used recently. Please choose a different password.");
        }

        // Hash and set new password
        var passwordHash = HashPassword(newPassword);
        user.PasswordHash = passwordHash;
        user.MustChangePasswordOnNextLogin = false;
        user.PasswordChangedAt = DateTime.UtcNow;
        user.PasswordChangedBy = userId;

        // Update password expiration
        await UpdatePasswordExpirationAsync(userId, user.CompanyId, cancellationToken);

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Add to password history
        await AddPasswordToHistoryAsync(userId, passwordHash, userId, ipAddress, cancellationToken);

        // Cleanup old history
        await CleanupPasswordHistoryAsync(userId, user.CompanyId, cancellationToken);

        // Log successful change
        await LogPasswordActionAsync(
            userId,
            PasswordAction.ChangedByUser,
            userId,
            true,
            "Password successfully changed by user",
            ipAddress,
            null,
            cancellationToken);

        _logger.LogInformation("Password changed successfully for user {UserId}", userId);

        return Result.Success();
    }

    #endregion

    #region Audit Logging

    public async Task LogPasswordActionAsync(
        Guid userId,
        PasswordAction action,
        Guid? performedBy,
        bool success,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default)
    {
        var auditLog = new PasswordAuditLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            PerformedBy = performedBy,
            Success = success,
            Details = details,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        await _dbContext.PasswordAuditLogs.AddAsync(auditLog, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
