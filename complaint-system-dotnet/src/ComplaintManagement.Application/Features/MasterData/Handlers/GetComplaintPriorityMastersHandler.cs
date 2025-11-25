using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class GetComplaintPriorityMastersHandler : IRequestHandler<GetComplaintPriorityMastersQuery, Result<List<ComplaintPriorityMasterDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintPriorityMastersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ComplaintPriorityMasterDto>>> Handle(GetComplaintPriorityMastersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var priorities = await _unitOfWork.ComplaintPriorityMasters.GetAllAsync(
                request.CompanyId,
                request.IsActive,
                request.IncludeSystem,
                cancellationToken);

            var priorityDtos = priorities.Select(x => new ComplaintPriorityMasterDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                Level = x.Level,
                ColorCode = x.ColorCode,
                IconClass = x.IconClass,
                IsActive = x.IsActive,
                IsSystem = x.IsSystem,
                CompanyId = x.CompanyId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt ?? x.CreatedAt
            }).ToList();

            return Result<List<ComplaintPriorityMasterDto>>.Success(priorityDtos, "Priorities retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<List<ComplaintPriorityMasterDto>>.Failure($"Error retrieving priorities: {ex.Message}");
        }
    }
}
