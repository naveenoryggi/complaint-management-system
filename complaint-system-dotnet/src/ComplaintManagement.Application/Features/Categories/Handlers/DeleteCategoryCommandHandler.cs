using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Features.Categories.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Categories.Handlers;

public class DeleteCategoryCommandHandler : IRequestHandler<DeleteCategoryCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DeleteCategoryCommandHandler> _logger;

    public DeleteCategoryCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<DeleteCategoryCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get existing category
            var category = await _unitOfWork.ComplaintCategories.GetByIdAsync(request.Id, cancellationToken);

            if (category == null || category.IsDeleted)
            {
                return Result<bool>.Failure("Category not found", "Not found");
            }

            // Check if category has any child categories
            var hasChildren = await _unitOfWork.ComplaintCategories
                .FindAsync(c => c.ParentCategoryId == request.Id && !c.IsDeleted, cancellationToken);

            if (hasChildren.Any())
            {
                return Result<bool>.Failure(
                    "Cannot delete category that has sub-categories. Please delete or reassign sub-categories first.",
                    "Has children");
            }

            // Check if category has any complaints
            var hasComplaints = await _unitOfWork.Complaints
                .FindAsync(c => c.CategoryId == request.Id && !c.IsDeleted, cancellationToken);

            if (hasComplaints.Any())
            {
                return Result<bool>.Failure(
                    $"Cannot delete category that has {hasComplaints.Count()} complaint(s). Please reassign or close these complaints first.",
                    "Has complaints");
            }

            // Soft delete the category
            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;

            _unitOfWork.ComplaintCategories.Update(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted category {Code} - {Name}", category.Code, category.Name);

            return Result<bool>.Success(true, "Category deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {Id}", request.Id);
            return Result<bool>.Failure($"Error deleting category: {ex.Message}", "Deletion failed");
        }
    }
}
