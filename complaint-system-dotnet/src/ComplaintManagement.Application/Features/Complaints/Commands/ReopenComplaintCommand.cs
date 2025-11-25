using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class ReopenComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public Guid ComplaintId { get; set; }
    public Guid ReopenedById { get; set; } // The user reopening the complaint
    public string Reason { get; set; } = string.Empty; // Reason for reopening
}
