using System.ComponentModel.DataAnnotations;

namespace ComplaintManagement.Application.DTOs.Categories;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Category name is required")]
    [StringLength(100, ErrorMessage = "Category name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category code is required")]
    [StringLength(50, ErrorMessage = "Category code cannot exceed 50 characters")]
    public string Code { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    [Range(1, 4, ErrorMessage = "Default priority must be between 1 and 4")]
    public int DefaultPriority { get; set; } = 2; // Normal

    [Range(1, 720, ErrorMessage = "Default SLA hours must be between 1 and 720 hours (30 days)")]
    public int DefaultSlaHours { get; set; } = 48;

    public bool IsActive { get; set; } = true;

    [Range(0, 9999, ErrorMessage = "Display order must be between 0 and 9999")]
    public int DisplayOrder { get; set; } = 0;
}
