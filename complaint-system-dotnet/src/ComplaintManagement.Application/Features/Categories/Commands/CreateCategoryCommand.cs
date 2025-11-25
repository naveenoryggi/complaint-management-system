using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Categories;
using MediatR;

namespace ComplaintManagement.Application.Features.Categories.Commands;

public class CreateCategoryCommand : IRequest<Result<CategoryDto>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? ParentCategoryId { get; set; }
    public int DefaultPriority { get; set; } = 2;
    public int DefaultSlaHours { get; set; } = 48;
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}
