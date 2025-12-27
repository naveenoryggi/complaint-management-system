using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Queries;

public class GetComplaintsQuery : IRequest<Result<PagedResult<ComplaintDto>>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public Guid? StatusMasterId { get; set; }
    public Guid? PriorityMasterId { get; set; }
    public Guid? CompanyId { get; set; }
    public Guid? AssignedToId { get; set; }
    public Guid? ComplainantId { get; set; }
    public string? SearchTerm { get; set; }

    /// <summary>
    /// When true, returns only unassigned complaints (where AssignedToId is null)
    /// </summary>
    public bool UnassignedOnly { get; set; } = false;

    /// <summary>
    /// When true, returns only complaints with customer responses awaiting handler response
    /// </summary>
    public bool WaitingForResponseOnly { get; set; } = false;
}
