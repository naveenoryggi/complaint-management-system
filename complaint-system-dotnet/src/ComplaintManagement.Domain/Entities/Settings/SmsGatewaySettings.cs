using ComplaintManagement.Domain.Entities.MasterData;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Domain.Entities.Settings;

/// <summary>
/// SMS gateway/provider configuration
/// </summary>
public class SmsGatewaySettings : BaseEntity
{
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// SMS provider (Twilio, Vonage, AWS SNS, etc.)
    /// </summary>
    [Required(ErrorMessage = "SMS provider is required")]
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// API endpoint URL
    /// </summary>
    public string? ApiUrl { get; set; }

    /// <summary>
    /// Account SID / API Key
    /// </summary>
    public string? AccountSid { get; set; }

    /// <summary>
    /// Auth Token / API Secret (encrypted)
    /// </summary>
    public string? AuthToken { get; set; }

    /// <summary>
    /// From phone number (sender ID)
    /// </summary>
    public string? FromNumber { get; set; }

    /// <summary>
    /// Sender name (for providers that support it)
    /// </summary>
    public string? SenderName { get; set; }

    /// <summary>
    /// Is this configuration active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this the default configuration?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Maximum SMS per hour (rate limiting)
    /// </summary>
    public int? MaxSmsPerHour { get; set; }

    /// <summary>
    /// Cost per SMS (for tracking)
    /// </summary>
    public decimal? CostPerSms { get; set; }

    /// <summary>
    /// Timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Company ID (null = global)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Additional configuration (as JSON)
    /// </summary>
    public string? AdditionalConfig { get; set; }

    /// <summary>
    /// Test connection notes
    /// </summary>
    public string? TestNotes { get; set; }

    /// <summary>
    /// Last successful test date
    /// </summary>
    public DateTime? LastTestedAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Company
    /// </summary>
    public Company? Company { get; set; }
}
