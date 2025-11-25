using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Queries;

public class GetEscalationMatricesQuery : IRequest<Result<List<EscalationMatrixDto>>>
{
    public Guid CompanyId { get; set; }
}
