using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Categories;
using ComplaintManagement.Application.Features.Categories.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Categories.Handlers;

public class GetCategoriesQueryHandler : IRequestHandler<GetCategoriesQuery, Result<List<CategoryDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<CategoryDto>>> Handle(GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _unitOfWork.ComplaintCategories.GetAllWithIncludesAsync(
                cancellationToken,
                c => c.ParentCategory
            );

            // Filter by active status if requested
            var filteredCategories = request.ActiveOnly
                ? categories.Where(c => c.IsActive).ToList()
                : categories.ToList();

            // Map to DTOs
            var categoryDtos = filteredCategories
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Code = c.Code,
                    Description = c.Description,
                    ParentCategoryId = c.ParentCategoryId,
                    ParentCategoryName = c.ParentCategory?.Name,
                    DefaultPriority = c.DefaultPriority,
                    IsActive = c.IsActive,
                    DisplayOrder = c.DisplayOrder
                }).ToList();

            return Result<List<CategoryDto>>.Success(categoryDtos, $"Retrieved {categoryDtos.Count} categories");
        }
        catch (Exception ex)
        {
            return Result<List<CategoryDto>>.Failure($"Error retrieving categories: {ex.Message}", "Query failed");
        }
    }
}
