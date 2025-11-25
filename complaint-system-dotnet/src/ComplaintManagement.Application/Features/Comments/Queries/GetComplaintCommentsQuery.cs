using ComplaintManagement.Application.Common.Models;
using ComplaintManagement.Application.DTOs.Comments;
using MediatR;

namespace ComplaintManagement.Application.Features.Comments.Queries;

public class GetComplaintCommentsQuery : IRequest<Result<List<CommentDto>>>
{
    public Guid ComplaintId { get; set; }
    public bool IncludeInternal { get; set; } = true;
}
