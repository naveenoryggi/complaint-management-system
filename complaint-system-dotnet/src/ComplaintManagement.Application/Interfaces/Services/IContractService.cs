using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Contract;
using ComplaintManagement.Domain.Enums.Service;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service interface for contract management operations
/// </summary>
public interface IContractService
{
    #region Contract CRUD Operations

    /// <summary>
    /// Get all contracts for a company with optional filtering
    /// </summary>
    Task<PagedResult<ContractSummaryDto>> GetContractsAsync(
        Guid companyId,
        ContractSearchRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a contract by ID
    /// </summary>
    Task<ContractDto?> GetContractByIdAsync(
        Guid companyId,
        Guid contractId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get a contract by contract number
    /// </summary>
    Task<ContractDto?> GetContractByNumberAsync(
        Guid companyId,
        string contractNumber,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Create a new contract
    /// </summary>
    Task<ContractDto> CreateContractAsync(
        Guid companyId,
        CreateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update an existing contract
    /// </summary>
    Task<ContractDto> UpdateContractAsync(
        Guid companyId,
        Guid contractId,
        UpdateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete a contract (soft delete)
    /// </summary>
    Task<bool> DeleteContractAsync(
        Guid companyId,
        Guid contractId,
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Contract Status Operations

    /// <summary>
    /// Activate a draft or pending contract
    /// </summary>
    Task<ContractDto> ActivateContractAsync(
        Guid companyId,
        Guid contractId,
        ActivateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renew an expiring or expired contract
    /// </summary>
    Task<ContractDto> RenewContractAsync(
        Guid companyId,
        Guid contractId,
        RenewContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate an active contract
    /// </summary>
    Task<ContractDto> TerminateContractAsync(
        Guid companyId,
        Guid contractId,
        TerminateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Suspend an active contract
    /// </summary>
    Task<ContractDto> SuspendContractAsync(
        Guid companyId,
        Guid contractId,
        string? reason,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reactivate a suspended contract
    /// </summary>
    Task<ContractDto> ReactivateContractAsync(
        Guid companyId,
        Guid contractId,
        string? notes,
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Contract Items

    /// <summary>
    /// Get items for a contract
    /// </summary>
    Task<List<ContractItemDto>> GetContractItemsAsync(
        Guid companyId,
        Guid contractId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Add an item to a contract
    /// </summary>
    Task<ContractItemDto> AddContractItemAsync(
        Guid companyId,
        Guid contractId,
        CreateContractItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update a contract item
    /// </summary>
    Task<ContractItemDto> UpdateContractItemAsync(
        Guid companyId,
        Guid contractId,
        Guid itemId,
        UpdateContractItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Remove an item from a contract
    /// </summary>
    Task<bool> RemoveContractItemAsync(
        Guid companyId,
        Guid contractId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Coverage Checks

    /// <summary>
    /// Check if a customer has active coverage
    /// </summary>
    Task<CoverageCheckResult> CheckCustomerCoverageAsync(
        Guid companyId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a specific product is covered
    /// </summary>
    Task<CoverageCheckResult> CheckProductCoverageAsync(
        Guid companyId,
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if a specific asset is covered
    /// </summary>
    Task<CoverageCheckResult> CheckAssetCoverageAsync(
        Guid companyId,
        Guid assetId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Renewal & Expiration

    /// <summary>
    /// Get contracts expiring within specified days
    /// </summary>
    Task<ExpiringContractsReportDto> GetExpiringContractsAsync(
        Guid companyId,
        int daysAhead = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get contracts by customer
    /// </summary>
    Task<List<ContractSummaryDto>> GetContractsByCustomerAsync(
        Guid companyId,
        Guid customerId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get renewal history for a contract
    /// </summary>
    Task<List<ContractRenewalSummaryDto>> GetRenewalHistoryAsync(
        Guid companyId,
        Guid contractId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Process auto-renewals for contracts
    /// </summary>
    Task<int> ProcessAutoRenewalsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Statistics & Reporting

    /// <summary>
    /// Get contract statistics for a company
    /// </summary>
    Task<ContractStatisticsDto> GetContractStatisticsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get contract lookup list for dropdowns
    /// </summary>
    Task<List<ContractLookupDto>> GetContractLookupAsync(
        Guid companyId,
        Guid? customerId = null,
        ContractStatus? status = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Usage Tracking

    /// <summary>
    /// Record usage of support hours
    /// </summary>
    Task<ContractDto> RecordUsageHoursAsync(
        Guid companyId,
        Guid contractId,
        int hours,
        string? notes,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Record an incident against a contract
    /// </summary>
    Task<ContractDto> RecordIncidentAsync(
        Guid companyId,
        Guid contractId,
        string? notes,
        Guid userId,
        CancellationToken cancellationToken = default);

    #endregion

    #region Warranty Operations

    /// <summary>
    /// Get all warranties for a company with optional filtering
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
    /// Create a new warranty
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
    /// Delete a warranty
    /// </summary>
    Task<bool> DeleteWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get warranty lookup for dropdowns
    /// </summary>
    Task<List<WarrantyLookupDto>> GetWarrantyLookupAsync(
        Guid companyId,
        Guid? productId = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check warranty coverage for a product
    /// </summary>
    Task<WarrantyCheckResult> CheckWarrantyCoverageAsync(
        Guid companyId,
        Guid productId,
        DateTime purchaseDate,
        CancellationToken cancellationToken = default);

    #endregion
}
