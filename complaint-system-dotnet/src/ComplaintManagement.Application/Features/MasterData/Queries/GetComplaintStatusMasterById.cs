using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Queries;

public class GetComplaintStatusMasterByIdQuery : IRequest<Result<ComplaintStatusMasterDto>>
{
    public Guid Id { get; set; }
}
