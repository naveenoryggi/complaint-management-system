using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Domain.Entities.Service;

namespace ComplaintManagement.Application.Interfaces.Services;

public interface IStockMovementService
{
    Task<Result<PagedResult<StockMovementDto>>> GetAllAsync(
        Guid companyId,
        StockMovementFilterRequest filter,
        int page = 1,
        int pageSize = 10);

    Task<Result<StockMovementDto>> GetByIdAsync(Guid id, Guid companyId);

    Task<Result<StockMovementDto>> GetByMovementNumberAsync(string movementNumber, Guid companyId);

    Task<Result<List<StockMovementDto>>> GetByStockItemAsync(Guid stockItemId, Guid companyId, int limit = 50);

    Task<Result<List<StockMovementDto>>> GetByAssetAsync(Guid assetId, Guid companyId, int limit = 50);

    Task<Result<List<StockMovementDto>>> GetByProductAsync(Guid productId, Guid companyId, int limit = 50);

    Task<Result<StockMovementDto>> CreateAsync(CreateStockMovementRequest request, Guid companyId, Guid userId);

    Task<Result<bool>> ApproveAsync(Guid id, Guid companyId, Guid userId);

    Task<Result<bool>> RejectAsync(Guid id, string reason, Guid companyId, Guid userId);

    Task<Result<bool>> CancelAsync(Guid id, string reason, Guid companyId, Guid userId);

    Task<Result<StockMovementDto>> ReverseAsync(Guid id, string reason, Guid companyId, Guid userId);

    string GenerateMovementNumber(StockMovementType type);
}
