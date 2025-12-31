using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;

namespace ComplaintManagement.Application.Interfaces.Services;

/// <summary>
/// Service for managing customers and their locations/contacts
/// </summary>
public interface ICustomerService
{
    #region Customer CRUD

    /// <summary>
    /// Gets all customers for a company with optional filtering
    /// </summary>
    Task<Result<PagedResult<CustomerSummaryDto>>> GetCustomersAsync(
        Guid companyId,
        string? searchTerm = null,
        string? status = null,
        string? type = null,
        string? segment = null,
        Guid? partnerId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by ID
    /// </summary>
    Task<Result<CustomerDto>> GetCustomerByIdAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer by code
    /// </summary>
    Task<Result<CustomerDto>> GetCustomerByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new customer
    /// </summary>
    Task<Result<CustomerDto>> CreateCustomerAsync(Guid companyId, CreateCustomerRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer
    /// </summary>
    Task<Result<CustomerDto>> UpdateCustomerAsync(Guid companyId, Guid customerId, UpdateCustomerRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a customer
    /// </summary>
    Task<Result<bool>> DeleteCustomerAsync(Guid companyId, Guid customerId, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets customer lookup list for dropdowns
    /// </summary>
    Task<Result<List<CustomerLookupDto>>> GetCustomerLookupAsync(Guid companyId, string? searchTerm = null, Guid? partnerId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Customer Locations

    /// <summary>
    /// Gets all locations for a customer
    /// </summary>
    Task<Result<List<CustomerLocationSummaryDto>>> GetCustomerLocationsAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer location by ID
    /// </summary>
    Task<Result<CustomerLocationDto>> GetCustomerLocationByIdAsync(Guid companyId, Guid locationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new customer location
    /// </summary>
    Task<Result<CustomerLocationDto>> CreateCustomerLocationAsync(Guid companyId, CreateCustomerLocationRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer location
    /// </summary>
    Task<Result<CustomerLocationDto>> UpdateCustomerLocationAsync(Guid companyId, Guid locationId, UpdateCustomerLocationRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a customer location
    /// </summary>
    Task<Result<bool>> DeleteCustomerLocationAsync(Guid companyId, Guid locationId, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets location lookup list for dropdowns
    /// </summary>
    Task<Result<List<CustomerLocationLookupDto>>> GetLocationLookupAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    #endregion

    #region Customer Contacts

    /// <summary>
    /// Gets all contacts for a customer
    /// </summary>
    Task<Result<List<CustomerContactSummaryDto>>> GetCustomerContactsAsync(Guid companyId, Guid customerId, Guid? locationId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a customer contact by ID
    /// </summary>
    Task<Result<CustomerContactDto>> GetCustomerContactByIdAsync(Guid companyId, Guid contactId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new customer contact
    /// </summary>
    Task<Result<CustomerContactDto>> CreateCustomerContactAsync(Guid companyId, CreateCustomerContactRequest request, string createdBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing customer contact
    /// </summary>
    Task<Result<CustomerContactDto>> UpdateCustomerContactAsync(Guid companyId, Guid contactId, UpdateCustomerContactRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes (soft delete) a customer contact
    /// </summary>
    Task<Result<bool>> DeleteCustomerContactAsync(Guid companyId, Guid contactId, string deletedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets or resets a customer contact's password
    /// </summary>
    Task<Result<bool>> SetCustomerContactPasswordAsync(Guid companyId, Guid contactId, SetContactPasswordRequest request, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks a locked customer contact account
    /// </summary>
    Task<Result<bool>> UnlockCustomerContactAsync(Guid companyId, Guid contactId, string updatedBy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets contact lookup list for dropdowns
    /// </summary>
    Task<Result<List<ContactLookupDto>>> GetContactLookupAsync(Guid companyId, Guid customerId, Guid? locationId = null, CancellationToken cancellationToken = default);

    #endregion

    #region Partner Relationships

    /// <summary>
    /// Gets all partners assigned to a customer
    /// </summary>
    Task<Result<List<PartnerCustomerDto>>> GetCustomerPartnersAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    #endregion

    #region Statistics

    /// <summary>
    /// Gets customer statistics
    /// </summary>
    Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default);

    #endregion
}

/// <summary>
/// Customer statistics DTO
/// </summary>
public class CustomerStatisticsDto
{
    public int TotalLocations { get; set; }
    public int ActiveLocations { get; set; }
    public int TotalContacts { get; set; }
    public int PortalUsers { get; set; }
    public int TotalAssets { get; set; }
    public int ActiveContracts { get; set; }
    public int OpenTickets { get; set; }
    public int ClosedTicketsThisMonth { get; set; }
    public decimal AverageResolutionTimeHours { get; set; }
    public int TotalTicketsAllTime { get; set; }
}
