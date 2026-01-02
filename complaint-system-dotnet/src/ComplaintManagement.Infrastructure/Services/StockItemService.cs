using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Services;

public class StockItemService : IStockItemService
{
    private readonly ComplaintDbContext _context;
    private readonly IStockMovementService _movementService;

    public StockItemService(ComplaintDbContext context, IStockMovementService movementService)
    {
        _context = context;
        _movementService = movementService;
    }

    public async Task<Result<PagedResult<StockItemDto>>> GetAllAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? locationId = null,
        Guid? stockCategoryId = null,
        Guid? productId = null,
        bool? lowStock = null)
    {
        var query = _context.StockItems
            .Include(s => s.Product)
            .Include(s => s.StockCategory)
            .Include(s => s.Location)
            .Where(s => s.CompanyId == companyId && !s.IsDeleted);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            searchTerm = searchTerm.ToLower();
            query = query.Where(s =>
                (s.ProductCode != null && s.ProductCode.ToLower().Contains(searchTerm)) ||
                (s.ProductName != null && s.ProductName.ToLower().Contains(searchTerm)) ||
                (s.SKU != null && s.SKU.ToLower().Contains(searchTerm)) ||
                (s.BatchNumber != null && s.BatchNumber.ToLower().Contains(searchTerm)));
        }

        if (locationId.HasValue)
        {
            query = query.Where(s => s.LocationId == locationId.Value);
        }

        if (stockCategoryId.HasValue)
        {
            query = query.Where(s => s.StockCategoryId == stockCategoryId.Value);
        }

        if (productId.HasValue)
        {
            query = query.Where(s => s.ProductId == productId.Value);
        }

        if (lowStock == true)
        {
            query = query.Where(s => s.MinimumQuantity.HasValue &&
                (s.QuantityOnHand - s.QuantityReserved) <= s.MinimumQuantity.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(s => s.ProductName)
            .ThenBy(s => s.Location != null ? s.Location.Name : "")
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return Result<PagedResult<StockItemDto>>.Success(
            new PagedResult<StockItemDto>(items, totalCount, page, pageSize));
    }

    public async Task<Result<List<StockItemLookupDto>>> GetLookupAsync(
        Guid companyId,
        Guid? locationId = null,
        Guid? stockCategoryId = null,
        bool availableOnly = true)
    {
        var query = _context.StockItems
            .Include(s => s.Location)
            .Include(s => s.StockCategory)
            .Where(s => s.CompanyId == companyId && !s.IsDeleted && s.Status == StockItemStatus.Active);

        if (locationId.HasValue)
        {
            query = query.Where(s => s.LocationId == locationId.Value);
        }

        if (stockCategoryId.HasValue)
        {
            query = query.Where(s => s.StockCategoryId == stockCategoryId.Value);
        }

        if (availableOnly)
        {
            query = query.Where(s => (s.QuantityOnHand - s.QuantityReserved) > 0);
        }

        var items = await query
            .OrderBy(s => s.ProductName)
            .Select(s => new StockItemLookupDto
            {
                Id = s.Id,
                ProductCode = s.ProductCode,
                ProductName = s.ProductName,
                SKU = s.SKU,
                LocationName = s.Location != null ? s.Location.Name : null,
                StockCategoryName = s.StockCategory != null ? s.StockCategory.Name : null,
                QuantityAvailable = s.QuantityOnHand - s.QuantityReserved
            })
            .ToListAsync();

        return Result<List<StockItemLookupDto>>.Success(items);
    }

    public async Task<Result<StockItemDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var item = await _context.StockItems
            .Include(s => s.Product)
            .Include(s => s.StockCategory)
            .Include(s => s.Location)
            .Where(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return Result<StockItemDto>.Failure("Stock item not found");
        }

        return Result<StockItemDto>.Success(MapToDto(item));
    }

    public async Task<Result<StockItemDto>> GetByProductAndLocationAsync(
        Guid productId,
        Guid? locationId,
        Guid stockCategoryId,
        Guid companyId)
    {
        var item = await _context.StockItems
            .Include(s => s.Product)
            .Include(s => s.StockCategory)
            .Include(s => s.Location)
            .Where(s => s.CompanyId == companyId &&
                        s.ProductId == productId &&
                        s.LocationId == locationId &&
                        s.StockCategoryId == stockCategoryId &&
                        !s.IsDeleted)
            .FirstOrDefaultAsync();

        if (item == null)
        {
            return Result<StockItemDto>.Failure("Stock item not found");
        }

        return Result<StockItemDto>.Success(MapToDto(item));
    }

    public async Task<Result<StockItemDto>> CreateAsync(CreateStockItemRequest request, Guid companyId, Guid userId)
    {
        // Check if stock item already exists for this product/location/category combination
        var exists = await _context.StockItems
            .AnyAsync(s => s.CompanyId == companyId &&
                          s.ProductId == request.ProductId &&
                          s.LocationId == request.LocationId &&
                          s.StockCategoryId == request.StockCategoryId &&
                          !s.IsDeleted);

        if (exists)
        {
            return Result<StockItemDto>.Failure("Stock item already exists for this product/location/category combination");
        }

        // Get product details for denormalization
        var product = await _context.Products
            .Where(p => p.Id == request.ProductId && p.CompanyId == companyId && !p.IsDeleted)
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return Result<StockItemDto>.Failure("Product not found");
        }

        var item = new StockItem
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ProductId = request.ProductId,
            StockCategoryId = request.StockCategoryId,
            LocationId = request.LocationId,
            QuantityOnHand = request.QuantityOnHand,
            QuantityReserved = request.QuantityReserved,
            MinimumQuantity = request.MinimumQuantity,
            MaximumQuantity = request.MaximumQuantity,
            ReorderQuantity = request.ReorderQuantity,
            SafetyStock = request.SafetyStock,
            ProductCode = product.ProductCode,
            ProductName = product.Name,
            SKU = product.SKU,
            UnitOfMeasure = request.UnitOfMeasure,
            UnitCost = request.UnitCost,
            Currency = request.Currency,
            CostingMethod = request.CostingMethod,
            Status = request.Status,
            Condition = request.Condition,
            ExpiryDate = request.ExpiryDate,
            BatchNumber = request.BatchNumber,
            CustomerId = request.CustomerId,
            ProjectId = request.ProjectId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.StockItems.Add(item);
        await _context.SaveChangesAsync();

        // Create initial receipt movement if quantity > 0
        if (request.QuantityOnHand > 0)
        {
            await _movementService.CreateAsync(new CreateStockMovementRequest
            {
                StockItemId = item.Id,
                ProductId = request.ProductId,
                MovementType = StockMovementType.Receipt,
                ToLocationId = request.LocationId,
                ToStockCategoryId = request.StockCategoryId,
                Quantity = request.QuantityOnHand,
                UnitOfMeasure = request.UnitOfMeasure,
                UnitCost = request.UnitCost,
                Currency = request.Currency,
                Reason = "Initial stock receipt",
                BatchNumber = request.BatchNumber,
                ExpiryDate = request.ExpiryDate
            }, companyId, userId);
        }

        return await GetByIdAsync(item.Id, companyId);
    }

    public async Task<Result<StockItemDto>> UpdateAsync(Guid id, UpdateStockItemRequest request, Guid companyId, Guid userId)
    {
        var item = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (item == null)
        {
            return Result<StockItemDto>.Failure("Stock item not found");
        }

        item.MinimumQuantity = request.MinimumQuantity;
        item.MaximumQuantity = request.MaximumQuantity;
        item.ReorderQuantity = request.ReorderQuantity;
        item.SafetyStock = request.SafetyStock;
        item.UnitOfMeasure = request.UnitOfMeasure;
        item.UnitCost = request.UnitCost;
        item.Currency = request.Currency;
        item.CostingMethod = request.CostingMethod;
        item.Status = request.Status;
        item.Condition = request.Condition;
        item.ExpiryDate = request.ExpiryDate;
        item.BatchNumber = request.BatchNumber;
        item.CustomerId = request.CustomerId;
        item.ProjectId = request.ProjectId;
        item.Notes = request.Notes;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return await GetByIdAsync(item.Id, companyId);
    }

    public async Task<Result<bool>> DeleteAsync(Guid id, Guid companyId, Guid userId)
    {
        var item = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == id && s.CompanyId == companyId && !s.IsDeleted);

        if (item == null)
        {
            return Result<bool>.Failure("Stock item not found");
        }

        if (item.QuantityOnHand > 0)
        {
            return Result<bool>.Failure("Cannot delete stock item with quantity on hand. Adjust quantity to zero first.");
        }

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = userId;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<StockMovementDto>> AdjustStockAsync(AdjustStockRequest request, Guid companyId, Guid userId)
    {
        var item = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == request.StockItemId && s.CompanyId == companyId && !s.IsDeleted);

        if (item == null)
        {
            return Result<StockMovementDto>.Failure("Stock item not found");
        }

        var newQuantity = item.QuantityOnHand + request.AdjustmentQuantity;
        if (newQuantity < 0)
        {
            return Result<StockMovementDto>.Failure("Adjustment would result in negative stock quantity");
        }

        var movementType = request.AdjustmentQuantity > 0
            ? StockMovementType.Adjustment_In
            : StockMovementType.Adjustment_Out;

        var movementResult = await _movementService.CreateAsync(new CreateStockMovementRequest
        {
            StockItemId = item.Id,
            ProductId = item.ProductId,
            MovementType = movementType,
            FromLocationId = movementType == StockMovementType.Adjustment_Out ? item.LocationId : null,
            ToLocationId = movementType == StockMovementType.Adjustment_In ? item.LocationId : null,
            FromStockCategoryId = movementType == StockMovementType.Adjustment_Out ? item.StockCategoryId : null,
            ToStockCategoryId = movementType == StockMovementType.Adjustment_In ? item.StockCategoryId : null,
            Quantity = Math.Abs(request.AdjustmentQuantity),
            UnitOfMeasure = item.UnitOfMeasure,
            UnitCost = item.UnitCost,
            Currency = item.Currency,
            Reason = request.Reason,
            Notes = request.Notes
        }, companyId, userId);

        if (!movementResult.IsSuccess)
        {
            return movementResult;
        }

        // Update stock quantity
        item.QuantityOnHand = newQuantity;
        item.LastMovementDate = DateTime.UtcNow;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = userId;

        // Check if attention is required
        UpdateAttentionStatus(item);

        await _context.SaveChangesAsync();

        return movementResult;
    }

    public async Task<Result<StockMovementDto>> TransferStockAsync(TransferStockRequest request, Guid companyId, Guid userId)
    {
        var sourceItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.Id == request.StockItemId && s.CompanyId == companyId && !s.IsDeleted);

        if (sourceItem == null)
        {
            return Result<StockMovementDto>.Failure("Source stock item not found");
        }

        var availableQuantity = sourceItem.QuantityOnHand - sourceItem.QuantityReserved;
        if (request.Quantity > availableQuantity)
        {
            return Result<StockMovementDto>.Failure($"Insufficient available quantity. Available: {availableQuantity}");
        }

        var targetCategoryId = request.ToStockCategoryId ?? sourceItem.StockCategoryId;

        // Check if target stock item exists
        var targetItem = await _context.StockItems
            .FirstOrDefaultAsync(s => s.CompanyId == companyId &&
                                      s.ProductId == sourceItem.ProductId &&
                                      s.LocationId == request.ToLocationId &&
                                      s.StockCategoryId == targetCategoryId &&
                                      !s.IsDeleted);

        // Create target stock item if it doesn't exist
        if (targetItem == null)
        {
            targetItem = new StockItem
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                ProductId = sourceItem.ProductId,
                StockCategoryId = targetCategoryId,
                LocationId = request.ToLocationId,
                QuantityOnHand = 0,
                QuantityReserved = 0,
                ProductCode = sourceItem.ProductCode,
                ProductName = sourceItem.ProductName,
                SKU = sourceItem.SKU,
                UnitOfMeasure = sourceItem.UnitOfMeasure,
                UnitCost = sourceItem.UnitCost,
                Currency = sourceItem.Currency,
                CostingMethod = sourceItem.CostingMethod,
                Status = StockItemStatus.Active,
                Condition = sourceItem.Condition,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };
            _context.StockItems.Add(targetItem);
        }

        // Create transfer movement
        var movementResult = await _movementService.CreateAsync(new CreateStockMovementRequest
        {
            StockItemId = sourceItem.Id,
            ProductId = sourceItem.ProductId,
            MovementType = StockMovementType.TransferOut,
            FromLocationId = sourceItem.LocationId,
            ToLocationId = request.ToLocationId,
            FromStockCategoryId = sourceItem.StockCategoryId,
            ToStockCategoryId = targetCategoryId,
            Quantity = request.Quantity,
            UnitOfMeasure = sourceItem.UnitOfMeasure,
            UnitCost = sourceItem.UnitCost,
            Currency = sourceItem.Currency,
            Reason = request.Reason,
            Notes = request.Notes
        }, companyId, userId);

        if (!movementResult.IsSuccess)
        {
            return movementResult;
        }

        // Update quantities
        sourceItem.QuantityOnHand -= request.Quantity;
        sourceItem.LastMovementDate = DateTime.UtcNow;
        sourceItem.UpdatedAt = DateTime.UtcNow;
        sourceItem.UpdatedBy = userId;
        UpdateAttentionStatus(sourceItem);

        targetItem.QuantityOnHand += request.Quantity;
        targetItem.LastMovementDate = DateTime.UtcNow;
        targetItem.UpdatedAt = DateTime.UtcNow;
        targetItem.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return movementResult;
    }

    public async Task<Result<List<StockItemDto>>> GetLowStockItemsAsync(Guid companyId)
    {
        var items = await _context.StockItems
            .Include(s => s.Product)
            .Include(s => s.StockCategory)
            .Include(s => s.Location)
            .Where(s => s.CompanyId == companyId &&
                       !s.IsDeleted &&
                       s.Status == StockItemStatus.Active &&
                       s.MinimumQuantity.HasValue &&
                       (s.QuantityOnHand - s.QuantityReserved) <= s.MinimumQuantity.Value)
            .OrderBy(s => s.QuantityOnHand - s.QuantityReserved)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return Result<List<StockItemDto>>.Success(items);
    }

    public async Task<Result<List<StockItemDto>>> GetExpiringItemsAsync(Guid companyId, int daysAhead = 30)
    {
        var expiryThreshold = DateTime.UtcNow.AddDays(daysAhead);

        var items = await _context.StockItems
            .Include(s => s.Product)
            .Include(s => s.StockCategory)
            .Include(s => s.Location)
            .Where(s => s.CompanyId == companyId &&
                       !s.IsDeleted &&
                       s.Status == StockItemStatus.Active &&
                       s.ExpiryDate.HasValue &&
                       s.ExpiryDate.Value <= expiryThreshold &&
                       s.QuantityOnHand > 0)
            .OrderBy(s => s.ExpiryDate)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return Result<List<StockItemDto>>.Success(items);
    }

    private void UpdateAttentionStatus(StockItem item)
    {
        var available = item.QuantityOnHand - item.QuantityReserved;

        if (item.MinimumQuantity.HasValue && available <= item.MinimumQuantity.Value)
        {
            item.RequiresAttention = true;
            item.AttentionReason = "Low stock level";
        }
        else if (item.ExpiryDate.HasValue && item.ExpiryDate.Value <= DateTime.UtcNow.AddDays(30))
        {
            item.RequiresAttention = true;
            item.AttentionReason = "Approaching expiry date";
        }
        else
        {
            item.RequiresAttention = false;
            item.AttentionReason = null;
        }
    }

    private static StockItemDto MapToDto(StockItem s)
    {
        return new StockItemDto
        {
            Id = s.Id,
            CompanyId = s.CompanyId,
            ProductId = s.ProductId,
            ProductCode = s.ProductCode,
            ProductName = s.ProductName,
            SKU = s.SKU,
            StockCategoryId = s.StockCategoryId,
            StockCategoryName = s.StockCategory?.Name,
            StockCategoryCode = s.StockCategory?.Code,
            LocationId = s.LocationId,
            LocationName = s.Location?.Name,
            LocationCode = s.Location?.Code,
            QuantityOnHand = s.QuantityOnHand,
            QuantityReserved = s.QuantityReserved,
            QuantityAvailable = s.QuantityOnHand - s.QuantityReserved,
            QuantityInTransit = s.QuantityInTransit,
            QuantityOnOrder = s.QuantityOnOrder,
            MinimumQuantity = s.MinimumQuantity,
            MaximumQuantity = s.MaximumQuantity,
            ReorderQuantity = s.ReorderQuantity,
            SafetyStock = s.SafetyStock,
            UnitOfMeasure = s.UnitOfMeasure,
            UnitCost = s.UnitCost,
            TotalValue = s.QuantityOnHand * (s.UnitCost ?? 0),
            Currency = s.Currency,
            CostingMethod = s.CostingMethod,
            LastPurchasePrice = s.LastPurchasePrice,
            LastPurchaseDate = s.LastPurchaseDate,
            Status = s.Status,
            Condition = s.Condition,
            RequiresAttention = s.RequiresAttention,
            AttentionReason = s.AttentionReason,
            LastCountDate = s.LastCountDate,
            LastCountQuantity = s.LastCountQuantity,
            LastCountVariance = s.LastCountVariance,
            LastMovementDate = s.LastMovementDate,
            DaysSinceLastMovement = s.LastMovementDate.HasValue
                ? (int)(DateTime.UtcNow - s.LastMovementDate.Value).TotalDays
                : null,
            ExpiryDate = s.ExpiryDate,
            BatchNumber = s.BatchNumber,
            CustomerId = s.CustomerId,
            ProjectId = s.ProjectId,
            Notes = s.Notes,
            CreatedAt = s.CreatedAt,
            UpdatedAt = s.UpdatedAt
        };
    }
}
