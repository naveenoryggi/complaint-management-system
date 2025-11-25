using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Communication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers;

/// <summary>
/// Controller for managing email ticketing configuration (IMAP/SMTP settings)
/// </summary>
[ApiController]
[Route("api/email-configuration")]
[Authorize]
public class EmailConfigurationController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailTicketingService _emailTicketingService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<EmailConfigurationController> _logger;

    public EmailConfigurationController(
        IUnitOfWork unitOfWork,
        IEmailTicketingService emailTicketingService,
        IEncryptionService encryptionService,
        ILogger<EmailConfigurationController> logger)
    {
        _unitOfWork = unitOfWork;
        _emailTicketingService = emailTicketingService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all email configurations for the user's company
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(Result<IEnumerable<EmailConfiguration>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmailConfigurations()
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to access email configurations without permission");
                return Forbid();
            }

            // Get configurations for this company
            var configs = await _unitOfWork.Repository<EmailConfiguration>()
                .FindAsync(e => e.CompanyId == companyId, CancellationToken.None);

            _logger.LogInformation("Retrieved {Count} email configurations for company {CompanyId}",
                configs.Count(), companyId);

            return Ok(Result<IEnumerable<EmailConfiguration>>.Success(configs));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email configurations");
            return StatusCode(500, Result.Failure("An error occurred while retrieving email configurations"));
        }
    }

    /// <summary>
    /// Get a specific email configuration by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(Result<EmailConfiguration>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetEmailConfiguration(Guid id)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to access email configuration without permission");
                return Forbid();
            }

            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(id);

            if (config == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (config.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to access email configuration from different company");
                return Forbid();
            }

            return Ok(Result<EmailConfiguration>.Success(config));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email configuration {Id}", id);
            return StatusCode(500, Result.Failure("An error occurred while retrieving the email configuration"));
        }
    }

    /// <summary>
    /// Create a new email configuration
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Result<EmailConfiguration>), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateEmailConfiguration([FromBody] CreateEmailConfigurationRequest request)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to create email configuration without permission");
                return Forbid();
            }

            // Map DTO to entity
            var config = new EmailConfiguration
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                FromName = request.FromName,
                FromEmail = request.FromEmail,
                ImapHost = request.ImapHost,
                ImapPort = request.ImapPort,
                ImapUseSsl = request.ImapUseSsl,
                ImapUsername = request.ImapUsername,
                ImapPassword = !string.IsNullOrEmpty(request.ImapPassword)
                    ? _encryptionService.EncryptPassword(request.ImapPassword)
                    : null, // SECURITY: Encrypt IMAP password
                ImapFolder = request.ImapFolder,
                SmtpHost = request.SmtpHost,
                SmtpPort = request.SmtpPort,
                SmtpUseSsl = request.SmtpUseSsl,
                SmtpUsername = request.SmtpUsername,
                SmtpPassword = !string.IsNullOrEmpty(request.SmtpPassword)
                    ? _encryptionService.EncryptPassword(request.SmtpPassword)
                    : null, // SECURITY: Encrypt SMTP password
                PollingIntervalMinutes = request.PollingIntervalMinutes,
                IsEnabled = false, // Start disabled for OAuth (enabled after authorization)
                SendAutoAcknowledgement = request.SendAutoAcknowledgement,
                AutoAcknowledgementTemplateId = request.AutoAcknowledgementTemplateId,
                EnableThreading = request.EnableThreading,
                ThreadTimeoutDays = request.ThreadTimeoutDays,
                MaxAttachmentSizeBytes = request.MaxAttachmentSizeBytes,
                AllowedAttachmentExtensions = request.AllowedAttachmentExtensions,
                // OAuth fields
                AuthenticationType = request.AuthenticationType,
                OAuthClientId = request.OAuthClientId,
                OAuthClientSecret = !string.IsNullOrEmpty(request.OAuthClientSecret)
                    ? _encryptionService.EncryptPassword(request.OAuthClientSecret)
                    : null, // SECURITY: Encrypt OAuth client secret
                OAuthTenantId = request.OAuthTenantId,
                OAuthTokenRefreshIntervalMinutes = request.OAuthTokenRefreshIntervalMinutes,
                CreatedAt = DateTime.UtcNow
            };

            _logger.LogInformation("Created email configuration with encrypted credentials for company {CompanyId}", companyId);

            // Validate configuration
            if (string.IsNullOrWhiteSpace(config.ImapHost) || string.IsNullOrWhiteSpace(config.SmtpHost))
            {
                return BadRequest(Result.Failure("IMAP and SMTP host are required"));
            }

            await _unitOfWork.Repository<EmailConfiguration>().AddAsync(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Created email configuration {ConfigId} for company {CompanyId}",
                config.Id, companyId);

            return CreatedAtAction(nameof(GetEmailConfiguration), new { id = config.Id },
                Result<EmailConfiguration>.Success(config));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email configuration");
            return StatusCode(500, Result.Failure("An error occurred while creating the email configuration"));
        }
    }

    /// <summary>
    /// Update an existing email configuration
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(Result<EmailConfiguration>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateEmailConfiguration(Guid id, [FromBody] EmailConfiguration updatedConfig)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to update email configuration without permission");
                return Forbid();
            }

            var existingConfig = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(id);

            if (existingConfig == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (existingConfig.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to update email configuration from different company");
                return Forbid();
            }

            // CRITICAL: Detect if email address changed - this requires OAuth re-authorization
            var emailAddressChanged = !string.Equals(existingConfig.FromEmail, updatedConfig.FromEmail, StringComparison.OrdinalIgnoreCase);

            if (emailAddressChanged && existingConfig.AuthenticationType == Domain.Enums.EmailAuthenticationType.OAuth2)
            {
                _logger.LogWarning(
                    "Email address changed from '{OldEmail}' to '{NewEmail}' for config {ConfigId}. Clearing OAuth tokens - user must re-authorize.",
                    existingConfig.FromEmail, updatedConfig.FromEmail, id);

                // Clear OAuth tokens - they're for the old email address and won't work for the new one
                existingConfig.OAuthAccessToken = null;
                existingConfig.OAuthRefreshToken = null;
                existingConfig.OAuthTokenExpiresAt = null;

                // Disable the config until user re-authorizes
                existingConfig.IsEnabled = false;
            }

            // Update fields
            existingConfig.ImapHost = updatedConfig.ImapHost;
            existingConfig.ImapPort = updatedConfig.ImapPort;
            existingConfig.ImapUseSsl = updatedConfig.ImapUseSsl;
            existingConfig.ImapUsername = updatedConfig.ImapUsername;

            // SECURITY: Only encrypt and update IMAP password if a new one is provided
            if (!string.IsNullOrEmpty(updatedConfig.ImapPassword))
            {
                existingConfig.ImapPassword = _encryptionService.EncryptPassword(updatedConfig.ImapPassword);
                _logger.LogInformation("IMAP password updated and encrypted for config {ConfigId}", id);
            }

            existingConfig.ImapFolder = updatedConfig.ImapFolder;
            existingConfig.SmtpHost = updatedConfig.SmtpHost;
            existingConfig.SmtpPort = updatedConfig.SmtpPort;
            existingConfig.SmtpUseSsl = updatedConfig.SmtpUseSsl;
            existingConfig.SmtpUsername = updatedConfig.SmtpUsername;

            // SECURITY: Only encrypt and update SMTP password if a new one is provided
            if (!string.IsNullOrEmpty(updatedConfig.SmtpPassword))
            {
                existingConfig.SmtpPassword = _encryptionService.EncryptPassword(updatedConfig.SmtpPassword);
                _logger.LogInformation("SMTP password updated and encrypted for config {ConfigId}", id);
            }

            // SECURITY: Encrypt separate SMTP credentials if using separate account
            if (existingConfig.UseSeparateSmtpAccount)
            {
                if (!string.IsNullOrEmpty(updatedConfig.SmtpSeparatePassword))
                {
                    existingConfig.SmtpSeparatePassword = _encryptionService.EncryptPassword(updatedConfig.SmtpSeparatePassword);
                    _logger.LogInformation("Separate SMTP password updated and encrypted for config {ConfigId}", id);
                }

                if (!string.IsNullOrEmpty(updatedConfig.SmtpSeparateOAuthClientSecret))
                {
                    existingConfig.SmtpSeparateOAuthClientSecret = _encryptionService.EncryptPassword(updatedConfig.SmtpSeparateOAuthClientSecret);
                    _logger.LogInformation("Separate SMTP OAuth secret updated and encrypted for config {ConfigId}", id);
                }
            }

            // SECURITY: Update OAuth client secret if provided
            if (!string.IsNullOrEmpty(updatedConfig.OAuthClientSecret))
            {
                existingConfig.OAuthClientSecret = _encryptionService.EncryptPassword(updatedConfig.OAuthClientSecret);
                _logger.LogInformation("OAuth client secret updated and encrypted for config {ConfigId}", id);
            }
            existingConfig.FromEmail = updatedConfig.FromEmail;
            existingConfig.FromName = updatedConfig.FromName;
            existingConfig.PollingIntervalMinutes = updatedConfig.PollingIntervalMinutes;
            existingConfig.OAuthTokenRefreshIntervalMinutes = updatedConfig.OAuthTokenRefreshIntervalMinutes;

            // Only allow IsEnabled=true if not requiring re-auth
            if (!emailAddressChanged || existingConfig.AuthenticationType != Domain.Enums.EmailAuthenticationType.OAuth2)
            {
                existingConfig.IsEnabled = updatedConfig.IsEnabled;
            }

            existingConfig.SendAutoAcknowledgement = updatedConfig.SendAutoAcknowledgement;
            existingConfig.AutoAcknowledgementTemplateId = updatedConfig.AutoAcknowledgementTemplateId;
            existingConfig.EnableThreading = updatedConfig.EnableThreading;
            existingConfig.ThreadTimeoutDays = updatedConfig.ThreadTimeoutDays;
            existingConfig.MaxAttachmentSizeBytes = updatedConfig.MaxAttachmentSizeBytes;
            existingConfig.AllowedAttachmentExtensions = updatedConfig.AllowedAttachmentExtensions;

            _unitOfWork.Repository<EmailConfiguration>().Update(existingConfig);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Updated email configuration {ConfigId} for company {CompanyId}",
                id, companyId);

            return Ok(Result<EmailConfiguration>.Success(existingConfig));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email configuration {Id}", id);
            return StatusCode(500, Result.Failure("An error occurred while updating the email configuration"));
        }
    }

    /// <summary>
    /// Delete an email configuration
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteEmailConfiguration(Guid id)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to delete email configuration without permission");
                return Forbid();
            }

            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(id);

            if (config == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (config.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to delete email configuration from different company");
                return Forbid();
            }

            _unitOfWork.Repository<EmailConfiguration>().Delete(config);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Deleted email configuration {ConfigId} for company {CompanyId}",
                id, companyId);

            return Ok(Result.Success());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email configuration {Id}", id);
            return StatusCode(500, Result.Failure("An error occurred while deleting the email configuration"));
        }
    }

    /// <summary>
    /// Test IMAP connection for a configuration
    /// SECURITY: Rate limited via AspNetCoreRateLimit middleware - monitor for abuse
    /// </summary>
    [HttpPost("{id}/test-imap")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestImapConnection(Guid id)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to test IMAP connection without permission");
                return Forbid();
            }

            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(id);

            if (config == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (config.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to test IMAP connection for different company");
                return Forbid();
            }

            // SECURITY: Log test attempts for monitoring
            _logger.LogWarning("IMAP connection test initiated for configuration {ConfigId} by company {CompanyId}", id, companyId);

            var result = await _emailTicketingService.TestImapConnectionAsync(config);

            _logger.LogInformation("IMAP connection test for configuration {ConfigId}: {Success}",
                id, result.IsSuccess);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing IMAP connection for configuration {Id}", id);
            return StatusCode(500, Result.Failure("An error occurred while testing the IMAP connection"));
        }
    }

    /// <summary>
    /// Test SMTP connection for a configuration
    /// </summary>
    [HttpPost("{id}/test-smtp")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> TestSmtpConnection(Guid id, [FromBody] TestSmtpRequest request)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to test SMTP connection without permission");
                return Forbid();
            }

            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(id);

            if (config == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (config.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to test SMTP connection for different company");
                return Forbid();
            }

            var result = await _emailTicketingService.TestSmtpConnectionAsync(config, request.TestRecipient);

            _logger.LogInformation("SMTP connection test for configuration {ConfigId}: {Success}",
                id, result.IsSuccess);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMTP connection for configuration {Id}", id);
            return StatusCode(500, Result.Failure("An error occurred while testing the SMTP connection"));
        }
    }

    /// <summary>
    /// Manually trigger email polling for a configuration (useful for testing)
    /// </summary>
    [HttpPost("{id}/poll-now")]
    [ProducesResponseType(typeof(Result<EmailProcessingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> PollNow(Guid id)
    {
        try
        {
            // SECURITY: Get current user's company
            var companyIdClaim = User.FindFirst("CompanyId")?.Value;
            if (string.IsNullOrEmpty(companyIdClaim) || !Guid.TryParse(companyIdClaim, out Guid companyId))
            {
                return Unauthorized(Result.Failure("Company information not found"));
            }

            // SECURITY: Check permissions
            var permissions = User.FindAll("Permission").Select(c => c.Value).ToList();
            if (!permissions.Contains("ManageSettings"))
            {
                _logger.LogWarning("User attempted to trigger email polling without permission");
                return Forbid();
            }

            var config = await _unitOfWork.Repository<EmailConfiguration>().GetByIdAsync(id);

            if (config == null)
            {
                return NotFound(Result.Failure("Email configuration not found"));
            }

            // SECURITY: Verify configuration belongs to user's company
            if (config.CompanyId != companyId)
            {
                _logger.LogWarning("User attempted to trigger email polling for different company");
                return Forbid();
            }

            var result = await _emailTicketingService.FetchAndProcessEmailsAsync(id);

            _logger.LogInformation(
                "Manual email poll for configuration {ConfigId}: Fetched={Fetched}, Created={Created}, Updated={Updated}",
                id, result.Data?.TotalEmailsFetched, result.Data?.NewTicketsCreated, result.Data?.ExistingTicketsUpdated);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering manual email poll for configuration {Id}", id);
            return StatusCode(500, Result.Failure("An error occurred while polling emails"));
        }
    }
}

/// <summary>
/// Request model for testing SMTP connection
/// </summary>
public class TestSmtpRequest
{
    public string TestRecipient { get; set; } = string.Empty;
}
