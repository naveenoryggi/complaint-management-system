using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class DeleteComplaintStatusMasterHandler : IRequestHandler<DeleteComplaintStatusMasterCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteComplaintStatusMasterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteComplaintStatusMasterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var status = await _unitOfWork.ComplaintStatusMasters.GetByIdAsync(request.Id, cancellationToken);

            if (status == null)
            {
                return Result<bool>.Failure("Status not found");
            }

            // Note: System statuses can be deleted but frontend shows warnings
            // Soft delete
            _unitOfWork.ComplaintStatusMasters.SoftDelete(status);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Status deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting status: {ex.Message}");
        }
    }
}
