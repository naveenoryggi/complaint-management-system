using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;

namespace ComplaintManagement.Application.Interfaces.Services;

public interface IStockLocationService
{
    Task<Result<PagedResult<StockLocationDto>>> GetAllAsync(
        Guid companyId,
        int page = 1,
        int pageSize = 10,
        string? searchTerm = null,
        bool? isActive = null);

    Task<Result<List<StockLocationLookupDto>>> GetLookupAsync(Guid companyId, bool activeOnly = true);

    Task<Result<List<StockLocationLookupDto>>> GetHierarchyAsync(Guid companyId, Guid? parentId = null);

    Task<Result<StockLocationDto>> GetByIdAsync(Guid id, Guid companyId);

    Task<Result<StockLocationDto>> GetByCodeAsync(string code, Guid companyId);

    Task<Result<StockLocationDto>> CreateAsync(CreateStockLocationRequest request, Guid companyId, Guid userId);

    Task<Result<StockLocationDto>> UpdateAsync(Guid id, UpdateStockLocationRequest request, Guid companyId, Guid userId);

    Task<Result<bool>> DeleteAsync(Guid id, Guid companyId, Guid userId);

    Task<Result<StockLocationDto>> SetDefaultAsync(Guid id, Guid companyId, Guid userId);
}
