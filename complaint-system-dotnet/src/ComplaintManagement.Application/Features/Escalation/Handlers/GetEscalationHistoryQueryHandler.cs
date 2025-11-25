using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Escalation.Queries;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Handlers;

public class GetEscalationHistoryQueryHandler : IRequestHandler<GetEscalationHistoryQuery, Result<List<EscalationHistoryDto>>>
{
    private readonly IEscalationService _escalationService;

    public GetEscalationHistoryQueryHandler(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    public async Task<Result<List<EscalationHistoryDto>>> Handle(GetEscalationHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var histories = await _escalationService.GetEscalationHistoryAsync(request.ComplaintId);

            var dtos = histories.Select(h => new EscalationHistoryDto
            {
                Id = h.Id,
                ComplaintId = h.ComplaintId,
                ComplaintNumber = h.Complaint?.ComplaintNumber ?? string.Empty,
                EscalationLevelId = h.EscalationLevelId,
                EscalationMatrixId = h.EscalationMatrixId,
                EscalationMatrixName = h.EscalationMatrix?.Name ?? string.Empty,
                Level = h.Level,
                FromUserId = h.FromUserId,
                FromUserName = h.FromUser?.FullName,
                ToUserId = h.ToUserId,
                ToUserName = h.ToUser?.FullName ?? string.Empty,
                EscalatedBy = h.EscalatedBy,
                EscalatedByName = h.EscalatedByUser?.FullName,
                EscalatedAt = h.EscalatedAt,
                Reason = h.Reason,
                IsAutoEscalation = h.IsAutoEscalation,
                AssignmentStrategy = h.AssignmentStrategy,
                Status = h.Status,
                AcknowledgedAt = h.AcknowledgedAt,
                ResolvedAt = h.ResolvedAt,
                SlaHoursAtEscalation = h.SlaHoursAtEscalation,
                HoursOverdue = h.HoursOverdue,
                EmailSent = h.EmailSent,
                EmailSentAt = h.EmailSentAt,
                Notes = h.Notes
            }).ToList();

            return Result<List<EscalationHistoryDto>>.Success(dtos, $"Retrieved {dtos.Count} escalation history records");
        }
        catch (Exception ex)
        {
            return Result<List<EscalationHistoryDto>>.Failure($"Error retrieving escalation history: {ex.Message}", "Retrieval failed");
        }
    }
}
