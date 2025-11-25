namespace ComplaintManagement.Domain.Enums;

/// <summary>
/// Types of media storage backends for WhatsApp document attachments
/// </summary>
public enum MediaStorageType
{
    /// <summary>
    /// Store media files on local filesystem with web server serving static files
    /// Requires: MediaStoragePath (local directory), MediaPublicBaseUrl (https://yourdomain.com/whatsapp-media)
    /// </summary>
    LocalFileSystem = 0,

    /// <summary>
    /// Store media files in Amazon S3
    /// Requires: MediaStoragePath (bucket name), MediaPublicBaseUrl, MediaStorageConfig (AWS credentials)
    /// </summary>
    AWS_S3 = 1,

    /// <summary>
    /// Store media files in Azure Blob Storage
    /// Requires: MediaStoragePath (container name), MediaPublicBaseUrl, MediaStorageConfig (Azure credentials)
    /// </summary>
    Azure_Blob = 2,

    /// <summary>
    /// Store media files in Google Cloud Storage
    /// Requires: MediaStoragePath (bucket name), MediaPublicBaseUrl, MediaStorageConfig (GCP credentials)
    /// </summary>
    Google_Cloud = 3,

    /// <summary>
    /// Use custom URL-based storage or CDN
    /// Requires: MediaPublicBaseUrl (base URL for media access)
    /// Files are managed externally, system only generates URLs
    /// </summary>
    Custom_URL = 4
}
