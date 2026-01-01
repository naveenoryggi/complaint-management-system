namespace ComplaintManagement.Application.DTOs.Portal;

/// <summary>
/// Ticket summary for portal list view
/// </summary>
public class PortalTicketSummaryDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string PriorityColor { get; set; } = string.Empty;
    public string? AssignedToName { get; set; }
    public string? LocationName { get; set; }
    public string? AssetName { get; set; }
    public string? AssetTag { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public bool HasCustomerResponse { get; set; }
    public int CommentCount { get; set; }
    public int AttachmentCount { get; set; }
    public bool IsOverdue { get; set; }
    public string? SLAStatus { get; set; }
}

/// <summary>
/// Full ticket details for portal view
/// </summary>
public class PortalTicketDetailDto
{
    public Guid Id { get; set; }
    public string ComplaintNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Category & Classification
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string StatusColor { get; set; } = string.Empty;
    public Guid? PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public string PriorityColor { get; set; } = string.Empty;

    // Assignment
    public string? AssignedToName { get; set; }
    public string? AssignedToEmail { get; set; }

    // Location & Asset
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public string? LocationAddress { get; set; }
    public Guid? AssetId { get; set; }
    public string? AssetName { get; set; }
    public string? AssetTag { get; set; }
    public string? AssetSerialNumber { get; set; }

    // Product & Brand (OEM)
    public Guid? ProductCategoryId { get; set; }
    public string? ProductCategoryName { get; set; }
    public Guid? ProductId { get; set; }
    public string? ProductName { get; set; }
    public string? ProductCode { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }

    // Contract & Warranty
    public Guid? ContractId { get; set; }
    public string? ContractNumber { get; set; }
    public bool IsUnderWarranty { get; set; }
    public bool IsUnderContract { get; set; }

    // Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime? ClosedAt { get; set; }

    // SLA
    public DateTime? ResponseDueDate { get; set; }
    public DateTime? ResolutionDueDate { get; set; }
    public bool IsOverdue { get; set; }
    public string? SLAStatus { get; set; }
    public string? SLAStatusColor { get; set; }

    // Comments & Attachments
    public List<PortalTicketCommentDto> Comments { get; set; } = new();
    public List<PortalTicketAttachmentDto> Attachments { get; set; } = new();

    // Customer feedback
    public int? CustomerRating { get; set; }
    public string? CustomerFeedback { get; set; }
    public bool CanRate { get; set; }
    public bool CanAddComment { get; set; }
    public bool CanReopen { get; set; }
}

/// <summary>
/// Ticket comment for portal view
/// </summary>
public class PortalTicketCommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
    public bool IsFromCustomer { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorRole { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<PortalTicketAttachmentDto>? Attachments { get; set; }
}

/// <summary>
/// Ticket attachment for portal view
/// </summary>
public class PortalTicketAttachmentDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long FileSize { get; set; }
    public string FileSizeDisplay => FormatFileSize(FileSize);
    public DateTime UploadedAt { get; set; }
    public string? UploadedByName { get; set; }
    public string DownloadUrl { get; set; } = string.Empty;

    private static string FormatFileSize(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB" };
        int counter = 0;
        decimal number = bytes;
        while (Math.Round(number / 1024) >= 1)
        {
            number /= 1024;
            counter++;
        }
        return $"{number:n1} {suffixes[counter]}";
    }
}

/// <summary>
/// Request to create a new ticket from portal
/// </summary>
public class PortalCreateTicketRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid CategoryId { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? AssetId { get; set; }

    // Product & Brand (OEM) selection
    public Guid? ProductCategoryId { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? BrandId { get; set; }

    public string? ContactPhone { get; set; }
    public string? PreferredContactMethod { get; set; }
    public List<string>? AttachmentPaths { get; set; }
    public Dictionary<string, string>? CustomFields { get; set; }
}

/// <summary>
/// Request to add a comment from portal
/// </summary>
public class PortalAddCommentRequest
{
    public string Content { get; set; } = string.Empty;
    public List<string>? AttachmentPaths { get; set; }
}

/// <summary>
/// Request to rate a ticket from portal
/// </summary>
public class PortalRateTicketRequest
{
    public int Rating { get; set; }
    public string? Feedback { get; set; }
}

/// <summary>
/// Request to reopen a ticket from portal
/// </summary>
public class PortalReopenTicketRequest
{
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// Portal ticket list query parameters
/// </summary>
public class PortalTicketListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? StatusId { get; set; }
    public Guid? PriorityId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? AssetId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsOpen { get; set; }
    public bool? IsOverdue { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

/// <summary>
/// Product category lookup for portal dropdown
/// </summary>
public class PortalProductCategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    public int Level { get; set; }
    public string? Icon { get; set; }
    public string? Color { get; set; }
    public bool HasChildren { get; set; }
}

/// <summary>
/// Product lookup for portal dropdown
/// </summary>
public class PortalProductDto
{
    public Guid Id { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public Guid? BrandId { get; set; }
    public string? BrandName { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsUnderWarranty { get; set; }
}

/// <summary>
/// Brand (OEM) lookup for portal dropdown
/// </summary>
public class PortalBrandDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? Website { get; set; }
}
