namespace ComplaintManagement.Application.DTOs.Portal;

/// <summary>
/// Portal dashboard data
/// </summary>
public class PortalDashboardDto
{
    // Ticket Statistics
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int OverdueTickets { get; set; }
    public int TicketsThisMonth { get; set; }
    public int TicketsAwaitingResponse { get; set; }

    // Recent Tickets
    public List<PortalTicketSummaryDto> RecentTickets { get; set; } = new();

    // Assets Summary (if permission)
    public int? TotalAssets { get; set; }
    public int? ActiveAssets { get; set; }
    public int? AssetsNeedingService { get; set; }

    // Contracts Summary (if permission)
    public int? ActiveContracts { get; set; }
    public int? ContractsExpiringSoon { get; set; }

    // Quick Stats
    public decimal? AverageResolutionTimeHours { get; set; }
    public decimal? CustomerSatisfactionRating { get; set; }
}

/// <summary>
/// Portal asset summary for list view
/// </summary>
public class PortalAssetSummaryDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? ProductName { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string ConditionName { get; set; } = string.Empty;
    public string? LocationName { get; set; }
    public bool IsUnderWarranty { get; set; }
    public bool IsUnderContract { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
}

/// <summary>
/// Portal asset detail
/// </summary>
public class PortalAssetDetailDto
{
    public Guid Id { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? SerialNumber { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string ConditionName { get; set; } = string.Empty;

    // Product Information
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public string? Manufacturer { get; set; }
    public string? Model { get; set; }
    public string? Version { get; set; }

    // Location
    public string? LocationName { get; set; }
    public string? LocationAddress { get; set; }
    public string? Building { get; set; }
    public string? Floor { get; set; }
    public string? Room { get; set; }

    // Warranty & Contract
    public bool IsUnderWarranty { get; set; }
    public DateTime? WarrantyStartDate { get; set; }
    public DateTime? WarrantyEndDate { get; set; }
    public bool IsUnderContract { get; set; }
    public string? ContractNumber { get; set; }
    public DateTime? ContractEndDate { get; set; }

    // Service Information
    public DateTime? InstallationDate { get; set; }
    public DateTime? LastServiceDate { get; set; }
    public DateTime? NextServiceDate { get; set; }
    public int ServiceCallCount { get; set; }

    // Recent Service History (limited)
    public List<PortalServiceHistorySummaryDto> RecentServiceHistory { get; set; } = new();

    // Related Tickets
    public List<PortalTicketSummaryDto> RecentTickets { get; set; } = new();

    // Actions
    public bool CanCreateTicket { get; set; }
}

/// <summary>
/// Portal service history summary
/// </summary>
public class PortalServiceHistorySummaryDto
{
    public Guid Id { get; set; }
    public string ServiceNumber { get; set; } = string.Empty;
    public string ServiceTypeName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ResultName { get; set; } = string.Empty;
    public DateTime? ServiceDate { get; set; }
    public string? TechnicianName { get; set; }
}

/// <summary>
/// Portal contract summary for list view
/// </summary>
public class PortalContractSummaryDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsExpiringSoon { get; set; }
    public int DaysUntilExpiry { get; set; }
    public int CoveredAssetsCount { get; set; }
    public string? SupportHours { get; set; }
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }
}

/// <summary>
/// Portal contract detail
/// </summary>
public class PortalContractDetailDto
{
    public Guid Id { get; set; }
    public string ContractNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string CoverageTypeName { get; set; } = string.Empty;

    // Dates
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public DateTime? SignedDate { get; set; }
    public bool AutoRenew { get; set; }
    public DateTime? NextRenewalDate { get; set; }
    public int? RenewalNoticeDays { get; set; }

    // Coverage
    public bool IncludesOnsite { get; set; }
    public bool IncludesRemote { get; set; }
    public int? IncludedHours { get; set; }
    public int? UsedHours { get; set; }
    public int? RemainingHours => IncludedHours.HasValue ? IncludedHours.Value - (UsedHours ?? 0) : null;
    public string? SupportHours { get; set; }

    // SLA
    public int? ResponseTimeHours { get; set; }
    public int? ResolutionTimeHours { get; set; }

    // Covered Assets
    public List<PortalContractAssetDto> CoveredAssets { get; set; } = new();

    // Status
    public bool IsActive { get; set; }
    public bool IsExpired { get; set; }
    public bool IsExpiringSoon { get; set; }
    public int DaysUntilExpiry { get; set; }
}

/// <summary>
/// Asset covered by a contract (for portal view)
/// </summary>
public class PortalContractAssetDto
{
    public Guid AssetId { get; set; }
    public string AssetTag { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string? ProductName { get; set; }
    public string? LocationName { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Portal location summary
/// </summary>
public class PortalLocationSummaryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public string? AddressLine1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public bool IsPrimary { get; set; }
    public int AssetCount { get; set; }
    public int OpenTicketCount { get; set; }
}

/// <summary>
/// Portal category for ticket creation
/// </summary>
public class PortalCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<PortalCategoryDto>? SubCategories { get; set; }
}

/// <summary>
/// Portal priority option
/// </summary>
public class PortalPriorityDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Color { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Portal status option (for filtering)
/// </summary>
public class PortalStatusDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public bool IsOpenStatus { get; set; }
    public bool IsClosedStatus { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>
/// Portal knowledge base article (future)
/// </summary>
public class PortalKnowledgeBaseArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? Content { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public List<string>? Tags { get; set; }
    public int ViewCount { get; set; }
    public bool IsHelpful { get; set; }
    public DateTime PublishedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Portal announcements/notifications
/// </summary>
public class PortalAnnouncementDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Info, Warning, Maintenance
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsDismissible { get; set; }
}
