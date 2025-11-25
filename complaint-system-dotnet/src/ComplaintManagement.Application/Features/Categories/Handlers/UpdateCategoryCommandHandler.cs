using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Categories;
using ComplaintManagement.Application.Features.Categories.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Categories.Handlers;

public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateCategoryCommandHandler> _logger;

    public UpdateCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing category
            var category = await _unitOfWork.ComplaintCategories.GetByIdAsync(request.Id, cancellationToken);

            if (category == null || category.IsDeleted)
            {
                return Result<CategoryDto>.Failure("Category not found", "Not found");
            }

            // Check if code is being changed and if new code already exists
            if (category.Code != request.Code)
            {
                var existingCategory = await _unitOfWork.ComplaintCategories
                    .FindAsync(c => c.Code == request.Code && c.Id != request.Id && !c.IsDeleted, cancellationToken);

                if (existingCategory.Any())
                {
                    return Result<CategoryDto>.Failure("Category with this code already exists", "Duplicate code");
                }
            }

            // Validate parent category if provided
            if (request.ParentCategoryId.HasValue)
            {
                // Prevent self-reference
                if (request.ParentCategoryId.Value == request.Id)
                {
                    return Result<CategoryDto>.Failure("Category cannot be its own parent", "Invalid parent");
                }

                var parentCategory = await _unitOfWork.ComplaintCategories
                    .GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);

                if (parentCategory == null || parentCategory.IsDeleted)
                {
                    return Result<CategoryDto>.Failure("Parent category not found", "Invalid parent");
                }

                // Prevent circular reference
                if (parentCategory.ParentCategoryId == request.Id)
                {
                    return Result<CategoryDto>.Failure("This would create a circular reference", "Invalid hierarchy");
                }

                // Prevent nesting more than 2 levels
                if (parentCategory.ParentCategoryId.HasValue)
                {
                    return Result<CategoryDto>.Failure("Cannot nest categories more than 2 levels deep", "Invalid hierarchy");
                }
            }

            // Check if this category has children and is being set as a child itself
            var hasChildren = await _unitOfWork.ComplaintCategories
                .FindAsync(c => c.ParentCategoryId == request.Id && !c.IsDeleted, cancellationToken);

            if (hasChildren.Any() && request.ParentCategoryId.HasValue)
            {
                return Result<CategoryDto>.Failure("Cannot make a parent category into a child category while it has children", "Has children");
            }

            // Update category
            category.Name = request.Name;
            category.Code = request.Code;
            category.Description = request.Description;
            category.ParentCategoryId = request.ParentCategoryId;
            category.DefaultPriority = request.DefaultPriority;
            category.IsActive = request.IsActive;
            category.DisplayOrder = request.DisplayOrder;
            category.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ComplaintCategories.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated category {Code} - {Name}", category.Code, category.Name);

            // Get parent category name if exists
            string? parentCategoryName = null;
            if (category.ParentCategoryId.HasValue)
            {
                var parentCategory = await _unitOfWork.ComplaintCategories
                    .GetByIdAsync(category.ParentCategoryId.Value, cancellationToken);
                parentCategoryName = parentCategory?.Name;
            }

            // Return DTO
            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                ParentCategoryName = parentCategoryName,
                DefaultPriority = category.DefaultPriority,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder
            };

            return Result<CategoryDto>.Success(categoryDto, "Category updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {Id}", request.Id);
            return Result<CategoryDto>.Failure($"Error updating category: {ex.Message}", "Update failed");
        }
    }
}
