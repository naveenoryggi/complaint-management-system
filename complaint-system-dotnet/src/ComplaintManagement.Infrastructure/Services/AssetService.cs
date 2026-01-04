using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service implementation for asset management operations
/// </summary>
public class AssetService : IAssetService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<AssetService> _logger;

    public AssetService(ComplaintDbContext context, ILogger<AssetService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Asset CRUD Operations

    public async Task<PagedResult<AssetSummaryDto>> GetAssetsAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 20,
        Guid? customerId = null,
        Guid? locationId = null,
        Guid? productId = null,
        Guid? contractId = null,
        AssetType? type = null,
        AssetStatus? status = null,
        AssetCondition? condition = null,
        bool? isOperational = null,
        bool? isUnderWarranty = null,
        bool? isUnderContract = null,
        string? searchTerm = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        var query = _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Include(a => a.Product)
            .Where(a => a.CompanyId == companyId);

        // Apply filters
        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId.Value);

        if (locationId.HasValue)
            query = query.Where(a => a.LocationId == locationId.Value);

        if (productId.HasValue)
            query = query.Where(a => a.ProductId == productId.Value);

        if (contractId.HasValue)
            query = query.Where(a => a.ContractId == contractId.Value);

        if (type.HasValue)
            query = query.Where(a => a.Type == type.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (condition.HasValue)
            query = query.Where(a => a.Condition == condition.Value);

        if (isOperational.HasValue)
            query = query.Where(a => a.IsOperational == isOperational.Value);

        if (isUnderWarranty == true)
            query = query.Where(a => a.WarrantyEndDate != null && a.WarrantyEndDate > DateTime.UtcNow);
        else if (isUnderWarranty == false)
            query = query.Where(a => a.WarrantyEndDate == null || a.WarrantyEndDate <= DateTime.UtcNow);

        if (isUnderContract == true)
            query = query.Where(a => a.ContractId != null);
        else if (isUnderContract == false)
            query = query.Where(a => a.ContractId == null);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(a =>
                a.AssetTag.ToLower().Contains(term) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(term)) ||
                a.Name.ToLower().Contains(term) ||
                (a.ProductName != null && a.ProductName.ToLower().Contains(term)) ||
                (a.Manufacturer != null && a.Manufacturer.ToLower().Contains(term)) ||
                (a.Model != null && a.Model.ToLower().Contains(term)));
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "assettag" => sortDescending ? query.OrderByDescending(a => a.AssetTag) : query.OrderBy(a => a.AssetTag),
            "name" => sortDescending ? query.OrderByDescending(a => a.Name) : query.OrderBy(a => a.Name),
            "customer" => sortDescending ? query.OrderByDescending(a => a.Customer.Name) : query.OrderBy(a => a.Customer.Name),
            "status" => sortDescending ? query.OrderByDescending(a => a.Status) : query.OrderBy(a => a.Status),
            "condition" => sortDescending ? query.OrderByDescending(a => a.Condition) : query.OrderBy(a => a.Condition),
            "lastservicedate" => sortDescending ? query.OrderByDescending(a => a.LastServiceDate) : query.OrderBy(a => a.LastServiceDate),
            "warrantyenddate" => sortDescending ? query.OrderByDescending(a => a.WarrantyEndDate) : query.OrderBy(a => a.WarrantyEndDate),
            "createdat" => sortDescending ? query.OrderByDescending(a => a.CreatedAt) : query.OrderBy(a => a.CreatedAt),
            _ => query.OrderByDescending(a => a.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();

        return new PagedResult<AssetSummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<AssetDto?> GetAssetByIdAsync(Guid companyId, Guid assetId)
    {
        var asset = await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Include(a => a.Product)
            .Include(a => a.Contract)
            .Include(a => a.ParentAsset)
            .Include(a => a.ServiceHistory.OrderByDescending(s => s.ServiceStartDate).Take(5))
            .Where(a => a.CompanyId == companyId && a.Id == assetId)
            .FirstOrDefaultAsync();

        return asset != null ? MapToDto(asset) : null;
    }

    public async Task<AssetDto?> GetAssetByTagAsync(Guid companyId, string assetTag)
    {
        var asset = await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Include(a => a.Product)
            .Include(a => a.Contract)
            .Where(a => a.CompanyId == companyId && a.AssetTag == assetTag)
            .FirstOrDefaultAsync();

        return asset != null ? MapToDto(asset) : null;
    }

    public async Task<AssetDto?> GetAssetBySerialNumberAsync(Guid companyId, string serialNumber)
    {
        var asset = await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Include(a => a.Product)
            .Include(a => a.Contract)
            .Where(a => a.CompanyId == companyId && a.SerialNumber == serialNumber)
            .FirstOrDefaultAsync();

        return asset != null ? MapToDto(asset) : null;
    }

    public async Task<Result<AssetDto>> CreateAssetAsync(Guid companyId, CreateAssetRequest request)
    {
        try
        {
            // Validate customer exists (only for non-internal assets with a specified customer)
            if (!request.IsInternal && request.CustomerId.HasValue)
            {
                var customerExists = await _context.Customers
                    .AnyAsync(c => c.CompanyId == companyId && c.Id == request.CustomerId);
                if (!customerExists)
                    return Result<AssetDto>.Failure("Customer not found");
            }

            // Generate asset tag if not provided
            var assetTag = string.IsNullOrWhiteSpace(request.AssetTag)
                ? await GenerateAssetTagAsync(companyId)
                : request.AssetTag;

            // Check if asset tag is unique
            if (!await IsAssetTagUniqueAsync(companyId, assetTag))
                return Result<AssetDto>.Failure($"Asset tag '{assetTag}' already exists");

            // Check if serial number is unique (if provided)
            if (!string.IsNullOrWhiteSpace(request.SerialNumber))
            {
                if (!await IsSerialNumberUniqueAsync(companyId, request.SerialNumber))
                    return Result<AssetDto>.Failure($"Serial number '{request.SerialNumber}' already exists");
            }

            var asset = new Asset
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                IsInternal = request.IsInternal,
                CustomerId = request.IsInternal ? null : request.CustomerId,
                LocationId = request.LocationId,
                ProductId = request.ProductId,
                ContractId = request.ContractId,
                WarrantyId = request.WarrantyId,
                AssetTag = assetTag,
                SerialNumber = request.SerialNumber,
                Name = request.Name,
                Description = request.Description,
                Type = request.Type,
                Barcode = request.Barcode,
                RFIDTag = request.RFIDTag,
                ProductName = request.ProductName,
                ProductCode = request.ProductCode,
                Manufacturer = request.Manufacturer,
                Model = request.Model,
                Version = request.Version,
                SKU = request.SKU,
                ManufactureDate = request.ManufactureDate,
                PurchaseDate = request.PurchaseDate,
                ReceivedDate = request.ReceivedDate,
                InstallationDate = request.InstallationDate,
                CommissionedDate = request.CommissionedDate,
                WarrantyStartDate = request.WarrantyStartDate,
                WarrantyEndDate = request.WarrantyEndDate,
                ExtendedWarrantyEndDate = request.ExtendedWarrantyEndDate,
                EndOfLifeDate = request.EndOfLifeDate,
                PurchasePrice = request.PurchasePrice,
                CurrentValue = request.CurrentValue ?? request.PurchasePrice,
                SalvageValue = request.SalvageValue,
                ReplacementCost = request.ReplacementCost,
                Currency = request.Currency,
                PONumber = request.PONumber,
                InvoiceNumber = request.InvoiceNumber,
                Vendor = request.Vendor,
                DepreciationMethod = request.DepreciationMethod,
                UsefulLifeMonths = request.UsefulLifeMonths,
                DepreciationRate = request.DepreciationRate,
                OwnershipType = request.OwnershipType,
                LeaseAgreementNumber = request.LeaseAgreementNumber,
                LeaseStartDate = request.LeaseStartDate,
                LeaseEndDate = request.LeaseEndDate,
                LeasePayment = request.LeasePayment,
                Status = request.Status,
                Condition = request.Condition,
                StatusReason = request.StatusReason,
                IsOperational = request.IsOperational,
                Specifications = request.Specifications,
                Configuration = request.Configuration,
                FirmwareVersion = request.FirmwareVersion,
                SoftwareVersion = request.SoftwareVersion,
                OperatingSystem = request.OperatingSystem,
                IPAddress = request.IPAddress,
                MACAddress = request.MACAddress,
                Hostname = request.Hostname,
                Building = request.Building,
                Floor = request.Floor,
                Room = request.Room,
                Rack = request.Rack,
                RackUnit = request.RackUnit,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                MaintenanceFrequency = request.MaintenanceFrequency,
                MaintenanceIntervalDays = request.MaintenanceIntervalDays,
                CurrentMeterReading = request.CurrentMeterReading,
                MeterUnit = request.MeterUnit,
                LastMeterReadingDate = request.CurrentMeterReading.HasValue ? DateTime.UtcNow : null,
                AssignedUserId = request.AssignedUserId,
                DepartmentId = request.DepartmentId,
                CostCenter = request.CostCenter,
                ExternalAssetId = request.ExternalAssetId,
                ParentAssetId = request.ParentAssetId,
                Notes = request.Notes,
                Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                CustomFields = request.CustomFields
            };

            // Copy product details from Product if ProductId is provided and details are empty
            if (request.ProductId.HasValue)
            {
                var product = await _context.Products.FindAsync(request.ProductId.Value);
                if (product != null)
                {
                    asset.ProductName ??= product.Name;
                    asset.ProductCode ??= product.ProductCode;
                    asset.Manufacturer ??= product.Manufacturer;
                    asset.Model ??= product.Model;
                    asset.SKU ??= product.SKU;

                    // Set warranty dates from product if not provided
                    if (!asset.WarrantyEndDate.HasValue && product.DefaultWarrantyMonths.HasValue)
                    {
                        var warrantyStart = asset.InstallationDate ?? asset.PurchaseDate ?? DateTime.UtcNow;
                        asset.WarrantyStartDate ??= warrantyStart;
                        asset.WarrantyEndDate = warrantyStart.AddMonths(product.DefaultWarrantyMonths.Value);
                    }
                }
            }

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created asset {AssetTag} for company {CompanyId}", assetTag, companyId);

            return Result<AssetDto>.Success(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating asset for company {CompanyId}", companyId);
            return Result<AssetDto>.Failure("Failed to create asset", ex.Message);
        }
    }

    public async Task<Result<AssetDto>> UpdateAssetAsync(Guid companyId, Guid assetId, UpdateAssetRequest request)
    {
        try
        {
            var asset = await _context.Assets
                .Include(a => a.Customer)
                .Where(a => a.CompanyId == companyId && a.Id == assetId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Result<AssetDto>.Failure("Asset not found");

            // Check serial number uniqueness if changed
            if (!string.IsNullOrWhiteSpace(request.SerialNumber) && request.SerialNumber != asset.SerialNumber)
            {
                if (!await IsSerialNumberUniqueAsync(companyId, request.SerialNumber, assetId))
                    return Result<AssetDto>.Failure($"Serial number '{request.SerialNumber}' already exists");
            }

            // Update fields
            asset.LocationId = request.LocationId;
            asset.ContractId = request.ContractId;
            asset.WarrantyId = request.WarrantyId;
            asset.SerialNumber = request.SerialNumber;
            asset.Name = request.Name;
            asset.Description = request.Description;
            asset.Type = request.Type;
            asset.Barcode = request.Barcode;
            asset.RFIDTag = request.RFIDTag;
            asset.ProductName = request.ProductName;
            asset.ProductCode = request.ProductCode;
            asset.Manufacturer = request.Manufacturer;
            asset.Model = request.Model;
            asset.Version = request.Version;
            asset.SKU = request.SKU;
            asset.ManufactureDate = request.ManufactureDate;
            asset.PurchaseDate = request.PurchaseDate;
            asset.ReceivedDate = request.ReceivedDate;
            asset.InstallationDate = request.InstallationDate;
            asset.CommissionedDate = request.CommissionedDate;
            asset.WarrantyStartDate = request.WarrantyStartDate;
            asset.WarrantyEndDate = request.WarrantyEndDate;
            asset.ExtendedWarrantyEndDate = request.ExtendedWarrantyEndDate;
            asset.EndOfLifeDate = request.EndOfLifeDate;
            asset.RetiredDate = request.RetiredDate;
            asset.DisposedDate = request.DisposedDate;
            asset.PurchasePrice = request.PurchasePrice;
            asset.CurrentValue = request.CurrentValue;
            asset.SalvageValue = request.SalvageValue;
            asset.ReplacementCost = request.ReplacementCost;
            asset.Currency = request.Currency;
            asset.PONumber = request.PONumber;
            asset.InvoiceNumber = request.InvoiceNumber;
            asset.Vendor = request.Vendor;
            asset.DepreciationMethod = request.DepreciationMethod;
            asset.UsefulLifeMonths = request.UsefulLifeMonths;
            asset.DepreciationRate = request.DepreciationRate;
            asset.AccumulatedDepreciation = request.AccumulatedDepreciation;
            asset.OwnershipType = request.OwnershipType;
            asset.LeaseAgreementNumber = request.LeaseAgreementNumber;
            asset.LeaseStartDate = request.LeaseStartDate;
            asset.LeaseEndDate = request.LeaseEndDate;
            asset.LeasePayment = request.LeasePayment;
            asset.Status = request.Status;
            asset.Condition = request.Condition;
            asset.StatusReason = request.StatusReason;
            asset.IsOperational = request.IsOperational;
            asset.Specifications = request.Specifications;
            asset.Configuration = request.Configuration;
            asset.FirmwareVersion = request.FirmwareVersion;
            asset.SoftwareVersion = request.SoftwareVersion;
            asset.OperatingSystem = request.OperatingSystem;
            asset.IPAddress = request.IPAddress;
            asset.MACAddress = request.MACAddress;
            asset.Hostname = request.Hostname;
            asset.Building = request.Building;
            asset.Floor = request.Floor;
            asset.Room = request.Room;
            asset.Rack = request.Rack;
            asset.RackUnit = request.RackUnit;
            asset.Latitude = request.Latitude;
            asset.Longitude = request.Longitude;
            asset.MaintenanceFrequency = request.MaintenanceFrequency;
            asset.MaintenanceIntervalDays = request.MaintenanceIntervalDays;
            asset.NextServiceDate = request.NextServiceDate;
            asset.CurrentMeterReading = request.CurrentMeterReading;
            asset.MeterUnit = request.MeterUnit;
            asset.AverageDailyUsage = request.AverageDailyUsage;
            asset.AssignedUserId = request.AssignedUserId;
            asset.DepartmentId = request.DepartmentId;
            asset.CostCenter = request.CostCenter;
            asset.ExternalAssetId = request.ExternalAssetId;
            asset.ParentAssetId = request.ParentAssetId;
            asset.Notes = request.Notes;
            asset.Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null;
            asset.CustomFields = request.CustomFields;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated asset {AssetId} for company {CompanyId}", assetId, companyId);

            return Result<AssetDto>.Success(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset {AssetId} for company {CompanyId}", assetId, companyId);
            return Result<AssetDto>.Failure("Failed to update asset", ex.Message);
        }
    }

    public async Task<Result<AssetDto>> UpdateAssetStatusAsync(Guid companyId, Guid assetId, UpdateAssetStatusRequest request)
    {
        try
        {
            var asset = await _context.Assets
                .Where(a => a.CompanyId == companyId && a.Id == assetId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Result<AssetDto>.Failure("Asset not found");

            asset.Status = request.Status;
            if (request.Condition.HasValue)
                asset.Condition = request.Condition.Value;
            asset.StatusReason = request.StatusReason;
            if (request.IsOperational.HasValue)
                asset.IsOperational = request.IsOperational.Value;

            // Set dates based on status
            switch (request.Status)
            {
                case AssetStatus.Retired:
                    asset.RetiredDate ??= DateTime.UtcNow;
                    asset.IsOperational = false;
                    break;
                case AssetStatus.Disposed:
                    asset.DisposedDate ??= DateTime.UtcNow;
                    asset.IsOperational = false;
                    break;
            }

            await _context.SaveChangesAsync();

            return Result<AssetDto>.Success(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating asset status {AssetId}", assetId);
            return Result<AssetDto>.Failure("Failed to update asset status", ex.Message);
        }
    }

    public async Task<Result> DeleteAssetAsync(Guid companyId, Guid assetId)
    {
        try
        {
            var asset = await _context.Assets
                .Where(a => a.CompanyId == companyId && a.Id == assetId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Result.Failure("Asset not found");

            _context.Assets.Remove(asset);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted asset {AssetId} for company {CompanyId}", assetId, companyId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting asset {AssetId}", assetId);
            return Result.Failure("Failed to delete asset", ex.Message);
        }
    }

    #endregion

    #region Asset Lookup

    public async Task<List<AssetLookupDto>> GetAssetLookupsAsync(
        Guid companyId,
        Guid? customerId = null,
        Guid? locationId = null,
        AssetStatus? status = null,
        bool? isOperational = null)
    {
        var query = _context.Assets
            .Include(a => a.Customer)
            .Where(a => a.CompanyId == companyId);

        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId.Value);

        if (locationId.HasValue)
            query = query.Where(a => a.LocationId == locationId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (isOperational.HasValue)
            query = query.Where(a => a.IsOperational == isOperational.Value);

        return await query
            .OrderBy(a => a.AssetTag)
            .Select(a => new AssetLookupDto
            {
                Id = a.Id,
                AssetTag = a.AssetTag,
                SerialNumber = a.SerialNumber,
                Name = a.Name,
                Type = a.Type,
                Status = a.Status,
                CustomerId = a.CustomerId,
                CustomerName = a.Customer.Name,
                IsOperational = a.IsOperational,
                IsUnderWarranty = a.WarrantyEndDate != null && a.WarrantyEndDate > DateTime.UtcNow
            })
            .ToListAsync();
    }

    public async Task<List<AssetSummaryDto>> SearchAssetsAsync(Guid companyId, string searchTerm, int maxResults = 10)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<AssetSummaryDto>();

        var term = searchTerm.ToLower();
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId &&
                (a.AssetTag.ToLower().Contains(term) ||
                 (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(term)) ||
                 a.Name.ToLower().Contains(term)))
            .Take(maxResults)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    #endregion

    #region Asset Operations

    public async Task<Result<AssetDto>> TransferAssetAsync(Guid companyId, Guid assetId, TransferAssetRequest request)
    {
        try
        {
            var asset = await _context.Assets
                .Where(a => a.CompanyId == companyId && a.Id == assetId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Result<AssetDto>.Failure("Asset not found");

            // Validate new customer exists
            var newCustomerExists = await _context.Customers
                .AnyAsync(c => c.CompanyId == companyId && c.Id == request.NewCustomerId);
            if (!newCustomerExists)
                return Result<AssetDto>.Failure("New customer not found");

            // Record transfer in service history
            var serviceHistory = new AssetServiceHistory
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                AssetId = assetId,
                ServiceNumber = await GenerateServiceNumberAsync(companyId),
                ServiceType = ServiceType.Relocation,
                Description = $"Asset transferred to new customer",
                WorkPerformed = request.TransferReason,
                Result = ServiceResult.Completed,
                ServiceStartDate = request.TransferDate,
                ServiceEndDate = request.TransferDate,
                LocationType = ServiceLocationType.Field,
                InternalNotes = request.Notes
            };

            asset.CustomerId = request.NewCustomerId;
            asset.LocationId = request.NewLocationId;
            asset.ContractId = null; // Clear contract on transfer

            _context.AssetServiceHistories.Add(serviceHistory);
            await _context.SaveChangesAsync();

            return Result<AssetDto>.Success(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transferring asset {AssetId}", assetId);
            return Result<AssetDto>.Failure("Failed to transfer asset", ex.Message);
        }
    }

    public async Task<Result<AssetDto>> RecordMeterReadingAsync(Guid companyId, Guid assetId, RecordMeterReadingRequest request)
    {
        try
        {
            var asset = await _context.Assets
                .Where(a => a.CompanyId == companyId && a.Id == assetId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Result<AssetDto>.Failure("Asset not found");

            var previousReading = asset.CurrentMeterReading;
            var previousDate = asset.LastMeterReadingDate;

            asset.CurrentMeterReading = request.MeterReading;
            asset.MeterUnit = request.MeterUnit ?? asset.MeterUnit;
            asset.LastMeterReadingDate = request.ReadingDate;

            // Calculate average daily usage if we have history
            if (previousReading.HasValue && previousDate.HasValue)
            {
                var daysDiff = (request.ReadingDate - previousDate.Value).TotalDays;
                if (daysDiff > 0)
                {
                    var usageDiff = request.MeterReading - previousReading.Value;
                    asset.AverageDailyUsage = usageDiff / (decimal)daysDiff;
                }
            }

            await _context.SaveChangesAsync();

            return Result<AssetDto>.Success(MapToDto(asset));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording meter reading for asset {AssetId}", assetId);
            return Result<AssetDto>.Failure("Failed to record meter reading", ex.Message);
        }
    }

    public async Task<List<AssetSummaryDto>> GetChildAssetsAsync(Guid companyId, Guid parentAssetId)
    {
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId && a.ParentAssetId == parentAssetId)
            .OrderBy(a => a.AssetTag)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    public async Task<List<AssetSummaryDto>> GetCustomerAssetsAsync(Guid companyId, Guid customerId)
    {
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId && a.CustomerId == customerId)
            .OrderBy(a => a.AssetTag)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    public async Task<List<AssetSummaryDto>> GetLocationAssetsAsync(Guid companyId, Guid locationId)
    {
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId && a.LocationId == locationId)
            .OrderBy(a => a.AssetTag)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    public async Task<List<AssetSummaryDto>> GetContractAssetsAsync(Guid companyId, Guid contractId)
    {
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId && a.ContractId == contractId)
            .OrderBy(a => a.AssetTag)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    public async Task<List<AssetSummaryDto>> GetAssetsNeedingServiceAsync(Guid companyId, int daysAhead = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId &&
                        a.NextServiceDate != null &&
                        a.NextServiceDate <= cutoffDate &&
                        a.IsOperational)
            .OrderBy(a => a.NextServiceDate)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    public async Task<List<AssetSummaryDto>> GetAssetsWithExpiringWarrantyAsync(Guid companyId, int daysAhead = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(daysAhead);
        return await _context.Assets
            .Include(a => a.Customer)
            .Include(a => a.Location)
            .Where(a => a.CompanyId == companyId &&
                        a.WarrantyEndDate != null &&
                        a.WarrantyEndDate > DateTime.UtcNow &&
                        a.WarrantyEndDate <= cutoffDate)
            .OrderBy(a => a.WarrantyEndDate)
            .Select(a => MapToSummaryDto(a))
            .ToListAsync();
    }

    #endregion

    #region Asset Statistics

    public async Task<AssetStatisticsDto> GetAssetStatisticsAsync(Guid companyId, Guid? customerId = null)
    {
        var query = _context.Assets.Where(a => a.CompanyId == companyId);
        if (customerId.HasValue)
            query = query.Where(a => a.CustomerId == customerId.Value);

        var now = DateTime.UtcNow;
        var assets = await query.ToListAsync();

        var stats = new AssetStatisticsDto
        {
            TotalAssets = assets.Count,
            OperationalAssets = assets.Count(a => a.IsOperational),
            NonOperationalAssets = assets.Count(a => !a.IsOperational),
            AssetsUnderWarranty = assets.Count(a => a.WarrantyEndDate != null && a.WarrantyEndDate > now),
            AssetsUnderContract = assets.Count(a => a.ContractId != null),
            AssetsNeedingService = assets.Count(a => a.NextServiceDate != null && a.NextServiceDate <= now.AddDays(7)),
            RetiredAssets = assets.Count(a => a.Status == AssetStatus.Retired || a.Status == AssetStatus.Disposed),
            ByStatus = assets.GroupBy(a => a.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ByType = assets.GroupBy(a => a.Type.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ByCondition = assets.GroupBy(a => a.Condition.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            TotalAssetValue = assets.Sum(a => a.CurrentValue ?? 0),
            TotalAccumulatedDepreciation = assets.Sum(a => a.AccumulatedDepreciation ?? 0)
        };

        return stats;
    }

    #endregion

    #region Service History Operations

    public async Task<PagedResult<AssetServiceHistorySummaryDto>> GetAssetServiceHistoryAsync(
        Guid companyId,
        Guid assetId,
        int page = 1,
        int pageSize = 20,
        ServiceType? serviceType = null,
        ServiceResult? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.AssetServiceHistories
            .Include(s => s.Asset)
            .Where(s => s.CompanyId == companyId && s.AssetId == assetId);

        if (serviceType.HasValue)
            query = query.Where(s => s.ServiceType == serviceType.Value);

        if (result.HasValue)
            query = query.Where(s => s.Result == result.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.ServiceStartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.ServiceStartDate <= toDate.Value);

        query = query.OrderByDescending(s => s.ServiceStartDate);

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToServiceSummaryDto(s))
            .ToListAsync();

        return new PagedResult<AssetServiceHistorySummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<AssetServiceHistoryDto?> GetServiceHistoryByIdAsync(Guid companyId, Guid serviceHistoryId)
    {
        var history = await _context.AssetServiceHistories
            .Include(s => s.Asset)
            .Include(s => s.Contract)
            .Where(s => s.CompanyId == companyId && s.Id == serviceHistoryId)
            .FirstOrDefaultAsync();

        return history != null ? MapToServiceDto(history) : null;
    }

    public async Task<Result<AssetServiceHistoryDto>> CreateServiceHistoryAsync(Guid companyId, CreateServiceHistoryRequest request)
    {
        try
        {
            var asset = await _context.Assets
                .Where(a => a.CompanyId == companyId && a.Id == request.AssetId)
                .FirstOrDefaultAsync();

            if (asset == null)
                return Result<AssetServiceHistoryDto>.Failure("Asset not found");

            var history = new AssetServiceHistory
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                AssetId = request.AssetId,
                ComplaintId = request.ComplaintId,
                ContractId = request.ContractId,
                TechnicianId = request.TechnicianId,
                ServiceNumber = await GenerateServiceNumberAsync(companyId),
                ServiceType = request.ServiceType,
                Description = request.Description,
                WorkPerformed = request.WorkPerformed,
                ProblemDescription = request.ProblemDescription,
                RootCause = request.RootCause,
                Resolution = request.Resolution,
                Result = request.Result,
                ScheduledDate = request.ScheduledDate,
                ServiceStartDate = request.ServiceStartDate,
                ServiceEndDate = request.ServiceEndDate,
                DurationHours = request.DurationHours,
                TravelTimeHours = request.TravelTimeHours,
                DowntimeHours = request.DowntimeHours,
                NextServiceDate = request.NextServiceDate,
                LocationType = request.LocationType,
                ServiceLocationId = request.ServiceLocationId,
                ServiceAddress = request.ServiceAddress,
                LaborCost = request.LaborCost,
                PartsCost = request.PartsCost,
                TravelCost = request.TravelCost,
                OtherCosts = request.OtherCosts,
                TotalCost = (request.LaborCost ?? 0) + (request.PartsCost ?? 0) + (request.TravelCost ?? 0) + (request.OtherCosts ?? 0),
                Currency = request.Currency,
                IsBillable = request.IsBillable,
                IsWarrantyCovered = request.IsWarrantyCovered,
                IsContractCovered = request.IsContractCovered,
                InvoiceNumber = request.InvoiceNumber,
                PartsUsed = request.PartsUsed != null ? JsonSerializer.Serialize(request.PartsUsed) : null,
                PartsReplaced = request.PartsReplaced != null ? JsonSerializer.Serialize(request.PartsReplaced) : null,
                StatusBefore = request.StatusBefore,
                StatusAfter = request.StatusAfter,
                ConditionBefore = request.ConditionBefore,
                ConditionAfter = request.ConditionAfter,
                MeterReadingBefore = request.MeterReadingBefore,
                MeterReadingAfter = request.MeterReadingAfter,
                FirmwareVersionBefore = request.FirmwareVersionBefore,
                FirmwareVersionAfter = request.FirmwareVersionAfter,
                SoftwareVersionBefore = request.SoftwareVersionBefore,
                SoftwareVersionAfter = request.SoftwareVersionAfter,
                ConfigurationChanges = request.ConfigurationChanges,
                CustomerSignedOff = request.CustomerSignedOff,
                SignedOffBy = request.SignedOffBy,
                SignedOffAt = request.SignedOffAt,
                CustomerRating = request.CustomerRating,
                CustomerFeedback = request.CustomerFeedback,
                InternalNotes = request.InternalNotes,
                Recommendations = request.Recommendations,
                FollowUpActions = request.FollowUpActions,
                Attachments = request.Attachments != null ? JsonSerializer.Serialize(request.Attachments) : null,
                Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null,
                ExternalReferenceNumber = request.ExternalReferenceNumber
            };

            _context.AssetServiceHistories.Add(history);

            // Update asset if requested
            if (request.UpdateAssetStatus && request.StatusAfter.HasValue)
            {
                asset.Status = request.StatusAfter.Value;
                if (request.ConditionAfter.HasValue)
                    asset.Condition = request.ConditionAfter.Value;
            }

            if (request.UpdateNextServiceDate && request.NextServiceDate.HasValue)
            {
                asset.NextServiceDate = request.NextServiceDate;
            }

            asset.LastServiceDate = request.ServiceEndDate ?? request.ServiceStartDate ?? DateTime.UtcNow;
            asset.ServiceCallCount++;

            if (request.MeterReadingAfter.HasValue)
            {
                asset.CurrentMeterReading = request.MeterReadingAfter;
                asset.LastMeterReadingDate = DateTime.UtcNow;
            }

            if (!string.IsNullOrEmpty(request.FirmwareVersionAfter))
                asset.FirmwareVersion = request.FirmwareVersionAfter;

            if (!string.IsNullOrEmpty(request.SoftwareVersionAfter))
                asset.SoftwareVersion = request.SoftwareVersionAfter;

            if (request.DowntimeHours.HasValue)
                asset.TotalDowntimeHours = (asset.TotalDowntimeHours ?? 0) + request.DowntimeHours.Value;

            await _context.SaveChangesAsync();

            return Result<AssetServiceHistoryDto>.Success(MapToServiceDto(history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating service history for asset {AssetId}", request.AssetId);
            return Result<AssetServiceHistoryDto>.Failure("Failed to create service history", ex.Message);
        }
    }

    public async Task<Result<AssetServiceHistoryDto>> UpdateServiceHistoryAsync(Guid companyId, Guid serviceHistoryId, UpdateServiceHistoryRequest request)
    {
        try
        {
            var history = await _context.AssetServiceHistories
                .Where(s => s.CompanyId == companyId && s.Id == serviceHistoryId)
                .FirstOrDefaultAsync();

            if (history == null)
                return Result<AssetServiceHistoryDto>.Failure("Service history record not found");

            history.TechnicianId = request.TechnicianId;
            history.Description = request.Description;
            history.WorkPerformed = request.WorkPerformed;
            history.ProblemDescription = request.ProblemDescription;
            history.RootCause = request.RootCause;
            history.Resolution = request.Resolution;
            history.Result = request.Result;
            history.ServiceStartDate = request.ServiceStartDate;
            history.ServiceEndDate = request.ServiceEndDate;
            history.DurationHours = request.DurationHours;
            history.TravelTimeHours = request.TravelTimeHours;
            history.DowntimeHours = request.DowntimeHours;
            history.NextServiceDate = request.NextServiceDate;
            history.LaborCost = request.LaborCost;
            history.PartsCost = request.PartsCost;
            history.TravelCost = request.TravelCost;
            history.OtherCosts = request.OtherCosts;
            history.TotalCost = (request.LaborCost ?? 0) + (request.PartsCost ?? 0) + (request.TravelCost ?? 0) + (request.OtherCosts ?? 0);
            history.IsBillable = request.IsBillable;
            history.InvoiceNumber = request.InvoiceNumber;
            history.PartsUsed = request.PartsUsed != null ? JsonSerializer.Serialize(request.PartsUsed) : null;
            history.PartsReplaced = request.PartsReplaced != null ? JsonSerializer.Serialize(request.PartsReplaced) : null;
            history.StatusAfter = request.StatusAfter;
            history.ConditionAfter = request.ConditionAfter;
            history.MeterReadingAfter = request.MeterReadingAfter;
            history.FirmwareVersionAfter = request.FirmwareVersionAfter;
            history.SoftwareVersionAfter = request.SoftwareVersionAfter;
            history.ConfigurationChanges = request.ConfigurationChanges;
            history.CustomerSignedOff = request.CustomerSignedOff;
            history.SignedOffBy = request.SignedOffBy;
            history.SignedOffAt = request.SignedOffAt;
            history.CustomerRating = request.CustomerRating;
            history.CustomerFeedback = request.CustomerFeedback;
            history.InternalNotes = request.InternalNotes;
            history.Recommendations = request.Recommendations;
            history.FollowUpActions = request.FollowUpActions;
            history.Attachments = request.Attachments != null ? JsonSerializer.Serialize(request.Attachments) : null;
            history.Tags = request.Tags != null ? JsonSerializer.Serialize(request.Tags) : null;

            await _context.SaveChangesAsync();

            return Result<AssetServiceHistoryDto>.Success(MapToServiceDto(history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating service history {ServiceHistoryId}", serviceHistoryId);
            return Result<AssetServiceHistoryDto>.Failure("Failed to update service history", ex.Message);
        }
    }

    public async Task<Result> DeleteServiceHistoryAsync(Guid companyId, Guid serviceHistoryId)
    {
        try
        {
            var history = await _context.AssetServiceHistories
                .Where(s => s.CompanyId == companyId && s.Id == serviceHistoryId)
                .FirstOrDefaultAsync();

            if (history == null)
                return Result.Failure("Service history record not found");

            _context.AssetServiceHistories.Remove(history);
            await _context.SaveChangesAsync();

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting service history {ServiceHistoryId}", serviceHistoryId);
            return Result.Failure("Failed to delete service history", ex.Message);
        }
    }

    public async Task<PagedResult<AssetServiceHistorySummaryDto>> GetAllServiceHistoryAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 20,
        Guid? customerId = null,
        Guid? technicianId = null,
        ServiceType? serviceType = null,
        ServiceResult? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? sortBy = null,
        bool sortDescending = false)
    {
        var query = _context.AssetServiceHistories
            .Include(s => s.Asset)
            .ThenInclude(a => a.Customer)
            .Where(s => s.CompanyId == companyId);

        if (customerId.HasValue)
            query = query.Where(s => s.Asset.CustomerId == customerId.Value);

        if (technicianId.HasValue)
            query = query.Where(s => s.TechnicianId == technicianId.Value);

        if (serviceType.HasValue)
            query = query.Where(s => s.ServiceType == serviceType.Value);

        if (result.HasValue)
            query = query.Where(s => s.Result == result.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.ServiceStartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.ServiceStartDate <= toDate.Value);

        query = sortBy?.ToLower() switch
        {
            "servicenumber" => sortDescending ? query.OrderByDescending(s => s.ServiceNumber) : query.OrderBy(s => s.ServiceNumber),
            "servicetype" => sortDescending ? query.OrderByDescending(s => s.ServiceType) : query.OrderBy(s => s.ServiceType),
            "result" => sortDescending ? query.OrderByDescending(s => s.Result) : query.OrderBy(s => s.Result),
            _ => query.OrderByDescending(s => s.ServiceStartDate)
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToServiceSummaryDto(s))
            .ToListAsync();

        return new PagedResult<AssetServiceHistorySummaryDto>(items, page, pageSize, totalCount);
    }

    public async Task<ServiceHistoryStatisticsDto> GetServiceHistoryStatisticsAsync(
        Guid companyId,
        Guid? assetId = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var query = _context.AssetServiceHistories
            .Include(s => s.Asset)
            .Where(s => s.CompanyId == companyId);

        if (assetId.HasValue)
            query = query.Where(s => s.AssetId == assetId.Value);

        if (customerId.HasValue)
            query = query.Where(s => s.Asset.CustomerId == customerId.Value);

        if (fromDate.HasValue)
            query = query.Where(s => s.ServiceStartDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(s => s.ServiceStartDate <= toDate.Value);

        var records = await query.ToListAsync();

        return new ServiceHistoryStatisticsDto
        {
            TotalServiceRecords = records.Count,
            CompletedServices = records.Count(r => r.Result == ServiceResult.Completed),
            FailedServices = records.Count(r => r.Result == ServiceResult.Failed),
            InProgressServices = records.Count(r => r.Result == ServiceResult.InProgress),
            WarrantyCoveredServices = records.Count(r => r.IsWarrantyCovered),
            ContractCoveredServices = records.Count(r => r.IsContractCovered),
            BillableServices = records.Count(r => r.IsBillable),
            TotalLaborCost = records.Sum(r => r.LaborCost ?? 0),
            TotalPartsCost = records.Sum(r => r.PartsCost ?? 0),
            TotalServiceCost = records.Sum(r => r.TotalCost ?? 0),
            TotalDowntimeHours = records.Sum(r => r.DowntimeHours ?? 0),
            AverageServiceDuration = records.Any(r => r.DurationHours.HasValue) ? records.Where(r => r.DurationHours.HasValue).Average(r => r.DurationHours!.Value) : 0,
            AverageCustomerRating = records.Any(r => r.CustomerRating.HasValue) ? (decimal)records.Where(r => r.CustomerRating.HasValue).Average(r => r.CustomerRating!.Value) : 0,
            ByServiceType = records.GroupBy(r => r.ServiceType.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ByResult = records.GroupBy(r => r.Result.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ByLocationType = records.GroupBy(r => r.LocationType.ToString()).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    #endregion

    #region Tag Generation

    public async Task<string> GenerateAssetTagAsync(Guid companyId)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"AST-{year}-";

        var lastAsset = await _context.Assets
            .Where(a => a.CompanyId == companyId && a.AssetTag.StartsWith(prefix))
            .OrderByDescending(a => a.AssetTag)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastAsset != null)
        {
            var lastNumberStr = lastAsset.AssetTag.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D5}";
    }

    public async Task<string> GenerateServiceNumberAsync(Guid companyId)
    {
        var year = DateTime.UtcNow.Year;
        var month = DateTime.UtcNow.Month;
        var prefix = $"SVC-{year}{month:D2}-";

        var lastService = await _context.AssetServiceHistories
            .Where(s => s.CompanyId == companyId && s.ServiceNumber.StartsWith(prefix))
            .OrderByDescending(s => s.ServiceNumber)
            .FirstOrDefaultAsync();

        int nextNumber = 1;
        if (lastService != null)
        {
            var lastNumberStr = lastService.ServiceNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumberStr, out int lastNumber))
                nextNumber = lastNumber + 1;
        }

        return $"{prefix}{nextNumber:D5}";
    }

    #endregion

    #region Validation

    public async Task<bool> IsAssetTagUniqueAsync(Guid companyId, string assetTag, Guid? excludeAssetId = null)
    {
        var query = _context.Assets.Where(a => a.CompanyId == companyId && a.AssetTag == assetTag);
        if (excludeAssetId.HasValue)
            query = query.Where(a => a.Id != excludeAssetId.Value);

        return !await query.AnyAsync();
    }

    public async Task<bool> IsSerialNumberUniqueAsync(Guid companyId, string serialNumber, Guid? excludeAssetId = null)
    {
        var query = _context.Assets.Where(a => a.CompanyId == companyId && a.SerialNumber == serialNumber);
        if (excludeAssetId.HasValue)
            query = query.Where(a => a.Id != excludeAssetId.Value);

        return !await query.AnyAsync();
    }

    public async Task<bool> IsAssetUnderWarrantyAsync(Guid assetId)
    {
        var asset = await _context.Assets.FindAsync(assetId);
        if (asset == null) return false;

        var effectiveWarrantyEnd = asset.ExtendedWarrantyEndDate ?? asset.WarrantyEndDate;
        return effectiveWarrantyEnd != null && effectiveWarrantyEnd > DateTime.UtcNow;
    }

    public async Task<bool> IsAssetUnderContractAsync(Guid assetId)
    {
        var asset = await _context.Assets
            .Include(a => a.Contract)
            .FirstOrDefaultAsync(a => a.Id == assetId);

        if (asset?.Contract == null) return false;

        return asset.Contract.Status == ContractStatus.Active &&
               asset.Contract.EndDate > DateTime.UtcNow;
    }

    #endregion

    #region Private Mapping Methods

    private static AssetDto MapToDto(Asset asset)
    {
        var now = DateTime.UtcNow;
        var effectiveWarrantyEnd = asset.ExtendedWarrantyEndDate ?? asset.WarrantyEndDate;
        var isUnderWarranty = effectiveWarrantyEnd != null && effectiveWarrantyEnd > now;
        var warrantyDaysRemaining = effectiveWarrantyEnd.HasValue ? (int?)(effectiveWarrantyEnd.Value - now).TotalDays : null;

        return new AssetDto
        {
            Id = asset.Id,
            CompanyId = asset.CompanyId,
            IsInternal = asset.IsInternal,
            CustomerId = asset.CustomerId,
            CustomerName = asset.Customer?.Name ?? string.Empty,
            LocationId = asset.LocationId,
            LocationName = asset.Location?.Name,
            ProductId = asset.ProductId,
            ContractId = asset.ContractId,
            ContractNumber = asset.Contract?.ContractNumber,
            WarrantyId = asset.WarrantyId,
            AssetTag = asset.AssetTag,
            SerialNumber = asset.SerialNumber,
            Name = asset.Name,
            Description = asset.Description,
            Type = asset.Type,
            Barcode = asset.Barcode,
            RFIDTag = asset.RFIDTag,
            ProductName = asset.ProductName,
            ProductCode = asset.ProductCode,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            Version = asset.Version,
            SKU = asset.SKU,
            ManufactureDate = asset.ManufactureDate,
            PurchaseDate = asset.PurchaseDate,
            ReceivedDate = asset.ReceivedDate,
            InstallationDate = asset.InstallationDate,
            CommissionedDate = asset.CommissionedDate,
            WarrantyStartDate = asset.WarrantyStartDate,
            WarrantyEndDate = asset.WarrantyEndDate,
            ExtendedWarrantyEndDate = asset.ExtendedWarrantyEndDate,
            LastServiceDate = asset.LastServiceDate,
            NextServiceDate = asset.NextServiceDate,
            EndOfLifeDate = asset.EndOfLifeDate,
            RetiredDate = asset.RetiredDate,
            DisposedDate = asset.DisposedDate,
            IsUnderWarranty = isUnderWarranty,
            WarrantyDaysRemaining = warrantyDaysRemaining > 0 ? (int)warrantyDaysRemaining : null,
            IsWarrantyExpiringSoon = warrantyDaysRemaining > 0 && warrantyDaysRemaining <= 30,
            PurchasePrice = asset.PurchasePrice,
            CurrentValue = asset.CurrentValue,
            SalvageValue = asset.SalvageValue,
            ReplacementCost = asset.ReplacementCost,
            Currency = asset.Currency,
            PONumber = asset.PONumber,
            InvoiceNumber = asset.InvoiceNumber,
            Vendor = asset.Vendor,
            DepreciationMethod = asset.DepreciationMethod,
            UsefulLifeMonths = asset.UsefulLifeMonths,
            DepreciationRate = asset.DepreciationRate,
            AccumulatedDepreciation = asset.AccumulatedDepreciation,
            OwnershipType = asset.OwnershipType,
            LeaseAgreementNumber = asset.LeaseAgreementNumber,
            LeaseStartDate = asset.LeaseStartDate,
            LeaseEndDate = asset.LeaseEndDate,
            LeasePayment = asset.LeasePayment,
            Status = asset.Status,
            Condition = asset.Condition,
            StatusReason = asset.StatusReason,
            IsOperational = asset.IsOperational,
            FirmwareVersion = asset.FirmwareVersion,
            SoftwareVersion = asset.SoftwareVersion,
            OperatingSystem = asset.OperatingSystem,
            IPAddress = asset.IPAddress,
            MACAddress = asset.MACAddress,
            Hostname = asset.Hostname,
            Building = asset.Building,
            Floor = asset.Floor,
            Room = asset.Room,
            Rack = asset.Rack,
            RackUnit = asset.RackUnit,
            Latitude = asset.Latitude,
            Longitude = asset.Longitude,
            MaintenanceFrequency = asset.MaintenanceFrequency,
            MaintenanceIntervalDays = asset.MaintenanceIntervalDays,
            ServiceCallCount = asset.ServiceCallCount,
            TotalDowntimeHours = asset.TotalDowntimeHours,
            MTBF = asset.MTBF,
            MTTR = asset.MTTR,
            CurrentMeterReading = asset.CurrentMeterReading,
            MeterUnit = asset.MeterUnit,
            LastMeterReadingDate = asset.LastMeterReadingDate,
            AverageDailyUsage = asset.AverageDailyUsage,
            AssignedUserId = asset.AssignedUserId,
            DepartmentId = asset.DepartmentId,
            CostCenter = asset.CostCenter,
            ExternalAssetId = asset.ExternalAssetId,
            ParentAssetId = asset.ParentAssetId,
            ParentAssetTag = asset.ParentAsset?.AssetTag,
            Notes = asset.Notes,
            Tags = !string.IsNullOrEmpty(asset.Tags) ? JsonSerializer.Deserialize<List<string>>(asset.Tags) ?? new() : new(),
            ChildAssetCount = asset.ChildAssets?.Count ?? 0,
            ServiceHistoryCount = asset.ServiceHistory?.Count ?? 0,
            ContractItemCount = asset.ContractItems?.Count ?? 0,
            RecentServiceHistory = asset.ServiceHistory?.Select(s => new AssetServiceHistorySummaryDto
            {
                Id = s.Id,
                ServiceNumber = s.ServiceNumber,
                AssetId = s.AssetId,
                AssetTag = asset.AssetTag,
                AssetName = asset.Name,
                ServiceType = s.ServiceType,
                Description = s.Description,
                Result = s.Result,
                ServiceStartDate = s.ServiceStartDate,
                ServiceEndDate = s.ServiceEndDate,
                DurationHours = s.DurationHours,
                TotalCost = s.TotalCost,
                Currency = s.Currency,
                IsBillable = s.IsBillable,
                IsWarrantyCovered = s.IsWarrantyCovered,
                IsContractCovered = s.IsContractCovered,
                CustomerSignedOff = s.CustomerSignedOff,
                CustomerRating = s.CustomerRating
            }).ToList() ?? new(),
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };
    }

    private static AssetSummaryDto MapToSummaryDto(Asset asset)
    {
        var now = DateTime.UtcNow;
        var effectiveWarrantyEnd = asset.ExtendedWarrantyEndDate ?? asset.WarrantyEndDate;

        return new AssetSummaryDto
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            SerialNumber = asset.SerialNumber,
            Name = asset.Name,
            Type = asset.Type,
            IsInternal = asset.IsInternal,
            CustomerId = asset.CustomerId,
            CustomerName = asset.Customer?.Name ?? string.Empty,
            LocationName = asset.Location?.Name,
            ProductName = asset.ProductName,
            Manufacturer = asset.Manufacturer,
            Model = asset.Model,
            Status = asset.Status,
            Condition = asset.Condition,
            IsOperational = asset.IsOperational,
            IsUnderWarranty = effectiveWarrantyEnd != null && effectiveWarrantyEnd > now,
            IsUnderContract = asset.ContractId != null,
            WarrantyEndDate = asset.WarrantyEndDate,
            LastServiceDate = asset.LastServiceDate,
            NextServiceDate = asset.NextServiceDate,
            CurrentValue = asset.CurrentValue,
            Currency = asset.Currency,
            ServiceCallCount = asset.ServiceCallCount
        };
    }

    private static AssetServiceHistoryDto MapToServiceDto(AssetServiceHistory history)
    {
        return new AssetServiceHistoryDto
        {
            Id = history.Id,
            CompanyId = history.CompanyId,
            AssetId = history.AssetId,
            AssetTag = history.Asset?.AssetTag ?? string.Empty,
            AssetName = history.Asset?.Name ?? string.Empty,
            ComplaintId = history.ComplaintId,
            ContractId = history.ContractId,
            ContractNumber = history.Contract?.ContractNumber,
            TechnicianId = history.TechnicianId,
            ServiceNumber = history.ServiceNumber,
            ServiceType = history.ServiceType,
            Description = history.Description,
            WorkPerformed = history.WorkPerformed,
            ProblemDescription = history.ProblemDescription,
            RootCause = history.RootCause,
            Resolution = history.Resolution,
            Result = history.Result,
            ScheduledDate = history.ScheduledDate,
            ServiceStartDate = history.ServiceStartDate,
            ServiceEndDate = history.ServiceEndDate,
            DurationHours = history.DurationHours,
            TravelTimeHours = history.TravelTimeHours,
            DowntimeHours = history.DowntimeHours,
            NextServiceDate = history.NextServiceDate,
            LocationType = history.LocationType,
            ServiceLocationId = history.ServiceLocationId,
            ServiceAddress = history.ServiceAddress,
            LaborCost = history.LaborCost,
            PartsCost = history.PartsCost,
            TravelCost = history.TravelCost,
            OtherCosts = history.OtherCosts,
            TotalCost = history.TotalCost,
            Currency = history.Currency,
            IsBillable = history.IsBillable,
            IsWarrantyCovered = history.IsWarrantyCovered,
            IsContractCovered = history.IsContractCovered,
            InvoiceNumber = history.InvoiceNumber,
            PartsUsed = !string.IsNullOrEmpty(history.PartsUsed) ? JsonSerializer.Deserialize<List<PartUsedDto>>(history.PartsUsed) : null,
            PartsReplaced = !string.IsNullOrEmpty(history.PartsReplaced) ? JsonSerializer.Deserialize<List<PartReplacedDto>>(history.PartsReplaced) : null,
            StatusBefore = history.StatusBefore,
            StatusAfter = history.StatusAfter,
            ConditionBefore = history.ConditionBefore,
            ConditionAfter = history.ConditionAfter,
            MeterReadingBefore = history.MeterReadingBefore,
            MeterReadingAfter = history.MeterReadingAfter,
            FirmwareVersionBefore = history.FirmwareVersionBefore,
            FirmwareVersionAfter = history.FirmwareVersionAfter,
            SoftwareVersionBefore = history.SoftwareVersionBefore,
            SoftwareVersionAfter = history.SoftwareVersionAfter,
            CustomerSignedOff = history.CustomerSignedOff,
            SignedOffBy = history.SignedOffBy,
            SignedOffAt = history.SignedOffAt,
            CustomerRating = history.CustomerRating,
            CustomerFeedback = history.CustomerFeedback,
            InternalNotes = history.InternalNotes,
            Recommendations = history.Recommendations,
            FollowUpActions = history.FollowUpActions,
            Attachments = !string.IsNullOrEmpty(history.Attachments) ? JsonSerializer.Deserialize<List<string>>(history.Attachments) : null,
            Tags = !string.IsNullOrEmpty(history.Tags) ? JsonSerializer.Deserialize<List<string>>(history.Tags) : null,
            ExternalReferenceNumber = history.ExternalReferenceNumber,
            CreatedAt = history.CreatedAt,
            UpdatedAt = history.UpdatedAt
        };
    }

    private static AssetServiceHistorySummaryDto MapToServiceSummaryDto(AssetServiceHistory history)
    {
        return new AssetServiceHistorySummaryDto
        {
            Id = history.Id,
            ServiceNumber = history.ServiceNumber,
            AssetId = history.AssetId,
            AssetTag = history.Asset?.AssetTag ?? string.Empty,
            AssetName = history.Asset?.Name ?? string.Empty,
            ServiceType = history.ServiceType,
            Description = history.Description,
            Result = history.Result,
            ServiceStartDate = history.ServiceStartDate,
            ServiceEndDate = history.ServiceEndDate,
            DurationHours = history.DurationHours,
            LocationType = history.LocationType,
            TotalCost = history.TotalCost,
            Currency = history.Currency,
            IsBillable = history.IsBillable,
            IsWarrantyCovered = history.IsWarrantyCovered,
            IsContractCovered = history.IsContractCovered,
            CustomerSignedOff = history.CustomerSignedOff,
            CustomerRating = history.CustomerRating
        };
    }

    #endregion
}
