using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class ReopenComplaintCommandHandler : IRequestHandler<ReopenComplaintCommand, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;

    public ReopenComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<Result<ComplaintDto>> Handle(ReopenComplaintCommand request, CancellationToken cancellationToken)
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

            // Check if complaint can be reopened (only Closed or Resolved complaints can be reopened)
            var currentStatusName = complaint.StatusMaster?.Name ?? "Unknown";
            var canReopen = currentStatusName.Equals("Closed", StringComparison.OrdinalIgnoreCase) ||
                           currentStatusName.Equals("Resolved", StringComparison.OrdinalIgnoreCase);

            if (!canReopen)
            {
                return Result<ComplaintDto>.Failure(
                    $"Cannot reopen a complaint with status '{currentStatusName}'. Only Closed or Resolved complaints can be reopened.",
                    "Invalid operation");
            }

            // Get the user who is reopening
            var user = await _unitOfWork.Users.GetByIdAsync(request.ReopenedById, cancellationToken);
            if (user == null)
            {
                return Result<ComplaintDto>.Failure("User not found", "Invalid user");
            }

            // Store previous status for audit
            var previousStatus = currentStatusName;

            // Get Reopened status master
            var reopenedStatus = await _unitOfWork.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Name.Equals("Reopened", StringComparison.OrdinalIgnoreCase) && s.CompanyId == complaint.CompanyId, cancellationToken);

            if (reopenedStatus == null)
            {
                return Result<ComplaintDto>.Failure("Reopened status not found in master data", "Configuration error");
            }

            // Update complaint status to Reopened
            complaint.StatusMasterId = reopenedStatus.Id;

            // Clear resolved/closed timestamps since complaint is being reopened
            complaint.ResolvedAt = null;
            complaint.ClosedAt = null;
            complaint.ResolutionNotes = null; // Clear resolution notes since we're reopening

            // Set new due date to 48 hours from now (standard reopened complaint SLA)
            complaint.DueDate = DateTime.UtcNow.AddHours(48);

            _unitOfWork.Complaints.Update(complaint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Dispatch COMPLAINT_REOPENED notification
            try
            {
                await _notificationDispatcher.DispatchEventNotificationsAsync(
                    "COMPLAINT_REOPENED",
                    complaint.Id,
                    new Dictionary<string, object>
                    {
                        ["complaintId"] = complaint.Id.ToString(),
                        ["complaintNumber"] = complaint.ComplaintNumber,
                        ["title"] = complaint.Title,
                        ["description"] = complaint.Description,
                        ["categoryName"] = complaint.Category?.Name ?? string.Empty,
                        ["priorityName"] = complaint.PriorityMaster?.Name ?? "Unknown",
                        ["statusName"] = reopenedStatus.Name,
                        ["previousStatus"] = previousStatus,
                        ["complainantName"] = complaint.Complainant?.FullName ?? string.Empty,
                        ["complainantEmail"] = complaint.Complainant?.Email ?? string.Empty,
                        ["reopenedByName"] = user.FullName,
                        ["reopenedByEmail"] = user.Email,
                        ["reopenReason"] = request.Reason,
                        ["reopenedDate"] = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["newDueDate"] = complaint.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                        ["companyName"] = complaint.Company?.Name ?? string.Empty
                    },
                    complaint.CompanyId,
                    cancellationToken
                );
            }
            catch (Exception)
            {
                // Log notification error but don't fail the reopen operation
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
                Status = reopenedStatus.Name,
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

            return Result<ComplaintDto>.Success(complaintDto, "Complaint reopened successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintDto>.Failure($"Error reopening complaint: {ex.Message}", "Reopen failed");
        }
    }
}
