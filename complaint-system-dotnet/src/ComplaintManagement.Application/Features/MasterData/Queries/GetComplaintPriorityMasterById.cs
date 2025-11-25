using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Queries;

public class GetComplaintPriorityMasterByIdQuery : IRequest<Result<ComplaintPriorityMasterDto>>
{
    public Guid Id { get; set; }
}
