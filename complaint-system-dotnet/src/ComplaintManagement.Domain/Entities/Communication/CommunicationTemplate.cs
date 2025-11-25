using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Template for communication messages (Email, SMS, WhatsApp, etc.)
/// </summary>
public class CommunicationTemplate : BaseEntity
{
    /// <summary>
    /// Template name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Template code for programmatic reference
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Description of template usage
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Communication channel this template is for
    /// </summary>
    public CommunicationChannel Channel { get; set; }

    /// <summary>
    /// Template subject (for Email)
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// Template body/content (supports placeholders like {{complaintNumber}})
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// HTML body (for Email)
    /// </summary>
    public string? HtmlBody { get; set; }

    /// <summary>
    /// Available placeholders (as JSON array)
    /// Example: ["{{complaintNumber}}", "{{complainantName}}", "{{status}}"]
    /// </summary>
    public string? AvailablePlaceholders { get; set; }

    /// <summary>
    /// Is this template active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Is this a system template (cannot be deleted)?
    /// </summary>
    public bool IsSystem { get; set; } = false;

    /// <summary>
    /// Company ID (null = global/system template)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Language/locale code (e.g., "en-US", "is-IS")
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// Category for organizing templates
    /// </summary>
    public string? Category { get; set; }

    // Navigation properties
    /// <summary>
    /// Company this template belongs to
    /// </summary>
    public Company? Company { get; set; }
}
