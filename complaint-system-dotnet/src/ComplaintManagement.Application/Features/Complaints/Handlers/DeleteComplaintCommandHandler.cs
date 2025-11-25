using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class DeleteComplaintCommandHandler : IRequestHandler<DeleteComplaintCommand, Result>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteComplaintCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteComplaintCommand request, CancellationToken cancellationToken)
    {
        // Get complaint
        var complaint = await _unitOfWork.Complaints.GetByIdAsync(request.ComplaintId, cancellationToken);
        if (complaint == null)
        {
            return Result.Failure("Complaint not found", "COMPLAINT_NOT_FOUND");
        }

        // Check if already deleted
        if (complaint.IsDeleted)
        {
            return Result.Failure("Complaint is already deleted", "COMPLAINT_ALREADY_DELETED");
        }

        // Soft delete the complaint
        complaint.IsDeleted = true;
        complaint.DeletedAt = DateTime.UtcNow;
        complaint.DeletedBy = request.DeletedById;

        _unitOfWork.Complaints.Update(complaint);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success("Complaint deleted successfully");
    }
}
