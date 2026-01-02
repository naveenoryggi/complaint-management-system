using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;

namespace ComplaintManagement.Application.Interfaces.Services;

public interface IStockItemService
{
    Task<Result<PagedResult<StockItemDto>>> GetAllAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        Guid? locationId = null,
        Guid? stockCategoryId = null,
        Guid? productId = null,
        bool? lowStock = null);

    Task<Result<List<StockItemLookupDto>>> GetLookupAsync(
        Guid companyId,
        Guid? locationId = null,
        Guid? stockCategoryId = null,
        bool availableOnly = true);

    Task<Result<StockItemDto>> GetByIdAsync(Guid id, Guid companyId);

    Task<Result<StockItemDto>> GetByProductAndLocationAsync(
        Guid productId,
        Guid? locationId,
        Guid stockCategoryId,
        Guid companyId);

    Task<Result<StockItemDto>> CreateAsync(CreateStockItemRequest request, Guid companyId, Guid userId);

    Task<Result<StockItemDto>> UpdateAsync(Guid id, UpdateStockItemRequest request, Guid companyId, Guid userId);

    Task<Result<bool>> DeleteAsync(Guid id, Guid companyId, Guid userId);

    Task<Result<StockMovementDto>> AdjustStockAsync(AdjustStockRequest request, Guid companyId, Guid userId);

    Task<Result<StockMovementDto>> TransferStockAsync(TransferStockRequest request, Guid companyId, Guid userId);

    Task<Result<List<StockItemDto>>> GetLowStockItemsAsync(Guid companyId);

    Task<Result<List<StockItemDto>>> GetExpiringItemsAsync(Guid companyId, int daysAhead = 30);
}
