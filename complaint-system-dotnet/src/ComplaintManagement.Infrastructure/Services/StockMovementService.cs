using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Services;

public class StockMovementService : IStockMovementService
{
    private readonly ComplaintDbContext _context;
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public StockMovementService(ComplaintDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<StockMovementDto>>> GetAllAsync(
        Guid companyId,
        StockMovementFilterRequest filter,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.StockItem)
            .Include(m => m.Asset)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.FromStockCategory)
            .Include(m => m.ToStockCategory)
            .Include(m => m.Customer)
            .Where(m => m.CompanyId == companyId && !m.IsDeleted);

        // Apply filters
        if (filter.FromDate.HasValue)
        {
            query = query.Where(m => m.MovementDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            query = query.Where(m => m.MovementDate <= filter.ToDate.Value);
        }

        if (filter.MovementType.HasValue)
        {
            query = query.Where(m => m.MovementType == filter.MovementType.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(m => m.Status == filter.Status.Value);
        }

        if (filter.ProductId.HasValue)
        {
            query = query.Where(m => m.ProductId == filter.ProductId.Value);
        }

        if (filter.LocationId.HasValue)
        {
            query = query.Where(m => m.FromLocationId == filter.LocationId.Value ||
                                     m.ToLocationId == filter.LocationId.Value);
        }

        if (filter.StockCategoryId.HasValue)
        {
            query = query.Where(m => m.FromStockCategoryId == filter.StockCategoryId.Value ||
                                     m.ToStockCategoryId == filter.StockCategoryId.Value);
        }

        if (filter.CustomerId.HasValue)
        {
            query = query.Where(m => m.CustomerId == filter.CustomerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var searchTerm = filter.SearchTerm.ToLower();
            query = query.Where(m =>
                m.MovementNumber.ToLower().Contains(searchTerm) ||
                (m.ReferenceNumber != null && m.ReferenceNumber.ToLower().Contains(searchTerm)) ||
                (m.SerialNumber != null && m.SerialNumber.ToLower().Contains(searchTerm)) ||
                (m.BatchNumber != null && m.BatchNumber.ToLower().Contains(searchTerm)) ||
                (m.Reason != null && m.Reason.ToLower().Contains(searchTerm)));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(m => m.MovementDate)
            .ThenByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = items.Select(MapToDto).ToList();

        return Result<PagedResult<StockMovementDto>>.Success(
            new PagedResult<StockMovementDto>(dtos, totalCount, page, pageSize));
    }

    public async Task<Result<StockMovementDto>> GetByIdAsync(Guid id, Guid companyId)
    {
        var movement = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.StockItem)
            .Include(m => m.Asset)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.FromStockCategory)
            .Include(m => m.ToStockCategory)
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted);

        if (movement == null)
        {
            return Result<StockMovementDto>.Failure("Stock movement not found");
        }

        return Result<StockMovementDto>.Success(MapToDto(movement));
    }

    public async Task<Result<StockMovementDto>> GetByMovementNumberAsync(string movementNumber, Guid companyId)
    {
        var movement = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.StockItem)
            .Include(m => m.Asset)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.FromStockCategory)
            .Include(m => m.ToStockCategory)
            .Include(m => m.Customer)
            .FirstOrDefaultAsync(m => m.MovementNumber == movementNumber &&
                                      m.CompanyId == companyId && !m.IsDeleted);

        if (movement == null)
        {
            return Result<StockMovementDto>.Failure("Stock movement not found");
        }

        return Result<StockMovementDto>.Success(MapToDto(movement));
    }

    public async Task<Result<List<StockMovementDto>>> GetByStockItemAsync(Guid stockItemId, Guid companyId, int limit = 50)
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.FromStockCategory)
            .Include(m => m.ToStockCategory)
            .Where(m => m.StockItemId == stockItemId && m.CompanyId == companyId && !m.IsDeleted)
            .OrderByDescending(m => m.MovementDate)
            .Take(limit)
            .ToListAsync();

        var dtos = movements.Select(MapToDto).ToList();
        return Result<List<StockMovementDto>>.Success(dtos);
    }

    public async Task<Result<List<StockMovementDto>>> GetByAssetAsync(Guid assetId, Guid companyId, int limit = 50)
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.Asset)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.FromStockCategory)
            .Include(m => m.ToStockCategory)
            .Where(m => m.AssetId == assetId && m.CompanyId == companyId && !m.IsDeleted)
            .OrderByDescending(m => m.MovementDate)
            .Take(limit)
            .ToListAsync();

        var dtos = movements.Select(MapToDto).ToList();
        return Result<List<StockMovementDto>>.Success(dtos);
    }

    public async Task<Result<List<StockMovementDto>>> GetByProductAsync(Guid productId, Guid companyId, int limit = 50)
    {
        var movements = await _context.StockMovements
            .Include(m => m.Product)
            .Include(m => m.StockItem)
            .Include(m => m.Asset)
            .Include(m => m.FromLocation)
            .Include(m => m.ToLocation)
            .Include(m => m.FromStockCategory)
            .Include(m => m.ToStockCategory)
            .Where(m => m.ProductId == productId && m.CompanyId == companyId && !m.IsDeleted)
            .OrderByDescending(m => m.MovementDate)
            .Take(limit)
            .ToListAsync();

        var dtos = movements.Select(MapToDto).ToList();
        return Result<List<StockMovementDto>>.Success(dtos);
    }

    public async Task<Result<StockMovementDto>> CreateAsync(CreateStockMovementRequest request, Guid companyId, Guid userId)
    {
        // Get current quantity for the stock item if provided
        decimal? quantityBefore = null;
        decimal? quantityAfter = null;
        StockItem? stockItem = null;

        if (request.StockItemId.HasValue)
        {
            stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.Id == request.StockItemId.Value && s.CompanyId == companyId);

            if (stockItem != null)
            {
                quantityBefore = stockItem.QuantityOnHand;

                // Calculate quantity after based on movement type
                if (IsIncomingMovement(request.MovementType))
                {
                    quantityAfter = quantityBefore + request.Quantity;
                }
                else if (IsOutgoingMovement(request.MovementType))
                {
                    quantityAfter = quantityBefore - request.Quantity;
                }
                else if (IsReservationMovement(request.MovementType))
                {
                    // Reservation doesn't change quantity on hand
                    quantityAfter = quantityBefore;
                }
                else
                {
                    quantityAfter = quantityBefore;
                }
            }
        }

        var movementNumber = await GenerateMovementNumberAsync(request.MovementType, companyId);

        var movement = new StockMovement
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            StockItemId = request.StockItemId,
            AssetId = request.AssetId,
            ProductId = request.ProductId,
            MovementType = request.MovementType,
            MovementNumber = movementNumber,
            FromLocationId = request.FromLocationId,
            ToLocationId = request.ToLocationId,
            FromStockCategoryId = request.FromStockCategoryId,
            ToStockCategoryId = request.ToStockCategoryId,
            Quantity = request.Quantity,
            UnitOfMeasure = request.UnitOfMeasure,
            QuantityBefore = quantityBefore,
            QuantityAfter = quantityAfter,
            ReferenceType = request.ReferenceType,
            ReferenceNumber = request.ReferenceNumber,
            ReferenceId = request.ReferenceId,
            ComplaintId = request.ComplaintId,
            CustomerId = request.CustomerId,
            ProjectId = request.ProjectId,
            UnitCost = request.UnitCost,
            TotalValue = request.Quantity * (request.UnitCost ?? 0),
            Currency = request.Currency,
            MovementDate = request.MovementDate ?? DateTime.UtcNow,
            Reason = request.Reason,
            Status = StockMovementStatus.Completed,
            SerialNumber = request.SerialNumber,
            BatchNumber = request.BatchNumber,
            ExpiryDate = request.ExpiryDate,
            PerformedByUserId = userId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.StockMovements.Add(movement);

        // Apply the movement to the stock item
        if (stockItem != null && quantityAfter.HasValue)
        {
            // Update quantity on hand
            if (IsIncomingMovement(request.MovementType))
            {
                stockItem.QuantityOnHand += request.Quantity;
            }
            else if (IsOutgoingMovement(request.MovementType))
            {
                stockItem.QuantityOnHand -= request.Quantity;
            }

            // Update reserved quantity for reservation movements
            if (IsReservationMovement(request.MovementType))
            {
                if (request.MovementType == StockMovementType.Reservation)
                {
                    stockItem.QuantityReserved += request.Quantity;
                }
                else if (request.MovementType == StockMovementType.Unreservation)
                {
                    stockItem.QuantityReserved -= request.Quantity;
                }
            }

            stockItem.LastMovementDate = DateTime.UtcNow;
            stockItem.UpdatedAt = DateTime.UtcNow;
            stockItem.UpdatedBy = userId;
        }

        await _context.SaveChangesAsync();

        return await GetByIdAsync(movement.Id, companyId);
    }

    public async Task<Result<bool>> ApproveAsync(Guid id, Guid companyId, Guid userId)
    {
        var movement = await _context.StockMovements
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted);

        if (movement == null)
        {
            return Result<bool>.Failure("Stock movement not found");
        }

        if (movement.Status != StockMovementStatus.Pending)
        {
            return Result<bool>.Failure("Only pending movements can be approved");
        }

        movement.Status = StockMovementStatus.Approved;
        movement.ApprovedByUserId = userId;
        movement.ApprovalDate = DateTime.UtcNow;
        movement.UpdatedAt = DateTime.UtcNow;
        movement.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> RejectAsync(Guid id, string reason, Guid companyId, Guid userId)
    {
        var movement = await _context.StockMovements
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted);

        if (movement == null)
        {
            return Result<bool>.Failure("Stock movement not found");
        }

        if (movement.Status != StockMovementStatus.Pending)
        {
            return Result<bool>.Failure("Only pending movements can be rejected");
        }

        movement.Status = StockMovementStatus.Cancelled;
        movement.Notes = (movement.Notes ?? "") + $"\nRejected: {reason}";
        movement.UpdatedAt = DateTime.UtcNow;
        movement.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<bool>> CancelAsync(Guid id, string reason, Guid companyId, Guid userId)
    {
        var movement = await _context.StockMovements
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted);

        if (movement == null)
        {
            return Result<bool>.Failure("Stock movement not found");
        }

        if (movement.Status == StockMovementStatus.Completed ||
            movement.Status == StockMovementStatus.Reversed)
        {
            return Result<bool>.Failure("Completed or reversed movements cannot be cancelled. Use reverse instead.");
        }

        movement.Status = StockMovementStatus.Cancelled;
        movement.Notes = (movement.Notes ?? "") + $"\nCancelled: {reason}";
        movement.UpdatedAt = DateTime.UtcNow;
        movement.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }

    public async Task<Result<StockMovementDto>> ReverseAsync(Guid id, string reason, Guid companyId, Guid userId)
    {
        var originalMovement = await _context.StockMovements
            .Include(m => m.Product)
            .FirstOrDefaultAsync(m => m.Id == id && m.CompanyId == companyId && !m.IsDeleted);

        if (originalMovement == null)
        {
            return Result<StockMovementDto>.Failure("Stock movement not found");
        }

        if (originalMovement.Status != StockMovementStatus.Completed)
        {
            return Result<StockMovementDto>.Failure("Only completed movements can be reversed");
        }

        // Create reverse movement
        var reverseMovementType = GetReverseMovementType(originalMovement.MovementType);
        var reverseMovementNumber = await GenerateMovementNumberAsync(reverseMovementType, companyId);

        var reverseMovement = new StockMovement
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            StockItemId = originalMovement.StockItemId,
            AssetId = originalMovement.AssetId,
            ProductId = originalMovement.ProductId,
            MovementType = reverseMovementType,
            MovementNumber = reverseMovementNumber,
            // Swap from/to for reversal
            FromLocationId = originalMovement.ToLocationId,
            ToLocationId = originalMovement.FromLocationId,
            FromStockCategoryId = originalMovement.ToStockCategoryId,
            ToStockCategoryId = originalMovement.FromStockCategoryId,
            Quantity = originalMovement.Quantity,
            UnitOfMeasure = originalMovement.UnitOfMeasure,
            ReferenceType = StockReferenceType.Manual,
            ReferenceNumber = originalMovement.MovementNumber,
            ReferenceId = originalMovement.Id,
            UnitCost = originalMovement.UnitCost,
            TotalValue = originalMovement.TotalValue,
            Currency = originalMovement.Currency,
            MovementDate = DateTime.UtcNow,
            Reason = $"Reversal of {originalMovement.MovementNumber}: {reason}",
            Status = StockMovementStatus.Completed,
            SerialNumber = originalMovement.SerialNumber,
            BatchNumber = originalMovement.BatchNumber,
            PerformedByUserId = userId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        // Update original movement status
        originalMovement.Status = StockMovementStatus.Reversed;
        originalMovement.Notes = (originalMovement.Notes ?? "") + $"\nReversed: {reason}";
        originalMovement.UpdatedAt = DateTime.UtcNow;
        originalMovement.UpdatedBy = userId;

        // Update stock item quantity if applicable
        if (originalMovement.StockItemId.HasValue)
        {
            var stockItem = await _context.StockItems
                .FirstOrDefaultAsync(s => s.Id == originalMovement.StockItemId.Value && s.CompanyId == companyId);

            if (stockItem != null)
            {
                reverseMovement.QuantityBefore = stockItem.QuantityOnHand;

                if (IsIncomingMovement(originalMovement.MovementType))
                {
                    // Original was incoming, so reversal decreases quantity
                    stockItem.QuantityOnHand -= originalMovement.Quantity;
                }
                else if (IsOutgoingMovement(originalMovement.MovementType))
                {
                    // Original was outgoing, so reversal increases quantity
                    stockItem.QuantityOnHand += originalMovement.Quantity;
                }

                reverseMovement.QuantityAfter = stockItem.QuantityOnHand;
                stockItem.LastMovementDate = DateTime.UtcNow;
                stockItem.UpdatedAt = DateTime.UtcNow;
                stockItem.UpdatedBy = userId;
            }
        }

        _context.StockMovements.Add(reverseMovement);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(reverseMovement.Id, companyId);
    }

    public async Task<string> GenerateMovementNumberAsync(StockMovementType type, Guid companyId)
    {
        await _semaphore.WaitAsync();
        try
        {
            var prefix = GetMovementPrefix(type);
            var timestamp = DateTime.UtcNow.ToString("yyMMdd");
            var pattern = $"{prefix}-{timestamp}-%";

            // Get the latest movement number for this prefix and date
            var latestNumber = await _context.StockMovements
                .Where(m => m.CompanyId == companyId && EF.Functions.Like(m.MovementNumber, pattern))
                .OrderByDescending(m => m.MovementNumber)
                .Select(m => m.MovementNumber)
                .FirstOrDefaultAsync();

            int nextSequence = 1;
            if (!string.IsNullOrEmpty(latestNumber))
            {
                // Parse the sequence number from the movement number (e.g., "RCV-260103-00005" -> 5)
                var parts = latestNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int currentSeq))
                {
                    nextSequence = currentSeq + 1;
                }
            }

            return $"{prefix}-{timestamp}-{nextSequence:D5}";
        }
        finally
        {
            _semaphore.Release();
        }
    }

    // Keep sync version for backward compatibility (interface requirement)
    public string GenerateMovementNumber(StockMovementType type)
    {
        // This is a fallback - use GenerateMovementNumberAsync when possible
        var prefix = GetMovementPrefix(type);
        var timestamp = DateTime.UtcNow.ToString("yyMMdd");
        var guid = Guid.NewGuid().ToString("N").Substring(0, 5).ToUpper();
        return $"{prefix}-{timestamp}-{guid}";
    }

    private static string GetMovementPrefix(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Receipt => "RCV",
            StockMovementType.Return => "RTN",
            StockMovementType.TransferIn => "TIN",
            StockMovementType.TransferOut => "TOU",
            StockMovementType.Production => "PRD",
            StockMovementType.Adjustment_In => "AJI",
            StockMovementType.Adjustment_Out => "AJO",
            StockMovementType.Issue => "ISS",
            StockMovementType.Sale => "SAL",
            StockMovementType.Consumption => "CON",
            StockMovementType.Scrap => "SCR",
            StockMovementType.Loss => "LOS",
            StockMovementType.CategoryChange => "CAT",
            StockMovementType.Reservation => "RSV",
            StockMovementType.Unreservation => "URS",
            StockMovementType.StockCount => "CNT",
            StockMovementType.Revaluation => "RVL",
            _ => "MOV"
        };
    }

    private static StockMovementType GetReverseMovementType(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Receipt => StockMovementType.Adjustment_Out,
            StockMovementType.Return => StockMovementType.Issue,
            StockMovementType.TransferIn => StockMovementType.TransferOut,
            StockMovementType.TransferOut => StockMovementType.TransferIn,
            StockMovementType.Issue => StockMovementType.Return,
            StockMovementType.Sale => StockMovementType.Return,
            StockMovementType.Adjustment_In => StockMovementType.Adjustment_Out,
            StockMovementType.Adjustment_Out => StockMovementType.Adjustment_In,
            _ => StockMovementType.Adjustment_In
        };
    }

    private static bool IsIncomingMovement(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Receipt => true,
            StockMovementType.Return => true,
            StockMovementType.TransferIn => true,
            StockMovementType.Production => true,
            StockMovementType.Adjustment_In => true,
            _ => false
        };
    }

    private static bool IsOutgoingMovement(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Issue => true,
            StockMovementType.Sale => true,
            StockMovementType.TransferOut => true,
            StockMovementType.Consumption => true,
            StockMovementType.Adjustment_Out => true,
            StockMovementType.Scrap => true,
            StockMovementType.Loss => true,
            _ => false
        };
    }

    private static bool IsReservationMovement(StockMovementType type)
    {
        return type switch
        {
            StockMovementType.Reservation => true,
            StockMovementType.Unreservation => true,
            _ => false
        };
    }

    private static StockMovementDto MapToDto(StockMovement m)
    {
        return new StockMovementDto
        {
            Id = m.Id,
            CompanyId = m.CompanyId,
            StockItemId = m.StockItemId,
            AssetId = m.AssetId,
            AssetTag = m.Asset?.AssetTag,
            ProductId = m.ProductId,
            ProductCode = m.Product?.ProductCode,
            ProductName = m.Product?.Name,
            MovementType = m.MovementType,
            MovementNumber = m.MovementNumber,
            FromLocationId = m.FromLocationId,
            FromLocationName = m.FromLocation?.Name,
            ToLocationId = m.ToLocationId,
            ToLocationName = m.ToLocation?.Name,
            FromStockCategoryId = m.FromStockCategoryId,
            FromStockCategoryName = m.FromStockCategory?.Name,
            ToStockCategoryId = m.ToStockCategoryId,
            ToStockCategoryName = m.ToStockCategory?.Name,
            Quantity = m.Quantity,
            UnitOfMeasure = m.UnitOfMeasure,
            QuantityBefore = m.QuantityBefore,
            QuantityAfter = m.QuantityAfter,
            ReferenceType = m.ReferenceType,
            ReferenceNumber = m.ReferenceNumber,
            ReferenceId = m.ReferenceId,
            ComplaintId = m.ComplaintId,
            CustomerId = m.CustomerId,
            CustomerName = m.Customer?.Name,
            ProjectId = m.ProjectId,
            UnitCost = m.UnitCost,
            TotalValue = m.TotalValue,
            Currency = m.Currency,
            MovementDate = m.MovementDate,
            Reason = m.Reason,
            Status = m.Status,
            SerialNumber = m.SerialNumber,
            BatchNumber = m.BatchNumber,
            ExpiryDate = m.ExpiryDate,
            PerformedByUserId = m.PerformedByUserId,
            ApprovedByUserId = m.ApprovedByUserId,
            ApprovalDate = m.ApprovalDate,
            Notes = m.Notes,
            CreatedAt = m.CreatedAt
        };
    }
}
