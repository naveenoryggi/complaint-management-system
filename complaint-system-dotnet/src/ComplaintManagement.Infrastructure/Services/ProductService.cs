using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Product;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Product;
using ComplaintManagement.Domain.Enums.Product;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing products and product categories
/// </summary>
public class ProductService : IProductService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ComplaintDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Product Categories

    public async Task<Result<List<ProductCategoryTreeDto>>> GetCategoryTreeAsync(
        Guid companyId,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ProductCategories
                .Where(c => c.CompanyId == companyId && c.ParentCategoryId == null);

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            var rootCategories = await query
                .Include(c => c.SubCategories)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .ToListAsync(cancellationToken);

            var result = rootCategories.Select(c => MapToCategoryTree(c, includeInactive)).ToList();
            return Result<List<ProductCategoryTreeDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category tree for company {CompanyId}", companyId);
            return Result<List<ProductCategoryTreeDto>>.Failure("Failed to retrieve category tree");
        }
    }

    public async Task<Result<List<ProductCategorySummaryDto>>> GetCategoriesAsync(
        Guid companyId,
        Guid? parentId = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ProductCategories
                .Where(c => c.CompanyId == companyId);

            if (parentId.HasValue)
                query = query.Where(c => c.ParentCategoryId == parentId.Value);
            else
                query = query.Where(c => c.ParentCategoryId == null);

            if (!includeInactive)
                query = query.Where(c => c.IsActive);

            var categories = await query
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Select(c => new ProductCategorySummaryDto
                {
                    Id = c.Id,
                    ParentCategoryId = c.ParentCategoryId,
                    Code = c.Code,
                    Name = c.Name,
                    FullPath = c.FullPath,
                    Level = c.Level,
                    Icon = c.Icon,
                    Color = c.Color,
                    IsActive = c.IsActive,
                    ProductCount = c.Products.Count(p => !p.IsDeleted),
                    SubCategoryCount = c.SubCategories.Count(s => !s.IsDeleted)
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductCategorySummaryDto>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories for company {CompanyId}", companyId);
            return Result<List<ProductCategorySummaryDto>>.Failure("Failed to retrieve categories");
        }
    }

    public async Task<Result<ProductCategoryDto>> GetCategoryByIdAsync(
        Guid companyId,
        Guid categoryId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.CompanyId == companyId, cancellationToken);

            if (category == null)
                return Result<ProductCategoryDto>.Failure("Category not found");

            return Result<ProductCategoryDto>.Success(MapToCategoryDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category {CategoryId}", categoryId);
            return Result<ProductCategoryDto>.Failure("Failed to retrieve category");
        }
    }

    public async Task<Result<ProductCategoryDto>> GetCategoryByCodeAsync(
        Guid companyId,
        string code,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.ParentCategory)
                .Include(c => c.SubCategories.Where(s => !s.IsDeleted))
                .FirstOrDefaultAsync(c => c.Code == code && c.CompanyId == companyId, cancellationToken);

            if (category == null)
                return Result<ProductCategoryDto>.Failure("Category not found");

            return Result<ProductCategoryDto>.Success(MapToCategoryDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category by code {Code}", code);
            return Result<ProductCategoryDto>.Failure("Failed to retrieve category");
        }
    }

    public async Task<Result<ProductCategoryDto>> CreateCategoryAsync(
        Guid companyId,
        CreateProductCategoryRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate code
            var exists = await _context.ProductCategories
                .AnyAsync(c => c.CompanyId == companyId && c.Code == request.Code, cancellationToken);

            if (exists)
                return Result<ProductCategoryDto>.Failure($"Category with code '{request.Code}' already exists");

            // Get parent category if specified
            ProductCategory? parentCategory = null;
            int level = 0;
            string path = "/";
            string fullPath = request.Name;

            if (request.ParentCategoryId.HasValue)
            {
                parentCategory = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == request.ParentCategoryId.Value && c.CompanyId == companyId, cancellationToken);

                if (parentCategory == null)
                    return Result<ProductCategoryDto>.Failure("Parent category not found");

                if (!parentCategory.AllowSubCategories)
                    return Result<ProductCategoryDto>.Failure("Parent category does not allow subcategories");

                level = parentCategory.Level + 1;
                path = $"{parentCategory.Path}{parentCategory.Id}/";
                fullPath = $"{parentCategory.FullPath} > {request.Name}";
            }

            var category = new ProductCategory
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ParentCategoryId = request.ParentCategoryId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Level = level,
                Path = path,
                FullPath = fullPath,
                DisplayOrder = request.DisplayOrder,
                Icon = request.Icon,
                Color = request.Color,
                ImageUrl = request.ImageUrl,
                ThumbnailUrl = request.ThumbnailUrl,
                DefaultWarrantyMonths = request.DefaultWarrantyMonths,
                DefaultResponseTimeHours = request.DefaultResponseTimeHours,
                DefaultResolutionTimeHours = request.DefaultResolutionTimeHours,
                DefaultSLASettingsId = request.DefaultSLASettingsId,
                DefaultTaxRate = request.DefaultTaxRate,
                DefaultHSNCode = request.DefaultHSNCode,
                DefaultSACCode = request.DefaultSACCode,
                RequireSerialNumber = request.RequireSerialNumber,
                TrackInventory = request.TrackInventory,
                IsPublic = request.IsPublic,
                AllowProducts = request.AllowProducts,
                AllowSubCategories = request.AllowSubCategories,
                IsActive = true,
                Slug = request.Slug ?? GenerateSlug(request.Name),
                MetaTitle = request.MetaTitle,
                MetaDescription = request.MetaDescription,
                Keywords = request.Keywords,
                ExternalCategoryId = request.ExternalCategoryId,
                Notes = request.Notes,
                CreatedBy = createdBy
            };

            _context.ProductCategories.Add(category);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created category {CategoryId} for company {CompanyId}", category.Id, companyId);
            return Result<ProductCategoryDto>.Success(MapToCategoryDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating category for company {CompanyId}", companyId);
            return Result<ProductCategoryDto>.Failure("Failed to create category");
        }
    }

    public async Task<Result<ProductCategoryDto>> UpdateCategoryAsync(
        Guid companyId,
        Guid categoryId,
        UpdateProductCategoryRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.CompanyId == companyId, cancellationToken);

            if (category == null)
                return Result<ProductCategoryDto>.Failure("Category not found");

            // Update fields
            category.Name = request.Name;
            category.Description = request.Description;
            category.DisplayOrder = request.DisplayOrder;
            category.Icon = request.Icon;
            category.Color = request.Color;
            category.ImageUrl = request.ImageUrl;
            category.ThumbnailUrl = request.ThumbnailUrl;
            category.DefaultWarrantyMonths = request.DefaultWarrantyMonths;
            category.DefaultResponseTimeHours = request.DefaultResponseTimeHours;
            category.DefaultResolutionTimeHours = request.DefaultResolutionTimeHours;
            category.DefaultSLASettingsId = request.DefaultSLASettingsId;
            category.DefaultTaxRate = request.DefaultTaxRate;
            category.DefaultHSNCode = request.DefaultHSNCode;
            category.DefaultSACCode = request.DefaultSACCode;
            category.RequireSerialNumber = request.RequireSerialNumber;
            category.TrackInventory = request.TrackInventory;
            category.IsPublic = request.IsPublic;
            category.AllowProducts = request.AllowProducts;
            category.AllowSubCategories = request.AllowSubCategories;
            category.IsActive = request.IsActive;
            category.Slug = request.Slug ?? GenerateSlug(request.Name);
            category.MetaTitle = request.MetaTitle;
            category.MetaDescription = request.MetaDescription;
            category.Keywords = request.Keywords;
            category.ExternalCategoryId = request.ExternalCategoryId;
            category.Notes = request.Notes;
            category.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated category {CategoryId}", categoryId);
            return Result<ProductCategoryDto>.Success(MapToCategoryDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating category {CategoryId}", categoryId);
            return Result<ProductCategoryDto>.Failure("Failed to update category");
        }
    }

    public async Task<Result<ProductCategoryDto>> MoveCategoryAsync(
        Guid companyId,
        Guid categoryId,
        MoveCategoryRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _context.ProductCategories
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.CompanyId == companyId, cancellationToken);

            if (category == null)
                return Result<ProductCategoryDto>.Failure("Category not found");

            // Validate new parent
            if (request.NewParentCategoryId.HasValue)
            {
                // Can't move to itself or its descendants
                if (request.NewParentCategoryId.Value == categoryId)
                    return Result<ProductCategoryDto>.Failure("Cannot move category to itself");

                var newParent = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == request.NewParentCategoryId.Value && c.CompanyId == companyId, cancellationToken);

                if (newParent == null)
                    return Result<ProductCategoryDto>.Failure("New parent category not found");

                // Check if new parent is a descendant of current category
                if (newParent.Path?.Contains($"/{categoryId}/") == true)
                    return Result<ProductCategoryDto>.Failure("Cannot move category to its own descendant");

                category.ParentCategoryId = request.NewParentCategoryId;
                category.Level = newParent.Level + 1;
                category.Path = $"{newParent.Path}{newParent.Id}/";
                category.FullPath = $"{newParent.FullPath} > {category.Name}";
            }
            else
            {
                // Moving to root
                category.ParentCategoryId = null;
                category.Level = 0;
                category.Path = "/";
                category.FullPath = category.Name;
            }

            if (request.NewDisplayOrder.HasValue)
                category.DisplayOrder = request.NewDisplayOrder.Value;

            category.UpdatedBy = updatedBy;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Moved category {CategoryId} to parent {ParentId}", categoryId, request.NewParentCategoryId);
            return Result<ProductCategoryDto>.Success(MapToCategoryDto(category));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving category {CategoryId}", categoryId);
            return Result<ProductCategoryDto>.Failure("Failed to move category");
        }
    }

    public async Task<Result<bool>> DeleteCategoryAsync(
        Guid companyId,
        Guid categoryId,
        Guid deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var category = await _context.ProductCategories
                .Include(c => c.SubCategories)
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.CompanyId == companyId, cancellationToken);

            if (category == null)
                return Result<bool>.Failure("Category not found");

            // Check for children
            if (category.SubCategories.Any(s => !s.IsDeleted))
                return Result<bool>.Failure("Cannot delete category with subcategories");

            if (category.Products.Any(p => !p.IsDeleted))
                return Result<bool>.Failure("Cannot delete category with products");

            category.IsDeleted = true;
            category.DeletedAt = DateTime.UtcNow;
            category.DeletedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted category {CategoryId}", categoryId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting category {CategoryId}", categoryId);
            return Result<bool>.Failure("Failed to delete category");
        }
    }

    public async Task<Result<List<ProductCategoryLookupDto>>> GetCategoryLookupAsync(
        Guid companyId,
        bool allowProducts = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.ProductCategories
                .Where(c => c.CompanyId == companyId && c.IsActive);

            if (allowProducts)
                query = query.Where(c => c.AllowProducts);

            var categories = await query
                .OrderBy(c => c.FullPath)
                .Select(c => new ProductCategoryLookupDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    FullPath = c.FullPath,
                    Level = c.Level,
                    AllowProducts = c.AllowProducts
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductCategoryLookupDto>>.Success(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting category lookup for company {CompanyId}", companyId);
            return Result<List<ProductCategoryLookupDto>>.Failure("Failed to retrieve categories");
        }
    }

    #endregion

    #region Products

    public async Task<Result<PagedResult<ProductSummaryDto>>> GetProductsAsync(
        Guid companyId,
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.CompanyId == companyId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(p =>
                    p.ProductCode.ToLower().Contains(searchTerm) ||
                    p.Name.ToLower().Contains(searchTerm) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(searchTerm)) ||
                    (p.Description != null && p.Description.ToLower().Contains(searchTerm)));
            }

            if (request.CategoryId.HasValue)
            {
                if (request.IncludeSubCategories)
                {
                    var category = await _context.ProductCategories
                        .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value, cancellationToken);

                    if (category != null)
                    {
                        var categoryPath = $"{category.Path}{category.Id}/";
                        var categoryIds = await _context.ProductCategories
                            .Where(c => c.CompanyId == companyId && (c.Id == request.CategoryId.Value || c.Path!.StartsWith(categoryPath)))
                            .Select(c => c.Id)
                            .ToListAsync(cancellationToken);

                        query = query.Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value));
                    }
                }
                else
                {
                    query = query.Where(p => p.CategoryId == request.CategoryId.Value);
                }
            }

            if (request.Type.HasValue)
                query = query.Where(p => p.Type == request.Type.Value);

            if (request.Status.HasValue)
                query = query.Where(p => p.Status == request.Status.Value);

            if (!string.IsNullOrWhiteSpace(request.Brand))
                query = query.Where(p => p.Brand == request.Brand);

            if (!string.IsNullOrWhiteSpace(request.Manufacturer))
                query = query.Where(p => p.Manufacturer == request.Manufacturer);

            if (request.MinPrice.HasValue)
                query = query.Where(p => p.UnitPrice >= request.MinPrice.Value);

            if (request.MaxPrice.HasValue)
                query = query.Where(p => p.UnitPrice <= request.MaxPrice.Value);

            if (request.InStock.HasValue)
            {
                // Stock quantities are now in StockItems, filter based on stock availability
                // Use actual columns (QuantityOnHand - QuantityReserved) since QuantityAvailable is a computed property
                if (request.InStock.Value)
                    query = query.Where(p => !p.TrackInventory || p.StockItems.Any(s => (s.QuantityOnHand - s.QuantityReserved) > 0) || p.AllowBackorder);
                else
                    query = query.Where(p => p.TrackInventory && !p.StockItems.Any(s => (s.QuantityOnHand - s.QuantityReserved) > 0) && !p.AllowBackorder);
            }

            if (request.IsFeatured.HasValue)
                query = query.Where(p => p.IsFeatured == request.IsFeatured.Value);

            if (request.IsPublic.HasValue)
                query = query.Where(p => p.IsPublic == request.IsPublic.Value);

            // Get total count
            var totalCount = await query.CountAsync(cancellationToken);

            // Apply sorting
            query = request.SortBy?.ToLower() switch
            {
                "name" => request.SortDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "code" => request.SortDescending ? query.OrderByDescending(p => p.ProductCode) : query.OrderBy(p => p.ProductCode),
                "price" => request.SortDescending ? query.OrderByDescending(p => p.UnitPrice) : query.OrderBy(p => p.UnitPrice),
                "status" => request.SortDescending ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status),
                "created" => request.SortDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // Apply pagination
            var products = await query
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    SKU = p.SKU,
                    Type = p.Type,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Brand = p.Brand,
                    UnitPrice = p.UnitPrice,
                    Currency = p.Currency,
                    Status = p.Status,
                    TrackInventory = p.TrackInventory,
                    // Use actual columns since QuantityAvailable is a computed property
                    InStock = !p.TrackInventory || p.StockItems.Any(s => (s.QuantityOnHand - s.QuantityReserved) > 0) || p.AllowBackorder,
                    TotalQuantityAvailable = p.StockItems.Sum(s => s.QuantityOnHand - s.QuantityReserved),
                    ThumbnailUrl = p.ThumbnailUrl,
                    IsFeatured = p.IsFeatured,
                    IsPublic = p.IsPublic
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ProductSummaryDto>
            {
                Items = products,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };

            return Result<PagedResult<ProductSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products for company {CompanyId}", companyId);
            return Result<PagedResult<ProductSummaryDto>>.Failure("Failed to retrieve products");
        }
    }

    public async Task<Result<ProductDto>> GetProductByIdAsync(
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ParentProduct)
                .Include(p => p.Variants.Where(v => !v.IsDeleted))
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found");

            return Result<ProductDto>.Success(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", productId);
            return Result<ProductDto>.Failure("Failed to retrieve product");
        }
    }

    public async Task<Result<ProductDto>> GetProductByCodeAsync(
        Guid companyId,
        string productCode,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductCode == productCode && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found");

            return Result<ProductDto>.Success(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by code {ProductCode}", productCode);
            return Result<ProductDto>.Failure("Failed to retrieve product");
        }
    }

    public async Task<Result<ProductDto>> GetProductBySKUAsync(
        Guid companyId,
        string sku,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.SKU == sku && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found");

            return Result<ProductDto>.Success(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by SKU {SKU}", sku);
            return Result<ProductDto>.Failure("Failed to retrieve product");
        }
    }

    public async Task<Result<ProductDto>> CreateProductAsync(
        Guid companyId,
        CreateProductRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate code
            var exists = await _context.Products
                .AnyAsync(p => p.CompanyId == companyId && p.ProductCode == request.ProductCode, cancellationToken);

            if (exists)
                return Result<ProductDto>.Failure($"Product with code '{request.ProductCode}' already exists");

            // Validate category if specified
            if (request.CategoryId.HasValue)
            {
                var category = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == request.CategoryId.Value && c.CompanyId == companyId, cancellationToken);

                if (category == null)
                    return Result<ProductDto>.Failure("Category not found");

                if (!category.AllowProducts)
                    return Result<ProductDto>.Failure("Selected category does not allow products");
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CategoryId = request.CategoryId,
                ParentProductId = request.ParentProductId,
                ProductCode = request.ProductCode,
                Name = request.Name,
                ShortDescription = request.ShortDescription,
                Description = request.Description,
                Type = request.Type,
                SKU = request.SKU,
                PartNumber = request.PartNumber,
                UPC = request.UPC,
                EAN = request.EAN,
                HSNCode = request.HSNCode,
                SACCode = request.SACCode,
                UnitPrice = request.UnitPrice,
                CostPrice = request.CostPrice,
                MSRP = request.MSRP,
                Currency = request.Currency,
                TaxRate = request.TaxRate,
                IsTaxable = request.IsTaxable,
                TaxType = request.TaxType,
                UnitOfMeasure = request.UnitOfMeasure,
                CustomUnitName = request.CustomUnitName,
                MinOrderQuantity = request.MinOrderQuantity,
                MaxOrderQuantity = request.MaxOrderQuantity,
                QuantityIncrement = request.QuantityIncrement,
                TrackInventory = request.TrackInventory,
                AllowBackorder = request.AllowBackorder,
                DefaultLocationId = request.DefaultLocationId,
                DefaultStockCategoryId = request.DefaultStockCategoryId,
                Brand = request.Brand,
                Manufacturer = request.Manufacturer,
                Model = request.Model,
                Version = request.Version,
                Specifications = request.Specifications,
                Features = request.Features,
                Dimensions = request.Dimensions,
                Weight = request.Weight,
                CountryOfOrigin = request.CountryOfOrigin,
                ImageUrl = request.ImageUrl,
                ThumbnailUrl = request.ThumbnailUrl,
                Images = request.Images,
                Videos = request.Videos,
                Documents = request.Documents,
                DefaultWarrantyMonths = request.DefaultWarrantyMonths,
                ExtendedWarrantyMonths = request.ExtendedWarrantyMonths,
                WarrantyTerms = request.WarrantyTerms,
                DefaultSLASettingsId = request.DefaultSLASettingsId,
                IsServiceable = request.IsServiceable,
                RequiresInstallation = request.RequiresInstallation,
                IsSubscription = request.IsSubscription,
                BillingFrequency = request.BillingFrequency,
                TrialPeriodDays = request.TrialPeriodDays,
                SetupFee = request.SetupFee,
                AutoRenew = request.AutoRenew,
                Status = request.Status,
                LaunchDate = request.LaunchDate,
                EndOfSaleDate = request.EndOfSaleDate,
                EndOfLifeDate = request.EndOfLifeDate,
                EndOfSupportDate = request.EndOfSupportDate,
                IsFeatured = request.IsFeatured,
                IsPublic = request.IsPublic,
                RequireSerialNumber = request.RequireSerialNumber,
                Slug = request.Slug ?? GenerateSlug(request.Name),
                MetaTitle = request.MetaTitle,
                MetaDescription = request.MetaDescription,
                Tags = request.Tags,
                Keywords = request.Keywords,
                ExternalProductId = request.ExternalProductId,
                VendorProductId = request.VendorProductId,
                VendorName = request.VendorName,
                CustomFields = request.CustomFields,
                Notes = request.Notes,
                CreatedBy = createdBy
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created product {ProductId} for company {CompanyId}", product.Id, companyId);
            return Result<ProductDto>.Success(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product for company {CompanyId}", companyId);
            return Result<ProductDto>.Failure("Failed to create product");
        }
    }

    public async Task<Result<ProductDto>> UpdateProductAsync(
        Guid companyId,
        Guid productId,
        UpdateProductRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<ProductDto>.Failure("Product not found");

            // Update fields
            product.CategoryId = request.CategoryId;
            product.Name = request.Name;
            product.ShortDescription = request.ShortDescription;
            product.Description = request.Description;
            product.Type = request.Type;
            product.SKU = request.SKU;
            product.PartNumber = request.PartNumber;
            product.UPC = request.UPC;
            product.EAN = request.EAN;
            product.HSNCode = request.HSNCode;
            product.SACCode = request.SACCode;
            product.UnitPrice = request.UnitPrice;
            product.CostPrice = request.CostPrice;
            product.MSRP = request.MSRP;
            product.MinimumPrice = request.MinimumPrice;
            product.Currency = request.Currency;
            product.TaxRate = request.TaxRate;
            product.IsTaxable = request.IsTaxable;
            product.TaxType = request.TaxType;
            product.UnitOfMeasure = request.UnitOfMeasure;
            product.CustomUnitName = request.CustomUnitName;
            product.MinOrderQuantity = request.MinOrderQuantity;
            product.MaxOrderQuantity = request.MaxOrderQuantity;
            product.QuantityIncrement = request.QuantityIncrement;
            product.TrackInventory = request.TrackInventory;
            product.AllowBackorder = request.AllowBackorder;
            product.DefaultLocationId = request.DefaultLocationId;
            product.DefaultStockCategoryId = request.DefaultStockCategoryId;
            product.Brand = request.Brand;
            product.Manufacturer = request.Manufacturer;
            product.Model = request.Model;
            product.Version = request.Version;
            product.Specifications = request.Specifications;
            product.Features = request.Features;
            product.Dimensions = request.Dimensions;
            product.Weight = request.Weight;
            product.CountryOfOrigin = request.CountryOfOrigin;
            product.ImageUrl = request.ImageUrl;
            product.ThumbnailUrl = request.ThumbnailUrl;
            product.Images = request.Images;
            product.Videos = request.Videos;
            product.Documents = request.Documents;
            product.DefaultWarrantyMonths = request.DefaultWarrantyMonths;
            product.ExtendedWarrantyMonths = request.ExtendedWarrantyMonths;
            product.WarrantyTerms = request.WarrantyTerms;
            product.DefaultSLASettingsId = request.DefaultSLASettingsId;
            product.SupportUrl = request.SupportUrl;
            product.IsServiceable = request.IsServiceable;
            product.RequiresInstallation = request.RequiresInstallation;
            product.IsSubscription = request.IsSubscription;
            product.BillingFrequency = request.BillingFrequency;
            product.TrialPeriodDays = request.TrialPeriodDays;
            product.SetupFee = request.SetupFee;
            product.AutoRenew = request.AutoRenew;
            product.CancellationPolicy = request.CancellationPolicy;
            product.Status = request.Status;
            product.StatusReason = request.StatusReason;
            product.LaunchDate = request.LaunchDate;
            product.EndOfSaleDate = request.EndOfSaleDate;
            product.EndOfLifeDate = request.EndOfLifeDate;
            product.EndOfSupportDate = request.EndOfSupportDate;
            product.IsFeatured = request.IsFeatured;
            product.IsPublic = request.IsPublic;
            product.IsConfigurable = request.IsConfigurable;
            product.RequireSerialNumber = request.RequireSerialNumber;
            product.Slug = request.Slug ?? GenerateSlug(request.Name);
            product.MetaTitle = request.MetaTitle;
            product.MetaDescription = request.MetaDescription;
            product.Tags = request.Tags;
            product.Keywords = request.Keywords;
            product.ExternalProductId = request.ExternalProductId;
            product.VendorProductId = request.VendorProductId;
            product.VendorName = request.VendorName;
            product.CustomFields = request.CustomFields;
            product.Notes = request.Notes;
            product.ReplacementProductId = request.ReplacementProductId;
            product.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated product {ProductId}", productId);
            return Result<ProductDto>.Success(MapToProductDto(product));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", productId);
            return Result<ProductDto>.Failure("Failed to update product");
        }
    }

    public async Task<Result<bool>> DeleteProductAsync(
        Guid companyId,
        Guid productId,
        Guid deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<bool>.Failure("Product not found");

            // Check for variants
            if (product.Variants.Any(v => !v.IsDeleted))
                return Result<bool>.Failure("Cannot delete product with variants");

            product.IsDeleted = true;
            product.DeletedAt = DateTime.UtcNow;
            product.DeletedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted product {ProductId}", productId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", productId);
            return Result<bool>.Failure("Failed to delete product");
        }
    }

    public async Task<Result<ProductDto>> CloneProductAsync(
        Guid companyId,
        Guid productId,
        CloneProductRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var source = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.CompanyId == companyId, cancellationToken);

            if (source == null)
                return Result<ProductDto>.Failure("Source product not found");

            // Check for duplicate code
            var exists = await _context.Products
                .AnyAsync(p => p.CompanyId == companyId && p.ProductCode == request.NewProductCode, cancellationToken);

            if (exists)
                return Result<ProductDto>.Failure($"Product with code '{request.NewProductCode}' already exists");

            var clone = new Product
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CategoryId = source.CategoryId,
                ParentProductId = request.CloneAsVariant ? productId : null,
                ProductCode = request.NewProductCode,
                Name = request.NewName,
                ShortDescription = source.ShortDescription,
                Description = source.Description,
                Type = source.Type,
                SKU = request.NewSKU,
                PartNumber = source.PartNumber,
                HSNCode = source.HSNCode,
                SACCode = source.SACCode,
                UnitOfMeasure = source.UnitOfMeasure,
                CustomUnitName = source.CustomUnitName,
                MinOrderQuantity = source.MinOrderQuantity,
                MaxOrderQuantity = source.MaxOrderQuantity,
                TrackInventory = source.TrackInventory,
                AllowBackorder = source.AllowBackorder,
                DefaultLocationId = source.DefaultLocationId,
                DefaultStockCategoryId = source.DefaultStockCategoryId,
                Brand = source.Brand,
                Manufacturer = source.Manufacturer,
                Model = source.Model,
                Specifications = source.Specifications,
                Features = source.Features,
                Dimensions = source.Dimensions,
                Weight = source.Weight,
                CountryOfOrigin = source.CountryOfOrigin,
                DefaultWarrantyMonths = source.DefaultWarrantyMonths,
                ExtendedWarrantyMonths = source.ExtendedWarrantyMonths,
                WarrantyTerms = source.WarrantyTerms,
                DefaultSLASettingsId = source.DefaultSLASettingsId,
                IsServiceable = source.IsServiceable,
                RequiresInstallation = source.RequiresInstallation,
                IsSubscription = source.IsSubscription,
                BillingFrequency = source.BillingFrequency,
                TrialPeriodDays = source.TrialPeriodDays,
                Status = ProductStatus.Draft,
                IsFeatured = false,
                IsPublic = source.IsPublic,
                RequireSerialNumber = source.RequireSerialNumber,
                Slug = GenerateSlug(request.NewName),
                Tags = source.Tags,
                Keywords = source.Keywords,
                CreatedBy = createdBy
            };

            if (request.ClonePricing)
            {
                clone.UnitPrice = source.UnitPrice;
                clone.CostPrice = source.CostPrice;
                clone.MSRP = source.MSRP;
                clone.Currency = source.Currency;
                clone.TaxRate = source.TaxRate;
                clone.IsTaxable = source.IsTaxable;
                clone.TaxType = source.TaxType;
            }

            if (request.CloneMedia)
            {
                clone.ImageUrl = source.ImageUrl;
                clone.ThumbnailUrl = source.ThumbnailUrl;
                clone.Images = source.Images;
                clone.Videos = source.Videos;
                clone.Documents = source.Documents;
            }

            _context.Products.Add(clone);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Cloned product {SourceId} to {CloneId}", productId, clone.Id);
            return Result<ProductDto>.Success(MapToProductDto(clone));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning product {ProductId}", productId);
            return Result<ProductDto>.Failure("Failed to clone product");
        }
    }

    // DEPRECATED: UpdateInventoryAsync has been removed.
    // Inventory is now managed through the Stock Management module.
    // Use IStockMovementService.CreateMovementAsync() to adjust stock quantities.
    // This provides location-based tracking, movement history, and full audit trails.

    public async Task<Result<List<ProductLookupDto>>> GetProductLookupAsync(
        Guid companyId,
        string? searchTerm = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Products
                .Where(p => p.CompanyId == companyId && p.Status == ProductStatus.Active);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(p =>
                    p.ProductCode.ToLower().Contains(term) ||
                    p.Name.ToLower().Contains(term) ||
                    (p.SKU != null && p.SKU.ToLower().Contains(term)));
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            var products = await query
                .Take(50)
                .Select(p => new ProductLookupDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    SKU = p.SKU,
                    Type = p.Type,
                    UnitPrice = p.UnitPrice,
                    Currency = p.Currency,
                    InStock = !p.TrackInventory || p.StockItems.Any(s => (s.QuantityOnHand - s.QuantityReserved) > 0) || p.AllowBackorder
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductLookupDto>>.Success(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product lookup for company {CompanyId}", companyId);
            return Result<List<ProductLookupDto>>.Failure("Failed to retrieve products");
        }
    }

    public async Task<Result<List<ProductSummaryDto>>> GetProductsByCategoryAsync(
        Guid companyId,
        Guid categoryId,
        bool includeSubCategories = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.CompanyId == companyId);

            if (includeSubCategories)
            {
                var category = await _context.ProductCategories
                    .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken);

                if (category != null)
                {
                    var categoryPath = $"{category.Path}{category.Id}/";
                    var categoryIds = await _context.ProductCategories
                        .Where(c => c.CompanyId == companyId && (c.Id == categoryId || c.Path!.StartsWith(categoryPath)))
                        .Select(c => c.Id)
                        .ToListAsync(cancellationToken);

                    query = query.Where(p => p.CategoryId.HasValue && categoryIds.Contains(p.CategoryId.Value));
                }
            }
            else
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            var products = await query
                .Select(p => new ProductSummaryDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    SKU = p.SKU,
                    Type = p.Type,
                    CategoryName = p.Category != null ? p.Category.Name : null,
                    Brand = p.Brand,
                    UnitPrice = p.UnitPrice,
                    Currency = p.Currency,
                    Status = p.Status,
                    TrackInventory = p.TrackInventory,
                    // Use actual columns since QuantityAvailable is a computed property
                    InStock = !p.TrackInventory || p.StockItems.Any(s => (s.QuantityOnHand - s.QuantityReserved) > 0) || p.AllowBackorder,
                    TotalQuantityAvailable = p.StockItems.Sum(s => s.QuantityOnHand - s.QuantityReserved),
                    ThumbnailUrl = p.ThumbnailUrl,
                    IsFeatured = p.IsFeatured,
                    IsPublic = p.IsPublic
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductSummaryDto>>.Success(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category {CategoryId}", categoryId);
            return Result<List<ProductSummaryDto>>.Failure("Failed to retrieve products");
        }
    }

    public async Task<Result<List<ProductVariantDto>>> GetProductVariantsAsync(
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var variants = await _context.Products
                .Where(p => p.CompanyId == companyId && p.ParentProductId == productId)
                .Select(p => new ProductVariantDto
                {
                    Id = p.Id,
                    ProductCode = p.ProductCode,
                    Name = p.Name,
                    SKU = p.SKU,
                    UnitPrice = p.UnitPrice,
                    InStock = !p.TrackInventory || p.StockItems.Any(s => (s.QuantityOnHand - s.QuantityReserved) > 0) || p.AllowBackorder,
                    Status = p.Status
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductVariantDto>>.Success(variants);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting variants for product {ProductId}", productId);
            return Result<List<ProductVariantDto>>.Failure("Failed to retrieve product variants");
        }
    }

    #endregion

    #region Product Price Lists

    public async Task<Result<List<ProductPriceListSummaryDto>>> GetProductPriceListsAsync(
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var priceLists = await _context.ProductPriceLists
                .Include(pl => pl.Product)
                .Include(pl => pl.Partner)
                .Include(pl => pl.Customer)
                .Where(pl => pl.ProductId == productId && pl.Product!.CompanyId == companyId)
                .Select(pl => new ProductPriceListSummaryDto
                {
                    Id = pl.Id,
                    ProductId = pl.ProductId,
                    ProductCode = pl.Product!.ProductCode,
                    Name = pl.Name,
                    Type = pl.Type,
                    TargetName = pl.Partner != null ? pl.Partner.Name : (pl.Customer != null ? pl.Customer.Name : null),
                    UnitPrice = pl.UnitPrice,
                    Currency = pl.Currency,
                    DiscountPercent = pl.DiscountPercent,
                    ValidFrom = pl.ValidFrom,
                    ValidTo = pl.ValidTo,
                    IsActive = pl.IsActive,
                    IsValid = pl.IsActive &&
                             (!pl.ValidFrom.HasValue || pl.ValidFrom <= DateTime.UtcNow) &&
                             (!pl.ValidTo.HasValue || pl.ValidTo >= DateTime.UtcNow),
                    Priority = pl.Priority
                })
                .OrderBy(pl => pl.Priority)
                .ToListAsync(cancellationToken);

            return Result<List<ProductPriceListSummaryDto>>.Success(priceLists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price lists for product {ProductId}", productId);
            return Result<List<ProductPriceListSummaryDto>>.Failure("Failed to retrieve price lists");
        }
    }

    public async Task<Result<ProductPriceListDto>> GetPriceListByIdAsync(
        Guid companyId,
        Guid priceListId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var priceList = await _context.ProductPriceLists
                .Include(pl => pl.Product)
                .Include(pl => pl.Partner)
                .Include(pl => pl.Customer)
                .FirstOrDefaultAsync(pl => pl.Id == priceListId && pl.Product!.CompanyId == companyId, cancellationToken);

            if (priceList == null)
                return Result<ProductPriceListDto>.Failure("Price list not found");

            return Result<ProductPriceListDto>.Success(MapToPriceListDto(priceList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting price list {PriceListId}", priceListId);
            return Result<ProductPriceListDto>.Failure("Failed to retrieve price list");
        }
    }

    public async Task<Result<ProductPriceListDto>> CreatePriceListAsync(
        Guid companyId,
        CreateProductPriceListRequest request,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate product
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<ProductPriceListDto>.Failure("Product not found");

            var priceList = new ProductPriceList
            {
                Id = Guid.NewGuid(),
                ProductId = request.ProductId,
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                PartnerId = request.PartnerId,
                CustomerId = request.CustomerId,
                PartnerTier = request.PartnerTier,
                CustomerSegment = request.CustomerSegment,
                UnitPrice = request.UnitPrice,
                Currency = request.Currency,
                DiscountPercent = request.DiscountPercent,
                DiscountAmount = request.DiscountAmount,
                MarkupPercent = request.MarkupPercent,
                MinQuantity = request.MinQuantity,
                MaxQuantity = request.MaxQuantity,
                TierPricing = request.TierPricing,
                ValidFrom = request.ValidFrom,
                ValidTo = request.ValidTo,
                IsActive = true,
                Priority = request.Priority,
                MinOrderValue = request.MinOrderValue,
                PromoCode = request.PromoCode,
                ContractId = request.ContractId,
                Conditions = request.Conditions,
                Notes = request.Notes,
                CreatedBy = createdBy
            };

            _context.ProductPriceLists.Add(priceList);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created price list {PriceListId} for product {ProductId}", priceList.Id, request.ProductId);
            return Result<ProductPriceListDto>.Success(MapToPriceListDto(priceList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating price list for product {ProductId}", request.ProductId);
            return Result<ProductPriceListDto>.Failure("Failed to create price list");
        }
    }

    public async Task<Result<ProductPriceListDto>> UpdatePriceListAsync(
        Guid companyId,
        Guid priceListId,
        UpdateProductPriceListRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var priceList = await _context.ProductPriceLists
                .Include(pl => pl.Product)
                .FirstOrDefaultAsync(pl => pl.Id == priceListId && pl.Product!.CompanyId == companyId, cancellationToken);

            if (priceList == null)
                return Result<ProductPriceListDto>.Failure("Price list not found");

            priceList.Name = request.Name;
            priceList.Description = request.Description;
            priceList.Type = request.Type;
            priceList.PartnerId = request.PartnerId;
            priceList.CustomerId = request.CustomerId;
            priceList.PartnerTier = request.PartnerTier;
            priceList.CustomerSegment = request.CustomerSegment;
            priceList.UnitPrice = request.UnitPrice;
            priceList.Currency = request.Currency;
            priceList.DiscountPercent = request.DiscountPercent;
            priceList.DiscountAmount = request.DiscountAmount;
            priceList.MarkupPercent = request.MarkupPercent;
            priceList.MinQuantity = request.MinQuantity;
            priceList.MaxQuantity = request.MaxQuantity;
            priceList.TierPricing = request.TierPricing;
            priceList.ValidFrom = request.ValidFrom;
            priceList.ValidTo = request.ValidTo;
            priceList.IsActive = request.IsActive;
            priceList.Priority = request.Priority;
            priceList.MinOrderValue = request.MinOrderValue;
            priceList.PromoCode = request.PromoCode;
            priceList.ContractId = request.ContractId;
            priceList.Conditions = request.Conditions;
            priceList.Notes = request.Notes;
            priceList.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated price list {PriceListId}", priceListId);
            return Result<ProductPriceListDto>.Success(MapToPriceListDto(priceList));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating price list {PriceListId}", priceListId);
            return Result<ProductPriceListDto>.Failure("Failed to update price list");
        }
    }

    public async Task<Result<bool>> DeletePriceListAsync(
        Guid companyId,
        Guid priceListId,
        Guid deletedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var priceList = await _context.ProductPriceLists
                .Include(pl => pl.Product)
                .FirstOrDefaultAsync(pl => pl.Id == priceListId && pl.Product!.CompanyId == companyId, cancellationToken);

            if (priceList == null)
                return Result<bool>.Failure("Price list not found");

            priceList.IsDeleted = true;
            priceList.DeletedAt = DateTime.UtcNow;
            priceList.DeletedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted price list {PriceListId}", priceListId);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting price list {PriceListId}", priceListId);
            return Result<bool>.Failure("Failed to delete price list");
        }
    }

    public async Task<Result<ProductPriceDto>> CalculatePriceAsync(
        Guid companyId,
        CalculatePriceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.CompanyId == companyId, cancellationToken);

            if (product == null)
                return Result<ProductPriceDto>.Failure("Product not found");

            var basePrice = product.UnitPrice ?? 0;
            var result = new ProductPriceDto
            {
                ProductId = product.Id,
                ProductCode = product.ProductCode,
                ProductName = product.Name,
                BasePrice = basePrice,
                Currency = product.Currency,
                Quantity = request.Quantity,
                FinalPrice = basePrice,
                TotalPrice = basePrice * request.Quantity
            };

            // Find applicable price list
            var now = DateTime.UtcNow;
            var priceListQuery = _context.ProductPriceLists
                .Where(pl => pl.ProductId == request.ProductId &&
                             pl.IsActive &&
                             (!pl.ValidFrom.HasValue || pl.ValidFrom <= now) &&
                             (!pl.ValidTo.HasValue || pl.ValidTo >= now));

            // Filter by customer/partner
            if (request.CustomerId.HasValue)
                priceListQuery = priceListQuery.Where(pl => pl.CustomerId == request.CustomerId || pl.CustomerId == null);

            if (request.PartnerId.HasValue)
                priceListQuery = priceListQuery.Where(pl => pl.PartnerId == request.PartnerId || pl.PartnerId == null);

            // Filter by quantity
            priceListQuery = priceListQuery.Where(pl =>
                (!pl.MinQuantity.HasValue || pl.MinQuantity <= request.Quantity) &&
                (!pl.MaxQuantity.HasValue || pl.MaxQuantity >= request.Quantity));

            // Filter by promo code
            if (!string.IsNullOrWhiteSpace(request.PromoCode))
            {
                priceListQuery = priceListQuery.Where(pl =>
                    pl.PromoCode == null || pl.PromoCode == request.PromoCode);
            }

            var applicablePriceList = await priceListQuery
                .OrderBy(pl => pl.Priority)
                .FirstOrDefaultAsync(cancellationToken);

            if (applicablePriceList != null)
            {
                result.AppliedPriceListId = applicablePriceList.Id;
                result.AppliedPriceListName = applicablePriceList.Name;
                result.AppliedPriceListType = applicablePriceList.Type;

                var unitPrice = applicablePriceList.UnitPrice;

                if (applicablePriceList.DiscountPercent.HasValue)
                {
                    result.DiscountPercent = applicablePriceList.DiscountPercent;
                    result.DiscountAmount = unitPrice * (applicablePriceList.DiscountPercent.Value / 100);
                    unitPrice -= result.DiscountAmount.Value;
                }
                else if (applicablePriceList.DiscountAmount.HasValue)
                {
                    result.DiscountAmount = applicablePriceList.DiscountAmount;
                    unitPrice -= applicablePriceList.DiscountAmount.Value;
                    result.DiscountPercent = basePrice > 0 ? (applicablePriceList.DiscountAmount.Value / basePrice) * 100 : 0;
                }

                result.FinalPrice = Math.Max(0, unitPrice);
                result.PromoCode = applicablePriceList.PromoCode;
                result.PromoApplied = !string.IsNullOrWhiteSpace(request.PromoCode) &&
                                      applicablePriceList.PromoCode == request.PromoCode;
            }

            // Calculate tax
            if (request.IncludeTax && product.IsTaxable && product.TaxRate.HasValue)
            {
                result.TaxRate = product.TaxRate;
                result.TaxAmount = result.FinalPrice * (product.TaxRate.Value / 100);
                result.PriceWithTax = result.FinalPrice + result.TaxAmount.Value;
            }

            result.TotalPrice = result.FinalPrice * request.Quantity;
            result.TotalPriceWithTax = (result.PriceWithTax ?? result.FinalPrice) * request.Quantity;

            return Result<ProductPriceDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating price for product {ProductId}", request.ProductId);
            return Result<ProductPriceDto>.Failure("Failed to calculate price");
        }
    }

    public async Task<Result<int>> BulkUpdatePricesAsync(
        Guid companyId,
        BulkPriceUpdateRequest request,
        Guid updatedBy,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!request.PercentageChange.HasValue && !request.FixedAmount.HasValue)
                return Result<int>.Failure("Either percentage change or fixed amount must be specified");

            var products = await _context.Products
                .Where(p => p.CompanyId == companyId && request.ProductIds.Contains(p.Id))
                .ToListAsync(cancellationToken);

            foreach (var product in products)
            {
                if (request.ApplyToBasePrice && product.UnitPrice.HasValue)
                {
                    if (request.PercentageChange.HasValue)
                        product.UnitPrice = product.UnitPrice * (1 + request.PercentageChange.Value / 100);
                    else if (request.FixedAmount.HasValue)
                        product.UnitPrice = product.UnitPrice + request.FixedAmount.Value;
                }

                if (request.ApplyToCostPrice && product.CostPrice.HasValue)
                {
                    if (request.PercentageChange.HasValue)
                        product.CostPrice = product.CostPrice * (1 + request.PercentageChange.Value / 100);
                    else if (request.FixedAmount.HasValue)
                        product.CostPrice = product.CostPrice + request.FixedAmount.Value;
                }

                if (request.ApplyToMSRP && product.MSRP.HasValue)
                {
                    if (request.PercentageChange.HasValue)
                        product.MSRP = product.MSRP * (1 + request.PercentageChange.Value / 100);
                    else if (request.FixedAmount.HasValue)
                        product.MSRP = product.MSRP + request.FixedAmount.Value;
                }

                product.UpdatedBy = updatedBy;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Bulk updated prices for {Count} products: {Reason}", products.Count, request.Reason);
            return Result<int>.Success(products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating prices");
            return Result<int>.Failure("Failed to update prices");
        }
    }

    #endregion

    #region Statistics

    public async Task<Result<ProductStatisticsDto>> GetProductStatisticsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CompanyId == companyId)
                .ToListAsync(cancellationToken);

            // Get stock items for stock-related statistics
            // Use actual columns since QuantityAvailable is a computed property
            var stockItems = await _context.StockItems
                .Where(s => s.CompanyId == companyId)
                .GroupBy(s => s.ProductId)
                .Select(g => new { ProductId = g.Key, TotalAvailable = g.Sum(s => s.QuantityOnHand - s.QuantityReserved) })
                .ToListAsync(cancellationToken);

            var productStockDict = stockItems.ToDictionary(s => s.ProductId, s => s.TotalAvailable);

            var stats = new ProductStatisticsDto
            {
                TotalProducts = products.Count,
                ActiveProducts = products.Count(p => p.Status == ProductStatus.Active),
                DraftProducts = products.Count(p => p.Status == ProductStatus.Draft),
                DiscontinuedProducts = products.Count(p => p.Status == ProductStatus.Discontinued || p.Status == ProductStatus.EndOfLife),
                TotalCategories = await _context.ProductCategories
                    .CountAsync(c => c.CompanyId == companyId && c.IsActive, cancellationToken),
                ProductsInStock = products.Count(p => !p.TrackInventory || productStockDict.GetValueOrDefault(p.Id, 0) > 0),
                ProductsOutOfStock = products.Count(p => p.TrackInventory && productStockDict.GetValueOrDefault(p.Id, 0) <= 0 && !p.AllowBackorder),
                // ProductsBelowReorderLevel now tracked via StockItems.MinimumQuantity in Stock Management
                ProductsBelowReorderLevel = await _context.StockItems
                    .CountAsync(s => s.CompanyId == companyId && s.MinimumQuantity.HasValue && s.QuantityOnHand < s.MinimumQuantity, cancellationToken),
                FeaturedProducts = products.Count(p => p.IsFeatured),
                ProductsByType = products.GroupBy(p => p.Type.ToString())
                    .ToDictionary(g => g.Key, g => g.Count()),
                ProductsByCategory = products
                    .Where(p => p.Category != null)
                    .GroupBy(p => p.Category!.Name)
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            return Result<ProductStatisticsDto>.Success(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product statistics for company {CompanyId}", companyId);
            return Result<ProductStatisticsDto>.Failure("Failed to retrieve statistics");
        }
    }

    #endregion

    #region Private Helper Methods

    private ProductCategoryTreeDto MapToCategoryTree(ProductCategory category, bool includeInactive)
    {
        var children = category.SubCategories?
            .Where(c => !c.IsDeleted && (includeInactive || c.IsActive))
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Name)
            .Select(c => MapToCategoryTree(c, includeInactive))
            .ToList() ?? new List<ProductCategoryTreeDto>();

        return new ProductCategoryTreeDto
        {
            Id = category.Id,
            Code = category.Code,
            Name = category.Name,
            Icon = category.Icon,
            Color = category.Color,
            Level = category.Level,
            IsActive = category.IsActive,
            HasChildren = children.Any(),
            ProductCount = category.Products?.Count(p => !p.IsDeleted) ?? 0,
            Children = children
        };
    }

    private ProductCategoryDto MapToCategoryDto(ProductCategory category)
    {
        return new ProductCategoryDto
        {
            Id = category.Id,
            CompanyId = category.CompanyId,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            Code = category.Code,
            Name = category.Name,
            Description = category.Description,
            Level = category.Level,
            Path = category.Path,
            FullPath = category.FullPath,
            DisplayOrder = category.DisplayOrder,
            IsRoot = !category.ParentCategoryId.HasValue,
            Icon = category.Icon,
            Color = category.Color,
            ImageUrl = category.ImageUrl,
            ThumbnailUrl = category.ThumbnailUrl,
            DefaultWarrantyMonths = category.DefaultWarrantyMonths,
            DefaultResponseTimeHours = category.DefaultResponseTimeHours,
            DefaultResolutionTimeHours = category.DefaultResolutionTimeHours,
            DefaultTaxRate = category.DefaultTaxRate,
            DefaultHSNCode = category.DefaultHSNCode,
            DefaultSACCode = category.DefaultSACCode,
            RequireSerialNumber = category.RequireSerialNumber,
            TrackInventory = category.TrackInventory,
            IsPublic = category.IsPublic,
            AllowProducts = category.AllowProducts,
            AllowSubCategories = category.AllowSubCategories,
            IsActive = category.IsActive,
            Slug = category.Slug,
            MetaTitle = category.MetaTitle,
            MetaDescription = category.MetaDescription,
            ProductCount = category.Products?.Count(p => !p.IsDeleted) ?? 0,
            SubCategoryCount = category.SubCategories?.Count(s => !s.IsDeleted) ?? 0,
            SubCategories = category.SubCategories?
                .Where(s => !s.IsDeleted)
                .Select(s => MapToCategoryDto(s))
                .ToList() ?? new List<ProductCategoryDto>(),
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    private ProductDto MapToProductDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            CompanyId = product.CompanyId,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name,
            CategoryPath = product.Category?.FullPath,
            ParentProductId = product.ParentProductId,
            ParentProductName = product.ParentProduct?.Name,
            ProductCode = product.ProductCode,
            Name = product.Name,
            ShortDescription = product.ShortDescription,
            Description = product.Description,
            Type = product.Type,
            SKU = product.SKU,
            PartNumber = product.PartNumber,
            UPC = product.UPC,
            EAN = product.EAN,
            HSNCode = product.HSNCode,
            SACCode = product.SACCode,
            UnitPrice = product.UnitPrice,
            CostPrice = product.CostPrice,
            MSRP = product.MSRP,
            Currency = product.Currency,
            TaxRate = product.TaxRate,
            IsTaxable = product.IsTaxable,
            TaxType = product.TaxType,
            UnitOfMeasure = product.UnitOfMeasure,
            CustomUnitName = product.CustomUnitName,
            MinOrderQuantity = product.MinOrderQuantity,
            MaxOrderQuantity = product.MaxOrderQuantity,
            TrackInventory = product.TrackInventory,
            AllowBackorder = product.AllowBackorder,
            DefaultLocationId = product.DefaultLocationId,
            DefaultLocationName = product.DefaultLocation?.Name,
            DefaultStockCategoryId = product.DefaultStockCategoryId,
            DefaultStockCategoryName = product.DefaultStockCategory?.Name,
            TotalQuantityOnHand = product.TotalQuantityOnHand,
            TotalQuantityReserved = product.TotalQuantityReserved,
            TotalQuantityAvailable = product.TotalQuantityAvailable,
            InStock = product.InStock,
            StockItemCount = product.StockItems?.Count ?? 0,
            StockItems = product.StockItems?.Select(s => new StockItemSummaryDto
            {
                Id = s.Id,
                LocationId = s.LocationId,
                LocationName = s.Location?.Name,
                StockCategoryId = s.StockCategoryId,
                StockCategoryName = s.StockCategory?.Name,
                QuantityOnHand = s.QuantityOnHand,
                QuantityReserved = s.QuantityReserved,
                QuantityAvailable = s.QuantityAvailable,
                UnitCost = s.UnitCost,
                BatchNumber = s.BatchNumber,
                ExpiryDate = s.ExpiryDate
            }).ToList() ?? new List<StockItemSummaryDto>(),
            Brand = product.Brand,
            Manufacturer = product.Manufacturer,
            Model = product.Model,
            Version = product.Version,
            Specifications = product.Specifications,
            Features = product.Features,
            Weight = product.Weight,
            CountryOfOrigin = product.CountryOfOrigin,
            ImageUrl = product.ImageUrl,
            ThumbnailUrl = product.ThumbnailUrl,
            Images = product.Images,
            Documents = product.Documents,
            DefaultWarrantyMonths = product.DefaultWarrantyMonths,
            ExtendedWarrantyMonths = product.ExtendedWarrantyMonths,
            WarrantyTerms = product.WarrantyTerms,
            IsServiceable = product.IsServiceable,
            RequiresInstallation = product.RequiresInstallation,
            IsSubscription = product.IsSubscription,
            BillingFrequency = product.BillingFrequency,
            TrialPeriodDays = product.TrialPeriodDays,
            SetupFee = product.SetupFee,
            Status = product.Status,
            LaunchDate = product.LaunchDate,
            EndOfSaleDate = product.EndOfSaleDate,
            EndOfLifeDate = product.EndOfLifeDate,
            EndOfSupportDate = product.EndOfSupportDate,
            IsFeatured = product.IsFeatured,
            IsPublic = product.IsPublic,
            IsAvailable = product.Status == ProductStatus.Active &&
                         (!product.LaunchDate.HasValue || product.LaunchDate <= DateTime.UtcNow) &&
                         (!product.EndOfSaleDate.HasValue || product.EndOfSaleDate > DateTime.UtcNow),
            IsDiscontinued = product.Status == ProductStatus.Discontinued || product.Status == ProductStatus.EndOfLife,
            RequireSerialNumber = product.RequireSerialNumber,
            Slug = product.Slug,
            Tags = product.Tags,
            ExternalProductId = product.ExternalProductId,
            VendorName = product.VendorName,
            IsVariant = product.ParentProductId.HasValue,
            HasVariants = product.Variants?.Any(v => !v.IsDeleted) ?? false,
            VariantCount = product.Variants?.Count(v => !v.IsDeleted) ?? 0,
            Variants = product.Variants?
                .Where(v => !v.IsDeleted)
                .Select(v => new ProductVariantDto
                {
                    Id = v.Id,
                    ProductCode = v.ProductCode,
                    Name = v.Name,
                    SKU = v.SKU,
                    UnitPrice = v.UnitPrice,
                    InStock = v.InStock,
                    Status = v.Status
                })
                .ToList() ?? new List<ProductVariantDto>(),
            ProfitMargin = product.UnitPrice.HasValue && product.CostPrice.HasValue && product.CostPrice > 0
                ? ((product.UnitPrice.Value - product.CostPrice.Value) / product.CostPrice.Value) * 100
                : null,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    private ProductPriceListDto MapToPriceListDto(ProductPriceList priceList)
    {
        var now = DateTime.UtcNow;
        return new ProductPriceListDto
        {
            Id = priceList.Id,
            ProductId = priceList.ProductId,
            ProductCode = priceList.Product?.ProductCode ?? string.Empty,
            ProductName = priceList.Product?.Name ?? string.Empty,
            Name = priceList.Name,
            Description = priceList.Description,
            Type = priceList.Type,
            PartnerId = priceList.PartnerId,
            PartnerName = priceList.Partner?.Name,
            CustomerId = priceList.CustomerId,
            CustomerName = priceList.Customer?.Name,
            PartnerTier = priceList.PartnerTier,
            CustomerSegment = priceList.CustomerSegment,
            UnitPrice = priceList.UnitPrice,
            Currency = priceList.Currency,
            DiscountPercent = priceList.DiscountPercent,
            DiscountAmount = priceList.DiscountAmount,
            MarkupPercent = priceList.MarkupPercent,
            EffectiveDiscount = priceList.DiscountPercent ??
                (priceList.DiscountAmount.HasValue && priceList.UnitPrice > 0
                    ? (priceList.DiscountAmount.Value / priceList.UnitPrice) * 100
                    : null),
            MinQuantity = priceList.MinQuantity,
            MaxQuantity = priceList.MaxQuantity,
            ValidFrom = priceList.ValidFrom,
            ValidTo = priceList.ValidTo,
            IsActive = priceList.IsActive,
            IsValid = priceList.IsActive &&
                     (!priceList.ValidFrom.HasValue || priceList.ValidFrom <= now) &&
                     (!priceList.ValidTo.HasValue || priceList.ValidTo >= now),
            Priority = priceList.Priority,
            MinOrderValue = priceList.MinOrderValue,
            PromoCode = priceList.PromoCode,
            ContractId = priceList.ContractId,
            ApprovalStatus = priceList.ApprovalStatus,
            ApprovedBy = priceList.ApprovedBy,
            ApprovedAt = priceList.ApprovedAt,
            Notes = priceList.Notes,
            CreatedAt = priceList.CreatedAt,
            UpdatedAt = priceList.UpdatedAt
        };
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLower()
            .Replace(" ", "-")
            .Replace("&", "and")
            .Replace("'", "")
            .Replace("\"", "")
            .Replace("/", "-")
            .Replace("\\", "-")
            .Replace(",", "")
            .Replace(".", "")
            .Replace("(", "")
            .Replace(")", "");
    }

    #endregion
}
