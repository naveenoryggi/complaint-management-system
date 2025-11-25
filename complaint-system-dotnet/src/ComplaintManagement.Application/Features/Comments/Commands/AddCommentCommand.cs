using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Comments;
using MediatR;

namespace ComplaintManagement.Application.Features.Comments.Commands;

public class AddCommentCommand : IRequest<Result<CommentDto>>
{
    public Guid ComplaintId { get; set; }
    public Guid UserId { get; set; }
    public string Comment { get; set; } = string.Empty;
    public bool IsInternal { get; set; }
}
