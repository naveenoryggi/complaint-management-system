using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class CloseComplaintCommandHandler : IRequestHandler<CloseComplaintCommand, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;

    public CloseComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<Result<ComplaintDto>> Handle(CloseComplaintCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the complaint
            var complaint = await _unitOfWork.Complaints.GetByIdWithIncludesAsync(
                request.ComplaintId,
                cancellationToken,
                c => c.Category,
                c => c.Complainant,
                c => c.Company,
                c => c.Branch,
                c => c.Department,
                c => c.AssignedTo,
                c => c.StatusMaster,
                c => c.PriorityMaster,
                c => c.Comments,
                c => c.Attachments
            );

            if (complaint == null)
            {
                return Result<ComplaintDto>.Failure("Complaint not found", "Not found");
            }

            // Check if complaint is already closed
            var currentStatusCode = complaint.StatusMaster?.Code ?? "Unknown";
            if (currentStatusCode == "CLOSED")
            {
                return Result<ComplaintDto>.Failure("Complaint is already closed", "Invalid operation");
            }

            // Get Closed status master - check company-specific first, then fall back to global
            var closedStatus = await _unitOfWork.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Code == "CLOSED" && s.CompanyId == complaint.CompanyId, cancellationToken)
                ?? await _unitOfWork.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Code == "CLOSED" && s.CompanyId == null, cancellationToken);

            if (closedStatus == null)
            {
                return Result<ComplaintDto>.Failure("Closed status not found in master data", "Configuration error");
            }

            // Update complaint status to Closed
            complaint.StatusMasterId = closedStatus.Id;
            complaint.ResolvedAt = DateTime.UtcNow;
            complaint.ClosedAt = DateTime.UtcNow;
            complaint.ResolutionNotes = request.ResolutionNotes;

            _unitOfWork.Complaints.Update(complaint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Dispatch COMPLAINT_CLOSED notification
            try
            {
                await _notificationDispatcher.DispatchEventNotificationsAsync(
                    "COMPLAINT_CLOSED",
                    complaint.Id,
                    new Dictionary<string, object>
                    {
                        ["complaintId"] = complaint.Id.ToString(),
                        ["complaintNumber"] = complaint.ComplaintNumber,
                        ["title"] = complaint.Title,
                        ["description"] = complaint.Description,
                        ["categoryName"] = complaint.Category?.Name ?? string.Empty,
                        ["priorityName"] = complaint.PriorityMaster?.Name ?? "Unknown",
                        ["statusName"] = closedStatus.Name,
                        ["complainantName"] = complaint.Complainant?.FullName ?? string.Empty,
                        ["complainantEmail"] = complaint.Complainant?.Email ?? string.Empty,
                        ["closedBy"] = complaint.AssignedTo?.FullName ?? "System",
                        ["resolution"] = request.ResolutionNotes ?? string.Empty,
                        ["closedDate"] = complaint.ClosedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["companyName"] = complaint.Company?.Name ?? string.Empty
                    },
                    complaint.CompanyId,
                    cancellationToken
                );
            }
            catch (Exception)
            {
                // Log notification error but don't fail the closure
            }

            // Map to DTO
            var complaintDto = new ComplaintDto
            {
                Id = complaint.Id,
                ComplaintNumber = complaint.ComplaintNumber,
                Title = complaint.Title,
                Description = complaint.Description,
                CategoryId = complaint.CategoryId,
                CategoryName = complaint.Category?.Name ?? string.Empty,
                ComplainantId = complaint.ComplainantId,
                ComplainantName = complaint.Complainant?.FullName ?? string.Empty,
                ComplainantEmail = complaint.Complainant?.Email ?? string.Empty,
                CompanyId = complaint.CompanyId,
                CompanyName = complaint.Company?.Name ?? string.Empty,
                BranchId = complaint.BranchId,
                BranchName = complaint.Branch?.Name,
                DepartmentId = complaint.DepartmentId,
                DepartmentName = complaint.Department?.Name,
                Status = closedStatus.Name,
                StatusId = complaint.StatusMasterId,
                Priority = complaint.PriorityMaster?.Name ?? "Unknown",
                PriorityId = complaint.PriorityMasterId,
                CurrentEscalationLevel = complaint.CurrentEscalationLevel,
                AssignedToId = complaint.AssignedToId,
                AssignedToName = complaint.AssignedTo?.FullName,
                SubmittedAt = complaint.SubmittedAt,
                DueDate = complaint.DueDate,
                ResolvedAt = complaint.ResolvedAt,
                ClosedAt = complaint.ClosedAt,
                ResolutionNotes = complaint.ResolutionNotes,
                IsAnonymous = complaint.IsAnonymous,
                Tags = complaint.Tags,
                CommentCount = complaint.Comments?.Count ?? 0,
                AttachmentCount = complaint.Attachments?.Count ?? 0
            };

            return Result<ComplaintDto>.Success(complaintDto, "Complaint closed successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintDto>.Failure($"Error closing complaint: {ex.Message}", "Close failed");
        }
    }
}
