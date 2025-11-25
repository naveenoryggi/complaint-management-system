using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class EscalateComplaintCommandHandler : IRequestHandler<EscalateComplaintCommand, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationDispatcher _notificationDispatcher;
    private const int MaxEscalationLevel = 5;

    public EscalateComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        INotificationDispatcher notificationDispatcher)
    {
        _unitOfWork = unitOfWork;
        _notificationDispatcher = notificationDispatcher;
    }

    public async Task<Result<ComplaintDto>> Handle(EscalateComplaintCommand request, CancellationToken cancellationToken)
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

            // Check if complaint can be escalated
            var currentStatusName = complaint.StatusMaster?.Name ?? "Unknown";
            var cannotEscalate = currentStatusName.Equals("Closed", StringComparison.OrdinalIgnoreCase) ||
                                currentStatusName.Equals("Resolved", StringComparison.OrdinalIgnoreCase);

            if (cannotEscalate)
            {
                return Result<ComplaintDto>.Failure("Cannot escalate a closed or resolved complaint", "Invalid operation");
            }

            if (complaint.CurrentEscalationLevel >= MaxEscalationLevel)
            {
                return Result<ComplaintDto>.Failure($"Complaint is already at maximum escalation level ({MaxEscalationLevel})", "Invalid operation");
            }

            // Get Escalated status master
            // Note: Using ToLower() for case-insensitive comparison as StringComparison is not supported in LINQ-to-SQL
            var escalatedStatus = await _unitOfWork.ComplaintStatusMasters
                .FirstOrDefaultAsync(s => s.Name.ToLower() == "escalated" && s.CompanyId == complaint.CompanyId, cancellationToken);

            if (escalatedStatus == null)
            {
                return Result<ComplaintDto>.Failure("Escalated status not found in master data", "Configuration error");
            }

            // Escalate the complaint
            complaint.CurrentEscalationLevel++;
            complaint.StatusMasterId = escalatedStatus.Id;

            // Clear current assignment to allow reassignment at higher level
            complaint.AssignedToId = null;

            // Extend due date by 24 hours when escalating (standard escalation extension)
            if (complaint.DueDate.HasValue)
            {
                complaint.DueDate = complaint.DueDate.Value.AddHours(24);
            }

            _unitOfWork.Complaints.Update(complaint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Dispatch COMPLAINT_ESCALATED notification
            try
            {
                await _notificationDispatcher.DispatchEventNotificationsAsync(
                    "COMPLAINT_ESCALATED",
                    complaint.Id,
                    new Dictionary<string, object>
                    {
                        ["complaintId"] = complaint.Id.ToString(),
                        ["complaintNumber"] = complaint.ComplaintNumber,
                        ["title"] = complaint.Title,
                        ["description"] = complaint.Description,
                        ["categoryName"] = complaint.Category?.Name ?? string.Empty,
                        ["priorityName"] = complaint.PriorityMaster?.Name ?? "Unknown",
                        ["statusName"] = escalatedStatus.Name,
                        ["escalationLevel"] = complaint.CurrentEscalationLevel.ToString(),
                        ["escalationReason"] = request.Reason,
                        ["complainantName"] = complaint.Complainant?.FullName ?? string.Empty,
                        ["complainantEmail"] = complaint.Complainant?.Email ?? string.Empty,
                        ["escalatedTo"] = "Management",  // Will be assigned to appropriate level manager
                        ["dueDate"] = complaint.DueDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? string.Empty,
                        ["companyName"] = complaint.Company?.Name ?? string.Empty
                    },
                    complaint.CompanyId,
                    cancellationToken
                );
            }
            catch (Exception)
            {
                // Log notification error but don't fail the escalation
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
                Status = escalatedStatus.Name,
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

            return Result<ComplaintDto>.Success(complaintDto, $"Complaint escalated to level {complaint.CurrentEscalationLevel}");
        }
        catch (Exception ex)
        {
            return Result<ComplaintDto>.Failure($"Error escalating complaint: {ex.Message}", "Escalation failed");
        }
    }
}
