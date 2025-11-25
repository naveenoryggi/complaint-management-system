using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.MasterData;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.MasterData;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class CreateComplaintStatusMasterHandler : IRequestHandler<CreateComplaintStatusMasterCommand, Result<ComplaintStatusMasterDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateComplaintStatusMasterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ComplaintStatusMasterDto>> Handle(CreateComplaintStatusMasterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Check if code already exists for this company
            var exists = await _unitOfWork.ComplaintStatusMasters.CodeExistsAsync(
                request.Code,
                request.CompanyId,
                cancellationToken);

            if (exists)
            {
                return Result<ComplaintStatusMasterDto>.Failure("A status with this code already exists");
            }

            var status = new ComplaintStatusMaster
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Code = request.Code.ToUpper(),
                Description = request.Description,
                DisplayOrder = request.DisplayOrder,
                ColorCode = request.ColorCode,
                IconClass = request.IconClass,
                IsActive = request.IsActive,
                IsFinal = request.IsFinal,
                IsSystem = false,  // User-created statuses are never system statuses
                CompanyId = request.CompanyId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ComplaintStatusMasters.AddAsync(status, cancellationToken);
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

            return Result<ComplaintStatusMasterDto>.Success(dto, "Status created successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintStatusMasterDto>.Failure($"Error creating status: {ex.Message}");
        }
    }
}
