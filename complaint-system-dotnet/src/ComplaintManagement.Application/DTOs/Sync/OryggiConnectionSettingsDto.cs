namespace ComplaintManagement.Application.DTOs.Sync;

/// <summary>
/// DTO for Oryggi connection settings (excludes sensitive data)
/// </summary>
public class OryggiConnectionSettingsDto
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty; // Decrypted for display
    public bool UseWindowsAuthentication { get; set; }
    public bool EncryptConnection { get; set; }
    public bool TrustServerCertificate { get; set; }
    public int ConnectionTimeout { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastTestedAt { get; set; }
    public string? LastTestResult { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// DTO for creating/updating connection settings
/// </summary>
public class CreateOryggiConnectionSettingsDto
{
    public Guid? TenantId { get; set; }
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 1433;
    public string DatabaseName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Plain text, will be encrypted
    public bool UseWindowsAuthentication { get; set; } = false;
    public bool EncryptConnection { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = false;
    public int ConnectionTimeout { get; set; } = 30;
    public string? Description { get; set; }
}

/// <summary>
/// DTO for updating connection settings
/// </summary>
public class UpdateOryggiConnectionSettingsDto
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string? Username { get; set; } // Optional - only update if provided
    public string? Password { get; set; } // Optional - only update if provided
    public bool UseWindowsAuthentication { get; set; }
    public bool EncryptConnection { get; set; }
    public bool TrustServerCertificate { get; set; }
    public int ConnectionTimeout { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO for testing connection
/// </summary>
public class TestConnectionDto
{
    public string ServerAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 1433;
    public string DatabaseName { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public bool UseWindowsAuthentication { get; set; } = false;
    public bool EncryptConnection { get; set; } = true;
    public bool TrustServerCertificate { get; set; } = false;
    public int ConnectionTimeout { get; set; } = 30;
}

/// <summary>
/// Result of connection test
/// </summary>
public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
    public DateTime TestedAt { get; set; }
    public int? ResponseTimeMs { get; set; }
}
