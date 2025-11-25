using ComplaintManagement.API.Models;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/email-settings")]
[Authorize]
public class EmailServerSettingsController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IEncryptionService _encryptionService;
    private readonly ILogger<EmailServerSettingsController> _logger;

    public EmailServerSettingsController(
        ComplaintDbContext context,
        IEmailService emailService,
        IEncryptionService encryptionService,
        ILogger<EmailServerSettingsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _encryptionService = encryptionService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.EmailServerSettings.AsQueryable();
            if (!includeInactive) query = query.Where(s => s.IsActive);

            var settings = await query.OrderByDescending(s => s.IsDefault).ThenBy(s => s.Name).ToListAsync();
            return Ok(ApiResponse<List<EmailServerSettings>>.Success(settings, "Email server settings retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email server settings");
            return StatusCode(500, ApiResponse<List<EmailServerSettings>>.Failure("Failed to retrieve email server settings"));
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var setting = await _context.EmailServerSettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "Email server setting not found" });

            return Ok(ApiResponse<EmailServerSettings>.Success(setting, "Email server settings retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving email server setting {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve email server setting" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] EmailServerSettings setting)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (setting.IsDefault)
            {
                var existingDefaults = await _context.EmailServerSettings.Where(s => s.IsDefault && !s.IsDeleted).ToListAsync();
                existingDefaults.ForEach(s => s.IsDefault = false);
            }

            // SECURITY: Encrypt password before storing in database
            if (!string.IsNullOrEmpty(setting.Password))
            {
                setting.Password = _encryptionService.EncryptPassword(setting.Password);
                _logger.LogInformation("Password encrypted for new email server setting");
            }

            // SECURITY: Encrypt OAuth client secret if using OAuth2
            if (setting.AuthenticationType == Domain.Enums.EmailAuthenticationType.OAuth2 &&
                !string.IsNullOrEmpty(setting.OAuthClientSecret))
            {
                setting.OAuthClientSecret = _encryptionService.EncryptPassword(setting.OAuthClientSecret);
                _logger.LogInformation("OAuth client secret encrypted for new email server setting");
            }

            setting.Id = Guid.NewGuid();
            setting.CreatedAt = DateTime.UtcNow;
            _context.EmailServerSettings.Add(setting);
            await _context.SaveChangesAsync();

            var response = ApiResponse<EmailServerSettings>.Success(setting, "Email server setting created successfully");
            return CreatedAtAction(nameof(GetById), new { id = setting.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating email server setting");
            return StatusCode(500, ApiResponse<EmailServerSettings>.Failure("Failed to create email server setting"));
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] EmailServerSettings setting)
    {
        try
        {
            if (id != setting.Id) return BadRequest(new { message = "ID mismatch" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.EmailServerSettings.FindAsync(id);
            if (existing == null || existing.IsDeleted)
                return NotFound(new { message = "Email server setting not found" });

            if (setting.IsDefault && !existing.IsDefault)
            {
                var otherDefaults = await _context.EmailServerSettings
                    .Where(s => s.IsDefault && s.Id != id && !s.IsDeleted).ToListAsync();
                otherDefaults.ForEach(s => s.IsDefault = false);
            }

            existing.Name = setting.Name;
            existing.Host = setting.Host;
            existing.Port = setting.Port;
            existing.UseSsl = setting.UseSsl;
            existing.Username = setting.Username;
            existing.AuthenticationType = setting.AuthenticationType; // FIX: Update authentication type

            // SECURITY: Only encrypt and update password if a new one is provided
            // If password is empty/null, keep existing encrypted password
            // For OAuth, clear the password field
            if (setting.AuthenticationType == Domain.Enums.EmailAuthenticationType.OAuth2)
            {
                existing.Password = null; // Clear password for OAuth
                _logger.LogInformation("Switched to OAuth2 authentication, cleared password for email server setting {Id}", id);
            }
            else if (!string.IsNullOrEmpty(setting.Password))
            {
                existing.Password = _encryptionService.EncryptPassword(setting.Password);
                _logger.LogInformation("Password updated and encrypted for email server setting {Id}", id);
            }

            existing.FromEmail = setting.FromEmail;
            existing.FromName = setting.FromName;
            existing.ReplyToEmail = setting.ReplyToEmail;
            existing.IsDefault = setting.IsDefault;
            existing.IsActive = setting.IsActive;
            existing.TimeoutSeconds = setting.TimeoutSeconds;

            // SECURITY: Update OAuth fields if using OAuth2
            if (setting.AuthenticationType == Domain.Enums.EmailAuthenticationType.OAuth2)
            {
                existing.OAuthClientId = setting.OAuthClientId;
                existing.OAuthTenantId = setting.OAuthTenantId;
                existing.OAuthTokenRefreshIntervalMinutes = setting.OAuthTokenRefreshIntervalMinutes;

                // Only update secret if provided
                if (!string.IsNullOrEmpty(setting.OAuthClientSecret))
                {
                    existing.OAuthClientSecret = _encryptionService.EncryptPassword(setting.OAuthClientSecret);
                    _logger.LogInformation("OAuth client secret updated and encrypted for email server setting {Id}", id);
                }
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ApiResponse<EmailServerSettings>.Success(existing, "Email server settings updated successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email server setting {Id}", id);
            return StatusCode(500, new { message = "Failed to update email server setting" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var setting = await _context.EmailServerSettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(ApiResponse<bool>.Failure("Email server setting not found"));

            if (setting.IsDefault)
                return BadRequest(ApiResponse<bool>.Failure("Cannot delete the default email server setting"));

            setting.IsDeleted = true;
            setting.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Email server setting {Id} deleted successfully", id);
            return Ok(ApiResponse<bool>.Success(true, "Email server setting deleted successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting email server setting {Id}", id);
            return StatusCode(500, ApiResponse<bool>.Failure("Failed to delete email server setting"));
        }
    }

    /// <summary>
    /// Test email server connection and send test email
    /// SECURITY: Rate limited to 5 requests per 10 minutes per IP to prevent abuse
    /// </summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestConnection(Guid id, [FromBody] TestEmailRequest request)
    {
        try
        {
            // SECURITY: Log test email attempts for monitoring
            _logger.LogWarning("Email server test initiated for settings {Id} to recipient {Recipient}", id, request.TestRecipient);

            var result = await _emailService.TestEmailServerAsync(id, request.TestRecipient);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing email server {Id}", id);
            return StatusCode(500, new { message = "Failed to test email server" });
        }
    }

    /// <summary>
    /// Set an email server setting as the default
    /// Automatically unsets any other default server
    /// </summary>
    [HttpPost("{id}/set-default")]
    public async Task<IActionResult> SetAsDefault(Guid id)
    {
        try
        {
            var setting = await _context.EmailServerSettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(ApiResponse<EmailServerSettings>.Failure("Email server setting not found"));

            if (!setting.IsActive)
                return BadRequest(ApiResponse<EmailServerSettings>.Failure("Cannot set inactive server as default"));

            // Unset all other defaults
            var otherDefaults = await _context.EmailServerSettings
                .Where(s => s.IsDefault && s.Id != id && !s.IsDeleted)
                .ToListAsync();

            foreach (var other in otherDefaults)
            {
                other.IsDefault = false;
                _logger.LogInformation("Removed default status from email server setting {Id}", other.Id);
            }

            // Set this one as default
            setting.IsDefault = true;
            setting.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Email server setting {Id} set as default", id);
            return Ok(ApiResponse<EmailServerSettings>.Success(setting, "Email server setting set as default successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting email server {Id} as default", id);
            return StatusCode(500, ApiResponse<EmailServerSettings>.Failure("Failed to set email server as default"));
        }
    }
}

public class TestEmailRequest
{
    public string TestRecipient { get; set; } = string.Empty;
}
