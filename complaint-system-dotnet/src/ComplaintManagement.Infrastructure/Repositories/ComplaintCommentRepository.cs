using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class ComplaintCommentRepository : Repository<ComplaintComment>, IComplaintCommentRepository
{
    public ComplaintCommentRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ComplaintComment>> GetCommentsByComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ComplaintId == complaintId && c.ParentCommentId == null)
            .Include(c => c.Commenter)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Commenter)
            .OrderByDescending(c => c.CommentedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ComplaintComment>> GetRepliesAsync(Guid parentCommentId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ParentCommentId == parentCommentId)
            .Include(c => c.Commenter)
            .OrderBy(c => c.CommentedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ComplaintComment?> GetCommentWithRepliesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Commenter)
            .Include(c => c.Replies)
                .ThenInclude(r => r.Commenter)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }
}
