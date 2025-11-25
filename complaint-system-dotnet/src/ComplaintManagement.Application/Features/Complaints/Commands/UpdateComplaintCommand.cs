using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Commands;

public class UpdateComplaintCommand : IRequest<Result<ComplaintDto>>
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public Guid PriorityMasterId { get; set; }
    public Guid? StatusMasterId { get; set; }
    public Guid? AssignedToId { get; set; }
    public string? ResolutionNotes { get; set; }
    public string? Tags { get; set; }
}
