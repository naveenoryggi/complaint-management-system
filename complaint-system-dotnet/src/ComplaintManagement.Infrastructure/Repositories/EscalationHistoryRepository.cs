using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Escalation;
using ComplaintManagement.Domain.Enums;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class EscalationHistoryRepository : Repository<EscalationHistory>, IEscalationHistoryRepository
{
    public EscalationHistoryRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<List<EscalationHistory>> GetByComplaintIdAsync(Guid complaintId)
    {
        return await _dbSet
            .Where(eh => eh.ComplaintId == complaintId)
            .Include(eh => eh.FromUser)
            .Include(eh => eh.ToUser)
            .Include(eh => eh.EscalatedByUser)
            .Include(eh => eh.EscalationMatrix)
            .Include(eh => eh.EscalationLevel)
            .OrderByDescending(eh => eh.EscalatedAt)
            .ToListAsync();
    }

    public async Task<EscalationHistory?> GetCurrentEscalationAsync(Guid complaintId)
    {
        return await _dbSet
            .Where(eh => eh.ComplaintId == complaintId)
            .Include(eh => eh.FromUser)
            .Include(eh => eh.ToUser)
            .Include(eh => eh.EscalationMatrix)
            .Include(eh => eh.EscalationLevel)
            .OrderByDescending(eh => eh.Level)
            .ThenByDescending(eh => eh.EscalatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<List<EscalationHistory>> GetPendingEscalationsAsync(Guid userId)
    {
        return await _dbSet
            .Where(eh => eh.ToUserId == userId &&
                        eh.Status == EscalationStatus.Triggered)
            .Include(eh => eh.Complaint)
                .ThenInclude(c => c.Category)
            .Include(eh => eh.FromUser)
            .Include(eh => eh.EscalationMatrix)
            .OrderByDescending(eh => eh.EscalatedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<string, int>> GetEscalationStatsAsync(Guid userId, DateTime? fromDate = null)
    {
        var query = _dbSet.Where(eh => eh.ToUserId == userId);

        if (fromDate.HasValue)
        {
            query = query.Where(eh => eh.EscalatedAt >= fromDate.Value);
        }

        var triggered = await query.CountAsync(eh => eh.Status == EscalationStatus.Triggered);
        var acknowledged = await query.CountAsync(eh => eh.Status == EscalationStatus.Acknowledged);
        var resolved = await query.CountAsync(eh => eh.Status == EscalationStatus.Resolved);

        return new Dictionary<string, int>
        {
            { "Triggered", triggered },
            { "Acknowledged", acknowledged },
            { "Resolved", resolved },
            { "Total", triggered + acknowledged + resolved }
        };
    }
}
