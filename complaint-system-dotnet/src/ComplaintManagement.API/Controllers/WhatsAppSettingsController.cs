using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/communication/whatsapp-settings")]
[Authorize]
public class WhatsAppSettingsController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<WhatsAppSettingsController> _logger;

    public WhatsAppSettingsController(
        ComplaintDbContext context,
        ILogger<WhatsAppSettingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all WhatsApp settings
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.WhatsAppSettings.AsQueryable();
            if (!includeInactive) query = query.Where(s => s.IsActive);

            var settings = await query.OrderByDescending(s => s.IsDefault).ThenBy(s => s.Name).ToListAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WhatsApp settings");
            return StatusCode(500, new { message = "Failed to retrieve WhatsApp settings" });
        }
    }

    /// <summary>
    /// Get WhatsApp setting by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var setting = await _context.WhatsAppSettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "WhatsApp setting not found" });

            return Ok(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving WhatsApp setting {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve WhatsApp setting" });
        }
    }

    /// <summary>
    /// Create new WhatsApp setting
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] WhatsAppSettings setting)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Custom validation for nullable required fields
            if (string.IsNullOrWhiteSpace(setting.BusinessAccountId))
            {
                return BadRequest(new { message = "Business Account ID is required" });
            }

            if (string.IsNullOrWhiteSpace(setting.PhoneNumberId))
            {
                return BadRequest(new { message = "Phone Number ID is required" });
            }

            // If this is set as default, unset other defaults
            if (setting.IsDefault)
            {
                var existingDefaults = await _context.WhatsAppSettings.Where(s => s.IsDefault && !s.IsDeleted).ToListAsync();
                existingDefaults.ForEach(s => s.IsDefault = false);
            }

            setting.Id = Guid.NewGuid();
            setting.CreatedAt = DateTime.UtcNow;
            _context.WhatsAppSettings.Add(setting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = setting.Id }, setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating WhatsApp setting");
            return StatusCode(500, new { message = "Failed to create WhatsApp setting" });
        }
    }

    /// <summary>
    /// Update existing WhatsApp setting
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] WhatsAppSettings setting)
    {
        try
        {
            if (id != setting.Id) return BadRequest(new { message = "ID mismatch" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.WhatsAppSettings.FindAsync(id);
            if (existing == null || existing.IsDeleted)
                return NotFound(new { message = "WhatsApp setting not found" });

            // If this is being set as default, unset other defaults
            if (setting.IsDefault && !existing.IsDefault)
            {
                var otherDefaults = await _context.WhatsAppSettings
                    .Where(s => s.IsDefault && s.Id != id && !s.IsDeleted).ToListAsync();
                otherDefaults.ForEach(s => s.IsDefault = false);
            }

            existing.Name = setting.Name;
            existing.Provider = setting.Provider;
            existing.ApiUrl = setting.ApiUrl;
            existing.BusinessAccountId = setting.BusinessAccountId;
            existing.PhoneNumberId = setting.PhoneNumberId;
            existing.AccessToken = setting.AccessToken;
            existing.WebhookToken = setting.WebhookToken;
            existing.FromNumber = setting.FromNumber;
            existing.BusinessName = setting.BusinessName;
            existing.IsDefault = setting.IsDefault;
            existing.IsActive = setting.IsActive;
            existing.MaxMessagesPerHour = setting.MaxMessagesPerHour;
            existing.TimeoutSeconds = setting.TimeoutSeconds;
            existing.AdditionalConfig = setting.AdditionalConfig;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating WhatsApp setting {Id}", id);
            return StatusCode(500, new { message = "Failed to update WhatsApp setting" });
        }
    }

    /// <summary>
    /// Delete WhatsApp setting (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var setting = await _context.WhatsAppSettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "WhatsApp setting not found" });

            if (setting.IsDefault)
                return BadRequest(new { message = "Cannot delete the default WhatsApp setting" });

            setting.IsDeleted = true;
            setting.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "WhatsApp setting deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting WhatsApp setting {Id}", id);
            return StatusCode(500, new { message = "Failed to delete WhatsApp setting" });
        }
    }

    /// <summary>
    /// Test WhatsApp API connection
    /// </summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestConnection(Guid id, [FromBody] TestWhatsAppRequest request)
    {
        try
        {
            var setting = await _context.WhatsAppSettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "WhatsApp setting not found" });

            // TODO: Implement WhatsApp service and test sending
            // For now, just return success if settings exist
            return Ok(new
            {
                success = true,
                message = "WhatsApp configuration validated. Actual sending requires WhatsApp service implementation.",
                provider = setting.Provider,
                businessName = setting.BusinessName,
                fromNumber = setting.FromNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing WhatsApp {Id}", id);
            return StatusCode(500, new { message = "Failed to test WhatsApp connection" });
        }
    }
}

public class TestWhatsAppRequest
{
    public string TestPhoneNumber { get; set; } = string.Empty;
    public string TestMessage { get; set; } = "Test message from Complaint Management System";
}
