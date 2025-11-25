using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class CreateComplaintPriorityMasterHandler : IRequestHandler<CreateComplaintPriorityMasterCommand, Result<ComplaintPriorityMasterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateComplaintPriorityMasterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintPriorityMasterDto>> Handle(CreateComplaintPriorityMasterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if code already exists for this company
            var exists = await _unitOfWork.ComplaintPriorityMasters.CodeExistsAsync(
                request.Code,
                request.CompanyId,
                cancellationToken);

            if (exists)
            {
                return Result<ComplaintPriorityMasterDto>.Failure("A priority with this code already exists");
            }

            var priority = new ComplaintPriorityMaster
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code.ToUpper(),
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                Level = request.Level,
                ColorCode = request.ColorCode,
                IconClass = request.IconClass,
                IsActive = request.IsActive,
                IsSystem = false,  // User-created priorities are never system priorities
                CompanyId = request.CompanyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ComplaintPriorityMasters.AddAsync(priority, cancellationToken);
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

            return Result<ComplaintPriorityMasterDto>.Success(dto, "Priority created successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintPriorityMasterDto>.Failure($"Error creating priority: {ex.Message}");
        }
    }
}
