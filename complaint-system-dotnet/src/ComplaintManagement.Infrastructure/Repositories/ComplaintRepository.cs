using ComplaintManagement.Application.Interfaces.Repositories;
using ComplaintManagement.Domain.Entities.Complaints;
using ComplaintManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ComplaintManagement.Infrastructure.Repositories;

public class ComplaintRepository : Repository<Complaint>, IComplaintRepository
{
    public ComplaintRepository(ComplaintDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Complaint>> GetComplaintsByUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.ComplainantId == userId)
            .Include(c => c.Category)
            .Include(c => c.AssignedTo)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Complaint>> GetComplaintsByAssigneeAsync(Guid assigneeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.AssignedToId == assigneeId)
            .Include(c => c.Category)
            .Include(c => c.Complainant)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Complaint>> GetComplaintsByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.CompanyId == companyId)
            .Include(c => c.Category)
            .Include(c => c.Complainant)
            .Include(c => c.AssignedTo)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Complaint>> GetComplaintsByStatusAsync(Guid statusMasterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.StatusMasterId == statusMasterId)
            .Include(c => c.Category)
            .Include(c => c.Complainant)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Complaint>> GetComplaintsByPriorityAsync(Guid priorityMasterId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.PriorityMasterId == priorityMasterId)
            .Include(c => c.Category)
            .Include(c => c.Complainant)
            .OrderByDescending(c => c.SubmittedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Complaint>> GetOverdueComplaintsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(c => c.StatusMaster)
            .Where(c => c.DueDate.HasValue && c.DueDate.Value < now && !c.StatusMaster.IsFinal)
            .Include(c => c.Category)
            .Include(c => c.Complainant)
            .Include(c => c.AssignedTo)
            .OrderBy(c => c.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Complaint>> GetComplaintsForEscalationAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _dbSet
            .Include(c => c.StatusMaster)
            .Where(c => c.DueDate.HasValue &&
                       c.DueDate.Value < now &&
                       !c.StatusMaster.IsFinal &&
                       c.CurrentEscalationLevel < 5)
            .Include(c => c.Category)
            .Include(c => c.Complainant)
            .Include(c => c.AssignedTo)
            .OrderBy(c => c.DueDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Complaint?> GetComplaintWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(c => c.Category)
            .Include(c => c.Complainant)
                .ThenInclude(u => u.Branch)
            .Include(c => c.Complainant)
                .ThenInclude(u => u.Department)
            .Include(c => c.Complainant)
                .ThenInclude(u => u.Section)
            .Include(c => c.Complainant)
                .ThenInclude(u => u.Manager)
            .Include(c => c.AssignedTo)
            .Include(c => c.Company)
            .Include(c => c.Branch)
            .Include(c => c.Department)
            .Include(c => c.Section)
            .Include(c => c.StatusMaster)
            .Include(c => c.PriorityMaster)
            .Include(c => c.Comments.OrderByDescending(cm => cm.CommentedAt))
                .ThenInclude(cm => cm.Commenter)
            .Include(c => c.Attachments)
                .ThenInclude(a => a.Uploader)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<string> GenerateComplaintNumberAsync(CancellationToken cancellationToken = default)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"CMP-{year}-";

        // Use atomic database sequence for thread-safe complaint number generation
        // MERGE with HOLDLOCK and table variable ensures atomicity - no race conditions possible
        var sql = @"
            DECLARE @ResultTable TABLE (LastNumber INT);

            MERGE ComplaintNumberSequences WITH (HOLDLOCK) AS target
            USING (SELECT @Year AS Year) AS source
            ON target.Year = source.Year
            WHEN MATCHED THEN
                UPDATE SET LastNumber = LastNumber + 1
            WHEN NOT MATCHED THEN
                INSERT (Year, LastNumber) VALUES (@Year, 1)
            OUTPUT INSERTED.LastNumber INTO @ResultTable;

            SELECT LastNumber FROM @ResultTable;
        ";

        var connection = _context.Database.GetDbConnection();
        if (connection.State != System.Data.ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.Add(new Microsoft.Data.SqlClient.SqlParameter("@Year", year));

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var nextNumber = Convert.ToInt32(result);
            return $"{prefix}{nextNumber:D4}";
        }
        finally
        {
            // Don't close connection if part of transaction (managed by EF Core)
            if (_context.Database.CurrentTransaction == null && connection.State == System.Data.ConnectionState.Open)
            {
                await connection.CloseAsync();
            }
        }
    }

    public async Task<Dictionary<Guid, int>> GetComplaintCountByStatusAsync(Guid? companyId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (companyId.HasValue)
        {
            query = query.Where(c => c.CompanyId == companyId.Value);
        }

        return await query
            .GroupBy(c => c.StatusMasterId)
            .Select(g => new { StatusMasterId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.StatusMasterId, x => x.Count, cancellationToken);
    }
}
