using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class Complaint
{
    public Guid Id { get; set; }

    public string ComplaintNumber { get; set; } = null!;

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public Guid ComplainantId { get; set; }

    public Guid CompanyId { get; set; }

    public Guid? BranchId { get; set; }

    public Guid? DepartmentId { get; set; }

    public int CurrentEscalationLevel { get; set; }

    public Guid? AssignedToId { get; set; }

    public DateTime SubmittedAt { get; set; }

    public DateTime? DueDate { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string? ResolutionNotes { get; set; }

    public bool IsAnonymous { get; set; }

    public string? Tags { get; set; }

    public Guid? RelatedComplaintId { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public Guid? ResourcePoolId { get; set; }

    public string? AlternatePhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? EmployeeCode { get; set; }

    public int PreferredContactMethod { get; set; }

    public Guid? SectionId { get; set; }

    public Guid PriorityMasterId { get; set; }

    public Guid StatusMasterId { get; set; }

    public virtual User? AssignedTo { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ComplaintCategory Category { get; set; } = null!;

    public virtual Company Company { get; set; } = null!;

    public virtual User Complainant { get; set; } = null!;

    public virtual ICollection<ComplaintAttachment> ComplaintAttachments { get; set; } = new List<ComplaintAttachment>();

    public virtual ICollection<ComplaintComment> ComplaintComments { get; set; } = new List<ComplaintComment>();

    public virtual ICollection<ComplaintEmailParticipant> ComplaintEmailParticipants { get; set; } = new List<ComplaintEmailParticipant>();

    public virtual ICollection<CustomFieldValue> CustomFieldValues { get; set; } = new List<CustomFieldValue>();

    public virtual Department? Department { get; set; }

    public virtual ICollection<EmailMessage> EmailMessages { get; set; } = new List<EmailMessage>();

    public virtual ICollection<EmailResponseHistory> EmailResponseHistories { get; set; } = new List<EmailResponseHistory>();

    public virtual ICollection<EscalationHistory> EscalationHistories { get; set; } = new List<EscalationHistory>();

    public virtual ICollection<Complaint> InverseRelatedComplaint { get; set; } = new List<Complaint>();

    public virtual ComplaintPriorityMaster PriorityMaster { get; set; } = null!;

    public virtual Complaint? RelatedComplaint { get; set; }

    public virtual ResourcePool? ResourcePool { get; set; }

    public virtual Section? Section { get; set; }

    public virtual ComplaintStatusMaster StatusMaster { get; set; } = null!;
}
