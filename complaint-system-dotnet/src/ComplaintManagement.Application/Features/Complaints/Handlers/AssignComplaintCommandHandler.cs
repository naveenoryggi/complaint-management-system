using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class AssignComplaintCommandHandler : IRequestHandler<AssignComplaintCommand, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;
    private readonly IAutoResponseService _autoResponseService;

    public AssignComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher,
        IAutoResponseService autoResponseService)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
        _autoResponseService = autoResponseService;
    }

    public async Task<Result<ComplaintDto>> Handle(AssignComplaintCommand request, CancellationToken cancellationToken)
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

            // Validate the user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.AssignedToId, cancellationToken);
            if (user == null)
            {
                return Result<ComplaintDto>.Failure("User not found", "Validation failed");
            }

            // Track previous assignment for reassignment notifications
            var previousHandlerId = complaint.AssignedToId;

            // Update assignment
            complaint.AssignedToId = request.AssignedToId;

            // Update status to In Progress if it's still Submitted
            if (complaint.StatusMaster?.Code == "SUBMITTED")
            {
                // Use Code for comparison (case-sensitive, consistent across system)
                // Check for company-specific status first, then fall back to global status (CompanyId = null)
                var inProgressStatus = await _unitOfWork.ComplaintStatusMasters
                    .FirstOrDefaultAsync(s => s.Code == "IN_PROGRESS" && s.CompanyId == complaint.CompanyId, cancellationToken)
                    ?? await _unitOfWork.ComplaintStatusMasters
                    .FirstOrDefaultAsync(s => s.Code == "IN_PROGRESS" && s.CompanyId == null, cancellationToken);

                if (inProgressStatus != null)
                {
                    complaint.StatusMasterId = inProgressStatus.Id;
                }
            }

            // No need to call Update() - entity is already tracked by EF change tracker
            // EF will automatically detect which properties changed and update only those
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Dispatch COMPLAINT_ASSIGNED notification
            try
            {
                await _notificationDispatcher.DispatchEventNotificationsAsync(
                    "COMPLAINT_ASSIGNED",
                    complaint.Id,
                    new Dictionary<string, object>
                    {
                        ["complaintId"] = complaint.Id.ToString(),
                        ["complaintNumber"] = complaint.ComplaintNumber,
                        ["title"] = complaint.Title,
                        ["description"] = complaint.Description,
                        ["categoryName"] = complaint.Category?.Name ?? string.Empty,
                        ["priorityName"] = complaint.PriorityMaster?.Name ?? "Unknown",
                        ["statusName"] = complaint.StatusMaster?.Name ?? "Unknown",
                        ["assignedToName"] = user.FullName,
                        ["assignedToEmail"] = user.Email,
                        ["assignedToPhone"] = user.Phone ?? string.Empty,
                        ["complainantName"] = complaint.Complainant?.FullName ?? string.Empty,
                        ["complainantEmail"] = complaint.Complainant?.Email ?? string.Empty,
                        ["dueDate"] = complaint.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                        ["companyName"] = complaint.Company?.Name ?? string.Empty
                    },
                    complaint.CompanyId,
                    cancellationToken
                );
            }
            catch (Exception)
            {
                // Log notification error but don't fail the assignment
            }

            // Send auto-response for assignment/reassignment
            try
            {
                await _autoResponseService.SendAssignmentAutoResponseAsync(
                    complaint, previousHandlerId, cancellationToken);
            }
            catch (Exception)
            {
                // Log but don't fail the assignment
            }

            // Reload with updated AssignedTo
            complaint = await _unitOfWork.Complaints.GetByIdWithIncludesAsync(
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

            // Map to DTO
            var complaintDto = new ComplaintDto
            {
                Id = complaint!.Id,
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
                Status = complaint.StatusMaster?.Name ?? "Unknown",
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

            return Result<ComplaintDto>.Success(complaintDto, $"Complaint assigned to {user.FullName}");
        }
        catch (Exception ex)
        {
            return Result<ComplaintDto>.Failure($"Error assigning complaint: {ex.Message}", "Assignment failed");
        }
    }
}
