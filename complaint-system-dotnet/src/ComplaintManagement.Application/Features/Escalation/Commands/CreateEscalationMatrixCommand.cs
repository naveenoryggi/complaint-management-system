using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Commands;

public class CreateEscalationMatrixCommand : IRequest<Result<EscalationMatrixDto>>
{
    public required CreateEscalationMatrixRequest Request { get; set; }
    public Guid CompanyId { get; set; }
    public Guid CreatedBy { get; set; }
}
