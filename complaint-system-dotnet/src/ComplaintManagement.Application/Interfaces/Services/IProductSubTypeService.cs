using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Product;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for Product Sub-Type management
/// </summary>
public interface IProductSubTypeService
{
    /// <summary>
    /// Get all sub-types with pagination and filtering
    /// </summary>
    Task<Result<PagedResult<ProductSubTypeSummaryDto>>> GetSubTypesAsync(
        Guid companyId,
        string? search = null,
        Guid? categoryId = null,
        bool? isActive = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a sub-type by ID
    /// </summary>
    Task<Result<ProductSubTypeDto>> GetSubTypeByIdAsync(Guid companyId, Guid subTypeId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a sub-type by code
    /// </summary>
    Task<Result<ProductSubTypeDto>> GetSubTypeByCodeAsync(Guid companyId, Guid categoryId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new sub-type
    /// </summary>
    Task<Result<ProductSubTypeDto>> CreateSubTypeAsync(Guid companyId, CreateProductSubTypeRequest request, Guid createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing sub-type
    /// </summary>
    Task<Result<ProductSubTypeDto>> UpdateSubTypeAsync(Guid companyId, Guid subTypeId, UpdateProductSubTypeRequest request, Guid updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a sub-type (soft delete)
    /// </summary>
    Task<Result<bool>> DeleteSubTypeAsync(Guid companyId, Guid subTypeId, Guid deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get sub-type lookup for dropdowns (optionally filtered by category)
    /// </summary>
    Task<Result<List<ProductSubTypeLookupDto>>> GetSubTypeLookupAsync(Guid companyId, Guid? categoryId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if sub-type code exists within a category
    /// </summary>
    Task<bool> SubTypeCodeExistsAsync(Guid companyId, Guid categoryId, string code, Guid? excludeSubTypeId = null, CancellationToken cancellationToken = default);
}
