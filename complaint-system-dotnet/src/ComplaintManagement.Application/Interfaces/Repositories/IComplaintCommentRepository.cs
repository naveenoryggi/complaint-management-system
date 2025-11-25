using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintCommentRepository : IRepository<ComplaintComment>
{
    Task<IEnumerable<ComplaintComment>> GetCommentsByComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ComplaintComment>> GetRepliesAsync(Guid parentCommentId, CancellationToken cancellationToken = default);
    Task<ComplaintComment?> GetCommentWithRepliesAsync(Guid id, CancellationToken cancellationToken = default);
}
