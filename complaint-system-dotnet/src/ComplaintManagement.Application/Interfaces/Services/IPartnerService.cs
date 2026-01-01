using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for managing partners (resellers, system integrators, distributors)
/// </summary>
public interface IPartnerService
{
    #region Partner CRUD

    /// <summary>
    /// Gets all partners for a company with optional filtering
    /// </summary>
    Task<Result<PagedResult<PartnerSummaryDto>>> GetPartnersAsync(
        Guid companyId,
        string? searchTerm = null,
        string? status = null,
        string? type = null,
        string? tier = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a partner by ID
    /// </summary>
    Task<Result<PartnerDto>> GetPartnerByIdAsync(Guid companyId, Guid partnerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a partner by code
    /// </summary>
    Task<Result<PartnerDto>> GetPartnerByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new partner
    /// </summary>
    Task<Result<PartnerDto>> CreatePartnerAsync(Guid companyId, CreatePartnerRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing partner
    /// </summary>
    Task<Result<PartnerDto>> UpdatePartnerAsync(Guid companyId, Guid partnerId, UpdatePartnerRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a partner
    /// </summary>
    Task<Result<bool>> DeletePartnerAsync(Guid companyId, Guid partnerId, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets partner lookup list for dropdowns
    /// </summary>
    Task<Result<List<PartnerLookupDto>>> GetPartnerLookupAsync(Guid companyId, string? searchTerm = null, CancellationToken cancellationToken = default);

    #endregion

    #region Partner Contacts

    /// <summary>
    /// Gets all contacts for a partner
    /// </summary>
    Task<Result<List<PartnerContactSummaryDto>>> GetPartnerContactsAsync(Guid companyId, Guid partnerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a partner contact by ID
    /// </summary>
    Task<Result<PartnerContactDto>> GetPartnerContactByIdAsync(Guid companyId, Guid contactId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new partner contact
    /// </summary>
    Task<Result<PartnerContactDto>> CreatePartnerContactAsync(Guid companyId, CreatePartnerContactRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing partner contact
    /// </summary>
    Task<Result<PartnerContactDto>> UpdatePartnerContactAsync(Guid companyId, Guid contactId, UpdatePartnerContactRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a partner contact
    /// </summary>
    Task<Result<bool>> DeletePartnerContactAsync(Guid companyId, Guid contactId, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or resets a partner contact's password
    /// </summary>
    Task<Result<bool>> SetPartnerContactPasswordAsync(Guid companyId, Guid contactId, SetContactPasswordRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks a locked partner contact account
    /// </summary>
    Task<Result<bool>> UnlockPartnerContactAsync(Guid companyId, Guid contactId, string updatedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Partner-Customer Relationships

    /// <summary>
    /// Gets all customers assigned to a partner
    /// </summary>
    Task<Result<List<PartnerCustomerDto>>> GetPartnerCustomersAsync(Guid companyId, Guid partnerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a customer to a partner
    /// </summary>
    Task<Result<PartnerCustomerDto>> AssignCustomerToPartnerAsync(Guid companyId, AssignPartnerToCustomerRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a partner-customer relationship
    /// </summary>
    Task<Result<PartnerCustomerDto>> UpdatePartnerCustomerAsync(Guid companyId, Guid partnerCustomerId, UpdatePartnerCustomerRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a customer from a partner
    /// </summary>
    Task<Result<bool>> RemoveCustomerFromPartnerAsync(Guid companyId, Guid partnerCustomerId, string deletedBy, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets partner statistics
    /// </summary>
    Task<Result<PartnerStatisticsDto>> GetPartnerStatisticsAsync(Guid companyId, Guid partnerId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Partner statistics DTO
/// </summary>
public class PartnerStatisticsDto
{
    public int TotalCustomers { get; set; }
    public int ActiveCustomers { get; set; }
    public int TotalContacts { get; set; }
    public int PortalUsers { get; set; }
    public int OpenTickets { get; set; }
    public int ClosedTicketsThisMonth { get; set; }
    public decimal AverageResolutionTimeHours { get; set; }
}
