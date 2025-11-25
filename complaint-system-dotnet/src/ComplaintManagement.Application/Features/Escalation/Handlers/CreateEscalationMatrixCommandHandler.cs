using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Escalation.Commands;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Handlers;

public class CreateEscalationMatrixCommandHandler : IRequestHandler<CreateEscalationMatrixCommand, Result<EscalationMatrixDto>>
{
    private readonly IEscalationService _escalationService;

    public CreateEscalationMatrixCommandHandler(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    public async Task<Result<EscalationMatrixDto>> Handle(CreateEscalationMatrixCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var matrix = await _escalationService.CreateMatrixAsync(
                request.Request,
                request.CompanyId,
                request.CreatedBy
            );

            var dto = new EscalationMatrixDto
            {
                Id = matrix.Id,
                Name = matrix.Name,
                Description = matrix.Description,
                CompanyId = matrix.CompanyId,
                CategoryId = matrix.CategoryId,
                CategoryName = matrix.Category?.Name,
                BranchId = matrix.BranchId,
                BranchName = matrix.Branch?.Name,
                DepartmentId = matrix.DepartmentId,
                DepartmentName = matrix.Department?.Name,
                IsActive = matrix.IsActive,
                Priority = matrix.Priority,
                EnableAutoEscalation = matrix.EnableAutoEscalation,
                SendEmailNotifications = matrix.SendEmailNotifications,
                EscalationLevels = matrix.EscalationLevels.Select(l => new EscalationLevelDto
                {
                    Id = l.Id,
                    EscalationMatrixId = l.EscalationMatrixId,
                    Level = l.Level,
                    Name = l.Name,
                    Description = l.Description,
                    TriggerAfterHours = l.TriggerAfterHours,
                    TriggerAfterValue = l.TriggerAfterValue,
                    TriggerTimeUnit = l.TriggerTimeUnit,
                    AssignmentStrategy = l.AssignmentStrategy,
                    AssignToUserId = l.AssignToUserId,
                    AssignToUserName = l.AssignToUser?.FullName,
                    AssignToRole = l.AssignToRole,
                    AssignToUserIds = l.AssignToUserIds,
                    IsActive = l.IsActive,
                    SendNotification = l.SendNotification,
                    EmailTemplateId = l.EmailTemplateId,
                    NotifyPreviousHandler = l.NotifyPreviousHandler,
                    EscalationMessage = l.EscalationMessage,
                    CreatedAt = l.CreatedAt,
                    UpdatedAt = l.UpdatedAt
                }).ToList(),
                CreatedAt = matrix.CreatedAt,
                UpdatedAt = matrix.UpdatedAt
            };

            return Result<EscalationMatrixDto>.Success(dto, "Escalation matrix created successfully");
        }
        catch (Exception ex)
        {
            return Result<EscalationMatrixDto>.Failure($"Error creating escalation matrix: {ex.Message}", "Creation failed");
        }
    }
}
