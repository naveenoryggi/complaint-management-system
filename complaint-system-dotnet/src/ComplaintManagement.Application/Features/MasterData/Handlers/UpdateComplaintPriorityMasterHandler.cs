using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class UpdateComplaintPriorityMasterHandler : IRequestHandler<UpdateComplaintPriorityMasterCommand, Result<ComplaintPriorityMasterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateComplaintPriorityMasterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintPriorityMasterDto>> Handle(UpdateComplaintPriorityMasterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var priority = await _unitOfWork.ComplaintPriorityMasters.GetByIdAsync(request.Id, cancellationToken);

            if (priority == null)
            {
                return Result<ComplaintPriorityMasterDto>.Failure("Priority not found");
            }

            // Note: System priorities can be edited but frontend shows warnings
            // Update fields
            priority.Name = request.Name;
            priority.Description = request.Description;
            priority.DisplayOrder = request.DisplayOrder;
            priority.Level = request.Level;
            priority.ColorCode = request.ColorCode;
            priority.IconClass = request.IconClass;
            priority.IsActive = request.IsActive;
            priority.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.ComplaintPriorityMasters.Update(priority);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new ComplaintPriorityMasterDto
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

            return Result<ComplaintPriorityMasterDto>.Success(dto, "Priority updated successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintPriorityMasterDto>.Failure($"Error updating priority: {ex.Message}");
        }
    }
}
