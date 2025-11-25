using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Commands;

public class UpdateEscalationMatrixCommand : IRequest<Result<EscalationMatrixDto>>
{
    public Guid MatrixId { get; set; }
    public required UpdateEscalationMatrixRequest Request { get; set; }
    public Guid UpdatedBy { get; set; }
}
