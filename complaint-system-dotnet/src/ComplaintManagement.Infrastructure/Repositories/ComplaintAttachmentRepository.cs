using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class ComplaintAttachmentRepository : Repository<ComplaintAttachment>, IComplaintAttachmentRepository
{
    public ComplaintAttachmentRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ComplaintAttachment>> GetAttachmentsByComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(a => a.ComplaintId == complaintId)
            .Include(a => a.Uploader)
            .OrderBy(a => a.UploadedAt)
            .ToListAsync(cancellationToken);
    }
}
