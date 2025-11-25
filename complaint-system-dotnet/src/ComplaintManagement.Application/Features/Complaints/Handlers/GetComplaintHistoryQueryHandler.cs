using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class GetComplaintHistoryQueryHandler : IRequestHandler<GetComplaintHistoryQuery, Result<ComplaintHistoryDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintHistoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintHistoryDto>> Handle(GetComplaintHistoryQuery request, CancellationToken cancellationToken)
    {
        // Get complaint with related data
        var complaint = await _unitOfWork.Complaints.GetComplaintWithDetailsAsync(request.ComplaintId, cancellationToken);

        // Return empty history if complaint doesn't exist (consistent with Get Comments behavior)
        if (complaint == null)
        {
            var emptyHistory = new ComplaintHistoryDto
            {
                ComplaintId = request.ComplaintId,
                ComplaintNumber = string.Empty,
                Events = new List<ComplaintHistoryEventDto>()
            };
            return Result<ComplaintHistoryDto>.Success(emptyHistory);
        }

        var historyDto = new ComplaintHistoryDto
        {
            ComplaintId = complaint.Id,
            ComplaintNumber = complaint.ComplaintNumber,
            Events = new List<ComplaintHistoryEventDto>()
        };

        // Add complaint creation event
        historyDto.Events.Add(new ComplaintHistoryEventDto
        {
            Timestamp = complaint.CreatedAt,
            EventType = "Created",
            Description = "Complaint submitted",
            PerformedBy = complaint.ComplainantId.ToString(),
            PerformedByName = complaint.Complainant?.FullName
        });

        // Add comments as events
        if (complaint.Comments != null)
        {
            foreach (var comment in complaint.Comments.Where(c => !c.IsDeleted).OrderBy(c => c.CreatedAt))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(comment.CreatedBy ?? Guid.Empty, cancellationToken);
                historyDto.Events.Add(new ComplaintHistoryEventDto
                {
                    Timestamp = comment.CreatedAt,
                    EventType = comment.IsInternal ? "InternalComment" : "Comment",
                    Description = comment.CommentText,
                    PerformedBy = comment.CreatedBy?.ToString(),
                    PerformedByName = user?.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "IsInternal", comment.IsInternal.ToString() }
                    }
                });
            }
        }

        // Add escalation events
        if (complaint.EscalationHistories != null)
        {
            foreach (var escalation in complaint.EscalationHistories.OrderBy(e => e.EscalatedAt))
            {
                var user = await _unitOfWork.Users.GetByIdAsync(escalation.EscalatedBy ?? Guid.Empty, cancellationToken);
                historyDto.Events.Add(new ComplaintHistoryEventDto
                {
                    Timestamp = escalation.EscalatedAt,
                    EventType = "Escalated",
                    Description = $"Escalated to level {escalation.Level}: {escalation.Reason}",
                    PerformedBy = escalation.EscalatedBy?.ToString(),
                    PerformedByName = user?.FullName,
                    Metadata = new Dictionary<string, string>
                    {
                        { "Level", escalation.Level.ToString() }
                    }
                });
            }
        }

        // Add closed event if applicable
        if (complaint.ClosedAt.HasValue)
        {
            historyDto.Events.Add(new ComplaintHistoryEventDto
            {
                Timestamp = complaint.ClosedAt.Value,
                EventType = "Closed",
                Description = $"Complaint closed: {complaint.ResolutionNotes}",
                PerformedBy = complaint.UpdatedBy?.ToString()
            });
        }

        // Sort all events by timestamp
        historyDto.Events = historyDto.Events.OrderBy(e => e.Timestamp).ToList();

        return Result<ComplaintHistoryDto>.Success(historyDto);
    }
}
