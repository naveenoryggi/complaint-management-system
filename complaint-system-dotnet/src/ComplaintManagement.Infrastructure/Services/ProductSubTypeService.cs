using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Product;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Product;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing product sub-types
/// </summary>
public class ProductSubTypeService : IProductSubTypeService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<ProductSubTypeService> _logger;

    public ProductSubTypeService(ComplaintDbContext context, ILogger<ProductSubTypeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<ProductSubTypeSummaryDto>>> GetSubTypesAsync(
        Guid companyId,
        string? search = null,
        Guid? categoryId = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Set<ProductSubType>()
                .Include(st => st.Category)
                .Where(st => st.CompanyId == companyId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(st =>
                    st.Name.ToLower().Contains(searchLower) ||
                    st.Code.ToLower().Contains(searchLower) ||
                    (st.Description != null && st.Description.ToLower().Contains(searchLower)));
            }

            if (categoryId.HasValue)
                query = query.Where(st => st.CategoryId == categoryId.Value);

            if (isActive.HasValue)
                query = query.Where(st => st.IsActive == isActive.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(st => st.Category!.Name)
                .ThenBy(st => st.DisplayOrder)
                .ThenBy(st => st.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(st => new ProductSubTypeSummaryDto
                {
                    Id = st.Id,
                    CategoryId = st.CategoryId,
                    CategoryName = st.Category != null ? st.Category.Name : "",
                    Code = st.Code,
                    Name = st.Name,
                    Icon = st.Icon,
                    Color = st.Color,
                    IsActive = st.IsActive,
                    ProductCount = st.Products.Count(p => !p.IsDeleted)
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<ProductSubTypeSummaryDto>(items, pageNumber, pageSize, totalCount);

            return Result<PagedResult<ProductSubTypeSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-types for company {CompanyId}", companyId);
            return Result<PagedResult<ProductSubTypeSummaryDto>>.Failure("Failed to retrieve sub-types");
        }
    }

    public async Task<Result<ProductSubTypeDto>> GetSubTypeByIdAsync(Guid companyId, Guid subTypeId, CancellationToken cancellationToken = default)
    {
        try
        {
            var subType = await _context.Set<ProductSubType>()
                .Include(st => st.Category)
                .Include(st => st.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(st => st.Id == subTypeId && st.CompanyId == companyId, cancellationToken);

            if (subType == null)
                return Result<ProductSubTypeDto>.Failure("Sub-type not found");

            return Result<ProductSubTypeDto>.Success(MapToSubTypeDto(subType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-type {SubTypeId}", subTypeId);
            return Result<ProductSubTypeDto>.Failure("Failed to retrieve sub-type");
        }
    }

    public async Task<Result<ProductSubTypeDto>> GetSubTypeByCodeAsync(Guid companyId, Guid categoryId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var subType = await _context.Set<ProductSubType>()
                .Include(st => st.Category)
                .Include(st => st.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(st => st.Code == code && st.CategoryId == categoryId && st.CompanyId == companyId, cancellationToken);

            if (subType == null)
                return Result<ProductSubTypeDto>.Failure("Sub-type not found");

            return Result<ProductSubTypeDto>.Success(MapToSubTypeDto(subType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-type by code {Code} in category {CategoryId}", code, categoryId);
            return Result<ProductSubTypeDto>.Failure("Failed to retrieve sub-type");
        }
    }

    public async Task<Result<ProductSubTypeDto>> CreateSubTypeAsync(Guid companyId, CreateProductSubTypeRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate category exists
            var categoryExists = await _context.Set<ProductCategory>()
                .AnyAsync(c => c.Id == request.CategoryId && c.CompanyId == companyId, cancellationToken);

            if (!categoryExists)
                return Result<ProductSubTypeDto>.Failure("Category not found");

            // Check for duplicate code within category
            if (await SubTypeCodeExistsAsync(companyId, request.CategoryId, request.Code, null, cancellationToken))
            {
                return Result<ProductSubTypeDto>.Failure($"A sub-type with code '{request.Code}' already exists in this category");
            }

            var subType = new ProductSubType
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                CategoryId = request.CategoryId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                Icon = request.Icon,
                Color = request.Color,
                DisplayOrder = request.DisplayOrder,
                IsActive = true,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Set<ProductSubType>().Add(subType);
            await _context.SaveChangesAsync(cancellationToken);

            // Reload with category
            subType = await _context.Set<ProductSubType>()
                .Include(st => st.Category)
                .FirstAsync(st => st.Id == subType.Id, cancellationToken);

            _logger.LogInformation("Created sub-type {SubTypeId} with code {Code} for company {CompanyId}",
                subType.Id, subType.Code, companyId);

            return Result<ProductSubTypeDto>.Success(MapToSubTypeDto(subType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sub-type for company {CompanyId}", companyId);
            return Result<ProductSubTypeDto>.Failure("Failed to create sub-type");
        }
    }

    public async Task<Result<ProductSubTypeDto>> UpdateSubTypeAsync(Guid companyId, Guid subTypeId, UpdateProductSubTypeRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var subType = await _context.Set<ProductSubType>()
                .Include(st => st.Category)
                .Include(st => st.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(st => st.Id == subTypeId && st.CompanyId == companyId, cancellationToken);

            if (subType == null)
                return Result<ProductSubTypeDto>.Failure("Sub-type not found");

            // Check if category is changing
            if (request.CategoryId != subType.CategoryId)
            {
                // Validate new category exists
                var categoryExists = await _context.Set<ProductCategory>()
                    .AnyAsync(c => c.Id == request.CategoryId && c.CompanyId == companyId, cancellationToken);

                if (!categoryExists)
                    return Result<ProductSubTypeDto>.Failure("Category not found");

                subType.CategoryId = request.CategoryId;
            }

            subType.Name = request.Name;
            subType.Description = request.Description;
            subType.Icon = request.Icon;
            subType.Color = request.Color;
            subType.IsActive = request.IsActive;
            subType.DisplayOrder = request.DisplayOrder;
            subType.Notes = request.Notes;
            subType.UpdatedAt = DateTime.UtcNow;
            subType.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync(cancellationToken);

            // Reload with category
            subType = await _context.Set<ProductSubType>()
                .Include(st => st.Category)
                .Include(st => st.Products.Where(p => !p.IsDeleted))
                .FirstAsync(st => st.Id == subType.Id, cancellationToken);

            _logger.LogInformation("Updated sub-type {SubTypeId} for company {CompanyId}", subTypeId, companyId);

            return Result<ProductSubTypeDto>.Success(MapToSubTypeDto(subType));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sub-type {SubTypeId}", subTypeId);
            return Result<ProductSubTypeDto>.Failure("Failed to update sub-type");
        }
    }

    public async Task<Result<bool>> DeleteSubTypeAsync(Guid companyId, Guid subTypeId, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var subType = await _context.Set<ProductSubType>()
                .Include(st => st.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(st => st.Id == subTypeId && st.CompanyId == companyId, cancellationToken);

            if (subType == null)
                return Result<bool>.Failure("Sub-type not found");

            // Check if sub-type has products
            if (subType.Products.Any())
            {
                return Result<bool>.Failure($"Cannot delete sub-type. It has {subType.Products.Count} product(s) associated with it. Please reassign or delete the products first.");
            }

            // Soft delete
            subType.IsDeleted = true;
            subType.DeletedAt = DateTime.UtcNow;
            subType.DeletedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted sub-type {SubTypeId} for company {CompanyId}", subTypeId, companyId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sub-type {SubTypeId}", subTypeId);
            return Result<bool>.Failure("Failed to delete sub-type");
        }
    }

    public async Task<Result<List<ProductSubTypeLookupDto>>> GetSubTypeLookupAsync(Guid companyId, Guid? categoryId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Set<ProductSubType>()
                .Where(st => st.CompanyId == companyId && st.IsActive);

            if (categoryId.HasValue)
                query = query.Where(st => st.CategoryId == categoryId.Value);

            var subTypes = await query
                .OrderBy(st => st.DisplayOrder)
                .ThenBy(st => st.Name)
                .Select(st => new ProductSubTypeLookupDto
                {
                    Id = st.Id,
                    CategoryId = st.CategoryId,
                    Code = st.Code,
                    Name = st.Name
                })
                .ToListAsync(cancellationToken);

            return Result<List<ProductSubTypeLookupDto>>.Success(subTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sub-type lookup for company {CompanyId}", companyId);
            return Result<List<ProductSubTypeLookupDto>>.Failure("Failed to retrieve sub-type lookup");
        }
    }

    public async Task<bool> SubTypeCodeExistsAsync(Guid companyId, Guid categoryId, string code, Guid? excludeSubTypeId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<ProductSubType>()
            .Where(st => st.CompanyId == companyId && st.CategoryId == categoryId && st.Code == code);

        if (excludeSubTypeId.HasValue)
            query = query.Where(st => st.Id != excludeSubTypeId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    private static ProductSubTypeDto MapToSubTypeDto(ProductSubType subType)
    {
        return new ProductSubTypeDto
        {
            Id = subType.Id,
            CompanyId = subType.CompanyId,
            CategoryId = subType.CategoryId,
            CategoryName = subType.Category?.Name ?? "",
            Code = subType.Code,
            Name = subType.Name,
            Description = subType.Description,
            Icon = subType.Icon,
            Color = subType.Color,
            DisplayOrder = subType.DisplayOrder,
            IsActive = subType.IsActive,
            Notes = subType.Notes,
            ProductCount = subType.Products?.Count(p => !p.IsDeleted) ?? 0,
            CreatedAt = subType.CreatedAt,
            UpdatedAt = subType.UpdatedAt
        };
    }
}
