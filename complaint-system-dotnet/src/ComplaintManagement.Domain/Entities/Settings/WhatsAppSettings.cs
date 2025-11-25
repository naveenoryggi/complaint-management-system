using ComplaintManagement.Domain.Entities.MasterData;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Domain.Entities.Settings;

/// <summary>
/// WhatsApp Business API configuration
/// </summary>
public class WhatsAppSettings : BaseEntity
{
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// WhatsApp provider (Twilio, MessageBird, official WhatsApp Business API, etc.)
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// API endpoint URL
    /// </summary>
    public string? ApiUrl { get; set; }

    /// <summary>
    /// WhatsApp Business Account ID
    /// </summary>
    public string? BusinessAccountId { get; set; }

    /// <summary>
    /// Phone number ID
    /// </summary>
    public string? PhoneNumberId { get; set; }

    /// <summary>
    /// Access token (encrypted)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Webhook verification token
    /// </summary>
    public string? WebhookToken { get; set; }

    /// <summary>
    /// From phone number
    /// </summary>
    public string? FromNumber { get; set; }

    /// <summary>
    /// Business display name
    /// </summary>
    public string? BusinessName { get; set; }

    /// <summary>
    /// Is this configuration active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this the default configuration?
    /// </summary>
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// Maximum messages per hour (rate limiting)
    /// </summary>
    public int? MaxMessagesPerHour { get; set; }

    /// <summary>
    /// Timeout in seconds
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Company ID (null = global)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Media storage type for WhatsApp documents/images
    /// Values: LocalFileSystem, AWS_S3, Azure_Blob, Google_Cloud, Custom_URL
    /// </summary>
    public string? MediaStorageType { get; set; }

    /// <summary>
    /// Local filesystem path or cloud storage container/bucket name for storing media files
    /// Examples: "C:\WhatsAppMedia", "/var/whatsapp-media", "my-whatsapp-bucket"
    /// </summary>
    public string? MediaStoragePath { get; set; }

    /// <summary>
    /// Public HTTPS base URL where media files are accessible
    /// Example: "https://yourdomain.com/whatsapp-media", "https://cdn.yourdomain.com/media"
    /// WhatsApp requires HTTPS URLs for all media
    /// </summary>
    public string? MediaPublicBaseUrl { get; set; }

    /// <summary>
    /// Cloud storage credentials and configuration (as JSON)
    /// For AWS S3: {"accessKeyId":"...", "secretAccessKey":"...", "region":"us-east-1"}
    /// For Azure: {"accountName":"...", "accountKey":"...", "containerName":"..."}
    /// For Google Cloud: {"projectId":"...", "keyFile":"..."}
    /// </summary>
    public string? MediaStorageConfig { get; set; }

    /// <summary>
    /// Maximum file size in MB for media uploads
    /// WhatsApp limits: Images 5MB, Documents 100MB, Videos 16MB
    /// </summary>
    public int? MaxMediaSizeMB { get; set; }

    /// <summary>
    /// Automatically clean up old media files after X days (0 = never delete)
    /// </summary>
    public int? MediaRetentionDays { get; set; }

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
