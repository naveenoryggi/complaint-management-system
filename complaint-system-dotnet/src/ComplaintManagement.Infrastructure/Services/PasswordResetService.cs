using ComplaintManagement.Application.DTOs.Auth;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Auth;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for handling self-service password reset functionality
/// Implements secure token-based password recovery with rate limiting
/// </summary>
public class PasswordResetService : IPasswordResetService
{
    private readonly ComplaintDbContext _dbContext;
    private readonly IPasswordService _passwordService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PasswordResetService> _logger;

    // Configuration values
    private readonly int _tokenExpirationHours;
    private readonly int _maxRequestsPerHour;
    private readonly string _resetLinkBaseUrl;

    public PasswordResetService(
        ComplaintDbContext dbContext,
        IPasswordService passwordService,
        IEmailService emailService,
        IConfiguration configuration,
        ILogger<PasswordResetService> logger)
    {
        _dbContext = dbContext;
        _passwordService = passwordService;
        _emailService = emailService;
        _configuration = configuration;
        _logger = logger;

        // Load configuration
        _tokenExpirationHours = _configuration.GetValue<int>("PasswordReset:TokenExpirationHours", 24);
        _maxRequestsPerHour = _configuration.GetValue<int>("PasswordReset:MaxRequestsPerHour", 3);
        _resetLinkBaseUrl = _configuration.GetValue<string>("PasswordReset:ResetLinkBaseUrl", "http://localhost:4200/reset-password") ?? "http://localhost:4200/reset-password";
    }

    public async Task<PasswordResetRequestResult> RequestPasswordResetAsync(
        string email,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            _logger.LogInformation("Password reset requested for email: {Email}", email);

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                return new PasswordResetRequestResult
                {
                    IsSuccess = true, // Don't reveal email doesn't exist
                    Message = "If an account with that email exists, a password reset link has been sent."
                };
            }

            // Check rate limiting
            var rateLimitOk = await CheckRateLimitAsync(email);
            if (!rateLimitOk)
            {
                _logger.LogWarning("Rate limit exceeded for email: {Email}, IP: {IP}", email, ipAddress);
                return new PasswordResetRequestResult
                {
                    IsSuccess = false,
                    Message = "Too many password reset requests. Please try again later.",
                    RateLimitExceeded = true,
                    MinutesUntilNextRequest = 60
                };
            }

            // Find user by email
            var user = await _dbContext.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

            // Always return success message for security (don't reveal if email exists)
            var genericMessage = "If an account with that email exists, a password reset link has been sent.";

            if (user == null)
            {
                _logger.LogInformation("No active user found with email: {Email}", email);
                return new PasswordResetRequestResult
                {
                    IsSuccess = true,
                    Message = genericMessage,
                    InternalError = "User not found"
                };
            }

            // Check if user uses external authentication
            if (user.AuthenticationProviderType != AuthenticationProviderType.Local)
            {
                _logger.LogWarning("User {Email} uses external authentication: {Provider}",
                    email, user.AuthenticationProviderType);
                return new PasswordResetRequestResult
                {
                    IsSuccess = true,
                    Message = genericMessage,
                    InternalError = "User uses external authentication"
                };
            }

            // Generate secure token
            var token = Guid.NewGuid().ToString("N"); // 32 character hex string
            var expiresAt = DateTime.UtcNow.AddHours(_tokenExpirationHours);

            // Create password reset token
            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                ExpiresAt = expiresAt,
                IsUsed = false,
                RequestIpAddress = ipAddress,
                RequestUserAgent = userAgent,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = user.Id
            };

            _dbContext.PasswordResetTokens.Add(resetToken);

            // Create audit log
            await _passwordService.LogPasswordActionAsync(
                user.Id,
                PasswordAction.PasswordResetRequested,
                user.Id,
                true,
                $"Password reset requested for {user.Email}",
                ipAddress,
                userAgent);

            await _dbContext.SaveChangesAsync();

            // Send password reset email
            var resetLink = $"{_resetLinkBaseUrl}?token={token}";
            await SendPasswordResetEmailAsync(user.Email, user.FirstName, resetLink, expiresAt);

            _logger.LogInformation("Password reset token created for user {UserId}, expires at {ExpiresAt}",
                user.Id, expiresAt);

            return new PasswordResetRequestResult
            {
                IsSuccess = true,
                Message = genericMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting password reset for email: {Email}", email);
            return new PasswordResetRequestResult
            {
                IsSuccess = true, // Still return success for security
                Message = "If an account with that email exists, a password reset link has been sent.",
                InternalError = ex.Message
            };
        }
    }

    public async Task<PasswordResetTokenValidationResult> ValidateResetTokenAsync(string token)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                return new PasswordResetTokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid token"
                };
            }

            var resetToken = await _dbContext.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken == null)
            {
                _logger.LogWarning("Password reset token not found: {Token}", token);
                return new PasswordResetTokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "Invalid or expired token"
                };
            }

            if (resetToken.IsUsed)
            {
                _logger.LogWarning("Password reset token already used: {Token}", token);
                return new PasswordResetTokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "This password reset link has already been used"
                };
            }

            if (DateTime.UtcNow > resetToken.ExpiresAt)
            {
                _logger.LogWarning("Password reset token expired: {Token}, expired at {ExpiresAt}",
                    token, resetToken.ExpiresAt);
                return new PasswordResetTokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "This password reset link has expired. Please request a new one."
                };
            }

            return new PasswordResetTokenValidationResult
            {
                IsValid = true,
                Email = resetToken.Email,
                UserId = resetToken.UserId,
                ExpiresAt = resetToken.ExpiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password reset token");
            return new PasswordResetTokenValidationResult
            {
                IsValid = false,
                ErrorMessage = "An error occurred. Please try again."
            };
        }
    }

    public async Task<PasswordResetResult> ResetPasswordAsync(
        string token,
        string newPassword,
        string? ipAddress = null,
        string? userAgent = null)
    {
        try
        {
            // Validate token
            var validationResult = await ValidateResetTokenAsync(token);
            if (!validationResult.IsValid)
            {
                return new PasswordResetResult
                {
                    IsSuccess = false,
                    Message = validationResult.ErrorMessage ?? "Invalid token",
                    TokenExpired = validationResult.ErrorMessage?.Contains("expired") ?? false,
                    TokenAlreadyUsed = validationResult.ErrorMessage?.Contains("already been used") ?? false
                };
            }

            var resetToken = await _dbContext.PasswordResetTokens
                .Include(t => t.User)
                .ThenInclude(u => u!.Company)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (resetToken?.User == null)
            {
                return new PasswordResetResult
                {
                    IsSuccess = false,
                    Message = "Invalid token"
                };
            }

            var user = resetToken.User;

            // Validate password against company policy
            var validationResultTask = await _passwordService.ValidatePasswordComplexityAsync(
                newPassword,
                user.CompanyId);

            if (!validationResultTask.IsValid)
            {
                _logger.LogWarning("Password validation failed for user {UserId}: {Errors}",
                    user.Id, string.Join(", ", validationResultTask.Errors));
                return new PasswordResetResult
                {
                    IsSuccess = false,
                    Message = "Password does not meet policy requirements",
                    ValidationErrors = validationResultTask.Errors
                };
            }

            // Check password history (prevent reuse)
            var isReused = await _passwordService.IsPasswordInHistoryAsync(user.Id, newPassword);
            if (isReused)
            {
                _logger.LogWarning("User {UserId} attempted to reuse a previous password", user.Id);
                return new PasswordResetResult
                {
                    IsSuccess = false,
                    Message = "This password has been used before. Please choose a different password.",
                    ValidationErrors = new List<string> { "Password has been used previously" }
                };
            }

            // Hash new password
            var passwordHash = _passwordService.HashPassword(newPassword);

            // Update user password
            user.PasswordHash = passwordHash;
            user.PasswordChangedAt = DateTime.UtcNow;
            user.PasswordChangedBy = user.Id; // Self-service reset
            user.MustChangePasswordOnNextLogin = false;
            user.FailedLoginAttempts = 0;
            user.AccountLockedUntil = null;

            // Update password expiration
            await _passwordService.UpdatePasswordExpirationAsync(user.Id, user.CompanyId);

            // Mark token as used
            resetToken.MarkAsUsed(ipAddress, userAgent);

            // Add to password history
            await _passwordService.AddPasswordToHistoryAsync(
                user.Id,
                passwordHash,
                user.Id,
                ipAddress);

            // Create audit log
            await _passwordService.LogPasswordActionAsync(
                user.Id,
                PasswordAction.PasswordResetCompleted,
                user.Id,
                true,
                $"Password reset completed for {user.Email}",
                ipAddress,
                userAgent);

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Password reset completed for user {UserId}", user.Id);

            // Send confirmation email
            await SendPasswordResetConfirmationEmailAsync(user.Email, user.FirstName);

            return new PasswordResetResult
            {
                IsSuccess = true,
                Message = "Your password has been reset successfully. You can now log in with your new password."
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return new PasswordResetResult
            {
                IsSuccess = false,
                Message = "An error occurred while resetting your password. Please try again."
            };
        }
    }

    public async Task<int> CleanupExpiredTokensAsync()
    {
        try
        {
            var expiredTokens = await _dbContext.PasswordResetTokens
                .Where(t => t.ExpiresAt < DateTime.UtcNow && !t.IsUsed)
                .ToListAsync();

            if (expiredTokens.Any())
            {
                _dbContext.PasswordResetTokens.RemoveRange(expiredTokens);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("Cleaned up {Count} expired password reset tokens", expiredTokens.Count);
            }

            return expiredTokens.Count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up expired password reset tokens");
            return 0;
        }
    }

    public async Task<bool> CheckRateLimitAsync(string email)
    {
        try
        {
            var oneHourAgo = DateTime.UtcNow.AddHours(-1);

            var recentRequestCount = await _dbContext.PasswordResetTokens
                .Where(t => t.Email.ToLower() == email.ToLower() && t.CreatedAt >= oneHourAgo)
                .CountAsync();

            return recentRequestCount < _maxRequestsPerHour;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking rate limit for email: {Email}", email);
            // Allow request on error (fail open)
            return true;
        }
    }

    #region Private Helper Methods

    private async Task SendPasswordResetEmailAsync(
        string email,
        string firstName,
        string resetLink,
        DateTime expiresAt)
    {
        try
        {
            var subject = "Password Reset Request";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #333;'>Password Reset Request</h2>
        <p>Hello {firstName},</p>
        <p>We received a request to reset your password. Click the button below to create a new password:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{resetLink}'
               style='background-color: #007bff; color: white; padding: 12px 30px;
                      text-decoration: none; border-radius: 5px; display: inline-block;'>
                Reset Password
            </a>
        </p>
        <p>Or copy and paste this link into your browser:</p>
        <p style='background-color: #f5f5f5; padding: 10px; word-break: break-all;'>{resetLink}</p>
        <p style='color: #666; font-size: 14px;'>
            This link will expire in {_tokenExpirationHours} hours ({expiresAt.ToLocalTime():g}).
        </p>
        <p style='color: #666; font-size: 14px;'>
            If you didn't request a password reset, please ignore this email or contact support if you have concerns.
        </p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
        <p style='color: #999; font-size: 12px;'>
            This is an automated message, please do not reply to this email.
        </p>
    </div>
</body>
</html>";

            await _emailService.SendEmailAsync(email, subject, body);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset email to {Email}", email);
            // Don't throw - we don't want to fail the entire reset process if email fails
        }
    }

    private async Task SendPasswordResetConfirmationEmailAsync(string email, string firstName)
    {
        try
        {
            var subject = "Password Reset Successful";
            var body = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <div style='max-width: 600px; margin: 0 auto; padding: 20px;'>
        <h2 style='color: #28a745;'>Password Reset Successful</h2>
        <p>Hello {firstName},</p>
        <p>Your password has been successfully reset.</p>
        <p>If you did not make this change, please contact your system administrator immediately.</p>
        <hr style='border: none; border-top: 1px solid #ddd; margin: 30px 0;'>
        <p style='color: #999; font-size: 12px;'>
            This is an automated message, please do not reply to this email.
        </p>
    </div>
</body>
</html>";

            await _emailService.SendEmailAsync(email, subject, body);
            _logger.LogInformation("Password reset confirmation email sent to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset confirmation email to {Email}", email);
            // Don't throw
        }
    }

    #endregion
}
