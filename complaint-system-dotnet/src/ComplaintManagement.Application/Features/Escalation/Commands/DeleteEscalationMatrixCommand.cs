using ComplaintManagement.Application.Common.Models;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Commands;

public class DeleteEscalationMatrixCommand : IRequest<Result>
{
    public Guid MatrixId { get; set; }
    public Guid DeletedBy { get; set; }
}
