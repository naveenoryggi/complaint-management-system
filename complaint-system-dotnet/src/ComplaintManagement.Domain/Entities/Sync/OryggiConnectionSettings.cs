using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Sync;

/// <summary>
/// Stores encrypted connection settings for the Oryggi database
/// </summary>
public class OryggiConnectionSettings : BaseEntity
{
    /// <summary>
    /// Tenant this connection belongs to
    /// </summary>
    public Guid TenantId { get; set; }
    public virtual Tenant Tenant { get; set; } = null!;

    /// <summary>
    /// Server address (e.g., localhost, 192.168.1.100, server.domain.com)
    /// </summary>
    public string ServerAddress { get; set; } = string.Empty;

    /// <summary>
    /// Port number (default: 1433 for SQL Server)
    /// </summary>
    public int Port { get; set; } = 1433;

    /// <summary>
    /// Database name
    /// </summary>
    public string DatabaseName { get; set; } = string.Empty;

    /// <summary>
    /// SQL Server username (encrypted)
    /// </summary>
    public string EncryptedUsername { get; set; } = string.Empty;

    /// <summary>
    /// SQL Server password (encrypted)
    /// </summary>
    public string EncryptedPassword { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use Windows Authentication
    /// </summary>
    public bool UseWindowsAuthentication { get; set; } = false;

    /// <summary>
    /// Whether to encrypt the connection (TLS/SSL)
    /// </summary>
    public bool EncryptConnection { get; set; } = true;

    /// <summary>
    /// Whether to trust the server certificate
    /// </summary>
    public bool TrustServerCertificate { get; set; } = false;

    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30;

    /// <summary>
    /// Whether this connection is currently active
    /// </summary>
    public bool IsActive { get; set; } = false;

    /// <summary>
    /// Last time this connection was tested successfully
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    /// <summary>
    /// Result of last connection test
    /// </summary>
    public string? LastTestResult { get; set; }

    /// <summary>
    /// Description of this connection
    /// </summary>
    public string? Description { get; set; }
}
