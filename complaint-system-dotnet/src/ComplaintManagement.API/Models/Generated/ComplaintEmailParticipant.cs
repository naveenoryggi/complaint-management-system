using System;
using System.Collections.Generic;

namespace ComplaintManagement.API.Models.Generated;

public partial class ComplaintEmailParticipant
{
    public Guid Id { get; set; }

    public Guid ComplaintId { get; set; }

    public string EmailAddress { get; set; } = null!;

    public string? DisplayName { get; set; }

    public string ParticipantType { get; set; } = null!;

    public Guid? AddedBy { get; set; }

    public Guid? AddedByUserId { get; set; }

    public DateTime AddedAt { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedBy { get; set; }

    public virtual User? AddedByUser { get; set; }

    public virtual Complaint Complaint { get; set; } = null!;
}
