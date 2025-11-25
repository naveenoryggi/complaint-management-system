using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Net;
using System.Net.Mail;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Email service implementation using SMTP with OAuth 2.0 support
/// Supports both Basic Authentication and OAuth 2.0 (Microsoft 365, Gmail, etc.)
/// </summary>
public class EmailService : IEmailService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailOAuthService _oauthService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEncryptionService _encryptionService;

    public EmailService(
        ComplaintDbContext context,
        ILogger<EmailService> logger,
        EmailOAuthService oauthService,
        IUnitOfWork unitOfWork,
        IEncryptionService encryptionService)
    {
        _context = context;
        _logger = logger;
        _oauthService = oauthService;
        _unitOfWork = unitOfWork;
        _encryptionService = encryptionService;
    }

    public async Task<Result> SendEmailAsync(
        string to,
        string subject,
        string body,
        bool isHtml = true,
        string? from = null,
        string? fromName = null,
        Guid? emailServerSettingsId = null,
        CancellationToken cancellationToken = default)
    {
        return await SendEmailAsync(
            new[] { to },
            subject,
            body,
            isHtml,
            from,
            fromName,
            emailServerSettingsId,
            cancellationToken);
    }

    public async Task<Result> SendEmailAsync(
        IEnumerable<string> toAddresses,
        string subject,
        string body,
        bool isHtml = true,
        string? from = null,
        string? fromName = null,
        Guid? emailServerSettingsId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get email server settings (exclude soft-deleted)
            var settings = emailServerSettingsId.HasValue
                ? await _context.EmailServerSettings.FirstOrDefaultAsync(
                    s => s.Id == emailServerSettingsId.Value && s.IsActive && !s.IsDeleted,
                    cancellationToken)
                : await _context.EmailServerSettings.FirstOrDefaultAsync(
                    s => s.IsDefault && s.IsActive && !s.IsDeleted,
                    cancellationToken);

            if (settings == null)
            {
                return Result.Failure("No active email server settings found");
            }

            // Create MimeMessage (MailKit)
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                fromName ?? settings.FromName ?? string.Empty,
                from ?? settings.FromEmail));

            // Add recipients
            var validRecipients = toAddresses.Where(addr => !string.IsNullOrWhiteSpace(addr)).ToList();
            if (validRecipients.Count == 0)
            {
                return Result.Failure("No valid recipient email addresses provided");
            }

            foreach (var toAddress in validRecipients)
            {
                message.To.Add(MailboxAddress.Parse(toAddress));
            }

            // Set reply-to if configured
            if (!string.IsNullOrEmpty(settings.ReplyToEmail))
            {
                message.ReplyTo.Add(MailboxAddress.Parse(settings.ReplyToEmail));
            }

            message.Subject = subject;

            // Create body
            var builder = new BodyBuilder();
            if (isHtml)
            {
                builder.HtmlBody = body;
            }
            else
            {
                builder.TextBody = body;
            }
            message.Body = builder.ToMessageBody();

            // Send using MailKit SMTP client
            using var client = new MailKit.Net.Smtp.SmtpClient();

            // Determine the correct SecureSocketOptions based on port and SSL setting
            var secureSocketOptions = GetSecureSocketOptions(settings.Port, settings.UseSsl);

            // Connect to SMTP server
            await client.ConnectAsync(settings.Host, settings.Port, secureSocketOptions, cancellationToken);

            // Authenticate using OAuth2 or basic auth
            await AuthenticateSmtpClientAsync(client, settings, cancellationToken);

            // Send message
            await client.SendAsync(message, cancellationToken);

            // Disconnect
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation(
                "Email sent successfully to {Recipients} using server {Server} ({AuthType})",
                string.Join(", ", validRecipients),
                settings.Name,
                settings.AuthenticationType == EmailAuthenticationType.OAuth2 ? "OAuth2" : "Basic");

            return Result.Success();
        }
        catch (MailKit.Net.Smtp.SmtpCommandException ex)
        {
            _logger.LogError(ex, "SMTP command error while sending email to {Recipients}: {StatusCode} - {Message}",
                string.Join(", ", toAddresses), ex.StatusCode, ex.Message);
            return Result.Failure($"SMTP error: {ex.Message}");
        }
        catch (MailKit.Net.Smtp.SmtpProtocolException ex)
        {
            _logger.LogError(ex, "SMTP protocol error while sending email to {Recipients}",
                string.Join(", ", toAddresses));
            return Result.Failure($"SMTP protocol error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {Recipients}", string.Join(", ", toAddresses));
            return Result.Failure($"Failed to send email: {ex.Message}");
        }
    }

    /// <summary>
    /// Authenticates SMTP client using OAuth2 or basic authentication
    /// Automatically refreshes OAuth token if expired
    /// </summary>
    private async Task AuthenticateSmtpClientAsync(
        MailKit.Net.Smtp.SmtpClient client,
        EmailServerSettings settings,
        CancellationToken cancellationToken)
    {
        if (settings.AuthenticationType == EmailAuthenticationType.OAuth2)
        {
            // Check if token needs refresh
            if (_oauthService.TokenNeedsRefresh(settings.OAuthTokenExpiresAt))
            {
                if (string.IsNullOrEmpty(settings.Username))
                {
                    throw new UnauthorizedAccessException("Email address is missing. Please complete email configuration.");
                }

                if (string.IsNullOrEmpty(settings.OAuthRefreshToken))
                {
                    throw new UnauthorizedAccessException("OAuth refresh token is missing. Please re-authorize the email configuration.");
                }

                _logger.LogInformation("Refreshing OAuth access token for email server settings {SettingsId}", settings.Id);

                // Create a temporary EmailConfiguration for token refresh
                var tempConfig = new ComplaintManagement.Domain.Entities.Communication.EmailConfiguration
                {
                    OAuthClientId = settings.OAuthClientId,
                    OAuthClientSecret = settings.OAuthClientSecret,
                    OAuthTenantId = settings.OAuthTenantId,
                    OAuthRefreshToken = settings.OAuthRefreshToken,
                    ImapHost = settings.Host // Used to determine provider (Office365 vs Gmail)
                };

                // Refresh the access token
                var tokenResult = await _oauthService.RefreshAccessTokenAsync(tempConfig);

                // Update settings with new tokens
                settings.OAuthAccessToken = tokenResult.AccessToken;
                if (!string.IsNullOrEmpty(tokenResult.RefreshToken))
                {
                    settings.OAuthRefreshToken = tokenResult.RefreshToken;
                }
                settings.OAuthTokenExpiresAt = tokenResult.ExpiresOn;

                // Save updated tokens
                _context.EmailServerSettings.Update(settings);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Refreshed OAuth access token for email server settings {SettingsId}, new expiry: {ExpiresAt}",
                    settings.Id, settings.OAuthTokenExpiresAt);
            }

            // Authenticate with OAuth2
            var oauth2 = new SaslMechanismOAuth2(settings.Username, settings.OAuthAccessToken);
            await client.AuthenticateAsync(oauth2, cancellationToken);
        }
        else
        {
            // Basic authentication - only authenticate if both username and password are provided
            if (!string.IsNullOrEmpty(settings.Username) && !string.IsNullOrEmpty(settings.Password))
            {
                // SECURITY: Decrypt password before using with SMTP
                string decryptedPassword;
                try
                {
                    decryptedPassword = _encryptionService.DecryptPassword(settings.Password);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to decrypt SMTP password for settings {SettingsId}", settings.Id);
                    throw new InvalidOperationException("Failed to decrypt email password. Please update your email settings.", ex);
                }

                await client.AuthenticateAsync(settings.Username, decryptedPassword, cancellationToken);
            }
            else
            {
                // No authentication required (some SMTP servers allow unauthenticated sending)
                _logger.LogInformation("No authentication credentials provided for SMTP server {SettingsId}. Proceeding without authentication.", settings.Id);
            }
        }
    }

    public async Task<Result> TestEmailServerAsync(
        Guid emailServerSettingsId,
        string testRecipient,
        CancellationToken cancellationToken = default)
    {
        var result = await SendEmailAsync(
            testRecipient,
            "Test Email - Complaint Management System",
            "<h2>Email Server Test</h2><p>This is a test email from the Complaint Management System.</p><p>If you received this email, the email server is configured correctly.</p>",
            isHtml: true,
            emailServerSettingsId: emailServerSettingsId,
            cancellationToken: cancellationToken);

        if (result.IsSuccess)
        {
            // Update last tested timestamp
            var settings = await _context.EmailServerSettings.FindAsync(emailServerSettingsId);
            if (settings != null)
            {
                settings.LastTestedAt = DateTime.UtcNow;
                settings.TestNotes = "Test successful";
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        return result;
    }

    /// <summary>
    /// Determines the correct SecureSocketOptions based on port and SSL setting
    /// </summary>
    /// <param name="port">SMTP port number</param>
    /// <param name="useSsl">Whether SSL/TLS is enabled</param>
    /// <returns>The appropriate SecureSocketOptions</returns>
    private static SecureSocketOptions GetSecureSocketOptions(int port, bool useSsl)
    {
        // If SSL is disabled, use None (plain-text connection)
        if (!useSsl)
        {
            return SecureSocketOptions.None;
        }

        // Port 465: Implicit SSL/TLS (SSL on connect)
        if (port == 465)
        {
            return SecureSocketOptions.SslOnConnect;
        }

        // Port 587: STARTTLS (upgrade to TLS after connecting)
        // Port 25: STARTTLS (legacy, but some servers still use it)
        if (port == 587 || port == 25)
        {
            return SecureSocketOptions.StartTls;
        }

        // For any other port with SSL enabled, use STARTTLS as the safer default
        return SecureSocketOptions.StartTlsWhenAvailable;
    }
}
