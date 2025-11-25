using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Queries;

public class GetComplaintHistoryQuery : IRequest<Result<ComplaintHistoryDto>>
{
    public Guid ComplaintId { get; set; }
}
