using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class EscalateComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public Guid ComplaintId { get; set; }
    public Guid EscalatedById { get; set; } // The user performing the escalation
    public string Reason { get; set; } = string.Empty;
}
