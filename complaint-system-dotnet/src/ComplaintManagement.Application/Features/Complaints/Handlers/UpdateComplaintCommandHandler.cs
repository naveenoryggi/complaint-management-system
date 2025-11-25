using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Application.Interfaces.Services;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class UpdateComplaintCommandHandler : IRequestHandler<UpdateComplaintCommand, Result<ComplaintDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAutoResponseService _autoResponseService;

    public UpdateComplaintCommandHandler(
        IUnitOfWork unitOfWork,
        IAutoResponseService autoResponseService)
    {
        _unitOfWork = unitOfWork;
        _autoResponseService = autoResponseService;
    }

    public async Task<Result<ComplaintDto>> Handle(UpdateComplaintCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Get the complaint
            var complaint = await _unitOfWork.Complaints.GetByIdWithIncludesAsync(
                request.Id,
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

            // Validate category exists
            var categoryExists = await _unitOfWork.ComplaintCategories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
            if (!categoryExists)
            {
                return Result<ComplaintDto>.Failure("Invalid category", "Validation failed");
            }

            // Validate assigned user if provided
            if (request.AssignedToId.HasValue)
            {
                var userExists = await _unitOfWork.Users.AnyAsync(u => u.Id == request.AssignedToId.Value, cancellationToken);
                if (!userExists)
                {
                    return Result<ComplaintDto>.Failure("Invalid assigned user", "Validation failed");
                }
            }

            // Track status change for auto-response
            string? oldStatusName = complaint.StatusMaster?.Name;
            string? newStatusName = null;
            bool statusChanged = false;

            // Track resolution for auto-response
            bool wasResolved = complaint.ResolvedAt.HasValue;

            // Update complaint properties
            complaint.Title = request.Title;
            complaint.Description = request.Description;
            complaint.CategoryId = request.CategoryId;
            complaint.PriorityMasterId = request.PriorityMasterId;
            complaint.Tags = request.Tags;

            if (request.StatusMasterId.HasValue && request.StatusMasterId.Value != complaint.StatusMasterId)
            {
                var newStatus = await _unitOfWork.Repository<Domain.Entities.MasterData.ComplaintStatusMaster>()
                    .GetByIdAsync(request.StatusMasterId.Value, cancellationToken);

                if (newStatus != null)
                {
                    complaint.StatusMasterId = request.StatusMasterId.Value;
                    newStatusName = newStatus.Name;
                    statusChanged = true;

                    // Auto-set ResolvedAt when status changes to "Resolved"
                    if (newStatus.Name.Equals("Resolved", StringComparison.OrdinalIgnoreCase) && !complaint.ResolvedAt.HasValue)
                    {
                        complaint.ResolvedAt = DateTime.UtcNow;
                    }
                }
            }

            if (request.AssignedToId.HasValue)
            {
                complaint.AssignedToId = request.AssignedToId.Value;
            }

            if (!string.IsNullOrWhiteSpace(request.ResolutionNotes))
            {
                complaint.ResolutionNotes = request.ResolutionNotes;
            }

            _unitOfWork.Complaints.Update(complaint);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send auto-response for status change
            if (statusChanged && !string.IsNullOrEmpty(oldStatusName) && !string.IsNullOrEmpty(newStatusName))
            {
                try
                {
                    await _autoResponseService.SendStatusChangeAutoResponseAsync(
                        complaint, oldStatusName, newStatusName, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the update
                    // Auto-response failures should not block the main operation
                }
            }

            // Send auto-response for resolution
            if (!wasResolved && complaint.ResolvedAt.HasValue)
            {
                try
                {
                    await _autoResponseService.SendResolutionAutoResponseAsync(complaint, cancellationToken);
                }
                catch (Exception ex)
                {
                    // Log but don't fail the update
                }
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

            return Result<ComplaintDto>.Success(complaintDto, "Complaint updated successfully");
        }
        catch (Exception ex)
        {
            return Result<ComplaintDto>.Failure($"Error updating complaint: {ex.Message}", "Update failed");
        }
    }
}
