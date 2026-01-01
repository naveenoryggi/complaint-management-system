using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Product;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Product;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing brands
/// </summary>
public class BrandService : IBrandService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<BrandService> _logger;

    public BrandService(ComplaintDbContext context, ILogger<BrandService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BrandSummaryDto>>> GetBrandsAsync(
        Guid companyId,
        string? search = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Set<Brand>()
                .Where(b => b.CompanyId == companyId);

            // Apply filters
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(searchLower) ||
                    b.Code.ToLower().Contains(searchLower) ||
                    (b.Country != null && b.Country.ToLower().Contains(searchLower)));
            }

            if (isActive.HasValue)
                query = query.Where(b => b.IsActive == isActive.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BrandSummaryDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Name = b.Name,
                    LogoUrl = b.LogoUrl,
                    Country = b.Country,
                    IsActive = b.IsActive,
                    ProductCount = b.Products.Count(p => !p.IsDeleted)
                })
                .ToListAsync(cancellationToken);

            var result = new PagedResult<BrandSummaryDto>(items, pageNumber, pageSize, totalCount);

            return Result<PagedResult<BrandSummaryDto>>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brands for company {CompanyId}", companyId);
            return Result<PagedResult<BrandSummaryDto>>.Failure("Failed to retrieve brands");
        }
    }

    public async Task<Result<BrandDto>> GetBrandByIdAsync(Guid companyId, Guid brandId, CancellationToken cancellationToken = default)
    {
        try
        {
            var brand = await _context.Set<Brand>()
                .Include(b => b.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(b => b.Id == brandId && b.CompanyId == companyId, cancellationToken);

            if (brand == null)
                return Result<BrandDto>.Failure("Brand not found");

            return Result<BrandDto>.Success(MapToBrandDto(brand));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand {BrandId}", brandId);
            return Result<BrandDto>.Failure("Failed to retrieve brand");
        }
    }

    public async Task<Result<BrandDto>> GetBrandByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var brand = await _context.Set<Brand>()
                .Include(b => b.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(b => b.Code == code && b.CompanyId == companyId, cancellationToken);

            if (brand == null)
                return Result<BrandDto>.Failure("Brand not found");

            return Result<BrandDto>.Success(MapToBrandDto(brand));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand by code {Code}", code);
            return Result<BrandDto>.Failure("Failed to retrieve brand");
        }
    }

    public async Task<Result<BrandDto>> CreateBrandAsync(Guid companyId, CreateBrandRequest request, Guid createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check for duplicate code
            if (await BrandCodeExistsAsync(companyId, request.Code, null, cancellationToken))
            {
                return Result<BrandDto>.Failure($"A brand with code '{request.Code}' already exists");
            }

            var brand = new Brand
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = request.Code,
                Name = request.Name,
                Description = request.Description,
                LogoUrl = request.LogoUrl,
                WebsiteUrl = request.WebsiteUrl,
                Country = request.Country,
                IsActive = true,
                DisplayOrder = request.DisplayOrder,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _context.Set<Brand>().Add(brand);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created brand {BrandId} with code {Code} for company {CompanyId}",
                brand.Id, brand.Code, companyId);

            return Result<BrandDto>.Success(MapToBrandDto(brand));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating brand for company {CompanyId}", companyId);
            return Result<BrandDto>.Failure("Failed to create brand");
        }
    }

    public async Task<Result<BrandDto>> UpdateBrandAsync(Guid companyId, Guid brandId, UpdateBrandRequest request, Guid updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var brand = await _context.Set<Brand>()
                .Include(b => b.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(b => b.Id == brandId && b.CompanyId == companyId, cancellationToken);

            if (brand == null)
                return Result<BrandDto>.Failure("Brand not found");

            brand.Name = request.Name;
            brand.Description = request.Description;
            brand.LogoUrl = request.LogoUrl;
            brand.WebsiteUrl = request.WebsiteUrl;
            brand.Country = request.Country;
            brand.IsActive = request.IsActive;
            brand.DisplayOrder = request.DisplayOrder;
            brand.Notes = request.Notes;
            brand.UpdatedAt = DateTime.UtcNow;
            brand.UpdatedBy = updatedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated brand {BrandId} for company {CompanyId}", brandId, companyId);

            return Result<BrandDto>.Success(MapToBrandDto(brand));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating brand {BrandId}", brandId);
            return Result<BrandDto>.Failure("Failed to update brand");
        }
    }

    public async Task<Result<bool>> DeleteBrandAsync(Guid companyId, Guid brandId, Guid deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var brand = await _context.Set<Brand>()
                .Include(b => b.Products.Where(p => !p.IsDeleted))
                .FirstOrDefaultAsync(b => b.Id == brandId && b.CompanyId == companyId, cancellationToken);

            if (brand == null)
                return Result<bool>.Failure("Brand not found");

            // Check if brand has products
            if (brand.Products.Any())
            {
                return Result<bool>.Failure($"Cannot delete brand. It has {brand.Products.Count} product(s) associated with it. Please reassign or delete the products first.");
            }

            // Soft delete
            brand.IsDeleted = true;
            brand.DeletedAt = DateTime.UtcNow;
            brand.DeletedBy = deletedBy;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted brand {BrandId} for company {CompanyId}", brandId, companyId);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting brand {BrandId}", brandId);
            return Result<bool>.Failure("Failed to delete brand");
        }
    }

    public async Task<Result<List<BrandLookupDto>>> GetBrandLookupAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        try
        {
            var brands = await _context.Set<Brand>()
                .Where(b => b.CompanyId == companyId && b.IsActive)
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.Name)
                .Select(b => new BrandLookupDto
                {
                    Id = b.Id,
                    Code = b.Code,
                    Name = b.Name
                })
                .ToListAsync(cancellationToken);

            return Result<List<BrandLookupDto>>.Success(brands);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting brand lookup for company {CompanyId}", companyId);
            return Result<List<BrandLookupDto>>.Failure("Failed to retrieve brand lookup");
        }
    }

    public async Task<bool> BrandCodeExistsAsync(Guid companyId, string code, Guid? excludeBrandId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Brand>()
            .Where(b => b.CompanyId == companyId && b.Code == code);

        if (excludeBrandId.HasValue)
            query = query.Where(b => b.Id != excludeBrandId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    private static BrandDto MapToBrandDto(Brand brand)
    {
        return new BrandDto
        {
            Id = brand.Id,
            CompanyId = brand.CompanyId,
            Code = brand.Code,
            Name = brand.Name,
            Description = brand.Description,
            LogoUrl = brand.LogoUrl,
            WebsiteUrl = brand.WebsiteUrl,
            Country = brand.Country,
            IsActive = brand.IsActive,
            DisplayOrder = brand.DisplayOrder,
            Notes = brand.Notes,
            ProductCount = brand.Products?.Count(p => !p.IsDeleted) ?? 0,
            CreatedAt = brand.CreatedAt,
            UpdatedAt = brand.UpdatedAt
        };
    }
}
