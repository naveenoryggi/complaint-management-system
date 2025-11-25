using ComplaintManagement.Application.Common.Models;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Commands;

public class DeleteComplaintStatusMasterCommand : IRequest<Result<bool>>
{
    public Guid Id { get; set; }
}
