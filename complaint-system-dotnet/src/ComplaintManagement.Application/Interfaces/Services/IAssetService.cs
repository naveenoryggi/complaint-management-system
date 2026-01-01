using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Asset;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for asset management operations
/// </summary>
public interface IAssetService
{
    #region Asset CRUD Operations

    /// <summary>
    /// Gets a paginated list of assets with optional filtering
    /// </summary>
    Task<PagedResult<AssetSummaryDto>> GetAssetsAsync(
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
        bool sortDescending = false);

    /// <summary>
    /// Gets an asset by ID
    /// </summary>
    Task<AssetDto?> GetAssetByIdAsync(Guid companyId, Guid assetId);

    /// <summary>
    /// Gets an asset by asset tag
    /// </summary>
    Task<AssetDto?> GetAssetByTagAsync(Guid companyId, string assetTag);

    /// <summary>
    /// Gets an asset by serial number
    /// </summary>
    Task<AssetDto?> GetAssetBySerialNumberAsync(Guid companyId, string serialNumber);

    /// <summary>
    /// Creates a new asset
    /// </summary>
    Task<Result<AssetDto>> CreateAssetAsync(Guid companyId, CreateAssetRequest request);

    /// <summary>
    /// Updates an existing asset
    /// </summary>
    Task<Result<AssetDto>> UpdateAssetAsync(Guid companyId, Guid assetId, UpdateAssetRequest request);

    /// <summary>
    /// Updates asset status
    /// </summary>
    Task<Result<AssetDto>> UpdateAssetStatusAsync(Guid companyId, Guid assetId, UpdateAssetStatusRequest request);

    /// <summary>
    /// Soft deletes an asset
    /// </summary>
    Task<Result> DeleteAssetAsync(Guid companyId, Guid assetId);

    #endregion

    #region Asset Lookup

    /// <summary>
    /// Gets assets for dropdown lookups
    /// </summary>
    Task<List<AssetLookupDto>> GetAssetLookupsAsync(
        Guid companyId,
        Guid? customerId = null,
        Guid? locationId = null,
        AssetStatus? status = null,
        bool? isOperational = null);

    /// <summary>
    /// Searches assets by various criteria
    /// </summary>
    Task<List<AssetSummaryDto>> SearchAssetsAsync(
        Guid companyId,
        string searchTerm,
        int maxResults = 10);

    #endregion

    #region Asset Operations

    /// <summary>
    /// Transfers an asset to a different customer/location
    /// </summary>
    Task<Result<AssetDto>> TransferAssetAsync(Guid companyId, Guid assetId, TransferAssetRequest request);

    /// <summary>
    /// Records a meter reading for an asset
    /// </summary>
    Task<Result<AssetDto>> RecordMeterReadingAsync(Guid companyId, Guid assetId, RecordMeterReadingRequest request);

    /// <summary>
    /// Gets child assets of a parent asset
    /// </summary>
    Task<List<AssetSummaryDto>> GetChildAssetsAsync(Guid companyId, Guid parentAssetId);

    /// <summary>
    /// Gets assets by customer
    /// </summary>
    Task<List<AssetSummaryDto>> GetCustomerAssetsAsync(Guid companyId, Guid customerId);

    /// <summary>
    /// Gets assets at a specific location
    /// </summary>
    Task<List<AssetSummaryDto>> GetLocationAssetsAsync(Guid companyId, Guid locationId);

    /// <summary>
    /// Gets assets covered under a specific contract
    /// </summary>
    Task<List<AssetSummaryDto>> GetContractAssetsAsync(Guid companyId, Guid contractId);

    /// <summary>
    /// Gets assets needing service (next service date passed or approaching)
    /// </summary>
    Task<List<AssetSummaryDto>> GetAssetsNeedingServiceAsync(Guid companyId, int daysAhead = 7);

    /// <summary>
    /// Gets assets with warranty expiring soon
    /// </summary>
    Task<List<AssetSummaryDto>> GetAssetsWithExpiringWarrantyAsync(Guid companyId, int daysAhead = 30);

    #endregion

    #region Asset Statistics

    /// <summary>
    /// Gets asset statistics for dashboard
    /// </summary>
    Task<AssetStatisticsDto> GetAssetStatisticsAsync(Guid companyId, Guid? customerId = null);

    #endregion

    #region Service History Operations

    /// <summary>
    /// Gets paginated service history for an asset
    /// </summary>
    Task<PagedResult<AssetServiceHistorySummaryDto>> GetAssetServiceHistoryAsync(
        Guid companyId,
        Guid assetId,
        int page = 1,
        int pageSize = 20,
        ServiceType? serviceType = null,
        ServiceResult? result = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    /// <summary>
    /// Gets a specific service history record
    /// </summary>
    Task<AssetServiceHistoryDto?> GetServiceHistoryByIdAsync(Guid companyId, Guid serviceHistoryId);

    /// <summary>
    /// Creates a new service history record
    /// </summary>
    Task<Result<AssetServiceHistoryDto>> CreateServiceHistoryAsync(Guid companyId, CreateServiceHistoryRequest request);

    /// <summary>
    /// Updates a service history record
    /// </summary>
    Task<Result<AssetServiceHistoryDto>> UpdateServiceHistoryAsync(Guid companyId, Guid serviceHistoryId, UpdateServiceHistoryRequest request);

    /// <summary>
    /// Deletes a service history record
    /// </summary>
    Task<Result> DeleteServiceHistoryAsync(Guid companyId, Guid serviceHistoryId);

    /// <summary>
    /// Gets all service history for a company
    /// </summary>
    Task<PagedResult<AssetServiceHistorySummaryDto>> GetAllServiceHistoryAsync(
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
        bool sortDescending = false);

    /// <summary>
    /// Gets service history statistics
    /// </summary>
    Task<ServiceHistoryStatisticsDto> GetServiceHistoryStatisticsAsync(
        Guid companyId,
        Guid? assetId = null,
        Guid? customerId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null);

    #endregion

    #region Tag Generation

    /// <summary>
    /// Generates a unique asset tag
    /// </summary>
    Task<string> GenerateAssetTagAsync(Guid companyId);

    /// <summary>
    /// Generates a unique service number
    /// </summary>
    Task<string> GenerateServiceNumberAsync(Guid companyId);

    #endregion

    #region Validation

    /// <summary>
    /// Checks if an asset tag is unique within a company
    /// </summary>
    Task<bool> IsAssetTagUniqueAsync(Guid companyId, string assetTag, Guid? excludeAssetId = null);

    /// <summary>
    /// Checks if a serial number is unique within a company
    /// </summary>
    Task<bool> IsSerialNumberUniqueAsync(Guid companyId, string serialNumber, Guid? excludeAssetId = null);

    /// <summary>
    /// Checks if an asset is under warranty
    /// </summary>
    Task<bool> IsAssetUnderWarrantyAsync(Guid assetId);

    /// <summary>
    /// Checks if an asset is covered under a contract
    /// </summary>
    Task<bool> IsAssetUnderContractAsync(Guid assetId);

    #endregion
}
