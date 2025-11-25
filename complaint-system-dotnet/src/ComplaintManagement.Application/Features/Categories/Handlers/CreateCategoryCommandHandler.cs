using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Categories;
using ComplaintManagement.Application.Features.Categories.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Categories.Handlers;

public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Result<CategoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateCategoryCommandHandler> _logger;

    public CreateCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<CreateCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<CategoryDto>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if category code already exists
            var existingCategory = await _unitOfWork.ComplaintCategories
                .FindAsync(c => c.Code == request.Code && !c.IsDeleted, cancellationToken);

            if (existingCategory.Any())
            {
                return Result<CategoryDto>.Failure("Category with this code already exists", "Duplicate code");
            }

            // Validate parent category if provided
            if (request.ParentCategoryId.HasValue)
            {
                var parentCategory = await _unitOfWork.ComplaintCategories
                    .GetByIdAsync(request.ParentCategoryId.Value, cancellationToken);

                if (parentCategory == null || parentCategory.IsDeleted)
                {
                    return Result<CategoryDto>.Failure("Parent category not found", "Invalid parent");
                }

                // Prevent circular reference
                if (parentCategory.ParentCategoryId.HasValue)
                {
                    return Result<CategoryDto>.Failure("Cannot nest categories more than 2 levels deep", "Invalid hierarchy");
                }
            }

            // Create new category
            var category = new ComplaintCategory
            {
                Name = request.Name,
                Code = request.Code,
                Description = request.Description,
                ParentCategoryId = request.ParentCategoryId,
                DefaultPriority = request.DefaultPriority,
                IsActive = request.IsActive,
                DisplayOrder = request.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ComplaintCategories.AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created category {Code} - {Name}", category.Code, category.Name);

            // Return DTO
            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                Description = category.Description,
                ParentCategoryId = category.ParentCategoryId,
                DefaultPriority = category.DefaultPriority,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder
            };

            return Result<CategoryDto>.Success(categoryDto, "Category created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category {Code}", request.Code);
            return Result<CategoryDto>.Failure($"Error creating category: {ex.Message}", "Creation failed");
        }
    }
}
