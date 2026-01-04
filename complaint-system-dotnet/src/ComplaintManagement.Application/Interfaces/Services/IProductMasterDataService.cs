using ComplaintManagement.Application.DTOs.MasterData;

namespace ComplaintManagement.Application.Interfaces.Services;

public interface IProductMasterDataService
{
    // Product Status Masters
    Task<IEnumerable<ProductStatusMasterDto>> GetProductStatusesAsync(Guid companyId, bool? isActive = null, bool includeSystem = true);
    Task<ProductStatusMasterDto?> GetProductStatusByIdAsync(Guid id, Guid companyId);
    Task<ProductStatusMasterDto?> GetProductStatusByCodeAsync(string code, Guid companyId);
    Task<ProductStatusMasterDto> CreateProductStatusAsync(CreateProductStatusMasterRequest request, Guid companyId, Guid userId);
    Task<ProductStatusMasterDto?> UpdateProductStatusAsync(UpdateProductStatusMasterRequest request, Guid companyId, Guid userId);
    Task<bool> DeleteProductStatusAsync(Guid id, Guid companyId, Guid userId);

    // Product Type Masters
    Task<IEnumerable<ProductTypeMasterDto>> GetProductTypesAsync(Guid companyId, bool? isActive = null, bool includeSystem = true);
    Task<ProductTypeMasterDto?> GetProductTypeByIdAsync(Guid id, Guid companyId);
    Task<ProductTypeMasterDto?> GetProductTypeByCodeAsync(string code, Guid companyId);
    Task<ProductTypeMasterDto> CreateProductTypeAsync(CreateProductTypeMasterRequest request, Guid companyId, Guid userId);
    Task<ProductTypeMasterDto?> UpdateProductTypeAsync(UpdateProductTypeMasterRequest request, Guid companyId, Guid userId);
    Task<bool> DeleteProductTypeAsync(Guid id, Guid companyId, Guid userId);

    // Unit of Measure Masters
    Task<IEnumerable<UnitOfMeasureMasterDto>> GetUnitsOfMeasureAsync(Guid companyId, bool? isActive = null, bool includeSystem = true, string? category = null);
    Task<UnitOfMeasureMasterDto?> GetUnitOfMeasureByIdAsync(Guid id, Guid companyId);
    Task<UnitOfMeasureMasterDto?> GetUnitOfMeasureByCodeAsync(string code, Guid companyId);
    Task<UnitOfMeasureMasterDto> CreateUnitOfMeasureAsync(CreateUnitOfMeasureMasterRequest request, Guid companyId, Guid userId);
    Task<UnitOfMeasureMasterDto?> UpdateUnitOfMeasureAsync(UpdateUnitOfMeasureMasterRequest request, Guid companyId, Guid userId);
    Task<bool> DeleteUnitOfMeasureAsync(Guid id, Guid companyId, Guid userId);
}
