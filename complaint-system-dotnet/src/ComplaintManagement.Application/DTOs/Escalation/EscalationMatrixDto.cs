using ComplaintManagement.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Escalation;

/// <summary>
/// DTO for Escalation Matrix
/// </summary>
public class EscalationMatrixDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CompanyId { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public bool EnableAutoEscalation { get; set; }
    public bool SendEmailNotifications { get; set; }
    public List<EscalationLevelDto> EscalationLevels { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating an escalation matrix
/// </summary>
public class CreateEscalationMatrixRequest
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(1, ErrorMessage = "Name cannot be empty")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public int Priority { get; set; } = 0;
    public bool EnableAutoEscalation { get; set; } = true;
    public bool SendEmailNotifications { get; set; } = true;
    public List<CreateEscalationLevelRequest> EscalationLevels { get; set; } = new();
}

/// <summary>
/// Request DTO for updating an escalation matrix
/// </summary>
public class UpdateEscalationMatrixRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? DepartmentId { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public bool EnableAutoEscalation { get; set; }
    public bool SendEmailNotifications { get; set; }
    public List<CreateEscalationLevelRequest> EscalationLevels { get; set; } = new();
}
