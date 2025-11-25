using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Escalation;
using ComplaintManagement.Application.Features.Escalation.Queries;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Escalation.Handlers;

public class GetEscalationMatricesQueryHandler : IRequestHandler<GetEscalationMatricesQuery, Result<List<EscalationMatrixDto>>>
{
    private readonly IEscalationService _escalationService;

    public GetEscalationMatricesQueryHandler(IEscalationService escalationService)
    {
        _escalationService = escalationService;
    }

    public async Task<Result<List<EscalationMatrixDto>>> Handle(GetEscalationMatricesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var matrices = await _escalationService.GetMatricesByCompanyAsync(request.CompanyId);

            var dtos = matrices.Select(matrix => new EscalationMatrixDto
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
                    Level = l.Level,
                    Name = l.Name,
                    TriggerAfterHours = l.TriggerAfterHours,
                    TriggerAfterValue = l.TriggerAfterValue,
                    TriggerTimeUnit = l.TriggerTimeUnit,
                    AssignmentStrategy = l.AssignmentStrategy,
                    IsActive = l.IsActive
                }).ToList(),
                CreatedAt = matrix.CreatedAt,
                UpdatedAt = matrix.UpdatedAt
            }).ToList();

            return Result<List<EscalationMatrixDto>>.Success(dtos, $"Retrieved {dtos.Count} escalation matrices");
        }
        catch (Exception ex)
        {
            return Result<List<EscalationMatrixDto>>.Failure($"Error retrieving escalation matrices: {ex.Message}", "Retrieval failed");
        }
    }
}
