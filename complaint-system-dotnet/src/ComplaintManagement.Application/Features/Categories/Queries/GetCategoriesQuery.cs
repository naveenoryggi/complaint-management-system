using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Categories;
using MediatR;

namespace ComplaintManagement.Application.Features.Categories.Queries;

public class GetCategoriesQuery : IRequest<Result<List<CategoryDto>>>
{
    public bool ActiveOnly { get; set; } = true;
}
