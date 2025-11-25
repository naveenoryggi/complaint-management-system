using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Comments;
using ComplaintManagement.Application.Features.Comments.Queries;
using ComplaintManagement.Application.Interfaces.Repositories;
using MediatR;

namespace ComplaintManagement.Application.Features.Comments.Handlers;

public class GetComplaintCommentsQueryHandler : IRequestHandler<GetComplaintCommentsQuery, Result<List<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetComplaintCommentsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<List<CommentDto>>> Handle(GetComplaintCommentsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all comments for the complaint with user details
            var comments = await _unitOfWork.ComplaintComments.FindWithIncludesAsync(
                c => c.ComplaintId == request.ComplaintId,
                cancellationToken,
                c => c.Commenter,
                c => c.Commenter.Branch,
                c => c.Commenter.Department,
                c => c.Commenter.Section
            );

            // Filter out internal comments if not requested
            var filteredComments = comments.Where(c => request.IncludeInternal || !c.IsInternal)
                                          .OrderBy(c => c.CommentedAt)
                                          .ToList();

            // Map to DTOs
            var commentDtos = filteredComments.Select(c => new CommentDto
            {
                Id = c.Id,
                ComplaintId = c.ComplaintId,
                UserId = c.CommentedBy,
                UserName = c.Commenter?.FullName ?? "Unknown",
                UserEmail = c.Commenter?.Email ?? string.Empty,
                UserPhone = c.Commenter?.Phone,
                BranchName = c.Commenter?.Branch?.Name,
                DepartmentName = c.Commenter?.Department?.Name,
                SectionName = c.Commenter?.Section?.Name,
                Comment = c.CommentText,
                IsInternal = c.IsInternal,
                CreatedAt = c.CommentedAt
            }).ToList();

            return Result<List<CommentDto>>.Success(commentDtos, $"Retrieved {commentDtos.Count} comments");
        }
        catch (Exception ex)
        {
            return Result<List<CommentDto>>.Failure($"Error retrieving comments: {ex.Message}", "Query failed");
        }
    }
}
