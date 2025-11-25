using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Queries;

public class GetComplaintByIdQuery : IRequest<Result<ComplaintDto>>
{
    public Guid Id { get; set; }

    public GetComplaintByIdQuery()
    {
    }

    public GetComplaintByIdQuery(Guid id)
    {
        Id = id;
    }
}
