using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Contract;
using ComplaintManagement.Application.Interfaces.Services;
using ComplaintManagement.Domain.Entities.Service;
using ComplaintManagement.Domain.Enums.Service;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Infrastructure.Services;

/// <summary>
/// Implementation of contract management service
/// </summary>
public class ContractService : IContractService
{
    private readonly ComplaintDbContext _context;
    private readonly ILogger<ContractService> _logger;

    public ContractService(ComplaintDbContext context, ILogger<ContractService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Contract CRUD Operations

    public async Task<PagedResult<ContractSummaryDto>> GetContractsAsync(
        Guid companyId,
        ContractSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Partner)
            .Include(c => c.Items)
            .Where(c => c.CompanyId == companyId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(c =>
                c.ContractNumber.ToLower().Contains(searchTerm) ||
                c.Name.ToLower().Contains(searchTerm) ||
                c.Customer.Name.ToLower().Contains(searchTerm));
        }

        if (request.CustomerId.HasValue)
            query = query.Where(c => c.CustomerId == request.CustomerId.Value);

        if (request.PartnerId.HasValue)
            query = query.Where(c => c.PartnerId == request.PartnerId.Value);

        if (request.Type.HasValue)
            query = query.Where(c => c.Type == request.Type.Value);

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.CoverageType.HasValue)
            query = query.Where(c => c.CoverageType == request.CoverageType.Value);

        if (request.StartDateFrom.HasValue)
            query = query.Where(c => c.StartDate >= request.StartDateFrom.Value);

        if (request.StartDateTo.HasValue)
            query = query.Where(c => c.StartDate <= request.StartDateTo.Value);

        if (request.EndDateFrom.HasValue)
            query = query.Where(c => c.EndDate >= request.EndDateFrom.Value);

        if (request.EndDateTo.HasValue)
            query = query.Where(c => c.EndDate <= request.EndDateTo.Value);

        if (request.ExpiringWithinDays == true && request.ExpiringDays.HasValue)
        {
            var expiryDate = DateTime.UtcNow.AddDays(request.ExpiringDays.Value);
            query = query.Where(c => c.EndDate <= expiryDate && c.Status == ContractStatus.Active);
        }

        if (request.AutoRenew.HasValue)
            query = query.Where(c => c.AutoRenew == request.AutoRenew.Value);

        // Get total count
        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "contractnumber" => request.SortDescending ? query.OrderByDescending(c => c.ContractNumber) : query.OrderBy(c => c.ContractNumber),
            "name" => request.SortDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name),
            "startdate" => request.SortDescending ? query.OrderByDescending(c => c.StartDate) : query.OrderBy(c => c.StartDate),
            "enddate" => request.SortDescending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate),
            "status" => request.SortDescending ? query.OrderByDescending(c => c.Status) : query.OrderBy(c => c.Status),
            "value" => request.SortDescending ? query.OrderByDescending(c => c.ContractValue) : query.OrderBy(c => c.ContractValue),
            _ => request.SortDescending ? query.OrderByDescending(c => c.EndDate) : query.OrderBy(c => c.EndDate)
        };

        // Apply pagination
        var contracts = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = contracts.Select(MapToSummaryDto).ToList();

        return new PagedResult<ContractSummaryDto>(items, request.PageNumber, request.PageSize, totalCount);
    }

    public async Task<ContractDto?> GetContractByIdAsync(
        Guid companyId,
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Partner)
            .Include(c => c.SLASettings)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken);

        return contract == null ? null : MapToDto(contract);
    }

    public async Task<ContractDto?> GetContractByNumberAsync(
        Guid companyId,
        string contractNumber,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Partner)
            .Include(c => c.SLASettings)
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber && c.CompanyId == companyId, cancellationToken);

        return contract == null ? null : MapToDto(contract);
    }

    public async Task<ContractDto> CreateContractAsync(
        Guid companyId,
        CreateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Generate contract number
        var contractNumber = await GenerateContractNumberAsync(companyId, cancellationToken);

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            CustomerId = request.CustomerId,
            PartnerId = request.PartnerId,
            ContractNumber = contractNumber,
            Name = request.Name,
            Type = request.Type,
            Description = request.Description,
            ExternalContractId = request.ExternalContractId,
            PONumber = request.PONumber,
            InvoiceNumber = request.InvoiceNumber,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            SignedDate = request.SignedDate,
            AutoRenew = request.AutoRenew,
            RenewalPeriodMonths = request.RenewalPeriodMonths,
            RenewalNoticeDays = request.RenewalNoticeDays,
            ContractValue = request.ContractValue,
            AnnualValue = request.AnnualValue,
            MonthlyValue = request.MonthlyValue,
            Currency = request.Currency,
            BillingFrequency = request.BillingFrequency,
            PaymentTerms = request.PaymentTerms,
            TaxRate = request.TaxRate,
            CoverageType = request.CoverageType,
            IncludesOnsite = request.IncludesOnsite,
            IncludesRemote = request.IncludesRemote,
            IncludesParts = request.IncludesParts,
            IncludesLabor = request.IncludesLabor,
            IncludesTravel = request.IncludesTravel,
            IncludesConsumables = request.IncludesConsumables,
            IncludedHours = request.IncludedHours,
            IncludedIncidents = request.IncludedIncidents,
            IncludedPMVisits = request.IncludedPMVisits,
            ExcludedItems = request.ExcludedItems,
            IncludedItems = request.IncludedItems,
            SLASettingsId = request.SLASettingsId,
            ResponseTimeHours = request.ResponseTimeHours,
            ResolutionTimeHours = request.ResolutionTimeHours,
            SupportHoursType = request.SupportHoursType,
            UptimeGuarantee = request.UptimeGuarantee,
            DocumentUrl = request.DocumentUrl,
            Terms = request.Terms,
            SpecialConditions = request.SpecialConditions,
            Notes = request.Notes,
            AccountManagerId = request.AccountManagerId,
            TechnicalManagerId = request.TechnicalManagerId,
            CustomFields = request.CustomFields,
            Tags = request.Tags,
            Status = ContractStatus.Draft,
            CreatedBy = userId
        };

        _context.Contracts.Add(contract);

        // Add contract items if provided
        if (request.Items?.Any() == true)
        {
            foreach (var itemRequest in request.Items)
            {
                var item = new ContractItem
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    ProductId = itemRequest.ProductId,
                    AssetId = itemRequest.AssetId,
                    ItemDescription = itemRequest.ItemDescription,
                    SerialNumbers = itemRequest.SerialNumbers,
                    Quantity = itemRequest.Quantity,
                    UnitOfMeasure = itemRequest.UnitOfMeasure,
                    CoverageStartDate = itemRequest.CoverageStartDate ?? request.StartDate,
                    CoverageEndDate = itemRequest.CoverageEndDate ?? request.EndDate,
                    ItemValue = itemRequest.ItemValue,
                    DiscountPercent = itemRequest.DiscountPercent,
                    Currency = itemRequest.Currency,
                    ResponseTimeHours = itemRequest.ResponseTimeHours,
                    ResolutionTimeHours = itemRequest.ResolutionTimeHours,
                    PriorityLevel = itemRequest.PriorityLevel,
                    LocationId = itemRequest.LocationId,
                    SiteInfo = itemRequest.SiteInfo,
                    MaintenanceIntervalDays = itemRequest.MaintenanceIntervalDays,
                    SpecialConditions = itemRequest.SpecialConditions,
                    Notes = itemRequest.Notes,
                    CustomFields = itemRequest.CustomFields,
                    Status = ContractItemStatus.Covered,
                    IsActive = true,
                    CreatedBy = userId
                };
                _context.ContractItems.Add(item);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractNumber} created for company {CompanyId}", contractNumber, companyId);

        return await GetContractByIdAsync(companyId, contract.Id, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve created contract");
    }

    public async Task<ContractDto> UpdateContractAsync(
        Guid companyId,
        Guid contractId,
        UpdateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        // Only allow updates to draft or pending contracts for most fields
        if (contract.Status == ContractStatus.Active)
        {
            // Limited updates for active contracts
            contract.Notes = request.Notes;
            contract.SpecialConditions = request.SpecialConditions;
            contract.AccountManagerId = request.AccountManagerId;
            contract.TechnicalManagerId = request.TechnicalManagerId;
            contract.CustomFields = request.CustomFields;
            contract.Tags = request.Tags;
        }
        else
        {
            contract.Name = request.Name;
            contract.Type = request.Type;
            contract.Description = request.Description;
            contract.ExternalContractId = request.ExternalContractId;
            contract.PONumber = request.PONumber;
            contract.InvoiceNumber = request.InvoiceNumber;
            contract.StartDate = request.StartDate;
            contract.EndDate = request.EndDate;
            contract.SignedDate = request.SignedDate;
            contract.AutoRenew = request.AutoRenew;
            contract.RenewalPeriodMonths = request.RenewalPeriodMonths;
            contract.RenewalNoticeDays = request.RenewalNoticeDays;
            contract.ContractValue = request.ContractValue;
            contract.AnnualValue = request.AnnualValue;
            contract.MonthlyValue = request.MonthlyValue;
            contract.Currency = request.Currency;
            contract.BillingFrequency = request.BillingFrequency;
            contract.PaymentTerms = request.PaymentTerms;
            contract.TaxRate = request.TaxRate;
            contract.CoverageType = request.CoverageType;
            contract.IncludesOnsite = request.IncludesOnsite;
            contract.IncludesRemote = request.IncludesRemote;
            contract.IncludesParts = request.IncludesParts;
            contract.IncludesLabor = request.IncludesLabor;
            contract.IncludesTravel = request.IncludesTravel;
            contract.IncludesConsumables = request.IncludesConsumables;
            contract.IncludedHours = request.IncludedHours;
            contract.IncludedIncidents = request.IncludedIncidents;
            contract.IncludedPMVisits = request.IncludedPMVisits;
            contract.ExcludedItems = request.ExcludedItems;
            contract.IncludedItems = request.IncludedItems;
            contract.SLASettingsId = request.SLASettingsId;
            contract.ResponseTimeHours = request.ResponseTimeHours;
            contract.ResolutionTimeHours = request.ResolutionTimeHours;
            contract.SupportHoursType = request.SupportHoursType;
            contract.UptimeGuarantee = request.UptimeGuarantee;
            contract.DocumentUrl = request.DocumentUrl;
            contract.Terms = request.Terms;
            contract.SpecialConditions = request.SpecialConditions;
            contract.Notes = request.Notes;
            contract.AccountManagerId = request.AccountManagerId;
            contract.TechnicalManagerId = request.TechnicalManagerId;
            contract.CustomFields = request.CustomFields;
            contract.Tags = request.Tags;
        }

        contract.UpdatedBy = userId;
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractId} updated by user {UserId}", contractId, userId);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve updated contract");
    }

    public async Task<bool> DeleteContractAsync(
        Guid companyId,
        Guid contractId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken);

        if (contract == null)
            return false;

        // Only allow deletion of draft contracts
        if (contract.Status != ContractStatus.Draft)
            throw new InvalidOperationException("Only draft contracts can be deleted. Use terminate for active contracts.");

        contract.IsDeleted = true;
        contract.DeletedAt = DateTime.UtcNow;
        contract.DeletedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractId} deleted by user {UserId}", contractId, userId);

        return true;
    }

    #endregion

    #region Contract Status Operations

    public async Task<ContractDto> ActivateContractAsync(
        Guid companyId,
        Guid contractId,
        ActivateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        if (contract.Status != ContractStatus.Draft && contract.Status != ContractStatus.Pending)
            throw new InvalidOperationException($"Cannot activate contract with status {contract.Status}");

        contract.Status = ContractStatus.Active;
        contract.ActivatedDate = request.ActivatedDate ?? DateTime.UtcNow;
        contract.Notes = string.IsNullOrEmpty(contract.Notes)
            ? request.Notes
            : $"{contract.Notes}\n{request.Notes}";
        contract.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractNumber} activated", contract.ContractNumber);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve activated contract");
    }

    public async Task<ContractDto> RenewContractAsync(
        Guid companyId,
        Guid contractId,
        RenewContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        // Record renewal history
        var renewalHistory = new ContractRenewalHistory
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            RenewalNumber = contract.RenewalCount + 1,
            PreviousEndDate = contract.EndDate,
            NewStartDate = request.NewStartDate,
            NewEndDate = request.NewEndDate,
            RenewalPeriodMonths = request.RenewalPeriodMonths,
            Action = RenewalAction.ManualRenewal,
            RenewalDate = DateTime.UtcNow,
            ProcessedBy = userId,
            PreviousValue = contract.ContractValue,
            NewValue = request.NewValue ?? contract.ContractValue,
            PriceChangePercent = request.PriceChangePercent,
            Currency = contract.Currency,
            RenewalDiscount = request.RenewalDiscount,
            PONumber = request.PONumber,
            InvoiceNumber = request.InvoiceNumber,
            PreviousType = contract.Type,
            NewType = request.NewType ?? contract.Type,
            PreviousCoverageType = contract.CoverageType,
            NewCoverageType = request.NewCoverageType ?? contract.CoverageType,
            Reason = request.Notes,
            CreatedBy = userId
        };

        _context.ContractRenewalHistories.Add(renewalHistory);

        // Update contract
        contract.StartDate = request.NewStartDate;
        contract.EndDate = request.NewEndDate;
        contract.ContractValue = request.NewValue ?? contract.ContractValue;
        contract.LastRenewalDate = DateTime.UtcNow;
        contract.NextRenewalDate = request.NewEndDate.AddMonths(-(contract.RenewalNoticeDays.GetValueOrDefault(30) / 30));
        contract.RenewalCount++;
        contract.PONumber = request.PONumber ?? contract.PONumber;
        contract.InvoiceNumber = request.InvoiceNumber ?? contract.InvoiceNumber;
        contract.Status = ContractStatus.Active;
        contract.UpdatedBy = userId;

        if (request.NewType.HasValue)
            contract.Type = request.NewType.Value;

        if (request.NewCoverageType.HasValue)
            contract.CoverageType = request.NewCoverageType.Value;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractNumber} renewed for {Months} months",
            contract.ContractNumber, request.RenewalPeriodMonths);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve renewed contract");
    }

    public async Task<ContractDto> TerminateContractAsync(
        Guid companyId,
        Guid contractId,
        TerminateContractRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        contract.Status = ContractStatus.Terminated;
        contract.TerminatedDate = request.TerminatedDate;
        contract.TerminationReason = request.Reason;
        contract.TerminatedBy = userId;
        contract.Notes = string.IsNullOrEmpty(contract.Notes)
            ? request.Notes
            : $"{contract.Notes}\nTermination: {request.Notes}";
        contract.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractNumber} terminated. Reason: {Reason}",
            contract.ContractNumber, request.Reason);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve terminated contract");
    }

    public async Task<ContractDto> SuspendContractAsync(
        Guid companyId,
        Guid contractId,
        string? reason,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        if (contract.Status != ContractStatus.Active)
            throw new InvalidOperationException("Only active contracts can be suspended");

        contract.Status = ContractStatus.Suspended;
        contract.Notes = string.IsNullOrEmpty(contract.Notes)
            ? $"Suspended: {reason}"
            : $"{contract.Notes}\nSuspended: {reason}";
        contract.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractNumber} suspended", contract.ContractNumber);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve suspended contract");
    }

    public async Task<ContractDto> ReactivateContractAsync(
        Guid companyId,
        Guid contractId,
        string? notes,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        if (contract.Status != ContractStatus.Suspended)
            throw new InvalidOperationException("Only suspended contracts can be reactivated");

        contract.Status = ContractStatus.Active;
        contract.Notes = string.IsNullOrEmpty(contract.Notes)
            ? $"Reactivated: {notes}"
            : $"{contract.Notes}\nReactivated: {notes}";
        contract.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Contract {ContractNumber} reactivated", contract.ContractNumber);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve reactivated contract");
    }

    #endregion

    #region Contract Items

    public async Task<List<ContractItemDto>> GetContractItemsAsync(
        Guid companyId,
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var items = await _context.ContractItems
            .Include(i => i.Product)
            .Include(i => i.Contract)
            .Where(i => i.ContractId == contractId && i.Contract.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        return items.Select(MapToItemDto).ToList();
    }

    public async Task<ContractItemDto> AddContractItemAsync(
        Guid companyId,
        Guid contractId,
        CreateContractItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        var item = new ContractItem
        {
            Id = Guid.NewGuid(),
            ContractId = contractId,
            ProductId = request.ProductId,
            AssetId = request.AssetId,
            ItemDescription = request.ItemDescription,
            SerialNumbers = request.SerialNumbers,
            Quantity = request.Quantity,
            UnitOfMeasure = request.UnitOfMeasure,
            CoverageStartDate = request.CoverageStartDate ?? contract.StartDate,
            CoverageEndDate = request.CoverageEndDate ?? contract.EndDate,
            ItemValue = request.ItemValue,
            DiscountPercent = request.DiscountPercent,
            Currency = request.Currency,
            ResponseTimeHours = request.ResponseTimeHours,
            ResolutionTimeHours = request.ResolutionTimeHours,
            PriorityLevel = request.PriorityLevel,
            LocationId = request.LocationId,
            SiteInfo = request.SiteInfo,
            MaintenanceIntervalDays = request.MaintenanceIntervalDays,
            SpecialConditions = request.SpecialConditions,
            Notes = request.Notes,
            CustomFields = request.CustomFields,
            Status = ContractItemStatus.Covered,
            IsActive = true,
            CreatedBy = userId
        };

        _context.ContractItems.Add(item);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToItemDto(item);
    }

    public async Task<ContractItemDto> UpdateContractItemAsync(
        Guid companyId,
        Guid contractId,
        Guid itemId,
        UpdateContractItemRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.ContractItems
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ContractId == contractId && i.Contract.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract item {itemId} not found");

        item.ItemDescription = request.ItemDescription;
        item.SerialNumbers = request.SerialNumbers;
        item.Quantity = request.Quantity;
        item.UnitOfMeasure = request.UnitOfMeasure;
        item.CoverageStartDate = request.CoverageStartDate;
        item.CoverageEndDate = request.CoverageEndDate;
        item.ItemValue = request.ItemValue;
        item.DiscountPercent = request.DiscountPercent;
        item.ResponseTimeHours = request.ResponseTimeHours;
        item.ResolutionTimeHours = request.ResolutionTimeHours;
        item.PriorityLevel = request.PriorityLevel;
        item.Status = request.Status;
        item.IsActive = request.IsActive;
        item.LocationId = request.LocationId;
        item.SiteInfo = request.SiteInfo;
        item.MaintenanceIntervalDays = request.MaintenanceIntervalDays;
        item.SpecialConditions = request.SpecialConditions;
        item.Notes = request.Notes;
        item.CustomFields = request.CustomFields;
        item.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToItemDto(item);
    }

    public async Task<bool> RemoveContractItemAsync(
        Guid companyId,
        Guid contractId,
        Guid itemId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var item = await _context.ContractItems
            .Include(i => i.Contract)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.ContractId == contractId && i.Contract.CompanyId == companyId, cancellationToken);

        if (item == null)
            return false;

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    #endregion

    #region Coverage Checks

    public async Task<CoverageCheckResult> CheckCustomerCoverageAsync(
        Guid companyId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var activeContract = await _context.Contracts
            .Where(c => c.CompanyId == companyId &&
                       c.CustomerId == customerId &&
                       c.Status == ContractStatus.Active &&
                       c.EndDate > DateTime.UtcNow)
            .OrderByDescending(c => c.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (activeContract == null)
        {
            return new CoverageCheckResult
            {
                IsCovered = false,
                Message = "No active contract found for this customer"
            };
        }

        return new CoverageCheckResult
        {
            IsCovered = true,
            ContractId = activeContract.Id,
            ContractNumber = activeContract.ContractNumber,
            ContractName = activeContract.Name,
            ContractType = activeContract.Type,
            CoverageType = activeContract.CoverageType,
            CoverageEndDate = activeContract.EndDate,
            DaysRemaining = (int)(activeContract.EndDate - DateTime.UtcNow).TotalDays,
            ResponseTimeHours = activeContract.ResponseTimeHours,
            ResolutionTimeHours = activeContract.ResolutionTimeHours,
            IncludesParts = activeContract.IncludesParts,
            IncludesLabor = activeContract.IncludesLabor,
            IncludesOnsite = activeContract.IncludesOnsite,
            Message = "Customer is covered under active contract"
        };
    }

    public async Task<CoverageCheckResult> CheckProductCoverageAsync(
        Guid companyId,
        Guid customerId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var coveredItem = await _context.ContractItems
            .Include(i => i.Contract)
            .Where(i => i.Contract.CompanyId == companyId &&
                       i.Contract.CustomerId == customerId &&
                       i.ProductId == productId &&
                       i.Contract.Status == ContractStatus.Active &&
                       i.IsActive &&
                       (i.CoverageEndDate ?? i.Contract.EndDate) > DateTime.UtcNow)
            .OrderByDescending(i => i.CoverageEndDate ?? i.Contract.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (coveredItem == null)
        {
            return new CoverageCheckResult
            {
                IsCovered = false,
                Message = "Product is not covered under any active contract"
            };
        }

        var contract = coveredItem.Contract;
        var endDate = coveredItem.CoverageEndDate ?? contract.EndDate;

        return new CoverageCheckResult
        {
            IsCovered = true,
            ContractId = contract.Id,
            ContractNumber = contract.ContractNumber,
            ContractName = contract.Name,
            ContractType = contract.Type,
            CoverageType = contract.CoverageType,
            CoverageEndDate = endDate,
            DaysRemaining = (int)(endDate - DateTime.UtcNow).TotalDays,
            ResponseTimeHours = coveredItem.ResponseTimeHours ?? contract.ResponseTimeHours,
            ResolutionTimeHours = coveredItem.ResolutionTimeHours ?? contract.ResolutionTimeHours,
            IncludesParts = contract.IncludesParts,
            IncludesLabor = contract.IncludesLabor,
            IncludesOnsite = contract.IncludesOnsite,
            Message = "Product is covered under active contract"
        };
    }

    public async Task<CoverageCheckResult> CheckAssetCoverageAsync(
        Guid companyId,
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        var coveredItem = await _context.ContractItems
            .Include(i => i.Contract)
            .Where(i => i.Contract.CompanyId == companyId &&
                       i.AssetId == assetId &&
                       i.Contract.Status == ContractStatus.Active &&
                       i.IsActive &&
                       (i.CoverageEndDate ?? i.Contract.EndDate) > DateTime.UtcNow)
            .OrderByDescending(i => i.CoverageEndDate ?? i.Contract.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (coveredItem == null)
        {
            return new CoverageCheckResult
            {
                IsCovered = false,
                Message = "Asset is not covered under any active contract"
            };
        }

        var contract = coveredItem.Contract;
        var endDate = coveredItem.CoverageEndDate ?? contract.EndDate;

        return new CoverageCheckResult
        {
            IsCovered = true,
            ContractId = contract.Id,
            ContractNumber = contract.ContractNumber,
            ContractName = contract.Name,
            ContractType = contract.Type,
            CoverageType = contract.CoverageType,
            CoverageEndDate = endDate,
            DaysRemaining = (int)(endDate - DateTime.UtcNow).TotalDays,
            ResponseTimeHours = coveredItem.ResponseTimeHours ?? contract.ResponseTimeHours,
            ResolutionTimeHours = coveredItem.ResolutionTimeHours ?? contract.ResolutionTimeHours,
            IncludesParts = contract.IncludesParts,
            IncludesLabor = contract.IncludesLabor,
            IncludesOnsite = contract.IncludesOnsite,
            Message = "Asset is covered under active contract"
        };
    }

    #endregion

    #region Renewal & Expiration

    public async Task<ExpiringContractsReportDto> GetExpiringContractsAsync(
        Guid companyId,
        int daysAhead = 30,
        CancellationToken cancellationToken = default)
    {
        var expiryDate = DateTime.UtcNow.AddDays(daysAhead);

        var contracts = await _context.Contracts
            .Include(c => c.Customer)
            .Where(c => c.CompanyId == companyId &&
                       c.Status == ContractStatus.Active &&
                       c.EndDate <= expiryDate &&
                       c.EndDate > DateTime.UtcNow)
            .OrderBy(c => c.EndDate)
            .ToListAsync(cancellationToken);

        var report = new ExpiringContractsReportDto
        {
            TotalExpiring = contracts.Count,
            TotalValue = contracts.Sum(c => c.ContractValue ?? 0),
            Contracts = contracts.Select(c => new ExpiringContractDto
            {
                Id = c.Id,
                ContractNumber = c.ContractNumber,
                Name = c.Name,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer.Name,
                CustomerEmail = c.Customer.PrimaryEmail,
                CustomerPhone = c.Customer.PrimaryPhone,
                Type = c.Type,
                EndDate = c.EndDate,
                DaysRemaining = (int)(c.EndDate - DateTime.UtcNow).TotalDays,
                ContractValue = c.ContractValue,
                Currency = c.Currency,
                AutoRenew = c.AutoRenew,
                Status = c.Status,
                AccountManagerId = c.AccountManagerId,
                RenewalNotes = c.Notes
            }).ToList()
        };

        return report;
    }

    public async Task<List<ContractSummaryDto>> GetContractsByCustomerAsync(
        Guid companyId,
        Guid customerId,
        bool activeOnly = false,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Include(c => c.Customer)
            .Include(c => c.Items)
            .Where(c => c.CompanyId == companyId && c.CustomerId == customerId);

        if (activeOnly)
            query = query.Where(c => c.Status == ContractStatus.Active);

        var contracts = await query
            .OrderByDescending(c => c.EndDate)
            .ToListAsync(cancellationToken);

        return contracts.Select(MapToSummaryDto).ToList();
    }

    public async Task<List<ContractRenewalSummaryDto>> GetRenewalHistoryAsync(
        Guid companyId,
        Guid contractId,
        CancellationToken cancellationToken = default)
    {
        var history = await _context.ContractRenewalHistories
            .Include(h => h.Contract)
            .Where(h => h.ContractId == contractId && h.Contract.CompanyId == companyId)
            .OrderByDescending(h => h.RenewalDate)
            .ToListAsync(cancellationToken);

        return history.Select(h => new ContractRenewalSummaryDto
        {
            Id = h.Id,
            RenewalNumber = h.RenewalNumber,
            Action = h.Action,
            RenewalDate = h.RenewalDate,
            NewStartDate = h.NewStartDate,
            NewEndDate = h.NewEndDate,
            PreviousValue = h.PreviousValue,
            NewValue = h.NewValue,
            PriceChangePercent = h.PriceChangePercent,
            Currency = h.Currency
        }).ToList();
    }

    public async Task<int> ProcessAutoRenewalsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var expiringContracts = await _context.Contracts
            .Where(c => c.CompanyId == companyId &&
                       c.Status == ContractStatus.Active &&
                       c.AutoRenew &&
                       c.EndDate <= DateTime.UtcNow.AddDays(1) &&
                       c.EndDate > DateTime.UtcNow.AddDays(-1) &&
                       (!c.MaxRenewals.HasValue || c.RenewalCount < c.MaxRenewals.Value))
            .ToListAsync(cancellationToken);

        var renewedCount = 0;

        foreach (var contract in expiringContracts)
        {
            try
            {
                var renewalMonths = contract.RenewalPeriodMonths ?? 12;
                var newStartDate = contract.EndDate;
                var newEndDate = contract.EndDate.AddMonths(renewalMonths);

                // Record renewal history
                var renewalHistory = new ContractRenewalHistory
                {
                    Id = Guid.NewGuid(),
                    ContractId = contract.Id,
                    RenewalNumber = contract.RenewalCount + 1,
                    PreviousEndDate = contract.EndDate,
                    NewStartDate = newStartDate,
                    NewEndDate = newEndDate,
                    RenewalPeriodMonths = renewalMonths,
                    Action = RenewalAction.AutoRenewed,
                    RenewalDate = DateTime.UtcNow,
                    PreviousValue = contract.ContractValue,
                    NewValue = contract.ContractValue,
                    Currency = contract.Currency,
                    Reason = "Automatic renewal"
                };

                _context.ContractRenewalHistories.Add(renewalHistory);

                // Update contract
                contract.StartDate = newStartDate;
                contract.EndDate = newEndDate;
                contract.LastRenewalDate = DateTime.UtcNow;
                contract.RenewalCount++;

                renewedCount++;

                _logger.LogInformation("Auto-renewed contract {ContractNumber}", contract.ContractNumber);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to auto-renew contract {ContractId}", contract.Id);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return renewedCount;
    }

    #endregion

    #region Statistics & Reporting

    public async Task<ContractStatisticsDto> GetContractStatisticsAsync(
        Guid companyId,
        CancellationToken cancellationToken = default)
    {
        var contracts = await _context.Contracts
            .Where(c => c.CompanyId == companyId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        return new ContractStatisticsDto
        {
            TotalContracts = contracts.Count,
            ActiveContracts = contracts.Count(c => c.Status == ContractStatus.Active),
            ExpiredContracts = contracts.Count(c => c.Status == ContractStatus.Expired),
            ExpiringIn30Days = contracts.Count(c => c.Status == ContractStatus.Active && c.EndDate <= now.AddDays(30) && c.EndDate > now),
            ExpiringIn60Days = contracts.Count(c => c.Status == ContractStatus.Active && c.EndDate <= now.AddDays(60) && c.EndDate > now),
            ExpiringIn90Days = contracts.Count(c => c.Status == ContractStatus.Active && c.EndDate <= now.AddDays(90) && c.EndDate > now),
            PendingRenewal = contracts.Count(c => c.Status == ContractStatus.PendingRenewal),
            TotalContractValue = contracts.Sum(c => c.ContractValue ?? 0),
            ActiveContractValue = contracts.Where(c => c.Status == ContractStatus.Active).Sum(c => c.ContractValue ?? 0),
            ContractsByType = contracts.GroupBy(c => c.Type.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ContractsByStatus = contracts.GroupBy(c => c.Status.ToString()).ToDictionary(g => g.Key, g => g.Count()),
            ContractsByCoverageType = contracts.GroupBy(c => c.CoverageType.ToString()).ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<List<ContractLookupDto>> GetContractLookupAsync(
        Guid companyId,
        Guid? customerId = null,
        ContractStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Contracts
            .Where(c => c.CompanyId == companyId);

        if (customerId.HasValue)
            query = query.Where(c => c.CustomerId == customerId.Value);

        if (status.HasValue)
            query = query.Where(c => c.Status == status.Value);

        var contracts = await query
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        return contracts.Select(c => new ContractLookupDto
        {
            Id = c.Id,
            ContractNumber = c.ContractNumber,
            Name = c.Name,
            Type = c.Type,
            Status = c.Status,
            EndDate = c.EndDate
        }).ToList();
    }

    #endregion

    #region Usage Tracking

    public async Task<ContractDto> RecordUsageHoursAsync(
        Guid companyId,
        Guid contractId,
        int hours,
        string? notes,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        contract.UsedHours = (contract.UsedHours ?? 0) + hours;
        contract.LastServiceDate = DateTime.UtcNow;
        contract.UpdatedBy = userId;

        if (!string.IsNullOrEmpty(notes))
        {
            contract.Notes = string.IsNullOrEmpty(contract.Notes)
                ? $"Usage recorded: {hours}h - {notes}"
                : $"{contract.Notes}\nUsage recorded: {hours}h - {notes}";
        }

        await _context.SaveChangesAsync(cancellationToken);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve contract after usage update");
    }

    public async Task<ContractDto> RecordIncidentAsync(
        Guid companyId,
        Guid contractId,
        string? notes,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var contract = await _context.Contracts
            .FirstOrDefaultAsync(c => c.Id == contractId && c.CompanyId == companyId, cancellationToken)
            ?? throw new KeyNotFoundException($"Contract {contractId} not found");

        contract.UsedIncidents = (contract.UsedIncidents ?? 0) + 1;
        contract.LastServiceDate = DateTime.UtcNow;
        contract.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetContractByIdAsync(companyId, contractId, cancellationToken)
            ?? throw new InvalidOperationException("Failed to retrieve contract after incident update");
    }

    #endregion

    #region Private Methods

    private async Task<string> GenerateContractNumberAsync(Guid companyId, CancellationToken cancellationToken)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CTR-{year}-";

        // Include deleted records to avoid duplicate key violations
        // (unique index on ContractNumber doesn't have IsDeleted filter)
        var lastNumber = await _context.Contracts
            .IgnoreQueryFilters()
            .Where(c => c.CompanyId == companyId && c.ContractNumber.StartsWith(prefix))
            .OrderByDescending(c => c.ContractNumber)
            .Select(c => c.ContractNumber)
            .FirstOrDefaultAsync(cancellationToken);

        var nextNumber = 1;
        if (lastNumber != null)
        {
            var parts = lastNumber.Split('-');
            if (parts.Length >= 3 && int.TryParse(parts[2], out var num))
            {
                nextNumber = num + 1;
            }
        }

        return $"{prefix}{nextNumber:D5}";
    }

    private static ContractDto MapToDto(Contract contract)
    {
        var daysRemaining = (int)(contract.EndDate - DateTime.UtcNow).TotalDays;

        return new ContractDto
        {
            Id = contract.Id,
            CompanyId = contract.CompanyId,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer?.Name ?? string.Empty,
            PartnerId = contract.PartnerId,
            PartnerName = contract.Partner?.Name,
            ContractNumber = contract.ContractNumber,
            Name = contract.Name,
            Type = contract.Type,
            Description = contract.Description,
            ExternalContractId = contract.ExternalContractId,
            PONumber = contract.PONumber,
            InvoiceNumber = contract.InvoiceNumber,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            SignedDate = contract.SignedDate,
            ActivatedDate = contract.ActivatedDate,
            DaysRemaining = daysRemaining,
            IsExpiringSoon = daysRemaining <= 30 && daysRemaining > 0,
            AutoRenew = contract.AutoRenew,
            RenewalPeriodMonths = contract.RenewalPeriodMonths,
            RenewalNoticeDays = contract.RenewalNoticeDays,
            NextRenewalDate = contract.NextRenewalDate,
            RenewalCount = contract.RenewalCount,
            ContractValue = contract.ContractValue,
            AnnualValue = contract.AnnualValue,
            MonthlyValue = contract.MonthlyValue,
            Currency = contract.Currency,
            BillingFrequency = contract.BillingFrequency,
            PaymentTerms = contract.PaymentTerms,
            CoverageType = contract.CoverageType,
            IncludesOnsite = contract.IncludesOnsite,
            IncludesRemote = contract.IncludesRemote,
            IncludesParts = contract.IncludesParts,
            IncludesLabor = contract.IncludesLabor,
            IncludedHours = contract.IncludedHours,
            UsedHours = contract.UsedHours,
            RemainingHours = contract.RemainingHours,
            IncludedIncidents = contract.IncludedIncidents,
            UsedIncidents = contract.UsedIncidents,
            SLASettingsId = contract.SLASettingsId,
            SLASettingsName = contract.SLASettingsId.HasValue ? "SLA Settings" : null,
            ResponseTimeHours = contract.ResponseTimeHours,
            ResolutionTimeHours = contract.ResolutionTimeHours,
            SupportHoursType = contract.SupportHoursType,
            Status = contract.Status,
            ItemCount = contract.Items?.Count ?? 0,
            Items = contract.Items?.Select(i => new ContractItemSummaryDto
            {
                Id = i.Id,
                ItemDescription = i.ItemDescription,
                ProductCode = i.Product?.ProductCode,
                ProductName = i.Product?.Name,
                AssetTag = null, // Will be set when Asset entity is linked
                SerialNumber = i.SerialNumbers,
                Quantity = i.Quantity,
                CoverageEndDate = i.CoverageEndDate,
                IsCovered = (i.CoverageEndDate ?? contract.EndDate) > DateTime.UtcNow,
                Status = i.Status,
                IsActive = i.IsActive
            }).ToList() ?? new List<ContractItemSummaryDto>(),
            CreatedAt = contract.CreatedAt,
            UpdatedAt = contract.UpdatedAt
        };
    }

    private static ContractSummaryDto MapToSummaryDto(Contract contract)
    {
        var daysRemaining = (int)(contract.EndDate - DateTime.UtcNow).TotalDays;

        return new ContractSummaryDto
        {
            Id = contract.Id,
            ContractNumber = contract.ContractNumber,
            Name = contract.Name,
            Type = contract.Type,
            CustomerId = contract.CustomerId,
            CustomerName = contract.Customer?.Name ?? string.Empty,
            PartnerName = contract.Partner?.Name,
            StartDate = contract.StartDate,
            EndDate = contract.EndDate,
            DaysRemaining = daysRemaining,
            IsExpiringSoon = daysRemaining <= 30 && daysRemaining > 0,
            ContractValue = contract.ContractValue,
            Currency = contract.Currency,
            Status = contract.Status,
            CoverageType = contract.CoverageType,
            ItemCount = contract.Items?.Count ?? 0,
            AutoRenew = contract.AutoRenew
        };
    }

    private static ContractItemDto MapToItemDto(ContractItem item)
    {
        return new ContractItemDto
        {
            Id = item.Id,
            ContractId = item.ContractId,
            ContractNumber = item.Contract?.ContractNumber ?? string.Empty,
            ProductId = item.ProductId,
            ProductCode = item.Product?.ProductCode,
            ProductName = item.Product?.Name,
            AssetId = item.AssetId,
            AssetTag = null, // Will be set when Asset entity is linked
            SerialNumber = item.SerialNumbers,
            ItemDescription = item.ItemDescription,
            SerialNumbers = item.SerialNumbers,
            Quantity = item.Quantity,
            UnitOfMeasure = item.UnitOfMeasure,
            CoverageStartDate = item.CoverageStartDate,
            CoverageEndDate = item.CoverageEndDate,
            IsCovered = item.CoverageEndDate.HasValue && item.CoverageEndDate.Value > DateTime.UtcNow,
            DaysRemaining = item.CoverageEndDate.HasValue ? (int)(item.CoverageEndDate.Value - DateTime.UtcNow).TotalDays : null,
            ItemValue = item.ItemValue,
            DiscountPercent = item.DiscountPercent,
            Currency = item.Currency,
            ResponseTimeHours = item.ResponseTimeHours,
            ResolutionTimeHours = item.ResolutionTimeHours,
            PriorityLevel = item.PriorityLevel,
            Status = item.Status,
            IsActive = item.IsActive,
            LocationId = item.LocationId,
            SiteInfo = item.SiteInfo,
            LastMaintenanceDate = item.LastMaintenanceDate,
            NextMaintenanceDate = item.NextMaintenanceDate,
            MaintenanceIntervalDays = item.MaintenanceIntervalDays,
            ServiceCallCount = item.ServiceCallCount,
            SpecialConditions = item.SpecialConditions,
            Notes = item.Notes,
            CreatedAt = item.CreatedAt,
            UpdatedAt = item.UpdatedAt
        };
    }

    #endregion

    #region Warranty Operations

    public async Task<PagedResult<WarrantySummaryDto>> GetWarrantiesAsync(
        Guid companyId,
        WarrantySearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Warranty>()
            .Include(w => w.Product)
            .Include(w => w.Category)
            .Where(w => w.CompanyId == companyId);

        // Apply filters
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(w =>
                w.Code.ToLower().Contains(searchTerm) ||
                w.Name.ToLower().Contains(searchTerm));
        }

        if (request.ProductId.HasValue)
            query = query.Where(w => w.ProductId == request.ProductId.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(w => w.CategoryId == request.CategoryId.Value);

        if (request.Type.HasValue)
            query = query.Where(w => w.Type == request.Type.Value);

        if (request.CoverageType.HasValue)
            query = query.Where(w => w.CoverageType == request.CoverageType.Value);

        if (request.IsActive.HasValue)
            query = query.Where(w => w.IsActive == request.IsActive.Value);

        if (request.IsDefault.HasValue)
            query = query.Where(w => w.IsDefault == request.IsDefault.Value);

        if (request.MinDurationMonths.HasValue)
            query = query.Where(w => w.DurationMonths >= request.MinDurationMonths.Value);

        if (request.MaxDurationMonths.HasValue)
            query = query.Where(w => w.DurationMonths <= request.MaxDurationMonths.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "code" => request.SortDescending ? query.OrderByDescending(w => w.Code) : query.OrderBy(w => w.Code),
            "name" => request.SortDescending ? query.OrderByDescending(w => w.Name) : query.OrderBy(w => w.Name),
            "duration" => request.SortDescending ? query.OrderByDescending(w => w.DurationMonths) : query.OrderBy(w => w.DurationMonths),
            _ => request.SortDescending ? query.OrderByDescending(w => w.DisplayOrder) : query.OrderBy(w => w.DisplayOrder)
        };

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(w => new WarrantySummaryDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Type = w.Type,
                DurationMonths = w.DurationMonths,
                ExtendedDurationMonths = w.ExtendedDurationMonths,
                CoverageType = w.CoverageType,
                IncludesParts = w.IncludesParts,
                IncludesLabor = w.IncludesLabor,
                ExtendedWarrantyPrice = w.ExtendedWarrantyPrice,
                Currency = w.Currency,
                IsActive = w.IsActive,
                IsDefault = w.IsDefault,
                ProductName = w.Product != null ? w.Product.Name : null,
                CategoryName = w.Category != null ? w.Category.Name : null
            })
            .ToListAsync(cancellationToken);

        return new PagedResult<WarrantySummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = request.PageNumber,
            PageSize = request.PageSize
        };
    }

    public async Task<WarrantyDto?> GetWarrantyByIdAsync(
        Guid companyId,
        Guid warrantyId,
        CancellationToken cancellationToken = default)
    {
        var warranty = await _context.Set<Warranty>()
            .Include(w => w.Product)
            .Include(w => w.Category)
            .FirstOrDefaultAsync(w => w.Id == warrantyId && w.CompanyId == companyId, cancellationToken);

        return warranty != null ? MapToWarrantyDto(warranty) : null;
    }

    public async Task<WarrantyDto?> GetWarrantyByCodeAsync(
        Guid companyId,
        string code,
        CancellationToken cancellationToken = default)
    {
        var warranty = await _context.Set<Warranty>()
            .Include(w => w.Product)
            .Include(w => w.Category)
            .FirstOrDefaultAsync(w => w.Code == code && w.CompanyId == companyId, cancellationToken);

        return warranty != null ? MapToWarrantyDto(warranty) : null;
    }

    public async Task<WarrantyDto> CreateWarrantyAsync(
        Guid companyId,
        CreateWarrantyRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // Validate code uniqueness
        var existingWarranty = await _context.Set<Warranty>()
            .FirstOrDefaultAsync(w => w.Code == request.Code && w.CompanyId == companyId, cancellationToken);

        if (existingWarranty != null)
            throw new ArgumentException($"A warranty with code '{request.Code}' already exists");

        var warranty = new Warranty
        {
            Id = Guid.NewGuid(),
            CompanyId = companyId,
            ProductId = request.ProductId,
            CategoryId = request.CategoryId,
            Code = request.Code,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            DurationMonths = request.DurationMonths,
            ExtendedDurationMonths = request.ExtendedDurationMonths,
            GracePeriodDays = request.GracePeriodDays,
            MaxDurationMonths = request.MaxDurationMonths,
            CoverageType = request.CoverageType,
            IncludesParts = request.IncludesParts,
            IncludesLabor = request.IncludesLabor,
            IncludesOnsite = request.IncludesOnsite,
            IncludesShipping = request.IncludesShipping,
            CoveredItems = request.CoveredItems,
            ExcludedItems = request.ExcludedItems,
            Terms = request.Terms,
            IsTransferable = request.IsTransferable,
            RequiresRegistration = request.RequiresRegistration,
            RegistrationDeadlineDays = request.RegistrationDeadlineDays,
            RequiresProofOfPurchase = request.RequiresProofOfPurchase,
            Conditions = request.Conditions,
            ExtendedWarrantyPrice = request.ExtendedWarrantyPrice,
            ExtendedWarrantyPricePercent = request.ExtendedWarrantyPricePercent,
            Currency = request.Currency,
            ResponseTimeHours = request.ResponseTimeHours,
            ResolutionTimeHours = request.ResolutionTimeHours,
            SupportHoursType = request.SupportHoursType,
            MaxClaims = request.MaxClaims,
            MaxClaimValue = request.MaxClaimValue,
            MaxClaimValuePercent = request.MaxClaimValuePercent,
            DeductibleAmount = request.DeductibleAmount,
            IsActive = true,
            IsDefault = request.IsDefault,
            DisplayOrder = request.DisplayOrder,
            Notes = request.Notes,
            CustomFields = request.CustomFields,
            ExternalWarrantyId = request.ExternalWarrantyId,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.Set<Warranty>().Add(warranty);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created warranty {WarrantyId} with code {Code}", warranty.Id, warranty.Code);

        return MapToWarrantyDto(warranty);
    }

    public async Task<WarrantyDto> UpdateWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        UpdateWarrantyRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var warranty = await _context.Set<Warranty>()
            .FirstOrDefaultAsync(w => w.Id == warrantyId && w.CompanyId == companyId, cancellationToken);

        if (warranty == null)
            throw new KeyNotFoundException($"Warranty {warrantyId} not found");

        warranty.Name = request.Name;
        warranty.Description = request.Description;
        warranty.Type = request.Type;
        warranty.DurationMonths = request.DurationMonths;
        warranty.ExtendedDurationMonths = request.ExtendedDurationMonths;
        warranty.GracePeriodDays = request.GracePeriodDays;
        warranty.MaxDurationMonths = request.MaxDurationMonths;
        warranty.CoverageType = request.CoverageType;
        warranty.IncludesParts = request.IncludesParts;
        warranty.IncludesLabor = request.IncludesLabor;
        warranty.IncludesOnsite = request.IncludesOnsite;
        warranty.IncludesShipping = request.IncludesShipping;
        warranty.CoveredItems = request.CoveredItems;
        warranty.ExcludedItems = request.ExcludedItems;
        warranty.Terms = request.Terms;
        warranty.IsTransferable = request.IsTransferable;
        warranty.RequiresRegistration = request.RequiresRegistration;
        warranty.RegistrationDeadlineDays = request.RegistrationDeadlineDays;
        warranty.RequiresProofOfPurchase = request.RequiresProofOfPurchase;
        warranty.Conditions = request.Conditions;
        warranty.ExtendedWarrantyPrice = request.ExtendedWarrantyPrice;
        warranty.ExtendedWarrantyPricePercent = request.ExtendedWarrantyPricePercent;
        warranty.Currency = request.Currency;
        warranty.ResponseTimeHours = request.ResponseTimeHours;
        warranty.ResolutionTimeHours = request.ResolutionTimeHours;
        warranty.SupportHoursType = request.SupportHoursType;
        warranty.MaxClaims = request.MaxClaims;
        warranty.MaxClaimValue = request.MaxClaimValue;
        warranty.MaxClaimValuePercent = request.MaxClaimValuePercent;
        warranty.DeductibleAmount = request.DeductibleAmount;
        warranty.IsActive = request.IsActive;
        warranty.IsDefault = request.IsDefault;
        warranty.DisplayOrder = request.DisplayOrder;
        warranty.Notes = request.Notes;
        warranty.CustomFields = request.CustomFields;
        warranty.ExternalWarrantyId = request.ExternalWarrantyId;
        warranty.UpdatedAt = DateTime.UtcNow;
        warranty.UpdatedBy = userId;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated warranty {WarrantyId}", warrantyId);

        return MapToWarrantyDto(warranty);
    }

    public async Task<bool> DeleteWarrantyAsync(
        Guid companyId,
        Guid warrantyId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var warranty = await _context.Set<Warranty>()
            .FirstOrDefaultAsync(w => w.Id == warrantyId && w.CompanyId == companyId, cancellationToken);

        if (warranty == null)
            return false;

        _context.Set<Warranty>().Remove(warranty);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted warranty {WarrantyId}", warrantyId);

        return true;
    }

    public async Task<List<WarrantyLookupDto>> GetWarrantyLookupAsync(
        Guid companyId,
        Guid? productId = null,
        Guid? categoryId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Set<Warranty>()
            .Where(w => w.CompanyId == companyId && w.IsActive);

        if (productId.HasValue)
            query = query.Where(w => w.ProductId == productId.Value || w.ProductId == null);

        if (categoryId.HasValue)
            query = query.Where(w => w.CategoryId == categoryId.Value || w.CategoryId == null);

        return await query
            .OrderBy(w => w.DisplayOrder)
            .ThenBy(w => w.Name)
            .Select(w => new WarrantyLookupDto
            {
                Id = w.Id,
                Code = w.Code,
                Name = w.Name,
                Type = w.Type,
                DurationMonths = w.DurationMonths,
                IsDefault = w.IsDefault
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<WarrantyCheckResult> CheckWarrantyCoverageAsync(
        Guid companyId,
        Guid productId,
        DateTime purchaseDate,
        CancellationToken cancellationToken = default)
    {
        var warranty = await _context.Set<Warranty>()
            .Where(w => w.CompanyId == companyId && w.IsActive)
            .Where(w => w.ProductId == productId || w.CategoryId == null)
            .OrderBy(w => w.ProductId.HasValue ? 0 : 1) // Prefer product-specific warranty
            .ThenByDescending(w => w.IsDefault)
            .FirstOrDefaultAsync(cancellationToken);

        if (warranty == null)
        {
            return new WarrantyCheckResult
            {
                IsUnderWarranty = false,
                Message = "No warranty found for this product"
            };
        }

        var warrantyEndDate = purchaseDate.AddMonths(warranty.DurationMonths);
        var isUnderWarranty = DateTime.UtcNow <= warrantyEndDate;
        var daysRemaining = isUnderWarranty ? (int)(warrantyEndDate - DateTime.UtcNow).TotalDays : 0;

        return new WarrantyCheckResult
        {
            IsUnderWarranty = isUnderWarranty,
            WarrantyId = warranty.Id,
            WarrantyCode = warranty.Code,
            WarrantyName = warranty.Name,
            WarrantyType = warranty.Type,
            WarrantyStartDate = purchaseDate,
            WarrantyEndDate = warrantyEndDate,
            DaysRemaining = daysRemaining,
            CoverageType = warranty.CoverageType,
            IncludesParts = warranty.IncludesParts,
            IncludesLabor = warranty.IncludesLabor,
            IncludesOnsite = warranty.IncludesOnsite,
            IsExtendable = warranty.ExtendedDurationMonths.HasValue,
            ExtendedWarrantyPrice = warranty.ExtendedWarrantyPrice,
            Message = isUnderWarranty
                ? $"Product is under warranty. {daysRemaining} days remaining."
                : "Warranty has expired."
        };
    }

    private WarrantyDto MapToWarrantyDto(Warranty warranty)
    {
        return new WarrantyDto
        {
            Id = warranty.Id,
            CompanyId = warranty.CompanyId,
            ProductId = warranty.ProductId,
            ProductCode = null, // Product entity doesn't have Code property
            ProductName = warranty.Product?.Name,
            CategoryId = warranty.CategoryId,
            CategoryName = warranty.Category?.Name,
            Code = warranty.Code,
            Name = warranty.Name,
            Description = warranty.Description,
            Type = warranty.Type,
            DurationMonths = warranty.DurationMonths,
            ExtendedDurationMonths = warranty.ExtendedDurationMonths,
            GracePeriodDays = warranty.GracePeriodDays,
            MaxDurationMonths = warranty.MaxDurationMonths,
            CoverageType = warranty.CoverageType,
            IncludesParts = warranty.IncludesParts,
            IncludesLabor = warranty.IncludesLabor,
            IncludesOnsite = warranty.IncludesOnsite,
            IncludesShipping = warranty.IncludesShipping,
            CoveredItems = warranty.CoveredItems,
            ExcludedItems = warranty.ExcludedItems,
            Terms = warranty.Terms,
            IsTransferable = warranty.IsTransferable,
            RequiresRegistration = warranty.RequiresRegistration,
            RegistrationDeadlineDays = warranty.RegistrationDeadlineDays,
            RequiresProofOfPurchase = warranty.RequiresProofOfPurchase,
            ExtendedWarrantyPrice = warranty.ExtendedWarrantyPrice,
            ExtendedWarrantyPricePercent = warranty.ExtendedWarrantyPricePercent,
            Currency = warranty.Currency,
            ResponseTimeHours = warranty.ResponseTimeHours,
            ResolutionTimeHours = warranty.ResolutionTimeHours,
            SupportHoursType = warranty.SupportHoursType,
            MaxClaims = warranty.MaxClaims,
            MaxClaimValue = warranty.MaxClaimValue,
            MaxClaimValuePercent = warranty.MaxClaimValuePercent,
            DeductibleAmount = warranty.DeductibleAmount,
            IsActive = warranty.IsActive,
            IsDefault = warranty.IsDefault,
            DisplayOrder = warranty.DisplayOrder,
            CreatedAt = warranty.CreatedAt,
            UpdatedAt = warranty.UpdatedAt
        };
    }

    #endregion
}
