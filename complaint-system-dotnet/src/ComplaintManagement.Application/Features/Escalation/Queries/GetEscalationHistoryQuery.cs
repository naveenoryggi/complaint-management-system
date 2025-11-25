using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Queries;

public class GetEscalationHistoryQuery : IRequest<Result<List<EscalationHistoryDto>>>
{
    public Guid ComplaintId { get; set; }
}
