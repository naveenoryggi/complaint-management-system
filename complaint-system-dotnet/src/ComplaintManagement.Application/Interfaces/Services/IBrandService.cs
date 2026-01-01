using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Product;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for Brand management
/// </summary>
public interface IBrandService
{
    /// <summary>
    /// Get all brands with pagination and filtering
    /// </summary>
    Task<Result<PagedResult<BrandSummaryDto>>> GetBrandsAsync(
        Guid companyId,
        string? search = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a brand by ID
    /// </summary>
    Task<Result<BrandDto>> GetBrandByIdAsync(Guid companyId, Guid brandId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a brand by code
    /// </summary>
    Task<Result<BrandDto>> GetBrandByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new brand
    /// </summary>
    Task<Result<BrandDto>> CreateBrandAsync(Guid companyId, CreateBrandRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing brand
    /// </summary>
    Task<Result<BrandDto>> UpdateBrandAsync(Guid companyId, Guid brandId, UpdateBrandRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a brand (soft delete)
    /// </summary>
    Task<Result<bool>> DeleteBrandAsync(Guid companyId, Guid brandId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get brand lookup for dropdowns
    /// </summary>
    Task<Result<List<BrandLookupDto>>> GetBrandLookupAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if brand code exists
    /// </summary>
    Task<bool> BrandCodeExistsAsync(Guid companyId, string code, Guid? excludeBrandId = null, CancellationToken cancellationToken = default);
}
