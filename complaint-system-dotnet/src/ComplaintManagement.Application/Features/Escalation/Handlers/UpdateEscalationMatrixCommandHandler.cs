using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Escalation.Commands;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ComplaintManagement.Application.Features.Escalation.Handlers;

public class UpdateEscalationMatrixCommandHandler : IRequestHandler<UpdateEscalationMatrixCommand, Result<EscalationMatrixDto>>
{
    private readonly IEscalationService _escalationService;
    private readonly ILogger<UpdateEscalationMatrixCommandHandler> _logger;

    public UpdateEscalationMatrixCommandHandler(
        IEscalationService escalationService,
        ILogger<UpdateEscalationMatrixCommandHandler> logger)
    {
        _escalationService = escalationService;
        _logger = logger;
    }

    public async Task<Result<EscalationMatrixDto>> Handle(UpdateEscalationMatrixCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("=== UpdateEscalationMatrixCommandHandler START ===");
        _logger.LogInformation("MatrixId: {MatrixId}, UpdatedBy: {UpdatedBy}", request.MatrixId, request.UpdatedBy);
        _logger.LogInformation("Request.Request.Name: {Name}, LevelCount: {Count}", request.Request.Name, request.Request.EscalationLevels.Count);

        for (int i = 0; i < request.Request.EscalationLevels.Count; i++)
        {
            var level = request.Request.EscalationLevels[i];
            _logger.LogInformation("Level {Index}: Level={Level}, TriggerAfterValue={Value}, TriggerTimeUnit={Unit}, TriggerAfterHours={Hours}",
                i, level.Level, level.TriggerAfterValue, level.TriggerTimeUnit, level.TriggerAfterHours);
        }

        try
        {
            var matrix = await _escalationService.UpdateMatrixAsync(
                request.MatrixId,
                request.Request,
                request.UpdatedBy
            );

            _logger.LogInformation("Service returned matrix with {LevelCount} levels", matrix.EscalationLevels.Count);

            foreach (var l in matrix.EscalationLevels)
            {
                _logger.LogInformation("Returned level {Level}: TriggerAfterHours={Hours}, TriggerAfterValue={Value}, TriggerTimeUnit={Unit}",
                    l.Level, l.TriggerAfterHours, l.TriggerAfterValue, l.TriggerTimeUnit);
            }

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
                    PrimaryContactId = l.PrimaryContactId,
                    PrimaryContactName = l.PrimaryContact?.FullName,
                    SecondaryContactId = l.SecondaryContactId,
                    SecondaryContactName = l.SecondaryContact?.FullName,
                    HrContactId = l.HrContactId,
                    HrContactName = l.HrContact?.FullName,
                    ResourcePoolId = l.ResourcePoolId,
                    ResourcePoolName = l.ResourcePool?.Name,
                    ResourcePoolAssignmentMethod = l.ResourcePoolAssignmentMethod,
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

            return Result<EscalationMatrixDto>.Success(dto, "Escalation matrix updated successfully");
        }
        catch (KeyNotFoundException)
        {
            return Result<EscalationMatrixDto>.Failure("Escalation matrix not found", "Not found");
        }
        catch (Exception ex)
        {
            return Result<EscalationMatrixDto>.Failure($"Error updating escalation matrix: {ex.Message}", "Update failed");
        }
    }
}
