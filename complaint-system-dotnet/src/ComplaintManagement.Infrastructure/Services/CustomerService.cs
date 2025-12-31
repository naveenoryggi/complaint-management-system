using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.CRM;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.CRM;
using ComplaintManagement.Domain.Enums.CRM;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Service for managing customers and their locations/contacts
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(ComplaintDbContext context, ILogger<CustomerService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Customer CRUD

    public async Task<Result<PagedResult<CustomerSummaryDto>>> GetCustomersAsync(
        Guid companyId,
        string? searchTerm = null,
        string? status = null,
        string? type = null,
        string? segment = null,
        Guid? partnerId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Customers
                .Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Code.ToLower().Contains(search) ||
                    c.Name.ToLower().Contains(search) ||
                    c.PrimaryEmail.ToLower().Contains(search) ||
                    (c.PrimaryPhone != null && c.PrimaryPhone.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<CustomerStatus>(status, out var statusEnum))
            {
                query = query.Where(c => c.Status == statusEnum);
            }

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<CustomerType>(type, out var typeEnum))
            {
                query = query.Where(c => c.Type == typeEnum);
            }

            if (!string.IsNullOrWhiteSpace(segment) && Enum.TryParse<CustomerSegment>(segment, out var segmentEnum))
            {
                query = query.Where(c => c.Segment == segmentEnum);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CustomerSummaryDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Type = c.Type,
                    Status = c.Status,
                    PrimaryEmail = c.PrimaryEmail,
                    BillingCity = c.BillingCity,
                    BillingCountry = c.BillingCountry,
                    Segment = c.Segment,
                    CustomerSince = c.CustomerSince,
                    LocationCount = c.Locations.Count(l => !l.IsDeleted),
                    ActiveTicketCount = 0 // Will be calculated separately if needed
                })
                .ToListAsync(cancellationToken);

            var pagedResult = new PagedResult<CustomerSummaryDto>(items, totalCount, page, pageSize);
            return Result<PagedResult<CustomerSummaryDto>>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customers for company {CompanyId}", companyId);
            return Result<PagedResult<CustomerSummaryDto>>.Failure("Error retrieving customers", ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> GetCustomerByIdAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _context.Customers
                .Include(c => c.AccountManager)
                .Include(c => c.SecondaryAccountManager)
                .Where(c => c.Id == customerId && c.CompanyId == companyId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
                return Result<CustomerDto>.Failure("Customer not found", "NOT_FOUND");

            return Result<CustomerDto>.Success(MapToDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer {CustomerId}", customerId);
            return Result<CustomerDto>.Failure("Error retrieving customer", ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> GetCustomerByCodeAsync(Guid companyId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _context.Customers
                .Include(c => c.AccountManager)
                .Include(c => c.SecondaryAccountManager)
                .Where(c => c.Code == code && c.CompanyId == companyId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
                return Result<CustomerDto>.Failure("Customer not found", "NOT_FOUND");

            return Result<CustomerDto>.Success(MapToDto(customer));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer by code {Code}", code);
            return Result<CustomerDto>.Failure("Error retrieving customer", ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(Guid companyId, CreateCustomerRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(request.Code))
                return Result<CustomerDto>.Failure("Customer code is required", "VALIDATION_ERROR");

            if (string.IsNullOrWhiteSpace(request.Name))
                return Result<CustomerDto>.Failure("Customer name is required", "VALIDATION_ERROR");

            // Check for duplicate code
            var existingCustomer = await _context.Customers
                .Where(c => c.Code == request.Code && c.CompanyId == companyId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingCustomer != null)
                return Result<CustomerDto>.Failure($"Customer with code '{request.Code}' already exists", "DUPLICATE_CODE");

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                CompanyId = companyId,
                Code = request.Code,
                Name = request.Name,
                LegalName = request.LegalName,
                Type = request.Type,
                Status = CustomerStatus.Active,
                PrimaryEmail = request.PrimaryEmail,
                PrimaryPhone = request.PrimaryPhone,
                Website = request.Website,
                BillingAddressLine1 = request.BillingAddressLine1,
                BillingAddressLine2 = request.BillingAddressLine2,
                BillingCity = request.BillingCity,
                BillingState = request.BillingState,
                BillingCountry = request.BillingCountry ?? "India",
                BillingPostalCode = request.BillingPostalCode,
                TaxId = request.TaxId,
                PanNumber = request.PanNumber,
                Industry = request.Industry,
                Segment = request.Segment,
                EmployeeCount = request.EmployeeCount,
                AnnualRevenue = request.AnnualRevenue,
                Currency = request.Currency,
                CustomerSince = request.CustomerSince ?? DateTime.UtcNow,
                CreditTerms = request.CreditTerms,
                CreditLimit = request.CreditLimit,
                AccountManagerId = request.AccountManagerId,
                SecondaryAccountManagerId = request.SecondaryAccountManagerId,
                PortalEnabled = request.PortalEnabled,
                PreferredLanguage = request.PreferredLanguage,
                PreferredTimeZone = request.PreferredTimeZone,
                ExternalCustomerId = request.ExternalCustomerId,
                AuthenticationProviderId = request.AuthenticationProviderId,
                Notes = request.Notes,
                Tags = request.Tags,
                CustomFields = request.CustomFields,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = Guid.TryParse(createdBy, out var createdByGuid) ? createdByGuid : null
            };

            await _context.Customers.AddAsync(customer, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {Code} created by {User}", customer.Code, createdBy);
            return Result<CustomerDto>.Success(MapToDto(customer), "Customer created successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer");
            return Result<CustomerDto>.Failure("Error creating customer", ex.Message);
        }
    }

    public async Task<Result<CustomerDto>> UpdateCustomerAsync(Guid companyId, Guid customerId, UpdateCustomerRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _context.Customers
                .Where(c => c.Id == customerId && c.CompanyId == companyId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
                return Result<CustomerDto>.Failure("Customer not found", "NOT_FOUND");

            customer.Name = request.Name;
            customer.LegalName = request.LegalName;
            customer.Type = request.Type;
            customer.Status = request.Status;
            customer.StatusReason = request.StatusReason;
            customer.PrimaryEmail = request.PrimaryEmail;
            customer.PrimaryPhone = request.PrimaryPhone;
            customer.Website = request.Website;
            customer.BillingAddressLine1 = request.BillingAddressLine1;
            customer.BillingAddressLine2 = request.BillingAddressLine2;
            customer.BillingCity = request.BillingCity;
            customer.BillingState = request.BillingState;
            customer.BillingCountry = request.BillingCountry;
            customer.BillingPostalCode = request.BillingPostalCode;
            customer.TaxId = request.TaxId;
            customer.PanNumber = request.PanNumber;
            customer.Industry = request.Industry;
            customer.Segment = request.Segment;
            customer.EmployeeCount = request.EmployeeCount;
            customer.AnnualRevenue = request.AnnualRevenue;
            customer.Currency = request.Currency;
            customer.CustomerSince = request.CustomerSince;
            customer.CreditTerms = request.CreditTerms;
            customer.CreditLimit = request.CreditLimit;
            customer.CustomerRating = request.CustomerRating;
            customer.AccountManagerId = request.AccountManagerId;
            customer.SecondaryAccountManagerId = request.SecondaryAccountManagerId;
            customer.PortalEnabled = request.PortalEnabled;
            customer.PreferredLanguage = request.PreferredLanguage;
            customer.PreferredTimeZone = request.PreferredTimeZone;
            customer.ExternalCustomerId = request.ExternalCustomerId;
            customer.AuthenticationProviderId = request.AuthenticationProviderId;
            customer.Notes = request.Notes;
            customer.Tags = request.Tags;
            customer.CustomFields = request.CustomFields;
            customer.UpdatedAt = DateTime.UtcNow;
            customer.UpdatedBy = Guid.TryParse(updatedBy, out var updatedByGuid) ? updatedByGuid : null;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} updated by {User}", customerId, updatedBy);
            return Result<CustomerDto>.Success(MapToDto(customer), "Customer updated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating customer {CustomerId}", customerId);
            return Result<CustomerDto>.Failure("Error updating customer", ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteCustomerAsync(Guid companyId, Guid customerId, string deletedBy, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await _context.Customers
                .Where(c => c.Id == customerId && c.CompanyId == companyId && !c.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);

            if (customer == null)
                return Result<bool>.Failure("Customer not found", "NOT_FOUND");

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.DeletedBy = Guid.TryParse(deletedBy, out var deletedByGuid) ? deletedByGuid : null;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer {CustomerId} deleted by {User}", customerId, deletedBy);
            return Result<bool>.Success(true, "Customer deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting customer {CustomerId}", customerId);
            return Result<bool>.Failure("Error deleting customer", ex.Message);
        }
    }

    public async Task<Result<List<CustomerLookupDto>>> GetCustomerLookupAsync(Guid companyId, string? searchTerm = null, Guid? partnerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _context.Customers
                .Where(c => c.CompanyId == companyId && !c.IsDeleted && c.Status == CustomerStatus.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var search = searchTerm.ToLower();
                query = query.Where(c =>
                    c.Code.ToLower().Contains(search) ||
                    c.Name.ToLower().Contains(search));
            }

            var items = await query
                .Take(50)
                .Select(c => new CustomerLookupDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Name = c.Name,
                    Type = c.Type,
                    Status = c.Status
                })
                .ToListAsync(cancellationToken);

            return Result<List<CustomerLookupDto>>.Success(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer lookup");
            return Result<List<CustomerLookupDto>>.Failure("Error retrieving customer lookup", ex.Message);
        }
    }

    #endregion

    #region Customer Locations - Stub implementations

    public Task<Result<List<CustomerLocationSummaryDto>>> GetCustomerLocationsAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<List<CustomerLocationSummaryDto>>.Success(new List<CustomerLocationSummaryDto>()));
    }

    public Task<Result<CustomerLocationDto>> GetCustomerLocationByIdAsync(Guid companyId, Guid locationId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerLocationDto>.Failure("Location not found", "NOT_FOUND"));
    }

    public Task<Result<CustomerLocationDto>> CreateCustomerLocationAsync(Guid companyId, CreateCustomerLocationRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerLocationDto>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<CustomerLocationDto>> UpdateCustomerLocationAsync(Guid companyId, Guid locationId, UpdateCustomerLocationRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerLocationDto>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<bool>> DeleteCustomerLocationAsync(Guid companyId, Guid locationId, string deletedBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<List<CustomerLocationLookupDto>>> GetLocationLookupAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<List<CustomerLocationLookupDto>>.Success(new List<CustomerLocationLookupDto>()));
    }

    #endregion

    #region Customer Contacts - Stub implementations

    public Task<Result<List<CustomerContactSummaryDto>>> GetCustomerContactsAsync(Guid companyId, Guid customerId, Guid? locationId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<List<CustomerContactSummaryDto>>.Success(new List<CustomerContactSummaryDto>()));
    }

    public Task<Result<CustomerContactDto>> GetCustomerContactByIdAsync(Guid companyId, Guid contactId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerContactDto>.Failure("Contact not found", "NOT_FOUND"));
    }

    public Task<Result<CustomerContactDto>> CreateCustomerContactAsync(Guid companyId, CreateCustomerContactRequest request, string createdBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerContactDto>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<CustomerContactDto>> UpdateCustomerContactAsync(Guid companyId, Guid contactId, UpdateCustomerContactRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerContactDto>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<bool>> DeleteCustomerContactAsync(Guid companyId, Guid contactId, string deletedBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<bool>> SetCustomerContactPasswordAsync(Guid companyId, Guid contactId, SetContactPasswordRequest request, string updatedBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<bool>> UnlockCustomerContactAsync(Guid companyId, Guid contactId, string updatedBy, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<bool>.Failure("Not implemented", "NOT_IMPLEMENTED"));
    }

    public Task<Result<List<ContactLookupDto>>> GetContactLookupAsync(Guid companyId, Guid customerId, Guid? locationId = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<List<ContactLookupDto>>.Success(new List<ContactLookupDto>()));
    }

    #endregion

    #region Partner Relationships - Stub implementation

    public Task<Result<List<PartnerCustomerDto>>> GetCustomerPartnersAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<List<PartnerCustomerDto>>.Success(new List<PartnerCustomerDto>()));
    }

    #endregion

    #region Statistics - Stub implementation

    public Task<Result<CustomerStatisticsDto>> GetCustomerStatisticsAsync(Guid companyId, Guid customerId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Result<CustomerStatisticsDto>.Success(new CustomerStatisticsDto()));
    }

    #endregion

    #region Private Methods

    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            CompanyId = customer.CompanyId,
            Code = customer.Code,
            Name = customer.Name,
            LegalName = customer.LegalName,
            Type = customer.Type,
            Status = customer.Status,
            PrimaryEmail = customer.PrimaryEmail,
            PrimaryPhone = customer.PrimaryPhone,
            Website = customer.Website,
            BillingAddressLine1 = customer.BillingAddressLine1,
            BillingAddressLine2 = customer.BillingAddressLine2,
            BillingCity = customer.BillingCity,
            BillingState = customer.BillingState,
            BillingCountry = customer.BillingCountry,
            BillingPostalCode = customer.BillingPostalCode,
            TaxId = customer.TaxId,
            PanNumber = customer.PanNumber,
            Industry = customer.Industry,
            Segment = customer.Segment,
            EmployeeCount = customer.EmployeeCount,
            AnnualRevenue = customer.AnnualRevenue,
            Currency = customer.Currency,
            CustomerSince = customer.CustomerSince,
            CreditTerms = customer.CreditTerms,
            CreditLimit = customer.CreditLimit,
            CustomerRating = customer.CustomerRating ?? 0,
            StatusReason = customer.StatusReason,
            AccountManagerId = customer.AccountManagerId,
            AccountManagerName = customer.AccountManager?.FullName,
            SecondaryAccountManagerId = customer.SecondaryAccountManagerId,
            SecondaryAccountManagerName = customer.SecondaryAccountManager?.FullName,
            PortalEnabled = customer.PortalEnabled,
            PreferredLanguage = customer.PreferredLanguage,
            PreferredTimeZone = customer.PreferredTimeZone,
            ExternalCustomerId = customer.ExternalCustomerId,
            AuthenticationProviderId = customer.AuthenticationProviderId,
            Notes = customer.Notes,
            Tags = customer.Tags,
            CustomFields = customer.CustomFields,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
    }

    #endregion
}
