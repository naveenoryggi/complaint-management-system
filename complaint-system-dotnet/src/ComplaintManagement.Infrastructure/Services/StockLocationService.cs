using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Services;

public class StockLocationService : IStockLocationService
{
    private readonly ComplaintDbContext _context;

    public StockLocationService(ComplaintDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<StockLocationDto>>> GetAllAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null)
    {
        var query = _context.StockLocations
            .Include(s => s.ParentLocation)
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(s =>
                s.Code.ToLower().Contains(searchTerm) ||
                s.Name.ToLower().Contains(searchTerm) ||
                (s.Description != null && s.Description.ToLower().Contains(searchTerm)) ||
                (s.City != null && s.City.ToLower().Contains(searchTerm)));
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        var totalCount = await query.CountAsync();

        var locations = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StockLocationDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                ParentLocationId = s.ParentLocationId,
                ParentLocationName = s.ParentLocation != null ? s.ParentLocation.Name : null,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                Address = s.Address,
                City = s.City,
                State = s.State,
                Country = s.Country,
                PostalCode = s.PostalCode,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                ContactPerson = s.ContactPerson,
                ContactPhone = s.ContactPhone,
                ContactEmail = s.ContactEmail,
                MaxCapacity = s.MaxCapacity,
                CapacityUnit = s.CapacityUnit,
                IsActive = s.IsActive,
                IsDefault = s.IsDefault,
                SortOrder = s.SortOrder,
                Notes = s.Notes,
                StockItemCount = s.StockItems.Count(si => !si.IsDeleted),
                ChildLocationCount = s.ChildLocations.Count(cl => !cl.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return Result<PagedResult<StockLocationDto>>.Success(
            new PagedResult<StockLocationDto>(locations, totalCount, page, pageSize));
    }

    public async Task<Result<List<StockLocationLookupDto>>> GetLookupAsync(Guid companyId, bool activeOnly = true)
    {
        var query = _context.StockLocations
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (activeOnly)
        {
            query = query.Where(s => s.IsActive);
        }

        var locations = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new StockLocationLookupDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Type = s.Type,
                ParentLocationId = s.ParentLocationId,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Result<List<StockLocationLookupDto>>.Success(locations);
    }

    public async Task<Result<List<StockLocationLookupDto>>> GetHierarchyAsync(Guid companyId, Guid? parentId = null)
    {
        var query = _context.StockLocations
            .Where(s => s.CompanyId == companyId && !s.IsDeleted && s.IsActive);

        if (parentId.HasValue)
        {
            query = query.Where(s => s.ParentLocationId == parentId.Value);
        }
        else
        {
            query = query.Where(s => s.ParentLocationId == null);
        }

        var locations = await query
            .OrderBy(s => s.SortOrder)
            .ThenBy(s => s.Name)
            .Select(s => new StockLocationLookupDto
            {
                Id = s.Id,
                Code = s.Code,
                Name = s.Name,
                Type = s.Type,
                ParentLocationId = s.ParentLocationId,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Result<List<StockLocationLookupDto>>.Success(locations);
    }

    public async Task<Result<StockLocationDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var location = await _context.StockLocations
            .Include(s => s.ParentLocation)
            .Where(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted)
            .Select(s => new StockLocationDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                ParentLocationId = s.ParentLocationId,
                ParentLocationName = s.ParentLocation != null ? s.ParentLocation.Name : null,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                Address = s.Address,
                City = s.City,
                State = s.State,
                Country = s.Country,
                PostalCode = s.PostalCode,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                ContactPerson = s.ContactPerson,
                ContactPhone = s.ContactPhone,
                ContactEmail = s.ContactEmail,
                MaxCapacity = s.MaxCapacity,
                CapacityUnit = s.CapacityUnit,
                IsActive = s.IsActive,
                IsDefault = s.IsDefault,
                SortOrder = s.SortOrder,
                Notes = s.Notes,
                StockItemCount = s.StockItems.Count(si => !si.IsDeleted),
                ChildLocationCount = s.ChildLocations.Count(cl => !cl.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (location == null)
        {
            return Result<StockLocationDto>.Failure("Stock location not found");
        }

        return Result<StockLocationDto>.Success(location);
    }

    public async Task<Result<StockLocationDto>> GetByCodeAsync(string code, Guid companyId)
    {
        var location = await _context.StockLocations
            .Include(s => s.ParentLocation)
            .Where(s => s.Code == code && s.CompanyId == companyId && !s.IsDeleted)
            .Select(s => new StockLocationDto
            {
                Id = s.Id,
                CompanyId = s.CompanyId,
                ParentLocationId = s.ParentLocationId,
                ParentLocationName = s.ParentLocation != null ? s.ParentLocation.Name : null,
                Code = s.Code,
                Name = s.Name,
                Description = s.Description,
                Type = s.Type,
                Address = s.Address,
                City = s.City,
                State = s.State,
                Country = s.Country,
                PostalCode = s.PostalCode,
                Latitude = s.Latitude,
                Longitude = s.Longitude,
                ContactPerson = s.ContactPerson,
                ContactPhone = s.ContactPhone,
                ContactEmail = s.ContactEmail,
                MaxCapacity = s.MaxCapacity,
                CapacityUnit = s.CapacityUnit,
                IsActive = s.IsActive,
                IsDefault = s.IsDefault,
                SortOrder = s.SortOrder,
                Notes = s.Notes,
                StockItemCount = s.StockItems.Count(si => !si.IsDeleted),
                ChildLocationCount = s.ChildLocations.Count(cl => !cl.IsDeleted),
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (location == null)
        {
            return Result<StockLocationDto>.Failure("Stock location not found");
        }

        return Result<StockLocationDto>.Success(location);
    }

    public async Task<Result<StockLocationDto>> CreateAsync(CreateStockLocationRequest request, Guid companyId, Guid userId)
    {
        var exists = await _context.StockLocations
            .AnyAsync(s => s.CompanyId == companyId && s.Code == request.Code && !s.IsDeleted);

        if (exists)
        {
            return Result<StockLocationDto>.Failure($"Stock location with code '{request.Code}' already exists");
        }

        var location = new StockLocation
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ParentLocationId = request.ParentLocationId,
            Code = request.Code.ToUpper(),
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PostalCode = request.PostalCode,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            ContactPerson = request.ContactPerson,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            MaxCapacity = request.MaxCapacity,
            CapacityUnit = request.CapacityUnit,
            IsActive = request.IsActive,
            IsDefault = request.IsDefault,
            SortOrder = request.SortOrder,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        if (request.IsDefault)
        {
            await ClearDefaultLocationAsync(companyId);
        }

        _context.StockLocations.Add(location);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(location.Id, companyId);
    }

    public async Task<Result<StockLocationDto>> UpdateAsync(Guid id, UpdateStockLocationRequest request, Guid companyId, Guid userId)
    {
        var location = await _context.StockLocations
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (location == null)
        {
            return Result<StockLocationDto>.Failure("Stock location not found");
        }

        var codeExists = await _context.StockLocations
            .AnyAsync(s => s.CompanyId == companyId && s.Code == request.Code && s.Id != id && !s.IsDeleted);

        if (codeExists)
        {
            return Result<StockLocationDto>.Failure($"Stock location with code '{request.Code}' already exists");
        }

        location.ParentLocationId = request.ParentLocationId;
        location.Code = request.Code.ToUpper();
        location.Name = request.Name;
        location.Description = request.Description;
        location.Type = request.Type;
        location.Address = request.Address;
        location.City = request.City;
        location.State = request.State;
        location.Country = request.Country;
        location.PostalCode = request.PostalCode;
        location.Latitude = request.Latitude;
        location.Longitude = request.Longitude;
        location.ContactPerson = request.ContactPerson;
        location.ContactPhone = request.ContactPhone;
        location.ContactEmail = request.ContactEmail;
        location.MaxCapacity = request.MaxCapacity;
        location.CapacityUnit = request.CapacityUnit;
        location.IsActive = request.IsActive;
        location.SortOrder = request.SortOrder;
        location.Notes = request.Notes;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = userId;

        if (request.IsDefault && !location.IsDefault)
        {
            await ClearDefaultLocationAsync(companyId);
            location.IsDefault = true;
        }
        else if (!request.IsDefault)
        {
            location.IsDefault = false;
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(location.Id, companyId);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, Guid companyId, Guid userId)
    {
        var location = await _context.StockLocations
            .Include(s => s.StockItems)
            .Include(s => s.ChildLocations)
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (location == null)
        {
            return Result<bool>.Failure("Stock location not found");
        }

        if (location.StockItems.Any(si => !si.IsDeleted))
        {
            return Result<bool>.Failure("Cannot delete location that has stock items");
        }

        if (location.ChildLocations.Any(cl => !cl.IsDeleted))
        {
            return Result<bool>.Failure("Cannot delete location that has child locations");
        }

        location.IsDeleted = true;
        location.DeletedAt = DateTime.UtcNow;
        location.DeletedBy = userId;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<StockLocationDto>> SetDefaultAsync(Guid id, Guid companyId, Guid userId)
    {
        var location = await _context.StockLocations
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (location == null)
        {
            return Result<StockLocationDto>.Failure("Stock location not found");
        }

        await ClearDefaultLocationAsync(companyId);

        location.IsDefault = true;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(location.Id, companyId);
    }

    private async Task ClearDefaultLocationAsync(Guid companyId)
    {
        var defaultLocations = await _context.StockLocations
            .Where(s => s.CompanyId == companyId && s.IsDefault && !s.IsDeleted)
            .ToListAsync();

        foreach (var loc in defaultLocations)
        {
            loc.IsDefault = false;
        }
    }
}
