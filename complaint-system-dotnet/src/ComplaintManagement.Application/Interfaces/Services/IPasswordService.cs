using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for password management operations
/// Handles password hashing, validation, history, expiration, and lockout
/// </summary>
public interface IPasswordService
{
    // ========== Password Hashing & Verification ==========

    /// <summary>
    /// Hash a plaintext password using bcrypt
    /// </summary>
    /// <param name="password">Plaintext password</param>
    /// <returns>Hashed password</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verify a password against a hash
    /// Supports both bcrypt (new) and AES (legacy) hashes for migration
    /// </summary>
    /// <param name="password">Plaintext password</param>
    /// <param name="passwordHash">Hashed password</param>
    /// <returns>True if password matches, false otherwise</returns>
    bool VerifyPassword(string password, string passwordHash);

    /// <summary>
    /// Check if a password hash is using legacy encryption (AES)
    /// </summary>
    /// <param name="passwordHash">Password hash to check</param>
    /// <returns>True if legacy format, false if bcrypt</returns>
    bool IsLegacyPasswordHash(string passwordHash);

    // ========== Password Generation ==========

    /// <summary>
    /// Generate a secure random password
    /// </summary>
    /// <param name="length">Password length (default: 12)</param>
    /// <param name="includeUppercase">Include uppercase letters</param>
    /// <param name="includeLowercase">Include lowercase letters</param>
    /// <param name="includeDigits">Include numbers</param>
    /// <param name="includeSpecialChars">Include special characters</param>
    /// <returns>Generated password</returns>
    string GenerateSecurePassword(
        int length = 12,
        bool includeUppercase = true,
        bool includeLowercase = true,
        bool includeDigits = true,
        bool includeSpecialChars = true);

    // ========== Password Complexity Validation ==========

    /// <summary>
    /// Validate password against company's password policy
    /// </summary>
    /// <param name="password">Password to validate</param>
    /// <param name="companyId">Company ID for policy lookup</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with errors if any</returns>
    Task<PasswordValidationResult> ValidatePasswordComplexityAsync(
        string password,
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate password strength score (0-100)
    /// </summary>
    /// <param name="password">Password to evaluate</param>
    /// <returns>Strength score and category (Weak, Fair, Good, Strong, Very Strong)</returns>
    PasswordStrengthResult CalculatePasswordStrength(string password);

    // ========== Password History ==========

    /// <summary>
    /// Check if a password is in user's password history
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="password">Password to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if password was used before, false otherwise</returns>
    Task<bool> IsPasswordInHistoryAsync(
        Guid userId,
        string password,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a password to user's history
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="passwordHash">Password hash</param>
    /// <param name="changedBy">Who changed the password (null if user themselves)</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task AddPasswordToHistoryAsync(
        Guid userId,
        string passwordHash,
        Guid? changedBy,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clean up old password history entries based on policy
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="companyId">Company ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task CleanupPasswordHistoryAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default);

    // ========== Password Expiration ==========

    /// <summary>
    /// Check if user's password is expired
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if expired, false otherwise</returns>
    Task<bool> IsPasswordExpiredAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get days until password expires
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Days until expiration (null if never expires, negative if already expired)</returns>
    Task<int?> GetDaysUntilPasswordExpiresAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update password expiration date based on policy
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="companyId">Company ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UpdatePasswordExpirationAsync(
        Guid userId,
        Guid companyId,
        CancellationToken cancellationToken = default);

    // ========== Account Lockout ==========

    /// <summary>
    /// Check if user account is locked
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if locked, false otherwise</returns>
    Task<bool> IsAccountLockedAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increment failed login attempt count
    /// Locks account if max attempts exceeded
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="companyId">Company ID</param>
    /// <param name="ipAddress">IP address of the attempt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if account was locked, false otherwise</returns>
    Task<bool> IncrementFailedLoginAttemptAsync(
        Guid userId,
        Guid companyId,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset failed login attempt count (called on successful login)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task ResetFailedLoginAttemptsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlock a locked user account
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="unlockedBy">Admin who unlocked the account</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task UnlockAccountAsync(
        Guid userId,
        Guid unlockedBy,
        CancellationToken cancellationToken = default);

    // ========== Password Operations (Admin) ==========

    /// <summary>
    /// Set a user's password (admin operation)
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="password">New password</param>
    /// <param name="setBy">Admin user ID</param>
    /// <param name="mustChangeOnNextLogin">Force password change on next login</param>
    /// <param name="sendEmail">Send password to user via email</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with success/failure</returns>
    Task<Result> SetUserPasswordAsync(
        Guid userId,
        string password,
        Guid setBy,
        bool mustChangeOnNextLogin = true,
        bool sendEmail = false,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset a user's password with auto-generated password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="resetBy">Admin user ID</param>
    /// <param name="sendEmail">Send new password to user via email</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with generated password</returns>
    Task<Result<string>> ResetUserPasswordAsync(
        Guid userId,
        Guid resetBy,
        bool sendEmail = true,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    // ========== Password Operations (User) ==========

    /// <summary>
    /// Change user's own password
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="currentPassword">Current password for verification</param>
    /// <param name="newPassword">New password</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result with success/failure</returns>
    Task<Result> ChangeUserPasswordAsync(
        Guid userId,
        string currentPassword,
        string newPassword,
        string? ipAddress = null,
        CancellationToken cancellationToken = default);

    // ========== Audit Logging ==========

    /// <summary>
    /// Log a password-related action
    /// </summary>
    /// <param name="userId">User affected</param>
    /// <param name="action">Action performed</param>
    /// <param name="performedBy">Who performed the action</param>
    /// <param name="success">Whether action succeeded</param>
    /// <param name="details">Additional details</param>
    /// <param name="ipAddress">IP address</param>
    /// <param name="userAgent">User agent string</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task LogPasswordActionAsync(
        Guid userId,
        PasswordAction action,
        Guid? performedBy,
        bool success,
        string? details = null,
        string? ipAddress = null,
        string? userAgent = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of password complexity validation
/// </summary>
public class PasswordValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();

    public static PasswordValidationResult Success() => new() { IsValid = true };
    public static PasswordValidationResult Failure(params string[] errors) =>
        new() { IsValid = false, Errors = errors.ToList() };
}

/// <summary>
/// Result of password strength calculation
/// </summary>
public class PasswordStrengthResult
{
    public int Score { get; set; } // 0-100
    public PasswordStrengthCategory Category { get; set; }
    public string CategoryDisplay => Category.ToString();
    public string ColorCode => Category switch
    {
        PasswordStrengthCategory.VeryWeak => "#dc3545", // Red
        PasswordStrengthCategory.Weak => "#fd7e14", // Orange
        PasswordStrengthCategory.Fair => "#ffc107", // Yellow
        PasswordStrengthCategory.Good => "#20c997", // Teal
        PasswordStrengthCategory.Strong => "#28a745", // Green
        PasswordStrengthCategory.VeryStrong => "#007bff", // Blue
        _ => "#6c757d" // Gray
    };
}

/// <summary>
/// Password strength categories
/// </summary>
public enum PasswordStrengthCategory
{
    VeryWeak = 0,
    Weak = 1,
    Fair = 2,
    Good = 3,
    Strong = 4,
    VeryStrong = 5
}
