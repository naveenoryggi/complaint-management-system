using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Commands;

public class AddEscalationLevelCommand : IRequest<Result<EscalationLevelDto>>
{
    public Guid MatrixId { get; set; }
    public required CreateEscalationLevelRequest Request { get; set; }
    public Guid CreatedBy { get; set; }
}
