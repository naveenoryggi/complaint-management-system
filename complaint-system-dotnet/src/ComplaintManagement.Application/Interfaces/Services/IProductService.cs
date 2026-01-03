using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Product;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for managing products and product categories
/// </summary>
public interface IProductService
{
    #region Product Categories

    /// <summary>
    /// Gets the category tree for a company
    /// </summary>
    Task<Result<List<ProductCategoryTreeDto>>> GetCategoryTreeAsync(Guid companyId, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all categories with optional filtering
    /// </summary>
    Task<Result<List<ProductCategorySummaryDto>>> GetCategoriesAsync(Guid companyId, Guid? parentId = null, bool includeInactive = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by ID
    /// </summary>
    Task<Result<ProductCategoryDto>> GetCategoryByIdAsync(Guid companyId, Guid categoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a category by code
    /// </summary>
    Task<Result<ProductCategoryDto>> GetCategoryByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new category
    /// </summary>
    Task<Result<ProductCategoryDto>> CreateCategoryAsync(Guid companyId, CreateProductCategoryRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing category
    /// </summary>
    Task<Result<ProductCategoryDto>> UpdateCategoryAsync(Guid companyId, Guid categoryId, UpdateProductCategoryRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Moves a category to a new parent
    /// </summary>
    Task<Result<ProductCategoryDto>> MoveCategoryAsync(Guid companyId, Guid categoryId, MoveCategoryRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a category (soft delete)
    /// </summary>
    Task<Result<bool>> DeleteCategoryAsync(Guid companyId, Guid categoryId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets category lookup for dropdowns
    /// </summary>
    Task<Result<List<ProductCategoryLookupDto>>> GetCategoryLookupAsync(Guid companyId, bool allowProducts = true, CancellationToken cancellationToken = default);

    #endregion

    #region Products

    /// <summary>
    /// Gets products with filtering and pagination
    /// </summary>
    Task<Result<PagedResult<ProductSummaryDto>>> GetProductsAsync(Guid companyId, ProductSearchRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    Task<Result<ProductDto>> GetProductByIdAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by code
    /// </summary>
    Task<Result<ProductDto>> GetProductByCodeAsync(Guid companyId, string productCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by SKU
    /// </summary>
    Task<Result<ProductDto>> GetProductBySKUAsync(Guid companyId, string sku, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new product
    /// </summary>
    Task<Result<ProductDto>> CreateProductAsync(Guid companyId, CreateProductRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing product
    /// </summary>
    Task<Result<ProductDto>> UpdateProductAsync(Guid companyId, Guid productId, UpdateProductRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a product (soft delete)
    /// </summary>
    Task<Result<bool>> DeleteProductAsync(Guid companyId, Guid productId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones a product
    /// </summary>
    Task<Result<ProductDto>> CloneProductAsync(Guid companyId, Guid productId, CloneProductRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    // NOTE: UpdateInventoryAsync has been removed.
    // Inventory is now managed through the Stock Management module (IStockMovementService).
    // Use StockMovements for all inventory adjustments to enable location-based tracking and full audit trails.

    /// <summary>
    /// Gets product lookup for dropdowns
    /// </summary>
    Task<Result<List<ProductLookupDto>>> GetProductLookupAsync(Guid companyId, string? searchTerm = null, Guid? categoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets products by category
    /// </summary>
    Task<Result<List<ProductSummaryDto>>> GetProductsByCategoryAsync(Guid companyId, Guid categoryId, bool includeSubCategories = true, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets product variants
    /// </summary>
    Task<Result<List<ProductVariantDto>>> GetProductVariantsAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);

    #endregion

    #region Product Price Lists

    /// <summary>
    /// Gets price lists for a product
    /// </summary>
    Task<Result<List<ProductPriceListSummaryDto>>> GetProductPriceListsAsync(Guid companyId, Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a price list by ID
    /// </summary>
    Task<Result<ProductPriceListDto>> GetPriceListByIdAsync(Guid companyId, Guid priceListId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new price list
    /// </summary>
    Task<Result<ProductPriceListDto>> CreatePriceListAsync(Guid companyId, CreateProductPriceListRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing price list
    /// </summary>
    Task<Result<ProductPriceListDto>> UpdatePriceListAsync(Guid companyId, Guid priceListId, UpdateProductPriceListRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a price list
    /// </summary>
    Task<Result<bool>> DeletePriceListAsync(Guid companyId, Guid priceListId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates price for a product based on context
    /// </summary>
    Task<Result<ProductPriceDto>> CalculatePriceAsync(Guid companyId, CalculatePriceRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk update prices
    /// </summary>
    Task<Result<int>> BulkUpdatePricesAsync(Guid companyId, BulkPriceUpdateRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets product statistics
    /// </summary>
    Task<Result<ProductStatisticsDto>> GetProductStatisticsAsync(Guid companyId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Product statistics DTO
/// </summary>
public class ProductStatisticsDto
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }
    public int DraftProducts { get; set; }
    public int DiscontinuedProducts { get; set; }
    public int TotalCategories { get; set; }
    public int ProductsInStock { get; set; }
    public int ProductsOutOfStock { get; set; }
    public int ProductsBelowReorderLevel { get; set; }
    public int FeaturedProducts { get; set; }
    public Dictionary<string, int> ProductsByType { get; set; } = new();
    public Dictionary<string, int> ProductsByCategory { get; set; } = new();
}
