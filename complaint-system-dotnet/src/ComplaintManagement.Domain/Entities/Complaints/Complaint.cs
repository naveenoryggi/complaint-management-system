using ComplaintManagement.Domain.Entities.MasterData;
using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Domain.Entities.Complaints;

/// <summary>
/// Represents a complaint submitted by a user
/// </summary>
public class Complaint : BaseEntity
{
    /// <summary>
    /// Complaint tracking number (auto-generated)
    /// </summary>
    public string ComplaintNumber { get; set; } = string.Empty;

    /// <summary>
    /// Complaint title/subject
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category ID (foreign key)
    /// </summary>
    public Guid CategoryId { get; set; }

    /// <summary>
    /// Complainant user ID (foreign key)
    /// </summary>
    public Guid ComplainantId { get; set; }

    /// <summary>
    /// Company ID (foreign key)
    /// </summary>
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Branch ID (foreign key)
    /// </summary>
    public Guid? BranchId { get; set; }

    /// <summary>
    /// Department ID (foreign key)
    /// </summary>
    public Guid? DepartmentId { get; set; }

    /// <summary>
    /// Section ID (foreign key)
    /// </summary>
    public Guid? SectionId { get; set; }

    /// <summary>
    /// Employee code of complainant (for reference)
    /// </summary>
    public string? EmployeeCode { get; set; }

    /// <summary>
    /// Contact email for complainant
    /// </summary>
    public string? ContactEmail { get; set; }

    /// <summary>
    /// Contact phone number for complainant
    /// </summary>
    public string? ContactPhone { get; set; }

    /// <summary>
    /// Alternate/secondary phone number
    /// </summary>
    public string? AlternatePhone { get; set; }

    /// <summary>
    /// Preferred contact method for this complaint
    /// </summary>
    public PreferredContactMethod PreferredContactMethod { get; set; } = PreferredContactMethod.Both;

    /// <summary>
    /// Status ID - reference to configurable status master (REQUIRED)
    /// </summary>
    public Guid StatusMasterId { get; set; }

    /// <summary>
    /// Priority ID - reference to configurable priority master (REQUIRED)
    /// </summary>
    public Guid PriorityMasterId { get; set; }

    /// <summary>
    /// Current escalation level (0 = initial, 1-5 = escalation levels)
    /// </summary>
    public int CurrentEscalationLevel { get; set; } = 0;

    /// <summary>
    /// Currently assigned handler user ID
    /// </summary>
    public Guid? AssignedToId { get; set; }

    /// <summary>
    /// Resource pool ID (if assigned via resource pool)
    /// </summary>
    public Guid? ResourcePoolId { get; set; }

    /// <summary>
    /// Date/time when complaint was submitted
    /// </summary>
    public DateTime SubmittedAt { get; set; }

    /// <summary>
    /// Date/time when complaint should be resolved (SLA)
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Date/time when complaint was resolved
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// Date/time when complaint was closed
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Resolution notes
    /// </summary>
    public string? ResolutionNotes { get; set; }

    /// <summary>
    /// Is anonymous complaint
    /// </summary>
    public bool IsAnonymous { get; set; } = false;

    /// <summary>
    /// Tags (comma-separated)
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// Related complaint ID (if reopened or linked)
    /// </summary>
    public Guid? RelatedComplaintId { get; set; }

    #region Customer Portal Fields

    /// <summary>
    /// Customer ID (for portal tickets)
    /// </summary>
    public Guid? CustomerId { get; set; }

    /// <summary>
    /// Customer contact ID (portal user who created the ticket)
    /// </summary>
    public Guid? CustomerContactId { get; set; }

    /// <summary>
    /// Customer location ID (site where the issue is)
    /// </summary>
    public Guid? CustomerLocationId { get; set; }

    /// <summary>
    /// Asset ID (asset related to the complaint)
    /// </summary>
    public Guid? AssetId { get; set; }

    /// <summary>
    /// Contract ID (contract covering the service)
    /// </summary>
    public Guid? ContractId { get; set; }

    /// <summary>
    /// Product ID (product related to the complaint)
    /// </summary>
    public Guid? ProductId { get; set; }

    /// <summary>
    /// Project ID (project related to the complaint)
    /// </summary>
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Source/channel from which the ticket was created
    /// </summary>
    public TicketSource Source { get; set; } = TicketSource.Internal;

    /// <summary>
    /// Whether the complaint is covered under warranty
    /// </summary>
    public bool IsUnderWarranty { get; set; }

    /// <summary>
    /// Whether the complaint is covered under a service contract
    /// </summary>
    public bool IsUnderContract { get; set; }

    /// <summary>
    /// Customer satisfaction rating (1-5)
    /// </summary>
    public int? CustomerSatisfactionRating { get; set; }

    /// <summary>
    /// Customer feedback text
    /// </summary>
    public string? CustomerFeedback { get; set; }

    #endregion

    /// <summary>
    /// Indicates if there is a customer response awaiting handler action
    /// </summary>
    public bool HasCustomerResponse { get; set; } = false;

    /// <summary>
    /// Who sent the last response (Customer or Handler)
    /// </summary>
    public string? LastResponseFrom { get; set; }

    /// <summary>
    /// When the last response was received
    /// </summary>
    public DateTime? LastResponseAt { get; set; }

    // Navigation properties
    /// <summary>
    /// Complaint category
    /// </summary>
    public ComplaintCategory Category { get; set; } = null!;

    /// <summary>
    /// User who submitted the complaint
    /// </summary>
    public User Complainant { get; set; } = null!;

    /// <summary>
    /// Company where complaint was submitted
    /// </summary>
    public Company Company { get; set; } = null!;

    /// <summary>
    /// Branch where complaint was submitted
    /// </summary>
    public Branch? Branch { get; set; }

    /// <summary>
    /// Department where complaint was submitted
    /// </summary>
    public Department? Department { get; set; }

    /// <summary>
    /// Section where complaint was submitted
    /// </summary>
    public Section? Section { get; set; }

    /// <summary>
    /// Currently assigned handler
    /// </summary>
    public User? AssignedTo { get; set; }

    /// <summary>
    /// Resource pool (if assigned via resource pool)
    /// </summary>
    public Escalation.ResourcePool? ResourcePool { get; set; }

    /// <summary>
    /// Related complaint (if any)
    /// </summary>
    public Complaint? RelatedComplaint { get; set; }

    /// <summary>
    /// Customer (for portal tickets)
    /// </summary>
    public CRM.Customer? Customer { get; set; }

    /// <summary>
    /// Customer contact (portal user)
    /// </summary>
    public CRM.CustomerContact? CustomerContact { get; set; }

    /// <summary>
    /// Customer location
    /// </summary>
    public CRM.CustomerLocation? CustomerLocation { get; set; }

    /// <summary>
    /// Related asset
    /// </summary>
    public Service.Asset? Asset { get; set; }

    /// <summary>
    /// Related contract
    /// </summary>
    public Service.Contract? Contract { get; set; }

    /// <summary>
    /// Related product
    /// </summary>
    public Product.Product? Product { get; set; }

    /// <summary>
    /// Related project
    /// </summary>
    public CRM.Project? Project { get; set; }

    /// <summary>
    /// Status master (when using configurable statuses)
    /// </summary>
    public ComplaintStatusMaster? StatusMaster { get; set; }

    /// <summary>
    /// Priority master (when using configurable priorities)
    /// </summary>
    public ComplaintPriorityMaster? PriorityMaster { get; set; }

    /// <summary>
    /// Comments on this complaint
    /// </summary>
    public ICollection<ComplaintComment> Comments { get; set; } = new List<ComplaintComment>();

    /// <summary>
    /// Attachments to this complaint
    /// </summary>
    public ICollection<ComplaintAttachment> Attachments { get; set; } = new List<ComplaintAttachment>();

    /// <summary>
    /// Escalation history for this complaint
    /// </summary>
    public ICollection<Escalation.EscalationHistory> EscalationHistories { get; set; } = new List<Escalation.EscalationHistory>();

    /// <summary>
    /// Custom field values for this complaint
    /// </summary>
    public ICollection<CustomFields.CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFields.CustomFieldValue>();
}
