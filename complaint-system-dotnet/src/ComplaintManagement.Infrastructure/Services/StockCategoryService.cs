using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Services;

public class StockCategoryService : IStockCategoryService
{
    private readonly ComplaintDbContext _context;

    public StockCategoryService(ComplaintDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<StockCategoryDto>>> GetAllAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null)
    {
        var query = _context.StockCategories
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(searchTerm) ||
                s.Name.ToLower().Contains(searchTerm) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var categories = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StockCategoryDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                ColorCode = s.ColorCode,
                Icon = s.Icon,
                IsForSale = s.IsForSale,
                IsFixedAsset = s.IsFixedAsset,
                IsServiceStock = s.IsServiceStock,
                IsFaulty = s.IsFaulty,
                IsDemo = s.IsDemo,
                RequiresSpecialHandling = s.RequiresSpecialHandling,
                AllowsCustomerAssignment = s.AllowsCustomerAssignment,
                AllowsEmployeeAssignment = s.AllowsEmployeeAssignment,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
                IsSystem = s.IsSystem,
                AssetCount = s.Assets.Count(a => !a.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return Result<PagedResult<StockCategoryDto>>.Success(
            new PagedResult<StockCategoryDto>(categories, totalCount, page, pageSize));
    }

    public async Task<Result<List<StockCategoryLookupDto>>> GetLookupAsync(Guid companyId, bool activeOnly = true)
    {
        var query = _context.StockCategories
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var categories = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new StockCategoryLookupDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                ColorCode = s.ColorCode,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Result<List<StockCategoryLookupDto>>.Success(categories);
    }

    public async Task<Result<StockCategoryDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var category = await _context.StockCategories
            .Where(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted)
            .Select(s => new StockCategoryDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                ColorCode = s.ColorCode,
                Icon = s.Icon,
                IsForSale = s.IsForSale,
                IsFixedAsset = s.IsFixedAsset,
                IsServiceStock = s.IsServiceStock,
                IsFaulty = s.IsFaulty,
                IsDemo = s.IsDemo,
                RequiresSpecialHandling = s.RequiresSpecialHandling,
                AllowsCustomerAssignment = s.AllowsCustomerAssignment,
                AllowsEmployeeAssignment = s.AllowsEmployeeAssignment,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
                IsSystem = s.IsSystem,
                AssetCount = s.Assets.Count(a => !a.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return Result<StockCategoryDto>.Failure("Stock category not found");
        }

        return Result<StockCategoryDto>.Success(category);
    }

    public async Task<Result<StockCategoryDto>> GetByCodeAsync(string code, Guid companyId)
    {
        var category = await _context.StockCategories
            .Where(s => s.Code == code && s.CompanyId == companyId && !s.IsDeleted)
            .Select(s => new StockCategoryDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                ColorCode = s.ColorCode,
                Icon = s.Icon,
                IsForSale = s.IsForSale,
                IsFixedAsset = s.IsFixedAsset,
                IsServiceStock = s.IsServiceStock,
                IsFaulty = s.IsFaulty,
                IsDemo = s.IsDemo,
                RequiresSpecialHandling = s.RequiresSpecialHandling,
                AllowsCustomerAssignment = s.AllowsCustomerAssignment,
                AllowsEmployeeAssignment = s.AllowsEmployeeAssignment,
                SortOrder = s.SortOrder,
                IsActive = s.IsActive,
                IsSystem = s.IsSystem,
                AssetCount = s.Assets.Count(a => !a.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            return Result<StockCategoryDto>.Failure("Stock category not found");
        }

        return Result<StockCategoryDto>.Success(category);
    }

    public async Task<Result<StockCategoryDto>> CreateAsync(CreateStockCategoryRequest request, Guid companyId, Guid userId)
    {
        // Check for duplicate code
        var exists = await _context.StockCategories
            .AnyAsync(s => s.CompanyId == companyId && s.Code == request.Code && !s.IsDeleted);

        if (exists)
        {
            return Result<StockCategoryDto>.Failure($"Stock category with code '{request.Code}' already exists");
        }

        var category = new StockCategory
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            ColorCode = request.ColorCode,
            Icon = request.Icon,
            IsForSale = request.IsForSale,
            IsFixedAsset = request.IsFixedAsset,
            IsServiceStock = request.IsServiceStock,
            IsFaulty = request.IsFaulty,
            IsDemo = request.IsDemo,
            RequiresSpecialHandling = request.RequiresSpecialHandling,
            AllowsCustomerAssignment = request.AllowsCustomerAssignment,
            AllowsEmployeeAssignment = request.AllowsEmployeeAssignment,
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            IsSystem = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.StockCategories.Add(category);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(category.Id, companyId);
    }

    public async Task<Result<StockCategoryDto>> UpdateAsync(Guid id, UpdateStockCategoryRequest request, Guid companyId, Guid userId)
    {
        var category = await _context.StockCategories
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (category == null)
        {
            return Result<StockCategoryDto>.Failure("Stock category not found");
        }

        // Check for duplicate code (excluding current)
        var codeExists = await _context.StockCategories
            .AnyAsync(s => s.CompanyId == companyId && s.Code == request.Code && s.Id != id && !s.IsDeleted);

        if (codeExists)
        {
            return Result<StockCategoryDto>.Failure($"Stock category with code '{request.Code}' already exists");
        }

        category.Code = request.Code.ToUpper();
        category.Name = request.Name;
        category.Description = request.Description;
        category.ColorCode = request.ColorCode;
        category.Icon = request.Icon;
        category.IsForSale = request.IsForSale;
        category.IsFixedAsset = request.IsFixedAsset;
        category.IsServiceStock = request.IsServiceStock;
        category.IsFaulty = request.IsFaulty;
        category.IsDemo = request.IsDemo;
        category.RequiresSpecialHandling = request.RequiresSpecialHandling;
        category.AllowsCustomerAssignment = request.AllowsCustomerAssignment;
        category.AllowsEmployeeAssignment = request.AllowsEmployeeAssignment;
        category.SortOrder = request.SortOrder;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;
        category.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(category.Id, companyId);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, Guid companyId, Guid userId)
    {
        var category = await _context.StockCategories
            .Include(s => s.Assets)
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (category == null)
        {
            return Result<bool>.Failure("Stock category not found");
        }

        if (category.IsSystem)
        {
            return Result<bool>.Failure("Cannot delete system stock category");
        }

        // Check if any assets are using this category
        if (category.Assets.Any(a => !a.IsDeleted))
        {
            return Result<bool>.Failure("Cannot delete stock category that has assets assigned to it");
        }

        category.IsDeleted = true;
        category.DeletedAt = DateTime.UtcNow;
        category.DeletedBy = userId;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task SeedDefaultCategoriesAsync(Guid companyId, Guid userId)
    {
        var existingCategories = await _context.StockCategories
            .AnyAsync(s => s.CompanyId == companyId && !s.IsDeleted);

        if (existingCategories)
        {
            return; // Already has categories
        }

        var defaultCategories = new List<StockCategory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = "SALES",
                Name = "Sales Stock",
                Description = "Stock available for sale to customers",
                ColorCode = "#22c55e",
                IsForSale = true,
                AllowsCustomerAssignment = true,
                AllowsEmployeeAssignment = false,
                SortOrder = 1,
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = "FIXED",
                Name = "Fixed Asset",
                Description = "Company-owned fixed assets for internal use",
                ColorCode = "#3b82f6",
                IsFixedAsset = true,
                AllowsCustomerAssignment = false,
                AllowsEmployeeAssignment = true,
                SortOrder = 2,
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = "AFTERSALES",
                Name = "Aftersales Stock",
                Description = "Replacement/warranty stock for service and support",
                ColorCode = "#f59e0b",
                IsServiceStock = true,
                AllowsCustomerAssignment = true,
                AllowsEmployeeAssignment = true,
                SortOrder = 3,
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = "FAULTY",
                Name = "Faulty Stock",
                Description = "Defective items awaiting repair or disposal",
                ColorCode = "#ef4444",
                IsFaulty = true,
                RequiresSpecialHandling = true,
                AllowsCustomerAssignment = false,
                AllowsEmployeeAssignment = false,
                SortOrder = 4,
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = "DEMO",
                Name = "Demo Stock",
                Description = "Assets used for demonstrations and evaluations",
                ColorCode = "#8b5cf6",
                IsDemo = true,
                AllowsCustomerAssignment = true,
                AllowsEmployeeAssignment = true,
                SortOrder = 5,
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            },
            new()
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = "SPARES",
                Name = "Spare Parts",
                Description = "Components kept for repairs and maintenance",
                ColorCode = "#06b6d4",
                IsServiceStock = true,
                AllowsCustomerAssignment = false,
                AllowsEmployeeAssignment = true,
                SortOrder = 6,
                IsActive = true,
                IsSystem = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            }
        };

        _context.StockCategories.AddRange(defaultCategories);
        await _context.SaveChangesAsync();
    }
}
