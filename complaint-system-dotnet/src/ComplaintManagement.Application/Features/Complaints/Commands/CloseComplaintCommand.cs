using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class CloseComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public Guid ComplaintId { get; set; }
    public Guid ClosedById { get; set; } // The user closing the complaint
    public string ResolutionNotes { get; set; } = string.Empty;
}
