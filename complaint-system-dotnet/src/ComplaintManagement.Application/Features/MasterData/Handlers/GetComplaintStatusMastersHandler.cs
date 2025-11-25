using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class GetComplaintStatusMastersHandler : IRequestHandler<GetComplaintStatusMastersQuery, Result<List<ComplaintStatusMasterDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintStatusMastersHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<ComplaintStatusMasterDto>>> Handle(GetComplaintStatusMastersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var statuses = await _unitOfWork.ComplaintStatusMasters.GetAllAsync(
                request.CompanyId,
                request.IsActive,
                request.IncludeSystem,
                cancellationToken);

            var statusDtos = statuses.Select(x => new ComplaintStatusMasterDto
            {
                Id = x.Id,
                Name = x.Name,
                Code = x.Code,
                Description = x.Description,
                DisplayOrder = x.DisplayOrder,
                ColorCode = x.ColorCode,
                IconClass = x.IconClass,
                IsActive = x.IsActive,
                IsSystem = x.IsSystem,
                IsFinal = x.IsFinal,
                CompanyId = x.CompanyId,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt ?? x.CreatedAt
            }).ToList();

            return Result<List<ComplaintStatusMasterDto>>.Success(statusDtos, "Statuses retrieved successfully");
        }
        catch (Exception ex)
        {
            return Result<List<ComplaintStatusMasterDto>>.Failure($"Error retrieving statuses: {ex.Message}");
        }
    }
}
