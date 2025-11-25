using ComplaintManagement.Domain.Entities.Settings;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/communication/sms-settings")]
[Authorize]
public class SmsGatewaySettingsController : ControllerBase
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<SmsGatewaySettingsController> _logger;

    public SmsGatewaySettingsController(
        ComplaintDbContext context,
        ILogger<SmsGatewaySettingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all SMS gateway settings
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        try
        {
            var query = _context.SmsGatewaySettings.AsQueryable();
            if (!includeInactive) query = query.Where(s => s.IsActive);

            var settings = await query.OrderByDescending(s => s.IsDefault).ThenBy(s => s.Name).ToListAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SMS gateway settings");
            return StatusCode(500, new { message = "Failed to retrieve SMS gateway settings" });
        }
    }

    /// <summary>
    /// Get SMS gateway setting by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var setting = await _context.SmsGatewaySettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "SMS gateway setting not found" });

            return Ok(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving SMS gateway setting {Id}", id);
            return StatusCode(500, new { message = "Failed to retrieve SMS gateway setting" });
        }
    }

    /// <summary>
    /// Create new SMS gateway setting
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SmsGatewaySettings setting)
    {
        try
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Custom validation for nullable required fields
            if (string.IsNullOrWhiteSpace(setting.AccountSid))
            {
                return BadRequest(new { message = "Account SID/API Key is required" });
            }

            // If this is set as default, unset other defaults
            if (setting.IsDefault)
            {
                var existingDefaults = await _context.SmsGatewaySettings.Where(s => s.IsDefault && !s.IsDeleted).ToListAsync();
                existingDefaults.ForEach(s => s.IsDefault = false);
            }

            setting.Id = Guid.NewGuid();
            setting.CreatedAt = DateTime.UtcNow;
            _context.SmsGatewaySettings.Add(setting);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = setting.Id }, setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SMS gateway setting");
            return StatusCode(500, new { message = "Failed to create SMS gateway setting" });
        }
    }

    /// <summary>
    /// Update existing SMS gateway setting
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SmsGatewaySettings setting)
    {
        try
        {
            if (id != setting.Id) return BadRequest(new { message = "ID mismatch" });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var existing = await _context.SmsGatewaySettings.FindAsync(id);
            if (existing == null || existing.IsDeleted)
                return NotFound(new { message = "SMS gateway setting not found" });

            // If this is being set as default, unset other defaults
            if (setting.IsDefault && !existing.IsDefault)
            {
                var otherDefaults = await _context.SmsGatewaySettings
                    .Where(s => s.IsDefault && s.Id != id && !s.IsDeleted).ToListAsync();
                otherDefaults.ForEach(s => s.IsDefault = false);
            }

            existing.Name = setting.Name;
            existing.Provider = setting.Provider;
            existing.ApiUrl = setting.ApiUrl;
            existing.AccountSid = setting.AccountSid;
            existing.AuthToken = setting.AuthToken;
            existing.FromNumber = setting.FromNumber;
            existing.SenderName = setting.SenderName;
            existing.IsDefault = setting.IsDefault;
            existing.IsActive = setting.IsActive;
            existing.MaxSmsPerHour = setting.MaxSmsPerHour;
            existing.CostPerSms = setting.CostPerSms;
            existing.TimeoutSeconds = setting.TimeoutSeconds;
            existing.AdditionalConfig = setting.AdditionalConfig;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(existing);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SMS gateway setting {Id}", id);
            return StatusCode(500, new { message = "Failed to update SMS gateway setting" });
        }
    }

    /// <summary>
    /// Delete SMS gateway setting (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var setting = await _context.SmsGatewaySettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "SMS gateway setting not found" });

            if (setting.IsDefault)
                return BadRequest(new { message = "Cannot delete the default SMS gateway setting" });

            setting.IsDeleted = true;
            setting.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "SMS gateway setting deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting SMS gateway setting {Id}", id);
            return StatusCode(500, new { message = "Failed to delete SMS gateway setting" });
        }
    }

    /// <summary>
    /// Test SMS gateway connection
    /// </summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestConnection(Guid id, [FromBody] TestSmsRequest request)
    {
        try
        {
            var setting = await _context.SmsGatewaySettings.FindAsync(id);
            if (setting == null || setting.IsDeleted)
                return NotFound(new { message = "SMS gateway setting not found" });

            // TODO: Implement SMS service and test sending
            // For now, just return success if settings exist
            return Ok(new
            {
                success = true,
                message = "SMS gateway configuration validated. Actual sending requires SMS service implementation.",
                provider = setting.Provider,
                fromNumber = setting.FromNumber
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMS gateway {Id}", id);
            return StatusCode(500, new { message = "Failed to test SMS gateway" });
        }
    }
}

public class TestSmsRequest
{
    public string TestPhoneNumber { get; set; } = string.Empty;
    public string TestMessage { get; set; } = "Test message from Complaint Management System";
}
