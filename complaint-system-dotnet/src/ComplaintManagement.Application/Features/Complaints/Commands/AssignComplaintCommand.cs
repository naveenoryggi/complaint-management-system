using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class AssignComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public Guid ComplaintId { get; set; }
    public Guid AssignedToId { get; set; }
    public Guid AssignedById { get; set; } // The user performing the assignment
    public string? Notes { get; set; }
}
