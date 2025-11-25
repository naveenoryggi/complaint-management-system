using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Queries;

public class GetEscalationMatrixByIdQuery : IRequest<Result<EscalationMatrixDto>>
{
    public Guid MatrixId { get; set; }
}
