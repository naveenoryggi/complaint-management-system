using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class UpdateComplaintStatusMasterHandler : IRequestHandler<UpdateComplaintStatusMasterCommand, Result<ComplaintStatusMasterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplaintStatusMasterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintStatusMasterDto>> Handle(UpdateComplaintStatusMasterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _unitOfWork.ComplaintStatusMasters.GetByIdAsync(request.Id, cancellationToken);

            if (status == null)
            {
                return Result<ComplaintStatusMasterDto>.Failure("Status not found");
            }

            // Note: System statuses can be edited but frontend shows warnings
            // Update fields
            status.Name = request.Name;
            status.Description = request.Description;
            status.DisplayOrder = request.DisplayOrder;
            status.ColorCode = request.ColorCode;
            status.IconClass = request.IconClass;
            status.IsActive = request.IsActive;
            status.IsFinal = request.IsFinal;
            status.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ComplaintStatusMasters.Update(status);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new ComplaintStatusMasterDto
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

            return Result<ComplaintStatusMasterDto>.Success(dto, "Status updated successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintStatusMasterDto>.Failure($"Error updating status: {ex.Message}");
        }
    }
}
