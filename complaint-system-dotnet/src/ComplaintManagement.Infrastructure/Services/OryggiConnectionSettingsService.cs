using System.Diagnostics;
using ComplaintManagement.Application.DTOs.Sync;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Sync;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

public interface IOryggiConnectionSettingsService
{
    Task<List<OryggiConnectionSettingsDto>> GetAllAsync(Guid tenantId);
    Task<OryggiConnectionSettingsDto?> GetByIdAsync(Guid id);
    Task<OryggiConnectionSettingsDto?> GetActiveAsync(Guid tenantId);
    Task<OryggiConnectionSettingsDto> CreateAsync(CreateOryggiConnectionSettingsDto dto);
    Task<OryggiConnectionSettingsDto> UpdateAsync(Guid id, UpdateOryggiConnectionSettingsDto dto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> SetActiveAsync(Guid id);
    Task<ConnectionTestResult> TestConnectionAsync(TestConnectionDto dto);
    Task<string> BuildConnectionStringAsync(Guid connectionSettingsId);
}

public class OryggiConnectionSettingsService : IOryggiConnectionSettingsService
{
    private readonly ComplaintDbContext _context;
    private readonly IEncryptionService _encryption;
    private readonly ILogger<OryggiConnectionSettingsService> _logger;

    public OryggiConnectionSettingsService(
        ComplaintDbContext context,
        IEncryptionService encryption,
        ILogger<OryggiConnectionSettingsService> logger)
    {
        _context = context;
        _encryption = encryption;
        _logger = logger;
    }

    public async Task<List<OryggiConnectionSettingsDto>> GetAllAsync(Guid tenantId)
    {
        var settings = await _context.OryggiConnectionSettings
            .Where(s => s.TenantId == tenantId && !s.IsDeleted)
            .OrderByDescending(s => s.IsActive)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync();

        return settings.Select(MapToDto).ToList();
    }

    public async Task<OryggiConnectionSettingsDto?> GetByIdAsync(Guid id)
    {
        var setting = await _context.OryggiConnectionSettings
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        return setting == null ? null : MapToDto(setting);
    }

    public async Task<OryggiConnectionSettingsDto?> GetActiveAsync(Guid tenantId)
    {
        var setting = await _context.OryggiConnectionSettings
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsActive && !s.IsDeleted);

        return setting == null ? null : MapToDto(setting);
    }

    public async Task<OryggiConnectionSettingsDto> CreateAsync(CreateOryggiConnectionSettingsDto dto)
    {
        // Get tenant ID
        var tenantId = dto.TenantId ?? await GetDefaultTenantIdAsync();

        var setting = new OryggiConnectionSettings
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ServerAddress = dto.ServerAddress,
            Port = dto.Port,
            DatabaseName = dto.DatabaseName,
            EncryptedUsername = string.IsNullOrWhiteSpace(dto.Username)
                ? string.Empty
                : _encryption.EncryptPassword(dto.Username),
            EncryptedPassword = string.IsNullOrWhiteSpace(dto.Password)
                ? string.Empty
                : _encryption.EncryptPassword(dto.Password),
            UseWindowsAuthentication = dto.UseWindowsAuthentication,
            EncryptConnection = dto.EncryptConnection,
            TrustServerCertificate = dto.TrustServerCertificate,
            ConnectionTimeout = dto.ConnectionTimeout,
            IsActive = false, // New connections start as inactive
            Description = dto.Description
        };

        _context.OryggiConnectionSettings.Add(setting);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Created Oryggi connection settings {SettingsId} for tenant {TenantId}",
            setting.Id,
            tenantId);

        return MapToDto(setting);
    }

    public async Task<OryggiConnectionSettingsDto> UpdateAsync(Guid id, UpdateOryggiConnectionSettingsDto dto)
    {
        var setting = await _context.OryggiConnectionSettings
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (setting == null)
            throw new InvalidOperationException($"Connection settings {id} not found");

        setting.ServerAddress = dto.ServerAddress;
        setting.Port = dto.Port;
        setting.DatabaseName = dto.DatabaseName;
        setting.UseWindowsAuthentication = dto.UseWindowsAuthentication;
        setting.EncryptConnection = dto.EncryptConnection;
        setting.TrustServerCertificate = dto.TrustServerCertificate;
        setting.ConnectionTimeout = dto.ConnectionTimeout;
        setting.Description = dto.Description;

        // Only update credentials if provided
        if (!string.IsNullOrWhiteSpace(dto.Username))
            setting.EncryptedUsername = _encryption.EncryptPassword(dto.Username);

        if (!string.IsNullOrWhiteSpace(dto.Password))
            setting.EncryptedPassword = _encryption.EncryptPassword(dto.Password);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated Oryggi connection settings {SettingsId}", id);

        return MapToDto(setting);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var setting = await _context.OryggiConnectionSettings
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (setting == null)
            return false;

        // Soft delete
        _context.OryggiConnectionSettings.Remove(setting);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted Oryggi connection settings {SettingsId}", id);

        return true;
    }

    public async Task<bool> SetActiveAsync(Guid id)
    {
        var setting = await _context.OryggiConnectionSettings
            .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);

        if (setting == null)
            return false;

        // Deactivate all other connections for this tenant
        var otherSettings = await _context.OryggiConnectionSettings
            .Where(s => s.TenantId == setting.TenantId && s.Id != id && s.IsActive && !s.IsDeleted)
            .ToListAsync();

        foreach (var other in otherSettings)
        {
            other.IsActive = false;
        }

        // Activate this connection
        setting.IsActive = true;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Set Oryggi connection settings {SettingsId} as active for tenant {TenantId}",
            id,
            setting.TenantId);

        return true;
    }

    public async Task<ConnectionTestResult> TestConnectionAsync(TestConnectionDto dto)
    {
        var stopwatch = Stopwatch.StartNew();
        var result = new ConnectionTestResult
        {
            TestedAt = DateTime.UtcNow
        };

        try
        {
            var connectionString = BuildConnectionString(
                dto.ServerAddress,
                dto.Port,
                dto.DatabaseName,
                dto.Username,
                dto.Password,
                dto.UseWindowsAuthentication,
                dto.EncryptConnection,
                dto.TrustServerCertificate,
                dto.ConnectionTimeout);

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Test a simple query
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT @@VERSION";
            command.CommandTimeout = dto.ConnectionTimeout;

            var version = await command.ExecuteScalarAsync();

            stopwatch.Stop();

            result.Success = true;
            result.Message = "Connection successful";
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogInformation(
                "Connection test successful to {Server}:{Port}/{Database} in {Ms}ms",
                dto.ServerAddress,
                dto.Port,
                dto.DatabaseName,
                result.ResponseTimeMs);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            result.Success = false;
            result.Message = "Connection failed";
            result.ErrorDetails = ex.Message;
            result.ResponseTimeMs = (int)stopwatch.ElapsedMilliseconds;

            _logger.LogError(
                ex,
                "Connection test failed to {Server}:{Port}/{Database}",
                dto.ServerAddress,
                dto.Port,
                dto.DatabaseName);
        }

        return result;
    }

    public async Task<string> BuildConnectionStringAsync(Guid connectionSettingsId)
    {
        var setting = await _context.OryggiConnectionSettings
            .FirstOrDefaultAsync(s => s.Id == connectionSettingsId && !s.IsDeleted);

        if (setting == null)
            throw new InvalidOperationException($"Connection settings {connectionSettingsId} not found");

        var username = string.IsNullOrWhiteSpace(setting.EncryptedUsername)
            ? null
            : _encryption.DecryptPassword(setting.EncryptedUsername);

        var password = string.IsNullOrWhiteSpace(setting.EncryptedPassword)
            ? null
            : _encryption.DecryptPassword(setting.EncryptedPassword);

        return BuildConnectionString(
            setting.ServerAddress,
            setting.Port,
            setting.DatabaseName,
            username,
            password,
            setting.UseWindowsAuthentication,
            setting.EncryptConnection,
            setting.TrustServerCertificate,
            setting.ConnectionTimeout);
    }

    private string BuildConnectionString(
        string server,
        int port,
        string database,
        string? username,
        string? password,
        bool useWindowsAuth,
        bool encrypt,
        bool trustCert,
        int timeout)
    {
        var builder = new SqlConnectionStringBuilder
        {
            DataSource = port == 1433 ? server : $"{server},{port}",
            InitialCatalog = database,
            ConnectTimeout = timeout,
            Encrypt = encrypt,
            TrustServerCertificate = trustCert
        };

        if (useWindowsAuth)
        {
            builder.IntegratedSecurity = true;
        }
        else
        {
            builder.UserID = username;
            builder.Password = password;
        }

        return builder.ConnectionString;
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

    private OryggiConnectionSettingsDto MapToDto(OryggiConnectionSettings setting)
    {
        var username = string.IsNullOrWhiteSpace(setting.EncryptedUsername)
            ? string.Empty
            : _encryption.DecryptPassword(setting.EncryptedUsername);

        return new OryggiConnectionSettingsDto
        {
            Id = setting.Id,
            TenantId = setting.TenantId,
            ServerAddress = setting.ServerAddress,
            Port = setting.Port,
            DatabaseName = setting.DatabaseName,
            Username = username,
            UseWindowsAuthentication = setting.UseWindowsAuthentication,
            EncryptConnection = setting.EncryptConnection,
            TrustServerCertificate = setting.TrustServerCertificate,
            ConnectionTimeout = setting.ConnectionTimeout,
            IsActive = setting.IsActive,
            LastTestedAt = setting.LastTestedAt,
            LastTestResult = setting.LastTestResult,
            Description = setting.Description,
            CreatedAt = setting.CreatedAt
        };
    }
}
