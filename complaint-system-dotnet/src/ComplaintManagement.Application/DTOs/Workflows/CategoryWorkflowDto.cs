using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Workflows;

/// <summary>
/// DTO for Category Workflow
/// </summary>
public class CategoryWorkflowDto
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public Guid? CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public List<CategoryWorkflowStatusDto> WorkflowStatuses { get; set; } = new();
    public List<CategoryWorkflowTransitionDto> Transitions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Request DTO for creating a category workflow
/// </summary>
public class CreateCategoryWorkflowRequest
{
    [Required(ErrorMessage = "Category ID is required")]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Workflow name is required")]
    [MinLength(3, ErrorMessage = "Workflow name must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Workflow name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = true;
    public Guid? CompanyId { get; set; }
}

/// <summary>
/// Request DTO for updating a category workflow
/// </summary>
public class UpdateCategoryWorkflowRequest
{
    [Required(ErrorMessage = "Workflow ID is required")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Workflow name is required")]
    [MinLength(3, ErrorMessage = "Workflow name must be at least 3 characters")]
    [MaxLength(200, ErrorMessage = "Workflow name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
}
