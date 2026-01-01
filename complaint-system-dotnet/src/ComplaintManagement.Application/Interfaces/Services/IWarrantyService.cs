using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Contract;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for warranty management operations
/// </summary>
public interface IWarrantyService
{
    #region Warranty CRUD Operations

    /// <summary>
    /// Get all warranty definitions for a company with optional filtering
    /// </summary>
    Task<PagedResult<WarrantySummaryDto>> GetWarrantiesAsync(
        Guid companyId,
        WarrantySearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a warranty by ID
    /// </summary>
    Task<WarrantyDto?> GetWarrantyByIdAsync(
        Guid companyId,
        Guid warrantyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a warranty by code
    /// </summary>
    Task<WarrantyDto?> GetWarrantyByCodeAsync(
        Guid companyId,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new warranty definition
    /// </summary>
    Task<WarrantyDto> CreateWarrantyAsync(
        Guid companyId,
        CreateWarrantyRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing warranty
    /// </summary>
    Task<WarrantyDto> UpdateWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        UpdateWarrantyRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a warranty (soft delete)
    /// </summary>
    Task<bool> DeleteWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Warranty Lookup

    /// <summary>
    /// Get warranty lookup list for dropdowns
    /// </summary>
    Task<List<WarrantyLookupDto>> GetWarrantyLookupAsync(
        Guid companyId,
        Guid? productId = null,
        Guid? categoryId = null,
        WarrantyType? type = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the default warranty for a product
    /// </summary>
    Task<WarrantyDto?> GetDefaultWarrantyForProductAsync(
        Guid companyId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the default warranty for a category
    /// </summary>
    Task<WarrantyDto?> GetDefaultWarrantyForCategoryAsync(
        Guid companyId,
        Guid categoryId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Warranty Checks

    /// <summary>
    /// Check warranty status for a product serial number
    /// </summary>
    Task<WarrantyCheckResult> CheckWarrantyBySerialAsync(
        Guid companyId,
        string serialNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check warranty status for an asset
    /// </summary>
    Task<WarrantyCheckResult> CheckAssetWarrantyAsync(
        Guid companyId,
        Guid assetId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate warranty end date based on purchase date
    /// </summary>
    Task<DateTime?> CalculateWarrantyEndDateAsync(
        Guid companyId,
        Guid warrantyId,
        DateTime purchaseDate,
        CancellationToken cancellationToken = default);

    #endregion

    #region Status Operations

    /// <summary>
    /// Activate a warranty
    /// </summary>
    Task<WarrantyDto> ActivateWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivate a warranty
    /// </summary>
    Task<WarrantyDto> DeactivateWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Set a warranty as the default for its product/category
    /// </summary>
    Task<WarrantyDto> SetAsDefaultAsync(
        Guid companyId,
        Guid warrantyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion
}
