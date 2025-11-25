using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class GetComplaintStatusMasterByIdHandler : IRequestHandler<GetComplaintStatusMasterByIdQuery, Result<ComplaintStatusMasterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintStatusMasterByIdHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintStatusMasterDto>> Handle(GetComplaintStatusMasterByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _unitOfWork.ComplaintStatusMasters.GetByIdAsync(request.Id, cancellationToken);

            if (status == null)
            {
                return Result<ComplaintStatusMasterDto>.Failure("Status not found");
            }

            var statusDto = new ComplaintStatusMasterDto
            {
                Id = status.Id,
                Name = status.Name,
                Code = status.Code,
                Description = status.Description,
                DisplayOrder = status.DisplayOrder,
                ColorCode = status.ColorCode,
                IconClass = status.IconClass,
                IsActive = status.IsActive,
                IsSystem = status.IsSystem,
                IsFinal = status.IsFinal,
                CompanyId = status.CompanyId,
                CreatedAt = status.CreatedAt,
                UpdatedAt = status.UpdatedAt ?? status.CreatedAt
            };

            return Result<ComplaintStatusMasterDto>.Success(statusDto, "Status retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintStatusMasterDto>.Failure($"Error retrieving status: {ex.Message}");
        }
    }
}
