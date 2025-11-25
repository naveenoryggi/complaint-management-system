using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Escalation.Commands;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Handlers;

public class AddEscalationLevelCommandHandler : IRequestHandler<AddEscalationLevelCommand, Result<EscalationLevelDto>>
{
    private readonly IEscalationService _escalationService;

    public AddEscalationLevelCommandHandler(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    public async Task<Result<EscalationLevelDto>> Handle(AddEscalationLevelCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var level = await _escalationService.AddLevelAsync(
                request.MatrixId,
                request.Request,
                request.CreatedBy
            );

            var dto = new EscalationLevelDto
            {
                Id = level.Id,
                EscalationMatrixId = level.EscalationMatrixId,
                Level = level.Level,
                Name = level.Name,
                Description = level.Description,
                TriggerAfterHours = level.TriggerAfterHours,
                TriggerAfterValue = level.TriggerAfterValue,
                TriggerTimeUnit = level.TriggerTimeUnit,
                AssignmentStrategy = level.AssignmentStrategy,
                IsActive = level.IsActive,
                CreatedAt = level.CreatedAt
            };

            return Result<EscalationLevelDto>.Success(dto, "Escalation level added successfully");
        }
        catch (KeyNotFoundException)
        {
            return Result<EscalationLevelDto>.Failure("Escalation matrix not found", "Not found");
        }
        catch (Exception ex)
        {
            return Result<EscalationLevelDto>.Failure($"Error adding escalation level: {ex.Message}", "Addition failed");
        }
    }
}
