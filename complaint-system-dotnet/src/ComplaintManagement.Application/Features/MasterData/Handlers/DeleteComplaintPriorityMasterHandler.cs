using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Features.MasterData.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.MasterData.Handlers;

public class DeleteComplaintPriorityMasterHandler : IRequestHandler<DeleteComplaintPriorityMasterCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteComplaintPriorityMasterHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<bool>> Handle(DeleteComplaintPriorityMasterCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var priority = await _unitOfWork.ComplaintPriorityMasters.GetByIdAsync(request.Id, cancellationToken);

            if (priority == null)
            {
                return Result<bool>.Failure("Priority not found");
            }

            // Note: System priorities can be deleted but frontend will show warnings
            // Soft delete
            _unitOfWork.ComplaintPriorityMasters.SoftDelete(priority);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true, "Priority deleted successfully");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting priority: {ex.Message}");
        }
    }
}
