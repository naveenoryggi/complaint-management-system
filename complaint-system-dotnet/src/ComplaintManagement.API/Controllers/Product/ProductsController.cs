using ComplaintManagement.API.Authorization;
using ComplaintManagement.Application.DTOs.Product;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ComplaintManagement.API.Controllers.Product;

/// <summary>
/// Controller for managing products and product categories
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[RequiresLicense(LicenseModule.CRM_Product)]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    private Guid GetCompanyId() =>
        Guid.Parse(User.FindFirstValue("CompanyId") ?? throw new UnauthorizedAccessException("Company ID not found"));

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new UnauthorizedAccessException("User ID not found"));

    #region Categories

    /// <summary>
    /// Gets the category tree for the company
    /// </summary>
    [HttpGet("categories/tree")]
    public async Task<IActionResult> GetCategoryTree([FromQuery] bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetCategoryTreeAsync(GetCompanyId(), includeInactive, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets categories with optional filtering
    /// </summary>
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(
        [FromQuery] Guid? parentId = null,
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetCategoriesAsync(GetCompanyId(), parentId, includeInactive, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a category by ID
    /// </summary>
    [HttpGet("categories/{id:guid}")]
    public async Task<IActionResult> GetCategoryById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetCategoryByIdAsync(GetCompanyId(), id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a category by code
    /// </summary>
    [HttpGet("categories/by-code/{code}")]
    public async Task<IActionResult> GetCategoryByCode(string code, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetCategoryByCodeAsync(GetCompanyId(), code, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Creates a new category
    /// </summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.CreateCategoryAsync(GetCompanyId(), request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(GetCategoryById), new { id = result.Data!.Id }, new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Updates an existing category
    /// </summary>
    [HttpPut("categories/{id:guid}")]
    public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] UpdateProductCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.UpdateCategoryAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Moves a category to a new parent
    /// </summary>
    [HttpPost("categories/{id:guid}/move")]
    public async Task<IActionResult> MoveCategory(Guid id, [FromBody] MoveCategoryRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _productService.MoveCategoryAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Deletes a category (soft delete)
    /// </summary>
    [HttpDelete("categories/{id:guid}")]
    public async Task<IActionResult> DeleteCategory(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.DeleteCategoryAsync(GetCompanyId(), id, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = "Category deleted successfully" });
    }

    /// <summary>
    /// Gets category lookup for dropdowns
    /// </summary>
    [HttpGet("categories/lookup")]
    public async Task<IActionResult> GetCategoryLookup([FromQuery] bool allowProducts = true, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetCategoryLookupAsync(GetCompanyId(), allowProducts, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    #endregion

    #region Products

    /// <summary>
    /// Gets products with filtering and pagination
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductSearchRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductsAsync(GetCompanyId(), request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a product by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductByIdAsync(GetCompanyId(), id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a product by code
    /// </summary>
    [HttpGet("by-code/{code}")]
    public async Task<IActionResult> GetProductByCode(string code, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductByCodeAsync(GetCompanyId(), code, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a product by SKU
    /// </summary>
    [HttpGet("by-sku/{sku}")]
    public async Task<IActionResult> GetProductBySKU(string sku, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductBySKUAsync(GetCompanyId(), sku, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Creates a new product
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.CreateProductAsync(GetCompanyId(), request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Updates an existing product
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.UpdateProductAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Deletes a product (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.DeleteProductAsync(GetCompanyId(), id, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = "Product deleted successfully" });
    }

    /// <summary>
    /// Clones a product
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    public async Task<IActionResult> CloneProduct(Guid id, [FromBody] CloneProductRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.CloneProductAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(GetProductById), new { id = result.Data!.Id }, new { isSuccess = true, data = result.Data });
    }

    // NOTE: Inventory updates are now managed through the Stock Management module.
    // Use POST /api/StockMovements to create stock adjustments, receipts, issues, etc.
    // This provides location-based tracking, movement history, and full audit trails.

    /// <summary>
    /// Gets product lookup for dropdowns
    /// </summary>
    [HttpGet("lookup")]
    public async Task<IActionResult> GetProductLookup(
        [FromQuery] string? searchTerm = null,
        [FromQuery] Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductLookupAsync(GetCompanyId(), searchTerm, categoryId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets products by category
    /// </summary>
    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<IActionResult> GetProductsByCategory(
        Guid categoryId,
        [FromQuery] bool includeSubCategories = true,
        CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductsByCategoryAsync(GetCompanyId(), categoryId, includeSubCategories, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets product variants
    /// </summary>
    [HttpGet("{id:guid}/variants")]
    public async Task<IActionResult> GetProductVariants(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductVariantsAsync(GetCompanyId(), id, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    #endregion

    #region Price Lists

    /// <summary>
    /// Gets price lists for a product
    /// </summary>
    [HttpGet("{productId:guid}/price-lists")]
    public async Task<IActionResult> GetProductPriceLists(Guid productId, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductPriceListsAsync(GetCompanyId(), productId, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Gets a price list by ID
    /// </summary>
    [HttpGet("price-lists/{id:guid}")]
    public async Task<IActionResult> GetPriceListById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetPriceListByIdAsync(GetCompanyId(), id, cancellationToken);
        if (!result.IsSuccess)
            return NotFound(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Creates a new price list
    /// </summary>
    [HttpPost("price-lists")]
    public async Task<IActionResult> CreatePriceList([FromBody] CreateProductPriceListRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.CreatePriceListAsync(GetCompanyId(), request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return CreatedAtAction(nameof(GetPriceListById), new { id = result.Data!.Id }, new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Updates an existing price list
    /// </summary>
    [HttpPut("price-lists/{id:guid}")]
    public async Task<IActionResult> UpdatePriceList(Guid id, [FromBody] UpdateProductPriceListRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.UpdatePriceListAsync(GetCompanyId(), id, request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Deletes a price list
    /// </summary>
    [HttpDelete("price-lists/{id:guid}")]
    public async Task<IActionResult> DeletePriceList(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await _productService.DeletePriceListAsync(GetCompanyId(), id, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, message = "Price list deleted successfully" });
    }

    /// <summary>
    /// Calculates price for a product based on context
    /// </summary>
    [HttpPost("calculate-price")]
    public async Task<IActionResult> CalculatePrice([FromBody] CalculatePriceRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.CalculatePriceAsync(GetCompanyId(), request, cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = result.Data });
    }

    /// <summary>
    /// Bulk update prices
    /// </summary>
    [HttpPost("bulk-update-prices")]
    public async Task<IActionResult> BulkUpdatePrices([FromBody] BulkPriceUpdateRequest request, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { isSuccess = false, message = "Invalid request data" });

        var result = await _productService.BulkUpdatePricesAsync(GetCompanyId(), request, GetUserId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });

        return Ok(new { isSuccess = true, data = new { updatedCount = result.Data } });
    }

    #endregion

    #region Statistics

    /// <summary>
    /// Gets product statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken = default)
    {
        var result = await _productService.GetProductStatisticsAsync(GetCompanyId(), cancellationToken);
        if (!result.IsSuccess)
            return BadRequest(new { isSuccess = false, message = result.Message });
        return Ok(new { isSuccess = true, data = result.Data });
    }

    #endregion
}
