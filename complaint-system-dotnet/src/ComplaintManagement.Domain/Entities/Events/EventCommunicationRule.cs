using ComplaintManagement.Domain.Entities.Communication;
using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;
using System.Text.Json.Serialization;

namespace ComplaintManagement.Domain.Entities.Events;

/// <summary>
/// Maps events to communication actions (who gets what message when an event occurs)
/// </summary>
public class EventCommunicationRule : BaseEntity
{
    /// <summary>
    /// Rule name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Rule description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Event type that triggers this rule
    /// </summary>
    public Guid EventTypeId { get; set; }

    /// <summary>
    /// Communication channel to use
    /// </summary>
    public CommunicationChannel Channel { get; set; }

    /// <summary>
    /// Template to use for the message
    /// </summary>
    public Guid? TemplateId { get; set; }

    /// <summary>
    /// Recipient type (who receives the message)
    /// </summary>
    public RecipientType RecipientType { get; set; }

    /// <summary>
    /// Specific user IDs to send to (as JSON array) - for RecipientType.SpecificUsers
    /// </summary>
    public string? SpecificUserIds { get; set; }

    /// <summary>
    /// Specific role IDs to send to (as JSON array) - for RecipientType.SpecificRoles
    /// </summary>
    public string? SpecificRoleIds { get; set; }

    /// <summary>
    /// Specific email addresses (as JSON array) - for RecipientType.SpecificEmails
    /// </summary>
    public string? SpecificEmails { get; set; }

    /// <summary>
    /// Conditions for this rule to trigger (as JSON)
    /// Example: {"status": "Resolved", "priority": "High"}
    /// </summary>
    public string? Conditions { get; set; }

    /// <summary>
    /// Is this rule active?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority for execution order (lower = higher priority)
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// Delay before sending (in minutes)
    /// </summary>
    public int DelayMinutes { get; set; } = 0;

    /// <summary>
    /// Send only once per entity (don't resend if event occurs again)
    /// </summary>
    public bool SendOnlyOnce { get; set; } = false;

    /// <summary>
    /// Company ID (null = global rule)
    /// </summary>
    public Guid? CompanyId { get; set; }

    /// <summary>
    /// Additional data to include in message (as JSON)
    /// </summary>
    public string? AdditionalData { get; set; }

    // Navigation properties
    /// <summary>
    /// Event type that triggers this rule
    /// </summary>
    [JsonIgnore]
    public EventType? EventType { get; set; }

    /// <summary>
    /// Communication template
    /// </summary>
    [JsonIgnore]
    public CommunicationTemplate? Template { get; set; }

    /// <summary>
    /// Company
    /// </summary>
    [JsonIgnore]
    public Company? Company { get; set; }
}
