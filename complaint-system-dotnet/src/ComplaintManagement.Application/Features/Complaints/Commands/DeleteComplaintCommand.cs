using ComplaintManagement.Application.Common.Models;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class DeleteComplaintCommand : IRequest<Result>
{
    public Guid ComplaintId { get; set; }
    public Guid DeletedById { get; set; }
}
