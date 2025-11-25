using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Complaints;
using ComplaintManagement.Application.Features.Complaints.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Complaints.Handlers;

public class GetComplaintsQueryHandler : IRequestHandler<GetComplaintsQuery, Result<PagedResult<ComplaintDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PagedResult<ComplaintDto>>> Handle(GetComplaintsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all complaints with includes (for MVP - can be optimized later with database-level filtering)
            // CRITICAL: Filter out soft-deleted records
            var allComplaints = (await _unitOfWork.Complaints.GetAllWithIncludesAsync(
                cancellationToken,
                c => c.Category,
                c => c.Complainant,
                c => c.Complainant.Branch,
                c => c.Complainant.Department,
                c => c.Complainant.Section,
                c => c.Complainant.Manager,
                c => c.Company,
                c => c.Branch,
                c => c.Department,
                c => c.Section,
                c => c.AssignedTo,
                c => c.StatusMaster,
                c => c.PriorityMaster,
                c => c.Comments,
                c => c.Attachments
            )).Where(c => !c.IsDeleted).AsQueryable();

            // Apply filters
            if (request.StatusMasterId.HasValue)
            {
                allComplaints = allComplaints.Where(c => c.StatusMasterId == request.StatusMasterId.Value);
            }

            if (request.PriorityMasterId.HasValue)
            {
                allComplaints = allComplaints.Where(c => c.PriorityMasterId == request.PriorityMasterId.Value);
            }

            if (request.CompanyId.HasValue)
            {
                allComplaints = allComplaints.Where(c => c.CompanyId == request.CompanyId.Value);
            }

            if (request.AssignedToId.HasValue)
            {
                allComplaints = allComplaints.Where(c => c.AssignedToId == request.AssignedToId.Value);
            }

            if (request.ComplainantId.HasValue)
            {
                allComplaints = allComplaints.Where(c => c.ComplainantId == request.ComplainantId.Value);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                allComplaints = allComplaints.Where(c =>
                    c.Title.ToLower().Contains(searchTerm) ||
                    c.Description.ToLower().Contains(searchTerm) ||
                    c.ComplaintNumber.ToLower().Contains(searchTerm));
            }

            // Order and paginate
            var totalCount = allComplaints.Count();
            var complaints = allComplaints
                .OrderByDescending(c => c.SubmittedAt)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            // Map to DTOs
            var complaintDtos = complaints.Select(c => new ComplaintDto
            {
                Id = c.Id,
                ComplaintNumber = c.ComplaintNumber,
                Title = c.Title,
                Description = c.Description,
                CategoryId = c.CategoryId,
                CategoryName = c.Category?.Name ?? string.Empty,
                ComplainantId = c.ComplainantId,
                ComplainantName = c.Complainant?.FullName ?? string.Empty,
                ComplainantEmail = c.Complainant?.Email ?? string.Empty,
                CompanyId = c.CompanyId,
                CompanyName = c.Company?.Name ?? string.Empty,
                BranchId = c.BranchId,
                BranchName = c.Branch?.Name,
                DepartmentId = c.DepartmentId,
                DepartmentName = c.Department?.Name,
                SectionId = c.SectionId,
                SectionName = c.Section?.Name,

                // Complainant organizational info
                ComplainantBranchId = c.Complainant?.BranchId,
                ComplainantBranchName = c.Complainant?.Branch?.Name,
                ComplainantDepartmentId = c.Complainant?.DepartmentId,
                ComplainantDepartmentName = c.Complainant?.Department?.Name,
                ComplainantSectionId = c.Complainant?.SectionId,
                ComplainantSectionName = c.Complainant?.Section?.Name,
                ComplainantEmployeeCode = c.Complainant?.EmployeeCode,

                // Contact information
                ContactEmail = c.ContactEmail,
                ContactPhone = c.ContactPhone,
                AlternatePhone = c.AlternatePhone,
                PreferredContactMethod = c.PreferredContactMethod,

                // Manager details
                ComplainantManagerId = c.Complainant?.ManagerId,
                ComplainantManagerName = c.Complainant?.Manager?.FullName,
                ComplainantManagerEmail = c.Complainant?.Manager?.Email,
                ComplainantManagerPhone = c.Complainant?.Manager?.Phone,

                Status = c.StatusMaster != null ? c.StatusMaster.Name : "Unknown",
                StatusId = c.StatusMasterId,
                Priority = c.PriorityMaster != null ? c.PriorityMaster.Name : "Unknown",
                PriorityId = c.PriorityMasterId,
                CurrentEscalationLevel = c.CurrentEscalationLevel,
                AssignedToId = c.AssignedToId,
                AssignedToName = c.AssignedTo?.FullName,
                SubmittedAt = c.SubmittedAt,
                DueDate = c.DueDate,
                ResolvedAt = c.ResolvedAt,
                ClosedAt = c.ClosedAt,
                ResolutionNotes = c.ResolutionNotes,
                IsAnonymous = c.IsAnonymous,
                Tags = c.Tags,
                CommentCount = c.Comments?.Count ?? 0,
                AttachmentCount = c.Attachments?.Count ?? 0
            }).ToList();

            var pagedResult = new PagedResult<ComplaintDto>
            {
                Items = complaintDtos,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalCount = totalCount
            };

            return Result<PagedResult<ComplaintDto>>.Success(pagedResult, $"Retrieved {complaintDtos.Count} complaints");
        }
        catch (Exception ex)
        {
            return Result<PagedResult<ComplaintDto>>.Failure($"Error retrieving complaints: {ex.Message}", "Query failed");
        }
    }
}
