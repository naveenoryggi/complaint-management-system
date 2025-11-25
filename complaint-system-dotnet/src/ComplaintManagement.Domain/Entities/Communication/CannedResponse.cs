using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Domain.Entities.MasterData;

namespace ComplaintManagement.Domain.Entities.Communication;

/// <summary>
/// Pre-written email response templates for quick replies
/// Supports template variables like {{complaintNumber}}, {{customerName}}
/// </summary>
public class CannedResponse : BaseEntity
{
    public Guid CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Optional: Limit to specific category
    /// NULL = available for all categories
    /// </summary>
    public Guid? CategoryId { get; set; }
    public ComplaintCategory? Category { get; set; }

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Short code for quick insertion (e.g., "greet", "close", "escalate")
    /// </summary>
    public string? ShortCode { get; set; }

    /// <summary>
    /// Optional subject line for the email
    /// </summary>
    public string? Subject { get; set; }

    /// <summary>
    /// HTML body with template variables
    /// Example: "Dear {{complainantName}}, your complaint {{complaintNumber}} has been received."
    /// </summary>
    public string Body { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Track how often this template is used
    /// </summary>
    public int UsageCount { get; set; } = 0;

    public Guid CreatedBy { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
