using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Comments;
using ComplaintManagement.Application.Features.Comments.Commands;
using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using MediatR;

namespace ComplaintManagement.Application.Features.Comments.Handlers;

public class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, Result<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CommentDto>> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verify complaint exists
            var complaintExists = await _unitOfWork.Complaints.AnyAsync(c => c.Id == request.ComplaintId, cancellationToken);
            if (!complaintExists)
            {
                return Result<CommentDto>.Failure("Complaint not found", "Not found");
            }

            // Verify user exists
            var user = await _unitOfWork.Users.GetByIdAsync(request.UserId, cancellationToken);
            if (user == null)
            {
                return Result<CommentDto>.Failure("User not found", "Not found");
            }

            // Create comment
            var comment = new ComplaintComment
            {
                Id = Guid.NewGuid(),
                ComplaintId = request.ComplaintId,
                CommentedBy = request.UserId,
                CommentText = request.Comment,
                IsInternal = request.IsInternal,
                CommentedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = request.UserId
            };

            await _unitOfWork.ComplaintComments.AddAsync(comment, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Map to DTO
            var commentDto = new CommentDto
            {
                Id = comment.Id,
                ComplaintId = comment.ComplaintId,
                UserId = comment.CommentedBy,
                UserName = user.FullName,
                Comment = comment.CommentText,
                IsInternal = comment.IsInternal,
                CreatedAt = comment.CommentedAt
            };

            return Result<CommentDto>.Success(commentDto, "Comment added successfully");
        }
        catch (Exception ex)
        {
            return Result<CommentDto>.Failure($"Error adding comment: {ex.Message}", "Operation failed");
        }
    }
}
