using ComplaintManagement.Application.DTOs.Sync;
using ComplaintManagement.Infrastructure.Data;
using ComplaintManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.API.Controllers;

[ApiController]
[Route("api/oryggi-connection-settings")]
[Authorize]
public class OryggiConnectionSettingsController : ControllerBase
{
    private readonly IOryggiConnectionSettingsService _connectionService;
    private readonly ComplaintDbContext _context;
    private readonly ILogger<OryggiConnectionSettingsController> _logger;

    public OryggiConnectionSettingsController(
        IOryggiConnectionSettingsService connectionService,
        ComplaintDbContext context,
        ILogger<OryggiConnectionSettingsController> logger)
    {
        _connectionService = connectionService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all connection settings for a tenant
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Guid? tenantId)
    {
        try
        {
            var actualTenantId = tenantId ?? await GetDefaultTenantIdAsync();
            var settings = await _connectionService.GetAllAsync(actualTenantId);

            return Ok(new
            {
                success = true,
                data = settings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection settings");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to retrieve connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get connection settings by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var settings = await _connectionService.GetByIdAsync(id);

            if (settings == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Connection settings not found"
                });
            }

            return Ok(new
            {
                success = true,
                data = settings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving connection settings {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to retrieve connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Get active connection settings for a tenant
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive([FromQuery] Guid? tenantId)
    {
        try
        {
            var actualTenantId = tenantId ?? await GetDefaultTenantIdAsync();
            var settings = await _connectionService.GetActiveAsync(actualTenantId);

            if (settings == null)
            {
                return Ok(new
                {
                    success = true,
                    message = "No active connection settings found",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                success = true,
                data = settings
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving active connection settings");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to retrieve active connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Create new connection settings
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOryggiConnectionSettingsDto dto)
    {
        try
        {
            var settings = await _connectionService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = settings.Id },
                new
                {
                    success = true,
                    message = "Connection settings created successfully",
                    data = settings
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating connection settings");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to create connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Update existing connection settings
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOryggiConnectionSettingsDto dto)
    {
        try
        {
            var settings = await _connectionService.UpdateAsync(id, dto);

            return Ok(new
            {
                success = true,
                message = "Connection settings updated successfully",
                data = settings
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new
            {
                success = false,
                message = ex.Message
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating connection settings {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to update connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Delete connection settings
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _connectionService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Connection settings not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Connection settings deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting connection settings {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to delete connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Set connection settings as active
    /// </summary>
    [HttpPost("{id}/activate")]
    public async Task<IActionResult> SetActive(Guid id)
    {
        try
        {
            var success = await _connectionService.SetActiveAsync(id);

            if (!success)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Connection settings not found"
                });
            }

            return Ok(new
            {
                success = true,
                message = "Connection settings activated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating connection settings {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to activate connection settings",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Test a connection before saving
    /// </summary>
    [HttpPost("test")]
    public async Task<IActionResult> TestConnection([FromBody] TestConnectionDto dto)
    {
        try
        {
            var result = await _connectionService.TestConnectionAsync(dto);

            return Ok(new
            {
                success = result.Success,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to test connection",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Test an existing saved connection
    /// </summary>
    [HttpPost("{id}/test")]
    public async Task<IActionResult> TestExistingConnection(Guid id)
    {
        try
        {
            var settings = await _connectionService.GetByIdAsync(id);

            if (settings == null)
            {
                return NotFound(new
                {
                    success = false,
                    message = "Connection settings not found"
                });
            }

            var testDto = new TestConnectionDto
            {
                ServerAddress = settings.ServerAddress,
                Port = settings.Port,
                DatabaseName = settings.DatabaseName,
                Username = settings.Username,
                Password = null, // Password not returned from Get
                UseWindowsAuthentication = settings.UseWindowsAuthentication,
                EncryptConnection = settings.EncryptConnection,
                TrustServerCertificate = settings.TrustServerCertificate,
                ConnectionTimeout = settings.ConnectionTimeout
            };

            var result = await _connectionService.TestConnectionAsync(testDto);

            // Update test results in database
            var entity = await _context.OryggiConnectionSettings.FindAsync(id);
            if (entity != null)
            {
                entity.LastTestedAt = result.TestedAt;
                entity.LastTestResult = result.Success ? "Success" : $"Failed: {result.ErrorDetails}";
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                success = result.Success,
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing existing connection {Id}", id);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to test connection",
                error = ex.Message
            });
        }
    }

    private async Task<Guid> GetDefaultTenantIdAsync()
    {
        var defaultTenant = await _context.Tenants
            .Where(t => t.Code == "DEFAULT" && !t.IsDeleted)
            .FirstOrDefaultAsync();

        if (defaultTenant == null)
            throw new InvalidOperationException("No default tenant found");

        return defaultTenant.Id;
    }
}
