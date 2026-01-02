using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for stock category management
/// </summary>
public interface IStockCategoryService
{
    /// <summary>
    /// Get all stock categories for a company with optional filtering
    /// </summary>
    Task<Result<PagedResult<StockCategoryDto>>> GetAllAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null);

    /// <summary>
    /// Get stock categories for dropdown
    /// </summary>
    Task<Result<List<StockCategoryLookupDto>>> GetLookupAsync(Guid companyId, bool activeOnly = true);

    /// <summary>
    /// Get a single stock category by ID
    /// </summary>
    Task<Result<StockCategoryDto>> GetByIdAsync(Guid id, Guid companyId);

    /// <summary>
    /// Get stock category by code
    /// </summary>
    Task<Result<StockCategoryDto>> GetByCodeAsync(string code, Guid companyId);

    /// <summary>
    /// Create a new stock category
    /// </summary>
    Task<Result<StockCategoryDto>> CreateAsync(CreateStockCategoryRequest request, Guid companyId, Guid userId);

    /// <summary>
    /// Update an existing stock category
    /// </summary>
    Task<Result<StockCategoryDto>> UpdateAsync(Guid id, UpdateStockCategoryRequest request, Guid companyId, Guid userId);

    /// <summary>
    /// Delete a stock category
    /// </summary>
    Task<Result<bool>> DeleteAsync(Guid id, Guid companyId, Guid userId);

    /// <summary>
    /// Seed default stock categories for a new company
    /// </summary>
    Task SeedDefaultCategoriesAsync(Guid companyId, Guid userId);
}
