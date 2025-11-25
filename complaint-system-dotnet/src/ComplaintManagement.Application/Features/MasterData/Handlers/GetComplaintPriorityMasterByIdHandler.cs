using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class GetComplaintPriorityMasterByIdHandler : IRequestHandler<GetComplaintPriorityMasterByIdQuery, Result<ComplaintPriorityMasterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintPriorityMasterByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintPriorityMasterDto>> Handle(GetComplaintPriorityMasterByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var priority = await _unitOfWork.ComplaintPriorityMasters.GetByIdAsync(request.Id, cancellationToken);

            if (priority == null)
            {
                return Result<ComplaintPriorityMasterDto>.Failure("Priority not found");
            }

            var priorityDto = new ComplaintPriorityMasterDto
            {
                Id = priority.Id,
                Name = priority.Name,
                Code = priority.Code,
                Description = priority.Description,
                DisplayOrder = priority.DisplayOrder,
                Level = priority.Level,
                ColorCode = priority.ColorCode,
                IconClass = priority.IconClass,
                IsActive = priority.IsActive,
                IsSystem = priority.IsSystem,
                CompanyId = priority.CompanyId,
                CreatedAt = priority.CreatedAt,
                UpdatedAt = priority.UpdatedAt ?? priority.CreatedAt
            };

            return Result<ComplaintPriorityMasterDto>.Success(priorityDto, "Priority retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintPriorityMasterDto>.Failure($"Error retrieving priority: {ex.Message}");
        }
    }
}
