using ComplaintManagement.Domain.Enums;

namespace ComplaintManagement.Application.DTOs.Escalation;

/// <summary>
/// DTO for Escalation Level
/// </summary>
public class EscalationLevelDto
{
    public Guid Id { get; set; }
    public Guid EscalationMatrixId { get; set; }
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int TriggerAfterHours { get; set; }
    public int TriggerAfterValue { get; set; }
    public TimeUnit TriggerTimeUnit { get; set; }
    public string TriggerTimeDisplay => $"{TriggerAfterValue} {TriggerTimeUnit.ToString().ToLower()}";
    public AssignmentStrategy AssignmentStrategy { get; set; }
    public Guid? AssignToUserId { get; set; }
    public string? AssignToUserName { get; set; }
    public string? AssignToRole { get; set; }
    public string? AssignToUserIds { get; set; }

    // Contact Hierarchy fields
    public Guid? PrimaryContactId { get; set; }
    public string? PrimaryContactName { get; set; }
    public Guid? SecondaryContactId { get; set; }
    public string? SecondaryContactName { get; set; }
    public Guid? HrContactId { get; set; }
    public string? HrContactName { get; set; }

    // Branch/Department fields
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }

    // Resource Pool fields
    public Guid? ResourcePoolId { get; set; }
    public string? ResourcePoolName { get; set; }
    public ResourcePoolAssignmentMethod? ResourcePoolAssignmentMethod { get; set; }

    public bool IsActive { get; set; }
    public bool SendNotification { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public bool NotifyPreviousHandler { get; set; }
    public string? EscalationMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating an escalation level
/// </summary>
public class CreateEscalationLevelRequest
{
    public int Level { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Legacy field - kept for backwards compatibility
    public int TriggerAfterHours { get; set; } = 0;

    // New flexible time configuration (recommended)
    public int TriggerAfterValue { get; set; } = 0;
    public TimeUnit TriggerTimeUnit { get; set; } = TimeUnit.Hours;

    public AssignmentStrategy AssignmentStrategy { get; set; }
    public Guid? AssignToUserId { get; set; }
    public string? AssignToRole { get; set; }
    public string? AssignToUserIds { get; set; }

    // Contact Hierarchy fields
    public Guid? PrimaryContactId { get; set; }
    public Guid? SecondaryContactId { get; set; }
    public Guid? HrContactId { get; set; }

    // Branch/Department fields
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }

    // Resource Pool fields
    public Guid? ResourcePoolId { get; set; }
    public ResourcePoolAssignmentMethod? ResourcePoolAssignmentMethod { get; set; }

    public bool SendNotification { get; set; } = true;
    public Guid? EmailTemplateId { get; set; }
    public bool NotifyPreviousHandler { get; set; } = true;
    public string? EscalationMessage { get; set; }
}

/// <summary>
/// Request DTO for updating an escalation level
/// </summary>
public class UpdateEscalationLevelRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Legacy field - kept for backwards compatibility
    public int TriggerAfterHours { get; set; }

    // New flexible time configuration (recommended)
    public int TriggerAfterValue { get; set; }
    public TimeUnit TriggerTimeUnit { get; set; }

    public AssignmentStrategy AssignmentStrategy { get; set; }
    public Guid? AssignToUserId { get; set; }
    public string? AssignToRole { get; set; }
    public string? AssignToUserIds { get; set; }

    // Contact Hierarchy fields
    public Guid? PrimaryContactId { get; set; }
    public Guid? SecondaryContactId { get; set; }
    public Guid? HrContactId { get; set; }

    // Branch/Department fields
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }

    // Resource Pool fields
    public Guid? ResourcePoolId { get; set; }
    public ResourcePoolAssignmentMethod? ResourcePoolAssignmentMethod { get; set; }

    public bool IsActive { get; set; }
    public bool SendNotification { get; set; }
    public Guid? EmailTemplateId { get; set; }
    public bool NotifyPreviousHandler { get; set; }
    public string? EscalationMessage { get; set; }
}
