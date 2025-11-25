using ComplaintManagement.Domain.Entities.Complaints;

namespace ComplaintManagement.Application.Interfaces.Repositories;

public interface IComplaintRepository : IRepository<Complaint>
{
    Task<IEnumerable<Complaint>> GetComplaintsByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Complaint>> GetComplaintsByAssigneeAsync(Guid assigneeId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Complaint>> GetComplaintsByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Complaint>> GetComplaintsByStatusAsync(Guid statusMasterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Complaint>> GetComplaintsByPriorityAsync(Guid priorityMasterId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Complaint>> GetOverdueComplaintsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<Complaint>> GetComplaintsForEscalationAsync(CancellationToken cancellationToken = default);
    Task<Complaint?> GetComplaintWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<string> GenerateComplaintNumberAsync(CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, int>> GetComplaintCountByStatusAsync(Guid? companyId = null, CancellationToken cancellationToken = default);
}
