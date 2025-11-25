using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintAttachmentRepository : IRepository<ComplaintAttachment>
{
    Task<IEnumerable<ComplaintAttachment>> GetAttachmentsByComplaintAsync(Guid complaintId, CancellationToken cancellationToken = default);
}
